using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProjectChimera.Data.Facilities
{
    // Import shared PlayerProfile from ProjectChimera.Data
    using ProjectChimera.Data;

    [System.Serializable]
    public enum FacilityRoomType
    {
        Vegetative,
        Flowering,
        Nursery,
        Mother,
        Drying,
        Curing,
        Processing,
        Storage,
        Laboratory,
        Office,
        Utility
    }

    [System.Serializable]
    public enum RoomStatus
    {
        Planned,
        Idle,
        Initializing,
        Active,
        Optimal,
        Maintenance,
        Error,
        Offline,
        Standby,
        Optimizing,
        Empty,
        Full,
        Alert
    }

    [System.Serializable]
    public enum FacilitySceneType
    {
        IndoorFacility,
        GreenhouseFacility,
        Greenhouse,
        OutdoorFarm,
        HybridFacility,
        MixedFacility,
        ProcessingFacility,
        ResearchFacility,
        CommercialGrowhouse,
        HomeGrow,
        UrbanRooftop
    }

    [System.Serializable]
    public enum WeatherType
    {
        Clear,
        Cloudy,
        Rainy,
        Stormy,
        Snowy,
        Foggy,
        Windy
    }

    [System.Serializable]
    public enum TimeOfDay
    {
        Dawn,
        Morning,
        Noon,
        Afternoon,
        Evening,
        Dusk,
        Night,
        Midnight
    }

    [System.Serializable]
    public enum AutomationProfile
    {
        Manual,
        SemiAutomatic,
        FullyAutomatic,
        AIOptimized,
        Custom
    }

    [System.Serializable]
    public enum WateringSchedule
    {
        Manual,
        Daily,
        EveryOtherDay,
        Weekly,
        Moisture_Based,
        Smart_Sensor,
        Custom
    }

    [System.Serializable]
    public struct FacilityConfiguration
    {
        public string facilityName;
        public Vector3 dimensions;
        public FacilityRoomType roomType;
        public int maxCapacity;
        public AutomationProfile automationLevel;
        public WateringSchedule wateringSchedule;
    }

    [System.Serializable]
    public struct RoomEnvironmentalSettings
    {
        public float targetTemperature;
        public float targetHumidity;
        public float lightIntensity;
        public float co2Level;
        public Vector2 temperatureRange;
        public Vector2 humidityRange;
    }

    [System.Serializable]
    public class HarvestResult
    {
        public string plantId;
        public float yieldAmount;
        public float quality;
        public DateTime harvestDate;
        public List<string> harvestedParts;
        
        // Additional fields for UI compatibility
        public float TotalYield;
        public string PlantStrain;
        public DateTime HarvestTime;
        public float Quality;
    }

    [System.Serializable]
    public class CO2Controller
    {
        public bool isActive;
        public float targetCO2Level;
        public float currentCO2Level;
        public float maxOutputRate;
        
        // Compatibility property for AdvancedGrowRoomController
        public float CurrentCO2Level => currentCO2Level;
        
        // Additional properties for advanced control
        public float PowerConsumption => isActive ? maxOutputRate * 0.1f : 0f; // Estimated power consumption
        public float InjectionRate => isActive ? maxOutputRate : 0f;

        public void SetTargetCO2Level(float co2Level)
        {
            targetCO2Level = co2Level;
        }
        
        public void StopInjection()
        {
            isActive = false;
        }
    }

    [System.Serializable]
    public class RoomLayout
    {
        public string roomName;
        public FacilityRoomType roomType;
        public Vector3 position;
        public Vector3 size;
        public List<Vector3> plantPositions;
        public RoomEnvironmentalSettings environmentalSettings;
        
        // Compatibility properties for ProceduralSceneGenerator
        public string RoomId { get; set; } = System.Guid.NewGuid().ToString();
        public string RoomName 
        { 
            get => roomName; 
            set => roomName = value; 
        }
        public string RoomType 
        { 
            get => roomType.ToString(); 
            set => System.Enum.TryParse(value, out roomType); 
        }
        public Vector3 Position 
        { 
            get => position; 
            set => position = value; 
        }
        public Vector3 Dimensions 
        { 
            get => size; 
            set => size = value; 
        }
        public float Area => size.x * size.z;
    }

    [System.Serializable]
    public class TerrainGenerator
    {
        public bool generateTerrain;
        public Vector2Int terrainSize;
        public float terrainHeight;
        public Material[] groundMaterials;
    }

    [System.Serializable]
    public class BuildingGenerator
    {
        public bool generateBuildings;
        public GameObject[] buildingPrefabs;
        public Material[] wallMaterials;
        public Material[] roofMaterials;
    }

    [System.Serializable]
    public class VegetationGenerator
    {
        public bool generateVegetation;
        public GameObject[] plantPrefabs;
        public float vegetationDensity;
        public bool includeWildVegetation;
    }
}