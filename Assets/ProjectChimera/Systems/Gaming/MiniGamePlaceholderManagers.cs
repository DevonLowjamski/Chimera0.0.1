using System;
using UnityEngine;
using ProjectChimera.Systems.Gaming;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Placeholder for PlantManager - will be replaced with proper reference
    /// This should eventually reference ProjectChimera.Systems.Cultivation.PlantManager
    /// </summary>
    public class PlantManager
    {
        public event Action<PlantInstance> OnPlantHealthChange;
        public event Action<PlantInstance> OnHarvestTime;
        
        // Placeholder methods
        public void TriggerHealthChange(PlantInstance plant)
        {
            OnPlantHealthChange?.Invoke(plant);
        }
        
        public void TriggerHarvestTime(PlantInstance plant)
        {
            OnHarvestTime?.Invoke(plant);
        }
    }
    
    /// <summary>
    /// Placeholder for EnvironmentalManager - will be replaced with proper reference
    /// This should eventually reference ProjectChimera.Systems.Environment.EnvironmentalManager
    /// </summary>
    public class EnvironmentalManager
    {
        public event Action<EnvironmentalCrisis> OnEnvironmentalCrisis;
        
        // Placeholder methods
        public void TriggerCrisis(EnvironmentalCrisis crisis)
        {
            OnEnvironmentalCrisis?.Invoke(crisis);
        }
    }
    
    /// <summary>
    /// Placeholder for GameManager - will be replaced with proper reference
    /// This should eventually reference ProjectChimera.Core.GameManager
    /// </summary>
    public class GameManager
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance ??= new GameManager();
        
        // Placeholder method to get managers
        public T GetManager<T>() where T : class, new()
        {
            // Return new instance for now - will be replaced with proper manager registry
            return new T();
        }
    }
}

// Add extension to PlantInstance for missing methods
namespace ProjectChimera.Systems.Gaming
{
    public static class PlantInstanceExtensions
    {
        public static PlantHealthData GetHealthData(this PlantInstance plant)
        {
            return new PlantHealthData
            {
                Health = plant.Health,
                HasDiseases = plant.Health < 0.7f,
                HasPests = plant.Health < 0.5f
            };
        }
    }
    
    /// <summary>
    /// Placeholder health data for PlantInstance
    /// </summary>
    public class PlantHealthData
    {
        public float Health;
        public bool HasDiseases;
        public bool HasPests;
        
        public bool HasActiveDiseases() => HasDiseases;
        public bool HasActivePests() => HasPests;
    }
} 