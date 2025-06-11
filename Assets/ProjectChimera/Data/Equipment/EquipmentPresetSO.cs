using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using System.Collections.Generic;

namespace ProjectChimera.Data.Equipment
{
    /// <summary>
    /// Defines preset equipment packages for specific cultivation setups or facility types.
    /// Provides curated combinations of equipment with balanced performance and cost optimization.
    /// </summary>
    [CreateAssetMenu(fileName = "New Equipment Preset", menuName = "Project Chimera/Equipment/Equipment Preset")]
    public class EquipmentPresetSO : ChimeraDataSO
    {
        [Header("Preset Identity")]
        [SerializeField] private string _presetName;
        [SerializeField] private PresetCategory _category = PresetCategory.Complete_Room;
        [SerializeField] private CultivationStyle _cultivationStyle = CultivationStyle.Indoor_Hydroponic;
        [SerializeField, TextArea(3, 5)] private string _description;
        [SerializeField] private Sprite _presetIcon;
        
        [Header("Target Specifications")]
        [SerializeField] private Vector2 _roomSizeRange = new Vector2(16f, 32f); // square meters
        [SerializeField] private int _targetPlantCount = 16;
        [SerializeField] private ProjectChimera.Data.Genetics.StrainType _targetStrainType = ProjectChimera.Data.Genetics.StrainType.Hybrid;
        [SerializeField] private DifficultyLevel _complexityLevel = DifficultyLevel.Intermediate;
        
        [Header("Equipment List")]
        [SerializeField] private List<EquipmentItem> _equipmentItems = new List<EquipmentItem>();
        [SerializeField] private List<EquipmentDataSO> _optionalEquipment = new List<EquipmentDataSO>();
        [SerializeField] private List<EquipmentDataSO> _upgradableEquipment = new List<EquipmentDataSO>();
        
        [Header("Performance Metrics")]
        [SerializeField] private PerformanceRating _performanceRating;
        [SerializeField] private EfficiencyMetrics _efficiencyMetrics;
        [SerializeField] private float _expectedYieldPerSqM = 0.5f; // kg per square meter
        
        [Header("Cost Analysis")]
        [SerializeField] private float _initialInvestment = 5000f;
        [SerializeField] private float _monthlyOperatingCost = 300f;
        [SerializeField] private float _paybackPeriodMonths = 12f;
        [SerializeField] private ROI_Analysis _roiAnalysis;
        
        [Header("Automation Level")]
        [SerializeField] private AutomationLevel _automationLevel = AutomationLevel.Semi_Automated;
        [SerializeField] private List<AutomationFeature> _automationFeatures = new List<AutomationFeature>();
        [SerializeField] private float _laborHoursPerWeek = 10f;
        
        [Header("Environmental Capabilities")]
        [SerializeField] private EnvironmentalCapabilities _environmentalCapabilities;
        [SerializeField] private List<EnvironmentalScenario> _supportedScenarios = new List<EnvironmentalScenario>();
        
        [Header("Maintenance Requirements")]
        [SerializeField] private MaintenanceProfile _maintenanceProfile;
        [SerializeField] private float _averageMaintenanceHoursPerMonth = 8f;
        [SerializeField] private List<PreventiveMaintenanceTask> _maintenanceTasks = new List<PreventiveMaintenanceTask>();
        
        // Public Properties
        public string PresetName => _presetName;
        public PresetCategory Category => _category;
        public CultivationStyle CultivationStyle => _cultivationStyle;
        public string Description => _description;
        public Sprite PresetIcon => _presetIcon;
        public Vector2 RoomSizeRange => _roomSizeRange;
        public int TargetPlantCount => _targetPlantCount;
        public ProjectChimera.Data.Genetics.StrainType TargetStrainType => _targetStrainType;
        public DifficultyLevel ComplexityLevel => _complexityLevel;
        public List<EquipmentItem> EquipmentItems => _equipmentItems;
        public List<EquipmentDataSO> OptionalEquipment => _optionalEquipment;
        public List<EquipmentDataSO> UpgradableEquipment => _upgradableEquipment;
        public PerformanceRating PerformanceRating => _performanceRating;
        public EfficiencyMetrics EfficiencyMetrics => _efficiencyMetrics;
        public float ExpectedYieldPerSqM => _expectedYieldPerSqM;
        public float InitialInvestment => _initialInvestment;
        public float MonthlyOperatingCost => _monthlyOperatingCost;
        public float PaybackPeriodMonths => _paybackPeriodMonths;
        public ROI_Analysis ROIAnalysis => _roiAnalysis;
        public AutomationLevel AutomationLevel => _automationLevel;
        public List<AutomationFeature> AutomationFeatures => _automationFeatures;
        public float LaborHoursPerWeek => _laborHoursPerWeek;
        public EnvironmentalCapabilities EnvironmentalCapabilities => _environmentalCapabilities;
        public List<EnvironmentalScenario> SupportedScenarios => _supportedScenarios;
        public MaintenanceProfile MaintenanceProfile => _maintenanceProfile;
        public float AverageMaintenanceHoursPerMonth => _averageMaintenanceHoursPerMonth;
        public List<PreventiveMaintenanceTask> MaintenanceTasks => _maintenanceTasks;
        
