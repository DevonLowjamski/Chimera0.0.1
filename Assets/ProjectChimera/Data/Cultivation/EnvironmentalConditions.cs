using UnityEngine;
using ProjectChimera.Core;
// Explicit alias to resolve LightSpectrum ambiguity - use enum from same namespace
using LightSpectrum = ProjectChimera.Data.Cultivation.LightSpectrum;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Represents environmental conditions that affect plant growth and development.
    /// Used throughout the cultivation and genetics systems for environmental calculations.
    /// </summary>
    [System.Serializable]
    public struct EnvironmentalConditions : IEnvironmentalConditions
    {
        [Header("Atmospheric Conditions")]
        [SerializeField, Range(-10f, 50f)] private float _temperature; // Celsius
        [SerializeField, Range(0f, 100f)] private float _humidity; // %RH
        [SerializeField, Range(200f, 2000f)] private float _co2Level; // ppm
        [SerializeField, Range(0f, 2f)] private float _airFlow; // Air movement factor

        [Header("Lighting")]
        [SerializeField, Range(0f, 1200f)] private float _lightIntensity; // PPFD (Photosynthetic Photon Flux Density)
        [SerializeField] private LightSpectrum _lightSpectrum; // Light spectrum type
        
        // Interface implementation properties
        public float Temperature { get => _temperature; set => _temperature = value; }
        public float Humidity { get => _humidity; set => _humidity = value; }
        public float CO2Level { get => _co2Level; set => _co2Level = value; }
        public float AirFlow { get => _airFlow; set => _airFlow = value; }
        public float LightIntensity { get => _lightIntensity; set => _lightIntensity = value; }
        public LightSpectrum LightSpectrum { get => _lightSpectrum; set => _lightSpectrum = value; }
        [SerializeField, Range(0f, 24f)] public float PhotoperiodHours; // Hours of light per day
        [SerializeField, Range(2700f, 6500f)] public float ColorTemperature; // Kelvin

        [Header("Water and Nutrients")]
        [SerializeField, Range(0f, 100f)] public float WaterAvailability; // % availability
        [SerializeField, Range(4f, 8f)] public float pH; // pH level
        [SerializeField, Range(0f, 3000f)] public float ElectricalConductivity; // EC (μS/cm)

        /// <summary>
        /// Nutrient level as a normalized 0-1 scale based on ElectricalConductivity.
        /// 0 = No nutrients, 0.5 = Optimal (1200 μS/cm), 1 = Maximum (3000 μS/cm)
        /// </summary>
        public float NutrientLevel 
        { 
            get 
            { 
                // Convert EC to normalized scale: 0-3000 μS/cm -> 0-1
                // Optimal EC for cannabis is around 1200 μS/cm (0.5 on scale)
                return Mathf.Clamp01(ElectricalConductivity / 3000f);
            } 
            set 
            { 
                // Convert normalized scale back to EC
                ElectricalConductivity = Mathf.Clamp(value * 3000f, 0f, 3000f);
            } 
        }

        [Header("Advanced Environmental Parameters")]
        [SerializeField, Range(0f, 1f)] public float OxygenLevel; // Root zone oxygen availability
        [SerializeField, Range(0f, 100f)] public float BarometricPressure; // kPa
        [SerializeField, Range(0f, 1f)] public float StressLevel; // Overall environmental stress (0 = no stress, 1 = maximum stress)
        
        [Header("Professional Cultivation Metrics")]
        [SerializeField, Range(0f, 3f)] public float VPD; // Vapor Pressure Deficit (kPa) - calculated or manually set
        [SerializeField, Range(-10f, 35f)] public float DewPoint; // Dew point temperature (°C)
        [SerializeField, Range(0f, 50f)] public float DailyLightIntegral; // DLI (mol/m²/day)
        [SerializeField, Range(0f, 5f)] public float AirVelocity; // Air movement speed (m/s)
        [SerializeField, Range(0f, 1000f)] public float AbsoluteHumidity; // g/m³
        
        [Header("Canopy Microclimate")]
        [SerializeField, Range(-5f, 5f)] public float CanopyTemperatureDelta; // Difference from ambient (°C)
        [SerializeField, Range(-20f, 20f)] public float CanopyHumidityDelta; // Difference from ambient (%)
        [SerializeField, Range(0f, 2f)] public float LeafSurfaceTemperature; // Direct leaf temperature (°C offset)
        [SerializeField, Range(0f, 100f)] public float CanopyLightPenetration; // Light penetration percentage
        
        [Header("Water Quality Parameters")]
        [SerializeField, Range(0f, 500f)] public float DissolvedOxygen; // ppm O2 in water
        [SerializeField, Range(0f, 50f)] public float WaterTemperature; // °C
        [SerializeField, Range(0f, 3000f)] public float TotalDissolvedSolids; // TDS ppm
        [SerializeField, Range(0f, 500f)] public float ChlorineLevel; // ppm Cl

        /// <summary>
        /// Creates environmental conditions with typical indoor cultivation values.
        /// </summary>
        public static EnvironmentalConditions CreateIndoorDefault()
        {
            var conditions = new EnvironmentalConditions();
            conditions.Temperature = 24f;
            conditions.Humidity = 55f;
            conditions.CO2Level = 800f;
            conditions.AirFlow = 1f;
            conditions.LightIntensity = 400f;
            conditions.LightSpectrum = LightSpectrum.FullSpectrum;
            conditions.PhotoperiodHours = 18f;
            conditions.ColorTemperature = 4000f;
            conditions.WaterAvailability = 80f;
            conditions.pH = 6.5f;
            conditions.ElectricalConductivity = 1200f;
            conditions.OxygenLevel = 1f;
            conditions.BarometricPressure = 101.3f;
            conditions.StressLevel = 0f;
            
            // Professional parameters
            conditions.AirVelocity = 0.5f;
            conditions.CanopyTemperatureDelta = -1f;
            conditions.CanopyHumidityDelta = 5f;
            conditions.LeafSurfaceTemperature = -2f;
            conditions.CanopyLightPenetration = 85f;
            
            // Water quality
            conditions.DissolvedOxygen = 8f;
            conditions.WaterTemperature = 20f;
            conditions.TotalDissolvedSolids = 800f;
            conditions.ChlorineLevel = 0f;
            
            // Calculate derived values
            conditions.CalculateDerivedValues();
            return conditions;
        }

        /// <summary>
        /// Creates environmental conditions with typical outdoor cultivation values.
        /// </summary>
        public static EnvironmentalConditions CreateOutdoorDefault()
        {
            var conditions = new EnvironmentalConditions();
            conditions.Temperature = 22f;
            conditions.Humidity = 65f;
            conditions.CO2Level = 400f;
            conditions.AirFlow = 1.5f;
            conditions.LightIntensity = 600f;
            conditions.LightSpectrum = LightSpectrum.SunlightSpectrum;
            conditions.PhotoperiodHours = 14f;
            conditions.ColorTemperature = 5500f;
            conditions.WaterAvailability = 70f;
            conditions.pH = 6.8f;
            conditions.ElectricalConductivity = 800f;
            conditions.OxygenLevel = 1f;
            conditions.BarometricPressure = 101.3f;
            conditions.StressLevel = 0.1f;
            
            // Professional parameters (outdoor has more variable conditions)
            conditions.AirVelocity = 1.2f;
            conditions.CanopyTemperatureDelta = -2f;
            conditions.CanopyHumidityDelta = 10f;
            conditions.LeafSurfaceTemperature = -3f;
            conditions.CanopyLightPenetration = 90f;
            
            // Natural water quality
            conditions.DissolvedOxygen = 7f;
            conditions.WaterTemperature = 18f;
            conditions.TotalDissolvedSolids = 200f;
            conditions.ChlorineLevel = 0f;
            
            conditions.CalculateDerivedValues();
            return conditions;
        }

        /// <summary>
        /// Creates environmental conditions representing stress conditions.
        /// </summary>
        public static EnvironmentalConditions CreateStressConditions()
        {
            var conditions = new EnvironmentalConditions();
            conditions.Temperature = 35f; // High temperature stress
            conditions.Humidity = 30f; // Low humidity stress
            conditions.CO2Level = 300f; // Low CO2
            conditions.AirFlow = 0.2f; // Poor air circulation
            conditions.LightIntensity = 150f; // Low light stress
            conditions.LightSpectrum = LightSpectrum.WarmWhite;
            conditions.PhotoperiodHours = 12f;
            conditions.ColorTemperature = 3000f;
            conditions.WaterAvailability = 30f; // Drought stress
            conditions.pH = 8.2f; // High pH stress
            conditions.ElectricalConductivity = 2500f; // High nutrient concentration
            conditions.OxygenLevel = 0.3f; // Low oxygen stress
            conditions.BarometricPressure = 95f; // Low pressure
            conditions.StressLevel = 0.8f;
            return conditions;
        }

        /// <summary>
        /// Calculates overall environmental suitability (0-1 scale, 1 = optimal).
        /// </summary>
        public float CalculateOverallSuitability()
        {
            // Define optimal ranges for general cannabis cultivation
            float tempScore = CalculateRangeScore(Temperature, 18f, 28f);
            float humidityScore = CalculateRangeScore(Humidity, 40f, 70f);
            float co2Score = CalculateRangeScore(CO2Level, 600f, 1200f);
            float lightScore = CalculateRangeScore(LightIntensity, 300f, 800f);
            float pHScore = CalculateRangeScore(pH, 6.0f, 7.0f);
            float waterScore = CalculateRangeScore(WaterAvailability, 60f, 90f);

            return (tempScore + humidityScore + co2Score + lightScore + pHScore + waterScore) / 6f;
        }

        /// <summary>
        /// Calculates stress level based on environmental conditions.
        /// </summary>
        public float CalculateEnvironmentalStress()
        {
            float suitability = CalculateOverallSuitability();
            float calculatedStress = 1f - suitability;
            
            // Combine with manually set stress level
            return Mathf.Max(calculatedStress, StressLevel);
        }

        /// <summary>
        /// Checks if the environmental conditions have been initialized (not default/empty).
        /// </summary>
        public bool IsInitialized()
        {
            // Check if any non-zero values are present (indicating it's not just default struct)
            return Temperature != 0f || Humidity != 0f || CO2Level != 0f || LightIntensity != 0f;
        }

        /// <summary>
        /// Checks if conditions are within acceptable ranges for cannabis cultivation.
        /// </summary>
        public bool IsWithinAcceptableRanges()
        {
            return Temperature >= 15f && Temperature <= 35f &&
                   Humidity >= 20f && Humidity <= 85f &&
                   CO2Level >= 250f && CO2Level <= 2000f &&
                   LightIntensity >= 100f && LightIntensity <= 1200f &&
                   pH >= 5.0f && pH <= 8.5f &&
                   WaterAvailability >= 10f;
        }

        /// <summary>
        /// Interpolates between two environmental conditions.
        /// </summary>
        public static EnvironmentalConditions Lerp(EnvironmentalConditions a, EnvironmentalConditions b, float t)
        {
            var result = new EnvironmentalConditions();
            result.Temperature = Mathf.Lerp(a.Temperature, b.Temperature, t);
            result.Humidity = Mathf.Lerp(a.Humidity, b.Humidity, t);
            result.CO2Level = Mathf.Lerp(a.CO2Level, b.CO2Level, t);
            result.AirFlow = Mathf.Lerp(a.AirFlow, b.AirFlow, t);
            result.LightIntensity = Mathf.Lerp(a.LightIntensity, b.LightIntensity, t);
            result.PhotoperiodHours = Mathf.Lerp(a.PhotoperiodHours, b.PhotoperiodHours, t);
            result.ColorTemperature = Mathf.Lerp(a.ColorTemperature, b.ColorTemperature, t);
            result.WaterAvailability = Mathf.Lerp(a.WaterAvailability, b.WaterAvailability, t);
            result.pH = Mathf.Lerp(a.pH, b.pH, t);
            result.ElectricalConductivity = Mathf.Lerp(a.ElectricalConductivity, b.ElectricalConductivity, t);
            result.OxygenLevel = Mathf.Lerp(a.OxygenLevel, b.OxygenLevel, t);
            result.BarometricPressure = Mathf.Lerp(a.BarometricPressure, b.BarometricPressure, t);
            result.StressLevel = Mathf.Lerp(a.StressLevel, b.StressLevel, t);
            return result;
        }

        /// <summary>
        /// Creates a copy of the environmental conditions with modified values.
        /// </summary>
        public EnvironmentalConditions WithModifications(
            float? temperature = null,
            float? humidity = null,
            float? co2Level = null,
            float? lightIntensity = null,
            float? pH = null,
            float? waterAvailability = null,
            float? stressLevel = null)
        {
            var result = this; // Copy the struct
            if (temperature.HasValue) result.Temperature = temperature.Value;
            if (humidity.HasValue) result.Humidity = humidity.Value;
            if (co2Level.HasValue) result.CO2Level = co2Level.Value;
            if (lightIntensity.HasValue) result.LightIntensity = lightIntensity.Value;
            if (pH.HasValue) result.pH = pH.Value;
            if (waterAvailability.HasValue) result.WaterAvailability = waterAvailability.Value;
            if (stressLevel.HasValue) result.StressLevel = stressLevel.Value;
            return result;
        }

        private float CalculateRangeScore(float value, float min, float max)
        {
            if (value < min || value > max)
            {
                // Outside range - calculate how far outside
                float distance = Mathf.Min(Mathf.Abs(value - min), Mathf.Abs(value - max));
                float rangeSize = max - min;
                return Mathf.Max(0f, 1f - (distance / (rangeSize * 0.5f)));
            }
            
            // Inside range - perfect score
            return 1f;
        }

        /// <summary>
        /// Calculates derived environmental parameters (VPD, DewPoint, DLI, etc.)
        /// </summary>
        public void CalculateDerivedValues()
        {
            VPD = CalculateVPD(Temperature, Humidity, LeafSurfaceTemperature);
            DewPoint = CalculateDewPoint(Temperature, Humidity);
            DailyLightIntegral = CalculateDLI(LightIntensity, PhotoperiodHours);
            AbsoluteHumidity = CalculateAbsoluteHumidity(Temperature, Humidity);
        }
        
        /// <summary>
        /// Calculates Vapor Pressure Deficit using the Magnus formula.
        /// This is critical for professional cannabis cultivation.
        /// </summary>
        public static float CalculateVPD(float temperature, float relativeHumidity, float leafTempOffset = -2f)
        {
            float leafTemp = temperature + leafTempOffset;
            
            // Magnus formula for saturation vapor pressure (kPa)
            float leafSVP = 0.6108f * Mathf.Exp((17.27f * leafTemp) / (leafTemp + 237.3f));
            float airSVP = 0.6108f * Mathf.Exp((17.27f * temperature) / (temperature + 237.3f));
            
            // Actual vapor pressure from relative humidity
            float actualVP = airSVP * (relativeHumidity / 100f);
            
            // VPD = Leaf SVP - Actual VP
            return Mathf.Max(0f, leafSVP - actualVP);
        }
        
        /// <summary>
        /// Calculates dew point using the Magnus approximation.
        /// Critical for preventing mold and understanding humidity dynamics.
        /// </summary>
        public static float CalculateDewPoint(float temperature, float relativeHumidity)
        {
            float a = 17.27f;
            float b = 237.7f;
            float alpha = ((a * temperature) / (b + temperature)) + Mathf.Log(relativeHumidity / 100f);
            return (b * alpha) / (a - alpha);
        }
        
        /// <summary>
        /// Calculates Daily Light Integral - critical measurement for plant photosynthesis.
        /// DLI = PPFD × photoperiod × 0.0036 (conversion factor)
        /// </summary>
        public static float CalculateDLI(float ppfd, float photoperiodHours)
        {
            return ppfd * photoperiodHours * 0.0036f;
        }
        
        /// <summary>
        /// Calculates absolute humidity (water vapor content in g/m³).
        /// Important for precise environmental control.
        /// </summary>
        public static float CalculateAbsoluteHumidity(float temperature, float relativeHumidity)
        {
            // Saturation vapor pressure (Pa) using Magnus formula
            float svp = 610.7f * Mathf.Pow(10f, (7.5f * temperature) / (237.3f + temperature));
            
            // Actual vapor pressure
            float vp = svp * (relativeHumidity / 100f);
            
            // Absolute humidity using ideal gas law approximation
            // AH = (vp * M) / (R * T) where M = 18.016 g/mol (water), R = 8.314 J/(mol·K)
            float tempKelvin = temperature + 273.15f;
            return (vp * 18.016f) / (8.314f * tempKelvin);
        }
        
        /// <summary>
        /// Calculates effective leaf temperature considering air movement and radiation.
        /// Professional cultivation considers leaf temperature vs air temperature differences.
        /// </summary>
        public float CalculateLeafTemperature()
        {
            float baseLeafTemp = Temperature + LeafSurfaceTemperature;
            
            // Air movement cooling effect
            float airCooling = AirVelocity * 0.5f; // Simplified cooling from air movement
            
            // Light heating effect (especially under HPS lights)
            float lightHeating = (LightIntensity / 400f) * 2f; // Approximate heating from lights
            
            return baseLeafTemp - airCooling + lightHeating;
        }
        
        /// <summary>
        /// Calculates canopy-level environmental conditions.
        /// Considers microclimate effects within the plant canopy.
        /// </summary>
        public EnvironmentalConditions GetCanopyConditions()
        {
            var canopyConditions = this;
            
            // Apply canopy deltas
            canopyConditions.Temperature += CanopyTemperatureDelta;
            canopyConditions.Humidity = Mathf.Clamp(Humidity + CanopyHumidityDelta, 0f, 100f);
            canopyConditions.LightIntensity *= (CanopyLightPenetration / 100f);
            
            // Recalculate derived values for canopy conditions
            canopyConditions.CalculateDerivedValues();
            
            return canopyConditions;
        }
        
        /// <summary>
        /// Evaluates water quality for cannabis cultivation.
        /// Returns quality score from 0-1 (1 = optimal).
        /// </summary>
        public float EvaluateWaterQuality()
        {
            float score = 1f;
            
            // pH scoring (optimal 5.8-6.2 for hydro, 6.0-7.0 for soil)
            float pHScore = CalculateRangeScore(pH, 5.8f, 6.8f);
            
            // EC scoring (optimal 800-1600 μS/cm depending on stage)
            float ecScore = CalculateRangeScore(ElectricalConductivity, 800f, 1600f);
            
            // Dissolved oxygen scoring (optimal 5+ ppm)
            float doScore = DissolvedOxygen >= 5f ? 1f : DissolvedOxygen / 5f;
            
            // Water temperature scoring (optimal 18-22°C)
            float tempScore = CalculateRangeScore(WaterTemperature, 18f, 22f);
            
            // TDS penalty for overly mineralized water
            float tdsScore = TotalDissolvedSolids < 300f ? 1f : Mathf.Max(0f, 1f - (TotalDissolvedSolids - 300f) / 1000f);
            
            // Chlorine penalty
            float chlorineScore = ChlorineLevel < 0.5f ? 1f : Mathf.Max(0f, 1f - ChlorineLevel / 5f);
            
            return (pHScore + ecScore + doScore + tempScore + tdsScore + chlorineScore) / 6f;
        }
        
        /// <summary>
        /// Advanced environmental suitability calculation including professional parameters.
        /// </summary>
        public float CalculateAdvancedSuitability()
        {
            float baseSuitability = CalculateOverallSuitability();
            
            // VPD scoring (optimal 0.8-1.2 kPa for most growth stages)
            float vpdScore = CalculateRangeScore(VPD, 0.8f, 1.2f);
            
            // DLI scoring (optimal 30-50 mol/m²/day for cannabis)
            float dliScore = CalculateRangeScore(DailyLightIntegral, 30f, 50f);
            
            // Air movement scoring (optimal 0.3-1.0 m/s)
            float airScore = CalculateRangeScore(AirVelocity, 0.3f, 1.0f);
            
            // Water quality scoring
            float waterScore = EvaluateWaterQuality();
            
            // Weighted combination (base gets 60%, advanced metrics get 40%)
            return baseSuitability * 0.6f + (vpdScore + dliScore + airScore + waterScore) * 0.1f;
        }
        
        /// <summary>
        /// Determines if conditions are within professional cultivation standards.
        /// More stringent than basic acceptability ranges.
        /// </summary>
        public bool MeetsProfessionalStandards()
        {
            return CalculateAdvancedSuitability() > 0.8f &&
                   VPD >= 0.4f && VPD <= 1.5f &&
                   DailyLightIntegral >= 25f && DailyLightIntegral <= 60f &&
                   pH >= 5.5f && pH <= 7.2f &&
                   DissolvedOxygen >= 4f &&
                   ChlorineLevel < 1f;
        }
        
        /// <summary>
        /// Gets professional cultivation recommendations based on current conditions.
        /// </summary>
        public string[] GetProfessionalRecommendations()
        {
            var recommendations = new System.Collections.Generic.List<string>();
            
            // VPD recommendations
            if (VPD < 0.6f)
                recommendations.Add($"VPD too low ({VPD:F2} kPa). Increase temperature or decrease humidity.");
            else if (VPD > 1.4f)
                recommendations.Add($"VPD too high ({VPD:F2} kPa). Decrease temperature or increase humidity.");
            
            // DLI recommendations
            if (DailyLightIntegral < 25f)
                recommendations.Add($"DLI insufficient ({DailyLightIntegral:F1} mol/m²/day). Increase light intensity or duration.");
            else if (DailyLightIntegral > 60f)
                recommendations.Add($"DLI excessive ({DailyLightIntegral:F1} mol/m²/day). May cause light stress.");
            
            // Water quality recommendations
            float waterQuality = EvaluateWaterQuality();
            if (waterQuality < 0.7f)
                recommendations.Add($"Water quality suboptimal ({waterQuality:F2}). Check pH, EC, and dissolved oxygen.");
            
            // Air movement recommendations
            if (AirVelocity < 0.2f)
                recommendations.Add("Insufficient air movement. Increase ventilation to prevent stagnant air.");
            else if (AirVelocity > 2f)
                recommendations.Add("Excessive air movement may cause wind stress.");
            
            return recommendations.ToArray();
        }

        public override string ToString()
        {
            return $"Environmental Conditions - Temp: {Temperature:F1}°C, Humidity: {Humidity:F1}%, " +
                   $"VPD: {VPD:F2}kPa, DLI: {DailyLightIntegral:F1}, CO2: {CO2Level:F0}ppm, " +
                   $"Light: {LightIntensity:F0}PPFD, pH: {pH:F1}, Advanced Suitability: {CalculateAdvancedSuitability():F2}";
        }
    }
}