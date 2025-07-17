using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.UI.Core;
using ProjectChimera.UI.Components;
using ProjectChimera.Data.UI;
using ProjectChimera.Data;
// using ProjectChimera.Systems.Progression;

namespace ProjectChimera.UI.Panels
{
    /// <summary>
    /// UI Panel for displaying competitive gameplay elements including leaderboards,
    /// tournaments, player rankings, and competitive achievements. Enhances social
    /// engagement and provides motivation through competitive comparison.
    /// </summary>
    public class CompetitivePanel : UIPanel
    {
        [Header("Competitive Configuration")]
        [SerializeField] private float _refreshInterval = 10f;
        [SerializeField] private bool _enableAutoRefresh = true;
        [SerializeField] private bool _showAnimations = true;
        [SerializeField] private AudioClip _rankUpSound;
        [SerializeField] private AudioClip _achievementSound;
        
        // System reference
        // private CompetitiveManager _competitiveManager;
        
        // Main UI containers
        private VisualElement _leaderboardContainer;
        private VisualElement _tournamentsContainer;
        private VisualElement _personalStatsContainer;
        private VisualElement _achievementsContainer;
        
        // Tab controls
        private Button _leaderboardTab;
        private Button _tournamentsTab;
        private Button _statsTab;
        private Button _achievementsTab;
        private string _currentTab = "leaderboard";
        
        // Header elements
        private Label _titleLabel;
        private Label _rankingSummaryLabel;
        private VisualElement _playerRankCard;
        private Button _refreshButton;
        
        // Leaderboard elements
        private VisualElement _leaderboardTypeSelector;
        private ScrollView _leaderboardList;
        private ProjectChimera.Data.LeaderboardType _currentLeaderboardType = ProjectChimera.Data.LeaderboardType.Overall;
        
        // Tournament elements
        private ScrollView _tournamentsList;
        private VisualElement _tournamentDetails;
        
        // Personal stats elements
        private VisualElement _statsGrid;
        private VisualElement _recordsGrid;
        private UIProgressBar _competitiveLevelBar;
        
        // State tracking
        private float _lastRefreshTime;
        private CompetitiveStatsSummary _lastStatsData;
        private List<LeaderboardDisplayData> _lastLeaderboardData = new List<LeaderboardDisplayData>();
        private int _lastKnownRank = -1;
        
        protected override void SetupUIElements()
        {
            base.SetupUIElements();
            
            // Get system reference
            // _competitiveManager = GameManager.Instance?.GetManager<CompetitiveManager>();
            
            // if (_competitiveManager == null)
            // {
                // LogWarning("CompetitiveManager not found - panel will show placeholder content");
            // }
            
            CreateMainLayout();
            CreateHeader();
            CreateTabNavigation();
            CreateLeaderboardSection();
            CreateTournamentsSection();
            CreatePersonalStatsSection();
            CreateAchievementsSection();
            
            // Show leaderboard tab by default
            ShowTab("leaderboard");
            
            if (_enableAutoRefresh)
            {
                InvokeRepeating(nameof(RefreshCompetitiveData), 0f, _refreshInterval);
            }
        }
        
        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            
            // Tab navigation
            _leaderboardTab?.RegisterCallback<ClickEvent>(evt => ShowTab("leaderboard"));
            _tournamentsTab?.RegisterCallback<ClickEvent>(evt => ShowTab("tournaments"));
            _statsTab?.RegisterCallback<ClickEvent>(evt => ShowTab("stats"));
            _achievementsTab?.RegisterCallback<ClickEvent>(evt => ShowTab("achievements"));
            
            // Header controls
            _refreshButton?.RegisterCallback<ClickEvent>(evt => RefreshCompetitiveData());
            
            // Subscribe to competitive manager events
            // if (_competitiveManager != null)
            // {
                // _competitiveManager.OnRankingChanged += OnRankingChanged;
                // _competitiveManager.OnCompetitionStarted += OnCompetitionStarted;
                // _competitiveManager.OnCompetitionEnded += OnCompetitionEnded;
                // _competitiveManager.OnPersonalRecordSet += OnPersonalRecordSet;
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
            mainContainer.name = "competitive-main-container";
            mainContainer.style.flexGrow = 1;
            mainContainer.style.flexDirection = FlexDirection.Column;
            
            _rootElement.Add(mainContainer);
        }
        
