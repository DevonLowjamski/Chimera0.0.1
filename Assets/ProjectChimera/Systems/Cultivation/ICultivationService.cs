using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// PC-013-2: Interface for cultivation service components
    /// Defines contracts for modular cultivation system components
    /// </summary>
    public interface ICultivationService
    {
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
    }

    /// <summary>
    /// Interface for plant lifecycle management
    /// </summary>
    public interface IPlantLifecycleManager : ICultivationService
    {
        PlantInstanceSO PlantSeed(string plantName, PlantStrainSO strain, GenotypeDataSO genotype, Vector3 position, string zoneId = "default");
        bool RemovePlant(string plantId, bool isHarvest = false);
        PlantInstanceSO GetPlant(string plantId);
        IEnumerable<PlantInstanceSO> GetAllPlants();
        IEnumerable<PlantInstanceSO> GetPlantsByStage(PlantGrowthStage stage);
        IEnumerable<PlantInstanceSO> GetPlantsNeedingAttention();
        
        int ActivePlantCount { get; }
        int TotalPlantsGrown { get; }
        int TotalPlantsHarvested { get; }
        float TotalYieldHarvested { get; }
    }

    /// <summary>
    /// Interface for plant care and maintenance
    /// </summary>
    public interface IPlantCareManager : ICultivationService
    {
        bool WaterPlant(string plantId, float waterAmount = 0.5f);
        bool FeedPlant(string plantId, float nutrientAmount = 0.4f);
        bool TrainPlant(string plantId, string trainingType);
        void WaterAllPlants(float waterAmount = 0.5f);
        void FeedAllPlants(float nutrientAmount = 0.4f);
    }

    /// <summary>
    /// Interface for environmental management
    /// </summary>
    public interface IEnvironmentalManager : ICultivationService
    {
        void SetZoneEnvironment(string zoneId, ProjectChimera.Data.Environment.EnvironmentalConditions environment);
        ProjectChimera.Data.Environment.EnvironmentalConditions GetZoneEnvironment(string zoneId);
        ProjectChimera.Data.Environment.EnvironmentalConditions GetEnvironmentForPlant(string plantId);
    }

    /// <summary>
    /// Interface for growth processing
    /// </summary>
    public interface IGrowthProcessor : ICultivationService
    {
        void ProcessDailyGrowthForAllPlants();
        void ForceGrowthUpdate();
        float AveragePlantHealth { get; }
        bool EnableAutoGrowth { get; set; }
        float TimeAcceleration { get; set; }
    }

    /// <summary>
    /// Interface for harvest management
    /// </summary>
    public interface IHarvestManager : ICultivationService
    {
        void ProcessHarvest(PlantInstanceSO plant);
        void AddHarvestToInventory(PlantInstanceSO plant, float yieldAmount, float qualityScore);
        bool HarvestPlant(string plantId);
    }
}