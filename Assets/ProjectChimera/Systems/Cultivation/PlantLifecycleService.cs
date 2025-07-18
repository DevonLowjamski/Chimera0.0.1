using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// PC-013-3: Plant Lifecycle Service - Handles plant creation, registration, and lifecycle management
    /// Extracted from monolithic PlantManager for Single Responsibility Principle
    /// Focuses solely on plant lifecycle operations and basic plant management
    /// </summary>
    public class PlantLifecycleService : IPlantLifecycleService
    {
        [Header("Lifecycle Configuration")]
        [SerializeField] private bool _enableDetailedLogging = false;
        
        // Dependencies
        private CultivationManager _cultivationManager;
        private List<PlantInstance> _plantsToUpdate = new List<PlantInstance>();
        
        // Event channels
        [SerializeField] private SimpleGameEventSO _onPlantCreated;
        
        // Events for other systems to subscribe to
        public System.Action<PlantInstance> OnPlantAdded { get; set; }
        public System.Action<PlantInstance> OnPlantStageChanged { get; set; }
        public System.Action<PlantInstance> OnPlantHealthUpdated { get; set; }
        
        public bool IsInitialized { get; private set; }
        
        public PlantLifecycleService(CultivationManager cultivationManager)
        {
            _cultivationManager = cultivationManager;
        }
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            Debug.Log("[PlantLifecycleService] Initializing plant lifecycle management...");
            
            // Get reference to CultivationManager as single source of truth
            if (_cultivationManager == null)
            {
                _cultivationManager = GameManager.Instance?.GetManager<CultivationManager>();
            }
            
            if (_cultivationManager == null)
            {
                Debug.LogError("[PlantLifecycleService] CultivationManager not found! PlantLifecycleService requires CultivationManager as data source.");
                return;
            }
            
            IsInitialized = true;
            Debug.Log("[PlantLifecycleService] Plant lifecycle management initialized successfully.");
        }
        
        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            Debug.Log("[PlantLifecycleService] Shutting down plant lifecycle management...");
            
            // Unsubscribe from all plant events
            foreach (var plant in _plantsToUpdate)
            {
                if (plant != null)
                {
                    UnsubscribeFromPlantEvents(plant);
                }
            }
            
            _plantsToUpdate.Clear();
            IsInitialized = false;
        }
        
        /// <summary>
        /// Creates a new plant instance from a strain definition.
        /// </summary>
        public PlantInstance CreatePlant(PlantStrainSO strain, Vector3 position, Transform parent = null)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantLifecycleService] Cannot create plant: Service not initialized");
                return null;
            }
            
            if (strain == null)
            {
                Debug.LogError("[PlantLifecycleService] Cannot create plant: strain is null");
                return null;
            }
            
            var plantInstance = PlantInstance.CreateFromStrain(strain, position, parent);
            RegisterPlant(plantInstance);
            
            Debug.Log($"[PlantLifecycleService] Created plant: {plantInstance.PlantID} (Strain: {strain.StrainName})");
            
            // Trigger plant created event
            _onPlantCreated?.Raise();
            
            return plantInstance;
        }
        
        /// <summary>
        /// Creates multiple plants from the same strain.
        /// </summary>
        public List<PlantInstance> CreatePlants(PlantStrainSO strain, List<Vector3> positions, Transform parent = null)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantLifecycleService] Cannot create plants: Service not initialized");
                return new List<PlantInstance>();
            }
            
            var plants = new List<PlantInstance>();
            
            foreach (var position in positions)
            {
                var plant = CreatePlant(strain, position, parent);
                if (plant != null)
                    plants.Add(plant);
            }
            
            Debug.Log($"[PlantLifecycleService] Created {plants.Count} plants from strain: {strain.StrainName}");
            return plants;
        }
        
        /// <summary>
        /// Registers an existing plant instance with the lifecycle service.
        /// </summary>
        public void RegisterPlant(PlantInstance plant)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantLifecycleService] Cannot register plant: Service not initialized");
                return;
            }
            
            if (plant == null)
            {
                Debug.LogError("[PlantLifecycleService] Cannot register null plant");
                return;
            }
            
            // Check if plant is already tracked by CultivationManager
            var existingPlant = _cultivationManager.GetPlant(plant.PlantID);
            if (existingPlant != null)
            {
                Debug.LogWarning($"[PlantLifecycleService] Plant {plant.PlantID} already registered in CultivationManager, skipping");
                return;
            }
            
            // Add to processing update list
            if (!_plantsToUpdate.Contains(plant))
            {
                _plantsToUpdate.Add(plant);
            }
            
            // Subscribe to plant events
            SubscribeToPlantEvents(plant);
            
            // Invoke OnPlantAdded event for other systems
            OnPlantAdded?.Invoke(plant);
            
            if (_enableDetailedLogging)
                Debug.Log($"[PlantLifecycleService] Registered plant: {plant.PlantID}");
        }
        
        /// <summary>
        /// Unregisters a plant from lifecycle management.
        /// </summary>
        public void UnregisterPlant(string plantID, PlantRemovalReason reason = PlantRemovalReason.Other)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantLifecycleService] Cannot unregister plant: Service not initialized");
                return;
            }
            
            // Find plant in processing list
            var plant = _plantsToUpdate.FirstOrDefault(p => p.PlantID == plantID);
            if (plant == null)
            {
                Debug.LogWarning($"[PlantLifecycleService] Attempted to unregister unknown plant: {plantID}");
                return;
            }
            
            // Unsubscribe from events
            UnsubscribeFromPlantEvents(plant);
            
            // Remove from processing list
            _plantsToUpdate.Remove(plant);
            
            Debug.Log($"[PlantLifecycleService] Unregistered plant: {plantID} (Reason: {reason})");
        }
        
        /// <summary>
        /// Gets a plant instance by ID - delegates to CultivationManager.
        /// </summary>
        public PlantInstanceSO GetPlant(string plantID)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantLifecycleService] Cannot get plant: Service not initialized");
                return null;
            }
            
            return _cultivationManager?.GetPlant(plantID);
        }
        
        /// <summary>
        /// Gets all active plants - delegates to CultivationManager.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetAllPlants()
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantLifecycleService] Cannot get plants: Service not initialized");
                return System.Linq.Enumerable.Empty<PlantInstanceSO>();
            }
            
            return _cultivationManager?.GetAllPlants() ?? System.Linq.Enumerable.Empty<PlantInstanceSO>();
        }
        
        /// <summary>
        /// Gets all plants in a specific growth stage - delegates to CultivationManager.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetPlantsInStage(PlantGrowthStage stage)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantLifecycleService] Cannot get plants by stage: Service not initialized");
                return System.Linq.Enumerable.Empty<PlantInstanceSO>();
            }
            
            return _cultivationManager?.GetPlantsByStage(stage) ?? System.Linq.Enumerable.Empty<PlantInstanceSO>();
        }
        
        /// <summary>
        /// Gets all plants ready for harvest - delegates to CultivationManager.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetHarvestablePlants()
        {
            return GetPlantsInStage(PlantGrowthStage.Harvest);
        }
        
        /// <summary>
        /// Gets all plants that need attention - delegates to CultivationManager.
        /// </summary>
        public IEnumerable<PlantInstanceSO> GetPlantsNeedingAttention()
        {
            if (!IsInitialized)
            {
                Debug.LogError("[PlantLifecycleService] Cannot get plants needing attention: Service not initialized");
                return System.Linq.Enumerable.Empty<PlantInstanceSO>();
            }
            
            return _cultivationManager?.GetPlantsNeedingAttention() ?? System.Linq.Enumerable.Empty<PlantInstanceSO>();
        }
        
        /// <summary>
        /// Gets all plants currently tracked for processing (PlantInstance objects).
        /// </summary>
        public IEnumerable<PlantInstance> GetTrackedPlants()
        {
            return _plantsToUpdate.AsReadOnly();
        }
        
        /// <summary>
        /// Gets a tracked plant instance by ID.
        /// </summary>
        public PlantInstance GetTrackedPlant(string plantID)
        {
            return _plantsToUpdate.FirstOrDefault(p => p.PlantID == plantID);
        }
        
        /// <summary>
        /// Validates plant health and performs basic maintenance checks.
        /// </summary>
        public void ValidatePlantHealth()
        {
            if (!IsInitialized) return;
            
            var plantsToRemove = new List<PlantInstance>();
            
            foreach (var plant in _plantsToUpdate)
            {
                if (plant == null || !plant.IsActive)
                {
                    plantsToRemove.Add(plant);
                    continue;
                }
                
                // Check for critical health issues
                if (plant.CurrentHealth <= 0f)
                {
                    Debug.LogWarning($"[PlantLifecycleService] Plant {plant.PlantID} has died (Health: {plant.CurrentHealth})");
                    plantsToRemove.Add(plant);
                }
            }
            
            // Remove inactive or dead plants
            foreach (var plant in plantsToRemove)
            {
                if (plant != null)
                {
                    UnregisterPlant(plant.PlantID, PlantRemovalReason.Died);
                }
                else
                {
                    _plantsToUpdate.Remove(plant);
                }
            }
            
            if (plantsToRemove.Count > 0)
            {
                Debug.Log($"[PlantLifecycleService] Removed {plantsToRemove.Count} inactive/dead plants during validation");
            }
        }
        
        /// <summary>
        /// Gets lifecycle statistics.
        /// </summary>
        public (int tracked, int total, int harvestable, int needingAttention) GetLifecycleStats()
        {
            if (!IsInitialized)
                return (0, 0, 0, 0);
            
            int tracked = _plantsToUpdate.Count;
            int total = _cultivationManager?.ActivePlantCount ?? 0;
            int harvestable = GetHarvestablePlants().Count();
            int needingAttention = GetPlantsNeedingAttention().Count();
            
            return (tracked, total, harvestable, needingAttention);
        }
        
        /// <summary>
        /// Subscribes to plant events for lifecycle tracking.
        /// </summary>
        private void SubscribeToPlantEvents(PlantInstance plant)
        {
            plant.OnGrowthStageChanged += OnPlantGrowthStageChanged;
            plant.OnHealthChanged += OnPlantHealthChanged;
            plant.OnPlantDied += OnPlantDied;
        }
        
        /// <summary>
        /// Unsubscribes from plant events.
        /// </summary>
        private void UnsubscribeFromPlantEvents(PlantInstance plant)
        {
            plant.OnGrowthStageChanged -= OnPlantGrowthStageChanged;
            plant.OnHealthChanged -= OnPlantHealthChanged;
            plant.OnPlantDied -= OnPlantDied;
        }
        
        /// <summary>
        /// Handles plant growth stage changes.
        /// </summary>
        private void OnPlantGrowthStageChanged(PlantInstance plant)
        {
            Debug.Log($"[PlantLifecycleService] Plant {plant.PlantID} advanced to {plant.CurrentGrowthStage}");
            OnPlantStageChanged?.Invoke(plant);
        }
        
        /// <summary>
        /// Handles plant health changes.
        /// </summary>
        private void OnPlantHealthChanged(PlantInstance plant)
        {
            if (_enableDetailedLogging)
                Debug.Log($"[PlantLifecycleService] Plant {plant.PlantID} health changed to {plant.CurrentHealth:F2}");
            
            OnPlantHealthUpdated?.Invoke(plant);
        }
        
        /// <summary>
        /// Handles plant death events.
        /// </summary>
        private void OnPlantDied(PlantInstance plant)
        {
            Debug.Log($"[PlantLifecycleService] Plant {plant.PlantID} died (Health: {plant.CurrentHealth:F2})");
            UnregisterPlant(plant.PlantID, PlantRemovalReason.Died);
        }
    }
}