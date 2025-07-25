using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Plant Interaction Configuration - Configuration for plant care and interaction systems
    /// Defines interaction types, care actions, and plant response mechanics
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Plant Interaction Config", menuName = "Project Chimera/Cultivation/Plant Interaction Config")]
    public class PlantInteractionConfigSO : ChimeraConfigSO
    {
        [Header("Basic Interactions")]
        public List<PlantInteractionType> AvailableInteractions = new List<PlantInteractionType>();
        
        [Header("Care Actions")]
        public List<CareActionConfig> CareActions = new List<CareActionConfig>();
        
        [Header("Response Settings")]
        [Range(0.1f, 5.0f)] public float InteractionResponseMultiplier = 1.0f;
        [Range(0.1f, 2.0f)] public float CareQualityBonus = 1.2f;
        
        [Header("Timing Configuration")]
        [Range(1, 72)] public int InteractionCooldownHours = 6;
        [Range(0.1f, 24.0f)] public float OptimalCareInterval = 12.0f;
    }
    
    [System.Serializable]
    public class PlantInteractionType
    {
        public string InteractionName;
        public InteractionCategory Category;
        [Range(0.1f, 3.0f)] public float EffectMultiplier = 1.0f;
        public bool RequiresTools = false;
        public List<string> RequiredTools = new List<string>();
    }
    
    [System.Serializable]
    public class CareActionConfig
    {
        public string ActionName;
        public CareActionType ActionType;
        [Range(0.1f, 2.0f)] public float QualityImpact = 1.0f;
        [Range(1, 168)] public int CooldownHours = 24;
        public bool IsAutomatable = false;
    }
    
    public enum InteractionCategory
    {
        Watering,
        Feeding,
        Pruning,
        Training,
        Monitoring,
        Harvesting,
        Maintenance,
        Observation
    }
    
    public enum CareActionType
    {
        BasicCare,
        AdvancedCare,
        SpecializedCare,
        MaintenanceAction,
        OptimizationAction,
        RecoveryAction,
        PreventativeAction,
        EnhancementAction
    }
}