        /// <summary>
        /// Calculates the total equipment cost for this preset.
        /// </summary>
        public float CalculateTotalEquipmentCost()
        {
            float totalCost = 0f;
            
            foreach (var item in _equipmentItems)
            {
                if (item.Equipment != null)
                {
                    totalCost += (item.Equipment.PurchaseCost + item.Equipment.InstallationCost) * item.Quantity;
                }
            }
            
            return totalCost;
        }
        
        /// <summary>
        /// Calculates total power consumption for all equipment in the preset.
        /// </summary>
        public float CalculateTotalPowerConsumption()
        {
            float totalPower = 0f;
            
            foreach (var item in _equipmentItems)
            {
                if (item.Equipment != null)
                {
                    totalPower += item.Equipment.PowerConsumption * item.Quantity;
                }
            }
            
            return totalPower;
        }
        
        /// <summary>
        /// Gets all equipment of a specific category in this preset.
        /// </summary>
        public List<EquipmentDataSO> GetEquipmentByCategory(EquipmentCategory category)
        {
            var equipmentList = new List<EquipmentDataSO>();
            
            foreach (var item in _equipmentItems)
            {
                if (item.Equipment != null && item.Equipment.Category == category)
                {
                    equipmentList.Add(item.Equipment);
                }
            }
            
            return equipmentList;
        }
        
        /// <summary>
        /// Evaluates compatibility with a specific facility component.
        /// </summary>
        public float EvaluateCompatibility(FacilityComponentSO facilityComponent)
        {
            float compatibility = 1f;
            
            // Check room size compatibility
            float roomArea = facilityComponent.FloorArea;
            if (roomArea < _roomSizeRange.x || roomArea > _roomSizeRange.y)
            {
                compatibility *= 0.7f;
            }
            
            // Check power load compatibility
            float totalPower = CalculateTotalPowerConsumption();
            if (totalPower > facilityComponent.MaxPowerLoad)
            {
                compatibility *= 0.5f;
            }
            
            // Check plant capacity compatibility
            if (_targetPlantCount > facilityComponent.MaxPlantCapacity)
            {
                compatibility *= 0.6f;
            }
            
            return compatibility;
        }
        
        /// <summary>
        /// Calculates expected ROI based on yield and market prices.
        /// </summary>
        public float CalculateExpectedROI(float marketPricePerGram, float roomSizeSqM)
        {
            float annualYield = _expectedYieldPerSqM * roomSizeSqM * _roiAnalysis.HarvestsPerYear;
            float annualRevenue = annualYield * 1000f * marketPricePerGram; // Convert kg to grams
            float annualCosts = _monthlyOperatingCost * 12f;
            float netAnnualProfit = annualRevenue - annualCosts;
            
            return (netAnnualProfit / _initialInvestment) * 100f; // ROI percentage
        }
        
        /// <summary>
        /// Gets maintenance schedule for this preset.
        /// </summary>
        public List<PreventiveMaintenanceTask> GetMaintenanceSchedule()
        {
            var schedule = new List<PreventiveMaintenanceTask>(_maintenanceTasks);
            
            // Add equipment-specific maintenance tasks
            foreach (var item in _equipmentItems)
            {
                if (item.Equipment != null)
                {
                    foreach (var task in item.Equipment.MaintenanceTasks)
                    {
                        var presetTask = new PreventiveMaintenanceTask
                        {
                            TaskName = $"{item.Equipment.EquipmentName} - {task.TaskName}",
                            Frequency = task.Frequency,
                            EstimatedDuration = task.TaskDuration,
                            EstimatedCost = task.TaskCost,
                            TaskDescription = task.TaskDescription
                        };
                        schedule.Add(presetTask);
                    }
                }
            }
            
            return schedule;
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_presetName))
                Debug.LogError($"Equipment Preset {name}: Preset name cannot be empty", this);
                
