using UnityEngine;
using UnityEngine.VFX;
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Construction;
using ProjectChimera.Systems.Cultivation;
using EnvironmentSystems = ProjectChimera.Systems.Environment;
using ProjectChimera.Systems.Construction;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
// Explicit alias for Data layer PlantGrowthStage to resolve ambiguity
using DataPlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;

// Explicit namespace aliases to resolve ambiguity
using DataEnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
using DataConstructionIssue = ProjectChimera.Data.Construction.ConstructionIssue;
using EnvironmentalAlert = ProjectChimera.Systems.Environment.EnvironmentalAlert;

namespace ProjectChimera.Systems.Effects
{
    /// <summary>
    /// Advanced effects manager for Project Chimera featuring particle systems,
    /// visual effects, feedback systems, and dynamic visual responses to gameplay.
    /// Uses Unity VFX Graph for high-performance particle systems and visual effects.
    /// </summary>
    public class AdvancedEffectsManager : ChimeraManager
    {
        [Header("VFX Configuration")]
        [SerializeField] private VisualEffectAsset _plantGrowthVFX;
        [SerializeField] private VisualEffectAsset _wateringVFX;
        [SerializeField] private VisualEffectAsset _harvestVFX;
        [SerializeField] private VisualEffectAsset _constructionVFX;
        [SerializeField] private VisualEffectAsset _environmentalVFX;
        [SerializeField] private VisualEffectAsset _sparksVFX;
        [SerializeField] private VisualEffectAsset _steamVFX;
        [SerializeField] private VisualEffectAsset _dustVFX;
        
        [Header("Particle System Templates")]
        [SerializeField] private ParticleSystem _growthParticleTemplate;
        [SerializeField] private ParticleSystem _waterDropletsTemplate;
        [SerializeField] private ParticleSystem _pollenTemplate;
        [SerializeField] private ParticleSystem _steamTemplate;
        [SerializeField] private ParticleSystem _sparkTemplate;
        [SerializeField] private ParticleSystem _dustTemplate;
        
        [Header("Effects Settings")]
        [SerializeField] private bool _enableHighQualityEffects = true;
        [SerializeField] private bool _enableEnvironmentalEffects = true;
        [SerializeField] private bool _enablePlantEffects = true;
        [SerializeField] private bool _enableConstructionEffects = true;
        [SerializeField] private bool _enableUIEffects = true;
        [SerializeField] private float _effectsQualityScale = 1f;
        [SerializeField] private int _maxConcurrentEffects = 50;
        
        [Header("Performance Settings")]
        [SerializeField] private LayerMask _effectsLayers = -1;
        [SerializeField] private float _cullingDistance = 100f;
        [SerializeField] private int _particlePoolSize = 20;
        [SerializeField] private bool _enableAdaptiveQuality = true;
        
        // Effect Management
        private Dictionary<string, VisualEffect> _activeVFXEffects = new Dictionary<string, VisualEffect>();
        private Dictionary<string, ParticleSystem> _activeParticleEffects = new Dictionary<string, ParticleSystem>();
        private Queue<ParticleSystem> _particleSystemPool = new Queue<ParticleSystem>();
        private Queue<VisualEffect> _vfxPool = new Queue<VisualEffect>();
        
        // Environmental Effects
        private Dictionary<string, EnvironmentalEffectInstance> _environmentalEffects = new Dictionary<string, EnvironmentalEffectInstance>();
        private List<WeatherEffectController> _weatherEffects = new List<WeatherEffectController>();
        
        // Plant Effects
        private Dictionary<string, PlantEffectController> _plantEffects = new Dictionary<string, PlantEffectController>();
        private PlantEffectLibrary _plantEffectLibrary;
        
        // Construction Effects
        private List<ConstructionEffectController> _constructionEffects = new List<ConstructionEffectController>();
        
        // UI Effects
        private UIEffectsController _uiEffectsController;
        
        // Performance Monitoring
        private EffectsPerformanceMetrics _performanceMetrics;
        private float _lastPerformanceUpdate = 0f;
        private int _effectsThisFrame = 0;
        private int _maxEffectsPerFrame = 10;
        
