using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Genetics;
using System.Collections.Generic;

namespace ProjectChimera.Data.Facilities
{
    /// <summary>
    /// ScriptableObject for defining equipment specifications and operational parameters.
    /// Used for all cultivation facility equipment including lights, HVAC, irrigation, and sensors.
    /// </summary>
    [CreateAssetMenu(fileName = "New Facility Equipment Data", menuName = "Project Chimera/Facilities/Facility Equipment Data", order = 1)]
    public class FacilityEquipmentDataSO : ChimeraDataSO
    {
        [Header("Basic Equipment Information")]
        [SerializeField] private string _equipmentName = "New Equipment";
        [SerializeField] private EquipmentCategory _category = EquipmentCategory.Lighting;
        [SerializeField] private string _manufacturer = "";
        [SerializeField] private string _modelNumber = "";
        [SerializeField] private string _description = "";
        
        [Header("Physical Specifications")]
        [SerializeField] private Vector3 _dimensions = Vector3.one;
        [SerializeField] private float _weight = 1f;
        [SerializeField] private string _mountingType = "Floor";
        [SerializeField] private bool _requiresVentilation = false;
        [SerializeField] private float _heatOutput = 0f;           // BTU/hr
        [SerializeField] private float _noiseLevel = 30f;         // dB
        
        [Header("Electrical Requirements")]
        [SerializeField] private float _powerConsumption = 100f;   // Watts
        [SerializeField] private float _voltage = 120f;           // Volts
        [SerializeField] private float _amperage = 1f;            // Amps
        [SerializeField] private string _plugType = "Standard";
        [SerializeField] private bool _requiresDedicatedCircuit = false;
        
        [Header("Environmental Effects")]
        [SerializeField] private EnvironmentalEffects _environmentalEffects;
        
        [Header("Operational Parameters")]
        [SerializeField] private Vector2 _operationalRange = new Vector2(0f, 100f);
        [SerializeField] private float _efficiency = 0.9f;        // 0-1
        [SerializeField] private float _lifespan = 50000f;        // Hours
        [SerializeField] private bool _isDimmable = false;
        [SerializeField] private bool _hasAutomation = false;
        [SerializeField] private ControlInterface _controlInterface = ControlInterface.Manual;
        
        [Header("Economic Data")]
        [SerializeField] private float _purchaseCost = 100f;
        [SerializeField] private float _installationCost = 50f;
        [SerializeField] private float _maintenanceCost = 10f;    // Per year
        [SerializeField] private float _operatingCost = 0.1f;     // Per hour
        [SerializeField] private int _warrantyPeriod = 12;        // Months
        
        [Header("Maintenance Requirements")]
        [SerializeField] private MaintenanceRequirements _maintenanceRequirements;
        
        [Header("Cannabis-Specific Features")]
        [SerializeField] private bool _isFullSpectrum = false;
        [SerializeField] private bool _hasUVOutput = false;
        [SerializeField] private bool _hasIROutput = false;
        [SerializeField] private CannabisGrowthStageOptimization _stageOptimization;
        [SerializeField] private SpectrumProfile _spectrumProfile;
        
