using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Systems.Environment;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Progression;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

// Explicit alias to resolve EnvironmentalConditions namespace ambiguity
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
// Explicit alias for EnvironmentalManager to resolve ambiguity
using EnvironmentalManager = ProjectChimera.Systems.Environment.EnvironmentalManager;

#if UNITY_SPEEDTREE
using SpeedTree;
#endif

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Advanced environmental response system for SpeedTree cannabis plants.
    /// Provides real-time adaptation to environmental conditions including temperature,
    /// humidity, light, CO2, airflow, and nutrients with visual feedback and genetic adaptation.
    /// </summary>
    public class SpeedTreeEnvironmentalSystem : ChimeraManager
    {
        [Header("Environmental Response Configuration")]
        [SerializeField] private SpeedTreeEnvironmentalConfigSO _environmentalConfig;
        [SerializeField] private EnvironmentalResponseCurvesSO _responseCurves;
        [SerializeField] private StressVisualizationConfigSO _stressVisualization;
        
        [Header("Real-Time Response Settings")]
        [SerializeField] private bool _enableRealTimeResponse = true;
        [SerializeField] private bool _enableAdaptiveResponse = true;
        [SerializeField] private bool _enableStressAccumulation = true;
        [SerializeField] private float _responseUpdateFrequency = 0.5f;
        [SerializeField] private float _adaptationRate = 0.01f;
        
        [Header("Environmental Monitoring")]
        [SerializeField] private bool _enableEnvironmentalLogging = true;
        [SerializeField] private bool _enableStressAnalytics = true;
        [SerializeField] private bool _enablePerformanceOptimization = true;
        [SerializeField] private int _maxSimultaneousPlants = 1000;
        
        [Header("Visual Response Settings")]
        [SerializeField] private bool _enableVisualStressResponse = true;
        [SerializeField] private bool _enableMorphologicalChanges = true;
        [SerializeField] private bool _enableColorChanges = true;
        [SerializeField] private bool _enableAnimationChanges = true;
        
        // Core Environmental Systems
        private EnvironmentalResponseProcessor _responseProcessor;
        private StressAccumulationManager _stressManager;
        private AdaptiveResponseManager _adaptiveManager;
        private EnvironmentalDataCollector _dataCollector;
        private StressVisualizationManager _visualizationManager;
        
        // Plant Response Tracking
        private Dictionary<int, PlantEnvironmentalState> _plantStates = new Dictionary<int, PlantEnvironmentalState>();
        private Dictionary<int, EnvironmentalHistory> _plantHistories = new Dictionary<int, EnvironmentalHistory>();
        private Dictionary<int, AdaptationProgress> _adaptationData = new Dictionary<int, AdaptationProgress>();
        
        // Environmental Zones and Gradients
        private Dictionary<string, EnvironmentalZone> _environmentalZones = new Dictionary<string, EnvironmentalZone>();
        private EnvironmentalGradientManager _gradientManager;
        private MicroclimateSimulator _microclimateSimulator;
        
        // System Integration
        private AdvancedSpeedTreeManager _speedTreeManager;
        private EnvironmentalManager _environmentalManager;
        private CannabisGeneticsEngine _geneticsEngine;
        // private ComprehensiveProgressionManager _progressionManager; // Disabled - using CleanProgressionManager instead
        
        // Performance Monitoring
        private EnvironmentalSystemMetrics _metrics;
        private List<EnvironmentalEvent> _recentEvents = new List<EnvironmentalEvent>();
        private PerformanceOptimizer _performanceOptimizer;
        
        // Coroutine Management
        private Coroutine _updateCoroutine;
        private Queue<PlantEnvironmentalUpdate> _updateQueue = new Queue<PlantEnvironmentalUpdate>();
        
        // Events
        public System.Action<int, EnvironmentalStressData> OnPlantStressChanged;
        public System.Action<int, EnvironmentalAdaptation> OnPlantAdapted;
        public System.Action<EnvironmentalZone, EnvironmentalConditions> OnZoneConditionsChanged;
        public System.Action<EnvironmentalSystemMetrics> OnMetricsUpdated;
        public System.Action<EnvironmentalEvent> OnEnvironmentalEvent;
        
        // Properties
        public EnvironmentalSystemMetrics SystemMetrics => _metrics;
        public int ActivePlantCount => _plantStates.Count;
        public bool SystemEnabled => _enableRealTimeResponse;
        
        protected override void OnManagerInitialize()
        {
            InitializeEnvironmentalSystems();
            InitializeZoneManagement();
            ConnectToGameSystems();
            InitializePerformanceMonitoring();
            StartEnvironmentalUpdateLoop();
            LogInfo("SpeedTree Environmental System initialized");
        }
        
        private void Update()
        {
            UpdateEnvironmentalSystems();
            ProcessUpdateQueue();
            UpdatePerformanceMetrics();
        }
        
        #region Initialization
        
        private void InitializeEnvironmentalSystems()
        {
            // Initialize core processors
            _responseProcessor = new EnvironmentalResponseProcessor(_environmentalConfig, _responseCurves);
            _stressManager = new StressAccumulationManager(_enableStressAccumulation);
            _adaptiveManager = new AdaptiveResponseManager(_enableAdaptiveResponse, _adaptationRate);
            _dataCollector = new EnvironmentalDataCollector(_enableEnvironmentalLogging);
            _visualizationManager = new StressVisualizationManager(_stressVisualization, _enableVisualStressResponse);
            
            // Initialize specialized systems
            _gradientManager = new EnvironmentalGradientManager();
            _microclimateSimulator = new MicroclimateSimulator();
            _performanceOptimizer = new PerformanceOptimizer(_maxSimultaneousPlants);
            
            LogInfo("Environmental response systems initialized");
        }
        
        private void InitializeZoneManagement()
        {
            // Create default environmental zones
            CreateDefaultEnvironmentalZones();
            
            // Initialize microclimate simulation
            _microclimateSimulator.Initialize(_environmentalZones.Values.ToList());
            
            LogInfo($"Initialized {_environmentalZones.Count} environmental zones");
        }
        
        private void CreateDefaultEnvironmentalZones()
        {
            // Vegetative zone
            var vegZone = new EnvironmentalZone
            {
                ZoneId = "vegetative_zone",
                ZoneName = "Vegetative Growth Zone",
                ZoneBounds = new Bounds(Vector3.zero, new Vector3(20f, 5f, 20f)),
                OptimalConditions = new EnvironmentalConditions
                {
                    Temperature = 24f,
                    Humidity = 65f,
                    LightIntensity = 600f,
                    CO2Level = 800f,
                    AirVelocity = 0.3f
                },
                ToleranceRanges = new EnvironmentalTolerances
                {
                    TemperatureRange = new Vector2(18f, 30f),
                    HumidityRange = new Vector2(40f, 80f),
                    LightRange = new Vector2(400f, 1000f),
                    CO2Range = new Vector2(400f, 1200f),
                    AirflowRange = new Vector2(0.1f, 0.8f)
                }
            };
            _environmentalZones[vegZone.ZoneId] = vegZone;
            
            // Flowering zone
            var flowerZone = new EnvironmentalZone
            {
                ZoneId = "flowering_zone",
                ZoneName = "Flowering Zone",
                ZoneBounds = new Bounds(new Vector3(25f, 0f, 0f), new Vector3(20f, 5f, 20f)),
                OptimalConditions = new EnvironmentalConditions
                {
                    Temperature = 22f,
                    Humidity = 50f,
                    LightIntensity = 800f,
                    CO2Level = 1000f,
                    AirVelocity = 0.4f
                },
                ToleranceRanges = new EnvironmentalTolerances
                {
                    TemperatureRange = new Vector2(16f, 28f),
                    HumidityRange = new Vector2(30f, 60f),
                    LightRange = new Vector2(600f, 1200f),
                    CO2Range = new Vector2(600f, 1500f),
                    AirflowRange = new Vector2(0.2f, 1f)
                }
            };
            _environmentalZones[flowerZone.ZoneId] = flowerZone;
            
            // Seedling zone
            var seedlingZone = new EnvironmentalZone
            {
                ZoneId = "seedling_zone",
                ZoneName = "Seedling Nursery",
                ZoneBounds = new Bounds(new Vector3(-25f, 0f, 0f), new Vector3(15f, 3f, 15f)),
                OptimalConditions = new EnvironmentalConditions
                {
                    Temperature = 26f,
                    Humidity = 75f,
                    LightIntensity = 300f,
                    CO2Level = 600f,
                    AirVelocity = 0.1f
                },
                ToleranceRanges = new EnvironmentalTolerances
                {
                    TemperatureRange = new Vector2(22f, 30f),
                    HumidityRange = new Vector2(60f, 85f),
                    LightRange = new Vector2(200f, 500f),
                    CO2Range = new Vector2(400f, 800f),
                    AirflowRange = new Vector2(0.05f, 0.3f)
                }
            };
            _environmentalZones[seedlingZone.ZoneId] = seedlingZone;
        }
        
        private void ConnectToGameSystems()
        {
            if (GameManager.Instance != null)
            {
                _speedTreeManager = GameManager.Instance.GetManager<AdvancedSpeedTreeManager>();
                _environmentalManager = GameManager.Instance.GetManager<EnvironmentalManager>();
                _geneticsEngine = GameManager.Instance.GetManager<CannabisGeneticsEngine>();
                // _progressionManager = GameManager.Instance.GetManager<ComprehensiveProgressionManager>(); // Disabled
            }
            
            ConnectSystemEvents();
        }
        
        private void ConnectSystemEvents()
        {
            if (_speedTreeManager != null)
            {
                _speedTreeManager.OnPlantInstanceCreated += HandlePlantInstanceCreated;
                _speedTreeManager.OnPlantInstanceDestroyed += HandlePlantInstanceDestroyed;
                _speedTreeManager.OnPlantStageChanged += HandlePlantStageChanged;
            }
            
            if (_environmentalManager != null)
            {
                _environmentalManager.OnConditionsChanged += HandleGlobalEnvironmentalChange;
                _environmentalManager.OnTemperatureChanged += HandleTemperatureChange;
                _environmentalManager.OnHumidityChanged += HandleHumidityChange;
                _environmentalManager.OnLightingChanged += HandleLightingChange;
                _environmentalManager.OnAirflowChanged += HandleAirflowChange;
            }
        }
        
        private void InitializePerformanceMonitoring()
        {
            _metrics = new EnvironmentalSystemMetrics
            {
                SystemStartTime = DateTime.Now,
                ActivePlants = 0,
                ProcessedUpdatesPerSecond = 0f,
                AverageResponseTime = 0f,
                StressEventsThisSession = 0,
                AdaptationsThisSession = 0
            };
            
            InvokeRepeating(nameof(UpdateDetailedMetrics), 1f, 5f);
        }
        
        private void StartEnvironmentalUpdateLoop()
        {
            _updateCoroutine = StartCoroutine(EnvironmentalUpdateCoroutine());
        }
        
        private IEnumerator EnvironmentalUpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_responseUpdateFrequency);
                
                if (_enableRealTimeResponse)
                {
                    ProcessEnvironmentalResponses();
                }
                
                if (_enableAdaptiveResponse)
                {
                    ProcessAdaptiveResponses();
                }
                
                if (_enableStressAccumulation)
                {
                    ProcessStressAccumulation();
                }
                
                UpdateEnvironmentalZones();
                UpdateMicroclimates();
            }
        }
        
        #endregion
        
        #region Plant Environmental State Management
        
        public void RegisterPlant(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            if (instance == null) return;
            
            var plantState = new PlantEnvironmentalState
            {
                InstanceId = instance.InstanceId,
                PlantInstance = instance,
                CurrentZone = DetermineEnvironmentalZone(instance.Position),
                LastConditions = GetEnvironmentalConditionsAtPosition(instance.Position),
                StressData = new EnvironmentalStressData(),
                RegistrationTime = DateTime.Now,
                LastUpdateTime = Time.time
            };
            
            _plantStates[instance.InstanceId] = plantState;
            
            // Initialize environmental history
            _plantHistories[instance.InstanceId] = new EnvironmentalHistory
            {
                InstanceId = instance.InstanceId,
                HistoryEntries = new List<EnvironmentalHistoryEntry>(),
                MaxHistorySize = 1000
            };
            
            // Initialize adaptation tracking
            _adaptationData[instance.InstanceId] = new AdaptationProgress
            {
                InstanceId = instance.InstanceId,
                AdaptationFactors = new Dictionary<string, float>(),
                AdaptationStartTime = DateTime.Now,
                TotalAdaptationTime = 0f
            };
            
            LogInfo($"Registered plant {instance.InstanceId} for environmental monitoring in zone {plantState.CurrentZone}");
        }
        
        public void UnregisterPlant(int instanceId)
        {
            _plantStates.Remove(instanceId);
            _plantHistories.Remove(instanceId);
            _adaptationData.Remove(instanceId);
            
            LogInfo($"Unregistered plant {instanceId} from environmental monitoring");
        }
        
        private string DetermineEnvironmentalZone(Vector3 position)
        {
            foreach (var zone in _environmentalZones.Values)
            {
                if (zone.ZoneBounds.Contains(position))
                {
                    return zone.ZoneId;
                }
            }
            
            return "default_zone";
        }
        
        #endregion
        
        #region Environmental Response Processing
        
        private void ProcessEnvironmentalResponses()
        {
            var plantsToProcess = _performanceOptimizer.GetPlantsForProcessing(_plantStates.Values.ToList());
            
            foreach (var plantState in plantsToProcess)
            {
                ProcessPlantEnvironmentalResponse(plantState);
            }
        }
        
        private void ProcessPlantEnvironmentalResponse(PlantEnvironmentalState plantState)
        {
            var currentConditions = GetEnvironmentalConditionsAtPosition(plantState.PlantInstance.Position);
            var previousConditions = plantState.LastConditions;
            
            // Calculate environmental changes
            var environmentalDelta = CalculateEnvironmentalDelta(currentConditions, previousConditions);
            
            // Process immediate responses
            var immediateResponse = _responseProcessor.ProcessImmediateResponse(plantState, currentConditions, environmentalDelta);
            
            // Apply response to plant
            ApplyEnvironmentalResponse(plantState, immediateResponse);
            
            // Update stress accumulation
            _stressManager.UpdateStressAccumulation(plantState, currentConditions);
            
            // Record environmental history
            RecordEnvironmentalHistory(plantState, currentConditions);
            
            // Update plant state
            plantState.LastConditions = currentConditions;
            plantState.LastUpdateTime = Time.time;
            
            // Queue visual update
            QueueVisualUpdate(plantState);
        }
        
        private EnvironmentalDelta CalculateEnvironmentalDelta(EnvironmentalConditions current, EnvironmentalConditions previous)
        {
            return new EnvironmentalDelta
            {
                TemperatureDelta = current.Temperature - previous.Temperature,
                HumidityDelta = current.Humidity - previous.Humidity,
                LightIntensityDelta = current.LightIntensity - previous.LightIntensity,
                CO2Delta = current.CO2Level - previous.CO2Level,
                AirVelocityDelta = current.AirVelocity - previous.AirVelocity,
                DeltaTime = (float)(current.Timestamp - previous.Timestamp).TotalSeconds
            };
        }
        
        private void ApplyEnvironmentalResponse(PlantEnvironmentalState plantState, EnvironmentalResponse response)
        {
            var instance = plantState.PlantInstance;
            if (instance?.Renderer == null) return;
            
#if UNITY_SPEEDTREE
            // Apply growth rate changes
            if (response.GrowthRateModifier != 1f)
            {
                instance.EnvironmentalModifiers["environmental_growth"] = response.GrowthRateModifier;
            }
            
            // Apply visual changes
            if (_enableColorChanges && response.ColorModification != Color.clear)
            {
                var currentColor = instance.CurrentLeafColor;
                var newColor = Color.Lerp(currentColor, response.ColorModification, response.ColorIntensity);
                instance.Renderer.materialProperties?.SetColor("_LeafColor", newColor);
                instance.CurrentLeafColor = newColor;
            }
            
            // Apply morphological changes
            if (_enableMorphologicalChanges)
            {
                ApplyMorphologicalResponse(instance, response);
            }
            
            // Apply animation changes
            if (_enableAnimationChanges)
            {
                ApplyAnimationResponse(instance, response);
            }
            
            // Update stress visualization
            if (_enableVisualStressResponse)
            {
                _visualizationManager.UpdateStressVisualization(instance, plantState.StressData);
            }
#endif
        }
        
        private void ApplyMorphologicalResponse(AdvancedSpeedTreeManager.SpeedTreePlantData instance, EnvironmentalResponse response)
        {
#if UNITY_SPEEDTREE
            if (instance.Renderer?.materialProperties == null) return;
            
            // Leaf morphology changes
            if (response.LeafMorphologyChanges.Count > 0)
            {
                foreach (var change in response.LeafMorphologyChanges)
                {
                    switch (change.Key)
                    {
                        case "curl":
                            instance.Renderer.materialProperties.SetFloat("_LeafCurl", change.Value);
                            break;
                        case "droop":
                            instance.Renderer.materialProperties.SetFloat("_LeafDroop", change.Value);
                            break;
                        case "size":
                            var currentScale = instance.Renderer.transform.localScale;
                            instance.Renderer.transform.localScale = currentScale * (1f + change.Value * 0.1f);
                            break;
                    }
                }
            }
            
            // Stem changes
            if (response.StemChanges.Count > 0)
            {
                foreach (var change in response.StemChanges)
                {
                    switch (change.Key)
                    {
                        case "thickness":
                            instance.Renderer.materialProperties.SetFloat("_StemThickness", 
                                instance.GeneticData.StemThickness * (1f + change.Value));
                            break;
                        case "flexibility":
                            instance.Renderer.materialProperties.SetFloat("_StemFlexibility", change.Value);
                            break;
                    }
                }
            }
#endif
        }
        
        private void ApplyAnimationResponse(AdvancedSpeedTreeManager.SpeedTreePlantData instance, EnvironmentalResponse response)
        {
#if UNITY_SPEEDTREE
            if (instance.Renderer?.materialProperties == null) return;
            
            // Wind response changes
            if (response.WindResponseModifier != 1f)
            {
                instance.Renderer.materialProperties.SetFloat("_WindResponse", response.WindResponseModifier);
            }
            
            // Growth animation speed
            if (response.AnimationSpeedModifier != 1f)
            {
                instance.GrowthAnimationTime *= response.AnimationSpeedModifier;
            }
#endif
        }
        
        #endregion
        
        #region Adaptive Response System
        
        private void ProcessAdaptiveResponses()
        {
            foreach (var adaptationData in _adaptationData.Values)
            {
                ProcessPlantAdaptation(adaptationData);
            }
        }
        
        private void ProcessPlantAdaptation(AdaptationProgress adaptationData)
        {
            if (!_plantStates.TryGetValue(adaptationData.InstanceId, out var plantState)) return;
            
            var currentConditions = plantState.LastConditions;
            var history = _plantHistories[adaptationData.InstanceId];
            
            // Calculate adaptation pressure
            var adaptationPressure = CalculateAdaptationPressure(history, currentConditions);
            
            // Process adaptation if pressure is sufficient
            if (adaptationPressure.TotalPressure > 0.3f)
            {
                var adaptation = _adaptiveManager.ProcessAdaptation(plantState, adaptationPressure, adaptationData);
                
                if (adaptation.HasAdapted)
                {
                    ApplyAdaptation(plantState, adaptation);
                    OnPlantAdapted?.Invoke(adaptationData.InstanceId, adaptation);
                    
                    // Log adaptation for research progress
                    // if (_progressionManager != null)
                    // {
                    //     _progressionManager.GainExperience("environmental_adaptation", 25f, "Plant Adaptation");
                    // }
                    
                    LogInfo($"Plant {adaptationData.InstanceId} adapted to environmental conditions");
                }
            }
        }
        
        private AdaptationPressure CalculateAdaptationPressure(EnvironmentalHistory history, EnvironmentalConditions currentConditions)
        {
            var pressure = new AdaptationPressure();
            
            if (history.HistoryEntries.Count < 10) return pressure; // Need minimum history
            
            var recentEntries = history.HistoryEntries.TakeLast(50).ToList();
            
            // Calculate temperature pressure
            var avgTemperature = recentEntries.Average(e => e.Conditions.Temperature);
            var tempVariance = recentEntries.Select(e => Mathf.Pow(e.Conditions.Temperature - avgTemperature, 2)).Average();
            pressure.TemperaturePressure = Mathf.Clamp01(tempVariance / 100f);
            
            // Calculate humidity pressure
            var avgHumidity = recentEntries.Average(e => e.Conditions.Humidity);
            var humidityVariance = recentEntries.Select(e => Mathf.Pow(e.Conditions.Humidity - avgHumidity, 2)).Average();
            pressure.HumidityPressure = Mathf.Clamp01(humidityVariance / 400f);
            
            // Calculate light pressure
            var avgLight = recentEntries.Average(e => e.Conditions.LightIntensity);
            var lightVariance = recentEntries.Select(e => Mathf.Pow(e.Conditions.LightIntensity - avgLight, 2)).Average();
            pressure.LightPressure = Mathf.Clamp01(lightVariance / 10000f);
            
            pressure.TotalPressure = (pressure.TemperaturePressure + pressure.HumidityPressure + pressure.LightPressure) / 3f;
            
            return pressure;
        }
        
        private void ApplyAdaptation(PlantEnvironmentalState plantState, EnvironmentalAdaptation adaptation)
        {
            var instance = plantState.PlantInstance;
            
            // Apply genetic adaptations
            if (adaptation.GeneticAdaptations.Count > 0)
            {
                foreach (var geneticAdaptation in adaptation.GeneticAdaptations)
                {
                    ApplyGeneticAdaptation(instance, geneticAdaptation.Key, geneticAdaptation.Value);
                }
            }
            
            // Apply tolerance improvements
            if (adaptation.ToleranceImprovements.Count > 0)
            {
                foreach (var tolerance in adaptation.ToleranceImprovements)
                {
                    ApplyToleranceImprovement(instance, tolerance.Key, tolerance.Value);
                }
            }
            
            // Apply efficiency improvements
            if (adaptation.EfficiencyImprovements.Count > 0)
            {
                foreach (var efficiency in adaptation.EfficiencyImprovements)
                {
                    ApplyEfficiencyImprovement(instance, efficiency.Key, efficiency.Value);
                }
            }
            
            // Update adaptation data
            _adaptationData[plantState.InstanceId].TotalAdaptationTime += adaptation.AdaptationTime;
            _adaptationData[plantState.InstanceId].AdaptationFactors[adaptation.AdaptationType] = adaptation.AdaptationStrength;
        }
        
        private void ApplyGeneticAdaptation(AdvancedSpeedTreeManager.SpeedTreePlantData instance, string adaptationType, float strength)
        {
            switch (adaptationType)
            {
                case "heat_tolerance":
                    instance.GeneticData.HeatTolerance += strength;
                    break;
                case "cold_tolerance":
                    instance.GeneticData.ColdTolerance += strength;
                    break;
                case "drought_tolerance":
                    instance.GeneticData.DroughtTolerance += strength;
                    break;
                case "light_efficiency":
                    instance.EnvironmentalModifiers["light_efficiency"] = 1f + strength;
                    break;
            }
        }
        
        private void ApplyToleranceImprovement(AdvancedSpeedTreeManager.SpeedTreePlantData instance, string toleranceType, float improvement)
        {
            // These would modify the plant's response curves
            instance.EnvironmentalModifiers[$"tolerance_{toleranceType}"] = 1f + improvement;
        }
        
        private void ApplyEfficiencyImprovement(AdvancedSpeedTreeManager.SpeedTreePlantData instance, string efficiencyType, float improvement)
        {
            instance.EnvironmentalModifiers[$"efficiency_{efficiencyType}"] = 1f + improvement;
        }
        
        #endregion
        
        #region Stress Accumulation System
        
        private void ProcessStressAccumulation()
        {
            foreach (var plantState in _plantStates.Values)
            {
                ProcessPlantStressAccumulation(plantState);
            }
        }
        
        private void ProcessPlantStressAccumulation(PlantEnvironmentalState plantState)
        {
            var currentConditions = plantState.LastConditions;
            var zone = _environmentalZones[plantState.CurrentZone];
            
            // Calculate stress factors
            var stressUpdate = _stressManager.CalculateStressUpdate(plantState, currentConditions, zone);
            
            // Apply stress accumulation
            _stressManager.ApplyStressAccumulation(plantState.StressData, stressUpdate);
            
            // Check for stress events
            if (stressUpdate.IsStressEvent)
            {
                HandleStressEvent(plantState, stressUpdate);
            }
            
            // Update stress visualization
            if (_enableVisualStressResponse)
            {
                _visualizationManager.UpdateStressVisualization(plantState.PlantInstance, plantState.StressData);
            }
            
            OnPlantStressChanged?.Invoke(plantState.InstanceId, plantState.StressData);
        }
        
        private void HandleStressEvent(PlantEnvironmentalState plantState, StressUpdate stressUpdate)
        {
            var stressEvent = new EnvironmentalEvent
            {
                EventId = Guid.NewGuid().ToString(),
                EventType = EnvironmentalEventType.StressEvent,
                PlantInstanceId = plantState.InstanceId,
                Severity = stressUpdate.Severity,
                Description = stressUpdate.Description,
                Timestamp = DateTime.Now,
                Conditions = plantState.LastConditions
            };
            
            _recentEvents.Add(stressEvent);
            _metrics.StressEventsThisSession++;
            
            OnEnvironmentalEvent?.Invoke(stressEvent);
            
            // Trigger stress response effects
            if (stressUpdate.Severity > EnvironmentalSeverity.Moderate)
            {
                TriggerStressResponseEffects(plantState, stressUpdate);
            }
            
            LogWarning($"Stress event for plant {plantState.InstanceId}: {stressUpdate.Description}");
        }
        
        private void TriggerStressResponseEffects(PlantEnvironmentalState plantState, StressUpdate stressUpdate)
        {
            var instance = plantState.PlantInstance;
            
            // Reduce growth rate
            var stressPenalty = 1f - (stressUpdate.Severity == EnvironmentalSeverity.Critical ? 0.5f : 0.3f);
            instance.EnvironmentalModifiers["stress_penalty"] = stressPenalty;
            
            // Visual stress indicators
#if UNITY_SPEEDTREE
            if (instance.Renderer?.materialProperties != null)
            {
                // Stress coloration
                var stressColor = GetStressColor(stressUpdate.Severity);
                instance.Renderer.materialProperties.SetColor("_StressColor", stressColor);
                instance.Renderer.materialProperties.SetFloat("_StressIntensity", (float)stressUpdate.Severity / 4f);
                
                // Morphological stress responses
                if (stressUpdate.StressType == "temperature")
                {
                    instance.Renderer.materialProperties.SetFloat("_LeafCurl", 0.3f + (float)stressUpdate.Severity * 0.2f);
                }
                else if (stressUpdate.StressType == "water")
                {
                    instance.Renderer.materialProperties.SetFloat("_LeafDroop", 0.2f + (float)stressUpdate.Severity * 0.3f);
                }
            }
#endif
        }
        
        private Color GetStressColor(EnvironmentalSeverity severity)
        {
            return severity switch
            {
                EnvironmentalSeverity.None => Color.green,
                EnvironmentalSeverity.Mild => Color.yellow,
                EnvironmentalSeverity.Moderate => Color.orange,
                EnvironmentalSeverity.Severe => Color.red,
                EnvironmentalSeverity.Critical => Color.magenta,
                _ => Color.white
            };
        }
        
        #endregion
        
        #region Environmental Zone Management
        
        private void UpdateEnvironmentalZones()
        {
            foreach (var zone in _environmentalZones.Values)
            {
                UpdateZoneConditions(zone);
            }
        }
        
        private void UpdateZoneConditions(EnvironmentalZone zone)
        {
            // Get environmental manager data for this zone
            var zoneConditions = GetEnvironmentalConditionsForZone(zone);
            
            // Apply zone-specific modifications
            ApplyZoneModifications(zone, zoneConditions);
            
            // Update microclimate simulation
            _microclimateSimulator.UpdateZoneMicroclimate(zone, zoneConditions);
            
            // Check for zone condition changes
            if (HasZoneConditionsChanged(zone, zoneConditions))
            {
                OnZoneConditionsChanged?.Invoke(zone, zoneConditions);
            }
        }
        
        private EnvironmentalConditions GetEnvironmentalConditionsForZone(EnvironmentalZone zone)
        {
            // Get base conditions from environmental manager
            var baseConditions = _environmentalManager?.GetConditionsAtPosition(zone.ZoneBounds.center) ?? 
                                zone.OptimalConditions;
            
            // Apply zone-specific variations
            return ApplyZoneVariations(baseConditions, zone);
        }
        
        private EnvironmentalConditions ApplyZoneVariations(EnvironmentalConditions baseConditions, EnvironmentalZone zone)
        {
            var zoneConditions = new EnvironmentalConditions
            {
                Temperature = baseConditions.Temperature + UnityEngine.Random.Range(-1f, 1f),
                Humidity = baseConditions.Humidity + UnityEngine.Random.Range(-2f, 2f),
                LightIntensity = baseConditions.LightIntensity + UnityEngine.Random.Range(-50f, 50f),
                CO2Level = baseConditions.CO2Level + UnityEngine.Random.Range(-20f, 20f),
                AirVelocity = baseConditions.AirVelocity + UnityEngine.Random.Range(-0.1f, 0.1f),
                Timestamp = DateTime.Now
            };
            
            return zoneConditions;
        }
        
        private void ApplyZoneModifications(EnvironmentalZone zone, EnvironmentalConditions conditions)
        {
            // Apply any zone-specific environmental modifications
            if (zone.EnvironmentalModifiers != null)
            {
                foreach (var modifier in zone.EnvironmentalModifiers)
                {
                    ApplyEnvironmentalModifier(conditions, modifier.Key, modifier.Value);
                }
            }
        }
        
        private void ApplyEnvironmentalModifier(EnvironmentalConditions conditions, string modifierType, float value)
        {
            switch (modifierType)
            {
                case "temperature_offset":
                    conditions.Temperature += value;
                    break;
                case "humidity_multiplier":
                    conditions.Humidity *= value;
                    break;
                case "light_intensity_multiplier":
                    conditions.LightIntensity *= value;
                    break;
                case "co2_offset":
                    conditions.CO2Level += value;
                    break;
                case "airflow_multiplier":
                    conditions.AirVelocity *= value;
                    break;
            }
        }
        
        private bool HasZoneConditionsChanged(EnvironmentalZone zone, EnvironmentalConditions newConditions)
        {
            if (zone.LastConditions == null) 
            {
                zone.LastConditions = newConditions;
                return true;
            }
            
            var tempDiff = Mathf.Abs(newConditions.Temperature - zone.LastConditions.Temperature);
            var humidityDiff = Mathf.Abs(newConditions.Humidity - zone.LastConditions.Humidity);
            var lightDiff = Mathf.Abs(newConditions.LightIntensity - zone.LastConditions.LightIntensity);
            
            if (tempDiff > 1f || humidityDiff > 5f || lightDiff > 50f)
            {
                zone.LastConditions = newConditions;
                return true;
            }
            
            return false;
        }
        
        #endregion
        
        #region Microclimate Simulation
        
        private void UpdateMicroclimates()
        {
            _microclimateSimulator.UpdateMicroclimates(_plantStates.Values.ToList());
        }
        
        private EnvironmentalConditions GetEnvironmentalConditionsAtPosition(Vector3 position)
        {
            // Get base conditions from environmental manager
            var baseConditions = _environmentalManager?.GetConditionsAtPosition(position) ?? 
                                new EnvironmentalConditions
                                {
                                    Temperature = 24f,
                                    Humidity = 60f,
                                    LightIntensity = 800f,
                                    CO2Level = 400f,
                                    AirVelocity = 0.3f,
                                    Timestamp = DateTime.Now
                                };
            
            // Apply microclimate modifications
            return _microclimateSimulator.GetConditionsAtPosition(position, baseConditions);
        }
        
        #endregion
        
        #region Visual and Performance Updates
        
        private void QueueVisualUpdate(PlantEnvironmentalState plantState)
        {
            var update = new PlantEnvironmentalUpdate
            {
                InstanceId = plantState.InstanceId,
                PlantState = plantState,
                UpdateTime = Time.time,
                Priority = CalculateUpdatePriority(plantState)
            };
            
            _updateQueue.Enqueue(update);
        }
        
        private int CalculateUpdatePriority(PlantEnvironmentalState plantState)
        {
            // Higher priority for stressed plants
            if (plantState.StressData.OverallStress > 0.7f) return 3;
            if (plantState.StressData.OverallStress > 0.4f) return 2;
            return 1;
        }
        
        private void ProcessUpdateQueue()
        {
            int maxUpdatesPerFrame = _performanceOptimizer.GetMaxUpdatesPerFrame();
            int processedUpdates = 0;
            
            while (_updateQueue.Count > 0 && processedUpdates < maxUpdatesPerFrame)
            {
                var update = _updateQueue.Dequeue();
                ProcessVisualUpdate(update);
                processedUpdates++;
            }
        }
        
        private void ProcessVisualUpdate(PlantEnvironmentalUpdate update)
        {
            if (_plantStates.TryGetValue(update.InstanceId, out var plantState))
            {
                _visualizationManager.UpdatePlantVisualization(plantState.PlantInstance, plantState.StressData);
            }
        }
        
        #endregion
        
        #region Environmental History and Analytics
        
        private void RecordEnvironmentalHistory(PlantEnvironmentalState plantState, EnvironmentalConditions conditions)
        {
            if (!_plantHistories.TryGetValue(plantState.InstanceId, out var history)) return;
            
            var entry = new EnvironmentalHistoryEntry
            {
                Timestamp = DateTime.Now,
                Conditions = conditions,
                StressData = new EnvironmentalStressData(plantState.StressData), // Copy
                GrowthRate = plantState.PlantInstance.EnvironmentalModifiers.GetValueOrDefault("environmental_growth", 1f),
                Health = plantState.PlantInstance.Health
            };
            
            history.HistoryEntries.Add(entry);
            
            // Maintain history size
            if (history.HistoryEntries.Count > history.MaxHistorySize)
            {
                history.HistoryEntries.RemoveAt(0);
            }
            
            // Data collection for analytics
            if (_enableEnvironmentalLogging)
            {
                _dataCollector.RecordDataPoint(plantState.InstanceId, entry);
            }
        }
        
        #endregion
        
        #region System Updates and Metrics
        
        private void UpdateEnvironmentalSystems()
        {
            _responseProcessor?.Update();
            _stressManager?.Update();
            _adaptiveManager?.Update();
            _dataCollector?.Update();
            _visualizationManager?.Update();
            _gradientManager?.Update();
            _microclimateSimulator?.Update();
            _performanceOptimizer?.Update();
        }
        
        private void UpdatePerformanceMetrics()
        {
            _metrics.ActivePlants = _plantStates.Count;
            _metrics.ProcessedUpdatesPerSecond = _updateQueue.Count / Time.unscaledDeltaTime;
            _metrics.LastUpdate = DateTime.Now;
        }
        
        private void UpdateDetailedMetrics()
        {
            _metrics.AverageResponseTime = CalculateAverageResponseTime();
            _metrics.AdaptationsThisSession = _adaptationData.Values.Sum(a => a.AdaptationFactors.Count);
            
            OnMetricsUpdated?.Invoke(_metrics);
        }
        
        private float CalculateAverageResponseTime()
        {
            if (_plantStates.Count == 0) return 0f;
            
            return _plantStates.Values.Average(p => Time.time - p.LastUpdateTime);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandlePlantInstanceCreated(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            RegisterPlant(instance);
        }
        
        private void HandlePlantInstanceDestroyed(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            UnregisterPlant(instance.InstanceId);
        }
        
        private void HandlePlantStageChanged(AdvancedSpeedTreeManager.SpeedTreePlantData instance, PlantGrowthStage newStage)
        {
            if (_plantStates.TryGetValue(instance.InstanceId, out var plantState))
            {
                // Update environmental requirements based on growth stage
                UpdatePlantStageRequirements(plantState, newStage);
            }
        }
        
        private void UpdatePlantStageRequirements(PlantEnvironmentalState plantState, PlantGrowthStage newStage)
        {
            // Different growth stages have different environmental needs
            var stageModifiers = GetStageEnvironmentalModifiers(newStage);
            
            foreach (var modifier in stageModifiers)
            {
                plantState.PlantInstance.EnvironmentalModifiers[modifier.Key] = modifier.Value;
            }
        }
        
        private Dictionary<string, float> GetStageEnvironmentalModifiers(PlantGrowthStage stage)
        {
            return stage switch
            {
                PlantGrowthStage.Seedling => new Dictionary<string, float>
                {
                    ["humidity_preference"] = 1.2f,
                    ["light_sensitivity"] = 0.7f,
                    ["temperature_sensitivity"] = 1.3f
                },
                PlantGrowthStage.Vegetative => new Dictionary<string, float>
                {
                    ["humidity_preference"] = 1.1f,
                    ["light_preference"] = 1.2f,
                    ["co2_efficiency"] = 1.1f
                },
                PlantGrowthStage.Flowering => new Dictionary<string, float>
                {
                    ["humidity_preference"] = 0.8f,
                    ["light_preference"] = 1.3f,
                    ["co2_efficiency"] = 1.2f,
                    ["stress_sensitivity"] = 1.2f
                },
                _ => new Dictionary<string, float>()
            };
        }
        
        private void HandleGlobalEnvironmentalChange(EnvironmentalConditions newConditions)
        {
            // Global environmental changes affect all zones
            foreach (var zone in _environmentalZones.Values)
            {
                UpdateZoneWithGlobalChange(zone, newConditions);
            }
        }
        
        private void UpdateZoneWithGlobalChange(EnvironmentalZone zone, EnvironmentalConditions globalConditions)
        {
            // Apply global changes to zone with zone-specific modifications
            var zoneConditions = ApplyZoneVariations(globalConditions, zone);
            _microclimateSimulator.UpdateZoneMicroclimate(zone, zoneConditions);
        }
        
        private void HandleTemperatureChange(float newTemperature)
        {
            // Immediate temperature response for all plants
            foreach (var plantState in _plantStates.Values)
            {
                var temperatureResponse = _responseProcessor.ProcessTemperatureResponse(plantState, newTemperature);
                ApplyTemperatureResponse(plantState, temperatureResponse);
            }
        }
        
        private void ApplyTemperatureResponse(PlantEnvironmentalState plantState, TemperatureResponse response)
        {
            var instance = plantState.PlantInstance;
            
            // Apply immediate temperature effects
            if (response.IsStressResponse)
            {
                instance.EnvironmentalModifiers["temperature_stress"] = response.StressMultiplier;
            }
            
            // Visual temperature response
#if UNITY_SPEEDTREE
            if (instance.Renderer?.materialProperties != null && response.VisualEffect != Color.clear)
            {
                instance.Renderer.materialProperties.SetColor("_TemperatureEffect", response.VisualEffect);
            }
#endif
        }
        
        private void HandleHumidityChange(float newHumidity)
        {
            // Similar to temperature but for humidity
            foreach (var plantState in _plantStates.Values)
            {
                var humidityResponse = _responseProcessor.ProcessHumidityResponse(plantState, newHumidity);
                ApplyHumidityResponse(plantState, humidityResponse);
            }
        }
        
        private void ApplyHumidityResponse(PlantEnvironmentalState plantState, HumidityResponse response)
        {
            var instance = plantState.PlantInstance;
            
            if (response.IsStressResponse)
            {
                instance.EnvironmentalModifiers["humidity_stress"] = response.StressMultiplier;
            }
        }
        
        private void HandleLightingChange(LightingConditions lightingConditions)
        {
            foreach (var plantState in _plantStates.Values)
            {
                var lightResponse = _responseProcessor.ProcessLightResponse(plantState, lightingConditions.Intensity, lightingConditions.Color);
                ApplyLightResponse(plantState, lightResponse);
            }
        }
        
        private void ApplyLightResponse(PlantEnvironmentalState plantState, LightResponse response)
        {
            var instance = plantState.PlantInstance;
            
            // Light efficiency changes
            instance.EnvironmentalModifiers["light_efficiency"] = response.EfficiencyMultiplier;
            
            // Photosynthesis rate changes
            instance.EnvironmentalModifiers["photosynthesis_rate"] = response.PhotosynthesisRate;
        }
        
        private void HandleAirflowChange(float airVelocity)
        {
            foreach (var plantState in _plantStates.Values)
            {
                var airflowResponse = _responseProcessor.ProcessAirflowResponse(plantState, airVelocity);
                ApplyAirflowResponse(plantState, airflowResponse);
            }
        }
        
        private void ApplyAirflowResponse(PlantEnvironmentalState plantState, AirflowResponse response)
        {
            var instance = plantState.PlantInstance;
            
            // Transpiration efficiency
            instance.EnvironmentalModifiers["transpiration_efficiency"] = response.TranspirationMultiplier;
            
            // Wind stress
            if (response.WindStress > 0f)
            {
                instance.EnvironmentalModifiers["wind_stress"] = response.WindStress;
            }
        }
        
        #endregion
        
        #region Public Interface
        
        public PlantEnvironmentalState GetPlantState(int instanceId)
        {
            return _plantStates.TryGetValue(instanceId, out var state) ? state : null;
        }
        
        public EnvironmentalHistory GetPlantHistory(int instanceId)
        {
            return _plantHistories.TryGetValue(instanceId, out var history) ? history : null;
        }
        
        public AdaptationProgress GetPlantAdaptation(int instanceId)
        {
            return _adaptationData.TryGetValue(instanceId, out var adaptation) ? adaptation : null;
        }
        
        public List<EnvironmentalZone> GetEnvironmentalZones()
        {
            return _environmentalZones.Values.ToList();
        }
        
        public EnvironmentalZone GetZone(string zoneId)
        {
            return _environmentalZones.TryGetValue(zoneId, out var zone) ? zone : null;
        }
        
        public List<EnvironmentalEvent> GetRecentEvents(int maxEvents = 50)
        {
            return _recentEvents.TakeLast(maxEvents).ToList();
        }
        
        public void SetSystemEnabled(bool enabled)
        {
            _enableRealTimeResponse = enabled;
        }
        
        public void SetAdaptiveResponseEnabled(bool enabled)
        {
            _enableAdaptiveResponse = enabled;
        }
        
        public void SetStressVisualizationEnabled(bool enabled)
        {
            _enableVisualStressResponse = enabled;
            _visualizationManager.SetEnabled(enabled);
        }
        
        public EnvironmentalSystemReport GetSystemReport()
        {
            return new EnvironmentalSystemReport
            {
                SystemMetrics = _metrics,
                ActivePlantCount = _plantStates.Count,
                EnvironmentalZoneCount = _environmentalZones.Count,
                RecentEventsCount = _recentEvents.Count,
                AdaptationCount = _adaptationData.Values.Sum(a => a.AdaptationFactors.Count),
                SystemStatus = new Dictionary<string, bool>
                {
                    ["RealTimeResponse"] = _enableRealTimeResponse,
                    ["AdaptiveResponse"] = _enableAdaptiveResponse,
                    ["StressAccumulation"] = _enableStressAccumulation,
                    ["VisualStressResponse"] = _enableVisualStressResponse,
                    ["EnvironmentalLogging"] = _enableEnvironmentalLogging,
                    ["PerformanceOptimization"] = _enablePerformanceOptimization
                }
            };
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            // Stop coroutines
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
            }
            CancelInvoke();
            
            // Cleanup systems
            _responseProcessor?.Cleanup();
            _stressManager?.Cleanup();
            _adaptiveManager?.Cleanup();
            _dataCollector?.Cleanup();
            _visualizationManager?.Cleanup();
            _gradientManager?.Cleanup();
            _microclimateSimulator?.Cleanup();
            _performanceOptimizer?.Cleanup();
            
            // Clear data
            _plantStates.Clear();
            _plantHistories.Clear();
            _adaptationData.Clear();
            _environmentalZones.Clear();
            _recentEvents.Clear();
            _updateQueue.Clear();
            
            // Disconnect events
            DisconnectSystemEvents();
            
            LogInfo("SpeedTree Environmental System shutdown complete");
        }
        
        private void DisconnectSystemEvents()
        {
            if (_speedTreeManager != null)
            {
                _speedTreeManager.OnPlantInstanceCreated -= HandlePlantInstanceCreated;
                _speedTreeManager.OnPlantInstanceDestroyed -= HandlePlantInstanceDestroyed;
                _speedTreeManager.OnPlantStageChanged -= HandlePlantStageChanged;
            }
            
            if (_environmentalManager != null)
            {
                _environmentalManager.OnConditionsChanged -= HandleGlobalEnvironmentalChange;
                _environmentalManager.OnTemperatureChanged -= HandleTemperatureChange;
                _environmentalManager.OnHumidityChanged -= HandleHumidityChange;
                _environmentalManager.OnLightingChanged -= HandleLightingChange;
                _environmentalManager.OnAirflowChanged -= HandleAirflowChange;
            }
        }
    }
}