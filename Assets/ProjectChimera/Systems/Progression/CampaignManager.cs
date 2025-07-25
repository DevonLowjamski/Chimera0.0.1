using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Manages campaign progression, phases, and story content
    /// </summary>
    public class CampaignManager
    {
        private CampaignConfigSO _campaignConfig;
        private CampaignProgressionData _campaignProgress;
        private PlayerProgressionData _playerProgression;
        
        // Events
        public System.Action<CampaignPhase> OnPhaseChanged;
        public System.Action<string> OnMilestoneReached;
        
        public CampaignPhase CurrentPhase => _campaignProgress?.CurrentPhase ?? CampaignPhase.Tutorial;
        public float PhaseProgress => _campaignProgress?.PhaseProgress ?? 0f;
        
        public CampaignManager(CampaignConfigSO campaignConfig)
        {
            _campaignConfig = campaignConfig;
            _campaignProgress = new CampaignProgressionData();
        }
        
        public void SetPlayerProgression(PlayerProgressionData playerProgression)
        {
            _playerProgression = playerProgression;
            LoadCampaignProgress();
        }
        
        public void InitializeWithData(CampaignProgressionData campaignData)
        {
            if (campaignData != null)
            {
                _campaignProgress = campaignData;
            }
            else
            {
                _campaignProgress = new CampaignProgressionData();
            }
        }
        
        private void LoadCampaignProgress()
        {
            if (_playerProgression != null && _playerProgression.CampaignProgress != null)
            {
                _campaignProgress = _playerProgression.CampaignProgress;
            }
            else
            {
                _campaignProgress = new CampaignProgressionData();
                if (_playerProgression != null)
                {
                    _playerProgression.CampaignProgress = _campaignProgress;
                }
            }
        }
        
        public void UpdateCampaignProgress()
        {
            if (_playerProgression == null || _campaignConfig == null) return;
            
            CampaignPhase newPhase = DetermineCurrentPhase();
            
            if (newPhase != _campaignProgress.CurrentPhase)
            {
                AdvanceToPhase(newPhase);
            }
            
            UpdatePhaseProgress();
            CheckMilestones();
        }
        
        /// <summary>
        /// Compatibility alias for UpdateCampaignProgress
        /// </summary>
        public void UpdateProgress()
        {
            UpdateCampaignProgress();
        }
        
        private CampaignPhase DetermineCurrentPhase()
        {
            if (_playerProgression.PlayTimeHours < 1f)
                return CampaignPhase.Tutorial;
                
            if (_playerProgression.OverallLevel < 10)
                return CampaignPhase.EarlyGrowth;
                
            if (_playerProgression.OverallLevel < 25)
                return CampaignPhase.Expansion;
                
            if (_playerProgression.OverallLevel < 50)
                return CampaignPhase.Specialization;
                
            if (_playerProgression.OverallLevel < 75)
                return CampaignPhase.Innovation;
                
            if (_playerProgression.OverallLevel < 100)
                return CampaignPhase.Mastery;
                
            return CampaignPhase.Legacy;
        }
        
        private void AdvanceToPhase(CampaignPhase newPhase)
        {
            CampaignPhase previousPhase = _campaignProgress.CurrentPhase;
            _campaignProgress.CurrentPhase = newPhase;
            _campaignProgress.PhaseProgress = 0f;
            _campaignProgress.PhaseStartTime = System.DateTime.Now;
            
            // Add to completed phases
            if (!_campaignProgress.CompletedPhases.Contains(previousPhase))
            {
                _campaignProgress.CompletedPhases.Add(previousPhase);
            }
            
            OnPhaseChanged?.Invoke(newPhase);
        }
        
        private void UpdatePhaseProgress()
        {
            float progress = CalculatePhaseProgress(_campaignProgress.CurrentPhase);
            _campaignProgress.PhaseProgress = Mathf.Clamp01(progress);
        }
        
        private float CalculatePhaseProgress(CampaignPhase phase)
        {
            switch (phase)
            {
                case CampaignPhase.Tutorial:
                    return Mathf.Clamp01(_playerProgression.PlayTimeHours / 2f);
                    
                case CampaignPhase.EarlyGrowth:
                    return Mathf.Clamp01((_playerProgression.OverallLevel - 1) / 9f);
                    
                case CampaignPhase.Expansion:
                    return Mathf.Clamp01((_playerProgression.OverallLevel - 10) / 15f);
                    
                case CampaignPhase.Specialization:
                    return Mathf.Clamp01((_playerProgression.OverallLevel - 25) / 25f);
                    
                case CampaignPhase.Innovation:
                    return Mathf.Clamp01((_playerProgression.OverallLevel - 50) / 25f);
                    
                case CampaignPhase.Mastery:
                    return Mathf.Clamp01((_playerProgression.OverallLevel - 75) / 25f);
                    
                case CampaignPhase.Legacy:
                    return 1f;
                    
                default:
                    return 0f;
            }
        }
        
        private void CheckMilestones()
        {
            if (_campaignConfig == null) return;
            
            var availableMilestones = GetAvailableMilestones();
            
            foreach (var milestone in availableMilestones)
            {
                if (!_campaignProgress.ReachedMilestones.Contains(milestone.MilestoneId) &&
                    IsMilestoneReached(milestone))
                {
                    ReachMilestone(milestone);
                }
            }
        }
        
        private List<CampaignMilestone> GetAvailableMilestones()
        {
            if (_campaignConfig?.Milestones == null) return new List<CampaignMilestone>();
            
            return _campaignConfig.Milestones
                .Where(m => m.RequiredPhase <= _campaignProgress.CurrentPhase)
                .ToList();
        }
        
        private bool IsMilestoneReached(CampaignMilestone milestone)
        {
            // Check all requirements for the milestone
            foreach (var requirement in milestone.Requirements)
            {
                if (!IsMilestoneRequirementMet(requirement))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private bool IsMilestoneRequirementMet(MilestoneRequirement requirement)
        {
            switch (requirement.RequirementType)
            {
                case MilestoneRequirementType.SkillLevel:
                    if (_playerProgression.SkillLevels.TryGetValue(requirement.TargetId, out int skillLevel))
                    {
                        return skillLevel >= requirement.RequiredValue;
                    }
                    return false;
                    
                case MilestoneRequirementType.ResearchCompletion:
                    return _playerProgression.CompletedResearch.Contains(requirement.TargetId);
                    
                case MilestoneRequirementType.AchievementUnlock:
                    return _playerProgression.CompletedAchievements.Contains(requirement.TargetId);
                    
                case MilestoneRequirementType.FacilityConstruction:
                    return _playerProgression.StatTracker.FacilitiesBuilt >= requirement.RequiredValue;
                    
                case MilestoneRequirementType.ProfitGeneration:
                    return _playerProgression.StatTracker.TotalProfit >= requirement.RequiredValue;
                    
                case MilestoneRequirementType.QualityThreshold:
                    return _playerProgression.StatTracker.BestQualityAchieved >= requirement.RequiredValue;
                    
                case MilestoneRequirementType.TimeInGame:
                    return _playerProgression.PlayTimeHours >= requirement.RequiredValue;
                    
                case MilestoneRequirementType.ContentUnlock:
                    return _playerProgression.UnlockedContent.Contains(requirement.TargetId);
                    
                default:
                    return false;
            }
        }
        
        private void ReachMilestone(CampaignMilestone milestone)
        {
            _campaignProgress.ReachedMilestones.Add(milestone.MilestoneId);
            
            // Apply milestone rewards
            ApplyMilestoneRewards(milestone);
            
            OnMilestoneReached?.Invoke(milestone.MilestoneId);
        }
        
        private void ApplyMilestoneRewards(CampaignMilestone milestone)
        {
            foreach (var reward in milestone.Rewards)
            {
                switch (reward.RewardType)
                {
                    case MilestoneRewardType.Permanent_Bonus:
                        // Apply permanent bonus
                        _playerProgression.TotalExperience += reward.RewardValue;
                        break;
                        
                    case MilestoneRewardType.Unlock_Feature:
                        if (!string.IsNullOrEmpty(reward.UnlockedFeature) && 
                            !_playerProgression.UnlockedContent.Contains(reward.UnlockedFeature))
                        {
                            _playerProgression.UnlockedContent.Add(reward.UnlockedFeature);
                        }
                        break;
                        
                    case MilestoneRewardType.Unlock_Skill:
                        if (reward.UnlockedSkillNode != null && 
                            !_playerProgression.UnlockedSkills.Contains(reward.UnlockedSkillNode.SkillId))
                        {
                            _playerProgression.UnlockedSkills.Add(reward.UnlockedSkillNode.SkillId);
                        }
                        break;
                        
                    case MilestoneRewardType.Title_Recognition:
                    case MilestoneRewardType.Special_Access:
                    case MilestoneRewardType.Multiplier_Bonus:
                    case MilestoneRewardType.Unique_Opportunity:
                    case MilestoneRewardType.Legacy_Achievement:
                        // Handle other reward types as needed
                        break;
                }
            }
        }
        
        public bool IsPhaseCompleted(CampaignPhase phase)
        {
            return _campaignProgress.CompletedPhases.Contains(phase);
        }
        
        public bool IsMilestoneReached(string milestoneId)
        {
            return _campaignProgress.ReachedMilestones.Contains(milestoneId);
        }
        
        public List<CampaignMilestone> GetCompletedMilestones()
        {
            if (_campaignConfig?.Milestones == null) return new List<CampaignMilestone>();
            
            return _campaignConfig.Milestones
                .Where(m => _campaignProgress.ReachedMilestones.Contains(m.MilestoneId))
                .ToList();
        }
        
        public List<CampaignMilestone> GetAvailableMilestonesForPhase(CampaignPhase phase)
        {
            if (_campaignConfig?.Milestones == null) return new List<CampaignMilestone>();
            
            return _campaignConfig.Milestones
                .Where(m => m.RequiredPhase == phase && !_campaignProgress.ReachedMilestones.Contains(m.MilestoneId))
                .ToList();
        }
        
        public void Update()
        {
            UpdateCampaignProgress();
        }
        
        public void Cleanup()
        {
            _campaignProgress = null;
        }
    }
} 