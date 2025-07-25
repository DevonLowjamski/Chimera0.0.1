using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Systems.Construction
{
    /// <summary>
    /// Extension methods for string types used in construction systems
    /// </summary>
    public static class ConstructionStringExtensions
    {
        /// <summary>
        /// Get MaterialId from a string (used when string represents a material identifier)
        /// </summary>
        public static string MaterialId(this string value)
        {
            // If the string is already a material ID, return it
            return value ?? string.Empty;
        }
        
        /// <summary>
        /// Star certification tracking method for string types
        /// </summary>
        public static string StarCertificationTracking(this string certificationId)
        {
            return $"Tracking-{certificationId}";
        }
        
        /// <summary>
        /// Calculate activity progress for string activity IDs
        /// </summary>
        public static string CalculateActivityProgress(this string activityId)
        {
            return "0%"; // Placeholder implementation
        }
    }
}