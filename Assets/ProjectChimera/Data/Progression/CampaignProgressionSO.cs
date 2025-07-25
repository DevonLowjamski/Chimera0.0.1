using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Progression
{
    /// <summary>
    /// ScriptableObject containing campaign progression configuration and milestones
    /// </summary>
    [CreateAssetMenu(fileName = "CampaignProgression", menuName = "Project Chimera/Progression/Campaign Progression")]
    public class CampaignProgressionSO : ScriptableObject
    {
        [Header("Campaign Configuration")]
        [SerializeField] private string _campaignId = "main_campaign";
        [SerializeField] private string _campaignName = "Cannabis Empire";
        [SerializeField] private string _campaignDescription = "Build your cannabis cultivation empire from seed to success";
        
        [Header("Campaign Phases")]
        [SerializeField] private List<CampaignPhaseData> _campaignPhases = new List<CampaignPhaseData>();
        
        [Header("Campaign Milestones")]
        [SerializeField] private List<CampaignMilestone> _campaignMilestones = new List<CampaignMilestone>();
        
        [Header("Campaign Settings")]
        [SerializeField] private bool _enablePhaseProgression = true;
        [SerializeField] private bool _enableMilestoneRewards = true;
        [SerializeField] private float _progressionSpeedMultiplier = 1f;
        
        // Properties
        public string CampaignId => _campaignId;
        public string CampaignName => _campaignName;
        public string CampaignDescription => _campaignDescription;
        public bool EnablePhaseProgression => _enablePhaseProgression;
        public bool EnableMilestoneRewards => _enableMilestoneRewards;
        public float ProgressionSpeedMultiplier => _progressionSpeedMultiplier;
        
        /// <summary>
        /// Get all campaign phases
        /// </summary>
        public List<CampaignPhaseData> GetAllPhases()
        {
            return new List<CampaignPhaseData>(_campaignPhases);
        }
        
        /// <summary>
        /// Get campaign phase by type
        /// </summary>
        public CampaignPhaseData GetPhase(CampaignPhase phase)
        {
            return _campaignPhases.FirstOrDefault(p => p.Phase == phase);
        }
        
        /// <summary>
        /// Get next campaign phase
        /// </summary>
        public CampaignPhaseData GetNextPhase(CampaignPhase currentPhase)
        {
            var currentIndex = _campaignPhases.FindIndex(p => p.Phase == currentPhase);
            if (currentIndex >= 0 && currentIndex < _campaignPhases.Count - 1)
            {
                return _campaignPhases[currentIndex + 1];
            }
            return null;
        }
        
        /// <summary>
        /// Get all campaign milestones
        /// </summary>
        public List<CampaignMilestone> GetAllMilestones()
        {
            return new List<CampaignMilestone>(_campaignMilestones);
        }
        
        /// <summary>
        /// Get milestones for a specific phase
        /// </summary>
        public List<CampaignMilestone> GetMilestonesForPhase(CampaignPhase phase)
        {
            return _campaignMilestones.Where(m => m.RequiredPhase == phase).ToList();
        }
        
        /// <summary>
        /// Get milestone by ID
        /// </summary>
        public CampaignMilestone GetMilestone(string milestoneId)
        {
            return _campaignMilestones.FirstOrDefault(m => m.MilestoneId == milestoneId);
        }
        
        /// <summary>
        /// Get available milestones (requirements met)
        /// </summary>
        public List<CampaignMilestone> GetAvailableMilestones(CampaignPhase currentPhase, PlayerProgressionData playerData)
        {
            var availableMilestones = new List<CampaignMilestone>();
            
            foreach (var milestone in _campaignMilestones)
            {
                if (IsMilestoneAvailable(milestone, currentPhase, playerData))
                {
                    availableMilestones.Add(milestone);
                }
            }
            
            return availableMilestones;
        }
        
        /// <summary>
        /// Check if milestone is available
        /// </summary>
        public bool IsMilestoneAvailable(CampaignMilestone milestone, CampaignPhase currentPhase, PlayerProgressionData playerData)
        {
            // Check phase requirement
            if (milestone.RequiredPhase > currentPhase)
            {
                return false;
            }
            
            // Check if already completed
            if (playerData.CampaignData.ReachedMilestones.Contains(milestone.MilestoneId))
            {
                return false;
            }
            
            // Check milestone requirements
            foreach (var requirement in milestone.Requirements)
            {
                if (!IsMilestoneRequirementMet(requirement, playerData))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if milestone requirement is met
        /// </summary>
        public bool IsMilestoneRequirementMet(MilestoneRequirement requirement, PlayerProgressionData playerData)
        {
            switch (requirement.RequirementType)
            {
                case MilestoneRequirementType.Player_Level:
                    return playerData.PlayerLevel >= requirement.RequiredValue;
                    
                case MilestoneRequirementType.Skill_Level:
                    return playerData.SkillLevels.ContainsKey(requirement.TargetId) && 
                           playerData.SkillLevels[requirement.TargetId] >= requirement.RequiredValue;
                           
                case MilestoneRequirementType.Research_Completion:
                    return playerData.CompletedResearch.Contains(requirement.TargetId);
                    
                case MilestoneRequirementType.Achievement_Unlock:
                    return playerData.CompletedAchievements.Contains(requirement.TargetId);
                    
                case MilestoneRequirementType.Facility_Construction:
                    return playerData.StatTracker.FacilitiesBuilt >= requirement.RequiredValue;
                    
                case MilestoneRequirementType.Profit_Generation:
                    return playerData.StatTracker.TotalProfit >= requirement.RequiredValue;
                    
                case MilestoneRequirementType.Quality_Achievement:
                    return playerData.StatTracker.BestQualityAchieved >= requirement.RequiredValue;
                    
                case MilestoneRequirementType.Time_Played:
                    return playerData.PlayTimeHours >= requirement.RequiredValue;
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Get phase progression requirements
        /// </summary>
        public List<PhaseRequirement> GetPhaseRequirements(CampaignPhase phase)
        {
            var phaseData = GetPhase(phase);
            return phaseData?.Requirements ?? new List<PhaseRequirement>();
        }
        
        /// <summary>
        /// Check if phase requirements are met
        /// </summary>
        public bool ArePhaseRequirementsMet(CampaignPhase phase, PlayerProgressionData playerData)
        {
            var requirements = GetPhaseRequirements(phase);
            
            foreach (var requirement in requirements)
            {
                if (!IsPhaseRequirementMet(requirement, playerData))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if phase requirement is met
        /// </summary>
        public bool IsPhaseRequirementMet(PhaseRequirement requirement, PlayerProgressionData playerData)
        {
            switch (requirement.RequirementType)
            {
                case PhaseRequirementType.Player_Level:
                    return playerData.PlayerLevel >= requirement.RequiredValue;
                    
                case PhaseRequirementType.Milestones_Completed:
                    return playerData.CampaignData.ReachedMilestones.Count >= requirement.RequiredValue;
                    
                case PhaseRequirementType.Skills_Unlocked:
                    return playerData.UnlockedSkills.Count >= requirement.RequiredValue;
                    
                case PhaseRequirementType.Research_Completed:
                    return playerData.CompletedResearch.Count >= requirement.RequiredValue;
                    
                case PhaseRequirementType.Achievements_Unlocked:
                    return playerData.CompletedAchievements.Count >= requirement.RequiredValue;
                    
                case PhaseRequirementType.Total_Experience:
                    return playerData.TotalExperience >= requirement.RequiredValue;
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Get campaign progress percentage
        /// </summary>
        public float GetCampaignProgress(PlayerProgressionData playerData)
        {
            var totalMilestones = _campaignMilestones.Count;
            var completedMilestones = playerData.CampaignData.ReachedMilestones.Count;
            
            if (totalMilestones == 0) return 100f;
            
            return (float)completedMilestones / totalMilestones * 100f;
        }
        
        /// <summary>
        /// Get phase progress percentage
        /// </summary>
        public float GetPhaseProgress(CampaignPhase phase, PlayerProgressionData playerData)
        {
            var phaseMilestones = GetMilestonesForPhase(phase);
            if (phaseMilestones.Count == 0) return 100f;
            
            var completedInPhase = phaseMilestones.Count(m => 
                playerData.CampaignData.ReachedMilestones.Contains(m.MilestoneId));
                
            return (float)completedInPhase / phaseMilestones.Count * 100f;
        }
        
        /// <summary>
        /// Validate campaign configuration
        /// </summary>
        public bool ValidateCampaign()
        {
            // Check for duplicate milestone IDs
            var milestoneIds = _campaignMilestones.Select(m => m.MilestoneId).ToList();
            if (milestoneIds.Count != milestoneIds.Distinct().Count())
            {
                Debug.LogError("Campaign contains duplicate milestone IDs");
                return false;
            }
            
            // Check phase order
            for (int i = 0; i < _campaignPhases.Count - 1; i++)
            {
                if ((int)_campaignPhases[i].Phase >= (int)_campaignPhases[i + 1].Phase)
                {
                    Debug.LogError("Campaign phases are not in correct order");
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get campaign statistics
        /// </summary>
        public CampaignStats GetCampaignStats()
        {
            return new CampaignStats
            {
                TotalPhases = _campaignPhases.Count,
                TotalMilestones = _campaignMilestones.Count,
                MilestonesByPhase = System.Enum.GetValues(typeof(CampaignPhase))
                    .Cast<CampaignPhase>()
                    .ToDictionary(phase => phase, phase => GetMilestonesForPhase(phase).Count),
                EstimatedCompletionHours = _campaignMilestones.Sum(m => m.EstimatedDurationHours)
            };
        }
    }
    
    /// <summary>
    /// Campaign phase data
    /// </summary>
    [System.Serializable]
    public class CampaignPhaseData
    {
        public CampaignPhase Phase;
        public string PhaseName;
        public string PhaseDescription;
        public List<PhaseRequirement> Requirements = new List<PhaseRequirement>();
        public List<string> UnlockedFeatures = new List<string>();
        public float EstimatedDurationHours;
    }
    
    /// <summary>
    /// Phase requirement
    /// </summary>
    [System.Serializable]
    public class PhaseRequirement
    {
        public PhaseRequirementType RequirementType;
        public float RequiredValue;
        public string Description;
    }
    
    /// <summary>
    /// Campaign statistics
    /// </summary>
    [System.Serializable]
    public class CampaignStats
    {
        public int TotalPhases;
        public int TotalMilestones;
        public Dictionary<CampaignPhase, int> MilestonesByPhase = new Dictionary<CampaignPhase, int>();
        public float EstimatedCompletionHours;
    }
    
    /// <summary>
    /// Phase requirement types
    /// </summary>
    public enum PhaseRequirementType
    {
        Player_Level,
        Milestones_Completed,
        Skills_Unlocked,
        Research_Completed,
        Achievements_Unlocked,
        Total_Experience
    }
} 