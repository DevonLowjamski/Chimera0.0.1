using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Enhanced Cultivation Gaming Configuration - Master configuration for all cultivation gaming systems
    /// Contains references to all sub-configurations for interactive plant care, automation progression,
    /// skill tree gaming, time acceleration, and player agency systems
    /// </summary>
    [CreateAssetMenu(fileName = "Enhanced_Cultivation_Gaming_Config", menuName = "Project Chimera/Cultivation/Enhanced Gaming Config")]
    public class EnhancedCultivationGamingConfigSO : ChimeraConfigSO
    {
        [Header("Core Gaming Configuration")]
        [SerializeField] private string _systemName = "Enhanced Cultivation Gaming System";
        [SerializeField] private string _version = "2.0";
        [SerializeField] private bool _enableAdvancedFeatures = true;
        
        [Header("Interactive Plant Care Systems")]
        [SerializeField] private InteractivePlantCareConfigSO _plantCareConfig;
        [SerializeField] private PlantInteractionConfigSO _interactionConfig;
        [SerializeField] private CareToolLibrarySO _careToolLibrary;
        
        [Header("Earned Automation Progression")]
        [SerializeField] private EarnedAutomationConfigSO _automationConfig;
        [SerializeField] private AutomationUnlockLibrarySO _automationUnlocks;
        [SerializeField] private BurdenCalculationConfigSO _burdenCalculation;
        
        [Header("Skill Tree Gaming")]
        [SerializeField] private TreeSkillProgressionConfigSO _skillTreeConfig;
        [SerializeField] private SkillTreeConfigSO _skillTreeConfigSO;
        [SerializeField] private SkillNodeLibrarySO _skillNodeLibrary;
        [SerializeField] private TreeVisualizationConfigSO _treeVisualization;
        
        [Header("Time Acceleration Gaming")]
        [SerializeField] private TimeAccelerationGamingConfigSO _timeAccelerationConfig;
        [SerializeField] private TimeTransitionConfigSO _timeTransitionConfig;
        [SerializeField] private TimeScaleLibrarySO _timeScaleLibrary;
        
        [Header("Player Agency Gaming")]
        [SerializeField] private PlayerAgencyGamingConfigSO _playerAgencyConfig;
        [SerializeField] private CultivationPathLibrarySO _cultivationPaths;
        [SerializeField] private FacilityDesignConfigSO _facilityDesignConfig;
        [SerializeField] private FacilityDesignLibrarySO _facilityDesignLibrary;
        
        [Header("Gaming Performance Settings")]
        [Range(0.1f, 5.0f)] public float GlobalGamingMultiplier = 1.0f;
        [Range(0.1f, 2.0f)] public float SkillProgressionRate = 1.0f;
        [Range(0.1f, 2.0f)] public float AutomationDesireMultiplier = 1.0f;
        [Range(0.1f, 3.0f)] public float ExpressionRewardMultiplier = 1.2f;
        [Range(0.1f, 2.0f)] public float FeedbackIntensityMultiplier = 1.0f;
        
        [Header("Gaming Balance Settings")]
        [Range(0.1f, 1.0f)] public float CareActionDifficultyBase = 0.5f;
        [Range(1.0f, 10.0f)] public float AutomationUnlockThreshold = 5.0f;
        [Range(0.1f, 2.0f)] public float TimeAccelerationLockInPeriod = 1.0f;
        [Range(0.1f, 1.0f)] public float PlayerChoiceImpactThreshold = 0.6f;
        
        // Public Properties for System Access
        public string SystemName => _systemName;
        public string Version => _version;
        public bool EnableAdvancedFeatures => _enableAdvancedFeatures;
        
        // Interactive Plant Care Configuration
        public InteractivePlantCareConfigSO PlantCareConfig => _plantCareConfig;
        public PlantInteractionConfigSO InteractionConfig => _interactionConfig;
        public CareToolLibrarySO CareToolLibrary => _careToolLibrary;
        
        // Earned Automation Configuration
        public EarnedAutomationConfigSO AutomationConfig => _automationConfig;
        public AutomationUnlockLibrarySO AutomationUnlocks => _automationUnlocks;
        public BurdenCalculationConfigSO BurdenCalculation => _burdenCalculation;
        
        // Skill Tree Gaming Configuration
        public TreeSkillProgressionConfigSO SkillTreeConfig => _skillTreeConfig;
        public SkillTreeConfigSO SkillTreeConfigSO => _skillTreeConfigSO;
        public SkillNodeLibrarySO SkillNodeLibrary => _skillNodeLibrary;
        public TreeVisualizationConfigSO TreeVisualization => _treeVisualization;
        
        // Time Acceleration Gaming Configuration
        public TimeAccelerationGamingConfigSO TimeAccelerationConfig => _timeAccelerationConfig;
        public TimeTransitionConfigSO TimeTransitionConfig => _timeTransitionConfig;
        public TimeScaleLibrarySO TimeScaleLibrary => _timeScaleLibrary;
        
        // Player Agency Gaming Configuration
        public PlayerAgencyGamingConfigSO PlayerAgencyConfig => _playerAgencyConfig;
        public CultivationPathLibrarySO CultivationPaths => _cultivationPaths;
        public FacilityDesignConfigSO FacilityDesignConfig => _facilityDesignConfig;
        
        // Additional properties for EnhancedCultivationGamingManager
        public InteractivePlantCareConfigSO InteractivePlantCareConfig => _plantCareConfig;
        public PlantInteractionConfigSO PlantInteractionConfig => _interactionConfig;
        public EarnedAutomationConfigSO EarnedAutomationConfig => _automationConfig;
        public TimeAccelerationGamingConfigSO TimeAccelerationGamingConfig => _timeAccelerationConfig;
        public PlayerAgencyGamingConfigSO PlayerAgencyGamingConfig => _playerAgencyConfig;
        public CultivationPathLibrarySO CultivationPathLibrary => _cultivationPaths;
        public FacilityDesignLibrarySO FacilityDesignLibrary => _facilityDesignLibrary;
        
        #region Validation
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            ValidateReferences();
            ValidateSettings();
        }
        
        private void ValidateReferences()
        {
            // Validate critical references
            if (_plantCareConfig == null)
                Debug.LogWarning($"[{name}] InteractivePlantCareConfigSO is not assigned", this);
                
            if (_automationConfig == null)
                Debug.LogWarning($"[{name}] EarnedAutomationConfigSO is not assigned", this);
                
            if (_skillTreeConfig == null)
                Debug.LogWarning($"[{name}] TreeSkillProgressionConfigSO is not assigned", this);
                
            if (_timeAccelerationConfig == null)
                Debug.LogWarning($"[{name}] TimeAccelerationGamingConfigSO is not assigned", this);
                
            if (_playerAgencyConfig == null)
                Debug.LogWarning($"[{name}] PlayerAgencyGamingConfigSO is not assigned", this);
        }
        
        private void ValidateSettings()
        {
            // Ensure multipliers are within reasonable ranges
            GlobalGamingMultiplier = Mathf.Clamp(GlobalGamingMultiplier, 0.1f, 5.0f);
            SkillProgressionRate = Mathf.Clamp(SkillProgressionRate, 0.1f, 2.0f);
            AutomationDesireMultiplier = Mathf.Clamp(AutomationDesireMultiplier, 0.1f, 2.0f);
            ExpressionRewardMultiplier = Mathf.Clamp(ExpressionRewardMultiplier, 0.1f, 3.0f);
            FeedbackIntensityMultiplier = Mathf.Clamp(FeedbackIntensityMultiplier, 0.1f, 2.0f);
            
            // Ensure thresholds are within valid ranges
            CareActionDifficultyBase = Mathf.Clamp01(CareActionDifficultyBase);
            PlayerChoiceImpactThreshold = Mathf.Clamp01(PlayerChoiceImpactThreshold);
        }
        
        #endregion
        
        #region Configuration Helpers
        
        /// <summary>
        /// Get configuration completeness percentage
        /// </summary>
        public float GetConfigurationCompleteness()
        {
            int totalConfigs = 15; // Total number of configuration references
            int assignedConfigs = 0;
            
            if (_plantCareConfig != null) assignedConfigs++;
            if (_interactionConfig != null) assignedConfigs++;
            if (_careToolLibrary != null) assignedConfigs++;
            if (_automationConfig != null) assignedConfigs++;
            if (_automationUnlocks != null) assignedConfigs++;
            if (_burdenCalculation != null) assignedConfigs++;
            if (_skillTreeConfig != null) assignedConfigs++;
            if (_skillNodeLibrary != null) assignedConfigs++;
            if (_treeVisualization != null) assignedConfigs++;
            if (_timeAccelerationConfig != null) assignedConfigs++;
            if (_timeTransitionConfig != null) assignedConfigs++;
            if (_timeScaleLibrary != null) assignedConfigs++;
            if (_playerAgencyConfig != null) assignedConfigs++;
            if (_cultivationPaths != null) assignedConfigs++;
            if (_facilityDesignConfig != null) assignedConfigs++;
            
            return (float)assignedConfigs / totalConfigs;
        }
        
        /// <summary>
        /// Get list of missing configuration references
        /// </summary>
        public System.Collections.Generic.List<string> GetMissingConfigurations()
        {
            var missing = new System.Collections.Generic.List<string>();
            
            if (_plantCareConfig == null) missing.Add("InteractivePlantCareConfigSO");
            if (_interactionConfig == null) missing.Add("PlantInteractionConfigSO");
            if (_careToolLibrary == null) missing.Add("CareToolLibrarySO");
            if (_automationConfig == null) missing.Add("EarnedAutomationConfigSO");
            if (_automationUnlocks == null) missing.Add("AutomationUnlockLibrarySO");
            if (_burdenCalculation == null) missing.Add("BurdenCalculationConfigSO");
            if (_skillTreeConfig == null) missing.Add("TreeSkillProgressionConfigSO");
            if (_skillNodeLibrary == null) missing.Add("SkillNodeLibrarySO");
            if (_treeVisualization == null) missing.Add("TreeVisualizationConfigSO");
            if (_timeAccelerationConfig == null) missing.Add("TimeAccelerationGamingConfigSO");
            if (_timeTransitionConfig == null) missing.Add("TimeTransitionConfigSO");
            if (_timeScaleLibrary == null) missing.Add("TimeScaleLibrarySO");
            if (_playerAgencyConfig == null) missing.Add("PlayerAgencyGamingConfigSO");
            if (_cultivationPaths == null) missing.Add("CultivationPathLibrarySO");
            if (_facilityDesignConfig == null) missing.Add("FacilityDesignConfigSO");
            
            return missing;
        }
        
        /// <summary>
        /// Check if configuration is ready for production use
        /// </summary>
        public bool IsProductionReady()
        {
            var completeness = GetConfigurationCompleteness();
            var hasRequiredConfigs = _plantCareConfig != null && _automationConfig != null && 
                                   _skillTreeConfig != null && _timeAccelerationConfig != null && 
                                   _playerAgencyConfig != null;
            
            return completeness >= 0.8f && hasRequiredConfigs;
        }
        
        #endregion
    }
}