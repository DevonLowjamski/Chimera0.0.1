using UnityEngine;
#if UNITY_URP
using UnityEngine.Rendering.Universal;
using URP = UnityEngine.Rendering.Universal;
#endif
using ProjectChimera.Core;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Facilities;
// using ProjectChimera.Systems.Cultivation; // Cannot reference without circular dependency
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using LightSpectrumData = ProjectChimera.Data.Environment.LightSpectrumData;

namespace ProjectChimera.Systems.Environment
{
    /// <summary>
    /// Advanced grow light system with full spectrum control, automation,
    /// and plant-specific optimization for cannabis cultivation.
    /// </summary>
    public class AdvancedGrowLightSystem : ChimeraSystem
    {
        [Header("Light Configuration")]
        [SerializeField] private Light[] _growLights;  // Added missing field
        [SerializeField] private float _maxIntensity = 1000f; // PPFD (μmol/m²/s)
        [SerializeField] private float _maxPowerConsumption = 600f; // Watts
        [SerializeField] private float _efficiency = 2.5f; // μmol/J
        [SerializeField] private Vector2 _coverageArea = new Vector2(4f, 4f); // meters
        
        [Header("Spectrum Control")]
        [SerializeField] private GrowLightType _lightType = GrowLightType.LED;
        [SerializeField] private bool _enableSpectrumControl = true;
        [SerializeField] private bool _enableDynamicSpectrum = true;
        [SerializeField] private AnimationCurve _spectrumTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Light Components")]
        [SerializeField] private Light _primaryLight;
        [SerializeField] private Light[] _supplementalLights;
        [SerializeField] private ParticleSystem _lightBeamEffect;
        [SerializeField] private Renderer _lightHousingRenderer;
        [SerializeField] private GameObject _heatSinkFan;
        
        [Header("Automation")]
        [SerializeField] private bool _enableScheduleControl = true;
        [SerializeField] private bool _enablePlantResponseOptimization = true;
        [SerializeField] private bool _enableEnergyOptimization = true;
        [SerializeField] private float _automationUpdateInterval = 30f;
        
        [Header("Visual Effects")]
        [SerializeField] private Material _lightMaterial;
        [SerializeField] private Color _baseEmissionColor = Color.white;
        [SerializeField] private ParticleSystem _photonParticles;
        [SerializeField] private AudioSource _fanAudioSource;
        [SerializeField] private AudioSource _ballastAudioSource;
        
        // Light State
        private bool _isOn = false;
        private float _currentIntensity = 0f;
        private float _targetIntensity = 0f;
        private float _currentPowerConsumption = 0f;
        private float _operatingTemperature = 25f;
        private ExtendedLightSchedule _activeSchedule;
        private GrowthStageSpectrum _targetSpectrumProfile;
        
        // Spectrum Management
        private LightSpectrumData _currentSpectrum;
        private Dictionary<PlantGrowthStage, LightSpectrumData> _spectrumProfiles;
        private LightSpectrumData _targetSpectrum;
        private bool _isTransitioningSpectrum = false;
        private float _spectrumTransitionProgress = 0f;
        
        // Plant Monitoring
        // private List<InteractivePlantComponent> _plantsInRange = new List<InteractivePlantComponent>();
        private float _lastPlantScan = 0f;
        private float _plantScanInterval = 10f;
        
        // Performance Metrics
        private LightPerformanceMetrics _performanceMetrics;
        private Queue<LightDataPoint> _performanceHistory = new Queue<LightDataPoint>();
        private const int MAX_HISTORY_SIZE = 1440; // 24 hours at 1-minute intervals
        
        // Automation
        private float _lastAutomationUpdate = 0f;
        private AutomationMode _automationMode = AutomationMode.Schedule;
        private bool _adaptiveControl = true;
        
        // Heat Management
        private float _ambientTemperature = 24f;
        private float _heatGeneration = 0f;
        private bool _thermalThrottling = false;
        
        // Placeholder variables for missing dependencies
        private float TimeManager; // Placeholder for missing TimeManager
        private List<object> allPlants = new List<object>(); // Placeholder for missing allPlants
        private List<object> stageGroups = new List<object>(); // Placeholder for missing stageGroups  
        private float averageHealth = 75f; // Placeholder for missing averageHealth

        // Events
        public System.Action<AdvancedGrowLightSystem, bool> OnLightStateChanged;
        public System.Action<AdvancedGrowLightSystem, float> OnIntensityChanged;
        public System.Action<AdvancedGrowLightSystem, LightSpectrumData> OnSpectrumChanged;
        public System.Action<AdvancedGrowLightSystem, LightAlert> OnLightAlert;
        
        // Properties
        public GrowLightType LightType => _lightType;
        public bool IsOn => _isOn;
        public float CurrentIntensity => _currentIntensity;
        public float TargetIntensity => _targetIntensity;
        public float PowerConsumption => _currentPowerConsumption;
        public float OperatingTemperature => _operatingTemperature;
        public LightSpectrumData CurrentSpectrum => _currentSpectrum;
        public Vector2 CoverageArea => _coverageArea;
        public LightPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        // public List<InteractivePlantComponent> PlantsInRange => _plantsInRange;
        
#if UNITY_URP
        private Dictionary<Light, URP.UniversalAdditionalLightData> _lightDataMap;
#else
        private Dictionary<Light, object> _lightDataMap; // Fallback for when URP not available
#endif
        
        protected override void Awake()
        {
            base.Awake();
            InitializeLightData();
            
            InitializeLightSystem();
        }
        
        protected override void OnSystemStart()
        {
            if (_growLights != null && _growLights.Length > 0)
            {
                foreach (var light in _growLights)
                {
                    if (light != null)
                    {
                        light.enabled = true;
                    }
                }
            }
        }

        protected override void OnSystemStop()
        {
            if (_growLights != null && _growLights.Length > 0)
            {
                foreach (var light in _growLights)
                {
                    if (light != null)
                    {
                        light.enabled = false;
                    }
                }
            }
        }
        
