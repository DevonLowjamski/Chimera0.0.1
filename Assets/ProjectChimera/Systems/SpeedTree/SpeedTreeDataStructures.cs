using System;
using UnityEngine;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Systems.Cultivation;
using System.Collections.Generic;

#if UNITY_SPEEDTREE
using SpeedTree;
#endif

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Core data structures for SpeedTree system extracted from AdvancedSpeedTreeManager.
    /// Provides essential data types for plant instances, genetic data, and configuration.
    /// </summary>
    
    /// <summary>
    /// SpeedTree plant instance data.
    /// </summary>
    [System.Serializable]
    public class SpeedTreePlantData
    {
        public int InstanceId;
        public InteractivePlantComponent PlantComponent;
        public string StrainId;
        public float CreationTime;
        public CannabisGeneticData GeneticData;
        public PlantGrowthStage CurrentGrowthStage;
        public float GrowthProgress;
        public float Health = 1f;
        public float StressLevel = 0f;
        
        #if UNITY_SPEEDTREE
        public SpeedTree.SpeedTreeRenderer Renderer;
        #else
        public SpeedTreeRenderer Renderer; // Use custom SpeedTreeRenderer for fallback
        #endif
        
        public Dictionary<string, object> Properties = new Dictionary<string, object>();
        
        // Additional properties for compatibility with AdvancedSpeedTreeManager
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale = Vector3.one;
        public PlantGrowthStage GrowthStage;
        public float Age;
        public float LastUpdateTime;
        public float DistanceToCamera;
        public CannabisStrainAsset StrainAsset;
        public EnvironmentalStressData StressData;
        public bool IsActive;
        public Dictionary<string, float> EnvironmentalModifiers = new Dictionary<string, float>();
        public ProjectChimera.Data.Environment.EnvironmentalConditions EnvironmentalConditions;
        
        // Additional properties for compatibility
        public float GrowthRate = 1f;
        public float StressResistance = 1f;
        public float MutationRate = 0.1f;
        
        public string SpeedTreeAssetPath => StrainAsset?.SpeedTreeAssetPath ?? "";
        public Color CurrentLeafColor = Color.green;
        public float GrowthAnimationTime = 1f;
        
        // Additional properties for Error Wave 143 compatibility
        public bool HasValue => IsActive;
        public SpeedTreePlantData Value => this;
        
        // Missing method for compatibility  
        public PlantGrowthStage GetGrowthStage() => GrowthStage;
    }
    
    /// <summary>
    /// Cannabis genetic data for SpeedTree visualization.
    /// </summary>
    [System.Serializable]
    public class CannabisGeneticData
    {
        public string StrainId;
        public GeneExpression HeightGenes;
        public GeneExpression BranchingGenes;
        public GeneExpression LeafSizeGenes;
        public GeneExpression ColorGenes;
        public GeneExpression GrowthRateGenes;
        public Dictionary<string, object> Properties = new Dictionary<string, object>();
        
        // Missing properties
        public float BudDensity = 1f;
        public Color BudColor = Color.green;
        
        // Additional missing properties from AdvancedSpeedTreeManager usage
        public float PlantSize = 1f;
        public float ColorVariation = 0.1f;
        public float Saturation = 1f;
        public float Brightness = 1f;
        public Color LeafColor = Color.green;
        public float TrichromeAmount = 0.5f;
        public float BranchDensity = 1f;
        public float LeafSize = 1f;
        public float LeafDensity = 1f;
        public float PistilLength = 1f;
        public float StemThickness = 1f;
        public float DroughtTolerance = 1f;
        
        // Missing properties referenced in other files
        public float FloweringSpeed = 1f;
        public float YieldPotential = 1f;
        public float GrowthRate = 1f;
        public float HeatTolerance = 1f;
        public float ColdTolerance = 1f;
        public float DiseaseResistance = 1f;
        
        // Additional properties for genetics service integration
        public string PhenotypeId = "";
        public DateTime LastGeneticUpdate = DateTime.Now;
    }
    
    /// <summary>
    /// SpeedTree asset data.
    /// </summary>
    [System.Serializable]
    public class SpeedTreeAsset
    {
        public string AssetPath;
        public string AssetName;
        public GameObject AssetPrefab;
        public Dictionary<string, float> EnvironmentalModifiers = new Dictionary<string, float>();
    }
    
    /// <summary>
    /// SpeedTree wind settings.
    /// </summary>
    [System.Serializable]
    public class SpeedTreeWindSettings
    {
        public float WindStrength = 1f;
        public Vector3 WindDirection = Vector3.forward;
        public float WindTurbulence = 0.5f;
        public float WindGustiness = 0.3f;
        public bool EnableBending = true;
        public bool EnableFlutter = true;
        
        // Missing properties referenced in AdvancedSpeedTreeManager
        public float WindMain = 1f;
        public float WindPulseMagnitude = 0.3f;
        public float WindPulseFrequency = 0.1f;
    }
    
    /// <summary>
    /// Cannabis strain asset configuration.
    /// </summary>
    [System.Serializable]
    public class CannabisStrainAsset
    {
        public string StrainId;
        public string StrainName;
        public string AssetPath;
        public CannabisGeneticData DefaultGenetics;
        public float GrowthRateModifier = 1f;
        public float HealthModifier = 1f;
        
        // Missing properties
        public string SpeedTreeAssetPath = "";
        public Vector2 HeightRange = new Vector2(0.5f, 2f);
        public Vector2 WidthRange = new Vector2(0.3f, 1.5f);
        public Vector2 FloweringTimeRange = new Vector2(8f, 12f);
    }
    
    // SpeedTree Renderer wrapper for non-SpeedTree builds
    #if !UNITY_SPEEDTREE
    /// <summary>
    /// Fallback SpeedTree renderer for when SpeedTree package is not available.
    /// </summary>
    public class SpeedTreeRenderer : MonoBehaviour
    {
        public SpeedTreeAsset speedTreeAsset;
        public bool enableCrossFade = true;
        public float fadeOutLength = 1f;
        public bool animateOnCulling = false;
        
        // Missing properties for compatibility
        public Renderer Renderer => GetComponent<Renderer>();
        public MaterialPropertyBlock materialProperties = new MaterialPropertyBlock();
        public Transform transform => base.transform;
        public GameObject gameObject => base.gameObject;
        public Material[] materials => Renderer?.materials ?? new Material[0];
        
        // Placeholder methods
        public void SetWindSettings(SpeedTreeWindSettings settings) { }
        public void UpdateMaterials() { }
        public void SetPropertyBlock(MaterialPropertyBlock block) { 
            materialProperties = block; 
            Renderer?.SetPropertyBlock(block);
        }
    }
    
    /// <summary>
    /// Fallback SpeedTree wind component.
    /// </summary>
    public class SpeedTreeWind : MonoBehaviour
    {
        public SpeedTreeWindSettings WindSettings;
        
        public void ApplyWindSettings(SpeedTreeWindSettings settings)
        {
            WindSettings = settings;
        }
    }
    #endif
    
}