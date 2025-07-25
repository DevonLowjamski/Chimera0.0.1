using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Story Choice Option ScriptableObject for Project Chimera narrative system.
    /// Represents individual options within a choice that players can select.
    /// </summary>
    [CreateAssetMenu(fileName = "New Story Choice Option", menuName = "Project Chimera/Narrative/Story Choice Option", order = 208)]
    public class StoryChoiceOptionSO : ChimeraDataSO
    {
        [Header("Option Configuration")]
        [SerializeField] private string _optionId;
        [SerializeField] private string _optionText;
        [SerializeField] private string _description;
        [SerializeField] private string _flavorText;
        
        [Header("Option Requirements")]
        [SerializeField] private List<string> _requiredFlags = new List<string>();
        [SerializeField] private List<string> _requiredItems = new List<string>();
        [SerializeField] private List<string> _requiredSkills = new List<string>();
        [SerializeField] private int _minimumLevel = 1;
        [SerializeField] private int _requiredReputation = 0;
        
        [Header("Option Consequences")]
        [SerializeField] private string _resultEventId;
        [SerializeField] private List<string> _setFlags = new List<string>();
        [SerializeField] private List<string> _removeFlags = new List<string>();
        [SerializeField] private List<string> _rewardItems = new List<string>();
        [SerializeField] private List<string> _removeItems = new List<string>();
        
        [Header("Option Properties")]
        [SerializeField] private OptionType _optionType = OptionType.Standard;
        [SerializeField] private int _experienceReward = 0;
        [SerializeField] private int _reputationChange = 0;
        [SerializeField] private float _successChance = 1.0f;
        [SerializeField] private bool _isOneTime = false;
        [SerializeField] private bool _requiresConfirmation = false;
        
        [Header("Visual Properties")]
        [SerializeField] private Color _textColor = Color.white;
        [SerializeField] private Sprite _optionIcon;
        [SerializeField] private string _tooltipText;
        
        // Properties
        public string OptionId => _optionId;
        public string OptionText => _optionText;
        public string Description => _description;
        public string FlavorText => _flavorText;
        public List<string> RequiredFlags => _requiredFlags;
        public List<string> RequiredItems => _requiredItems;
        public List<string> RequiredSkills => _requiredSkills;
        public int MinimumLevel => _minimumLevel;
        public int RequiredReputation => _requiredReputation;
        public string ResultEventId => _resultEventId;
        public List<string> SetFlags => _setFlags;
        public List<string> RemoveFlags => _removeFlags;
        public List<string> RewardItems => _rewardItems;
        public List<string> RemoveItems => _removeItems;
        public OptionType OptionType => _optionType;
        public int ExperienceReward => _experienceReward;
        public int ReputationChange => _reputationChange;
        public float SuccessChance => _successChance;
        public bool IsOneTime => _isOneTime;
        public bool RequiresConfirmation => _requiresConfirmation;
        public Color TextColor => _textColor;
        public Sprite OptionIcon => _optionIcon;
        public string TooltipText => _tooltipText;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_optionId))
            {
                _optionId = name.Replace(" ", "").Replace("Option", "").ToLower();
            }
            
            if (string.IsNullOrEmpty(_optionText))
            {
                _optionText = name.Replace("Option", "").Trim();
            }
            
            // Clamp success chance between 0 and 1
            _successChance = Mathf.Clamp01(_successChance);
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_optionId))
            {
                LogError("Option ID cannot be empty");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(_optionText))
            {
                LogError("Option text cannot be empty");
                isValid = false;
            }
            
            if (_minimumLevel < 1)
            {
                LogError("Minimum level must be at least 1");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Check if this option is available based on player state
        /// </summary>
        public bool IsAvailable(List<string> playerFlags, List<string> playerInventory, 
                               Dictionary<string, int> playerSkills, int playerLevel, 
                               int playerReputation, List<string> usedOptions)
        {
            // Check if already used and is one-time only
            if (_isOneTime && usedOptions.Contains(_optionId))
                return false;
            
            // Check level requirement
            if (playerLevel < _minimumLevel)
                return false;
            
            // Check reputation requirement
            if (playerReputation < _requiredReputation)
                return false;
            
            // Check required flags
            foreach (var requiredFlag in _requiredFlags)
            {
                if (!playerFlags.Contains(requiredFlag))
                    return false;
            }
            
            // Check required items
            foreach (var requiredItem in _requiredItems)
            {
                if (!playerInventory.Contains(requiredItem))
                    return false;
            }
            
            // Check required skills
            foreach (var requiredSkill in _requiredSkills)
            {
                var parts = requiredSkill.Split(':');
                if (parts.Length == 2)
                {
                    var skillName = parts[0];
                    if (int.TryParse(parts[1], out int requiredValue))
                    {
                        if (!playerSkills.ContainsKey(skillName) || playerSkills[skillName] < requiredValue)
                            return false;
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Apply the consequences of selecting this option
        /// </summary>
        public OptionResult ApplyOption(List<string> playerFlags, List<string> playerInventory,
                                       ref int experience, ref int reputation)
        {
            var result = new OptionResult();
            
            // Check for success based on success chance
            var random = new System.Random();
            bool success = random.NextDouble() <= _successChance;
            result.WasSuccessful = success;
            
            if (success)
            {
                // Set flags
                foreach (var flag in _setFlags)
                {
                    if (!playerFlags.Contains(flag))
                    {
                        playerFlags.Add(flag);
                        result.AddedFlags.Add(flag);
                    }
                }
                
                // Remove flags
                foreach (var flag in _removeFlags)
                {
                    if (playerFlags.Remove(flag))
                    {
                        result.RemovedFlags.Add(flag);
                    }
                }
                
                // Add reward items
                foreach (var item in _rewardItems)
                {
                    playerInventory.Add(item);
                    result.RewardItems.Add(item);
                }
                
                // Apply rewards
                experience += _experienceReward;
                reputation += _reputationChange;
                
                result.ExperienceGained = _experienceReward;
                result.ReputationChange = _reputationChange;
            }
            
            // Remove items (happens regardless of success)
            foreach (var item in _removeItems)
            {
                if (playerInventory.Remove(item))
                {
                    result.RemovedItems.Add(item);
                }
            }
            
            result.NextEventId = _resultEventId;
            
            return result;
        }
        
        /// <summary>
        /// Get the display text with any dynamic formatting
        /// </summary>
        public string GetDisplayText(Dictionary<string, object> gameState = null)
        {
            var displayText = _optionText;
            
            // Add any success chance indication for skill checks
            if (_optionType == OptionType.SkillCheck && _successChance < 1.0f)
            {
                var percentage = Mathf.RoundToInt(_successChance * 100);
                displayText += $" ({percentage}% chance)";
            }
            
            return displayText;
        }
    }
    
    [System.Serializable]
    public enum OptionType
    {
        Standard,
        SkillCheck,
        Aggressive,
        Diplomatic,
        Sneaky,
        Intellectual,
        Emotional,
        Risky,
        Safe,
        Creative
    }
    
    [System.Serializable]
    public class OptionResult
    {
        public bool WasSuccessful = true;
        public List<string> AddedFlags = new List<string>();
        public List<string> RemovedFlags = new List<string>();
        public List<string> RewardItems = new List<string>();
        public List<string> RemovedItems = new List<string>();
        public string NextEventId;
        public int ExperienceGained = 0;
        public int ReputationChange = 0;
    }
} 