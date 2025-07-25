using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Cannabis strain asset for extended strain data.
    /// Contains genetic and environmental characteristics for cannabis strains.
    /// </summary>
    [CreateAssetMenu(fileName = "CannabisStrain", menuName = "Project Chimera/Genetics/Cannabis Strain")]
    public class CannabisStrainAsset : ScriptableObject
    {
        [Header("Strain Identity")]
        public string StrainId;
        public string StrainName;
        public string Description;
        public StrainType StrainType;
        public bool IsFounderStrain;
        public bool IsCustomStrain; // Added for UI compatibility - indicates if this is a custom/player-created strain
        
        [Header("Genetic Profile")]
        public List<GeneticTrait> GeneticTraits = new List<GeneticTrait>();
        public Dictionary<string, float> TraitValues = new Dictionary<string, float>();
        
        [Header("Growth Characteristics")]
        public float FloweringTime = 8.0f;
        public float YieldPotential = 1.0f;
        public float VigorRating = 1.0f;
        public float StressResistance = 1.0f;

        [Header("Genetic Properties")]
        public Vector2 HeightRange = new Vector2(0.5f, 2.0f);
        public float LeafSize = 1.0f;
        public float BudDensity = 1.0f;
        public float TrichromeAmount = 1.0f;
        public Vector2 FloweringTimeRange = new Vector2(8f, 12f);
        public Vector2 YieldRange = new Vector2(300f, 600f);

        [Header("Visual Properties")]
        public Color LeafColorBase = Color.green;
        public Color BudColorBase = Color.white;
        public Vector3 Morphology = Vector3.one;

        [Header("Strain Type Influences")]
        [Range(0f, 1f)] public float IndicaDominance = 0.5f;
        [Range(0f, 1f)] public float SativaDominance = 0.5f;
        [Range(0f, 1f)] public float RuderalisInfluence = 0.0f;
        
        [Header("Environmental Preferences")]
        public EnvironmentalRange TemperatureRange;
        public EnvironmentalRange HumidityRange;
        public EnvironmentalRange LightIntensityRange;
        public EnvironmentalRange NutrientRange;
        
        /// <summary>
        /// Gets the THC content for this strain asset.
        /// </summary>
        public float THCContent()
        {
            // Try to get from trait values, fallback to default
            if (TraitValues.TryGetValue("THC", out float thc))
                return thc;
            return 15f; // Default THC percentage
        }

        /// <summary>
        /// Gets the CBD content for this strain asset.
        /// </summary>
        public float CBDContent()
        {
            // Try to get from trait values, fallback to default
            if (TraitValues.TryGetValue("CBD", out float cbd))
                return cbd;
            return 1f; // Default CBD percentage
        }

        /// <summary>
        /// Gets the base yield for this strain asset.
        /// </summary>
        public float BaseYield()
        {
            return YieldRange.y; // Use maximum yield as base yield
        }
    }

    [System.Serializable]
    public class EnvironmentalRange
    {
        public float MinValue;
        public float MaxValue;
        public float OptimalValue;
    }
}