using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.UI.Core;
using ProjectChimera.UI.Events;
using ProjectChimera.UI.Components;
using ProjectChimera.Data;
using ProjectChimera.Data.UI;

namespace ProjectChimera.UI.Panels
{
    /// <summary>
    /// Gameplay HUD panel for Project Chimera.
    /// Provides real-time game information and quick access to key functions during gameplay.
    /// </summary>
    public class GameplayHUDPanel : UIPanel
    {
        [Header("HUD Events")]
        [SerializeField] private SimpleGameEventSO _onPauseClicked;
        [SerializeField] private SimpleGameEventSO _onSettingsClicked;
        [SerializeField] private SimpleGameEventSO _onMenuClicked;
        [SerializeField] private UIButtonClickEventSO _onHUDButtonClicked;
        
        [Header("Update Settings")]
        [SerializeField] private float _updateInterval = 1f;
        [SerializeField] private bool _enableRealTimeUpdates = true;
        
        // Main HUD containers
        private VisualElement _topBar;
        private VisualElement _bottomBar;
        private VisualElement _sidePanel;
        private VisualElement _centerHUD;
        private VisualElement _notificationArea;
        
        // Top bar elements
        private UIDataCard _cashCard;
        private UIDataCard _timeCard;
        private UIProgressBar _overallProgressBar;
        private Button _pauseButton;
        private Button _settingsButton;
        private Button _menuButton;
        
        // Bottom bar elements
        private VisualElement _quickActionBar;
        private Button _facilitiesButton;
        private Button _plantsButton;
        private Button _researchButton;
        private Button _marketButton;
        private Button _inventoryButton;
        
        // Side panel elements
        private UIStatusIndicator _facilityStatus;
        private UIStatusIndicator _plantHealthStatus;
        private UIStatusIndicator _financeStatus;
        private UISimpleChart _profitChart;
        
        // Center HUD elements
        private Label _currentTaskLabel;
        private UIProgressBar _currentTaskProgress;
        private VisualElement _alertsContainer;
        
        // State tracking
        private float _lastUpdateTime;
        private bool _isUpdatePaused;
        private Queue<UINotificationToast> _notificationQueue;
        
        // Game managers (would be injected or found)
        // private TimeManager _timeManager;
        // private DataManager _dataManager;
        
        protected override void SetupUIElements()
        {
            base.SetupUIElements();
            
            // Initialize state
            _notificationQueue = new Queue<UINotificationToast>();
            
            // Get manager references
            // _timeManager = GameManager.Instance?.GetManager<TimeManager>();
            // _dataManager = GameManager.Instance?.GetManager<DataManager>();
            
            CreateHUDLayout();
            CreateTopBar();
            CreateBottomBar();
            CreateSidePanel();
            CreateCenterHUD();
            CreateNotificationArea();
            
            StartHUDUpdates();
        }
        
        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            
            // Top bar buttons
            _pauseButton?.RegisterCallback<ClickEvent>(OnPauseClicked);
            _settingsButton?.RegisterCallback<ClickEvent>(OnSettingsClicked);
            _menuButton?.RegisterCallback<ClickEvent>(OnMenuClicked);
            
            // Quick action buttons
            _facilitiesButton?.RegisterCallback<ClickEvent>(OnFacilitiesClicked);
            _plantsButton?.RegisterCallback<ClickEvent>(OnPlantsClicked);
            _researchButton?.RegisterCallback<ClickEvent>(OnResearchClicked);
            _marketButton?.RegisterCallback<ClickEvent>(OnMarketClicked);
            _inventoryButton?.RegisterCallback<ClickEvent>(OnInventoryClicked);
            
            // Setup hover effects
            SetupButtonEffects();
        }
        