        // Public Properties
        public string EquipmentName => _equipmentName;
        public EquipmentCategory Category => _category;
        public string Manufacturer => _manufacturer;
        public string ModelNumber => _modelNumber;
        public string Description => _description;
        public Vector3 Dimensions => _dimensions;
        public float Weight => _weight;
        public string MountingType => _mountingType;
        public bool RequiresVentilation => _requiresVentilation;
        public float HeatOutput => _heatOutput;
        public float NoiseLevel => _noiseLevel;
        public float PowerConsumption => _powerConsumption;
        public float Voltage => _voltage;
        public float Amperage => _amperage;
        public string PlugType => _plugType;
        public bool RequiresDedicatedCircuit => _requiresDedicatedCircuit;
        public EnvironmentalEffects EnvironmentalEffects => _environmentalEffects;
        public Vector2 OperationalRange => _operationalRange;
        public float Efficiency => _efficiency;
        public float Lifespan => _lifespan;
        public bool IsDimmable => _isDimmable;
        public bool HasAutomation => _hasAutomation;
        public ControlInterface ControlInterface => _controlInterface;
        public float PurchaseCost => _purchaseCost;
        public float InstallationCost => _installationCost;
        public float MaintenanceCost => _maintenanceCost;
        public float OperatingCost => _operatingCost;
        public int WarrantyPeriod => _warrantyPeriod;
        public MaintenanceRequirements MaintenanceRequirements => _maintenanceRequirements;
        public bool IsFullSpectrum => _isFullSpectrum;
        public bool HasUVOutput => _hasUVOutput;
        public bool HasIROutput => _hasIROutput;
        public CannabisGrowthStageOptimization StageOptimization => _stageOptimization;
        public SpectrumProfile SpectrumProfile => _spectrumProfile;
        
        /// <summary>
        /// Calculates total cost of ownership over specified period.
        /// </summary>
        public float CalculateTotalCostOfOwnership(float operationalHours)
        {
            float initialCost = _purchaseCost + _installationCost;
            float operationalCost = _operatingCost * operationalHours;
            float maintenanceCost = _maintenanceCost * (operationalHours / 8760f); // Per year
            return initialCost + operationalCost + maintenanceCost;
        }
        
        /// <summary>
        /// Gets equipment effectiveness for specific growth stage.
        /// </summary>
        public float GetStageEffectiveness(PlantGrowthStage stage)
        {
            switch (stage)
            {
                case PlantGrowthStage.Seedling:
                    return _stageOptimization.SeedlingEffectiveness;
                case PlantGrowthStage.Vegetative:
                    return _stageOptimization.VegetativeEffectiveness;
                case PlantGrowthStage.Flowering:
                    return _stageOptimization.FloweringEffectiveness;
                default:
                    return 1f;
            }
        }
        
