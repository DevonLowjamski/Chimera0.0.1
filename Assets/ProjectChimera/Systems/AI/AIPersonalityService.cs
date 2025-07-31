using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.AI;
using ProjectChimera.Systems.Progression;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC-012-3: AI Personality Service - Specialized AI personality adaptation and player interaction management
    /// Extracted from AIAdvisorManager for improved modularity and testability
    /// Handles personality adaptation, player preference learning, and interaction personalization
    /// Target: 500 lines focused on personality and interaction management
    /// </summary>
    public class AIPersonalityService : MonoBehaviour, IAIPersonalityService
    {
        [Header("Personality Configuration")]
        [SerializeField] private PersonalitySettings _personalityConfig;
        [SerializeField] private AIPersonality _defaultPersonality = AIPersonality.Analytical;
        [SerializeField] private float _adaptationRate = 0.1f;
        [SerializeField] private int _maxInteractionHistory = 1000;
        [SerializeField] private float _personalityStabilityThreshold = 0.8f;
        
        [Header("Learning Settings")]
        [SerializeField] private float _learningRate = 0.05f;
        [SerializeField] private int _minInteractionsForAdaptation = 10;
        [SerializeField] private float _feedbackWeight = 0.3f;
        [SerializeField] private bool _enablePersonalityEvolution = true;
        [SerializeField] private float _evolutionThreshold = 100f;
        
        [Header("Communication Style")]
        [SerializeField] private CommunicationStyle _defaultCommunicationStyle = CommunicationStyle.Professional;
        [SerializeField] private float _styleAdaptationRate = 0.08f;
        [SerializeField] private bool _enableContextualAdaptation = true;
        [SerializeField] private float _formalityAdaptationWeight = 0.2f;
        
        // Service dependencies - injected via DI container
        private IPlayerProgressionService _progressionService;
        private MonoBehaviour _eventBus; // Temporarily using MonoBehaviour instead of GameEventBus
        
        // Personality state management
        private AIPersonality _currentPersonality;
        private Dictionary<AIPersonality, float> _personalityWeights = new Dictionary<AIPersonality, float>();
        private Dictionary<string, PersonalityTrait> _personalityTraits = new Dictionary<string, PersonalityTrait>();
        private PersonalityProfile _activeProfile;
        
        // Player interaction tracking
        private List<PlayerInteractionData> _interactionHistory = new List<PlayerInteractionData>();
        private Dictionary<string, float> _playerPreferences = new Dictionary<string, float>();
        private Dictionary<AIPersonality, int> _personalityFeedbackCounts = new Dictionary<AIPersonality, int>();
        private PlayerBehaviorAnalysis _behaviorAnalysis;
        
        // Communication adaptation
        private CommunicationStyle _currentCommunicationStyle;
        private Dictionary<CommunicationStyle, float> _styleEffectiveness = new Dictionary<CommunicationStyle, float>();
        private Dictionary<string, float> _contextualStylePreferences = new Dictionary<string, float>();
        
        // Adaptation timing
        private float _lastAdaptationTime;
        private float _adaptationInterval = 30f; // 30 seconds
        
        // Properties
        public bool IsInitialized { get; private set; }
        public AIPersonality CurrentPersonality => _currentPersonality;
        public CommunicationStyle CurrentCommunicationStyle => _currentCommunicationStyle;
        public PersonalityProfile ActiveProfile => _activeProfile;
        public PlayerBehaviorAnalysis BehaviorAnalysis => _behaviorAnalysis;
        public int InteractionCount => _interactionHistory.Count;
        
        // Events
        public event Action<AIPersonality, AIPersonality> OnPersonalityChanged;
        public event Action<CommunicationStyle> OnCommunicationStyleChanged;
        public event Action<PlayerInteractionData> OnPlayerInteractionRecorded;
        public event Action<PersonalityProfile> OnPersonalityProfileUpdated;
        public event Action<PlayerBehaviorAnalysis> OnBehaviorAnalysisUpdated;
        
        #region Initialization & Lifecycle
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            LogInfo("Initializing AI Personality Service...");
            
            InitializeServiceDependencies();
            InitializePersonalitySystem();
            InitializeCommunicationSystem();
            InitializePlayerAnalysis();
            
            IsInitialized = true;
            LogInfo("AI Personality Service initialized successfully");
        }
        
        private void InitializeServiceDependencies()
        {
            // Dependencies will be injected via DI container
            // For now, we'll find them in the scene as fallback
            if (_progressionService == null)
            {
                var progressionManager = FindObjectOfType<MonoBehaviour>();
                // _progressionService = progressionManager?.GetComponent<IPlayerProgressionService>();
            }
            
            if (_eventBus == null)
            {
                _eventBus = FindObjectOfType<MonoBehaviour>(); // Placeholder for GameEventBus
            }
        }
        
        private void InitializePersonalitySystem()
        {
            _currentPersonality = _defaultPersonality;
            
            // Initialize personality weights
            foreach (AIPersonality personality in Enum.GetValues(typeof(AIPersonality)))
            {
                _personalityWeights[personality] = personality == _defaultPersonality ? 1.0f : 0.1f;
                _personalityFeedbackCounts[personality] = 0;
            }
            
            // Initialize personality traits
            InitializePersonalityTraits();
            
            // Create initial personality profile
            _activeProfile = CreatePersonalityProfile(_currentPersonality);
            
            LogInfo($"Personality system initialized with default personality: {_currentPersonality}");
        }
        
        private void InitializeCommunicationSystem()
        {
            _currentCommunicationStyle = _defaultCommunicationStyle;
            
            // Initialize style effectiveness tracking
            foreach (CommunicationStyle style in Enum.GetValues(typeof(CommunicationStyle)))
            {
                _styleEffectiveness[style] = 0.5f; // Neutral starting effectiveness
            }
            
            LogInfo($"Communication system initialized with style: {_currentCommunicationStyle}");
        }
        
        private void InitializePlayerAnalysis()
        {
            _behaviorAnalysis = new PlayerBehaviorAnalysis
            {
                LastAnalysisTime = DateTime.UtcNow,
                TotalInteractions = 0,
                AverageSessionLength = 0f,
                PreferredPersonalities = new Dictionary<AIPersonality, float>(),
                PreferredCommunicationStyles = new Dictionary<CommunicationStyle, float>(),
                BehaviorPatterns = new Dictionary<string, float>()
            };
            
            LogInfo("Player behavior analysis system initialized");
        }
        
        private void InitializePersonalityTraits()
        {
            _personalityTraits.Clear();
            
            // Define traits for each personality type
            switch (_currentPersonality)
            {
                case AIPersonality.Analytical:
                    AddTrait("DataDriven", 0.9f);
                    AddTrait("Methodical", 0.8f);
                    AddTrait("Precise", 0.85f);
                    AddTrait("Objective", 0.9f);
                    break;
                    
                case AIPersonality.Creative:
                    AddTrait("Innovative", 0.9f);
                    AddTrait("Imaginative", 0.85f);
                    AddTrait("Flexible", 0.8f);
                    AddTrait("Inspiring", 0.75f);
                    break;
                    
                case AIPersonality.Systematic:
                    AddTrait("Organized", 0.9f);
                    AddTrait("Structured", 0.85f);
                    AddTrait("Reliable", 0.9f);
                    AddTrait("Thorough", 0.8f);
                    break;
                    
                case AIPersonality.Experimental:
                    AddTrait("Curious", 0.9f);
                    AddTrait("Bold", 0.8f);
                    AddTrait("Adaptive", 0.85f);
                    AddTrait("Pioneering", 0.75f);
                    break;
                    
                default:
                    AddTrait("Balanced", 0.7f);
                    AddTrait("Adaptable", 0.75f);
                    break;
            }
        }
        
        private void AddTrait(string traitName, float intensity)
        {
            _personalityTraits[traitName] = new PersonalityTrait
            {
                Name = traitName,
                Intensity = intensity,
                LastUpdated = DateTime.UtcNow
            };
        }
        
        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            LogInfo("Shutting down AI Personality Service...");
            
            // Save personality data before shutdown
            SavePersonalityData();
            
            IsInitialized = false;
            LogInfo("AI Personality Service shut down successfully");
        }
        
        #endregion
        
        #region Player Interaction Management
        
        public void RecordPlayerInteraction(string interactionType, string context, float satisfaction = 0.5f)
        {
            var interaction = new PlayerInteractionData
            {
                InteractionId = Guid.NewGuid().ToString(),
                InteractionType = interactionType,
                Context = context,
                Timestamp = DateTime.UtcNow,
                PersonalityAtTime = _currentPersonality,
                CommunicationStyleAtTime = _currentCommunicationStyle,
                PlayerSatisfaction = satisfaction,
                ResponseTime = Time.time,
                SessionContext = GetCurrentSessionContext()
            };
            
            _interactionHistory.Add(interaction);
            
            // Maintain history size limit
            if (_interactionHistory.Count > _maxInteractionHistory)
            {
                _interactionHistory.RemoveAt(0);
            }
            
            // Update player preferences based on interaction
            UpdatePlayerPreferences(interaction);
            
            OnPlayerInteractionRecorded?.Invoke(interaction);
            
            LogInfo($"Recorded player interaction: {interactionType} with satisfaction: {satisfaction:F2}");
        }
        
        public void ProcessPlayerFeedback(string feedbackType, float rating, string context = "")
        {
            // Record feedback as a special interaction
            RecordPlayerInteraction($"Feedback_{feedbackType}", context, rating);
            
            // Update personality feedback counts
            _personalityFeedbackCounts[_currentPersonality]++;
            
            // Adjust personality weights based on feedback
            var adjustment = (rating - 0.5f) * _feedbackWeight;
            _personalityWeights[_currentPersonality] = Mathf.Clamp01(_personalityWeights[_currentPersonality] + adjustment);
            
            // Update communication style effectiveness
            _styleEffectiveness[_currentCommunicationStyle] = 
                Mathf.Lerp(_styleEffectiveness[_currentCommunicationStyle], rating, _styleAdaptationRate);
            
            LogInfo($"Processed player feedback: {feedbackType} with rating: {rating:F2}");
        }
        
        private void UpdatePlayerPreferences(PlayerInteractionData interaction)
        {
            // Update personality preferences
            var personalityKey = $"Personality_{interaction.PersonalityAtTime}";
            if (!_playerPreferences.ContainsKey(personalityKey))
                _playerPreferences[personalityKey] = 0.5f;
            
            _playerPreferences[personalityKey] = Mathf.Lerp(
                _playerPreferences[personalityKey], 
                interaction.PlayerSatisfaction,
                _learningRate
            );
            
            // Update communication style preferences
            var styleKey = $"CommunicationStyle_{interaction.CommunicationStyleAtTime}";
            if (!_playerPreferences.ContainsKey(styleKey))
                _playerPreferences[styleKey] = 0.5f;
            
            _playerPreferences[styleKey] = Mathf.Lerp(
                _playerPreferences[styleKey],
                interaction.PlayerSatisfaction,
                _learningRate
            );
            
            // Update contextual preferences
            if (!string.IsNullOrEmpty(interaction.Context))
            {
                var contextKey = $"Context_{interaction.Context}";
                if (!_playerPreferences.ContainsKey(contextKey))
                    _playerPreferences[contextKey] = 0.5f;
                
                _playerPreferences[contextKey] = Mathf.Lerp(
                    _playerPreferences[contextKey],
                    interaction.PlayerSatisfaction,
                    _learningRate * 0.5f
                );
            }
        }
        
        private string GetCurrentSessionContext()
        {
            // Determine current context based on game state
            if (_progressionService != null)
            {
                // Add contextual information based on player progression
                return "Cultivation_Session"; // Placeholder
            }
            
            return "General_Session";
        }
        
        #endregion
        
        #region Personality Adaptation
        
        public void AdaptPersonality()
        {
            if (_interactionHistory.Count < _minInteractionsForAdaptation)
                return;
            
            var bestPersonality = DetermineBestPersonality();
            
            if (bestPersonality != _currentPersonality && ShouldAdaptPersonality(bestPersonality))
            {
                var previousPersonality = _currentPersonality;
                _currentPersonality = bestPersonality;
                
                // Update personality traits
                InitializePersonalityTraits();
                
                // Update active profile
                _activeProfile = CreatePersonalityProfile(_currentPersonality);
                
                OnPersonalityChanged?.Invoke(previousPersonality, _currentPersonality);
                OnPersonalityProfileUpdated?.Invoke(_activeProfile);
                
                LogInfo($"Personality adapted from {previousPersonality} to {_currentPersonality}");
            }
            
            // Adapt communication style
            AdaptCommunicationStyle();
            
            _lastAdaptationTime = Time.time;
        }
        
        public void AdaptPersonality(AIPersonality newPersonality)
        {
            var previousPersonality = _currentPersonality;
            _currentPersonality = newPersonality;
            
            // Update personality traits
            InitializePersonalityTraits();
            
            // Update active profile
            _activeProfile = CreatePersonalityProfile(_currentPersonality);
            
            OnPersonalityChanged?.Invoke(previousPersonality, _currentPersonality);
            OnPersonalityProfileUpdated?.Invoke(_activeProfile);
            
            LogInfo($"Personality manually adapted from {previousPersonality} to {_currentPersonality}");
            
            // Adapt communication style
            AdaptCommunicationStyle();
            
            _lastAdaptationTime = Time.time;
        }
        
        private AIPersonality DetermineBestPersonality()
        {
            var recentInteractions = _interactionHistory
                .Where(i => (DateTime.UtcNow - i.Timestamp).TotalMinutes < 30)
                .ToList();
            
            if (recentInteractions.Count == 0)
                return _currentPersonality;
            
            var personalityScores = new Dictionary<AIPersonality, float>();
            
            foreach (AIPersonality personality in Enum.GetValues(typeof(AIPersonality)))
            {
                var relevantInteractions = recentInteractions
                    .Where(i => i.PersonalityAtTime == personality)
                    .ToList();
                
                if (relevantInteractions.Count > 0)
                {
                    personalityScores[personality] = relevantInteractions.Average(i => i.PlayerSatisfaction);
                }
                else
                {
                    personalityScores[personality] = _personalityWeights[personality];
                }
            }
            
            return personalityScores.OrderByDescending(kvp => kvp.Value).First().Key;
        }
        
        private bool ShouldAdaptPersonality(AIPersonality newPersonality)
        {
            if (!_enablePersonalityEvolution)
                return false;
            
            var currentScore = _personalityWeights[_currentPersonality];
            var newScore = _personalityWeights[newPersonality];
            
            return (newScore - currentScore) > _personalityStabilityThreshold;
        }
        
        private void AdaptCommunicationStyle()
        {
            var bestStyle = _styleEffectiveness.OrderByDescending(kvp => kvp.Value).First().Key;
            
            if (bestStyle != _currentCommunicationStyle)
            {
                _currentCommunicationStyle = bestStyle;
                OnCommunicationStyleChanged?.Invoke(_currentCommunicationStyle);
                
                LogInfo($"Communication style adapted to: {_currentCommunicationStyle}");
            }
        }
        
        private PersonalityProfile CreatePersonalityProfile(AIPersonality personality)
        {
            return new PersonalityProfile
            {
                ProfileId = Guid.NewGuid().ToString(),
                PrimaryPersonality = personality,
                Traits = new Dictionary<string, float>(_personalityTraits.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Intensity)),
                CommunicationStyle = _currentCommunicationStyle,
                AdaptationLevel = CalculateAdaptationLevel(),
                LastUpdated = DateTime.UtcNow,
                EffectivenessScore = _personalityWeights[personality]
            };
        }
        
        private float CalculateAdaptationLevel()
        {
            if (_interactionHistory.Count == 0)
                return 0f;
            
            var recentInteractions = _interactionHistory.TakeLast(50).ToList();
            var averageSatisfaction = recentInteractions.Average(i => i.PlayerSatisfaction);
            
            return Mathf.Clamp01(averageSatisfaction * 2f - 1f); // Convert 0.5-1.0 to 0-1.0 range
        }
        
        #endregion
        
        #region Behavior Analysis
        
        public void AnalyzePlayerBehavior()
        {
            if (_interactionHistory.Count == 0)
                return;
            
            _behaviorAnalysis = new PlayerBehaviorAnalysis
            {
                LastAnalysisTime = DateTime.UtcNow,
                TotalInteractions = _interactionHistory.Count,
                AverageSessionLength = CalculateAverageSessionLength(),
                PreferredPersonalities = AnalyzePersonalityPreferences(),
                PreferredCommunicationStyles = AnalyzeCommunicationPreferences(),
                BehaviorPatterns = AnalyzeBehaviorPatterns()
            };
            
            OnBehaviorAnalysisUpdated?.Invoke(_behaviorAnalysis);
            
            LogInfo($"Player behavior analysis updated - {_behaviorAnalysis.TotalInteractions} interactions analyzed");
        }
        
        private float CalculateAverageSessionLength()
        {
            // Placeholder implementation
            return _interactionHistory.Count > 0 ? 300f : 0f; // 5 minutes average
        }
        
        private Dictionary<AIPersonality, float> AnalyzePersonalityPreferences()
        {
            var preferences = new Dictionary<AIPersonality, float>();
            
            foreach (AIPersonality personality in Enum.GetValues(typeof(AIPersonality)))
            {
                var relevantInteractions = _interactionHistory
                    .Where(i => i.PersonalityAtTime == personality)
                    .ToList();
                
                if (relevantInteractions.Count > 0)
                {
                    preferences[personality] = relevantInteractions.Average(i => i.PlayerSatisfaction);
                }
                else
                {
                    preferences[personality] = 0.5f;
                }
            }
            
            return preferences;
        }
        
        private Dictionary<CommunicationStyle, float> AnalyzeCommunicationPreferences()
        {
            var preferences = new Dictionary<CommunicationStyle, float>();
            
            foreach (CommunicationStyle style in Enum.GetValues(typeof(CommunicationStyle)))
            {
                var relevantInteractions = _interactionHistory
                    .Where(i => i.CommunicationStyleAtTime == style)
                    .ToList();
                
                if (relevantInteractions.Count > 0)
                {
                    preferences[style] = relevantInteractions.Average(i => i.PlayerSatisfaction);
                }
                else
                {
                    preferences[style] = 0.5f;
                }
            }
            
            return preferences;
        }
        
        private Dictionary<string, float> AnalyzeBehaviorPatterns()
        {
            var patterns = new Dictionary<string, float>();
            
            // Analyze interaction frequency
            patterns["InteractionFrequency"] = _interactionHistory.Count / Mathf.Max(1f, Time.time / 3600f); // Per hour
            
            // Analyze feedback positivity
            var feedbackInteractions = _interactionHistory.Where(i => i.InteractionType.StartsWith("Feedback_")).ToList();
            if (feedbackInteractions.Count > 0)
            {
                patterns["FeedbackPositivity"] = feedbackInteractions.Average(i => i.PlayerSatisfaction);
            }
            
            // Analyze engagement consistency
            if (_interactionHistory.Count > 10)
            {
                var recent = _interactionHistory.TakeLast(10).Average(i => i.PlayerSatisfaction);
                var older = _interactionHistory.Take(_interactionHistory.Count - 10).Average(i => i.PlayerSatisfaction);
                patterns["EngagementTrend"] = recent - older;
            }
            
            return patterns;
        }
        
        #endregion
        
        #region Data Management
        
        public PersonalityProfile GetPersonalityProfile()
        {
            return _activeProfile;
        }
        
        public PlayerBehaviorAnalysis GetBehaviorAnalysis()
        {
            return _behaviorAnalysis;
        }
        
        public Dictionary<string, float> GetPlayerPreferences()
        {
            return new Dictionary<string, float>(_playerPreferences);
        }
        
        public List<PlayerInteractionData> GetRecentInteractions(int count = 50)
        {
            return _interactionHistory.TakeLast(count).ToList();
        }
        
        private void SavePersonalityData()
        {
            // Placeholder for personality data persistence
            LogInfo("Personality data saved");
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Update()
        {
            if (!IsInitialized) return;
            
            // Periodic personality adaptation
            if (Time.time - _lastAdaptationTime >= _adaptationInterval)
            {
                if (_enablePersonalityEvolution)
                {
                    AdaptPersonality();
                }
                
                AnalyzePlayerBehavior();
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private void LogInfo(string message)
        {
            Debug.Log($"[AIPersonalityService] {message}");
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[AIPersonalityService] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[AIPersonalityService] {message}");
        }
        
        #endregion
        
        #region Public API Methods
        
        public List<PlayerInteractionData> GetInteractionHistory()
        {
            return _interactionHistory.ToList();
        }
        
        public void RecordPlayerInteraction(string interactionType, float playerSatisfaction)
        {
            var interaction = new PlayerInteractionData
            {
                InteractionId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                InteractionType = interactionType,
                PlayerSatisfaction = playerSatisfaction,
                PersonalityAtTime = _currentPersonality,
                CommunicationStyleAtTime = _currentCommunicationStyle,
                Context = GetCurrentSessionContext()
            };
            
            _interactionHistory.Add(interaction);
            
            // Maintain history size limit
            if (_interactionHistory.Count > _maxInteractionHistory)
            {
                _interactionHistory.RemoveAt(0);
            }
            
            // Update learning systems with the interaction
            UpdatePersonalityWeights(interaction);
            
            OnPlayerInteractionRecorded?.Invoke(interaction);
            
            LogInfo($"Player interaction recorded: {interactionType} (Satisfaction: {playerSatisfaction:F2})");
        }
        
        private void UpdatePersonalityWeights(PlayerInteractionData interaction)
        {
            // Update personality weights based on interaction satisfaction
            var personalityKey = interaction.PersonalityAtTime;
            var adjustment = (interaction.PlayerSatisfaction - 0.5f) * _learningRate;
            _personalityWeights[personalityKey] = Mathf.Clamp01(_personalityWeights[personalityKey] + adjustment);
        }
        
        public void UpdatePersonalityFromInteractions()
        {
            if (_interactionHistory.Count < 5) return; // Need minimum interactions
            
            var recentInteractions = _interactionHistory.TakeLast(20).ToList();
            var averageSatisfaction = recentInteractions.Average(i => i.PlayerSatisfaction);
            
            // Adapt personality based on satisfaction
            if (averageSatisfaction < 0.3f)
            {
                // Switch to more supportive personality
                if (_currentPersonality != AIPersonality.Supportive)
                {
                    AdaptPersonality(AIPersonality.Supportive);
                }
            }
            else if (averageSatisfaction > 0.8f)
            {
                // Can be more analytical or efficient
                if (_currentPersonality == AIPersonality.Supportive)
                {
                    AdaptPersonality(AIPersonality.Analytical);
                }
            }
        }
        
        public string GeneratePersonalizedMessage(string baseMessage, string context)
        {
            string personalizedMessage = baseMessage;
            
            switch (_currentPersonality)
            {
                case AIPersonality.Supportive:
                    personalizedMessage = $"I'm here to help! {baseMessage} Let me know if you need any clarification.";
                    break;
                case AIPersonality.Analytical:
                    personalizedMessage = $"Based on the data: {baseMessage}";
                    break;
                case AIPersonality.Efficient:
                    personalizedMessage = $"{baseMessage}";
                    break;
                case AIPersonality.Educational:
                    personalizedMessage = $"{baseMessage} This will help you understand the cultivation process better.";
                    break;
            }
            
            return personalizedMessage;
        }
        
        #endregion
    }
    
    #region Supporting Data Structures
    
    [System.Serializable]
    public class PersonalitySettings
    {
        [Header("Adaptation Settings")]
        public float adaptationSensitivity = 0.1f;
        public int minInteractionsForChange = 10;
        public float stabilityThreshold = 0.8f;
        public bool enableEvolution = true;
        
        [Header("Communication Settings")]
        public CommunicationStyle defaultStyle = CommunicationStyle.Professional;
        public float styleAdaptationRate = 0.08f;
        public bool enableContextualAdaptation = true;
    }
    
    [System.Serializable]
    public class PersonalityTrait
    {
        public string Name;
        public float Intensity;
        public DateTime LastUpdated;
    }
    
    [System.Serializable]
    public class PersonalityProfile
    {
        public string ProfileId;
        public AIPersonality PrimaryPersonality;
        public Dictionary<string, float> Traits;
        public CommunicationStyle CommunicationStyle;
        public float AdaptationLevel;
        public DateTime LastUpdated;
        public float EffectivenessScore;
    }
    
    [System.Serializable]
    public class PlayerBehaviorAnalysis
    {
        public DateTime LastAnalysisTime;
        public int TotalInteractions;
        public float AverageSessionLength;
        public Dictionary<AIPersonality, float> PreferredPersonalities;
        public Dictionary<CommunicationStyle, float> PreferredCommunicationStyles;
        public Dictionary<string, float> BehaviorPatterns;
    }
    
    #endregion
}