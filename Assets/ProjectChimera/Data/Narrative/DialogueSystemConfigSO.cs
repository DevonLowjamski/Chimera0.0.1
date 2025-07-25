using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Comprehensive dialogue system configuration ScriptableObject for Project Chimera's narrative engine.
    /// Features advanced dialogue management with scientific accuracy validation, emotional modeling,
    /// and cross-system integration for cannabis cultivation education.
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialogue System Config", menuName = "Project Chimera/Narrative/Dialogue System Config", order = 105)]
    public class DialogueSystemConfigSO : ChimeraConfigSO
    {
        [Header("Core Dialogue Settings")]
        [SerializeField] private float _dialogueDisplaySpeed = 50f;
        [SerializeField] private float _choiceTimeoutDuration = 30f;
        [SerializeField] private bool _enableAutoAdvance = false;
        [SerializeField] private float _autoAdvanceDelay = 3f;
        [SerializeField] private bool _enableSkipDialogue = true;
        [SerializeField] private bool _enableDialogueHistory = true;
        
        [Header("Voice and Audio")]
        [SerializeField] private bool _enableVoiceActing = false;
        [SerializeField] private bool _enableSubtitles = true;
        [SerializeField] private float _voiceVolume = 0.8f;
        [SerializeField] private bool _enableLipSync = false;
        [SerializeField] private AudioMixerGroup _dialogueAudioMixer;
        
        [Header("Educational Integration")]
        [SerializeField] private bool _enableEducationalValidation = true;
        [SerializeField] private bool _requireScientificAccuracy = true;
        [SerializeField] private float _educationalContentThreshold = 0.6f;
        [SerializeField] private bool _enableKnowledgeTracking = true;
        [SerializeField] private bool _enableLearningAssessment = true;
        
        [Header("Emotional Modeling")]
        [SerializeField] private bool _enableEmotionalTracking = true;
        [SerializeField] private bool _enableMoodInfluence = true;
        [SerializeField] private float _emotionalDecayRate = 0.1f;
        [SerializeField] private bool _enableEmotionalMemory = true;
        [SerializeField] private int _maxEmotionalMemoryEntries = 50;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxDialogueHistoryEntries = 1000;
        [SerializeField] private bool _enableDialogueCaching = true;
        [SerializeField] private int _maxCachedDialogues = 100;
        [SerializeField] private bool _enableLazyLoading = true;
        [SerializeField] private float _cacheCleanupInterval = 300f;
        
        [Header("Localization")]
        [SerializeField] private bool _enableLocalization = true;
        [SerializeField] private List<SystemLanguage> _supportedLanguages = new List<SystemLanguage>();
        [SerializeField] private SystemLanguage _defaultLanguage = SystemLanguage.English;
        [SerializeField] private bool _enableDynamicLanguageSwitching = true;
        
        [Header("Accessibility")]
        [SerializeField] private bool _enableAccessibilityFeatures = true;
        [SerializeField] private float _minimumFontSize = 12f;
        [SerializeField] private float _maximumFontSize = 24f;
        [SerializeField] private bool _enableHighContrast = true;
        [SerializeField] private bool _enableColorBlindSupport = true;
        [SerializeField] private bool _enableScreenReader = false;
        
        [Header("Choice and Branching")]
        [SerializeField] private int _maxChoicesPerDialogue = 6;
        [SerializeField] private bool _enableChoicePreview = true;
        [SerializeField] private bool _showChoiceConsequences = false;
        [SerializeField] private bool _enableChoiceHistory = true;
        [SerializeField] private float _choiceHighlightDuration = 0.3f;
        
        [Header("Character Interaction")]
        [SerializeField] private bool _enableRelationshipInfluence = true;
        [SerializeField] private bool _enableCharacterMoodTracking = true;
        [SerializeField] private bool _enableContextualResponses = true;
        [SerializeField] private float _relationshipInfluenceWeight = 0.7f;
        [SerializeField] private bool _enableCharacterMemory = true;
        
        // Public Properties
        public float DialogueDisplaySpeed => _dialogueDisplaySpeed;
        public float ChoiceTimeoutDuration => _choiceTimeoutDuration;
        public bool EnableAutoAdvance => _enableAutoAdvance;
        public float AutoAdvanceDelay => _autoAdvanceDelay;
        public bool EnableSkipDialogue => _enableSkipDialogue;
        public bool EnableDialogueHistory => _enableDialogueHistory;
        
        public bool EnableVoiceActing => _enableVoiceActing;
        public bool EnableSubtitles => _enableSubtitles;
        public float VoiceVolume => _voiceVolume;
        public bool EnableLipSync => _enableLipSync;
        public AudioMixerGroup DialogueAudioMixer => _dialogueAudioMixer;
        
        public bool EnableEducationalValidation => _enableEducationalValidation;
        public bool RequireScientificAccuracy => _requireScientificAccuracy;
        public float EducationalContentThreshold => _educationalContentThreshold;
        public bool EnableKnowledgeTracking => _enableKnowledgeTracking;
        public bool EnableLearningAssessment => _enableLearningAssessment;
        
        public bool EnableEmotionalTracking => _enableEmotionalTracking;
        public bool EnableMoodInfluence => _enableMoodInfluence;
        public float EmotionalDecayRate => _emotionalDecayRate;
        public bool EnableEmotionalMemory => _enableEmotionalMemory;
        public int MaxEmotionalMemoryEntries => _maxEmotionalMemoryEntries;
        
        public int MaxDialogueHistoryEntries => _maxDialogueHistoryEntries;
        public bool EnableDialogueCaching => _enableDialogueCaching;
        public int MaxCachedDialogues => _maxCachedDialogues;
        public bool EnableLazyLoading => _enableLazyLoading;
        public float CacheCleanupInterval => _cacheCleanupInterval;
        
        public bool EnableLocalization => _enableLocalization;
        public IReadOnlyList<SystemLanguage> SupportedLanguages => _supportedLanguages.AsReadOnly();
        public SystemLanguage DefaultLanguage => _defaultLanguage;
        public bool EnableDynamicLanguageSwitching => _enableDynamicLanguageSwitching;
        
        public bool EnableAccessibilityFeatures => _enableAccessibilityFeatures;
        public float MinimumFontSize => _minimumFontSize;
        public float MaximumFontSize => _maximumFontSize;
        public bool EnableHighContrast => _enableHighContrast;
        public bool EnableColorBlindSupport => _enableColorBlindSupport;
        public bool EnableScreenReader => _enableScreenReader;
        
        public int MaxChoicesPerDialogue => _maxChoicesPerDialogue;
        public bool EnableChoicePreview => _enableChoicePreview;
        public bool ShowChoiceConsequences => _showChoiceConsequences;
        public bool EnableChoiceHistory => _enableChoiceHistory;
        public float ChoiceHighlightDuration => _choiceHighlightDuration;
        
        public bool EnableRelationshipInfluence => _enableRelationshipInfluence;
        public bool EnableCharacterMoodTracking => _enableCharacterMoodTracking;
        public bool EnableContextualResponses => _enableContextualResponses;
        public float RelationshipInfluenceWeight => _relationshipInfluenceWeight;
        public bool EnableCharacterMemory => _enableCharacterMemory;
        
        protected override bool ValidateDataSpecific()
        {
            var isValid = true;
            var validationErrors = new List<string>();
            
            // Validate dialogue display settings
            if (_dialogueDisplaySpeed <= 0f)
            {
                validationErrors.Add("Dialogue Display Speed must be greater than 0");
                isValid = false;
            }
            
            if (_choiceTimeoutDuration <= 0f)
            {
                validationErrors.Add("Choice Timeout Duration must be greater than 0");
                isValid = false;
            }
            
            if (_autoAdvanceDelay < 0f)
            {
                validationErrors.Add("Auto Advance Delay cannot be negative");
                isValid = false;
            }
            
            // Validate audio settings
            if (_voiceVolume < 0f || _voiceVolume > 1f)
            {
                validationErrors.Add("Voice Volume must be between 0 and 1");
                isValid = false;
            }
            
            // Validate educational settings
            if (_educationalContentThreshold < 0f || _educationalContentThreshold > 1f)
            {
                validationErrors.Add("Educational Content Threshold must be between 0 and 1");
                isValid = false;
            }
            
            // Validate emotional modeling settings
            if (_emotionalDecayRate < 0f || _emotionalDecayRate > 1f)
            {
                validationErrors.Add("Emotional Decay Rate must be between 0 and 1");
                isValid = false;
            }
            
            if (_maxEmotionalMemoryEntries <= 0)
            {
                validationErrors.Add("Max Emotional Memory Entries must be greater than 0");
                isValid = false;
            }
            
            // Validate performance settings
            if (_maxDialogueHistoryEntries <= 0)
            {
                validationErrors.Add("Max Dialogue History Entries must be greater than 0");
                isValid = false;
            }
            
            if (_maxCachedDialogues <= 0)
            {
                validationErrors.Add("Max Cached Dialogues must be greater than 0");
                isValid = false;
            }
            
            if (_cacheCleanupInterval <= 0f)
            {
                validationErrors.Add("Cache Cleanup Interval must be greater than 0");
                isValid = false;
            }
            
            // Validate accessibility settings
            if (_minimumFontSize <= 0f || _maximumFontSize <= 0f || _minimumFontSize > _maximumFontSize)
            {
                validationErrors.Add("Invalid font size range - minimum must be positive and less than maximum");
                isValid = false;
            }
            
            // Validate choice settings
            if (_maxChoicesPerDialogue <= 0)
            {
                validationErrors.Add("Max Choices Per Dialogue must be greater than 0");
                isValid = false;
            }
            
            if (_choiceHighlightDuration < 0f)
            {
                validationErrors.Add("Choice Highlight Duration cannot be negative");
                isValid = false;
            }
            
            // Validate relationship influence settings
            if (_relationshipInfluenceWeight < 0f || _relationshipInfluenceWeight > 1f)
            {
                validationErrors.Add("Relationship Influence Weight must be between 0 and 1");
                isValid = false;
            }
            
            // Validate supported languages
            if (_enableLocalization && _supportedLanguages.Count == 0)
            {
                validationErrors.Add("Localization enabled but no supported languages specified");
                isValid = false;
            }
            
            if (_enableLocalization && !_supportedLanguages.Contains(_defaultLanguage))
            {
                validationErrors.Add("Default language must be in supported languages list");
                isValid = false;
            }
            
            // Log validation results
            if (!isValid)
            {
                Debug.LogError($"[DialogueSystemConfigSO] Validation failed for {name}: {string.Join(", ", validationErrors)}");
            }
            
            return base.ValidateDataSpecific() && isValid;
        }
        
        /// <summary>
        /// Get optimized settings for different performance modes
        /// </summary>
        public DialogueSystemConfigSO GetOptimizedSettings(PerformanceMode mode)
        {
            var optimized = Instantiate(this);
            
            switch (mode)
            {
                case PerformanceMode.HighPerformance:
                    optimized._enableVoiceActing = false;
                    optimized._enableLipSync = false;
                    optimized._enableEmotionalTracking = false;
                    optimized._maxDialogueHistoryEntries = Mathf.Max(100, _maxDialogueHistoryEntries / 2);
                    optimized._maxCachedDialogues = Mathf.Max(10, _maxCachedDialogues / 2);
                    optimized._enableLazyLoading = true;
                    break;
                    
                case PerformanceMode.Balanced:
                    // Use default settings
                    break;
                    
                case PerformanceMode.HighQuality:
                    optimized._enableVoiceActing = true;
                    optimized._enableLipSync = true;
                    optimized._enableEmotionalTracking = true;
                    optimized._maxDialogueHistoryEntries = _maxDialogueHistoryEntries * 2;
                    optimized._maxCachedDialogues = _maxCachedDialogues * 2;
                    optimized._enableEducationalValidation = true;
                    break;
            }
            
            return optimized;
        }
        
        public enum PerformanceMode
        {
            HighPerformance,
            Balanced,
            HighQuality
        }
    }
}