        /// <summary>
        /// Create the main HUD layout structure
        /// </summary>
        private void CreateHUDLayout()
        {
            _rootElement.Clear();
            
            // Main container with flex layout
            var mainContainer = new VisualElement();
            mainContainer.name = "hud-main-container";
            mainContainer.style.flexGrow = 1;
            mainContainer.style.flexDirection = FlexDirection.Column;
            
            // Top bar
            _topBar = new VisualElement();
            _topBar.name = "top-bar";
            _topBar.style.height = 60;
            _topBar.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            _topBar.style.flexDirection = FlexDirection.Row;
            _topBar.style.alignItems = Align.Center;
            _topBar.style.justifyContent = Justify.SpaceBetween;
            _topBar.style.paddingLeft = 16;
            _topBar.style.paddingRight = 16;
            
            // Center area container
            var centerContainer = new VisualElement();
            centerContainer.name = "center-container";
            centerContainer.style.flexGrow = 1;
            centerContainer.style.flexDirection = FlexDirection.Row;
            
            // Center HUD
            _centerHUD = new VisualElement();
            _centerHUD.name = "center-hud";
            _centerHUD.style.flexGrow = 1;
            _centerHUD.style.position = Position.Relative;
            
            // Side panel
            _sidePanel = new VisualElement();
            _sidePanel.name = "side-panel";
            _sidePanel.style.width = 300;
            _sidePanel.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            _sidePanel.style.paddingTop = 16;
            _sidePanel.style.paddingBottom = 16;
            _sidePanel.style.paddingLeft = 16;
            _sidePanel.style.paddingRight = 16;
            
            // Bottom bar
            _bottomBar = new VisualElement();
            _bottomBar.name = "bottom-bar";
            _bottomBar.style.height = 80;
            _bottomBar.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            _bottomBar.style.flexDirection = FlexDirection.Row;
            _bottomBar.style.alignItems = Align.Center;
            _bottomBar.style.justifyContent = Justify.Center;
            _bottomBar.style.paddingLeft = 16;
            _bottomBar.style.paddingRight = 16;
            
            centerContainer.Add(_centerHUD);
            centerContainer.Add(_sidePanel);
            
            mainContainer.Add(_topBar);
            mainContainer.Add(centerContainer);
            mainContainer.Add(_bottomBar);
            
            _rootElement.Add(mainContainer);
        }
        
        /// <summary>
        /// Create the top bar with game information and controls
        /// </summary>
        private void CreateTopBar()
        {
            // Left section - Game status
            var leftSection = new VisualElement();
            leftSection.name = "top-bar-left";
            leftSection.style.flexDirection = FlexDirection.Row;
            leftSection.style.alignItems = Align.Center;
            
            // Cash display
            _cashCard = new UIDataCard("Cash", "$0", "");
            _cashCard.style.marginRight = 16;
            // _cashCard.SetValueColor(_uiManager.DesignSystem.ColorPalette.AccentGold);
            
            // Time display
            _timeCard = new UIDataCard("Day", "1", "");
            _timeCard.style.marginRight = 16;
            
            // Overall progress
            _overallProgressBar = new UIProgressBar(100f);
            _overallProgressBar.Format = "Progress: {0:F0}%";
            _overallProgressBar.style.minWidth = 200;
            // _overallProgressBar.SetColor(_uiManager.DesignSystem.ColorPalette.PrimaryGreen);
            
            leftSection.Add(_cashCard);
            leftSection.Add(_timeCard);
            leftSection.Add(_overallProgressBar);
            
            // Right section - Controls
            var rightSection = new VisualElement();
            rightSection.name = "top-bar-right";
            rightSection.style.flexDirection = FlexDirection.Row;
            rightSection.style.alignItems = Align.Center;
            
            // Pause button
            _pauseButton = new Button();
            _pauseButton.name = "pause-button";
            _pauseButton.text = "⏸";
            _pauseButton.style.width = 40;
            _pauseButton.style.height = 40;
            _pauseButton.style.marginRight = 8;
            // _uiManager.ApplyDesignSystemStyle(_pauseButton, UIStyleToken.SecondaryButton);
            
            // Settings button
            _settingsButton = new Button();
            _settingsButton.name = "settings-button";
            _settingsButton.text = "⚙";
            _settingsButton.style.width = 40;
            _settingsButton.style.height = 40;
            _settingsButton.style.marginRight = 8;
            // _uiManager.ApplyDesignSystemStyle(_settingsButton, UIStyleToken.SecondaryButton);
            
            // Menu button
            _menuButton = new Button();
            _menuButton.name = "menu-button";
            _menuButton.text = "☰";
            _menuButton.style.width = 40;
            _menuButton.style.height = 40;
            // _uiManager.ApplyDesignSystemStyle(_menuButton, UIStyleToken.SecondaryButton);
            
            rightSection.Add(_pauseButton);
            rightSection.Add(_settingsButton);
            rightSection.Add(_menuButton);
            
            _topBar.Add(leftSection);
            _topBar.Add(rightSection);
        }
        
