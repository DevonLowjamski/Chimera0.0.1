using UnityEngine;
using ProjectChimera.Data.Genetics;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Systems.Prefabs
{
    /// <summary>
    /// Specialized prefab library for plant objects across all growth stages.
    /// Manages plant prefabs, strain variations, and growth stage transitions.
    /// </summary>
    [CreateAssetMenu(fileName = "New Plant Prefab Library", menuName = "Project Chimera/Prefabs/Plant Library")]
    public class PlantPrefabLibrary : ScriptableObject
    {
        [Header("Plant Prefab Configuration")]
        [SerializeField] private List<PlantPrefabEntry> _plantPrefabs = new List<PlantPrefabEntry>();
        [SerializeField] private List<StrainPrefabCollection> _strainCollections = new List<StrainPrefabCollection>();
        
        [Header("Growth Stage Prefabs")]
        [SerializeField] private List<GrowthStagePrefabSet> _growthStageSets = new List<GrowthStagePrefabSet>();
        [SerializeField] private bool _enableStageTransitions = true;
        [SerializeField] private float _transitionDuration = 2f;
        
        [Header("Variation Settings")]
        [SerializeField] private bool _enableRandomVariations = true;
        [SerializeField] private int _maxVariationsPerStrain = 5;
        [SerializeField] private Vector2 _sizeVariationRange = new Vector2(0.8f, 1.2f);
        [SerializeField] private Vector2 _colorVariationRange = new Vector2(0.9f, 1.1f);
        
        // Cached data for quick access
        private Dictionary<string, PlantPrefabEntry> _prefabLookup;
        private Dictionary<PlantGrowthStage, List<PlantPrefabEntry>> _stageLookup;
        private Dictionary<string, StrainPrefabCollection> _strainLookup;
        
        public List<PlantPrefabEntry> PlantPrefabs => _plantPrefabs;
        
        public void InitializeDefaults()
        {
            if (_plantPrefabs.Count == 0)
            {
                CreateDefaultPlantPrefabs();
            }
            
            BuildLookupTables();
        }
        
        private void CreateDefaultPlantPrefabs()
        {
            // Seed Stage Prefabs
            _plantPrefabs.Add(new PlantPrefabEntry
            {
                PrefabId = "cannabis_seed_01",
                PrefabName = "Cannabis Seed",
                Prefab = null, // Would be assigned in inspector
                GrowthStage = PlantGrowthStage.Seed,
                StrainId = "default",
                PrefabType = PlantPrefabType.Base,
                RequiredComponents = new List<string> { "SeedComponent", "PlantBase" },
                Tags = new List<string> { "seed", "germination" }
            });
            
            // Seedling Stage Prefabs
            _plantPrefabs.Add(new PlantPrefabEntry
            {
                PrefabId = "cannabis_seedling_01",
                PrefabName = "Cannabis Seedling",
                Prefab = null,
                GrowthStage = PlantGrowthStage.Seedling,
                StrainId = "default",
                PrefabType = PlantPrefabType.Base,
                RequiredComponents = new List<string> { "SeedlingComponent", "PlantBase", "GrowthController" },
                Tags = new List<string> { "seedling", "young_plant" }
            });
            
            // Vegetative Stage Prefabs
            _plantPrefabs.Add(new PlantPrefabEntry
            {
                PrefabId = "cannabis_vegetative_01",
                PrefabName = "Cannabis Vegetative",
                Prefab = null,
                GrowthStage = PlantGrowthStage.Vegetative,
                StrainId = "default",
                PrefabType = PlantPrefabType.Base,
                RequiredComponents = new List<string> { "VegetativeComponent", "PlantBase", "LeafSystem", "RootSystem" },
                Tags = new List<string> { "vegetative", "growing", "leaves" }
            });
            
            // Flowering Stage Prefabs
            _plantPrefabs.Add(new PlantPrefabEntry
            {
                PrefabId = "cannabis_flowering_01",
                PrefabName = "Cannabis Flowering",
                Prefab = null,
                GrowthStage = PlantGrowthStage.Flowering,
                StrainId = "default",
                PrefabType = PlantPrefabType.Base,
                RequiredComponents = new List<string> { "FloweringComponent", "PlantBase", "BudSystem", "TrichomeSystem" },
                Tags = new List<string> { "flowering", "buds", "trichomes" }
            });
            
            // Harvest Stage Prefabs
            _plantPrefabs.Add(new PlantPrefabEntry
            {
                PrefabId = "cannabis_harvest_01",
                PrefabName = "Cannabis Harvest Ready",
                Prefab = null,
                GrowthStage = PlantGrowthStage.Harvest,
                StrainId = "default",
                PrefabType = PlantPrefabType.Base,
                RequiredComponents = new List<string> { "HarvestComponent", "PlantBase", "QualityAnalyzer" },
                Tags = new List<string> { "harvest", "mature", "ready" }
            });
            
            CreateDefaultStrainCollections();
        }
        
        private void CreateDefaultStrainCollections()
        {
            // Create default strain collections for popular cannabis strains
            var indicaCollection = new StrainPrefabCollection
            {
                StrainId = "indica_classic",
                StrainName = "Classic Indica",
                StrainType = StrainType.Indica,
                PrefabEntries = new List<string>(),
                GrowthCharacteristics = new StrainGrowthProfile
                {
                    MaxHeight = 1.5f,
                    GrowthRate = 0.8f,
                    FloweringTime = 56f, // 8 weeks
                    YieldPotential = 0.9f,
                    ResistanceLevel = 0.7f
                },
                VisualCharacteristics = new StrainVisualProfile
                {
                    LeafColor = new Color(0.2f, 0.6f, 0.2f),
                    BudColor = new Color(0.4f, 0.3f, 0.6f),
                    TrichomeColor = Color.white,
                    PlantShape = PlantShape.Bushy,
                    LeafShape = LeafShape.Broad
                }
            };
            
            var sativaCollection = new StrainPrefabCollection
            {
                StrainId = "sativa_classic",
                StrainName = "Classic Sativa",
                StrainType = StrainType.Sativa,
                PrefabEntries = new List<string>(),
                GrowthCharacteristics = new StrainGrowthProfile
                {
                    MaxHeight = 3f,
                    GrowthRate = 1.2f,
                    FloweringTime = 84f, // 12 weeks
                    YieldPotential = 1.1f,
                    ResistanceLevel = 0.6f
                },
                VisualCharacteristics = new StrainVisualProfile
                {
                    LeafColor = new Color(0.3f, 0.7f, 0.3f),
                    BudColor = new Color(0.5f, 0.6f, 0.3f),
                    TrichomeColor = Color.white,
                    PlantShape = PlantShape.Tall,
                    LeafShape = LeafShape.Narrow
                }
            };
            
            _strainCollections.Add(indicaCollection);
            _strainCollections.Add(sativaCollection);
        }
        
        private void BuildLookupTables()
        {
            _prefabLookup = _plantPrefabs.ToDictionary(p => p.PrefabId, p => p);
            
            _stageLookup = _plantPrefabs.GroupBy(p => p.GrowthStage)
                                       .ToDictionary(g => g.Key, g => g.ToList());
            
            _strainLookup = _strainCollections.ToDictionary(s => s.StrainId, s => s);
        }
        
        public PlantPrefabEntry GetPrefabForStage(string strainId, PlantGrowthStage stage)
        {
            // First try to find strain-specific prefab
            var strainPrefabs = _plantPrefabs.Where(p => p.StrainId == strainId && p.GrowthStage == stage);
            if (strainPrefabs.Any())
            {
                return GetVariation(strainPrefabs.ToList());
            }
            
            // Fall back to default strain
            var defaultPrefabs = _plantPrefabs.Where(p => p.StrainId == "default" && p.GrowthStage == stage);
            if (defaultPrefabs.Any())
            {
                return GetVariation(defaultPrefabs.ToList());
            }
            
            return null;
        }
        
        private PlantPrefabEntry GetVariation(List<PlantPrefabEntry> availablePrefabs)
        {
            if (!_enableRandomVariations || availablePrefabs.Count <= 1)
            {
                return availablePrefabs[0];
            }
            
            // Select random variation
            int randomIndex = Random.Range(0, Mathf.Min(availablePrefabs.Count, _maxVariationsPerStrain));
            return availablePrefabs[randomIndex];
        }
        
        public List<PlantPrefabEntry> GetPrefabsForStage(PlantGrowthStage stage)
        {
            return _stageLookup.TryGetValue(stage, out var prefabs) ? prefabs : new List<PlantPrefabEntry>();
        }
        
        public StrainPrefabCollection GetStrainCollection(string strainId)
        {
            return _strainLookup.TryGetValue(strainId, out var collection) ? collection : null;
        }
        
        public List<string> GetAvailableStrains()
        {
            return _strainCollections.Select(s => s.StrainId).ToList();
        }
        
        public PlantVariationData GenerateVariation(string prefabId)
        {
            if (!_enableRandomVariations)
                return null;
            
            return new PlantVariationData
            {
                SizeMultiplier = Random.Range(_sizeVariationRange.x, _sizeVariationRange.y),
                ColorTint = new Color(
                    Random.Range(_colorVariationRange.x, _colorVariationRange.y),
                    Random.Range(_colorVariationRange.x, _colorVariationRange.y),
                    Random.Range(_colorVariationRange.x, _colorVariationRange.y),
                    1f
                ),
                RotationOffset = Random.Range(0f, 360f),
                ShapeVariation = Random.Range(-0.2f, 0.2f)
            };
        }
        
        public bool CanTransitionToStage(string currentPrefabId, PlantGrowthStage targetStage)
        {
            if (!_enableStageTransitions)
                return false;
            
            var currentPrefab = _prefabLookup.TryGetValue(currentPrefabId, out var prefab) ? prefab : null;
            if (currentPrefab == null)
                return false;
            
            // Check if transition is valid based on growth stage progression
            return IsValidStageTransition(currentPrefab.GrowthStage, targetStage);
        }
        
        private bool IsValidStageTransition(PlantGrowthStage from, PlantGrowthStage to)
        {
            int fromIndex = (int)from;
            int toIndex = (int)to;
            
            // Only allow forward progression (no going backwards in growth)
            return toIndex > fromIndex && toIndex <= fromIndex + 1;
        }
        
        public void AddCustomPrefab(PlantPrefabEntry prefabEntry)
        {
            if (_plantPrefabs.Any(p => p.PrefabId == prefabEntry.PrefabId))
            {
                Debug.LogWarning($"Prefab with ID {prefabEntry.PrefabId} already exists");
                return;
            }
            
            _plantPrefabs.Add(prefabEntry);
            BuildLookupTables();
        }
        
        public void RemovePrefab(string prefabId)
        {
            _plantPrefabs.RemoveAll(p => p.PrefabId == prefabId);
            BuildLookupTables();
        }
        
        public PlantLibraryStats GetLibraryStats()
        {
            return new PlantLibraryStats
            {
                TotalPrefabs = _plantPrefabs.Count,
                StrainCount = _strainCollections.Count,
                StageDistribution = _stageLookup.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Count
                ),
                VariationsEnabled = _enableRandomVariations,
                MaxVariationsPerStrain = _maxVariationsPerStrain
            };
        }
        
        private void OnValidate()
        {
            // Rebuild lookup tables when values change in inspector
            if (Application.isPlaying)
            {
                BuildLookupTables();
            }
        }
    }
    
    // Supporting data structures
    [System.Serializable]
    public class PlantPrefabEntry
    {
        public string PrefabId;
        public string PrefabName;
        public GameObject Prefab;
        public PlantGrowthStage GrowthStage;
        public string StrainId;
        public PlantPrefabType PrefabType;
        public List<string> RequiredComponents = new List<string>();
        public List<string> Tags = new List<string>();
        public PlantPrefabMetadata Metadata;
    }
    
    [System.Serializable]
    public class StrainPrefabCollection
    {
        public string StrainId;
        public string StrainName;
        public StrainType StrainType;
        public List<string> PrefabEntries = new List<string>();
        public StrainGrowthProfile GrowthCharacteristics;
        public StrainVisualProfile VisualCharacteristics;
    }
    
    [System.Serializable]
    public class GrowthStagePrefabSet
    {
        public PlantGrowthStage Stage;
        public List<PlantPrefabEntry> StagePrefabs = new List<PlantPrefabEntry>();
        public float StageTransitionTime = 1f;
        public bool RequiresSpecialHandling = false;
    }
    
    [System.Serializable]
    public class StrainGrowthProfile
    {
        public float MaxHeight = 2f;
        public float GrowthRate = 1f;
        public float FloweringTime = 56f; // days
        public float YieldPotential = 1f;
        public float ResistanceLevel = 0.5f;
    }
    
    [System.Serializable]
    public class StrainVisualProfile
    {
        public Color LeafColor = Color.green;
        public Color BudColor = Color.green;
        public Color TrichomeColor = Color.white;
        public PlantShape PlantShape = PlantShape.Balanced;
        public LeafShape LeafShape = LeafShape.Medium;
    }
    
    [System.Serializable]
    public class PlantPrefabMetadata
    {
        public float EstimatedPolyCount;
        public float EstimatedMemoryUsage;
        public List<string> RequiredShaders = new List<string>();
        public List<string> RequiredTextures = new List<string>();
        public bool SupportsLOD;
        public int LODLevels;
    }
    
    [System.Serializable]
    public class PlantVariationData
    {
        public float SizeMultiplier = 1f;
        public Color ColorTint = Color.white;
        public float RotationOffset = 0f;
        public float ShapeVariation = 0f;
        public Vector3 PositionOffset = Vector3.zero;
    }
    
    [System.Serializable]
    public class PlantLibraryStats
    {
        public int TotalPrefabs;
        public int StrainCount;
        public Dictionary<PlantGrowthStage, int> StageDistribution;
        public bool VariationsEnabled;
        public int MaxVariationsPerStrain;
    }
    
    // Enums
    public enum PlantPrefabType
    {
        Base,
        Variation,
        Special,
        Hybrid,
        Premium
    }
    
    public enum StrainType
    {
        Indica,
        Sativa,
        Hybrid,
        Ruderalis,
        Custom
    }
    
    public enum PlantShape
    {
        Tall,
        Bushy,
        Balanced,
        Compact,
        Spreading
    }
    
    public enum LeafShape
    {
        Narrow,
        Medium,
        Broad,
        Serrated,
        Smooth
    }
}