        private void InitializeLightData()
        {
#if UNITY_URP
            _lightDataMap = new Dictionary<Light, URP.UniversalAdditionalLightData>();
            
            if (_growLights != null)
            {
                foreach (var light in _growLights)
                {
                    if (light != null)
                    {
                        var additionalData = light.GetUniversalAdditionalLightData();
                        if (additionalData == null)
                        {
                            additionalData = light.gameObject.AddComponent<URP.UniversalAdditionalLightData>();
                        }
                        _lightDataMap[light] = additionalData;
                    }
                }
            }
#else
            _lightDataMap = new Dictionary<Light, object>();
            // Basic light setup without URP-specific features
            if (_growLights != null)
            {
                foreach (var light in _growLights)
                {
                    if (light != null)
                    {
                        _lightDataMap[light] = null; // Placeholder
                    }
                }
            }
#endif
        }
        
        private void Start()
        {
            SetupLightComponents();
            InitializeSpectrumProfiles();
            SetupAutomation();
            StartPerformanceMonitoring();
        }
        
        private void Update()
        {
            float currentTime = Time.time;
            
            // Update light intensity smoothly
            UpdateLightIntensity();
            
            // Update spectrum transition
            if (_isTransitioningSpectrum)
            {
                UpdateSpectrumTransition();
            }
            
            // Update automation
            if (currentTime - _lastAutomationUpdate >= _automationUpdateInterval)
            {
                ProcessAutomation();
                _lastAutomationUpdate = currentTime;
            }
            
            // Scan for plants in range
            if (currentTime - _lastPlantScan >= _plantScanInterval)
            {
                ScanForPlantsInRange();
                _lastPlantScan = currentTime;
            }
            
            // Update thermal management
            UpdateThermalManagement();
            
            // Update visual effects
            UpdateVisualEffects();
            
            // Update performance metrics
            UpdatePerformanceMetrics();
        }
        
        #region Initialization
        
        private void InitializeLightSystem()
        {
            // Initialize current spectrum based on light type
            _currentSpectrum = CreateDefaultSpectrum();
            _targetSpectrum = _currentSpectrum;
            
            // Initialize performance metrics
            _performanceMetrics = new LightPerformanceMetrics
            {
                LightId = gameObject.name,
                LightType = _lightType,
                MaxIntensity = _maxIntensity,
                MaxPowerConsumption = _maxPowerConsumption
            };
        }
        
        private LightSpectrumData CreateDefaultSpectrum()
        {
            return new ProjectChimera.Data.Environment.LightSpectrumData
            {
                UV_A_315_400nm = 15f,          // UV-A 315-400nm
                Blue_420_490nm = 100f,         // Blue 420-490nm  
                Green_490_550nm = 80f,         // Green 490-550nm
                Red_630_660nm = 120f,          // Red 630-660nm
                FarRed_700_850nm = 40f         // Far Red 700-850nm
            };
        }
        
        private void SetupLightComponents()
        {
            // Setup primary light
            if (_primaryLight == null)
                _primaryLight = GetComponent<Light>();
            
            if (_primaryLight == null)
            {
                _primaryLight = gameObject.AddComponent<Light>();
                _primaryLight.type = UnityEngine.LightType.Spot;
                _primaryLight.range = 10f;
                _primaryLight.spotAngle = 60f;
            }
            
            // Configure light for URP
#if UNITY_URP
            var lightData = _primaryLight.GetUniversalAdditionalLightData();
            if (lightData == null)
            {
                lightData = _primaryLight.gameObject.AddComponent<URP.UniversalAdditionalLightData>();
            }
#endif
            
            // Setup supplemental lights for spectrum control
            SetupSupplementalLights();
            
            // Setup visual components
            SetupVisualComponents();
            
            // Setup audio components
            SetupAudioComponents();
        }
        
        private void SetupSupplementalLights()
        {
            if (_supplementalLights == null || _supplementalLights.Length == 0)
            {
                _supplementalLights = new Light[4]; // UV, Blue, Red, Far-Red
                
                for (int i = 0; i < _supplementalLights.Length; i++)
                {
                    GameObject lightGO = new GameObject($"SupplementalLight_{i}");
                    lightGO.transform.SetParent(transform);
                    lightGO.transform.localPosition = Vector3.zero;
                    
                    var light = lightGO.AddComponent<Light>();
                    light.type = UnityEngine.LightType.Spot;
                    light.range = _primaryLight.range;
                    light.spotAngle = _primaryLight.spotAngle;
                    light.intensity = 0f;
                    light.enabled = false;
                    
                    // Add URP data
#if UNITY_URP
                    lightGO.AddComponent<URP.UniversalAdditionalLightData>();
#endif
                    
                    _supplementalLights[i] = light;
                }
                
                // Set spectrum-specific colors
                _supplementalLights[0].color = new Color(0.4f, 0.1f, 0.8f); // UV
                _supplementalLights[1].color = new Color(0.2f, 0.3f, 1f);   // Blue
                _supplementalLights[2].color = new Color(1f, 0.2f, 0.2f);   // Red
                _supplementalLights[3].color = new Color(0.8f, 0.1f, 0.1f); // Far-Red
            }
        }
        
        private void SetupVisualComponents()
        {
            if (_lightHousingRenderer == null)
                _lightHousingRenderer = GetComponent<Renderer>();
            
            if (_lightMaterial == null && _lightHousingRenderer != null)
            {
                _lightMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                _lightMaterial.EnableKeyword("_EMISSION");
                _lightHousingRenderer.material = _lightMaterial;
            }
            
            // Setup particle effects
            if (_photonParticles != null)
            {
                var main = _photonParticles.main;
                main.startColor = Color.white;
                main.startLifetime = 2f;
                main.startSpeed = 5f;
                main.maxParticles = 100;
                _photonParticles.Stop();
            }
            
            if (_lightBeamEffect != null)
            {
                var main = _lightBeamEffect.main;
                main.startColor = new Color(1f, 1f, 0.8f, 0.3f);
                main.startLifetime = 1f;
                main.startSpeed = 0.1f;
                _lightBeamEffect.Stop();
            }
        }
        
