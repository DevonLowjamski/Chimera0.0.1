using UnityEngine;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Base class for entities that affect or monitor environmental conditions.
    /// Includes HVAC equipment, sensors, and environmental control systems.
    /// </summary>
    public abstract class EnvironmentalEntity : SimulationEntity
    {
        [Header("Environmental Properties")]
        [SerializeField] private string _environmentalZoneID;
        [SerializeField] private EnvironmentalInfluenceType _influenceType;
        [SerializeField] private float _influenceRadius = 5.0f;
        [SerializeField] private bool _requiresPower = true;

        /// <summary>
        /// ID of the environmental zone this entity affects or monitors.
        /// </summary>
        public string EnvironmentalZoneID 
        { 
            get => _environmentalZoneID; 
            set => _environmentalZoneID = value; 
        }

        /// <summary>
        /// Type of environmental influence this entity provides.
        /// </summary>
        public EnvironmentalInfluenceType InfluenceType => _influenceType;

        /// <summary>
        /// Radius of influence for this environmental entity.
        /// </summary>
        public float InfluenceRadius => _influenceRadius;

        /// <summary>
        /// Whether this entity requires electrical power to function.
        /// </summary>
        public bool RequiresPower => _requiresPower;

        /// <summary>
        /// Whether this entity is currently powered and operational.
        /// </summary>
        public virtual bool IsPowered { get; protected set; } = true;

        /// <summary>
        /// Whether this entity is currently active and affecting the environment.
        /// </summary>
        public virtual bool IsActive { get; protected set; } = true;

        /// <summary>
        /// Current power consumption of this entity (in watts).
        /// </summary>
        public virtual float PowerConsumption { get; protected set; } = 0.0f;

        /// <summary>
        /// Current environmental effect being produced by this entity.
        /// </summary>
        public virtual EnvironmentalEffect CurrentEffect { get; protected set; }

        protected override void OnSimulationStart()
        {
            base.OnSimulationStart();
            
            InitializeEnvironmentalSystems();
            UpdatePowerStatus();
            
            LogDebug($"Environmental entity initialized in zone: {_environmentalZoneID}");
        }

        protected override void OnSimulationUpdate(float deltaTime)
        {
            base.OnSimulationUpdate(deltaTime);

            // Update power status
            UpdatePowerStatus();

            // Only process environmental effects if powered and active
            if (IsPowered && IsActive)
            {
                ProcessEnvironmentalEffects(deltaTime);
                OnEnvironmentalUpdate(deltaTime);
            }
        }

        /// <summary>
        /// Override this method to implement environmental system initialization.
        /// </summary>
        protected virtual void InitializeEnvironmentalSystems()
        {
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Override this method to implement environmental-specific update logic.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        protected virtual void OnEnvironmentalUpdate(float deltaTime)
        {
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Processes the environmental effects this entity produces.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        protected virtual void ProcessEnvironmentalEffects(float deltaTime)
        {
            // This would integrate with the environmental system
            CurrentEffect = CalculateEnvironmentalEffect(deltaTime);
        }

        /// <summary>
        /// Override this method to calculate the environmental effect this entity produces.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        /// <returns>The environmental effect being produced</returns>
        protected virtual EnvironmentalEffect CalculateEnvironmentalEffect(float deltaTime)
        {
            // Base implementation - override in derived classes
            return new EnvironmentalEffect();
        }

        /// <summary>
        /// Updates the power status of this entity.
        /// </summary>
        protected virtual void UpdatePowerStatus()
        {
            if (!_requiresPower)
            {
                IsPowered = true;
                PowerConsumption = 0.0f;
                return;
            }

            // This would integrate with the electrical system
            // For now, assume powered if active
            IsPowered = IsActive;
            PowerConsumption = IsPowered ? CalculatePowerConsumption() : 0.0f;
        }

        /// <summary>
        /// Override this method to calculate current power consumption.
        /// </summary>
        /// <returns>Current power consumption in watts</returns>
        protected virtual float CalculatePowerConsumption()
        {
            // Base implementation - override in derived classes
            return 100.0f; // Default 100W
        }

        /// <summary>
        /// Activates this environmental entity.
        /// </summary>
        public virtual void Activate()
        {
            if (IsActive) return;

            IsActive = true;
            OnActivated();
            LogDebug("Environmental entity activated");
        }

        /// <summary>
        /// Deactivates this environmental entity.
        /// </summary>
        public virtual void Deactivate()
        {
            if (!IsActive) return;

            IsActive = false;
            OnDeactivated();
            LogDebug("Environmental entity deactivated");
        }

        /// <summary>
        /// Override this method to handle activation events.
        /// </summary>
        protected virtual void OnActivated()
        {
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Override this method to handle deactivation events.
        /// </summary>
        protected virtual void OnDeactivated()
        {
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Called when this entity is moved to a different environmental zone.
        /// </summary>
        /// <param name="newZoneID">ID of the new zone</param>
        public virtual void OnEnvironmentalZoneChanged(string newZoneID)
        {
            string oldZone = _environmentalZoneID;
            _environmentalZoneID = newZoneID;
            
            OnEnvironmentalZoneChangedInternal(oldZone, newZoneID);
            LogDebug($"Moved from environmental zone {oldZone} to zone {newZoneID}");
        }

        /// <summary>
        /// Override this method to handle environmental zone change effects.
        /// </summary>
        /// <param name="oldZoneID">Previous zone ID</param>
        /// <param name="newZoneID">New zone ID</param>
        protected virtual void OnEnvironmentalZoneChangedInternal(string oldZoneID, string newZoneID)
        {
            // Base implementation - override in derived classes
        }
    }

    /// <summary>
    /// Types of environmental influence an entity can provide.
    /// </summary>
    public enum EnvironmentalInfluenceType
    {
        Temperature,    // Heating or cooling
        Humidity,       // Humidifying or dehumidifying
        Airflow,        // Air circulation
        CO2,            // CO2 generation or scrubbing
        Light,          // Lighting systems
        Monitoring,     // Sensor systems
        Filtration,     // Air or water filtration
        Combined        // Multiple environmental effects
    }

    /// <summary>
    /// Represents an environmental effect produced by an entity.
    /// This is a placeholder - full implementation would be in the Environment system.
    /// </summary>
    [System.Serializable]
    public class EnvironmentalEffect
    {
        public float TemperatureChange;
        public float HumidityChange;
        public float CO2Change;
        public float LightIntensityChange;
        public float AirFlowChange;
        public Vector3 EffectPosition;
        public float EffectRadius;
        
        public static EnvironmentalEffect Zero => new EnvironmentalEffect
        {
            TemperatureChange = 0.0f,
            HumidityChange = 0.0f,
            CO2Change = 0.0f,
            LightIntensityChange = 0.0f,
            AirFlowChange = 0.0f,
            EffectPosition = Vector3.zero,
            EffectRadius = 0.0f
        };
    }
}