using UnityEngine;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Base class for entities directly involved in cannabis cultivation.
    /// Includes plants, growing containers, and cultivation equipment.
    /// </summary>
    public abstract class CultivationEntity : SimulationEntity
    {
        [Header("Cultivation Properties")]
        [SerializeField] private string _cultivationZoneID;
        [SerializeField] private bool _requiresNutrients = false;
        [SerializeField] private bool _requiresWater = false;
        [SerializeField] private bool _affectedByEnvironment = true;

        /// <summary>
        /// ID of the cultivation zone this entity belongs to.
        /// </summary>
        public string CultivationZoneID 
        { 
            get => _cultivationZoneID; 
            set => _cultivationZoneID = value; 
        }

        /// <summary>
        /// Whether this entity requires nutrients to function properly.
        /// </summary>
        public bool RequiresNutrients => _requiresNutrients;

        /// <summary>
        /// Whether this entity requires water to function properly.
        /// </summary>
        public bool RequiresWater => _requiresWater;

        /// <summary>
        /// Whether this entity is affected by environmental conditions.
        /// </summary>
        public bool AffectedByEnvironment => _affectedByEnvironment;

        /// <summary>
        /// Current environmental conditions affecting this entity.
        /// This would be populated by the environmental system.
        /// </summary>
        public virtual IEnvironmentalConditions CurrentEnvironment { get; protected set; }

        /// <summary>
        /// Current nutrition status of this entity.
        /// </summary>
        public virtual NutritionStatus CurrentNutrition { get; protected set; }

        /// <summary>
        /// Current water status of this entity.
        /// </summary>
        public virtual WaterStatus CurrentWater { get; protected set; }

        protected override void OnSimulationStart()
        {
            base.OnSimulationStart();
            
            // Initialize cultivation-specific data
            InitializeCultivationSystems();
            
            LogDebug($"Cultivation entity initialized in zone: {_cultivationZoneID}");
        }

        protected override void OnSimulationUpdate(float deltaTime)
        {
            base.OnSimulationUpdate(deltaTime);

            // Update cultivation-specific systems
            if (_affectedByEnvironment)
            {
                UpdateEnvironmentalEffects(deltaTime);
            }

            if (_requiresNutrients)
            {
                UpdateNutritionalNeeds(deltaTime);
            }

            if (_requiresWater)
            {
                UpdateWaterNeeds(deltaTime);
            }

            OnCultivationUpdate(deltaTime);
        }

        /// <summary>
        /// Override this method to implement cultivation-specific initialization.
        /// </summary>
        protected virtual void InitializeCultivationSystems()
        {
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Override this method to implement cultivation-specific update logic.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        protected virtual void OnCultivationUpdate(float deltaTime)
        {
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Updates effects from environmental conditions.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        protected virtual void UpdateEnvironmentalEffects(float deltaTime)
        {
            // This would integrate with the environmental system
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Updates nutritional needs and effects.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        protected virtual void UpdateNutritionalNeeds(float deltaTime)
        {
            // This would integrate with the nutrition system
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Updates water needs and effects.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        protected virtual void UpdateWaterNeeds(float deltaTime)
        {
            // This would integrate with the irrigation system
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Called when this entity is moved to a different cultivation zone.
        /// </summary>
        /// <param name="newZoneID">ID of the new zone</param>
        public virtual void OnZoneChanged(string newZoneID)
        {
            string oldZone = _cultivationZoneID;
            _cultivationZoneID = newZoneID;
            
            OnZoneChangedInternal(oldZone, newZoneID);
            LogDebug($"Moved from zone {oldZone} to zone {newZoneID}");
        }

        /// <summary>
        /// Override this method to handle zone change effects.
        /// </summary>
        /// <param name="oldZoneID">Previous zone ID</param>
        /// <param name="newZoneID">New zone ID</param>
        protected virtual void OnZoneChangedInternal(string oldZoneID, string newZoneID)
        {
            // Base implementation - override in derived classes
        }
    }



    /// <summary>
    /// Represents the nutritional status of a cultivation entity.
    /// This is a placeholder - full implementation would be in the Nutrition system.
    /// </summary>
    [System.Serializable]
    public class NutritionStatus
    {
        public float Nitrogen;
        public float Phosphorus;
        public float Potassium;
        public float pH;
        public float EC; // Electrical Conductivity
        
        public static NutritionStatus Default => new NutritionStatus
        {
            Nitrogen = 1.0f,
            Phosphorus = 1.0f,
            Potassium = 1.0f,
            pH = 6.0f,
            EC = 1.2f
        };
    }

    /// <summary>
    /// Represents the water status of a cultivation entity.
    /// This is a placeholder - full implementation would be in the Irrigation system.
    /// </summary>
    [System.Serializable]
    public class WaterStatus
    {
        public float MoistureLevel;     // 0-1 range
        public float DrainageRate;      // How quickly water drains
        public float WaterQuality;      // Quality of available water
        
        public static WaterStatus Default => new WaterStatus
        {
            MoistureLevel = 0.6f,
            DrainageRate = 0.5f,
            WaterQuality = 1.0f
        };
    }

    /// <summary>
    /// Interface for environmental conditions that affect cultivation entities.
    /// Implemented by the comprehensive EnvironmentalConditions in Data.Cultivation.
    /// </summary>
    public interface IEnvironmentalConditions
    {
        float Temperature { get; }
        float Humidity { get; }
        float CO2Level { get; }
        float LightIntensity { get; }
        float AirFlow { get; }
    }
}