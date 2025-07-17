using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Core narrative configuration ScriptableObject for Project Chimera's narrative system.
    /// Defines system-wide settings, progression parameters, character interaction rules,
    /// and integration settings for seamless story-driven cannabis cultivation experiences.
    /// </summary>
    [CreateAssetMenu(fileName = "New Narrative Config", menuName = "Project Chimera/Narrative/Narrative Config", order = 100)]
    public class NarrativeConfigSO : ChimeraConfigSO
    {
        [Header("Storytelling Configuration")]
        [SerializeField] private bool _enableDynamicStorytelling = true;
        [SerializeField] private bool _enablePlayerChoices = true;
        [SerializeField] private bool _enableBranchingNarratives = true;
        [SerializeField] private float _storyProgressionSpeed = 1.0f;
        [SerializeField] private bool _enableNonLinearProgression = true;
        [SerializeField] private int _maxConcurrentStoryArcs = 3;
        
        [Header("Character System")]
        [SerializeField] private bool _enableCharacterDevelopment = true;
        [SerializeField] private bool _enableCharacterRelationships = true;
        [SerializeField] private bool _enableCompanionSystem = true;
        [SerializeField] private bool _enableMentorSystem = true;
        [SerializeField] private bool _enableRivalSystem = true;
        [SerializeField] private int _maxActiveCharacters = 10;
        [SerializeField] private float _relationshipDecayRate = 0.01f;
        
        [Header("Dialogue System")]
        [SerializeField] private bool _enableDialogueSystem = true;
        [SerializeField] private bool _enableVoiceActing = false;
        [SerializeField] private bool _enableEmotionalResponses = true;
        [SerializeField] private bool _enableContextualDialogue = true;
        [SerializeField] private float _dialogueSpeed = 1.0f;
        [SerializeField] private bool _enableSkippableDialogue = true;
        [SerializeField] private bool _enableDialogueHistory = true;
        
        [Header("Quest and Mission System")]
        [SerializeField] private bool _enableQuestSystem = true;
        [SerializeField] private bool _enableDynamicQuests = true;
        [SerializeField] private bool _enableQuestChaining = true;
        [SerializeField] private bool _enableCommunityQuests = true;
        [SerializeField] private int _maxActiveQuests = 5;
        [SerializeField] private int _maxCompletedQuestHistory = 100;
        
        [Header("Integration Settings")]
        [SerializeField] private bool _enableCultivationIntegration = true;
        [SerializeField] private bool _enableLiveEventIntegration = true;
        [SerializeField] private bool _enableEducationalIntegration = true;
        [SerializeField] private bool _enableCommunityStories = true;
        [SerializeField] private bool _enableEconomicIntegration = false;
        [SerializeField] private float _integrationUpdateInterval = 60f;
        
        [Header("Content Generation")]
        [SerializeField] private bool _enableDynamicContentGeneration = false;
        [SerializeField] private bool _enableProceduralDialogue = false;
        [SerializeField] private bool _enableAdaptiveNarratives = true;
        [SerializeField] private float _contentGenerationComplexity = 0.5f;
        [SerializeField] private bool _enablePlayerPersonalization = true;
        
        [Header("Performance and Optimization")]
        [SerializeField] private bool _enableNarrativeContentCaching = true;
        [SerializeField] private int _maxCachedContent = 500;
        [SerializeField] private bool _enableLazyLoading = true;
        [SerializeField] private bool _enableContentPreloading = true;
        [SerializeField] private float _updateFrequency = 10f;
        [SerializeField] private int _maxUpdatesPerFrame = 5;
        
        [Header("Analytics and Metrics")]
        [SerializeField] private bool _enableNarrativeAnalytics = true;
        [SerializeField] private bool _enablePlayerBehaviorTracking = true;
        [SerializeField] private bool _enableStoryProgressionMetrics = true;
        [SerializeField] private bool _enableChoiceAnalytics = true;
        [SerializeField] private bool _enableEngagementMetrics = true;
        
        [Header("Educational Integration")]
        [SerializeField] private bool _enableEducationalContent = true;
        [SerializeField] private bool _requireScientificAccuracy = true;
        [SerializeField] private float _educationalContentRatio = 0.4f;
        [SerializeField] private bool _enableLearningPathways = true;
        [SerializeField] private bool _enableKnowledgeValidation = true;
        
        [Header("Cultural and Social Features")]
        [SerializeField] private bool _enableCulturalContent = true;
        [SerializeField] private bool _enableSocialInteractions = true;
        [SerializeField] private bool _enableCommunityStorytelling = false;
        [SerializeField] private bool _enableCulturalAuthenticity = true;
        [SerializeField] private bool _enableAccessibilityFeatures = true;
        
        [Header("Localization Support")]
        [SerializeField] private bool _enableLocalization = true;
        [SerializeField] private List<string> _supportedLanguages = new List<string>();
        [SerializeField] private bool _enableRightToLeftLanguages = false;
        [SerializeField] private bool _enableCulturalAdaptation = true;
        
        // Public Properties
        public bool EnableDynamicStorytelling => _enableDynamicStorytelling;
        public bool EnablePlayerChoices => _enablePlayerChoices;
        public bool EnableBranchingNarratives => _enableBranchingNarratives;
        public float StoryProgressionSpeed => _storyProgressionSpeed;
        public bool EnableNonLinearProgression => _enableNonLinearProgression;
        public int MaxConcurrentStoryArcs => _maxConcurrentStoryArcs;
        
        public bool EnableCharacterDevelopment => _enableCharacterDevelopment;
        public bool EnableCharacterRelationships => _enableCharacterRelationships;
        public bool EnableCompanionSystem => _enableCompanionSystem;
        public bool EnableMentorSystem => _enableMentorSystem;
        public bool EnableRivalSystem => _enableRivalSystem;
        public int MaxActiveCharacters => _maxActiveCharacters;
        public float RelationshipDecayRate => _relationshipDecayRate;
        
        public bool EnableDialogueSystem => _enableDialogueSystem;
        public bool EnableVoiceActing => _enableVoiceActing;
        public bool EnableEmotionalResponses => _enableEmotionalResponses;
        public bool EnableContextualDialogue => _enableContextualDialogue;
        public float DialogueSpeed => _dialogueSpeed;
        public bool EnableSkippableDialogue => _enableSkippableDialogue;
        public bool EnableDialogueHistory => _enableDialogueHistory;
        
        public bool EnableQuestSystem => _enableQuestSystem;
        public bool EnableDynamicQuests => _enableDynamicQuests;
        public bool EnableQuestChaining => _enableQuestChaining;
        public bool EnableCommunityQuests => _enableCommunityQuests;
        public int MaxActiveQuests => _maxActiveQuests;
        public int MaxCompletedQuestHistory => _maxCompletedQuestHistory;
        
        public bool EnableCultivationIntegration => _enableCultivationIntegration;
        public bool EnableLiveEventIntegration => _enableLiveEventIntegration;
        public bool EnableEducationalIntegration => _enableEducationalIntegration;
        public bool EnableCommunityStories => _enableCommunityStories;
        public bool EnableEconomicIntegration => _enableEconomicIntegration;
        public float IntegrationUpdateInterval => _integrationUpdateInterval;
        
        public bool EnableDynamicContentGeneration => _enableDynamicContentGeneration;
        public bool EnableProceduralDialogue => _enableProceduralDialogue;
        public bool EnableAdaptiveNarratives => _enableAdaptiveNarratives;
        public float ContentGenerationComplexity => _contentGenerationComplexity;
        public bool EnablePlayerPersonalization => _enablePlayerPersonalization;
        
        public bool EnableNarrativeContentCaching => _enableNarrativeContentCaching;
        public int MaxCachedContent => _maxCachedContent;
        public bool EnableLazyLoading => _enableLazyLoading;
        public bool EnableContentPreloading => _enableContentPreloading;
        public float UpdateFrequency => _updateFrequency;
        public int MaxUpdatesPerFrame => _maxUpdatesPerFrame;
        
        public bool EnableNarrativeAnalytics => _enableNarrativeAnalytics;
        public bool EnablePlayerBehaviorTracking => _enablePlayerBehaviorTracking;
        public bool EnableStoryProgressionMetrics => _enableStoryProgressionMetrics;
        public bool EnableChoiceAnalytics => _enableChoiceAnalytics;
        public bool EnableEngagementMetrics => _enableEngagementMetrics;
        
        public bool EnableEducationalContent => _enableEducationalContent;
        public bool RequireScientificAccuracy => _requireScientificAccuracy;
        public float EducationalContentRatio => _educationalContentRatio;
        public bool EnableLearningPathways => _enableLearningPathways;
        public bool EnableKnowledgeValidation => _enableKnowledgeValidation;
        
        public bool EnableCulturalContent => _enableCulturalContent;
        public bool EnableSocialInteractions => _enableSocialInteractions;
        public bool EnableCommunityStorytelling => _enableCommunityStorytelling;
        public bool EnableCulturalAuthenticity => _enableCulturalAuthenticity;
        public bool EnableAccessibilityFeatures => _enableAccessibilityFeatures;
        
        public bool EnableLocalization => _enableLocalization;
        public List<string> SupportedLanguages => _supportedLanguages;
        public bool EnableRightToLeftLanguages => _enableRightToLeftLanguages;
        public bool EnableCulturalAdaptation => _enableCulturalAdaptation;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Validate progression settings
            _storyProgressionSpeed = Mathf.Max(0.1f, _storyProgressionSpeed);
            _maxConcurrentStoryArcs = Mathf.Max(1, _maxConcurrentStoryArcs);
            _maxActiveCharacters = Mathf.Max(1, _maxActiveCharacters);
            _maxActiveQuests = Mathf.Max(1, _maxActiveQuests);
            _maxCompletedQuestHistory = Mathf.Max(1, _maxCompletedQuestHistory);
            
            // Validate performance settings
            _maxCachedContent = Mathf.Max(1, _maxCachedContent);
            _updateFrequency = Mathf.Max(0.1f, _updateFrequency);
            _maxUpdatesPerFrame = Mathf.Max(1, _maxUpdatesPerFrame);
            _integrationUpdateInterval = Mathf.Max(1f, _integrationUpdateInterval);
            
            // Validate dialogue settings
            _dialogueSpeed = Mathf.Max(0.1f, _dialogueSpeed);
            _relationshipDecayRate = Mathf.Max(0f, _relationshipDecayRate);
            
            // Validate content generation
            _contentGenerationComplexity = Mathf.Clamp01(_contentGenerationComplexity);
            _educationalContentRatio = Mathf.Clamp01(_educationalContentRatio);
            
            // Initialize default supported languages if empty
            if (_enableLocalization && _supportedLanguages.Count == 0)
            {
                InitializeDefaultLanguages();
            }
        }
        
        private void InitializeDefaultLanguages()
        {
            _supportedLanguages = new List<string>
            {
                "en", // English
                "es", // Spanish
                "fr", // French
                "de", // German
                "it", // Italian
                "pt", // Portuguese
                "nl", // Dutch
                "zh", // Chinese
                "ja", // Japanese
                "ko"  // Korean
            };
        }
        
        public NarrativeSystemLimits GetSystemLimits()
        {
            return new NarrativeSystemLimits
            {
                MaxConcurrentStoryArcs = _maxConcurrentStoryArcs,
                MaxActiveCharacters = _maxActiveCharacters,
                MaxActiveQuests = _maxActiveQuests,
                MaxCachedContent = _maxCachedContent,
                MaxUpdatesPerFrame = _maxUpdatesPerFrame
            };
        }
        
        public NarrativePerformanceSettings GetPerformanceSettings()
        {
            return new NarrativePerformanceSettings
            {
                EnableCaching = _enableNarrativeContentCaching,
                EnableLazyLoading = _enableLazyLoading,
                EnableContentPreloading = _enableContentPreloading,
                UpdateFrequency = _updateFrequency,
                MaxUpdatesPerFrame = _maxUpdatesPerFrame
            };
        }
        
        public NarrativeIntegrationSettings GetIntegrationSettings()
        {
            return new NarrativeIntegrationSettings
            {
                EnableCultivationIntegration = _enableCultivationIntegration,
                EnableLiveEventIntegration = _enableLiveEventIntegration,
                EnableEducationalIntegration = _enableEducationalIntegration,
                EnableCommunityStories = _enableCommunityStories,
                EnableEconomicIntegration = _enableEconomicIntegration,
                UpdateInterval = _integrationUpdateInterval
            };
        }
        
        public bool IsLanguageSupported(string languageCode)
        {
            return !_enableLocalization || _supportedLanguages.Contains(languageCode);
        }
        
        public bool ShouldIncludeEducationalContent()
        {
            return _enableEducationalContent && UnityEngine.Random.value <= _educationalContentRatio;
        }
    }
    
    // Supporting data structures
    [Serializable]
    public class NarrativeSystemLimits
    {
        public int MaxConcurrentStoryArcs;
        public int MaxActiveCharacters;
        public int MaxActiveQuests;
        public int MaxCachedContent;
        public int MaxUpdatesPerFrame;
    }
    
    [Serializable]
    public class NarrativePerformanceSettings
    {
        public bool EnableCaching;
        public bool EnableLazyLoading;
        public bool EnableContentPreloading;
        public float UpdateFrequency;
        public int MaxUpdatesPerFrame;
    }
    
    [Serializable]
    public class NarrativeIntegrationSettings
    {
        public bool EnableCultivationIntegration;
        public bool EnableLiveEventIntegration;
        public bool EnableEducationalIntegration;
        public bool EnableCommunityStories;
        public bool EnableEconomicIntegration;
        public float UpdateInterval;
    }
}