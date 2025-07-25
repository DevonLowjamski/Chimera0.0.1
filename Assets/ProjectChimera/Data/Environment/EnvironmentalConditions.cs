using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Data.Events;  // Added to access Season enum

namespace ProjectChimera.Data.Environment
{
    /// <summary>
    /// Comprehensive environmental conditions data structure for sophisticated cannabis cultivation simulation.
    /// Includes all environmental parameters that affect plant growth, health, and cannabinoid/terpene production.
    /// </summary>
    [System.Serializable]
    public class EnvironmentalConditions
    {
        [Header("Atmospheric Conditions")]
        public float Temperature = 22f;                    // °C
        public float Humidity = 55f;                       // %RH
        public float BarometricPressure = 1013.25f;       // hPa
        public float VaporPressureDeficit = 1.0f;          // kPa (calculated)
        public float CO2Level = 400f;                      // ppm
        public float OxygenLevel = 21f;                    // %
        
        [Header("Air Movement & Quality")]
        public float AirVelocity = 0.3f;                   // m/s
        public float AirFlow = 0.3f;                       // m/s (alias for AirVelocity)
        public float AirflowRate = 0.3f;                   // m/s (alias for AirFlow)
        public float AirCirculation = 0.3f;                // m/s (alias for AirFlow)
        public Vector3 AirFlowDirection = Vector3.forward;  // Directional airflow
        public float AirExchangeRate = 1.0f;               // air changes per hour
        public float AirQualityIndex = 1.0f;               // 0-1 (1 = pristine)
        public List<AirContaminant> Contaminants = new List<AirContaminant>();
        
        [Header("Light Spectrum & Intensity")]
        public float LightIntensity = 400f;                // PPFD µmol/m²/s
        public float DailyLightIntegral = 25f;             // DLI mol/m²/day
        public LightSpectrumData LightSpectrumData = new LightSpectrumData();
        public float UVIndex = 0f;                         // UV exposure
        public float InfraredLevel = 0f;                   // IR heat contribution
        public float PhotosyntheticActive = 400f;          // PAR µmol/m²/s
        
        [Header("Environmental Stability")]
        public float TemperatureStability = 1f;            // 0-1 (variance measure)
        public float HumidityStability = 1f;               // 0-1 (variance measure)
        public float LightStability = 1f;                  // 0-1 (flicker/consistency)
        public DateTime LastMeasurement = DateTime.Now;
        public DateTime Timestamp = DateTime.Now;          // Alias for LastMeasurement for compatibility
        public float MeasurementAccuracy = 0.95f;          // Sensor accuracy 0-1
        
        [Header("Microclimate Variations")]
        public float CanopyTemperature = 22f;              // Leaf surface temperature
        public float RootZoneTemperature = 20f;            // Growing medium temperature
        public float CanopyHumidity = 55f;                 // Humidity at plant level
        public float VerticalTempGradient = 0f;            // °C difference floor to ceiling
        public float HorizontalTempVariation = 0f;         // °C variation across room
        
        [Header("Advanced Parameters")]
        public float AtmosphericCharge = 0f;               // Electrical field effects
        public float MagneticFieldVariation = 0f;          // Geomagnetic influences
        public float SonicEnvironment = 40f;               // dB ambient sound
        public float VibrationLevel = 0f;                  // Structural vibrations
        public SeasonalContext SeasonalContext = new SeasonalContext();
        
        /// <summary>
        /// Calculates the current Vapor Pressure Deficit based on temperature and humidity.
        /// </summary>
        public void UpdateVPD()
        {
            // Saturation vapor pressure using Magnus formula (accurate for cannabis range)
            float svp = 0.6108f * Mathf.Exp((17.27f * Temperature) / (Temperature + 237.3f));
            float avp = svp * (Humidity / 100f);
            VaporPressureDeficit = svp - avp;
        }
        