        private void SetupAudioComponents()
        {
            if (_fanAudioSource == null)
            {
                _fanAudioSource = gameObject.AddComponent<AudioSource>();
                _fanAudioSource.loop = true;
                _fanAudioSource.volume = 0.3f;
                _fanAudioSource.pitch = 1f;
                _fanAudioSource.spatialBlend = 1f; // 3D sound
            }
            
            if (_ballastAudioSource == null)
            {
                _ballastAudioSource = gameObject.AddComponent<AudioSource>();
                _ballastAudioSource.loop = true;
                _ballastAudioSource.volume = 0.1f;
                _ballastAudioSource.pitch = 1.2f;
                _ballastAudioSource.spatialBlend = 1f;
            }
        }
        
        private void InitializeSpectrumProfiles()
        {
            _spectrumProfiles = new Dictionary<PlantGrowthStage, LightSpectrumData>
            {
                [PlantGrowthStage.Seed] = new ProjectChimera.Data.Environment.LightSpectrumData
                {
                    UV_A_315_400nm = 2f, Blue_420_490nm = 30f, Green_490_550nm = 20f, Red_630_660nm = 35f, FarRed_700_850nm = 13f
                },
                [PlantGrowthStage.Seedling] = new ProjectChimera.Data.Environment.LightSpectrumData
                {
                    UV_A_315_400nm = 3f, Blue_420_490nm = 35f, Green_490_550nm = 18f, Red_630_660nm = 32f, FarRed_700_850nm = 12f
                },
                [PlantGrowthStage.Vegetative] = new ProjectChimera.Data.Environment.LightSpectrumData
                {
                    UV_A_315_400nm = 5f, Blue_420_490nm = 25f, Green_490_550nm = 15f, Red_630_660nm = 40f, FarRed_700_850nm = 15f
                },
                [PlantGrowthStage.Flowering] = new ProjectChimera.Data.Environment.LightSpectrumData
                {
                    UV_A_315_400nm = 8f, Blue_420_490nm = 15f, Green_490_550nm = 12f, Red_630_660nm = 50f, FarRed_700_850nm = 15f
                },
                [PlantGrowthStage.Harvest] = new ProjectChimera.Data.Environment.LightSpectrumData
                {
                    UV_A_315_400nm = 10f, Blue_420_490nm = 10f, Green_490_550nm = 10f, Red_630_660nm = 55f, FarRed_700_850nm = 15f
                }
            };
        }
        
        #endregion
        
        #region Light Control
        
        public void TurnOn()
        {
            if (_isOn) return;
            
            _isOn = true;
            _targetIntensity = _maxIntensity * 0.8f; // Start at 80% intensity
            
            OnLightStateChanged?.Invoke(this, true);
            
            StartCoroutine(SmoothLightTransition());
            UpdateAudioEffects();
            
            Debug.Log($"Grow light {gameObject.name} turned ON");
        }
        
        public void TurnOff()
        {
            if (!_isOn) return;
            
            _isOn = false;
            _targetIntensity = 0f;
            
            OnLightStateChanged?.Invoke(this, false);
            
            StartCoroutine(SmoothLightTransition());
            UpdateAudioEffects();
            
            Debug.Log($"Grow light {gameObject.name} turned OFF");
        }
        
        public void SetIntensity(float intensity)
        {
            intensity = Mathf.Clamp(intensity, 0f, _maxIntensity);
            
            if (!_thermalThrottling)
            {
                _targetIntensity = intensity;
                OnIntensityChanged?.Invoke(this, intensity);
            }
            else
            {
                float maxSafeIntensity = _maxIntensity * 0.7f; // 70% when thermal throttling
                _targetIntensity = Mathf.Min(intensity, maxSafeIntensity);
                
                if (intensity > maxSafeIntensity)
                {
                    TriggerThermalAlert();
                }
            }
        }
        
        private IEnumerator SmoothLightTransition()
        {
            float startIntensity = _currentIntensity;
            float transitionTime = _isOn ? 2f : 1f; // Slower ramp up, faster ramp down
            float elapsed = 0f;
            
            while (elapsed < transitionTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionTime;
                t = Mathf.SmoothStep(0f, 1f, t);
                
                _currentIntensity = Mathf.Lerp(startIntensity, _targetIntensity, t);
                ApplyLightIntensity();
                
                yield return null;
            }
            
            _currentIntensity = _targetIntensity;
            ApplyLightIntensity();
        }
        
        private void UpdateLightIntensity()
        {
            if (Mathf.Abs(_currentIntensity - _targetIntensity) > 1f)
            {
                float transitionSpeed = 50f; // PPFD per second
                _currentIntensity = Mathf.MoveTowards(_currentIntensity, _targetIntensity, 
                                                     transitionSpeed * Time.deltaTime);
                ApplyLightIntensity();
            }
        }
        
        private void ApplyLightIntensity()
        {
            if (_primaryLight != null)
            {
                // Convert PPFD to Unity light intensity (approximate conversion)
                float unityIntensity = _currentIntensity / 1000f * 2f; // Rough conversion
                _primaryLight.intensity = unityIntensity;
                _primaryLight.enabled = _currentIntensity > 0f;
            }
            
            // Update supplemental lights for spectrum
            UpdateSupplementalLightIntensities();
            
            // Update power consumption
            _currentPowerConsumption = (_currentIntensity / _maxIntensity) * _maxPowerConsumption;
            
            // Update heat generation
            _heatGeneration = _currentPowerConsumption * 0.7f; // 70% of power becomes heat
        }
        
        #endregion
        
        #region Spectrum Control
        
        public void SetSpectrum(LightSpectrumData spectrum)
        {
            if (!_enableSpectrumControl) return;
            
            _targetSpectrum = spectrum;
            StartSpectrumTransition();
            
            OnSpectrumChanged?.Invoke(this, spectrum);
        }
        
        public void SetSpectrumForGrowthStage(PlantGrowthStage stage)
        {
            if (_spectrumProfiles.ContainsKey(stage))
            {
                SetSpectrum(_spectrumProfiles[stage]);
                Debug.Log($"Set spectrum for {stage} stage");
            }
        }
        
        private void StartSpectrumTransition()
        {
            if (_isTransitioningSpectrum) return;
            
            _isTransitioningSpectrum = true;
            _spectrumTransitionProgress = 0f;
            
            StartCoroutine(SpectrumTransitionCoroutine());
        }
        