        private void CreateHeader()
        {
            var headerContainer = new VisualElement();
            headerContainer.name = "competitive-header";
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.marginBottom = 20;
            
            // Left section - Title and current rank
            var leftSection = new VisualElement();
            leftSection.style.flexDirection = FlexDirection.Column;
            
            _titleLabel = new Label("Competitive Arena");
            _titleLabel.name = "competitive-title";
            _titleLabel.style.fontSize = 24;
            // _titleLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.marginBottom = 8;
            
            _rankingSummaryLabel = new Label("Compete with growers worldwide");
            _rankingSummaryLabel.name = "ranking-summary";
            _rankingSummaryLabel.style.fontSize = 14;
            // _rankingSummaryLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            leftSection.Add(_titleLabel);
            leftSection.Add(_rankingSummaryLabel);
            
            // Center section - Player rank card
            _playerRankCard = CreatePlayerRankCard();
            
            // Right section - Controls
            var rightSection = new VisualElement();
            rightSection.style.flexDirection = FlexDirection.Row;
            rightSection.style.alignItems = Align.Center;
            
            _refreshButton = new Button();
            _refreshButton.name = "refresh-button";
            _refreshButton.text = "🔄 Refresh";
            _refreshButton.style.height = 40;
            // _uiManager.ApplyDesignSystemStyle(_refreshButton, UIStyleToken.SecondaryButton);
            
            rightSection.Add(_refreshButton);
            
            headerContainer.Add(leftSection);
            headerContainer.Add(_playerRankCard);
            headerContainer.Add(rightSection);
            
            _rootElement.Add(headerContainer);
        }
        
        private VisualElement CreatePlayerRankCard()
        {
            var card = new VisualElement();
            card.name = "player-rank-card";
            card.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            card.style.borderTopLeftRadius = 12;
            card.style.borderTopRightRadius = 12;
            card.style.borderBottomLeftRadius = 12;
            card.style.borderBottomRightRadius = 12;
            card.style.paddingTop = 16;
            card.style.paddingBottom = 16;
            card.style.paddingLeft = 20;
            card.style.paddingRight = 20;
            card.style.width = 300;
            
            var rankTitle = new Label("Your Current Rank");
            rankTitle.style.fontSize = 12;
            // rankTitle.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            rankTitle.style.marginBottom = 4;
            
            var rankValue = new Label("#--");
            rankValue.name = "current-rank";
            rankValue.style.fontSize = 24;
            // rankValue.style.color = _uiManager.DesignSystem.ColorPalette.AccentGold;
            rankValue.style.unityFontStyleAndWeight = FontStyle.Bold;
            rankValue.style.marginBottom = 4;
            
            var rankCategory = new Label("Overall Leaderboard");
            rankCategory.name = "rank-category";
            rankCategory.style.fontSize = 10;
            // rankCategory.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            card.Add(rankTitle);
            card.Add(rankValue);
            card.Add(rankCategory);
            
            return card;
        }
        
        private void CreateTabNavigation()
        {
            var tabContainer = new VisualElement();
            tabContainer.name = "tab-container";
            tabContainer.style.flexDirection = FlexDirection.Row;
            tabContainer.style.marginBottom = 20;
            
            // Leaderboard tab
            _leaderboardTab = new Button();
            _leaderboardTab.name = "leaderboard-tab";
            _leaderboardTab.text = "🏆 Leaderboards";
            _leaderboardTab.style.flexGrow = 1;
            _leaderboardTab.style.height = 50;
            _leaderboardTab.style.marginRight = 4;
            // _uiManager.ApplyDesignSystemStyle(_leaderboardTab, UIStyleToken.TabButton);
            
            // Tournaments tab
            _tournamentsTab = new Button();
            _tournamentsTab.name = "tournaments-tab";
            _tournamentsTab.text = "⚔️ Tournaments";
            _tournamentsTab.style.flexGrow = 1;
            _tournamentsTab.style.height = 50;
            _tournamentsTab.style.marginRight = 4;
            // _uiManager.ApplyDesignSystemStyle(_tournamentsTab, UIStyleToken.TabButton);
            
            // Personal stats tab
            _statsTab = new Button();
            _statsTab.name = "stats-tab";
            _statsTab.text = "📊 My Stats";
            _statsTab.style.flexGrow = 1;
            _statsTab.style.height = 50;
            _tournamentsTab.style.marginRight = 4;
            // _uiManager.ApplyDesignSystemStyle(_statsTab, UIStyleToken.TabButton);
            
            // Achievements tab
            _achievementsTab = new Button();
            _achievementsTab.name = "achievements-tab";
            _achievementsTab.text = "🎖️ Records";
            _achievementsTab.style.flexGrow = 1;
            _achievementsTab.style.height = 50;
            // _uiManager.ApplyDesignSystemStyle(_achievementsTab, UIStyleToken.TabButton);
            
            tabContainer.Add(_leaderboardTab);
            tabContainer.Add(_tournamentsTab);
            tabContainer.Add(_statsTab);
            tabContainer.Add(_achievementsTab);
            
            _rootElement.Add(tabContainer);
        }
        