        // System References
        // PHASE 1 PC-007: Removed direct manager references - using event-driven communication only
        // System connections now handled through GameEventSO channels instead of direct references
        private Camera _mainCamera;
        
        // Events
        public System.Action<EffectType, Vector3> OnEffectTriggered;
        public System.Action<string> OnEffectCompleted;
        public System.Action<EffectsPerformanceMetrics> OnPerformanceUpdate;
        
        // Properties
        public EffectsPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        public bool EffectsEnabled => _enableHighQualityEffects;
        public float EffectsQuality => _effectsQualityScale;
        
        protected override void OnManagerInitialize()
        {
            InitializeEffectsSystem();
            SetupParticlePools();
            ConnectToGameSystems();
            InitializePerformanceMonitoring();
            LogInfo("Advanced Effects Manager initialized");
        }
        
        private void Update()
        {
            UpdateEffectsSystem();
            UpdatePerformanceMetrics();
            UpdateAdaptiveQuality();
            CleanupFinishedEffects();
            _effectsThisFrame = 0;
        }
        
        #region Initialization
        
        private void InitializeEffectsSystem()
        {
            // Find main camera if not assigned
            if (_mainCamera == null)
            {
                _mainCamera = FindObjectOfType<Camera>();
            }
            
            // Initialize effect libraries
            _plantEffectLibrary = new PlantEffectLibrary();
            _uiEffectsController = new UIEffectsController();
            
            // Initialize performance metrics
            _performanceMetrics = new EffectsPerformanceMetrics
            {
                MaxConcurrentEffects = _maxConcurrentEffects,
                EffectsQuality = _effectsQualityScale,
                AdaptiveQualityEnabled = _enableAdaptiveQuality
            };
        }
        
        private void SetupParticlePools()
        {
            // Create particle system pool
            for (int i = 0; i < _particlePoolSize; i++)
            {
                if (_growthParticleTemplate != null)
                {
                    var particles = CreatePooledParticleSystem("PooledParticles_" + i, _growthParticleTemplate);
                    _particleSystemPool.Enqueue(particles);
                }
            }
            
            LogInfo($"Created particle system pool with {_particlePoolSize} systems");
        }
        
        private ParticleSystem CreatePooledParticleSystem(string name, ParticleSystem template)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            go.SetActive(false);
            
            var particles = go.AddComponent<ParticleSystem>();
            
            // Copy settings from template
            if (template != null)
            {
                var main = particles.main;
                var templateMain = template.main;
                main.startLifetime = templateMain.startLifetime;
                main.startSpeed = templateMain.startSpeed;
                main.startSize = templateMain.startSize;
                main.startColor = templateMain.startColor;
                main.maxParticles = templateMain.maxParticles;
                
                var emission = particles.emission;
                var templateEmission = template.emission;
                emission.rateOverTime = templateEmission.rateOverTime;
            }
            
            return particles;
        }
        
        private void ConnectToGameSystems()
        {
            // PHASE 1 PC-007: Removed direct manager references - using event-driven communication only
            // Direct manager access violates architectural principles established in Phase 1
            // All system communication now uses GameEventSO channels
            
            ConnectSystemEvents();
        }
        
        private void ConnectSystemEvents()
        {
            // PHASE 1 PC-007: Event connections now handled through GameEventSO channels
            // Direct manager event subscriptions removed - will be replaced with event-driven communication
            
            // TODO Phase 2: Subscribe to GameEventSO channels for:
            // - Plant events (OnPlantAdded, OnPlantStageChanged, OnPlantHarvested, OnPlantWatered)
            // - Environmental events (OnConditionsChanged, OnAlertTriggered)  
            // - Construction events (OnProjectStarted, OnConstructionProgress, OnProjectCompleted)
            
            LogInfo("Advanced Effects Manager: Event system connections deferred to Phase 2 event channel implementation");
        }
        
        private void InitializePerformanceMonitoring()
        {
            InvokeRepeating(nameof(UpdateDetailedPerformanceMetrics), 1f, 1f);
        }
        
        #endregion
        
        #region Audio Effects
        