        private IEnumerator SpectrumTransitionCoroutine()
        {
            LightSpectrumData startSpectrum = _currentSpectrum;
            float transitionDuration = 30f; // 30 seconds for smooth transition
            
            while (_spectrumTransitionProgress < 1f)
            {
                _spectrumTransitionProgress += Time.deltaTime / transitionDuration;
                float t = _spectrumTransitionCurve.Evaluate(_spectrumTransitionProgress);
                
                _currentSpectrum = LerpSpectrum(startSpectrum, _targetSpectrum, t);
                ApplySpectrum();
                
                yield return null;
            }
            
            _currentSpectrum = _targetSpectrum;
            _isTransitioningSpectrum = false;
            ApplySpectrum();
        }
        
        private void UpdateSpectrumTransition()
        {
            // This method is called from Update() for additional spectrum updates
            if (_enableDynamicSpectrum)
            {
                UpdateDynamicSpectrum();
            }
        }
        
        private void UpdateDynamicSpectrum()
        {
            // Adjust spectrum based on time of day for naturalistic lighting
            float dayProgress = GetDayProgress();
            
            if (dayProgress >= 0f && dayProgress <= 1f)
            {
                // Simulate sunrise to sunset spectrum changes
                LightSpectrumData dynamicSpectrum = CalculateDynamicSpectrum(dayProgress);
                
                if (!_isTransitioningSpectrum)
                {
                    _currentSpectrum = LerpSpectrum(_currentSpectrum, dynamicSpectrum, Time.deltaTime * 0.1f);
                    ApplySpectrum();
                }
            }
        }
        
        private float GetDayProgress()
        {
            // var timeManager = GameManager.Instance?.GetManager<TimeManager>();
            // var currentTime = timeManager?.CurrentGameTime ?? DateTime.Now;
            var currentTime = DateTime.Now; // Placeholder - use system time
            float totalMinutes = currentTime.Hour * 60 + currentTime.Minute;
            return (totalMinutes - 360f) / 720f; // 6 AM to 6 PM normalized to 0-1
        }
        
        private LightSpectrumData CalculateDynamicSpectrum(float dayProgress)
        {
            // Morning: more blue, Evening: more red
            float blueIntensity = Mathf.Lerp(0.3f, 0.15f, dayProgress);
            float redIntensity = Mathf.Lerp(0.35f, 0.55f, dayProgress);
            
            return new ProjectChimera.Data.Environment.LightSpectrumData
            {
                UV_A_315_400nm = Mathf.Lerp(0.03f, 0.08f, dayProgress),
                Blue_420_490nm = blueIntensity,
                Green_490_550nm = 0.15f,
                Red_630_660nm = redIntensity,
                FarRed_700_850nm = 0.15f
            };
        }
        
        private LightSpectrumData LerpSpectrum(LightSpectrumData a, LightSpectrumData b, float t)
        {
            return new ProjectChimera.Data.Environment.LightSpectrumData
            {
                UV_A_315_400nm = Mathf.Lerp(a.UV_A_315_400nm, b.UV_A_315_400nm, t),
                Blue_420_490nm = Mathf.Lerp(a.Blue_420_490nm, b.Blue_420_490nm, t),
                Green_490_550nm = Mathf.Lerp(a.Green_490_550nm, b.Green_490_550nm, t),
                Red_630_660nm = Mathf.Lerp(a.Red_630_660nm, b.Red_630_660nm, t),
                FarRed_700_850nm = Mathf.Lerp(a.FarRed_700_850nm, b.FarRed_700_850nm, t)
            };
        }
        
        private void ApplySpectrum()
        {
            // Update primary light color based on spectrum
            Color spectrumColor = CalculateSpectrumColor(_currentSpectrum);
            if (_primaryLight != null)
            {
                _primaryLight.color = spectrumColor;
            }
            
            // Update supplemental lights
            UpdateSupplementalLightIntensities();
            
            // Update material emission
            UpdateEmissionColor(spectrumColor);
        }
        
        private Color CalculateSpectrumColor(LightSpectrumData spectrum)
        {
            // Combine spectrum components into RGB color
            float r = (spectrum.Red_630_660nm + spectrum.FarRed_700_850nm * 0.5f) / 200f;
            float g = (spectrum.Green_490_550nm + spectrum.Blue_420_490nm * 0.3f + spectrum.Red_630_660nm * 0.2f) / 200f;
            float b = (spectrum.Blue_420_490nm + spectrum.UV_A_315_400nm * 0.8f) / 150f;
            
            return new Color(r, g, b, 1f);
        }
        
        private void UpdateSupplementalLightIntensities()
        {
            if (_supplementalLights == null || !_enableSpectrumControl) return;
            
            float baseIntensity = _currentIntensity / _maxIntensity;
            
            // UV light
            if (_supplementalLights[0] != null)
            {
                _supplementalLights[0].intensity = baseIntensity * _currentSpectrum.UV_A_315_400nm * 2f;
                _supplementalLights[0].enabled = _supplementalLights[0].intensity > 0.01f;
            }
            
            // Blue light
            if (_supplementalLights[1] != null)
            {
                _supplementalLights[1].intensity = baseIntensity * _currentSpectrum.Blue_420_490nm * 2f;
                _supplementalLights[1].enabled = _supplementalLights[1].intensity > 0.01f;
            }
            
            // Red light
            if (_supplementalLights[2] != null)
            {
                _supplementalLights[2].intensity = baseIntensity * _currentSpectrum.Red_630_660nm * 2f;
                _supplementalLights[2].enabled = _supplementalLights[2].intensity > 0.01f;
            }
            
            // Far-Red light
            if (_supplementalLights[3] != null)
            {
                _supplementalLights[3].intensity = baseIntensity * _currentSpectrum.FarRed_700_850nm * 2f;
                _supplementalLights[3].enabled = _supplementalLights[3].intensity > 0.01f;
            }
        }
        
        #endregion
        
        #region Plant Monitoring and Optimization
        
