using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.UI.Core;
using ProjectChimera.UI.Components;
using ProjectChimera.Data.UI;
// using ProjectChimera.Systems.Progression;

namespace ProjectChimera.UI.Panels
{
    /// <summary>
    /// UI Panel for displaying player objectives, daily challenges, and progress tracking.
    /// Provides an engaging interface for goal-oriented gameplay.
    /// </summary>
    public class ObjectivesPanel : UIPanel
    {
        [Header("Objectives Configuration")]
        [SerializeField] private float _refreshInterval = 2f;
        [SerializeField] private bool _enableAutoRefresh = true;
        [SerializeField] private bool _showCompletionAnimations = true;
        [SerializeField] private AudioClip _objectiveCompleteSound;
        [SerializeField] private AudioClip _challengeCompleteSound;
        
        // System reference
        // private ObjectiveManager _objectiveManager;
        
        // Main UI containers
        private VisualElement _objectivesContainer;
        private VisualElement _challengesContainer;
        private VisualElement _headerContainer;
        private VisualElement _progressSummary;
        
        // Tab controls
        private Button _objectivesTab;
        private Button _challengesTab;
        private string _currentTab = "objectives";
        
        // Header elements
        private Label _titleLabel;
        private Label _progressSummaryLabel;
        private UIProgressBar _overallProgressBar;
        private Button _refreshButton;
        
        // Objective lists
        private ScrollView _objectivesList;
        private ScrollView _challengesList;
        
        // State tracking
        private float _lastRefreshTime;
        private List<ObjectiveProgressData> _lastObjectiveData = new List<ObjectiveProgressData>();
        private List<ChallengeProgressData> _lastChallengeData = new List<ChallengeProgressData>();
        
        protected override void SetupUIElements()
        {
            base.SetupUIElements();
            
            // Get system reference
            // _objectiveManager = GameManager.Instance?.GetManager<ObjectiveManager>();
            
            // if (_objectiveManager == null)
            // {
                // LogWarning("ObjectiveManager not found - panel will show placeholder content");
            // }
            
            CreateMainLayout();
            CreateHeader();
            CreateTabNavigation();
            CreateObjectivesSection();
            CreateChallengesSection();
            
            // Show objectives tab by default
            ShowTab("objectives");
            
            if (_enableAutoRefresh)
            {
                InvokeRepeating(nameof(RefreshObjectiveData), 0f, _refreshInterval);
            }
        }
        
        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            
            // Tab navigation
            _objectivesTab?.RegisterCallback<ClickEvent>(evt => ShowTab("objectives"));
            _challengesTab?.RegisterCallback<ClickEvent>(evt => ShowTab("challenges"));
            
            // Header controls
            _refreshButton?.RegisterCallback<ClickEvent>(evt => RefreshObjectiveData());
            
            // Subscribe to objective manager events
            // if (_objectiveManager != null)
            // {
                // _objectiveManager.OnObjectiveCompleted += OnObjectiveCompleted;
                // _objectiveManager.OnChallengeCompleted += OnChallengeCompleted;
                // _objectiveManager.OnNewObjectiveGenerated += OnNewObjectiveGenerated;
            // }
        }
        
        private void CreateMainLayout()
        {
            _rootElement.Clear();
            _rootElement.style.paddingTop = 20;
            _rootElement.style.paddingBottom = 20;
            _rootElement.style.paddingLeft = 20;
            _rootElement.style.paddingRight = 20;
            
            var mainContainer = new VisualElement();
            mainContainer.name = "objectives-main-container";
            mainContainer.style.flexGrow = 1;
            mainContainer.style.flexDirection = FlexDirection.Column;
            
            _rootElement.Add(mainContainer);
        }
        
