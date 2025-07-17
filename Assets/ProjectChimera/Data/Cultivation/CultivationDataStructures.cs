using UnityEngine;
using System.Collections.Generic;
using System;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Additional data structures for cultivation gaming systems
    /// Contains missing types referenced by various cultivation managers
    /// Only includes types that don't already exist elsewhere
    /// </summary>
    
    /// <summary>
    /// Facility design data for player agency gaming
    /// </summary>
    [System.Serializable]
    public class FacilityDesignData
    {
        public string DesignId;
        public string DesignName;
        public FacilityDesignApproach Approach;
        public Dictionary<string, float> EfficiencyMetrics = new Dictionary<string, float>();
        public Dictionary<string, object> DesignParameters = new Dictionary<string, object>();
        public float CostMultiplier = 1f;
        public float ProductionMultiplier = 1f;
        public float QualityModifier = 1f;
        public bool IsUnlocked = false;
        public DateTime UnlockDate;
        
        public FacilityDesignData()
        {
            DesignId = Guid.NewGuid().ToString();
            UnlockDate = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Cultivation path effects for progression gaming
    /// </summary>
    [System.Serializable]
    public class CultivationPathEffects
    {
        public string EffectId;
        public string EffectName;
        public CultivationApproach Approach;
        public Dictionary<string, float> StatModifiers = new Dictionary<string, float>();
        public Dictionary<string, bool> FeatureUnlocks = new Dictionary<string, bool>();
        public float Duration = 0f; // 0 = permanent
        public bool IsActive = false;
        public DateTime ActivationTime;
        
        public CultivationPathEffects()
        {
            EffectId = Guid.NewGuid().ToString();
        }
        
        public void Activate()
        {
            IsActive = true;
            ActivationTime = DateTime.Now;
        }
        
        public void Deactivate()
        {
            IsActive = false;
        }
        
        public bool IsExpired()
        {
            if (Duration <= 0f) return false; // Permanent effects never expire
            return (DateTime.Now - ActivationTime).TotalSeconds > Duration;
        }
    }
    
    /// <summary>
    /// Cultivation approaches for player choice
    /// </summary>
    public enum CultivationApproach
    {
        OrganicTraditional,
        HydroponicPrecision,
        AeroponicCutting,
        BiodynamicHolistic,
        TechnologicalAutomated,
        ExperimentalInnovative,
        EconomicOptimized
    }
    
    /// <summary>
    /// Facility design approaches for player choice
    /// </summary>
    public enum FacilityDesignApproach
    {
        MinimalistEfficient,
        CreativeInnovative,
        ModularExpandable,
        AestheticShowcase,
        BudgetOptimized,
        TechnologicalCutting,
        SustainableEcological
    }
    
    /// <summary>
    /// Facility design effects for player agency gaming
    /// </summary>
    [System.Serializable]
    public class FacilityDesignEffects
    {
        public string EffectId;
        public FacilityDesignApproach Approach;
        public Dictionary<string, float> EfficiencyModifiers = new Dictionary<string, float>();
        public Dictionary<string, bool> DesignFeatures = new Dictionary<string, bool>();
        public float Duration = 0f; // 0 = permanent
        public bool IsActive = false;
        public float ActivationTime;
        
        public FacilityDesignEffects()
        {
            EffectId = System.Guid.NewGuid().ToString();
        }
    }
    
    /// <summary>
    /// Alias for SkillBranch to maintain compatibility
    /// </summary>
    public enum SkillTreeBranch
    {
        Cultivation = SkillBranch.Cultivation,
        Automation = SkillBranch.Automation,
        Science = SkillBranch.Science,
        Business = SkillBranch.Business,
        Genetics = SkillBranch.Genetics,
        Processing = SkillBranch.Processing
    }
}