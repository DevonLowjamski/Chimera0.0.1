using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Cultivation
{
    /// <summary>
    /// Library of facility design templates and effects
    /// </summary>
    [CreateAssetMenu(fileName = "FacilityDesignLibrary", menuName = "Project Chimera/Cultivation/Facility Design Library")]
    public class FacilityDesignLibrarySO : ChimeraScriptableObject
    {
        [Header("Design Library")]
        [SerializeField] private List<FacilityDesignData> _availableDesigns = new List<FacilityDesignData>();
        
        public List<FacilityDesignData> AvailableDesigns => _availableDesigns;
        
        public FacilityDesignData GetDesign(string designId)
        {
            return _availableDesigns.Find(d => d.DesignId == designId);
        }
        
        public List<FacilityDesignData> GetDesignsByApproach(FacilityDesignApproach approach)
        {
            return _availableDesigns.FindAll(d => d.Approach == approach);
        }
        
        /// <summary>
        /// Get design data for a specific approach - used by PlayerAgencyGamingSystem
        /// </summary>
        public FacilityDesignData GetDesignData(FacilityDesignApproach approach)
        {
            return _availableDesigns.Find(d => d.Approach == approach);
        }
        
        /// <summary>
        /// Get design effects for a specific approach - used by PlayerAgencyGamingSystem
        /// </summary>
        public FacilityDesignEffects GetDesignEffects(FacilityDesignApproach approach)
        {
            var designData = GetDesignData(approach);
            if (designData == null) return null;
            
            return new FacilityDesignEffects
            {
                EffectId = $"design_effect_{approach}",
                Approach = approach,
                EfficiencyModifiers = designData.EfficiencyMetrics ?? new Dictionary<string, float>(),
                DesignFeatures = designData.DesignParameters?.ToDictionary(p => p.Key, p => true) ?? new Dictionary<string, bool>(),
                Duration = 0f, // Permanent
                IsActive = false,
                ActivationTime = 0f
            };
        }
    }
}