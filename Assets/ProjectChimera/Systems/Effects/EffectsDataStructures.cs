using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic;
using System;
using System.Linq;
using ProjectChimera.Data.Construction;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Systems.Construction;
using ProjectChimera.Systems.Cultivation;
// Explicit alias for Data layer PlantGrowthStage to resolve ambiguity
using DataPlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;

namespace ProjectChimera.Systems.Effects
{
    /// <summary>
    /// Comprehensive data structures for the advanced effects system.
    /// Includes effect types, performance metrics, controllers, and configuration data.
    /// </summary>
    
    // Core Effect Enums
    public enum EffectType
    {
        PlantGrowth,
        PlantWatering,
        PlantHarvest,
        PlantFlowering,
        PlantMaturation,
        Construction,
        Environmental,
        Sparks,
        Steam,
        Dust,
        Smoke,
        Fire,
        Water,
        Magic,
        UI,
        Cinematic,
        Achievement
    }
    
    public enum EnvironmentalEffectType
    {
        Heat,
        Humidity,
        AirFlow,
        Light,
        Smoke,
        Steam,
        Dust,
        Pollen
    }
    
    public enum EffectQuality
    {
        Low,
        Medium,
        High,
        Ultra
    }
    
    public enum EffectState
    {
        Inactive,
        Playing,
        Paused,
        Stopping,
        Finished
    }
    
    // Performance and Monitoring
    [System.Serializable]
    public class EffectsPerformanceMetrics
    {
        public int ActiveVFXEffects;
        public int ActiveParticleEffects;
        public int EnvironmentalEffects;
        public int MaxConcurrentEffects;
        public int EffectsThisFrame;
        public float CurrentQuality;
        public float EffectsQuality;
        public bool AdaptiveQualityEnabled;
        public float MemoryUsage;
        public float CPUUsage;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class EffectsPerformanceReport
    {
        public EffectsPerformanceMetrics Metrics;
        public List<string> ActiveEffectsList;
        public List<string> ActiveParticlesList;
        public List<string> EnvironmentalEffectsList;
        public Dictionary<EffectType, int> EffectTypeCounts;
        public float AverageEffectDuration;
        public int EffectsCreatedThisSecond;
    }
    
    // Environmental Effects
    [System.Serializable]
    public class EnvironmentalEffectInstance
    {
        public string EffectId;
        public EnvironmentalEffectType EffectType;
        public Vector3 Position;
        public float Radius;
        public bool IsActive;
        public float StartTime;
        public float LastParticleSpawn;
        public float Intensity = 1f;
        public Transform AttachedTransform;
        public GameObject EffectObject;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
    }
    
    [System.Serializable]
    public class WeatherEffectData
    {
        public string WeatherType;
        public float Intensity;
        public Vector3 WindDirection;
        public float WindSpeed;
        public bool EnableRain;
        public bool EnableFog;
        public Color FogColor = Color.gray;
        public float FogDensity = 0.1f;
    }
    
    // Plant Effects
    [System.Serializable]
    public class PlantEffectData
    {
        public string PlantId;
        public DataPlantGrowthStage GrowthStage;
        public float Health;
        public float GrowthProgress;
        public Vector3 PlantPosition;
        public bool NeedsWatering;
        public bool IsFlowering;
        public bool ReadyToHarvest;
        public float EnvironmentalStress;
    }
    
    public class PlantEffectController
    {
        private InteractivePlantComponent _plant;
        private AdvancedEffectsManager _effectsManager;
        private List<string> _activeEffects = new List<string>();
        private float _lastGrowthEffect = 0f;
        private float _lastHealthEffect = 0f;
        
        public PlantEffectController(InteractivePlantComponent plant, AdvancedEffectsManager effectsManager)
        {
            _plant = plant;
            _effectsManager = effectsManager;
        }
        
        public void Initialize()
        {
            if (_plant != null)
            {
                // Start with initial growth effect
                CreateGrowthEffect();
            }
        }
        
        public void Update()
        {
            if (_plant == null) return;
            
            // Update growth effects based on plant health and stage
            UpdateGrowthEffects();
            
            // Update health-based effects
            UpdateHealthEffects();
            
            // Update stage-specific effects
            UpdateStageEffects();
        }
        
        private void UpdateGrowthEffects()
        {
            if (Time.time - _lastGrowthEffect > 10f && _plant.Health > 50f)
            {
                CreateGrowthEffect();
                _lastGrowthEffect = Time.time;
            }
        }
        