        private void CreateLeaderboardSection()
        {
            _leaderboardContainer = new VisualElement();
            _leaderboardContainer.name = "leaderboard-section";
            _leaderboardContainer.style.flexGrow = 1;
            
            // Leaderboard type selector
            CreateLeaderboardTypeSelector();
            
            // Leaderboard list
            _leaderboardList = new ScrollView();
            _leaderboardList.name = "leaderboard-list";
            _leaderboardList.style.flexGrow = 1;
            _leaderboardList.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.3f);
            _leaderboardList.style.borderTopLeftRadius = 8;
            _leaderboardList.style.borderTopRightRadius = 8;
            _leaderboardList.style.borderBottomLeftRadius = 8;
            _leaderboardList.style.borderBottomRightRadius = 8;
            _leaderboardList.style.paddingTop = 16;
            _leaderboardList.style.paddingBottom = 16;
            _leaderboardList.style.paddingLeft = 16;
            _leaderboardList.style.paddingRight = 16;
            
            _leaderboardContainer.Add(_leaderboardTypeSelector);
            _leaderboardContainer.Add(_leaderboardList);
            
            _rootElement.Add(_leaderboardContainer);
        }
        
        private void CreateLeaderboardTypeSelector()
        {
            _leaderboardTypeSelector = new VisualElement();
            _leaderboardTypeSelector.name = "leaderboard-selector";
            _leaderboardTypeSelector.style.flexDirection = FlexDirection.Row;
            _leaderboardTypeSelector.style.marginBottom = 16;
            
            var leaderboardTypes = new (ProjectChimera.Data.LeaderboardType, string, string)[]
            {
                (ProjectChimera.Data.LeaderboardType.Overall, "🌟 Overall", "Combined ranking across all categories"),
                (ProjectChimera.Data.LeaderboardType.Cultivation, "🌱 Cultivation", "Total plants successfully grown"),
                (ProjectChimera.Data.LeaderboardType.Economic, "💰 Economic", "Total profit generated"),
                (ProjectChimera.Data.LeaderboardType.Quality, "⭐ Quality", "Highest quality achievements"),
                (ProjectChimera.Data.LeaderboardType.Speed, "⚡ Speed", "Fastest growth cycles")
            };
            
            foreach (var (type, label, tooltip) in leaderboardTypes)
            {
                var button = new Button();
                button.name = $"leaderboard-{type.ToString().ToLower()}";
                button.text = label;
                button.style.flexGrow = 1;
                button.style.height = 40;
                button.style.marginRight = 4;
                button.tooltip = tooltip;
                // _uiManager.ApplyDesignSystemStyle(button, UIStyleToken.SecondaryButton);
                
                // Capture the type for the lambda
                var capturedType = type;
                button.RegisterCallback<ClickEvent>(evt => SelectLeaderboard(capturedType));
                
                _leaderboardTypeSelector.Add(button);
            }
        }
        
        private void CreateTournamentsSection()
        {
            _tournamentsContainer = new VisualElement();
            _tournamentsContainer.name = "tournaments-section";
            _tournamentsContainer.style.flexGrow = 1;
            
            // Section header
            var sectionHeader = new VisualElement();
            sectionHeader.style.flexDirection = FlexDirection.Row;
            sectionHeader.style.justifyContent = Justify.SpaceBetween;
            sectionHeader.style.alignItems = Align.Center;
            sectionHeader.style.marginBottom = 16;
            
            var headerLabel = new Label("Active Tournaments");
            headerLabel.style.fontSize = 18;
            // headerLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            var tournamentStatus = new Label("Next tournament starts Monday");
            tournamentStatus.name = "tournament-status";
            tournamentStatus.style.fontSize = 12;
            // tournamentStatus.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            sectionHeader.Add(headerLabel);
            sectionHeader.Add(tournamentStatus);
            
            // Tournaments list
            _tournamentsList = new ScrollView();
            _tournamentsList.name = "tournaments-list";
            _tournamentsList.style.flexGrow = 1;
            _tournamentsList.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.3f);
            _tournamentsList.style.borderTopLeftRadius = 8;
            _tournamentsList.style.borderTopRightRadius = 8;
            _tournamentsList.style.borderBottomLeftRadius = 8;
            _tournamentsList.style.borderBottomRightRadius = 8;
            _tournamentsList.style.paddingTop = 16;
            _tournamentsList.style.paddingBottom = 16;
            _tournamentsList.style.paddingLeft = 16;
            _tournamentsList.style.paddingRight = 16;
            
            _tournamentsContainer.Add(sectionHeader);
            _tournamentsContainer.Add(_tournamentsList);
            
            _rootElement.Add(_tournamentsContainer);
        }
        
        private void CreatePersonalStatsSection()
        {
            _personalStatsContainer = new VisualElement();
            _personalStatsContainer.name = "personal-stats-section";
            _personalStatsContainer.style.flexGrow = 1;
            
            // Competitive level progress
            var levelSection = new VisualElement();
            levelSection.style.marginBottom = 20;
            
            var levelLabel = new Label("Competitive Level");
            levelLabel.style.fontSize = 16;
            // levelLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            levelLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            levelLabel.style.marginBottom = 8;
            
            _competitiveLevelBar = new UIProgressBar(1f);
            _competitiveLevelBar.Format = "Level {0} - {1:P0} to next level";
            _competitiveLevelBar.style.height = 24;
            // _competitiveLevelBar.SetColor(_uiManager.DesignSystem.ColorPalette.AccentGold);
            
            levelSection.Add(levelLabel);
            levelSection.Add(_competitiveLevelBar);
            
            // Stats grid
            var statsLabel = new Label("Performance Statistics");
            statsLabel.style.fontSize = 16;
            // statsLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            statsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            statsLabel.style.marginBottom = 12;
            
            _statsGrid = new VisualElement();
            _statsGrid.name = "stats-grid";
            _statsGrid.style.flexDirection = FlexDirection.Row;
            _statsGrid.style.flexWrap = Wrap.Wrap;
            _statsGrid.style.marginBottom = 20;
            
            // Personal records grid
            var recordsLabel = new Label("Personal Records");
            recordsLabel.style.fontSize = 16;
            // recordsLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            recordsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            recordsLabel.style.marginBottom = 12;
            
            _recordsGrid = new VisualElement();
            _recordsGrid.name = "records-grid";
            _recordsGrid.style.flexDirection = FlexDirection.Row;
            _recordsGrid.style.flexWrap = Wrap.Wrap;
            
            _personalStatsContainer.Add(levelSection);
            _personalStatsContainer.Add(statsLabel);
            _personalStatsContainer.Add(_statsGrid);
            _personalStatsContainer.Add(recordsLabel);
            _personalStatsContainer.Add(_recordsGrid);
            
            _rootElement.Add(_personalStatsContainer);
        }
        
        private void CreateAchievementsSection()
        {
            _achievementsContainer = new VisualElement();
            _achievementsContainer.name = "achievements-section";
            _achievementsContainer.style.flexGrow = 1;
            
            var comingSoonLabel = new Label("🚧 Competitive Achievements Coming Soon!");
            comingSoonLabel.style.fontSize = 18;
            // comingSoonLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            comingSoonLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            comingSoonLabel.style.alignSelf = Align.Center;
            comingSoonLabel.style.marginTop = 100;
            
            _achievementsContainer.Add(comingSoonLabel);
            
            _rootElement.Add(_achievementsContainer);
        }
        
        private void ShowTab(string tabName)
        {
            _currentTab = tabName;
            
            // Update tab button states
            _leaderboardTab?.RemoveFromClassList("tab-active");
            _tournamentsTab?.RemoveFromClassList("tab-active");
            _statsTab?.RemoveFromClassList("tab-active");
            _achievementsTab?.RemoveFromClassList("tab-active");
            
            // Hide all containers
            _leaderboardContainer?.AddToClassList("hidden");
            _tournamentsContainer?.AddToClassList("hidden");
            _personalStatsContainer?.AddToClassList("hidden");
            _achievementsContainer?.AddToClassList("hidden");
            
            // Show selected tab
            switch (tabName)
            {
                case "leaderboard":
                    _leaderboardTab?.AddToClassList("tab-active");
                    _leaderboardContainer?.RemoveFromClassList("hidden");
                    _titleLabel.text = "Global Leaderboards";
                    RefreshLeaderboardDisplay();
                    break;
                case "tournaments":
                    _tournamentsTab?.AddToClassList("tab-active");
                    _tournamentsContainer?.RemoveFromClassList("hidden");
                    _titleLabel.text = "Tournaments & Events";
                    RefreshTournamentsDisplay();
                    break;
                case "stats":
                    _statsTab?.AddToClassList("tab-active");
                    _personalStatsContainer?.RemoveFromClassList("hidden");
                    _titleLabel.text = "Your Competitive Stats";
                    RefreshPersonalStatsDisplay();
                    break;
                case "achievements":
                    _achievementsTab?.AddToClassList("tab-active");
                    _achievementsContainer?.RemoveFromClassList("hidden");
                    _titleLabel.text = "Competitive Records";
                    break;
            }
        }
        
        private void SelectLeaderboard(ProjectChimera.Data.LeaderboardType type)
        {
            _currentLeaderboardType = type;
            
            // Update button states
            foreach (var button in _leaderboardTypeSelector.Children().OfType<Button>())
            {
                button.RemoveFromClassList("selected");
            }
            
            var selectedButton = _leaderboardTypeSelector.Q<Button>($"leaderboard-{type.ToString().ToLower()}");
            selectedButton?.AddToClassList("selected");
            
            RefreshLeaderboardDisplay();
        }
        
        [ContextMenu("Refresh Competitive Data")]
        private void RefreshCompetitiveData()
        {
            // if (_competitiveManager == null) return;
            
            // Update data based on current tab
            switch (_currentTab)
            {
                case "leaderboard":
                    RefreshLeaderboardDisplay();
                    break;
                case "tournaments":
                    RefreshTournamentsDisplay();
                    break;
                case "stats":
                    RefreshPersonalStatsDisplay();
                    break;
            }
            
            UpdatePlayerRankCard();
            _lastRefreshTime = Time.time;
        }
        
        private void RefreshLeaderboardDisplay()
        {
            // if (_competitiveManager == null) return;
            
            _leaderboardList.Clear();
            
            // _lastLeaderboardData = _competitiveManager.GetLeaderboardDisplayData(_currentLeaderboardType, 20);
            
            if (_lastLeaderboardData.Count == 0)
            {
                ShowEmptyLeaderboardState();
                return;
            }
            
            foreach (var entry in _lastLeaderboardData)
            {
                var entryElement = CreateLeaderboardEntryElement(entry);
                _leaderboardList.Add(entryElement);
            }
        }
        
        private void RefreshTournamentsDisplay()
        {
            // if (_competitiveManager == null) return;
            
            _tournamentsList.Clear();
            
            // var activeCompetitions = _competitiveManager.ActiveCompetitions;
            var activeCompetitions = new List<CompetitionEvent>(); // Placeholder
            
            if (activeCompetitions.Count == 0)
            {
                ShowEmptyTournamentsState();
                return;
            }
            
            foreach (var competition in activeCompetitions)
            {
                var competitionElement = CreateCompetitionElement(competition);
                _tournamentsList.Add(competitionElement);
            }
        }
        
        private void RefreshPersonalStatsDisplay()
        {
            // if (_competitiveManager == null) return;
            
            // _lastStatsData = _competitiveManager.GetCompetitiveStatsSummary();
            
            // Update competitive level bar
            _competitiveLevelBar.Value = _lastStatsData.NextLevelProgress;
            _competitiveLevelBar.Format = $"Level {_lastStatsData.CompetitiveLevel} - {{1:P0}} to next level";
            
            // Update stats grid
            RefreshStatsGrid();
            
            // Update records grid
            RefreshRecordsGrid();
        }
        
        private void RefreshStatsGrid()
        {
            _statsGrid.Clear();
            
            var stats = new[]
            {
                ("🏆", "Competitions Won", $"{_lastStatsData.TotalWins}"),
                ("🥉", "Podium Finishes", $"{_lastStatsData.TotalPodiumFinishes}"),
                ("📈", "Win Rate", $"{_lastStatsData.WinRate:P1}"),
                ("🎯", "Competitions Entered", $"{_lastStatsData.TotalCompetitionsEntered}")
            };
            
            foreach (var (icon, label, value) in stats)
            {
                var statCard = CreateStatCard(icon, label, value);
                _statsGrid.Add(statCard);
            }
        }
        
        private void RefreshRecordsGrid()
        {
            _recordsGrid.Clear();
            
            var records = new[]
            {
                ("🌱", "Highest Yield", $"{_lastStatsData.PersonalRecords.HighestSingleHarvest:F1}g"),
                ("⭐", "Best Quality", $"{_lastStatsData.PersonalRecords.HighestQualityRating:F1}%"),
                ("⚡", "Fastest Growth", _lastStatsData.PersonalRecords.FastestGrowthCycle > 0 ? $"{_lastStatsData.PersonalRecords.FastestGrowthCycle:F1} days" : "Not set"),
                ("💰", "Biggest Profit", $"${_lastStatsData.PersonalRecords.LargestSingleProfit:F0}")
            };
            
            foreach (var (icon, label, value) in records)
            {
                var recordCard = CreateStatCard(icon, label, value);
                _recordsGrid.Add(recordCard);
            }
        }
        
        private VisualElement CreateLeaderboardEntryElement(LeaderboardDisplayData entry)
        {
            var container = new VisualElement();
            container.name = "leaderboard-entry";
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.backgroundColor = entry.IsCurrentPlayer ? 
                new Color(0.2f, 0.4f, 0.2f, 0.5f) : 
                new Color(0.1f, 0.1f, 0.1f, 0.3f);
            container.style.borderTopLeftRadius = 8;
            container.style.borderTopRightRadius = 8;
            container.style.borderBottomLeftRadius = 8;
            container.style.borderBottomRightRadius = 8;
            container.style.paddingTop = 12;
            container.style.paddingBottom = 12;
            container.style.paddingLeft = 16;
            container.style.paddingRight = 16;
            container.style.marginBottom = 8;
            
            // Rank badge
            var rankBadge = new Label($"{entry.Badge} #{entry.Rank}");
            rankBadge.style.fontSize = 16;
            // rankBadge.style.color = entry.Rank <= 3 ? _uiManager.DesignSystem.ColorPalette.AccentGold : _uiManager.DesignSystem.ColorPalette.Info;
            rankBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
            rankBadge.style.width = 80;
            
            // Player info
            var playerInfo = new VisualElement();
            playerInfo.style.flexDirection = FlexDirection.Column;
            playerInfo.style.flexGrow = 1;
            playerInfo.style.marginLeft = 16;
            
            var playerName = new Label(entry.PlayerName);
            playerName.style.fontSize = 14;
            // playerName.style.color = entry.IsCurrentPlayer ? _uiManager.DesignSystem.ColorPalette.Success : _uiManager.DesignSystem.ColorPalette.TextPrimary;
            playerName.style.unityFontStyleAndWeight = entry.IsCurrentPlayer ? FontStyle.Bold : FontStyle.Normal;
            
            var lastActive = new Label($"Last active: {GetTimeAgoString(entry.LastActive)}");
            lastActive.style.fontSize = 10;
            // lastActive.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            playerInfo.Add(playerName);
            playerInfo.Add(lastActive);
            
            // Score
            var scoreLabel = new Label(entry.FormattedScore);
            scoreLabel.style.fontSize = 16;
            // scoreLabel.style.color = _uiManager.DesignSystem.ColorPalette.AccentGold;
            scoreLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            scoreLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            scoreLabel.style.width = 120;
            
            container.Add(rankBadge);
            container.Add(playerInfo);
            container.Add(scoreLabel);
            
            return container;
        }
        
        private VisualElement CreateCompetitionElement(CompetitionEvent competition)
        {
            var container = new VisualElement();
            container.name = "competition-item";
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
            
            // Header
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.justifyContent = Justify.SpaceBetween;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 8;
            
            var titleLabel = new Label($"🏆 {competition.Name}");
            titleLabel.style.fontSize = 16;
            // titleLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            var statusBadge = new Label(competition.IsActive ? "ACTIVE" : "ENDED");
            statusBadge.style.fontSize = 10;
            statusBadge.style.color = Color.white;
            // statusBadge.style.backgroundColor = competition.IsActive ? _uiManager.DesignSystem.ColorPalette.Success : _uiManager.DesignSystem.ColorPalette.TextSecondary;
            statusBadge.style.paddingTop = 4;
            statusBadge.style.paddingBottom = 4;
            statusBadge.style.paddingLeft = 8;
            statusBadge.style.paddingRight = 8;
            statusBadge.style.borderTopLeftRadius = 4;
            statusBadge.style.borderTopRightRadius = 4;
            statusBadge.style.borderBottomLeftRadius = 4;
            statusBadge.style.borderBottomRightRadius = 4;
            
            headerRow.Add(titleLabel);
            headerRow.Add(statusBadge);
            
            // Description
            var descriptionLabel = new Label(competition.Description);
            descriptionLabel.style.fontSize = 14;
            // descriptionLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            descriptionLabel.style.marginBottom = 12;
            
            // Time remaining
            var timeRemaining = competition.EndTime - System.DateTime.Now;
            var timeText = timeRemaining.TotalDays > 1 ? 
                $"⏰ {timeRemaining.Days}d {timeRemaining.Hours}h remaining" :
                $"⏰ {timeRemaining.Hours}h {timeRemaining.Minutes}m remaining";
            
            var timeLabel = new Label(timeText);
            timeLabel.style.fontSize = 12;
            // timeLabel.style.color = timeRemaining.TotalHours < 24 ? 
                // _uiManager.DesignSystem.ColorPalette.Warning : 
                // _uiManager.DesignSystem.ColorPalette.Info;
            
            container.Add(headerRow);
            container.Add(descriptionLabel);
            container.Add(timeLabel);
            
            return container;
        }
        
        private VisualElement CreateStatCard(string icon, string label, string value)
        {
            var card = new VisualElement();
            card.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.6f);
            card.style.borderTopLeftRadius = 8;
            card.style.borderTopRightRadius = 8;
            card.style.borderBottomLeftRadius = 8;
            card.style.borderBottomRightRadius = 8;
            card.style.paddingTop = 12;
            card.style.paddingBottom = 12;
            card.style.paddingLeft = 16;
            card.style.paddingRight = 16;
            card.style.marginRight = 12;
            card.style.marginBottom = 12;
            card.style.width = 150;
            card.style.alignItems = Align.Center;
            
            var iconLabel = new Label(icon);
            iconLabel.style.fontSize = 24;
            iconLabel.style.marginBottom = 4;
            
            var valueLabel = new Label(value);
            valueLabel.style.fontSize = 18;
            // valueLabel.style.color = _uiManager.DesignSystem.ColorPalette.AccentGold;
            valueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            valueLabel.style.marginBottom = 2;
            
            var labelText = new Label(label);
            labelText.style.fontSize = 10;
            // labelText.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            labelText.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            card.Add(iconLabel);
            card.Add(valueLabel);
            card.Add(labelText);
            
            return card;
        }
        
        private void UpdatePlayerRankCard()
        {
            // if (_competitiveManager == null) return;
            
            // var overallRank = _competitiveManager.GetPlayerRanking(ProjectChimera.Data.LeaderboardType.Overall);
            // Placeholder until competitive manager is implemented
            var overallRank = _lastKnownRank > 0 ? _lastKnownRank : 0;
            
            var rankLabel = _playerRankCard.Q<Label>("current-rank");
            var categoryLabel = _playerRankCard.Q<Label>("rank-category");
            
            if (rankLabel != null)
            {
                rankLabel.text = overallRank > 0 ? $"#{overallRank}" : "Unranked";
                // rankLabel.style.color = overallRank <= 10 ? _uiManager.DesignSystem.ColorPalette.AccentGold : _uiManager.DesignSystem.ColorPalette.Info;
            }
            
            if (categoryLabel != null)
            {
                categoryLabel.text = $"Overall • {_lastLeaderboardData.Count} total players";
            }
            
            // Update ranking summary
            if (overallRank > 0)
            {
                _rankingSummaryLabel.text = $"You're ranked #{overallRank} globally • Keep competing to climb higher!";
            }
            // else
            // {
                _rankingSummaryLabel.text = "Complete objectives to establish your ranking";
            // }
        }
        
        private void ShowEmptyLeaderboardState()
        {
            var emptyState = new VisualElement();
            emptyState.style.alignItems = Align.Center;
            emptyState.style.justifyContent = Justify.Center;
            emptyState.style.flexGrow = 1;
            emptyState.style.paddingTop = 60;
            emptyState.style.paddingBottom = 60;
            
            var emptyIcon = new Label("🏆");
            emptyIcon.style.fontSize = 48;
            emptyIcon.style.marginBottom = 16;
            
            var emptyText = new Label("Leaderboard updating...");
            emptyText.style.fontSize = 18;
            // emptyText.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            emptyText.style.marginBottom = 8;
            
            var emptyDescription = new Label("Complete objectives to see your ranking");
            emptyDescription.style.fontSize = 14;
            // emptyDescription.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            emptyDescription.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            emptyState.Add(emptyIcon);
            emptyState.Add(emptyText);
            emptyState.Add(emptyDescription);
            
            _leaderboardList.Add(emptyState);
        }
        
        private void ShowEmptyTournamentsState()
        {
            var emptyState = new VisualElement();
            emptyState.style.alignItems = Align.Center;
            emptyState.style.justifyContent = Justify.Center;
            emptyState.style.flexGrow = 1;
            emptyState.style.paddingTop = 60;
            emptyState.style.paddingBottom = 60;
            
            var emptyIcon = new Label("⚔️");
            emptyIcon.style.fontSize = 48;
            emptyIcon.style.marginBottom = 16;
            
            var emptyText = new Label("No active tournaments");
            emptyText.style.fontSize = 18;
            // emptyText.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            emptyText.style.marginBottom = 8;
            
            var emptyDescription = new Label("New tournaments start weekly on Mondays");
            emptyDescription.style.fontSize = 14;
            // emptyDescription.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            emptyDescription.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            emptyState.Add(emptyIcon);
            emptyState.Add(emptyText);
            emptyState.Add(emptyDescription);
            
            _tournamentsList.Add(emptyState);
        }
        
        private string GetTimeAgoString(System.DateTime dateTime)
        {
            var timeSpan = System.DateTime.Now - dateTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            else if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            else if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours}h ago";
            else if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            else
                return dateTime.ToString("MMM dd");
        }
        
        // Event handlers
        private void OnRankingChanged(ProjectChimera.Data.LeaderboardType leaderboardType, int newRank)
        {
            if (_showAnimations && newRank < _lastKnownRank && _lastKnownRank > 0)
            {
                ShowRankUpCelebration(newRank, _lastKnownRank);
            }
            
            _lastKnownRank = newRank;
            
            if (_currentTab == "leaderboard")
            {
                RefreshLeaderboardDisplay();
            }
            
            UpdatePlayerRankCard();
        }
        
        private void OnCompetitionStarted(CompetitionEvent competition)
        {
            if (_currentTab == "tournaments")
            {
                RefreshTournamentsDisplay();
            }
            
            ShowNotification($"🏆 {competition.Name} has started!", UIStatus.Info);
        }
        
        private void OnCompetitionEnded(CompetitionEvent competition)
        {
            if (_currentTab == "tournaments")
            {
                RefreshTournamentsDisplay();
            }
            
            ShowNotification($"🏁 {competition.Name} has ended!", UIStatus.Info);
        }
        
        private void OnPersonalRecordSet(RecordType recordType, float value)
        {
            if (_showAnimations)
            {
                ShowPersonalRecordCelebration(recordType, value);
            }
            
            if (_currentTab == "stats")
            {
                RefreshPersonalStatsDisplay();
            }
        }
        
        private void ShowRankUpCelebration(int newRank, int oldRank)
        {
            var celebration = new VisualElement();
            celebration.style.position = Position.Absolute;
            celebration.style.top = 0;
            celebration.style.left = 0;
            celebration.style.right = 0;
            celebration.style.bottom = 0;
            celebration.style.backgroundColor = new Color(0f, 0f, 0f, 0.7f);
            celebration.style.alignItems = Align.Center;
            celebration.style.justifyContent = Justify.Center;
            
            var celebrationText = new Label($"🎉 Rank Up!\n#{oldRank} → #{newRank}");
            celebrationText.style.fontSize = 24;
            // celebrationText.style.color = _uiManager.DesignSystem.ColorPalette.AccentGold;
            celebrationText.style.unityFontStyleAndWeight = FontStyle.Bold;
            celebrationText.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            celebration.Add(celebrationText);
            _rootElement.Add(celebration);
            
            if (_rankUpSound != null)
            {
                AudioSource.PlayClipAtPoint(_rankUpSound, Camera.main.transform.position);
            }
            
            StartCoroutine(RemoveCelebrationAfterDelay(celebration, 3f));
        }
        
        private void ShowPersonalRecordCelebration(RecordType recordType, float value)
        {
            var recordName = recordType switch
            {
                RecordType.HighestYield => "Highest Yield",
                RecordType.HighestQuality => "Best Quality",
                RecordType.FastestGrowth => "Fastest Growth",
                RecordType.ProfitGenerated => "Biggest Profit",
                RecordType.StrainMastery => "Strain Mastery",
                _ => "Personal Record"
            };
            
            ShowNotification($"🏆 New {recordName}: {value:F1}!", UIStatus.Success);
            
            if (_achievementSound != null)
            {
                AudioSource.PlayClipAtPoint(_achievementSound, Camera.main.transform.position);
            }
        }
        
        private System.Collections.IEnumerator RemoveCelebrationAfterDelay(VisualElement element, float delay)
        {
            yield return new WaitForSeconds(delay);
            _rootElement.Remove(element);
        }
        
        private void ShowNotification(string message, UIStatus status)
        {
            // This would integrate with the game's notification system
            Debug.Log($"Competitive Notification: {message}");
        }
        
        protected virtual void OnDestroy()
        {
            // Clean up competitive panel
            
            // Unsubscribe from events
            // if (_competitiveManager != null)
            // {
                // _competitiveManager.OnRankingChanged -= OnRankingChanged;
                // _competitiveManager.OnCompetitionStarted -= OnCompetitionStarted;
                // _competitiveManager.OnCompetitionEnded -= OnCompetitionEnded;
                // _competitiveManager.OnPersonalRecordSet -= OnPersonalRecordSet;
            // }
            
            // Stop auto-refresh
            CancelInvoke(nameof(RefreshCompetitiveData));
        }
    }
}