        public void PlayAudioEffect(string effectName, Vector3 position, float volume = 1f)
        {
            // Play audio effect at specified position with given volume
            // This is a placeholder implementation for audio effect integration
            LogInfo($"Playing audio effect '{effectName}' at position {position} with volume {volume}");
            
            // TODO: Integrate with actual audio system when available
            // Example implementation:
            // if (_audioManager != null)
            // {
            //     _audioManager.PlaySpatialAudio(effectName, position, volume);
            // }
        }
        
        #endregion
        
        #region Effect Triggering
        
        public void PlayEffect(EffectType effectType, Vector3 position, Transform parent = null, float duration = 5f)
        {
            if (!_enableHighQualityEffects || _effectsThisFrame >= _maxEffectsPerFrame)
            {
                return;
            }
            
            if (IsPositionCulled(position))
            {
                return;
            }
            
            switch (effectType)
            {
                case EffectType.PlantGrowth:
                    PlayPlantGrowthEffect(position, parent, duration);
                    break;
                case EffectType.PlantWatering:
                    PlayWateringEffect(position, parent, duration);
                    break;
                case EffectType.PlantHarvest:
                    PlayHarvestEffect(position, parent, duration);
                    break;
                case EffectType.Construction:
                    PlayConstructionEffect(position, parent, duration);
                    break;
                case EffectType.Environmental:
                    PlayEnvironmentalEffect(position, parent, duration);
                    break;
                case EffectType.Sparks:
                    PlaySparksEffect(position, parent, duration);
                    break;
                case EffectType.Steam:
                    PlaySteamEffect(position, parent, duration);
                    break;
                case EffectType.Dust:
                    PlayDustEffect(position, parent, duration);
                    break;
            }
            
            _effectsThisFrame++;
            OnEffectTriggered?.Invoke(effectType, position);
        }
        
        private void PlayPlantGrowthEffect(Vector3 position, Transform parent, float duration)
        {
            if (_enablePlantEffects && _plantGrowthVFX != null)
            {
                var vfx = GetPooledVFXEffect();
                if (vfx != null)
                {
                    SetupVFXEffect(vfx, _plantGrowthVFX, position, parent, duration);
                    
                    // Configure plant growth specific parameters
                    vfx.SetFloat("GrowthIntensity", _effectsQualityScale);
                    vfx.SetVector3("WindDirection", Vector3.forward);
                    vfx.Play();
                    
                    _activeVFXEffects[$"growth_{vfx.GetInstanceID()}"] = vfx;
                }
            }
            
            // Fallback to particle system
            if (_growthParticleTemplate != null)
            {
                var particles = GetPooledParticleSystem();
                if (particles != null)
                {
                    SetupParticleSystem(particles, position, parent, duration);
                    ConfigureGrowthParticles(particles);
                }
            }
        }
        
        private void PlayWateringEffect(Vector3 position, Transform parent, float duration)
        {
            if (_waterDropletsTemplate != null)
            {
                var particles = GetPooledParticleSystem();
                if (particles != null)
                {
                    SetupParticleSystem(particles, position, parent, duration);
                    ConfigureWateringParticles(particles);
                }
            }
        }
        
        private void PlayHarvestEffect(Vector3 position, Transform parent, float duration)
        {
            if (_harvestVFX != null)
            {
                var vfx = GetPooledVFXEffect();
                if (vfx != null)
                {
                    SetupVFXEffect(vfx, _harvestVFX, position, parent, duration);
                    vfx.SetFloat("HarvestIntensity", 1f);
                    vfx.Play();
                    
                    _activeVFXEffects[$"harvest_{vfx.GetInstanceID()}"] = vfx;
                }
            }
        }
        
        private void PlayConstructionEffect(Vector3 position, Transform parent, float duration)
        {
            if (_enableConstructionEffects && _constructionVFX != null)
            {
                var vfx = GetPooledVFXEffect();
                if (vfx != null)
                {
                    SetupVFXEffect(vfx, _constructionVFX, position, parent, duration);
                    vfx.SetFloat("ConstructionIntensity", 0.8f);
                    vfx.Play();
                    
                    _activeVFXEffects[$"construction_{vfx.GetInstanceID()}"] = vfx;
                }
            }
            
            // Add dust particles for construction
            PlayDustEffect(position, parent, duration * 0.5f);
        }
        
