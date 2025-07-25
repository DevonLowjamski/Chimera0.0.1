using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// PC014-2c: Type conversion utilities for genetic system interoperability
    /// Handles conversions between different type representations used across the genetic systems
    /// </summary>
    public static class GeneticTypeConverters
    {
        /// <summary>
        /// Convert Dictionary<TraitType, float> to Dictionary<string, float>
        /// </summary>
        public static Dictionary<string, float> ToStringDictionary(this Dictionary<TraitType, float> traitDict)
        {
            if (traitDict == null) return new Dictionary<string, float>();
            
            return traitDict.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value
            );
        }

        /// <summary>
        /// Convert Dictionary<string, float> to Dictionary<TraitType, float>
        /// </summary>
        public static Dictionary<TraitType, float> ToTraitTypeDictionary(this Dictionary<string, float> stringDict)
        {
            if (stringDict == null) return new Dictionary<TraitType, float>();
            
            var result = new Dictionary<TraitType, float>();
            foreach (var kvp in stringDict)
            {
                if (System.Enum.TryParse<TraitType>(kvp.Key, true, out var traitType))
                {
                    result[traitType] = kvp.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// Convert List<TraitType> to List<string>
        /// </summary>
        public static List<string> ToStringList(this List<TraitType> traitList)
        {
            if (traitList == null) return new List<string>();
            return traitList.Select(t => t.ToString()).ToList();
        }

        /// <summary>
        /// Convert List<string> to List<TraitType>
        /// </summary>
        public static List<TraitType> ToTraitTypeList(this List<string> stringList)
        {
            if (stringList == null) return new List<TraitType>();
            
            var result = new List<TraitType>();
            foreach (var str in stringList)
            {
                if (System.Enum.TryParse<TraitType>(str, true, out var traitType))
                {
                    result.Add(traitType);
                }
            }
            return result;
        }

        /// <summary>
        /// Convert List<PlantGenotype> to List<PlantInstanceSO>
        /// Note: This creates placeholder PlantInstanceSO objects
        /// </summary>
        public static List<PlantInstanceSO> ToPlantInstanceList(this List<PlantGenotype> genotypeList)
        {
            if (genotypeList == null) return new List<PlantInstanceSO>();
            
            var result = new List<PlantInstanceSO>();
            foreach (var genotype in genotypeList)
            {
                // Create placeholder PlantInstanceSO - in real implementation this would 
                // require proper plant instance creation with the genotype
                var plantInstance = UnityEngine.ScriptableObject.CreateInstance<PlantInstanceSO>();
                if (plantInstance != null)
                {
                    // Set basic properties if available
                    // plantInstance.PlantID = genotype.GenotypeID;
                    result.Add(plantInstance);
                }
            }
            return result;
        }

        /// <summary>
        /// Convert float to string safely
        /// </summary>
        public static string ToSafeString(this float value)
        {
            return value.ToString("F2");
        }

        /// <summary>
        /// Convert nullable BreedingStrategy to string
        /// </summary>
        public static string ToSafeString(this BreedingStrategy? strategy)
        {
            return strategy?.ToString() ?? "NotSpecified";
        }

        /// <summary>
        /// Convert BreedingGoalStatus to string
        /// </summary>
        public static string ToSafeString(this BreedingGoalStatus status)
        {
            return status.ToString();
        }
    }
}