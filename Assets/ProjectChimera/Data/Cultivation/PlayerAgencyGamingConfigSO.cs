using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Player Agency Gaming Configuration - ScriptableObject for player choice and automation balance
    /// Configures the balance between manual control and automation systems
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerAgencyGamingConfig", menuName = "Project Chimera/Cultivation/Player Agency Config")]
    public class PlayerAgencyGamingConfigSO : ScriptableObject
    {
        [Header("Agency Balance Settings")]
        [Range(0f, 1f)] public float DefaultAgencyLevel = 0.7f;
        [Range(0f, 1f)] public float MinimumAgencyLevel = 0.3f;
        [Range(0f, 1f)] public float MaximumAgencyLevel = 1.0f;
        [Range(0.1f, 2f)] public float AgencyTransitionSpeed = 1.0f;
        
        [Header("Manual Control Settings")]
        [Range(0.5f, 3f)] public float ManualControlRewardMultiplier = 1.5f;
        [Range(0.1f, 2f)] public float ManualTaskComplexity = 1.2f;
        [Range(0.5f, 5f)] public float ManualSkillGainMultiplier = 2.0f;
        [Range(0.1f, 1f)] public float ManualPrecisionRequirement = 0.8f;
        
        [Header("Automation Settings")]
        [Range(0.1f, 2f)] public float AutomationEfficiencyMultiplier = 1.3f;
        [Range(0.5f, 3f)] public float AutomationConsistencyBonus = 1.8f;
        [Range(0.1f, 1f)] public float AutomationSkillGainMultiplier = 0.6f;
        [Range(1f, 100f)] public float AutomationUnlockCost = 50f;
        
        [Header("Hybrid Control Settings")]
        [Range(0.1f, 2f)] public float HybridControlBonus = 1.2f;
        [Range(0.5f, 2f)] public float HybridComplexityMultiplier = 1.1f;
        [Range(0.8f, 2f)] public float HybridSkillGainMultiplier = 1.3f;
        public bool AllowHybridControl = true;
        
        [Header("Player Choice Framework")]
        public bool EnableChoiceConsequences = true;
        public bool EnableAgencyFeedback = true;
        public bool EnableAutomationToggle = true;
        [Range(1f, 10f)] public float ChoiceImpactDuration = 5f; // hours
        
        [Header("Progression Gates")]
        [SerializeField] private AgencyGate[] _agencyGates = new AgencyGate[]
        {
            new AgencyGate { RequiredSkillLevel = 10, UnlockedFeature = "Basic Automation", AgencyImpact = 0.1f },
            new AgencyGate { RequiredSkillLevel = 25, UnlockedFeature = "Advanced Automation", AgencyImpact = 0.2f },
            new AgencyGate { RequiredSkillLevel = 50, UnlockedFeature = "Full Automation", AgencyImpact = 0.3f }
        };
        
        [Header("Feedback Systems")]
        [Range(0.1f, 2f)] public float FeedbackIntensity = 1.0f;
        [Range(0.5f, 5f)] public float FeedbackDuration = 2.0f;
        public bool EnableVisualFeedback = true;
        public bool EnableAudioFeedback = true;
        public bool EnableHapticFeedback = false;
        
        // Public Properties
        public AgencyGate[] AgencyGates => _agencyGates;
        
        /// <summary>
        /// Calculate the effective agency level based on player choices and progression
        /// </summary>
        public float CalculateEffectiveAgency(float baseAgency, float playerSkillLevel, bool hasAutomation)
        {
            var effectiveAgency = Mathf.Clamp(baseAgency, MinimumAgencyLevel, MaximumAgencyLevel);
            
            // Apply skill-based modifiers
            var skillModifier = playerSkillLevel / 100f * 0.2f;
            effectiveAgency += skillModifier;
            
            // Apply automation impact
            if (hasAutomation)
            {
                var automationImpact = GetAutomationImpact(playerSkillLevel);
                effectiveAgency -= automationImpact;
            }
            
            return Mathf.Clamp(effectiveAgency, MinimumAgencyLevel, MaximumAgencyLevel);
        }
        
        /// <summary>
        /// Get the automation impact based on skill level
        /// </summary>
        public float GetAutomationImpact(float skillLevel)
        {
            foreach (var gate in _agencyGates)
            {
                if (skillLevel >= gate.RequiredSkillLevel)
                    return gate.AgencyImpact;
            }
            return 0f;
        }
        
        /// <summary>
        /// Calculate reward multiplier based on control method
        /// </summary>
        public float GetRewardMultiplier(ControlMethod method, float agencyLevel)
        {
            return method switch
            {
                ControlMethod.Manual => ManualControlRewardMultiplier * agencyLevel,
                ControlMethod.Automated => AutomationEfficiencyMultiplier * (1f - agencyLevel * 0.5f),
                ControlMethod.Hybrid => HybridControlBonus * (agencyLevel * 0.7f + 0.3f),
                _ => 1f
            };
        }
        
        /// <summary>
        /// Calculate skill gain multiplier based on control method
        /// </summary>
        public float GetSkillGainMultiplier(ControlMethod method, float agencyLevel)
        {
            return method switch
            {
                ControlMethod.Manual => ManualSkillGainMultiplier * agencyLevel,
                ControlMethod.Automated => AutomationSkillGainMultiplier,
                ControlMethod.Hybrid => HybridSkillGainMultiplier * (agencyLevel * 0.8f + 0.2f),
                _ => 1f
            };
        }
        
        /// <summary>
        /// Check if a feature is unlocked at the given skill level
        /// </summary>
        public bool IsFeatureUnlocked(float skillLevel, string featureName)
        {
            foreach (var gate in _agencyGates)
            {
                if (gate.UnlockedFeature == featureName)
                    return skillLevel >= gate.RequiredSkillLevel;
            }
            return false;
        }
        
        /// <summary>
        /// Get the next unlock threshold for progression feedback
        /// </summary>
        public AgencyGate GetNextUnlock(float currentSkillLevel)
        {
            foreach (var gate in _agencyGates)
            {
                if (currentSkillLevel < gate.RequiredSkillLevel)
                    return gate;
            }
            return null; // All unlocked
        }
    }
    
    [System.Serializable]
    public class AgencyGate
    {
        [Range(1, 100)] public int RequiredSkillLevel;
        public string UnlockedFeature;
        [Range(0f, 1f)] public float AgencyImpact;
        public string Description;
    }
    
    public enum ControlMethod
    {
        Manual,
        Automated, 
        Hybrid
    }
    
    public enum AgencyLevel
    {
        Low,      // Heavy automation
        Medium,   // Balanced
        High,     // Mostly manual
        Full      // Complete manual control
    }
}