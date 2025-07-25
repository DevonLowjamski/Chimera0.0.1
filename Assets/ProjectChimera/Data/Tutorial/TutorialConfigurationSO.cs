using UnityEngine;
using ProjectChimera.Core;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Data.Tutorial
{
    /// <summary>
    /// Global tutorial system configuration for Project Chimera.
    /// Defines system-wide tutorial settings and available sequences.
    /// </summary>
    [CreateAssetMenu(fileName = "New Tutorial Configuration", menuName = "Project Chimera/Tutorial/Tutorial Configuration")]
    public class TutorialConfigurationSO : ChimeraConfigSO
    {
        [Header("Tutorial System Settings")]
        [SerializeField] private bool _enableTutorials = true;
        [SerializeField] private bool _allowSkipping = true;
        [SerializeField] private bool _showHints = true;
        [SerializeField] private float _defaultHintDelay = 5f;
        [SerializeField] private TutorialSpeed _defaultSpeed = TutorialSpeed.Normal;
        
        [Header("UI Configuration")]
        [SerializeField] private bool _highlightTargets = true;
        [SerializeField] private Color _highlightColor = Color.yellow;
        [SerializeField] private float _highlightPulseSpeed = 2f;
        [SerializeField] private bool _dimBackground = true;
        [SerializeField] private float _backgroundDimAmount = 0.7f;
        [SerializeField] private Color _backgroundDimColor = Color.black;
        
        [Header("Audio Settings")]
        [SerializeField] private bool _enableNarration = true;
        [SerializeField] private float _narrationVolume = 0.8f;
        [SerializeField] private bool _enableSoundEffects = true;
        [SerializeField] private float _soundEffectVolume = 0.6f;
        [SerializeField] private AudioClip _stepCompletedSound;
        [SerializeField] private AudioClip _sequenceCompletedSound;
        [SerializeField] private AudioClip _errorSound;
        
        [Header("Gameplay Integration")]
        [SerializeField] private bool _pauseGameDuringTutorial = false;
        [SerializeField] private bool _disableInputOutsideTutorial = true;
        [SerializeField] private bool _saveProgressAutomatically = true;
        [SerializeField] private float _autoSaveInterval = 10f;
        
        [Header("Analytics & Tracking")]
        [SerializeField] private bool _trackAnalytics = true;
        [SerializeField] private bool _logInteractions = true;
        [SerializeField] private bool _collectPerformanceMetrics = true;
        [SerializeField] private int _maxSessionHistory = 100;
        
        [Header("Available Tutorial Sequences")]
        [SerializeField] private List<TutorialSequenceSO> _availableSequences = new List<TutorialSequenceSO>();
        [SerializeField] private List<TutorialSequenceSO> _onboardingSequences = new List<TutorialSequenceSO>();
        [SerializeField] private List<TutorialSequenceSO> _advancedSequences = new List<TutorialSequenceSO>();
        
        [Header("Accessibility")]
        [SerializeField] private bool _enableAccessibilityFeatures = true;
        [SerializeField] private bool _supportScreenReaders = true;
        [SerializeField] private bool _enableKeyboardNavigation = true;
        [SerializeField] private float _textDisplayDuration = 3f;
        [SerializeField] private bool _allowTextSpeedAdjustment = true;
        
        [Header("Localization")]
        [SerializeField] private string _defaultLanguage = "en";
        [SerializeField] private List<string> _supportedLanguages = new List<string> { "en" };
        [SerializeField] private bool _autoDetectLanguage = true;
        
        // Properties
        public bool EnableTutorials => _enableTutorials;
        public bool AllowSkipping => _allowSkipping;
        public bool ShowHints => _showHints;
        public float DefaultHintDelay => _defaultHintDelay;
        public TutorialSpeed DefaultSpeed => _defaultSpeed;
        
        public bool HighlightTargets => _highlightTargets;
        public Color HighlightColor => _highlightColor;
        public float HighlightPulseSpeed => _highlightPulseSpeed;
        public bool DimBackground => _dimBackground;
        public float BackgroundDimAmount => _backgroundDimAmount;
        public Color BackgroundDimColor => _backgroundDimColor;
        
        public bool EnableNarration => _enableNarration;
        public float NarrationVolume => _narrationVolume;
        public bool EnableSoundEffects => _enableSoundEffects;
        public float SoundEffectVolume => _soundEffectVolume;
        public AudioClip StepCompletedSound => _stepCompletedSound;
        public AudioClip SequenceCompletedSound => _sequenceCompletedSound;
        public AudioClip ErrorSound => _errorSound;
        
        public bool PauseGameDuringTutorial => _pauseGameDuringTutorial;
        public bool DisableInputOutsideTutorial => _disableInputOutsideTutorial;
        public bool SaveProgressAutomatically => _saveProgressAutomatically;
        public float AutoSaveInterval => _autoSaveInterval;
        
        public bool TrackAnalytics => _trackAnalytics;
        public bool LogInteractions => _logInteractions;
        public bool CollectPerformanceMetrics => _collectPerformanceMetrics;
        public int MaxSessionHistory => _maxSessionHistory;
        
        public List<TutorialSequenceSO> AvailableSequences => _availableSequences;
        public List<TutorialSequenceSO> OnboardingSequences => _onboardingSequences;
        public List<TutorialSequenceSO> AdvancedSequences => _advancedSequences;
        
        public bool EnableAccessibilityFeatures => _enableAccessibilityFeatures;
        public bool SupportScreenReaders => _supportScreenReaders;
        public bool EnableKeyboardNavigation => _enableKeyboardNavigation;
        public float TextDisplayDuration => _textDisplayDuration;
        public bool AllowTextSpeedAdjustment => _allowTextSpeedAdjustment;
        
        public string DefaultLanguage => _defaultLanguage;
        public List<string> SupportedLanguages => _supportedLanguages;
        public bool AutoDetectLanguage => _autoDetectLanguage;
        
        public override bool ValidateData()
        {
            bool isValid = true;
            
            _defaultHintDelay = Mathf.Max(1f, _defaultHintDelay);
            _highlightPulseSpeed = Mathf.Max(0.1f, _highlightPulseSpeed);
            _backgroundDimAmount = Mathf.Clamp01(_backgroundDimAmount);
            _narrationVolume = Mathf.Clamp01(_narrationVolume);
            _soundEffectVolume = Mathf.Clamp01(_soundEffectVolume);
            _autoSaveInterval = Mathf.Max(5f, _autoSaveInterval);
            _maxSessionHistory = Mathf.Max(10, _maxSessionHistory);
            _textDisplayDuration = Mathf.Max(1f, _textDisplayDuration);
            
            // Remove null sequences
            _availableSequences.RemoveAll(seq => seq == null);
            _onboardingSequences.RemoveAll(seq => seq == null);
            _advancedSequences.RemoveAll(seq => seq == null);
            
            // Ensure default language is in supported languages
            if (!_supportedLanguages.Contains(_defaultLanguage))
            {
                _supportedLanguages.Insert(0, _defaultLanguage);
            }
            
            // Add validation checks
            if (string.IsNullOrEmpty(_defaultLanguage))
            {
                LogError("Default language cannot be empty");
                isValid = false;
            }
            
            if (_supportedLanguages.Count == 0)
            {
                LogError("At least one supported language must be specified");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Get tutorial settings as runtime data
        /// </summary>
        public TutorialSettings GetTutorialSettings()
        {
            return new TutorialSettings
            {
                EnableTutorials = _enableTutorials,
                AllowSkipping = _allowSkipping,
                ShowHints = _showHints,
                HintDelay = _defaultHintDelay,
                EnableNarration = _enableNarration,
                NarrationVolume = _narrationVolume,
                HighlightTargets = _highlightTargets,
                HighlightColor = _highlightColor,
                DimBackground = _dimBackground,
                BackgroundDimAmount = _backgroundDimAmount,
                PauseGameDuringTutorial = _pauseGameDuringTutorial,
                DefaultSpeed = _defaultSpeed,
                SaveProgress = _saveProgressAutomatically,
                TrackAnalytics = _trackAnalytics
            };
        }
        
        /// <summary>
        /// Get sequence by ID
        /// </summary>
        public TutorialSequenceSO GetSequenceById(string sequenceId)
        {
            return _availableSequences.Find(seq => seq != null && seq.SequenceId == sequenceId);
        }
        
        /// <summary>
        /// Get sequences by category
        /// </summary>
        public List<TutorialSequenceSO> GetSequencesByCategory(TutorialCategory category)
        {
            return _availableSequences.FindAll(seq => seq != null && seq.Category == category);
        }
        
        /// <summary>
        /// Get required tutorial sequences
        /// </summary>
        public List<TutorialSequenceSO> GetRequiredSequences()
        {
            return _availableSequences.FindAll(seq => seq != null && seq.IsRequired);
        }
        
        /// <summary>
        /// Get sequences available for player level
        /// </summary>
        public List<TutorialSequenceSO> GetAvailableSequencesForLevel(int playerLevel, List<string> completedSequences, List<string> unlockedFeatures)
        {
            var availableSequences = new List<TutorialSequenceSO>();
            
            foreach (var sequence in _availableSequences)
            {
                if (sequence != null && sequence.ArePrerequisitesSatisfied(completedSequences, playerLevel, unlockedFeatures))
                {
                    availableSequences.Add(sequence);
                }
            }
            
            return availableSequences;
        }
        
        /// <summary>
        /// Check if language is supported
        /// </summary>
        public bool IsLanguageSupported(string languageCode)
        {
            return _supportedLanguages.Contains(languageCode);
        }
        
        /// <summary>
        /// Get system configuration summary
        /// </summary>
        public TutorialSystemInfo GetSystemInfo()
        {
            return new TutorialSystemInfo
            {
                EnableTutorials = _enableTutorials,
                TotalSequences = _availableSequences.Count,
                OnboardingSequences = _onboardingSequences.Count,
                AdvancedSequences = _advancedSequences.Count,
                RequiredSequences = GetRequiredSequences().Count,
                SupportedLanguages = _supportedLanguages.Count,
                EnableAccessibility = _enableAccessibilityFeatures,
                EnableAnalytics = _trackAnalytics
            };
        }
        
        /// <summary>
        /// Validate tutorial system configuration
        /// </summary>
        public TutorialConfigurationValidation ValidateConfiguration()
        {
            var validation = new TutorialConfigurationValidation
            {
                IsValid = true,
                Errors = new List<string>(),
                Warnings = new List<string>()
            };
            
            // Check for empty sequence lists
            if (_availableSequences.Count == 0)
            {
                validation.Warnings.Add("No tutorial sequences configured");
            }
            
            if (_onboardingSequences.Count == 0)
            {
                validation.Warnings.Add("No onboarding sequences configured");
            }
            
            // Check for missing audio clips
            if (_enableSoundEffects && (_stepCompletedSound == null || _sequenceCompletedSound == null))
            {
                validation.Warnings.Add("Sound effects enabled but audio clips not assigned");
            }
            
            // Check language configuration
            if (_supportedLanguages.Count == 0)
            {
                validation.IsValid = false;
                validation.Errors.Add("No supported languages configured");
            }
            
            // Validate sequence integrity
            foreach (var sequence in _availableSequences)
            {
                if (sequence != null)
                {
                    var sequenceValidation = sequence.ValidateSequence();
                    if (!sequenceValidation.IsValid)
                    {
                        validation.IsValid = false;
                        validation.Errors.AddRange(sequenceValidation.Errors.Select(e => $"Sequence {sequence.SequenceId}: {e}"));
                    }
                    validation.Warnings.AddRange(sequenceValidation.Warnings.Select(w => $"Sequence {sequence.SequenceId}: {w}"));
                }
            }
            
            return validation;
        }
    }
    
    /// <summary>
    /// Tutorial system information
    /// </summary>
    public struct TutorialSystemInfo
    {
        public bool EnableTutorials;
        public int TotalSequences;
        public int OnboardingSequences;
        public int AdvancedSequences;
        public int RequiredSequences;
        public int SupportedLanguages;
        public bool EnableAccessibility;
        public bool EnableAnalytics;
    }
    
    /// <summary>
    /// Tutorial configuration validation result
    /// </summary>
    public struct TutorialConfigurationValidation
    {
        public bool IsValid;
        public List<string> Errors;
        public List<string> Warnings;
    }
}