        private void CreateHeader()
        {
            _headerContainer = new VisualElement();
            _headerContainer.name = "objectives-header";
            _headerContainer.style.flexDirection = FlexDirection.Row;
            _headerContainer.style.justifyContent = Justify.SpaceBetween;
            _headerContainer.style.alignItems = Align.Center;
            _headerContainer.style.marginBottom = 20;
            
            // Left section - Title and progress
            var leftSection = new VisualElement();
            leftSection.style.flexDirection = FlexDirection.Column;
            
            _titleLabel = new Label("Your Objectives");
            _titleLabel.name = "objectives-title";
            _titleLabel.style.fontSize = 24;
            // _titleLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.marginBottom = 8;
            
            _progressSummaryLabel = new Label("Track your cultivation goals and daily challenges");
            _progressSummaryLabel.name = "progress-summary";
            _progressSummaryLabel.style.fontSize = 14;
            // _progressSummaryLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            leftSection.Add(_titleLabel);
            leftSection.Add(_progressSummaryLabel);
            
            // Right section - Controls
            var rightSection = new VisualElement();
            rightSection.style.flexDirection = FlexDirection.Row;
            rightSection.style.alignItems = Align.Center;
            
            // Overall progress bar
            _overallProgressBar = new UIProgressBar(100f);
            _overallProgressBar.Format = "Progress: {0:F0}%";
            _overallProgressBar.style.width = 200;
            _overallProgressBar.style.marginRight = 16;
            // _overallProgressBar.SetColor(_uiManager.DesignSystem.ColorPalette.PrimaryGreen);
            
            // Refresh button
            _refreshButton = new Button();
            _refreshButton.name = "refresh-button";
            _refreshButton.text = "🔄";
            _refreshButton.style.width = 40;
            _refreshButton.style.height = 40;
            // _uiManager.ApplyDesignSystemStyle(_refreshButton, UIStyleToken.SecondaryButton);
            
            rightSection.Add(_overallProgressBar);
            rightSection.Add(_refreshButton);
            
            _headerContainer.Add(leftSection);
            _headerContainer.Add(rightSection);
            
            _rootElement.Add(_headerContainer);
        }
        
        private void CreateTabNavigation()
        {
            var tabContainer = new VisualElement();
            tabContainer.name = "tab-container";
            tabContainer.style.flexDirection = FlexDirection.Row;
            tabContainer.style.marginBottom = 20;
            
            // Objectives tab
            _objectivesTab = new Button();
            _objectivesTab.name = "objectives-tab";
            _objectivesTab.text = "🎯 Current Objectives";
            _objectivesTab.style.flexGrow = 1;
            _objectivesTab.style.height = 50;
            _objectivesTab.style.marginRight = 8;
            // _uiManager.ApplyDesignSystemStyle(_objectivesTab, UIStyleToken.TabButton);
            
            // Challenges tab
            _challengesTab = new Button();
            _challengesTab.name = "challenges-tab";
            _challengesTab.text = "⚡ Daily Challenges";
            _challengesTab.style.flexGrow = 1;
            _challengesTab.style.height = 50;
            // _uiManager.ApplyDesignSystemStyle(_challengesTab, UIStyleToken.TabButton);
            
            tabContainer.Add(_objectivesTab);
            tabContainer.Add(_challengesTab);
            
            _rootElement.Add(tabContainer);
        }
        
        private void CreateObjectivesSection()
        {
            _objectivesContainer = new VisualElement();
            _objectivesContainer.name = "objectives-section";
            _objectivesContainer.style.flexGrow = 1;
            
            // Section header
            var sectionHeader = new VisualElement();
            sectionHeader.style.flexDirection = FlexDirection.Row;
            sectionHeader.style.justifyContent = Justify.SpaceBetween;
            sectionHeader.style.alignItems = Align.Center;
            sectionHeader.style.marginBottom = 16;
            
            var headerLabel = new Label("Active Objectives");
            headerLabel.style.fontSize = 18;
            // headerLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            var objectiveCount = new Label("0 objectives");
            objectiveCount.name = "objective-count";
            objectiveCount.style.fontSize = 14;
            // objectiveCount.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            sectionHeader.Add(headerLabel);
            sectionHeader.Add(objectiveCount);
            
            // Objectives list
            _objectivesList = new ScrollView();
            _objectivesList.name = "objectives-list";
            _objectivesList.style.flexGrow = 1;
            _objectivesList.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.3f);
            _objectivesList.style.borderTopLeftRadius = 8;
            _objectivesList.style.borderTopRightRadius = 8;
            _objectivesList.style.borderBottomLeftRadius = 8;
            _objectivesList.style.borderBottomRightRadius = 8;
            _objectivesList.style.paddingTop = 16;
            _objectivesList.style.paddingBottom = 16;
            _objectivesList.style.paddingLeft = 16;
            _objectivesList.style.paddingRight = 16;
            