        /// <summary>
        /// Create the bottom bar with quick actions
        /// </summary>
        private void CreateBottomBar()
        {
            _quickActionBar = new VisualElement();
            _quickActionBar.name = "quick-action-bar";
            _quickActionBar.style.flexDirection = FlexDirection.Row;
            _quickActionBar.style.justifyContent = Justify.Center;
            _quickActionBar.style.alignItems = Align.Center;
            
            // Quick action buttons
            _facilitiesButton = CreateQuickActionButton("🏭", "Facilities", "Manage your cultivation facilities");
            _plantsButton = CreateQuickActionButton("🌱", "Plants", "Monitor and care for your plants");
            _researchButton = CreateQuickActionButton("🔬", "Research", "Unlock new technologies and techniques");
            _marketButton = CreateQuickActionButton("📈", "Market", "View market prices and trading opportunities");
            _inventoryButton = CreateQuickActionButton("📦", "Inventory", "Manage seeds, nutrients, and equipment");
            
            _quickActionBar.Add(_facilitiesButton);
            _quickActionBar.Add(_plantsButton);
            _quickActionBar.Add(_researchButton);
            _quickActionBar.Add(_marketButton);
            _quickActionBar.Add(_inventoryButton);
            
            _bottomBar.Add(_quickActionBar);
        }
        
        /// <summary>
        /// Create a quick action button
        /// </summary>
        private Button CreateQuickActionButton(string icon, string label, string tooltip)
        {
            var button = new Button();
            button.name = label.ToLower() + "-quick-button";
            
            // Create button content
            var content = new VisualElement();
            content.style.alignItems = Align.Center;
            content.pickingMode = PickingMode.Ignore;
            
            var iconLabel = new Label(icon);
            iconLabel.style.fontSize = 24;
            iconLabel.style.marginBottom = 4;
            
            var textLabel = new Label(label);
            textLabel.style.fontSize = 12;
            // textLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            content.Add(iconLabel);
            content.Add(textLabel);
            button.Add(content);
            
            // Styling
            button.style.width = 80;
            button.style.height = 60;
            button.style.marginLeft = 8;
            button.style.marginRight = 8;
            button.style.backgroundColor = Color.clear;
            button.style.borderTopWidth = 0;
            button.style.borderRightWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderTopLeftRadius = 8;
            button.style.borderTopRightRadius = 8;
            button.style.borderBottomLeftRadius = 8;
            button.style.borderBottomRightRadius = 8;
            
            // Tooltip
            if (!string.IsNullOrEmpty(tooltip))
            {
                button.SetupTooltip(tooltip, _rootElement);
            }
            
            return button;
        }
        
        /// <summary>
        /// Create the side panel with detailed information
        /// </summary>
        private void CreateSidePanel()
        {
            // Title
            var titleLabel = new Label("Status Overview");
            titleLabel.name = "side-panel-title";
            titleLabel.style.fontSize = 16;
            // titleLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.marginBottom = 16;
            
            // Status indicators
            _facilityStatus = new UIStatusIndicator(UIStatus.Success, "Facilities Online");
            _facilityStatus.style.marginBottom = 8;
            
            _plantHealthStatus = new UIStatusIndicator(UIStatus.Success, "Plants Healthy");
            _plantHealthStatus.style.marginBottom = 8;
            
            _financeStatus = new UIStatusIndicator(UIStatus.Warning, "Cash Flow Positive");
            _financeStatus.style.marginBottom = 16;
            
            // Profit chart
            _profitChart = new UISimpleChart("Daily Profit");
            _profitChart.style.marginBottom = 16;
            _profitChart.SetRange(-1000f, 5000f);
            
            // Sample data for chart
            var sampleData = new List<float> { 100f, 250f, 180f, 320f, 450f, 380f, 520f };
            _profitChart.SetData(sampleData);
            
            _sidePanel.Add(titleLabel);
            _sidePanel.Add(_facilityStatus);
            _sidePanel.Add(_plantHealthStatus);
            _sidePanel.Add(_financeStatus);
            _sidePanel.Add(_profitChart);
        }
        
        /// <summary>
        /// Create the center HUD elements
        /// </summary>
        private void CreateCenterHUD()
        {
            // Current task display
            var taskContainer = new VisualElement();
            taskContainer.name = "task-container";
            taskContainer.style.position = Position.Absolute;
            taskContainer.style.top = 20;
            taskContainer.style.left = 20;
            taskContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            taskContainer.style.borderTopLeftRadius = 8;
            taskContainer.style.borderTopRightRadius = 8;
            taskContainer.style.borderBottomLeftRadius = 8;
            taskContainer.style.borderBottomRightRadius = 8;
            taskContainer.style.paddingTop = 12;
            taskContainer.style.paddingBottom = 12;
            taskContainer.style.paddingLeft = 16;
            taskContainer.style.paddingRight = 16;
            taskContainer.style.minWidth = 250;
            
            _currentTaskLabel = new Label("Current Task: Plant Monitoring");
            _currentTaskLabel.name = "current-task-label";
            _currentTaskLabel.style.fontSize = 14;
            // _currentTaskLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            _currentTaskLabel.style.marginBottom = 8;
            
            _currentTaskProgress = new UIProgressBar(100f);
            _currentTaskProgress.Value = 65f;
            _currentTaskProgress.Format = "{0:F0}% Complete";
            // _currentTaskProgress.SetColor(_uiManager.DesignSystem.ColorPalette.Info);
            
            taskContainer.Add(_currentTaskLabel);
            taskContainer.Add(_currentTaskProgress);
            
            // Alerts container
            _alertsContainer = new VisualElement();
            _alertsContainer.name = "alerts-container";
            _alertsContainer.style.position = Position.Absolute;
            _alertsContainer.style.bottom = 20;
            _alertsContainer.style.left = 20;
            _alertsContainer.style.right = 20;
            _alertsContainer.style.flexDirection = FlexDirection.Column;
            _alertsContainer.style.alignItems = Align.FlexStart;
            
            _centerHUD.Add(taskContainer);
            _centerHUD.Add(_alertsContainer);
        }
        
