using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Environment;
using System.Collections.Generic;
using System;
using System.Linq;
// Explicit alias for Data layer PlantGrowthStage to resolve ambiguity
using DataPlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;
// Explicit alias for Data layer EnvironmentalConditions to resolve namespace conflicts
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
// Explicit alias for EnvironmentalManager to resolve ambiguity
using EnvironmentalManager = ProjectChimera.Systems.Environment.EnvironmentalManager;

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// SpeedTree plant instance for Project Chimera cannabis simulation.
    /// Manages individual plant instances with SpeedTree integration for realistic cannabis cultivation.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class SpeedTreePlantInstance : MonoBehaviour
    {
        [Header("Plant Identity")]
        [SerializeField] private string _plantId;
        [SerializeField] private PlantStrainSO _plantStrain;
        [SerializeField] private CannabisGenotype _genotype;
        [SerializeField] private int _generationNumber = 1;
        
        [Header("Growth Configuration")]
        [SerializeField] private DataPlantGrowthStage _currentGrowthStage = DataPlantGrowthStage.Seedling;
        [SerializeField] private float _growthRate = 1.0f;
        [SerializeField] private float _maturityLevel = 0.0f; // 0-1
        [SerializeField] private bool _enableAutoGrowth = true;
        
        [Header("Plant Health")]
        [SerializeField] private float _health = 100f;
        [SerializeField] private float _vigor = 80f;
        [SerializeField] private float _stressLevel = 0f;
        [SerializeField] private float _diseaseResistance = 75f;
        
        [Header("Environmental Response")]
        [SerializeField] private EnvironmentalConditions _currentConditions;
        [SerializeField] private float _environmentalAdaptation = 1.0f;
        [SerializeField] private bool _isStressed = false;
        
        [Header("Visual Properties")]
        [SerializeField] private Color _leafColor = Color.green;
        [SerializeField] private float _leafDensity = 1.0f;
        [SerializeField] private float _branchDensity = 1.0f;
        [SerializeField] private Vector3 _plantScale = Vector3.one;
        
        [Header("SpeedTree Integration")]
        [SerializeField] private bool _speedTreeEnabled = true;
        [SerializeField] private Material[] _speedTreeMaterials;
        [SerializeField] private float _windStrength = 1.0f;
        [SerializeField] private float _windFrequency = 1.0f;
        
        // Component references
        private Renderer _renderer;
        private PlantManager _plantManager;
        private EnvironmentalManager _environmentalManager;
        
        // Runtime data
        private Dictionary<string, float> _geneticTraitExpressions = new Dictionary<string, float>();
        private List<string> _activeEffects = new List<string>();
        private DateTime _plantedDate;
        private DateTime _lastUpdateTime;
        private float _totalGrowthTime = 0f;
        
        // Events
        public event System.Action<SpeedTreePlantInstance, DataPlantGrowthStage> OnGrowthStageChanged;
        public event System.Action<SpeedTreePlantInstance, float> OnHealthChanged;
        public event System.Action<SpeedTreePlantInstance> OnPlantHarvested;
        public event System.Action<SpeedTreePlantInstance> OnPlantDied;
        
        // Public Properties
        public string PlantId => _plantId;
        public int InstanceId => GetInstanceID(); // Unity's built-in instance ID
        public Vector3 Position => transform.position;
        public Vector3 Scale => transform.localScale;
        public CannabisGenotype GeneticData => _genotype;
        public PlantGrowthStage GrowthStage => ConvertToPlantGrowthStage(_currentGrowthStage);
        public Renderer Renderer => _renderer;
        public float Health => _health;
        public PlantStrainSO PlantStrain => _plantStrain;
        public CannabisGenotype Genotype => _genotype;
        public int GenerationNumber => _generationNumber;
        public DataPlantGrowthStage CurrentGrowthStage => _currentGrowthStage;
        public float GrowthRate => _growthRate;
        public float MaturityLevel => _maturityLevel;
        public float Vigor => _vigor;
        public float StressLevel => _stressLevel;
        public float DiseaseResistance => _diseaseResistance;
        public bool IsAlive => _health > 0f;
        public bool IsMatured => _maturityLevel >= 1.0f;
        public bool CanHarvest => _currentGrowthStage == DataPlantGrowthStage.Flowering && _maturityLevel >= 0.8f;
        public DateTime PlantedDate => _plantedDate;
        public float TotalGrowthTime => _totalGrowthTime;
        
        // Additional properties for Error Wave 143 compatibility
        public string SpeedTreeAssetPath => _plantStrain?.name ?? "";
        public Material[] materials => _speedTreeMaterials;
        public bool HasValue => IsAlive;
        public SpeedTreePlantInstance Value => this;
        public float MutationRate => 0.1f;
        public Dictionary<string, float> EnvironmentalModifiers => _geneticTraitExpressions;
        
        // Type conversion methods for compatibility with AdvancedSpeedTreeManager.SpeedTreePlantData
        public AdvancedSpeedTreeManager.SpeedTreePlantData ToSpeedTreePlantData()
        {
            var data = new AdvancedSpeedTreeManager.SpeedTreePlantData
            {
                InstanceId = GetInstanceID(),
                Position = transform.position,
                Rotation = transform.rotation,
                Scale = transform.localScale,
                GrowthStage = ConvertToPlantGrowthStage(_currentGrowthStage),
                Health = _health,
                Age = _totalGrowthTime,
                CreationTime = Time.time - _totalGrowthTime,
                LastUpdateTime = Time.time,
                IsActive = IsAlive,
                EnvironmentalModifiers = new Dictionary<string, float>(_geneticTraitExpressions),
                EnvironmentalConditions = _currentConditions,
                GrowthRate = _growthRate,
                StressResistance = _diseaseResistance / 100f,
                MutationRate = 0.1f
            };
            
            // Set genetic data if available
            if (_genotype != null)
            {
                data.GeneticData = new CannabisGeneticData
                {
                    PlantSize = _genotype.Traits?.FirstOrDefault(t => t.TraitName == "PlantSize")?.ExpressedValue ?? 1f,
                    BudColor = _leafColor,
                    TrichromeAmount = _genotype.Traits?.FirstOrDefault(t => t.TraitName == "TrichromeAmount")?.ExpressedValue ?? 0.5f,
                    HeatTolerance = _genotype.Traits?.FirstOrDefault(t => t.TraitName == "HeatTolerance")?.ExpressedValue ?? 0.5f,
                    ColdTolerance = _genotype.Traits?.FirstOrDefault(t => t.TraitName == "ColdTolerance")?.ExpressedValue ?? 0.5f,
                    DroughtTolerance = _genotype.Traits?.FirstOrDefault(t => t.TraitName == "DroughtTolerance")?.ExpressedValue ?? 0.5f
                };
            }
            
            return data;
        }
        
        public void UpdateFromSpeedTreePlantData(AdvancedSpeedTreeManager.SpeedTreePlantData data)
        {
            if (data == null) return;
            
            // Update position and scale
            transform.position = data.Position;
            transform.rotation = data.Rotation;
            transform.localScale = data.Scale;
            
            // Update health and growth
            _health = data.Health;
            _growthRate = data.GrowthRate;
            _diseaseResistance = data.StressResistance * 100f;
            
            // Update environmental modifiers
            if (data.EnvironmentalModifiers != null)
            {
                _geneticTraitExpressions.Clear();
                foreach (var modifier in data.EnvironmentalModifiers)
                {
                    _geneticTraitExpressions[modifier.Key] = modifier.Value;
                }
            }
            
            // Update environmental conditions
            if (data.EnvironmentalConditions != null)
            {
                _currentConditions = data.EnvironmentalConditions;
            }
        }
        
        private void Awake()
        {
            InitializePlantInstance();
        }
        
        private void Start()
        {
            RegisterWithManagers();
            ApplyGeneticTraits();
            InitializeSpeedTreeIntegration();
        }
        
        private void Update()
        {
            if (_enableAutoGrowth)
            {
                UpdatePlantGrowth();
            }
            
            UpdatePlantHealth();
            UpdateVisualProperties();
            UpdateSpeedTreeProperties();
        }
        
        /// <summary>
        /// Initialize plant instance
        /// </summary>
        private void InitializePlantInstance()
        {
            if (string.IsNullOrEmpty(_plantId))
            {
                _plantId = Guid.NewGuid().ToString();
            }
            
            _renderer = GetComponent<Renderer>();
            _plantedDate = DateTime.Now;
            _lastUpdateTime = DateTime.Now;
            
            // Initialize genetic trait expressions
            InitializeGeneticExpressions();
            
            Debug.Log($"Initialized SpeedTree plant instance: {_plantId}");
        }
        
        /// <summary>
        /// Register with manager systems
        /// </summary>
        private void RegisterWithManagers()
        {
            _plantManager = FindFirstObjectByType<PlantManager>();
            _environmentalManager = FindFirstObjectByType<EnvironmentalManager>();
            
            if (_plantManager != null)
            {
                _plantManager.RegisterPlantInstance(this);
            }
        }
        
        /// <summary>
        /// Initialize genetic trait expressions
        /// </summary>
        private void InitializeGeneticExpressions()
        {
            if (_genotype != null && _genotype.Traits != null)
            {
                foreach (var trait in _genotype.Traits)
                {
                    _geneticTraitExpressions[trait.TraitName] = trait.ExpressedValue;
                }
            }
        }
        
        /// <summary>
        /// Apply genetic traits to plant appearance and behavior
        /// </summary>
        private void ApplyGeneticTraits()
        {
            if (_plantStrain == null || _genotype == null) return;
            
            // Apply genetic modifiers
            foreach (var trait in _genotype.Traits)
            {
                ApplyGeneticTrait(trait);
            }
            
            // Update visual properties based on genetics
            UpdateGeneticVisualTraits();
        }
        
        /// <summary>
        /// Apply individual genetic trait
        /// </summary>
        private void ApplyGeneticTrait(GeneticTrait trait)
        {
            switch (trait.TraitName.ToLower())
            {
                case "growth_rate":
                    _growthRate *= trait.ExpressedValue;
                    break;
                case "disease_resistance":
                    _diseaseResistance *= trait.ExpressedValue;
                    break;
                case "vigor":
                    _vigor *= trait.ExpressedValue;
                    break;
                case "leaf_color":
                    _leafColor = Color.Lerp(_leafColor, Color.green, trait.ExpressedValue);
                    break;
                case "plant_height":
                    _plantScale.y *= trait.ExpressedValue;
                    break;
                case "branch_density":
                    _branchDensity *= trait.ExpressedValue;
                    break;
            }
        }
        
        /// <summary>
        /// Update genetic visual traits
        /// </summary>
        private void UpdateGeneticVisualTraits()
        {
            // Apply scale based on genetics
            transform.localScale = _plantScale;
            
            // Update material properties if available
            if (_renderer != null && _renderer.material != null)
            {
                _renderer.material.color = _leafColor;
            }
        }
        
        /// <summary>
        /// Initialize SpeedTree integration
        /// </summary>
        private void InitializeSpeedTreeIntegration()
        {
            if (!_speedTreeEnabled) return;
            
            #if UNITY_SPEEDTREE
            // SpeedTree specific initialization
            var speedTreeComponent = GetComponent<SpeedTreeWind>();
            if (speedTreeComponent != null)
            {
                speedTreeComponent.m_WindStrength = _windStrength;
                speedTreeComponent.m_WindFrequency = _windFrequency;
            }
            #endif
            
            // Apply SpeedTree materials
            if (_speedTreeMaterials != null && _speedTreeMaterials.Length > 0 && _renderer != null)
            {
                _renderer.materials = _speedTreeMaterials;
            }
            
            Debug.Log($"SpeedTree integration initialized for plant: {_plantId}");
        }
        
        /// <summary>
        /// Update plant growth over time
        /// </summary>
        private void UpdatePlantGrowth()
        {
            if (!IsAlive) return;
            
            float deltaTime = Time.deltaTime;
            _totalGrowthTime += deltaTime;
            
            // Calculate growth progress
            float growthIncrement = _growthRate * deltaTime * CalculateGrowthModifier();
            _maturityLevel = Mathf.Clamp01(_maturityLevel + growthIncrement * 0.01f); // Slow growth
            
            // Check for growth stage transitions
            CheckGrowthStageTransition();
            
            _lastUpdateTime = DateTime.Now;
        }
        
        /// <summary>
        /// Calculate growth modifier based on environmental conditions
        /// </summary>
        private float CalculateGrowthModifier()
        {
            float modifier = 1.0f;
            
            // Environmental conditions affect growth
            if (_currentConditions != null)
            {
                // Temperature factor
                float tempOptimal = 24f; // Optimal temperature for cannabis
                float tempDiff = Mathf.Abs(_currentConditions.Temperature - tempOptimal);
                float tempModifier = Mathf.Max(0.1f, 1f - (tempDiff / 10f));
                
                // Humidity factor
                float humidityOptimal = 60f; // Optimal humidity for cannabis
                float humidityDiff = Mathf.Abs(_currentConditions.Humidity - humidityOptimal);
                float humidityModifier = Mathf.Max(0.1f, 1f - (humidityDiff / 20f));
                
                // Light factor
                float lightModifier = Mathf.Clamp01(_currentConditions.LightIntensity / 100f);
                
                modifier = tempModifier * humidityModifier * lightModifier;
            }
            
            // Health affects growth
            modifier *= (_health / 100f);
            
            // Stress reduces growth
            modifier *= (1f - (_stressLevel / 100f));
            
            return Mathf.Clamp01(modifier);
        }
        
        /// <summary>
        /// Check for growth stage transitions
        /// </summary>
        private void CheckGrowthStageTransition()
        {
            DataPlantGrowthStage newStage = _currentGrowthStage;
            
            switch (_currentGrowthStage)
            {
                case DataPlantGrowthStage.Seed:
                    if (_maturityLevel > 0.1f) newStage = DataPlantGrowthStage.Germination;
                    break;
                case DataPlantGrowthStage.Germination:
                    if (_maturityLevel > 0.2f) newStage = DataPlantGrowthStage.Seedling;
                    break;
                case DataPlantGrowthStage.Seedling:
                    if (_maturityLevel > 0.4f) newStage = DataPlantGrowthStage.Vegetative;
                    break;
                case DataPlantGrowthStage.Vegetative:
                    if (_maturityLevel > 0.7f) newStage = DataPlantGrowthStage.PreFlowering;
                    break;
                case DataPlantGrowthStage.PreFlowering:
                    if (_maturityLevel > 0.8f) newStage = DataPlantGrowthStage.Flowering;
                    break;
                case DataPlantGrowthStage.Flowering:
                    if (_maturityLevel >= 1.0f) newStage = DataPlantGrowthStage.Harvest;
                    break;
            }
            
            if (newStage != _currentGrowthStage)
            {
                SetGrowthStage(newStage);
            }
        }
        
        /// <summary>
        /// Set growth stage
        /// </summary>
        public void SetGrowthStage(DataPlantGrowthStage newStage)
        {
            var previousStage = _currentGrowthStage;
            _currentGrowthStage = newStage;
            
            OnGrowthStageChanged?.Invoke(this, newStage);
            
            Debug.Log($"Plant {_plantId} transitioned from {previousStage} to {newStage}");
        }
        
        /// <summary>
        /// Update plant health
        /// </summary>
        private void UpdatePlantHealth()
        {
            if (!IsAlive) return;
            
            float healthChange = 0f;
            
            // Environmental stress affects health
            if (_isStressed)
            {
                healthChange -= Time.deltaTime * 2f; // Lose 2 health per second when stressed
            }
            
            // Good conditions slowly restore health
            if (!_isStressed && _health < 100f)
            {
                healthChange += Time.deltaTime * 0.5f; // Gain 0.5 health per second in good conditions
            }
            
            // Apply health change
            if (healthChange != 0f)
            {
                SetHealth(_health + healthChange);
            }
        }
        
        /// <summary>
        /// Set plant health
        /// </summary>
        public void SetHealth(float newHealth)
        {
            float previousHealth = _health;
            _health = Mathf.Clamp(newHealth, 0f, 100f);
            
            if (_health != previousHealth)
            {
                OnHealthChanged?.Invoke(this, _health);
            }
            
            // Check for death
            if (_health <= 0f && previousHealth > 0f)
            {
                OnPlantDied?.Invoke(this);
                Debug.Log($"Plant {_plantId} has died");
            }
        }
        
        /// <summary>
        /// Update visual properties based on current state
        /// </summary>
        private void UpdateVisualProperties()
        {
            if (_renderer == null) return;
            
            // Update color based on health
            Color healthColor = Color.Lerp(Color.red, _leafColor, _health / 100f);
            
            // Update color based on growth stage
            Color stageColor = GetGrowthStageColor();
            Color finalColor = Color.Lerp(healthColor, stageColor, 0.5f);
            
            if (_renderer.material != null)
            {
                _renderer.material.color = finalColor;
            }
        }
        
        /// <summary>
        /// Get color based on growth stage
        /// </summary>
        private Color GetGrowthStageColor()
        {
            return _currentGrowthStage switch
            {
                DataPlantGrowthStage.Seed => Color.brown,
                DataPlantGrowthStage.Germination => Color.yellow,
                DataPlantGrowthStage.Seedling => Color.green * 0.7f,
                DataPlantGrowthStage.Vegetative => Color.green,
                DataPlantGrowthStage.PreFlowering => Color.green * 1.2f,
                DataPlantGrowthStage.Flowering => Color.Lerp(Color.green, Color.white, 0.3f),
                DataPlantGrowthStage.Harvest => Color.Lerp(Color.green, Color.yellow, 0.4f),
                DataPlantGrowthStage.Harvested => Color.brown,
                _ => Color.green
            };
        }
        
        /// <summary>
        /// Update SpeedTree properties
        /// </summary>
        private void UpdateSpeedTreeProperties()
        {
            if (!_speedTreeEnabled) return;
            
            #if UNITY_SPEEDTREE
            var speedTreeComponent = GetComponent<SpeedTreeWind>();
            if (speedTreeComponent != null)
            {
                // Update wind based on stress level
                speedTreeComponent.m_WindStrength = _windStrength * (1f + _stressLevel / 100f);
                
                // Update based on growth stage
                float maturityScale = Mathf.Lerp(0.3f, 1.0f, _maturityLevel);
                speedTreeComponent.transform.localScale = _plantScale * maturityScale;
            }
            #endif
        }
        
        /// <summary>
        /// Update environmental conditions
        /// </summary>
        public void UpdateEnvironmentalConditions(EnvironmentalConditions conditions)
        {
            _currentConditions = conditions;
            
            // Calculate stress based on optimal conditions
            _isStressed = IsEnvironmentStressful(conditions);
            
            // Update environmental adaptation
            _environmentalAdaptation = CalculateEnvironmentalAdaptation(conditions);
        }
        
        /// <summary>
        /// Check if environment is stressful
        /// </summary>
        private bool IsEnvironmentStressful(EnvironmentalConditions conditions)
        {
            bool tempStress = conditions.Temperature < 18f || conditions.Temperature > 30f;
            bool humidityStress = conditions.Humidity < 40f || conditions.Humidity > 80f;
            bool lightStress = conditions.LightIntensity < 20f;
            
            return tempStress || humidityStress || lightStress;
        }
        
        /// <summary>
        /// Calculate environmental adaptation
        /// </summary>
        private float CalculateEnvironmentalAdaptation(EnvironmentalConditions conditions)
        {
            float adaptation = 1.0f;
            
            // Temperature adaptation
            float tempOptimal = 24f;
            float tempDiff = Mathf.Abs(conditions.Temperature - tempOptimal);
            adaptation *= Mathf.Max(0.2f, 1f - (tempDiff / 15f));
            
            // Humidity adaptation
            float humidityOptimal = 60f;
            float humidityDiff = Mathf.Abs(conditions.Humidity - humidityOptimal);
            adaptation *= Mathf.Max(0.2f, 1f - (humidityDiff / 30f));
            
            return Mathf.Clamp01(adaptation);
        }
        
        /// <summary>
        /// Harvest the plant
        /// </summary>
        public PlantHarvestResult HarvestPlant()
        {
            if (!CanHarvest)
            {
                Debug.LogWarning($"Cannot harvest plant {_plantId} - not ready for harvest");
                return null;
            }
            
            var harvestResult = new PlantHarvestResult
            {
                PlantId = _plantId,
                PlantStrain = _plantStrain,
                HarvestWeight = CalculateHarvestWeight(),
                Quality = CalculateHarvestQuality(),
                HarvestDate = DateTime.Now,
                GrowthTimeTotal = _totalGrowthTime,
                MaturityAtHarvest = _maturityLevel
            };
            
            // Change to harvested state
            SetGrowthStage(DataPlantGrowthStage.Harvested);
            
            OnPlantHarvested?.Invoke(this);
            
            Debug.Log($"Harvested plant {_plantId}: {harvestResult.HarvestWeight}g at {harvestResult.Quality}% quality");
            
            return harvestResult;
        }
        
        /// <summary>
        /// Calculate harvest weight
        /// </summary>
        private float CalculateHarvestWeight()
        {
            float baseWeight = _plantStrain?.BaseYield() ?? 50f;
            float healthModifier = _health / 100f;
            float maturityModifier = _maturityLevel;
            float environmentalModifier = _environmentalAdaptation;
            
            return baseWeight * healthModifier * maturityModifier * environmentalModifier;
        }
        
        /// <summary>
        /// Calculate harvest quality
        /// </summary>
        private float CalculateHarvestQuality()
        {
            float baseQuality = 70f; // Base quality percentage
            float healthBonus = (_health - 70f) * 0.3f; // Health above 70% adds quality
            float maturityBonus = (_maturityLevel - 0.8f) * 50f; // Maturity above 80% adds quality
            float stressPenalty = _stressLevel * 0.5f; // Stress reduces quality
            
            return Mathf.Clamp(baseQuality + healthBonus + maturityBonus - stressPenalty, 10f, 100f);
        }
        
        /// <summary>
        /// Get plant information for UI display
        /// </summary>
        public PlantDisplayInfo GetDisplayInfo()
        {
            return new PlantDisplayInfo
            {
                PlantId = _plantId,
                StrainName = _plantStrain?.StrainName ?? "Unknown",
                GrowthStage = _currentGrowthStage,
                Health = _health,
                MaturityLevel = _maturityLevel,
                GrowthRate = _growthRate,
                IsStressed = _isStressed,
                CanHarvest = CanHarvest,
                PlantedDate = _plantedDate,
                TotalGrowthTime = _totalGrowthTime
            };
        }
        
        /// <summary>
        /// Convert DataPlantGrowthStage to PlantGrowthStage for compatibility
        /// </summary>
        private PlantGrowthStage ConvertToPlantGrowthStage(DataPlantGrowthStage dataStage)
        {
            switch (dataStage)
            {
                case DataPlantGrowthStage.Seed: return PlantGrowthStage.Seed;
                case DataPlantGrowthStage.Germination: return PlantGrowthStage.Germination;
                case DataPlantGrowthStage.Seedling: return PlantGrowthStage.Seedling;
                case DataPlantGrowthStage.Vegetative: return PlantGrowthStage.Vegetative;
                case DataPlantGrowthStage.Flowering: return PlantGrowthStage.Flowering;
                case DataPlantGrowthStage.Harvest: return PlantGrowthStage.Harvest;
                case DataPlantGrowthStage.Harvested: return PlantGrowthStage.Harvest;
                default: return PlantGrowthStage.Seedling;
            }
        }
        
        private void OnDestroy()
        {
            // Unregister from managers
            if (_plantManager != null)
            {
                _plantManager.UnregisterPlantInstance(_plantId);
            }
        }
    }
    
    /// <summary>
    /// Plant harvest result data
    /// </summary>
    [System.Serializable]
    public class PlantHarvestResult
    {
        public string PlantId;
        public PlantStrainSO PlantStrain;
        public float HarvestWeight;
        public float Quality;
        public DateTime HarvestDate;
        public float GrowthTimeTotal;
        public float MaturityAtHarvest;
    }
    
    /// <summary>
    /// Plant display information for UI
    /// </summary>
    [System.Serializable]
    public class PlantDisplayInfo
    {
        public string PlantId;
        public string StrainName;
        public DataPlantGrowthStage GrowthStage;
        public float Health;
        public float MaturityLevel;
        public float GrowthRate;
        public bool IsStressed;
        public bool CanHarvest;
        public DateTime PlantedDate;
        public float TotalGrowthTime;
    }
}

#if UNITY_SPEEDTREE
// SpeedTree wind component integration
namespace ProjectChimera.Systems.SpeedTree
{
    public class SpeedTreeWind : MonoBehaviour
    {
        public float m_WindStrength = 1.0f;
        public float m_WindFrequency = 1.0f;
    }
}
#endif