        /// <summary>
        /// Calculates environmental stress indicators.
        /// </summary>
        public EnvironmentalStressIndicators CalculateStressIndicators(EnvironmentalParametersSO optimalParams)
        {
            var stress = new EnvironmentalStressIndicators();
            
            if (optimalParams != null)
            {
                // Temperature stress
                stress.TemperatureStress = CalculateParameterStress(Temperature, optimalParams.TemperatureRange);
                stress.HeatStress = Temperature > optimalParams.StressThresholds.HeatStressThreshold ? 
                    (Temperature - optimalParams.StressThresholds.HeatStressThreshold) / 10f : 0f;
                stress.ColdStress = Temperature < optimalParams.StressThresholds.ColdStressThreshold ? 
                    (optimalParams.StressThresholds.ColdStressThreshold - Temperature) / 10f : 0f;
                
                // Humidity stress
                stress.HumidityStress = CalculateParameterStress(Humidity, optimalParams.HumidityRange);
                stress.VPDStress = CalculateParameterStress(VaporPressureDeficit, optimalParams.VPDRange);
                
                // Light stress
                stress.LightStress = CalculateParameterStress(LightIntensity, optimalParams.LightIntensityRange);
                stress.LightBurn = LightIntensity > optimalParams.StressThresholds.LightBurnThreshold ?
                    (LightIntensity - optimalParams.StressThresholds.LightBurnThreshold) / 500f : 0f;
                
                // CO2 stress
                stress.CO2Stress = CalculateParameterStress(CO2Level, optimalParams.CO2Range);
                
                // Air quality stress
                stress.AirQualityStress = 1f - AirQualityIndex;
                stress.AirMovementStress = CalculateParameterStress(AirVelocity, optimalParams.AirVelocityRange);
            }
            else
            {
                // Default stress calculation when no optimal params provided
                stress.TemperatureStress = Mathf.Abs(Temperature - 22f) / 10f; // Default optimal temp 22°C
                stress.HumidityStress = Mathf.Abs(Humidity - 55f) / 20f; // Default optimal humidity 55%
                stress.LightStress = Mathf.Abs(LightIntensity - 400f) / 200f; // Default optimal light 400 PPFD
                stress.CO2Stress = Mathf.Abs(CO2Level - 800f) / 400f; // Default optimal CO2 800ppm
                stress.AirQualityStress = 1f - AirQualityIndex;
                stress.AirMovementStress = Mathf.Abs(AirVelocity - 0.3f) / 0.5f; // Default optimal airflow 0.3 m/s
            }
            
            // Environmental instability stress
            stress.InstabilityStress = (2f - TemperatureStability - HumidityStability) / 2f;
            
            // Calculate overall stress index
            stress.OverallStressIndex = (stress.TemperatureStress + stress.HumidityStress + 
                stress.LightStress + stress.CO2Stress + stress.AirQualityStress + 
                stress.InstabilityStress) / 6f;
            
            return stress;
        }
        
        /// <summary>
        /// Gets environmental quality score for plant growth.
        /// </summary>
        public float GetEnvironmentalQuality(EnvironmentalParametersSO optimalParams)
        {
            float tempQuality, humidityQuality, lightQuality, co2Quality, vpdQuality;
            
            if (optimalParams != null)
            {
                tempQuality = GetParameterQuality(Temperature, optimalParams.TemperatureRange);
                humidityQuality = GetParameterQuality(Humidity, optimalParams.HumidityRange);
                lightQuality = GetParameterQuality(LightIntensity, optimalParams.LightIntensityRange);
                co2Quality = GetParameterQuality(CO2Level, optimalParams.CO2Range);
                vpdQuality = GetParameterQuality(VaporPressureDeficit, optimalParams.VPDRange);
            }
            else
            {
                // Default quality calculation when no optimal params provided
                tempQuality = 1f - Mathf.Abs(Temperature - 22f) / 10f; // Default optimal temp 22°C
                humidityQuality = 1f - Mathf.Abs(Humidity - 55f) / 20f; // Default optimal humidity 55%
                lightQuality = 1f - Mathf.Abs(LightIntensity - 400f) / 200f; // Default optimal light 400 PPFD
                co2Quality = 1f - Mathf.Abs(CO2Level - 800f) / 400f; // Default optimal CO2 800ppm
                vpdQuality = 1f - Mathf.Abs(VaporPressureDeficit - 1.0f) / 1.0f; // Default optimal VPD 1.0 kPa
            }
            
            float airQuality = AirQualityIndex;
            float stabilityQuality = (TemperatureStability + HumidityStability + LightStability) / 3f;
            
            // Weighted average with emphasis on critical parameters
            return Mathf.Clamp01(tempQuality * 0.20f + humidityQuality * 0.15f + lightQuality * 0.20f + 
                   co2Quality * 0.15f + vpdQuality * 0.15f + airQuality * 0.10f + 
                   stabilityQuality * 0.05f);
        }
        
