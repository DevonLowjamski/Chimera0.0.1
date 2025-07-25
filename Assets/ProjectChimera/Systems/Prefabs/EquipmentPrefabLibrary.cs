using UnityEngine;
using ProjectChimera.Data.Equipment;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Systems.Prefabs
{
    /// <summary>
    /// Specialized prefab library for equipment objects including grow lights,
    /// HVAC systems, irrigation, monitoring devices, and processing equipment.
    /// </summary>
    [CreateAssetMenu(fileName = "New Equipment Prefab Library", menuName = "Project Chimera/Prefabs/Equipment Library")]
    public class EquipmentPrefabLibrary : ScriptableObject
    {
        [Header("Equipment Categories")]
        [SerializeField] private List<EquipmentPrefabEntry> _equipmentPrefabs = new List<EquipmentPrefabEntry>();
        [SerializeField] private List<EquipmentCategoryGroup> _categoryGroups = new List<EquipmentCategoryGroup>();
        
        [Header("Equipment Specifications")]
        [SerializeField] private List<EquipmentSpecificationSet> _specificationSets = new List<EquipmentSpecificationSet>();
        [SerializeField] private bool _enablePerformanceVariations = true;
        [SerializeField] private bool _enableUpgradeSystem = true;
        
        [Header("Installation Settings")]
        [SerializeField] private bool _requiresInstallation = true;
        [SerializeField] private bool _validatePowerRequirements = true;
        [SerializeField] private bool _checkSpaceRequirements = true;
        
        // Cached lookup tables
        private Dictionary<string, EquipmentPrefabEntry> _prefabLookup;
        private Dictionary<EquipmentType, List<EquipmentPrefabEntry>> _typeLookup;
        private Dictionary<string, EquipmentCategoryGroup> _categoryLookup;
        
        public List<EquipmentPrefabEntry> EquipmentPrefabs => _equipmentPrefabs;
        
        public void InitializeDefaults()
        {
            if (_equipmentPrefabs.Count == 0)
            {
                CreateDefaultEquipmentPrefabs();
            }
            
            BuildLookupTables();
        }
        
        private void CreateDefaultEquipmentPrefabs()
        {
            CreateLightingEquipment();
            CreateHVACEquipment();
            CreateIrrigationEquipment();
            CreateMonitoringEquipment();
            CreateProcessingEquipment();
        }
        
        private void CreateLightingEquipment()
        {
            // LED Grow Light
            _equipmentPrefabs.Add(new EquipmentPrefabEntry
            {
                PrefabId = "led_grow_light_600w",
                PrefabName = "600W LED Grow Light",
                Prefab = null,
                EquipmentType = EquipmentType.GrowLight,
                Manufacturer = "ChimeraGrow",
                Model = "CG-LED-600",
                PowerRequirement = 600f,
                Dimensions = new Vector3(1.2f, 0.15f, 0.6f),
                Weight = 12f,
                Cost = 800f,
                EfficiencyRating = 2.5f,
                RequiredComponents = new List<string> { "AdvancedGrowLightSystem", "PowerController", "HeatSink" },
                InstallationRequirements = new EquipmentInstallationRequirements
                {
                    RequiresCeiling = true,
                    MinClearance = 0.5f,
                    RequiresPowerOutlet = true,
                    RequiresVentilation = true
                }
            });
            
            // HPS Light
            _equipmentPrefabs.Add(new EquipmentPrefabEntry
            {
                PrefabId = "hps_light_1000w",
                PrefabName = "1000W HPS Light",
                Prefab = null,
                EquipmentType = EquipmentType.GrowLight,
                Manufacturer = "ClassicGrow",
                Model = "HPS-1000",
                PowerRequirement = 1100f,
                Dimensions = new Vector3(0.8f, 0.4f, 0.8f),
                Weight = 25f,
                Cost = 400f,
                EfficiencyRating = 1.3f,
                RequiredComponents = new List<string> { "HPSLightSystem", "Ballast", "Reflector", "CoolingTube" },
                InstallationRequirements = new EquipmentInstallationRequirements
                {
                    RequiresCeiling = true,
                    MinClearance = 1f,
                    RequiresPowerOutlet = true,
                    RequiresVentilation = true,
                    RequiresExhaust = true
                }
            });
        }
        
        private void CreateHVACEquipment()
        {
            // Air Conditioning Unit
            _equipmentPrefabs.Add(new EquipmentPrefabEntry
            {
                PrefabId = "hvac_split_system_5ton",
                PrefabName = "5-Ton Split HVAC System",
                Prefab = null,
                EquipmentType = EquipmentType.HVAC,
                Manufacturer = "ClimateControl Pro",
                Model = "CCP-5T-Split",
                PowerRequirement = 5000f,
                Dimensions = new Vector3(1.5f, 0.8f, 0.6f),
                Weight = 150f,
                Cost = 3500f,
                EfficiencyRating = 16f, // SEER rating
                RequiredComponents = new List<string> { "HVACController", "TemperatureSensor", "HumiditySensor", "CompressorUnit" },
                InstallationRequirements = new EquipmentInstallationRequirements
                {
                    RequiresWall = true,
                    MinClearance = 1f,
                    RequiresPowerOutlet = true,
                    RequiresDrainage = true,
                    RequiresExteriorAccess = true
                }
            });
            
            // Dehumidifier
            _equipmentPrefabs.Add(new EquipmentPrefabEntry
            {
                PrefabId = "dehumidifier_commercial_150pint",
                PrefabName = "150 Pint Commercial Dehumidifier",
                Prefab = null,
                EquipmentType = EquipmentType.Dehumidifier,
                Manufacturer = "DryAir Systems",
                Model = "DAS-150C",
                PowerRequirement = 1200f,
                Dimensions = new Vector3(0.8f, 1.2f, 0.5f),
                Weight = 80f,
                Cost = 2200f,
                EfficiencyRating = 4.5f, // Liters per kWh
                RequiredComponents = new List<string> { "DehumidifierController", "HumiditySensor", "WaterCollection" },
                InstallationRequirements = new EquipmentInstallationRequirements
                {
                    RequiresFloor = true,
                    MinClearance = 0.3f,
                    RequiresPowerOutlet = true,
                    RequiresDrainage = true
                }
            });
        }
        
        private void CreateIrrigationEquipment()
        {
            // Automated Irrigation System
            _equipmentPrefabs.Add(new EquipmentPrefabEntry
            {
                PrefabId = "irrigation_hydroponic_16_site",
                PrefabName = "16-Site Hydroponic System",
                Prefab = null,
                EquipmentType = EquipmentType.Irrigation,
                Manufacturer = "HydroFlow",
                Model = "HF-16-DWC",
                PowerRequirement = 200f,
                Dimensions = new Vector3(2.4f, 0.3f, 1.2f),
                Weight = 45f,
                Cost = 1200f,
                EfficiencyRating = 0.95f, // Water use efficiency
                RequiredComponents = new List<string> { "IrrigationController", "WaterPump", "NutrientReservoir", "pHController" },
                InstallationRequirements = new EquipmentInstallationRequirements
                {
                    RequiresFloor = true,
                    MinClearance = 0.5f,
                    RequiresPowerOutlet = true,
                    RequiresWaterSupply = true,
                    RequiresDrainage = true
                }
            });
            
            // Drip Irrigation
            _equipmentPrefabs.Add(new EquipmentPrefabEntry
            {
                PrefabId = "drip_irrigation_soil_32_emitter",
                PrefabName = "32-Emitter Drip System",
                Prefab = null,
                EquipmentType = EquipmentType.Irrigation,
                Manufacturer = "PrecisionGrow",
                Model = "PG-DRIP-32",
                PowerRequirement = 50f,
                Dimensions = new Vector3(3f, 0.1f, 2f),
                Weight = 15f,
                Cost = 400f,
                EfficiencyRating = 0.9f,
                RequiredComponents = new List<string> { "DripController", "PressureRegulator", "FilterSystem", "TimerController" },
                InstallationRequirements = new EquipmentInstallationRequirements
                {
                    RequiresFloor = false,
                    MinClearance = 0f,
                    RequiresPowerOutlet = true,
                    RequiresWaterSupply = true
                }
            });
        }
        
        private void CreateMonitoringEquipment()
        {
            // Environmental Sensor Suite
            _equipmentPrefabs.Add(new EquipmentPrefabEntry
            {
                PrefabId = "sensor_suite_environmental_pro",
                PrefabName = "Pro Environmental Sensor Suite",
                Prefab = null,
                EquipmentType = EquipmentType.Sensor,
                Manufacturer = "SensorTech",
                Model = "ST-ENV-PRO",
                PowerRequirement = 25f,
                Dimensions = new Vector3(0.2f, 0.15f, 0.1f),
                Weight = 1f,
                Cost = 350f,
                EfficiencyRating = 1f,
                RequiredComponents = new List<string> { "EnvironmentalSensor", "WirelessTransmitter", "DataLogger" },
                InstallationRequirements = new EquipmentInstallationRequirements
                {
                    RequiresWall = true,
                    MinClearance = 0.1f,
                    RequiresPowerOutlet = false, // Battery powered
                    RequiresNetworkConnection = true
                }
            });
            
            // Security Camera
            _equipmentPrefabs.Add(new EquipmentPrefabEntry
            {
                PrefabId = "security_camera_4k_ptz",
                PrefabName = "4K PTZ Security Camera",
                Prefab = null,
                EquipmentType = EquipmentType.Security,
                Manufacturer = "SecureView",
                Model = "SV-4K-PTZ",
                PowerRequirement = 60f,
                Dimensions = new Vector3(0.3f, 0.3f, 0.4f),
                Weight = 3f,
                Cost = 800f,
                EfficiencyRating = 1f,
                RequiredComponents = new List<string> { "SecurityCamera", "MotionDetector", "NightVision", "NetworkStream" },
                InstallationRequirements = new EquipmentInstallationRequirements
                {
                    RequiresCeiling = true,
                    MinClearance = 0.2f,
                    RequiresPowerOutlet = true,
                    RequiresNetworkConnection = true
                }
            });
        }
        
        private void CreateProcessingEquipment()
        {
            // Drying Rack System
            _equipmentPrefabs.Add(new EquipmentPrefabEntry
            {
                PrefabId = "drying_rack_commercial_8_tier",
                PrefabName = "8-Tier Commercial Drying Rack",
                Prefab = null,
                EquipmentType = EquipmentType.Drying_Rack,
                Manufacturer = "CureRight",
                Model = "CR-DRY-8T",
                PowerRequirement = 0f, // Passive system
                Dimensions = new Vector3(1.5f, 2.5f, 0.8f),
                Weight = 35f,
                Cost = 600f,
                EfficiencyRating = 1f,
                RequiredComponents = new List<string> { "DryingRack", "AirflowSystem", "HumidityMonitor" },
                InstallationRequirements = new EquipmentInstallationRequirements
                {
                    RequiresFloor = true,
                    MinClearance = 0.5f,
                    RequiresVentilation = true
                }
            });
            
            // Trimming Station
            _equipmentPrefabs.Add(new EquipmentPrefabEntry
            {
                PrefabId = "trimming_station_ergonomic",
                PrefabName = "Ergonomic Trimming Station",
                Prefab = null,
                EquipmentType = EquipmentType.Trimming_Machine,
                Manufacturer = "TrimMaster",
                Model = "TM-ERGO-ST",
                PowerRequirement = 100f,
                Dimensions = new Vector3(1.2f, 1f, 0.8f),
                Weight = 50f,
                Cost = 1500f,
                EfficiencyRating = 1f,
                RequiredComponents = new List<string> { "TrimmingStation", "VacuumSystem", "LED_WorkLight", "ErgoChair" },
                InstallationRequirements = new EquipmentInstallationRequirements
                {
                    RequiresFloor = true,
                    MinClearance = 1f,
                    RequiresPowerOutlet = true,
                    RequiresVentilation = true
                }
            });
        }
        
        private void BuildLookupTables()
        {
            _prefabLookup = _equipmentPrefabs.ToDictionary(e => e.PrefabId, e => e);
            
            _typeLookup = _equipmentPrefabs.GroupBy(e => e.EquipmentType)
                                         .ToDictionary(g => g.Key, g => g.ToList());
            
            _categoryLookup = _categoryGroups.ToDictionary(c => c.CategoryId, c => c);
        }
        
        public EquipmentPrefabEntry GetEquipmentPrefab(EquipmentType equipmentType, string equipmentId)
        {
            if (!string.IsNullOrEmpty(equipmentId) && _prefabLookup.TryGetValue(equipmentId, out var specificPrefab))
            {
                return specificPrefab;
            }
            
            if (_typeLookup.TryGetValue(equipmentType, out var typePrefabs) && typePrefabs.Count > 0)
            {
                return typePrefabs[0]; // Return first prefab of this type
            }
            
            return null;
        }
        
        public List<EquipmentPrefabEntry> GetEquipmentByType(EquipmentType equipmentType)
        {
            return _typeLookup.TryGetValue(equipmentType, out var prefabs) ? prefabs : new List<EquipmentPrefabEntry>();
        }
        
        public List<EquipmentPrefabEntry> GetEquipmentByManufacturer(string manufacturer)
        {
            return _equipmentPrefabs.Where(e => e.Manufacturer == manufacturer).ToList();
        }
        
        public List<EquipmentPrefabEntry> GetEquipmentByPowerRange(float minPower, float maxPower)
        {
            return _equipmentPrefabs.Where(e => e.PowerRequirement >= minPower && e.PowerRequirement <= maxPower).ToList();
        }
        
        public List<EquipmentPrefabEntry> GetEquipmentByCostRange(float minCost, float maxCost)
        {
            return _equipmentPrefabs.Where(e => e.Cost >= minCost && e.Cost <= maxCost).ToList();
        }
        
        public bool ValidateInstallationRequirements(string prefabId, Vector3 position, Transform parent)
        {
            if (!_requiresInstallation)
                return true;
            
            var prefab = _prefabLookup.TryGetValue(prefabId, out var entry) ? entry : null;
            if (prefab?.InstallationRequirements == null)
                return true;
            
            var requirements = prefab.InstallationRequirements;
            
            // Check clearance requirements
            if (_checkSpaceRequirements)
            {
                var colliders = Physics.OverlapSphere(position, requirements.MinClearance);
                if (colliders.Length > 0)
                {
                    Debug.LogWarning($"Insufficient clearance for {prefab.PrefabName} at position {position}");
                    return false;
                }
            }
            
            // Check power requirements
            if (_validatePowerRequirements && requirements.RequiresPowerOutlet)
            {
                // Would implement power grid validation here
                Debug.Log($"Checking power requirements for {prefab.PrefabName}: {prefab.PowerRequirement}W");
            }
            
            return true;
        }
        
        public EquipmentUpgradeInfo GetUpgradeOptions(string currentPrefabId)
        {
            if (!_enableUpgradeSystem)
                return null;
            
            var currentPrefab = _prefabLookup.TryGetValue(currentPrefabId, out var prefab) ? prefab : null;
            if (currentPrefab == null)
                return null;
            
            var upgrades = _equipmentPrefabs
                .Where(e => e.EquipmentType == currentPrefab.EquipmentType && 
                           e.Cost > currentPrefab.Cost &&
                           e.EfficiencyRating > currentPrefab.EfficiencyRating)
                .OrderBy(e => e.Cost)
                .Take(3)
                .ToList();
            
            return new EquipmentUpgradeInfo
            {
                CurrentEquipment = currentPrefab,
                AvailableUpgrades = upgrades,
                UpgradeBenefits = CalculateUpgradeBenefits(currentPrefab, upgrades)
            };
        }
        
        private List<UpgradeBenefit> CalculateUpgradeBenefits(EquipmentPrefabEntry current, List<EquipmentPrefabEntry> upgrades)
        {
            return upgrades.Select(upgrade => new UpgradeBenefit
            {
                UpgradePrefab = upgrade,
                EfficiencyImprovement = upgrade.EfficiencyRating - current.EfficiencyRating,
                CostDifference = upgrade.Cost - current.Cost,
                PowerDifference = upgrade.PowerRequirement - current.PowerRequirement,
                ROIEstimate = CalculateROI(current, upgrade)
            }).ToList();
        }
        
        private float CalculateROI(EquipmentPrefabEntry current, EquipmentPrefabEntry upgrade)
        {
            // Simplified ROI calculation based on efficiency improvements
            float efficiencyGain = upgrade.EfficiencyRating - current.EfficiencyRating;
            float costDifference = upgrade.Cost - current.Cost;
            
            if (costDifference <= 0f || efficiencyGain <= 0f)
                return 0f;
            
            // Assume 12 months of operation
            float annualSavings = efficiencyGain * 365f * 0.12f; // $0.12 per kWh
            return (annualSavings * 12f) / costDifference; // 12-month ROI
        }
        
        public void AddCustomEquipment(EquipmentPrefabEntry equipmentEntry)
        {
            if (_equipmentPrefabs.Any(e => e.PrefabId == equipmentEntry.PrefabId))
            {
                Debug.LogWarning($"Equipment with ID {equipmentEntry.PrefabId} already exists");
                return;
            }
            
            _equipmentPrefabs.Add(equipmentEntry);
            BuildLookupTables();
        }
        
        public EquipmentLibraryStats GetLibraryStats()
        {
            return new EquipmentLibraryStats
            {
                TotalEquipment = _equipmentPrefabs.Count,
                TypeDistribution = _typeLookup.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count),
                TotalValue = _equipmentPrefabs.Sum(e => e.Cost),
                AverageEfficiency = _equipmentPrefabs.Average(e => e.EfficiencyRating),
                PowerConsumptionRange = new Vector2(
                    _equipmentPrefabs.Min(e => e.PowerRequirement),
                    _equipmentPrefabs.Max(e => e.PowerRequirement)
                )
            };
        }
        
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                BuildLookupTables();
            }
        }
    }
    
    // Supporting data structures
    [System.Serializable]
    public class EquipmentPrefabEntry
    {
        public string PrefabId;
        public string PrefabName;
        public GameObject Prefab;
        public EquipmentType EquipmentType;
        public string Manufacturer;
        public string Model;
        public float PowerRequirement; // Watts
        public Vector3 Dimensions; // meters
        public float Weight; // kg
        public float Cost; // currency
        public float EfficiencyRating;
        public List<string> RequiredComponents = new List<string>();
        public EquipmentInstallationRequirements InstallationRequirements;
        public EquipmentMetadata Metadata;
    }
    
    [System.Serializable]
    public class EquipmentCategoryGroup
    {
        public string CategoryId;
        public string CategoryName;
        public EquipmentType PrimaryType;
        public List<string> EquipmentIds = new List<string>();
        public string Description;
    }
    
    [System.Serializable]
    public class EquipmentSpecificationSet
    {
        public string SpecId;
        public EquipmentType EquipmentType;
        public Dictionary<string, float> PerformanceMetrics;
        public List<string> RequiredFeatures;
        public List<string> OptionalFeatures;
    }
    
    [System.Serializable]
    public class EquipmentInstallationRequirements
    {
        public bool RequiresCeiling = false;
        public bool RequiresWall = false;
        public bool RequiresFloor = true;
        public float MinClearance = 0.5f;
        public bool RequiresPowerOutlet = true;
        public bool RequiresWaterSupply = false;
        public bool RequiresDrainage = false;
        public bool RequiresVentilation = false;
        public bool RequiresExhaust = false;
        public bool RequiresExteriorAccess = false;
        public bool RequiresNetworkConnection = false;
    }
    
    [System.Serializable]
    public class EquipmentMetadata
    {
        public float LifespanYears = 10f;
        public float MaintenanceIntervalDays = 30f;
        public List<string> MaintenanceTasks = new List<string>();
        public string WarrantyPeriod = "1 Year";
        public bool RequiresProfessionalInstallation = false;
    }
    
    [System.Serializable]
    public class EquipmentUpgradeInfo
    {
        public EquipmentPrefabEntry CurrentEquipment;
        public List<EquipmentPrefabEntry> AvailableUpgrades;
        public List<UpgradeBenefit> UpgradeBenefits;
    }
    
    [System.Serializable]
    public class UpgradeBenefit
    {
        public EquipmentPrefabEntry UpgradePrefab;
        public float EfficiencyImprovement;
        public float CostDifference;
        public float PowerDifference;
        public float ROIEstimate;
    }
    
    [System.Serializable]
    public class EquipmentLibraryStats
    {
        public int TotalEquipment;
        public Dictionary<EquipmentType, int> TypeDistribution;
        public float TotalValue;
        public float AverageEfficiency;
        public Vector2 PowerConsumptionRange;
    }
}