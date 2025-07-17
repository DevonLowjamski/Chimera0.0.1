using UnityEngine;
using System.Collections.Generic;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// ScriptableObject configuration for campaign progression system
    /// </summary>
    [CreateAssetMenu(fileName = "CampaignConfig", menuName = "Project Chimera/Progression/Campaign Config")]
    public class CampaignConfigSO : ScriptableObject
    {
        [Header("Campaign Phases")]
        [SerializeField] private List<CampaignPhaseConfig> _phases = new List<CampaignPhaseConfig>();
        
        [Header("Milestones")]
        [SerializeField] private List<CampaignMilestone> _milestones = new List<CampaignMilestone>();
        
        [Header("Configuration")]
        [SerializeField] private bool _enableAutomaticPhaseProgression = true;
        [SerializeField] private bool _requireMilestonesForProgression = true;
        [SerializeField] private float _phaseProgressionDelay = 2f;
        
        public List<CampaignPhaseConfig> Phases => _phases;
        public List<CampaignMilestone> Milestones => _milestones;
        public bool EnableAutomaticPhaseProgression => _enableAutomaticPhaseProgression;
        public bool RequireMilestonesForProgression => _requireMilestonesForProgression;
        public float PhaseProgressionDelay => _phaseProgressionDelay;
        
        /// <summary>
        /// Get phase configuration by phase type
        /// </summary>
        public CampaignPhaseConfig GetPhaseConfig(CampaignPhase phase)
        {
            return _phases.Find(p => p.Phase == phase);
        }
        
        /// <summary>
        /// Get milestones for a specific phase
        /// </summary>
        public List<CampaignMilestone> GetMilestonesForPhase(CampaignPhase phase)
        {
            return _milestones.FindAll(m => m.RequiredPhase == phase);
        }
        
        /// <summary>
        /// Get next phase in progression
        /// </summary>
        public CampaignPhase GetNextPhase(CampaignPhase currentPhase)
        {
            int currentIndex = (int)currentPhase;
            int nextIndex = currentIndex + 1;
            
            if (nextIndex < System.Enum.GetValues(typeof(CampaignPhase)).Length)
            {
                return (CampaignPhase)nextIndex;
            }
            
            return currentPhase; // Already at max phase
        }
    }
    
    /// <summary>
    /// Configuration for individual campaign phases
    /// </summary>
    [System.Serializable]
    public class CampaignPhaseConfig
    {
        public CampaignPhase Phase;
        public string PhaseName;
        [TextArea(2, 4)] public string PhaseDescription;
        public Sprite PhaseIcon;
        public int RequiredPlayerLevel = 1;
        public List<string> RequiredAchievements = new List<string>();
        public List<string> RequiredResearch = new List<string>();
        public List<string> UnlockedFeatures = new List<string>();
        public float EstimatedDurationDays = 30f;
    }
} 