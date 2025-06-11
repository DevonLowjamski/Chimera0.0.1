using UnityEngine;
using ProjectChimera.Core;
using System.Collections.Generic;

namespace ProjectChimera.Data.Equipment
{
    /// <summary>
    /// Defines structural components of cultivation facilities including rooms, zones, and infrastructure.
    /// Handles capacity, utility requirements, and environmental containment properties.
    /// </summary>
    [CreateAssetMenu(fileName = "New Facility Component", menuName = "Project Chimera/Equipment/Facility Component")]
    public class FacilityComponentSO : ChimeraDataSO
    {
        [Header("Component Identity")]
        [SerializeField] private string _componentName;
        [SerializeField] private FacilityComponentType _componentType = FacilityComponentType.GrowRoom;
        [SerializeField, TextArea(3, 5)] private string _description;
        [SerializeField] private GameObject _componentPrefab;
        [SerializeField] private Sprite _componentIcon;
        
        [Header("Physical Properties")]
        [SerializeField] private Vector3 _dimensions = new Vector3(4f, 3f, 4f); // LxHxW in meters
        [SerializeField] private float _floorArea = 16f; // square meters
        [SerializeField] private float _volume = 48f; // cubic meters
        [SerializeField] private int _gridSizeX = 4;
        [SerializeField] private int _gridSizeY = 4;
        
        [Header("Construction Properties")]
        [SerializeField] private float _constructionCost = 1000f;
        [SerializeField] private float _constructionTime = 24f; // hours
        [SerializeField] private List<MaterialRequirement> _materialRequirements = new List<MaterialRequirement>();
        [SerializeField] private DifficultyLevel _constructionDifficulty = DifficultyLevel.Intermediate;
        
        [Header("Environmental Properties")]
        [SerializeField] private EnvironmentalIsolation _environmentalIsolation;
        [SerializeField] private float _thermalMass = 1000f; // thermal capacity
        [SerializeField] private float _airExchangeRate = 1f; // air changes per hour
        [SerializeField] private bool _isClimateControlled = true;
        
        [Header("Capacity and Limits")]
        [SerializeField] private int _maxPlantCapacity = 16;
        [SerializeField] private float _maxEquipmentWeight = 500f; // kg
        [SerializeField] private float _maxPowerLoad = 5000f; // watts
        [SerializeField] private float _maxWaterFlow = 100f; // liters per hour
        
        [Header("Utility Requirements")]
        [SerializeField] private List<UtilityRequirement> _utilityRequirements = new List<UtilityRequirement>();
        [SerializeField] private bool _requiresSpecialVentilation = false;
        [SerializeField] private bool _requiresWaterproof = false;
        [SerializeField] private bool _requiresFireSuppression = false;
        
        [Header("Equipment Compatibility")]
        [SerializeField] private List<EquipmentCategory> _compatibleEquipmentCategories = new List<EquipmentCategory>();
        [SerializeField] private List<EquipmentDataSO> _requiredEquipment = new List<EquipmentDataSO>();
        [SerializeField] private List<EquipmentDataSO> _recommendedEquipment = new List<EquipmentDataSO>();
        
        [Header("Automation Features")]
        [SerializeField] private bool _supportsAutomation = true;
        [SerializeField] private List<AutomationCapability> _automationCapabilities = new List<AutomationCapability>();
        [SerializeField] private int _maxSensorPoints = 8;
        [SerializeField] private int _maxControllerPoints = 4;
        
        [Header("Safety and Compliance")]
        [SerializeField] private List<SafetyFeature> _safetyFeatures = new List<SafetyFeature>();
        [SerializeField] private List<ComplianceRequirement> _complianceRequirements = new List<ComplianceRequirement>();
        [SerializeField] private SecurityLevel _securityLevel = SecurityLevel.Standard;
        
