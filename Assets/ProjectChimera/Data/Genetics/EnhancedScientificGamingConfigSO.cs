using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Enhanced Scientific Gaming Configuration - Main configuration for unified genetics and terpene gaming systems
    /// Defines global settings, integration parameters, and cross-system synergy rules
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Enhanced Scientific Gaming Config", menuName = "Project Chimera/Gaming/Enhanced Scientific Gaming Config")]
    public class EnhancedScientificGamingConfigSO : ChimeraConfigSO
    {
        [Header("System Integration Settings")]
        [Range(0.1f, 5.0f)] public float CrossSystemSynergyMultiplier = 2.0f;
        [Range(0.1f, 3.0f)] public float IntegrationRewardBonus = 1.5f;
        [Range(1.0f, 10.0f)] public float FullIntegrationThreshold = 7.0f;
        [Range(0.1f, 1.0f)] public float SystemBalanceWeight = 0.8f;
        
        [Header("Gaming Experience Modifiers")]
        [Range(0.1f, 3.0f)] public float GlobalGamingDifficultyMultiplier = 1.0f;
        [Range(0.1f, 5.0f)] public float InnovationRewardMultiplier = 2.5f;
        [Range(0.1f, 2.0f)] public float CommunityEngagementBonus = 1.3f;
        [Range(0.1f, 3.0f)] public float CompetitiveProgressionRate = 1.2f;
        
        [Header("System Configurations")]
        public GeneticsGamingConfigSO GeneticsGamingConfig;
        public AromaticGamingConfigSO AromaticGamingConfig;
        public ScientificCompetitionConfigSO CompetitionConfig;
        public CommunityCollaborationConfigSO CommunityConfig;
        public ScientificProgressionConfigSO ProgressionConfig;
        public GeneticDiscoveryDatabaseSO GeneticDiscoveryConfig;
        public GeneticVisualizationConfigSO GeneticInterfaceConfig;
        
        [Header("Cross-System Synergy Rules")]
        public List<CrossSystemSynergyRule> SynergyRules = new List<CrossSystemSynergyRule>();
        
        [Header("Gaming Events Configuration")]
        public List<ScientificGamingEventConfig> EventConfigurations = new List<ScientificGamingEventConfig>();
        
        [Header("Seasonal Gaming Events")]
        public List<SeasonalGamingEvent> SeasonalEvents = new List<SeasonalGamingEvent>();
        
        [Header("Achievement Integration")]
        public List<CrossSystemAchievementConfig> IntegratedAchievements = new List<CrossSystemAchievementConfig>();
        
        [Header("Performance Optimization")]
        [Range(10, 500)] public int MaxConcurrentChallenges = 100;
        [Range(1, 60)] public int AnalyticsUpdateInterval = 30;
        [Range(0.1f, 5.0f)] public float PerformanceScalingFactor = 1.0f;
        public bool EnableAdvancedAnalytics = true;
        
        [Header("UI Integration Settings")]
        public ScientificGamingUIConfigSO UIConfiguration;
        public bool EnableCrossSystemNotifications = true;
        public bool EnableProgressionVisualizations = true;
        public bool EnableRealTimeUpdates = true;
        
        [Header("Validation Settings")]
        public bool ValidateSystemIntegration = true;
        public bool EnableDebugLogging = false;
        public bool EnablePerformanceMetrics = true;
        
        #region Validation
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (ValidateSystemIntegration)
            {
                ValidateSystemConfigurations();
                ValidateSynergyRules();
                ValidateEventConfigurations();
            }
        }
        
        private void ValidateSystemConfigurations()
        {
            if (GeneticsGamingConfig == null)
                Debug.LogWarning("GeneticsGamingConfig is not assigned", this);
                
            if (AromaticGamingConfig == null)
                Debug.LogWarning("AromaticGamingConfig is not assigned", this);
                
            if (CompetitionConfig == null)
                Debug.LogWarning("CompetitionConfig is not assigned", this);
                
            if (CommunityConfig == null)
                Debug.LogWarning("CommunityConfig is not assigned", this);
                
            if (ProgressionConfig == null)
                Debug.LogWarning("ProgressionConfig is not assigned", this);
        }
        
        private void ValidateSynergyRules()
        {
            foreach (var rule in SynergyRules)
            {
                if (rule.RequiredSystemCount < 2)
                {
                    Debug.LogWarning($"Synergy rule {rule.SynergyName} requires at least 2 systems", this);
                }
            }
        }
        
        private void ValidateEventConfigurations()
        {
            var eventNames = new HashSet<string>();
            foreach (var eventConfig in EventConfigurations)
            {
                if (eventNames.Contains(eventConfig.EventName))
                {
                    Debug.LogWarning($"Duplicate event configuration: {eventConfig.EventName}", this);
                }
                eventNames.Add(eventConfig.EventName);
            }
        }
        
        #endregion
        
        #region Runtime Methods
        
        public float GetSynergyMultiplier(CrossSystemSynergyType synergyType)
        {
            var rule = SynergyRules.Find(r => r.SynergyType == synergyType);
            return rule?.SynergyMultiplier ?? CrossSystemSynergyMultiplier;
        }
        
        public bool IsSynergyEnabled(CrossSystemSynergyType synergyType)
        {
            var rule = SynergyRules.Find(r => r.SynergyType == synergyType);
            return rule?.IsEnabled ?? true;
        }
        
        public ScientificGamingEventConfig GetEventConfiguration(string eventName)
        {
            return EventConfigurations.Find(e => e.EventName == eventName);
        }
        
        public List<SeasonalGamingEvent> GetActiveSeasonalEvents()
        {
            var currentSeason = GetCurrentSeason();
            return SeasonalEvents.FindAll(e => e.Season == currentSeason && e.IsActive);
        }
        
        private GameSeason GetCurrentSeason()
        {
            // Simple season calculation based on month
            var month = System.DateTime.Now.Month;
            return month switch
            {
                12 or 1 or 2 => GameSeason.Winter,
                3 or 4 or 5 => GameSeason.Spring,
                6 or 7 or 8 => GameSeason.Summer,
                9 or 10 or 11 => GameSeason.Fall,
                _ => GameSeason.Spring
            };
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class CrossSystemSynergyRule
    {
        public string SynergyName;
        public CrossSystemSynergyType SynergyType;
        public bool IsEnabled = true;
        [Range(0.1f, 5.0f)] public float SynergyMultiplier = 1.5f;
        [Range(2, 4)] public int RequiredSystemCount = 2;
        public List<ScientificGamingSystemType> RequiredSystems = new List<ScientificGamingSystemType>();
        public List<SynergyCondition> ActivationConditions = new List<SynergyCondition>();
    }
    
    [System.Serializable]
    public class SynergyCondition
    {
        public string ConditionName;
        public SynergyConditionType ConditionType;
        public float RequiredValue;
        public string Description;
    }
    
    [System.Serializable]
    public class ScientificGamingEventConfig
    {
        public string EventName;
        public ScientificGamingEventType EventType;
        public bool IsEnabled = true;
        [Range(0.1f, 5.0f)] public float EventMultiplier = 1.0f;
        public List<string> RequiredAchievements = new List<string>();
        public List<string> UnlockedFeatures = new List<string>();
    }
    
    [System.Serializable]
    public class SeasonalGamingEvent
    {
        public string EventName;
        public GameSeason Season;
        public bool IsActive = true;
        [Range(1, 365)] public int DurationDays = 30;
        [Range(0.1f, 3.0f)] public float SeasonalBonus = 1.2f;
        public List<string> SpecialRewards = new List<string>();
        public SeasonalEventType EventType;
    }
    
    [System.Serializable]
    public class CrossSystemAchievementConfig
    {
        public string AchievementName;
        public List<ScientificGamingSystemType> RequiredSystems = new List<ScientificGamingSystemType>();
        public List<AchievementRequirement> Requirements = new List<AchievementRequirement>();
        [Range(0.1f, 5.0f)] public float CompletionReward = 2.0f;
        public bool IsLegacyAchievement = false;
    }
    
    [System.Serializable]
    public class AchievementRequirement
    {
        public string RequirementName;
        public AchievementRequirementType RequirementType;
        public float TargetValue;
        public string Description;
    }
    
    #endregion
    
}
