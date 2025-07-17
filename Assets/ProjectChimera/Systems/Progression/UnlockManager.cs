using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Events;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Manages content unlocking based on progression requirements
    /// </summary>
    public class UnlockManager
    {
        private UnlockConfigSO _unlockConfig;
        private HashSet<string> _unlockedContent = new HashSet<string>();
        private PlayerProgressionData _playerProgression;
        
        // Events
        public System.Action<string> OnContentUnlocked;
        public System.Action<string> OnContentLocked;
        
        public UnlockManager(UnlockConfigSO unlockConfig)
        {
            _unlockConfig = unlockConfig;
        }
        
        public void SetPlayerProgression(PlayerProgressionData playerProgression)
        {
            _playerProgression = playerProgression;
            LoadUnlockedContent();
        }
        
        private void LoadUnlockedContent()
        {
            if (_playerProgression != null)
            {
                _unlockedContent.Clear();
                foreach (var contentId in _playerProgression.UnlockedContent)
                {
                    _unlockedContent.Add(contentId);
                }
            }
        }
        
        public void CheckUnlocks()
        {
            if (_unlockConfig == null || _playerProgression == null) return;
            
            var allUnlockables = _unlockConfig.GetAllUnlockables();
            
            foreach (var unlockable in allUnlockables)
            {
                if (!_unlockedContent.Contains(unlockable.ContentId))
                {
                    if (AreRequirementsMet(unlockable.Requirements))
                    {
                        UnlockContent(unlockable.ContentId);
                    }
                }
            }
        }
        
        private bool AreRequirementsMet(List<UnlockRequirement> requirements)
        {
            if (requirements == null || requirements.Count == 0) return true;
            
            foreach (var requirement in requirements)
            {
                if (!IsRequirementMet(requirement))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private bool IsRequirementMet(UnlockRequirement requirement)
        {
            switch (requirement.RequirementType)
            {
                case UnlockRequirementType.PlayerLevel:
                    return _playerProgression.OverallLevel >= requirement.RequiredValue;
                    
                case UnlockRequirementType.SkillLevel:
                    if (_playerProgression.SkillLevels.TryGetValue(requirement.TargetId, out int skillLevel))
                    {
                        return skillLevel >= requirement.RequiredValue;
                    }
                    return false;
                    
                case UnlockRequirementType.AchievementCompleted:
                    return _playerProgression.CompletedAchievements.Contains(requirement.TargetId);
                    
                case UnlockRequirementType.ResearchCompleted:
                    return _playerProgression.CompletedResearch.Contains(requirement.TargetId);
                    
                case UnlockRequirementType.TimeInGame:
                    return _playerProgression.PlayTimeHours >= requirement.RequiredValue;
                    
                case UnlockRequirementType.StatThreshold:
                    return _playerProgression.StatTracker.TotalProfit >= requirement.RequiredValue;
                    
                case UnlockRequirementType.ContentUnlocked:
                    return _unlockedContent.Contains(requirement.TargetId);
                    
                case UnlockRequirementType.CampaignProgress:
                    return _playerProgression.CampaignProgress != null && 
                           _playerProgression.CampaignProgress.CampaignProgress >= requirement.RequiredValue;
                    
                default:
                    return false;
            }
        }
        
        public void UnlockContent(string contentId)
        {
            if (!_unlockedContent.Contains(contentId))
            {
                _unlockedContent.Add(contentId);
                _playerProgression.UnlockedContent.Add(contentId);
                
                OnContentUnlocked?.Invoke(contentId);
            }
        }
        
        public void LockContent(string contentId)
        {
            if (_unlockedContent.Contains(contentId))
            {
                _unlockedContent.Remove(contentId);
                _playerProgression.UnlockedContent.Remove(contentId);
                
                OnContentLocked?.Invoke(contentId);
            }
        }
        
        public bool IsContentUnlocked(string contentId)
        {
            return _unlockedContent.Contains(contentId);
        }
        
        public List<string> GetUnlockedContent()
        {
            return new List<string>(_unlockedContent);
        }
        
        public List<string> GetLockedContent()
        {
            if (_unlockConfig == null) return new List<string>();
            
            var allContent = _unlockConfig.GetAllUnlockables().Select(u => u.ContentId);
            return allContent.Where(c => !_unlockedContent.Contains(c)).ToList();
        }
        
        public List<UnlockableContent> GetAvailableUnlocks()
        {
            if (_unlockConfig == null) return new List<UnlockableContent>();
            
            return _unlockConfig.GetAllUnlockables()
                .Where(u => !_unlockedContent.Contains(u.ContentId) && 
                           AreRequirementsMet(u.Requirements))
                .ToList();
        }
        
        public List<UnlockRequirement> GetMissingRequirements(string contentId)
        {
            if (_unlockConfig == null) return new List<UnlockRequirement>();
            
            var unlockable = _unlockConfig.GetAllUnlockables()
                .FirstOrDefault(u => u.ContentId == contentId);
                
            if (unlockable == null) return new List<UnlockRequirement>();
            
            return unlockable.Requirements
                .Where(r => !IsRequirementMet(r))
                .ToList();
        }
        
        public float GetUnlockProgress(string contentId)
        {
            if (_unlockConfig == null) return 0f;
            
            var unlockable = _unlockConfig.GetAllUnlockables()
                .FirstOrDefault(u => u.ContentId == contentId);
                
            if (unlockable == null) return 0f;
            if (_unlockedContent.Contains(contentId)) return 1f;
            
            if (unlockable.Requirements == null || unlockable.Requirements.Count == 0) return 1f;
            
            float totalProgress = 0f;
            foreach (var requirement in unlockable.Requirements)
            {
                totalProgress += GetRequirementProgress(requirement);
            }
            
            return totalProgress / unlockable.Requirements.Count;
        }
        
        private float GetRequirementProgress(UnlockRequirement requirement)
        {
            switch (requirement.RequirementType)
            {
                case UnlockRequirementType.PlayerLevel:
                    return Mathf.Clamp01(_playerProgression.OverallLevel / requirement.RequiredValue);
                    
                case UnlockRequirementType.SkillLevel:
                    if (_playerProgression.SkillLevels.TryGetValue(requirement.TargetId, out int skillLevel))
                    {
                        return Mathf.Clamp01(skillLevel / requirement.RequiredValue);
                    }
                    return 0f;
                    
                case UnlockRequirementType.TimeInGame:
                    return Mathf.Clamp01(_playerProgression.PlayTimeHours / requirement.RequiredValue);
                    
                case UnlockRequirementType.StatThreshold:
                    return Mathf.Clamp01(_playerProgression.StatTracker.TotalProfit / requirement.RequiredValue);
                    
                case UnlockRequirementType.CampaignProgress:
                    if (_playerProgression.CampaignProgress != null)
                    {
                        return Mathf.Clamp01(_playerProgression.CampaignProgress.CampaignProgress / requirement.RequiredValue);
                    }
                    return 0f;
                    
                case UnlockRequirementType.AchievementCompleted:
                case UnlockRequirementType.ResearchCompleted:
                case UnlockRequirementType.ContentUnlocked:
                    return IsRequirementMet(requirement) ? 1f : 0f;
                    
                default:
                    return 0f;
            }
        }
        
        public List<string> GetUnlocksForSkillLevel(string skillId, int level)
        {
            var unlocks = new List<string>();
            
            if (_unlockConfig == null) return unlocks;
            
            var allUnlockables = _unlockConfig.GetAllUnlockables();
            
            foreach (var unlockable in allUnlockables)
            {
                foreach (var requirement in unlockable.Requirements)
                {
                    if (requirement.RequirementType == UnlockRequirementType.SkillLevel &&
                        requirement.TargetId == skillId &&
                        requirement.RequiredValue == level)
                    {
                        unlocks.Add(unlockable.ContentId);
                        break;
                    }
                }
            }
            
            return unlocks;
        }
        
        public void Update()
        {
            CheckUnlocks();
        }
        
        public void Cleanup()
        {
            _unlockedContent.Clear();
        }
    }
} 