            _objectivesContainer.Add(sectionHeader);
            _objectivesContainer.Add(_objectivesList);
            
            _rootElement.Add(_objectivesContainer);
        }
        
        private void CreateChallengesSection()
        {
            _challengesContainer = new VisualElement();
            _challengesContainer.name = "challenges-section";
            _challengesContainer.style.flexGrow = 1;
            
            // Section header
            var sectionHeader = new VisualElement();
            sectionHeader.style.flexDirection = FlexDirection.Row;
            sectionHeader.style.justifyContent = Justify.SpaceBetween;
            sectionHeader.style.alignItems = Align.Center;
            sectionHeader.style.marginBottom = 16;
            
            var headerLabel = new Label("Daily Challenges");
            headerLabel.style.fontSize = 18;
            // headerLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            var challengeInfo = new Label("Refreshes daily at midnight");
            challengeInfo.name = "challenge-info";
            challengeInfo.style.fontSize = 12;
            // challengeInfo.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            sectionHeader.Add(headerLabel);
            sectionHeader.Add(challengeInfo);
            
            // Challenges list
            _challengesList = new ScrollView();
            _challengesList.name = "challenges-list";
            _challengesList.style.flexGrow = 1;
            _challengesList.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.3f);
            _challengesList.style.borderTopLeftRadius = 8;
            _challengesList.style.borderTopRightRadius = 8;
            _challengesList.style.borderBottomLeftRadius = 8;
            _challengesList.style.borderBottomRightRadius = 8;
            _challengesList.style.paddingTop = 16;
            _challengesList.style.paddingBottom = 16;
            _challengesList.style.paddingLeft = 16;
            _challengesList.style.paddingRight = 16;
            
            _challengesContainer.Add(sectionHeader);
            _challengesContainer.Add(_challengesList);
            
            _rootElement.Add(_challengesContainer);
        }
        
        private void ShowTab(string tabName)
        {
            _currentTab = tabName;
            
            // Update tab button states
            _objectivesTab?.RemoveFromClassList("tab-active");
            _challengesTab?.RemoveFromClassList("tab-active");
            
            // Hide all containers
            _objectivesContainer?.AddToClassList("hidden");
            _challengesContainer?.AddToClassList("hidden");
            
            // Show selected tab
            switch (tabName)
            {
                case "objectives":
                    _objectivesTab?.AddToClassList("tab-active");
                    _objectivesContainer?.RemoveFromClassList("hidden");
                    _titleLabel.text = "Your Objectives";
                    RefreshObjectivesDisplay();
                    break;
                case "challenges":
                    _challengesTab?.AddToClassList("tab-active");
                    _challengesContainer?.RemoveFromClassList("hidden");
                    _titleLabel.text = "Daily Challenges";
                    RefreshChallengesDisplay();
                    break;
            }
        }
        
        [ContextMenu("Refresh Objective Data")]
        private void RefreshObjectiveData()
        {
            // if (_objectiveManager == null) return;
            
            // _lastObjectiveData = _objectiveManager.GetObjectiveProgressData();
            // _lastChallengeData = _objectiveManager.GetChallengeProgressData();
            
            // Update displays based on current tab
            if (_currentTab == "objectives")
            {
                RefreshObjectivesDisplay();
            }
            // else if (_currentTab == "challenges")
            // {
                RefreshChallengesDisplay();
            // }
            
            UpdateProgressSummary();
            _lastRefreshTime = Time.time;
        }
        
        private void RefreshObjectivesDisplay()
        {
            _objectivesList.Clear();
            
            if (_lastObjectiveData.Count == 0)
            {
                ShowEmptyObjectivesState();
                return;
            }
            
            foreach (var objective in _lastObjectiveData)
            {
                var objectiveElement = CreateObjectiveElement(objective);
                _objectivesList.Add(objectiveElement);
            }
            
            // Update objective count
            var countLabel = _objectivesContainer.Q<Label>("objective-count");
            if (countLabel != null)
            {
                countLabel.text = $"{_lastObjectiveData.Count} active objectives";
            }
        }
        
        private void RefreshChallengesDisplay()
        {
            _challengesList.Clear();
            
            if (_lastChallengeData.Count == 0)
            {
                ShowEmptyChallengesState();
                return;
            }
            
            foreach (var challenge in _lastChallengeData)
            {
                var challengeElement = CreateChallengeElement(challenge);
                _challengesList.Add(challengeElement);
            }
        }
        
        private VisualElement CreateObjectiveElement(ObjectiveProgressData objective)
        {
            var container = new VisualElement();
            container.name = "objective-item";
            container.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            container.style.borderTopLeftRadius = 12;
            container.style.borderTopRightRadius = 12;
            container.style.borderBottomLeftRadius = 12;
            container.style.borderBottomRightRadius = 12;
            container.style.paddingTop = 16;
            container.style.paddingBottom = 16;
            container.style.paddingLeft = 16;
            container.style.paddingRight = 16;
            container.style.marginBottom = 12;
            
            // Header row
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.justifyContent = Justify.SpaceBetween;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 8;
            
            // Title with icon
            var titleContainer = new VisualElement();
            titleContainer.style.flexDirection = FlexDirection.Row;
            titleContainer.style.alignItems = Align.Center;
            
            var iconLabel = new Label("🎯");
            iconLabel.style.fontSize = 20;
            iconLabel.style.marginRight = 8;
            
            var titleLabel = new Label(objective.Title);
            titleLabel.style.fontSize = 16;
            // titleLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            titleContainer.Add(iconLabel);
            titleContainer.Add(titleLabel);
            
            // Difficulty badge
            var difficultyBadge = CreateDifficultyBadge(objective.Difficulty);
            
            headerRow.Add(titleContainer);
            headerRow.Add(difficultyBadge);
            
            // Description
            var descriptionLabel = new Label(objective.Description);
            descriptionLabel.style.fontSize = 14;
            // descriptionLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            descriptionLabel.style.marginBottom = 12;
            descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            
            // Progress section
            var progressContainer = new VisualElement();
            progressContainer.style.flexDirection = FlexDirection.Column;
            progressContainer.style.marginBottom = 12;
            
            var progressHeader = new VisualElement();
            progressHeader.style.flexDirection = FlexDirection.Row;
            progressHeader.style.justifyContent = Justify.SpaceBetween;
            progressHeader.style.marginBottom = 4;
            
            var progressText = new Label($"Progress: {objective.CurrentProgress:F0} / {objective.TargetProgress:F0}");
            progressText.style.fontSize = 12;
            // progressText.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            var percentageText = new Label($"{objective.ProgressPercentage * 100:F0}%");
            percentageText.style.fontSize = 12;
            // percentageText.style.color = _uiManager.DesignSystem.ColorPalette.Info;
            percentageText.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            progressHeader.Add(progressText);
            progressHeader.Add(percentageText);
            
            // Progress bar
            var progressBar = new UIProgressBar(objective.TargetProgress);
            progressBar.Value = objective.CurrentProgress;
            progressBar.SetColor(GetDifficultyColor(objective.Difficulty));
            progressBar.style.height = 8;
            
            progressContainer.Add(progressHeader);
            progressContainer.Add(progressBar);
            
            // Footer row
            var footerRow = new VisualElement();
            footerRow.style.flexDirection = FlexDirection.Row;
            footerRow.style.justifyContent = Justify.SpaceBetween;
            footerRow.style.alignItems = Align.Center;
            
            // Time remaining
            var timeText = objective.TimeRemaining.TotalDays > 0 ? 
                $"⏰ {objective.TimeRemaining.Days}d {objective.TimeRemaining.Hours}h remaining" :
                "⏰ Expires soon!";
                
            var timeLabel = new Label(timeText);
            timeLabel.style.fontSize = 12;
            // timeLabel.style.color = objective.TimeRemaining.TotalHours < 24 ? 
                // _uiManager.DesignSystem.ColorPalette.Warning : 
                // _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            // Rewards preview
            var rewardLabel = new Label(objective.RewardPreview);
            rewardLabel.style.fontSize = 12;
            // rewardLabel.style.color = _uiManager.DesignSystem.ColorPalette.AccentGold;
            
            footerRow.Add(timeLabel);
            footerRow.Add(rewardLabel);
            
            container.Add(headerRow);
            container.Add(descriptionLabel);
            container.Add(progressContainer);
            container.Add(footerRow);
            
            return container;
        }
        
        private VisualElement CreateChallengeElement(ChallengeProgressData challenge)
        {
            var container = new VisualElement();
            container.name = "challenge-item";
            container.style.backgroundColor = challenge.IsCompleted ? 
                new Color(0.1f, 0.3f, 0.1f, 0.8f) : 
                new Color(0.1f, 0.1f, 0.1f, 0.8f);
            container.style.borderTopLeftRadius = 12;
            container.style.borderTopRightRadius = 12;
            container.style.borderBottomLeftRadius = 12;
            container.style.borderBottomRightRadius = 12;
            container.style.paddingTop = 16;
            container.style.paddingBottom = 16;
            container.style.paddingLeft = 16;
            container.style.paddingRight = 16;
            container.style.marginBottom = 12;
            
            // Add completion overlay if completed
            if (challenge.IsCompleted)
            {
                var completionBadge = new Label("✅ COMPLETED");
                completionBadge.style.position = Position.Absolute;
                completionBadge.style.top = 8;
                completionBadge.style.right = 8;
                completionBadge.style.fontSize = 10;
                // completionBadge.style.color = _uiManager.DesignSystem.ColorPalette.Success;
                completionBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
                container.Add(completionBadge);
            }
            
            // Header
            var titleContainer = new VisualElement();
            titleContainer.style.flexDirection = FlexDirection.Row;
            titleContainer.style.alignItems = Align.Center;
            titleContainer.style.marginBottom = 8;
            
            var iconLabel = new Label("⚡");
            iconLabel.style.fontSize = 18;
            iconLabel.style.marginRight = 8;
            
            var titleLabel = new Label(challenge.Title);
            titleLabel.style.fontSize = 16;
            // titleLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            titleContainer.Add(iconLabel);
            titleContainer.Add(titleLabel);
            
            // Description
            var descriptionLabel = new Label(challenge.Description);
            descriptionLabel.style.fontSize = 14;
            // descriptionLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            descriptionLabel.style.marginBottom = 12;
            
            // Progress
            var progressBar = new UIProgressBar(challenge.TargetProgress);
            progressBar.Value = challenge.CurrentProgress;
            progressBar.Format = $"{challenge.CurrentProgress:F0} / {challenge.TargetProgress:F0} ({challenge.ProgressPercentage * 100:F0}%)";
            // progressBar.SetColor(challenge.IsCompleted ? 
                // _uiManager.DesignSystem.ColorPalette.Success : 
                // _uiManager.DesignSystem.ColorPalette.Warning);
            progressBar.style.marginBottom = 8;
            
            // Rewards
            var rewardLabel = new Label($"Reward: {challenge.RewardPreview}");
            rewardLabel.style.fontSize = 12;
            // rewardLabel.style.color = _uiManager.DesignSystem.ColorPalette.AccentGold;
            
            container.Add(titleContainer);
            container.Add(descriptionLabel);
            container.Add(progressBar);
            container.Add(rewardLabel);
            
            return container;
        }
        
        private VisualElement CreateDifficultyBadge(ObjectiveDifficulty difficulty)
        {
            var badge = new VisualElement();
            badge.style.backgroundColor = GetDifficultyColor(difficulty);
            badge.style.borderTopLeftRadius = 12;
            badge.style.borderTopRightRadius = 12;
            badge.style.borderBottomLeftRadius = 12;
            badge.style.borderBottomRightRadius = 12;
            badge.style.paddingTop = 4;
            badge.style.paddingBottom = 4;
            badge.style.paddingLeft = 8;
            badge.style.paddingRight = 8;
            
            var badgeLabel = new Label(difficulty.ToString().ToUpper());
            badgeLabel.style.fontSize = 10;
            badgeLabel.style.color = Color.white;
            badgeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            badge.Add(badgeLabel);
            return badge;
        }
        
        private Color GetDifficultyColor(ObjectiveDifficulty difficulty)
        {
            return difficulty switch
            {
                // ObjectiveDifficulty.Easy => _uiManager.DesignSystem.ColorPalette.Success,
                // ObjectiveDifficulty.Medium => _uiManager.DesignSystem.ColorPalette.Warning,
                // ObjectiveDifficulty.Hard => _uiManager.DesignSystem.ColorPalette.Error,
                ObjectiveDifficulty.Expert => new Color(0.6f, 0.1f, 0.8f, 1f), // Purple
                // ObjectiveDifficulty.Legendary => _uiManager.DesignSystem.ColorPalette.AccentGold,
                // _ => _uiManager.DesignSystem.ColorPalette.TextSecondary
            };
        }
        
        private void ShowEmptyObjectivesState()
        {
            var emptyState = new VisualElement();
            emptyState.style.alignItems = Align.Center;
            emptyState.style.justifyContent = Justify.Center;
            emptyState.style.flexGrow = 1;
            emptyState.style.paddingTop = 60;
            emptyState.style.paddingBottom = 60;
            
            var emptyIcon = new Label("🎯");
            emptyIcon.style.fontSize = 48;
            emptyIcon.style.marginBottom = 16;
            
            var emptyText = new Label("No active objectives");
            emptyText.style.fontSize = 18;
            // emptyText.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            emptyText.style.marginBottom = 8;
            
            var emptyDescription = new Label("New objectives will appear as you progress in the game");
            emptyDescription.style.fontSize = 14;
            // emptyDescription.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            emptyDescription.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            emptyState.Add(emptyIcon);
            emptyState.Add(emptyText);
            emptyState.Add(emptyDescription);
            
            _objectivesList.Add(emptyState);
        }
        
        private void ShowEmptyChallengesState()
        {
            var emptyState = new VisualElement();
            emptyState.style.alignItems = Align.Center;
            emptyState.style.justifyContent = Justify.Center;
            emptyState.style.flexGrow = 1;
            emptyState.style.paddingTop = 60;
            emptyState.style.paddingBottom = 60;
            
            var emptyIcon = new Label("⚡");
            emptyIcon.style.fontSize = 48;
            emptyIcon.style.marginBottom = 16;
            
            var emptyText = new Label("No daily challenges");
            emptyText.style.fontSize = 18;
            // emptyText.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            emptyText.style.marginBottom = 8;
            
            var emptyDescription = new Label("Daily challenges refresh automatically at midnight");
            emptyDescription.style.fontSize = 14;
            // emptyDescription.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            emptyDescription.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            emptyState.Add(emptyIcon);
            emptyState.Add(emptyText);
            emptyState.Add(emptyDescription);
            
            _challengesList.Add(emptyState);
        }
        
        private void UpdateProgressSummary()
        {
            // if (_objectiveManager == null) return;
            
            // var activeCount = _objectiveManager.ActiveObjectiveCount;
            // var completedCount = _objectiveManager.TotalObjectivesCompleted;
            
            // Placeholder data for compilation
            var activeCount = _lastObjectiveData.Count;
            var completedCount = 0; // Would come from objective manager
            
            _progressSummaryLabel.text = $"{activeCount} active • {completedCount} completed overall";
            
            // Update overall progress based on active objective progress
            if (_lastObjectiveData.Count > 0)
            {
                var averageProgress = _lastObjectiveData.Average(o => o.ProgressPercentage) * 100f;
                _overallProgressBar.Value = averageProgress;
            }
        }
        
        // Event handlers
        private void OnObjectiveCompleted(ActiveObjective objective)
        {
            if (_showCompletionAnimations)
            {
                ShowCompletionCelebration($"🎯 {objective.Title} Complete!");
            }
            
            if (_objectiveCompleteSound != null)
            {
                // Play completion sound
                AudioSource.PlayClipAtPoint(_objectiveCompleteSound, Camera.main.transform.position);
            }
            
            // Refresh display
            RefreshObjectiveData();
        }
        
        private void OnChallengeCompleted(ActiveChallenge challenge)
        {
            if (_showCompletionAnimations)
            {
                ShowCompletionCelebration($"⚡ {challenge.Title} Complete!");
            }
            
            if (_challengeCompleteSound != null)
            {
                // Play completion sound
                AudioSource.PlayClipAtPoint(_challengeCompleteSound, Camera.main.transform.position);
            }
            
            // Refresh display
            RefreshObjectiveData();
        }
        
        private void OnNewObjectiveGenerated()
        {
            // Refresh to show new objective
            RefreshObjectiveData();
            
            // Show notification
            ShowNotification("🎯 New objective available!", UIStatus.Info);
        }
        
        private void ShowCompletionCelebration(string message)
        {
            // Create a temporary celebration overlay
            var celebration = new VisualElement();
            celebration.style.position = Position.Absolute;
            celebration.style.top = 0;
            celebration.style.left = 0;
            celebration.style.right = 0;
            celebration.style.bottom = 0;
            celebration.style.backgroundColor = new Color(0f, 0f, 0f, 0.7f);
            celebration.style.alignItems = Align.Center;
            celebration.style.justifyContent = Justify.Center;
            
            var celebrationText = new Label(message);
            celebrationText.style.fontSize = 24;
            // celebrationText.style.color = _uiManager.DesignSystem.ColorPalette.AccentGold;
            celebrationText.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            celebration.Add(celebrationText);
            _rootElement.Add(celebration);
            
            // Remove after 2 seconds
            StartCoroutine(RemoveCelebrationAfterDelay(celebration, 2f));
        }
        
        private System.Collections.IEnumerator RemoveCelebrationAfterDelay(VisualElement element, float delay)
        {
            yield return new WaitForSeconds(delay);
            _rootElement.Remove(element);
        }
        
        private void ShowNotification(string message, UIStatus status)
        {
            // This would integrate with the game's notification system
            Debug.Log($"Notification: {message}");
        }
        
        protected virtual void OnDestroy()
        {
            // Clean up objectives panel
            
            // Unsubscribe from events
            // if (_objectiveManager != null)
            // {
                // _objectiveManager.OnObjectiveCompleted -= OnObjectiveCompleted;
                // _objectiveManager.OnChallengeCompleted -= OnChallengeCompleted;
                // _objectiveManager.OnNewObjectiveGenerated -= OnNewObjectiveGenerated;
            // }
            
            // Stop auto-refresh
            CancelInvoke(nameof(RefreshObjectiveData));
        }
    }
}