        private void PlayEnvironmentalEffect(Vector3 position, Transform parent, float duration)
        {
            if (_enableEnvironmentalEffects && _environmentalVFX != null)
            {
                var vfx = GetPooledVFXEffect();
                if (vfx != null)
                {
                    SetupVFXEffect(vfx, _environmentalVFX, position, parent, duration);
                    vfx.Play();
                    
                    _activeVFXEffects[$"environmental_{vfx.GetInstanceID()}"] = vfx;
                }
            }
        }
        
        private void PlaySparksEffect(Vector3 position, Transform parent, float duration)
        {
            if (_sparkTemplate != null)
            {
                var particles = GetPooledParticleSystem();
                if (particles != null)
                {
                    SetupParticleSystem(particles, position, parent, duration);
                    ConfigureSparksParticles(particles);
                }
            }
        }
        
        private void PlaySteamEffect(Vector3 position, Transform parent, float duration)
        {
            if (_steamTemplate != null)
            {
                var particles = GetPooledParticleSystem();
                if (particles != null)
                {
                    SetupParticleSystem(particles, position, parent, duration);
                    ConfigureSteamParticles(particles);
                }
            }
        }
        
        private void PlayDustEffect(Vector3 position, Transform parent, float duration)
        {
            if (_dustTemplate != null)
            {
                var particles = GetPooledParticleSystem();
                if (particles != null)
                {
                    SetupParticleSystem(particles, position, parent, duration);
                    ConfigureDustParticles(particles);
                }
            }
        }
        
        #endregion
        
        #region Plant-Specific Effects
        
        public void CreatePlantEffectController(InteractivePlantComponent plant)
        {
            if (!_enablePlantEffects || plant == null) return;
            
            string plantId = plant.GetInstanceID().ToString();
            
            if (!_plantEffects.ContainsKey(plantId))
            {
                var controller = new PlantEffectController(plant, this);
                _plantEffects[plantId] = controller;
                controller.Initialize();
            }
        }
        
        public void RemovePlantEffectController(string plantId)
        {
            if (_plantEffects.TryGetValue(plantId, out var controller))
            {
                controller.Cleanup();
                _plantEffects.Remove(plantId);
            }
        }
        
        public void UpdatePlantEffects()
        {
            foreach (var controller in _plantEffects.Values)
            {
                controller.Update();
            }
        }
        
        #endregion
        
        #region Environmental Effects
        
        public void CreateEnvironmentalEffect(string effectId, EnvironmentalEffectType effectType, Vector3 position, float radius = 5f)
        {
            if (!_enableEnvironmentalEffects) return;
            
            var effectInstance = new EnvironmentalEffectInstance
            {
                EffectId = effectId,
                EffectType = effectType,
                Position = position,
                Radius = radius,
                IsActive = true,
                StartTime = Time.time
            };
            
            _environmentalEffects[effectId] = effectInstance;
            StartCoroutine(ManageEnvironmentalEffect(effectInstance));
        }
        
