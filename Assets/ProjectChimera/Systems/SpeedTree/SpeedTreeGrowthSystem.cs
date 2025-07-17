using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Progression;
using ProjectChimera.Systems.SpeedTree;
using ProjectChimera.Systems.Environment;
using ProjectChimera.Systems.Effects;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

// Explicit alias for Data layer EnvironmentalConditions to resolve namespace conflicts
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
using SeasonType = ProjectChimera.Data.Environment.SeasonType;
// Alias to resolve EffectType ambiguity
using EffectsEffectType = ProjectChimera.Systems.Effects.EffectType;
// Note: Removed references to removed Systems namespaces (Cultivation, Environment, Effects, Progression)
// These will need to be replaced with data-driven approaches or proper Data layer references

#if UNITY_SPEEDTREE
using SpeedTree;
#endif

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Comprehensive SpeedTree growth animation and lifecycle integration system.
    /// Provides realistic cannabis plant growth with genetic variation, environmental response,
    /// stage-based development, and sophisticated animation systems for photorealistic cultivation.
    /// </summary>
    public class SpeedTreeGrowthSystem : ChimeraManager
    {
        [Header("Growth Configuration")]
        [SerializeField] private SpeedTreeGrowthConfigSO _growthConfig;
        [SerializeField] private CannabisLifecycleConfigSO _lifecycleConfig;
        [SerializeField] private GrowthAnimationConfigSO _animationConfig;
        
        [Header("Growth Timing")]
        [SerializeField] private bool _enableRealTimeGrowth = true;
        [SerializeField] private bool _enableAcceleratedGrowth = false;
        [SerializeField] private float _growthTimeMultiplier = 1f;
        [SerializeField] private float _stageDurationMultiplier = 1f;
        
        [Header("Visual Growth")]
        [SerializeField] private bool _enableSmoothTransitions = true;
        [SerializeField] private bool _enableMorphologicalChanges = true;
        [SerializeField] private bool _enableColorProgression = true;
        [SerializeField] private bool _enableSizeProgression = true;
        
        [Header("Lifecycle Integration")]
        [SerializeField] private bool _enableAutomaticStageProgression = true;
        [SerializeField] private bool _enableEnvironmentalTriggers = true;
        [SerializeField] private bool _enableGeneticVariation = true;
        [SerializeField] private bool _enableSeasonalEffects = true;
        
        [Header("Animation Systems")]
        [SerializeField] private bool _enableGrowthAnimation = true;
        [SerializeField] private bool _enableWindAnimation = true;
        [SerializeField] private bool _enableInteractionAnimation = true;
        [SerializeField] private float _animationTimeScale = 1f;
        
        // Core Growth Systems
        private CannabisLifecycleManager _lifecycleManager;
        private GrowthAnimationController _animationController;
        private StageTransitionManager _stageTransitionManager;
        private MorphologicalProgressionManager _morphologyManager;
        private ColorProgressionManager _colorManager;
        
        // Specialized Growth Components
        private BudDevelopmentSystem _budDevelopmentSystem;
        private TrichromeGrowthSystem _trichromeGrowthSystem;
        private RootGrowthSimulator _rootGrowthSimulator;
        private CanopyDevelopmentManager _canopyManager;
        
        // Growth State Tracking
        private Dictionary<int, PlantGrowthState> _plantGrowthStates = new Dictionary<int, PlantGrowthState>();
        private Dictionary<int, GrowthAnimationData> _growthAnimations = new Dictionary<int, GrowthAnimationData>();
        private Dictionary<int, LifecycleProgressData> _lifecycleProgress = new Dictionary<int, LifecycleProgressData>();
        
        // Environmental Integration
        private AdvancedSpeedTreeManager _speedTreeManager;
        private SpeedTreeEnvironmentalSystem _environmentalSystem;
        private CannabisGeneticsEngine _geneticsEngine;
        private EnvironmentalManager _environmentalManager;
        // private AdvancedEffectsManager _effectsManager; // Disabled - system not available
        // private ComprehensiveProgressionManager _progressionManager; // Disabled - using CleanProgressionManager instead
        
        // Performance and Optimization
        private GrowthPerformanceMetrics _performanceMetrics;
        private GrowthUpdateScheduler _updateScheduler;
        private LODGrowthManager _lodGrowthManager;
        
        // Growth Events and Callbacks
        private Queue<GrowthEvent> _pendingGrowthEvents = new Queue<GrowthEvent>();
        private List<GrowthMilestone> _achievedMilestones = new List<GrowthMilestone>();
        
        // Coroutines
        private Coroutine _growthUpdateCoroutine;
        private Coroutine _animationUpdateCoroutine;
        
        // Events
        public System.Action<int, PlantGrowthStage, PlantGrowthStage> OnStageTransition;
        public System.Action<int, GrowthMilestone> OnGrowthMilestone;
        public System.Action<int, float> OnGrowthProgress;
        public System.Action<int, BudDevelopmentData> OnBudDevelopment;
        public System.Action<int, TrichromeData> OnTrichromeGrowth;
        public System.Action<GrowthPerformanceMetrics> OnPerformanceMetricsUpdated;
        
        // Properties
        public GrowthPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        public int ActiveGrowingPlants => _plantGrowthStates.Count(p => p.Value.IsActivelyGrowing);
        public bool GrowthSystemEnabled => _enableRealTimeGrowth;
        
        protected override void OnManagerInitialize()
        {
            InitializeGrowthSystems();
            InitializeLifecycleManagement();
            ConnectToGameSystems();
            InitializePerformanceMonitoring();
            StartGrowthUpdateLoops();
            LogInfo("SpeedTree Growth System initialized");
        }
        
        private void Update()
        {
            UpdateGrowthSystems();
            ProcessGrowthEvents();
            UpdatePerformanceMetrics();
        }
        
        private void LateUpdate()
        {
            UpdateAnimationSystems();
            UpdateLODGrowth();
        }
        
        #region Initialization
        
        private void InitializeGrowthSystems()
        {
            // Initialize core growth managers
            _lifecycleManager = new CannabisLifecycleManager(_lifecycleConfig, _enableAutomaticStageProgression);
            _animationController = new GrowthAnimationController(_animationConfig, _enableGrowthAnimation);
            _stageTransitionManager = new StageTransitionManager(_enableSmoothTransitions);
            _morphologyManager = new MorphologicalProgressionManager(_enableMorphologicalChanges);
            _colorManager = new ColorProgressionManager(_enableColorProgression);
            
            // Initialize specialized systems
            _budDevelopmentSystem = new BudDevelopmentSystem();
            _trichromeGrowthSystem = new TrichromeGrowthSystem();
            _rootGrowthSimulator = new RootGrowthSimulator();
            _canopyManager = new CanopyDevelopmentManager();
            
            // Initialize optimization systems
            _updateScheduler = new GrowthUpdateScheduler();
            _lodGrowthManager = new LODGrowthManager();
            
            LogInfo("Growth subsystems initialized");
        }
        
        private void InitializeLifecycleManagement()
        {
            // Configure lifecycle timings based on settings
            if (_lifecycleConfig != null)
            {
                _lifecycleManager.SetGrowthMultiplier(_growthTimeMultiplier);
                _lifecycleManager.SetStageDurationMultiplier(_stageDurationMultiplier);
                _lifecycleManager.SetAcceleratedGrowth(_enableAcceleratedGrowth);
            }
            
            LogInfo("Lifecycle management initialized");
        }
        
        private void ConnectToGameSystems()
        {
            if (GameManager.Instance != null)
            {
                _speedTreeManager = GameManager.Instance.GetManager<AdvancedSpeedTreeManager>();
                _environmentalSystem = GameManager.Instance.GetManager<SpeedTreeEnvironmentalSystem>();
                _geneticsEngine = GameManager.Instance.GetManager<CannabisGeneticsEngine>();
                _environmentalManager = GameManager.Instance.GetManager<EnvironmentalManager>();
                // _effectsManager = GameManager.Instance.GetManager<AdvancedEffectsManager>(); // Disabled
                // _progressionManager = GameManager.Instance.GetManager<ComprehensiveProgressionManager>(); // Disabled
            }
            
            ConnectSystemEvents();
        }
        
        private void ConnectSystemEvents()
        {
            if (_speedTreeManager != null)
            {
                _speedTreeManager.OnPlantInstanceCreated += HandlePlantInstanceCreated;
                _speedTreeManager.OnPlantInstanceDestroyed += HandlePlantInstanceDestroyed;
            }
            
            if (_environmentalSystem != null)
            {
                _environmentalSystem.OnPlantStressChanged += HandlePlantStressChanged;
                _environmentalSystem.OnPlantAdapted += HandlePlantAdapted;
            }
            
            if (_environmentalManager != null)
            {
                _environmentalManager.OnConditionsChanged += HandleEnvironmentalChange;
                _environmentalManager.OnSeasonChanged += HandleSeasonChange;
            }
        }
        
        private void InitializePerformanceMonitoring()
        {
            _performanceMetrics = new GrowthPerformanceMetrics
            {
                SystemStartTime = DateTime.Now,
                ActiveGrowingPlants = 0,
                GrowthUpdatesPerSecond = 0f,
                AnimationUpdatesPerSecond = 0f,
                AverageGrowthRate = 0f,
                MemoryUsage = 0f
            };
            
            InvokeRepeating(nameof(UpdateDetailedPerformanceMetrics), 1f, 5f);
        }
        
        private void StartGrowthUpdateLoops()
        {
            _growthUpdateCoroutine = StartCoroutine(GrowthUpdateCoroutine());
            _animationUpdateCoroutine = StartCoroutine(AnimationUpdateCoroutine());
        }
        
        private IEnumerator GrowthUpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f); // 10 FPS for growth updates
                
                if (_enableRealTimeGrowth)
                {
                    ProcessPlantGrowth();
                }
                
                ProcessStageTransitions();
                ProcessGrowthMilestones();
            }
        }
        
        private IEnumerator AnimationUpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                
                if (_enableGrowthAnimation)
                {
                    UpdateGrowthAnimations();
                }
                
                UpdateVisualProgression();
            }
        }
        
        #endregion
        
        #region Plant Growth State Management
        
        public void RegisterPlantForGrowth(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            if (instance == null) return;
            
            var growthState = new PlantGrowthState
            {
                InstanceId = instance.InstanceId,
                PlantInstance = instance,
                CurrentStage = instance.GrowthStage,
                GrowthProgress = 0f,
                StageProgress = 0f,
                GrowthRate = CalculateInitialGrowthRate(instance),
                LastUpdateTime = Time.time,
                IsActivelyGrowing = true,
                GrowthStartTime = DateTime.Now
            };
            
            _plantGrowthStates[instance.InstanceId] = growthState;
            
            // Initialize growth animation
            var animationData = new GrowthAnimationData
            {
                InstanceId = instance.InstanceId,
                AnimationStartTime = Time.time,
                CurrentAnimationPhase = GrowthAnimationPhase.Initial,
                AnimationSpeed = _animationTimeScale,
                TargetSize = CalculateTargetSize(instance),
                CurrentSize = instance.Scale.x
            };
            
            _growthAnimations[instance.InstanceId] = animationData;
            
            // Initialize lifecycle progress
            var lifecycleData = new LifecycleProgressData
            {
                InstanceId = instance.InstanceId,
                LifecycleStartTime = DateTime.Now,
                EstimatedHarvestTime = CalculateEstimatedHarvestTime(instance),
                StageHistory = new List<StageTransitionRecord>(),
                GeneticModifiers = ExtractGeneticModifiers(instance)
            };
            
            _lifecycleProgress[instance.InstanceId] = lifecycleData;
            
            // Start specialized growth systems
            _budDevelopmentSystem.InitializeBudGrowth(instance);
            _trichromeGrowthSystem.InitializeTrichromeGrowth(instance);
            _rootGrowthSimulator.InitializeRootSystem(instance);
            
            LogInfo($"Registered plant {instance.InstanceId} for growth tracking");
        }
        
        public void UnregisterPlantFromGrowth(int instanceId)
        {
            _plantGrowthStates.Remove(instanceId);
            _growthAnimations.Remove(instanceId);
            _lifecycleProgress.Remove(instanceId);
            
            _budDevelopmentSystem.RemovePlant(instanceId);
            _trichromeGrowthSystem.RemovePlant(instanceId);
            _rootGrowthSimulator.RemovePlant(instanceId);
            
            LogInfo($"Unregistered plant {instanceId} from growth tracking");
        }
        
        private float CalculateInitialGrowthRate(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            var baseRate = 1f;
            
            // Apply genetic modifiers
            if (instance.GeneticData != null)
            {
                baseRate *= instance.GeneticData.GrowthRate;
            }
            
            // Apply environmental modifiers
            foreach (var modifier in instance.EnvironmentalModifiers)
            {
                if (modifier.Key.Contains("growth"))
                {
                    baseRate *= modifier.Value;
                }
            }
            
            return baseRate;
        }
        
        private Vector3 CalculateTargetSize(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            var baseSize = Vector3.one;
            
            if (instance.GeneticData != null)
            {
                baseSize *= instance.GeneticData.PlantSize;
            }
            
            if (instance.StrainAsset != null)
            {
                var heightMultiplier = Mathf.Lerp(instance.StrainAsset.HeightRange.x, 
                                                 instance.StrainAsset.HeightRange.y, 0.8f) / 2f;
                var widthMultiplier = Mathf.Lerp(instance.StrainAsset.WidthRange.x, 
                                                instance.StrainAsset.WidthRange.y, 0.8f) / 2f;
                
                baseSize = new Vector3(widthMultiplier, heightMultiplier, widthMultiplier);
            }
            
            return baseSize;
        }
        
        private DateTime CalculateEstimatedHarvestTime(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            var totalLifecycleDays = 120f; // Default 4 months
            
            if (instance.StrainAsset != null)
            {
                var floweringWeeks = instance.StrainAsset.FloweringTimeRange.y;
                totalLifecycleDays = (8f + floweringWeeks) * 7f; // Veg + flower in days
            }
            
            // Apply growth rate modifiers
            if (instance.GeneticData != null)
            {
                totalLifecycleDays /= instance.GeneticData.GrowthRate;
            }
            
            totalLifecycleDays *= _stageDurationMultiplier;
            
            return DateTime.Now.AddDays(totalLifecycleDays);
        }
        
        private Dictionary<string, float> ExtractGeneticModifiers(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            var modifiers = new Dictionary<string, float>();
            
            if (instance.GeneticData != null)
            {
                modifiers["growth_rate"] = instance.GeneticData.GrowthRate;
                modifiers["flowering_speed"] = instance.GeneticData.FloweringSpeed;
                modifiers["yield_potential"] = instance.GeneticData.YieldPotential;
                modifiers["bud_density"] = instance.GeneticData.BudDensity;
                modifiers["trichrome_amount"] = instance.GeneticData.TrichromeAmount;
            }
            
            return modifiers;
        }
        
        #endregion
        
        #region Growth Processing
        
        private void ProcessPlantGrowth()
        {
            var plantsToUpdate = _updateScheduler.GetPlantsForUpdate(_plantGrowthStates.Values.ToList());
            
            foreach (var growthState in plantsToUpdate)
            {
                ProcessIndividualPlantGrowth(growthState);
            }
        }
        
        private void ProcessIndividualPlantGrowth(PlantGrowthState growthState)
        {
            if (!growthState.IsActivelyGrowing) return;
            
            var deltaTime = Time.time - growthState.LastUpdateTime;
            var growthIncrement = CalculateGrowthIncrement(growthState, deltaTime);
            
            // Update growth progress
            growthState.GrowthProgress += growthIncrement;
            growthState.StageProgress += growthIncrement / GetStageProgressRequirement(growthState.CurrentStage);
            
            // Apply growth to visual representation
            ApplyGrowthToPlant(growthState, growthIncrement);
            
            // Check for stage transitions
            if (growthState.StageProgress >= 1f && _enableAutomaticStageProgression)
            {
                var nextStage = _lifecycleManager.GetNextStage(growthState.CurrentStage);
                if (nextStage != growthState.CurrentStage)
                {
                    TriggerStageTransition(growthState, nextStage);
                }
            }
            
            // Update specialized growth systems
            UpdateSpecializedGrowthSystems(growthState, deltaTime);
            
            // Check for growth milestones
            CheckForGrowthMilestones(growthState);
            
            // Update metrics
            OnGrowthProgress?.Invoke(growthState.InstanceId, growthState.GrowthProgress);
            
            growthState.LastUpdateTime = Time.time;
        }
        
        private float CalculateGrowthIncrement(PlantGrowthState growthState, float deltaTime)
        {
            var baseRate = growthState.GrowthRate;
            
            // Apply environmental factors
            var environmentalMultiplier = CalculateEnvironmentalGrowthMultiplier(growthState);
            
            // Apply genetic factors
            var geneticMultiplier = CalculateGeneticGrowthMultiplier(growthState);
            
            // Apply stage-specific factors
            var stageMultiplier = GetStageGrowthMultiplier(growthState.CurrentStage);
            
            // Apply time multipliers
            var timeMultiplier = _growthTimeMultiplier * (_enableAcceleratedGrowth ? 10f : 1f);
            
            return baseRate * environmentalMultiplier * geneticMultiplier * stageMultiplier * timeMultiplier * deltaTime;
        }
        
        private float CalculateEnvironmentalGrowthMultiplier(PlantGrowthState growthState)
        {
            var multiplier = 1f;
            var instance = growthState.PlantInstance;
            
            // Get environmental conditions
            if (_environmentalSystem != null)
            {
                var plantEnvState = _environmentalSystem.GetPlantState(instance.InstanceId);
                if (plantEnvState != null)
                {
                    var stressLevel = plantEnvState.StressData.OverallStress;
                    multiplier = Mathf.Lerp(1f, 0.3f, stressLevel); // Stress reduces growth
                }
            }
            
            // Apply environmental modifiers from plant
            foreach (var modifier in instance.EnvironmentalModifiers)
            {
                if (modifier.Key.Contains("growth") || modifier.Key.Contains("efficiency"))
                {
                    multiplier *= modifier.Value;
                }
            }
            
            return Mathf.Clamp(multiplier, 0.1f, 2f);
        }
        
        private float CalculateGeneticGrowthMultiplier(PlantGrowthState growthState)
        {
            var multiplier = 1f;
            var instance = growthState.PlantInstance;
            
            if (instance.GeneticData != null)
            {
                multiplier *= instance.GeneticData.GrowthRate;
                
                // Stage-specific genetic modifiers
                switch (growthState.CurrentStage)
                {
                    case PlantGrowthStage.Flowering:
                        multiplier *= instance.GeneticData.FloweringSpeed;
                        break;
                    case PlantGrowthStage.Vegetative:
                        multiplier *= 1.2f; // Vegetative growth bonus
                        break;
                }
            }
            
            return multiplier;
        }
        
        private float GetStageGrowthMultiplier(PlantGrowthStage stage)
        {
            return stage switch
            {
                PlantGrowthStage.Seedling => 2f,     // Fast initial growth
                PlantGrowthStage.Vegetative => 1.5f, // Active vegetative growth
                PlantGrowthStage.Flowering => 0.8f,  // Slower growth during flowering
                PlantGrowthStage.Harvest => 0.1f,    // Minimal growth at harvest
                _ => 1f
            };
        }
        
        private float GetStageProgressRequirement(PlantGrowthStage stage)
        {
            if (_lifecycleConfig == null) return 1f;
            
            return stage switch
            {
                PlantGrowthStage.Seedling => _lifecycleConfig.SeedlingDuration,
                PlantGrowthStage.Vegetative => _lifecycleConfig.VegetativeDuration,
                PlantGrowthStage.Flowering => _lifecycleConfig.FloweringDuration,
                _ => 1f
            };
        }
        
        private void ApplyGrowthToPlant(PlantGrowthState growthState, float growthIncrement)
        {
            var instance = growthState.PlantInstance;
            if (instance?.Renderer == null) return;
            
            // Apply size progression
            if (_enableSizeProgression)
            {
                _morphologyManager.UpdatePlantSize(instance, growthState.GrowthProgress);
            }
            
            // Apply color progression
            if (_enableColorProgression)
            {
                _colorManager.UpdatePlantColors(instance, growthState.CurrentStage, growthState.StageProgress);
            }
            
            // Apply morphological changes
            if (_enableMorphologicalChanges)
            {
                _morphologyManager.UpdatePlantMorphology(instance, growthState.CurrentStage, growthState.StageProgress);
            }
        }
        
        private void UpdateSpecializedGrowthSystems(PlantGrowthState growthState, float deltaTime)
        {
            var instance = growthState.PlantInstance;
            
            // Update bud development
            var budProgress = _budDevelopmentSystem.UpdateBudGrowth(instance, deltaTime);
            if (budProgress.HasSignificantChange)
            {
                OnBudDevelopment?.Invoke(instance.InstanceId, budProgress);
            }
            
            // Update trichrome growth
            var trichromeProgress = _trichromeGrowthSystem.UpdateTrichromeGrowth(instance, deltaTime);
            if (trichromeProgress.HasSignificantChange)
            {
                OnTrichromeGrowth?.Invoke(instance.InstanceId, trichromeProgress);
            }
            
            // Update root growth
            _rootGrowthSimulator.UpdateRootGrowth(instance, deltaTime);
            
            // Update canopy development
            _canopyManager.UpdateCanopyVisualization(instance);
        }
        
        #endregion
        
        #region Stage Transitions
        
        private void ProcessStageTransitions()
        {
            // Process any pending stage transitions
            while (_pendingGrowthEvents.Count > 0)
            {
                var growthEvent = _pendingGrowthEvents.Dequeue();
                ProcessGrowthEvent(growthEvent);
            }
        }
        
        public void TriggerStageTransition(PlantGrowthState growthState, PlantGrowthStage newStage)
        {
            var oldStage = growthState.CurrentStage;
            
            // Record transition in lifecycle progress
            if (_lifecycleProgress.TryGetValue(growthState.InstanceId, out var lifecycleData))
            {
                var transitionRecord = new StageTransitionRecord
                {
                    FromStage = oldStage,
                    ToStage = newStage,
                    TransitionTime = DateTime.Now,
                    GrowthProgress = growthState.GrowthProgress,
                    EnvironmentalConditions = GetCurrentEnvironmentalConditions(growthState.PlantInstance)
                };
                
                lifecycleData.StageHistory.Add(transitionRecord);
            }
            
            // Execute stage transition
            _stageTransitionManager.ExecuteStageTransition(growthState, oldStage, newStage);
            
            // Update growth state
            growthState.CurrentStage = newStage;
            growthState.StageProgress = 0f;
            growthState.GrowthRate = CalculateInitialGrowthRate(growthState.PlantInstance);
            
            // Apply stage-specific visual changes
            ApplyStageVisualChanges(growthState, newStage);
            
            // Trigger stage-specific effects
            TriggerStageTransitionEffects(growthState, oldStage, newStage);
            
            // Update specialized systems for new stage
            UpdateSpecializedSystemsForStage(growthState, newStage);
            
            // Notify systems and UI
            OnStageTransition?.Invoke(growthState.InstanceId, oldStage, newStage);
            
            // Award progression experience
            // if (_progressionManager != null)
            // {
            //     var experienceGain = CalculateStageTransitionExperience(oldStage, newStage);
            //     _progressionManager.GainExperience("cultivation", experienceGain, $"Stage Transition: {newStage}");
            // }
            
            LogInfo($"Plant {growthState.InstanceId} transitioned from {oldStage} to {newStage}");
        }
        
        private void ApplyStageVisualChanges(PlantGrowthState growthState, PlantGrowthStage newStage)
        {
            var instance = growthState.PlantInstance;
            if (instance?.Renderer == null) return;
            
#if UNITY_SPEEDTREE
            var stageConfig = _growthConfig?.GetStageConfiguration(newStage);
            if (stageConfig == null) return;
            
            // Apply stage-specific visual properties
            instance.Renderer.materialProperties?.SetFloat("_StageProgress", 0f);
            
            // Update colors for new stage
            switch (newStage)
            {
                case PlantGrowthStage.Seedling:
                    instance.Renderer.materialProperties?.SetColor("_LeafColor", Color.green * 0.8f);
                    instance.Renderer.materialProperties?.SetFloat("_BudVisibility", 0f);
                    break;
                    
                case PlantGrowthStage.Vegetative:
                    instance.Renderer.materialProperties?.SetColor("_LeafColor", Color.green);
                    instance.Renderer.materialProperties?.SetFloat("_LeafDensity", 1f);
                    instance.Renderer.materialProperties?.SetFloat("_BudVisibility", 0.1f);
                    break;
                    
                case PlantGrowthStage.Flowering:
                    instance.Renderer.materialProperties?.SetFloat("_BudVisibility", 0.8f);
                    instance.Renderer.materialProperties?.SetColor("_BudColor", instance.GeneticData?.BudColor ?? Color.green);
                    instance.Renderer.materialProperties?.SetFloat("_TrichromeVisibility", 0.3f);
                    break;
                    
                case PlantGrowthStage.Harvest:
                    instance.Renderer.materialProperties?.SetFloat("_BudVisibility", 1f);
                    instance.Renderer.materialProperties?.SetFloat("_TrichromeVisibility", 1f);
                    instance.Renderer.materialProperties?.SetColor("_TrichromeColor", Color.white);
                    break;
            }
#endif
        }
        
        private void TriggerStageTransitionEffects(PlantGrowthState growthState, PlantGrowthStage oldStage, PlantGrowthStage newStage)
        {
            var instance = growthState.PlantInstance;
            if (instance?.Renderer == null) return;
            
            // Play visual effects
            // var effectType = GetStageTransitionEffectType(newStage);
            // _effectsManager.PlayEffect(effectType, instance.Position, instance.Renderer?.transform, 3f);
            
            // Play stage-specific sounds
            // var audioEffectName = GetStageTransitionAudioEffectName(newStage);
            // if (!string.IsNullOrEmpty(audioEffectName))
            // {
            //     _effectsManager.PlayAudioEffect(audioEffectName, instance.Position, 0.7f);
            // }
        }
        
        private EffectsEffectType GetStageTransitionEffectType(PlantGrowthStage stage)
        {
            return stage switch
            {
                PlantGrowthStage.Seedling => EffectsEffectType.PlantGrowth,
                PlantGrowthStage.Vegetative => EffectsEffectType.PlantGrowth,
                PlantGrowthStage.Flowering => EffectsEffectType.PlantFlowering,
                PlantGrowthStage.Harvest => EffectsEffectType.PlantMaturation,
                _ => EffectsEffectType.PlantGrowth
            };
        }
        
        private string GetStageTransitionAudioEffectName(PlantGrowthStage stage)
        {
            // This would reference actual audio clips
            return null; // Placeholder
        }
        
        private void UpdateSpecializedSystemsForStage(PlantGrowthState growthState, PlantGrowthStage newStage)
        {
            var instance = growthState.PlantInstance;
            
            switch (newStage)
            {
                case PlantGrowthStage.Flowering:
                    _budDevelopmentSystem.ActivateBudGrowth(instance);
                    _trichromeGrowthSystem.ActivateTrichromeGrowth(instance);
                    break;
                    
                case PlantGrowthStage.Harvest:
                    _budDevelopmentSystem.FinalizebudDevelopment(instance);
                    _trichromeGrowthSystem.MaximizeTrichromeProduction(instance);
                    break;
            }
        }
        
        private float CalculateStageTransitionExperience(PlantGrowthStage oldStage, PlantGrowthStage newStage)
        {
            return newStage switch
            {
                PlantGrowthStage.Vegetative => 25f,
                PlantGrowthStage.Flowering => 50f,
                PlantGrowthStage.Harvest => 100f,
                _ => 10f
            };
        }
        
        #endregion
        
        #region Growth Animation System
        
        private void UpdateGrowthAnimations()
        {
            foreach (var animationData in _growthAnimations.Values)
            {
                UpdateIndividualGrowthAnimation(animationData);
            }
        }
        
        private void UpdateIndividualGrowthAnimation(GrowthAnimationData animationData)
        {
            if (!_plantGrowthStates.TryGetValue(animationData.InstanceId, out var growthState)) return;
            
            var instance = growthState.PlantInstance;
            if (instance?.Renderer == null) return;
            
            // Update animation based on growth progress
            var animationProgress = CalculateAnimationProgress(animationData, growthState);
            
            // Apply smooth scaling animation
            if (_enableSizeProgression)
            {
                UpdateSizeAnimation(instance, animationData, animationProgress);
            }
            
            // Apply morphological animations
            if (_enableMorphologicalChanges)
            {
                UpdateMorphologyAnimation(instance, animationData, animationProgress);
            }
            
            // Apply wind animation
            if (_enableWindAnimation)
            {
                UpdateWindAnimation(instance, animationData);
            }
            
            // Update animation phase
            UpdateAnimationPhase(animationData, growthState);
        }
        
        private float CalculateAnimationProgress(GrowthAnimationData animationData, PlantGrowthState growthState)
        {
            var timeSinceStart = Time.time - animationData.AnimationStartTime;
            var progress = growthState.GrowthProgress;
            
            // Smooth the progress using animation curves
            if (_animationConfig?.GrowthAnimationCurve != null)
            {
                progress = _animationConfig.GrowthAnimationCurve.Evaluate(progress);
            }
            
            return progress;
        }
        
        private void UpdateSizeAnimation(AdvancedSpeedTreeManager.SpeedTreePlantData instance, GrowthAnimationData animationData, float progress)
        {
            var targetSize = animationData.TargetSize;
            var currentSize = Vector3.Lerp(Vector3.one * animationData.CurrentSize, targetSize, progress);
            
            if (instance.Renderer?.transform != null)
            {
                instance.Renderer.transform.localScale = currentSize;
            }
        }
        
        private void UpdateMorphologyAnimation(AdvancedSpeedTreeManager.SpeedTreePlantData instance, GrowthAnimationData animationData, float progress)
        {
#if UNITY_SPEEDTREE
            if (instance.Renderer?.materialProperties == null) return;
            
            // Animate leaf development
            var leafDevelopment = Mathf.Lerp(0.1f, 1f, progress);
            instance.Renderer.materialProperties.SetFloat("_LeafDevelopment", leafDevelopment);
            
            // Animate stem thickness
            var stemThickness = Mathf.Lerp(0.5f, 1f, progress);
            instance.Renderer.materialProperties.SetFloat("_StemThickness", stemThickness);
            
            // Animate branch density
            var branchDensity = Mathf.Lerp(0.3f, 1f, progress);
            instance.Renderer.materialProperties.SetFloat("_BranchDensity", branchDensity);
#endif
        }
        
        private void UpdateWindAnimation(AdvancedSpeedTreeManager.SpeedTreePlantData instance, GrowthAnimationData animationData)
        {
#if UNITY_SPEEDTREE
            if (instance.Renderer?.materialProperties == null) return;
            
            var windStrength = GetCurrentWindStrength(instance.Position);
            var windDirection = GetCurrentWindDirection();
            
            // Apply wind response based on plant size and flexibility
            var windResponse = windStrength * (1f - animationData.CurrentSize * 0.3f);
            instance.Renderer.materialProperties.SetFloat("_WindResponse", windResponse);
            instance.Renderer.materialProperties.SetVector("_WindDirection", windDirection);
#endif
        }
        
        private void UpdateAnimationPhase(GrowthAnimationData animationData, PlantGrowthState growthState)
        {
            var newPhase = DetermineAnimationPhase(growthState);
            
            if (newPhase != animationData.CurrentAnimationPhase)
            {
                animationData.CurrentAnimationPhase = newPhase;
                animationData.PhaseTransitionTime = Time.time;
                
                // Adjust animation speed for new phase
                animationData.AnimationSpeed = GetAnimationSpeedForPhase(newPhase);
            }
        }
        
        private GrowthAnimationPhase DetermineAnimationPhase(PlantGrowthState growthState)
        {
            return growthState.CurrentStage switch
            {
                PlantGrowthStage.Seedling => GrowthAnimationPhase.EarlyGrowth,
                PlantGrowthStage.Vegetative => GrowthAnimationPhase.VegetativeGrowth,
                PlantGrowthStage.Flowering => GrowthAnimationPhase.FloweringGrowth,
                PlantGrowthStage.Harvest => GrowthAnimationPhase.MaturationPhase,
                _ => GrowthAnimationPhase.Initial
            };
        }
        
        private float GetAnimationSpeedForPhase(GrowthAnimationPhase phase)
        {
            return phase switch
            {
                GrowthAnimationPhase.EarlyGrowth => 2f,
                GrowthAnimationPhase.VegetativeGrowth => 1.5f,
                GrowthAnimationPhase.FloweringGrowth => 1f,
                GrowthAnimationPhase.MaturationPhase => 0.5f,
                _ => 1f
            };
        }
        
        #endregion
        
        #region Visual Progression Updates
        
        private void UpdateVisualProgression()
        {
            foreach (var growthState in _plantGrowthStates.Values)
            {
                if (ShouldUpdateVisualProgression(growthState))
                {
                    UpdatePlantVisualProgression(growthState);
                }
            }
        }
        
        private bool ShouldUpdateVisualProgression(PlantGrowthState growthState)
        {
            // Update visual progression based on distance from camera and importance
            var instance = growthState.PlantInstance;
            var distanceToCamera = Vector3.Distance(instance.Position, Camera.main?.transform.position ?? Vector3.zero);
            
            // Priority plants (flowering/harvest) get more frequent updates
            var isHighPriority = growthState.CurrentStage == PlantGrowthStage.Flowering || 
                               growthState.CurrentStage == PlantGrowthStage.Harvest;
            
            var updateFrequency = isHighPriority ? 0.1f : 0.5f;
            var distanceMultiplier = Mathf.Clamp01(50f / distanceToCamera);
            
            return Time.time - growthState.LastUpdateTime >= updateFrequency * distanceMultiplier;
        }
        
        private void UpdatePlantVisualProgression(PlantGrowthState growthState)
        {
            var instance = growthState.PlantInstance;
            
            // Update color progression
            _colorManager.UpdatePlantColors(instance, growthState.CurrentStage, growthState.StageProgress);
            
            // Update morphological progression
            _morphologyManager.UpdatePlantMorphology(instance, growthState.CurrentStage, growthState.StageProgress);
            
            // Update specialized visual elements
            UpdateSpecializedVisualElements(instance, growthState);
        }
        
        private void UpdateSpecializedVisualElements(AdvancedSpeedTreeManager.SpeedTreePlantData instance, PlantGrowthState growthState)
        {
            // Update bud development visualization
            if (_budDevelopmentSystem != null)
            {
                _budDevelopmentSystem.UpdateVisualBudDevelopment(instance, growthState.GrowthProgress);
            }
            
            // Update trichrome development
            if (_trichromeGrowthSystem != null)
            {
                _trichromeGrowthSystem.UpdateTrichromeVisualization(instance, growthState.GrowthProgress);
            }
            
            // Update root system visualization (if visible)
            if (_rootGrowthSimulator != null)
            {
                _rootGrowthSimulator.UpdateRootVisualization(instance);
            }
            
            // Update canopy development
            if (_canopyManager != null)
            {
                _canopyManager.UpdateCanopyVisualization(instance);
            }
        }
        
        #endregion
        
        #region Growth Milestones and Events
        
        private void CheckForGrowthMilestones(PlantGrowthState growthState)
        {
            var milestones = GetGrowthMilestonesForStage(growthState.CurrentStage);
            
            foreach (var milestone in milestones)
            {
                if (IsGrowthMilestoneAchieved(growthState, milestone) && 
                    !HasMilestoneBeenAchieved(growthState.InstanceId, milestone))
                {
                    AchieveGrowthMilestone(growthState, milestone);
                }
            }
        }
        
        private List<GrowthMilestone> GetGrowthMilestonesForStage(PlantGrowthStage stage)
        {
            return stage switch
            {
                PlantGrowthStage.Seedling => new List<GrowthMilestone>
                {
                    new GrowthMilestone { Id = "first_leaves", Name = "First True Leaves", ProgressThreshold = 0.3f },
                    new GrowthMilestone { Id = "established_seedling", Name = "Established Seedling", ProgressThreshold = 0.8f }
                },
                PlantGrowthStage.Vegetative => new List<GrowthMilestone>
                {
                    new GrowthMilestone { Id = "vigorous_growth", Name = "Vigorous Vegetative Growth", ProgressThreshold = 0.5f },
                    new GrowthMilestone { Id = "pre_flower", Name = "Pre-Flower Development", ProgressThreshold = 0.9f }
                },
                PlantGrowthStage.Flowering => new List<GrowthMilestone>
                {
                    new GrowthMilestone { Id = "flower_initiation", Name = "Flower Initiation", ProgressThreshold = 0.1f },
                    new GrowthMilestone { Id = "bud_development", Name = "Bud Development", ProgressThreshold = 0.5f },
                    new GrowthMilestone { Id = "trichrome_production", Name = "Trichrome Production", ProgressThreshold = 0.7f }
                },
                PlantGrowthStage.Harvest => new List<GrowthMilestone>
                {
                    new GrowthMilestone { Id = "harvest_ready", Name = "Harvest Ready", ProgressThreshold = 1f }
                },
                _ => new List<GrowthMilestone>()
            };
        }
        
        private bool IsGrowthMilestoneAchieved(PlantGrowthState growthState, GrowthMilestone milestone)
        {
            return growthState.StageProgress >= milestone.ProgressThreshold;
        }
        
        private bool HasMilestoneBeenAchieved(int instanceId, GrowthMilestone milestone)
        {
            return _achievedMilestones.Any(m => m.PlantInstanceId == instanceId && m.Id == milestone.Id);
        }
        
        private void AchieveGrowthMilestone(PlantGrowthState growthState, GrowthMilestone milestone)
        {
            milestone.PlantInstanceId = growthState.InstanceId;
            milestone.AchievementTime = DateTime.Now;
            
            _achievedMilestones.Add(milestone);
            
            // Trigger milestone effects
            TriggerMilestoneEffects(growthState, milestone);
            
            // Award progression experience
            // if (_progressionManager != null)
            // {
            //     var experienceGain = CalculateMilestoneExperience(milestone);
            //     _progressionManager.GainExperience("cultivation", experienceGain, $"Milestone: {milestone.Name}");
            // }
            
            OnGrowthMilestone?.Invoke(growthState.InstanceId, milestone);
            
            LogInfo($"Plant {growthState.InstanceId} achieved milestone: {milestone.Name}");
        }
        
        private void TriggerMilestoneEffects(PlantGrowthState growthState, GrowthMilestone milestone)
        {
            var instance = growthState.PlantInstance;
            if (instance?.Renderer == null) return;
            
            // Play visual effects
            // _effectsManager.PlayEffect(EffectsEffectType.Achievement, instance.Position, instance.Renderer?.transform, 2f);
            
            // Play milestone sound
            // var audioEffectName = GetMilestoneAudioEffectName(milestone.Id);
            // if (!string.IsNullOrEmpty(audioEffectName))
            // {
            //     _effectsManager.PlayAudioEffect(audioEffectName, instance.Position, 0.5f);
            // }
        }
        
        private float CalculateMilestoneExperience(GrowthMilestone milestone)
        {
            return milestone.Id switch
            {
                "first_leaves" => 10f,
                "established_seedling" => 15f,
                "vigorous_growth" => 25f,
                "pre_flower" => 30f,
                "flower_initiation" => 40f,
                "bud_development" => 50f,
                "trichrome_production" => 60f,
                "harvest_ready" => 100f,
                _ => 20f
            };
        }
        
        private string GetMilestoneAudioEffectName(string milestoneId)
        {
            // This would reference actual audio clips
            return null; // Placeholder
        }
        
        private void ProcessGrowthMilestones()
        {
            // Process any pending milestone achievements
            foreach (var growthState in _plantGrowthStates.Values)
            {
                CheckForGrowthMilestones(growthState);
            }
        }
        
        #endregion
        
        #region Environmental Integration
        
        private EnvironmentalConditions GetCurrentEnvironmentalConditions(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            if (_environmentalManager != null)
            {
                return _environmentalManager.GetConditionsAtPosition(instance.Position);
            }
            
            return new EnvironmentalConditions
            {
                Temperature = 24f,
                Humidity = 60f,
                LightIntensity = 800f,
                CO2Level = 400f,
                AirVelocity = 0.3f
            };
        }
        
        private float GetCurrentWindStrength(Vector3 position)
        {
            if (_environmentalManager != null)
            {
                var conditions = _environmentalManager.GetConditionsAtPosition(position);
                return conditions.AirVelocity;
            }
            
            return 0.3f; // Default wind strength
        }
        
        private Vector3 GetCurrentWindDirection()
        {
            // This would integrate with weather/wind systems
            return new Vector3(1f, 0f, 0f); // Default wind direction
        }
        
        #endregion
        
        #region Performance and System Updates
        
        private void UpdateGrowthSystems()
        {
            _lifecycleManager?.Update();
            _animationController?.Update();
            _stageTransitionManager?.Update();
            _morphologyManager?.Update();
            _colorManager?.Update();
            _budDevelopmentSystem?.Update();
            _trichromeGrowthSystem?.Update();
            _rootGrowthSimulator?.Update();
            _canopyManager?.Update();
            _updateScheduler?.Update();
        }
        
        private void UpdateAnimationSystems()
        {
            _animationController?.UpdateAnimations();
        }
        
        private void UpdateLODGrowth()
        {
            _lodGrowthManager?.UpdateLODs(_plantGrowthStates.Values.ToList());
        }
        
        private void ProcessGrowthEvents()
        {
            while (_pendingGrowthEvents.Count > 0)
            {
                var growthEvent = _pendingGrowthEvents.Dequeue();
                ProcessGrowthEvent(growthEvent);
            }
        }
        
        private void ProcessGrowthEvent(GrowthEvent growthEvent)
        {
            switch (growthEvent.EventType)
            {
                case GrowthEventType.StageTransition:
                    // Handle stage transition event
                    break;
                case GrowthEventType.Milestone:
                    // Handle milestone event
                    break;
                case GrowthEventType.EnvironmentalResponse:
                    // Handle environmental response event
                    break;
            }
        }
        
        private void UpdatePerformanceMetrics()
        {
            _performanceMetrics.ActiveGrowingPlants = _plantGrowthStates.Count(p => p.Value.IsActivelyGrowing);
            _performanceMetrics.AverageGrowthRate = CalculateAverageGrowthRate();
            _performanceMetrics.LastUpdate = DateTime.Now;
        }
        
        private float CalculateAverageGrowthRate()
        {
            if (_plantGrowthStates.Count == 0) return 0f;
            
            return _plantGrowthStates.Values.Average(p => p.GrowthRate);
        }
        
        private void UpdateDetailedPerformanceMetrics()
        {
            _performanceMetrics.GrowthUpdatesPerSecond = _plantGrowthStates.Count / 0.1f; // Update frequency
            _performanceMetrics.AnimationUpdatesPerSecond = _growthAnimations.Count * Time.deltaTime;
            _performanceMetrics.MemoryUsage = GC.GetTotalMemory(false) / 1024f / 1024f; // MB
            
            OnPerformanceMetricsUpdated?.Invoke(_performanceMetrics);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandlePlantInstanceCreated(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            RegisterPlantForGrowth(instance);
        }
        
        private void HandlePlantInstanceDestroyed(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            UnregisterPlantFromGrowth(instance.InstanceId);
        }
        
        private void HandlePlantStressChanged(int instanceId, EnvironmentalStressData stressData)
        {
            if (_plantGrowthStates.TryGetValue(instanceId, out var growthState))
            {
                // Adjust growth rate based on stress
                var stressPenalty = 1f - (stressData.OverallStress * 0.5f);
                growthState.GrowthRate *= stressPenalty;
            }
        }
        
        private void HandlePlantAdapted(int instanceId, EnvironmentalAdaptation adaptation)
        {
            if (_plantGrowthStates.TryGetValue(instanceId, out var growthState))
            {
                // Apply adaptation benefits to growth
                if (adaptation.EfficiencyImprovements.ContainsKey("growth_rate"))
                {
                    growthState.GrowthRate *= (1f + adaptation.EfficiencyImprovements["growth_rate"]);
                }
            }
        }
        
        private void HandleEnvironmentalChange(EnvironmentalConditions conditions)
        {
            // Update all plants based on global environmental changes
            foreach (var growthState in _plantGrowthStates.Values)
            {
                UpdatePlantEnvironmentalResponse(growthState, conditions);
            }
        }
        
        private void UpdatePlantEnvironmentalResponse(PlantGrowthState growthState, EnvironmentalConditions conditions)
        {
            // Calculate environmental impact on growth
            var environmentalMultiplier = CalculateEnvironmentalGrowthMultiplier(growthState);
            growthState.GrowthRate = CalculateInitialGrowthRate(growthState.PlantInstance) * environmentalMultiplier;
        }
        
        private void HandleSeasonChange(SeasonType newSeason)
        {
            if (!_enableSeasonalEffects) return;
            
            // Apply seasonal effects to all plants
            foreach (var growthState in _plantGrowthStates.Values)
            {
                ApplySeasonalEffects(growthState, newSeason);
            }
        }
        
        private void ApplySeasonalEffects(PlantGrowthState growthState, SeasonType season)
        {
            var seasonalMultiplier = season switch
            {
                SeasonType.Spring => 1.2f,  // Accelerated growth
                SeasonType.Summer => 1.1f,  // Optimal growth
                SeasonType.Fall => 0.9f,    // Slower growth
                SeasonType.Winter => 0.7f,  // Reduced growth
                _ => 1f
            };
            
            growthState.GrowthRate *= seasonalMultiplier;
            
            // Apply seasonal visual effects
            _colorManager.ApplySeasonalColoring(growthState.PlantInstance, season);
        }
        
        #endregion
        
        #region Public Interface
        
        public PlantGrowthState GetPlantGrowthState(int instanceId)
        {
            return _plantGrowthStates.TryGetValue(instanceId, out var state) ? state : null;
        }
        
        public GrowthAnimationData GetGrowthAnimationData(int instanceId)
        {
            return _growthAnimations.TryGetValue(instanceId, out var data) ? data : null;
        }
        
        public LifecycleProgressData GetLifecycleProgress(int instanceId)
        {
            return _lifecycleProgress.TryGetValue(instanceId, out var data) ? data : null;
        }
        
        public List<GrowthMilestone> GetAchievedMilestones(int instanceId)
        {
            return _achievedMilestones.Where(m => m.PlantInstanceId == instanceId).ToList();
        }
        
        public void SetGrowthTimeMultiplier(float multiplier)
        {
            _growthTimeMultiplier = Mathf.Max(0.1f, multiplier);
        }
        
        public void SetAcceleratedGrowth(bool enabled)
        {
            _enableAcceleratedGrowth = enabled;
            _lifecycleManager?.SetAcceleratedGrowth(enabled);
        }
        
        public void SetGrowthAnimationEnabled(bool enabled)
        {
            _enableGrowthAnimation = enabled;
            _animationController?.SetEnabled(enabled);
        }
        
        public void TriggerManualStageTransition(int instanceId, PlantGrowthStage targetStage)
        {
            if (_plantGrowthStates.TryGetValue(instanceId, out var growthState))
            {
                TriggerStageTransition(growthState, targetStage);
            }
        }
        
        public GrowthSystemReport GetSystemReport()
        {
            var vegetativeCount = _plantGrowthStates.Values.Count(p => p.CurrentStage == PlantGrowthStage.Vegetative);
            var floweringCount = _plantGrowthStates.Values.Count(p => p.CurrentStage == PlantGrowthStage.Flowering);
            var harvestCount = _plantGrowthStates.Values.Count(p => p.CurrentStage == PlantGrowthStage.Harvest);
            var averageGrowthRate = _plantGrowthStates.Values.Any() ? _plantGrowthStates.Values.Average(p => p.GrowthRate) : 0f;
            var averageGrowthProgress = _plantGrowthStates.Values.Any() ? _plantGrowthStates.Values.Average(p => p.GrowthProgress) : 0f;
            
            return new GrowthSystemReport
            {
                PerformanceMetrics = _performanceMetrics,
                ActiveGrowingPlants = ActiveGrowingPlants,
                PlantsInVegetativeStage = vegetativeCount,
                PlantsInFloweringStage = floweringCount,
                PlantsReadyForHarvest = harvestCount,
                AverageGrowthRate = averageGrowthRate,
                SystemEfficiency = CalculateSystemEfficiency(),
                TotalMilestonesAchieved = _achievedMilestones.Count,
                AverageGrowthProgress = averageGrowthProgress,
                SystemStatus = new Dictionary<string, bool>
                {
                    ["RealTimeGrowth"] = _enableRealTimeGrowth,
                    ["AcceleratedGrowth"] = _enableAcceleratedGrowth,
                    ["GrowthAnimation"] = _enableGrowthAnimation,
                    ["AutomaticStageProgression"] = _enableAutomaticStageProgression,
                    ["EnvironmentalTriggers"] = _enableEnvironmentalTriggers,
                    ["SeasonalEffects"] = _enableSeasonalEffects
                },
                RecentEvents = GetRecentGrowthEvents(),
                ReportGenerated = DateTime.Now,
                LastUpdate = DateTime.Now
            };
        }
        
        private float CalculateSystemEfficiency()
        {
            if (_plantGrowthStates.Count == 0) return 1.0f;
            
            var healthyPlants = _plantGrowthStates.Values.Count(p => p.HealthMultiplier > 0.8f);
            var totalPlants = _plantGrowthStates.Count;
            
            return (float)healthyPlants / totalPlants;
        }
        
        private List<string> GetRecentGrowthEvents()
        {
            var recentEvents = new List<string>();
            var cutoffTime = DateTime.Now.AddHours(-24); // Last 24 hours
            
            foreach (var milestone in _achievedMilestones.Where(m => m.AchievementTime > cutoffTime))
            {
                recentEvents.Add($"Milestone achieved: {milestone.Name}");
            }
            
            return recentEvents;
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            // Stop coroutines
            if (_growthUpdateCoroutine != null)
            {
                StopCoroutine(_growthUpdateCoroutine);
            }
            if (_animationUpdateCoroutine != null)
            {
                StopCoroutine(_animationUpdateCoroutine);
            }
            CancelInvoke();
            
            // Cleanup systems
            _lifecycleManager?.Cleanup();
            _animationController?.Cleanup();
            _stageTransitionManager?.Cleanup();
            _morphologyManager?.Cleanup();
            _colorManager?.Cleanup();
            _budDevelopmentSystem?.Cleanup();
            _trichromeGrowthSystem?.Cleanup();
            _rootGrowthSimulator?.Cleanup();
            _canopyManager?.Cleanup();
            _updateScheduler?.Cleanup();
            _lodGrowthManager?.Cleanup();
            
            // Clear data
            _plantGrowthStates.Clear();
            _growthAnimations.Clear();
            _lifecycleProgress.Clear();
            _achievedMilestones.Clear();
            _pendingGrowthEvents.Clear();
            
            // Disconnect events
            DisconnectSystemEvents();
            
            LogInfo("SpeedTree Growth System shutdown complete");
        }
        
        private void DisconnectSystemEvents()
        {
            if (_speedTreeManager != null)
            {
                _speedTreeManager.OnPlantInstanceCreated -= HandlePlantInstanceCreated;
                _speedTreeManager.OnPlantInstanceDestroyed -= HandlePlantInstanceDestroyed;
            }
            
            if (_environmentalSystem != null)
            {
                _environmentalSystem.OnPlantStressChanged -= HandlePlantStressChanged;
                _environmentalSystem.OnPlantAdapted -= HandlePlantAdapted;
            }
            
            if (_environmentalManager != null)
            {
                _environmentalManager.OnConditionsChanged -= HandleEnvironmentalChange;
                _environmentalManager.OnSeasonChanged -= HandleSeasonChange;
            }
        }
    }
}