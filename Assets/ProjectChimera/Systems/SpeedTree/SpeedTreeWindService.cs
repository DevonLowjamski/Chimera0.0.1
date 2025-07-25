using UnityEngine;
using ProjectChimera.Data.SpeedTree;
using System.Collections.Generic;
using System.Collections;

#if UNITY_SPEEDTREE
using SpeedTree;
#endif

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// PC013-8d: SpeedTreeWindService extracted from AdvancedSpeedTreeManager
    /// Handles wind simulation, settings, and effects for cannabis plants.
    /// Manages dynamic wind responses and environmental wind conditions.
    /// </summary>
    public class SpeedTreeWindService
    {
        private readonly SpeedTreeWindSettings _defaultWindSettings;
        private readonly bool _enableWind;
        private readonly float _windUpdateFrequency;
        
        private Dictionary<int, SpeedTreeWind> _windComponents;
        private SpeedTreeWindSettings _currentWindSettings;
        private Coroutine _windUpdateCoroutine;
        private MonoBehaviour _coroutineHost;
        
        // Wind simulation parameters
        private float _windTime;
        private Vector3 _baseWindDirection;
        private float _baseWindStrength;
        private float _gustTimer;
        private float _gustDuration;
        private bool _isGusting;
        
        // Events for wind changes
        public System.Action<SpeedTreeWindSettings> OnWindSettingsChanged;
        public System.Action<float> OnWindStrengthChanged;
        public System.Action<Vector3> OnWindDirectionChanged;
        
        public SpeedTreeWindService(SpeedTreeWindSettings defaultSettings, MonoBehaviour coroutineHost,
            bool enableWind = true, float updateFrequency = 0.1f)
        {
            _defaultWindSettings = defaultSettings ?? new SpeedTreeWindSettings();
            _coroutineHost = coroutineHost;
            _enableWind = enableWind;
            _windUpdateFrequency = updateFrequency;
            
            _windComponents = new Dictionary<int, SpeedTreeWind>();
            _currentWindSettings = new SpeedTreeWindSettings
            {
                WindStrength = _defaultWindSettings.WindStrength,
                WindDirection = _defaultWindSettings.WindDirection,
                WindTurbulence = _defaultWindSettings.WindTurbulence,
                WindGustiness = _defaultWindSettings.WindGustiness,
                EnableBending = _defaultWindSettings.EnableBending,
                EnableFlutter = _defaultWindSettings.EnableFlutter
            };
            
            _baseWindDirection = _currentWindSettings.WindDirection;
            _baseWindStrength = _currentWindSettings.WindStrength;
            
            if (_enableWind && _coroutineHost != null)
            {
                StartWindSimulation();
            }
            
            Debug.Log($"[SpeedTreeWindService] Initialized with wind enabled: {_enableWind}");
        }
        
        /// <summary>
        /// Registers a plant instance for wind simulation.
        /// </summary>
        public void RegisterPlantForWind(SpeedTreePlantData plantData)
        {
            if (plantData?.Renderer == null || !_enableWind)
            {
                Debug.LogWarning("[SpeedTreeWindService] Cannot register plant: invalid data or wind disabled");
                return;
            }
            
            try
            {
                var windComponent = GetOrCreateWindComponent(plantData.Renderer);
                if (windComponent != null)
                {
                    _windComponents[plantData.InstanceId] = windComponent;
                    ApplyWindSettings(windComponent, _currentWindSettings);
                    
                    Debug.Log($"[SpeedTreeWindService] Registered plant {plantData.InstanceId} for wind simulation");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SpeedTreeWindService] Failed to register plant {plantData.InstanceId}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Unregisters a plant instance from wind simulation.
        /// </summary>
        public void UnregisterPlantFromWind(int instanceId)
        {
            if (_windComponents.TryGetValue(instanceId, out var windComponent))
            {
                _windComponents.Remove(instanceId);
                
                if (windComponent != null)
                {
                    Object.DestroyImmediate(windComponent);
                }
                
                Debug.Log($"[SpeedTreeWindService] Unregistered plant {instanceId} from wind simulation");
            }
        }
        
        /// <summary>
        /// Updates wind settings for all registered plants.
        /// </summary>
        public void UpdateWindSettings(SpeedTreeWindSettings newSettings)
        {
            if (newSettings == null)
            {
                Debug.LogWarning("[SpeedTreeWindService] Cannot update with null wind settings");
                return;
            }
            
            _currentWindSettings = newSettings;
            _baseWindDirection = newSettings.WindDirection;
            _baseWindStrength = newSettings.WindStrength;
            
            foreach (var windComponent in _windComponents.Values)
            {
                if (windComponent != null)
                {
                    ApplyWindSettings(windComponent, newSettings);
                }
            }
            
            OnWindSettingsChanged?.Invoke(newSettings);
            
            Debug.Log($"[SpeedTreeWindService] Updated wind settings: Strength={newSettings.WindStrength}, Direction={newSettings.WindDirection}");
        }
        
        /// <summary>
        /// Updates wind strength while maintaining other settings.
        /// </summary>
        public void UpdateWindStrength(float strength)
        {
            _currentWindSettings.WindStrength = Mathf.Clamp01(strength);
            _baseWindStrength = _currentWindSettings.WindStrength;
            
            foreach (var windComponent in _windComponents.Values)
            {
                if (windComponent != null)
                {
                    ApplyWindStrength(windComponent, _currentWindSettings.WindStrength);
                }
            }
            
            OnWindStrengthChanged?.Invoke(strength);
        }
        
        /// <summary>
        /// Updates wind direction while maintaining other settings.
        /// </summary>
        public void UpdateWindDirection(Vector3 direction)
        {
            _currentWindSettings.WindDirection = direction.normalized;
            _baseWindDirection = _currentWindSettings.WindDirection;
            
            foreach (var windComponent in _windComponents.Values)
            {
                if (windComponent != null)
                {
                    ApplyWindDirection(windComponent, _currentWindSettings.WindDirection);
                }
            }
            
            OnWindDirectionChanged?.Invoke(direction);
        }
        
        /// <summary>
        /// Gets current wind settings.
        /// </summary>
        public SpeedTreeWindSettings GetCurrentWindSettings()
        {
            return _currentWindSettings;
        }
        
        /// <summary>
        /// Gets wind statistics.
        /// </summary>
        public WindSimulationStats GetWindStats()
        {
            return new WindSimulationStats
            {
                RegisteredPlants = _windComponents.Count,
                WindEnabled = _enableWind,
                CurrentStrength = _currentWindSettings.WindStrength,
                CurrentDirection = _currentWindSettings.WindDirection,
                IsGusting = _isGusting,
                UpdateFrequency = _windUpdateFrequency
            };
        }
        
        /// <summary>
        /// Enables or disables wind simulation.
        /// </summary>
        public void SetWindEnabled(bool enabled)
        {
            if (enabled && !_enableWind && _coroutineHost != null)
            {
                StartWindSimulation();
            }
            else if (!enabled && _enableWind)
            {
                StopWindSimulation();
            }
        }
        
        /// <summary>
        /// Cleans up wind simulation resources.
        /// </summary>
        public void Cleanup()
        {
            StopWindSimulation();
            
            foreach (var windComponent in _windComponents.Values)
            {
                if (windComponent != null)
                {
                    Object.DestroyImmediate(windComponent);
                }
            }
            
            _windComponents.Clear();
            
            Debug.Log("[SpeedTreeWindService] Cleanup completed");
        }
        
        // Private methods
        private void StartWindSimulation()
        {
            if (_coroutineHost != null)
            {
                _windUpdateCoroutine = _coroutineHost.StartCoroutine(WindUpdateCoroutine());
                Debug.Log("[SpeedTreeWindService] Started wind simulation");
            }
        }
        
        private void StopWindSimulation()
        {
            if (_windUpdateCoroutine != null && _coroutineHost != null)
            {
                _coroutineHost.StopCoroutine(_windUpdateCoroutine);
                _windUpdateCoroutine = null;
                Debug.Log("[SpeedTreeWindService] Stopped wind simulation");
            }
        }
        
        private IEnumerator WindUpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_windUpdateFrequency);
                
                UpdateWindSimulation(Time.deltaTime);
            }
        }
        
        private void UpdateWindSimulation(float deltaTime)
        {
            _windTime += deltaTime;
            
            // Update gust system
            UpdateGustSystem(deltaTime);
            
            // Calculate dynamic wind parameters
            var dynamicSettings = CalculateDynamicWindSettings();
            
            // Apply to all registered plants
            foreach (var windComponent in _windComponents.Values)
            {
                if (windComponent != null)
                {
                    ApplyDynamicWindSettings(windComponent, dynamicSettings);
                }
            }
        }
        
        private void UpdateGustSystem(float deltaTime)
        {
            _gustTimer += deltaTime;
            
            if (!_isGusting)
            {
                // Check if we should start a gust
                var gustChance = _currentWindSettings.WindGustiness * deltaTime;
                if (Random.value < gustChance)
                {
                    StartGust();
                }
            }
            else
            {
                // Check if gust should end
                if (_gustTimer >= _gustDuration)
                {
                    EndGust();
                }
            }
        }
        
        private void StartGust()
        {
            _isGusting = true;
            _gustTimer = 0f;
            _gustDuration = Random.Range(0.5f, 2f);
            
            Debug.Log("[SpeedTreeWindService] Started wind gust");
        }
        
        private void EndGust()
        {
            _isGusting = false;
            _gustTimer = 0f;
            
            Debug.Log("[SpeedTreeWindService] Ended wind gust");
        }
        
        private SpeedTreeWindSettings CalculateDynamicWindSettings()
        {
            var dynamicSettings = new SpeedTreeWindSettings
            {
                WindDirection = _baseWindDirection,
                WindStrength = _baseWindStrength,
                WindTurbulence = _currentWindSettings.WindTurbulence,
                WindGustiness = _currentWindSettings.WindGustiness,
                EnableBending = _currentWindSettings.EnableBending,
                EnableFlutter = _currentWindSettings.EnableFlutter
            };
            
            // Apply turbulence to direction
            var turbulenceOffset = new Vector3(
                Mathf.Sin(_windTime * 2f) * _currentWindSettings.WindTurbulence,
                Mathf.Sin(_windTime * 1.5f) * _currentWindSettings.WindTurbulence * 0.5f,
                Mathf.Cos(_windTime * 1.8f) * _currentWindSettings.WindTurbulence
            );
            
            dynamicSettings.WindDirection = (_baseWindDirection + turbulenceOffset).normalized;
            
            // Apply gust effects
            if (_isGusting)
            {
                var gustIntensity = Mathf.Sin((_gustTimer / _gustDuration) * Mathf.PI);
                dynamicSettings.WindStrength = _baseWindStrength * (1f + gustIntensity * 0.5f);
            }
            
            // Add wind pulse effects
            var pulseEffect = Mathf.Sin(_windTime * _currentWindSettings.WindPulseFrequency) * _currentWindSettings.WindPulseMagnitude;
            dynamicSettings.WindStrength *= (1f + pulseEffect);
            
            return dynamicSettings;
        }
        
        private SpeedTreeWind GetOrCreateWindComponent(SpeedTreeRenderer renderer)
        {
            #if UNITY_SPEEDTREE
            var windComponent = renderer.GetComponent<SpeedTreeWind>();
            if (windComponent == null)
            {
                windComponent = renderer.gameObject.AddComponent<SpeedTreeWind>();
            }
            return windComponent;
            #else
            // Fallback for non-SpeedTree builds
            var windComponent = renderer.GetComponent<SpeedTreeWind>();
            if (windComponent == null)
            {
                windComponent = renderer.gameObject.AddComponent<SpeedTreeWind>();
            }
            return windComponent;
            #endif
        }
        
        private void ApplyWindSettings(SpeedTreeWind windComponent, SpeedTreeWindSettings settings)
        {
            if (windComponent == null || settings == null) return;
            
            windComponent.ApplyWindSettings(settings);
            
            #if UNITY_SPEEDTREE
            // Apply SpeedTree-specific wind settings
            ApplySpeedTreeWindSettings(windComponent, settings);
            #endif
        }
        
        private void ApplyWindStrength(SpeedTreeWind windComponent, float strength)
        {
            if (windComponent?.WindSettings == null) return;
            
            windComponent.WindSettings.WindStrength = strength;
            windComponent.ApplyWindSettings(windComponent.WindSettings);
        }
        
        private void ApplyWindDirection(SpeedTreeWind windComponent, Vector3 direction)
        {
            if (windComponent?.WindSettings == null) return;
            
            windComponent.WindSettings.WindDirection = direction;
            windComponent.ApplyWindSettings(windComponent.WindSettings);
        }
        
        private void ApplyDynamicWindSettings(SpeedTreeWind windComponent, SpeedTreeWindSettings settings)
        {
            if (windComponent == null || settings == null) return;
            
            // Apply the dynamic settings
            windComponent.ApplyWindSettings(settings);
        }
        
        #if UNITY_SPEEDTREE
        private void ApplySpeedTreeWindSettings(SpeedTreeWind windComponent, SpeedTreeWindSettings settings)
        {
            // Apply SpeedTree-specific wind parameters
            // This would interface with the actual SpeedTree wind system
        }
        #endif
    }
    
    /// <summary>
    /// Wind simulation statistics.
    /// </summary>
    [System.Serializable]
    public class WindSimulationStats
    {
        public int RegisteredPlants;
        public bool WindEnabled;
        public float CurrentStrength;
        public Vector3 CurrentDirection;
        public bool IsGusting;
        public float UpdateFrequency;
    }
}