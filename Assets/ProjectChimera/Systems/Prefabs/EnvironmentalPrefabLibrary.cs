using UnityEngine;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Systems.Prefabs
{
    /// <summary>
    /// Specialized prefab library for environmental objects including atmospheric effects,
    /// weather systems, air particles, and environmental sensors.
    /// </summary>
    [CreateAssetMenu(fileName = "New Environmental Prefab Library", menuName = "Project Chimera/Prefabs/Environmental Library")]
    public class EnvironmentalPrefabLibrary : ScriptableObject
    {
        [Header("Environmental Categories")]
        [SerializeField] private List<EnvironmentalPrefabEntry> _environmentalPrefabs = new List<EnvironmentalPrefabEntry>();
        [SerializeField] private List<AtmosphericEffectSet> _atmosphericEffects = new List<AtmosphericEffectSet>();
        
        [Header("Weather & Climate")]
        [SerializeField] private List<WeatherSystemPrefab> _weatherSystems = new List<WeatherSystemPrefab>();
        [SerializeField] private List<ClimateZonePrefab> _climateZones = new List<ClimateZonePrefab>();
        
        [Header("Air Quality & Particles")]
        [SerializeField] private List<AirQualitySystemPrefab> _airQualitySystems = new List<AirQualitySystemPrefab>();
        [SerializeField] private bool _enableRealtimeWeather = true;
        [SerializeField] private bool _enableSeasonalChanges = true;
        
        // Cached lookup tables
        private Dictionary<string, EnvironmentalPrefabEntry> _prefabLookup;
        private Dictionary<EnvironmentalType, List<EnvironmentalPrefabEntry>> _typeLookup;
        private Dictionary<ClimateType, ClimateZonePrefab> _climateLookup;
        
        public List<EnvironmentalPrefabEntry> EnvironmentalPrefabs => _environmentalPrefabs;
        
        public void InitializeDefaults()
        {
            if (_environmentalPrefabs.Count == 0)
            {
                CreateDefaultEnvironmentalPrefabs();
            }
            
            if (_atmosphericEffects.Count == 0)
            {
                CreateDefaultAtmosphericEffects();
            }
            
            if (_weatherSystems.Count == 0)
            {
                CreateDefaultWeatherSystems();
            }
            
            BuildLookupTables();
        }
        
        private void CreateDefaultEnvironmentalPrefabs()
        {
            CreateAtmosphericPrefabs();
            CreateAirQualityPrefabs();
            CreateSensorPrefabs();
            CreateWeatherEffectPrefabs();
        }
        
        private void CreateAtmosphericPrefabs()
        {
            // Air Circulation System
            _environmentalPrefabs.Add(new EnvironmentalPrefabEntry
            {
                PrefabId = "air_circulation_standard",
                PrefabName = "Standard Air Circulation",
                Prefab = null,
                EnvironmentalType = EnvironmentalType.AirFlow,
                EffectRadius = 10f,
                Intensity = 1f,
                PowerConsumption = 200f,
                NoiseLevel = 35f, // dB
                RequiredComponents = new List<string> { "AirCirculationController", "ParticleSystem", "AudioSource" },
                EnvironmentalProperties = new EnvironmentalProperties
                {
                    AffectsTemperature = true,
                    AffectsHumidity = false,
                    AffectsAirFlow = true,
                    AffectsCO2 = true,
                    TemperatureChange = -2f,
                    AirFlowIncrease = 50f,
                    CO2Reduction = 10f
                },
                VisualEffects = new List<string> { "AirParticles", "CirculationLines", "TemperatureGradient" }
            });
            
            // Humidity Control
            _environmentalPrefabs.Add(new EnvironmentalPrefabEntry
            {
                PrefabId = "humidity_control_advanced",
                PrefabName = "Advanced Humidity Control",
                Prefab = null,
                EnvironmentalType = EnvironmentalType.Humidity,
                EffectRadius = 15f,
                Intensity = 1f,
                PowerConsumption = 800f,
                NoiseLevel = 40f,
                RequiredComponents = new List<string> { "HumidityController", "MistSystem", "DrainageSystem" },
                EnvironmentalProperties = new EnvironmentalProperties
                {
                    AffectsTemperature = true,
                    AffectsHumidity = true,
                    AffectsAirFlow = false,
                    AffectsCO2 = false,
                    TemperatureChange = -1f,
                    HumidityRange = new Vector2(40f, 80f),
                    ResponseTime = 5f
                },
                VisualEffects = new List<string> { "MistParticles", "CondensationEffect", "HumidityVisualization" }
            });
            
            // CO2 Enrichment System
            _environmentalPrefabs.Add(new EnvironmentalPrefabEntry
            {
                PrefabId = "co2_enrichment_precision",
                PrefabName = "Precision CO2 Enrichment",
                Prefab = null,
                EnvironmentalType = EnvironmentalType.CO2,
                EffectRadius = 12f,
                Intensity = 1f,
                PowerConsumption = 150f,
                NoiseLevel = 25f,
                RequiredComponents = new List<string> { "CO2Controller", "SafetyShutoff", "CO2Sensor" },
                EnvironmentalProperties = new EnvironmentalProperties
                {
                    AffectsTemperature = false,
                    AffectsHumidity = false,
                    AffectsAirFlow = false,
                    AffectsCO2 = true,
                    CO2TargetPPM = 1200f,
                    CO2ReleaseRate = 50f, // L/min
                    SafetyThreshold = 5000f // ppm
                },
                VisualEffects = new List<string> { "CO2Dispersal", "ConcentrationMap", "SafetyIndicators" }
            });
        }
        
        private void CreateAirQualityPrefabs()
        {
            // Air Purification System
            _environmentalPrefabs.Add(new EnvironmentalPrefabEntry
            {
                PrefabId = "air_purifier_hepa_commercial",
                PrefabName = "Commercial HEPA Air Purifier",
                Prefab = null,
                EnvironmentalType = EnvironmentalType.AirQuality,
                EffectRadius = 20f,
                Intensity = 1f,
                PowerConsumption = 300f,
                NoiseLevel = 30f,
                RequiredComponents = new List<string> { "AirPurifier", "HEPAFilter", "UVSterilizer", "OzoneGenerator" },
                EnvironmentalProperties = new EnvironmentalProperties
                {
                    AffectsTemperature = false,
                    AffectsHumidity = false,
                    AffectsAirFlow = true,
                    AirPurification = 99.97f, // percentage
                    FilterEfficiency = 0.3f, // microns
                    PathogenReduction = 99.9f // percentage
                },
                VisualEffects = new List<string> { "CleanAirFlow", "UVLightGlow", "FilterIndicator" }
            });
            
            // Ozone Generator
            _environmentalPrefabs.Add(new EnvironmentalPrefabEntry
            {
                PrefabId = "ozone_generator_safety",
                PrefabName = "Safety Ozone Generator",
                Prefab = null,
                EnvironmentalType = EnvironmentalType.AirQuality,
                EffectRadius = 25f,
                Intensity = 0.5f,
                PowerConsumption = 100f,
                NoiseLevel = 20f,
                RequiredComponents = new List<string> { "OzoneGenerator", "SafetyTimer", "OzoneDetector" },
                EnvironmentalProperties = new EnvironmentalProperties
                {
                    AffectsTemperature = false,
                    AffectsHumidity = false,
                    AffectsAirFlow = false,
                    PathogenReduction = 99.99f,
                    OzoneProduction = 10f, // g/hour
                    SafetyShutoff = true
                },
                VisualEffects = new List<string> { "OzoneGlow", "SterilizationField", "SafetyStatus" }
            });
        }
        
        private void CreateSensorPrefabs()
        {
            // Multi-Environmental Sensor
            _environmentalPrefabs.Add(new EnvironmentalPrefabEntry
            {
                PrefabId = "sensor_environmental_multi",
                PrefabName = "Multi-Environmental Sensor",
                Prefab = null,
                EnvironmentalType = EnvironmentalType.Monitoring,
                EffectRadius = 5f,
                Intensity = 0f,
                PowerConsumption = 10f,
                NoiseLevel = 0f,
                RequiredComponents = new List<string> { "EnvironmentalSensor", "DataLogger", "WirelessTransmitter" },
                EnvironmentalProperties = new EnvironmentalProperties
                {
                    MonitoringCapabilities = new List<string> 
                    { 
                        "Temperature", "Humidity", "CO2", "Light", "AirFlow", "Pressure", "VPD" 
                    },
                    SensorAccuracy = 99.5f,
                    ResponseTime = 1f,
                    DataLoggingInterval = 60f // seconds
                },
                VisualEffects = new List<string> { "SensorReadings", "DataVisualization", "StatusLights" }
            });
            
            // Air Quality Monitor
            _environmentalPrefabs.Add(new EnvironmentalPrefabEntry
            {
                PrefabId = "air_quality_monitor_pro",
                PrefabName = "Professional Air Quality Monitor",
                Prefab = null,
                EnvironmentalType = EnvironmentalType.Monitoring,
                EffectRadius = 3f,
                Intensity = 0f,
                PowerConsumption = 25f,
                NoiseLevel = 0f,
                RequiredComponents = new List<string> { "AirQualityMonitor", "ParticleCounter", "VOCDetector" },
                EnvironmentalProperties = new EnvironmentalProperties
                {
                    MonitoringCapabilities = new List<string> 
                    { 
                        "PM2.5", "PM10", "VOCs", "Ozone", "Formaldehyde", "CO", "NO2" 
                    },
                    SensorAccuracy = 98f,
                    ResponseTime = 30f,
                    AlertThresholds = true
                },
                VisualEffects = new List<string> { "AirQualityDisplay", "AlertIndicators", "TrendGraphs" }
            });
        }
        
        private void CreateWeatherEffectPrefabs()
        {
            // Indoor Rain Simulation
            _environmentalPrefabs.Add(new EnvironmentalPrefabEntry
            {
                PrefabId = "weather_rain_simulation",
                PrefabName = "Rain Simulation System",
                Prefab = null,
                EnvironmentalType = EnvironmentalType.Weather,
                EffectRadius = 30f,
                Intensity = 0.7f,
                PowerConsumption = 500f,
                NoiseLevel = 45f,
                RequiredComponents = new List<string> { "RainSystem", "DrainageNetwork", "HumidityBoost" },
                EnvironmentalProperties = new EnvironmentalProperties
                {
                    AffectsTemperature = true,
                    AffectsHumidity = true,
                    AffectsAirFlow = true,
                    TemperatureChange = -3f,
                    HumidityIncrease = 20f,
                    WeatherType = WeatherType.Rain
                },
                VisualEffects = new List<string> { "RainParticles", "WaterDroplets", "Puddles", "SoundEffects" }
            });
            
            // Wind Simulation
            _environmentalPrefabs.Add(new EnvironmentalPrefabEntry
            {
                PrefabId = "weather_wind_simulation",
                PrefabName = "Wind Simulation System",
                Prefab = null,
                EnvironmentalType = EnvironmentalType.Weather,
                EffectRadius = 50f,
                Intensity = 0.8f,
                PowerConsumption = 1000f,
                NoiseLevel = 50f,
                RequiredComponents = new List<string> { "WindGenerator", "TurbulenceSystem", "PlantMovement" },
                EnvironmentalProperties = new EnvironmentalProperties
                {
                    AffectsTemperature = true,
                    AffectsHumidity = true,
                    AffectsAirFlow = true,
                    TemperatureChange = -1f,
                    WindSpeed = 15f, // km/h
                    WeatherType = WeatherType.Windy
                },
                VisualEffects = new List<string> { "WindParticles", "PlantSway", "AirCurrents", "LeafMovement" }
            });
        }
        
        private void CreateDefaultAtmosphericEffects()
        {
            // Heat Shimmer Effect
            _atmosphericEffects.Add(new AtmosphericEffectSet
            {
                EffectId = "heat_shimmer_effect",
                EffectName = "Heat Shimmer",
                EffectType = AtmosphericEffectType.Temperature,
                TriggerConditions = new EffectTriggerConditions
                {
                    MinTemperature = 28f,
                    MinHumidity = 0f,
                    RequiredLightIntensity = 500f
                },
                VisualParameters = new VisualEffectParameters
                {
                    ParticleCount = 100,
                    EffectColor = new Color(1f, 0.8f, 0.6f, 0.3f),
                    AnimationSpeed = 2f,
                    DistortionStrength = 0.1f
                },
                AudioParameters = new AudioEffectParameters
                {
                    SoundClip = null,
                    Volume = 0f,
                    Pitch = 1f
                }
            });
            
            // Moisture Condensation
            _atmosphericEffects.Add(new AtmosphericEffectSet
            {
                EffectId = "moisture_condensation",
                EffectName = "Moisture Condensation",
                EffectType = AtmosphericEffectType.Humidity,
                TriggerConditions = new EffectTriggerConditions
                {
                    MinTemperature = 0f,
                    MinHumidity = 80f,
                    TemperatureDifferential = 5f
                },
                VisualParameters = new VisualEffectParameters
                {
                    ParticleCount = 200,
                    EffectColor = new Color(0.8f, 0.9f, 1f, 0.6f),
                    AnimationSpeed = 0.5f,
                    DropletSize = 0.01f
                },
                AudioParameters = new AudioEffectParameters
                {
                    SoundClip = null,
                    Volume = 0.2f,
                    Pitch = 0.8f
                }
            });
        }
        
        private void CreateDefaultWeatherSystems()
        {
            // Controlled Climate System
            _weatherSystems.Add(new WeatherSystemPrefab
            {
                SystemId = "controlled_climate_standard",
                SystemName = "Standard Controlled Climate",
                WeatherType = WeatherType.Controlled,
                TemperatureRange = new Vector2(20f, 26f),
                HumidityRange = new Vector2(50f, 70f),
                CO2Range = new Vector2(400f, 1200f),
                AirFlowRange = new Vector2(0.2f, 1f),
                LightCycleHours = 18f,
                SeasonalVariation = false,
                RequiredComponents = new List<string> { "ClimateController", "WeatherStation", "AutomationSystem" }
            });
            
            // Tropical Greenhouse Climate
            _weatherSystems.Add(new WeatherSystemPrefab
            {
                SystemId = "tropical_greenhouse",
                SystemName = "Tropical Greenhouse Climate",
                WeatherType = WeatherType.Tropical,
                TemperatureRange = new Vector2(24f, 30f),
                HumidityRange = new Vector2(70f, 90f),
                CO2Range = new Vector2(600f, 1500f),
                AirFlowRange = new Vector2(0.5f, 1.5f),
                LightCycleHours = 12f,
                SeasonalVariation = true,
                RequiredComponents = new List<string> { "TropicalController", "MistingSystems", "VentilationBoost" }
            });
        }
        
        private void BuildLookupTables()
        {
            _prefabLookup = _environmentalPrefabs.ToDictionary(e => e.PrefabId, e => e);
            
            _typeLookup = _environmentalPrefabs.GroupBy(e => e.EnvironmentalType)
                                            .ToDictionary(g => g.Key, g => g.ToList());
            
            _climateLookup = _climateZones.ToDictionary(c => c.ClimateType, c => c);
        }
        
        public EnvironmentalPrefabEntry GetEnvironmentalPrefab(EnvironmentalType environmentalType, string prefabId = null)
        {
            if (!string.IsNullOrEmpty(prefabId) && _prefabLookup.TryGetValue(prefabId, out var specificPrefab))
            {
                return specificPrefab;
            }
            
            if (_typeLookup.TryGetValue(environmentalType, out var typePrefabs) && typePrefabs.Count > 0)
            {
                return typePrefabs[0];
            }
            
            return null;
        }
        
        public List<EnvironmentalPrefabEntry> GetEnvironmentalPrefabsByType(EnvironmentalType environmentalType)
        {
            return _typeLookup.TryGetValue(environmentalType, out var prefabs) ? prefabs : new List<EnvironmentalPrefabEntry>();
        }
        
        public List<EnvironmentalPrefabEntry> GetPrefabsByPowerConsumption(float maxPower)
        {
            return _environmentalPrefabs.Where(e => e.PowerConsumption <= maxPower).ToList();
        }
        
        public List<EnvironmentalPrefabEntry> GetPrefabsByEffectRadius(float minRadius, float maxRadius)
        {
            return _environmentalPrefabs.Where(e => e.EffectRadius >= minRadius && e.EffectRadius <= maxRadius).ToList();
        }
        
        public WeatherSystemPrefab GetWeatherSystem(WeatherType weatherType)
        {
            return _weatherSystems.FirstOrDefault(w => w.WeatherType == weatherType);
        }
        
        public List<AtmosphericEffectSet> GetAtmosphericEffectsForConditions(EnvironmentalConditions conditions)
        {
            return _atmosphericEffects.Where(effect =>
                conditions.Temperature >= effect.TriggerConditions.MinTemperature &&
                conditions.Humidity >= effect.TriggerConditions.MinHumidity &&
                (effect.TriggerConditions.RequiredLightIntensity == 0f || 
                 conditions.LightIntensity >= effect.TriggerConditions.RequiredLightIntensity)
            ).ToList();
        }
        
        public EnvironmentalSystemRecommendation GetSystemRecommendation(EnvironmentalConditions currentConditions, 
                                                                         EnvironmentalConditions targetConditions)
        {
            var recommendation = new EnvironmentalSystemRecommendation
            {
                RecommendedSystems = new List<string>(),
                EstimatedCost = 0f,
                EstimatedPowerConsumption = 0f,
                PriorityLevel = SystemPriority.Medium
            };
            
            // Temperature control recommendations
            if (Mathf.Abs(currentConditions.Temperature - targetConditions.Temperature) > 2f)
            {
                if (currentConditions.Temperature > targetConditions.Temperature)
                {
                    recommendation.RecommendedSystems.Add("air_circulation_standard");
                    recommendation.EstimatedPowerConsumption += 200f;
                    recommendation.EstimatedCost += 500f;
                }
            }
            
            // Humidity control recommendations
            if (Mathf.Abs(currentConditions.Humidity - targetConditions.Humidity) > 10f)
            {
                recommendation.RecommendedSystems.Add("humidity_control_advanced");
                recommendation.EstimatedPowerConsumption += 800f;
                recommendation.EstimatedCost += 2000f;
            }
            
            // CO2 enrichment recommendations
            if (currentConditions.CO2Level < targetConditions.CO2Level - 100f)
            {
                recommendation.RecommendedSystems.Add("co2_enrichment_precision");
                recommendation.EstimatedPowerConsumption += 150f;
                recommendation.EstimatedCost += 1500f;
            }
            
            // Air quality recommendations
            if (targetConditions.LightIntensity > 800f) // High light environments need good air quality
            {
                recommendation.RecommendedSystems.Add("air_purifier_hepa_commercial");
                recommendation.EstimatedPowerConsumption += 300f;
                recommendation.EstimatedCost += 1200f;
            }
            
            // Monitoring recommendations
            recommendation.RecommendedSystems.Add("sensor_environmental_multi");
            recommendation.EstimatedPowerConsumption += 10f;
            recommendation.EstimatedCost += 350f;
            
            // Set priority based on deviations
            float totalDeviation = Mathf.Abs(currentConditions.Temperature - targetConditions.Temperature) +
                                  Mathf.Abs(currentConditions.Humidity - targetConditions.Humidity) / 10f +
                                  Mathf.Abs(currentConditions.CO2Level - targetConditions.CO2Level) / 100f;
            
            if (totalDeviation > 10f)
                recommendation.PriorityLevel = SystemPriority.High;
            else if (totalDeviation > 5f)
                recommendation.PriorityLevel = SystemPriority.Medium;
            else
                recommendation.PriorityLevel = SystemPriority.Low;
            
            return recommendation;
        }
        
        public EnvironmentalLibraryStats GetLibraryStats()
        {
            return new EnvironmentalLibraryStats
            {
                TotalEnvironmentalPrefabs = _environmentalPrefabs.Count,
                TotalAtmosphericEffects = _atmosphericEffects.Count,
                TotalWeatherSystems = _weatherSystems.Count,
                TypeDistribution = _typeLookup.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count),
                AveragePowerConsumption = _environmentalPrefabs.Average(e => e.PowerConsumption),
                TotalSystemValue = _environmentalPrefabs.Sum(e => e.PowerConsumption * 2f), // Rough cost estimate
                EffectRadiusRange = new Vector2(
                    _environmentalPrefabs.Min(e => e.EffectRadius),
                    _environmentalPrefabs.Max(e => e.EffectRadius)
                )
            };
        }
        
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                BuildLookupTables();
            }
        }
    }
    
    // Supporting data structures
    [System.Serializable]
    public class EnvironmentalPrefabEntry
    {
        public string PrefabId;
        public string PrefabName;
        public GameObject Prefab;
        public EnvironmentalType EnvironmentalType;
        public float EffectRadius;
        public float Intensity;
        public float PowerConsumption;
        public float NoiseLevel;
        public List<string> RequiredComponents = new List<string>();
        public EnvironmentalProperties EnvironmentalProperties;
        public List<string> VisualEffects = new List<string>();
    }
    
    [System.Serializable]
    public class EnvironmentalProperties
    {
        public bool AffectsTemperature = false;
        public bool AffectsHumidity = false;
        public bool AffectsAirFlow = false;
        public bool AffectsCO2 = false;
        public float TemperatureChange = 0f;
        public float HumidityIncrease = 0f;
        public Vector2 HumidityRange = Vector2.zero;
        public float AirFlowIncrease = 0f;
        public float CO2Reduction = 0f;
        public float CO2TargetPPM = 0f;
        public float CO2ReleaseRate = 0f;
        public float SafetyThreshold = 0f;
        public float AirPurification = 0f;
        public float FilterEfficiency = 0f;
        public float PathogenReduction = 0f;
        public float OzoneProduction = 0f;
        public bool SafetyShutoff = false;
        public List<string> MonitoringCapabilities = new List<string>();
        public float SensorAccuracy = 0f;
        public float ResponseTime = 0f;
        public float DataLoggingInterval = 0f;
        public bool AlertThresholds = false;
        public float WindSpeed = 0f;
        public WeatherType WeatherType = WeatherType.Controlled;
    }
    
    [System.Serializable]
    public class AtmosphericEffectSet
    {
        public string EffectId;
        public string EffectName;
        public AtmosphericEffectType EffectType;
        public EffectTriggerConditions TriggerConditions;
        public VisualEffectParameters VisualParameters;
        public AudioEffectParameters AudioParameters;
    }
    
    [System.Serializable]
    public class EffectTriggerConditions
    {
        public float MinTemperature = 0f;
        public float MaxTemperature = 100f;
        public float MinHumidity = 0f;
        public float MaxHumidity = 100f;
        public float RequiredLightIntensity = 0f;
        public float TemperatureDifferential = 0f;
        public bool RequiresAirFlow = false;
    }
    
    [System.Serializable]
    public class VisualEffectParameters
    {
        public int ParticleCount = 100;
        public Color EffectColor = Color.white;
        public float AnimationSpeed = 1f;
        public float DistortionStrength = 0f;
        public float DropletSize = 0f;
        public float OpacityMultiplier = 1f;
    }
    
    [System.Serializable]
    public class AudioEffectParameters
    {
        public AudioClip SoundClip;
        public float Volume = 1f;
        public float Pitch = 1f;
        public bool Loop = true;
        public float FadeInTime = 1f;
        public float FadeOutTime = 1f;
    }
    
    [System.Serializable]
    public class WeatherSystemPrefab
    {
        public string SystemId;
        public string SystemName;
        public WeatherType WeatherType;
        public Vector2 TemperatureRange;
        public Vector2 HumidityRange;
        public Vector2 CO2Range;
        public Vector2 AirFlowRange;
        public float LightCycleHours;
        public bool SeasonalVariation;
        public List<string> RequiredComponents = new List<string>();
    }
    
    [System.Serializable]
    public class ClimateZonePrefab
    {
        public ClimateType ClimateType;
        public string ZoneName;
        public EnvironmentalConditions BaseConditions;
        public List<string> SupportedWeatherSystems = new List<string>();
        public float EnergyEfficiency;
    }
    
    [System.Serializable]
    public class AirQualitySystemPrefab
    {
        public string SystemId;
        public string SystemName;
        public List<string> FilteredContaminants = new List<string>();
        public float FilterEfficiency;
        public float MaxAirFlowRate;
        public float PowerConsumption;
        public float MaintenanceInterval; // days
    }
    
    [System.Serializable]
    public class EnvironmentalSystemRecommendation
    {
        public List<string> RecommendedSystems = new List<string>();
        public float EstimatedCost;
        public float EstimatedPowerConsumption;
        public SystemPriority PriorityLevel;
        public string ReasoningExplanation;
    }
    
    [System.Serializable]
    public class EnvironmentalLibraryStats
    {
        public int TotalEnvironmentalPrefabs;
        public int TotalAtmosphericEffects;
        public int TotalWeatherSystems;
        public Dictionary<EnvironmentalType, int> TypeDistribution;
        public float AveragePowerConsumption;
        public float TotalSystemValue;
        public Vector2 EffectRadiusRange;
    }
    
    // Enums
    public enum EnvironmentalType
    {
        AirFlow,
        Temperature,
        Humidity,
        CO2,
        AirQuality,
        Monitoring,
        Weather,
        Lighting,
        Sound
    }
    
    public enum AtmosphericEffectType
    {
        Temperature,
        Humidity,
        Pressure,
        AirFlow,
        Particle,
        Chemical
    }
    
    public enum WeatherType
    {
        Controlled,
        Tropical,
        Arid,
        Temperate,
        Mediterranean,
        Rain,
        Windy,
        Sunny
    }
    
    public enum ClimateType
    {
        Indoor_Controlled,
        Greenhouse,
        Outdoor,
        Hybrid,
        Laboratory
    }
    
    public enum SystemPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}