        /// <summary>
        /// Create the notification area
        /// </summary>
        private void CreateNotificationArea()
        {
            _notificationArea = new VisualElement();
            _notificationArea.name = "notification-area";
            _notificationArea.style.position = Position.Absolute;
            _notificationArea.style.top = 80;
            _notificationArea.style.right = 20;
            _notificationArea.style.width = 350;
            _notificationArea.style.flexDirection = FlexDirection.Column;
            _notificationArea.style.alignItems = Align.FlexEnd;
            
            _rootElement.Add(_notificationArea);
        }
        
        /// <summary>
        /// Setup button hover and click effects
        /// </summary>
        private void SetupButtonEffects()
        {
            var buttons = new[] { _pauseButton, _settingsButton, _menuButton, 
                                _facilitiesButton, _plantsButton, _researchButton, _marketButton, _inventoryButton };
            
            foreach (var button in buttons)
            {
                if (button == null) continue;
                
                button.AddClickAnimation();
                
                // var hoverColor = _uiManager.DesignSystem.ColorPalette.InteractiveHover;
                var hoverColor = new Color(0.67f, 0.47f, 1f, 1f); // Placeholder hover color
                var normalColor = button.style.backgroundColor.value;
                button.AddHoverEffects(hoverColor, normalColor);
            }
        }
        
        /// <summary>
        /// Start HUD updates
        /// </summary>
        private void StartHUDUpdates()
        {
            if (_enableRealTimeUpdates)
            {
                InvokeRepeating(nameof(UpdateHUDData), 0f, _updateInterval);
            }
        }
        
        /// <summary>
        /// Update HUD data from game managers
        /// </summary>
        private void UpdateHUDData()
        {
            if (_isUpdatePaused) return;
            
            UpdateFinancialData();
            UpdateTimeData();
            UpdateStatusIndicators();
            UpdateProgressBars();
            
            _lastUpdateTime = Time.time;
        }
        
        /// <summary>
        /// Update financial information
        /// </summary>
        private void UpdateFinancialData()
        {
            // This would integrate with the economics manager
            // For now, use sample data
            var currentCash = 125000f;
            _cashCard.Value = FormatCurrency(currentCash);
            
            // Update color based on cash flow
            if (currentCash < 10000f)
            {
                // _cashCard.SetValueColor(_uiManager.DesignSystem.ColorPalette.Error);
            }
            // else if (currentCash < 50000f)
            // {
                // _cashCard.SetValueColor(_uiManager.DesignSystem.ColorPalette.Warning);
            // }
            // else
            // {
                // _cashCard.SetValueColor(_uiManager.DesignSystem.ColorPalette.AccentGold);
            // }
        }
        
        /// <summary>
        /// Update time information
        /// </summary>
        private void UpdateTimeData()
        {
            // if (_timeManager != null)
            // {
                // Get current game day from time manager
                var currentDay = 1; // TODO: Get from time manager
                _timeCard.Value = currentDay.ToString();
                
                // Update time card with additional info
                var timeOfDay = System.DateTime.Now; // TODO: Get from time manager
                _timeCard.Unit = $"{timeOfDay:HH:mm}";
            // }
            // else
            // {
                // Fallback to sample data
                _timeCard.Value = "15";
                _timeCard.Unit = "14:30";
            // }
        }
        