        /// <summary>
        /// Predicts cannabinoid/terpene production potential based on current conditions.
        /// </summary>
        public CannabinoidProductionPotential PredictCannabinoidProduction()
        {
            var potential = new CannabinoidProductionPotential();
            
            // THC production favors: moderate temps (20-25°C), lower humidity (45-55%), high light
            potential.THCPotential = CalculateTHCPotential();
            
            // CBD production favors: slightly cooler temps, consistent conditions
            potential.CBDPotential = CalculateCBDPotential();
            
            // Terpene production favors: temperature swings, specific light spectrums, stress
            potential.TerpenePotential = CalculateTerpenePotential();
            
            // Trichome production responds to UV, temperature differential, humidity
            potential.TrichomePotential = CalculateTrichomePotential();
            
            return potential;
        }
        
        /// <summary>
        /// Gets the value of a specific environmental factor
        /// </summary>
        public float GetValue(EnvironmentalFactor factor)
        {
            return factor switch
            {
                EnvironmentalFactor.Temperature => Temperature,
                EnvironmentalFactor.Humidity => Humidity,
                EnvironmentalFactor.LightIntensity => LightIntensity,
                EnvironmentalFactor.CO2Level => CO2Level,
                EnvironmentalFactor.AirFlow => AirFlow,
                EnvironmentalFactor.VaporPressureDeficit => VaporPressureDeficit,
                EnvironmentalFactor.DailyLightIntegral => DailyLightIntegral,
                EnvironmentalFactor.BarometricPressure => BarometricPressure,
                EnvironmentalFactor.AirQuality => AirQualityIndex,
                EnvironmentalFactor.RootZoneTemperature => RootZoneTemperature,
                EnvironmentalFactor.CanopyTemperature => CanopyTemperature,
                EnvironmentalFactor.UVIndex => UVIndex,
                _ => 0f
            };
        }
        
        private float CalculateParameterStress(float value, Vector2 optimalRange)
        {
            if (value >= optimalRange.x && value <= optimalRange.y)
                return 0f;
            
            float rangeSize = optimalRange.y - optimalRange.x;
            float distance = value < optimalRange.x ? optimalRange.x - value : value - optimalRange.y;
            return Mathf.Clamp01(distance / rangeSize);
        }
        
        private float GetParameterQuality(float value, Vector2 optimalRange)
        {
            return 1f - CalculateParameterStress(value, optimalRange);
        }
        
        private float CalculateTHCPotential()
        {
            float tempFactor = Temperature >= 20f && Temperature <= 25f ? 1f : 
                Mathf.Clamp01(1f - Mathf.Abs(22.5f - Temperature) / 10f);
            float humidityFactor = Humidity >= 45f && Humidity <= 55f ? 1f :
                Mathf.Clamp01(1f - Mathf.Abs(50f - Humidity) / 20f);
            float lightFactor = Mathf.Clamp01(LightIntensity / 800f);
            float uvFactor = Mathf.Clamp01(UVIndex / 10f);
            
            return (tempFactor + humidityFactor + lightFactor + uvFactor) / 4f;
        }
        
        private float CalculateCBDPotential()
        {
            float tempFactor = Temperature >= 18f && Temperature <= 23f ? 1f :
                Mathf.Clamp01(1f - Mathf.Abs(20.5f - Temperature) / 8f);
            float stabilityFactor = (TemperatureStability + HumidityStability) / 2f;
            float lightFactor = Mathf.Clamp01(LightIntensity / 600f);
            
            return (tempFactor + stabilityFactor + lightFactor) / 3f;
        }
        
        private float CalculateTerpenePotential()
        {
            float tempSwingFactor = 1f - TemperatureStability; // Stress enhances terpenes
            float spectrumFactor = LightSpectrumData != null ? LightSpectrumData.GetTerpeneEnhancingRatio() : 0f;
            float stressFactor = 0.5f; // Simplified stress calculation when optimal params are null
            
            return (tempSwingFactor + spectrumFactor + stressFactor) / 3f;
        }
        
        private float CalculateTrichomePotential()
        {
            float uvFactor = Mathf.Clamp01(UVIndex / 8f);
            float tempDiffFactor = Mathf.Abs(Temperature - RootZoneTemperature) / 5f;
            float humidityFactor = Humidity >= 40f && Humidity <= 60f ? 1f :
                Mathf.Clamp01(1f - Mathf.Abs(50f - Humidity) / 25f);
            
            return (uvFactor + tempDiffFactor + humidityFactor) / 3f;
        }
        