        // Public Properties
        public string ComponentName => _componentName;
        public FacilityComponentType ComponentType => _componentType;
        public string Description => _description;
        public GameObject ComponentPrefab => _componentPrefab;
        public Sprite ComponentIcon => _componentIcon;
        public Vector3 Dimensions => _dimensions;
        public float FloorArea => _floorArea;
        public float Volume => _volume;
        public int GridSizeX => _gridSizeX;
        public int GridSizeY => _gridSizeY;
        public float ConstructionCost => _constructionCost;
        public float ConstructionTime => _constructionTime;
        public List<MaterialRequirement> MaterialRequirements => _materialRequirements;
        public DifficultyLevel ConstructionDifficulty => _constructionDifficulty;
        public EnvironmentalIsolation EnvironmentalIsolation => _environmentalIsolation;
        public float ThermalMass => _thermalMass;
        public float AirExchangeRate => _airExchangeRate;
        public bool IsClimateControlled => _isClimateControlled;
        public int MaxPlantCapacity => _maxPlantCapacity;
        public float MaxEquipmentWeight => _maxEquipmentWeight;
        public float MaxPowerLoad => _maxPowerLoad;
        public float MaxWaterFlow => _maxWaterFlow;
        public List<UtilityRequirement> UtilityRequirements => _utilityRequirements;
        public bool RequiresSpecialVentilation => _requiresSpecialVentilation;
        public bool RequiresWaterproof => _requiresWaterproof;
        public bool RequiresFireSuppression => _requiresFireSuppression;
        public List<EquipmentCategory> CompatibleEquipmentCategories => _compatibleEquipmentCategories;
        public List<EquipmentDataSO> RequiredEquipment => _requiredEquipment;
        public List<EquipmentDataSO> RecommendedEquipment => _recommendedEquipment;
        public bool SupportsAutomation => _supportsAutomation;
        public List<AutomationCapability> AutomationCapabilities => _automationCapabilities;
        public int MaxSensorPoints => _maxSensorPoints;
        public int MaxControllerPoints => _maxControllerPoints;
        public List<SafetyFeature> SafetyFeatures => _safetyFeatures;
        public List<ComplianceRequirement> ComplianceRequirements => _complianceRequirements;
        public SecurityLevel SecurityLevel => _securityLevel;
        
        /// <summary>
        /// Calculates the total construction cost including materials and labor.
        /// </summary>
        public float CalculateTotalConstructionCost(float laborRate = 25f)
        {
            float materialCost = 0f;
            foreach (var material in _materialRequirements)
            {
                materialCost += material.Quantity * material.UnitCost;
            }
            
            float laborCost = _constructionTime * laborRate;
            
            return _constructionCost + materialCost + laborCost;
        }
        
        /// <summary>
        /// Checks if the component can accommodate a specific equipment piece.
        /// </summary>
        public bool CanAccommodateEquipment(EquipmentDataSO equipment)
        {
            // Check category compatibility
            if (!_compatibleEquipmentCategories.Contains(equipment.Category))
                return false;
            
            // Check physical constraints
            if (equipment.Weight > _maxEquipmentWeight)
                return false;
            
            if (equipment.PowerConsumption > _maxPowerLoad)
                return false;
            
            if (equipment.WaterConsumption > _maxWaterFlow)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Calculates environmental stability based on isolation properties.
        /// </summary>
        public float CalculateEnvironmentalStability()
        {
            float stability = 0.5f; // Base stability
            
            stability += _environmentalIsolation.ThermalInsulation * 0.2f;
            stability += _environmentalIsolation.AirSeal * 0.2f;
            stability += _environmentalIsolation.LightSeal * 0.1f;
            
            // Thermal mass contribution
            stability += Mathf.Min(0.2f, _thermalMass / 5000f);
            
            return Mathf.Clamp01(stability);
        }
        
        /// <summary>
        /// Gets the optimal plant density for this component type.
        /// </summary>
        public float GetOptimalPlantDensity()
        {
            return _maxPlantCapacity / _floorArea; // plants per square meter
        }
        
        /// <summary>
        /// Checks if all utility requirements are met.
        /// </summary>
        public bool AreUtilityRequirementsMet(List<UtilityType> availableUtilities)
        {
            foreach (var requirement in _utilityRequirements)
            {
                if (requirement.IsRequired && !availableUtilities.Contains(requirement.UtilityType))
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Calculates monthly operating cost for this component.
        /// </summary>
        public float CalculateMonthlyOperatingCost()
        {
            float cost = 0f;
            
            // Base maintenance cost proportional to construction cost
            cost += _constructionCost * 0.01f; // 1% per month
            
            // Utility costs (estimated)
            foreach (var utility in _utilityRequirements)
            {
                cost += utility.EstimatedMonthlyCost;
            }
            
            return cost;
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_componentName))
            {
                Debug.LogError($"FacilityComponentSO '{name}' has no component name assigned.", this);
                isValid = false;
            }
            
            if (_floorArea <= 0f)
            {
                Debug.LogError($"Facility Component {name}: Floor area must be positive");
                isValid = false;
            }
            
            if (_volume <= 0f)
            {
                Debug.LogError($"Facility Component {name}: Volume must be positive");
                isValid = false;
            }
            
            if (_maxPlantCapacity < 0)
            {
                Debug.LogError($"Facility Component {name}: Plant capacity cannot be negative");
                isValid = false;
            }
            
            // Validate dimensions match calculated area/volume
            float calculatedArea = _dimensions.x * _dimensions.z;
            if (Mathf.Abs(calculatedArea - _floorArea) > 0.1f)
            {
                Debug.LogWarning($"Facility Component {name}: Floor area doesn't match dimensions");
                isValid = false;
            }
            
            return isValid;
        }
    }
    
