using UnityEngine;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Real-time environmental conditions affecting plant growth and health.
    /// </summary>
    [System.Serializable]
    public class CultivationEnvironmentalConditions
    {
        [Header("Climate Parameters")]
        [Range(-10f, 50f)] public float Temperature = 24f; // Celsius
        [Range(0f, 100f)] public float Humidity = 60f; // Percentage
        [Range(0f, 2000f)] public float LightIntensity = 600f; // PPFD μmol/m²/s
        [Range(12f, 24f)] public float PhotoperiodHours = 18f; // Hours of light per day
        [Range(300f, 2000f)] public float CO2Level = 400f; // PPM
        
        [Header("Air Quality")]
        [Range(0f, 100f)] public float AirCirculation = 80f; // Percentage
        [Range(0f, 5f)] public float AirVelocity = 0.5f; // m/s
        public bool HasFreshAirIntake = true;
        
        [Header("Soil/Growing Medium")]
        [Range(5.5f, 8.5f)] public float pH = 6.5f;
        [Range(0f, 3000f)] public float EC = 1200f; // Electrical Conductivity μS/cm
        [Range(0f, 100f)] public float MoisureLevel = 70f; // Percentage
        public GrowingMediumType GrowingMedium = GrowingMediumType.Soil;
        
        [Header("Nutrient Levels")]
        [Range(0f, 500f)] public float NitrogenPPM = 150f;
        [Range(0f, 200f)] public float PhosphorusPPM = 50f;
        [Range(0f, 300f)] public float PotassiumPPM = 200f;
        [Range(0f, 100f)] public float CalciumPPM = 80f;
        [Range(0f, 60f)] public float MagnesiumPPM = 40f;
        
        [Header("Environmental Status")]
        public bool IsStableEnvironment = true;
        public float StabilityScore = 1f; // 0-1, how stable conditions are
        public System.DateTime LastUpdated = System.DateTime.Now;
        
        /// <summary>
        /// Creates default optimal growing conditions.
        /// </summary>
        public static CultivationEnvironmentalConditions CreateOptimal()
        {
            return new CultivationEnvironmentalConditions
            {
                Temperature = 24f,
                Humidity = 60f,
                LightIntensity = 600f,
                PhotoperiodHours = 18f,
                CO2Level = 800f,
                AirCirculation = 85f,
                AirVelocity = 0.8f,
                HasFreshAirIntake = true,
                pH = 6.2f,
                EC = 1200f,
                MoisureLevel = 70f,
                GrowingMedium = GrowingMediumType.Soil,
                NitrogenPPM = 150f,
                PhosphorusPPM = 50f,
                PotassiumPPM = 200f,
                CalciumPPM = 80f,
                MagnesiumPPM = 40f,
                IsStableEnvironment = true,
                StabilityScore = 1f
            };
        }
        
        /// <summary>
        /// Creates stressed environmental conditions for testing.
        /// </summary>
        public static CultivationEnvironmentalConditions CreateStressed()
        {
            return new CultivationEnvironmentalConditions
            {
                Temperature = 32f, // Too hot
                Humidity = 85f, // Too humid
                LightIntensity = 200f, // Too dim
                PhotoperiodHours = 12f,
                CO2Level = 350f, // Too low
                AirCirculation = 30f, // Poor circulation
                AirVelocity = 0.1f,
                HasFreshAirIntake = false,
                pH = 7.5f, // Too alkaline
                EC = 2500f, // Too high nutrient concentration
                MoisureLevel = 90f, // Overwatered
                GrowingMedium = GrowingMediumType.Soil,
                NitrogenPPM = 300f, // Nitrogen toxicity
                PhosphorusPPM = 20f, // Phosphorus deficiency
                PotassiumPPM = 100f, // Potassium deficiency
                CalciumPPM = 40f,
                MagnesiumPPM = 20f,
                IsStableEnvironment = false,
                StabilityScore = 0.3f
            };
        }
        
        /// <summary>
        /// Compares this condition set with another and returns a similarity score.
        /// </summary>
        public float CompareWith(CultivationEnvironmentalConditions other)
        {
            if (other == null)
                return 0f;
            
            float tempSimilarity = 1f - (Mathf.Abs(Temperature - other.Temperature) / 40f);
            float humiditySimilarity = 1f - (Mathf.Abs(Humidity - other.Humidity) / 100f);
            float lightSimilarity = 1f - (Mathf.Abs(LightIntensity - other.LightIntensity) / 2000f);
            float co2Similarity = 1f - (Mathf.Abs(CO2Level - other.CO2Level) / 1700f);
            float pHSimilarity = 1f - (Mathf.Abs(pH - other.pH) / 3f);
            
            float avgSimilarity = (tempSimilarity + humiditySimilarity + lightSimilarity + co2Similarity + pHSimilarity) / 5f;
            
            return Mathf.Clamp01(avgSimilarity);
        }
        
        /// <summary>
        /// Calculates overall environmental quality score.
        /// </summary>
        public float CalculateQualityScore()
        {
            float score = 0f;
            
            // Temperature scoring (optimal 22-26°C)
            if (Temperature >= 22f && Temperature <= 26f)
                score += 0.2f;
            else
                score += Mathf.Max(0f, 0.2f - (Mathf.Abs(Temperature - 24f) / 20f));
            
            // Humidity scoring (optimal 55-65%)
            if (Humidity >= 55f && Humidity <= 65f)
                score += 0.2f;
            else
                score += Mathf.Max(0f, 0.2f - (Mathf.Abs(Humidity - 60f) / 40f));
            
            // Light scoring (optimal 500-800 PPFD)
            if (LightIntensity >= 500f && LightIntensity <= 800f)
                score += 0.2f;
            else
                score += Mathf.Max(0f, 0.2f - (Mathf.Abs(LightIntensity - 650f) / 1350f));
            
            // CO2 scoring (optimal 600-1000 PPM)
            if (CO2Level >= 600f && CO2Level <= 1000f)
                score += 0.2f;
            else
                score += Mathf.Max(0f, 0.2f - (Mathf.Abs(CO2Level - 800f) / 1200f));
            
            // pH scoring (optimal 6.0-6.8)
            if (pH >= 6.0f && pH <= 6.8f)
                score += 0.2f;
            else
                score += Mathf.Max(0f, 0.2f - (Mathf.Abs(pH - 6.4f) / 2.4f));
            
            return Mathf.Clamp01(score);
        }
        
        /// <summary>
        /// Applies gradual changes to environmental conditions.
        /// </summary>
        public void ApplyGradualChange(CultivationEnvironmentalConditions target, float changeRate, float deltaTime)
        {
            float change = changeRate * deltaTime;
            
            Temperature = Mathf.MoveTowards(Temperature, target.Temperature, change * 10f);
            Humidity = Mathf.MoveTowards(Humidity, target.Humidity, change * 100f);
            LightIntensity = Mathf.MoveTowards(LightIntensity, target.LightIntensity, change * 200f);
            CO2Level = Mathf.MoveTowards(CO2Level, target.CO2Level, change * 100f);
            pH = Mathf.MoveTowards(pH, target.pH, change * 0.5f);
            EC = Mathf.MoveTowards(EC, target.EC, change * 500f);
            MoisureLevel = Mathf.MoveTowards(MoisureLevel, target.MoisureLevel, change * 50f);
            
            // Update stability based on how close we are to target
            float similarity = CompareWith(target);
            StabilityScore = Mathf.Lerp(StabilityScore, similarity, deltaTime);
            IsStableEnvironment = StabilityScore > 0.8f;
            
            LastUpdated = System.DateTime.Now;
        }
        
        /// <summary>
        /// Validates that all environmental parameters are within reasonable ranges.
        /// </summary>
        public bool IsValid()
        {
            if (Temperature < -10f || Temperature > 50f) return false;
            if (Humidity < 0f || Humidity > 100f) return false;
            if (LightIntensity < 0f || LightIntensity > 2000f) return false;
            if (PhotoperiodHours < 12f || PhotoperiodHours > 24f) return false;
            if (CO2Level < 300f || CO2Level > 2000f) return false;
            if (pH < 5.5f || pH > 8.5f) return false;
            if (EC < 0f || EC > 3000f) return false;
            if (MoisureLevel < 0f || MoisureLevel > 100f) return false;
            
            return true;
        }
        
        /// <summary>
        /// Creates a copy of these environmental conditions.
        /// </summary>
        public CultivationEnvironmentalConditions Clone()
        {
            return new CultivationEnvironmentalConditions
            {
                Temperature = this.Temperature,
                Humidity = this.Humidity,
                LightIntensity = this.LightIntensity,
                PhotoperiodHours = this.PhotoperiodHours,
                CO2Level = this.CO2Level,
                AirCirculation = this.AirCirculation,
                AirVelocity = this.AirVelocity,
                HasFreshAirIntake = this.HasFreshAirIntake,
                pH = this.pH,
                EC = this.EC,
                MoisureLevel = this.MoisureLevel,
                GrowingMedium = this.GrowingMedium,
                NitrogenPPM = this.NitrogenPPM,
                PhosphorusPPM = this.PhosphorusPPM,
                PotassiumPPM = this.PotassiumPPM,
                CalciumPPM = this.CalciumPPM,
                MagnesiumPPM = this.MagnesiumPPM,
                IsStableEnvironment = this.IsStableEnvironment,
                StabilityScore = this.StabilityScore,
                LastUpdated = this.LastUpdated
            };
        }

        /// <summary>
        /// Check if the environmental conditions are initialized (compatibility method).
        /// </summary>
        public bool IsInitialized()
        {
            // Consider initialized if basic parameters are set to reasonable values
            return Temperature > -50f && Temperature < 100f &&
                   Humidity >= 0f && Humidity <= 100f &&
                   LightIntensity >= 0f &&
                   pH > 0f && pH < 14f;
        }

        /// <summary>
        /// Creates default indoor environmental conditions (compatibility method).
        /// </summary>
        public static CultivationEnvironmentalConditions CreateIndoorDefault()
        {
            return CreateOptimal(); // Use existing optimal conditions as indoor default
        }

        public override string ToString()
        {
            return $"Temp: {Temperature:F1}°C, Humidity: {Humidity:F1}%, Light: {LightIntensity:F0} PPFD, pH: {pH:F1}";
        }
    }
    
    /// <summary>
    /// Types of growing media used in cultivation.
    /// </summary>
    public enum GrowingMediumType
    {
        Soil,
        Coco_Coir,
        Perlite,
        Rockwool,
        Hydroton,
        Deep_Water_Culture,
        Nutrient_Film_Technique,
        Aeroponics,
        Custom_Mix
    }
    
    /// <summary>
    /// Types of environmental stress that can affect plants.
    /// </summary>
    public enum StressType
    {
        Heat_Stress,
        Cold_Stress,
        Light_Burn,
        Light_Deficiency,
        Overwatering,
        Underwatering,
        Nutrient_Burn,
        Nutrient_Deficiency,
        pH_Imbalance,
        Disease,
        Pest_Infestation,
        Air_Stagnation,
        Humidity_Stress,
        Physical_Damage
    }
    
    /// <summary>
    /// Cannabinoid concentration profile.
    /// </summary>
    [System.Serializable]
    public class CannabinoidProfile
    {
        [Range(0f, 35f)] public float THC = 20f; // Percentage
        [Range(0f, 25f)] public float CBD = 1f;
        [Range(0f, 5f)] public float CBG = 0.5f;
        [Range(0f, 3f)] public float CBN = 0.1f;
        [Range(0f, 2f)] public float CBC = 0.2f;
        [Range(0f, 1f)] public float THCV = 0.1f;
        [Range(0f, 1f)] public float CBDV = 0.05f;
        
        // Compatibility properties for PlantUpdateProcessor and PlantManager
        public float THCPercentage 
        { 
            get => THC; 
            set => THC = value; 
        }
        
        public float CBDPercentage 
        { 
            get => CBD; 
            set => CBD = value; 
        }
        
        public float TotalCannabinoids => THC + CBD + CBG + CBN + CBC + THCV + CBDV;
        
        public override string ToString()
        {
            return $"THC: {THC:F1}%, CBD: {CBD:F1}%, CBG: {CBG:F1}%, Total: {TotalCannabinoids:F1}%";
        }
    }
    
    /// <summary>
    /// Terpene concentration profile.
    /// </summary>
    [System.Serializable]
    public class TerpeneProfile
    {
        [Range(0f, 3f)] public float Myrcene = 0.8f; // Percentage
        [Range(0f, 2f)] public float Limonene = 0.5f;
        [Range(0f, 1.5f)] public float Pinene = 0.3f;
        [Range(0f, 1f)] public float Linalool = 0.2f;
        [Range(0f, 1.5f)] public float Caryophyllene = 0.4f;
        [Range(0f, 1f)] public float Humulene = 0.2f;
        [Range(0f, 0.8f)] public float Terpinolene = 0.1f;
        [Range(0f, 0.5f)] public float Ocimene = 0.05f;
        
        public float TotalTerpenes => Myrcene + Limonene + Pinene + Linalool + Caryophyllene + Humulene + Terpinolene + Ocimene;
        
        public override string ToString()
        {
            return $"Myrcene: {Myrcene:F2}%, Limonene: {Limonene:F2}%, Pinene: {Pinene:F2}%, Total: {TotalTerpenes:F2}%";
        }
    }
}