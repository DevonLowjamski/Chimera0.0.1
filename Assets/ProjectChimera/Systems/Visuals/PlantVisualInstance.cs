using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Visuals;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using PlantGrowthStage = ProjectChimera.Data.Genetics.PlantGrowthStage;

namespace ProjectChimera.Systems.Visuals
{
    /// <summary>
    /// Individual plant visual instance that manages a SpeedTree asset's runtime parameters,
    /// LOD state, and visual response to cultivation conditions.
    /// </summary>
    public class PlantVisualInstance : ChimeraMonoBehaviour
    {
        [Header("Instance Data")]
        [SerializeField] private string _instanceId;
        [SerializeField] private PlantStrainSO _strain;
        [SerializeField] private SpeedTreePlantAssetSO _speedTreeAsset;
        [SerializeField] private PlantGrowthStage _currentGrowthStage = PlantGrowthStage.Seedling;

        [Header("Current State")]
        [SerializeField] private CultivationConditions _currentConditions;
        [SerializeField] private SpeedTreeParameters _currentParameters;
        [SerializeField] private int _currentLODLevel = 0;

        [Header("Visual Components")]
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private LODGroup _lodGroup;
        [SerializeField] private WindZone _windZone;

        [Header("Runtime Settings")]
        [SerializeField] private bool _enableRealTimeUpdates = true;
        [SerializeField] private bool _enableSeasonalTransitions = true;
        [SerializeField, Range(0.1f, 5f)] private float _parameterUpdateSpeed = 1f;

        // Private fields
        private Camera _observerCamera;
        private MaterialPropertyBlock _materialPropertyBlock;
        private float _lastUpdateTime;
        private bool _isInitialized = false;
        
        // Cached components
        private Transform _cachedTransform;
        private Bounds _cachedBounds;

        // Animation state
        private float _targetSeasonalParameter = 0f;
        private float _currentSeasonalParameter = 0f;
        private Vector3 _targetScale = Vector3.one;
        private Vector3 _currentScale = Vector3.one;
        private Color _targetTintColor = Color.white;
        private Color _currentTintColor = Color.white;

        // Events
        public System.Action<PlantVisualInstance, PlantGrowthStage> OnGrowthStageChanged;
        public System.Action<PlantVisualInstance, int> OnLODChanged;

        // Public Properties
        public string InstanceId => _instanceId;
        public PlantStrainSO Strain => _strain;
        public SpeedTreePlantAssetSO SpeedTreeAsset => _speedTreeAsset;
        public PlantGrowthStage CurrentGrowthStage => _currentGrowthStage;
        public CultivationConditions CurrentConditions => _currentConditions;
        public SpeedTreeParameters CurrentParameters => _currentParameters;
        public int CurrentLODLevel => _currentLODLevel;
        public bool IsInitialized => _isInitialized;

        public void Initialize(string instanceId, PlantStrainSO strain, SpeedTreePlantAssetSO speedTreeAsset, Camera observerCamera)
        {
            _instanceId = instanceId;
            _strain = strain;
            _speedTreeAsset = speedTreeAsset;
            _observerCamera = observerCamera;

            // Cache transform and initialize components
            _cachedTransform = transform;
            _materialPropertyBlock = new MaterialPropertyBlock();
            
            // Find renderers and LOD group
            InitializeComponents();
            
            // Initialize visual parameters
            InitializeVisualParameters();
            
            _isInitialized = true;
            _lastUpdateTime = Time.time;

            Debug.Log($"[Chimera] PlantVisualInstance '{_instanceId}' initialized for strain '{_strain.StrainName}'.");
        }

        private void Update()
        {
            if (!_isInitialized || !_enableRealTimeUpdates) return;

            UpdateAnimatedParameters();
        }

        /// <summary>
        /// Updates cultivation conditions and triggers visual parameter recalculation.
        /// </summary>
        public void UpdateCultivationConditions(CultivationConditions conditions)
        {
            _currentConditions = conditions;
            RecalculateVisualParameters();
        }

        /// <summary>
        /// Updates the plant's growth stage and associated visual changes.
        /// </summary>
        public void UpdateGrowthStage(PlantGrowthStage newStage)
        {
            if (_currentGrowthStage != newStage)
            {
                PlantGrowthStage previousStage = _currentGrowthStage;
                _currentGrowthStage = newStage;
                
                OnGrowthStageChanged?.Invoke(this, newStage);
                
                // Apply growth stage specific changes
                ApplyGrowthStageChanges(previousStage, newStage);
                
                Debug.Log($"[Chimera] Plant '{_instanceId}' growth stage changed from {previousStage} to {newStage}.");
            }
        }

        /// <summary>
        /// Updates visual parameters based on current conditions.
        /// </summary>
        public void UpdateVisualParameters()
        {
            if (!_isInitialized) return;

            RecalculateVisualParameters();
        }

