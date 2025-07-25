using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Story Chapter ScriptableObject for Project Chimera narrative system.
    /// Represents a chapter within a story arc with events, choices, and character interactions.
    /// </summary>
    [CreateAssetMenu(fileName = "New Story Chapter", menuName = "Project Chimera/Narrative/Story Chapter", order = 202)]
    public class StoryChapterSO : ChimeraDataSO
    {
        [Header("Chapter Configuration")]
        [SerializeField] private string _chapterId;
        [SerializeField] private string _chapterName;
        [SerializeField] private string _description;
        [SerializeField] private int _chapterNumber = 1;
        [SerializeField] private List<StoryEventSO> _chapterEvents = new List<StoryEventSO>();
        
        [Header("Chapter Content")]
        [SerializeField] private List<string> _introducedCharacters = new List<string>();
        [SerializeField] private List<string> _availableQuests = new List<string>();
        [SerializeField] private List<string> _learningObjectives = new List<string>();
        
        [Header("Chapter Progression")]
        [SerializeField] private int _estimatedDurationMinutes = 15;
        [SerializeField] private float _difficultyRating = 1.0f;
        [SerializeField] private bool _isOptional = false;
        [SerializeField] private bool _canBeSkipped = false;
        
        [Header("Chapter Requirements")]
        [SerializeField] private List<string> _prerequisiteChapters = new List<string>();
        [SerializeField] private List<string> _requiredCompletions = new List<string>();
        [SerializeField] private int _minimumPlayerLevel = 1;
        
        // Properties
        public string ChapterId => _chapterId;
        public string ChapterName => _chapterName;
        public string Description => _description;
        public int ChapterNumber => _chapterNumber;
        public List<StoryEventSO> ChapterEvents => _chapterEvents;
        public List<string> IntroducedCharacters => _introducedCharacters;
        public List<string> AvailableQuests => _availableQuests;
        public List<string> LearningObjectives => _learningObjectives;
        public int EstimatedDurationMinutes => _estimatedDurationMinutes;
        public float DifficultyRating => _difficultyRating;
        public bool IsOptional => _isOptional;
        public bool CanBeSkipped => _canBeSkipped;
        public List<string> PrerequisiteChapters => _prerequisiteChapters;
        public List<string> RequiredCompletions => _requiredCompletions;
        public int MinimumPlayerLevel => _minimumPlayerLevel;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_chapterId))
            {
                _chapterId = name.Replace(" ", "").Replace("Chapter", "").ToLower();
            }
            
            if (string.IsNullOrEmpty(_chapterName))
            {
                _chapterName = name.Replace("Chapter", "").Trim();
            }
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_chapterId))
            {
                LogError("Chapter ID cannot be empty");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(_chapterName))
            {
                LogError("Chapter name cannot be empty");
                isValid = false;
            }
            
            if (_chapterNumber <= 0)
            {
                LogError("Chapter number must be positive");
                isValid = false;
            }
            
            if (_estimatedDurationMinutes <= 0)
            {
                LogError("Estimated duration must be positive");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Get the first event in this chapter
        /// </summary>
        public StoryEventSO GetFirstEvent()
        {
            return _chapterEvents.Count > 0 ? _chapterEvents[0] : null;
        }
        
        /// <summary>
        /// Get event by ID
        /// </summary>
        public StoryEventSO GetEvent(string eventId)
        {
            return _chapterEvents.Find(e => e.EventId == eventId);
        }
        
        /// <summary>
        /// Check if chapter meets prerequisites
        /// </summary>
        public bool MeetsPrerequisites(List<string> completedChapterIds, int playerLevel)
        {
            if (playerLevel < _minimumPlayerLevel)
                return false;
                
            foreach (var prerequisite in _prerequisiteChapters)
            {
                if (!completedChapterIds.Contains(prerequisite))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Calculate completion percentage based on completed events
        /// </summary>
        public float CalculateCompletionPercentage(List<string> completedEventIds)
        {
            if (_chapterEvents.Count == 0)
                return 0f;
                
            int completedCount = 0;
            foreach (var chapterEvent in _chapterEvents)
            {
                if (chapterEvent != null && completedEventIds.Contains(chapterEvent.EventId))
                {
                    completedCount++;
                }
            }
            
            return (float)completedCount / _chapterEvents.Count;
        }
        
        /// <summary>
        /// Get all characters introduced in this chapter
        /// </summary>
        public List<string> GetNewCharacters(List<string> previouslyIntroducedCharacters)
        {
            var newCharacters = new List<string>();
            foreach (var character in _introducedCharacters)
            {
                if (!previouslyIntroducedCharacters.Contains(character))
                {
                    newCharacters.Add(character);
                }
            }
            return newCharacters;
        }
    }
} 