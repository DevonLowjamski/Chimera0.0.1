using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

#if UNITY_VFX_GRAPH
using UnityEngine.VFX;
#endif

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Controls comprehensive plant health visualization effects for cannabis plants.
    /// Manages visual indicators for stress, disease, nutrient deficiencies, vitality,
    /// and overall plant health status through sophisticated VFX systems.
    /// </summary>
    public class PlantHealthVFXController : ChimeraManager
    {
        [Header("Health VFX Configuration")]
        [SerializeField] private bool _enableHealthVFX = true;
        [SerializeField] private bool _enableRealtimeHealthUpdates = true;
        [SerializeField] private float _healthUpdateInterval = 0.2f;
        
        [Header("Health Status Settings")]
        [SerializeField, Range(0f, 1f)] private float _overallHealthLevel = 1.0f;
        [SerializeField, Range(0f, 1f)] private float _stressLevel = 0.0f;
        [SerializeField, Range(0f, 1f)] private float _vitalityLevel = 1.0f;
        [SerializeField, Range(0f, 1f)] private float _immuneResponseLevel = 0.8f;
        
        [Header("Stress Visualization")]
        [SerializeField] private bool _enableStressIndicators = true;
        [SerializeField] private Color _stressColor = Color.red;
        [SerializeField] private float _stressParticleIntensity = 0.5f;
        [SerializeField] private AnimationCurve _stressIntensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Vitality Visualization")]
        [SerializeField] private bool _enableVitalityGlow = true;
        [SerializeField] private Color _vitalityColor = Color.green;
        [SerializeField] private float _vitalityGlowIntensity = 0.8f;
        [SerializeField] private AnimationCurve _vitalityPulseCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1f);
        
        [Header("Disease Visualization")]
        [SerializeField] private bool _enableDiseaseIndicators = true;
        [SerializeField] private Color _diseaseColor = new Color(0.8f, 0.4f, 0.1f); // Orange-brown
        [SerializeField] private float _diseaseSpreadRate = 0.3f;
        
        [Header("Nutrient Deficiency Visualization")]
        [SerializeField] private bool _enableNutrientIndicators = true;
        [SerializeField] private Color _nitrogenDeficiencyColor = Color.yellow;
        [SerializeField] private Color _phosphorusDeficiencyColor = Color.magenta;
        [SerializeField] private Color _potassiumDeficiencyColor = new Color(0.8f, 0.6f, 0.2f);
        
        [Header("Environmental Stress Visualization")]
        [SerializeField] private bool _enableEnvironmentalStress = true;
        [SerializeField] private Color _heatStressColor = new Color(1f, 0.3f, 0.1f);
        [SerializeField] private Color _coldStressColor = new Color(0.3f, 0.5f, 1f);
        [SerializeField] private Color _lightStressColor = Color.white;
        [SerializeField] private Color _waterStressColor = new Color(0.4f, 0.2f, 0.8f);
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxConcurrentHealthUpdates = 25;
        [SerializeField] private float _healthCullingDistance = 20f;
        [SerializeField] private bool _enableLODHealthEffects = true;
        [SerializeField] private bool _enableAdaptiveQuality = true;
        
        // Health VFX State
        private Dictionary<string, PlantHealthVFXInstance> _activeHealthInstances = new Dictionary<string, PlantHealthVFXInstance>();
        private Queue<string> _healthUpdateQueue = new Queue<string>();
        private List<HealthStatusData> _pendingHealthUpdates = new List<HealthStatusData>();
        
        // VFX Integration
        private CannabisVFXTemplateManager _vfxTemplateManager;
        private SpeedTreeVFXIntegrationManager _speedTreeIntegration;
        private TrichromeVFXController _trichromeController;
        
        // Health Monitoring
        private Dictionary<PlantHealthCategory, HealthCategoryVFXData> _healthCategories;
        private HealthVFXPerformanceMetrics _performanceMetrics;
        private float _lastHealthUpdate = 0f;
        private int _healthUpdatesProcessedThisFrame = 0;
        
        // Events
        public System.Action<string, float> OnHealthLevelChanged;
        public System.Action<string, PlantHealthCategory, float> OnHealthCategoryChanged;
        public System.Action<string, PlantHealthVFXInstance> OnHealthInstanceCreated;
        public System.Action<HealthVFXPerformanceMetrics> OnHealthPerformanceUpdate;
        
        // Properties
        public float OverallHealthLevel => _overallHealthLevel;
        public float StressLevel => _stressLevel;
        public float VitalityLevel => _vitalityLevel;
        public int ActiveHealthInstances => _activeHealthInstances.Count;
        public HealthVFXPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        
        protected override void OnManagerInitialize()
        {
            InitializeHealthVFXSystem();
            InitializeHealthCategories();
            InitializePerformanceTracking();
            ConnectToVFXManagers();
            StartHealthMonitoring();
            LogInfo("Plant Health VFX Controller initialized");
        }
        
        private void Update()
        {
            if (_enableHealthVFX && Time.time - _lastHealthUpdate >= _healthUpdateInterval)
            {
                ProcessHealthUpdateQueue();
                UpdateHealthVFXEffects();
                UpdatePerformanceMetrics();
                _lastHealthUpdate = Time.time;
            }
        }
        
        #region Initialization
        
        private void InitializeHealthVFXSystem()
        {
            LogInfo("=== INITIALIZING PLANT HEALTH VFX SYSTEM ===");
            
            #if UNITY_VFX_GRAPH
            LogInfo("✅ VFX Graph available - health visualization effects enabled");
            #else
            LogWarning("⚠️ VFX Graph not available - using fallback health indicators");
            #endif
            
            // Initialize performance metrics
            _performanceMetrics = new HealthVFXPerformanceMetrics
            {
                ActiveHealthInstances = 0,
                HealthUpdatesPerSecond = 0f,
                AverageUpdateTime = 0f,
                TargetFrameRate = 60f,
                LastUpdate = DateTime.Now
            };
            
            LogInfo("✅ Health VFX system initialized");
        }
        
        private void InitializeHealthCategories()
        {
            LogInfo("Setting up plant health categories...");
            
            _healthCategories = new Dictionary<PlantHealthCategory, HealthCategoryVFXData>
            {
                [PlantHealthCategory.OverallHealth] = new HealthCategoryVFXData
                {
                    Category = PlantHealthCategory.OverallHealth,
                    CategoryName = "Overall Health",
                    HealthColor = _vitalityColor,
                    UnhealthyColor = _stressColor,
                    EffectIntensity = 1.0f,
                    RequiresVFXGraph = false,
                    Description = "General plant vitality and wellness"
                },
                
                [PlantHealthCategory.NutrientStatus] = new HealthCategoryVFXData
                {
                    Category = PlantHealthCategory.NutrientStatus,
                    CategoryName = "Nutrient Status",
                    HealthColor = Color.green,
                    UnhealthyColor = _nitrogenDeficiencyColor,
                    EffectIntensity = 0.8f,
                    RequiresVFXGraph = true,
                    Description = "Nutrient availability and uptake status"
                },
                
                [PlantHealthCategory.WaterStatus] = new HealthCategoryVFXData
                {
                    Category = PlantHealthCategory.WaterStatus,
                    CategoryName = "Water Status",
                    HealthColor = Color.cyan,
                    UnhealthyColor = _waterStressColor,
                    EffectIntensity = 0.7f,
                    RequiresVFXGraph = true,
                    Description = "Hydration and water stress indicators"
                },
                
                [PlantHealthCategory.EnvironmentalStress] = new HealthCategoryVFXData
                {
                    Category = PlantHealthCategory.EnvironmentalStress,
                    CategoryName = "Environmental Stress",
                    HealthColor = Color.white,
                    UnhealthyColor = _heatStressColor,
                    EffectIntensity = 0.9f,
                    RequiresVFXGraph = true,
                    Description = "Temperature, light, and atmospheric stress"
                },
                
                [PlantHealthCategory.DiseaseStatus] = new HealthCategoryVFXData
                {
                    Category = PlantHealthCategory.DiseaseStatus,
                    CategoryName = "Disease Status",
                    HealthColor = Color.green,
                    UnhealthyColor = _diseaseColor,
                    EffectIntensity = 1.0f,
                    RequiresVFXGraph = true,
                    Description = "Pathogen presence and immune response"
                },
                
                [PlantHealthCategory.PestPressure] = new HealthCategoryVFXData
                {
                    Category = PlantHealthCategory.PestPressure,
                    CategoryName = "Pest Pressure",
                    HealthColor = Color.green,
                    UnhealthyColor = new Color(0.6f, 0.3f, 0.1f),
                    EffectIntensity = 0.8f,
                    RequiresVFXGraph = true,
                    Description = "Pest infestation and damage indicators"
                }
            };
            
            LogInfo($"✅ Configured {_healthCategories.Count} health categories");
        }
        
        private void InitializePerformanceTracking()
        {
            InvokeRepeating(nameof(UpdateDetailedPerformanceMetrics), 1f, 1f);
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
        
        private void StartHealthMonitoring()
        {
            StartCoroutine(ContinuousHealthMonitoring());
            LogInfo("✅ Health monitoring started");
        }
        
        #endregion
        
        #region Health VFX Instance Management
        
        public string CreateHealthVFXInstance(Transform plantTransform, PlantStrainSO strainData = null)
        {
            string instanceId = Guid.NewGuid().ToString();
            
            var healthInstance = new PlantHealthVFXInstance
            {
                InstanceId = instanceId,
                PlantTransform = plantTransform,
                StrainData = strainData,
                IsActive = true,
                CreationTime = Time.time,
                LastUpdateTime = 0f,
                HealthLevels = new Dictionary<PlantHealthCategory, float>(),
                VFXComponents = new Dictionary<PlantHealthCategory, string>(),
                HealthHistory = new List<HealthHistoryEntry>()
            };
            
            // Initialize health levels
            InitializeHealthLevels(healthInstance);
            
            // Create VFX components for each health category
            CreateHealthVFXComponents(healthInstance);
            
            // Apply strain-specific health traits
            if (strainData != null)
            {
                ApplyStrainHealthTraits(healthInstance, strainData);
            }
            
            _activeHealthInstances[instanceId] = healthInstance;
            _healthUpdateQueue.Enqueue(instanceId);
            
            LogInfo($"Health VFX instance created: {instanceId[..8]} for plant {plantTransform.name}");
            OnHealthInstanceCreated?.Invoke(instanceId, healthInstance);
            
            return instanceId;
        }
        
        private void InitializeHealthLevels(PlantHealthVFXInstance instance)
        {
            // Initialize all health categories to healthy defaults
            foreach (var category in _healthCategories.Keys)
            {
                instance.HealthLevels[category] = 1.0f; // Start healthy
            }
            
            // Record initial health history
            var initialEntry = new HealthHistoryEntry
            {
                Timestamp = Time.time,
                OverallHealth = 1.0f,
                CategoryLevels = new Dictionary<PlantHealthCategory, float>(instance.HealthLevels),
                Notes = "Initial healthy state"
            };
            
            instance.HealthHistory.Add(initialEntry);
        }
        
        private void CreateHealthVFXComponents(PlantHealthVFXInstance instance)
        {
            #if UNITY_VFX_GRAPH
            if (_vfxTemplateManager != null)
            {
                foreach (var kvp in _healthCategories)
                {
                    var category = kvp.Key;
                    var categoryData = kvp.Value;
                    
                    if (categoryData.RequiresVFXGraph)
                    {
                        // Create VFX instance for this health category
                        string vfxInstanceId = _vfxTemplateManager.CreateVFXInstance(
                            CannabisVFXType.HealthIndicator,
                            instance.PlantTransform,
                            instance.PlantTransform.GetComponent<MonoBehaviour>()
                        );
                        
                        if (vfxInstanceId != null)
                        {
                            instance.VFXComponents[category] = vfxInstanceId;
                            
                            // Configure VFX for this specific health category
                            ConfigureHealthCategoryVFX(vfxInstanceId, category, categoryData);
                        }
                    }
                }
            }
            #endif
        }
        
        private void ConfigureHealthCategoryVFX(string vfxInstanceId, PlantHealthCategory category, HealthCategoryVFXData categoryData)
        {
            #if UNITY_VFX_GRAPH
            if (_vfxTemplateManager != null)
            {
                // Set category-specific VFX parameters
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "HealthCategory", (float)category);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "HealthyColor", categoryData.HealthColor);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "UnhealthyColor", categoryData.UnhealthyColor);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "EffectIntensity", categoryData.EffectIntensity);
                _vfxTemplateManager.UpdateVFXParameter(vfxInstanceId, "HealthLevel", 1.0f); // Start healthy
            }
            #endif
        }
        
        private void ApplyStrainHealthTraits(PlantHealthVFXInstance instance, PlantStrainSO strainData)
        {
            // Apply strain-specific health characteristics
            float strainVitality = 0.8f; // Would get from strain genetics
            float strainDiseaseResistance = 0.7f; // Would get from strain genetics
            float strainStressResistance = 0.6f; // Would get from strain genetics
            
            // Adjust health levels based on genetics
            instance.HealthLevels[PlantHealthCategory.OverallHealth] *= strainVitality;
            instance.HealthLevels[PlantHealthCategory.DiseaseStatus] *= strainDiseaseResistance;
            instance.HealthLevels[PlantHealthCategory.EnvironmentalStress] *= strainStressResistance;
            
            LogInfo($"Applied strain health traits to instance {instance.InstanceId[..8]}: Vitality={strainVitality:F2}, Disease Resistance={strainDiseaseResistance:F2}");
        }
        
        #endregion
        
        #region Health Status Updates
        
        public void UpdatePlantHealth(string instanceId, PlantHealthCategory category, float healthLevel)
        {
            if (!_activeHealthInstances.ContainsKey(instanceId))
            {
                LogError($"Health instance not found: {instanceId}");
                return;
            }
            
            var instance = _activeHealthInstances[instanceId];
            float previousLevel = instance.HealthLevels.ContainsKey(category) ? instance.HealthLevels[category] : 1.0f;
            
            // Update health level
            instance.HealthLevels[category] = Mathf.Clamp01(healthLevel);
            
            // Record health history if significant change
            if (Mathf.Abs(healthLevel - previousLevel) > 0.05f)
            {
                RecordHealthChange(instance, category, previousLevel, healthLevel);
            }
            
            // Queue for VFX update
            var healthUpdate = new HealthStatusData
            {
                InstanceId = instanceId,
                Category = category,
                HealthLevel = healthLevel,
                PreviousLevel = previousLevel,
                UpdateTime = Time.time
            };
            
            _pendingHealthUpdates.Add(healthUpdate);
            
            OnHealthCategoryChanged?.Invoke(instanceId, category, healthLevel);
        }
        
        public void UpdateOverallPlantHealth(string instanceId, float healthLevel)
        {
            if (!_activeHealthInstances.ContainsKey(instanceId))
            {
                LogError($"Health instance not found: {instanceId}");
                return;
            }
            
            var instance = _activeHealthInstances[instanceId];
            float previousLevel = instance.HealthLevels[PlantHealthCategory.OverallHealth];
            
            // Update overall health
            instance.HealthLevels[PlantHealthCategory.OverallHealth] = Mathf.Clamp01(healthLevel);
            
            // Update global health indicators
            _overallHealthLevel = healthLevel;
            _stressLevel = 1f - healthLevel; // Inverse relationship
            _vitalityLevel = Mathf.Lerp(0.3f, 1f, healthLevel); // Vitality correlates with health
            
            // Record significant changes
            if (Mathf.Abs(healthLevel - previousLevel) > 0.05f)
            {
                RecordHealthChange(instance, PlantHealthCategory.OverallHealth, previousLevel, healthLevel);
            }
            
            OnHealthLevelChanged?.Invoke(instanceId, healthLevel);
        }
        
        private void RecordHealthChange(PlantHealthVFXInstance instance, PlantHealthCategory category, float previousLevel, float newLevel)
        {
            var historyEntry = new HealthHistoryEntry
            {
                Timestamp = Time.time,
                OverallHealth = instance.HealthLevels[PlantHealthCategory.OverallHealth],
                CategoryLevels = new Dictionary<PlantHealthCategory, float>(instance.HealthLevels),
                Notes = $"{category} changed from {previousLevel:F2} to {newLevel:F2}"
            };
            
            instance.HealthHistory.Add(historyEntry);
            
            // Limit history size for performance
            if (instance.HealthHistory.Count > 100)
            {
                instance.HealthHistory.RemoveAt(0);
            }
        }
        
        #endregion
        
        #region Health VFX Processing
        
        private void ProcessHealthUpdateQueue()
        {
            _healthUpdatesProcessedThisFrame = 0;
            
            // Process pending health updates
            ProcessPendingHealthUpdates();
            
            // Update health instances in queue
            while (_healthUpdateQueue.Count > 0 && _healthUpdatesProcessedThisFrame < _maxConcurrentHealthUpdates)
            {
                string instanceId = _healthUpdateQueue.Dequeue();
                
                if (_activeHealthInstances.ContainsKey(instanceId))
                {
                    UpdateHealthInstanceVFX(instanceId);
                    _healthUpdatesProcessedThisFrame++;
                    
                    // Re-queue for next update cycle
                    _healthUpdateQueue.Enqueue(instanceId);
                }
            }
        }
        
        private void ProcessPendingHealthUpdates()
        {
            foreach (var healthUpdate in _pendingHealthUpdates)
            {
                ApplyHealthVFXUpdate(healthUpdate);
            }
            
            _pendingHealthUpdates.Clear();
        }
        
        private void ApplyHealthVFXUpdate(HealthStatusData healthUpdate)
        {
            var instance = _activeHealthInstances[healthUpdate.InstanceId];
            var categoryData = _healthCategories[healthUpdate.Category];
            
            #if UNITY_VFX_GRAPH
            if (instance.VFXComponents.ContainsKey(healthUpdate.Category))
            {
                string vfxInstanceId = instance.VFXComponents[healthUpdate.Category];
                
                // Update VFX parameters based on health level
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "HealthLevel", healthUpdate.HealthLevel);
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "HealthChangeRate", 
                    Mathf.Abs(healthUpdate.HealthLevel - healthUpdate.PreviousLevel) / Time.deltaTime);
                
                // Color interpolation based on health
                Color healthColor = Color.Lerp(categoryData.UnhealthyColor, categoryData.HealthColor, healthUpdate.HealthLevel);
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "CurrentHealthColor", healthColor);
                
                // Intensity based on severity
                float effectIntensity = GetHealthEffectIntensity(healthUpdate.Category, healthUpdate.HealthLevel);
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "EffectIntensity", effectIntensity);
            }
            #endif
        }
        
        private void UpdateHealthInstanceVFX(string instanceId)
        {
            var instance = _activeHealthInstances[instanceId];
            
            // Skip if plant is too far away
            if (_enableLODHealthEffects && IsInstanceCulled(instance))
            {
                SetInstanceVFXActive(instance, false);
                return;
            }
            
            SetInstanceVFXActive(instance, true);
            
            // Update health-based visual effects
            UpdateHealthBasedEffects(instance);
            
            // Update stress indicators
            UpdateStressIndicators(instance);
            
            // Update vitality glow
            UpdateVitalityGlow(instance);
            
            instance.LastUpdateTime = Time.time;
        }
        
        private bool IsInstanceCulled(PlantHealthVFXInstance instance)
        {
            if (Camera.main == null) return false;
            
            float distance = Vector3.Distance(Camera.main.transform.position, instance.PlantTransform.position);
            return distance > _healthCullingDistance;
        }
        
        private void SetInstanceVFXActive(PlantHealthVFXInstance instance, bool active)
        {
            #if UNITY_VFX_GRAPH
            foreach (string vfxInstanceId in instance.VFXComponents.Values)
            {
                if (vfxInstanceId != null)
                {
                    _vfxTemplateManager?.SetVFXActive(vfxInstanceId, active);
                }
            }
            #endif
        }
        
        private void UpdateHealthBasedEffects(PlantHealthVFXInstance instance)
        {
            foreach (var kvp in instance.HealthLevels)
            {
                var category = kvp.Key;
                var healthLevel = kvp.Value;
                
                if (instance.VFXComponents.ContainsKey(category))
                {
                    UpdateCategorySpecificEffects(instance, category, healthLevel);
                }
            }
        }
        
        private void UpdateCategorySpecificEffects(PlantHealthVFXInstance instance, PlantHealthCategory category, float healthLevel)
        {
            #if UNITY_VFX_GRAPH
            if (!instance.VFXComponents.ContainsKey(category)) return;
            
            string vfxInstanceId = instance.VFXComponents[category];
            var categoryData = _healthCategories[category];
            
            switch (category)
            {
                case PlantHealthCategory.NutrientStatus:
                    UpdateNutrientDeficiencyEffects(vfxInstanceId, healthLevel);
                    break;
                    
                case PlantHealthCategory.WaterStatus:
                    UpdateWaterStressEffects(vfxInstanceId, healthLevel);
                    break;
                    
                case PlantHealthCategory.EnvironmentalStress:
                    UpdateEnvironmentalStressEffects(vfxInstanceId, healthLevel);
                    break;
                    
                case PlantHealthCategory.DiseaseStatus:
                    UpdateDiseaseIndicators(vfxInstanceId, healthLevel);
                    break;
                    
                case PlantHealthCategory.PestPressure:
                    UpdatePestPressureEffects(vfxInstanceId, healthLevel);
                    break;
            }
            #endif
        }
        
        private void UpdateNutrientDeficiencyEffects(string vfxInstanceId, float nutrientLevel)
        {
            #if UNITY_VFX_GRAPH
            // Nutrient deficiency visualization
            float deficiencyLevel = 1f - nutrientLevel;
            
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "NutrientDeficiency", deficiencyLevel);
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "DeficiencyIntensity", deficiencyLevel * 2f);
            
            // Color based on deficiency type (simplified - would be more complex in real implementation)
            Color deficiencyColor = Color.Lerp(_nitrogenDeficiencyColor, _potassiumDeficiencyColor, deficiencyLevel);
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "DeficiencyColor", deficiencyColor);
            #endif
        }
        
        private void UpdateWaterStressEffects(string vfxInstanceId, float waterLevel)
        {
            #if UNITY_VFX_GRAPH
            float waterStress = 1f - waterLevel;
            
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "WaterStress", waterStress);
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "WiltingIntensity", waterStress);
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "WaterStressColor", _waterStressColor);
            #endif
        }
        
        private void UpdateEnvironmentalStressEffects(string vfxInstanceId, float environmentalHealth)
        {
            #if UNITY_VFX_GRAPH
            float environmentalStress = 1f - environmentalHealth;
            
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "EnvironmentalStress", environmentalStress);
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "HeatStress", environmentalStress * 0.6f);
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "LightStress", environmentalStress * 0.4f);
            
            // Color based on stress type
            Color stressColor = Color.Lerp(_heatStressColor, _coldStressColor, environmentalStress);
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "StressColor", stressColor);
            #endif
        }
        
        private void UpdateDiseaseIndicators(string vfxInstanceId, float diseaseResistance)
        {
            #if UNITY_VFX_GRAPH
            float diseaseLevel = 1f - diseaseResistance;
            
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "DiseaseLevel", diseaseLevel);
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "DiseaseSpread", diseaseLevel * _diseaseSpreadRate);
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "DiseaseColor", _diseaseColor);
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "InfectionIntensity", diseaseLevel * 1.5f);
            #endif
        }
        
        private void UpdatePestPressureEffects(string vfxInstanceId, float pestResistance)
        {
            #if UNITY_VFX_GRAPH
            float pestPressure = 1f - pestResistance;
            
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "PestPressure", pestPressure);
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "PestDamage", pestPressure * 0.8f);
            _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "PestActivityLevel", pestPressure);
            #endif
        }
        
        private void UpdateStressIndicators(PlantHealthVFXInstance instance)
        {
            if (!_enableStressIndicators) return;
            
            float overallStress = 1f - instance.HealthLevels[PlantHealthCategory.OverallHealth];
            float stressIntensity = _stressIntensityCurve.Evaluate(overallStress) * _stressParticleIntensity;
            
            #if UNITY_VFX_GRAPH
            foreach (string vfxInstanceId in instance.VFXComponents.Values)
            {
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "StressLevel", overallStress);
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "StressIntensity", stressIntensity);
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "StressColor", _stressColor);
            }
            #endif
        }
        
        private void UpdateVitalityGlow(PlantHealthVFXInstance instance)
        {
            if (!_enableVitalityGlow) return;
            
            float vitality = instance.HealthLevels[PlantHealthCategory.OverallHealth];
            float glowIntensity = _vitalityPulseCurve.Evaluate(vitality) * _vitalityGlowIntensity;
            
            #if UNITY_VFX_GRAPH
            foreach (string vfxInstanceId in instance.VFXComponents.Values)
            {
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "VitalityLevel", vitality);
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "VitalityGlow", glowIntensity);
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "VitalityColor", _vitalityColor);
            }
            #endif
        }
        
        private float GetHealthEffectIntensity(PlantHealthCategory category, float healthLevel)
        {
            float baseIntensity = _healthCategories[category].EffectIntensity;
            float healthMultiplier = 1f - healthLevel; // More effect when less healthy
            
            return baseIntensity * healthMultiplier;
        }
        
        #endregion
        
        #region Health Monitoring
        
        private IEnumerator ContinuousHealthMonitoring()
        {
            while (_enableRealtimeHealthUpdates)
            {
                yield return new WaitForSeconds(_healthUpdateInterval);
                
                // Monitor all active health instances
                foreach (var instance in _activeHealthInstances.Values)
                {
                    MonitorInstanceHealth(instance);
                }
            }
        }
        
        private void MonitorInstanceHealth(PlantHealthVFXInstance instance)
        {
            // Simulate health changes based on various factors
            // In a real implementation, this would connect to actual plant simulation data
            
            float deltaTime = Time.time - instance.LastUpdateTime;
            
            // Simulate gradual health changes
            SimulateHealthChanges(instance, deltaTime);
            
            // Check for health events
            CheckHealthEvents(instance);
        }
        
        private void SimulateHealthChanges(PlantHealthVFXInstance instance, float deltaTime)
        {
            // Simulate environmental effects on health
            float environmentalFactor = Mathf.Sin(Time.time * 0.1f) * 0.05f;
            
            // Gradual changes to health categories
            foreach (var category in instance.HealthLevels.Keys.ToList())
            {
                float currentLevel = instance.HealthLevels[category];
                float changeRate = GetHealthChangeRate(category);
                
                float newLevel = currentLevel + (environmentalFactor * changeRate * deltaTime);
                newLevel = Mathf.Clamp01(newLevel);
                
                if (Mathf.Abs(newLevel - currentLevel) > 0.01f)
                {
                    UpdatePlantHealth(instance.InstanceId, category, newLevel);
                }
            }
        }
        
        private float GetHealthChangeRate(PlantHealthCategory category)
        {
            return category switch
            {
                PlantHealthCategory.OverallHealth => 0.5f,
                PlantHealthCategory.NutrientStatus => 0.8f,
                PlantHealthCategory.WaterStatus => 1.0f,
                PlantHealthCategory.EnvironmentalStress => 0.7f,
                PlantHealthCategory.DiseaseStatus => 0.3f,
                PlantHealthCategory.PestPressure => 0.4f,
                _ => 0.5f
            };
        }
        
        private void CheckHealthEvents(PlantHealthVFXInstance instance)
        {
            // Check for critical health levels
            foreach (var kvp in instance.HealthLevels)
            {
                var category = kvp.Key;
                var healthLevel = kvp.Value;
                
                if (healthLevel < 0.2f) // Critical health
                {
                    TriggerHealthAlert(instance, category, healthLevel);
                }
                else if (healthLevel < 0.5f) // Warning level
                {
                    TriggerHealthWarning(instance, category, healthLevel);
                }
            }
        }
        
        private void TriggerHealthAlert(PlantHealthVFXInstance instance, PlantHealthCategory category, float healthLevel)
        {
            LogWarning($"CRITICAL HEALTH ALERT: Plant {instance.InstanceId[..8]} - {category}: {healthLevel:F2}");
            
            // Trigger emergency VFX
            #if UNITY_VFX_GRAPH
            if (instance.VFXComponents.ContainsKey(category))
            {
                string vfxInstanceId = instance.VFXComponents[category];
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "EmergencyAlert", 1f);
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "AlertIntensity", 2f);
            }
            #endif
        }
        
        private void TriggerHealthWarning(PlantHealthVFXInstance instance, PlantHealthCategory category, float healthLevel)
        {
            LogInfo($"Health Warning: Plant {instance.InstanceId[..8]} - {category}: {healthLevel:F2}");
            
            // Trigger warning VFX
            #if UNITY_VFX_GRAPH
            if (instance.VFXComponents.ContainsKey(category))
            {
                string vfxInstanceId = instance.VFXComponents[category];
                _vfxTemplateManager?.UpdateVFXParameter(vfxInstanceId, "WarningLevel", 1f);
            }
            #endif
        }
        
        #endregion
        
        #region Performance and Effects Management
        
        private void UpdateHealthVFXEffects()
        {
            // Update global health effects
            UpdateGlobalHealthIndicators();
            
            // Update adaptive quality if enabled
            if (_enableAdaptiveQuality)
            {
                UpdateAdaptiveQuality();
            }
        }
        
        private void UpdateGlobalHealthIndicators()
        {
            // Calculate average health across all instances
            if (_activeHealthInstances.Count > 0)
            {
                float totalHealth = 0f;
                float totalStress = 0f;
                float totalVitality = 0f;
                
                foreach (var instance in _activeHealthInstances.Values)
                {
                    totalHealth += instance.HealthLevels[PlantHealthCategory.OverallHealth];
                    totalStress += (1f - instance.HealthLevels[PlantHealthCategory.OverallHealth]);
                    totalVitality += instance.HealthLevels[PlantHealthCategory.OverallHealth];
                }
                
                _overallHealthLevel = totalHealth / _activeHealthInstances.Count;
                _stressLevel = totalStress / _activeHealthInstances.Count;
                _vitalityLevel = totalVitality / _activeHealthInstances.Count;
            }
        }
        
        private void UpdateAdaptiveQuality()
        {
            // Adjust VFX quality based on performance
            float currentFrameRate = 1f / Time.deltaTime;
            float targetFrameRate = _performanceMetrics.TargetFrameRate;
            
            if (currentFrameRate < targetFrameRate * 0.8f)
            {
                // Reduce quality
                ReduceHealthVFXQuality();
            }
            else if (currentFrameRate > targetFrameRate * 1.2f)
            {
                // Increase quality
                IncreaseHealthVFXQuality();
            }
        }
        
        private void ReduceHealthVFXQuality()
        {
            // Implement quality reduction strategies
            _maxConcurrentHealthUpdates = Mathf.Max(10, _maxConcurrentHealthUpdates - 2);
            _healthUpdateInterval = Mathf.Min(0.5f, _healthUpdateInterval + 0.05f);
        }
        
        private void IncreaseHealthVFXQuality()
        {
            // Implement quality increase strategies
            _maxConcurrentHealthUpdates = Mathf.Min(50, _maxConcurrentHealthUpdates + 1);
            _healthUpdateInterval = Mathf.Max(0.1f, _healthUpdateInterval - 0.02f);
        }
        
        private void UpdatePerformanceMetrics()
        {
            _performanceMetrics.ActiveHealthInstances = _activeHealthInstances.Count;
            _performanceMetrics.HealthUpdatesPerSecond = _healthUpdatesProcessedThisFrame / Time.deltaTime;
            _performanceMetrics.LastUpdate = DateTime.Now;
        }
        
        private void UpdateDetailedPerformanceMetrics()
        {
            OnHealthPerformanceUpdate?.Invoke(_performanceMetrics);
        }
        
        #endregion
        
        #region Public Interface
        
        public PlantHealthVFXInstance GetHealthInstance(string instanceId)
        {
            return _activeHealthInstances.ContainsKey(instanceId) ? _activeHealthInstances[instanceId] : null;
        }
        
        public List<PlantHealthVFXInstance> GetAllHealthInstances()
        {
            return new List<PlantHealthVFXInstance>(_activeHealthInstances.Values);
        }
        
        public void SetHealthCategory(string instanceId, PlantHealthCategory category, float level)
        {
            UpdatePlantHealth(instanceId, category, level);
        }
        
        public float GetHealthLevel(string instanceId, PlantHealthCategory category)
        {
            if (_activeHealthInstances.ContainsKey(instanceId))
            {
                var instance = _activeHealthInstances[instanceId];
                return instance.HealthLevels.ContainsKey(category) ? instance.HealthLevels[category] : 0f;
            }
            return 0f;
        }
        
        public void DestroyHealthInstance(string instanceId)
        {
            if (_activeHealthInstances.ContainsKey(instanceId))
            {
                var instance = _activeHealthInstances[instanceId];
                
                // Cleanup VFX instances
                foreach (string vfxInstanceId in instance.VFXComponents.Values)
                {
                    _vfxTemplateManager?.DestroyVFXInstance(vfxInstanceId);
                }
                
                _activeHealthInstances.Remove(instanceId);
                LogInfo($"Health VFX instance destroyed: {instanceId[..8]}");
            }
        }
        
        public PlantHealthReport GetHealthReport()
        {
            return new PlantHealthReport
            {
                ActiveInstances = _activeHealthInstances.Count,
                OverallHealthLevel = _overallHealthLevel,
                StressLevel = _stressLevel,
                VitalityLevel = _vitalityLevel,
                HealthCategories = _healthCategories.Keys.ToDictionary(k => k, k => _overallHealthLevel),
                PerformanceMetrics = _performanceMetrics
            };
        }
        
        #endregion
        
        #region Context Menu Actions
        
        [ContextMenu("Show Health Status")]
        public void ShowHealthStatus()
        {
            LogInfo("=== PLANT HEALTH VFX STATUS ===");
            LogInfo($"Active Health Instances: {_activeHealthInstances.Count}");
            LogInfo($"Overall Health Level: {_overallHealthLevel:F2}");
            LogInfo($"Stress Level: {_stressLevel:F2}");
            LogInfo($"Vitality Level: {_vitalityLevel:F2}");
            LogInfo($"Health Categories: {_healthCategories.Count}");
        }
        
        [ContextMenu("Simulate Health Crisis")]
        public void SimulateHealthCrisis()
        {
            foreach (var instance in _activeHealthInstances.Values)
            {
                UpdateOverallPlantHealth(instance.InstanceId, 0.2f);
                UpdatePlantHealth(instance.InstanceId, PlantHealthCategory.NutrientStatus, 0.1f);
                UpdatePlantHealth(instance.InstanceId, PlantHealthCategory.WaterStatus, 0.3f);
            }
            
            LogInfo("Health crisis simulation activated");
        }
        
        [ContextMenu("Restore Plant Health")]
        public void RestorePlantHealth()
        {
            foreach (var instance in _activeHealthInstances.Values)
            {
                foreach (var category in _healthCategories.Keys)
                {
                    UpdatePlantHealth(instance.InstanceId, category, 1.0f);
                }
            }
            
            LogInfo("Plant health restored to optimal levels");
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            StopAllCoroutines();
            
            // Cleanup all health instances
            var instanceIds = new List<string>(_activeHealthInstances.Keys);
            foreach (string instanceId in instanceIds)
            {
                DestroyHealthInstance(instanceId);
            }
            
            CancelInvoke();
            LogInfo("Plant Health VFX Controller shutdown complete");
        }
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public enum PlantHealthCategory
    {
        OverallHealth = 0,
        NutrientStatus = 1,
        WaterStatus = 2,
        EnvironmentalStress = 3,
        DiseaseStatus = 4,
        PestPressure = 5
    }
    
    [System.Serializable]
    public class PlantHealthVFXInstance
    {
        public string InstanceId;
        public Transform PlantTransform;
        public PlantStrainSO StrainData;
        public bool IsActive;
        public float CreationTime;
        public float LastUpdateTime;
        public Dictionary<PlantHealthCategory, float> HealthLevels;
        public Dictionary<PlantHealthCategory, string> VFXComponents;
        public List<HealthHistoryEntry> HealthHistory;
    }
    
    [System.Serializable]
    public class HealthCategoryVFXData
    {
        public PlantHealthCategory Category;
        public string CategoryName;
        public Color HealthColor;
        public Color UnhealthyColor;
        public float EffectIntensity;
        public bool RequiresVFXGraph;
        public string Description;
    }
    
    [System.Serializable]
    public class HealthStatusData
    {
        public string InstanceId;
        public PlantHealthCategory Category;
        public float HealthLevel;
        public float PreviousLevel;
        public float UpdateTime;
    }
    
    [System.Serializable]
    public class HealthHistoryEntry
    {
        public float Timestamp;
        public float OverallHealth;
        public Dictionary<PlantHealthCategory, float> CategoryLevels;
        public string Notes;
    }
    
    [System.Serializable]
    public class HealthVFXPerformanceMetrics
    {
        public int ActiveHealthInstances;
        public float HealthUpdatesPerSecond;
        public float AverageUpdateTime;
        public float TargetFrameRate;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class PlantHealthReport
    {
        public int ActiveInstances;
        public float OverallHealthLevel;
        public float StressLevel;
        public float VitalityLevel;
        public Dictionary<PlantHealthCategory, float> HealthCategories;
        public HealthVFXPerformanceMetrics PerformanceMetrics;
    }
    
    #endregion
}