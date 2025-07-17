using UnityEngine;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Base class for all entities that participate in Project Chimera's simulation.
    /// Provides common simulation functionality like time scaling and state management.
    /// </summary>
    public abstract class SimulationEntity : ChimeraMonoBehaviour
    {
        [Header("Simulation Properties")]
        [SerializeField] private bool _participateInTimeScaling = true;
        [SerializeField] private bool _pauseWhenGamePaused = true;
        [SerializeField] private float _simulationUpdateInterval = 0.1f; // Update every 100ms by default

        private float _lastSimulationUpdate;
        private bool _isSimulationPaused;

        /// <summary>
        /// Whether this entity is affected by global time scaling.
        /// </summary>
        public bool ParticipateInTimeScaling => _participateInTimeScaling;

        /// <summary>
        /// Whether this entity should pause when the game is paused.
        /// </summary>
        public bool PauseWhenGamePaused => _pauseWhenGamePaused;

        /// <summary>
        /// How often this entity updates its simulation (in seconds).
        /// </summary>
        public float SimulationUpdateInterval => _simulationUpdateInterval;

        /// <summary>
        /// Whether this entity's simulation is currently paused.
        /// </summary>
        public bool IsSimulationPaused => _isSimulationPaused;

        /// <summary>
        /// Current effective time scale for this entity.
        /// </summary>
        public virtual float EffectiveTimeScale 
        { 
            get 
            { 
                if (!_participateInTimeScaling) return 1.0f;
                
                // This would be retrieved from a TimeManager in a full implementation
                return Time.timeScale; 
            } 
        }

        protected override void Start()
        {
            base.Start();
            _lastSimulationUpdate = Time.time;
            OnSimulationStart();
        }

        protected virtual void Update()
        {
            if (_isSimulationPaused) return;

            // Check if enough time has passed for the next simulation update
            float currentTime = Time.time;
            float deltaTime = currentTime - _lastSimulationUpdate;

            if (deltaTime >= _simulationUpdateInterval)
            {
                // Calculate effective delta time with time scaling
                float effectiveDeltaTime = deltaTime * EffectiveTimeScale;
                
                OnSimulationUpdate(effectiveDeltaTime);
                _lastSimulationUpdate = currentTime;
            }
        }

        /// <summary>
        /// Pauses this entity's simulation.
        /// </summary>
        public virtual void PauseSimulation()
        {
            if (_isSimulationPaused) return;

            _isSimulationPaused = true;
            OnSimulationPaused();
            LogDebug("Simulation paused");
        }

        /// <summary>
        /// Resumes this entity's simulation.
        /// </summary>
        public virtual void ResumeSimulation()
        {
            if (!_isSimulationPaused) return;

            _isSimulationPaused = false;
            _lastSimulationUpdate = Time.time; // Reset timer to prevent time jumps
            OnSimulationResumed();
            LogDebug("Simulation resumed");
        }

        /// <summary>
        /// Sets the simulation update interval for this entity.
        /// </summary>
        /// <param name="interval">New update interval in seconds</param>
        public void SetSimulationUpdateInterval(float interval)
        {
            _simulationUpdateInterval = Mathf.Max(0.0f, interval);
            LogDebug($"Simulation update interval set to {_simulationUpdateInterval}s");
        }

        /// <summary>
        /// Override this method to implement simulation startup logic.
        /// </summary>
        protected virtual void OnSimulationStart()
        {
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Override this method to implement simulation update logic.
        /// </summary>
        /// <param name="deltaTime">Time since last update, scaled by time scale</param>
        protected virtual void OnSimulationUpdate(float deltaTime)
        {
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Override this method to handle simulation pause events.
        /// </summary>
        protected virtual void OnSimulationPaused()
        {
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Override this method to handle simulation resume events.
        /// </summary>
        protected virtual void OnSimulationResumed()
        {
            // Base implementation - override in derived classes
        }

        /// <summary>
        /// Called when the game's global time scale changes.
        /// </summary>
        /// <param name="newTimeScale">The new time scale value</param>
        public virtual void OnTimeScaleChanged(float newTimeScale)
        {
            if (!_participateInTimeScaling) return;

            OnTimeScaleChangedInternal(newTimeScale);
            LogDebug($"Time scale changed to {newTimeScale}");
        }

        /// <summary>
        /// Override this method to handle time scale changes.
        /// </summary>
        /// <param name="newTimeScale">The new time scale value</param>
        protected virtual void OnTimeScaleChangedInternal(float newTimeScale)
        {
            // Base implementation - override in derived classes
        }

        protected override void OnDestroy()
        {
            OnSimulationDestroy();
            base.OnDestroy();
        }

        /// <summary>
        /// Override this method to handle simulation cleanup.
        /// </summary>
        protected virtual void OnSimulationDestroy()
        {
            // Base implementation - override in derived classes
        }
    }
}