        private void ScanForPlantsInRange()
        {
            // _plantsInRange.Clear();
            
            // var allPlants = UnityEngine.Object.FindObjectsByType<InteractivePlantComponent>(FindObjectsSortMode.None);
            var allPlants = new object[0]; // Placeholder - empty array (avoiding InteractivePlantComponent reference)
            
            foreach (var plant in allPlants)
            {
                // if (plant != null && IsPlantInRange(plant))
                // {
                //     // _plantsInRange.Add(plant);
                // }
            }
            
            // Optimize spectrum based on plants found
            // if (_enablePlantResponseOptimization && _plantsInRange.Count > 0)
            {
                OptimizeForPlants();
            }
        }
        
        // private bool IsPlantInRange(InteractivePlantComponent plant)
        // {
        //     Vector3 lightPosition = transform.position;
        //     Vector3 plantPosition = plant.transform.position;
        //     
        //     // Check if plant is within coverage area
        //     float distance = Vector3.Distance(lightPosition, plantPosition);
        //     float maxRange = Mathf.Max(_coverageArea.x, _coverageArea.y) / 2f;
        //     
        //     if (distance > maxRange) return false;
        //     
        //     // Check if plant is within light cone (for spot lights)
        //     if (_primaryLight != null && _primaryLight.type == LightType.Spot)
        //     {
        //         Vector3 lightDirection = transform.forward;
        //         Vector3 toPlant = (plantPosition - lightPosition).normalized;
        //         float angle = Vector3.Angle(lightDirection, toPlant);
        //         
        //         return angle <= _primaryLight.spotAngle / 2f;
        //     }
        //     
        //     return true;
        // }
        
        private void OptimizeForPlants()
        {
            // if (_plantsInRange.Count == 0) return;
            
            // Determine optimal spectrum based on plant stages
            // var stageGroups = _plantsInRange.GroupBy(p => p.CurrentStage).ToList();
            var stageGroups = new List<object>(); // Placeholder - avoiding IGrouping<PlantGrowthStage, InteractivePlantComponent>
            
            if (stageGroups.Count == 1)
            {
                // All plants are in the same stage
                // var stage = stageGroups[0].Key;
                var stage = PlantGrowthStage.Seedling; // Placeholder default stage
                if (_spectrumProfiles.ContainsKey(stage))
                {
                    SetSpectrum(_spectrumProfiles[stage]);
                }
            }
            else
            {
                // Mixed stages - calculate weighted average spectrum
                // LightSpectrumData optimizedSpectrum = CalculateOptimizedSpectrum(stageGroups);
                // Placeholder - use default spectrum when method is unavailable due to circular dependencies
                LightSpectrumData optimizedSpectrum = CreateDefaultSpectrum();
                SetSpectrum(optimizedSpectrum);
            }
            
            // Adjust intensity based on plant health
            OptimizeIntensityForPlantHealth();
        }
        
        // private LightSpectrumData CalculateOptimizedSpectrum(IEnumerable<IGrouping<PlantGrowthStage, InteractivePlantComponent>> stageGroups)
        // {
        //     var result = new ProjectChimera.Data.Environment.LightSpectrumData();
        //     float totalWeight = 0f;
        //     
        //     foreach (var group in stageGroups)
        //     {
        //         if (_spectrumProfiles.ContainsKey(group.Key))
        //         {
        //             float weight = group.Count();
        //             var spectrum = _spectrumProfiles[group.Key];
        //             
        //             result.UVA += spectrum.UVA * weight;
        //             result.Blue += spectrum.Blue * weight;
        //             result.Green += spectrum.Green * weight;
        //             result.Red += spectrum.Red * weight;
        //             result.FarRed += spectrum.FarRed * weight;
        //             
        //             totalWeight += weight;
        //         }
        //     }
        //     
        //     if (totalWeight > 0f)
        //     {
        //         result.UVA /= totalWeight;
        //         result.Blue /= totalWeight;
        //         result.Green /= totalWeight;
        //         result.Red /= totalWeight;
        //         result.FarRed /= totalWeight;
        //     }
        //     
        //     return result;
        // }
        
        private void OptimizeIntensityForPlantHealth()
        {
            // if (_plantsInRange.Count == 0) return;
            
            // float averageHealth = _plantsInRange.Average(p => p.Health);
            float averageHealth = 75f; // Placeholder - default healthy value
            
            // Adjust intensity based on plant health
            if (averageHealth < 50f)
            {
                // Reduce intensity for struggling plants
                SetIntensity(_targetIntensity * 0.8f);
            }
            else if (averageHealth > 80f)
            {
                // Increase intensity for healthy plants (if not at max)
                float targetIntensity = Mathf.Min(_maxIntensity, _targetIntensity * 1.1f);
                SetIntensity(targetIntensity);
            }
        }
        
        #endregion
        
        #region Automation
        
        private void SetupAutomation()
        {
            if (!_enableScheduleControl) return;
            
            // Create default schedule if none exists
            if (_activeSchedule == null)
            {
                _activeSchedule = CreateDefaultSchedule();
            }
        }
        
        private ExtendedLightSchedule CreateDefaultSchedule()
        {
            return new ExtendedLightSchedule
            {
                ScheduleName = "18/6 Vegetative",
                LightPeriodHours = 18,
                DarkPeriodHours = 6,
                LightStartTime = new TimeSpan(6, 0, 0),
                IntensityRampTime = 30,
                IsActive = true,
                MaxIntensity = _maxIntensity * 0.8f
            };
        }
        
        private void ProcessAutomation()
        {
            if (_automationMode == AutomationMode.Manual) return;
            
            switch (_automationMode)
            {
                case AutomationMode.Schedule:
                    ProcessScheduleAutomation();
                    break;
                case AutomationMode.Adaptive:
                    ProcessAdaptiveAutomation();
                    break;
                case AutomationMode.PlantOptimized:
                    ProcessPlantOptimizedAutomation();
                    break;
            }
            
            if (_enableEnergyOptimization)
            {
                ProcessEnergyOptimization();
            }
        }
        
        private void ProcessScheduleAutomation()
        {
            if (_activeSchedule == null || !_activeSchedule.IsActive) return;
            
            var currentTime = DateTime.Now; // Placeholder - use system time
            
            bool shouldBeOn = IsWithinLightPeriod(currentTime, _activeSchedule);
            
            if (shouldBeOn && !_isOn)
            {
                TurnOn();
                SetIntensity(_activeSchedule.MaxIntensity);
            }
            else if (!shouldBeOn && _isOn)
            {
                TurnOff();
            }
        }
        