        private void UpdateHealthEffects()
        {
            if (_plant.Health < 30f && Time.time - _lastHealthEffect > 5f)
            {
                CreateStressEffect();
                _lastHealthEffect = Time.time;
            }
        }
        
        private void UpdateStageEffects()
        {
            switch (_plant.CurrentGrowthStage)
            {
                case DataPlantGrowthStage.Flowering:
                    CreateFloweringEffect();
                    break;
                case DataPlantGrowthStage.Harvest:
                    CreateHarvestReadyEffect();
                    break;
            }
        }
        
        private void CreateGrowthEffect()
        {
            _effectsManager.PlayEffect(EffectType.PlantGrowth, _plant.transform.position, _plant.transform, 3f);
        }
        
        private void CreateStressEffect()
        {
            _effectsManager.PlayEffect(EffectType.Steam, _plant.transform.position + Vector3.up * 0.5f, _plant.transform, 2f);
        }
        
        private void CreateFloweringEffect()
        {
            if (UnityEngine.Random.value < 0.1f) // 10% chance per update
            {
                _effectsManager.PlayEffect(EffectType.Environmental, _plant.transform.position + Vector3.up * 1f, _plant.transform, 4f);
            }
        }
        
        private void CreateHarvestReadyEffect()
        {
            if (UnityEngine.Random.value < 0.05f) // 5% chance per update
            {
                _effectsManager.PlayEffect(EffectType.PlantHarvest, _plant.transform.position + Vector3.up * 1.2f, _plant.transform, 5f);
            }
        }
        
        public void Cleanup()
        {
            // Clean up any active effects for this plant
            _activeEffects.Clear();
        }
    }
    
    public class PlantEffectLibrary
    {
        private Dictionary<DataPlantGrowthStage, List<EffectType>> _stageEffects;
        
        public PlantEffectLibrary()
        {
            InitializeEffectLibrary();
        }
        
        private void InitializeEffectLibrary()
        {
            _stageEffects = new Dictionary<DataPlantGrowthStage, List<EffectType>>
            {
                { DataPlantGrowthStage.Seedling, new List<EffectType> { EffectType.PlantGrowth, EffectType.PlantWatering } },
                { DataPlantGrowthStage.Vegetative, new List<EffectType> { EffectType.PlantGrowth, EffectType.Environmental } },
                { DataPlantGrowthStage.Flowering, new List<EffectType> { EffectType.PlantGrowth, EffectType.Environmental, EffectType.Dust } },
                { DataPlantGrowthStage.Harvest, new List<EffectType> { EffectType.PlantHarvest, EffectType.Environmental } }
            };
        }
        
        public List<EffectType> GetEffectsForStage(DataPlantGrowthStage stage)
        {
            return _stageEffects.TryGetValue(stage, out var effects) ? effects : new List<EffectType>();
        }
    }
    
    // Construction Effects
    [System.Serializable]
    public class ConstructionEffectData
    {
        public string ProjectId;
        public string ProjectName;
        public Vector3 BuildSite;
        public float ConstructionProgress;
        public ConstructionPhase CurrentPhase;
        public int ActiveWorkers;
        public bool HasPowerTools;
        public bool IsOutdoor;
    }
    
    public class ConstructionEffectController
    {
        private ConstructionProject _project;
        private AdvancedEffectsManager _effectsManager;
        private List<string> _activeEffects = new List<string>();
        private float _lastDustEffect = 0f;
        private float _lastSparksEffect = 0f;
        
        public ConstructionEffectController(ConstructionProject project, AdvancedEffectsManager effectsManager)
        {
            _project = project;
            _effectsManager = effectsManager;
        }
        
        public void Update()
        {
            if (_project == null || _project.Status != ProjectStatus.InProgress) return;
            
            UpdatePhaseEffects();
            UpdateProgressEffects();
            UpdateWorkerEffects();
        }
        
        private void UpdatePhaseEffects()
        {
            switch (_project.CurrentPhase)
            {
                case ConstructionPhase.Foundation:
                    CreateFoundationEffects();
                    break;
                case ConstructionPhase.Structure:
                    CreateStructureEffects();
                    break;
                case ConstructionPhase.Systems:
                    CreateSystemsEffects();
                    break;
            }
        }
        
        private void UpdateProgressEffects()
        {
            if (Time.time - _lastDustEffect > 3f)
            {
                _effectsManager.PlayEffect(EffectType.Dust, _project.BuildingSite + UnityEngine.Random.insideUnitSphere * 3f, null, 4f);
                _lastDustEffect = Time.time;
            }
        }
        
