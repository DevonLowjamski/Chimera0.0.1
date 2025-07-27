using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.SpeedTree;
using ProjectChimera.Systems.Cultivation;
using System.Collections.Generic;
using System.Linq;

#if UNITY_SPEEDTREE
using SpeedTree;
#endif

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// PC013-8a: SpeedTreeRenderingService extracted from AdvancedSpeedTreeManager
    /// Handles SpeedTree plant instance creation, rendering configuration, and genetic variations.
    /// Manages the visual representation of cannabis plants using SpeedTree technology.
    /// </summary>
    public class SpeedTreeRenderingService
    {
        private readonly SpeedTreeLibrarySO _speedTreeLibrary;
        private readonly SpeedTreeShaderConfigSO _shaderConfig;
        private readonly SpeedTreeLODConfigSO _lodConfig;
        private readonly SpeedTreeGeneticsConfigSO _geneticsConfig;
        
        private Dictionary<string, SpeedTreeAsset> _loadedAssets;
        private Dictionary<int, SpeedTreePlantData> _plantInstances;
        private List<CannabisStrainAssetSO> _cannabisStrains;
        
        // Events for rendering operations
        public System.Action<SpeedTreePlantData> OnPlantInstanceCreated;
        public System.Action<int> OnPlantInstanceDestroyed;
        public System.Action<SpeedTreePlantData, CannabisGeneticData> OnGeneticVariationsApplied;
        
        public SpeedTreeRenderingService(SpeedTreeLibrarySO library, SpeedTreeShaderConfigSO shaderConfig,
            SpeedTreeLODConfigSO lodConfig, SpeedTreeGeneticsConfigSO geneticsConfig,
            List<CannabisStrainAssetSO> cannabisStrains)
        {
            _speedTreeLibrary = library;
            _shaderConfig = shaderConfig;
            _lodConfig = lodConfig;
            _geneticsConfig = geneticsConfig;
            _cannabisStrains = cannabisStrains ?? new List<CannabisStrainAssetSO>();
            
            _loadedAssets = new Dictionary<string, SpeedTreeAsset>();
            _plantInstances = new Dictionary<int, SpeedTreePlantData>();
            
            InitializeAssetLibrary();
            
            Debug.Log("[SpeedTreeRenderingService] Initialized with SpeedTree rendering capabilities");
        }
        
        /// <summary>
        /// Creates a new SpeedTree plant instance with genetic variations.
        /// </summary>
        public SpeedTreePlantData CreatePlantInstance(InteractivePlantComponent plantComponent, string strainId = "")
        {
            if (plantComponent == null)
            {
                Debug.LogError("[SpeedTreeRenderingService] Cannot create plant instance: plantComponent is null");
                return null;
            }
            
            if (!IsSpeedTreeAvailable())
            {
                Debug.LogWarning("[SpeedTreeRenderingService] SpeedTree not available, using fallback");
                return CreateFallbackPlantInstance(plantComponent);
            }
            
            var instance = new SpeedTreePlantData
            {
                InstanceId = plantComponent.GetInstanceID(),
                PlantComponent = plantComponent,
                StrainId = !string.IsNullOrEmpty(strainId) ? strainId : GetDefaultStrainId(),
                CreationTime = Time.time,
                GeneticData = GenerateGeneticData(strainId),
                CurrentGrowthStage = PlantGrowthStage.Seedling,
                GrowthProgress = 0f,
                Health = 1f
            };
            
            try
            {
                // Create SpeedTree renderer
                var renderer = CreateSpeedTreeRenderer(instance);
                if (renderer != null)
                {
                    instance.Renderer = renderer;
                    
                    // Apply genetic variations
                    ApplyGeneticVariationsToRenderer(renderer, instance.GeneticData);
                    
                    // Configure cannabis-specific properties
                    ConfigureRendererForCannabis(renderer, instance);
                    
                    // Add physics interaction
                    AddPhysicsInteraction(renderer, instance);
                    
                    // Store instance
                    _plantInstances[instance.InstanceId] = instance;
                    
                    OnPlantInstanceCreated?.Invoke(instance);
                    
                    Debug.Log($"[SpeedTreeRenderingService] Created SpeedTree plant instance: {instance.InstanceId}");
                    return instance;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SpeedTreeRenderingService] Failed to create plant instance: {ex.Message}");
            }
            
            return CreateFallbackPlantInstance(plantComponent);
        }
        
        /// <summary>
        /// Destroys a SpeedTree plant instance and cleans up resources.
        /// </summary>
        public bool DestroyPlantInstance(int instanceId)
        {
            if (!_plantInstances.TryGetValue(instanceId, out var instance))
            {
                Debug.LogWarning($"[SpeedTreeRenderingService] Plant instance not found: {instanceId}");
                return false;
            }
            
            try
            {
                // Cleanup renderer
                if (instance.Renderer != null)
                {
                    Object.DestroyImmediate(instance.Renderer.gameObject);
                }
                
                // Remove from tracking
                _plantInstances.Remove(instanceId);
                
                OnPlantInstanceDestroyed?.Invoke(instanceId);
                
                Debug.Log($"[SpeedTreeRenderingService] Destroyed plant instance: {instanceId}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SpeedTreeRenderingService] Failed to destroy plant instance {instanceId}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Updates genetic variations for an existing plant instance.
        /// </summary>
        public void UpdateGeneticVariations(int instanceId, CannabisGeneticData newGeneticData)
        {
            if (!_plantInstances.TryGetValue(instanceId, out var instance))
            {
                Debug.LogWarning($"[SpeedTreeRenderingService] Plant instance not found: {instanceId}");
                return;
            }
            
            if (instance.Renderer == null)
            {
                Debug.LogWarning($"[SpeedTreeRenderingService] No renderer for plant instance: {instanceId}");
                return;
            }
            
            instance.GeneticData = newGeneticData;
            ApplyGeneticVariationsToRenderer(instance.Renderer, newGeneticData);
            
            OnGeneticVariationsApplied?.Invoke(instance, newGeneticData);
            
            Debug.Log($"[SpeedTreeRenderingService] Updated genetic variations for instance: {instanceId}");
        }
        
        /// <summary>
        /// Gets a plant instance by ID.
        /// </summary>
        public SpeedTreePlantData GetPlantInstance(int instanceId)
        {
            return _plantInstances.TryGetValue(instanceId, out var instance) ? instance : null;
        }
        
        /// <summary>
        /// Gets all active plant instances.
        /// </summary>
        public IReadOnlyDictionary<int, SpeedTreePlantData> GetAllPlantInstances()
        {
            return _plantInstances;
        }
        
        /// <summary>
        /// Gets rendering statistics.
        /// </summary>
        public SpeedTreeRenderingStats GetRenderingStats()
        {
            return new SpeedTreeRenderingStats
            {
                ActiveInstances = _plantInstances.Count,
                LoadedAssets = _loadedAssets.Count,
                AvailableStrains = _cannabisStrains.Count,
                SpeedTreeEnabled = IsSpeedTreeAvailable(),
                MemoryUsage = CalculateMemoryUsage()
            };
        }
        
        // Private methods
        private void InitializeAssetLibrary()
        {
            if (_speedTreeLibrary == null)
            {
                Debug.LogError("[SpeedTreeRenderingService] SpeedTree library is null");
                return;
            }
            
            LoadSpeedTreeAssets();
            InitializeStrainDatabase();
        }
        
        private void LoadSpeedTreeAssets()
        {
            if (_speedTreeLibrary?.AssetPaths == null) return;
            
            foreach (var assetPath in _speedTreeLibrary.AssetPaths)
            {
                try
                {
                    var asset = LoadSpeedTreeAsset(assetPath);
                    if (asset != null)
                    {
                        _loadedAssets[assetPath] = asset;
                        ConfigureAssetForCannabis(asset);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[SpeedTreeRenderingService] Failed to load asset {assetPath}: {ex.Message}");
                }
            }
            
            Debug.Log($"[SpeedTreeRenderingService] Loaded {_loadedAssets.Count} SpeedTree assets");
        }
        
        private SpeedTreeAsset LoadSpeedTreeAsset(string assetPath)
        {
            #if UNITY_SPEEDTREE
            return Resources.Load<SpeedTreeAsset>(assetPath);
            #else
            Debug.LogWarning($"[SpeedTreeRenderingService] SpeedTree not available, cannot load asset: {assetPath}");
            return null;
            #endif
        }
        
        private void ConfigureAssetForCannabis(SpeedTreeAsset asset)
        {
            if (asset == null) return;
            
            #if UNITY_SPEEDTREE
            // Configure asset for cannabis-specific properties
            // This would include setting up materials, shaders, and parameters
            // specific to cannabis plant rendering
            #endif
        }
        
        private void InitializeStrainDatabase()
        {
            foreach (var strain in _cannabisStrains)
            {
                if (strain != null)
                {
                    Debug.Log($"[SpeedTreeRenderingService] Registered cannabis strain: {strain.StrainName}");
                }
            }
        }
        
        private bool IsSpeedTreeAvailable()
        {
            #if UNITY_SPEEDTREE
            return true;
            #else
            return false;
            #endif
        }
        
        private SpeedTreeRenderer CreateSpeedTreeRenderer(SpeedTreePlantData instance)
        {
            #if UNITY_SPEEDTREE
            var gameObject = new GameObject($"SpeedTree_Plant_{instance.InstanceId}");
            
            // Position at plant component location
            if (instance.PlantComponent != null)
            {
                gameObject.transform.position = instance.PlantComponent.transform.position;
                gameObject.transform.rotation = instance.PlantComponent.transform.rotation;
                gameObject.transform.SetParent(instance.PlantComponent.transform);
            }
            
            var renderer = gameObject.AddComponent<SpeedTreeRenderer>();
            
            // Configure renderer with appropriate asset
            var asset = GetAssetForStrain(instance.StrainId);
            if (asset != null)
            {
                renderer.speedTreeAsset = asset;
            }
            
            return renderer;
            #else
            Debug.LogWarning("[SpeedTreeRenderingService] SpeedTree not available, cannot create renderer");
            return null;
            #endif
        }
        
        private void ApplyGeneticVariationsToRenderer(SpeedTreeRenderer renderer, CannabisGeneticData genetics)
        {
            if (renderer == null || genetics == null) return;
            
            #if UNITY_SPEEDTREE
            // Apply morphological variations
            ApplyMorphologicalVariations(renderer, genetics);
            
            // Apply color variations
            ApplyColorVariations(renderer, genetics);
            
            // Apply growth characteristics
            ApplyGrowthCharacteristics(renderer, genetics);
            #endif
        }
        
        private void ApplyMorphologicalVariations(SpeedTreeRenderer renderer, CannabisGeneticData genetics)
        {
            #if UNITY_SPEEDTREE
            if (renderer.speedTreeAsset == null) return;
            
            // Plant height variation
            var heightMultiplier = genetics.HeightGenes?.ExpressedValue ?? 1f;
            renderer.transform.localScale = new Vector3(1f, heightMultiplier, 1f);
            
            // Branch density variation
            var branchDensity = genetics.BranchingGenes?.ExpressedValue ?? 1f;
            // Apply branching modifications through SpeedTree properties
            
            // Leaf size variation
            var leafSize = genetics.LeafSizeGenes?.ExpressedValue ?? 1f;
            // Apply leaf size modifications
            #endif
        }
        
        private void ApplyColorVariations(SpeedTreeRenderer renderer, CannabisGeneticData genetics)
        {
            #if UNITY_SPEEDTREE
            // Apply color genetics to materials
            if (renderer.GetComponent<Renderer>()?.materials != null)
            {
                foreach (var material in renderer.GetComponent<Renderer>().materials)
                {
                    // Apply color variations based on genetics
                    var colorVariation = genetics.ColorGenes?.ExpressedValue ?? 0f;
                    // Modify material properties for color expression
                }
            }
            #endif
        }
        
        private void ApplyGrowthCharacteristics(SpeedTreeRenderer renderer, CannabisGeneticData genetics)
        {
            #if UNITY_SPEEDTREE
            // Configure growth-related parameters
            var growthRate = genetics.GrowthRateGenes?.ExpressedValue ?? 1f;
            // Store growth rate for animation system
            #endif
        }
        
        private void ConfigureRendererForCannabis(SpeedTreeRenderer renderer, SpeedTreePlantData instance)
        {
            if (renderer == null) return;
            
            #if UNITY_SPEEDTREE
            // Configure LOD settings
            ConfigureLODSettings(renderer);
            
            // Configure wind response
            ConfigureWindResponse(renderer);
            
            // Configure shader properties
            ConfigureShaderProperties(renderer, instance);
            #endif
        }
        
        private void ConfigureLODSettings(SpeedTreeRenderer renderer)
        {
            #if UNITY_SPEEDTREE
            if (_lodConfig != null)
            {
                // Apply LOD configuration
                renderer.enableCrossFade = _lodConfig.EnableCrossFade;
                renderer.fadeOutLength = _lodConfig.FadeOutLength;
                renderer.animateOnCulling = _lodConfig.AnimateOnCulling;
            }
            #endif
        }
        
        private void ConfigureWindResponse(SpeedTreeRenderer renderer)
        {
            #if UNITY_SPEEDTREE
            // Configure wind response parameters
            var windComponent = renderer.gameObject.AddComponent<SpeedTreeWind>();
            if (windComponent != null)
            {
                // Configure wind parameters for cannabis plants
            }
            #endif
        }
        
        private void ConfigureShaderProperties(SpeedTreeRenderer renderer, SpeedTreePlantData instance)
        {
            #if UNITY_SPEEDTREE
            if (_shaderConfig == null) return;
            
            var rendererComponent = renderer.GetComponent<Renderer>();
            if (rendererComponent?.materials != null)
            {
                foreach (var material in rendererComponent.materials)
                {
                    // Apply shader configuration
                    ApplyShaderConfig(material, instance);
                }
            }
            #endif
        }
        
        private void ApplyShaderConfig(Material material, SpeedTreePlantData instance)
        {
            if (material == null || _shaderConfig == null) return;
            
            // Apply cannabis-specific shader properties
            material.SetFloat("_HealthLevel", instance.Health);
            material.SetFloat("_GrowthProgress", instance.GrowthProgress);
            material.SetFloat("_StressLevel", instance.StressLevel);
        }
        
        private void AddPhysicsInteraction(SpeedTreeRenderer renderer, SpeedTreePlantData instance)
        {
            if (renderer == null) return;
            
            // Add physics components for interaction
            var collider = renderer.gameObject.AddComponent<CapsuleCollider>();
            if (collider != null)
            {
                collider.isTrigger = true;
                collider.radius = 0.5f;
                collider.height = 2f;
            }
            
            // Add rigidbody for physics simulation
            var rigidbody = renderer.gameObject.AddComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
            }
        }
        
        private SpeedTreePlantData CreateFallbackPlantInstance(InteractivePlantComponent plantComponent)
        {
            // Create basic plant data without SpeedTree renderer
            var instance = new SpeedTreePlantData
            {
                InstanceId = plantComponent.GetInstanceID(),
                PlantComponent = plantComponent,
                StrainId = GetDefaultStrainId(),
                CreationTime = Time.time,
                GeneticData = GenerateGeneticData(""),
                CurrentGrowthStage = PlantGrowthStage.Seedling,
                GrowthProgress = 0f,
                Health = 1f,
                Renderer = null // No SpeedTree renderer
            };
            
            _plantInstances[instance.InstanceId] = instance;
            OnPlantInstanceCreated?.Invoke(instance);
            
            Debug.Log($"[SpeedTreeRenderingService] Created fallback plant instance: {instance.InstanceId}");
            return instance;
        }
        
        private CannabisGeneticData GenerateGeneticData(string strainId)
        {
            var strain = _cannabisStrains.FirstOrDefault(s => s.StrainId == strainId);
            
            return new CannabisGeneticData
            {
                StrainId = strainId,
                HeightGenes = new GeneExpression { ExpressedValue = UnityEngine.Random.Range(0.8f, 1.2f) },
                BranchingGenes = new GeneExpression { ExpressedValue = UnityEngine.Random.Range(0.7f, 1.3f) },
                LeafSizeGenes = new GeneExpression { ExpressedValue = UnityEngine.Random.Range(0.9f, 1.1f) },
                ColorGenes = new GeneExpression { ExpressedValue = UnityEngine.Random.Range(0f, 1f) },
                GrowthRateGenes = new GeneExpression { ExpressedValue = UnityEngine.Random.Range(0.8f, 1.2f) }
            };
        }
        
        private string GetDefaultStrainId()
        {
            return _cannabisStrains.FirstOrDefault()?.StrainId ?? "default_indica";
        }
        
        private SpeedTreeAsset GetAssetForStrain(string strainId)
        {
            // Return appropriate asset based on strain
            return _loadedAssets.Values.FirstOrDefault();
        }
        
        private float CalculateMemoryUsage()
        {
            // Calculate approximate memory usage
            float totalMemory = 0f;
            
            foreach (var instance in _plantInstances.Values)
            {
                if (instance.Renderer != null)
                {
                    totalMemory += EstimateRendererMemoryUsage(instance.Renderer);
                }
            }
            
            return totalMemory;
        }
        
        private float EstimateRendererMemoryUsage(SpeedTreeRenderer renderer)
        {
            // Rough estimate of memory usage per renderer
            return 2f; // MB
        }
    }
    
    /// <summary>
    /// Rendering statistics for SpeedTree system.
    /// </summary>
    [System.Serializable]
    public class SpeedTreeRenderingStats
    {
        public int ActiveInstances;
        public int LoadedAssets;
        public int AvailableStrains;
        public bool SpeedTreeEnabled;
        public float MemoryUsage;
    }
    
    /// <summary>
    /// Gene expression data for cannabis genetics.
    /// </summary>
    [System.Serializable]
    public class GeneExpression
    {
        public float ExpressedValue;
        public float DominanceLevel = 0.5f;
        public bool IsActive = true;
        
        // Alias for compatibility
        public float Value => ExpressedValue;
    }
}