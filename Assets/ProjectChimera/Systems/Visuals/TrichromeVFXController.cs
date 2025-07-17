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
    /// Controls trichrome development visual effects for cannabis plants.
    /// Manages particle systems that visualize trichrome formation, growth, and maturation
    /// based on genetic traits, environmental conditions, and plant development stage.
    /// </summary>
    public class TrichromeVFXController : ChimeraManager
    {
        [Header("Trichrome VFX Configuration")]
        [SerializeField] private bool _enableTrichromeVFX = true;
        [SerializeField] private bool _enableRealtimeUpdates = true;
        [SerializeField] private float _updateInterval = 0.1f;
        
        [Header("Trichrome Development Settings")]
        [SerializeField] private TrichromeDevelopmentStage _currentStage = TrichromeDevelopmentStage.Formation;
        [SerializeField] private float _trichromeAmount = 0.0f;
        [SerializeField] private float _trichromeDensity = 0.3f;
        [SerializeField] private float _trichromeMaturity = 0.0f;
        [SerializeField] private Color _trichromeColor = Color.white;
        
        [Header("Particle System Configuration")]
        [SerializeField] private int _maxTrichromeParticles = 1000;
        [SerializeField] private float _particleLifetime = 30f;
        [SerializeField] private float _particleSize = 0.01f;
        [SerializeField] private float _emissionRate = 10f;
        
        [Header("Genetic Integration")]
        [SerializeField] private bool _enableGeneticVariation = true;
        [SerializeField] private float _geneticInfluenceStrength = 0.8f;
        [SerializeField] private TrichromeGeneticProfile _geneticProfile;
        
        [Header("Environmental Response")]
        [SerializeField] private bool _enableEnvironmentalResponse = true;
        [SerializeField] private float _lightInfluence = 0.6f;
        [SerializeField] private float _temperatureInfluence = 0.4f;
        [SerializeField] private float _humidityInfluence = 0.3f;
        
        [Header("Material Properties")]
        [SerializeField] private Material _trichromeMaterial;
        [SerializeField] private Texture2D _trichromeTexture;
        [SerializeField] private AnimationCurve _opacityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve _sizeCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1f);
        
        // VFX State
        private Dictionary<string, TrichromeVFXInstance> _activeInstances = new Dictionary<string, TrichromeVFXInstance>();
        private List<TrichromeParticleData> _particlePool = new List<TrichromeParticleData>();
        private Queue<TrichromeEmissionPoint> _emissionQueue = new Queue<TrichromeEmissionPoint>();
        
        // Managers and Systems
        private CannabisVFXTemplateManager _vfxTemplateManager;
        private SpeedTreeVFXIntegrationManager _speedTreeIntegration;
        
        // Performance Tracking
        private float _lastUpdate = 0f;
        private int _activeParticles = 0;
        private TrichromeVFXPerformanceMetrics _performanceMetrics;
        
        // Trichrome Development Simulation
        private TrichromeSimulationData _simulationData;
        private float _developmentRate = 1f;
        private bool _isSimulationRunning = false;
        
        // Events
        public System.Action<TrichromeDevelopmentStage> OnDevelopmentStageChanged;
        public System.Action<float> OnTrichromeAmountChanged;
        public System.Action<string, TrichromeVFXInstance> OnInstanceCreated;
        public System.Action<TrichromeVFXPerformanceMetrics> OnPerformanceUpdate;
        
        // Properties
        public float TrichromeAmount => _trichromeAmount;
        public float TrichromeDensity => _trichromeDensity;
        public float TrichromeMaturity => _trichromeMaturity;
        public TrichromeDevelopmentStage CurrentStage => _currentStage;
        public bool IsSimulationRunning => _isSimulationRunning;
        public TrichromeVFXPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        
        protected override void OnManagerInitialize()
        {
            InitializeTrichromeVFXSystem();
            InitializeParticlePool();
            InitializeSimulationData();
            ConnectToManagers();
            StartTrichromeSimulation();
            LogInfo("Trichrome VFX Controller initialized");
        }
        
        private void Update()
        {
            if (_enableTrichromeVFX && Time.time - _lastUpdate >= _updateInterval)
            {
                UpdateTrichromeSimulation();
                UpdateActiveInstances();
                UpdatePerformanceMetrics();
                _lastUpdate = Time.time;
            }
        }
        
        #region Initialization
        
        private void InitializeTrichromeVFXSystem()
        {
            LogInfo("=== INITIALIZING TRICHROME VFX SYSTEM ===");
            
            #if UNITY_VFX_GRAPH
            LogInfo("✅ VFX Graph available - full trichrome effects enabled");
            #else
            LogWarning("⚠️ VFX Graph not available - using fallback particle system");
            #endif
            
            // Initialize genetic profile if not set
            if (_geneticProfile == null)
            {
                _geneticProfile = CreateDefaultGeneticProfile();
            }
            
            // Initialize performance metrics
            _performanceMetrics = new TrichromeVFXPerformanceMetrics
            {
                ActiveInstances = 0,
                ActiveParticles = 0,
                EmissionRate = _emissionRate,
                TargetFrameRate = 60f,
                LastUpdate = DateTime.Now
            };
            
            LogInfo("✅ Trichrome VFX system initialized");
        }
        
        private void InitializeParticlePool()
        {
            LogInfo("Creating trichrome particle pool...");
            
            // Pre-allocate particle pool for performance
            for (int i = 0; i < _maxTrichromeParticles; i++)
            {
                var particle = new TrichromeParticleData
                {
                    Id = Guid.NewGuid().ToString(),
                    IsActive = false,
                    Position = Vector3.zero,
                    Size = _particleSize,
                    Opacity = 0f,
                    CreationTime = 0f,
                    DevelopmentStage = TrichromeDevelopmentStage.Formation
                };
                
                _particlePool.Add(particle);
            }
            
            LogInfo($"✅ Particle pool created with {_particlePool.Count} particles");
        }
        
        private void InitializeSimulationData()
        {
            _simulationData = new TrichromeSimulationData
            {
                DevelopmentStartTime = Time.time,
                CurrentPhase = TrichromePhase.Initiation,
                PhaseProgress = 0f,
                EnvironmentalInfluence = 1f,
                GeneticExpression = 1f,
                MaturationRate = 1f
            };
            
            LogInfo("✅ Trichrome simulation data initialized");
        }
        
        private void ConnectToManagers()
        {
            _vfxTemplateManager = FindObjectOfType<CannabisVFXTemplateManager>();
            _speedTreeIntegration = FindObjectOfType<SpeedTreeVFXIntegrationManager>();
            
            if (_vfxTemplateManager == null)
            {
                LogWarning("CannabisVFXTemplateManager not found");
            }
            
            if (_speedTreeIntegration == null)
            {
                LogWarning("SpeedTreeVFXIntegrationManager not found");
            }
            
            LogInfo("✅ Connected to VFX management systems");
        }
        
        #endregion
        
        #region Trichrome VFX Instance Management
        
        public string CreateTrichromeVFXInstance(Transform attachmentPoint, PlantStrainSO strainData = null)
        {
            string instanceId = Guid.NewGuid().ToString();
            
            var instance = new TrichromeVFXInstance
            {
                InstanceId = instanceId,
                AttachmentPoint = attachmentPoint,
                StrainData = strainData,
                IsActive = true,
                CreationTime = Time.time,
                CurrentStage = TrichromeDevelopmentStage.Formation,
                TrichromeAmount = 0f,
                DevelopmentProgress = 0f,
                EmissionPoints = new List<TrichromeEmissionPoint>(),
                ActiveParticles = new List<string>()
            };
            
            // Create VFX components
            CreateVFXComponents(instance);
            
            // Initialize emission points based on attachment area
            CreateEmissionPoints(instance);
            
            // Apply genetic traits if available
            if (strainData != null)
            {
                ApplyGeneticTraits(instance, strainData);
            }
            
            _activeInstances[instanceId] = instance;
            
            LogInfo($"Trichrome VFX instance created: {instanceId[..8]} at {attachmentPoint.name}");
            OnInstanceCreated?.Invoke(instanceId, instance);
            
            return instanceId;
        }
        
        private void CreateVFXComponents(TrichromeVFXInstance instance)
        {
            #if UNITY_VFX_GRAPH
            // Create VFX Graph component
            var vfxObject = new GameObject($"TrichromeVFX_{instance.InstanceId[..8]}");
            vfxObject.transform.SetParent(instance.AttachmentPoint);
            vfxObject.transform.localPosition = Vector3.zero;
            
            var vfxComponent = vfxObject.AddComponent<VisualEffect>();
            
            // Get trichrome template from VFX manager
            if (_vfxTemplateManager != null)
            {
                var template = _vfxTemplateManager.GetTemplate(CannabisVFXType.TrichromeGrowth);
                if (template != null && template.VFXAsset != null)
                {
                    vfxComponent.visualEffectAsset = template.VFXAsset;
                }
            }
            
            instance.VFXComponent = vfxComponent;
            instance.GameObject = vfxObject;
            
            #else
            // Create fallback particle system
            var particleObject = new GameObject($"TrichromeParticles_{instance.InstanceId[..8]}");
            particleObject.transform.SetParent(instance.AttachmentPoint);
            particleObject.transform.localPosition = Vector3.zero;
            
            var particleSystem = particleObject.AddComponent<ParticleSystem>();
            ConfigureFallbackParticleSystem(particleSystem);
            
            instance.FallbackParticleSystem = particleSystem;
            instance.GameObject = particleObject;
            #endif
        }
        
        private void CreateEmissionPoints(TrichromeVFXInstance instance)
        {
            // Create emission points distributed across the attachment surface
            int numEmissionPoints = Mathf.RoundToInt(_trichromeDensity * 20f); // Scale based on density
            
            for (int i = 0; i < numEmissionPoints; i++)
            {
                var emissionPoint = new TrichromeEmissionPoint
                {
                    Id = Guid.NewGuid().ToString(),
                    LocalPosition = GetRandomEmissionPosition(),
                    IsActive = false,
                    ActivationTime = 0f,
                    EmissionRate = _emissionRate / numEmissionPoints,
                    DevelopmentStage = TrichromeDevelopmentStage.Formation
                };
                
                instance.EmissionPoints.Add(emissionPoint);
            }
            
            LogInfo($"Created {instance.EmissionPoints.Count} emission points for instance {instance.InstanceId[..8]}");
        }
        
        private Vector3 GetRandomEmissionPosition()
        {
            // Create random positions within a small area to simulate trichrome distribution
            float radius = 0.05f; // 5cm radius
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * radius;
            return new Vector3(randomCircle.x, 0f, randomCircle.y);
        }
        
        private void ApplyGeneticTraits(TrichromeVFXInstance instance, PlantStrainSO strainData)
        {
            if (!_enableGeneticVariation) return;
            
            // Apply strain-specific trichrome characteristics
            // In a full implementation, this would read actual genetic data
            
            float geneticTrichromeAmount = 0.6f; // Would get from strain genetics
            float geneticDensity = 0.7f; // Would get from strain genetics
            Color geneticColor = Color.Lerp(Color.white, new Color(0.9f, 0.9f, 0.8f), 0.3f);
            
            instance.GeneticTrichromeAmount = geneticTrichromeAmount;
            instance.GeneticDensity = geneticDensity;
            instance.GeneticColor = geneticColor;
            
            LogInfo($"Applied genetic traits to instance {instance.InstanceId[..8]}: Amount={geneticTrichromeAmount:F2}, Density={geneticDensity:F2}");
        }
        
        #endregion
        
        #region Trichrome Development Simulation
        
        private void StartTrichromeSimulation()
        {
            _isSimulationRunning = true;
            LogInfo("Trichrome development simulation started");
        }
        
        private void UpdateTrichromeSimulation()
        {
            if (!_isSimulationRunning) return;
            
            float deltaTime = Time.deltaTime;
            
            // Update global development parameters
            UpdateGlobalDevelopment(deltaTime);
            
            // Update each active instance
            foreach (var instance in _activeInstances.Values)
            {
                UpdateInstanceDevelopment(instance, deltaTime);
            }
            
            // Process emission queue
            ProcessEmissionQueue();
        }
        
        private void UpdateGlobalDevelopment(float deltaTime)
        {
            // Simulate overall trichrome development progression
            _simulationData.PhaseProgress += deltaTime * _developmentRate;
            
            // Advance development stage based on progress
            if (_simulationData.PhaseProgress >= 1f)
            {
                AdvanceDevelopmentStage();
                _simulationData.PhaseProgress = 0f;
            }
            
            // Update environmental influences
            UpdateEnvironmentalInfluences();
            
            // Update genetic expression
            UpdateGeneticExpression();
        }
        
        private void AdvanceDevelopmentStage()
        {
            var previousStage = _currentStage;
            
            switch (_currentStage)
            {
                case TrichromeDevelopmentStage.Formation:
                    _currentStage = TrichromeDevelopmentStage.Growth;
                    _simulationData.CurrentPhase = TrichromePhase.Expansion;
                    break;
                case TrichromeDevelopmentStage.Growth:
                    _currentStage = TrichromeDevelopmentStage.Maturation;
                    _simulationData.CurrentPhase = TrichromePhase.Maturation;
                    break;
                case TrichromeDevelopmentStage.Maturation:
                    _currentStage = TrichromeDevelopmentStage.Peak;
                    _simulationData.CurrentPhase = TrichromePhase.Peak;
                    break;
                case TrichromeDevelopmentStage.Peak:
                    _currentStage = TrichromeDevelopmentStage.Degradation;
                    _simulationData.CurrentPhase = TrichromePhase.Degradation;
                    break;
            }
            
            if (previousStage != _currentStage)
            {
                LogInfo($"Trichrome development stage advanced: {previousStage} → {_currentStage}");
                OnDevelopmentStageChanged?.Invoke(_currentStage);
            }
        }
        
        private void UpdateEnvironmentalInfluences()
        {
            if (!_enableEnvironmentalResponse) return;
            
            // Simulate environmental effects on trichrome development
            float lightEffect = Mathf.Lerp(0.5f, 1.2f, _lightInfluence);
            float tempEffect = Mathf.Lerp(0.8f, 1.1f, _temperatureInfluence);
            float humidityEffect = Mathf.Lerp(0.9f, 1.05f, _humidityInfluence);
            
            _simulationData.EnvironmentalInfluence = lightEffect * tempEffect * humidityEffect;
        }
        
        private void UpdateGeneticExpression()
        {
            if (!_enableGeneticVariation) return;
            
            // Simulate genetic expression affecting trichrome development
            _simulationData.GeneticExpression = Mathf.Lerp(0.8f, 1.2f, _geneticInfluenceStrength);
        }
        
        private void UpdateInstanceDevelopment(TrichromeVFXInstance instance, float deltaTime)
        {
            // Update instance development progress
            float developmentMultiplier = _simulationData.EnvironmentalInfluence * _simulationData.GeneticExpression;
            instance.DevelopmentProgress += deltaTime * _developmentRate * developmentMultiplier;
            instance.DevelopmentProgress = Mathf.Clamp01(instance.DevelopmentProgress);
            
            // Update trichrome amount based on development stage and progress
            UpdateTrichromeAmount(instance);
            
            // Update emission points
            UpdateEmissionPoints(instance);
            
            // Update VFX parameters
            UpdateVFXParameters(instance);
        }
        
        private void UpdateTrichromeAmount(TrichromeVFXInstance instance)
        {
            float targetAmount = GetTargetTrichromeAmount(instance);
            instance.TrichromeAmount = Mathf.Lerp(instance.TrichromeAmount, targetAmount, Time.deltaTime * 2f);
            
            // Update global amount if this is the primary instance
            if (_activeInstances.Count > 0)
            {
                float averageAmount = 0f;
                foreach (var inst in _activeInstances.Values)
                {
                    averageAmount += inst.TrichromeAmount;
                }
                
                float newGlobalAmount = averageAmount / _activeInstances.Count;
                if (Mathf.Abs(newGlobalAmount - _trichromeAmount) > 0.01f)
                {
                    _trichromeAmount = newGlobalAmount;
                    OnTrichromeAmountChanged?.Invoke(_trichromeAmount);
                }
            }
        }
        
        private float GetTargetTrichromeAmount(TrichromeVFXInstance instance)
        {
            float baseAmount = instance.GeneticTrichromeAmount;
            float stageMultiplier = GetStageMultiplier(_currentStage);
            float progressMultiplier = _opacityCurve.Evaluate(instance.DevelopmentProgress);
            
            return baseAmount * stageMultiplier * progressMultiplier;
        }
        
        private float GetStageMultiplier(TrichromeDevelopmentStage stage)
        {
            return stage switch
            {
                TrichromeDevelopmentStage.Formation => 0.1f,
                TrichromeDevelopmentStage.Growth => 0.4f,
                TrichromeDevelopmentStage.Maturation => 0.8f,
                TrichromeDevelopmentStage.Peak => 1.0f,
                TrichromeDevelopmentStage.Degradation => 0.7f,
                _ => 0.5f
            };
        }
        
        private void UpdateEmissionPoints(TrichromeVFXInstance instance)
        {
            foreach (var emissionPoint in instance.EmissionPoints)
            {
                // Activate emission points based on development progress
                float activationThreshold = UnityEngine.Random.Range(0f, 1f);
                
                if (!emissionPoint.IsActive && instance.DevelopmentProgress > activationThreshold)
                {
                    emissionPoint.IsActive = true;
                    emissionPoint.ActivationTime = Time.time;
                    emissionPoint.DevelopmentStage = _currentStage;
                    
                    // Queue for particle emission
                    _emissionQueue.Enqueue(emissionPoint);
                }
            }
        }
        
        private void UpdateVFXParameters(TrichromeVFXInstance instance)
        {
            #if UNITY_VFX_GRAPH
            if (instance.VFXComponent != null)
            {
                // Update VFX Graph parameters
                instance.VFXComponent.SetFloat("TrichromeAmount", instance.TrichromeAmount);
                instance.VFXComponent.SetFloat("TrichromeDensity", instance.GeneticDensity * _trichromeDensity);
                instance.VFXComponent.SetFloat("GrowthSpeed", _developmentRate);
                instance.VFXComponent.SetFloat("ParticleSize", _particleSize * _sizeCurve.Evaluate(instance.DevelopmentProgress));
                instance.VFXComponent.SetVector4("TrichromeColor", instance.GeneticColor);
            }
            #else
            if (instance.FallbackParticleSystem != null)
            {
                // Update fallback particle system
                var main = instance.FallbackParticleSystem.main;
                var emission = instance.FallbackParticleSystem.emission;
                
                main.startColor = instance.GeneticColor;
                main.startSize = _particleSize * _sizeCurve.Evaluate(instance.DevelopmentProgress);
                emission.rateOverTime = _emissionRate * instance.TrichromeAmount;
            }
            #endif
        }
        
        #endregion
        
        #region Particle Emission and Management
        
        private void ProcessEmissionQueue()
        {
            int processedThisFrame = 0;
            int maxProcessPerFrame = 5; // Limit for performance
            
            while (_emissionQueue.Count > 0 && processedThisFrame < maxProcessPerFrame)
            {
                var emissionPoint = _emissionQueue.Dequeue();
                EmitTrichromeParticle(emissionPoint);
                processedThisFrame++;
            }
        }
        
        private void EmitTrichromeParticle(TrichromeEmissionPoint emissionPoint)
        {
            var particle = GetAvailableParticle();
            if (particle == null) return;
            
            // Configure particle
            particle.IsActive = true;
            particle.Position = emissionPoint.LocalPosition;
            particle.Size = _particleSize;
            particle.Opacity = _opacityCurve.Evaluate(0f);
            particle.CreationTime = Time.time;
            particle.DevelopmentStage = emissionPoint.DevelopmentStage;
            particle.EmissionPointId = emissionPoint.Id;
            
            _activeParticles++;
        }
        
        private TrichromeParticleData GetAvailableParticle()
        {
            foreach (var particle in _particlePool)
            {
                if (!particle.IsActive)
                {
                    return particle;
                }
            }
            
            return null; // Pool exhausted
        }
        
        #endregion
        
        #region Performance and Utilities
        
        private void UpdatePerformanceMetrics()
        {
            _performanceMetrics.ActiveInstances = _activeInstances.Count;
            _performanceMetrics.ActiveParticles = _activeParticles;
            _performanceMetrics.EmissionRate = _emissionRate;
            _performanceMetrics.LastUpdate = DateTime.Now;
        }
        
        private void UpdateActiveInstances()
        {
            foreach (var instance in _activeInstances.Values)
            {
                if (instance.IsActive)
                {
                    // Update instance-specific logic
                    UpdateInstanceVisibility(instance);
                }
            }
        }
        
        private void UpdateInstanceVisibility(TrichromeVFXInstance instance)
        {
            if (instance.GameObject != null)
            {
                // Simple distance-based visibility
                bool isVisible = true;
                if (Camera.main != null)
                {
                    float distance = Vector3.Distance(Camera.main.transform.position, instance.GameObject.transform.position);
                    isVisible = distance < 20f; // 20m visibility range
                }
                
                instance.GameObject.SetActive(isVisible);
            }
        }
        
        private TrichromeGeneticProfile CreateDefaultGeneticProfile()
        {
            return new TrichromeGeneticProfile
            {
                BaseTrichromeAmount = 0.6f,
                MaxTrichromeAmount = 1.0f,
                DevelopmentRate = 1.0f,
                ColorVariation = 0.2f,
                SizeVariation = 0.3f,
                DensityVariation = 0.4f
            };
        }
        
        #if !UNITY_VFX_GRAPH
        private void ConfigureFallbackParticleSystem(ParticleSystem ps)
        {
            var main = ps.main;
            var emission = ps.emission;
            var shape = ps.shape;
            var sizeOverLifetime = ps.sizeOverLifetime;
            var colorOverLifetime = ps.colorOverLifetime;
            
            // Configure main module
            main.startLifetime = _particleLifetime;
            main.startSpeed = 0.01f;
            main.startSize = _particleSize;
            main.startColor = _trichromeColor;
            main.maxParticles = _maxTrichromeParticles;
            
            // Configure emission
            emission.rateOverTime = _emissionRate;
            
            // Configure shape
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.05f;
            
            // Configure size over lifetime
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, _sizeCurve);
            
            // Configure color over lifetime
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(1.0f, 0.3f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLifetime.color = gradient;
            
            // Set material if available
            if (_trichromeMaterial != null)
            {
                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                renderer.material = _trichromeMaterial;
            }
        }
        #endif
        
        #endregion
        
        #region Public Interface
        
        public void SetTrichromeAmount(float amount)
        {
            _trichromeAmount = Mathf.Clamp01(amount);
            OnTrichromeAmountChanged?.Invoke(_trichromeAmount);
        }
        
        public void SetTrichromeDensity(float density)
        {
            _trichromeDensity = Mathf.Clamp01(density);
        }
        
        public void SetDevelopmentRate(float rate)
        {
            _developmentRate = Mathf.Max(0.1f, rate);
        }
        
        public void SetEnvironmentalInfluences(float light, float temperature, float humidity)
        {
            _lightInfluence = Mathf.Clamp01(light);
            _temperatureInfluence = Mathf.Clamp01(temperature);
            _humidityInfluence = Mathf.Clamp01(humidity);
        }
        
        public TrichromeVFXInstance GetInstance(string instanceId)
        {
            return _activeInstances.ContainsKey(instanceId) ? _activeInstances[instanceId] : null;
        }
        
        public void DestroyInstance(string instanceId)
        {
            if (_activeInstances.ContainsKey(instanceId))
            {
                var instance = _activeInstances[instanceId];
                if (instance.GameObject != null)
                {
                    DestroyImmediate(instance.GameObject);
                }
                
                _activeInstances.Remove(instanceId);
                LogInfo($"Trichrome VFX instance destroyed: {instanceId[..8]}");
            }
        }
        
        public TrichromeVFXReport GetTrichromeReport()
        {
            return new TrichromeVFXReport
            {
                ActiveInstances = _activeInstances.Count,
                TotalParticles = _activeParticles,
                GlobalTrichromeAmount = _trichromeAmount,
                CurrentDevelopmentStage = _currentStage,
                SimulationRunning = _isSimulationRunning,
                PerformanceMetrics = _performanceMetrics
            };
        }
        
        #endregion
        
        #region Context Menu Actions
        
        [ContextMenu("Start Development Simulation")]
        public void StartSimulation()
        {
            StartTrichromeSimulation();
        }
        
        [ContextMenu("Stop Development Simulation")]
        public void StopSimulation()
        {
            _isSimulationRunning = false;
            LogInfo("Trichrome development simulation stopped");
        }
        
        [ContextMenu("Advance Development Stage")]
        public void ForceAdvanceStage()
        {
            AdvanceDevelopmentStage();
        }
        
        [ContextMenu("Reset Development")]
        public void ResetDevelopment()
        {
            _currentStage = TrichromeDevelopmentStage.Formation;
            _trichromeAmount = 0f;
            _simulationData.PhaseProgress = 0f;
            _simulationData.CurrentPhase = TrichromePhase.Initiation;
            
            LogInfo("Trichrome development reset to formation stage");
        }
        
        [ContextMenu("Show Trichrome Report")]
        public void ShowTrichromeReport()
        {
            var report = GetTrichromeReport();
            LogInfo("=== TRICHROME VFX REPORT ===");
            LogInfo($"Active Instances: {report.ActiveInstances}");
            LogInfo($"Total Particles: {report.TotalParticles}");
            LogInfo($"Trichrome Amount: {report.GlobalTrichromeAmount:F2}");
            LogInfo($"Development Stage: {report.CurrentDevelopmentStage}");
            LogInfo($"Simulation Running: {report.SimulationRunning}");
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            StopSimulation();
            
            // Cleanup all instances
            var instanceIds = new List<string>(_activeInstances.Keys);
            foreach (string instanceId in instanceIds)
            {
                DestroyInstance(instanceId);
            }
            
            StopAllCoroutines();
            CancelInvoke();
            
            LogInfo("Trichrome VFX Controller shutdown complete");
        }
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public enum TrichromeDevelopmentStage
    {
        Formation = 0,
        Growth = 1,
        Maturation = 2,
        Peak = 3,
        Degradation = 4
    }
    
    [System.Serializable]
    public enum TrichromePhase
    {
        Initiation = 0,
        Expansion = 1,
        Maturation = 2,
        Peak = 3,
        Degradation = 4
    }
    
    [System.Serializable]
    public class TrichromeVFXInstance
    {
        public string InstanceId;
        public Transform AttachmentPoint;
        public PlantStrainSO StrainData;
        public GameObject GameObject;
        public bool IsActive;
        public float CreationTime;
        public TrichromeDevelopmentStage CurrentStage;
        public float TrichromeAmount;
        public float DevelopmentProgress;
        public float GeneticTrichromeAmount;
        public float GeneticDensity;
        public Color GeneticColor;
        public List<TrichromeEmissionPoint> EmissionPoints;
        public List<string> ActiveParticles;
        
        #if UNITY_VFX_GRAPH
        public VisualEffect VFXComponent;
        #else
        public ParticleSystem FallbackParticleSystem;
        #endif
    }
    
    [System.Serializable]
    public class TrichromeEmissionPoint
    {
        public string Id;
        public Vector3 LocalPosition;
        public bool IsActive;
        public float ActivationTime;
        public float EmissionRate;
        public TrichromeDevelopmentStage DevelopmentStage;
    }
    
    [System.Serializable]
    public class TrichromeParticleData
    {
        public string Id;
        public bool IsActive;
        public Vector3 Position;
        public float Size;
        public float Opacity;
        public float CreationTime;
        public TrichromeDevelopmentStage DevelopmentStage;
        public string EmissionPointId;
    }
    
    [System.Serializable]
    public class TrichromeSimulationData
    {
        public float DevelopmentStartTime;
        public TrichromePhase CurrentPhase;
        public float PhaseProgress;
        public float EnvironmentalInfluence;
        public float GeneticExpression;
        public float MaturationRate;
    }
    
    [System.Serializable]
    public class TrichromeGeneticProfile
    {
        public float BaseTrichromeAmount;
        public float MaxTrichromeAmount;
        public float DevelopmentRate;
        public float ColorVariation;
        public float SizeVariation;
        public float DensityVariation;
    }
    
    [System.Serializable]
    public class TrichromeVFXPerformanceMetrics
    {
        public int ActiveInstances;
        public int ActiveParticles;
        public float EmissionRate;
        public float TargetFrameRate;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class TrichromeVFXReport
    {
        public int ActiveInstances;
        public int TotalParticles;
        public float GlobalTrichromeAmount;
        public TrichromeDevelopmentStage CurrentDevelopmentStage;
        public bool SimulationRunning;
        public TrichromeVFXPerformanceMetrics PerformanceMetrics;
    }
    
    #endregion
}