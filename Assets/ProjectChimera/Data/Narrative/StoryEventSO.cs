using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Story Event ScriptableObject for Project Chimera narrative system.
    /// Represents individual narrative events within story chapters.
    /// </summary>
    [CreateAssetMenu(fileName = "New Story Event", menuName = "Project Chimera/Narrative/Story Event", order = 203)]
    public class StoryEventSO : ChimeraDataSO
    {
        [Header("Event Configuration")]
        [SerializeField] private string _eventId;
        [SerializeField] private string _eventName;
        [SerializeField] private string _description;
        [SerializeField] private EventType _eventType = EventType.Dialogue;
        
        [Header("Event Content")]
        [SerializeField] private string _dialogueText;
        [SerializeField] private List<StoryChoiceSO> _availableChoices = new List<StoryChoiceSO>();
        [SerializeField] private List<string> _characterIds = new List<string>();
        [SerializeField] private List<string> _requiredItems = new List<string>();
        
        [Header("Event Flow")]
        [SerializeField] private List<string> _nextEventIds = new List<string>();
        [SerializeField] private List<string> _prerequisiteEventIds = new List<string>();
        [SerializeField] private bool _isRepeatable = false;
        [SerializeField] private bool _isOptional = true;
        
        [Header("Event Rewards")]
        [SerializeField] private int _experienceReward = 0;
        [SerializeField] private List<string> _itemRewards = new List<string>();
        [SerializeField] private List<string> _unlockedContent = new List<string>();
        
        // Properties
        public string EventId => _eventId;
        public string EventName => _eventName;
        public string Description => _description;
        public EventType EventType => _eventType;
        public string DialogueText => _dialogueText;
        public List<StoryChoiceSO> AvailableChoices => _availableChoices;
        public List<string> CharacterIds => _characterIds;
        public List<string> RequiredItems => _requiredItems;
        public List<string> NextEventIds => _nextEventIds;
        public List<string> PrerequisiteEventIds => _prerequisiteEventIds;
        public bool IsRepeatable => _isRepeatable;
        public bool IsOptional => _isOptional;
        public int ExperienceReward => _experienceReward;
        public List<string> ItemRewards => _itemRewards;
        public List<string> UnlockedContent => _unlockedContent;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_eventId))
            {
                _eventId = name.Replace(" ", "").Replace("Event", "").ToLower();
            }
            
            if (string.IsNullOrEmpty(_eventName))
            {
                _eventName = name.Replace("Event", "").Trim();
            }
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_eventId))
            {
                LogError("Event ID cannot be empty");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(_eventName))
            {
                LogError("Event name cannot be empty");
                isValid = false;
            }
            
            if (_eventType == EventType.Dialogue && string.IsNullOrEmpty(_dialogueText))
            {
                LogError("Dialogue events must have dialogue text");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Get choice by ID
        /// </summary>
        public StoryChoiceSO GetChoice(string choiceId)
        {
            return _availableChoices.Find(c => c.ChoiceId == choiceId);
        }
        
        /// <summary>
        /// Check if event meets prerequisites
        /// </summary>
        public bool MeetsPrerequisites(List<string> completedEventIds, List<string> playerInventory)
        {
            foreach (var prerequisite in _prerequisiteEventIds)
            {
                if (!completedEventIds.Contains(prerequisite))
                    return false;
            }
            
            foreach (var requiredItem in _requiredItems)
            {
                if (!playerInventory.Contains(requiredItem))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get available choices based on conditions
        /// </summary>
        public List<StoryChoiceSO> GetAvailableChoices(List<string> playerFlags)
        {
            var availableChoices = new List<StoryChoiceSO>();
            foreach (var choice in _availableChoices)
            {
                if (choice != null && choice.IsAvailable(playerFlags))
                {
                    availableChoices.Add(choice);
                }
            }
            return availableChoices;
        }
        
        /// <summary>
        /// Reset the story event to its initial state
        /// </summary>
        public void Reset()
        {
            // Reset any runtime state if needed
            // This method is called to reset the event for replay or testing
        }
    }
    
    [System.Serializable]
    public enum EventType
    {
        Dialogue,
        Action,
        Choice,
        Cutscene,
        Tutorial,
        Combat,
        Discovery,
        Achievement
    }
} 