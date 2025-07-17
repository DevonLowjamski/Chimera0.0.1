using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Simple data structure for trait expression results.
    /// Data-only version for use in ScriptableObjects.
    /// </summary>
    [System.Serializable]
    public class TraitExpressionResult
    {
        [Header("Basic Information")]
        public string GenotypeID;
        public float OverallFitness = 1.0f;
        public float EnvironmentalStress = 0.0f;
        
        [Header("Trait Values")]
        public List<TraitValue> TraitValues = new List<TraitValue>();
        
        public void Reset()
        {
            GenotypeID = "";
            OverallFitness = 1.0f;
            EnvironmentalStress = 0.0f;
            TraitValues.Clear();
        }
        
        public float GetTraitValue(TraitType trait)
        {
            var traitValue = TraitValues.Find(tv => tv.Trait == trait);
            return traitValue?.Value ?? 0f;
        }
        
        public void SetTraitValue(TraitType trait, float value)
        {
            var existing = TraitValues.Find(tv => tv.Trait == trait);
            if (existing != null)
            {
                existing.Value = value;
            }
            else
            {
                TraitValues.Add(new TraitValue { Trait = trait, Value = value });
            }
        }
        
        /// <summary>
        /// Gets the height expression value for compatibility.
        /// </summary>
        public float HeightExpression()
        {
            return GetTraitValue(TraitType.PlantHeight);
        }
    }
    
    [System.Serializable]
    public class TraitValue
    {
        public TraitType Trait;
        public float Value;
    }
}