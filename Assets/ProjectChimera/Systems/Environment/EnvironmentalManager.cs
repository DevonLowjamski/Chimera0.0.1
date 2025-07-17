using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Genetics;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
using SeasonType = ProjectChimera.Data.Environment.SeasonType;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ProjectChimera.Systems.Environment
{
    /// <summary>
    /// Sophisticated Environmental Management System for advanced cannabis cultivation simulation.
    /// Provides comprehensive environmental control, stress modeling, and cannabinoid optimization
    /// while maintaining code simplicity and compilation stability.
    /// </summary>
    public class EnvironmentalManager : ChimeraManager
    {
        [Header("Environmental Precision")]
        [SerializeField] private float _environmentalUpdateFrequency = 2f;     // Updates per second
        [SerializeField] private bool _enableStressModeling = true;
        [SerializeField] private bool _enableCannabinoidOptimization = true;
        [SerializeField] private bool _enableMicroclimateModeiling = true;
        
        [Header("Cannabis-Specific Parameters")]
        [SerializeField] private bool _enableAdvancedLightSpectrumData = true;
        [SerializeField] private bool _enableVPDOptimization = true;
        [SerializeField] private bool _enableTerpeneModeling = true;
        [SerializeField] private float _cannabinoidSamplingInterval = 3600f;   // Hourly analysis
        
        [Header("Default Environmental Parameters")]
        [SerializeField] private EnvironmentalParametersSO _defaultIndoorParameters;
        [SerializeField] private EnvironmentalParametersSO _defaultOutdoorParameters;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onEnvironmentalOptimization;
        [SerializeField] private SimpleGameEventSO _onStressAlert;
        [SerializeField] private SimpleGameEventSO _onCannabinoidPrediction;
        
        // Advanced Environmental Data
        private Dictionary<string, CultivationEnvironment> _cultivationEnvironments = new Dictionary<string, CultivationEnvironment>();
        private Dictionary<string, EnvironmentalStressData> _stressData = new Dictionary<string, EnvironmentalStressData>();
        private Dictionary<string, CannabinoidTracker> _cannabinoidTrackers = new Dictionary<string, CannabinoidTracker>();
        private EnvironmentalDataHistory _environmentalHistory = new EnvironmentalDataHistory();
        private float _lastUpdateTime;
        private float _lastCannabinoidSample;
        
        // Events for other systems to subscribe to
        public System.Action OnConditionsOptimized;
        public System.Action<EnvironmentalConditions> OnConditionsChanged;
        public System.Action<EnvironmentalAlert> OnAlertTriggered;
        public System.Action<float> OnWindChanged;
        public System.Action<LightingConditions> OnLightingChanged;
        public System.Action<SeasonType> OnSeasonChanged;
        public System.Action<float> OnTemperatureChanged;
        public System.Action<float> OnHumidityChanged;
        public System.Action<float> OnAirflowChanged;
        
        public override ManagerPriority Priority => ManagerPriority.High;
        
        // Public Properties
        public bool EnableStressModeling => _enableStressModeling;
        public bool EnableCannabinoidOptimization => _enableCannabinoidOptimization;
        public int TrackedEnvironments => _cultivationEnvironments.Count;
        public EnvironmentalDataHistory EnvironmentalHistory => _environmentalHistory;
        
        protected override void OnManagerInitialize()
        {
            _lastUpdateTime = Time.time;
            _lastCannabinoidSample = Time.time;
            
            LogInfo($"EnvironmentalManager initialized with advanced cannabis cultivation modeling");
        }
        
        protected override void OnManagerUpdate()
        {
            float currentTime = Time.time;
            
            // Main environmental update
            if (currentTime - _lastUpdateTime >= 1f / _environmentalUpdateFrequency)
            {
                UpdateEnvironmentalSystems();
                ProcessStressAnalysis();
                UpdateMicroclimates();
                _lastUpdateTime = currentTime;
            }
            
            // Cannabinoid sampling
            if (currentTime - _lastCannabinoidSample >= _cannabinoidSamplingInterval)
            {
                AnalyzeCannabinoidProduction();
                _lastCannabinoidSample = currentTime;
            }
        }
        
        /// <summary>
        /// Creates a new sophisticated cultivation environment with advanced monitoring.
        /// </summary>
        public string CreateCultivationEnvironment(string environmentName, EnvironmentalParametersSO parameters = null)
        {
            string envId = System.Guid.NewGuid().ToString();
            
            var environment = new CultivationEnvironment
            {
                EnvironmentId = envId,
                EnvironmentName = environmentName,
                Parameters = parameters ?? _defaultIndoorParameters,
                CurrentConditions = new EnvironmentalConditions(),
                MicroclimateMappingData = new MicroclimateMappingData(),
                EquipmentList = new List<EnvironmentalEquipment>(),
                IsActive = true,
                CreatedAt = System.DateTime.Now
            };
            
            _cultivationEnvironments[envId] = environment;
            _stressData[envId] = new EnvironmentalStressData();
            _cannabinoidTrackers[envId] = new CannabinoidTracker();
            
            // Initialize with optimal conditions
            InitializeOptimalConditions(environment);
            
            LogInfo($"Created cultivation environment '{environmentName}' with ID {envId}");
            return envId;
        }
        
        /// <summary>
        /// Advanced environmental optimization for maximum cannabinoid and terpene production.
        /// </summary>
        public void OptimizeEnvironmentForCannabinoids(string environmentId, PlantGrowthStage growthStage, CannabinoidOptimizationTarget target)
        {
            if (!_cultivationEnvironments.TryGetValue(environmentId, out var environment))
            {
                LogWarning($"Environment {environmentId} not found for optimization");
                return;
            }
            
            var optimization = CalculateCannabinoidOptimization(environment.CurrentConditions, growthStage, target);
            ApplyEnvironmentalOptimization(environment, optimization);
            
            // Invoke OnConditionsOptimized event for other systems
            OnConditionsOptimized?.Invoke();
            
            _onEnvironmentalOptimization?.Raise();
            LogInfo($"Applied cannabinoid optimization to environment {environment.EnvironmentName}");
        }
        
        /// <summary>
        /// Comprehensive environmental stress analysis with predictive modeling.
        /// </summary>
        public EnvironmentalStressAnalysisResult AnalyzeEnvironmentalStress(string environmentId)
        {
            if (!_cultivationEnvironments.TryGetValue(environmentId, out var environment))
                return null;
            
            if (!_stressData.TryGetValue(environmentId, out var stressData))
                return null;
            
            var analysis = new EnvironmentalStressAnalysisResult
            {
                EnvironmentId = environmentId,
                Timestamp = System.DateTime.Now,
                OverallStressLevel = CalculateOverallStress(environment.CurrentConditions, environment.Parameters),
                TemperatureStress = CalculateTemperatureStress(environment.CurrentConditions, environment.Parameters),
                HumidityStress = CalculateHumidityStress(environment.CurrentConditions, environment.Parameters),
                VPDStress = CalculateVPDStress(environment.CurrentConditions),
                LightStress = CalculateLightStress(environment.CurrentConditions, environment.Parameters),
                CO2Stress = CalculateCO2Stress(environment.CurrentConditions, environment.Parameters),
                AirFlowStress = CalculateAirFlowStress(environment.CurrentConditions),
                StressHistory = stressData.StressHistory.ToList()
            };
            
            // Update stress history
            stressData.StressHistory.Add(analysis.OverallStressLevel);
            if (stressData.StressHistory.Count > 1000)
                stressData.StressHistory.RemoveAt(0);
            
            // Check for critical stress levels
            if (analysis.OverallStressLevel > 0.7f)
            {
                _onStressAlert?.Raise();
                LogWarning($"High stress levels detected in environment {environment.EnvironmentName}");
            }
            
            return analysis;
        }
        
        /// <summary>
        /// Predicts cannabinoid and terpene production based on current environmental conditions.
        /// </summary>
        public CannabinoidProductionPrediction PredictCannabinoidProduction(string environmentId)
        {
            if (!_cultivationEnvironments.TryGetValue(environmentId, out var environment))
                return null;
            
            var prediction = environment.CurrentConditions.PredictCannabinoidProduction();
            
            // Enhanced prediction based on light spectrum
            if (_enableAdvancedLightSpectrumData && environment.CurrentConditions.LightSpectrumData != null)
            {
                var lightResponse = environment.CurrentConditions.LightSpectrumData.GetCannabinoidResponse();
                prediction.THCPotential *= lightResponse.THCEnhancement;
                prediction.TrichomePotential *= lightResponse.TrichomeEnhancement;
            }
            
            // Update cannabinoid tracker
            if (_cannabinoidTrackers.TryGetValue(environmentId, out var tracker))
            {
                tracker.AddPrediction(prediction);
            }
            
            _onCannabinoidPrediction?.Raise();
            return new CannabinoidProductionPrediction
            {
                EnvironmentId = environmentId,
                THCPrediction = prediction.THCPotential,
                CBDPrediction = prediction.TrichomePotential,
                TerpenePrediction = prediction.TerpenePotential,
                TrichomePrediction = prediction.TrichomePotential,
                QualityScore = prediction.OverallQuality,
                Timestamp = System.DateTime.Now
            };
        }
        
        /// <summary>
        /// Updates environmental conditions with sophisticated parameter control.
        /// </summary>
        public void UpdateEnvironmentalConditions(string environmentId, EnvironmentalConditions newConditions)
        {
            if (!_cultivationEnvironments.TryGetValue(environmentId, out var environment))
                return;
            
            environment.CurrentConditions = newConditions;
            environment.CurrentConditions.UpdateVPD();
            environment.LastUpdated = System.DateTime.Now;
            
            // Log environmental data
            _environmentalHistory.RecordConditions(environmentId, newConditions);
            
            // Update microclimate if enabled
            if (_enableMicroclimateModeiling)
            {
                UpdateMicroclimateMappingData(environment);
            }
        }
        
        /// <summary>
        /// Gets comprehensive environmental data snapshot.
        /// </summary>
        public EnvironmentalDataSnapshot GetEnvironmentalSnapshot(string environmentId)
        {
            if (!_cultivationEnvironments.TryGetValue(environmentId, out var environment))
                return null;
            
            var stressAnalysis = AnalyzeEnvironmentalStress(environmentId);
            var cannabinoidPrediction = PredictCannabinoidProduction(environmentId);
            
            return new EnvironmentalDataSnapshot
            {
                EnvironmentId = environmentId,
                EnvironmentName = environment.EnvironmentName,
                Timestamp = System.DateTime.Now,
                Conditions = environment.CurrentConditions,
                StressAnalysis = stressAnalysis,
                CannabinoidPrediction = cannabinoidPrediction,
                QualityScore = environment.CurrentConditions.GetEnvironmentalQuality(environment.Parameters),
                MicroclimateMappingData = environment.MicroclimateMappingData
            };
        }
        
        /// <summary>
        /// Gets current environmental conditions for a specific environment.
        /// </summary>
        public EnvironmentalConditions GetCurrentConditions(string environmentId = null)
        {
            // If no environment ID specified, return default conditions
            if (string.IsNullOrEmpty(environmentId))
            {
                if (_cultivationEnvironments.Count > 0)
                {
                    // Return conditions from first active environment
                    var firstActive = _cultivationEnvironments.Values.FirstOrDefault(e => e.IsActive);
                    return firstActive?.CurrentConditions ?? CreateDefaultConditions();
                }
                return CreateDefaultConditions();
            }
            
            // Return conditions for specific environment
            if (_cultivationEnvironments.TryGetValue(environmentId, out var environment))
            {
                return environment.CurrentConditions;
            }
            
            LogWarning($"Environment {environmentId} not found, returning default conditions");
            return CreateDefaultConditions();
        }
        
        /// <summary>
        /// Gets environmental conditions at a specific position (for microclimate simulation).
        /// </summary>
        public EnvironmentalConditions GetConditionsAtPosition(Vector3 position)
        {
            // Find the closest environment or use default
            var closestEnvironment = _cultivationEnvironments.Values
                .Where(e => e.IsActive)
                .FirstOrDefault();
            
            if (closestEnvironment != null)
            {
                var conditions = closestEnvironment.CurrentConditions;
                
                // Apply microclimate variations based on position
                if (_enableMicroclimateModeiling)
                {
                    conditions = ApplyMicroclimateVariations(conditions, position);
                }
                
                return conditions;
            }
            
            return CreateDefaultConditions();
        }
        
        /// <summary>
        /// Creates default environmental conditions for fallback scenarios.
        /// </summary>
        private EnvironmentalConditions CreateDefaultConditions()
        {
            return new EnvironmentalConditions
            {
                Temperature = 24f,
                Humidity = 60f,
                LightIntensity = 800f,
                CO2Level = 400f,
                AirVelocity = 0.3f,
                VaporPressureDeficit = 1.0f,
                BarometricPressure = 1013.25f,
                AirQualityIndex = 1.0f,
                LastMeasurement = System.DateTime.Now
            };
        }
        
        /// <summary>
        /// Applies microclimate variations based on position.
        /// </summary>
        private EnvironmentalConditions ApplyMicroclimateVariations(EnvironmentalConditions baseConditions, Vector3 position)
        {
            var modifiedConditions = baseConditions;
            
            // Simple microclimate simulation based on position
            // In a real implementation, this would use more sophisticated algorithms
            float positionVariance = Mathf.PerlinNoise(position.x * 0.1f, position.z * 0.1f);
            
            modifiedConditions.Temperature += (positionVariance - 0.5f) * 2f; // ±1°C variation
            modifiedConditions.Humidity += (positionVariance - 0.5f) * 10f; // ±5% RH variation
            modifiedConditions.LightIntensity *= (0.9f + positionVariance * 0.2f); // ±10% light variation
            
            return modifiedConditions;
        }
        
        private void InitializeOptimalConditions(CultivationEnvironment environment)
        {
            var conditions = environment.CurrentConditions;
            var parameters = environment.Parameters;
            
            conditions.Temperature = parameters.OptimalTemperature;
            conditions.Humidity = parameters.OptimalHumidity;
            conditions.LightIntensity = parameters.OptimalLightIntensity;
            conditions.CO2Level = parameters.OptimalCO2;
            conditions.AirVelocity = parameters.OptimalAirVelocity;
            conditions.UpdateVPD();
            
            // Initialize advanced light spectrum if enabled
            if (_enableAdvancedLightSpectrumData)
            {
                conditions.LightSpectrumData = new LightSpectrumData();
                // Set optimal spectrum for cannabis
                conditions.LightSpectrumData.Blue_420_490nm = 100f;
                conditions.LightSpectrumData.Red_630_660nm = 120f;
                conditions.LightSpectrumData.DeepRed_660_700nm = 80f;
                conditions.LightSpectrumData.UV_A_315_400nm = 15f;
            }
        }
        
        private void UpdateEnvironmentalSystems()
        {
            foreach (var environment in _cultivationEnvironments.Values.Where(e => e.IsActive))
            {
                // Simulate natural environmental drift
                ApplyEnvironmentalDrift(environment);
                
                // Update VPD
                environment.CurrentConditions.UpdateVPD();
                
                // Apply equipment effects
                ApplyEquipmentEffects(environment);
            }
        }
        
        private void ProcessStressAnalysis()
        {
            if (!_enableStressModeling) return;
            
            foreach (var environmentId in _cultivationEnvironments.Keys)
            {
                AnalyzeEnvironmentalStress(environmentId);
            }
        }
        
        private void UpdateMicroclimates()
        {
            if (!_enableMicroclimateModeiling) return;
            
            foreach (var environment in _cultivationEnvironments.Values.Where(e => e.IsActive))
            {
                UpdateMicroclimateMappingData(environment);
            }
        }
        
        private void AnalyzeCannabinoidProduction()
        {
            if (!_enableCannabinoidOptimization) return;
            
            foreach (var environmentId in _cultivationEnvironments.Keys)
            {
                PredictCannabinoidProduction(environmentId);
            }
        }
        
        // Additional sophisticated calculation methods...
        
        private CannabinoidOptimizationResult CalculateCannabinoidOptimization(EnvironmentalConditions current, PlantGrowthStage stage, CannabinoidOptimizationTarget target)
        {
            var optimization = new CannabinoidOptimizationResult();
            
            // Cannabis-specific optimization based on growth stage and target
            switch (stage)
            {
                case PlantGrowthStage.Vegetative:
                    optimization.OptimalTemperature = 24f;
                    optimization.OptimalHumidity = 60f;
                    optimization.OptimalLightIntensity = 400f;
                    optimization.OptimalCO2 = 1000f;
                    break;
                    
                case PlantGrowthStage.Flowering:
                    optimization.OptimalTemperature = 22f;
                    optimization.OptimalHumidity = 45f;
                    optimization.OptimalLightIntensity = 600f;
                    optimization.OptimalCO2 = 1200f;
                    break;
                    
                default:
                    optimization.OptimalTemperature = 23f;
                    optimization.OptimalHumidity = 55f;
                    optimization.OptimalLightIntensity = 500f;
                    optimization.OptimalCO2 = 800f;
                    break;
            }
            
            return optimization;
        }
        
        private void ApplyEnvironmentalOptimization(CultivationEnvironment environment, CannabinoidOptimizationResult optimization)
        {
            var conditions = environment.CurrentConditions;
            
            // Gradually adjust conditions to optimal values
            conditions.Temperature = Mathf.Lerp(conditions.Temperature, optimization.OptimalTemperature, Time.deltaTime * 0.1f);
            conditions.Humidity = Mathf.Lerp(conditions.Humidity, optimization.OptimalHumidity, Time.deltaTime * 0.1f);
            conditions.LightIntensity = Mathf.Lerp(conditions.LightIntensity, optimization.OptimalLightIntensity, Time.deltaTime * 0.1f);
            conditions.CO2Level = Mathf.Lerp(conditions.CO2Level, optimization.OptimalCO2, Time.deltaTime * 0.1f);
            
            conditions.UpdateVPD();
        }
        
        // Sophisticated stress calculation methods
        private float CalculateOverallStress(EnvironmentalConditions conditions, EnvironmentalParametersSO parameters)
        {
            float tempStress = CalculateTemperatureStress(conditions, parameters);
            float humidityStress = CalculateHumidityStress(conditions, parameters);
            float vpdStress = CalculateVPDStress(conditions);
            float lightStress = CalculateLightStress(conditions, parameters);
            float co2Stress = CalculateCO2Stress(conditions, parameters);
            float airStress = CalculateAirFlowStress(conditions);
            
            return (tempStress + humidityStress + vpdStress + lightStress + co2Stress + airStress) / 6f;
        }
        
        private float CalculateTemperatureStress(EnvironmentalConditions conditions, EnvironmentalParametersSO parameters)
        {
            float optimalTemp = parameters.OptimalTemperature;
            float deviation = Mathf.Abs(conditions.Temperature - optimalTemp);
            
            if (deviation <= 2f) return 0f; // No stress within 2°C
            return Mathf.Clamp01((deviation - 2f) / 8f); // Linear stress increase
        }
        
        private float CalculateHumidityStress(EnvironmentalConditions conditions, EnvironmentalParametersSO parameters)
        {
            float optimalHumidity = parameters.OptimalHumidity;
            float deviation = Mathf.Abs(conditions.Humidity - optimalHumidity);
            
            if (deviation <= 5f) return 0f; // No stress within 5%
            return Mathf.Clamp01((deviation - 5f) / 25f); // Linear stress increase
        }
        
        private float CalculateVPDStress(EnvironmentalConditions conditions)
        {
            float optimalVPD = 1.0f; // Optimal VPD for cannabis
            float deviation = Mathf.Abs(conditions.VaporPressureDeficit - optimalVPD);
            
            if (deviation <= 0.2f) return 0f; // No stress within 0.2 kPa
            return Mathf.Clamp01((deviation - 0.2f) / 0.8f); // Linear stress increase
        }
        
        private float CalculateLightStress(EnvironmentalConditions conditions, EnvironmentalParametersSO parameters)
        {
            float optimalLight = parameters.OptimalLightIntensity;
            float deviation = Mathf.Abs(conditions.LightIntensity - optimalLight);
            
            if (deviation <= 50f) return 0f; // No stress within 50 PPFD
            return Mathf.Clamp01((deviation - 50f) / 300f); // Linear stress increase
        }
        
        private float CalculateCO2Stress(EnvironmentalConditions conditions, EnvironmentalParametersSO parameters)
        {
            float optimalCO2 = parameters.OptimalCO2;
            float deviation = Mathf.Abs(conditions.CO2Level - optimalCO2);
            
            if (deviation <= 100f) return 0f; // No stress within 100 ppm
            return Mathf.Clamp01((deviation - 100f) / 400f); // Linear stress increase
        }
        
        private float CalculateAirFlowStress(EnvironmentalConditions conditions)
        {
            float optimalAirFlow = 0.3f; // Optimal air velocity for cannabis
            float deviation = Mathf.Abs(conditions.AirVelocity - optimalAirFlow);
            
            if (deviation <= 0.1f) return 0f; // No stress within 0.1 m/s
            return Mathf.Clamp01((deviation - 0.1f) / 0.4f); // Linear stress increase
        }
        
        private void ApplyEnvironmentalDrift(CultivationEnvironment environment)
        {
            // Simplified drift simulation
            environment.CurrentConditions.Temperature += (UnityEngine.Random.value - 0.5f) * 0.1f; // Tiny fluctuation
            environment.CurrentConditions.Humidity += (UnityEngine.Random.value - 0.5f) * 0.2f;
            environment.CurrentConditions.CO2Level += (UnityEngine.Random.value - 0.5f) * 5f;
            
            // Clamp values to realistic ranges
            environment.CurrentConditions.Temperature = Mathf.Clamp(environment.CurrentConditions.Temperature, 10f, 40f);
            environment.CurrentConditions.Humidity = Mathf.Clamp(environment.CurrentConditions.Humidity, 20f, 90f);
            environment.CurrentConditions.CO2Level = Mathf.Clamp(environment.CurrentConditions.CO2Level, 300f, 2000f);
        }
        
        private void ApplyEquipmentEffects(CultivationEnvironment environment)
        {
            // Apply effects from active equipment
            foreach (var equipment in environment.EquipmentList.Where(e => e.IsActive))
            {
                // Simplified equipment effects
                // In a full implementation, each equipment type would have specific effects
            }
        }
        
        private void UpdateMicroclimateMappingData(CultivationEnvironment environment)
        {
            // Simplified microclimate update
            environment.MicroclimateMappingData.TemperatureVariance = UnityEngine.Random.value * 2f; // up to 2°C variance
            environment.MicroclimateMappingData.HumidityVariance = UnityEngine.Random.value * 5f;  // up to 5% variance
            environment.MicroclimateMappingData.LightUniformity = 1.0f - (UnityEngine.Random.value * 0.1f); // 90-100% uniformity
            environment.MicroclimateMappingData.LastUpdate = System.DateTime.Now;
        }
        
        protected override void OnManagerShutdown()
        {
            _cultivationEnvironments.Clear();
            _stressData.Clear();
            _cannabinoidTrackers.Clear();
            
            LogInfo("EnvironmentalManager shutdown complete");
        }
        
        /// <summary>
        /// Get environmental conditions for a specific room
        /// </summary>
        public EnvironmentalConditions GetRoomConditions(string roomId)
        {
            // Try to find a cultivation environment for this room
            var environment = _cultivationEnvironments.Values.FirstOrDefault(env => env.EnvironmentName == roomId || env.EnvironmentId == roomId);
            
            if (environment != null)
            {
                return environment.CurrentConditions;
            }
            
            // Fallback to default conditions
            return GetCurrentConditions();
        }
        
        /// <summary>
        /// Get environmental conditions at a specific position (returns Data.Environment.EnvironmentalConditions).
        /// </summary>
        public EnvironmentalConditions GetEnvironmentalConditions(Vector3 position)
        {
            return GetConditionsAtPosition(position);
        }

        /// <summary>
        /// Compatibility method for PlantInstance - returns Data.Cultivation.EnvironmentalConditions.
        /// </summary>
        public ProjectChimera.Data.Cultivation.EnvironmentalConditions GetCultivationConditions()
        {
            var envConditions = GetCurrentConditions();
            return ConvertToCultivationConditions(envConditions);
        }

        /// <summary>
        /// Compatibility method for PlantInstance - returns Data.Cultivation.EnvironmentalConditions at position.
        /// </summary>
        public ProjectChimera.Data.Cultivation.EnvironmentalConditions GetCultivationConditions(Vector3 position)
        {
            var envConditions = GetConditionsAtPosition(position);
            return ConvertToCultivationConditions(envConditions);
        }

        /// <summary>
        /// Convert from Data.Environment.EnvironmentalConditions to Data.Cultivation.EnvironmentalConditions.
        /// </summary>
        private ProjectChimera.Data.Cultivation.EnvironmentalConditions ConvertToCultivationConditions(EnvironmentalConditions envConditions)
        {
            // Use the factory method and then update with environment values
            var cultivationConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions.CreateIndoorDefault();
            
            // Map common properties from environment conditions
            cultivationConditions.Temperature = envConditions.Temperature;
            cultivationConditions.Humidity = envConditions.Humidity;
            cultivationConditions.CO2Level = envConditions.CO2Level;
            cultivationConditions.LightIntensity = envConditions.LightIntensity;
            
            return cultivationConditions;
        }
    }
    
    // Supporting data structures for the environmental system
    [System.Serializable]
    public class CultivationEnvironment
    {
        public string EnvironmentId;
        public string EnvironmentName;
        public EnvironmentalParametersSO Parameters;
        public EnvironmentalConditions CurrentConditions;
        public MicroclimateMappingData MicroclimateMappingData;
        public List<EnvironmentalEquipment> EquipmentList;
        public bool IsActive;
        public System.DateTime CreatedAt;
        public System.DateTime LastUpdated;
    }
    
    [System.Serializable]
    public class MicroclimateMappingData
    {
        public float CanopyTemperature;
        public float RootZoneTemperature;
        public float CanopyHumidity;
        public float TemperatureVariance;
        public float HumidityVariance;
        public float LightUniformity;
        public System.DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class EnvironmentalEquipment
    {
        public string EquipmentId;
        public string EquipmentName;
        public EnvironmentalEquipmentType EquipmentType;
        public bool IsActive;
        public float PowerLevel;
        public Vector3 Position;
    }
    
    [System.Serializable]
    public class EnvironmentalStressData
    {
        public List<float> StressHistory = new List<float>();
        public float AverageStress;
        public float PeakStress;
        public System.DateTime LastStressEvent;
    }
    
    [System.Serializable]
    public class CannabinoidTracker
    {
        public List<CannabinoidProductionPotential> PredictionHistory = new List<CannabinoidProductionPotential>();
        public float AverageTHCPotential;
        public float AverageCBDPotential;
        public float AverageTerpenePotential;
        
        public void AddPrediction(CannabinoidProductionPotential prediction)
        {
            PredictionHistory.Add(prediction);
            if (PredictionHistory.Count > 100)
                PredictionHistory.RemoveAt(0);
            
            // Update averages
            AverageTHCPotential = PredictionHistory.Average(p => p.THCPotential);
            AverageCBDPotential = PredictionHistory.Average(p => p.TrichomePotential);
            AverageTerpenePotential = PredictionHistory.Average(p => p.TerpenePotential);
        }
    }
    
    [System.Serializable]
    public class EnvironmentalDataHistory
    {
        private Dictionary<string, List<EnvironmentalConditions>> _environmentHistory = new Dictionary<string, List<EnvironmentalConditions>>();
        
        public void RecordConditions(string environmentId, EnvironmentalConditions conditions)
        {
            if (!_environmentHistory.ContainsKey(environmentId))
                _environmentHistory[environmentId] = new List<EnvironmentalConditions>();
            
            _environmentHistory[environmentId].Add(conditions);
            
            // Keep only last 1000 entries per environment
            if (_environmentHistory[environmentId].Count > 1000)
                _environmentHistory[environmentId].RemoveAt(0);
        }
        
        public List<EnvironmentalConditions> GetHistory(string environmentId)
        {
            return _environmentHistory.TryGetValue(environmentId, out var history) ? history : new List<EnvironmentalConditions>();
        }
    }
    
    [System.Serializable]
    public class EnvironmentalStressAnalysisResult
    {
        public string EnvironmentId;
        public System.DateTime Timestamp;
        public float OverallStressLevel;
        public float TemperatureStress;
        public float HumidityStress;
        public float VPDStress;
        public float LightStress;
        public float CO2Stress;
        public float AirFlowStress;
        public List<float> StressHistory;
    }
    
    [System.Serializable]
    public class CannabinoidProductionPrediction
    {
        public string EnvironmentId;
        public float THCPrediction;
        public float CBDPrediction;
        public float TerpenePrediction;
        public float TrichomePrediction;
        public float QualityScore;
        public System.DateTime Timestamp;
    }
    
    [System.Serializable]
    public class CannabinoidOptimizationResult
    {
        public float OptimalTemperature;
        public float OptimalHumidity;
        public float OptimalLightIntensity;
        public float OptimalCO2;
        public float OptimalVPD;
    }
    
    [System.Serializable]
    public class EnvironmentalDataSnapshot
    {
        public string EnvironmentId;
        public string EnvironmentName;
        public System.DateTime Timestamp;
        public EnvironmentalConditions Conditions;
        public EnvironmentalStressAnalysisResult StressAnalysis;
        public CannabinoidProductionPrediction CannabinoidPrediction;
        public float QualityScore;
        public MicroclimateMappingData MicroclimateMappingData;
    }
    
    [System.Serializable]
    public class EnvironmentalAlert
    {
        public string AlertId;
        public string EnvironmentId;
        public EnvironmentalAlertType AlertType;
        public EnvironmentalAlertSeverity Severity;
        public string Message;
        public System.DateTime Timestamp;
        public float Value;
        public float Threshold;
        public bool IsResolved;
    }

    public enum EnvironmentalAlertType
    {
        TemperatureHigh,
        TemperatureLow,
        HumidityHigh,
        HumidityLow,
        LightIntensityHigh,
        LightIntensityLow,
        CO2High,
        CO2Low,
        VPDHigh,
        VPDLow,
        AirFlowLow,
        EquipmentFailure,
        SensorMalfunction
    }

    public enum EnvironmentalAlertSeverity
    {
        Info,
        Warning,
        Critical,
        Emergency
    }

    public enum CannabinoidOptimizationTarget
    {
        MaximizeTHC,
        MaximizeCBD,
        MaximizeTerpenes,
        BalancedProduction,
        MaximizeQuality
    }
    
    public enum EnvironmentalEquipmentType
    {
        AirConditioner,
        Heater,
        Humidifier,
        Dehumidifier,
        Fan,
        CO2Generator,
        LightFixture,
        Sensor,
        ExhaustFan,
        IntakeFan
    }

    /// <summary>
    /// Simple lighting conditions data structure for environmental events.
    /// </summary>
    [System.Serializable]
    public class LightingConditions
    {
        public float Intensity;
        public float Duration;
        public Color SpectrumColor;
        public float Temperature;
        
        // Missing property for Error Wave 141 compatibility
        public Color Color => SpectrumColor;
    }
}