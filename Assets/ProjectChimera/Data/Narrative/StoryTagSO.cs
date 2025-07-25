using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Story Tag ScriptableObject for Project Chimera narrative system.
    /// Represents tags for categorizing and filtering story content.
    /// </summary>
    [CreateAssetMenu(fileName = "New Story Tag", menuName = "Project Chimera/Narrative/Story Tag", order = 206)]
    public class StoryTagSO : ChimeraDataSO
    {
        [Header("Tag Configuration")]
        [SerializeField] private string _tagId;
        [SerializeField] private string _tagName;
        [SerializeField] private string _description;
        [SerializeField] private Color _tagColor = Color.white;
        
        [Header("Tag Properties")]
        [SerializeField] private TagType _tagType = TagType.Content;
        [SerializeField] private int _priority = 0;
        [SerializeField] private bool _isVisible = true;
        [SerializeField] private Sprite _tagIcon;
        
        [Header("Tag Relationships")]
        [SerializeField] private List<string> _relatedTagIds = new List<string>();
        [SerializeField] private List<string> _excludedTagIds = new List<string>();
        [SerializeField] private List<string> _parentTagIds = new List<string>();
        
        [Header("Educational Properties")]
        [SerializeField] private bool _isEducational = false;
        [SerializeField] private string _skillArea;
        [SerializeField] private int _difficultyLevel = 1;
        [SerializeField] private List<string> _learningObjectives = new List<string>();
        
        // Properties
        public string TagId => _tagId;
        public string TagName => _tagName;
        public string Description => _description;
        public Color TagColor => _tagColor;
        public TagType TagType => _tagType;
        public int Priority => _priority;
        public bool IsVisible => _isVisible;
        public Sprite TagIcon => _tagIcon;
        public List<string> RelatedTagIds => _relatedTagIds;
        public List<string> ExcludedTagIds => _excludedTagIds;
        public List<string> ParentTagIds => _parentTagIds;
        public bool IsEducational => _isEducational;
        public string SkillArea => _skillArea;
        public int DifficultyLevel => _difficultyLevel;
        public List<string> LearningObjectives => _learningObjectives;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_tagId))
            {
                _tagId = name.Replace(" ", "").Replace("Tag", "").ToLower();
            }
            
            if (string.IsNullOrEmpty(_tagName))
            {
                _tagName = name.Replace("Tag", "").Trim();
            }
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_tagId))
            {
                LogError("Tag ID cannot be empty");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(_tagName))
            {
                LogError("Tag name cannot be empty");
                isValid = false;
            }
            
            if (_difficultyLevel < 1)
            {
                LogError("Difficulty level must be at least 1");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Check if this tag is compatible with another tag
        /// </summary>
        public bool IsCompatibleWith(string otherTagId)
        {
            return !_excludedTagIds.Contains(otherTagId);
        }
        
        /// <summary>
        /// Check if this tag is related to another tag
        /// </summary>
        public bool IsRelatedTo(string otherTagId)
        {
            return _relatedTagIds.Contains(otherTagId);
        }
        
        /// <summary>
        /// Check if this tag is a child of another tag
        /// </summary>
        public bool IsChildOf(string parentTagId)
        {
            return _parentTagIds.Contains(parentTagId);
        }
        
        /// <summary>
        /// Get the hierarchy level of this tag
        /// </summary>
        public int GetHierarchyLevel()
        {
            return _parentTagIds.Count;
        }
        
        /// <summary>
        /// Check if this tag is appropriate for the given skill level
        /// </summary>
        public bool IsAppropriateForLevel(int playerLevel)
        {
            if (!_isEducational)
                return true;
                
            // Allow some flexibility around the difficulty level
            int levelVariance = 1;
            return playerLevel >= (_difficultyLevel - levelVariance);
        }
    }
    
    [System.Serializable]
    public enum TagType
    {
        Content,
        Educational,
        Emotional,
        Gameplay,
        Character,
        Location,
        Theme,
        Difficulty
    }
} 