        private void UpdateWorkerEffects()
        {
            if (_project.AssignedWorkers.Count > 0 && Time.time - _lastSparksEffect > 8f)
            {
                _effectsManager.PlayEffect(EffectType.Sparks, _project.BuildingSite + Vector3.up * 2f, null, 2f);
                _lastSparksEffect = Time.time;
            }
        }
        
        private void CreateFoundationEffects()
        {
            if (UnityEngine.Random.value < 0.1f)
            {
                _effectsManager.PlayEffect(EffectType.Dust, _project.BuildingSite, null, 3f);
            }
        }
        
        private void CreateStructureEffects()
        {
            if (UnityEngine.Random.value < 0.05f)
            {
                _effectsManager.PlayEffect(EffectType.Sparks, _project.BuildingSite + Vector3.up * 3f, null, 2f);
            }
        }
        
        private void CreateSystemsEffects()
        {
            if (UnityEngine.Random.value < 0.03f)
            {
                _effectsManager.PlayEffect(EffectType.Sparks, _project.BuildingSite + Vector3.up * 1.5f, null, 1.5f);
            }
        }
        
        public void Cleanup()
        {
            // Clean up any active effects for this construction project
            _activeEffects.Clear();
            _lastDustEffect = 0f;
            _lastSparksEffect = 0f;
        }
    }
    
    // Weather Effects
    public class WeatherEffectController
    {
        private WeatherEffectData _weatherData;
        private AdvancedEffectsManager _effectsManager;
        private ParticleSystem _rainParticles;
        private ParticleSystem _fogParticles;
        
        public WeatherEffectController(WeatherEffectData weatherData, AdvancedEffectsManager effectsManager)
        {
            _weatherData = weatherData;
            _effectsManager = effectsManager;
        }
        
        public void Initialize()
        {
            SetupWeatherEffects();
        }
        
        public void Update()
        {
            UpdateWeatherIntensity();
            UpdateWeatherDirection();
        }
        
        private void SetupWeatherEffects()
        {
            if (_weatherData.EnableRain)
            {
                CreateRainEffect();
            }
            
            if (_weatherData.EnableFog)
            {
                CreateFogEffect();
            }
        }
        
        private void CreateRainEffect()
        {
            // Implementation for rain particles
        }
        
        private void CreateFogEffect()
        {
            // Implementation for fog effects
        }
        
        private void UpdateWeatherIntensity()
        {
            // Update weather effect intensity based on data
        }
        
        private void UpdateWeatherDirection()
        {
            // Update wind direction and effects
        }
        
        public void Cleanup()
        {
            // Clean up weather effects
            if (_rainParticles != null)
            {
                _rainParticles.Stop();
                _rainParticles = null;
            }
            
            if (_fogParticles != null)
            {
                _fogParticles.Stop();
                _fogParticles = null;
            }
        }
    }
    
    // UI Effects
    public class UIEffectsController
    {
        private Dictionary<string, VisualEffect> _uiEffects = new Dictionary<string, VisualEffect>();
        
        public void CreateUIEffect(string effectId, UIEffectType effectType, Vector2 screenPosition)
        {
            switch (effectType)
            {
                case UIEffectType.ButtonHover:
                    CreateButtonHoverEffect(effectId, screenPosition);
                    break;
                case UIEffectType.ButtonClick:
                    CreateButtonClickEffect(effectId, screenPosition);
                    break;
                case UIEffectType.Notification:
                    CreateNotificationEffect(effectId, screenPosition);
                    break;
                case UIEffectType.Transition:
                    CreateTransitionEffect(effectId, screenPosition);
                    break;
            }
        }
        
        private void CreateButtonHoverEffect(string effectId, Vector2 position)
        {
            // Implementation for button hover effect
        }
        
        private void CreateButtonClickEffect(string effectId, Vector2 position)
        {
            // Implementation for button click effect
        }
        
        private void CreateNotificationEffect(string effectId, Vector2 position)
        {
            // Implementation for notification effect
        }
        
        private void CreateTransitionEffect(string effectId, Vector2 position)
        {
            // Implementation for transition effect
        }
    }
    
    public enum UIEffectType
    {
        ButtonHover,
        ButtonClick,
        Notification,
        Transition,
        Error,
        Success,
        Loading
    }
    