        private bool IsWithinLightPeriod(DateTime currentTime, ExtendedLightSchedule schedule)
        {
            TimeSpan currentTimeOfDay = currentTime.TimeOfDay;
            TimeSpan lightStart = schedule.LightStartTime;
            TimeSpan lightEnd = lightStart.Add(TimeSpan.FromHours(schedule.LightPeriodHours));
            
            if (lightEnd.TotalHours <= 24)
            {
                return currentTimeOfDay >= lightStart && currentTimeOfDay < lightEnd;
            }
            else
            {
                // Light period spans midnight
                TimeSpan nextDayEnd = lightEnd.Subtract(TimeSpan.FromDays(1));
                return currentTimeOfDay >= lightStart || currentTimeOfDay < nextDayEnd;
            }
        }
        
        private void ProcessAdaptiveAutomation()
        {
            // Adapt to environmental conditions and plant responses
            AdaptToEnvironmentalConditions();
            AdaptToPlantResponses();
        }
        
        private void AdaptToEnvironmentalConditions()
        {
            // Get environmental data from nearby systems
            var environment = SampleEnvironmentalConditions();
            
            // Adjust intensity based on temperature
            if (environment.Temperature > 28f && _isOn)
            {
                // Reduce intensity to lower heat production
                float reductionFactor = Mathf.Clamp01(1f - ((environment.Temperature - 28f) / 5f));
                SetIntensity(_targetIntensity * reductionFactor);
            }
            
            // Adjust spectrum based on CO2 levels
            if (environment.CO2Level > 1000f)
            {
                // Increase red spectrum for enhanced photosynthesis
                var enhancedSpectrum = _currentSpectrum;
                enhancedSpectrum.Red_630_660nm = Mathf.Min(0.6f, enhancedSpectrum.Red_630_660nm * 1.1f);
                SetSpectrum(enhancedSpectrum);
            }
        }
        
        private EnvironmentalConditions SampleEnvironmentalConditions()
        {
            // Find nearby environmental sensors or controllers
            // var growRoomController = GetComponentInParent<AdvancedGrowRoomController>();
            // if (growRoomController != null)
            // {
            //     return growRoomController.CurrentConditions;
            // }
            
            // Fallback to default conditions
            return new EnvironmentalConditions
            {
                Temperature = 24f,
                Humidity = 60f,
                CO2Level = 400f,
                LightIntensity = _currentIntensity,
                AirVelocity = 0.5f  // Using AirVelocity instead of AirFlow
            };
        }
        
        private void AdaptToPlantResponses()
        {
            // if (_plantsInRange.Count == 0) return;
            
            // Monitor plant health trends
            // float avgHealth = _plantsInRange.Average(p => p.Health);
            // float avgGrowthProgress = _plantsInRange.Average(p => p.GrowthProgress);
            
            // Placeholder values since plant monitoring is commented out
            float avgHealth = 75f; // Default healthy value
            float avgGrowthProgress = 0.5f; // Default progress
            
            // Adjust lighting based on plant performance
            if (avgHealth < 70f)
            {
                // Plants are struggling - be more conservative
                SetIntensity(_targetIntensity * 0.9f);
            }
            else if (avgHealth > 85f && avgGrowthProgress > 0.7f)
            {
                // Plants are thriving - optimize for maximum growth
                SetIntensity(Mathf.Min(_maxIntensity, _targetIntensity * 1.05f));
            }
        }
        
        private void ProcessPlantOptimizedAutomation()
        {
            // Fully automated optimization based on plant feedback
            OptimizeForPlants();
            
            // Dynamic schedule adjustment
            AdjustScheduleBasedOnPlantStage();
        }
        
        private void AdjustScheduleBasedOnPlantStage()
        {
            // if (_plantsInRange.Count == 0) return;
            
            // var dominantStage = _plantsInRange.GroupBy(p => p.CurrentStage)
            //                                  .OrderByDescending(g => g.Count())
            //                                  .First().Key;
            
            // Adjust light schedule based on dominant plant stage
            var dominantStage = PlantGrowthStage.Vegetative; // Default placeholder
            switch (dominantStage)
            {
                case PlantGrowthStage.Vegetative:
                    UpdateSchedule(18, 6); // 18/6 for vegetative
                    break;
                case PlantGrowthStage.Flowering:
                    UpdateSchedule(12, 12); // 12/12 for flowering
                    break;
            }
        }
        
        private void UpdateSchedule(int lightHours, int darkHours)
        {
            if (_activeSchedule != null)
            {
                _activeSchedule.LightPeriodHours = lightHours;
                _activeSchedule.DarkPeriodHours = darkHours;
            }
        }
        
        private void ProcessEnergyOptimization()
        {
            // Optimize energy consumption while maintaining plant health
            float efficiencyTarget = 0.85f; // Target 85% efficiency
            float currentEfficiency = CalculateCurrentEfficiency();
            
            if (currentEfficiency < efficiencyTarget)
            {
                // Reduce intensity slightly to improve efficiency
                SetIntensity(_targetIntensity * 0.98f);
            }
        }
        
        private float CalculateCurrentEfficiency()
        {
            if (_currentPowerConsumption <= 0f) return 1f;
            
            // Calculate efficiency as PPFD per watt
            float ppfdPerWatt = _currentIntensity / _currentPowerConsumption;
            float maxPossibleEfficiency = _maxIntensity / _maxPowerConsumption;
            
            return ppfdPerWatt / maxPossibleEfficiency;
        }
        
        #endregion
        
        #region Thermal Management
        
        private void UpdateThermalManagement()
        {
            // Update operating temperature
            UpdateOperatingTemperature();
            
            // Check for thermal throttling
            CheckThermalThrottling();
            
            // Update cooling systems
            UpdateCoolingSystems();
        }
        
        private void UpdateOperatingTemperature()
        {
            // Calculate temperature based on power consumption and ambient temperature
            float heatFromPower = (_currentPowerConsumption / _maxPowerConsumption) * 15f; // Max 15°C increase
            float targetTemp = _ambientTemperature + heatFromPower;
            
            // Smooth temperature changes
            _operatingTemperature = Mathf.Lerp(_operatingTemperature, targetTemp, Time.deltaTime * 0.5f);
        }
        
