using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Systems.Professional
{
    /// <summary>
    /// Advanced Visual Effects System for Professional Polish
    /// Manages particle effects, post-processing, and visual enhancements
    /// </summary>
    public class VisualEffectsSystem
    {
        private bool _isInitialized = false;
        private bool _effectsEnabled = true;
        private bool _particleEffectsEnabled = true;
        private bool _advancedEffectsEnabled = true;
        private float _particleDensity = 1.0f;
        private float _animationComplexity = 1.0f;
        
        private Dictionary<string, ParticleSystem> _particleSystems = new Dictionary<string, ParticleSystem>();
        private Dictionary<string, VisualEffect> _visualEffects = new Dictionary<string, VisualEffect>();
        private List<EffectPool> _effectPools = new List<EffectPool>();
        
        public void Initialize(bool effectsEnabled, bool particleEffectsEnabled)
        {
            _effectsEnabled = effectsEnabled;
            _particleEffectsEnabled = particleEffectsEnabled;
            
            InitializeEffectPools();
            LoadEffectPresets();
            
            _isInitialized = true;
            Debug.Log("Visual Effects System initialized");
        }
        
        public void UpdateEffects()
        {
            if (!_isInitialized || !_effectsEnabled) return;
            
            // Update effect pools
            foreach (var pool in _effectPools)
            {
                pool.UpdatePool();
            }
            
            // Update particle systems
            UpdateParticleSystems();
            
            // Update visual effects
            UpdateVisualEffects();
        }
        
        public void SetEnabled(bool enabled)
        {
            _effectsEnabled = enabled;
            
            // Enable/disable all particle systems
            foreach (var particleSystem in _particleSystems.Values)
            {
                if (particleSystem != null)
                {
                    if (enabled)
                        particleSystem.Play();
                    else
                        particleSystem.Stop();
                }
            }
        }
        
        public void SetParticleEffectsEnabled(bool enabled)
        {
            _particleEffectsEnabled = enabled;
            
            foreach (var particleSystem in _particleSystems.Values)
            {
                if (particleSystem != null)
                {
                    particleSystem.gameObject.SetActive(enabled);
                }
            }
        }
        
        public void SetAdvancedEffectsEnabled(bool enabled)
        {
            _advancedEffectsEnabled = enabled;
            
            foreach (var effect in _visualEffects.Values)
            {
                if (effect != null)
                {
                    effect.SetAdvancedEnabled(enabled);
                }
            }
        }
        
        public void SetParticleDensity(float density)
        {
            _particleDensity = Mathf.Clamp01(density);
            
            foreach (var particleSystem in _particleSystems.Values)
            {
                if (particleSystem != null)
                {
                    var emission = particleSystem.emission;
                    emission.rateOverTime = emission.rateOverTime.constant * _particleDensity;
                }
            }
        }
        
        public void OnQualityChanged(QualityLevel quality)
        {
            switch (quality)
            {
                case QualityLevel.Low:
                    SetParticleDensity(0.3f);
                    SetAdvancedEffectsEnabled(false);
                    break;
                case QualityLevel.Medium:
                    SetParticleDensity(0.6f);
                    SetAdvancedEffectsEnabled(true);
                    break;
                case QualityLevel.High:
                    SetParticleDensity(0.8f);
                    SetAdvancedEffectsEnabled(true);
                    break;
                case QualityLevel.Ultra:
                    SetParticleDensity(1.0f);
                    SetAdvancedEffectsEnabled(true);
                    break;
            }
        }
        
        public void PlayEffect(string effectName, Vector3 position, Quaternion rotation)
        {
            if (!_effectsEnabled) return;
            
            var pool = _effectPools.FirstOrDefault(p => p.EffectName == effectName);
            if (pool != null)
            {
                pool.PlayEffect(position, rotation);
            }
        }
        
        public void StopEffect(string effectName)
        {
            if (_particleSystems.ContainsKey(effectName))
            {
                _particleSystems[effectName]?.Stop();
            }
            
            if (_visualEffects.ContainsKey(effectName))
            {
                _visualEffects[effectName]?.Stop();
            }
        }
        
        public void Cleanup()
        {
            foreach (var pool in _effectPools)
            {
                pool.Cleanup();
            }
            
            _particleSystems.Clear();
            _visualEffects.Clear();
            _effectPools.Clear();
            
            _isInitialized = false;
        }
        
        private void InitializeEffectPools()
        {
            // Create effect pools for common effects
            _effectPools.Add(new EffectPool("GrowthSparkles", 10));
            _effectPools.Add(new EffectPool("HarvestGlow", 5));
            _effectPools.Add(new EffectPool("PestDefeatBurst", 15));
            _effectPools.Add(new EffectPool("LevelUpEffect", 3));
            _effectPools.Add(new EffectPool("AchievementUnlock", 5));
        }
        
        private void LoadEffectPresets()
        {
            // Load effect presets from resources
            var presets = Resources.LoadAll<GameObject>("Effects/");
            foreach (var preset in presets)
            {
                var particleSystem = preset.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    _particleSystems[preset.name] = particleSystem;
                }
                
                var visualEffect = preset.GetComponent<VisualEffect>();
                if (visualEffect != null)
                {
                    _visualEffects[preset.name] = visualEffect;
                }
            }
        }
        
        private void UpdateParticleSystems()
        {
            foreach (var kvp in _particleSystems.ToList())
            {
                if (kvp.Value == null)
                {
                    _particleSystems.Remove(kvp.Key);
                }
            }
        }
        
        private void UpdateVisualEffects()
        {
            foreach (var effect in _visualEffects.Values)
            {
                effect?.UpdateEffect();
            }
        }
    }
    
    // Supporting classes
    public class EffectPool
    {
        public string EffectName { get; private set; }
        private Queue<GameObject> _pool = new Queue<GameObject>();
        private List<GameObject> _activeEffects = new List<GameObject>();
        private GameObject _prefab;
        private int _poolSize;
        
        public EffectPool(string effectName, int poolSize)
        {
            EffectName = effectName;
            _poolSize = poolSize;
            
            // Load prefab
            _prefab = Resources.Load<GameObject>($"Effects/{effectName}");
            
            // Pre-instantiate pool objects
            for (int i = 0; i < _poolSize; i++)
            {
                if (_prefab != null)
                {
                    var obj = GameObject.Instantiate(_prefab);
                    obj.SetActive(false);
                    _pool.Enqueue(obj);
                }
            }
        }
        
        public void PlayEffect(Vector3 position, Quaternion rotation)
        {
            GameObject effect = null;
            
            if (_pool.Count > 0)
            {
                effect = _pool.Dequeue();
            }
            else if (_prefab != null)
            {
                effect = GameObject.Instantiate(_prefab);
            }
            
            if (effect != null)
            {
                effect.transform.position = position;
                effect.transform.rotation = rotation;
                effect.SetActive(true);
                _activeEffects.Add(effect);
                
                // Auto-return to pool after duration
                var autoReturn = effect.GetComponent<AutoReturnToPool>();
                if (autoReturn == null)
                {
                    autoReturn = effect.AddComponent<AutoReturnToPool>();
                }
                autoReturn.Initialize(this, 5f); // 5 second default duration
            }
        }
        
        public void ReturnToPool(GameObject effect)
        {
            if (_activeEffects.Contains(effect))
            {
                _activeEffects.Remove(effect);
                effect.SetActive(false);
                _pool.Enqueue(effect);
            }
        }
        
        public void UpdatePool()
        {
            // Remove null references
            _activeEffects.RemoveAll(e => e == null);
        }
        
        public void Cleanup()
        {
            foreach (var effect in _activeEffects)
            {
                if (effect != null)
                {
                    GameObject.DestroyImmediate(effect);
                }
            }
            
            while (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                if (obj != null)
                {
                    GameObject.DestroyImmediate(obj);
                }
            }
            
            _activeEffects.Clear();
        }
    }
    
    public class VisualEffect
    {
        private bool _advancedEnabled = true;
        
        public void SetAdvancedEnabled(bool enabled)
        {
            _advancedEnabled = enabled;
        }
        
        public void UpdateEffect()
        {
            // Update visual effect logic
        }
        
        public void Stop()
        {
            // Stop visual effect
        }
    }
    
    // Component for auto-returning effects to pool
    public class AutoReturnToPool : MonoBehaviour
    {
        private EffectPool _pool;
        private float _duration;
        private float _timer;
        
        public void Initialize(EffectPool pool, float duration)
        {
            _pool = pool;
            _duration = duration;
            _timer = 0f;
        }
        
        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _duration)
            {
                _pool?.ReturnToPool(gameObject);
            }
        }
    }
}