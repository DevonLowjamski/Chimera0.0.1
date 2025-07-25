using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Test data ScriptableObjects for validating the DataManager.
    /// </summary>

    [CreateAssetMenu(fileName = "Test Plant Data", menuName = "Project Chimera/Testing/Test Plant Data")]
    public class TestPlantDataSO : ChimeraDataSO
    {
        [Header("Plant Properties")]
        public string PlantName = "Test Cannabis Plant";
        public float GrowthRate = 1.0f;
        public float MaxHeight = 2.0f;
        public int FloweringDays = 60;
        public float THCContent = 0.2f;
        public float CBDContent = 0.1f;

        [Header("Environmental Requirements")]
        public float OptimalTemperature = 24.0f;
        public float OptimalHumidity = 0.6f;
        public float LightRequirement = 800.0f;
        public float WaterRequirement = 2.0f;

        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(PlantName))
            {
                LogError("Plant name cannot be empty");
                isValid = false;
            }

            if (GrowthRate <= 0)
            {
                LogError("Growth rate must be positive");
                isValid = false;
            }

            if (MaxHeight <= 0)
            {
                LogError("Max height must be positive");
                isValid = false;
            }

            if (FloweringDays <= 0)
            {
                LogError("Flowering days must be positive");
                isValid = false;
            }

            if (THCContent < 0 || THCContent > 1)
            {
                LogError("THC content must be between 0 and 1");
                isValid = false;
            }

            if (CBDContent < 0 || CBDContent > 1)
            {
                LogError("CBD content must be between 0 and 1");
                isValid = false;
            }

            if (OptimalTemperature < 10 || OptimalTemperature > 40)
            {
                LogError("Optimal temperature must be between 10 and 40 degrees");
                isValid = false;
            }

            if (OptimalHumidity < 0 || OptimalHumidity > 1)
            {
                LogError("Optimal humidity must be between 0 and 1");
                isValid = false;
            }

            return isValid;
        }
    }

    [CreateAssetMenu(fileName = "Test Equipment Data", menuName = "Project Chimera/Testing/Test Equipment Data")]
    public class TestEquipmentDataSO : ChimeraDataSO
    {
        [Header("Equipment Properties")]
        public string EquipmentName = "Test LED Light";
        public EquipmentType Type = EquipmentType.Lighting;
        public float PowerConsumption = 300.0f;
        public float Efficiency = 0.85f;
        public float Cost = 500.0f;
        public float Durability = 1000.0f;

        [Header("Performance Characteristics")]
        public float Coverage = 4.0f;
        public float Intensity = 1000.0f;
        public Vector2 OperatingRange = new Vector2(0.1f, 1.0f);

        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(EquipmentName))
            {
                LogError("Equipment name cannot be empty");
                isValid = false;
            }

            if (PowerConsumption < 0)
            {
                LogError("Power consumption cannot be negative");
                isValid = false;
            }

            if (Efficiency < 0 || Efficiency > 1)
            {
                LogError("Efficiency must be between 0 and 1");
                isValid = false;
            }

            if (Cost <= 0)
            {
                LogError("Cost must be positive");
                isValid = false;
            }

            if (Durability <= 0)
            {
                LogError("Durability must be positive");
                isValid = false;
            }

            if (Coverage <= 0)
            {
                LogError("Coverage must be positive");
                isValid = false;
            }

            if (OperatingRange.x >= OperatingRange.y)
            {
                LogError("Operating range minimum must be less than maximum");
                isValid = false;
            }

            return isValid;
        }
    }

    [CreateAssetMenu(fileName = "Test Environment Config", menuName = "Project Chimera/Testing/Test Environment Config")]
    public class TestEnvironmentConfigSO : ChimeraConfigSO
    {
        [Header("Environmental Settings")]
        public float DefaultTemperature = 24.0f;
        public float DefaultHumidity = 0.6f;
        public float DefaultCO2Level = 400.0f;
        public float DefaultLightIntensity = 800.0f;

        [Header("Simulation Parameters")]
        public float TemperatureFluctuation = 2.0f;
        public float HumidityFluctuation = 0.1f;
        public float WeatherChangeInterval = 3600.0f;
        public bool EnableRandomEvents = true;

        public override bool ValidateData()
        {
            if (!base.ValidateData()) return false;
            
            bool isValid = true;

            if (DefaultTemperature < 10 || DefaultTemperature > 40)
            {
                Debug.LogError($"[Chimera][{GetType().Name}] Default temperature must be between 10 and 40 degrees", this);
                isValid = false;
            }

            if (DefaultHumidity < 0 || DefaultHumidity > 1)
            {
                Debug.LogError($"[Chimera][{GetType().Name}] Default humidity must be between 0 and 1", this);
                isValid = false;
            }

            if (DefaultCO2Level < 200 || DefaultCO2Level > 2000)
            {
                Debug.LogError($"[Chimera][{GetType().Name}] Default CO2 level must be between 200 and 2000 ppm", this);
                isValid = false;
            }

            if (DefaultLightIntensity < 0)
            {
                Debug.LogError($"[Chimera][{GetType().Name}] Default light intensity cannot be negative", this);
                isValid = false;
            }

            if (TemperatureFluctuation < 0)
            {
                Debug.LogError($"[Chimera][{GetType().Name}] Temperature fluctuation cannot be negative", this);
                isValid = false;
            }

            if (HumidityFluctuation < 0 || HumidityFluctuation > 0.5f)
            {
                Debug.LogError($"[Chimera][{GetType().Name}] Humidity fluctuation must be between 0 and 0.5", this);
                isValid = false;
            }

            if (WeatherChangeInterval <= 0)
            {
                Debug.LogError($"[Chimera][{GetType().Name}] Weather change interval must be positive", this);
                isValid = false;
            }

            return isValid;
        }
    }

    public enum EquipmentType
    {
        Lighting,
        Ventilation,
        Irrigation,
        Monitoring,
        Environmental,
        Harvesting,
        Processing
    }
}