        private void CheckThermalThrottling()
        {
            bool shouldThrottle = _operatingTemperature > 50f; // 50°C threshold
            
            if (shouldThrottle && !_thermalThrottling)
            {
                _thermalThrottling = true;
                TriggerThermalAlert();
                Debug.LogWarning($"Thermal throttling activated for {gameObject.name}");
            }
            else if (!shouldThrottle && _thermalThrottling)
            {
                _thermalThrottling = false;
                Debug.Log($"Thermal throttling deactivated for {gameObject.name}");
            }
        }
        
        private void UpdateCoolingSystems()
        {
            // Update heat sink fan based on temperature
            if (_heatSinkFan != null)
            {
                bool fanShouldRun = _operatingTemperature > 35f;
                _heatSinkFan.SetActive(fanShouldRun);
                
                // Update fan speed audio
                if (_fanAudioSource != null && fanShouldRun)
                {
                    float fanSpeed = Mathf.Clamp01((_operatingTemperature - 35f) / 15f);
                    _fanAudioSource.pitch = 0.8f + fanSpeed * 0.4f;
                    _fanAudioSource.volume = 0.2f + fanSpeed * 0.3f;
                    
                    if (!_fanAudioSource.isPlaying)
                        _fanAudioSource.Play();
                }
                else if (_fanAudioSource != null && _fanAudioSource.isPlaying)
                {
                    _fanAudioSource.Stop();
                }
            }
        }
        
        private void TriggerThermalAlert()
        {
            var alert = new LightAlert
            {
                AlertType = LightAlertType.Thermal,
                Severity = ProjectChimera.Data.Automation.AlertSeverity.Warning,
                Message = $"Light overheating: {_operatingTemperature:F1}°C",
                LightId = gameObject.name,
                Timestamp = DateTime.Now
            };
            
            OnLightAlert?.Invoke(this, alert);
        }
        
        #endregion
        
        #region Visual Effects
        
        private void UpdateVisualEffects()
        {
            UpdateEmissionColor();
            UpdateParticleEffects();
            UpdateLightBeam();
        }
        
        private void UpdateEmissionColor(Color? overrideColor = null)
        {
            if (_lightMaterial == null) return;
            
            Color emissionColor = overrideColor ?? CalculateSpectrumColor(_currentSpectrum);
            float intensity = (_currentIntensity / _maxIntensity) * 2f;
            
            emissionColor *= intensity;
            _lightMaterial.SetColor("_EmissionColor", emissionColor);
        }
        
        private void UpdateParticleEffects()
        {
            // Update photon particles
            if (_photonParticles != null)
            {
                var emission = _photonParticles.emission;
                emission.rateOverTime = (_currentIntensity / _maxIntensity) * 50f;
                
                var main = _photonParticles.main;
                main.startColor = CalculateSpectrumColor(_currentSpectrum);
                
                if (_isOn && _currentIntensity > 50f && !_photonParticles.isPlaying)
                {
                    _photonParticles.Play();
                }
                else if ((!_isOn || _currentIntensity <= 50f) && _photonParticles.isPlaying)
                {
                    _photonParticles.Stop();
                }
            }
        }
        
        private void UpdateLightBeam()
        {
            if (_lightBeamEffect != null)
            {
                bool shouldShowBeam = _isOn && _currentIntensity > 100f;
                
                if (shouldShowBeam && !_lightBeamEffect.isPlaying)
                {
                    var main = _lightBeamEffect.main;
                    main.startColor = new Color(CalculateSpectrumColor(_currentSpectrum).r,
                                               CalculateSpectrumColor(_currentSpectrum).g,
                                               CalculateSpectrumColor(_currentSpectrum).b,
                                               0.3f);
                    _lightBeamEffect.Play();
                }
                else if (!shouldShowBeam && _lightBeamEffect.isPlaying)
                {
                    _lightBeamEffect.Stop();
                }
            }
        }
        
        private void UpdateAudioEffects()
        {
            // Update ballast hum
            if (_ballastAudioSource != null)
            {
                if (_isOn && _currentIntensity > 0f)
                {
                    if (!_ballastAudioSource.isPlaying)
                        _ballastAudioSource.Play();
                    
                    _ballastAudioSource.volume = 0.05f + (_currentIntensity / _maxIntensity) * 0.15f;
                }
                else if (_ballastAudioSource.isPlaying)
                {
                    _ballastAudioSource.Stop();
                }
            }
        }
        
        #endregion
        
        #region Performance Monitoring
        
        private void StartPerformanceMonitoring()
        {
            InvokeRepeating(nameof(CollectPerformanceData), 0f, 60f); // Every minute
        }
        
        private void CollectPerformanceData()
        {
            var dataPoint = new LightDataPoint
            {
                Timestamp = DateTime.Now,
                Intensity = _currentIntensity,
                PowerConsumption = _currentPowerConsumption,
                OperatingTemperature = _operatingTemperature,
                Spectrum = _currentSpectrum,
                // PlantsInRange = _plantsInRange.Count
            };
            
            _performanceHistory.Enqueue(dataPoint);
            
            // Maintain maximum history size
            while (_performanceHistory.Count > MAX_HISTORY_SIZE)
            {
                _performanceHistory.Dequeue();
            }
        }
        
        private void UpdatePerformanceMetrics()
        {
            if (_performanceHistory.Count == 0) return;
            
            var recentData = _performanceHistory.TakeLast(60).ToList(); // Last hour
            
            _performanceMetrics.AverageIntensity = recentData.Average(d => d.Intensity);
            _performanceMetrics.AveragePowerConsumption = recentData.Average(d => d.PowerConsumption);
            _performanceMetrics.AverageTemperature = recentData.Average(d => d.OperatingTemperature);
            _performanceMetrics.TotalOperatingHours += Time.deltaTime / 3600f;
            _performanceMetrics.Efficiency = CalculateCurrentEfficiency();
            _performanceMetrics.LastUpdate = DateTime.Now;
            
            // Calculate plant coverage effectiveness
            // if (_plantsInRange.Count > 0)
            {
                // float healthSum = _plantsInRange.Sum(p => p.Health);
                // _performanceMetrics.PlantHealthIndex = healthSum / _plantsInRange.Count;
            }
        }
        