    [System.Serializable]
    public class EnvironmentalIsolation
    {
        [Range(0f, 1f)] public float ThermalInsulation = 0.7f;
        [Range(0f, 1f)] public float AirSeal = 0.8f;
        [Range(0f, 1f)] public float LightSeal = 0.9f;
        [Range(0f, 1f)] public float SoundInsulation = 0.5f;
        [Range(0f, 1f)] public float MoistureBarrier = 0.8f;
    }
    
    [System.Serializable]
    public class MaterialRequirement
    {
        public string MaterialName;
        public float Quantity = 1f;
        public string Unit = "units";
        public float UnitCost = 10f;
        [TextArea(2, 3)] public string MaterialDescription;
    }
    
    [System.Serializable]
    public class UtilityRequirement
    {
        public UtilityType UtilityType;
        public bool IsRequired = true;
        public float CapacityRequired = 100f;
        public string CapacityUnit = "units";
        public float EstimatedMonthlyCost = 50f;
        [TextArea(2, 3)] public string RequirementDescription;
    }
    
    [System.Serializable]
    public class AutomationCapability
    {
        public string CapabilityName;
        public AutomationType AutomationType;
        public bool IsStandardFeature = false;
        public float ImplementationCost = 100f;
        [TextArea(2, 3)] public string CapabilityDescription;
    }
    
    [System.Serializable]
    public class SafetyFeature
    {
        public string FeatureName;
        public SafetyType SafetyType;
        public bool IsRequired = true;
        public float ImplementationCost = 50f;
        [TextArea(2, 3)] public string FeatureDescription;
    }
    
    [System.Serializable]
    public class ComplianceRequirement
    {
        public string RequirementName;
        public ComplianceType ComplianceType;
        public bool IsLegalRequirement = true;
        public float ComplianceCost = 100f;
        [TextArea(2, 3)] public string RequirementDescription;
    }
    
    public enum FacilityComponentType
    {
        GrowRoom,
        VegetationRoom,
        FloweringRoom,
        CloneRoom,
        DryingRoom,
        CuringRoom,
        ProcessingRoom,
        StorageRoom,
        ControlRoom,
        UtilityRoom,
        QuarantineRoom,
        Laboratory,
        Office,
        Reception,
        SecurityRoom,
        MechanicalRoom,
        Corridor,
        LoadingDock,
        WasteArea,
        Extraction_Lab
    }
    
    public enum UtilityType
    {
        Electricity,
        Water,
        Sewer,
        NaturalGas,
        HVAC,
        Internet,
        Phone,
        Security,
        Fire_Suppression,
        CompressedAir,
        CO2_Supply,
        WasteManagement
    }
    
    public enum AutomationType
    {
        Environmental_Control,
        Irrigation_Control,
        Lighting_Control,
        Security_Control,
        Monitoring,
        Data_Logging,
        Alert_System,
        Remote_Access
    }
    
    public enum SafetyType
    {
        Fire_Safety,
        Electrical_Safety,
        Chemical_Safety,
        Structural_Safety,
        Emergency_Exit,
        First_Aid,
        Personal_Protection,
        Ventilation_Safety
    }
    
    public enum ComplianceType
    {
        Building_Code,
        Fire_Code,
        Electrical_Code,
        Cannabis_Regulation,
        Environmental_Regulation,
        Safety_Regulation,
        Security_Requirement,
        Quality_Standard
    }
    
    public enum SecurityLevel
    {
        Minimal,
        Standard,
        Enhanced,
        Maximum,
        Military_Grade
    }
}