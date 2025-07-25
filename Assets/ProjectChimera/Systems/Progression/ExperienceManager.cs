using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.Events;
using System.Collections.Generic;
using System;

namespace ProjectChimera.Systems.Progression
{
    /// <summary>
    /// Manages experience gain, level calculations, and skill progression
    /// </summary>
    public class ExperienceManager
    {
        private ExperienceConfigSO _experienceConfig;
        private Dictionary<string, float> _skillExperience = new Dictionary<string, float>();
        private Dictionary<string, int> _skillLevels = new Dictionary<string, int>();
        private PlayerProgressionData _playerProgression;
        
        // Events
        public System.Action<string, float> OnExperienceGained;
        public System.Action<string, int> OnSkillLevelUp;
        
        public ExperienceManager(ExperienceConfigSO experienceConfig)
        {
            _experienceConfig = experienceConfig;
        }
        
        public void SetPlayerProgression(PlayerProgressionData playerProgression)
        {
            _playerProgression = playerProgression;
            LoadSkillData();
        }
        
        private void LoadSkillData()
        {
            if (_playerProgression != null)
            {
                _skillExperience = new Dictionary<string, float>(_playerProgression.SkillExperience);
                _skillLevels = new Dictionary<string, int>(_playerProgression.SkillLevels);
            }
        }
        
        public void GainExperience(string skillId, float amount, string source = "")
        {
            if (amount <= 0f) return;
            
            // Apply multipliers
            float finalAmount = amount;
            if (_experienceConfig != null)
            {
                finalAmount *= _experienceConfig.GlobalExperienceMultiplier;
            }
            
            // Add experience
            if (!_skillExperience.ContainsKey(skillId))
            {
                _skillExperience[skillId] = 0f;
                _skillLevels[skillId] = 1;
            }
            
            _skillExperience[skillId] += finalAmount;
            _playerProgression.TotalExperience += finalAmount;
            
            // Update player progression data
            _playerProgression.SkillExperience[skillId] = _skillExperience[skillId];
            
            // Check for level up
            CheckSkillLevelUp(skillId);
            
            // Trigger event
            OnExperienceGained?.Invoke(skillId, finalAmount);
        }
        
        private void CheckSkillLevelUp(string skillId)
        {
            if (!_skillExperience.ContainsKey(skillId) || !_skillLevels.ContainsKey(skillId))
                return;
                
            int currentLevel = _skillLevels[skillId];
            float currentExperience = _skillExperience[skillId];
            
            int newLevel = CalculateLevelFromExperience(currentExperience);
            
            if (newLevel > currentLevel)
            {
                _skillLevels[skillId] = newLevel;
                _playerProgression.SkillLevels[skillId] = newLevel;
                
                // Update overall level
                UpdateOverallLevel();
                
                // Trigger event
                OnSkillLevelUp?.Invoke(skillId, newLevel);
            }
        }
        
        private int CalculateLevelFromExperience(float experience)
        {
            if (_experienceConfig == null)
            {
                // Default calculation if no config
                return Mathf.FloorToInt(Mathf.Sqrt(experience / 100f)) + 1;
            }
            
            // Use experience config to calculate level
            for (int level = 1; level <= _experienceConfig.MaxSkillLevel; level++)
            {
                float requiredExp = _experienceConfig.GetSkillExperienceRequiredForLevel(level);
                if (experience < requiredExp)
                {
                    return level - 1;
                }
            }
            
            return _experienceConfig.MaxSkillLevel;
        }
        
        private void UpdateOverallLevel()
        {
            if (_playerProgression == null) return;
            
            int totalLevels = 0;
            foreach (var level in _skillLevels.Values)
            {
                totalLevels += level;
            }
            
            _playerProgression.OverallLevel = Mathf.Max(1, totalLevels / Mathf.Max(1, _skillLevels.Count));
        }
        
        public float GetExperienceRequiredForLevel(string skillId, int level)
        {
            if (_experienceConfig == null)
            {
                // Default calculation
                return level * level * 100f;
            }
            
            return _experienceConfig.GetSkillExperienceRequiredForLevel(level);
        }
        
        public float GetExperienceToNextLevel(string skillId)
        {
            if (!_skillExperience.ContainsKey(skillId) || !_skillLevels.ContainsKey(skillId))
                return 0f;
                
            int currentLevel = _skillLevels[skillId];
            float currentExperience = _skillExperience[skillId];
            float requiredForNext = GetExperienceRequiredForLevel(skillId, currentLevel + 1);
            
            return Mathf.Max(0f, requiredForNext - currentExperience);
        }
        
        public float GetSkillProgress(string skillId)
        {
            if (!_skillExperience.ContainsKey(skillId) || !_skillLevels.ContainsKey(skillId))
                return 0f;
                
            int currentLevel = _skillLevels[skillId];
            float currentExperience = _skillExperience[skillId];
            float currentLevelExp = GetExperienceRequiredForLevel(skillId, currentLevel);
            float nextLevelExp = GetExperienceRequiredForLevel(skillId, currentLevel + 1);
            
            if (nextLevelExp <= currentLevelExp) return 1f;
            
            return (currentExperience - currentLevelExp) / (nextLevelExp - currentLevelExp);
        }
        
        public int GetSkillLevel(string skillId)
        {
            return _skillLevels.TryGetValue(skillId, out int level) ? level : 1;
        }
        
        public float GetSkillExperience(string skillId)
        {
            return _skillExperience.TryGetValue(skillId, out float exp) ? exp : 0f;
        }
        
        public Dictionary<string, int> GetAllSkillLevels()
        {
            return new Dictionary<string, int>(_skillLevels);
        }
        
        public Dictionary<string, float> GetAllSkillExperience()
        {
            return new Dictionary<string, float>(_skillExperience);
        }
        
        public void Update()
        {
            // Experience manager doesn't need regular updates
        }
        
        public void Cleanup()
        {
            _skillExperience.Clear();
            _skillLevels.Clear();
        }
    }
} 