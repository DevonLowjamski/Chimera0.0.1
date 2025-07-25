using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Facilities;
using ProjectChimera.Data.Cultivation;
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Interactive plant component for Project Chimera cultivation system.
    /// Handles player interactions with individual plants including harvesting, treatment, and inspection.
    /// </summary>
    public class InteractivePlantComponent : MonoBehaviour
    {
        [Header("Plant Configuration")]
        [SerializeField] private PlantStrainSO _plantStrain;
        [SerializeField] private CannabisGenotype _genotype;
        [SerializeField] private PlantGrowthStage _currentGrowthStage = PlantGrowthStage.Seedling;
        
        [Header("Interaction Settings")]
        [SerializeField] private bool _canHarvest = false;
        [SerializeField] private bool _canTreat = true;
        [SerializeField] private bool _canInspect = true;
        [SerializeField] private float _interactionRange = 2f;
        
        [Header("Plant Health")]
        [SerializeField, Range(0f, 100f)] private float _health = 100f;
        [SerializeField, Range(0f, 100f)] private float _stress = 0f;
        [SerializeField, Range(0f, 100f)] private float _hydration = 80f;
        [SerializeField, Range(0f, 100f)] private float _nutrition = 75f;
        
        // Component References
        // private Component _speedTreeInstance; // Generic reference to avoid SpeedTree dependency - commented out for compilation
        private ProjectChimera.Systems.Cultivation.PlantInstance _plantInstance;
        
        // Public Properties
        public PlantStrainSO PlantStrain => _plantStrain;
        public CannabisGenotype Genotype => _genotype;
        public PlantGrowthStage CurrentGrowthStage => _currentGrowthStage;
        public bool CanHarvest => _canHarvest;
        public bool CanTreat => _canTreat;
        public bool CanInspect => _canInspect;
        public float InteractionRange => _interactionRange;
        public float Health => _health;
        public float Stress => _stress;
        public float Hydration => _hydration;
        public float Nutrition => _nutrition;
        
        // Additional properties expected by UI system
        public PlantGrowthStage CurrentStage => _currentGrowthStage;
        public float GrowthProgress => _plantInstance?.GrowthProgress ?? 0f;
        public float WaterLevel => _hydration;
        public float NutrientLevel => _nutrition;
        public bool IsHarvestable => _canHarvest;
        public PlantInspectionData PlantData => InspectPlant();
        public EnvironmentalConditions CurrentConditions => new EnvironmentalConditions
        {
            Temperature = 22f, // Default values - would be from environment system
            Humidity = 60f,
            LightIntensity = 400f,
            CO2Level = 400f,
            AirFlow = 1f
        };
        
        // Events - Fixed delegate signatures
        public System.Action<InteractivePlantComponent> OnPlantInteracted;
        public System.Action<InteractivePlantComponent> OnPlantHarvested;
        public System.Action<InteractivePlantComponent> OnPlantTreated;
        public System.Action<InteractivePlantComponent> OnPlantInspected;
        public System.Action<InteractivePlantComponent> OnPlantClicked;
        public System.Action<InteractivePlantComponent> OnPlantHovered;
        public System.Action<InteractivePlantComponent> OnPlantGrowthStageChanged;
        public System.Action<InteractivePlantComponent> OnPlantHealthChanged;
        
        private void Awake()
        {
            // _speedTreeInstance = GetComponent<SpeedTreePlantInstance>(); // Commented out - SpeedTreePlantInstance not available
            _plantInstance = GetComponent<ProjectChimera.Systems.Cultivation.PlantInstance>();
            
            // if (_speedTreeInstance == null)
            // {
            //     Debug.LogError($"InteractivePlantComponent requires SpeedTreePlantInstance component on {gameObject.name}");
            // }
        }
        
        private void Start()
        {
            InitializePlantInteraction();
        }
        
        /// <summary>
        /// Initialize plant interaction system
        /// </summary>
        private void InitializePlantInteraction()
        {
            // Set up interaction based on plant strain and growth stage
            UpdateInteractionCapabilities();
            
            // Subscribe to plant events
            if (_plantInstance != null)
            {
                _plantInstance.OnGrowthStageChanged += HandleGrowthStageChanged;
                _plantInstance.OnHealthChanged += HandleHealthChanged;
            }
        }
        
        /// <summary>
        /// Update interaction capabilities based on current state
        /// </summary>
        private void UpdateInteractionCapabilities()
        {
            // Update harvest capability based on growth stage
            _canHarvest = (_currentGrowthStage == PlantGrowthStage.Harvest || _currentGrowthStage == PlantGrowthStage.Ripening) && _health > 50f;
            
            // Update treatment capability based on health
            _canTreat = _health < 100f || _stress > 0f || _hydration < 100f || _nutrition < 100f;
            
            // Inspection is always available
            _canInspect = true;
        }
        
        /// <summary>
        /// Handle player interaction with plant
        /// </summary>
        public void OnPlayerInteraction()
        {
            if (!CanPlayerInteract())
                return;
            
            OnPlantInteracted?.Invoke(this);
            
            // Log interaction
            Debug.Log($"Player interacted with plant: {_plantStrain?.StrainName ?? "Unknown"} (Stage: {_currentGrowthStage})");
        }
        
        /// <summary>
        /// Harvest the plant
        /// </summary>
        public HarvestResult HarvestPlant()
        {
            if (!_canHarvest)
            {
                Debug.LogWarning($"Cannot harvest plant {gameObject.name} - not ready for harvest");
                return null;
            }
            
            OnPlantHarvested?.Invoke(this);
            
            // Update plant state
            _currentGrowthStage = PlantGrowthStage.Harvest;
            UpdateInteractionCapabilities();
            
            var result = new HarvestResult
            {
                TotalYield = CalculateEstimatedYield(),
                PlantStrain = _plantStrain?.StrainName ?? "Unknown",
                HarvestTime = System.DateTime.Now,
                Quality = _health / 100f
            };
            
            Debug.Log($"Harvested plant: {_plantStrain?.StrainName ?? "Unknown"}");
            return result;
        }
        
        /// <summary>
        /// Apply treatment to plant
        /// </summary>
        public bool TreatPlant(string treatmentType, float intensity = 1f)
        {
            if (!_canTreat)
            {
                Debug.LogWarning($"Cannot treat plant {gameObject.name} - treatment not available");
                return false;
            }
            
            // Apply treatment effects
            switch (treatmentType.ToLower())
            {
                case "water":
                    _hydration = Mathf.Min(100f, _hydration + (20f * intensity));
                    break;
                case "nutrients":
                    _nutrition = Mathf.Min(100f, _nutrition + (15f * intensity));
                    break;
                case "pesticide":
                    _stress = Mathf.Max(0f, _stress - (25f * intensity));
                    break;
                case "medicine":
                    _health = Mathf.Min(100f, _health + (10f * intensity));
                    break;
            }
            
            OnPlantTreated?.Invoke(this);
            UpdateInteractionCapabilities();
            
            Debug.Log($"Applied {treatmentType} treatment to plant: {_plantStrain?.StrainName ?? "Unknown"}");
            return true;
        }
        
        /// <summary>
        /// Inspect the plant for detailed information
        /// </summary>
        public PlantInspectionData InspectPlant()
        {
            if (!_canInspect)
            {
                Debug.LogWarning($"Cannot inspect plant {gameObject.name} - inspection not available");
                return null;
            }
            
            var inspectionData = new PlantInspectionData
            {
                PlantId = GetInstanceID().ToString(),
                StrainName = _plantStrain?.StrainName ?? "Unknown",
                GrowthStage = _currentGrowthStage,
                CurrentStage = _currentGrowthStage,
                Health = _health,
                CurrentHealth = _health,
                Stress = _stress,
                Hydration = _hydration,
                Nutrition = _nutrition,
                GrowthProgress = GrowthProgress,
                WaterLevel = _hydration,
                NutrientLevel = _nutrition,
                CanHarvest = _canHarvest,
                IsHarvestable = _canHarvest,
                EstimatedYield = CalculateEstimatedYield(),
                DaysOld = Mathf.FloorToInt(Time.time / 86400f), // Simplified age calculation
                InspectionTime = System.DateTime.Now
            };
            
            OnPlantInspected?.Invoke(this);
            
            Debug.Log($"Inspected plant: {_plantStrain?.StrainName ?? "Unknown"} - Health: {_health:F1}%");
            return inspectionData;
        }
        
        /// <summary>
        /// Check if player can interact with this plant
        /// </summary>
        private bool CanPlayerInteract()
        {
            // Check if player is within interaction range
            var player = Camera.main?.transform;
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                return distance <= _interactionRange;
            }
            
            return true; // Default to true if no player found
        }
        
        /// <summary>
        /// Calculate estimated yield for this plant
        /// </summary>
        private float CalculateEstimatedYield()
        {
            float baseYield = _plantStrain?.BaseYieldGrams ?? 50f;
            float healthModifier = _health / 100f;
            float stressModifier = 1f - (_stress / 100f);
            float nutritionModifier = _nutrition / 100f;
            
            return baseYield * healthModifier * stressModifier * nutritionModifier;
        }
        
        /// <summary>
        /// Handle growth stage changes from PlantInstance
        /// </summary>
        private void HandleGrowthStageChanged(ProjectChimera.Systems.Cultivation.PlantInstance plantInstance)
        {
            if (plantInstance != null)
            {
                // Both classes now use the same enum from ProjectChimera.Data.Genetics
                _currentGrowthStage = plantInstance.CurrentGrowthStage;
                UpdateInteractionCapabilities();
                OnPlantGrowthStageChanged?.Invoke(this);
            }
        }
        
        /// <summary>
        /// Handle health changes from PlantInstance
        /// </summary>
        private void HandleHealthChanged(ProjectChimera.Systems.Cultivation.PlantInstance plantInstance)
        {
            if (plantInstance != null)
            {
                _health = plantInstance.CurrentHealth * 100f; // Convert from 0-1 to 0-100 scale
                UpdateInteractionCapabilities();
                OnPlantHealthChanged?.Invoke(this);
            }
        }
        
        /// <summary>
        /// Update plant statistics
        /// </summary>
        public void UpdatePlantStats(float health, float stress, float hydration, float nutrition)
        {
            _health = Mathf.Clamp(health, 0f, 100f);
            _stress = Mathf.Clamp(stress, 0f, 100f);
            _hydration = Mathf.Clamp(hydration, 0f, 100f);
            _nutrition = Mathf.Clamp(nutrition, 0f, 100f);
            
            UpdateInteractionCapabilities();
        }
        
        /// <summary>
        /// Get interaction tooltip text
        /// </summary>
        public string GetInteractionTooltip()
        {
            if (!CanPlayerInteract())
                return "Too far away";
            
            var tooltip = $"{_plantStrain?.StrainName ?? "Unknown Plant"}\n";
            tooltip += $"Stage: {_currentGrowthStage}\n";
            tooltip += $"Health: {_health:F0}%\n";
            
            if (_canHarvest)
                tooltip += "Press E to Harvest\n";
            if (_canTreat)
                tooltip += "Press T to Treat\n";
            if (_canInspect)
                tooltip += "Press I to Inspect";
            
            return tooltip;
        }
        
        /// <summary>
        /// Water the plant
        /// </summary>
        public void WaterPlant(float amount)
        {
            TreatPlant("water", amount / 20f); // Convert amount to intensity
        }
        
        /// <summary>
        /// Add nutrients to the plant
        /// </summary>
        public void AddNutrients(float amount, string nutrientType = "General")
        {
            TreatPlant("nutrients", amount / 15f); // Convert amount to intensity
        }
        
        /// <summary>
        /// Get status information for UI
        /// </summary>
        public PlantInspectionData GetStatusInfo()
        {
            return InspectPlant();
        }
        
        /// <summary>
        /// Handle plant clicked event
        /// </summary>
        public void OnPlantClick()
        {
            OnPlantClicked?.Invoke(this);
        }
        
        /// <summary>
        /// Handle plant hovered event
        /// </summary>
        public void OnPlantHover()
        {
            OnPlantHovered?.Invoke(this);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_plantInstance != null)
            {
                _plantInstance.OnGrowthStageChanged -= HandleGrowthStageChanged;
                _plantInstance.OnHealthChanged -= HandleHealthChanged;
            }
        }
    }
    
    /// <summary>
    /// Plant inspection data structure
    /// </summary>
    [System.Serializable]
    public class PlantInspectionData
    {
        public string PlantId;
        public string StrainName;
        public PlantGrowthStage GrowthStage;
        public PlantGrowthStage CurrentStage;
        public float Health;
        public float CurrentHealth;
        public float Stress;
        public float Hydration;
        public float Nutrition;
        public float GrowthProgress;
        public float WaterLevel;
        public float NutrientLevel;
        public bool CanHarvest;
        public bool IsHarvestable;
        public float EstimatedYield;
        public int DaysOld;
        public System.DateTime InspectionTime;
    }
}