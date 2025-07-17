using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Story Theme ScriptableObject for Project Chimera narrative system.
    /// Represents thematic elements and emotional tones for story content.
    /// </summary>
    [CreateAssetMenu(fileName = "New Story Theme", menuName = "Project Chimera/Narrative/Story Theme", order = 207)]
    public class StoryThemeSO : ChimeraDataSO
    {
        [Header("Theme Configuration")]
        [SerializeField] private string _themeId;
        [SerializeField] private string _themeName;
        [SerializeField] private string _description;
        [SerializeField] private Color _themeColor = Color.white;
        
        [Header("Theme Properties")]
        [SerializeField] private ThemeType _themeType = ThemeType.Emotional;
        [SerializeField] private EmotionalTone _emotionalTone = EmotionalTone.Neutral;
        [SerializeField] private float _intensity = 1.0f;
        [SerializeField] private Sprite _themeIcon;
        
        [Header("Theme Associations")]
        [SerializeField] private List<string> _associatedTagIds = new List<string>();
        [SerializeField] private List<string> _conflictingThemeIds = new List<string>();
        [SerializeField] private List<string> _complementaryThemeIds = new List<string>();
        
        [Header("Content Guidelines")]
        [SerializeField] private List<string> _contentKeywords = new List<string>();
        [SerializeField] private List<string> _excludedKeywords = new List<string>();
        [SerializeField] private string _toneGuideline;
        [SerializeField] private string _styleGuideline;
        
        [Header("Educational Context")]
        [SerializeField] private bool _hasEducationalValue = false;
        [SerializeField] private List<string> _learningTopics = new List<string>();
        [SerializeField] private int _appropriateAgeRating = 13;
        [SerializeField] private List<string> _skillsReinforced = new List<string>();
        
        // Properties
        public string ThemeId => _themeId;
        public string ThemeName => _themeName;
        public string Description => _description;
        public Color ThemeColor => _themeColor;
        public ThemeType ThemeType => _themeType;
        public EmotionalTone EmotionalTone => _emotionalTone;
        public float Intensity => _intensity;
        public Sprite ThemeIcon => _themeIcon;
        public List<string> AssociatedTagIds => _associatedTagIds;
        public List<string> ConflictingThemeIds => _conflictingThemeIds;
        public List<string> ComplementaryThemeIds => _complementaryThemeIds;
        public List<string> ContentKeywords => _contentKeywords;
        public List<string> ExcludedKeywords => _excludedKeywords;
        public string ToneGuideline => _toneGuideline;
        public string StyleGuideline => _styleGuideline;
        public bool HasEducationalValue => _hasEducationalValue;
        public List<string> LearningTopics => _learningTopics;
        public int AppropriateAgeRating => _appropriateAgeRating;
        public List<string> SkillsReinforced => _skillsReinforced;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_themeId))
            {
                _themeId = name.Replace(" ", "").Replace("Theme", "").ToLower();
            }
            
            if (string.IsNullOrEmpty(_themeName))
            {
                _themeName = name.Replace("Theme", "").Trim();
            }
            
            // Clamp intensity between 0 and 1
            _intensity = Mathf.Clamp01(_intensity);
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_themeId))
            {
                LogError("Theme ID cannot be empty");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(_themeName))
            {
                LogError("Theme name cannot be empty");
                isValid = false;
            }
            
            if (_appropriateAgeRating < 0)
            {
                LogError("Age rating cannot be negative");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Check if this theme is compatible with another theme
        /// </summary>
        public bool IsCompatibleWith(string otherThemeId)
        {
            return !_conflictingThemeIds.Contains(otherThemeId);
        }
        
        /// <summary>
        /// Check if this theme complements another theme
        /// </summary>
        public bool ComplementsWith(string otherThemeId)
        {
            return _complementaryThemeIds.Contains(otherThemeId);
        }
        
        /// <summary>
        /// Check if content matches this theme based on keywords
        /// </summary>
        public bool MatchesContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return false;
            
            var lowerContent = content.ToLower();
            
            // Check for excluded keywords first
            foreach (var excludedKeyword in _excludedKeywords)
            {
                if (lowerContent.Contains(excludedKeyword.ToLower()))
                    return false;
            }
            
            // Check for content keywords
            int matchCount = 0;
            foreach (var keyword in _contentKeywords)
            {
                if (lowerContent.Contains(keyword.ToLower()))
                    matchCount++;
            }
            
            // Return true if at least one keyword matches
            return matchCount > 0;
        }
        
        /// <summary>
        /// Calculate compatibility score with a list of other themes
        /// </summary>
        public float CalculateCompatibilityScore(List<string> otherThemeIds)
        {
            if (otherThemeIds == null || otherThemeIds.Count == 0)
                return 1.0f;
            
            int compatibleCount = 0;
            int complementaryCount = 0;
            int conflictingCount = 0;
            
            foreach (var themeId in otherThemeIds)
            {
                if (_conflictingThemeIds.Contains(themeId))
                    conflictingCount++;
                else if (_complementaryThemeIds.Contains(themeId))
                    complementaryCount++;
                else
                    compatibleCount++;
            }
            
            // Calculate score: complementary themes boost score, conflicts reduce it
            float score = (float)(compatibleCount + complementaryCount * 2 - conflictingCount * 3) / otherThemeIds.Count;
            return Mathf.Clamp01(score);
        }
        
        /// <summary>
        /// Get suggested complementary themes
        /// </summary>
        public List<string> GetSuggestedComplementaryThemes()
        {
            return new List<string>(_complementaryThemeIds);
        }
        
        /// <summary>
        /// Check if theme is appropriate for given age
        /// </summary>
        public bool IsAppropriateForAge(int age)
        {
            return age >= _appropriateAgeRating;
        }
    }
    
    [System.Serializable]
    public enum ThemeType
    {
        Emotional,
        Narrative,
        Educational,
        Atmospheric,
        Character,
        Conflict,
        Setting,
        Cultural
    }
    
    // Note: EmotionalTone enum is defined in NarrativeDataStructures.cs
} 