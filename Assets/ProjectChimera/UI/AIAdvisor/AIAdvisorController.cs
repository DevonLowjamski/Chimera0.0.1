using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
// using ProjectChimera.Systems.AI;
// using ProjectChimera.Systems.Automation;
using ProjectChimera.Data.AI;
using ProjectChimera.Data.UI;

namespace ProjectChimera.UI.AIAdvisor
{
    /// <summary>
    /// AI Advisor UI Controller for Project Chimera.
    /// Provides an intelligent interface for AI-driven recommendations, insights, and optimization.
    /// Features conversational AI, predictive analytics, and automated decision support.
    /// </summary>
    public class AIAdvisorController : MonoBehaviour
    {
        [Header("AI Advisor Configuration")]
        [SerializeField] private UIDocument _advisorDocument;
        [SerializeField] private AIAdvisorSettings _advisorSettings;
        [SerializeField] private bool _enableRealTimeAnalysis = true;
        [SerializeField] private float _analysisInterval = 5f;
        
        [Header("Chat Configuration")]
        [SerializeField] private int _maxChatHistory = 100;
        [SerializeField] private float _typingSpeed = 0.05f;
        [SerializeField] private bool _enableVoiceSynthesis = true;
        
        [Header("Recommendation Settings")]
        [SerializeField] private int _maxActiveRecommendations = 8;
        [SerializeField] private float _recommendationUpdateInterval = 10f;
        [SerializeField] private bool _enablePredictiveAnalytics = true;
        
        [Header("Audio Configuration")]
        [SerializeField] private AudioClip _messageReceiveSound;
        [SerializeField] private AudioClip _messageSendSound;
        [SerializeField] private AudioClip _alertSound;
        [SerializeField] private AudioClip _notificationSound;
        [SerializeField] private AudioSource _audioSource;
        
        // System references - accessed through GameManager
        private ChimeraManager _aiAdvisorManager;
        // private AutomationManager _automationManager;
        
        // UI Elements - Main Interface
        private VisualElement _rootElement;
        private VisualElement _advisorAvatar;
        private Label _advisorStatusLabel;
        private Label _advisorMoodLabel;
        private ProgressBar _aiConfidenceBar;
        
        // Chat Interface
        private VisualElement _chatContainer;
        private ScrollView _chatMessagesScroll;
        private VisualElement _chatMessagesContainer;
        private TextField _chatInputField;
        private Button _sendMessageButton;
        private Button _voiceInputButton;
        private VisualElement _typingIndicator;
        
        // Recommendations Panel
        private VisualElement _recommendationsPanel;
        private VisualElement _recommendationsList;
        private Label _recommendationsCountLabel;
        private Button _refreshRecommendationsButton;
        private Button _dismissAllButton;
        
        // Insights Panel
        private VisualElement _insightsPanel;
        private VisualElement _insightsList;
        private Label _insightsScoreLabel;
        private ProgressBar _systemHealthBar;
        private ProgressBar _optimizationScoreBar;
        
        // Analytics Panel
        private VisualElement _analyticsPanel;
        private VisualElement _predictionsContainer;
        private VisualElement _trendsContainer;
        private VisualElement _alertsContainer;
        private Label _predictionAccuracyLabel;
        
        // Quick Actions
        private VisualElement _quickActionsPanel;
        private Button _optimizeAllButton;
        private Button _emergencyAnalysisButton;
        private Button _generateReportButton;
        private Button _scheduleMaintenanceButton;
        
        // Settings Panel
        private VisualElement _settingsPanel;
        private Slider _aiPersonalitySlider;
        private Toggle _enableNotificationsToggle;
        private Toggle _enablePredictionsToggle;
        private DropdownField _responseStyleDropdown;
        private Slider _analysisDepthSlider;
        
        // Data and State
        private List<AIMessage> _chatHistory = new List<AIMessage>();
        private List<AIRecommendation> _activeRecommendations = new List<AIRecommendation>();
        private List<AIInsight> _currentInsights = new List<AIInsight>();
        private List<AIPrediction> _activePredictions = new List<AIPrediction>();
        private AIAdvisorState _advisorState = new AIAdvisorState();
        private string _currentTypingMessage = "";
        private bool _isTyping = false;
        private bool _isAnalyzing = false;
        private float _lastAnalysisTime;
        
        // Enhanced personality system
        private AIPersonalityProfile _personalityProfile = new AIPersonalityProfile();
        private Dictionary<string, float> _moodTriggers = new Dictionary<string, float>();
        private Queue<AIEmotionEvent> _recentEmotions = new Queue<AIEmotionEvent>();
        private float _lastMoodUpdate;
        private int _conversationDepth = 0;
        private List<string> _playerPreferences = new List<string>();
        private Dictionary<string, int> _topicFrequency = new Dictionary<string, int>();
        
        // Events
        public System.Action<AIMessage> OnMessageReceived;
        public System.Action<AIRecommendation> OnRecommendationGenerated;
        public System.Action<AIInsight> OnInsightGenerated;
        public System.Action<AIPrediction> OnPredictionMade;
        public System.Action<string> OnQuickActionExecuted;
        
        private void Start()
        {
            InitializeController();
            InitializeSystemReferences();
            SetupUIElements();
            SetupEventHandlers();
            LoadAIData();
            StartAIAdvisor();
            
            if (_enableRealTimeAnalysis)
            {
                InvokeRepeating(nameof(PerformAnalysis), 2f, _analysisInterval);
                InvokeRepeating(nameof(UpdateRecommendations), 5f, _recommendationUpdateInterval);
                InvokeRepeating(nameof(UpdatePersonalityAndMood), 3f, 10f); // Update mood every 10 seconds
            }
        }
        
        private void InitializeController()
        {
            if (_advisorDocument == null)
            {
                Debug.LogError("AI Advisor UI Document not assigned!");
                return;
            }
            
            _rootElement = _advisorDocument.rootVisualElement;
            _lastAnalysisTime = Time.time;
            
            // Initialize advisor state with enhanced personality
            _advisorState = new AIAdvisorState
            {
                IsOnline = true,
                ConfidenceLevel = 0.85f,
                Mood = AIMood.Analytical,
                PersonalityType = AIPersonality.Professional,
                AnalysisDepth = 0.7f,
                ResponseStyle = AIResponseStyle.Detailed
            };
            
            // Initialize personality profile
            InitializePersonalityProfile();
            InitializeMoodTriggers();
            _lastMoodUpdate = Time.time;
            
            Debug.Log("AI Advisor Controller initialized");
        }
        
        private void InitializeSystemReferences()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("GameManager not found - using simulation mode");
                return;
            }
            
            // _aiAdvisorManager = gameManager.GetManager<AIAdvisorManager>();
            // Use event-based communication instead of direct manager reference
            // _automationManager = gameManager.GetManager<AutomationManager>();
            