        private IEnumerator ManageEnvironmentalEffect(EnvironmentalEffectInstance effect)
        {
            while (effect.IsActive)
            {
                switch (effect.EffectType)
                {
                    case EnvironmentalEffectType.Heat:
                        UpdateHeatEffect(effect);
                        break;
                    case EnvironmentalEffectType.Humidity:
                        UpdateHumidityEffect(effect);
                        break;
                    case EnvironmentalEffectType.AirFlow:
                        UpdateAirFlowEffect(effect);
                        break;
                    case EnvironmentalEffectType.Light:
                        UpdateLightEffect(effect);
                        break;
                }
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private void UpdateHeatEffect(EnvironmentalEffectInstance effect)
        {
            // Create heat shimmer effect
            if (Time.time - effect.LastParticleSpawn > 2f)
            {
                PlayEffect(EffectType.Steam, effect.Position, null, 3f);
                effect.LastParticleSpawn = Time.time;
            }
        }
        
        private void UpdateHumidityEffect(EnvironmentalEffectInstance effect)
        {
            // Create water vapor effects
            if (Time.time - effect.LastParticleSpawn > 3f)
            {
                PlaySteamEffect(effect.Position + Vector3.up * 0.5f, null, 4f);
                effect.LastParticleSpawn = Time.time;
            }
        }
        
        private void UpdateAirFlowEffect(EnvironmentalEffectInstance effect)
        {
            // Create air movement particles
            if (Time.time - effect.LastParticleSpawn > 1f)
            {
                PlayDustEffect(effect.Position, null, 2f);
                effect.LastParticleSpawn = Time.time;
            }
        }
        
        private void UpdateLightEffect(EnvironmentalEffectInstance effect)
        {
            // Light effects are handled by the lighting system
        }
        
        #endregion
        
        #region Effect Pooling and Management
        
        private VisualEffect GetPooledVFXEffect()
        {
            if (_vfxPool.Count > 0)
            {
                return _vfxPool.Dequeue();
            }
            
            // Create new VFX if pool is empty
            var go = new GameObject("PooledVFX");
            go.transform.SetParent(transform);
            go.SetActive(false);
            
            return go.AddComponent<VisualEffect>();
        }
        
        private ParticleSystem GetPooledParticleSystem()
        {
            if (_particleSystemPool.Count > 0)
            {
                var particles = _particleSystemPool.Dequeue();
                particles.gameObject.SetActive(true);
                return particles;
            }
            
            // Create new particle system if pool is empty
            if (_growthParticleTemplate != null)
            {
                return CreatePooledParticleSystem("DynamicParticles", _growthParticleTemplate);
            }
            
            return null;
        }
        
        private void SetupVFXEffect(VisualEffect vfx, VisualEffectAsset asset, Vector3 position, Transform parent, float duration)
        {
            vfx.visualEffectAsset = asset;
            vfx.transform.position = position;
            vfx.transform.SetParent(parent);
            vfx.gameObject.SetActive(true);
            
            StartCoroutine(ReturnVFXToPoolAfterDelay(vfx, duration));
        }
        
        private void SetupParticleSystem(ParticleSystem particles, Vector3 position, Transform parent, float duration)
        {
            particles.transform.position = position;
            particles.transform.SetParent(parent);
            particles.gameObject.SetActive(true);
            particles.Play();
            
            _activeParticleEffects[$"particles_{particles.GetInstanceID()}"] = particles;
            StartCoroutine(ReturnParticlesToPoolAfterDelay(particles, duration));
        }
        
        private IEnumerator ReturnVFXToPoolAfterDelay(VisualEffect vfx, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            vfx.Stop();
            vfx.gameObject.SetActive(false);
            vfx.transform.SetParent(transform);
            _vfxPool.Enqueue(vfx);
            
            // Remove from active effects
            var key = _activeVFXEffects.FirstOrDefault(kvp => kvp.Value == vfx).Key;
            if (!string.IsNullOrEmpty(key))
            {
                _activeVFXEffects.Remove(key);
                OnEffectCompleted?.Invoke(key);
            }
        }
        
        private IEnumerator ReturnParticlesToPoolAfterDelay(ParticleSystem particles, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            particles.Stop();
            particles.gameObject.SetActive(false);
            particles.transform.SetParent(transform);
            _particleSystemPool.Enqueue(particles);
            
            // Remove from active effects
            var key = $"particles_{particles.GetInstanceID()}";
            if (_activeParticleEffects.ContainsKey(key))
            {
                _activeParticleEffects.Remove(key);
                OnEffectCompleted?.Invoke(key);
            }
        }
        
        #endregion
        
        #region Particle Configuration
        
        private void ConfigureGrowthParticles(ParticleSystem particles)
        {
            var main = particles.main;
            main.startColor = new Color(0.2f, 0.8f, 0.3f, 0.6f);
            main.startSize = 0.1f;
            main.startLifetime = 3f;
            main.startSpeed = 0.5f;
            
            var emission = particles.emission;
            emission.rateOverTime = 5f * _effectsQualityScale;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
        }
        
        private void ConfigureWateringParticles(ParticleSystem particles)
        {
            var main = particles.main;
            main.startColor = new Color(0.3f, 0.6f, 1f, 0.8f);
            main.startSize = 0.05f;
            main.startLifetime = 2f;
            main.startSpeed = 2f;
            
            var emission = particles.emission;
            emission.rateOverTime = 20f * _effectsQualityScale;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
        }
        
        private void ConfigureSparksParticles(ParticleSystem particles)
        {
            var main = particles.main;
            main.startColor = new Color(1f, 0.8f, 0.2f, 1f);
            main.startSize = 0.02f;
            main.startLifetime = 1f;
            main.startSpeed = 3f;
            
            var emission = particles.emission;
            emission.rateOverTime = 50f * _effectsQualityScale;
        }
        
        private void ConfigureSteamParticles(ParticleSystem particles)
        {
            var main = particles.main;
            main.startColor = new Color(1f, 1f, 1f, 0.3f);
            main.startSize = 0.3f;
            main.startLifetime = 4f;
            main.startSpeed = 1f;
            
            var emission = particles.emission;
            emission.rateOverTime = 10f * _effectsQualityScale;
        }
        
        private void ConfigureDustParticles(ParticleSystem particles)
        {
            var main = particles.main;
            main.startColor = new Color(0.6f, 0.5f, 0.4f, 0.4f);
            main.startSize = 0.1f;
            main.startLifetime = 5f;
            main.startSpeed = 0.5f;
            
            var emission = particles.emission;
            emission.rateOverTime = 15f * _effectsQualityScale;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandlePlantAdded(PlantInstance plant)
        {
            PlayEffect(EffectType.PlantGrowth, plant.transform.position, plant.transform, 3f);
            // Note: CreatePlantEffectController expects InteractivePlantComponent, commenting out for now
            // CreatePlantEffectController(plant);
        }
        
        private void HandlePlantStageChanged(PlantInstance plant)
        {
            DataPlantGrowthStage newStage = plant.CurrentGrowthStage;
            PlayEffect(EffectType.PlantGrowth, plant.transform.position, plant.transform, 2f);
            
            if (newStage == DataPlantGrowthStage.Flowering)
            {
                PlayEffect(EffectType.Environmental, plant.transform.position + Vector3.up * 0.5f, plant.transform, 5f);
            }
        }
        
        private void HandlePlantHarvested(PlantInstance plant)
        {
            PlayEffect(EffectType.PlantHarvest, plant.transform.position, null, 4f);
            
            string plantId = plant.PlantID;
            RemovePlantEffectController(plantId);
        }
        
        private void HandlePlantWatered(PlantInstance plant)
        {
            PlayEffect(EffectType.PlantWatering, plant.transform.position + Vector3.up * 1f, plant.transform, 3f);
        }
        
        private void HandleEnvironmentalChange(DataEnvironmentalConditions conditions)
        {
            // Create environmental effects based on conditions
            if (conditions.Temperature > 30f)
            {
                CreateEnvironmentalEffect("heat_effect", EnvironmentalEffectType.Heat, Vector3.zero, 10f);
            }
            
            if (conditions.Humidity > 80f)
            {
                CreateEnvironmentalEffect("humidity_effect", EnvironmentalEffectType.Humidity, Vector3.zero, 8f);
            }
            
            if (conditions.AirVelocity > 2f)
            {
                CreateEnvironmentalEffect("airflow_effect", EnvironmentalEffectType.AirFlow, Vector3.zero, 15f);
            }
        }
        
        private void HandleEnvironmentalAlert(EnvironmentalAlert alert)
        {
            // Visual alert effects
            PlayEffect(EffectType.Sparks, Vector3.zero, null, 2f);
        }
        
        private void HandleConstructionStarted(object projectObj)
        {
            PlayEffect(EffectType.Construction, Vector3.zero, null, 10f);
            CreateEnvironmentalEffect("construction_dust", EnvironmentalEffectType.AirFlow, Vector3.zero, 10f);
        }
        
        private void HandleConstructionProgress(ConstructionProgress progress)
        {
            if (progress?.Project == null) return;
            
            // Create construction effects based on progress
            var position = progress.Project.Position;
            PlayConstructionEffect(position, null, 2f);
            
            LogInfo($"Construction progress effect triggered for project {progress.Project.ProjectName}: {progress.Progress:P0}");
        }
        
        private void HandleConstructionCompleted(object projectObj)
        {
            PlayEffect(EffectType.Environmental, Vector3.zero, null, 5f);
            
            // Remove construction effects
            if (_environmentalEffects.ContainsKey("construction_dust"))
            {
                _environmentalEffects["construction_dust"].IsActive = false;
                _environmentalEffects.Remove("construction_dust");
            }
        }
        
        #endregion
        
        #region Performance and Quality Management
        
        private void UpdateEffectsSystem()
        {
            UpdatePlantEffects();
            UpdateEnvironmentalEffects();
            CullDistantEffects();
        }
        
        private void UpdateEnvironmentalEffects()
        {
            var effectsToRemove = new List<string>();
            
            foreach (var kvp in _environmentalEffects)
            {
                var effect = kvp.Value;
                if (!effect.IsActive || Time.time - effect.StartTime > 300f) // 5 minute max duration
                {
                    effectsToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var effectId in effectsToRemove)
            {
                _environmentalEffects.Remove(effectId);
            }
        }
        
        private void CullDistantEffects()
        {
            if (_mainCamera == null) return;
            
            Vector3 cameraPos = _mainCamera.transform.position;
            var effectsToRemove = new List<string>();
            
            // Cull VFX effects
            foreach (var kvp in _activeVFXEffects)
            {
                var vfx = kvp.Value;
                if (vfx != null && Vector3.Distance(cameraPos, vfx.transform.position) > _cullingDistance)
                {
                    vfx.Stop();
                    effectsToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in effectsToRemove)
            {
                _activeVFXEffects.Remove(key);
            }
        }
        
        private void UpdateAdaptiveQuality()
        {
            if (!_enableAdaptiveQuality) return;
            
            // Adjust effects quality based on performance
            float targetFrameRate = 60f;
            float currentFrameRate = 1f / Time.unscaledDeltaTime;
            
            if (currentFrameRate < targetFrameRate * 0.8f) // Below 80% of target
            {
                _effectsQualityScale = Mathf.Max(0.5f, _effectsQualityScale - 0.1f);
                _maxEffectsPerFrame = Mathf.Max(5, _maxEffectsPerFrame - 1);
            }
            else if (currentFrameRate > targetFrameRate * 0.95f) // Above 95% of target
            {
                _effectsQualityScale = Mathf.Min(1f, _effectsQualityScale + 0.05f);
                _maxEffectsPerFrame = Mathf.Min(15, _maxEffectsPerFrame + 1);
            }
        }
        
        private void CleanupFinishedEffects()
        {
            // Clean up VFX effects
            var vfxToRemove = new List<string>();
            foreach (var kvp in _activeVFXEffects)
            {
                if (kvp.Value == null || !kvp.Value.isActiveAndEnabled)
                {
                    vfxToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in vfxToRemove)
            {
                _activeVFXEffects.Remove(key);
            }
            
            // Clean up particle effects
            var particlesToRemove = new List<string>();
            foreach (var kvp in _activeParticleEffects)
            {
                if (kvp.Value == null || !kvp.Value.isPlaying)
                {
                    particlesToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in particlesToRemove)
            {
                _activeParticleEffects.Remove(key);
            }
        }
        
        private void UpdatePerformanceMetrics()
        {
            if (Time.time - _lastPerformanceUpdate < 1f) return;
            
            _performanceMetrics.ActiveVFXEffects = _activeVFXEffects.Count;
            _performanceMetrics.ActiveParticleEffects = _activeParticleEffects.Count;
            _performanceMetrics.EnvironmentalEffects = _environmentalEffects.Count;
            _performanceMetrics.EffectsThisFrame = _effectsThisFrame;
            _performanceMetrics.CurrentQuality = _effectsQualityScale;
            _performanceMetrics.LastUpdate = DateTime.Now;
            
            _lastPerformanceUpdate = Time.time;
        }
        
        private void UpdateDetailedPerformanceMetrics()
        {
            OnPerformanceUpdate?.Invoke(_performanceMetrics);
        }
        
        private bool IsPositionCulled(Vector3 position)
        {
            if (_mainCamera == null) return false;
            
            float distance = Vector3.Distance(_mainCamera.transform.position, position);
            return distance > _cullingDistance;
        }
        
        #endregion
        
        #region Public Interface
        
        public void SetEffectsEnabled(bool enabled)
        {
            _enableHighQualityEffects = enabled;
            
            if (!enabled)
            {
                // Stop all active effects
                foreach (var vfx in _activeVFXEffects.Values)
                {
                    if (vfx != null) vfx.Stop();
                }
                
                foreach (var particles in _activeParticleEffects.Values)
                {
                    if (particles != null) particles.Stop();
                }
            }
        }
        
        public void SetEffectsQuality(float quality)
        {
            _effectsQualityScale = Mathf.Clamp01(quality);
        }
        
        public void SetMaxConcurrentEffects(int maxEffects)
        {
            _maxConcurrentEffects = Mathf.Max(10, maxEffects);
            _performanceMetrics.MaxConcurrentEffects = _maxConcurrentEffects;
        }
        
        public EffectsPerformanceReport GetPerformanceReport()
        {
            return new EffectsPerformanceReport
            {
                Metrics = _performanceMetrics,
                ActiveEffectsList = _activeVFXEffects.Keys.ToList(),
                ActiveParticlesList = _activeParticleEffects.Keys.ToList(),
                EnvironmentalEffectsList = _environmentalEffects.Keys.ToList()
            };
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            // Stop all coroutines and cleanup
            StopAllCoroutines();
            CancelInvoke();
            
            // Cleanup active effects
            foreach (var vfx in _activeVFXEffects.Values)
            {
                if (vfx != null)
                {
                    vfx.Stop();
                    DestroyImmediate(vfx.gameObject);
                }
            }
            _activeVFXEffects.Clear();
            
            foreach (var particles in _activeParticleEffects.Values)
            {
                if (particles != null)
                {
                    particles.Stop();
                    DestroyImmediate(particles.gameObject);
                }
            }
            _activeParticleEffects.Clear();
            
            // Cleanup pools
            while (_particleSystemPool.Count > 0)
            {
                var particles = _particleSystemPool.Dequeue();
                if (particles != null)
                {
                    DestroyImmediate(particles.gameObject);
                }
            }
            
            while (_vfxPool.Count > 0)
            {
                var vfx = _vfxPool.Dequeue();
                if (vfx != null)
                {
                    DestroyImmediate(vfx.gameObject);
                }
            }
            
            // Cleanup environmental effects
            foreach (var effect in _environmentalEffects.Values)
            {
                if (effect.EffectObject != null)
                {
                    DestroyImmediate(effect.EffectObject);
                }
            }
            _environmentalEffects.Clear();
            
            // Cleanup plant effects
            foreach (var controller in _plantEffects.Values)
            {
                controller?.Cleanup();
            }
            _plantEffects.Clear();
            
            // Cleanup construction effects
            foreach (var controller in _constructionEffects)
            {
                controller?.Cleanup();
            }
            _constructionEffects.Clear();
            
            // Cleanup weather effects
            foreach (var controller in _weatherEffects)
            {
                controller?.Cleanup();
            }
            _weatherEffects.Clear();
            
            // Disconnect system events
            DisconnectSystemEvents();
            
            LogInfo("Advanced Effects Manager shutdown complete");
        }
        
        private void DisconnectSystemEvents()
        {
            // PHASE 1 PC-007: Event disconnections now handled through GameEventSO channels
            // Direct manager event unsubscriptions removed - will be replaced with event-driven communication
            
            // TODO Phase 2: Unsubscribe from GameEventSO channels when implemented
            
            LogInfo("Advanced Effects Manager: Event system disconnections deferred to Phase 2 event channel implementation");
        }
    }
}