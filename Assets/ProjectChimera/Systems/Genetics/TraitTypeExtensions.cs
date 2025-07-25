using ProjectChimera.Data.Genetics;
using System.Collections.Generic;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// PC014-2c: Extension methods for TraitType to handle string conversions
    /// </summary>
    public static class TraitTypeExtensions
    {
        /// <summary>
        /// Convert TraitType to string representation
        /// </summary>
        public static string ToTraitString(this TraitType traitType)
        {
            return traitType.ToString();
        }

        /// <summary>
        /// Convert list of TraitType to list of strings
        /// </summary>
        public static List<string> ToStringList(this List<TraitType> traitTypes)
        {
            var result = new List<string>();
            foreach (var trait in traitTypes)
            {
                result.Add(trait.ToString());
            }
            return result;
        }

        /// <summary>
        /// Convert string to TraitType
        /// </summary>
        public static TraitType FromString(string traitString)
        {
            if (System.Enum.TryParse<TraitType>(traitString, true, out var result))
            {
                return result;
            }
            return TraitType.Height; // Default fallback
        }

        /// <summary>
        /// Convert list of strings to list of TraitType
        /// </summary>
        public static List<TraitType> FromStringList(List<string> traitStrings)
        {
            var result = new List<TraitType>();
            foreach (var traitString in traitStrings)
            {
                result.Add(FromString(traitString));
            }
            return result;
        }

        /// <summary>
        /// Get trait name as string
        /// </summary>
        public static string Trait(this TraitType traitType)
        {
            return traitType.ToString();
        }

        /// <summary>
        /// Get target value for trait (placeholder implementation)
        /// </summary>
        public static float TargetValue(this TraitType traitType)
        {
            switch (traitType)
            {
                case TraitType.Yield:
                case TraitType.TotalYield:
                    return 1.0f;
                case TraitType.THCContent:
                case TraitType.THCPotency:
                    return 0.25f;
                case TraitType.CBDContent:
                    return 0.15f;
                case TraitType.TerpeneProduction:
                    return 0.05f;
                case TraitType.Height:
                case TraitType.PlantHeight:
                    return 1.5f;
                case TraitType.FloweringTime:
                    return 60f;
                case TraitType.DiseaseResistance:
                    return 0.8f;
                case TraitType.HeatTolerance:
                case TraitType.ColdTolerance:
                case TraitType.DroughtTolerance:
                    return 0.7f;
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Get trait weight for breeding optimization
        /// </summary>
        public static float Weight(this TraitType traitType)
        {
            switch (traitType)
            {
                case TraitType.Yield:
                case TraitType.TotalYield:
                    return 1.0f;
                case TraitType.THCContent:
                case TraitType.THCPotency:
                    return 0.8f;
                case TraitType.CBDContent:
                    return 0.6f;
                case TraitType.TerpeneProduction:
                    return 0.4f;
                case TraitType.Height:
                case TraitType.PlantHeight:
                    return 0.3f;
                case TraitType.FloweringTime:
                    return 0.7f;
                case TraitType.DiseaseResistance:
                    return 0.9f;
                case TraitType.HeatTolerance:
                case TraitType.ColdTolerance:
                case TraitType.DroughtTolerance:
                    return 0.5f;
                default:
                    return 0.5f;
            }
        }

        /// <summary>
        /// Get completeness status for breeding goal
        /// </summary>
        public static string CompletedAt(this BreedingGoal goal)
        {
            if (goal.Status == BreedingGoalStatus.Completed)
            {
                return System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return "Not completed";
        }

        /// <summary>
        /// Get breeding strategy as string
        /// Note: BreedingRecommendation type not yet defined - commenting out for now
        /// </summary>
        /*
        public static string Strategy(this BreedingRecommendation recommendation)
        {
            return recommendation.Strategy.ToString();
        }
        */
    }
}