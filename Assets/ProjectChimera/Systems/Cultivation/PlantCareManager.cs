using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// PC-013-2: Plant Care Manager - Handles plant watering, feeding, and training
    /// Extracted from monolithic CultivationManager for Single Responsibility Principle
    /// </summary>
    public class PlantCareManager : IPlantCareManager
    {
        // Dependencies
        private IPlantLifecycleManager _plantLifecycleManager;
        
        public bool IsInitialized { get; private set; }
        
        public PlantCareManager(IPlantLifecycleManager plantLifecycleManager)
        {
            _plantLifecycleManager = plantLifecycleManager;
        }
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("[PlantCareManager] Initializing plant care management...");
            IsInitialized = true;
        }
        
        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("[PlantCareManager] Shutting down plant care management...");
            IsInitialized = false;
        }
        
        /// <summary>
        /// Waters a specific plant.
        /// </summary>
        public bool WaterPlant(string plantId, float waterAmount = 0.5f)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantCareManager] Cannot water plant: Manager not initialized.");
                return false;
            }
            
            var plant = _plantLifecycleManager.GetPlant(plantId);
            if (plant == null)
            {
                Debug.LogWarning($"[PlantCareManager] Cannot water plant: Plant ID '{plantId}' not found.");
                return false;
            }
            
            // Validate water amount
            waterAmount = Mathf.Clamp01(waterAmount);
            
            plant.Water(waterAmount, System.DateTime.Now);
            Debug.Log($"[PlantCareManager] Watered plant '{plantId}' with {waterAmount * 100f}% water.");
            
            return true;
        }
        
        /// <summary>
        /// Feeds nutrients to a specific plant.
        /// </summary>
        public bool FeedPlant(string plantId, float nutrientAmount = 0.4f)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantCareManager] Cannot feed plant: Manager not initialized.");
                return false;
            }
            
            var plant = _plantLifecycleManager.GetPlant(plantId);
            if (plant == null)
            {
                Debug.LogWarning($"[PlantCareManager] Cannot feed plant: Plant ID '{plantId}' not found.");
                return false;
            }
            
            // Validate nutrient amount
            nutrientAmount = Mathf.Clamp01(nutrientAmount);
            
            plant.Feed(nutrientAmount, System.DateTime.Now);
            Debug.Log($"[PlantCareManager] Fed plant '{plantId}' with {nutrientAmount * 100f}% nutrients.");
            
            return true;
        }
        
        /// <summary>
        /// Applies training to a specific plant.
        /// </summary>
        public bool TrainPlant(string plantId, string trainingType)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantCareManager] Cannot train plant: Manager not initialized.");
                return false;
            }
            
            var plant = _plantLifecycleManager.GetPlant(plantId);
            if (plant == null)
            {
                Debug.LogWarning($"[PlantCareManager] Cannot train plant: Plant ID '{plantId}' not found.");
                return false;
            }
            
            if (string.IsNullOrEmpty(trainingType))
            {
                Debug.LogWarning($"[PlantCareManager] Cannot train plant '{plantId}': Training type not specified.");
                return false;
            }
            
            plant.ApplyTraining(trainingType, System.DateTime.Now);
            Debug.Log($"[PlantCareManager] Applied '{trainingType}' training to plant '{plantId}'.");
            
            return true;
        }
        
        /// <summary>
        /// Waters all plants in the cultivation system.
        /// </summary>
        public void WaterAllPlants(float waterAmount = 0.5f)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantCareManager] Cannot water all plants: Manager not initialized.");
                return;
            }
            
            // Validate water amount
            waterAmount = Mathf.Clamp01(waterAmount);
            
            int wateredCount = 0;
            var allPlants = _plantLifecycleManager.GetAllPlants();
            
            foreach (var plant in allPlants)
            {
                if (plant.WaterLevel < 0.8f) // Only water if needed
                {
                    plant.Water(waterAmount, System.DateTime.Now);
                    wateredCount++;
                }
            }
            
            Debug.Log($"[PlantCareManager] Auto-watered {wateredCount}/{_plantLifecycleManager.ActivePlantCount} plants.");
        }
        
        /// <summary>
        /// Feeds all plants in the cultivation system.
        /// </summary>
        public void FeedAllPlants(float nutrientAmount = 0.4f)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantCareManager] Cannot feed all plants: Manager not initialized.");
                return;
            }
            
            // Validate nutrient amount
            nutrientAmount = Mathf.Clamp01(nutrientAmount);
            
            int fedCount = 0;
            var allPlants = _plantLifecycleManager.GetAllPlants();
            
            foreach (var plant in allPlants)
            {
                if (plant.NutrientLevel < 0.7f) // Only feed if needed
                {
                    plant.Feed(nutrientAmount, System.DateTime.Now);
                    fedCount++;
                }
            }
            
            Debug.Log($"[PlantCareManager] Auto-fed {fedCount}/{_plantLifecycleManager.ActivePlantCount} plants.");
        }
        
        /// <summary>
        /// Performs comprehensive care for plants needing attention
        /// </summary>
        public void CareForPlantsNeedingAttention()
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantCareManager] Cannot care for plants: Manager not initialized.");
                return;
            }
            
            var plantsNeedingAttention = _plantLifecycleManager.GetPlantsNeedingAttention();
            int caredForCount = 0;
            
            foreach (var plant in plantsNeedingAttention)
            {
                bool careProvided = false;
                
                // Water if needed
                if (plant.WaterLevel < 0.3f)
                {
                    WaterPlant(plant.PlantID, 0.6f);
                    careProvided = true;
                }
                
                // Feed if needed
                if (plant.NutrientLevel < 0.3f)
                {
                    FeedPlant(plant.PlantID, 0.5f);
                    careProvided = true;
                }
                
                if (careProvided)
                {
                    caredForCount++;
                }
            }
            
            Debug.Log($"[PlantCareManager] Provided care for {caredForCount} plants needing attention.");
        }
        
        /// <summary>
        /// Gets care statistics for all plants
        /// </summary>
        public (int needsWater, int needsNutrients, int needsTraining) GetCareStatistics()
        {
            if (!IsInitialized) return (0, 0, 0);
            
            var allPlants = _plantLifecycleManager.GetAllPlants();
            int needsWater = 0;
            int needsNutrients = 0;
            int needsTraining = 0;
            
            foreach (var plant in allPlants)
            {
                if (plant.WaterLevel < 0.3f) needsWater++;
                if (plant.NutrientLevel < 0.3f) needsNutrients++;
                if (plant.StressLevel > 0.7f) needsTraining++;
            }
            
            return (needsWater, needsNutrients, needsTraining);
        }
    }
}