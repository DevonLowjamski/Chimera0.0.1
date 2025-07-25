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
    /// Manages achievement tracking, validation, and unlocking
    /// </summary>
    public class AchievementManager
    {
        private AchievementLibrarySO _achievementLibrary;
        private Dictionary<string, float> _achievementProgress = new Dictionary<string, float>();
        private HashSet<string> _completedAchievements = new HashSet<string>();
        private PlayerProgressionData _playerProgression;
        
        // Events
        public System.Action<AchievementDefinition> OnAchievementUnlocked;
        public System.Action<AchievementDefinition, float> OnAchievementProgressUpdated;
        
        public AchievementManager(AchievementLibrarySO achievementLibrary)
        {
            _achievementLibrary = achievementLibrary;
            InitializeAchievements();
        }
        
        private void InitializeAchievements()
        {
            if (_achievementLibrary != null)
            {
                var allAchievements = _achievementLibrary.GetAllAchievements();
                foreach (var achievement in allAchievements)
                {
                    if (!_achievementProgress.ContainsKey(achievement.AchievementId))
                    {
                        _achievementProgress[achievement.AchievementId] = 0f;
                    }
                }
            }
        }
        
        public void SetPlayerProgression(PlayerProgressionData playerProgression)
        {
            _playerProgression = playerProgression;
            LoadCompletedAchievements();
        }
        
        private void LoadCompletedAchievements()
        {
            if (_playerProgression != null)
            {
                _completedAchievements.Clear();
                foreach (var achievementId in _playerProgression.CompletedAchievements)
                {
                    _completedAchievements.Add(achievementId);
                }
            }
        }
        
        public void UpdateAchievementProgress()
        {
            if (_achievementLibrary == null || _playerProgression == null) return;
            
            var allAchievements = _achievementLibrary.GetAllAchievements();
            foreach (var achievement in allAchievements)
            {
                if (!_completedAchievements.Contains(achievement.AchievementId))
                {
                    CheckAchievementProgress(achievement);
                }
            }
        }
        
        private void CheckAchievementProgress(AchievementDefinition achievement)
        {
            float progress = CalculateAchievementProgress(achievement);
            string achievementId = achievement.AchievementId;
            
            // Update progress
            if (!_achievementProgress.ContainsKey(achievementId) || 
                _achievementProgress[achievementId] != progress)
            {
                _achievementProgress[achievementId] = progress;
                OnAchievementProgressUpdated?.Invoke(achievement, progress);
            }
            
            // Check if completed
            if (progress >= 1f && !_completedAchievements.Contains(achievementId))
            {
                UnlockAchievement(achievement);
            }
        }
        
        private float CalculateAchievementProgress(AchievementDefinition achievement)
        {
            if (_playerProgression == null) return 0f;
            
            // AchievementDefinition has a single Requirements object, not a list
            if (achievement.Requirements == null) return 1f; // No requirements means completed
            
            return CalculateRequirementsProgress(achievement.Requirements);
        }
        
        private float CalculateRequirementsProgress(AchievementRequirements requirements)
        {
            float progress = 0f;
            int checkCount = 0;
            
            // Check minimum player level
            if (requirements.MinimumPlayerLevel > 1)
            {
                progress += _playerProgression.PlayerLevel >= requirements.MinimumPlayerLevel ? 1f : 
                           Mathf.Clamp01((float)_playerProgression.PlayerLevel / requirements.MinimumPlayerLevel);
                checkCount++;
            }
            
            // Check required skills
            if (requirements.RequiredSkills.Count > 0)
            {
                int skillsMet = 0;
                foreach (var skill in requirements.RequiredSkills)
                {
                    if (_playerProgression.UnlockedSkills.Contains(skill.SkillId))
                    {
                        skillsMet++;
                    }
                }
                progress += (float)skillsMet / requirements.RequiredSkills.Count;
                checkCount++;
            }
            
            // Check required achievements
            if (requirements.RequiredAchievements.Count > 0)
            {
                int achievementsMet = 0;
                foreach (var achievementId in requirements.RequiredAchievements)
                {
                    if (_playerProgression.CompletedAchievements.Contains(achievementId))
                    {
                        achievementsMet++;
                    }
                }
                progress += (float)achievementsMet / requirements.RequiredAchievements.Count;
                checkCount++;
            }
            
            // Check required research
            if (requirements.RequiredResearch.Count > 0)
            {
                int researchMet = 0;
                foreach (var research in requirements.RequiredResearch)
                {
                    if (_playerProgression.CompletedResearch.Contains(research.ProjectId))
                    {
                        researchMet++;
                    }
                }
                progress += (float)researchMet / requirements.RequiredResearch.Count;
                checkCount++;
            }
            
            return checkCount > 0 ? progress / checkCount : 1f;
        }
        
        private void UnlockAchievement(AchievementDefinition achievement)
        {
            string achievementId = achievement.AchievementId;
            
            if (!_completedAchievements.Contains(achievementId))
            {
                _completedAchievements.Add(achievementId);
                _playerProgression.CompletedAchievements.Add(achievementId);
                _achievementProgress[achievementId] = 1f;
                
                // Apply rewards
                ApplyAchievementRewards(achievement);
                
                // Trigger event
                OnAchievementUnlocked?.Invoke(achievement);
            }
        }
        
        private void ApplyAchievementRewards(AchievementDefinition achievement)
        {
            // Apply base rewards
            _playerProgression.TotalExperience += achievement.ExperienceReward;
            _playerProgression.AvailableSkillPoints += achievement.SkillPointReward;
            
            // Apply additional rewards
            foreach (var reward in achievement.Rewards)
            {
                switch (reward.RewardType)
                {
                    case AchievementRewardType.Experience_Bonus:
                        _playerProgression.TotalExperience += reward.RewardValue;
                        break;
                        
                    case AchievementRewardType.Skill_Points:
                        _playerProgression.AvailableSkillPoints += (int)reward.RewardValue;
                        break;
                        
                    case AchievementRewardType.Unlock_Skill:
                        if (reward.UnlockedSkill != null && 
                            !_playerProgression.UnlockedSkills.Contains(reward.UnlockedSkill.SkillId))
                        {
                            _playerProgression.UnlockedSkills.Add(reward.UnlockedSkill.SkillId);
                        }
                        break;
                        
                    case AchievementRewardType.Unlock_Research:
                        if (reward.UnlockedResearch != null && 
                            !_playerProgression.CompletedResearch.Contains(reward.UnlockedResearch.ProjectId))
                        {
                            _playerProgression.CompletedResearch.Add(reward.UnlockedResearch.ProjectId);
                        }
                        break;
                        
                    case AchievementRewardType.Unlock_Equipment:
                        if (reward.UnlockedEquipment != null && 
                            !_playerProgression.UnlockedContent.Contains(reward.UnlockedEquipment.EquipmentId))
                        {
                            _playerProgression.UnlockedContent.Add(reward.UnlockedEquipment.EquipmentId);
                        }
                        break;
                        
                    case AchievementRewardType.Unlock_Feature:
                        if (!string.IsNullOrEmpty(reward.UnlockedFeature) && 
                            !_playerProgression.UnlockedContent.Contains(reward.UnlockedFeature))
                        {
                            _playerProgression.UnlockedContent.Add(reward.UnlockedFeature);
                        }
                        break;
                        
                    case AchievementRewardType.Permanent_Bonus:
                    case AchievementRewardType.Cosmetic_Reward:
                        // Handle other reward types as needed
                        break;
                }
            }
        }
        
        public bool IsAchievementCompleted(string achievementId)
        {
            return _completedAchievements.Contains(achievementId);
        }
        
        public float GetAchievementProgress(string achievementId)
        {
            return _achievementProgress.TryGetValue(achievementId, out float progress) ? progress : 0f;
        }
        
        public List<AchievementDefinition> GetAvailableAchievements()
        {
            if (_achievementLibrary == null) return new List<AchievementDefinition>();
            
            return _achievementLibrary.GetAllAchievements()
                .Where(a => !_completedAchievements.Contains(a.AchievementId))
                .ToList();
        }
        
        public List<AchievementDefinition> GetCompletedAchievements()
        {
            if (_achievementLibrary == null) return new List<AchievementDefinition>();
            
            return _achievementLibrary.GetAllAchievements()
                .Where(a => _completedAchievements.Contains(a.AchievementId))
                .ToList();
        }
        
        public void Update()
        {
            UpdateAchievementProgress();
        }
        
        public void Cleanup()
        {
            _achievementProgress.Clear();
            _completedAchievements.Clear();
        }
    }
} 