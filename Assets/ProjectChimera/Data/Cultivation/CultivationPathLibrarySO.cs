using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Simple cultivation path library for path management
    /// </summary>
    [CreateAssetMenu(fileName = "CultivationPathLibrary", menuName = "Project Chimera/Cultivation/Cultivation Path Library")]
    public class CultivationPathLibrarySO : ChimeraScriptableObjectSO
    {
        [Header("Available Paths")]
        [SerializeField] private List<CultivationPathData> _availablePaths = new List<CultivationPathData>();
        
        public List<CultivationPathData> AvailablePaths => _availablePaths;
        
        public CultivationPathData GetPath(string pathId)
        {
            return _availablePaths.Find(p => p.PathId == pathId);
        }
        
        public List<CultivationPathData> GetPathsBySpecialization(CultivationSpecialization specialization)
        {
            return _availablePaths.FindAll(p => p.Specialization == specialization);
        }
        
        public override bool ValidateData()
        {
            bool isValid = base.ValidateData();
            
            if (_availablePaths == null || _availablePaths.Count == 0)
            {
                Debug.LogWarning($"CultivationPathLibrarySO '{name}' has no available paths.", this);
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Get path data for a specific approach - used by PlayerAgencyGamingSystem
        /// </summary>
        public CultivationPathData GetPathData(CultivationApproach approach)
        {
            return _availablePaths.Find(p => p.Approach == approach);
        }
        
        /// <summary>
        /// Get path effects for a specific approach - used by PlayerAgencyGamingSystem
        /// </summary>
        public CultivationPathEffects GetPathEffects(CultivationApproach approach)
        {
            var pathData = GetPathData(approach);
            if (pathData == null) return null;
            
            return new CultivationPathEffects
            {
                EffectId = $"path_effect_{approach}",
                Approach = approach,
                StatModifiers = pathData.PathModifiers ?? new Dictionary<string, float>(),
                FeatureUnlocks = pathData.UnlockedFeatures?.ToDictionary(f => f, _ => true) ?? new Dictionary<string, bool>(),
                Duration = 0f, // Permanent
                IsActive = false,
                ActivationTime = DateTime.Now
            };
        }
    }
    
    /// <summary>
    /// Missing SynergyEffect class for cultivation synergies
    /// </summary>
    [System.Serializable]
    public class SynergyEffect
    {
        public string EffectName;
        public SynergyEffectType EffectType;
        [Range(0.1f, 5f)] public float EffectValue = 1f;
        public string Description;
    }
    
    /// <summary>
    /// Types of synergy effects
    /// </summary>
    public enum SynergyEffectType
    {
        YieldBonus,
        QualityBonus,
        GrowthSpeedBonus,
        ResourceReduction,
        SkillBonus,
        ExperienceBonus
    }
}