using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using ProjectChimera.Data.Environment;

// Explicit alias for Data layer EnvironmentalConditions to resolve namespace conflicts
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Comprehensive data structures for the SpeedTree environmental response system.
    /// Includes plant states, adaptation tracking, environmental zones, stress management,
    /// and performance optimization data for realistic cannabis plant simulation.
    /// </summary>
    
    // Core Environmental Enums
    public enum EnvironmentalSeverity
    {
        None = 0,
        Mild = 1,
        Moderate = 2,
        Severe = 3,
        Critical = 4
    }
    
    public enum EnvironmentalEventType
    {
        StressEvent,
        Adaptation,
        Recovery,
        ZoneChange,
        ConditionChange,
        ThresholdCrossed
    }
    
    public enum ResponseType
    {
        Immediate,
        Gradual,
        Adaptive,
        Delayed,
        Cumulative
    }
    
    // Plant Environmental State
    [System.Serializable]
    public class PlantEnvironmentalState
    {
        public int InstanceId;
        public AdvancedSpeedTreeManager.SpeedTreePlantData PlantInstance;
        public string CurrentZone;
        public EnvironmentalConditions LastConditions;
        public EnvironmentalStressData StressData;
        public DateTime RegistrationTime;
        public float LastUpdateTime;
        public Dictionary<string, float> ResponseModifiers = new Dictionary<string, float>();
        public List<string> ActiveStressors = new List<string>();
        public float AdaptationLevel = 0f;
        public bool IsMonitored = true;
    }
    
    // Environmental Stress Data
    [System.Serializable]
    public class EnvironmentalStressData
    {
        public int InstanceId;
        public AdvancedSpeedTreeManager.SpeedTreePlantData PlantInstance;
        public float TemperatureStress;
        public float HumidityStress;
        public float LightStress;
        public float WaterStress;
        public float NutrientStress;
        public float CO2Stress;
        public float AirflowStress;
        public float OverallStressLevel;
        public DateTime LastUpdate;
        public Dictionary<string, float> StressFactors = new Dictionary<string, float>();
        public List<string> ActiveStressors = new List<string>();
        public bool IsUnderStress => OverallStressLevel > 0.3f;
        public bool IsCriticalStress => OverallStressLevel > 0.7f;
        
        // Cumulative stress tracking
        public float AccumulatedStress = 0f;
        public float PeakStress = 0f;
        public DateTime LastStressEvent;
        public float StressRecoveryRate = 0.1f;
        
        public float OverallStress => (TemperatureStress + HumidityStress + LightStress + 
                                      WaterStress + CO2Stress + NutrientStress + AirflowStress) / 7f;
        
        public EnvironmentalSeverity GetStressSeverity()
        {
            float stress = OverallStress;
            if (stress < 0.2f) return EnvironmentalSeverity.None;
            if (stress < 0.4f) return EnvironmentalSeverity.Mild;
            if (stress < 0.6f) return EnvironmentalSeverity.Moderate;
            if (stress < 0.8f) return EnvironmentalSeverity.Severe;
            return EnvironmentalSeverity.Critical;
        }
        
        public Color GetStressColor()
        {
            return GetStressSeverity() switch
            {
                EnvironmentalSeverity.None => Color.green,
                EnvironmentalSeverity.Mild => Color.yellow,
                EnvironmentalSeverity.Moderate => Color.orange,
                EnvironmentalSeverity.Severe => Color.red,
                EnvironmentalSeverity.Critical => Color.magenta,
                _ => Color.white
            };
        }
        
        // Copy constructor
        public EnvironmentalStressData(EnvironmentalStressData other)
        {
            TemperatureStress = other.TemperatureStress;
            HumidityStress = other.HumidityStress;
            LightStress = other.LightStress;
            WaterStress = other.WaterStress;
            CO2Stress = other.CO2Stress;
            NutrientStress = other.NutrientStress;
            AirflowStress = other.AirflowStress;
            OverallStressLevel = other.OverallStressLevel;
            AccumulatedStress = other.AccumulatedStress;
            PeakStress = other.PeakStress;
            LastStressEvent = other.LastStressEvent;
            StressRecoveryRate = other.StressRecoveryRate;
        }
        
        public EnvironmentalStressData() { }
    }
    
    // Environmental Response Data
    [System.Serializable]
    public class EnvironmentalResponse
    {
        public float GrowthRateModifier = 1f;
        public Color ColorModification = Color.clear;
        public float ColorIntensity = 0f;
        public Dictionary<string, float> LeafMorphologyChanges = new Dictionary<string, float>();
        public Dictionary<string, float> StemChanges = new Dictionary<string, float>();
        public float WindResponseModifier = 1f;
        public float AnimationSpeedModifier = 1f;
        public List<string> TriggeredEffects = new List<string>();
        public ResponseType ResponseType = ResponseType.Immediate;
        public float ResponseDuration = 0f;
        public float ResponseIntensity = 1f;
    }
    
    // Environmental Delta (Change Detection)
    [System.Serializable]
    public class EnvironmentalDelta
    {
        public float TemperatureDelta;
        public float HumidityDelta;
        public float LightIntensityDelta;
        public float CO2Delta;
        public float AirVelocityDelta;
        public float DeltaTime;
        public float TotalMagnitude => Mathf.Abs(TemperatureDelta) + Mathf.Abs(HumidityDelta) + 
                                       Mathf.Abs(LightIntensityDelta) + Mathf.Abs(CO2Delta) + 
                                       Mathf.Abs(AirVelocityDelta);
    }
    
    // Environmental Zones
    [System.Serializable]
    public class EnvironmentalZone
    {
        public string ZoneId;
        public string ZoneName;
        public Bounds ZoneBounds;
        public EnvironmentalConditions OptimalConditions;
        public EnvironmentalTolerances ToleranceRanges;
        public EnvironmentalConditions LastConditions;
        public Dictionary<string, float> EnvironmentalModifiers = new Dictionary<string, float>();
        public List<int> PlantsInZone = new List<int>();
        public Color ZoneVisualizationColor = Color.white;
        public bool IsActive = true;
        public float ZonePriority = 1f;
    }
    
    [System.Serializable]
    public class EnvironmentalTolerances
    {
        public Vector2 TemperatureRange;
        public Vector2 HumidityRange;
        public Vector2 LightRange;
        public Vector2 CO2Range;
        public Vector2 AirflowRange;
        public Vector2 SoilMoistureRange;
        public Vector2 SoilPHRange;
        public Vector2 UVRange;
        public Vector2 VPDRange;
    }
    
    // Adaptation System
    [System.Serializable]
    public class AdaptationProgress
    {
        public int InstanceId;
        public Dictionary<string, float> AdaptationFactors = new Dictionary<string, float>();
        public DateTime AdaptationStartTime;
        public float TotalAdaptationTime;
        public List<AdaptationEvent> AdaptationHistory = new List<AdaptationEvent>();
        public float AdaptationRate = 0.01f;
        public float MaxAdaptation = 0.5f;
        public bool CanAdapt = true;
    }
    
    [System.Serializable]
    public class AdaptationEvent
    {
        public DateTime Timestamp;
        public string AdaptationType;
        public float AdaptationStrength;
        public EnvironmentalConditions TriggeringConditions;
        public string Description;
    }
    
    [System.Serializable]
    public class EnvironmentalAdaptation
    {
        public string AdaptationType;
        public float AdaptationStrength;
        public float AdaptationTime;
        public bool HasAdapted;
        public Dictionary<string, float> GeneticAdaptations = new Dictionary<string, float>();
        public Dictionary<string, float> ToleranceImprovements = new Dictionary<string, float>();
        public Dictionary<string, float> EfficiencyImprovements = new Dictionary<string, float>();
        public EnvironmentalConditions AdaptationConditions;
        
        // Additional properties used by CannabisGeneticsEngine
        public string GenotypeId;
        public EnvironmentalConditions EnvironmentalConditions;
        public DateTime StartDate;
        public float AdaptationProgress;
        public bool AdaptationApplied;
    }
    
    [System.Serializable]
    public class AdaptationPressure
    {
        public float TemperaturePressure;
        public float HumidityPressure;
        public float LightPressure;
        public float WaterPressure;
        public float TotalPressure;
        public DateTime CalculationTime;
        public int DataPoints;
    }
    
    // Environmental History
    [System.Serializable]
    public class EnvironmentalHistory
    {
        public int InstanceId;
        public List<EnvironmentalHistoryEntry> HistoryEntries = new List<EnvironmentalHistoryEntry>();
        public int MaxHistorySize = 1000;
        public DateTime FirstEntry;
        public DateTime LastEntry;
    }
    
    [System.Serializable]
    public class EnvironmentalHistoryEntry
    {
        public DateTime Timestamp;
        public EnvironmentalConditions Conditions;
        public EnvironmentalStressData StressData;
        public float GrowthRate;
        public float Health;
        public Dictionary<string, float> AdditionalMetrics = new Dictionary<string, float>();
    }
    
    // Environmental Events
    [System.Serializable]
    public class EnvironmentalEvent
    {
        public string EventId;
        public EnvironmentalEventType EventType;
        public int PlantInstanceId;
        public EnvironmentalSeverity Severity;
        public string Description;
        public DateTime Timestamp;
        public EnvironmentalConditions Conditions;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public bool RequiresAction;
        public string RecommendedAction;
    }
    
    // Stress Management
    [System.Serializable]
    public class StressUpdate
    {
        public bool IsStressEvent;
        public EnvironmentalSeverity Severity;
        public string StressType;
        public string Description;
        public float StressIncrease;
        public float RecoveryRate;
        public List<string> AffectedSystems = new List<string>();
        public Dictionary<string, float> StressModifiers = new Dictionary<string, float>();
    }
    
    // Response Types
    [System.Serializable]
    public class TemperatureResponse
    {
        public bool IsStressResponse;
        public float StressMultiplier = 1f;
        public Color VisualEffect = Color.clear;
        public float MorphologicalEffect;
        public float GrowthRateChange;
        public List<string> TriggeredAdaptations = new List<string>();
    }
    
    [System.Serializable]
    public class HumidityResponse
    {
        public bool IsStressResponse;
        public float StressMultiplier = 1f;
        public float TranspirationRate = 1f;
        public float StomalConductance = 1f;
        public Color LeafColorChange = Color.clear;
        public float WiltingFactor = 0f;
    }
    
    [System.Serializable]
    public class LightResponse
    {
        public float EfficiencyMultiplier = 1f;
        public float PhotosynthesisRate = 1f;
        public Color PhotosynthesisColor = Color.green;
        public float ChlorophyllProduction = 1f;
        public float LeafAngleAdjustment = 0f;
        public bool IsPhotoinhibition = false;
        public float UVProtectionLevel = 0f;
    }
    
    [System.Serializable]
    public class AirflowResponse
    {
        public float TranspirationMultiplier = 1f;
        public float GasExchangeRate = 1f;
        public float WindStress = 0f;
        public float StemStrengthening = 0f;
        public float LeafMovementIntensity = 0f;
        public bool IsWindDamage = false;
    }
    
    // Performance and Optimization
    [System.Serializable]
    public class PlantEnvironmentalUpdate
    {
        public int InstanceId;
        public PlantEnvironmentalState PlantState;
        public float UpdateTime;
        public int Priority;
        public bool IsUrgent;
        public string UpdateReason;
    }
    
    [System.Serializable]
    public class EnvironmentalSystemMetrics
    {
        public DateTime SystemStartTime;
        public int ActivePlants;
        public float ProcessedUpdatesPerSecond;
        public float AverageResponseTime;
        public int StressEventsThisSession;
        public int AdaptationsThisSession;
        public float SystemEfficiency;
        public int QueuedUpdates;
        public int SkippedUpdates;
        public DateTime LastUpdate;
        public Dictionary<string, float> PerformanceMetrics = new Dictionary<string, float>();
    }
    
    [System.Serializable]
    public class EnvironmentalSystemReport
    {
        public EnvironmentalSystemMetrics SystemMetrics;
        public int ActivePlantCount;
        public int EnvironmentalZoneCount;
        public int RecentEventsCount;
        public int AdaptationCount;
        public Dictionary<string, bool> SystemStatus;
        public List<string> PerformanceWarnings = new List<string>();
        public Dictionary<EnvironmentalSeverity, int> StressDistribution = new Dictionary<EnvironmentalSeverity, int>();
        public float AverageAdaptationLevel;
        public DateTime ReportGenerated;
    }
    
    // Configuration ScriptableObjects
    [CreateAssetMenu(fileName = "Environmental Config", menuName = "Project Chimera/SpeedTree/Environmental Config")]
    public class SpeedTreeEnvironmentalConfigSO : ScriptableObject
    {
        [Header("Response Thresholds")]
        public float TemperatureStressThreshold = 5f;
        public float HumidityStressThreshold = 15f;
        public float LightStressThreshold = 200f;
        public float CO2StressThreshold = 100f;
        public float AirflowStressThreshold = 0.3f;
        
        [Header("Response Rates")]
        public float ImmediateResponseRate = 1f;
        public float GradualResponseRate = 0.1f;
        public float AdaptiveResponseRate = 0.01f;
        public float RecoveryRate = 0.05f;
        
        [Header("Stress Accumulation")]
        public float StressAccumulationRate = 0.1f;
        public float StressDecayRate = 0.02f;
        public float MaxStressLevel = 2f;
        public bool EnableStressMemory = true;
        
        [Header("Adaptation Settings")]
        public bool EnableAdaptation = true;
        public float MaxAdaptationLevel = 0.8f;
        public float AdaptationTimeRequired = 3600f; // 1 hour
        public bool EnableEpigeneticAdaptation = true;
        
        [Header("Performance Settings")]
        public float MaxUpdateFrequency = 10f;
        public float MinUpdateInterval = 0.1f;
        public bool EnablePerformanceOptimization = true;
        public int HistoryRetentionDays = 7;
    }
    
    [CreateAssetMenu(fileName = "Response Curves", menuName = "Project Chimera/SpeedTree/Response Curves")]
    public class EnvironmentalResponseCurvesSO : ScriptableObject
    {
        [Header("Temperature Response Curves")]
        public AnimationCurve TemperatureGrowthCurve = AnimationCurve.EaseInOut(15f, 0.5f, 30f, 1f);
        public AnimationCurve TemperatureStressCurve = AnimationCurve.Linear(0f, 0f, 40f, 1f);
        public AnimationCurve TemperatureColorCurve = AnimationCurve.EaseInOut(0f, 0f, 40f, 1f);
        
        [Header("Humidity Response Curves")]
        public AnimationCurve HumidityGrowthCurve = AnimationCurve.EaseInOut(40f, 0.5f, 70f, 1f);
        public AnimationCurve HumidityStressCurve = AnimationCurve.Linear(0f, 0f, 100f, 1f);
        public AnimationCurve TranspirationCurve = AnimationCurve.EaseInOut(0f, 0f, 100f, 2f);
        
        [Header("Light Response Curves")]
        public AnimationCurve PhotosynthesisCurve = AnimationCurve.EaseInOut(200f, 0.2f, 1000f, 1f);
        public AnimationCurve LightSaturationCurve = AnimationCurve.EaseInOut(0f, 0f, 2000f, 1f);
        public AnimationCurve UVStressCurve = AnimationCurve.Linear(0f, 0f, 100f, 1f);
        
        [Header("CO2 Response Curves")]
        public AnimationCurve CO2SaturationCurve = AnimationCurve.EaseInOut(400f, 1f, 1500f, 1.2f);
        
        [Header("Airflow Response Curves")]
        public AnimationCurve AirflowTranspirationCurve = AnimationCurve.EaseInOut(0f, 0.8f, 2f, 1.5f);
        public AnimationCurve WindStressCurve = AnimationCurve.Linear(0f, 0f, 3f, 1f);
        
        [Header("Adaptation Curves")]
        public AnimationCurve AdaptationEffectivenessCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Recovery Curves")]
        public AnimationCurve HealthRecoveryCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }
    
    [CreateAssetMenu(fileName = "Stress Visualization Config", menuName = "Project Chimera/SpeedTree/Stress Visualization")]
    public class StressVisualizationConfigSO : ScriptableObject
    {
        [Header("Color Settings")]
        public Gradient StressColorGradient = new Gradient();
        public Color HealthyColor = Color.green;
        public Color MildStressColor = Color.yellow;
        public Color ModerateStressColor = Color.orange;
        public Color SevereStressColor = Color.red;
        public Color CriticalStressColor = Color.magenta;
        
        [Header("Visual Effects")]
        public bool EnableMorphologicalChanges = true;
        public bool EnableAnimationChanges = true;
        public bool EnableParticleEffects = false;
        
        [Header("Stress Indicators")]
        public float LeafDroopIntensity = 0.3f;
        public float LeafCurlIntensity = 0.2f;
        public float WiltingThreshold = 0.7f;
        public float ColorChangeIntensity = 0.8f;
        
        [Header("UI Visualization")]
        public bool ShowStressNumbers = false;
        public bool ShowStressHistory = false;
        public float UIUpdateFrequency = 1f;
    }
    
    // Processor and Manager Classes
    public class EnvironmentalResponseProcessor
    {
        private SpeedTreeEnvironmentalConfigSO _config;
        private EnvironmentalResponseCurvesSO _curves;
        
        public EnvironmentalResponseProcessor(SpeedTreeEnvironmentalConfigSO config, EnvironmentalResponseCurvesSO curves)
        {
            _config = config;
            _curves = curves;
        }
        
        public EnvironmentalResponse ProcessImmediateResponse(PlantEnvironmentalState plantState, EnvironmentalConditions conditions, EnvironmentalDelta delta)
        {
            var response = new EnvironmentalResponse();
            
            // Process temperature response
            if (Mathf.Abs(delta.TemperatureDelta) > _config.TemperatureStressThreshold)
            {
                response.GrowthRateModifier *= _curves.TemperatureGrowthCurve.Evaluate(conditions.Temperature);
                response.ColorModification = Color.Lerp(Color.white, Color.red, 
                    _curves.TemperatureColorCurve.Evaluate(conditions.Temperature));
            }
            
            // Process humidity response
            if (Mathf.Abs(delta.HumidityDelta) > _config.HumidityStressThreshold)
            {
                response.GrowthRateModifier *= _curves.HumidityGrowthCurve.Evaluate(conditions.Humidity);
            }
            
            // Process light response
            if (Mathf.Abs(delta.LightIntensityDelta) > _config.LightStressThreshold)
            {
                response.GrowthRateModifier *= _curves.PhotosynthesisCurve.Evaluate(conditions.LightIntensity);
            }
            
            response.ResponseType = ResponseType.Immediate;
            response.ResponseIntensity = delta.TotalMagnitude / 100f; // Normalize
            
            return response;
        }
        
        public TemperatureResponse ProcessTemperatureResponse(PlantEnvironmentalState plantState, float temperature)
        {
            var response = new TemperatureResponse();
            var optimalTemp = 24f; // Cannabis optimal temperature
            var deviation = Mathf.Abs(temperature - optimalTemp);
            
            if (deviation > 5f)
            {
                response.IsStressResponse = true;
                response.StressMultiplier = 1f + (deviation / 20f);
                response.VisualEffect = deviation > 10f ? Color.red : Color.yellow;
                response.GrowthRateChange = _curves.TemperatureGrowthCurve.Evaluate(temperature);
            }
            
            return response;
        }
        
        public HumidityResponse ProcessHumidityResponse(PlantEnvironmentalState plantState, float humidity)
        {
            var response = new HumidityResponse();
            var optimalHumidity = 60f; // Cannabis optimal humidity
            var deviation = Mathf.Abs(humidity - optimalHumidity);
            
            if (deviation > 15f)
            {
                response.IsStressResponse = true;
                response.StressMultiplier = 1f + (deviation / 50f);
                response.TranspirationRate = _curves.TranspirationCurve.Evaluate(humidity);
            }
            
            return response;
        }
        
        public LightResponse ProcessLightResponse(PlantEnvironmentalState plantState, float lightIntensity, Color lightColor)
        {
            var response = new LightResponse();
            
            response.EfficiencyMultiplier = _curves.PhotosynthesisCurve.Evaluate(lightIntensity);
            response.PhotosynthesisRate = _curves.LightSaturationCurve.Evaluate(lightIntensity);
            response.PhotosynthesisColor = Color.Lerp(Color.green, lightColor, 0.3f);
            
            // Check for photoinhibition
            if (lightIntensity > 1500f)
            {
                response.IsPhotoinhibition = true;
                response.UVProtectionLevel = (lightIntensity - 1500f) / 500f;
            }
            
            return response;
        }
        
        public AirflowResponse ProcessAirflowResponse(PlantEnvironmentalState plantState, float airVelocity)
        {
            var response = new AirflowResponse();
            
            response.TranspirationMultiplier = _curves.AirflowTranspirationCurve.Evaluate(airVelocity);
            response.GasExchangeRate = 1f + (airVelocity * 0.2f);
            
            // Wind stress
            if (airVelocity > 1f)
            {
                response.WindStress = _curves.WindStressCurve.Evaluate(airVelocity);
                response.IsWindDamage = airVelocity > 2.5f;
            }
            
            return response;
        }
        
        public void Update() { }
        public void Cleanup() { }
    }
    
    public class StressAccumulationManager
    {
        private bool _enabled;
        
        public StressAccumulationManager(bool enabled)
        {
            _enabled = enabled;
        }
        
        public void UpdateStressAccumulation(PlantEnvironmentalState plantState, EnvironmentalConditions conditions)
        {
            if (!_enabled || plantState == null) return;
            
            // Calculate stress based on environmental conditions
            var stressData = plantState.StressData;
            var currentTime = DateTime.Now;
            var deltaTime = (float)(currentTime - stressData.LastStressEvent).TotalHours;
            
            // Calculate individual stress factors
            float tempStress = CalculateTemperatureStress(conditions.Temperature, 24f); // Optimal temp
            float humidityStress = CalculateHumidityStress(conditions.Humidity, 60f); // Optimal humidity
            float lightStress = CalculateLightStress(conditions.LightIntensity, 800f); // Optimal light
            
            // Update stress data
            stressData.TemperatureStress = tempStress;
            stressData.HumidityStress = humidityStress;
            stressData.LightStress = lightStress;
            
            // Calculate overall stress
            float overallStress = (tempStress + humidityStress + lightStress) / 3f;
            
            // Apply stress accumulation
            if (overallStress > 0.1f)
            {
                stressData.AccumulatedStress += overallStress * deltaTime * 0.1f;
                stressData.AccumulatedStress = Mathf.Clamp01(stressData.AccumulatedStress);
                
                if (stressData.AccumulatedStress > stressData.PeakStress)
                {
                    stressData.PeakStress = stressData.AccumulatedStress;
                }
                
                stressData.LastStressEvent = currentTime;
            }
            else
            {
                // Stress recovery when conditions are good
                stressData.AccumulatedStress -= stressData.StressRecoveryRate * deltaTime;
                stressData.AccumulatedStress = Mathf.Max(0f, stressData.AccumulatedStress);
            }
        }
        
        public StressUpdate CalculateStressUpdate(PlantEnvironmentalState plantState, EnvironmentalConditions conditions, EnvironmentalZone zone)
        {
            var update = new StressUpdate();
            
            if (!_enabled) return update;
            
            // Calculate stress based on deviation from optimal conditions
            var tempStress = CalculateTemperatureStress(conditions.Temperature, zone.OptimalConditions.Temperature);
            var humidityStress = CalculateHumidityStress(conditions.Humidity, zone.OptimalConditions.Humidity);
            var lightStress = CalculateLightStress(conditions.LightIntensity, zone.OptimalConditions.LightIntensity);
            
            var totalStress = (tempStress + humidityStress + lightStress) / 3f;
            
            if (totalStress > 0.3f)
            {
                update.IsStressEvent = true;
                update.Severity = GetStressSeverity(totalStress);
                update.StressType = GetDominantStressType(tempStress, humidityStress, lightStress);
                update.Description = $"Environmental stress detected: {update.StressType}";
                update.StressIncrease = totalStress * 0.1f;
            }
            
            return update;
        }
        
        private float CalculateTemperatureStress(float current, float optimal)
        {
            var deviation = Mathf.Abs(current - optimal);
            return Mathf.Clamp01(deviation / 10f); // 10 degree tolerance
        }
        
        private float CalculateHumidityStress(float current, float optimal)
        {
            var deviation = Mathf.Abs(current - optimal);
            return Mathf.Clamp01(deviation / 30f); // 30% tolerance
        }
        
        private float CalculateLightStress(float current, float optimal)
        {
            var deviation = Mathf.Abs(current - optimal);
            return Mathf.Clamp01(deviation / 500f); // 500 PPFD tolerance
        }
        
        private EnvironmentalSeverity GetStressSeverity(float stress)
        {
            if (stress < 0.2f) return EnvironmentalSeverity.Mild;
            if (stress < 0.5f) return EnvironmentalSeverity.Moderate;
            if (stress < 0.8f) return EnvironmentalSeverity.Severe;
            return EnvironmentalSeverity.Critical;
        }
        
        private string GetDominantStressType(float tempStress, float humidityStress, float lightStress)
        {
            if (tempStress >= humidityStress && tempStress >= lightStress) return "temperature";
            if (humidityStress >= lightStress) return "humidity";
            return "light";
        }
        
        public void ApplyStressAccumulation(EnvironmentalStressData stressData, StressUpdate update)
        {
            if (update.IsStressEvent)
            {
                stressData.AccumulatedStress += update.StressIncrease;
                stressData.AccumulatedStress = Mathf.Clamp01(stressData.AccumulatedStress);
                
                if (stressData.AccumulatedStress > stressData.PeakStress)
                {
                    stressData.PeakStress = stressData.AccumulatedStress;
                }
                
                stressData.LastStressEvent = DateTime.Now;
            }
            else
            {
                // Stress recovery
                stressData.AccumulatedStress -= stressData.StressRecoveryRate * Time.deltaTime;
                stressData.AccumulatedStress = Mathf.Max(0f, stressData.AccumulatedStress);
            }
        }
        
        public void Update() { }
        public void Cleanup() { }
    }
    
    public class AdaptiveResponseManager
    {
        private bool _enabled;
        private float _adaptationRate;
        
        public AdaptiveResponseManager(bool enabled, float adaptationRate)
        {
            _enabled = enabled;
            _adaptationRate = adaptationRate;
        }
        
        public EnvironmentalAdaptation ProcessAdaptation(PlantEnvironmentalState plantState, AdaptationPressure pressure, AdaptationProgress adaptationData)
        {
            var adaptation = new EnvironmentalAdaptation();
            
            if (!_enabled) return adaptation;
            
            // Determine adaptation type based on pressure
            var adaptationType = DetermineAdaptationType(pressure);
            var adaptationStrength = CalculateAdaptationStrength(pressure, adaptationData);
            
            if (adaptationStrength > 0.1f)
            {
                adaptation.HasAdapted = true;
                adaptation.AdaptationType = adaptationType;
                adaptation.AdaptationStrength = adaptationStrength;
                adaptation.AdaptationTime = CalculateAdaptationTime(adaptationStrength);
                
                // Apply specific adaptations
                ApplySpecificAdaptations(adaptation, adaptationType, adaptationStrength);
            }
            
            return adaptation;
        }
        
        private string DetermineAdaptationType(AdaptationPressure pressure)
        {
            if (pressure.TemperaturePressure > pressure.HumidityPressure && 
                pressure.TemperaturePressure > pressure.LightPressure)
                return "temperature";
            
            if (pressure.HumidityPressure > pressure.LightPressure)
                return "humidity";
            
            return "light";
        }
        
        private float CalculateAdaptationStrength(AdaptationPressure pressure, AdaptationProgress adaptationData)
        {
            var basePressure = pressure.TotalPressure;
            var timeMultiplier = Mathf.Clamp01(adaptationData.TotalAdaptationTime / 3600f); // 1 hour for full strength
            
            return basePressure * timeMultiplier * _adaptationRate;
        }
        
        private float CalculateAdaptationTime(float strength)
        {
            return strength * 3600f; // Stronger adaptations take longer
        }
        
        private void ApplySpecificAdaptations(EnvironmentalAdaptation adaptation, string adaptationType, float strength)
        {
            switch (adaptationType)
            {
                case "temperature":
                    adaptation.GeneticAdaptations["heat_tolerance"] = strength * 0.2f;
                    adaptation.ToleranceImprovements["temperature_range"] = strength * 0.1f;
                    break;
                case "humidity":
                    adaptation.GeneticAdaptations["drought_tolerance"] = strength * 0.15f;
                    adaptation.EfficiencyImprovements["water_efficiency"] = strength * 0.1f;
                    break;
                case "light":
                    adaptation.GeneticAdaptations["light_efficiency"] = strength * 0.25f;
                    adaptation.EfficiencyImprovements["photosynthesis"] = strength * 0.15f;
                    break;
            }
        }
        
        public void Update() { }
        public void Cleanup() { }
    }
    
    // Additional supporting classes
    public class EnvironmentalDataCollector
    {
        private bool _enabled;
        private List<EnvironmentalHistoryEntry> _dataPoints = new List<EnvironmentalHistoryEntry>();
        
        public EnvironmentalDataCollector(bool enabled)
        {
            _enabled = enabled;
        }
        
        public void RecordDataPoint(int instanceId, EnvironmentalHistoryEntry entry)
        {
            if (_enabled)
            {
                _dataPoints.Add(entry);
                
                // Maintain data size
                if (_dataPoints.Count > 10000)
                {
                    _dataPoints.RemoveAt(0);
                }
            }
        }
        
        public void Update() { }
        public void Cleanup() { _dataPoints.Clear(); }
    }
    
    public class StressVisualizationManager
    {
        private StressVisualizationConfigSO _config;
        private bool _enabled;
        
        public StressVisualizationManager(StressVisualizationConfigSO config, bool enabled)
        {
            _config = config;
            _enabled = enabled;
        }
        
        public void UpdateStressVisualization(AdvancedSpeedTreeManager.SpeedTreePlantData instance, EnvironmentalStressData stressData)
        {
            if (!_enabled || instance == null) return;
            
            UpdatePlantVisualization(instance, stressData);
        }
        
        public void UpdatePlantVisualization(AdvancedSpeedTreeManager.SpeedTreePlantData instance, EnvironmentalStressData stressData)
        {
#if UNITY_SPEEDTREE
            if (instance.Renderer?.materialProperties == null) return;
            
            var stressLevel = stressData.OverallStress;
            var stressColor = _config.StressColorGradient.Evaluate(stressLevel);
            
            instance.Renderer.materialProperties.SetColor("_StressColor", stressColor);
            instance.Renderer.materialProperties.SetFloat("_StressLevel", stressLevel);
            
            // Apply morphological changes
            if (_config.EnableMorphologicalChanges)
            {
                if (stressData.TemperatureStress > 0.5f)
                {
                    instance.Renderer.materialProperties.SetFloat("_LeafCurl", 
                        stressData.TemperatureStress * _config.LeafCurlIntensity);
                }
                
                if (stressData.WaterStress > 0.5f)
                {
                    instance.Renderer.materialProperties.SetFloat("_LeafDroop", 
                        stressData.WaterStress * _config.LeafDroopIntensity);
                }
            }
#endif
        }
        
        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }
        
        public void Update() { }
        public void Cleanup() { }
    }
    
    public class EnvironmentalGradientManager
    {
        public void Update() { }
        public void Cleanup() { }
    }
    
    public class MicroclimateSimulator
    {
        private List<EnvironmentalZone> _zones = new List<EnvironmentalZone>();
        
        public void Initialize(List<EnvironmentalZone> zones)
        {
            _zones = zones;
        }
        
        public void UpdateMicroclimates(List<PlantEnvironmentalState> plantStates)
        {
            // Update microclimates around plants
            foreach (var plantState in plantStates)
            {
                UpdatePlantMicroclimate(plantState);
            }
        }
        
        private void UpdatePlantMicroclimate(PlantEnvironmentalState plantState)
        {
            // Simulate microclimate effects around individual plants
            var baseConditions = plantState.LastConditions;
            
            // Plants create their own microclimate
            baseConditions.Humidity += 2f; // Plants increase local humidity
            baseConditions.Temperature -= 0.5f; // Transpiration cooling
            baseConditions.CO2Level -= 20f; // CO2 consumption
        }
        
        public void UpdateZoneMicroclimate(EnvironmentalZone zone, EnvironmentalConditions conditions)
        {
            // Update zone-level microclimate
            zone.LastConditions = conditions;
        }
        
        public EnvironmentalConditions GetConditionsAtPosition(Vector3 position, EnvironmentalConditions baseConditions)
        {
            // Calculate microclimate at specific position
            var modifiedConditions = new EnvironmentalConditions
            {
                Temperature = baseConditions.Temperature + UnityEngine.Random.Range(-0.5f, 0.5f),
                Humidity = baseConditions.Humidity + UnityEngine.Random.Range(-1f, 1f),
                LightIntensity = baseConditions.LightIntensity + UnityEngine.Random.Range(-20f, 20f),
                CO2Level = baseConditions.CO2Level + UnityEngine.Random.Range(-10f, 10f),
                AirVelocity = baseConditions.AirVelocity + UnityEngine.Random.Range(-0.05f, 0.05f),
                Timestamp = DateTime.Now
            };
            
            return modifiedConditions;
        }
        
        public void Update() { }
        public void Cleanup() { }
    }
    
    public class PerformanceOptimizer
    {
        private int _maxSimultaneousPlants;
        
        public PerformanceOptimizer(int maxPlants)
        {
            _maxSimultaneousPlants = maxPlants;
        }
        
        public List<PlantEnvironmentalState> GetPlantsForProcessing(List<PlantEnvironmentalState> allPlants)
        {
            // Prioritize plants for processing based on stress level and distance to camera
            return allPlants
                .Where(p => p.IsMonitored)
                .OrderByDescending(p => p.StressData.OverallStress)
                .ThenBy(p => Vector3.Distance(p.PlantInstance.Position, Camera.main?.transform.position ?? Vector3.zero))
                .Take(_maxSimultaneousPlants)
                .ToList();
        }
        
        public int GetMaxUpdatesPerFrame()
        {
            // Adjust based on current performance
            var frameRate = 1f / Time.unscaledDeltaTime;
            
            if (frameRate > 50f) return 20;
            if (frameRate > 30f) return 15;
            if (frameRate > 20f) return 10;
            return 5;
        }
        
        public void Update() { }
        public void Cleanup() { }
    }
}