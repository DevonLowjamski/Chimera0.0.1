using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Systems.Effects
{
    /// <summary>
    /// Extension methods and utilities for Unity particle systems.
    /// Provides enhanced functionality for dynamic particle configuration,
    /// performance optimization, and advanced visual effects.
    /// </summary>
    public static class ParticleSystemExtensions
    {
        /// <summary>
        /// Smoothly transitions particle emission rate over time
        /// </summary>
        public static void TransitionEmissionRate(this ParticleSystem particles, float targetRate, float duration)
        {
            if (particles != null && particles.gameObject.activeInHierarchy)
            {
                var behaviour = particles.gameObject.GetComponent<ParticleTransitionBehaviour>();
                if (behaviour == null)
                {
                    behaviour = particles.gameObject.AddComponent<ParticleTransitionBehaviour>();
                }
                behaviour.StartEmissionTransition(particles, targetRate, duration);
            }
        }
        
        /// <summary>
        /// Configures particle system for plant growth effects
        /// </summary>
        public static void ConfigureForPlantGrowth(this ParticleSystem particles, float healthPercentage)
        {
            var main = particles.main;
            var emission = particles.emission;
            var colorOverLifetime = particles.colorOverLifetime;
            
            // Adjust color based on plant health
            Color healthyColor = new Color(0.2f, 0.8f, 0.3f, 0.6f);
            Color unhealthyColor = new Color(0.8f, 0.8f, 0.2f, 0.4f);
            Color currentColor = Color.Lerp(unhealthyColor, healthyColor, healthPercentage / 100f);
            
            main.startColor = currentColor;
            main.startSize = Mathf.Lerp(0.05f, 0.15f, healthPercentage / 100f);
            main.startLifetime = Mathf.Lerp(2f, 5f, healthPercentage / 100f);
            
            emission.rateOverTime = Mathf.Lerp(2f, 10f, healthPercentage / 100f);
            
            // Configure shape for organic growth
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;
            shape.radiusThickness = 0.8f;
            
            // Add upward velocity
            var velocityOverLifetime = particles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.5f, new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 1f),
                new Keyframe(1f, 0.2f)
            ));
        }
        
        /// <summary>
        /// Configures particle system for water effects
        /// </summary>
        public static void ConfigureForWater(this ParticleSystem particles, float intensity = 1f)
        {
            var main = particles.main;
            var emission = particles.emission;
            var shape = particles.shape;
            var velocityOverLifetime = particles.velocityOverLifetime;
            var gravityModifier = particles.main;
            
            // Water droplet appearance
            main.startColor = new Color(0.3f, 0.6f, 1f, 0.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startLifetime = 2f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
            gravityModifier.gravityModifier = 0.5f;
            
            // Emission configuration
            emission.rateOverTime = 30f * intensity;
            
            // Shape configuration for spray pattern
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.1f;
            
            // Add collision for realistic water behavior
            var collision = particles.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.mode = ParticleSystemCollisionMode.Collision3D;
            collision.dampen = 0.3f;
            collision.bounce = 0.1f;
            collision.lifetimeLoss = 0.2f;
        }
        
        /// <summary>
        /// Configures particle system for steam effects
        /// </summary>
        public static void ConfigureForSteam(this ParticleSystem particles, float temperature = 25f)
        {
            var main = particles.main;
            var emission = particles.emission;
            var shape = particles.shape;
            var sizeOverLifetime = particles.sizeOverLifetime;
            var colorOverLifetime = particles.colorOverLifetime;
            
            // Steam appearance - more intense at higher temperatures
            float intensity = Mathf.Clamp01((temperature - 20f) / 15f); // Scale from 20-35°C
            
            main.startColor = new Color(1f, 1f, 1f, Mathf.Lerp(0.1f, 0.4f, intensity));
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startLifetime = Mathf.Lerp(3f, 8f, intensity);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.gravityModifier = -0.1f; // Slight upward drift
            
            // Emission based on temperature
            emission.rateOverTime = Mathf.Lerp(5f, 20f, intensity);
            
            // Shape for steam rising
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;
            
            // Grow and fade over lifetime
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(0.3f, 1f),
                new Keyframe(1f, 2f)
            ));
            
            colorOverLifetime.enabled = true;
            Gradient steamGradient = new Gradient();
            steamGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0.3f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = steamGradient;
        }
        
        /// <summary>
        /// Configures particle system for sparks effects
        /// </summary>
        public static void ConfigureForSparks(this ParticleSystem particles, float intensity = 1f)
        {
            var main = particles.main;
            var emission = particles.emission;
            var shape = particles.shape;
            var velocityOverLifetime = particles.velocityOverLifetime;
            var colorOverLifetime = particles.colorOverLifetime;
            
            // Spark appearance
            main.startColor = new Color(1f, 0.8f, 0.2f, 1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.03f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
            main.gravityModifier = 0.3f;
            
            // Burst emission for sparks
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, (short)(20 * intensity), (short)(40 * intensity))
            });
            
            // Spherical emission
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;
            
            // Sparks fade from bright to dark
            colorOverLifetime.enabled = true;
            Gradient sparkGradient = new Gradient();
            sparkGradient.SetKeys(
                new GradientColorKey[] 
                { 
                    new GradientColorKey(new Color(1f, 1f, 0.8f), 0f),
                    new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f),
                    new GradientColorKey(new Color(0.5f, 0.1f, 0f), 1f)
                },
                new GradientAlphaKey[] 
                { 
                    new GradientAlphaKey(1f, 0f), 
                    new GradientAlphaKey(0.8f, 0.3f), 
                    new GradientAlphaKey(0f, 1f) 
                }
            );
            colorOverLifetime.color = sparkGradient;
        }
        
        /// <summary>
        /// Configures particle system for dust effects
        /// </summary>
        public static void ConfigureForDust(this ParticleSystem particles, Vector3 windDirection, float windSpeed = 1f)
        {
            var main = particles.main;
            var emission = particles.emission;
            var shape = particles.shape;
            var velocityOverLifetime = particles.velocityOverLifetime;
            var forceOverLifetime = particles.forceOverLifetime;
            
            // Dust appearance
            main.startColor = new Color(0.6f, 0.5f, 0.4f, 0.4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.gravityModifier = 0.1f;
            
            // Continuous emission
            emission.rateOverTime = 15f;
            
            // Ground-level emission
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(2f, 0.1f, 2f);
            
            // Wind effect
            if (windDirection != Vector3.zero)
            {
                forceOverLifetime.enabled = true;
                forceOverLifetime.space = ParticleSystemSimulationSpace.World;
                forceOverLifetime.x = windDirection.x * windSpeed;
                forceOverLifetime.y = windDirection.y * windSpeed * 0.3f; // Less vertical movement
                forceOverLifetime.z = windDirection.z * windSpeed;
            }
        }
        
        /// <summary>
        /// Applies environmental conditions to particle system
        /// </summary>
        public static void ApplyEnvironmentalConditions(this ParticleSystem particles, 
            float temperature, float humidity, Vector3 windDirection, float windSpeed)
        {
            var main = particles.main;
            var forceOverLifetime = particles.forceOverLifetime;
            
            // Temperature affects particle movement speed
            float tempMultiplier = Mathf.Lerp(0.5f, 1.5f, (temperature - 15f) / 20f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(
                main.startSpeed.constantMin * tempMultiplier,
                main.startSpeed.constantMax * tempMultiplier
            );
            
            // Humidity affects particle size and lifetime
            float humidityMultiplier = Mathf.Lerp(0.8f, 1.2f, humidity / 100f);
            main.startSize = new ParticleSystem.MinMaxCurve(
                main.startSize.constantMin * humidityMultiplier,
                main.startSize.constantMax * humidityMultiplier
            );
            
            // Wind affects particle direction
            if (windSpeed > 0.1f)
            {
                forceOverLifetime.enabled = true;
                forceOverLifetime.space = ParticleSystemSimulationSpace.World;
                forceOverLifetime.x = windDirection.x * windSpeed * 0.5f;
                forceOverLifetime.y = windDirection.y * windSpeed * 0.2f;
                forceOverLifetime.z = windDirection.z * windSpeed * 0.5f;
            }
        }
        
        /// <summary>
        /// Optimizes particle system for performance
        /// </summary>
        public static void OptimizeForPerformance(this ParticleSystem particles, EffectQuality quality)
        {
            var main = particles.main;
            var emission = particles.emission;
            
            switch (quality)
            {
                case EffectQuality.Low:
                    main.maxParticles = Mathf.Min(main.maxParticles, 50);
                    emission.rateOverTime = emission.rateOverTime.constant * 0.5f;
                    break;
                case EffectQuality.Medium:
                    main.maxParticles = Mathf.Min(main.maxParticles, 100);
                    emission.rateOverTime = emission.rateOverTime.constant * 0.75f;
                    break;
                case EffectQuality.High:
                    main.maxParticles = Mathf.Min(main.maxParticles, 200);
                    // No reduction
                    break;
                case EffectQuality.Ultra:
                    main.maxParticles = Mathf.Min(main.maxParticles, 500);
                    emission.rateOverTime = emission.rateOverTime.constant * 1.25f;
                    break;
            }
            
            // Disable expensive modules for lower quality
            if (quality <= EffectQuality.Medium)
            {
                var collision = particles.collision;
                collision.enabled = false;
                
                var lights = particles.lights;
                lights.enabled = false;
            }
        }
        
        /// <summary>
        /// Creates a pulsing effect for emphasis
        /// </summary>
        public static void CreatePulseEffect(this ParticleSystem particles, float pulseRate = 1f)
        {
            var behaviour = particles.gameObject.GetComponent<ParticlePulseBehaviour>();
            if (behaviour == null)
            {
                behaviour = particles.gameObject.AddComponent<ParticlePulseBehaviour>();
            }
            behaviour.StartPulsing(particles, pulseRate);
        }
        
        /// <summary>
        /// Synchronizes multiple particle systems
        /// </summary>
        public static void SynchronizeWith(this ParticleSystem primary, params ParticleSystem[] others)
        {
            if (others == null || others.Length == 0) return;
            
            var behaviour = primary.gameObject.GetComponent<ParticleSyncBehaviour>();
            if (behaviour == null)
            {
                behaviour = primary.gameObject.AddComponent<ParticleSyncBehaviour>();
            }
            behaviour.SynchronizeSystems(primary, others);
        }
    }
    
    /// <summary>
    /// Helper component for smooth particle transitions
    /// </summary>
    public class ParticleTransitionBehaviour : MonoBehaviour
    {
        private Coroutine _transitionCoroutine;
        
        public void StartEmissionTransition(ParticleSystem particles, float targetRate, float duration)
        {
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }
            _transitionCoroutine = StartCoroutine(TransitionEmission(particles, targetRate, duration));
        }
        
        private IEnumerator TransitionEmission(ParticleSystem particles, float targetRate, float duration)
        {
            var emission = particles.emission;
            float startRate = emission.rateOverTime.constant;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float currentRate = Mathf.Lerp(startRate, targetRate, progress);
                
                emission.rateOverTime = currentRate;
                
                yield return null;
            }
            
            emission.rateOverTime = targetRate;
            _transitionCoroutine = null;
        }
    }
    
    /// <summary>
    /// Helper component for pulsing particle effects
    /// </summary>
    public class ParticlePulseBehaviour : MonoBehaviour
    {
        private Coroutine _pulseCoroutine;
        
        public void StartPulsing(ParticleSystem particles, float pulseRate)
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
            }
            _pulseCoroutine = StartCoroutine(PulseEffect(particles, pulseRate));
        }
        
        private IEnumerator PulseEffect(ParticleSystem particles, float pulseRate)
        {
            var emission = particles.emission;
            float baseRate = emission.rateOverTime.constant;
            
            while (true)
            {
                float time = Time.time * pulseRate;
                float multiplier = 1f + 0.5f * Mathf.Sin(time);
                emission.rateOverTime = baseRate * multiplier;
                
                yield return null;
            }
        }
        
        public void StopPulsing()
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }
        }
    }
    
    /// <summary>
    /// Helper component for synchronizing multiple particle systems
    /// </summary>
    public class ParticleSyncBehaviour : MonoBehaviour
    {
        private ParticleSystem[] _syncedSystems;
        
        public void SynchronizeSystems(ParticleSystem primary, ParticleSystem[] others)
        {
            _syncedSystems = new ParticleSystem[others.Length + 1];
            _syncedSystems[0] = primary;
            for (int i = 0; i < others.Length; i++)
            {
                _syncedSystems[i + 1] = others[i];
            }
        }
        
        public void PlayAll()
        {
            if (_syncedSystems != null)
            {
                foreach (var system in _syncedSystems)
                {
                    if (system != null) system.Play();
                }
            }
        }
        
        public void StopAll()
        {
            if (_syncedSystems != null)
            {
                foreach (var system in _syncedSystems)
                {
                    if (system != null) system.Stop();
                }
            }
        }
        
        public void PauseAll()
        {
            if (_syncedSystems != null)
            {
                foreach (var system in _syncedSystems)
                {
                    if (system != null) system.Pause();
                }
            }
        }
    }
}