        /// <summary>
        /// Updates LOD level based on distance from observer camera.
        /// </summary>
        public void UpdateLOD(float distanceFromCamera)
        {
            if (_speedTreeAsset?.LODConfig == null) return;

            int newLODLevel = CalculateLODLevel(distanceFromCamera);
            
            if (newLODLevel != _currentLODLevel)
            {
                SetLODLevel(newLODLevel);
            }
        }

        /// <summary>
        /// Forces a specific LOD level (for performance optimization).
        /// </summary>
        public void ForceLODLevel(int lodLevel)
        {
            SetLODLevel(lodLevel);
        }

        /// <summary>
        /// Destroys this plant instance and cleans up resources.
        /// </summary>
        public void Destroy()
        {
            if (gameObject != null)
            {
                DestroyImmediate(gameObject);
            }
        }

        private void InitializeComponents()
        {
            // Find all renderers
            _renderers = GetComponentsInChildren<Renderer>();
            
            // Find LOD group
            _lodGroup = GetComponent<LODGroup>();
            if (_lodGroup == null)
            {
                _lodGroup = GetComponentInChildren<LODGroup>();
            }

            // Find wind zone
            _windZone = GetComponentInChildren<WindZone>();

            // Calculate bounds
            _cachedBounds = CalculateCombinedBounds();
        }

        private void InitializeVisualParameters()
        {
            if (_speedTreeAsset?.ParameterMapper == null) return;

            // Set initial cultivation conditions if not provided
            if (_currentConditions.Temperature == 0f)
            {
                _currentConditions = GetDefaultCultivationConditions();
            }

            // Calculate initial parameters
            RecalculateVisualParameters();
        }

        private void RecalculateVisualParameters()
        {
            if (_speedTreeAsset?.ParameterMapper == null) return;

            // Calculate new parameters using the parameter mapper
            _currentParameters = _speedTreeAsset.ParameterMapper.CalculateSpeedTreeParameters(_currentConditions, _strain);

            // Set animation targets
            _targetSeasonalParameter = _currentParameters.SeasonalParameter;
            _targetScale = _currentParameters.ScaleModifier;
            _targetTintColor = _currentParameters.TintColor;

            // Apply immediate changes for non-animated parameters
            ApplyImmediateParameters();
        }

        private void UpdateAnimatedParameters()
        {
            float deltaTime = Time.deltaTime;
            float updateSpeed = _parameterUpdateSpeed;

            // Animate seasonal parameter
            if (_enableSeasonalTransitions)
            {
                _currentSeasonalParameter = Mathf.Lerp(_currentSeasonalParameter, _targetSeasonalParameter, updateSpeed * deltaTime);
                ApplySeasonalParameter(_currentSeasonalParameter);
            }

            // Animate scale
            _currentScale = Vector3.Lerp(_currentScale, _targetScale, updateSpeed * deltaTime);
            _cachedTransform.localScale = _currentScale;

            // Animate tint color
            _currentTintColor = Color.Lerp(_currentTintColor, _targetTintColor, updateSpeed * deltaTime);
            ApplyTintColor(_currentTintColor);
        }

        private void ApplyImmediateParameters()
        {
            // Apply foliage and branch density (typically handled by SpeedTree shaders)
            SetShaderParameter("_FoliageDensity", _currentParameters.FoliageDensity);
            SetShaderParameter("_BranchDensity", _currentParameters.BranchDensity);
            SetShaderParameter("_LeafSize", _currentParameters.LeafSize);
            SetShaderParameter("_TrunkThickness", _currentParameters.TrunkThickness);
            SetShaderParameter("_ColorVariation", _currentParameters.ColorVariation);
        }

        private void ApplySeasonalParameter(float seasonalValue)
        {
            // Apply seasonal parameter to SpeedTree shader
            SetShaderParameter("_SeasonalParameter", seasonalValue);
            SetShaderParameter("_Season", seasonalValue);
        }

        private void ApplyTintColor(Color tintColor)
        {
            SetShaderParameter("_TintColor", tintColor);
        }

        private void SetShaderParameter(string parameterName, float value)
        {
            if (_renderers == null) return;

            foreach (var renderer in _renderers)
            {
                if (renderer != null)
                {
                    renderer.GetPropertyBlock(_materialPropertyBlock);
                    _materialPropertyBlock.SetFloat(parameterName, value);
                    renderer.SetPropertyBlock(_materialPropertyBlock);
                }
            }
        }

        private void SetShaderParameter(string parameterName, Color value)
        {
            if (_renderers == null) return;

            foreach (var renderer in _renderers)
            {
                if (renderer != null)
                {
                    renderer.GetPropertyBlock(_materialPropertyBlock);
                    _materialPropertyBlock.SetColor(parameterName, value);
                    renderer.SetPropertyBlock(_materialPropertyBlock);
                }
            }
        }

