using System;
using System.Collections.Generic;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Interface for player progression tracking service
    /// Placeholder interface for AI service dependency resolution
    /// </summary>
    public interface IPlayerProgressionService
    {
        /// <summary>
        /// Whether the service is initialized
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Initialize the progression service
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Shutdown the progression service
        /// </summary>
        void Shutdown();
        
        /// <summary>
        /// Get current player level (placeholder)
        /// </summary>
        /// <returns>Player level</returns>
        int GetPlayerLevel();
        
        /// <summary>
        /// Get player experience points (placeholder)
        /// </summary>
        /// <returns>Experience points</returns>
        float GetExperiencePoints();
        
        /// <summary>
        /// Get player preferences for AI adaptation (placeholder)
        /// </summary>
        /// <returns>Dictionary of preference keys and values</returns>
        Dictionary<string, object> GetPlayerPreferences();
    }
}