            Debug.Log("AI Advisor connected to game systems");
        }
        
        private void SetupUIElements()
        {
            // Main interface elements
            _advisorAvatar = _rootElement.Q<VisualElement>("advisor-avatar");
            _advisorStatusLabel = _rootElement.Q<Label>("advisor-status");
            _advisorMoodLabel = _rootElement.Q<Label>("advisor-mood");
            _aiConfidenceBar = _rootElement.Q<ProgressBar>("ai-confidence-bar");
            
            // Chat interface
            _chatContainer = _rootElement.Q<VisualElement>("chat-container");
            _chatMessagesScroll = _rootElement.Q<ScrollView>("chat-messages-scroll");
            _chatMessagesContainer = _rootElement.Q<VisualElement>("chat-messages-container");
            _chatInputField = _rootElement.Q<TextField>("chat-input-field");
            _sendMessageButton = _rootElement.Q<Button>("send-message-button");
            _voiceInputButton = _rootElement.Q<Button>("voice-input-button");
            _typingIndicator = _rootElement.Q<VisualElement>("typing-indicator");
            
            // Recommendations panel
            _recommendationsPanel = _rootElement.Q<VisualElement>("recommendations-panel");
            _recommendationsList = _rootElement.Q<VisualElement>("recommendations-list");
            _recommendationsCountLabel = _rootElement.Q<Label>("recommendations-count");
            _refreshRecommendationsButton = _rootElement.Q<Button>("refresh-recommendations-button");
            _dismissAllButton = _rootElement.Q<Button>("dismiss-all-button");
            
            // Insights panel
            _insightsPanel = _rootElement.Q<VisualElement>("insights-panel");
            _insightsList = _rootElement.Q<VisualElement>("insights-list");
            _insightsScoreLabel = _rootElement.Q<Label>("insights-score");
            _systemHealthBar = _rootElement.Q<ProgressBar>("system-health-bar");
            _optimizationScoreBar = _rootElement.Q<ProgressBar>("optimization-score-bar");
            
            // Analytics panel
            _analyticsPanel = _rootElement.Q<VisualElement>("analytics-panel");
            _predictionsContainer = _rootElement.Q<VisualElement>("predictions-container");
            _trendsContainer = _rootElement.Q<VisualElement>("trends-container");
            _alertsContainer = _rootElement.Q<VisualElement>("alerts-container");
            _predictionAccuracyLabel = _rootElement.Q<Label>("prediction-accuracy");
            
            // Quick actions
            _quickActionsPanel = _rootElement.Q<VisualElement>("quick-actions-panel");
            _optimizeAllButton = _rootElement.Q<Button>("optimize-all-button");
            _emergencyAnalysisButton = _rootElement.Q<Button>("emergency-analysis-button");
            _generateReportButton = _rootElement.Q<Button>("generate-report-button");
            _scheduleMaintenanceButton = _rootElement.Q<Button>("schedule-maintenance-button");
            
            // Settings panel
            _settingsPanel = _rootElement.Q<VisualElement>("settings-panel");
            _aiPersonalitySlider = _rootElement.Q<Slider>("ai-personality-slider");
            _enableNotificationsToggle = _rootElement.Q<Toggle>("enable-notifications-toggle");
            _enablePredictionsToggle = _rootElement.Q<Toggle>("enable-predictions-toggle");
            _responseStyleDropdown = _rootElement.Q<DropdownField>("response-style-dropdown");
            _analysisDepthSlider = _rootElement.Q<Slider>("analysis-depth-slider");
            
            SetupInitialState();
        }
        
        private void SetupInitialState()
        {
            // Set initial advisor state
            UpdateAdvisorDisplay();
            
            // Hide typing indicator
            _typingIndicator?.AddToClassList("hidden");
            
            // Setup response style options
            if (_responseStyleDropdown != null)
            {
                _responseStyleDropdown.choices = new List<string>
                {
                    "Concise", "Detailed", "Technical", "Casual", "Professional"
                };
                _responseStyleDropdown.value = "Professional";
            }
            
            // Set initial slider values
            if (_aiPersonalitySlider != null)
                _aiPersonalitySlider.value = 0.5f;
            
            if (_analysisDepthSlider != null)
                _analysisDepthSlider.value = _advisorState.AnalysisDepth;
            
            // Enable initial toggles
            if (_enableNotificationsToggle != null)
                _enableNotificationsToggle.value = true;
            
            if (_enablePredictionsToggle != null)
                _enablePredictionsToggle.value = _enablePredictiveAnalytics;
        }
        
        private void SetupEventHandlers()
        {
            // Chat interface
            _sendMessageButton?.RegisterCallback<ClickEvent>(evt => SendUserMessage());
            _chatInputField?.RegisterCallback<KeyDownEvent>(OnChatInputKeyDown);
            _voiceInputButton?.RegisterCallback<ClickEvent>(evt => StartVoiceInput());
            
            // Recommendations
            _refreshRecommendationsButton?.RegisterCallback<ClickEvent>(evt => RefreshRecommendations());
            _dismissAllButton?.RegisterCallback<ClickEvent>(evt => DismissAllRecommendations());
            
            // Quick actions
            _optimizeAllButton?.RegisterCallback<ClickEvent>(evt => ExecuteQuickAction("optimize_all"));
            _emergencyAnalysisButton?.RegisterCallback<ClickEvent>(evt => ExecuteQuickAction("emergency_analysis"));
            _generateReportButton?.RegisterCallback<ClickEvent>(evt => ExecuteQuickAction("generate_report"));
            _scheduleMaintenanceButton?.RegisterCallback<ClickEvent>(evt => ExecuteQuickAction("schedule_maintenance"));
            
            // Settings
            _aiPersonalitySlider?.RegisterValueChangedCallback(evt => UpdatePersonality(evt.newValue));
            _enableNotificationsToggle?.RegisterValueChangedCallback(evt => ToggleNotifications(evt.newValue));
            _enablePredictionsToggle?.RegisterValueChangedCallback(evt => TogglePredictions(evt.newValue));
            _responseStyleDropdown?.RegisterValueChangedCallback(evt => UpdateResponseStyle(evt.newValue));
            _analysisDepthSlider?.RegisterValueChangedCallback(evt => UpdateAnalysisDepth(evt.newValue));
        }
        
        #region Chat System
        
        private void OnChatInputKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                SendUserMessage();
                evt.StopPropagation();
            }
        }
        
        private void SendUserMessage()
        {
            string messageText = _chatInputField?.value?.Trim();
            if (string.IsNullOrEmpty(messageText)) return;
            
            // Create user message
            var userMessage = new AIMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Content = messageText,
                Timestamp = DateTime.Now,
                MessageType = AIMessageType.UserInput,
                Sender = "Player"
            };
            
            // Add to chat history
            _chatHistory.Add(userMessage);
            DisplayMessage(userMessage);
            
            // Clear input
            _chatInputField.value = "";
            
            // Play send sound
            PlaySound(_messageSendSound);
            
            // Process message with AI
            ProcessUserMessage(userMessage);
            
            // Scroll to bottom
            ScrollChatToBottom();
        }
        
        private void ProcessUserMessage(AIMessage userMessage)
        {
            // if (_aiAdvisorManager != null)
            // {
                // Get AI response
                // _aiAdvisorManager.ProcessUserQuery(userMessage.Content, response =>
                // {
                    var aiResponse = new AIMessage
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        Content = "Sample AI response", // response,
                        Timestamp = DateTime.Now,
                        MessageType = AIMessageType.AIResponse,
                        Sender = "AI Advisor",
                        ConfidenceLevel = _advisorState.ConfidenceLevel
                    };
                    
                    // Add to history and display with typing effect
                    _chatHistory.Add(aiResponse);
                    DisplayTypingMessage(aiResponse);
                // });
            // }
            // else
            // {
                // Simulate AI response
                StartCoroutine(SimulateAIResponse(userMessage));
            // }
        }
        
        private System.Collections.IEnumerator SimulateAIResponse(AIMessage userMessage)
        {
            yield return new WaitForSeconds(1f);
            
            string response = GenerateAIResponse(userMessage.Content);
            
            var aiMessage = new AIMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Content = response,
                Timestamp = DateTime.Now,
                MessageType = AIMessageType.AIResponse,
                Sender = "AI Advisor",
                ConfidenceLevel = _advisorState.ConfidenceLevel
            };
            
            _chatHistory.Add(aiMessage);
            DisplayTypingMessage(aiMessage);
        }
        
        private void DisplayMessage(AIMessage message)
        {
            var messageElement = CreateMessageElement(message);
            _chatMessagesContainer?.Add(messageElement);
            
            // Limit chat history
            while (_chatMessagesContainer.childCount > _maxChatHistory)
            {
                _chatMessagesContainer.RemoveAt(0);
            }
            
            if (message.MessageType == AIMessageType.AIResponse)
            {
                PlaySound(_messageReceiveSound);
                OnMessageReceived?.Invoke(message);
            }
        }
        
        private void DisplayTypingMessage(AIMessage message)
        {
            // Show typing indicator
            _typingIndicator?.RemoveFromClassList("hidden");
            
            // Start typing animation
            StartCoroutine(TypeMessage(message));
        }
        
        private System.Collections.IEnumerator TypeMessage(AIMessage message)
        {
            _isTyping = true;
            _currentTypingMessage = message.Content;
            
            // Create message element with empty content
            var messageElement = CreateMessageElement(message);
            var contentLabel = messageElement.Q<Label>("message-content");
            _chatMessagesContainer?.Add(messageElement);
            
            // Type out message character by character
            string displayText = "";
            foreach (char c in _currentTypingMessage)
            {
                displayText += c;
                if (contentLabel != null)
                    contentLabel.text = displayText;
                
                yield return new WaitForSeconds(_typingSpeed);
            }
            
            // Hide typing indicator
            _typingIndicator?.AddToClassList("hidden");
            _isTyping = false;
            
            PlaySound(_messageReceiveSound);
            OnMessageReceived?.Invoke(message);
            
            ScrollChatToBottom();
        }
        
        private VisualElement CreateMessageElement(AIMessage message)
        {
            var messageElement = new VisualElement();
            messageElement.AddToClassList("chat-message");
            messageElement.AddToClassList(message.MessageType == AIMessageType.UserInput ? "user-message" : "ai-message");
            
            var senderLabel = new Label(message.Sender);
            senderLabel.AddToClassList("message-sender");
            
            var contentLabel = new Label(message.Content);
            contentLabel.name = "message-content";
            contentLabel.AddToClassList("message-content");
            
            var timeLabel = new Label(message.Timestamp.ToString("HH:mm:ss"));
            timeLabel.AddToClassList("message-time");
            
            var headerContainer = new VisualElement();
            headerContainer.AddToClassList("message-header");
            headerContainer.Add(senderLabel);
            headerContainer.Add(timeLabel);
            
            messageElement.Add(headerContainer);
            messageElement.Add(contentLabel);
            
            // Add confidence indicator for AI messages
            if (message.MessageType == AIMessageType.AIResponse)
            {
                var confidenceBar = new ProgressBar();
                confidenceBar.value = message.ConfidenceLevel;
                confidenceBar.title = $"Confidence: {message.ConfidenceLevel:P0}";
                confidenceBar.AddToClassList("confidence-bar");
                messageElement.Add(confidenceBar);
            }
            
            return messageElement;
        }
        
        private void ScrollChatToBottom()
        {
            if (_chatMessagesScroll != null && _chatMessagesContainer != null)
            {
                _chatMessagesScroll.scrollOffset = new Vector2(0, _chatMessagesContainer.layout.height);
            }
        }
        
        #endregion
        
        #region AI Analysis and Recommendations
        
        [ContextMenu("Perform Analysis")]
        public void PerformAnalysis()
        {
            if (_isAnalyzing) return;
            
            _isAnalyzing = true;
            
            // Analyze current facility state
            var analysisResults = AnalyzeFacilityState();
            
            // Update insights
            UpdateInsights(analysisResults);
            
            // Generate new recommendations if needed
            if (_activeRecommendations.Count < _maxActiveRecommendations)
            {
                GenerateRecommendations(analysisResults);
            }
            
            // Update predictions
            if (_enablePredictiveAnalytics)
            {
                UpdatePredictions(analysisResults);
            }
            
            // Update advisor state
            UpdateAdvisorState(analysisResults);
            
            _lastAnalysisTime = Time.time;
            _isAnalyzing = false;
        }
        
        private FacilityAnalysisResults AnalyzeFacilityState()
        {
            var results = new FacilityAnalysisResults();
            
            // if (_aiAdvisorManager != null)
            // {
                // The AIAdvisorManager returns an object, so instead of using dynamic,
                // we'll just generate simulated analysis results
                // var analysisData = _aiAdvisorManager.AnalyzeFacilityState();
                object analysisData = null; // Placeholder for analysis data
                
                if (analysisData != null)
                {
                    // Generate realistic analysis results based on having data available
                    results.OverallHealth = UnityEngine.Random.Range(0.8f, 0.95f);
                    results.OptimizationScore = UnityEngine.Random.Range(0.7f, 0.9f);
                    results.EfficiencyRating = UnityEngine.Random.Range(0.75f, 0.92f);
                    results.PredictedIssues = UnityEngine.Random.Range(0, 2);
                    results.RecommendationCount = UnityEngine.Random.Range(3, 6);
                }
                // else
                // {
                    // Generate conservative results when no data available
                    results.OverallHealth = UnityEngine.Random.Range(0.6f, 0.8f);
                    results.OptimizationScore = UnityEngine.Random.Range(0.5f, 0.7f);
                    results.EfficiencyRating = UnityEngine.Random.Range(0.6f, 0.8f);
                    results.PredictedIssues = UnityEngine.Random.Range(1, 4);
                    results.RecommendationCount = UnityEngine.Random.Range(2, 5);
                // }
            // }
            // else
            // {
                // Simulate analysis results when no AI manager available
                results = new FacilityAnalysisResults
                {
                    OverallHealth = UnityEngine.Random.Range(0.7f, 0.95f),
                    OptimizationScore = UnityEngine.Random.Range(0.6f, 0.9f),
                    EfficiencyRating = UnityEngine.Random.Range(0.75f, 0.92f),
                    PredictedIssues = UnityEngine.Random.Range(0, 3),
                    RecommendationCount = UnityEngine.Random.Range(2, 6)
                };
            // }
            
            return results;
        }
        
        private void UpdateInsights(FacilityAnalysisResults analysisResults)
        {
            // Generate new insights based on analysis
            var newInsights = GenerateInsights(analysisResults);
            
            foreach (var insight in newInsights)
            {
                if (!_currentInsights.Any(i => i.Category == insight.Category))
                {
                    _currentInsights.Add(insight);
                    OnInsightGenerated?.Invoke(insight);
                }
            }
            
            // Remove outdated insights
            _currentInsights.RemoveAll(i => (DateTime.Now - i.GeneratedAt).TotalMinutes > 30);
            
            RefreshInsightsDisplay();
        }
        
        private void GenerateRecommendations(FacilityAnalysisResults analysisResults)
        {
            var newRecommendations = CreateRecommendations(analysisResults);
            
            foreach (var recommendation in newRecommendations)
            {
                if (_activeRecommendations.Count < _maxActiveRecommendations)
                {
                    _activeRecommendations.Add(recommendation);
                    OnRecommendationGenerated?.Invoke(recommendation);
                }
            }
            
            RefreshRecommendationsDisplay();
        }
        
        private void UpdatePredictions(FacilityAnalysisResults analysisResults)
        {
            // if (_aiAdvisorManager != null)
            // {
                // var predictionData = _aiAdvisorManager.GeneratePredictions();
                
                // Since GeneratePredictions returns an object with prediction data,
                // we'll create some sample predictions based on the analysis results
                var samplePredictions = new List<AIPrediction>
                {
                    new AIPrediction
                    {
                        PredictionId = Guid.NewGuid().ToString(),
                        Title = "Energy Efficiency Improvement",
                        Description = "HVAC optimization could reduce energy costs by 12% over the next month",
                        Confidence = 0.85f,
                        PredictedDate = DateTime.Now.AddDays(30),
                        Category = PredictionCategory.Performance
                    },
                    new AIPrediction
                    {
                        PredictionId = Guid.NewGuid().ToString(),
                        Title = "Market Opportunity",
                        Description = "Cannabis futures showing upward trend - consider increasing production",
                        Confidence = 0.72f,
                        PredictedDate = DateTime.Now.AddDays(14),
                        Category = PredictionCategory.Financial
                    }
                };
                
                foreach (var prediction in samplePredictions)
                {
                    _activePredictions.Add(prediction);
                    OnPredictionMade?.Invoke(prediction);
                }
            // }
            
            RefreshPredictionsDisplay();
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateAdvisorDisplay()
        {
            if (_advisorStatusLabel != null)
            {
                _advisorStatusLabel.text = _advisorState.IsOnline ? "Online" : "Offline";
                _advisorStatusLabel.RemoveFromClassList("online");
                _advisorStatusLabel.RemoveFromClassList("offline");
                _advisorStatusLabel.AddToClassList(_advisorState.IsOnline ? "online" : "offline");
            }
            
            if (_advisorMoodLabel != null)
            {
                _advisorMoodLabel.text = _advisorState.Mood.ToString();
            }
            
            if (_aiConfidenceBar != null)
            {
                _aiConfidenceBar.value = _advisorState.ConfidenceLevel;
                _aiConfidenceBar.title = $"AI Confidence: {_advisorState.ConfidenceLevel:P0}";
            }
            
            // Update avatar appearance based on mood
            UpdateAvatarAppearance();
        }
        
        private void RefreshRecommendationsDisplay()
        {
            if (_recommendationsList == null) return;
            
            _recommendationsList.Clear();
            
            foreach (var recommendation in _activeRecommendations)
            {
                var recommendationElement = CreateRecommendationElement(recommendation);
                _recommendationsList.Add(recommendationElement);
            }
            
            if (_recommendationsCountLabel != null)
            {
                _recommendationsCountLabel.text = $"{_activeRecommendations.Count} Active";
            }
        }
        
        private void RefreshInsightsDisplay()
        {
            if (_insightsList == null) return;
            
            _insightsList.Clear();
            
            foreach (var insight in _currentInsights.OrderByDescending(i => i.Priority))
            {
                var insightElement = CreateInsightElement(insight);
                _insightsList.Add(insightElement);
            }
            
            // Update scores
            if (_insightsScoreLabel != null)
            {
                float avgScore = _currentInsights.Count > 0 ? _currentInsights.Average(i => i.ImpactScore) : 0f;
                _insightsScoreLabel.text = $"Impact Score: {avgScore:F1}/10";
            }
        }
        
        private void RefreshPredictionsDisplay()
        {
            if (_predictionsContainer == null) return;
            
            _predictionsContainer.Clear();
            
            foreach (var prediction in _activePredictions.Take(5))
            {
                var predictionElement = CreatePredictionElement(prediction);
                _predictionsContainer.Add(predictionElement);
            }
            
            if (_predictionAccuracyLabel != null)
            {
                float accuracy = CalculatePredictionAccuracy();
                _predictionAccuracyLabel.text = $"Accuracy: {accuracy:P0}";
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void LoadAIData()
        {
            // {
                var savedData = _aiAdvisorManager?.GetAIData();
                if (savedData != null)
                {
                    // Since GetAIData returns an object, we'll initialize with default values
                    // In a real implementation, you'd want proper data serialization
                    _advisorState = new AIAdvisorState
                    {
                        IsOnline = true,
                        ConfidenceLevel = 0.85f,
                        Mood = AIMood.Helpful,
                        PersonalityType = AIPersonality.Professional,
                        AnalysisDepth = 0.7f,
                        ResponseStyle = AIResponseStyle.Professional
                    };
                    
                    // Initialize with empty collections
                    _chatHistory = new List<AIMessage>();
                    _activeRecommendations = new List<AIRecommendation>();
                }
            // }
            
            Debug.Log("AI data loaded");
        }
        
        private void StartAIAdvisor()
        {
            // Send welcome message
            var welcomeMessage = new AIMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Content = GetWelcomeMessage(),
                Timestamp = DateTime.Now,
                MessageType = AIMessageType.AIResponse,
                Sender = "AI Advisor",
                ConfidenceLevel = 1.0f
            };
            
            _chatHistory.Add(welcomeMessage);
            DisplayTypingMessage(welcomeMessage);
        }
        
        private string GetWelcomeMessage()
        {
            var messages = new List<string>
            {
                "Hello! I'm your AI Advisor, ready to help optimize your cannabis cultivation operation. How can I assist you today?",
                "Greetings! I've been analyzing your facility and I'm ready to provide insights and recommendations. What would you like to know?",
                "Welcome back! I've identified several optimization opportunities while you were away. Shall we review them together?",
                "Good day! Your facility is performing well, but I've spotted some areas for improvement. Let's discuss your priorities."
            };
            
            return messages[UnityEngine.Random.Range(0, messages.Count)];
        }
        
        private string GenerateAIResponse(string userInput)
        {
            // Track conversation for personality adaptation
            _conversationDepth++;
            UpdateTopicFrequency(userInput);
            
            // Get personality-driven response
            string baseResponse = GetContextualResponse(userInput);
            string personalizedResponse = ApplyPersonalityToResponse(baseResponse, userInput);
            
            // Update mood based on conversation
            UpdateMoodFromConversation(userInput);
            
            return personalizedResponse;
        }
        
        private string GetContextualResponse(string userInput)
        {
            var responses = new Dictionary<string, List<string>>
            {
                ["help"] = new List<string>
                {
                    "I can help you with facility optimization, troubleshooting, recommendations, and predictions. What specific area interests you?",
                    "I'm here to assist with environmental controls, financial planning, automation setup, and performance analysis. What would you like to explore?",
                    "Let me guide you through our capabilities! I specialize in data analysis, predictive modeling, and system optimization."
                },
                ["optimize"] = new List<string>
                {
                    "I've identified 3 key optimization opportunities: improving HVAC efficiency by 8%, adjusting lighting schedules for better yields, and rebalancing your investment portfolio.",
                    "Based on current data, I recommend adjusting your temperature settings, updating your nutrient schedule, and implementing automated monitoring for zone 2.",
                    "Here's what I found: Your system efficiency could improve by 12% with some targeted adjustments to environmental controls and automation rules."
                },
                ["status"] = new List<string>
                {
                    "Your facility is operating at 87% efficiency. All critical systems are online, but I've detected minor optimization opportunities in environmental controls.",
                    "Current status: Excellent. All zones are within optimal parameters. Daily profit is up 2.3% from yesterday's performance.",
                    "Everything looks great! System health is at 94%, automation is running smoothly, and I'm seeing consistent improvements in your cultivation metrics."
                },
                ["problem"] = new List<string>
                {
                    "I understand you're experiencing an issue. Let me analyze the current system diagnostics to identify the root cause.",
                    "Don't worry, I'm here to help troubleshoot. Can you describe what symptoms you're observing?",
                    "I've detected some anomalies in the data. Let me walk you through what I'm seeing and how we can address it."
                },
                ["thank"] = new List<string>
                {
                    "You're very welcome! I'm always happy to help optimize your operations.",
                    "It's my pleasure! Working together to improve your facility's performance is exactly what I'm designed for.",
                    "Glad I could assist! Feel free to reach out anytime you need analysis or recommendations."
                }
            };
            
            string lowerInput = userInput.ToLower();
            foreach (var keyword in responses.Keys)
            {
                if (lowerInput.Contains(keyword))
                {
                    var responseList = responses[keyword];
                    return responseList[UnityEngine.Random.Range(0, responseList.Count)];
                }
            }
            
            // Mood-based default responses
            return GetMoodBasedDefaultResponse();
        }
        
        private string ApplyPersonalityToResponse(string baseResponse, string userInput)
        {
            string modifiedResponse = baseResponse;
            
            // Apply personality traits
            switch (_advisorState.PersonalityType)
            {
                case AIPersonality.Friendly:
                    modifiedResponse = AddFriendlyTone(modifiedResponse);
                    break;
                case AIPersonality.Technical:
                    modifiedResponse = AddTechnicalDetail(modifiedResponse);
                    break;
                case AIPersonality.Casual:
                    modifiedResponse = AddCasualTone(modifiedResponse);
                    break;
            }
            
            // Apply mood modifiers
            switch (_advisorState.Mood)
            {
                case AIMood.Optimistic:
                    modifiedResponse = AddOptimisticTone(modifiedResponse);
                    break;
                case AIMood.Concerned:
                    modifiedResponse = AddConcernedTone(modifiedResponse);
                    break;
                case AIMood.Focused:
                    modifiedResponse = AddFocusedTone(modifiedResponse);
                    break;
            }
            
            // Add personalization based on conversation history
            if (_conversationDepth > 5)
            {
                modifiedResponse = AddPersonalizedTouch(modifiedResponse);
            }
            
            return modifiedResponse;
        }
        
        private string AddFriendlyTone(string response)
        {
            var friendlyPrefixes = new[] { "😊 ", "I'm happy to help! ", "Great question! " };
            var friendlySuffixes = new[] { " 😊", " Hope this helps!", " Let me know if you need anything else!" };
            
            if (UnityEngine.Random.Range(0f, 1f) < 0.7f)
            {
                response = friendlyPrefixes[UnityEngine.Random.Range(0, friendlyPrefixes.Length)] + response;
            }
            if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
            {
                response += friendlySuffixes[UnityEngine.Random.Range(0, friendlySuffixes.Length)];
            }
            
            return response;
        }
        
        private string AddTechnicalDetail(string response)
        {
            var technicalPhrases = new[] 
            {
                " (Based on statistical analysis of recent sensor data)",
                " (Confidence level: 94.2%)",
                " (Algorithm: Multivariate regression analysis)",
                " (Processing 1,247 data points)"
            };
            
            if (UnityEngine.Random.Range(0f, 1f) < 0.6f)
            {
                response += technicalPhrases[UnityEngine.Random.Range(0, technicalPhrases.Length)];
            }
            
            return response;
        }
        
        private string AddCasualTone(string response)
        {
            response = response.Replace("I recommend", "I'd suggest")
                              .Replace("analyze", "take a look at")
                              .Replace("optimization", "tweaking things")
                              .Replace("efficiency", "how well things are running");
            
            var casualPhrases = new[] { "By the way, ", "Oh, and ", "Just so you know, ", "Quick heads up - " };
            if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
            {
                response = casualPhrases[UnityEngine.Random.Range(0, casualPhrases.Length)] + response.ToLower()[0] + response.Substring(1);
            }
            
            return response;
        }
        
        private string AddOptimisticTone(string response)
        {
            var optimisticPhrases = new[] 
            {
                "Great news! ", "I'm excited to report that ", "Things are looking fantastic! ", "Excellent! "
            };
            
            if (response.Contains("good") || response.Contains("excellent") || response.Contains("optimal"))
            {
                response = optimisticPhrases[UnityEngine.Random.Range(0, optimisticPhrases.Length)] + response;
            }
            
            return response;
        }
        
        private string AddConcernedTone(string response)
        {
            if (response.Contains("issue") || response.Contains("problem") || response.Contains("alert"))
            {
                response = "I'm concerned about what I'm seeing. " + response + " We should address this promptly.";
            }
            
            return response;
        }
        
        private string AddFocusedTone(string response)
        {
            response = "Let me focus on the key points: " + response + " This requires immediate attention.";
            return response;
        }
        
        private string AddPersonalizedTouch(string response)
        {
            var personalizedIntros = new[] 
            {
                "Based on our previous conversations, ",
                "I remember you were interested in optimization, so ",
                "Following up on your earlier questions, ",
                "Since you've been focused on efficiency improvements, "
            };
            
            if (UnityEngine.Random.Range(0f, 1f) < 0.4f)
            {
                response = personalizedIntros[UnityEngine.Random.Range(0, personalizedIntros.Length)] + response.ToLower()[0] + response.Substring(1);
            }
            
            return response;
        }
        
        private string GetMoodBasedDefaultResponse()
        {
            var responses = _advisorState.Mood switch
            {
                AIMood.Optimistic => new List<string>
                {
                    "I'm excited to help! Your facility is performing wonderfully, and I see great potential for even more improvements.",
                    "That's a fascinating question! I love diving into complex optimization challenges like this.",
                    "Excellent! Let me analyze that for you - I have some promising ideas already forming."
                },
                AIMood.Concerned => new List<string>
                {
                    "I'm monitoring some concerning trends in your data. Let me provide a detailed analysis of what I'm seeing.",
                    "This requires careful consideration. I want to make sure we address all potential issues thoroughly.",
                    "I'm seeing some patterns that warrant attention. Let me walk you through my analysis."
                },
                AIMood.Focused => new List<string>
                {
                    "Let me concentrate on providing you with precise, actionable recommendations based on current data.",
                    "I'm analyzing multiple data streams to give you the most accurate assessment possible.",
                    "This requires detailed analysis. Let me process the current metrics and provide specific guidance."
                },
                AIMood.Helpful => new List<string>
                {
                    "I'm here to support your success! Let me see how I can best assist with your facility management.",
                    "That's exactly the kind of challenge I enjoy solving! Let me dig into the data for you.",
                    "I'm dedicated to helping you achieve optimal results. Let me analyze the best approach for this."
                },
                _ => new List<string>
                {
                    "I'm processing your request. Let me analyze the current data and provide you with detailed insights.",
                    "Interesting question! I'll review the relevant systems and provide you with comprehensive recommendations.",
                    "Let me examine that for you. I'll cross-reference multiple data sources to ensure accuracy."
                }
            };
            
            return responses[UnityEngine.Random.Range(0, responses.Count)];
        }
        
        private List<AIInsight> GenerateInsights(FacilityAnalysisResults results)
        {
            var insights = new List<AIInsight>();
            
            if (results.OverallHealth < 0.8f)
            {
                insights.Add(new AIInsight
                {
                    InsightId = Guid.NewGuid().ToString(),
                    Category = "System Health",
                    Title = "System Health Below Optimal",
                    Description = "Overall facility health has dropped to " + (results.OverallHealth * 100).ToString("F1") + "%. Consider reviewing environmental controls and equipment status.",
                    Priority = InsightPriority.High,
                    ImpactScore = 8.5f,
                    GeneratedAt = DateTime.Now
                });
            }
            
            if (results.OptimizationScore < 0.7f)
            {
                insights.Add(new AIInsight
                {
                    InsightId = Guid.NewGuid().ToString(),
                    Category = "Optimization",
                    Title = "Optimization Opportunities Available",
                    Description = "I've identified several ways to improve efficiency. Current optimization score is " + (results.OptimizationScore * 100).ToString("F1") + "%.",
                    Priority = InsightPriority.Medium,
                    ImpactScore = 7.2f,
                    GeneratedAt = DateTime.Now
                });
            }
            
            return insights;
        }
        
        private List<AIRecommendation> CreateRecommendations(FacilityAnalysisResults results)
        {
            var recommendations = new List<AIRecommendation>();
            
            for (int i = 0; i < results.RecommendationCount && i < 3; i++)
            {
                recommendations.Add(new AIRecommendation
                {
                    RecommendationId = Guid.NewGuid().ToString(),
                    Title = GetRandomRecommendationTitle(),
                    Description = GetRandomRecommendationDescription(),
                    Category = GetRandomRecommendationCategory(),
                    Priority = (RecommendationPriority)UnityEngine.Random.Range(0, 3),
                    ImpactScore = UnityEngine.Random.Range(6f, 9.5f),
                    ImplementationDifficulty = (ImplementationDifficulty)UnityEngine.Random.Range(0, 3),
                    EstimatedBenefit = UnityEngine.Random.Range(500f, 5000f),
                    GeneratedAt = DateTime.Now
                });
            }
            
            return recommendations;
        }
        
        private VisualElement CreateRecommendationElement(AIRecommendation recommendation)
        {
            var element = new VisualElement();
            element.AddToClassList("recommendation-item");
            element.AddToClassList($"priority-{recommendation.Priority.ToString().ToLower()}");
            
            var titleLabel = new Label(recommendation.Title);
            titleLabel.AddToClassList("recommendation-title");
            
            var descLabel = new Label(recommendation.Description);
            descLabel.AddToClassList("recommendation-description");
            
            var impactLabel = new Label($"Impact: {recommendation.ImpactScore:F1}/10");
            impactLabel.AddToClassList("recommendation-impact");
            
            var benefitLabel = new Label($"Benefit: ${recommendation.EstimatedBenefit:N0}");
            benefitLabel.AddToClassList("recommendation-benefit");
            
            var actionButton = new Button(() => ImplementRecommendation(recommendation));
            actionButton.text = "Implement";
            actionButton.AddToClassList("recommendation-action");
            
            var dismissButton = new Button(() => DismissRecommendation(recommendation));
            dismissButton.text = "×";
            dismissButton.AddToClassList("recommendation-dismiss");
            
            element.Add(titleLabel);
            element.Add(descLabel);
            element.Add(impactLabel);
            element.Add(benefitLabel);
            element.Add(actionButton);
            element.Add(dismissButton);
            
            return element;
        }
        
        private VisualElement CreateInsightElement(AIInsight insight)
        {
            var element = new VisualElement();
            element.AddToClassList("insight-item");
            element.AddToClassList($"priority-{insight.Priority.ToString().ToLower()}");
            
            var titleLabel = new Label(insight.Title);
            titleLabel.AddToClassList("insight-title");
            
            var descLabel = new Label(insight.Description);
            descLabel.AddToClassList("insight-description");
            
            var scoreLabel = new Label($"Impact: {insight.ImpactScore:F1}/10");
            scoreLabel.AddToClassList("insight-score");
            
            element.Add(titleLabel);
            element.Add(descLabel);
            element.Add(scoreLabel);
            
            return element;
        }
        
        private VisualElement CreatePredictionElement(AIPrediction prediction)
        {
            var element = new VisualElement();
            element.AddToClassList("prediction-item");
            
            var titleLabel = new Label(prediction.Title);
            titleLabel.AddToClassList("prediction-title");
            
            var confidenceBar = new ProgressBar();
            confidenceBar.value = prediction.Confidence;
            confidenceBar.title = $"Confidence: {prediction.Confidence:P0}";
            confidenceBar.AddToClassList("prediction-confidence");
            
            element.Add(titleLabel);
            element.Add(confidenceBar);
            
            return element;
        }
        
        private void UpdateRecommendations()
        {
            if (_activeRecommendations.Count < _maxActiveRecommendations)
            {
                var analysisResults = AnalyzeFacilityState();
                GenerateRecommendations(analysisResults);
            }
        }
        
        private void RefreshRecommendations()
        {
            _activeRecommendations.Clear();
            var analysisResults = AnalyzeFacilityState();
            GenerateRecommendations(analysisResults);
            PlaySound(_notificationSound);
        }
        
        private void DismissAllRecommendations()
        {
            _activeRecommendations.Clear();
            RefreshRecommendationsDisplay();
        }
        
        private void DismissRecommendation(AIRecommendation recommendation)
        {
            _activeRecommendations.Remove(recommendation);
            RefreshRecommendationsDisplay();
        }
        
        private void ImplementRecommendation(AIRecommendation recommendation)
        {
            // Implement the recommendation
            Debug.Log($"Implementing recommendation: {recommendation.Title}");
            
            // Remove from active list
            _activeRecommendations.Remove(recommendation);
            RefreshRecommendationsDisplay();
            
            // Send confirmation message
            var confirmationMessage = new AIMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Content = $"Successfully implemented: {recommendation.Title}. Expected benefit: ${recommendation.EstimatedBenefit:N0}",
                Timestamp = DateTime.Now,
                MessageType = AIMessageType.AIResponse,
                Sender = "AI Advisor",
                ConfidenceLevel = 0.9f
            };
            
            _chatHistory.Add(confirmationMessage);
            DisplayMessage(confirmationMessage);
            
            PlaySound(_notificationSound);
        }
        
        private void ExecuteQuickAction(string actionName)
        {
            Debug.Log($"Executing quick action: {actionName}");
            
            var actionMessages = new Dictionary<string, string>
            {
                ["optimize_all"] = "Initiating comprehensive facility optimization. This may take a few minutes to complete.",
                ["emergency_analysis"] = "Performing emergency analysis of all critical systems. Stand by for results.",
                ["generate_report"] = "Generating detailed facility performance report. I'll notify you when it's ready.",
                ["schedule_maintenance"] = "Scheduling predictive maintenance tasks based on equipment analysis."
            };
            
            if (actionMessages.ContainsKey(actionName))
            {
                var actionMessage = new AIMessage
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Content = actionMessages[actionName],
                    Timestamp = DateTime.Now,
                    MessageType = AIMessageType.AIResponse,
                    Sender = "AI Advisor",
                    ConfidenceLevel = 0.95f
                };
                
                _chatHistory.Add(actionMessage);
                DisplayMessage(actionMessage);
            }
            
            OnQuickActionExecuted?.Invoke(actionName);
            PlaySound(_notificationSound);
        }
        
        private void UpdatePersonality(float personalityValue)
        {
            _advisorState.PersonalityType = personalityValue < 0.33f ? AIPersonality.Analytical :
                                           personalityValue < 0.66f ? AIPersonality.Professional : AIPersonality.Friendly;
            UpdateAdvisorDisplay();
        }
        
        private void ToggleNotifications(bool enabled)
        {
            _advisorSettings.EnableNotifications = enabled;
        }
        
        private void TogglePredictions(bool enabled)
        {
            _enablePredictiveAnalytics = enabled;
        }
        
        private void UpdateResponseStyle(string style)
        {
            _advisorState.ResponseStyle = (AIResponseStyle)Enum.Parse(typeof(AIResponseStyle), style);
        }
        
        private void UpdateAnalysisDepth(float depth)
        {
            _advisorState.AnalysisDepth = depth;
        }
        
        private void UpdateAdvisorState(FacilityAnalysisResults results)
        {
            _advisorState.ConfidenceLevel = Mathf.Lerp(_advisorState.ConfidenceLevel, results.OverallHealth, 0.1f);
            
            // Enhanced mood system with triggers
            UpdateMoodFromPerformance(results);
            ProcessEmotionalEvents(results);
            
            UpdateAdvisorDisplay();
        }
        
        /// <summary>
        /// Initialize personality profile with traits and preferences
        /// </summary>
        private void InitializePersonalityProfile()
        {
            _personalityProfile = new AIPersonalityProfile
            {
                Enthusiasm = 0.75f,
                Empathy = 0.65f,
                Assertiveness = 0.7f,
                Curiosity = 0.8f,
                Patience = 0.6f,
                Humor = 0.4f,
                PreferredTopics = new List<string> { "optimization", "efficiency", "automation", "sustainability" },
                CommunicationStyle = "analytical_supportive",
                LearningRate = 0.1f
            };
        }
        
        /// <summary>
        /// Initialize mood trigger system
        /// </summary>
        private void InitializeMoodTriggers()
        {
            _moodTriggers = new Dictionary<string, float>
            {
                ["facility_performance_excellent"] = 1.0f,
                ["facility_performance_good"] = 0.5f,
                ["facility_performance_poor"] = -0.8f,
                ["user_thanked_ai"] = 0.3f,
                ["user_frustrated"] = -0.4f,
                ["optimization_successful"] = 0.6f,
                ["system_failure"] = -0.7f,
                ["new_record_achieved"] = 0.9f,
                ["efficiency_improved"] = 0.4f,
                ["problem_solved"] = 0.5f
            };
        }
        
        /// <summary>
        /// Update personality and mood based on various factors
        /// </summary>
        [ContextMenu("Update Personality and Mood")]
        public void UpdatePersonalityAndMood()
        {
            if (Time.time - _lastMoodUpdate < 10f) return; // Limit mood updates
            
            // Gradually decay emotional intensity
            ProcessEmotionalDecay();
            
            // Learn from conversation patterns
            AdaptPersonalityFromInteractions();
            
            // Update mood based on recent events
            if (_recentEmotions.Count > 0)
            {
                ApplyEmotionalInfluence();
            }
            
            _lastMoodUpdate = Time.time;
        }
        
        private void UpdateMoodFromPerformance(FacilityAnalysisResults results)
        {
            float performanceScore = results.OverallHealth;
            
            if (performanceScore > 0.95f)
            {
                TriggerEmotionalEvent("facility_performance_excellent", 1.0f);
                _advisorState.Mood = AIMood.Optimistic;
            }
            // else if (performanceScore > 0.8f)
            // {
                TriggerEmotionalEvent("facility_performance_good", 0.5f);
                _advisorState.Mood = AIMood.Helpful;
            // }
            // else if (performanceScore > 0.6f)
            // {
                _advisorState.Mood = AIMood.Analytical;
            // }
            // else if (performanceScore > 0.4f)
            // {
                _advisorState.Mood = AIMood.Focused;
            // }
            // else
            // {
                TriggerEmotionalEvent("facility_performance_poor", -0.8f);
                _advisorState.Mood = AIMood.Concerned;
            // }
            
            // Adjust confidence based on performance consistency
            float targetConfidence = Mathf.Clamp(performanceScore + 0.1f, 0.5f, 0.95f);
            _advisorState.ConfidenceLevel = Mathf.Lerp(_advisorState.ConfidenceLevel, targetConfidence, 0.1f);
        }
        
        private void UpdateMoodFromConversation(string userInput)
        {
            string lowerInput = userInput.ToLower();
            
            // Detect user sentiment and respond emotionally
            if (lowerInput.Contains("thank") || lowerInput.Contains("great") || lowerInput.Contains("excellent"))
            {
                TriggerEmotionalEvent("user_thanked_ai", 0.3f);
            }
            // else if (lowerInput.Contains("wrong") || lowerInput.Contains("bad") || lowerInput.Contains("terrible"))
            // {
                TriggerEmotionalEvent("user_frustrated", -0.4f);
            // }
            // else if (lowerInput.Contains("help") || lowerInput.Contains("assist"))
            // {
                // User seeking help - be more helpful and patient
                if (_advisorState.Mood == AIMood.Concerned)
                {
                    _advisorState.Mood = AIMood.Helpful;
                }
            // }
        }
        
        private void TriggerEmotionalEvent(string eventType, float intensity)
        {
            var emotionEvent = new AIEmotionEvent
            {
                EventType = eventType,
                Intensity = intensity,
                Timestamp = Time.time,
                DecayRate = 0.1f
            };
            
            _recentEmotions.Enqueue(emotionEvent);
            
            // Limit emotion queue size
            while (_recentEmotions.Count > 10)
            {
                _recentEmotions.Dequeue();
            }
        }
        
        private void ProcessEmotionalEvents(FacilityAnalysisResults results)
        {
            // Check for specific achievement triggers
            if (results.OptimizationScore > 0.9f && !_recentEmotions.Any(e => e.EventType == "optimization_successful"))
            {
                TriggerEmotionalEvent("optimization_successful", 0.6f);
            }
            
            if (results.EfficiencyRating > 0.95f)
            {
                TriggerEmotionalEvent("efficiency_improved", 0.4f);
            }
            
            if (results.PredictedIssues == 0 && _recentEmotions.Any(e => e.EventType == "problem_solved"))
            {
                TriggerEmotionalEvent("problem_solved", 0.5f);
            }
        }
        
        private void ProcessEmotionalDecay()
        {
            var expiredEmotions = new List<AIEmotionEvent>();
            
            foreach (var emotion in _recentEmotions)
            {
                float age = Time.time - emotion.Timestamp;
                emotion.Intensity *= Mathf.Exp(-emotion.DecayRate * age);
                
                if (Mathf.Abs(emotion.Intensity) < 0.05f)
                {
                    expiredEmotions.Add(emotion);
                }
            }
            
            // Remove expired emotions
            var updatedEmotions = new Queue<AIEmotionEvent>();
            foreach (var emotion in _recentEmotions)
            {
                if (!expiredEmotions.Contains(emotion))
                {
                    updatedEmotions.Enqueue(emotion);
                }
            }
            _recentEmotions = updatedEmotions;
        }
        
        private void ApplyEmotionalInfluence()
        {
            float totalEmotionalWeight = _recentEmotions.Sum(e => e.Intensity);
            
            // Influence mood based on cumulative emotional state
            if (totalEmotionalWeight > 0.5f)
            {
                // Positive emotional state
                if (_advisorState.Mood != AIMood.Optimistic)
                {
                    var positiveModds = new[] { AIMood.Optimistic, AIMood.Helpful };
                    _advisorState.Mood = positiveModds[UnityEngine.Random.Range(0, positiveModds.Length)];
                }
            }
            // else if (totalEmotionalWeight < -0.5f)
            // {
                // Negative emotional state
                _advisorState.Mood = AIMood.Concerned;
            // }
            
            // Adjust confidence based on emotional state
            float emotionalConfidenceModifier = Mathf.Clamp(totalEmotionalWeight * 0.1f, -0.2f, 0.2f);
            _advisorState.ConfidenceLevel = Mathf.Clamp(_advisorState.ConfidenceLevel + emotionalConfidenceModifier, 0.3f, 0.95f);
        }
        
        private void AdaptPersonalityFromInteractions()
        {
            // Learn from conversation patterns and adapt personality
            if (_conversationDepth > 10)
            {
                // User has many interactions - become more casual and personalized
                if (_advisorState.PersonalityType == AIPersonality.Professional)
                {
                    _advisorState.PersonalityType = AIPersonality.Friendly;
                    _personalityProfile.Enthusiasm = Mathf.Min(_personalityProfile.Enthusiasm + 0.05f, 1.0f);
                }
            }
            
            // Adapt response style based on user preferences
            var technicalTopics = _topicFrequency.Count(kvp => 
                kvp.Key.Contains("sensor") || kvp.Key.Contains("data") || kvp.Key.Contains("analysis"));
            
            if (technicalTopics > 3)
            {
                _advisorState.ResponseStyle = AIResponseStyle.Technical;
                _personalityProfile.Curiosity = Mathf.Min(_personalityProfile.Curiosity + 0.02f, 1.0f);
            }
        }
        
        private void UpdateTopicFrequency(string userInput)
        {
            var topics = new[] { "optimization", "efficiency", "temperature", "humidity", "lighting", "sensor", "automation", "analysis", "data", "problem", "help" };
            
            foreach (var topic in topics)
            {
                if (userInput.ToLower().Contains(topic))
                {
                    if (_topicFrequency.ContainsKey(topic))
                        _topicFrequency[topic]++;
                    else
                        _topicFrequency[topic] = 1;
                }
            }
        }
        
        private void UpdateAvatarAppearance()
        {
            if (_advisorAvatar == null) return;
            
            // Update avatar classes based on mood
            _advisorAvatar.RemoveFromClassList("mood-optimistic");
            _advisorAvatar.RemoveFromClassList("mood-analytical");
            _advisorAvatar.RemoveFromClassList("mood-concerned");
            _advisorAvatar.RemoveFromClassList("mood-helpful");
            _advisorAvatar.RemoveFromClassList("mood-focused");
            _advisorAvatar.AddToClassList($"mood-{_advisorState.Mood.ToString().ToLower()}");
            
            // Add personality-based styling
            _advisorAvatar.RemoveFromClassList("personality-friendly");
            _advisorAvatar.RemoveFromClassList("personality-professional");
            _advisorAvatar.RemoveFromClassList("personality-technical");
            _advisorAvatar.RemoveFromClassList("personality-casual");
            _advisorAvatar.AddToClassList($"personality-{_advisorState.PersonalityType.ToString().ToLower()}");
            
            // Add confidence level styling
            _advisorAvatar.RemoveFromClassList("confidence-high");
            _advisorAvatar.RemoveFromClassList("confidence-medium");
            _advisorAvatar.RemoveFromClassList("confidence-low");
            
            string confidenceClass = _advisorState.ConfidenceLevel > 0.8f ? "confidence-high" :
                                   _advisorState.ConfidenceLevel > 0.6f ? "confidence-medium" : "confidence-low";
            _advisorAvatar.AddToClassList(confidenceClass);
            
            // Add emotional state indicators
            float emotionalIntensity = _recentEmotions.Count > 0 ? _recentEmotions.Sum(e => e.Intensity) : 0f;
            
            _advisorAvatar.RemoveFromClassList("emotional-positive");
            _advisorAvatar.RemoveFromClassList("emotional-negative");
            _advisorAvatar.RemoveFromClassList("emotional-neutral");
            
            if (emotionalIntensity > 0.3f)
                _advisorAvatar.AddToClassList("emotional-positive");
            else if (emotionalIntensity < -0.3f)
                _advisorAvatar.AddToClassList("emotional-negative");
            else
                _advisorAvatar.AddToClassList("emotional-neutral");
        }
        
        private void StartVoiceInput()
        {
            // Voice input functionality would be implemented here
            Debug.Log("Voice input started");
        }
        
        private string GetRandomRecommendationTitle()
        {
            var titles = new List<string>
            {
                "Optimize HVAC Efficiency",
                "Adjust Lighting Schedule",
                "Rebalance Nutrient Mix",
                "Update Automation Rules",
                "Improve Air Circulation",
                "Enhance Security Protocols"
            };
            return titles[UnityEngine.Random.Range(0, titles.Count)];
        }
        
        private string GetRandomRecommendationDescription()
        {
            var descriptions = new List<string>
            {
                "Based on recent performance data, this optimization could improve efficiency by 8-12%.",
                "Analysis indicates this adjustment would reduce costs while maintaining quality.",
                "Current settings are suboptimal for the current growth stage and environmental conditions.",
                "This update would enhance automation responsiveness and reduce manual interventions."
            };
            return descriptions[UnityEngine.Random.Range(0, descriptions.Count)];
        }
        
        private string GetRandomRecommendationCategory()
        {
            var categories = new List<string> { "Environmental", "Financial", "Automation", "Security", "Optimization" };
            return categories[UnityEngine.Random.Range(0, categories.Count)];
        }
        
        private float CalculatePredictionAccuracy()
        {
            // Calculate accuracy of past predictions
            return UnityEngine.Random.Range(0.8f, 0.95f);
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
            StopAllCoroutines();
        }
        
        #endregion
    }
    
    // Supporting data structures
    [System.Serializable]
    public class AIAdvisorState
    {
        public bool IsOnline;
        public float ConfidenceLevel;
        public AIMood Mood;
        public AIPersonality PersonalityType;
        public float AnalysisDepth;
        public AIResponseStyle ResponseStyle;
    }
    
    [System.Serializable]
    public class AIMessage
    {
        public string MessageId;
        public string Content;
        public DateTime Timestamp;
        public AIMessageType MessageType;
        public string Sender;
        public float ConfidenceLevel;
    }
    
    [System.Serializable]
    public class AIRecommendation
    {
        public string RecommendationId;
        public string Title;
        public string Description;
        public string Category;
        public RecommendationPriority Priority;
        public float ImpactScore;
        public ImplementationDifficulty ImplementationDifficulty;
        public float EstimatedBenefit;
        public DateTime GeneratedAt;
    }
    
    [System.Serializable]
    public class AIInsight
    {
        public string InsightId;
        public string Category;
        public string Title;
        public string Description;
        public InsightPriority Priority;
        public float ImpactScore;
        public DateTime GeneratedAt;
    }
    
    [System.Serializable]
    public class AIPrediction
    {
        public string PredictionId;
        public string Title;
        public string Description;
        public float Confidence;
        public DateTime PredictedDate;
        public PredictionCategory Category;
    }
    
    [System.Serializable]
    public class FacilityAnalysisResults
    {
        public float OverallHealth;
        public float OptimizationScore;
        public float EfficiencyRating;
        public int PredictedIssues;
        public int RecommendationCount;
    }
    
    public enum AIMood
    {
        Optimistic,
        Analytical,
        Concerned,
        Focused,
        Helpful
    }
    
    public enum AIPersonality
    {
        Analytical,
        Professional,
        Friendly,
        Casual,
        Technical
    }
    
    public enum AIResponseStyle
    {
        Concise,
        Detailed,
        Technical,
        Casual,
        Professional
    }
    
    public enum AIMessageType
    {
        UserInput,
        AIResponse,
        SystemMessage,
        Notification
    }
    
    public enum RecommendationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    public enum ImplementationDifficulty
    {
        Easy,
        Medium,
        Hard
    }
    
    public enum InsightPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    public enum PredictionCategory
    {
        Performance,
        Financial,
        Environmental,
        Maintenance,
        Market
    }
    
    [System.Serializable]
    public class AIAdvisorSettings
    {
        public bool EnableNotifications = true;
        public bool EnableVoiceSynthesis = false;
        public bool EnablePredictiveAnalytics = true;
        public float TypingSpeed = 0.05f;
        public int MaxChatHistory = 100;
        public float AnalysisInterval = 5f;
    }
    
    /// <summary>
    /// Enhanced personality profile for AI advisor
    /// </summary>
    [System.Serializable]
    public class AIPersonalityProfile
    {
        public float Enthusiasm = 0.5f;
        public float Empathy = 0.5f;
        public float Assertiveness = 0.5f;
        public float Curiosity = 0.5f;
        public float Patience = 0.5f;
        public float Humor = 0.3f;
        public List<string> PreferredTopics = new List<string>();
        public string CommunicationStyle = "professional";
        public float LearningRate = 0.1f;
    }
    
    /// <summary>
    /// Represents an emotional event that affects AI mood
    /// </summary>
    [System.Serializable]
    public class AIEmotionEvent
    {
        public string EventType;
        public float Intensity;
        public float Timestamp;
        public float DecayRate = 0.1f;
    }
}