        /// <summary>
        /// Validates equipment data for completeness and accuracy.
        /// </summary>
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_equipmentName))
            {
                LogError("Equipment name cannot be empty");
                isValid = false;
            }
            
            if (_powerConsumption < 0f)
            {
                LogError("Power consumption cannot be negative");
                isValid = false;
            }
            
            if (_efficiency < 0f || _efficiency > 1f)
            {
                LogError("Efficiency must be between 0 and 1");
                isValid = false;
            }
            
            if (_lifespan <= 0f)
            {
                LogError("Lifespan must be positive");
                isValid = false;
            }
            
            if (_purchaseCost < 0f)
            {
                LogError("Purchase cost cannot be negative");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Applies environmental effects to room conditions.
        /// </summary>
        public void ApplyEnvironmentalEffects(ref EnvironmentalConditions conditions, float powerLevel = 1f)
        {
            conditions.Temperature += _environmentalEffects.TemperatureChange * powerLevel;
            conditions.Humidity += _environmentalEffects.HumidityChange * powerLevel;
            conditions.LightIntensity += _environmentalEffects.LightIntensityChange * powerLevel;
            conditions.CO2Level += _environmentalEffects.CO2Change * powerLevel;
            conditions.AirVelocity += _environmentalEffects.AirVelocityChange * powerLevel;
            
            // Apply spectrum if this is a light
            if (_category == EquipmentCategory.Lighting && conditions.LightSpectrumData != null)
            {
                ApplySpectrumProfile(ref conditions.LightSpectrumData, powerLevel);
            }
        }
        
        private void ApplySpectrumProfile(ref LightSpectrumData spectrum, float intensity)
        {
            if (_spectrumProfile == null) return;
            
            spectrum.Blue_420_490nm += _spectrumProfile.BlueOutput * intensity;
            spectrum.Red_630_660nm += _spectrumProfile.RedOutput * intensity;
            spectrum.DeepRed_660_700nm += _spectrumProfile.DeepRedOutput * intensity;
            spectrum.UV_A_315_400nm += _spectrumProfile.UVOutput * intensity;
            spectrum.Green_490_550nm += _spectrumProfile.GreenOutput * intensity;
            spectrum.Yellow_550_590nm += _spectrumProfile.YellowOutput * intensity;
            spectrum.Orange_590_630nm += _spectrumProfile.OrangeOutput * intensity;
            spectrum.FarRed_700_850nm += _spectrumProfile.FarRedOutput * intensity;
        }
    }
    
    /// <summary>
    /// Environmental effects of equipment operation.
    /// </summary>
    [System.Serializable]
    public class EnvironmentalEffects
    {
        public float TemperatureChange = 0f;      // °C change
        public float HumidityChange = 0f;         // %RH change
        public float LightIntensityChange = 0f;   // PPFD change
        public float CO2Change = 0f;              // ppm change
        public float AirVelocityChange = 0f;      // m/s change
        public float NoiseIncrease = 0f;          // dB increase
        public float VibrationIncrease = 0f;      // Vibration units
    }
    
    /// <summary>
    /// Maintenance requirements for equipment.
    /// </summary>
    [System.Serializable]
    public class MaintenanceRequirements
    {
        public int CleaningFrequency = 30;        // Days
        public int CalibrationFrequency = 90;     // Days
        public int InspectionFrequency = 180;     // Days
        public int ReplacementFrequency = 365;    // Days
        public bool RequiresSpecialist = false;
        public string MaintenanceNotes = "";
        public List<string> RequiredTools = new List<string>();
        public List<string> ReplacementParts = new List<string>();
    }
    
    /// <summary>
    /// Cannabis growth stage optimization parameters.
    /// </summary>
    [System.Serializable]
    public class CannabisGrowthStageOptimization
    {
        [Range(0f, 2f)] public float SeedlingEffectiveness = 1f;
        [Range(0f, 2f)] public float VegetativeEffectiveness = 1f;
        [Range(0f, 2f)] public float FloweringEffectiveness = 1f;
        public bool OptimizeForTHC = false;
        public bool OptimizeForCBD = false;
        public bool OptimizeForTerpenes = false;
        public bool OptimizeForYield = true;
    }
    
    /// <summary>
    /// Light spectrum profile for lighting equipment.
    /// </summary>
    [System.Serializable]
    public class SpectrumProfile
    {
        [Range(0f, 500f)] public float BlueOutput = 0f;      // 420-490nm
        [Range(0f, 500f)] public float GreenOutput = 0f;     // 490-570nm
        [Range(0f, 500f)] public float YellowOutput = 0f;    // 570-590nm
        [Range(0f, 500f)] public float OrangeOutput = 0f;    // 590-630nm
        [Range(0f, 500f)] public float RedOutput = 0f;       // 630-660nm
        [Range(0f, 500f)] public float DeepRedOutput = 0f;   // 660-700nm
        [Range(0f, 100f)] public float FarRedOutput = 0f;    // 700-850nm
        [Range(0f, 50f)] public float UVOutput = 0f;         // 315-400nm
        [Range(0f, 100f)] public float InfraredOutput = 0f;  // 700+nm
    }
    
    /// <summary>
    /// Equipment categories for organization.
    /// </summary>
    public enum EquipmentCategory
    {
        Lighting,
        HVAC,
        Irrigation,
        Sensors,
        Security,
        Processing,
        Electrical,
        Plumbing,
        Ventilation,
        Monitoring,
        Automation,
        Safety
    }
    
    /// <summary>
    /// Control interface types.
    /// </summary>
    public enum ControlInterface
    {
        Manual,
        Analog,
        Digital,
        Ethernet,
        WiFi,
        Bluetooth,
        ModBus,
        BACnet
    }
    

}