            if (_equipmentItems.Count == 0)
                Debug.LogWarning($"Equipment Preset {name}: No equipment items defined", this);
                
            if (_initialInvestment <= 0f)
                Debug.LogError($"Equipment Preset {name}: Initial investment must be positive", this);
                
            if (_targetPlantCount <= 0)
                Debug.LogError($"Equipment Preset {name}: Target plant count must be positive", this);
                
            return isValid;
        }
    }
    
    [System.Serializable]
    public class EquipmentItem
    {
        public EquipmentDataSO Equipment;
        public int Quantity = 1;
        public bool IsRequired = true;
        public Vector3 PreferredPosition = Vector3.zero;
        [TextArea(2, 3)] public string PlacementNotes;
    }
    
    [System.Serializable]
    public class PerformanceRating
    {
        [Range(1, 5)] public int YieldPotential = 3;
        [Range(1, 5)] public int QualityPotential = 3;
        [Range(1, 5)] public int Efficiency = 3;
        [Range(1, 5)] public int Reliability = 3;
        [Range(1, 5)] public int EaseOfUse = 3;
    }
    
    [System.Serializable]
    public class EfficiencyMetrics
    {
        [Range(0f, 10f)] public float PowerEfficiency = 5f; // yield per kWh
        [Range(0f, 100f)] public float WaterEfficiency = 90f; // percentage water use efficiency
        [Range(0f, 100f)] public float SpaceUtilization = 85f; // percentage floor space used
        [Range(0f, 5f)] public float LaborEfficiency = 3f; // yield per labor hour
    }
    
    [System.Serializable]
    public class ROI_Analysis
    {
        [Range(1f, 6f)] public float HarvestsPerYear = 4f;
        [Range(0f, 1f)] public float QualityGradeMultiplier = 0.8f;
        [Range(0.5f, 2f)] public float MarketPremium = 1f;
        [Range(5f, 50f)] public float BreakevenPricePerGram = 15f;
    }
    
    [System.Serializable]
    public class AutomationFeature
    {
        public string FeatureName;
        public AutomationLevel RequiredLevel;
        public bool IsIncluded = false;
        public float ImplementationCost = 100f;
        [TextArea(2, 3)] public string FeatureDescription;
    }
    
    [System.Serializable]
    public class EnvironmentalCapabilities
    {
        public Vector2 TemperatureControlRange = new Vector2(18f, 30f);
        public Vector2 HumidityControlRange = new Vector2(30f, 80f);
        public Vector2 LightIntensityRange = new Vector2(200f, 1000f);
        public Vector2 CO2ControlRange = new Vector2(400f, 1200f);
        [Range(0f, 1f)] public float ControlPrecision = 0.8f;
    }
    
    [System.Serializable]
    public class EnvironmentalScenario
    {
        public string ScenarioName;
        public PlantGrowthStage TargetStage;
        public Vector2 TemperatureRange;
        public Vector2 HumidityRange;
        public float LightIntensity;
        [TextArea(2, 3)] public string ScenarioDescription;
    }
    
    [System.Serializable]
    public class MaintenanceProfile
    {
        [Range(1, 5)] public int MaintenanceComplexity = 3;
        [Range(1, 5)] public int SkillRequirement = 3;
        [Range(0f, 100f)] public float PreventiveMaintenanceRatio = 80f;
        [Range(0.5f, 5f)] public float MaintenanceCostMultiplier = 1f;
    }
    
    [System.Serializable]
    public class PreventiveMaintenanceTask
    {
        public string TaskName;
        public MaintenanceFrequency Frequency;
        public float EstimatedDuration = 1f; // hours
        public float EstimatedCost = 25f;
        [TextArea(2, 3)] public string TaskDescription;
    }
    
    public enum PresetCategory
    {
        Complete_Room,
        Lighting_Package,
        HVAC_Package,
        Irrigation_Package,
        Monitoring_Package,
        Automation_Package,
        Starter_Kit,
        Professional_Kit,
        Commercial_Kit,
        Research_Kit
    }
    
    public enum CultivationStyle
    {
        Indoor_Soil,
        Indoor_Hydroponic,
        Indoor_Aeroponic,
        Greenhouse_Soil,
        Greenhouse_Hydroponic,
        Outdoor_Soil,
        Outdoor_Container,
        Sea_Of_Green,
        Screen_Of_Green,
        Vertical_Growing
    }
    
    public enum AutomationLevel
    {
        Manual,
        Basic_Timers,
        Semi_Automated,
        Fully_Automated,
        AI_Controlled
    }
}