        /// <summary>
        /// Checks if environmental conditions have been properly initialized.
        /// </summary>
        public bool IsInitialized()
        {
            return Temperature > 0f && Humidity > 0f && LightIntensity > 0f;
        }

        /// <summary>
        /// Creates environmental conditions with typical indoor cultivation values.
        /// </summary>
        public static EnvironmentalConditions CreateIndoorDefault()
        {
            var conditions = new EnvironmentalConditions
            {
                Temperature = 24f,
                Humidity = 55f,
                CO2Level = 800f,
                LightIntensity = 400f,
                AirVelocity = 0.3f,
                AirFlow = 0.3f,
                AirflowRate = 0.3f,
                AirCirculation = 0.3f,
                DailyLightIntegral = 25f,
                BarometricPressure = 1013.25f,
                VaporPressureDeficit = 1.0f,
                OxygenLevel = 21f,
                AirQualityIndex = 1.0f,
                TemperatureStability = 1f,
                HumidityStability = 1f,
                LightStability = 1f,
                CanopyTemperature = 22f,
                RootZoneTemperature = 20f,
                CanopyHumidity = 55f,
                SonicEnvironment = 40f,
                SeasonalContext = new SeasonalContext(),
                LightSpectrumData = new LightSpectrumData()
            };
            
            conditions.UpdateVPD();
            return conditions;
        }
    }
    
    /// <summary>
    /// Air contaminant data.
    /// </summary>
    [System.Serializable]
    public class AirContaminant
    {
        public ContaminantType Type;
        public float Concentration;  // ppm or relevant unit
        public float ToxicityLevel;  // 0-1
        public string Source;
    }
    
    /// <summary>
    /// Environmental stress indicators.
    /// </summary>
    [System.Serializable]
    public class EnvironmentalStressIndicators
    {
        public float TemperatureStress;
        public float HeatStress;
        public float ColdStress;
        public float HumidityStress;
        public float VPDStress;
        public float LightStress;
        public float LightBurn;
        public float CO2Stress;
        public float AirQualityStress;
        public float AirMovementStress;
        public float InstabilityStress;
        public float OverallStressIndex;
    }
    
    /// <summary>
    /// Cannabinoid production potential prediction.
    /// </summary>
    [System.Serializable]
    public class CannabinoidProductionPotential
    {
        public float THCPotential;
        public float CBDPotential;
        public float TerpenePotential;
        public float TrichomePotential;
        public float OverallQuality;
    }
    
    /// <summary>
    /// Seasonal environmental context.
    /// </summary>
    [System.Serializable]
    public class SeasonalContext
    {
        public Season CurrentSeason = Season.Spring;
        public float SeasonProgress = 0f;  // 0-1 through the season
        public int DayOfYear = 100;
        public float DaylightHours = 12f;
        public float SolarAngle = 45f;     // Sun elevation
        public float OutdoorTemperature = 20f;
        public float OutdoorHumidity = 60f;
    }
    
    /// <summary>
    /// Types of air contaminants.
    /// </summary>
    public enum ContaminantType
    {
        Dust,
        Pollen,
        Mold,
        Bacteria,
        Pesticide,
        VOC,           // Volatile Organic Compounds
        Ammonia,
        Ethylene,
        Ozone,
        Other
    }
    
    /// <summary>
    /// Season types for environmental simulation and cultivation timing.
    /// Compatible with existing Systems layer code while maintaining Data layer consistency.
    /// </summary>
    public enum SeasonType
    {
        Spring,
        Summer,
        Fall,
        Winter
    }
    
    /// <summary>
    /// Environmental factors that can be measured and controlled in cultivation environments.
    /// Comprehensive list supporting all environmental monitoring and control systems.
    /// </summary>
    public enum EnvironmentalFactor
    {
        Temperature,
        Humidity,
        LightIntensity,
        Light = LightIntensity,  // Alias for compatibility
        CO2Level,
        CO2 = CO2Level,          // Alias for compatibility
        AirFlow,
        VaporPressureDeficit,
        DailyLightIntegral,
        BarometricPressure,
        AirQuality,
        RootZoneTemperature,
        CanopyTemperature,
        UVIndex,
        pH,
        ElectricalConductivity,
        WaterLevel,
        Pressure,
        AirPressure,
        LightDuration,
        MoistureLevel,
        OxygenLevel
    }
}