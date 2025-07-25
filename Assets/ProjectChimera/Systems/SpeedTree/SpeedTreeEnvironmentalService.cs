using UnityEngine;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Genetics;
using System.Collections.Generic;
using System.Collections;

#if UNITY_SPEEDTREE
using SpeedTree;
#endif

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// PC013-8c: SpeedTreeEnvironmentalService extracted from AdvancedSpeedTreeManager
    /// Handles environmental response system for SpeedTree plants including seasonal changes,
    /// stress visualization, and real-time environmental adaptation.
    /// </summary>
    public class SpeedTreeEnvironmentalService
    {
        private readonly bool _enableEnvironmentalResponse;
        private readonly bool _enableSeasonalChanges;
        private readonly bool _enableStressVisualization;
        private readonly float _environmentalUpdateFrequency;
        
        private Dictionary<int, EnvironmentalResponseState> _plantEnvironmentalStates;
        private Dictionary<int, SeasonalState> _plantSeasonalStates;
        private Coroutine _environmentalUpdateCoroutine;
        private MonoBehaviour _coroutineHost;
        
        private EnvironmentalConditions _currentEnvironment;
        private SeasonalConditions _currentSeason;
        
        // Events for environmental responses
        public System.Action<int, EnvironmentalStressLevel> OnStressLevelChanged;
        public System.Action<int, EnvironmentalAdaptation> OnEnvironmentalAdaptation;
        public System.Action<SeasonalConditions> OnSeasonalChange;
        
        public SpeedTreeEnvironmentalService(MonoBehaviour coroutineHost, bool enableEnvironmentalResponse = true,
            bool enableSeasonalChanges = true, bool enableStressVisualization = true, float updateFrequency = 0.5f)
        {
            _coroutineHost = coroutineHost;
            _enableEnvironmentalResponse = enableEnvironmentalResponse;
            _enableSeasonalChanges = enableSeasonalChanges;
            _enableStressVisualization = enableStressVisualization;
            _environmentalUpdateFrequency = updateFrequency;
            
            _plantEnvironmentalStates = new Dictionary<int, EnvironmentalResponseState>();
            _plantSeasonalStates = new Dictionary<int, SeasonalState>();
            
            InitializeEnvironmentalSystem();
            
            Debug.Log("[SpeedTreeEnvironmentalService] Initialized environmental response system");
        }
        
        /// <summary>
        /// Registers a plant for environmental monitoring.
        /// </summary>
        public void RegisterPlantForEnvironmentalMonitoring(SpeedTreePlantData plantData)
        {
            if (plantData == null)
            {
                Debug.LogWarning("[SpeedTreeEnvironmentalService] Cannot register null plant data");
                return;
            }
            
            var environmentalState = new EnvironmentalResponseState
            {
                InstanceId = plantData.InstanceId,
                CurrentStressLevel = EnvironmentalStressLevel.None,
                TemperatureAdaptation = 1f,
                HumidityAdaptation = 1f,
                LightAdaptation = 1f,
                LastEnvironmentalUpdate = Time.time,
                AccumulatedStress = 0f
            };
            
            _plantEnvironmentalStates[plantData.InstanceId] = environmentalState;
            
            if (_enableSeasonalChanges)
            {
                var seasonalState = new SeasonalState
                {
                    InstanceId = plantData.InstanceId,
                    CurrentSeason = Season.Spring,
                    SeasonProgress = 0f,
                    SeasonalAdaptations = new Dictionary<Season, float>()
                };
                
                _plantSeasonalStates[plantData.InstanceId] = seasonalState;
            }
            
            Debug.Log($"[SpeedTreeEnvironmentalService] Registered plant {plantData.InstanceId} for environmental monitoring");
        }
        
        /// <summary>
        /// Unregisters a plant from environmental monitoring.
        /// </summary>
        public void UnregisterPlantFromEnvironmentalMonitoring(int instanceId)
        {
            _plantEnvironmentalStates.Remove(instanceId);
            _plantSeasonalStates.Remove(instanceId);
            
            Debug.Log($"[SpeedTreeEnvironmentalService] Unregistered plant {instanceId} from environmental monitoring");
        }
        
        /// <summary>
        /// Updates environmental conditions for all monitored plants.
        /// </summary>
        public void UpdateEnvironmentalConditions(EnvironmentalConditions newConditions)
        {
            _currentEnvironment = newConditions;
            
            if (!_enableEnvironmentalResponse) return;
            
            foreach (var kvp in _plantEnvironmentalStates)
            {
                UpdatePlantEnvironmentalResponse(kvp.Key, kvp.Value, newConditions);
            }
        }
        
        /// <summary>
        /// Updates environmental response for a specific plant.
        /// </summary>
        public void UpdatePlantEnvironmentalResponse(int instanceId, EnvironmentalResponseState state, 
            EnvironmentalConditions environment)
        {
            if (state == null || environment == null) return;
            
            var deltaTime = Time.time - state.LastEnvironmentalUpdate;
            if (deltaTime < _environmentalUpdateFrequency) return;
            
            // Calculate environmental stress
            var stressFactors = CalculateEnvironmentalStress(environment, state);
            var newStressLevel = DetermineStressLevel(stressFactors);
            
            // Update stress accumulation
            state.AccumulatedStress += stressFactors.TotalStress * deltaTime;
            state.AccumulatedStress = Mathf.Clamp01(state.AccumulatedStress * 0.95f); // Natural recovery
            
            // Check for stress level changes
            if (newStressLevel != state.CurrentStressLevel)
            {
                state.CurrentStressLevel = newStressLevel;
                OnStressLevelChanged?.Invoke(instanceId, newStressLevel);
            }
            
            // Update environmental adaptations
            UpdateEnvironmentalAdaptations(state, environment, deltaTime);
            
            state.LastEnvironmentalUpdate = Time.time;
        }
        
        /// <summary>
        /// Applies environmental effects to a plant's SpeedTree renderer.
        /// </summary>
        public void ApplyEnvironmentalEffects(SpeedTreePlantData plantData, EnvironmentalConditions environment)
        {
            if (plantData?.Renderer == null || !_enableStressVisualization) return;
            
            if (!_plantEnvironmentalStates.TryGetValue(plantData.InstanceId, out var state)) return;
            
            #if UNITY_SPEEDTREE
            var renderer = plantData.Renderer;
            
            // Apply stress visualization
            ApplyStressVisualization(renderer, state);
            
            // Apply environmental color changes
            ApplyEnvironmentalColorEffects(renderer, environment, state);
            
            // Apply morphological changes due to environment
            ApplyEnvironmentalMorphologyChanges(renderer, environment, state);
            
            // Update plant data stress level
            plantData.StressLevel = state.AccumulatedStress;
            #endif
        }
        
        /// <summary>
        /// Updates seasonal conditions and effects.
        /// </summary>
        public void UpdateSeasonalConditions(SeasonalConditions newSeason)
        {
            if (!_enableSeasonalChanges) return;
            
            _currentSeason = newSeason;
            OnSeasonalChange?.Invoke(newSeason);
            
            foreach (var kvp in _plantSeasonalStates)
            {
                UpdatePlantSeasonalEffects(kvp.Key, kvp.Value, newSeason);
            }
        }
        
        /// <summary>
        /// Gets environmental response state for a plant.
        /// </summary>
        public EnvironmentalResponseState GetEnvironmentalState(int instanceId)
        {
            return _plantEnvironmentalStates.TryGetValue(instanceId, out var state) ? state : null;
        }
        
        /// <summary>
        /// Registers a plant for environmental monitoring and effects.
        /// </summary>
        public void RegisterPlant(SpeedTreePlantData plantData)
        {
            if (plantData == null)
            {
                Debug.LogWarning("[SpeedTreeEnvironmentalService] Cannot register null plant data");
                return;
            }
            
            var environmentalState = new EnvironmentalResponseState
            {
                InstanceId = plantData.InstanceId,
                PlantData = plantData,
                IsActive = true,
                LastUpdateTime = Time.time
            };
            
            var seasonalState = new SeasonalState
            {
                InstanceId = plantData.InstanceId,
                CurrentSeason = GetCurrentSeason(),
                SeasonProgress = 0f,
                IsActive = true
            };
            
            _plantEnvironmentalStates[plantData.InstanceId] = environmentalState;
            _plantSeasonalStates[plantData.InstanceId] = seasonalState;
            
            Debug.Log($"[SpeedTreeEnvironmentalService] Registered plant {plantData.InstanceId} for environmental monitoring");
        }
        
        /// <summary>
        /// Unregisters a plant from environmental monitoring.
        /// </summary>
        public void UnregisterPlant(int instanceId)
        {
            _plantEnvironmentalStates.Remove(instanceId);
            _plantSeasonalStates.Remove(instanceId);
            
            Debug.Log($"[SpeedTreeEnvironmentalService] Unregistered plant {instanceId} from environmental monitoring");
        }

        /// <summary>
        /// Gets environmental service statistics.
        /// </summary>
        public EnvironmentalServiceStats GetEnvironmentalStats()
        {
            var stressedPlants = 0;
            var averageStress = 0f;
            var totalStress = 0f;
            
            foreach (var state in _plantEnvironmentalStates.Values)
            {
                if (state.CurrentStressLevel != EnvironmentalStressLevel.None)
                    stressedPlants++;
                
                totalStress += state.AccumulatedStress;
            }
            
            var monitoredPlants = _plantEnvironmentalStates.Count;
            averageStress = monitoredPlants > 0 ? totalStress / monitoredPlants : 0f;
            
            return new EnvironmentalServiceStats
            {
                MonitoredPlants = monitoredPlants,
                StressedPlants = stressedPlants,
                AverageStressLevel = averageStress,
                EnvironmentalResponseEnabled = _enableEnvironmentalResponse,
                SeasonalChangesEnabled = _enableSeasonalChanges,
                StressVisualizationEnabled = _enableStressVisualization,
                UpdateFrequency = _environmentalUpdateFrequency
            };
        }
        
        /// <summary>
        /// Gets the current season based on time or configuration.
        /// </summary>
        private Season GetCurrentSeason()
        {
            // Simple season calculation based on time
            var dayOfYear = System.DateTime.Now.DayOfYear;
            
            if (dayOfYear < 80 || dayOfYear >= 355) return Season.Winter;
            if (dayOfYear < 172) return Season.Spring;
            if (dayOfYear < 266) return Season.Summer;
            return Season.Autumn;
        }

        /// <summary>
        /// Cleans up environmental monitoring.
        /// </summary>
        public void Cleanup()
        {
            if (_environmentalUpdateCoroutine != null && _coroutineHost != null)
            {
                _coroutineHost.StopCoroutine(_environmentalUpdateCoroutine);
                _environmentalUpdateCoroutine = null;
            }
            
            _plantEnvironmentalStates.Clear();
            _plantSeasonalStates.Clear();
            
            Debug.Log("[SpeedTreeEnvironmentalService] Cleanup completed");
        }
        
        // Private methods
        private void InitializeEnvironmentalSystem()
        {
            // Initialize default environmental conditions
            _currentEnvironment = new EnvironmentalConditions
            {
                Temperature = 24f,
                Humidity = 60f,
                LightIntensity = 800f,
                CO2Level = 1200f
            };
            
            _currentSeason = new SeasonalConditions
            {
                CurrentSeason = Season.Spring,
                SeasonProgress = 0f,
                DayLength = 12f,
                AverageTemperature = 20f
            };
            
            if (_enableEnvironmentalResponse && _coroutineHost != null)
            {
                _environmentalUpdateCoroutine = _coroutineHost.StartCoroutine(EnvironmentalUpdateCoroutine());
            }
        }
        
        private IEnumerator EnvironmentalUpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_environmentalUpdateFrequency);
                
                if (_currentEnvironment != null)
                {
                    var plantsToUpdate = new List<int>(_plantEnvironmentalStates.Keys);
                    
                    foreach (var instanceId in plantsToUpdate)
                    {
                        if (_plantEnvironmentalStates.TryGetValue(instanceId, out var state))
                        {
                            UpdatePlantEnvironmentalResponse(instanceId, state, _currentEnvironment);
                        }
                    }
                }
            }
        }
        
        private EnvironmentalStressFactors CalculateEnvironmentalStress(EnvironmentalConditions environment, 
            EnvironmentalResponseState state)
        {
            var factors = new EnvironmentalStressFactors();
            
            // Temperature stress
            factors.TemperatureStress = CalculateTemperatureStress(environment.Temperature);
            
            // Humidity stress
            factors.HumidityStress = CalculateHumidityStress(environment.Humidity);
            
            // Light stress
            factors.LightStress = CalculateLightStress(environment.LightIntensity);
            
            // CO2 stress
            factors.CO2Stress = CalculateCO2Stress(environment.CO2Level);
            
            // Calculate total stress
            factors.TotalStress = (factors.TemperatureStress + factors.HumidityStress + 
                                 factors.LightStress + factors.CO2Stress) / 4f;
            
            return factors;
        }
        
        private float CalculateTemperatureStress(float temperature)
        {
            // Optimal temperature range for cannabis: 18-26°C
            var optimalMin = 18f;
            var optimalMax = 26f;
            
            if (temperature >= optimalMin && temperature <= optimalMax)
                return 0f;
            
            if (temperature < optimalMin)
                return Mathf.Clamp01((optimalMin - temperature) / 10f);
            
            return Mathf.Clamp01((temperature - optimalMax) / 15f);
        }
        
        private float CalculateHumidityStress(float humidity)
        {
            // Optimal humidity range: 40-60%
            var optimalMin = 40f;
            var optimalMax = 60f;
            
            if (humidity >= optimalMin && humidity <= optimalMax)
                return 0f;
            
            if (humidity < optimalMin)
                return Mathf.Clamp01((optimalMin - humidity) / 30f);
            
            return Mathf.Clamp01((humidity - optimalMax) / 40f);
        }
        
        private float CalculateLightStress(float lightIntensity)
        {
            // Optimal light intensity: 600-1000 PPFD
            var optimalMin = 600f;
            var optimalMax = 1000f;
            
            if (lightIntensity >= optimalMin && lightIntensity <= optimalMax)
                return 0f;
            
            if (lightIntensity < optimalMin)
                return Mathf.Clamp01((optimalMin - lightIntensity) / 400f);
            
            return Mathf.Clamp01((lightIntensity - optimalMax) / 500f);
        }
        
        private float CalculateCO2Stress(float co2Level)
        {
            // Optimal CO2 range: 1000-1500 ppm
            var optimalMin = 1000f;
            var optimalMax = 1500f;
            
            if (co2Level >= optimalMin && co2Level <= optimalMax)
                return 0f;
            
            if (co2Level < optimalMin)
                return Mathf.Clamp01((optimalMin - co2Level) / 600f);
            
            return Mathf.Clamp01((co2Level - optimalMax) / 1000f);
        }
        
        private EnvironmentalStressLevel DetermineStressLevel(EnvironmentalStressFactors factors)
        {
            var totalStress = factors.TotalStress;
            
            return totalStress switch
            {
                < 0.1f => EnvironmentalStressLevel.None,
                < 0.3f => EnvironmentalStressLevel.Mild,
                < 0.6f => EnvironmentalStressLevel.Moderate,
                < 0.8f => EnvironmentalStressLevel.High,
                _ => EnvironmentalStressLevel.Severe
            };
        }
        
        private void UpdateEnvironmentalAdaptations(EnvironmentalResponseState state, 
            EnvironmentalConditions environment, float deltaTime)
        {
            var adaptationRate = 0.1f * deltaTime;
            
            // Temperature adaptation
            var tempOptimality = 1f - CalculateTemperatureStress(environment.Temperature);
            state.TemperatureAdaptation = Mathf.Lerp(state.TemperatureAdaptation, tempOptimality, adaptationRate);
            
            // Humidity adaptation
            var humidityOptimality = 1f - CalculateHumidityStress(environment.Humidity);
            state.HumidityAdaptation = Mathf.Lerp(state.HumidityAdaptation, humidityOptimality, adaptationRate);
            
            // Light adaptation
            var lightOptimality = 1f - CalculateLightStress(environment.LightIntensity);
            state.LightAdaptation = Mathf.Lerp(state.LightAdaptation, lightOptimality, adaptationRate);
        }
        
        private void ApplyStressVisualization(SpeedTreeRenderer renderer, EnvironmentalResponseState state)
        {
            #if UNITY_SPEEDTREE
            var rendererComponent = renderer.GetComponent<Renderer>();
            if (rendererComponent?.materials == null) return;
            
            foreach (var material in rendererComponent.materials)
            {
                if (material != null)
                {
                    material.SetFloat("_StressLevel", state.AccumulatedStress);
                    material.SetInt("_StressType", (int)state.CurrentStressLevel);
                    
                    // Color adjustments based on stress
                    var stressColor = GetStressColor(state.CurrentStressLevel);
                    material.SetColor("_StressColor", stressColor);
                }
            }
            #endif
        }
        
        private Color GetStressColor(EnvironmentalStressLevel stressLevel)
        {
            return stressLevel switch
            {
                EnvironmentalStressLevel.None => Color.green,
                EnvironmentalStressLevel.Mild => Color.yellow,
                EnvironmentalStressLevel.Moderate => new Color(1f, 0.5f, 0f), // Orange
                EnvironmentalStressLevel.High => Color.red,
                EnvironmentalStressLevel.Severe => Color.magenta,
                _ => Color.white
            };
        }
        
        private void ApplyEnvironmentalColorEffects(SpeedTreeRenderer renderer, 
            EnvironmentalConditions environment, EnvironmentalResponseState state)
        {
            #if UNITY_SPEEDTREE
            var rendererComponent = renderer.GetComponent<Renderer>();
            if (rendererComponent?.materials == null) return;
            
            foreach (var material in rendererComponent.materials)
            {
                if (material != null)
                {
                    // Adjust color based on environmental conditions
                    var colorModification = CalculateEnvironmentalColorModification(environment, state);
                    material.SetColor("_EnvironmentalTint", colorModification);
                }
            }
            #endif
        }
        
        private Color CalculateEnvironmentalColorModification(EnvironmentalConditions environment, 
            EnvironmentalResponseState state)
        {
            var baseColor = Color.white;
            
            // Light intensity affects color saturation
            var lightFactor = Mathf.Clamp01(environment.LightIntensity / 1000f);
            
            // Temperature affects red/blue balance
            var tempFactor = Mathf.Clamp01((environment.Temperature - 20f) / 15f);
            
            return new Color(
                baseColor.r * (0.8f + 0.2f * tempFactor),
                baseColor.g * lightFactor,
                baseColor.b * (1.2f - 0.2f * tempFactor),
                baseColor.a
            );
        }
        
        private void ApplyEnvironmentalMorphologyChanges(SpeedTreeRenderer renderer, 
            EnvironmentalConditions environment, EnvironmentalResponseState state)
        {
            #if UNITY_SPEEDTREE
            // Adjust plant morphology based on environmental stress
            var morphologyScale = CalculateMorphologyScale(state);
            
            // Apply subtle scale changes based on environmental adaptation
            var currentScale = renderer.transform.localScale;
            var targetScale = currentScale * morphologyScale;
            
            renderer.transform.localScale = Vector3.Lerp(currentScale, targetScale, Time.deltaTime * 0.1f);
            #endif
        }
        
        private float CalculateMorphologyScale(EnvironmentalResponseState state)
        {
            // Plants under stress tend to be smaller
            var averageAdaptation = (state.TemperatureAdaptation + state.HumidityAdaptation + state.LightAdaptation) / 3f;
            return Mathf.Lerp(0.8f, 1.1f, averageAdaptation);
        }
        
        private void UpdatePlantSeasonalEffects(int instanceId, SeasonalState seasonalState, SeasonalConditions season)
        {
            seasonalState.CurrentSeason = season.CurrentSeason;
            seasonalState.SeasonProgress = season.SeasonProgress;
            
            // Update seasonal adaptations
            if (!seasonalState.SeasonalAdaptations.ContainsKey(season.CurrentSeason))
            {
                seasonalState.SeasonalAdaptations[season.CurrentSeason] = 0f;
            }
            
            var adaptationRate = 0.05f * Time.deltaTime;
            seasonalState.SeasonalAdaptations[season.CurrentSeason] += adaptationRate;
            seasonalState.SeasonalAdaptations[season.CurrentSeason] = Mathf.Clamp01(seasonalState.SeasonalAdaptations[season.CurrentSeason]);
        }
    }
    
    /// <summary>
    /// Environmental response state for individual plants.
    /// </summary>
    [System.Serializable]
    public class EnvironmentalResponseState
    {
        public int InstanceId;
        public SpeedTreePlantData PlantData;
        public bool IsActive = true;
        public float LastUpdateTime;
        public EnvironmentalStressLevel CurrentStressLevel;
        public float TemperatureAdaptation;
        public float HumidityAdaptation;
        public float LightAdaptation;
        public float LastEnvironmentalUpdate;
        public float AccumulatedStress;
    }
    
    /// <summary>
    /// Seasonal state tracking for plants.
    /// </summary>
    [System.Serializable]
    public class SeasonalState
    {
        public int InstanceId;
        public Season CurrentSeason;
        public float SeasonProgress;
        public bool IsActive = true;
        public Dictionary<Season, float> SeasonalAdaptations = new Dictionary<Season, float>();
    }
    
    /// <summary>
    /// Environmental stress factors calculation.
    /// </summary>
    [System.Serializable]
    public class EnvironmentalStressFactors
    {
        public float TemperatureStress;
        public float HumidityStress;
        public float LightStress;
        public float CO2Stress;
        public float TotalStress;
    }
    
    /// <summary>
    /// Environmental service statistics.
    /// </summary>
    [System.Serializable]
    public class EnvironmentalServiceStats
    {
        public int MonitoredPlants;
        public int StressedPlants;
        public float AverageStressLevel;
        public bool EnvironmentalResponseEnabled;
        public bool SeasonalChangesEnabled;
        public bool StressVisualizationEnabled;
        public float UpdateFrequency;
    }
    
    /// <summary>
    /// Environmental stress levels.
    /// </summary>
    public enum EnvironmentalStressLevel
    {
        None,
        Mild,
        Moderate,
        High,
        Severe
    }
    
    
    /// <summary>
    /// Seasonal conditions.
    /// </summary>
    [System.Serializable]
    public class SeasonalConditions
    {
        public Season CurrentSeason;
        public float SeasonProgress;
        public float DayLength;
        public float AverageTemperature;
    }
    
    /// <summary>
    /// Seasons enumeration.
    /// </summary>
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }
}