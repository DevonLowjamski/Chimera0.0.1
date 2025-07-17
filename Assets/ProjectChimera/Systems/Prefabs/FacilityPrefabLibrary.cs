using UnityEngine;
using ProjectChimera.Data.Facilities;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Systems.Prefabs
{
    /// <summary>
    /// Specialized prefab library for facility components including grow rooms,
    /// processing areas, storage facilities, and complete facility templates.
    /// </summary>
    [CreateAssetMenu(fileName = "New Facility Prefab Library", menuName = "Project Chimera/Prefabs/Facility Library")]
    public class FacilityPrefabLibrary : ScriptableObject
    {
        [Header("Facility Types")]
        [SerializeField] private List<FacilityPrefabEntry> _facilityPrefabs = new List<FacilityPrefabEntry>();
        [SerializeField] private List<CompleteFacilityTemplate> _facilityTemplates = new List<CompleteFacilityTemplate>();
        
        [Header("Room Categories")]
        [SerializeField] private List<RoomPrefabCollection> _roomCollections = new List<RoomPrefabCollection>();
        [SerializeField] private List<FacilityModuleSet> _moduleSets = new List<FacilityModuleSet>();
        
        [Header("Facility Configuration")]
        [SerializeField] private bool _enableModularConstruction = true;
        [SerializeField] private bool _enforceBuilding_codes = true;
        [SerializeField] private bool _requireUtilityConnections = true;
        [SerializeField] private Vector2 _facilitySizeRange = new Vector2(50f, 10000f); // square meters
        
        // Cached lookup tables
        private Dictionary<string, FacilityPrefabEntry> _prefabLookup;
        private Dictionary<FacilityType, List<FacilityPrefabEntry>> _typeLookup;
        private Dictionary<string, CompleteFacilityTemplate> _templateLookup;
        
        public List<FacilityPrefabEntry> FacilityPrefabs => _facilityPrefabs;
        
        public void InitializeDefaults()
        {
            if (_facilityPrefabs.Count == 0)
            {
                CreateDefaultFacilityPrefabs();
            }
            
            if (_facilityTemplates.Count == 0)
            {
                CreateDefaultFacilityTemplates();
            }
            
            BuildLookupTables();
        }
        
        private void CreateDefaultFacilityPrefabs()
        {
            CreateGrowRoomPrefabs();
            CreateProcessingRoomPrefabs();
            CreateStorageRoomPrefabs();
            CreateUtilityRoomPrefabs();
            CreateOfficeSpacePrefabs();
        }
        
        private void CreateGrowRoomPrefabs()
        {
            // Small Grow Room
            _facilityPrefabs.Add(new FacilityPrefabEntry
            {
                PrefabId = "grow_room_small_4x4",
                PrefabName = "Small Grow Room (4x4m)",
                Prefab = null,
                FacilityType = FacilityType.GrowRoom,
                RoomSize = new Vector3(4f, 3f, 4f),
                MaxCapacity = 16, // plants
                PowerRequirement = 3000f, // watts
                CoolingRequirement = 12000f, // BTU
                VentilationRequirement = 200f, // CFM
                Cost = 15000f,
                RequiredFeatures = new List<FacilityFeature>
                {
                    FacilityFeature.EnvironmentalControl,
                    FacilityFeature.Lighting,
                    FacilityFeature.Irrigation,
                    FacilityFeature.Ventilation
                },
                RequiredComponents = new List<string> { "AdvancedGrowRoomController", "EnvironmentalSensors", "SecuritySystem" },
                ConstructionRequirements = new FacilityConstructionRequirements
                {
                    RequiresFoundation = true,
                    RequiresInsulation = true,
                    RequiresVaporBarrier = true,
                    RequiresFireSafety = true,
                    MinCeilingHeight = 3f,
                    RequiredWallType = WallType.Insulated,
                    RequiredFloorType = FloorType.Sealed_Concrete
                }
            });
            
            // Medium Grow Room
            _facilityPrefabs.Add(new FacilityPrefabEntry
            {
                PrefabId = "grow_room_medium_8x8",
                PrefabName = "Medium Grow Room (8x8m)",
                Prefab = null,
                FacilityType = FacilityType.GrowRoom,
                RoomSize = new Vector3(8f, 3.5f, 8f),
                MaxCapacity = 64,
                PowerRequirement = 12000f,
                CoolingRequirement = 48000f,
                VentilationRequirement = 800f,
                Cost = 45000f,
                RequiredFeatures = new List<FacilityFeature>
                {
                    FacilityFeature.EnvironmentalControl,
                    FacilityFeature.Lighting,
                    FacilityFeature.Irrigation,
                    FacilityFeature.Ventilation,
                    FacilityFeature.CO2_Enrichment,
                    FacilityFeature.SecuritySystem
                },
                RequiredComponents = new List<string> { "AdvancedGrowRoomController", "MultiZoneHVAC", "AdvancedSecurity" },
                ConstructionRequirements = new FacilityConstructionRequirements
                {
                    RequiresFoundation = true,
                    RequiresInsulation = true,
                    RequiresVaporBarrier = true,
                    RequiresFireSafety = true,
                    MinCeilingHeight = 3.5f,
                    RequiredWallType = WallType.Insulated,
                    RequiredFloorType = FloorType.Sealed_Concrete,
                    RequiresEmergencyExits = true
                }
            });
            
            // Large Commercial Grow Room
            _facilityPrefabs.Add(new FacilityPrefabEntry
            {
                PrefabId = "grow_room_large_commercial_20x40",
                PrefabName = "Large Commercial Grow Room (20x40m)",
                Prefab = null,
                FacilityType = FacilityType.GrowRoom,
                RoomSize = new Vector3(20f, 4f, 40f),
                MaxCapacity = 3200,
                PowerRequirement = 150000f,
                CoolingRequirement = 600000f,
                VentilationRequirement = 10000f,
                Cost = 500000f,
                RequiredFeatures = new List<FacilityFeature>
                {
                    FacilityFeature.EnvironmentalControl,
                    FacilityFeature.Lighting,
                    FacilityFeature.Irrigation,
                    FacilityFeature.Ventilation,
                    FacilityFeature.CO2_Enrichment,
                    FacilityFeature.SecuritySystem,
                    FacilityFeature.Automation,
                    FacilityFeature.DataMonitoring
                },
                RequiredComponents = new List<string> { "CommercialGrowController", "IndustrialHVAC", "AdvancedAutomation" },
                ConstructionRequirements = new FacilityConstructionRequirements
                {
                    RequiresFoundation = true,
                    RequiresInsulation = true,
                    RequiresVaporBarrier = true,
                    RequiresFireSafety = true,
                    MinCeilingHeight = 4f,
                    RequiredWallType = WallType.Commercial_Grade,
                    RequiredFloorType = FloorType.Industrial_Epoxy,
                    RequiresEmergencyExits = true,
                    RequiresSprinklerSystem = true,
                    RequiresBackupPower = true
                }
            });
        }
        
        private void CreateProcessingRoomPrefabs()
        {
            // Drying Room
            _facilityPrefabs.Add(new FacilityPrefabEntry
            {
                PrefabId = "drying_room_standard_6x4",
                PrefabName = "Standard Drying Room (6x4m)",
                Prefab = null,
                FacilityType = FacilityType.ProcessingRoom,
                RoomSize = new Vector3(6f, 3f, 4f),
                MaxCapacity = 100, // kg capacity
                PowerRequirement = 2000f,
                CoolingRequirement = 8000f,
                VentilationRequirement = 300f,
                Cost = 25000f,
                RequiredFeatures = new List<FacilityFeature>
                {
                    FacilityFeature.EnvironmentalControl,
                    FacilityFeature.Ventilation,
                    FacilityFeature.SecuritySystem
                },
                RequiredComponents = new List<string> { "DryingController", "HumidityControl", "AirCirculation" },
                ConstructionRequirements = new FacilityConstructionRequirements
                {
                    RequiresFoundation = true,
                    RequiresInsulation = true,
                    RequiresVaporBarrier = false,
                    RequiresFireSafety = true,
                    MinCeilingHeight = 3f,
                    RequiredWallType = WallType.Standard,
                    RequiredFloorType = FloorType.Sealed_Concrete
                }
            });
            
            // Trimming Room
            _facilityPrefabs.Add(new FacilityPrefabEntry
            {
                PrefabId = "trimming_room_8x6",
                PrefabName = "Trimming Room (8x6m)",
                Prefab = null,
                FacilityType = FacilityType.ProcessingRoom,
                RoomSize = new Vector3(8f, 3f, 6f),
                MaxCapacity = 8, // workstations
                PowerRequirement = 1500f,
                CoolingRequirement = 12000f,
                VentilationRequirement = 400f,
                Cost = 30000f,
                RequiredFeatures = new List<FacilityFeature>
                {
                    FacilityFeature.EnvironmentalControl,
                    FacilityFeature.Ventilation,
                    FacilityFeature.SecuritySystem,
                    FacilityFeature.QualityControl
                },
                RequiredComponents = new List<string> { "TrimmingStations", "VacuumSystem", "WorkstationLighting" },
                ConstructionRequirements = new FacilityConstructionRequirements
                {
                    RequiresFoundation = true,
                    RequiresInsulation = false,
                    RequiresVaporBarrier = false,
                    RequiresFireSafety = true,
                    MinCeilingHeight = 3f,
                    RequiredWallType = WallType.Standard,
                    RequiredFloorType = FloorType.Easy_Clean
                }
            });
        }
        
        private void CreateStorageRoomPrefabs()
        {
            // Climate Controlled Storage
            _facilityPrefabs.Add(new FacilityPrefabEntry
            {
                PrefabId = "storage_climate_controlled_10x8",
                PrefabName = "Climate Controlled Storage (10x8m)",
                Prefab = null,
                FacilityType = FacilityType.StorageRoom,
                RoomSize = new Vector3(10f, 3f, 8f),
                MaxCapacity = 500, // kg storage
                PowerRequirement = 3000f,
                CoolingRequirement = 18000f,
                VentilationRequirement = 200f,
                Cost = 40000f,
                RequiredFeatures = new List<FacilityFeature>
                {
                    FacilityFeature.EnvironmentalControl,
                    FacilityFeature.SecuritySystem,
                    FacilityFeature.InventoryTracking
                },
                RequiredComponents = new List<string> { "ClimateController", "SecurityVault", "InventorySystem" },
                ConstructionRequirements = new FacilityConstructionRequirements
                {
                    RequiresFoundation = true,
                    RequiresInsulation = true,
                    RequiresVaporBarrier = true,
                    RequiresFireSafety = true,
                    MinCeilingHeight = 3f,
                    RequiredWallType = WallType.Security_Grade,
                    RequiredFloorType = FloorType.Sealed_Concrete
                }
            });
        }
        
        private void CreateUtilityRoomPrefabs()
        {
            // Electrical Room
            _facilityPrefabs.Add(new FacilityPrefabEntry
            {
                PrefabId = "electrical_room_4x3",
                PrefabName = "Electrical Room (4x3m)",
                Prefab = null,
                FacilityType = FacilityType.UtilityRoom,
                RoomSize = new Vector3(4f, 3f, 3f),
                MaxCapacity = 0,
                PowerRequirement = 0f,
                CoolingRequirement = 6000f,
                VentilationRequirement = 100f,
                Cost = 20000f,
                RequiredFeatures = new List<FacilityFeature>
                {
                    FacilityFeature.PowerDistribution,
                    FacilityFeature.SecuritySystem,
                    FacilityFeature.FireSafety
                },
                RequiredComponents = new List<string> { "MainElectricalPanel", "BackupGenerator", "UPS_System" },
                ConstructionRequirements = new FacilityConstructionRequirements
                {
                    RequiresFoundation = true,
                    RequiresInsulation = false,
                    RequiresVaporBarrier = false,
                    RequiresFireSafety = true,
                    MinCeilingHeight = 3f,
                    RequiredWallType = WallType.Fire_Rated,
                    RequiredFloorType = FloorType.Anti_Static
                }
            });
        }
        
        private void CreateOfficeSpacePrefabs()
        {
            // Control Room
            _facilityPrefabs.Add(new FacilityPrefabEntry
            {
                PrefabId = "control_room_6x4",
                PrefabName = "Control Room (6x4m)",
                Prefab = null,
                FacilityType = FacilityType.Office,
                RoomSize = new Vector3(6f, 3f, 4f),
                MaxCapacity = 4, // workstations
                PowerRequirement = 2000f,
                CoolingRequirement = 8000f,
                VentilationRequirement = 150f,
                Cost = 35000f,
                RequiredFeatures = new List<FacilityFeature>
                {
                    FacilityFeature.DataMonitoring,
                    FacilityFeature.SecuritySystem,
                    FacilityFeature.NetworkInfrastructure
                },
                RequiredComponents = new List<string> { "MonitoringWorkstations", "ServerRack", "SecurityMonitors" },
                ConstructionRequirements = new FacilityConstructionRequirements
                {
                    RequiresFoundation = true,
                    RequiresInsulation = false,
                    RequiresVaporBarrier = false,
                    RequiresFireSafety = true,
                    MinCeilingHeight = 3f,
                    RequiredWallType = WallType.Standard,
                    RequiredFloorType = FloorType.Raised_Access
                }
            });
        }
        
        private void CreateDefaultFacilityTemplates()
        {
            // Small Personal Facility
            _facilityTemplates.Add(new CompleteFacilityTemplate
            {
                TemplateId = "personal_facility_small",
                TemplateName = "Small Personal Facility",
                FacilityScale = FacilityScale.Personal,
                TotalFootprint = new Vector2(12f, 8f),
                RoomLayout = new List<RoomLayoutEntry>
                {
                    new RoomLayoutEntry { RoomPrefabId = "grow_room_small_4x4", Position = new Vector3(0, 0, 0), Rotation = Quaternion.identity },
                    new RoomLayoutEntry { RoomPrefabId = "drying_room_standard_6x4", Position = new Vector3(8, 0, 0), Rotation = Quaternion.identity },
                    new RoomLayoutEntry { RoomPrefabId = "electrical_room_4x3", Position = new Vector3(0, 0, 6), Rotation = Quaternion.identity }
                },
                EstimatedCost = 80000f,
                ConstructionTime = 30f, // days
                AnnualOperatingCost = 15000f,
                RequiredLicenses = new List<string> { "Personal_Cultivation", "Home_Processing" }
            });
            
            // Medium Commercial Facility
            _facilityTemplates.Add(new CompleteFacilityTemplate
            {
                TemplateId = "commercial_facility_medium",
                TemplateName = "Medium Commercial Facility",
                FacilityScale = FacilityScale.Commercial,
                TotalFootprint = new Vector2(30f, 20f),
                RoomLayout = new List<RoomLayoutEntry>
                {
                    new RoomLayoutEntry { RoomPrefabId = "grow_room_medium_8x8", Position = new Vector3(0, 0, 0) },
                    new RoomLayoutEntry { RoomPrefabId = "grow_room_medium_8x8", Position = new Vector3(12, 0, 0) },
                    new RoomLayoutEntry { RoomPrefabId = "drying_room_standard_6x4", Position = new Vector3(0, 0, 12) },
                    new RoomLayoutEntry { RoomPrefabId = "trimming_room_8x6", Position = new Vector3(8, 0, 12) },
                    new RoomLayoutEntry { RoomPrefabId = "storage_climate_controlled_10x8", Position = new Vector3(16, 0, 12) },
                    new RoomLayoutEntry { RoomPrefabId = "control_room_6x4", Position = new Vector3(0, 0, 18) },
                    new RoomLayoutEntry { RoomPrefabId = "electrical_room_4x3", Position = new Vector3(8, 0, 18) }
                },
                EstimatedCost = 350000f,
                ConstructionTime = 90f,
                AnnualOperatingCost = 120000f,
                RequiredLicenses = new List<string> { "Commercial_Cultivation", "Processing_License", "Distribution_License" }
            });
        }
        
        private void BuildLookupTables()
        {
            _prefabLookup = _facilityPrefabs.ToDictionary(f => f.PrefabId, f => f);
            
            _typeLookup = _facilityPrefabs.GroupBy(f => f.FacilityType)
                                        .ToDictionary(g => g.Key, g => g.ToList());
            
            _templateLookup = _facilityTemplates.ToDictionary(t => t.TemplateId, t => t);
        }
        
        public FacilityPrefabEntry GetFacilityPrefab(FacilityType facilityType)
        {
            if (_typeLookup.TryGetValue(facilityType, out var prefabs) && prefabs.Count > 0)
            {
                return prefabs[0]; // Return first prefab of this type
            }
            
            return null;
        }
        
        public List<FacilityPrefabEntry> GetFacilitiesByType(FacilityType facilityType)
        {
            return _typeLookup.TryGetValue(facilityType, out var prefabs) ? prefabs : new List<FacilityPrefabEntry>();
        }
        
        public List<FacilityPrefabEntry> GetFacilitiesByCapacity(int minCapacity, int maxCapacity)
        {
            return _facilityPrefabs.Where(f => f.MaxCapacity >= minCapacity && f.MaxCapacity <= maxCapacity).ToList();
        }
        
        public List<FacilityPrefabEntry> GetFacilitiesByPowerRequirement(float maxPower)
        {
            return _facilityPrefabs.Where(f => f.PowerRequirement <= maxPower).ToList();
        }
        
        public CompleteFacilityTemplate GetFacilityTemplate(string templateId)
        {
            return _templateLookup.TryGetValue(templateId, out var template) ? template : null;
        }
        
        public List<CompleteFacilityTemplate> GetTemplatesByScale(FacilityScale scale)
        {
            return _facilityTemplates.Where(t => t.FacilityScale == scale).ToList();
        }
        
        public FacilityValidationResult ValidateFacilityLayout(CompleteFacilityTemplate template)
        {
            var result = new FacilityValidationResult
            {
                IsValid = true,
                ValidationErrors = new List<string>(),
                ValidationWarnings = new List<string>()
            };
            
            // Check if all required rooms are present
            var growRooms = template.RoomLayout.Count(r => GetRoomType(r.RoomPrefabId) == FacilityType.GrowRoom);
            if (growRooms == 0)
            {
                result.ValidationErrors.Add("Facility must have at least one grow room");
                result.IsValid = false;
            }
            
            // Check power requirements
            float totalPower = CalculateTotalPowerRequirement(template);
            if (totalPower > 200000f) // 200kW limit for example
            {
                result.ValidationWarnings.Add($"High power requirement: {totalPower:F0}W. Consider load management.");
            }
            
            // Check room spacing and clearances
            ValidateRoomSpacing(template, result);
            
            // Check building code compliance
            if (_enforceBuilding_codes)
            {
                ValidateBuildingCodes(template, result);
            }
            
            return result;
        }
        
        private FacilityType GetRoomType(string roomPrefabId)
        {
            return _prefabLookup.TryGetValue(roomPrefabId, out var prefab) ? prefab.FacilityType : FacilityType.GrowRoom;
        }
        
        private float CalculateTotalPowerRequirement(CompleteFacilityTemplate template)
        {
            return template.RoomLayout.Sum(room => 
                _prefabLookup.TryGetValue(room.RoomPrefabId, out var prefab) ? prefab.PowerRequirement : 0f
            );
        }
        
        private void ValidateRoomSpacing(CompleteFacilityTemplate template, FacilityValidationResult result)
        {
            for (int i = 0; i < template.RoomLayout.Count; i++)
            {
                for (int j = i + 1; j < template.RoomLayout.Count; j++)
                {
                    var room1 = template.RoomLayout[i];
                    var room2 = template.RoomLayout[j];
                    
                    float distance = Vector3.Distance(room1.Position, room2.Position);
                    if (distance < 1f) // Minimum 1m clearance
                    {
                        result.ValidationWarnings.Add($"Rooms {room1.RoomPrefabId} and {room2.RoomPrefabId} may be too close");
                    }
                }
            }
        }
        
        private void ValidateBuildingCodes(CompleteFacilityTemplate template, FacilityValidationResult result)
        {
            // Check emergency exits
            var totalArea = template.TotalFootprint.x * template.TotalFootprint.y;
            if (totalArea > 500f) // 500 sq meters
            {
                var hasEmergencyExits = template.RoomLayout.Any(room =>
                    _prefabLookup.TryGetValue(room.RoomPrefabId, out var prefab) &&
                    prefab.ConstructionRequirements.RequiresEmergencyExits
                );
                
                if (!hasEmergencyExits)
                {
                    result.ValidationErrors.Add("Large facilities require emergency exits");
                    result.IsValid = false;
                }
            }
            
            // Check fire safety systems
            var hasFireSafety = template.RoomLayout.All(room =>
                _prefabLookup.TryGetValue(room.RoomPrefabId, out var prefab) &&
                prefab.ConstructionRequirements.RequiresFireSafety
            );
            
            if (!hasFireSafety)
            {
                result.ValidationWarnings.Add("Consider adding fire safety systems to all rooms");
            }
        }
        
        public FacilityConstructionEstimate EstimateConstruction(CompleteFacilityTemplate template)
        {
            var estimate = new FacilityConstructionEstimate
            {
                TemplateId = template.TemplateId,
                TotalCost = template.EstimatedCost,
                ConstructionDays = template.ConstructionTime,
                MaterialCosts = new Dictionary<string, float>(),
                LaborCosts = new Dictionary<string, float>(),
                PermitCosts = new Dictionary<string, float>()
            };
            
            // Calculate detailed costs
            foreach (var room in template.RoomLayout)
            {
                if (_prefabLookup.TryGetValue(room.RoomPrefabId, out var prefab))
                {
                    estimate.MaterialCosts[prefab.PrefabName] = prefab.Cost * 0.6f; // 60% materials
                    estimate.LaborCosts[prefab.PrefabName] = prefab.Cost * 0.3f; // 30% labor
                    estimate.PermitCosts[prefab.PrefabName] = prefab.Cost * 0.1f; // 10% permits/fees
                }
            }
            
            return estimate;
        }
        
        public FacilityLibraryStats GetLibraryStats()
        {
            return new FacilityLibraryStats
            {
                TotalFacilities = _facilityPrefabs.Count,
                TotalTemplates = _facilityTemplates.Count,
                TypeDistribution = _typeLookup.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count),
                AverageSize = _facilityPrefabs.Average(f => f.RoomSize.x * f.RoomSize.y * f.RoomSize.z),
                TotalValue = _facilityPrefabs.Sum(f => f.Cost),
                PowerRequirementRange = new Vector2(
                    _facilityPrefabs.Min(f => f.PowerRequirement),
                    _facilityPrefabs.Max(f => f.PowerRequirement)
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
    public class FacilityPrefabEntry
    {
        public string PrefabId;
        public string PrefabName;
        public GameObject Prefab;
        public FacilityType FacilityType;
        public Vector3 RoomSize; // length x height x width
        public int MaxCapacity;
        public float PowerRequirement;
        public float CoolingRequirement;
        public float VentilationRequirement;
        public float Cost;
        public List<FacilityFeature> RequiredFeatures = new List<FacilityFeature>();
        public List<string> RequiredComponents = new List<string>();
        public FacilityConstructionRequirements ConstructionRequirements;
    }
    
    [System.Serializable]
    public class CompleteFacilityTemplate
    {
        public string TemplateId;
        public string TemplateName;
        public FacilityScale FacilityScale;
        public Vector2 TotalFootprint;
        public List<RoomLayoutEntry> RoomLayout = new List<RoomLayoutEntry>();
        public float EstimatedCost;
        public float ConstructionTime; // days
        public float AnnualOperatingCost;
        public List<string> RequiredLicenses = new List<string>();
    }
    
    [System.Serializable]
    public class RoomLayoutEntry
    {
        public string RoomPrefabId;
        public Vector3 Position;
        public Quaternion Rotation = Quaternion.identity;
        public Vector3 Scale = Vector3.one;
        public string RoomName;
        public bool IsRequired = true;
    }
    
    [System.Serializable]
    public class RoomPrefabCollection
    {
        public string CollectionId;
        public string CollectionName;
        public FacilityType RoomType;
        public List<string> RoomPrefabIds = new List<string>();
        public string Description;
    }
    
    [System.Serializable]
    public class FacilityModuleSet
    {
        public string ModuleId;
        public string ModuleName;
        public List<string> RequiredRooms = new List<string>();
        public List<string> OptionalRooms = new List<string>();
        public float ModuleCost;
        public bool IsExpandable;
    }
    
    [System.Serializable]
    public class FacilityConstructionRequirements
    {
        public bool RequiresFoundation = true;
        public bool RequiresInsulation = false;
        public bool RequiresVaporBarrier = false;
        public bool RequiresFireSafety = true;
        public bool RequiresEmergencyExits = false;
        public bool RequiresSprinklerSystem = false;
        public bool RequiresBackupPower = false;
        public float MinCeilingHeight = 2.5f;
        public WallType RequiredWallType = WallType.Standard;
        public FloorType RequiredFloorType = FloorType.Concrete;
    }
    
    [System.Serializable]
    public class FacilityValidationResult
    {
        public bool IsValid;
        public List<string> ValidationErrors = new List<string>();
        public List<string> ValidationWarnings = new List<string>();
        public float ComplianceScore;
    }
    
    [System.Serializable]
    public class FacilityConstructionEstimate
    {
        public string TemplateId;
        public float TotalCost;
        public float ConstructionDays;
        public Dictionary<string, float> MaterialCosts = new Dictionary<string, float>();
        public Dictionary<string, float> LaborCosts = new Dictionary<string, float>();
        public Dictionary<string, float> PermitCosts = new Dictionary<string, float>();
    }
    
    [System.Serializable]
    public class FacilityLibraryStats
    {
        public int TotalFacilities;
        public int TotalTemplates;
        public Dictionary<FacilityType, int> TypeDistribution;
        public float AverageSize;
        public float TotalValue;
        public Vector2 PowerRequirementRange;
    }
    
    // Enums
    public enum FacilityScale
    {
        Personal,
        Small_Commercial,
        Commercial,
        Industrial,
        Enterprise
    }
    
    public enum FacilityFeature
    {
        EnvironmentalControl,
        Lighting,
        Irrigation,
        Ventilation,
        CO2_Enrichment,
        SecuritySystem,
        Automation,
        DataMonitoring,
        QualityControl,
        PowerDistribution,
        FireSafety,
        NetworkInfrastructure,
        InventoryTracking
    }
    
    public enum WallType
    {
        Standard,
        Insulated,
        Fire_Rated,
        Security_Grade,
        Commercial_Grade
    }
    
    public enum FloorType
    {
        Concrete,
        Sealed_Concrete,
        Easy_Clean,
        Anti_Static,
        Industrial_Epoxy,
        Raised_Access
    }
}