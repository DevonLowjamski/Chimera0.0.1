using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
// Use canonical SeasonType from Data.Environment
using SeasonType = ProjectChimera.Data.Environment.SeasonType;
// Explicit alias for Data layer EnvironmentalConditions to resolve namespace conflicts
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
using ProjectChimera.Systems.SpeedTree;
using ProjectChimera.Systems.Prefabs;

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Comprehensive data structures for SpeedTree growth animation and lifecycle integration.
    /// Includes plant growth states, animation data, lifecycle progression, milestone tracking,
    /// and specialized growth systems for realistic cannabis cultivation simulation.
    /// </summary>
    
    // Growth Enums
    public enum GrowthAnimationPhase
    {
        Initial,
        EarlyGrowth,
        VegetativeGrowth,
        FloweringGrowth,
        MaturationPhase,
        Dormant
    }
    
    public enum GrowthEventType
    {
        StageTransition,
        Milestone,
        EnvironmentalResponse,
        GeneticExpression,
        SeasonalChange,
        UserIntervention
    }
    
    public enum BudDevelopmentStage
    {
        PreFlower,
        EarlyBud,
        BudFormation,
        BudSwelling,
        Maturation,
        HarvestReady
    }
    
    public enum TrichromeType
    {
        Bulbous,
        CapitateSessile,
        CapitateStalked
    }
    
    // Core Growth State
    [System.Serializable]
    public class PlantGrowthState
    {
        public int InstanceId;
        public AdvancedSpeedTreeManager.SpeedTreePlantData PlantInstance;
        public PlantGrowthStage CurrentStage;
        public float GrowthProgress; // 0-1 overall lifecycle progress
        public float StageProgress; // 0-1 current stage progress
        public float GrowthRate; // Growth units per second
        public float LastUpdateTime;
        public bool IsActivelyGrowing;
        public DateTime GrowthStartTime;
        public DateTime LastStageTransition;
        public Dictionary<string, float> GrowthModifiers = new Dictionary<string, float>();
        public List<string> ActiveGrowthFactors = new List<string>();
        public float HealthMultiplier = 1f;
        public float EnvironmentalMultiplier = 1f;
        public float GeneticMultiplier = 1f;
        public EnvironmentalConditions LastEnvironmentalConditions;
        public float EnvironmentalStressLevel;
    }
    
    // Growth Animation Data
    [System.Serializable]
    public class GrowthAnimationData
    {
        public int InstanceId;
        public float AnimationStartTime;
        public GrowthAnimationPhase CurrentAnimationPhase;
        public float AnimationSpeed;
        public Vector3 TargetSize;
        public float CurrentSize;
        public Vector3 TargetRotation;
        public float PhaseTransitionTime;
        public Dictionary<string, float> AnimationParameters = new Dictionary<string, float>();
        public bool IsAnimating = true;
        public float AnimationBlendWeight = 1f;
        public List<AnimationKeyframe> Keyframes = new List<AnimationKeyframe>();
        public float AnimationProgress; // 0-1
        public float LastAnimationUpdate;
    }
    
    [System.Serializable]
    public class AnimationKeyframe
    {
        public float Time;
        public Vector3 Scale;
        public Vector3 Rotation;
        public Color Color;
        public Dictionary<string, float> Properties = new Dictionary<string, float>();
    }
    
    // Lifecycle Progress Data
    [System.Serializable]
    public class LifecycleProgressData
    {
        public int InstanceId;
        public DateTime LifecycleStartTime;
        public DateTime EstimatedHarvestTime;
        public List<StageTransitionRecord> StageHistory = new List<StageTransitionRecord>();
        public Dictionary<string, float> GeneticModifiers = new Dictionary<string, float>();
        public float TotalLifecycleDays;
        public float DaysRemaining;
        public float LifecycleCompletion; // 0-1
        public LifecycleHealthData HealthData = new LifecycleHealthData();
        public LifecycleYieldPrediction YieldPrediction = new LifecycleYieldPrediction();
        public float TotalLifecycleProgress; // 0-1
        public bool IsCompleted;
    }
    
    [System.Serializable]
    public class StageTransitionRecord
    {
        public PlantGrowthStage FromStage;
        public PlantGrowthStage ToStage;
        public DateTime TransitionTime;
        public float GrowthProgress;
        public EnvironmentalConditions EnvironmentalConditions;
        public float TransitionQuality; // 0-1 how optimal the transition was
        public List<string> TriggeringFactors = new List<string>();
        public string Notes;
        public float TransitionDuration;
        public string TransitionTrigger;
    }
    
    [System.Serializable]
    public class LifecycleHealthData
    {
        public float AverageHealth;
        public float MinHealth;
        public float MaxHealth;
        public List<HealthEvent> HealthEvents = new List<HealthEvent>();
        public Dictionary<string, float> StageHealthAverages = new Dictionary<string, float>();
        public float StressImpactScore;
        public float RecoveryRate;
    }
    
    [System.Serializable]
    public class HealthEvent
    {
        public DateTime Timestamp;
        public float HealthBefore;
        public float HealthAfter;
        public string Cause;
        public string Treatment;
        public float Duration;
    }
    
    [System.Serializable]
    public class LifecycleYieldPrediction
    {
        public float PredictedYield; // grams
        public float YieldPotential; // maximum possible
        public float YieldEfficiency; // actual vs potential
        public Dictionary<string, float> YieldFactors = new Dictionary<string, float>();
        public float QualityPrediction; // 0-1
        public DateTime LastPredictionUpdate;
        public float PredictionConfidence; // 0-1
    }
    
    // Growth Milestones
    [System.Serializable]
    public class GrowthMilestone
    {
        public string Id;
        public string Name;
        public string Description;
        public float ProgressThreshold; // 0-1
        public PlantGrowthStage RequiredStage;
        public int PlantInstanceId;
        public DateTime AchievementTime;
        public Dictionary<string, object> MilestoneData = new Dictionary<string, object>();
        public List<string> Prerequisites = new List<string>();
        public MilestoneReward Reward = new MilestoneReward();
        public bool IsRepeatable = false;
        public int TimesAchieved = 0;
        public float RequiredProgress;
        public Dictionary<string, float> RequiredConditions = new Dictionary<string, float>();
        public float ExperienceReward;
        public bool IsAchieved;
    }
    
    [System.Serializable]
    public class MilestoneReward
    {
        public float ExperienceGain;
        public List<string> UnlockedFeatures = new List<string>();
        public Dictionary<string, float> BonusMultipliers = new Dictionary<string, float>();
        public string SpecialEffect;
    }
    
    // Growth Events
    [System.Serializable]
    public class GrowthEvent
    {
        public string EventId;
        public GrowthEventType EventType;
        public int PlantInstanceId;
        public DateTime Timestamp;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public string Description;
        public float Impact; // -1 to 1, negative is harmful
        public bool RequiresPlayerAction;
        public string RecommendedAction;
        public bool IsProcessed;
    }
    
    // Specialized Growth Systems Data
    
    // Bud Development System
    [System.Serializable]
    public class BudDevelopmentData
    {
        public int InstanceId;
        public BudDevelopmentStage Stage;
        public float BudDensity;
        public float BudSize;
        public float CalyxDevelopment;
        public float PistilLength;
        public Color BudColor;
        public float ResinProduction;
        public Dictionary<string, float> BudMetrics = new Dictionary<string, float>();
        public bool HasSignificantChange = false;
        public System.DateTime LastUpdate;
        public float TrichromeAmount;
        public float Potency;
    }
    
    [System.Serializable]
    public class BudCluster
    {
        public Vector3 Position;
        public float Size;
        public float Density;
        public BudDevelopmentStage DevelopmentStage;
        public float Quality;
        public Color Color;
        public List<BudSite> BudSites = new List<BudSite>();
    }
    
    [System.Serializable]
    public class BudSite
    {
        public Vector3 LocalPosition;
        public float Development; // 0-1
        public float Size;
        public int CalyxCount;
        public float PistilDensity;
        public float TrichromeAmount;
        public bool IsMainCola;
    }
    
    // Trichrome System
    [System.Serializable]
    public class TrichromeData
    {
        public int InstanceId;
        public float TrichromeAmount; // 0-1
        public float TrichromeSize;
        public Color TrichromeColor;
        public TrichromeType DominantType;
        public Dictionary<TrichromeType, float> TypeDistribution = new Dictionary<TrichromeType, float>();
        public float Clarity; // 0-1, clear to amber
        public float Density; // trichomes per square mm
        public bool HasSignificantChange = false;
        public DateTime LastUpdate;
        public TrichromeMaturityData MaturityData = new TrichromeMaturityData();
        public float MaturityLevel;
        public float ProductionRate;
    }
    
    [System.Serializable]
    public class TrichromeMaturityData
    {
        public float ClearPercentage;
        public float CloudyPercentage;
        public float AmberPercentage;
        public float OptimalHarvestWindow; // days
        public bool IsInHarvestWindow;
        public float PotencyEstimate; // THC/CBD prediction
    }
    
    // Root Growth System
    [System.Serializable]
    public class RootSystemData
    {
        public int InstanceId;
        public float RootMass;
        public float RootSpread; // radius
        public float RootDepth;
        public float RootHealth;
        public Dictionary<string, float> NutrientUptake = new Dictionary<string, float>();
        public float WaterUptakeRate;
        public List<RootZone> RootZones = new List<RootZone>();
        public bool HasMycorrhizae;
        public float SoilColonization; // 0-1
    }
    
    [System.Serializable]
    public class RootZone
    {
        public Vector3 Position;
        public float Radius;
        public float Density;
        public float Health;
        public Dictionary<string, float> LocalNutrients = new Dictionary<string, float>();
        public float Moisture;
        public float Temperature;
        public float pH;
    }
    
    // Canopy Development
    [System.Serializable]
    public class CanopyData
    {
        public int InstanceId;
        public float CanopyCoverage; // square meters
        public float CanopyHeight;
        public float CanopyDensity;
        public float LightPenetration; // 0-1
        public Vector3 CanopyCenter;
        public List<CanopyLayer> Layers = new List<CanopyLayer>();
        public float LeafAreaIndex; // LAI
        public float PhotosyntheticEfficiency;
    }
    
    [System.Serializable]
    public class CanopyLayer
    {
        public float Height;
        public float LeafDensity;
        public float LightInterception;
        public Color AverageLeafColor;
        public float Health;
        public int BranchCount;
        public float Biomass;
    }
    
    // Performance and Metrics
    [System.Serializable]
    public class GrowthPerformanceMetrics
    {
        public DateTime SystemStartTime;
        public int ActiveGrowingPlants;
        public float GrowthUpdatesPerSecond;
        public float AnimationUpdatesPerSecond;
        public float AverageGrowthRate;
        public float MemoryUsage; // MB
        public int TotalStageTransitions;
        public int TotalMilestonesAchieved;
        public float SystemEfficiency; // 0-1
        public Dictionary<string, float> SubSystemPerformance = new Dictionary<string, float>();
        public DateTime LastUpdate;
        public float AverageAnimationFrameTime;
    }
    
    [System.Serializable]
    public class GrowthSystemReport
    {
        public GrowthPerformanceMetrics PerformanceMetrics;
        public int ActiveGrowingPlants;
        public int PlantsInVegetativeStage;
        public int PlantsInFloweringStage;
        public int PlantsReadyForHarvest;
        public float AverageGrowthRate;
        public float SystemEfficiency;
        public int TotalMilestonesAchieved;
        public int TotalGrowthEvents;
        public float AverageGrowthProgress;
        public Dictionary<string, bool> SystemStatus = new Dictionary<string, bool>();
        public List<string> RecentEvents = new List<string>();
        public DateTime ReportGenerated;
        public DateTime LastUpdate;
        
        // Additional properties for Error Wave 138 compatibility
        public List<string> ActiveGrowthProcesses = new List<string>();
        public Dictionary<string, float> GrowthMetrics = new Dictionary<string, float>();
        public bool IsSystemHealthy => SystemEfficiency > 0.8f;
    }
    
    // Configuration ScriptableObjects
    [CreateAssetMenu(fileName = "Cannabis Lifecycle Config", menuName = "Project Chimera/SpeedTree/Lifecycle Config")]
    public class CannabisLifecycleConfigSO : ScriptableObject
    {
        [Header("Stage Durations (in days)")]
        public float SeedlingDuration = 14f;
        public float VegetativeDuration = 56f;
        public float FloweringDuration = 63f;
        public float HarvestWindow = 7f;
        
        [Header("Growth Rates")]
        public AnimationCurve SeedlingGrowthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve VegetativeGrowthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public AnimationCurve FloweringGrowthCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
        
        [Header("Environmental Triggers")]
        public bool EnablePhotoperiodTriggers = true;
        public float FloweringLightHours = 12f;
        public bool EnableTemperatureTriggers = true;
        public Vector2 OptimalTemperatureRange = new Vector2(20f, 28f);
        
        [Header("Genetic Variation")]
        public Vector2 GrowthRateVariation = new Vector2(0.8f, 1.2f);
        public Vector2 StageDurationVariation = new Vector2(0.9f, 1.1f);
        public bool EnableStrainSpecificTiming = true;
        
        [Header("Quality Factors")]
        public AnimationCurve HealthImpactCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve StressImpactCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        public float MinQualityThreshold = 0.3f;
        public bool EnableAutomaticProgression = true;
        public float GrowthRateMultiplier = 1f;
    }
    
    [CreateAssetMenu(fileName = "Growth Animation Config", menuName = "Project Chimera/SpeedTree/Animation Config")]
    public class GrowthAnimationConfigSO : ScriptableObject
    {
        [Header("Animation Curves")]
        public AnimationCurve GrowthAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve SizeProgressionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve MorphologyProgressionCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public AnimationCurve ColorProgressionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Animation Timing")]
        public float AnimationSpeed = 1f;
        public float TransitionDuration = 2f;
        public float BlendTime = 0.5f;
        public bool EnableSmoothTransitions = true;
        
        [Header("Stage-Specific Animations")]
        public List<StageAnimationConfig> StageAnimations = new List<StageAnimationConfig>();
        
        [Header("Wind Animation")]
        public bool EnableWindAnimation = true;
        public AnimationCurve WindResponseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public float WindAnimationSpeed = 1f;
        
        [Header("Interaction Animation")]
        public bool EnableTouchResponse = true;
        public float TouchResponseIntensity = 0.5f;
        public float TouchRecoveryTime = 2f;
        public bool EnableGrowthAnimation = true;
        public bool EnableSizeAnimation = true;
        public bool EnableColorAnimation = true;
        public bool EnableMorphologyAnimation = true;
    }
    
    [System.Serializable]
    public class StageAnimationConfig
    {
        public PlantGrowthStage Stage;
        public AnimationCurve SizeAnimation;
        public AnimationCurve ColorAnimation;
        public AnimationCurve MorphologyAnimation;
        public float AnimationSpeed = 1f;
        public List<string> EnabledFeatures = new List<string>();
        public List<AnimationEvent> AnimationEvents = new List<AnimationEvent>();
    }
    
    [System.Serializable]
    public class AnimationEvent
    {
        public float TriggerTime; // 0-1 progress
        public string EventType;
        public string Description;
        public Dictionary<string, float> Parameters = new Dictionary<string, float>();
    }
    
    // Specialized System Classes
    public class CannabisLifecycleManager
    {
        private CannabisLifecycleConfigSO _config;
        private bool _automaticStageProgression;
        private float _growthMultiplier = 1f;
        private float _stageDurationMultiplier = 1f;
        private bool _acceleratedGrowth = false;
        
        public CannabisLifecycleManager(CannabisLifecycleConfigSO config, bool automaticProgression)
        {
            _config = config;
            _automaticStageProgression = automaticProgression;
        }
        
        public PlantGrowthStage GetNextStage(PlantGrowthStage currentStage)
        {
            return currentStage switch
            {
                PlantGrowthStage.Seed => PlantGrowthStage.Seedling,
                PlantGrowthStage.Seedling => PlantGrowthStage.Vegetative,
                PlantGrowthStage.Vegetative => PlantGrowthStage.Flowering,
                PlantGrowthStage.Flowering => PlantGrowthStage.Harvest,
                PlantGrowthStage.Harvest => PlantGrowthStage.Drying,
                _ => currentStage
            };
        }
        
        public float GetStageDuration(PlantGrowthStage stage)
        {
            if (_config == null) return 30f; // Default
            
            var baseDuration = stage switch
            {
                PlantGrowthStage.Seedling => _config.SeedlingDuration,
                PlantGrowthStage.Vegetative => _config.VegetativeDuration,
                PlantGrowthStage.Flowering => _config.FloweringDuration,
                PlantGrowthStage.Harvest => _config.HarvestWindow,
                _ => 30f
            };
            
            return baseDuration * _stageDurationMultiplier;
        }
        
        public void SetGrowthMultiplier(float multiplier) { _growthMultiplier = multiplier; }
        public void SetStageDurationMultiplier(float multiplier) { _stageDurationMultiplier = multiplier; }
        public void SetAcceleratedGrowth(bool enabled) { _acceleratedGrowth = enabled; }
        
        public void Update() { }
        public void Cleanup() { }
    }
    
    public class GrowthAnimationController
    {
        private GrowthAnimationConfigSO _config;
        private bool _enabled;
        private Dictionary<int, AnimationState> _activeAnimations = new Dictionary<int, AnimationState>();
        
        public GrowthAnimationController(GrowthAnimationConfigSO config, bool enabled)
        {
            _config = config;
            _enabled = enabled;
        }
        
        public void StartAnimation(int instanceId, GrowthAnimationPhase phase)
        {
            if (!_enabled) return;
            
            var animationState = new AnimationState
            {
                InstanceId = instanceId,
                Phase = phase,
                StartTime = Time.time,
                Progress = 0f,
                IsActive = true
            };
            
            _activeAnimations[instanceId] = animationState;
        }
        
        public void UpdateAnimations()
        {
            foreach (var animation in _activeAnimations.Values)
            {
                UpdateAnimation(animation);
            }
        }
        
        private void UpdateAnimation(AnimationState animation)
        {
            if (!animation.IsActive) return;
            
            animation.Progress += Time.deltaTime * _config.AnimationSpeed;
            
            if (animation.Progress >= 1f)
            {
                animation.IsActive = false;
            }
        }
        
        public void SetEnabled(bool enabled) { _enabled = enabled; }
        public void Update() { }
        public void Cleanup() { _activeAnimations.Clear(); }
        
        private class AnimationState
        {
            public int InstanceId;
            public GrowthAnimationPhase Phase;
            public float StartTime;
            public float Progress;
            public bool IsActive;
        }
    }
    
    public class StageTransitionManager
    {
        private bool _enableSmoothTransitions;
        private Dictionary<int, TransitionState> _activeTransitions = new Dictionary<int, TransitionState>();
        
        public StageTransitionManager(bool smoothTransitions)
        {
            _enableSmoothTransitions = smoothTransitions;
        }
        
        public void ExecuteStageTransition(PlantGrowthState growthState, PlantGrowthStage fromStage, PlantGrowthStage toStage)
        {
            if (_enableSmoothTransitions)
            {
                var transition = new TransitionState
                {
                    InstanceId = growthState.InstanceId,
                    FromStage = fromStage,
                    ToStage = toStage,
                    StartTime = Time.time,
                    Duration = 2f,
                    Progress = 0f
                };
                
                _activeTransitions[growthState.InstanceId] = transition;
            }
        }
        
        public void Update()
        {
            foreach (var transition in _activeTransitions.Values.ToList())
            {
                transition.Progress += Time.deltaTime / transition.Duration;
                
                if (transition.Progress >= 1f)
                {
                    _activeTransitions.Remove(transition.InstanceId);
                }
            }
        }
        
        public void Cleanup() { _activeTransitions.Clear(); }
        
        private class TransitionState
        {
            public int InstanceId;
            public PlantGrowthStage FromStage;
            public PlantGrowthStage ToStage;
            public float StartTime;
            public float Duration;
            public float Progress;
        }
    }
    
    public class MorphologicalProgressionManager
    {
        private bool _enabled;
        
        public MorphologicalProgressionManager(bool enabled)
        {
            _enabled = enabled;
        }
        
        public void UpdatePlantSize(AdvancedSpeedTreeManager.SpeedTreePlantData instance, float growthProgress)
        {
            if (!_enabled || instance?.Renderer == null) return;
            
            var targetSize = CalculateTargetSize(instance, growthProgress);
            instance.Renderer.transform.localScale = Vector3.Lerp(instance.Renderer.transform.localScale, targetSize, Time.deltaTime);
        }
        
        public void UpdatePlantMorphology(AdvancedSpeedTreeManager.SpeedTreePlantData instance, PlantGrowthStage stage, float stageProgress)
        {
            if (!_enabled) return;
            
            // Update morphological characteristics based on stage and progress
            // This would modify SpeedTree material properties
        }
        
        private Vector3 CalculateTargetSize(AdvancedSpeedTreeManager.SpeedTreePlantData instance, float progress)
        {
            var baseSize = Vector3.one;
            if (instance.GeneticData != null)
            {
                baseSize *= instance.GeneticData.PlantSize;
            }
            
            return baseSize * Mathf.Lerp(0.1f, 1f, progress);
        }
        
        public void Update() { }
        public void Cleanup() { }
    }
    
    public class ColorProgressionManager
    {
        private bool _enabled;
        
        public ColorProgressionManager(bool enabled)
        {
            _enabled = enabled;
        }
        
        public void UpdatePlantColors(AdvancedSpeedTreeManager.SpeedTreePlantData instance, PlantGrowthStage stage, float stageProgress)
        {
            if (!_enabled) return;
            
#if UNITY_SPEEDTREE
            if (instance.Renderer?.materialProperties == null) return;
            
            var stageColor = GetStageColor(stage, stageProgress);
            instance.Renderer.materialProperties.SetColor("_LeafColor", stageColor);
#endif
        }
        
        public void ApplySeasonalColoring(AdvancedSpeedTreeManager.SpeedTreePlantData instance, SeasonType season)
        {
            if (!_enabled) return;
            
            var seasonalColor = GetSeasonalColor(season);
            // Apply seasonal tinting
        }
        
        private Color GetStageColor(PlantGrowthStage stage, float progress)
        {
            return stage switch
            {
                PlantGrowthStage.Seedling => Color.Lerp(Color.yellow, Color.green, progress),
                PlantGrowthStage.Vegetative => Color.green,
                PlantGrowthStage.Flowering => Color.Lerp(Color.green, new Color(0.8f, 1f, 0.8f), progress),
                PlantGrowthStage.Harvest => Color.Lerp(Color.green, Color.yellow, progress * 0.3f),
                _ => Color.green
            };
        }
        
        private Color GetSeasonalColor(SeasonType season)
        {
            return season switch
            {
                SeasonType.Spring => Color.green,
                SeasonType.Summer => new Color(0.9f, 1f, 0.9f),
                SeasonType.Fall => new Color(1f, 0.9f, 0.7f),
                SeasonType.Winter => new Color(0.8f, 0.8f, 0.9f),
                _ => Color.white
            };
        }
        
        public void Update() { }
        public void Cleanup() { }
    }
    
    public class BudDevelopmentSystem
    {
        private Dictionary<int, BudDevelopmentData> _budData = new Dictionary<int, BudDevelopmentData>();
        
        public void InitializeBudGrowth(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            var budData = new BudDevelopmentData
            {
                InstanceId = instance.InstanceId,
                Stage = BudDevelopmentStage.PreFlower,
                BudDensity = instance.GeneticData?.BudDensity ?? 0.5f,
                BudSize = 0.1f,
                CalyxDevelopment = 0f,
                PistilLength = 0f,
                BudColor = instance.GeneticData?.BudColor ?? Color.green,
                LastUpdate = DateTime.Now
            };
            
            _budData[instance.InstanceId] = budData;
        }
        
        public BudDevelopmentData UpdateBudGrowth(AdvancedSpeedTreeManager.SpeedTreePlantData instance, float deltaTime)
        {
            if (!_budData.TryGetValue(instance.InstanceId, out var budData))
                return new BudDevelopmentData { InstanceId = instance.InstanceId };
            
            if (instance.GrowthStage == PlantGrowthStage.Flowering || 
                instance.GrowthStage == PlantGrowthStage.Harvest)
            {
                var growthRate = CalculateBudGrowthRate(instance);
                budData.BudSize += growthRate * deltaTime;
                budData.CalyxDevelopment += growthRate * 0.5f * deltaTime;
                budData.PistilLength += growthRate * 0.3f * deltaTime;
                
                budData.HasSignificantChange = growthRate * deltaTime > 0.01f;
                budData.LastUpdate = DateTime.Now;
            }
            
            return budData;
        }
        
        public void ActivateBudGrowth(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            if (_budData.TryGetValue(instance.InstanceId, out var budData))
            {
                budData.Stage = BudDevelopmentStage.EarlyBud;
            }
        }
        
        public void FinalizebudDevelopment(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            if (_budData.TryGetValue(instance.InstanceId, out var budData))
            {
                budData.Stage = BudDevelopmentStage.HarvestReady;
                budData.BudSize = Mathf.Min(budData.BudSize, 1f);
            }
        }
        
        public float GetBudProgress(int instanceId)
        {
            return _budData.TryGetValue(instanceId, out var budData) ? budData.BudSize : 0f;
        }
        
        private float CalculateBudGrowthRate(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            var baseRate = 0.1f;
            
            if (instance.GeneticData != null)
            {
                baseRate *= instance.GeneticData.FloweringSpeed;
                baseRate *= instance.GeneticData.BudDensity;
            }
            
            // Environmental factors
            var environmentalMultiplier = 1f;
            foreach (var modifier in instance.EnvironmentalModifiers)
            {
                if (modifier.Key.Contains("flowering") || modifier.Key.Contains("bud"))
                {
                    environmentalMultiplier *= modifier.Value;
                }
            }
            
            return baseRate * environmentalMultiplier;
        }
        
        public void RemovePlant(int instanceId) { _budData.Remove(instanceId); }
        
        public void UpdateVisualBudDevelopment(AdvancedSpeedTreeManager.SpeedTreePlantData instance, float growthProgress)
        {
            if (_budData.TryGetValue(instance.InstanceId, out var budData))
            {
                // Update visual representation of bud development
                if (instance.Renderer?.Renderer != null)
                {
                    var materialPropertyBlock = new MaterialPropertyBlock();
                    materialPropertyBlock.SetFloat("_BudSize", budData.BudSize * growthProgress);
                    materialPropertyBlock.SetFloat("_BudDensity", budData.BudDensity);
                    materialPropertyBlock.SetColor("_BudColor", budData.BudColor);
                    materialPropertyBlock.SetFloat("_CalyxDevelopment", budData.CalyxDevelopment * growthProgress);
                    materialPropertyBlock.SetFloat("_PistilLength", budData.PistilLength * growthProgress);
                    
                    instance.Renderer.Renderer.SetPropertyBlock(materialPropertyBlock);
                }
            }
        }
        
        public void Update() { }
        public void Cleanup() { _budData.Clear(); }
    }
    
    public class TrichromeGrowthSystem
    {
        private Dictionary<int, TrichromeData> _trichromeData = new Dictionary<int, TrichromeData>();
        
        public void InitializeTrichromeGrowth(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            var trichromeData = new TrichromeData
            {
                InstanceId = instance.InstanceId,
                TrichromeAmount = 0f,
                TrichromeSize = 0.1f,
                TrichromeColor = Color.clear,
                DominantType = TrichromeType.CapitateStalked,
                Clarity = 1f, // Start clear
                Density = 0f,
                LastUpdate = DateTime.Now
            };
            
            trichromeData.TypeDistribution[TrichromeType.Bulbous] = 0.1f;
            trichromeData.TypeDistribution[TrichromeType.CapitateSessile] = 0.3f;
            trichromeData.TypeDistribution[TrichromeType.CapitateStalked] = 0.6f;
            
            _trichromeData[instance.InstanceId] = trichromeData;
        }
        
        public TrichromeData UpdateTrichromeGrowth(AdvancedSpeedTreeManager.SpeedTreePlantData instance, float deltaTime)
        {
            if (!_trichromeData.TryGetValue(instance.InstanceId, out var trichromeData))
                return new TrichromeData { InstanceId = instance.InstanceId };
            
            if (instance.GrowthStage == PlantGrowthStage.Flowering || 
                instance.GrowthStage == PlantGrowthStage.Harvest)
            {
                var growthRate = CalculateTrichromeGrowthRate(instance);
                trichromeData.TrichromeAmount += growthRate * deltaTime;
                trichromeData.Density += growthRate * 0.5f * deltaTime;
                
                // Update clarity (clear -> cloudy -> amber)
                UpdateTrichromeMaturity(trichromeData, deltaTime);
                
                trichromeData.HasSignificantChange = growthRate * deltaTime > 0.005f;
                trichromeData.LastUpdate = DateTime.Now;
            }
            
            return trichromeData;
        }
        
        public void ActivateTrichromeGrowth(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            if (_trichromeData.TryGetValue(instance.InstanceId, out var trichromeData))
            {
                trichromeData.TrichromeColor = Color.white;
            }
        }
        
        public void MaximizeTrichromeProduction(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            if (_trichromeData.TryGetValue(instance.InstanceId, out var trichromeData))
            {
                trichromeData.TrichromeAmount = Mathf.Min(trichromeData.TrichromeAmount, 1f);
                trichromeData.Density = Mathf.Min(trichromeData.Density, 1f);
            }
        }
        
        public float GetTrichromeProgress(int instanceId)
        {
            return _trichromeData.TryGetValue(instanceId, out var trichromeData) ? trichromeData.TrichromeAmount : 0f;
        }
        
        private float CalculateTrichromeGrowthRate(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            var baseRate = 0.05f;
            
            if (instance.GeneticData != null)
            {
                baseRate *= instance.GeneticData.TrichromeAmount;
            }
            
            return baseRate;
        }
        
        private void UpdateTrichromeMaturity(TrichromeData trichromeData, float deltaTime)
        {
            var maturityRate = 0.01f * deltaTime;
            
            // Clear -> Cloudy
            if (trichromeData.MaturityData.ClearPercentage > 0.1f)
            {
                trichromeData.MaturityData.ClearPercentage -= maturityRate;
                trichromeData.MaturityData.CloudyPercentage += maturityRate;
            }
            // Cloudy -> Amber (slower)
            else if (trichromeData.MaturityData.CloudyPercentage > 0.7f)
            {
                trichromeData.MaturityData.CloudyPercentage -= maturityRate * 0.3f;
                trichromeData.MaturityData.AmberPercentage += maturityRate * 0.3f;
            }
            
            // Check harvest window
            trichromeData.MaturityData.IsInHarvestWindow = 
                trichromeData.MaturityData.CloudyPercentage > 0.7f && 
                trichromeData.MaturityData.AmberPercentage < 0.3f;
        }
        
        public void RemovePlant(int instanceId) { _trichromeData.Remove(instanceId); }
        
        public void UpdateTrichromeVisualization(AdvancedSpeedTreeManager.SpeedTreePlantData instance, float growthProgress)
        {
            if (_trichromeData.TryGetValue(instance.InstanceId, out var trichromeData))
            {
                // Update visual representation of trichrome development
                if (instance.Renderer?.Renderer != null)
                {
                    var materialPropertyBlock = new MaterialPropertyBlock();
                    materialPropertyBlock.SetFloat("_TrichromeAmount", trichromeData.TrichromeAmount * growthProgress);
                    materialPropertyBlock.SetFloat("_TrichromeDensity", trichromeData.Density * growthProgress);
                    materialPropertyBlock.SetColor("_TrichromeColor", trichromeData.TrichromeColor);
                    materialPropertyBlock.SetFloat("_TrichromeClarity", trichromeData.Clarity);
                    materialPropertyBlock.SetFloat("_TrichromeMaturity", trichromeData.MaturityData.CloudyPercentage);
                    
                    instance.Renderer.Renderer.SetPropertyBlock(materialPropertyBlock);
                }
            }
        }
        
        public void Update() { }
        public void Cleanup() { _trichromeData.Clear(); }
    }
    
    public class RootGrowthSimulator
    {
        private Dictionary<int, RootSystemData> _rootData = new Dictionary<int, RootSystemData>();
        
        public void InitializeRootSystem(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            var rootData = new RootSystemData
            {
                InstanceId = instance.InstanceId,
                RootMass = 0.1f,
                RootSpread = 0.2f,
                RootDepth = 0.1f,
                RootHealth = 1f,
                WaterUptakeRate = 0.5f,
                HasMycorrhizae = false,
                SoilColonization = 0f
            };
            
            _rootData[instance.InstanceId] = rootData;
        }
        
        public void UpdateRootGrowth(AdvancedSpeedTreeManager.SpeedTreePlantData instance, float deltaTime)
        {
            if (!_rootData.TryGetValue(instance.InstanceId, out var rootData)) return;
            
            var growthRate = CalculateRootGrowthRate(instance);
            
            rootData.RootMass += growthRate * deltaTime;
            rootData.RootSpread += growthRate * 0.5f * deltaTime;
            rootData.RootDepth += growthRate * 0.3f * deltaTime;
        }
        
        private float CalculateRootGrowthRate(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            var baseRate = 0.1f;
            
            // Root growth is faster in vegetative stage
            if (instance.GrowthStage == PlantGrowthStage.Vegetative)
            {
                baseRate *= 1.5f;
            }
            
            return baseRate;
        }
        
        public void RemovePlant(int instanceId) { _rootData.Remove(instanceId); }
        
        public void UpdateRootVisualization(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            if (_rootData.TryGetValue(instance.InstanceId, out var rootData))
            {
                // Update visual representation of root system
                if (instance.Renderer?.Renderer != null)
                {
                    var materialPropertyBlock = new MaterialPropertyBlock();
                    materialPropertyBlock.SetFloat("_RootMass", rootData.RootMass);
                    materialPropertyBlock.SetFloat("_RootSpread", rootData.RootSpread);
                    materialPropertyBlock.SetFloat("_RootHealth", rootData.RootHealth);
                    materialPropertyBlock.SetFloat("_WaterUptake", rootData.WaterUptakeRate);
                    
                    instance.Renderer.Renderer.SetPropertyBlock(materialPropertyBlock);
                }
            }
        }
        
        public void Update() { }
        public void Cleanup() { _rootData.Clear(); }
    }
    
    public class CanopyDevelopmentManager
    {
        private Dictionary<int, CanopyData> _canopyData = new Dictionary<int, CanopyData>();
        
        public void UpdateCanopyGrowth(AdvancedSpeedTreeManager.SpeedTreePlantData instance, float deltaTime)
        {
            if (!_canopyData.TryGetValue(instance.InstanceId, out var canopyData))
            {
                canopyData = new CanopyData
                {
                    InstanceId = instance.InstanceId,
                    CanopyCoverage = 0.1f,
                    CanopyHeight = 0.2f,
                    CanopyDensity = 0.3f,
                    LightPenetration = 1f,
                    LeafAreaIndex = 0.5f,
                    PhotosyntheticEfficiency = 0.7f
                };
                _canopyData[instance.InstanceId] = canopyData;
            }
            
            var growthRate = CalculateCanopyGrowthRate(instance);
            
            canopyData.CanopyCoverage += growthRate * deltaTime;
            canopyData.CanopyHeight += growthRate * 0.8f * deltaTime;
            canopyData.LeafAreaIndex += growthRate * 0.6f * deltaTime;
            
            // Update light penetration based on density
            canopyData.LightPenetration = Mathf.Lerp(1f, 0.3f, canopyData.CanopyDensity);
        }
        
        private float CalculateCanopyGrowthRate(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            var baseRate = 0.05f;
            
            if (instance.GrowthStage == PlantGrowthStage.Vegetative)
            {
                baseRate *= 2f; // Rapid canopy development in veg
            }
            
            return baseRate;
        }
        
        public void Update() { }
        
        public void UpdateCanopyVisualization(AdvancedSpeedTreeManager.SpeedTreePlantData instance)
        {
            if (_canopyData.TryGetValue(instance.InstanceId, out var canopyData))
            {
                // Update visual representation of canopy development
                if (instance.Renderer?.Renderer != null)
                {
                    var materialPropertyBlock = new MaterialPropertyBlock();
                    materialPropertyBlock.SetFloat("_CanopyCoverage", canopyData.CanopyCoverage);
                    materialPropertyBlock.SetFloat("_CanopyHeight", canopyData.CanopyHeight);
                    materialPropertyBlock.SetFloat("_CanopyDensity", canopyData.CanopyDensity);
                    materialPropertyBlock.SetFloat("_LightPenetration", canopyData.LightPenetration);
                    materialPropertyBlock.SetFloat("_LeafAreaIndex", canopyData.LeafAreaIndex);
                    
                    instance.Renderer.Renderer.SetPropertyBlock(materialPropertyBlock);
                }
            }
        }
        
        public void Cleanup() { _canopyData.Clear(); }
    }
    
    public class GrowthUpdateScheduler
    {
        private Queue<PlantGrowthState> _updateQueue = new Queue<PlantGrowthState>();
        private float _lastScheduleTime = 0f;
        
        public List<PlantGrowthState> GetPlantsForUpdate(List<PlantGrowthState> allPlants)
        {
            // Schedule plants for update based on priority and performance
            var plantsToUpdate = new List<PlantGrowthState>();
            
            foreach (var plant in allPlants)
            {
                if (ShouldUpdatePlant(plant))
                {
                    plantsToUpdate.Add(plant);
                }
            }
            
            return plantsToUpdate.Take(GetMaxUpdatesThisFrame()).ToList();
        }
        
        private bool ShouldUpdatePlant(PlantGrowthState plant)
        {
            var timeSinceUpdate = Time.time - plant.LastUpdateTime;
            var updateFrequency = GetUpdateFrequency(plant);
            
            return timeSinceUpdate >= updateFrequency;
        }
        
        private float GetUpdateFrequency(PlantGrowthState plant)
        {
            // High priority plants update more frequently
            if (plant.CurrentStage == PlantGrowthStage.Flowering || 
                plant.CurrentStage == PlantGrowthStage.Harvest)
            {
                return 0.1f; // 10 FPS
            }
            
            return 0.2f; // 5 FPS
        }
        
        private int GetMaxUpdatesThisFrame()
        {
            var frameRate = 1f / Time.unscaledDeltaTime;
            
            if (frameRate > 50f) return 20;
            if (frameRate > 30f) return 15;
            return 10;
        }
        
        public void Update() { }
        public void Cleanup() { _updateQueue.Clear(); }
    }
    
    public class LODGrowthManager
    {
        public void UpdateLODs(List<PlantGrowthState> plants)
        {
            foreach (var plant in plants)
            {
                UpdatePlantLOD(plant);
            }
        }
        
        private void UpdatePlantLOD(PlantGrowthState plant)
        {
            var instance = plant.PlantInstance;
            var distanceToCamera = Vector3.Distance(instance.Position, Camera.main?.transform.position ?? Vector3.zero);
            
            // Adjust update frequency based on distance
            if (distanceToCamera > 50f)
            {
                // Reduce growth updates for distant plants
                plant.GrowthRate *= 0.5f;
            }
        }
        
        public void Update() { }
        public void Cleanup() { }
    }
}