        /// <summary>
        /// Update status indicators
        /// </summary>
        private void UpdateStatusIndicators()
        {
            // This would integrate with facility, cultivation, and economics managers
            // For now, use sample logic
            
            // Facility status
            var facilityHealth = 85f; // Would come from facility manager
            if (facilityHealth > 80f)
                _facilityStatus.Status = UIStatus.Success;
            else if (facilityHealth > 50f)
                _facilityStatus.Status = UIStatus.Warning;
            else
                _facilityStatus.Status = UIStatus.Error;
            
            // Plant health status
            var avgPlantHealth = 92f; // Would come from cultivation manager
            if (avgPlantHealth > 80f)
                _plantHealthStatus.Status = UIStatus.Success;
            else if (avgPlantHealth > 60f)
                _plantHealthStatus.Status = UIStatus.Warning;
            else
                _plantHealthStatus.Status = UIStatus.Error;
            
            // Finance status
            var cashFlow = 1500f; // Would come from economics manager
            if (cashFlow > 0f)
                _financeStatus.Status = UIStatus.Success;
            else if (cashFlow > -1000f)
                _financeStatus.Status = UIStatus.Warning;
            else
                _financeStatus.Status = UIStatus.Error;
        }
        
        /// <summary>
        /// Update progress bars
        /// </summary>
        private void UpdateProgressBars()
        {
            // Overall progress (could be based on research, skills, or objectives)
            var overallProgress = 67f; // Would come from progression manager
            _overallProgressBar.Value = overallProgress;
            
            // Current task progress
            var taskProgress = 78f; // Would come from task manager
            _currentTaskProgress.Value = taskProgress;
        }
        
        /// <summary>
        /// Format currency display
        /// </summary>
        private string FormatCurrency(float amount)
        {
            if (amount >= 1000000f)
                return $"${amount / 1000000f:F1}M";
            else if (amount >= 1000f)
                return $"${amount / 1000f:F1}K";
            else
                return $"${amount:F0}";
        }
        
        /// <summary>
        /// Show notification toast
        /// </summary>
        public void ShowNotification(string message, UIStatus type = UIStatus.Info, float duration = 5f)
        {
            var notification = new UINotificationToast(message, type, () => {
                // Notification closed callback
            });
            
            notification.Show(_notificationArea, duration);
            _notificationQueue.Enqueue(notification);
            
            // Limit number of notifications
            while (_notificationQueue.Count > 5)
            {
                var oldNotification = _notificationQueue.Dequeue();
                // Could remove old notification here
            }
        }
        
        /// <summary>
        /// Pause/resume HUD updates
        /// </summary>
        public void SetUpdatesPaused(bool paused)
        {
            _isUpdatePaused = paused;
        }
        
        // Event handlers
        private void OnPauseClicked(ClickEvent evt)
        {
            _onPauseClicked?.Raise();
            _onHUDButtonClicked?.RaiseButtonClick("pause", PanelId, evt.position);
            
            // Toggle pause state
            // _uiManager.SetUIState(UIState.Paused);
        }
        
        private void OnSettingsClicked(ClickEvent evt)
        {
            _onSettingsClicked?.Raise();
            _onHUDButtonClicked?.RaiseButtonClick("settings", PanelId, evt.position);
            
            // _uiManager.ShowPanel("settings-menu");
        }
        
        private void OnMenuClicked(ClickEvent evt)
        {
            _onMenuClicked?.Raise();
            _onHUDButtonClicked?.RaiseButtonClick("menu", PanelId, evt.position);
            
            // _uiManager.SetUIState(UIState.MainMenu);
        }
        
        private void OnFacilitiesClicked(ClickEvent evt)
        {
            _onHUDButtonClicked?.RaiseButtonClick("facilities", PanelId, evt.position);
            // _uiManager.ShowPanel("facility-management-panel");
        }
        
        private void OnPlantsClicked(ClickEvent evt)
        {
            _onHUDButtonClicked?.RaiseButtonClick("plants", PanelId, evt.position);
            // _uiManager.ShowPanel("plant-management-panel");
        }
        
        private void OnResearchClicked(ClickEvent evt)
        {
            _onHUDButtonClicked?.RaiseButtonClick("research", PanelId, evt.position);
            // _uiManager.ShowPanel("research-panel");
        }
        
        private void OnMarketClicked(ClickEvent evt)
        {
            _onHUDButtonClicked?.RaiseButtonClick("market", PanelId, evt.position);
            // _uiManager.ShowPanel("market-panel");
        }
        
        private void OnInventoryClicked(ClickEvent evt)
        {
            _onHUDButtonClicked?.RaiseButtonClick("inventory", PanelId, evt.position);
            // _uiManager.ShowPanel("inventory-panel");
        }
        
        protected virtual void OnDestroy()
        {
            // Clean up gameplay HUD panel
            
            // Clean up updates
            CancelInvoke(nameof(UpdateHUDData));
        }
    }
}