    // Effect Templates and Configuration
    [System.Serializable]
    public class EffectTemplate
    {
        public string TemplateId;
        public string TemplateName;
        public EffectType EffectType;
        public VisualEffectAsset VFXAsset;
        public ParticleSystem ParticleTemplate;
        public EffectParameters DefaultParameters;
        public float DefaultDuration = 5f;
        public bool LoopEffect = false;
        public EffectQuality MinimumQuality = EffectQuality.Low;
    }
    
    [System.Serializable]
    public class EffectParameters
    {
        public float Intensity = 1f;
        public float Scale = 1f;
        public Color Tint = Color.white;
        public Vector3 Velocity = Vector3.zero;
        public bool UseGravity = true;
        public float EmissionRate = 10f;
        public Dictionary<string, float> CustomParameters = new Dictionary<string, float>();
    }
    
    [System.Serializable]
    public class EffectInstance
    {
        public string InstanceId;
        public EffectTemplate Template;
        public Vector3 Position;
        public Transform Parent;
        public EffectState State;
        public float StartTime;
        public float Duration;
        public EffectParameters Parameters;
        public VisualEffect VFXComponent;
        public ParticleSystem ParticleComponent;
    }
    
    // ScriptableObject-based Effect Libraries
    [CreateAssetMenu(fileName = "New Effect Library", menuName = "Project Chimera/Effects/Effect Library")]
    public class EffectLibrarySO : ScriptableObject
    {
        [SerializeField] private List<EffectTemplate> _effectTemplates = new List<EffectTemplate>();
        [SerializeField] private List<VisualEffectAsset> _vfxAssets = new List<VisualEffectAsset>();
        [SerializeField] private List<ParticleSystem> _particleTemplates = new List<ParticleSystem>();
        
        private Dictionary<string, EffectTemplate> _templateLookup;
        private Dictionary<EffectType, List<EffectTemplate>> _typeLookup;
        
        public void InitializeDefaults()
        {
            if (_effectTemplates.Count == 0)
            {
                CreateDefaultEffectTemplates();
            }
            
            BuildLookupTables();
        }
        
        private void CreateDefaultEffectTemplates()
        {
            _effectTemplates.AddRange(new[]
            {
                CreateEffectTemplate("plant_growth", "Plant Growth", EffectType.PlantGrowth, 5f),
                CreateEffectTemplate("plant_watering", "Plant Watering", EffectType.PlantWatering, 3f),
                CreateEffectTemplate("plant_harvest", "Plant Harvest", EffectType.PlantHarvest, 4f),
                CreateEffectTemplate("construction_work", "Construction Work", EffectType.Construction, 10f, true),
                CreateEffectTemplate("environmental_heat", "Environmental Heat", EffectType.Environmental, 8f, true),
                CreateEffectTemplate("sparks_effect", "Sparks", EffectType.Sparks, 2f),
                CreateEffectTemplate("steam_effect", "Steam", EffectType.Steam, 6f),
                CreateEffectTemplate("dust_cloud", "Dust Cloud", EffectType.Dust, 4f)
            });
        }
        
        private EffectTemplate CreateEffectTemplate(string id, string name, EffectType type, float duration, bool loop = false)
        {
            return new EffectTemplate
            {
                TemplateId = id,
                TemplateName = name,
                EffectType = type,
                DefaultDuration = duration,
                LoopEffect = loop,
                DefaultParameters = new EffectParameters()
            };
        }
        
        private void BuildLookupTables()
        {
            _templateLookup = new Dictionary<string, EffectTemplate>();
            _typeLookup = new Dictionary<EffectType, List<EffectTemplate>>();
            
            foreach (var template in _effectTemplates)
            {
                _templateLookup[template.TemplateId] = template;
                
                if (!_typeLookup.ContainsKey(template.EffectType))
                {
                    _typeLookup[template.EffectType] = new List<EffectTemplate>();
                }
                _typeLookup[template.EffectType].Add(template);
            }
        }
        
        public EffectTemplate GetTemplate(string templateId)
        {
            return _templateLookup.TryGetValue(templateId, out var template) ? template : null;
        }
        
        public List<EffectTemplate> GetTemplatesByType(EffectType effectType)
        {
            return _typeLookup.TryGetValue(effectType, out var templates) ? templates : new List<EffectTemplate>();
        }
        
        public VisualEffectAsset GetVFXAsset(EffectType effectType)
        {
            // Return appropriate VFX asset based on effect type
            return _vfxAssets.FirstOrDefault();
        }
        
        public ParticleSystem GetParticleTemplate(EffectType effectType)
        {
            // Return appropriate particle template based on effect type
            return _particleTemplates.FirstOrDefault();
        }
    }
}