        private void ApplyGrowthStageChanges(PlantGrowthStage previousStage, PlantGrowthStage newStage)
        {
            // Get growth stage specific asset if available
            var stageAsset = _speedTreeAsset.GetAssetForGrowthStage(newStage);
            if (stageAsset != null)
            {
                // Apply stage-specific materials
                ApplyGrowthStageMaterials(stageAsset);
                
                // Apply stage-specific scale
                if (stageAsset.ScaleMultiplier != Vector3.one)
                {
                    _targetScale = Vector3.Scale(_targetScale, stageAsset.ScaleMultiplier);
                }

                // Apply seasonal parameter override if specified
                if (stageAsset.HasOverride)
                {
                    _targetSeasonalParameter = stageAsset.SeasonalParameterOverride;
                }
            }

            // Calculate new seasonal parameter for the stage
            float newSeasonalParameter = _speedTreeAsset.CalculateSeasonalParameter(newStage);
            _targetSeasonalParameter = newSeasonalParameter;
        }

        private void ApplyGrowthStageMaterials(GrowthStageAsset stageAsset)
        {
            if (stageAsset.StageMaterials == null || stageAsset.StageMaterials.Length == 0) return;

            for (int i = 0; i < _renderers.Length && i < stageAsset.StageMaterials.Length; i++)
            {
                if (_renderers[i] != null && stageAsset.StageMaterials[i] != null)
                {
                    _renderers[i].material = stageAsset.StageMaterials[i];
                }
            }
        }

        private int CalculateLODLevel(float distance)
        {
            var lodConfig = _speedTreeAsset.LODConfig;
            
            if (distance <= lodConfig.HighDetailDistance)
                return 0; // High detail
            else if (distance <= lodConfig.MediumDetailDistance)
                return 1; // Medium detail
            else if (distance <= lodConfig.LowDetailDistance)
                return 2; // Low detail
            else
                return 3; // Culled/Billboard
        }

        private void SetLODLevel(int lodLevel)
        {
            _currentLODLevel = lodLevel;
            
            // Apply LOD to Unity's LODGroup if available
            if (_lodGroup != null)
            {
                _lodGroup.ForceLOD(lodLevel);
            }

            // Apply SpeedTree specific LOD quality
            ApplyLODQuality(lodLevel);
            
            OnLODChanged?.Invoke(this, lodLevel);
        }

        private void ApplyLODQuality(int lodLevel)
        {
            var lodConfig = _speedTreeAsset.LODConfig;
            
            float quality = lodLevel switch
            {
                0 => lodConfig.HighDetailQuality,
                1 => lodConfig.MediumDetailQuality,
                2 => lodConfig.LowDetailQuality,
                _ => 0f
            };

            SetShaderParameter("_LODQuality", quality);

            // Manage wind based on LOD settings
            if (_windZone != null)
            {
                bool enableWind = lodConfig.EnableWindOnAllLODs || (lodConfig.WindOnHighLODOnly && lodLevel == 0);
                _windZone.gameObject.SetActive(enableWind);
            }
        }

        private Bounds CalculateCombinedBounds()
        {
            if (_renderers == null || _renderers.Length == 0)
                return new Bounds(transform.position, Vector3.one);

            Bounds combinedBounds = _renderers[0].bounds;
            for (int i = 1; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    combinedBounds.Encapsulate(_renderers[i].bounds);
                }
            }

            return combinedBounds;
        }

        private CultivationConditions GetDefaultCultivationConditions()
        {
            if (_strain?.BaseSpecies != null)
            {
                var optimal = _strain.BaseSpecies.GetOptimalEnvironment();
                return new CultivationConditions
                {
                    Temperature = optimal.Temperature,
                    Humidity = optimal.Humidity,
                    LightIntensity = optimal.LightIntensity,
                    CO2Level = optimal.CO2Level,
                    NitrogenLevel = 0.8f,
                    PhosphorusLevel = 0.8f,
                    PotassiumLevel = 0.8f,
                    GrowthStageProgress = 0f,
                    WaterLevel = 0.8f,
                    pH = 6.5f
                };
            }

            return new CultivationConditions
            {
                Temperature = 24f,
                Humidity = 55f,
                LightIntensity = 400f,
                CO2Level = 800f,
                NitrogenLevel = 0.8f,
                PhosphorusLevel = 0.8f,
                PotassiumLevel = 0.8f,
                GrowthStageProgress = 0f,
                WaterLevel = 0.8f,
                pH = 6.5f
            };
        }

        private void OnDrawGizmosSelected()
        {
            if (_isInitialized)
            {
                // Draw bounds
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(_cachedBounds.center, _cachedBounds.size);
                
                // Draw LOD distance indicators
                if (_speedTreeAsset?.LODConfig != null && _observerCamera != null)
                {
                    Vector3 position = transform.position;
                    var lodConfig = _speedTreeAsset.LODConfig;
                    
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(position, lodConfig.HighDetailDistance);
                    
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(position, lodConfig.MediumDetailDistance);
                    
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(position, lodConfig.LowDetailDistance);
                }
            }
        }
    }
}