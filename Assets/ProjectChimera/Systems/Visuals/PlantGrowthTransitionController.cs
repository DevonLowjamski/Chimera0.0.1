using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Collections;
using System;
#if UNITY_VFX_GRAPH
using UnityEngine.VFX;
#endif

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Controls growth transition animations for cannabis plants.
    /// Manages smooth visual transitions between growth stages, including morphological
    /// changes, bud development, leaf emergence, and structural growth animations.
    /// </summary>
    public class PlantGrowthTransitionController : ChimeraManager
    {
        [Header("Growth Animation Configuration")]
        [SerializeField] private bool _enableGrowthTransitions = true;
        [SerializeField] private bool _enableRealtimeAnimation = true;
        [SerializeField] private float _transitionSpeed = 1f;
        [SerializeField] private float _animationUpdateInterval = 0.05f;
        
        [Header("Growth Stage Settings")]
        [SerializeField] private ProjectChimera.Data.Genetics.PlantGrowthStage _currentGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage.Seedling;
        [SerializeField] private float _stageProgress = 0f;
        [SerializeField] private float _overallGrowthProgress = 0f;
        [SerializeField] private bool _autoAdvanceStages = true;
        
        [Header("Morphological Animation")]
        [SerializeField] private bool _enableSizeTransitions = true;
        [SerializeField] private bool _enableLeafDevelopment = true;
        [SerializeField] private bool _enableBudFormation = true;
        [SerializeField] private bool _enableStructuralChanges = true;
        
        [Header("VFX Integration")]
        [SerializeField] private bool _enableGrowthParticles = true;
        [SerializeField] private bool _enableMorphologyVFX = true;
        [SerializeField] private float _particleEmissionRate = 20f;
        [SerializeField] private Color _growthParticleColor = Color.green;
        
        [Header("Animation Curves")]
        [SerializeField] private AnimationCurve _sizeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _leafDensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _budFormationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _transitionEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxConcurrentAnimations = 50;
        [SerializeField] private float _cullingDistance = 25f;
        [SerializeField] private bool _enableLODAnimations = true;
        [SerializeField] private bool _enableAdaptiveQuality = true;
        
        // Growth Animation State
        private Dictionary<string, PlantGrowthAnimation> _activeAnimations = new Dictionary<string, PlantGrowthAnimation>();
        private Queue<string> _animationUpdateQueue = new Queue<string>();
        private List<GrowthTransitionData> _queuedTransitions = new List<GrowthTransitionData>();
        
        // VFX Integration
        private CannabisVFXTemplateManager _vfxTemplateManager;
        private SpeedTreeVFXIntegrationManager _speedTreeIntegration;
        private TrichromeVFXController _trichromeController;
        
        // Performance Tracking
        private float _lastAnimationUpdate = 0f;
        private int _animationsProcessedThisFrame = 0;
        private GrowthAnimationPerformanceMetrics _performanceMetrics;
        
        // Growth Stage Definitions
        private Dictionary<ProjectChimera.Data.Genetics.PlantGrowthStage, GrowthStageDefinition> _stageDefinitions;
        
        // Events
        public System.Action<ProjectChimera.Data.Genetics.PlantGrowthStage, ProjectChimera.Data.Genetics.PlantGrowthStage> OnGrowthStageTransition;
        public System.Action<string, float> OnGrowthProgressUpdate;
        public System.Action<string, PlantGrowthAnimation> OnAnimationCreated;
        public System.Action<GrowthAnimationPerformanceMetrics> OnPerformanceUpdate;
        
        // Properties
        public ProjectChimera.Data.Genetics.PlantGrowthStage CurrentGrowthStage => _currentGrowthStage;
        public float StageProgress => _stageProgress;
        public float OverallGrowthProgress => _overallGrowthProgress;
        public int ActiveAnimationsCount => _activeAnimations.Count;
        public GrowthAnimationPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        
        protected override void OnManagerInitialize()
        {
            InitializeGrowthAnimationSystem();
            InitializeStageDefinitions();
            ConnectToVFXManagers();
            InitializePerformanceTracking();
            StartGrowthSimulation();
            LogInfo("Plant Growth Transition Controller initialized");
        }
        
        private void Update()
        {
            if (_enableGrowthTransitions && Time.time - _lastAnimationUpdate >= _animationUpdateInterval)
            {
                ProcessAnimationQueue();
                UpdateGrowthSimulation();
                UpdatePerformanceMetrics();
                _lastAnimationUpdate = Time.time;
            }
        }
        
        #region Initialization
        
        private void InitializeGrowthAnimationSystem()
        {
            LogInfo("=== INITIALIZING GROWTH ANIMATION SYSTEM ===");
            
            #if UNITY_VFX_GRAPH
            LogInfo("✅ VFX Graph available - growth particle effects enabled");
            #else
            LogInfo("⚠️ VFX Graph not available - using fallback animation system");
            _enableGrowthParticles = false;
            #endif
            
            // Initialize performance metrics
            _performanceMetrics = new GrowthAnimationPerformanceMetrics
            {
                ActiveAnimations = 0,
                TransitionsPerSecond = 0f,
                AverageTransitionTime = 0f,
                TargetFrameRate = 60f,
                LastUpdate = DateTime.Now
            };
            
            LogInfo("✅ Growth animation system initialized");
        }
        
        private void InitializeStageDefinitions()
        {
            LogInfo("Setting up cannabis growth stage definitions...");
            
            _stageDefinitions = new Dictionary<ProjectChimera.Data.Genetics.PlantGrowthStage, GrowthStageDefinition>
            {
                [ProjectChimera.Data.Genetics.PlantGrowthStage.Seed] = new GrowthStageDefinition
                {
                    Stage = ProjectChimera.Data.Genetics.PlantGrowthStage.Seed,
                    StageName = "Seed",
                    Duration = 7f, // 7 days
                    RelativeSize = 0.05f,
                    LeafDensity = 0f,
                    BudFormation = 0f,
                    StructuralComplexity = 0.1f,
                    Description = "Dormant seed stage with no visible growth"
                },
                
                [ProjectChimera.Data.Genetics.PlantGrowthStage.Germination] = new GrowthStageDefinition
                {
                    Stage = ProjectChimera.Data.Genetics.PlantGrowthStage.Germination,
                    StageName = "Germination",
                    Duration = 5f, // 5 days
                    RelativeSize = 0.1f,
                    LeafDensity = 0.1f,
                    BudFormation = 0f,
                    StructuralComplexity = 0.2f,
                    Description = "Initial root and shoot emergence"
                },
                
                [ProjectChimera.Data.Genetics.PlantGrowthStage.Seedling] = new GrowthStageDefinition
                {
                    Stage = ProjectChimera.Data.Genetics.PlantGrowthStage.Seedling,
                    StageName = "Seedling",
                    Duration = 14f, // 2 weeks
                    RelativeSize = 0.25f,
                    LeafDensity = 0.3f,
                    BudFormation = 0f,
                    StructuralComplexity = 0.4f,
                    Description = "Cotyledon and first true leaves development"
                },
                
                [ProjectChimera.Data.Genetics.PlantGrowthStage.Vegetative] = new GrowthStageDefinition
                {
                    Stage = ProjectChimera.Data.Genetics.PlantGrowthStage.Vegetative,
                    StageName = "Vegetative",
                    Duration = 42f, // 6 weeks
                    RelativeSize = 0.7f,
                    LeafDensity = 0.8f,
                    BudFormation = 0.1f,
                    StructuralComplexity = 0.7f,
                    Description = "Rapid leaf and stem growth, branch development"
                },
                
                [ProjectChimera.Data.Genetics.PlantGrowthStage.PreFlowering] = new GrowthStageDefinition
                {
                    Stage = ProjectChimera.Data.Genetics.PlantGrowthStage.PreFlowering,
                    StageName = "Pre-Flower",
                    Duration = 14f, // 2 weeks
                    RelativeSize = 0.9f,
                    LeafDensity = 0.9f,
                    BudFormation = 0.3f,
                    StructuralComplexity = 0.8f,
                    Description = "Early flower site formation and stretch"
                },
                
                [ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering] = new GrowthStageDefinition
                {
                    Stage = ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering,
                    StageName = "Flowering",
                    Duration = 56f, // 8 weeks
                    RelativeSize = 1.0f,
                    LeafDensity = 0.7f,
                    BudFormation = 0.8f,
                    StructuralComplexity = 0.9f,
                    Description = "Active flower development and bud formation"
                },
                
                [ProjectChimera.Data.Genetics.PlantGrowthStage.Ripening] = new GrowthStageDefinition
                {
                    Stage = ProjectChimera.Data.Genetics.PlantGrowthStage.Ripening,
                    StageName = "Ripening",
                    Duration = 14f, // 2 weeks
                    RelativeSize = 1.0f,
                    LeafDensity = 0.5f,
                    BudFormation = 1.0f,
                    StructuralComplexity = 1.0f,
                    Description = "Final maturation and trichrome development"
                }
            };
            
            LogInfo($"✅ Configured {_stageDefinitions.Count} growth stage definitions");
        }
        
        private void ConnectToVFXManagers()
        {
            _vfxTemplateManager = FindObjectOfType<CannabisVFXTemplateManager>();
            _speedTreeIntegration = FindObjectOfType<SpeedTreeVFXIntegrationManager>();
            _trichromeController = FindObjectOfType<TrichromeVFXController>();
            
            if (_vfxTemplateManager == null)
            {
                LogWarning("CannabisVFXTemplateManager not found");
            }
            
            if (_speedTreeIntegration == null)
            {
                LogWarning("SpeedTreeVFXIntegrationManager not found");
            }
            
            if (_trichromeController == null)
            {
                LogWarning("TrichromeVFXController not found");
            }
            
            LogInfo("✅ Connected to VFX management systems");
        }
        
        private void InitializePerformanceTracking()
        {
            InvokeRepeating(nameof(UpdateDetailedPerformanceMetrics), 1f, 1f);
        }
        
        private void StartGrowthSimulation()
        {
            if (_autoAdvanceStages)
            {
                StartCoroutine(AutoGrowthSimulation());
                LogInfo("✅ Auto growth simulation started");
            }
        }
        
        #endregion
        
        #region Growth Animation Management
        
        public string CreateGrowthAnimation(Transform plantTransform, PlantStrainSO strainData = null)
        {
            string animationId = Guid.NewGuid().ToString();
            
            var animation = new PlantGrowthAnimation
            {
                AnimationId = animationId,
                PlantTransform = plantTransform,
                StrainData = strainData,
                CurrentStage = _currentGrowthStage,
                StageProgress = 0f,
                OverallProgress = 0f,
                IsActive = true,
                CreationTime = Time.time,
                LastUpdateTime = Time.time,
                TransitionData = new GrowthTransitionData(),
                MorphologyData = new PlantMorphologyData()
            };
            
            // Initialize morphology data
            InitializePlantMorphology(animation);
            
            // Create VFX components
            CreateGrowthVFXComponents(animation);
            
            // Apply genetic traits
            if (strainData != null)
            {
                ApplyGeneticGrowthTraits(animation, strainData);
            }
            
            _activeAnimations[animationId] = animation;
            _animationUpdateQueue.Enqueue(animationId);
            
            LogInfo($"Growth animation created: {animationId[..8]} for plant at {plantTransform.name}");
            OnAnimationCreated?.Invoke(animationId, animation);
            
            return animationId;
        }
        
        private void InitializePlantMorphology(PlantGrowthAnimation animation)
        {
            var currentStageDefinition = _stageDefinitions[animation.CurrentStage];
            
            animation.MorphologyData = new PlantMorphologyData
            {
                BaseSize = Vector3.one * currentStageDefinition.RelativeSize,
                CurrentSize = Vector3.one * currentStageDefinition.RelativeSize,
                TargetSize = Vector3.one * currentStageDefinition.RelativeSize,
                LeafDensity = currentStageDefinition.LeafDensity,
                BudFormation = currentStageDefinition.BudFormation,
                StructuralComplexity = currentStageDefinition.StructuralComplexity,
                GrowthRate = 1f,
                MorphologyVersion = 1
            };
        }
        
        private void CreateGrowthVFXComponents(PlantGrowthAnimation animation)
        {
            #if UNITY_VFX_GRAPH
            if (_enableGrowthParticles && _vfxTemplateManager != null)
            {
                // Create growth VFX instance
                string vfxInstanceId = _vfxTemplateManager.CreateVFXInstance(
                    CannabisVFXType.PlantGrowth,
                    animation.PlantTransform,
                    animation.PlantTransform.GetComponent<MonoBehaviour>()
                );
                
                animation.GrowthVFXInstanceId = vfxInstanceId;
                
                LogInfo($"Growth VFX created for animation {animation.AnimationId[..8]}: {vfxInstanceId[..8]}");
            }
            #endif
        }
        
        private void ApplyGeneticGrowthTraits(PlantGrowthAnimation animation, PlantStrainSO strainData)
        {
            // Apply strain-specific growth characteristics
            float geneticGrowthRate = 1.0f; // Would get from strain genetics
            float geneticMaxSize = 1.2f; // Would get from strain genetics
            float geneticLeafDensity = 0.8f; // Would get from strain genetics
            
            animation.GeneticGrowthRate = geneticGrowthRate;
            animation.GeneticMaxSize = geneticMaxSize;
            animation.GeneticLeafDensity = geneticLeafDensity;
            
            // Apply to morphology data
            animation.MorphologyData.GrowthRate *= geneticGrowthRate;
            animation.MorphologyData.BaseSize *= geneticMaxSize;
            
            LogInfo($"Applied genetic growth traits to animation {animation.AnimationId[..8]}");
        }
        
        #endregion
        
        #region Growth Stage Transitions
        
        public void InitiateStageTransition(string animationId, ProjectChimera.Data.Genetics.PlantGrowthStage targetStage)
        {
            if (!_activeAnimations.ContainsKey(animationId))
            {
                LogError($"Animation not found: {animationId}");
                return;
            }
            
            var animation = _activeAnimations[animationId];
            var currentStage = animation.CurrentStage;
            
            if (currentStage == targetStage)
            {
                LogWarning($"Animation {animationId[..8]} already in target stage: {targetStage}");
                return;
            }
            
            // Create transition data
            var transitionData = new GrowthTransitionData
            {
                AnimationId = animationId,
                FromStage = currentStage,
                ToStage = targetStage,
                TransitionStartTime = Time.time,
                TransitionDuration = CalculateTransitionDuration(currentStage, targetStage),
                TransitionProgress = 0f,
                IsActive = true,
                TransitionType = GetTransitionType(currentStage, targetStage)
            };
            
            // Set up morphology targets
            SetupMorphologyTransition(animation, targetStage);
            
            animation.TransitionData = transitionData;
            animation.IsTransitioning = true;
            
            _queuedTransitions.Add(transitionData);
            
            LogInfo($"Growth stage transition initiated: {currentStage} → {targetStage} for animation {animationId[..8]}");
            OnGrowthStageTransition?.Invoke(currentStage, targetStage);
            
            // Trigger transition VFX
            TriggerTransitionVFX(animation, transitionData);
        }
        
        private float CalculateTransitionDuration(ProjectChimera.Data.Genetics.PlantGrowthStage fromStage, ProjectChimera.Data.Genetics.PlantGrowthStage toStage)
        {
            // Base transition duration modified by stage complexity
            float baseDuration = 2f; // 2 seconds base
            
            var fromDefinition = _stageDefinitions[fromStage];
            var toDefinition = _stageDefinitions[toStage];
            
            float complexityFactor = Mathf.Abs(toDefinition.StructuralComplexity - fromDefinition.StructuralComplexity);
            
            return baseDuration * (1f + complexityFactor) / _transitionSpeed;
        }
        
        private GrowthTransitionType GetTransitionType(ProjectChimera.Data.Genetics.PlantGrowthStage fromStage, ProjectChimera.Data.Genetics.PlantGrowthStage toStage)
        {
            // Determine the type of transition based on stage progression
            if ((int)toStage > (int)fromStage)
            {
                return GrowthTransitionType.Forward;
            }
            else if ((int)toStage < (int)fromStage)
            {
                return GrowthTransitionType.Reverse;
            }
            else
            {
                return GrowthTransitionType.None;
            }
        }
        
        private void SetupMorphologyTransition(PlantGrowthAnimation animation, ProjectChimera.Data.Genetics.PlantGrowthStage targetStage)
        {
            var targetDefinition = _stageDefinitions[targetStage];
            var morphology = animation.MorphologyData;
            
            // Store current values as transition start
            morphology.StartSize = morphology.CurrentSize;
            morphology.StartLeafDensity = morphology.LeafDensity;
            morphology.StartBudFormation = morphology.BudFormation;
            
            // Set target values
            morphology.TargetSize = morphology.BaseSize * targetDefinition.RelativeSize * animation.GeneticMaxSize;
            morphology.TargetLeafDensity = targetDefinition.LeafDensity * animation.GeneticLeafDensity;
            morphology.TargetBudFormation = targetDefinition.BudFormation;
            morphology.TargetStructuralComplexity = targetDefinition.StructuralComplexity;
        }
        
        private void TriggerTransitionVFX(PlantGrowthAnimation animation, GrowthTransitionData transitionData)
        {
            #if UNITY_VFX_GRAPH
            if (_enableMorphologyVFX && animation.GrowthVFXInstanceId != null)
            {
                // Update VFX parameters for transition
                _vfxTemplateManager?.UpdateVFXParameter(animation.GrowthVFXInstanceId, "TransitionActive", 1f);
                _vfxTemplateManager?.UpdateVFXParameter(animation.GrowthVFXInstanceId, "TransitionIntensity", 1f);
                _vfxTemplateManager?.UpdateVFXParameter(animation.GrowthVFXInstanceId, "GrowthStage", (float)transitionData.ToStage / 6f);
            }
            #endif
        }
        
        #endregion
        
        #region Animation Processing
        
        private void ProcessAnimationQueue()
        {
            _animationsProcessedThisFrame = 0;
            
            // Process active transitions
            ProcessActiveTransitions();
            
            // Update animations in queue
            while (_animationUpdateQueue.Count > 0 && _animationsProcessedThisFrame < _maxConcurrentAnimations)
            {
                string animationId = _animationUpdateQueue.Dequeue();
                
                if (_activeAnimations.ContainsKey(animationId))
                {
                    UpdateGrowthAnimation(animationId);
                    _animationsProcessedThisFrame++;
                    
                    // Re-queue for next update
                    _animationUpdateQueue.Enqueue(animationId);
                }
            }
        }
        
        private void ProcessActiveTransitions()
        {
            for (int i = _queuedTransitions.Count - 1; i >= 0; i--)
            {
                var transition = _queuedTransitions[i];
                
                if (UpdateTransition(transition))
                {
                    // Transition completed
                    CompleteTransition(transition);
                    _queuedTransitions.RemoveAt(i);
                }
            }
        }
        
        private bool UpdateTransition(GrowthTransitionData transition)
        {
            float elapsed = Time.time - transition.TransitionStartTime;
            transition.TransitionProgress = Mathf.Clamp01(elapsed / transition.TransitionDuration);
            
            if (!_activeAnimations.ContainsKey(transition.AnimationId))
            {
                return true; // Animation no longer exists, complete transition
            }
            
            var animation = _activeAnimations[transition.AnimationId];
            
            // Update morphology during transition
            UpdateMorphologyTransition(animation, transition);
            
            // Update VFX parameters
            UpdateTransitionVFX(animation, transition);
            
            return transition.TransitionProgress >= 1f;
        }
        
        private void UpdateMorphologyTransition(PlantGrowthAnimation animation, GrowthTransitionData transition)
        {
            var morphology = animation.MorphologyData;
            float progress = _transitionEasing.Evaluate(transition.TransitionProgress);
            
            // Interpolate morphology values
            morphology.CurrentSize = Vector3.Lerp(morphology.StartSize, morphology.TargetSize, progress);
            morphology.LeafDensity = Mathf.Lerp(morphology.StartLeafDensity, morphology.TargetLeafDensity, progress);
            morphology.BudFormation = Mathf.Lerp(morphology.StartBudFormation, morphology.TargetBudFormation, progress);
            morphology.StructuralComplexity = Mathf.Lerp(morphology.StartStructuralComplexity, morphology.TargetStructuralComplexity, progress);
            
            // Apply to plant transform
            if (animation.PlantTransform != null && _enableSizeTransitions)
            {
                animation.PlantTransform.localScale = morphology.CurrentSize;
            }
        }
        
        private void UpdateTransitionVFX(PlantGrowthAnimation animation, GrowthTransitionData transition)
        {
            #if UNITY_VFX_GRAPH
            if (animation.GrowthVFXInstanceId != null)
            {
                float progress = transition.TransitionProgress;
                
                _vfxTemplateManager?.UpdateVFXParameter(animation.GrowthVFXInstanceId, "TransitionProgress", progress);
                _vfxTemplateManager?.UpdateVFXParameter(animation.GrowthVFXInstanceId, "GrowthRate", animation.GeneticGrowthRate * _transitionSpeed);
                _vfxTemplateManager?.UpdateVFXParameter(animation.GrowthVFXInstanceId, "LeafDensity", animation.MorphologyData.LeafDensity);
                _vfxTemplateManager?.UpdateVFXParameter(animation.GrowthVFXInstanceId, "BudFormation", animation.MorphologyData.BudFormation);
            }
            #endif
        }
        
        private void CompleteTransition(GrowthTransitionData transition)
        {
            if (!_activeAnimations.ContainsKey(transition.AnimationId))
            {
                return;
            }
            
            var animation = _activeAnimations[transition.AnimationId];
            
            // Update animation state
            animation.CurrentStage = transition.ToStage;
            animation.IsTransitioning = false;
            animation.TransitionData = null;
            
            // Finalize morphology
            var morphology = animation.MorphologyData;
            morphology.CurrentSize = morphology.TargetSize;
            morphology.LeafDensity = morphology.TargetLeafDensity;
            morphology.BudFormation = morphology.TargetBudFormation;
            morphology.StructuralComplexity = morphology.TargetStructuralComplexity;
            
            // Update VFX to stable state
            #if UNITY_VFX_GRAPH
            if (animation.GrowthVFXInstanceId != null)
            {
                _vfxTemplateManager?.UpdateVFXParameter(animation.GrowthVFXInstanceId, "TransitionActive", 0f);
                _vfxTemplateManager?.UpdateVFXParameter(animation.GrowthVFXInstanceId, "TransitionIntensity", 0f);
            }
            #endif
            
            LogInfo($"Growth transition completed: {transition.FromStage} → {transition.ToStage} for animation {animation.AnimationId[..8]}");
        }
        
        private void UpdateGrowthAnimation(string animationId)
        {
            var animation = _activeAnimations[animationId];
            
            // Skip if too far away (LOD)
            if (_enableLODAnimations && IsAnimationCulled(animation))
            {
                return;
            }
            
            // Update stage progress if not transitioning
            if (!animation.IsTransitioning && _autoAdvanceStages)
            {
                UpdateStageProgress(animation);
            }
            
            // Update overall progress
            UpdateOverallProgress(animation);
            
            // Update continuous effects
            UpdateContinuousEffects(animation);
            
            animation.LastUpdateTime = Time.time;
        }
        
        private bool IsAnimationCulled(PlantGrowthAnimation animation)
        {
            if (Camera.main == null || animation.PlantTransform == null)
            {
                return false;
            }
            
            float distance = Vector3.Distance(Camera.main.transform.position, animation.PlantTransform.position);
            return distance > _cullingDistance;
        }
        
        private void UpdateStageProgress(PlantGrowthAnimation animation)
        {
            var stageDefinition = _stageDefinitions[animation.CurrentStage];
            float deltaTime = Time.time - animation.LastUpdateTime;
            
            // Progress based on stage duration and growth rate
            float progressDelta = (deltaTime / stageDefinition.Duration) * animation.GeneticGrowthRate * _transitionSpeed;
            animation.StageProgress += progressDelta;
            
            // Check for stage advancement
            if (animation.StageProgress >= 1f && CanAdvanceToNextStage(animation.CurrentStage))
            {
                var nextStage = GetNextGrowthStage(animation.CurrentStage);
                InitiateStageTransition(animation.AnimationId, nextStage);
                animation.StageProgress = 0f;
            }
        }
        
        private void UpdateOverallProgress(PlantGrowthAnimation animation)
        {
            // Calculate overall progress based on stage and stage progress
            float stageWeight = 1f / (float)System.Enum.GetValues(typeof(ProjectChimera.Data.Genetics.PlantGrowthStage)).Length;
            float stageIndex = (float)animation.CurrentStage;
            
            animation.OverallProgress = (stageIndex * stageWeight) + (animation.StageProgress * stageWeight);
            animation.OverallProgress = Mathf.Clamp01(animation.OverallProgress);
            
            OnGrowthProgressUpdate?.Invoke(animation.AnimationId, animation.OverallProgress);
        }
        
        private void UpdateContinuousEffects(PlantGrowthAnimation animation)
        {
            #if UNITY_VFX_GRAPH
            if (animation.GrowthVFXInstanceId != null)
            {
                // Update continuous VFX parameters
                _vfxTemplateManager?.UpdateVFXParameter(animation.GrowthVFXInstanceId, "OverallProgress", animation.OverallProgress);
                _vfxTemplateManager?.UpdateVFXParameter(animation.GrowthVFXInstanceId, "StageProgress", animation.StageProgress);
                _vfxTemplateManager?.UpdateVFXParameter(animation.GrowthVFXInstanceId, "GrowthColor", _growthParticleColor);
            }
            #endif
        }
        
        #endregion
        
        #region Growth Simulation
        
        private IEnumerator AutoGrowthSimulation()
        {
            while (_autoAdvanceStages)
            {
                yield return new WaitForSeconds(1f);
                
                // Update global growth progress
                if (_activeAnimations.Count > 0)
                {
                    UpdateGlobalGrowthProgress();
                }
            }
        }
        
        private void UpdateGrowthSimulation()
        {
            // Update global stage and progress
            _stageProgress += Time.deltaTime * _transitionSpeed * 0.01f; // Slow progression for demo
            
            if (_stageProgress >= 1f && CanAdvanceToNextStage(_currentGrowthStage))
            {
                AdvanceGlobalGrowthStage();
                _stageProgress = 0f;
            }
            
            // Update overall progress
            UpdateGlobalOverallProgress();
        }
        
        private void AdvanceGlobalGrowthStage()
        {
            var previousStage = _currentGrowthStage;
            _currentGrowthStage = GetNextGrowthStage(_currentGrowthStage);
            
            LogInfo($"Global growth stage advanced: {previousStage} → {_currentGrowthStage}");
            OnGrowthStageTransition?.Invoke(previousStage, _currentGrowthStage);
        }
        
        private void UpdateGlobalGrowthProgress()
        {
            if (_activeAnimations.Count == 0) return;
            
            float totalProgress = 0f;
            foreach (var animation in _activeAnimations.Values)
            {
                totalProgress += animation.OverallProgress;
            }
            
            _overallGrowthProgress = totalProgress / _activeAnimations.Count;
        }
        
        private void UpdateGlobalOverallProgress()
        {
            float stageWeight = 1f / (float)System.Enum.GetValues(typeof(ProjectChimera.Data.Genetics.PlantGrowthStage)).Length;
            float stageIndex = (float)_currentGrowthStage;
            
            _overallGrowthProgress = (stageIndex * stageWeight) + (_stageProgress * stageWeight);
            _overallGrowthProgress = Mathf.Clamp01(_overallGrowthProgress);
        }
        
        private bool CanAdvanceToNextStage(ProjectChimera.Data.Genetics.PlantGrowthStage currentStage)
        {
            return currentStage != ProjectChimera.Data.Genetics.PlantGrowthStage.Ripening; // Last stage
        }
        
        private ProjectChimera.Data.Genetics.PlantGrowthStage GetNextGrowthStage(ProjectChimera.Data.Genetics.PlantGrowthStage currentStage)
        {
            return currentStage switch
            {
                ProjectChimera.Data.Genetics.PlantGrowthStage.Seed => ProjectChimera.Data.Genetics.PlantGrowthStage.Germination,
                ProjectChimera.Data.Genetics.PlantGrowthStage.Germination => ProjectChimera.Data.Genetics.PlantGrowthStage.Seedling,
                ProjectChimera.Data.Genetics.PlantGrowthStage.Seedling => ProjectChimera.Data.Genetics.PlantGrowthStage.Vegetative,
                ProjectChimera.Data.Genetics.PlantGrowthStage.Vegetative => ProjectChimera.Data.Genetics.PlantGrowthStage.PreFlowering,
                ProjectChimera.Data.Genetics.PlantGrowthStage.PreFlowering => ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering,
                ProjectChimera.Data.Genetics.PlantGrowthStage.Flowering => ProjectChimera.Data.Genetics.PlantGrowthStage.Ripening,
                ProjectChimera.Data.Genetics.PlantGrowthStage.Ripening => ProjectChimera.Data.Genetics.PlantGrowthStage.Ripening, // Stay at final stage
                _ => currentStage
            };
        }
        
        #endregion
        
        #region Performance Monitoring
        
        private void UpdatePerformanceMetrics()
        {
            _performanceMetrics.ActiveAnimations = _activeAnimations.Count;
            _performanceMetrics.TransitionsPerSecond = _queuedTransitions.Count / Time.deltaTime;
            _performanceMetrics.LastUpdate = DateTime.Now;
        }
        
        private void UpdateDetailedPerformanceMetrics()
        {
            OnPerformanceUpdate?.Invoke(_performanceMetrics);
        }
        
        #endregion
        
        #region Public Interface
        
        public PlantGrowthAnimation GetAnimation(string animationId)
        {
            return _activeAnimations.ContainsKey(animationId) ? _activeAnimations[animationId] : null;
        }
        
        public List<PlantGrowthAnimation> GetAllAnimations()
        {
            return new List<PlantGrowthAnimation>(_activeAnimations.Values);
        }
        
        public void SetGrowthStage(ProjectChimera.Data.Genetics.PlantGrowthStage stage)
        {
            var previousStage = _currentGrowthStage;
            _currentGrowthStage = stage;
            _stageProgress = 0f;
            
            OnGrowthStageTransition?.Invoke(previousStage, stage);
            LogInfo($"Growth stage manually set to: {stage}");
        }
        
        public void SetTransitionSpeed(float speed)
        {
            _transitionSpeed = Mathf.Max(0.1f, speed);
            LogInfo($"Growth transition speed set to: {_transitionSpeed}x");
        }
        
        public void SetAutoAdvanceStages(bool autoAdvance)
        {
            _autoAdvanceStages = autoAdvance;
            
            if (autoAdvance && !_autoAdvanceStages)
            {
                StartCoroutine(AutoGrowthSimulation());
            }
            
            LogInfo($"Auto advance stages: {(autoAdvance ? "ENABLED" : "DISABLED")}");
        }
        
        public void DestroyAnimation(string animationId)
        {
            if (_activeAnimations.ContainsKey(animationId))
            {
                var animation = _activeAnimations[animationId];
                
                // Cleanup VFX
                if (animation.GrowthVFXInstanceId != null)
                {
                    _vfxTemplateManager?.DestroyVFXInstance(animation.GrowthVFXInstanceId);
                }
                
                _activeAnimations.Remove(animationId);
                LogInfo($"Growth animation destroyed: {animationId[..8]}");
            }
        }
        
        public GrowthAnimationReport GetAnimationReport()
        {
            return new GrowthAnimationReport
            {
                ActiveAnimations = _activeAnimations.Count,
                CurrentGrowthStage = _currentGrowthStage,
                GlobalStageProgress = _stageProgress,
                GlobalOverallProgress = _overallGrowthProgress,
                ActiveTransitions = _queuedTransitions.Count,
                TransitionSpeed = _transitionSpeed,
                AutoAdvanceEnabled = _autoAdvanceStages,
                PerformanceMetrics = _performanceMetrics
            };
        }
        
        #endregion
        
        #region Context Menu Actions
        
        [ContextMenu("Advance Growth Stage")]
        public void ForceAdvanceStage()
        {
            if (CanAdvanceToNextStage(_currentGrowthStage))
            {
                AdvanceGlobalGrowthStage();
            }
        }
        
        [ContextMenu("Reset Growth")]
        public void ResetGrowth()
        {
            _currentGrowthStage = PlantGrowthStage.Seed;
            _stageProgress = 0f;
            _overallGrowthProgress = 0f;
            
            LogInfo("Growth reset to seed stage");
        }
        
        [ContextMenu("Show Animation Report")]
        public void ShowAnimationReport()
        {
            var report = GetAnimationReport();
            LogInfo("=== GROWTH ANIMATION REPORT ===");
            LogInfo($"Active Animations: {report.ActiveAnimations}");
            LogInfo($"Current Stage: {report.CurrentGrowthStage}");
            LogInfo($"Stage Progress: {report.GlobalStageProgress:F2}");
            LogInfo($"Overall Progress: {report.GlobalOverallProgress:F2}");
            LogInfo($"Active Transitions: {report.ActiveTransitions}");
            LogInfo($"Transition Speed: {report.TransitionSpeed}x");
            LogInfo($"Auto Advance: {report.AutoAdvanceEnabled}");
        }
        
        [ContextMenu("Create Test Animation")]
        public void CreateTestAnimation()
        {
            var testObject = new GameObject("Test_Growth_Plant");
            testObject.transform.position = Vector3.zero;
            
            string animationId = CreateGrowthAnimation(testObject.transform);
            LogInfo($"Test growth animation created: {animationId[..8]}");
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            StopAllCoroutines();
            
            // Cleanup all animations
            var animationIds = new List<string>(_activeAnimations.Keys);
            foreach (string animationId in animationIds)
            {
                DestroyAnimation(animationId);
            }
            
            CancelInvoke();
            LogInfo("Plant Growth Transition Controller shutdown complete");
        }
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public enum VisualsPlantGrowthStage
    {
        Seed = 0,
        Germination = 1,
        Seedling = 2,
        Vegetative = 3,
        PreFlower = 4,
        Flowering = 5,
        Ripening = 6
    }
    
    [System.Serializable]
    public enum GrowthTransitionType
    {
        None = 0,
        Forward = 1,
        Reverse = 2
    }
    
    [System.Serializable]
    public class PlantGrowthAnimation
    {
        public string AnimationId;
        public Transform PlantTransform;
        public PlantStrainSO StrainData;
        public ProjectChimera.Data.Genetics.PlantGrowthStage CurrentStage;
        public float StageProgress;
        public float OverallProgress;
        public bool IsActive;
        public bool IsTransitioning;
        public float CreationTime;
        public float LastUpdateTime;
        public float GeneticGrowthRate;
        public float GeneticMaxSize;
        public float GeneticLeafDensity;
        public string GrowthVFXInstanceId;
        public GrowthTransitionData TransitionData;
        public PlantMorphologyData MorphologyData;
    }
    
    [System.Serializable]
    public class GrowthTransitionData
    {
        public string AnimationId;
        public ProjectChimera.Data.Genetics.PlantGrowthStage FromStage;
        public ProjectChimera.Data.Genetics.PlantGrowthStage ToStage;
        public float TransitionStartTime;
        public float TransitionDuration;
        public float TransitionProgress;
        public bool IsActive;
        public GrowthTransitionType TransitionType;
    }
    
    [System.Serializable]
    public class PlantMorphologyData
    {
        public Vector3 BaseSize;
        public Vector3 CurrentSize;
        public Vector3 TargetSize;
        public Vector3 StartSize;
        public float LeafDensity;
        public float TargetLeafDensity;
        public float StartLeafDensity;
        public float BudFormation;
        public float TargetBudFormation;
        public float StartBudFormation;
        public float StructuralComplexity;
        public float TargetStructuralComplexity;
        public float StartStructuralComplexity;
        public float GrowthRate;
        public int MorphologyVersion;
    }
    
    [System.Serializable]
    public class GrowthStageDefinition
    {
        public ProjectChimera.Data.Genetics.PlantGrowthStage Stage;
        public string StageName;
        public float Duration; // In days
        public float RelativeSize;
        public float LeafDensity;
        public float BudFormation;
        public float StructuralComplexity;
        public string Description;
    }
    
    [System.Serializable]
    public class GrowthAnimationPerformanceMetrics
    {
        public int ActiveAnimations;
        public float TransitionsPerSecond;
        public float AverageTransitionTime;
        public float TargetFrameRate;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class GrowthAnimationReport
    {
        public int ActiveAnimations;
        public ProjectChimera.Data.Genetics.PlantGrowthStage CurrentGrowthStage;
        public float GlobalStageProgress;
        public float GlobalOverallProgress;
        public int ActiveTransitions;
        public float TransitionSpeed;
        public bool AutoAdvanceEnabled;
        public GrowthAnimationPerformanceMetrics PerformanceMetrics;
    }
    
    #endregion
}