        #endregion
        
        #region Public Interface
        
        public void SetAutomationMode(AutomationMode mode)
        {
            _automationMode = mode;
            Debug.Log($"Light automation mode set to: {mode}");
        }
        
        public void SetLightSchedule(ExtendedLightSchedule schedule)
        {
            _activeSchedule = schedule;
            Debug.Log($"Light schedule updated: {schedule.ScheduleName}");
        }
        
        public void EnableAdaptiveControl(bool enabled)
        {
            _adaptiveControl = enabled;
        }
        
        public LightStatusInfo GetStatusInfo()
        {
            return new LightStatusInfo
            {
                LightId = gameObject.name,
                LightType = _lightType,
                IsOn = _isOn,
                CurrentIntensity = _currentIntensity,
                TargetIntensity = _targetIntensity,
                PowerConsumption = _currentPowerConsumption,
                OperatingTemperature = _operatingTemperature,
                CurrentSpectrum = _currentSpectrum,
                // PlantsInRange = _plantsInRange.Count,
                AutomationMode = _automationMode,
                ThermalThrottling = _thermalThrottling,
                PerformanceMetrics = _performanceMetrics
            };
        }
        
        public List<LightDataPoint> GetPerformanceHistory(int hours = 24)
        {
            int dataPoints = Mathf.Min(hours * 60, _performanceHistory.Count);
            return _performanceHistory.TakeLast(dataPoints).ToList();
        }
        
        #endregion
        
        private void OnDestroy()
        {
            StopAllCoroutines();
            CancelInvoke();
        }
        
        #region Debug Visualization
        
        private void OnDrawGizmosSelected()
        {
            // Draw coverage area
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position;
            Vector3 size = new Vector3(_coverageArea.x, 0.1f, _coverageArea.y);
            Gizmos.DrawWireCube(center, size);
            
            // Draw light cone for spot lights
            if (_primaryLight != null && _primaryLight.type == UnityEngine.LightType.Spot)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                float range = _primaryLight.range;
                float angle = _primaryLight.spotAngle;
                
                Vector3 forward = transform.forward * range;
                Vector3 right = transform.right * Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * range;
                Vector3 up = transform.up * Mathf.Tan(angle * 0.5f * Mathf.Deg2Rad) * range;
                
                Vector3 farCenter = center + forward;
                Vector3 farTopLeft = farCenter - right + up;
                Vector3 farTopRight = farCenter + right + up;
                Vector3 farBottomLeft = farCenter - right - up;
                Vector3 farBottomRight = farCenter + right - up;
                
                Gizmos.DrawLine(center, farTopLeft);
                Gizmos.DrawLine(center, farTopRight);
                Gizmos.DrawLine(center, farBottomLeft);
                Gizmos.DrawLine(center, farBottomRight);
                
                Gizmos.DrawLine(farTopLeft, farTopRight);
                Gizmos.DrawLine(farTopRight, farBottomRight);
                Gizmos.DrawLine(farBottomRight, farBottomLeft);
                Gizmos.DrawLine(farBottomLeft, farTopLeft);
            }
            
            // Draw plant positions
            // Gizmos.color = Color.green;
            // foreach (var plant in _plantsInRange)
            // {
            //     if (plant != null)
            //     {
            //         Gizmos.DrawWireSphere(plant.transform.position, 0.5f);
            //         Gizmos.DrawLine(transform.position, plant.transform.position);
            //     }
            // }
        }
        
        #endregion
    }
    
    // Removed duplicate LightSpectrumData struct - using ProjectChimera.Data.Environment.LightSpectrumData instead
    
    [System.Serializable]
    public class LightPerformanceMetrics
    {
        public string LightId;
        public GrowLightType LightType;
        public float MaxIntensity;
        public float MaxPowerConsumption;
        public float AverageIntensity;
        public float AveragePowerConsumption;
        public float AverageTemperature;
        public float TotalOperatingHours;
        public float Efficiency;
        public float PlantHealthIndex;
        public DateTime LastUpdate;
    }
    
    [System.Serializable]
    public class LightDataPoint
    {
        public DateTime Timestamp;
        public float Intensity;
        public float PowerConsumption;
        public float OperatingTemperature;
        public LightSpectrumData Spectrum;
    }
    
    [System.Serializable]
    public class LightStatusInfo
    {
        public string LightId;
        public GrowLightType LightType;
        public bool IsOn;
        public float CurrentIntensity;
        public float TargetIntensity;
        public float PowerConsumption;
        public float OperatingTemperature;
        public LightSpectrumData CurrentSpectrum;
        public AutomationMode AutomationMode;
        public bool ThermalThrottling;
        public LightPerformanceMetrics PerformanceMetrics;
    }
    
    // EnvironmentalConditions class removed - using the one from ProjectChimera.Data.Environment namespace
    
    // Extended LightSchedule for Environment assembly with additional properties
    [System.Serializable]
    public class ExtendedLightSchedule : LightSchedule
    {
        public string ScheduleName = "Default Schedule";
        public int LightPeriodHours = 18;
        public int DarkPeriodHours = 6;
        public TimeSpan LightStartTime = TimeSpan.FromHours(6);
        public int IntensityRampTime = 30;
        public bool IsActive = true;
        public float MaxIntensity = 800f;
    }
    
    public enum GrowLightType
    {
        LED,
        HPS,
        CMH,
        Fluorescent,
        FullSpectrum,
        UV,
        InfraRed
    }
    
    public enum AutomationMode
    {
        Manual,
        Schedule,
        Adaptive,
        PlantOptimized
    }
    
    public enum GrowthStageSpectrum
    {
        Seedling,
        Vegetative,
        Flowering,
        Harvest
    }
    
    [System.Serializable]
    public class LightAlert
    {
        public LightAlertType AlertType;
        public ProjectChimera.Data.Automation.AlertSeverity Severity;
        public string Message;
        public string LightId;
        public DateTime Timestamp;
    }
    
    public enum LightAlertType
    {
        Thermal,
        Power,
        Spectrum,
        Efficiency,
        Maintenance
    }
}