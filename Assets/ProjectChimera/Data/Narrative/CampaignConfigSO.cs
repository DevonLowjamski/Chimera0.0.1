using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Enterprise-grade campaign configuration ScriptableObject providing comprehensive narrative system settings
    /// for Project Chimera's sophisticated story-driven gameplay. Follows Project Chimera standards with
    /// scientific accuracy validation, performance optimization, and cross-system integration capabilities.
    /// </summary>
    [CreateAssetMenu(fileName = "New Campaign Config", menuName = "Project Chimera/Narrative/Campaign Config", order = 100)]
    public class CampaignConfigSO : ChimeraConfigSO
    {
        [Header("Core Campaign Configuration")]
        [SerializeField] private bool _enableStoryMode = true;
        [SerializeField] private bool _enableBranchingNarratives = true;
        [SerializeField] private bool _enableCharacterRelationships = true;
        [SerializeField] private float _storyProgressionRate = 1.0f;
        [SerializeField] private int _maxActiveStoryArcs = 5;
        [SerializeField] private float _narrativeUpdateInterval = 0.1f;
        
        [Header("Narrative Features Configuration")]
        [SerializeField] private bool _enableDynamicCharacterRelationships = true;
        [SerializeField] private bool _enableConsequenceTracking = true;
        [SerializeField] private bool _enableEmotionalEngagement = true;
        [SerializeField] private bool _enableAdaptiveStorytelling = true;
        [SerializeField] private bool _enableContextualEvents = true;
        
        [Header("Educational Integration")]
        [SerializeField] private bool _enableScientificAccuracy = true;
        [SerializeField] private bool _enableCultivationEducation = true;
        [SerializeField] private float _educationalContentRatio = 0.7f;
        [SerializeField] private bool _enableRealWorldIntegration = true;
        [SerializeField] private bool _requireScientificValidation = true;
        
        [Header("Performance Optimization")]
        [SerializeField] private bool _enablePerformanceOptimization = true;
        [SerializeField] private bool _enableMemoryPooling = true;
        [SerializeField] private int _maxCachedDialogues = 100;
        [SerializeField] private int _maxCharacterUpdateBatchSize = 10;
        [SerializeField] private float _characterUpdateInterval = 0.1f;
        
        [Header("Dialogue System Configuration")]
        [SerializeField] private float _dialogueDisplaySpeed = 50f;
        [SerializeField] private float _choiceTimeoutDuration = 30f;
        [SerializeField] private bool _enableDialogueHistory = true;
        [SerializeField] private int _maxDialogueHistoryEntries = 1000;
        [SerializeField] private bool _enableVoiceActing = false;
        
        [Header("Character Development Settings")]
        [SerializeField] private CharacterDatabaseSO _characterDatabase;
        [SerializeField] private float _relationshipChangeRate = 1.0f;
        [SerializeField] private float _trustDecayRate = 0.001f;
        [SerializeField] private float _respectGrowthMultiplier = 1.2f;
        [SerializeField] private int _maxActiveRelationships = 20;
        
        [Header("Story Arc Management")]
        [SerializeField] private StoryArcLibrarySO _storyArcLibrary;
        [SerializeField] private bool _enableParallelStoryArcs = true;
        [SerializeField] private int _maxParallelArcs = 3;
        [SerializeField] private float _arcTransitionDuration = 2.0f;
        [SerializeField] private bool _enableArcPrioritization = true;
        [SerializeField] private int _maxCachedConsequences = 100;
        
        [Header("Analytics and Tracking")]
        [SerializeField] private bool _enableNarrativeAnalytics = true;
        [SerializeField] private bool _enablePlayerBehaviorTracking = true;
        [SerializeField] private bool _enableEmotionalResponseTracking = true;
        [SerializeField] private float _analyticsUpdateInterval = 1.0f;
        
        // Public Properties
        public bool EnableStoryMode => _enableStoryMode;
        public bool EnableBranchingNarratives => _enableBranchingNarratives;
        public bool EnableCharacterRelationships => _enableCharacterRelationships;
        public float StoryProgressionRate => _storyProgressionRate;
        public int MaxActiveStoryArcs => _maxActiveStoryArcs;
        public float NarrativeUpdateInterval => _narrativeUpdateInterval;
        
        public bool EnableDynamicCharacterRelationships => _enableDynamicCharacterRelationships;
        public bool EnableConsequenceTracking => _enableConsequenceTracking;
        public bool EnableEmotionalEngagement => _enableEmotionalEngagement;
        public bool EnableAdaptiveStorytelling => _enableAdaptiveStorytelling;
        public bool EnableContextualEvents => _enableContextualEvents;
        
        public bool EnableScientificAccuracy => _enableScientificAccuracy;
        public bool EnableCultivationEducation => _enableCultivationEducation;
        public float EducationalContentRatio => _educationalContentRatio;
        public bool EnableRealWorldIntegration => _enableRealWorldIntegration;
        public bool RequireScientificValidation => _requireScientificValidation;
        
        public bool EnablePerformanceOptimization => _enablePerformanceOptimization;
        public bool EnableMemoryPooling => _enableMemoryPooling;
        public int MaxCachedDialogues => _maxCachedDialogues;
        public int MaxCharacterUpdateBatchSize => _maxCharacterUpdateBatchSize;
        public float CharacterUpdateInterval => _characterUpdateInterval;
        
        public float DialogueDisplaySpeed => _dialogueDisplaySpeed;
        public float ChoiceTimeoutDuration => _choiceTimeoutDuration;
        public bool EnableDialogueHistory => _enableDialogueHistory;
        public int MaxDialogueHistoryEntries => _maxDialogueHistoryEntries;
        public bool EnableVoiceActing => _enableVoiceActing;
        
        public CharacterDatabaseSO CharacterDatabase => _characterDatabase;
        public float RelationshipChangeRate => _relationshipChangeRate;
        public float TrustDecayRate => _trustDecayRate;
        public float RespectGrowthMultiplier => _respectGrowthMultiplier;
        public int MaxActiveRelationships => _maxActiveRelationships;
        
        public StoryArcLibrarySO StoryArcLibrary => _storyArcLibrary;
        public bool EnableParallelStoryArcs => _enableParallelStoryArcs;
        public int MaxParallelArcs => _maxParallelArcs;
        public float ArcTransitionDuration => _arcTransitionDuration;
        public bool EnableArcPrioritization => _enableArcPrioritization;
        public int MaxCachedConsequences => _maxCachedConsequences;
        
        public bool EnableNarrativeAnalytics => _enableNarrativeAnalytics;
        public bool EnablePlayerBehaviorTracking => _enablePlayerBehaviorTracking;
        public bool EnableEmotionalResponseTracking => _enableEmotionalResponseTracking;
        public float AnalyticsUpdateInterval => _analyticsUpdateInterval;
        
        protected override bool ValidateDataSpecific()
        {
            var isValid = true;
            var validationErrors = new List<string>();
            
            // Validate performance settings
            if (_maxActiveStoryArcs <= 0)
            {
                validationErrors.Add("Max Active Story Arcs must be greater than 0");
                isValid = false;
            }
            
            if (_narrativeUpdateInterval <= 0f)
            {
                validationErrors.Add("Narrative Update Interval must be greater than 0");
                isValid = false;
            }
            
            // Validate educational content ratio
            if (_educationalContentRatio < 0f || _educationalContentRatio > 1f)
            {
                validationErrors.Add("Educational Content Ratio must be between 0 and 1");
                isValid = false;
            }
            
            // Validate character development settings
            if (_maxActiveRelationships <= 0)
            {
                validationErrors.Add("Max Active Relationships must be greater than 0");
                isValid = false;
            }
            
            if (_relationshipChangeRate < 0f)
            {
                validationErrors.Add("Relationship Change Rate cannot be negative");
                isValid = false;
            }
            
            // Validate dialogue settings
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
            
            // Validate memory and performance settings
            if (_maxCachedDialogues <= 0)
            {
                validationErrors.Add("Max Cached Dialogues must be greater than 0");
                isValid = false;
            }
            
            if (_maxCharacterUpdateBatchSize <= 0)
            {
                validationErrors.Add("Max Character Update Batch Size must be greater than 0");
                isValid = false;
            }
            
            // Log validation results
            if (!isValid)
            {
                Debug.LogError($"[CampaignConfigSO] Validation failed for {name}: {string.Join(", ", validationErrors)}");
            }
            
            return base.ValidateDataSpecific() && isValid;
        }
        
        /// <summary>
        /// Get optimized settings for different performance modes
        /// </summary>
        public CampaignConfigSO GetOptimizedSettings(PerformanceMode mode)
        {
            var optimized = Instantiate(this);
            
            switch (mode)
            {
                case PerformanceMode.HighPerformance:
                    optimized._maxActiveStoryArcs = Mathf.Max(1, _maxActiveStoryArcs / 2);
                    optimized._maxCachedDialogues = Mathf.Max(10, _maxCachedDialogues / 2);
                    optimized._narrativeUpdateInterval = _narrativeUpdateInterval * 2f;
                    optimized._enableEmotionalResponseTracking = false;
                    break;
                    
                case PerformanceMode.Balanced:
                    // Use default settings
                    break;
                    
                case PerformanceMode.HighQuality:
                    optimized._maxActiveStoryArcs = _maxActiveStoryArcs * 2;
                    optimized._maxCachedDialogues = _maxCachedDialogues * 2;
                    optimized._narrativeUpdateInterval = _narrativeUpdateInterval * 0.5f;
                    optimized._enableEmotionalResponseTracking = true;
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