using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.Core;
using ProjectChimera.Systems.Competition;
using ProjectChimera.Systems.Narrative;
using ProjectChimera.Systems.IPM;
using ProjectChimera.Systems.Equipment;

namespace ProjectChimera.UI
{
    /// <summary>
    /// Phase 4.4.a: Advanced UI Management System
    /// Provides a modern, responsive interface that integrates all Phase 4 systems
    /// Features adaptive layouts, real-time updates, and contextual information display
    /// </summary>
    public class AdvancedUIManager : ChimeraManager
    {
        [Header("Phase 4.4.a Configuration")]
        public bool EnableAdvancedUI = true;
        public bool EnableResponsiveLayout = true;
        public bool EnableRealTimeUpdates = true;
        public bool EnableContextualHelp = true;
        
        [Header("UI Settings")]
        [Range(0.1f, 2f)] public float UIScale = 1f;
        [Range(30, 120)] public int UpdateFrequency = 60; // Updates per second
        [Range(0f, 1f)] public float AnimationSpeed = 0.8f;
        public UITheme CurrentTheme = UITheme.Dark;
        
        [Header("UI Root Elements")]
        [SerializeField] private VisualElement rootElement;
        [SerializeField] private VisualElement mainContainer;
        [SerializeField] private VisualElement headerBar;
        [SerializeField] private VisualElement contentArea;
        [SerializeField] private VisualElement sidePanel;
        [SerializeField] private VisualElement bottomBar;
        
        [Header("Active Panels")]
        [SerializeField] private Dictionary<string, UIPanel> activePanels = new Dictionary<string, UIPanel>();
        [SerializeField] private List<UINotification> activeNotifications = new List<UINotification>();
        [SerializeField] private Queue<UINotification> pendingNotifications = new Queue<UINotification>();
        [SerializeField] private Dictionary<string, UIWidget> activeWidgets = new Dictionary<string, UIWidget>();
        
        [Header("System Integration")]
        [SerializeField] private LegacyAchievementManager achievementManager;
        [SerializeField] private CannabisCupManager competitionManager;
        [SerializeField] private DynamicEventManager eventManager;
        [SerializeField] private NPCInteractionManager npcManager;
        [SerializeField] private CleanIPMManager pestManager;
        [SerializeField] private EquipmentDegradationManager equipmentManager;
        
        // Phase 4.4.a Data Structures
        [System.Serializable]
        public class UIPanel
        {
            public string PanelId;
            public string Title;
            public PanelType Type;
            public VisualElement Element;
            public Vector2 Position;
            public Vector2 Size;
            public bool IsVisible;
            public bool IsResizable;
            public bool IsDraggable;
            public PanelPriority Priority;
            public DateTime LastUpdate;
            public Dictionary<string, object> PanelData;
            public List<UIWidget> Widgets;
            public UIAnimation Animation;
        }
        
        [System.Serializable]
        public class UIWidget
        {
            public string WidgetId;
            public string Title;
            public WidgetType Type;
            public VisualElement Element;
            public Vector2 Position;
            public Vector2 Size;
            public bool IsActive;
            public bool AutoUpdate;
            public float UpdateInterval;
            public DateTime LastUpdate;
            public Dictionary<string, object> WidgetData;
            public List<string> DataSources;
            public WidgetStyle Style;
        }
        
        [System.Serializable]
        public class UINotification
        {
            public string NotificationId;
            public string Title;
            public string Message;
            public NotificationType Type;
            public NotificationPriority Priority;
            public DateTime CreatedTime;
            public float Duration;
            public bool IsSticky;
            public bool RequiresAction;
            public List<NotificationAction> Actions;
            public Dictionary<string, object> Metadata;
            public VisualElement Element;
        }
        
        [System.Serializable]
        public class NotificationAction
        {
            public string ActionId;
            public string Label;
            public ActionType Type;
            public System.Action<string> Callback;
            public Dictionary<string, object> Parameters;
        }
        
        [System.Serializable]
        public class UIAnimation
        {
            public string AnimationId;
            public AnimationType Type;
            public float Duration;
            public AnimationCurve Curve;
            public Vector2 StartValue;
            public Vector2 EndValue;
            public bool IsPlaying;
            public bool Loop;
            public System.Action OnComplete;
        }
        
        [System.Serializable]
        public class UIThemeData
        {
            public string ThemeName;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public Color BackgroundColor;
            public Color TextColor;
            public Color AccentColor;
            public Color WarningColor;
            public Color ErrorColor;
            public Color SuccessColor;
            public Dictionary<string, StyleSheet> StyleSheets;
        }
        
        [System.Serializable]
        public class ResponsiveBreakpoint
        {
            public string Name;
            public int MinWidth;
            public int MaxWidth;
            public float UIScale;
            public LayoutType Layout;
            public Dictionary<string, Vector2> PanelSizes;
        }
        
        // Enums for Phase 4.4.a
        public enum UITheme
        {
            Light, Dark, HighContrast, Cannabis, Professional, Minimal
        }
        
        public enum PanelType
        {
            Dashboard, Achievement, Competition, Narrative, Equipment, IPM, Analytics, Settings
        }
        
        public enum PanelPriority
        {
            Low, Normal, High, Critical, System
        }
        
        public enum WidgetType
        {
            Status, Chart, List, Progress, Metric, Alert, Timer, Map, Camera
        }
        
        public enum WidgetStyle
        {
            Compact, Normal, Expanded, Minimalist, Detailed
        }
        
        public enum NotificationType
        {
            Info, Warning, Error, Success, Achievement, Event, Competition, System
        }
        
        public enum NotificationPriority
        {
            Low, Normal, High, Urgent, Emergency
        }
        
        public enum ActionType
        {
            Dismiss, Navigate, Execute, External, Custom
        }
        
        public enum AnimationType
        {
            FadeIn, FadeOut, SlideIn, SlideOut, Scale, Bounce, Elastic
        }
        
        public enum LayoutType
        {
            Desktop, Tablet, Mobile, Ultrawide, Portrait
        }
        
        protected override void OnManagerInitialize()
        {
            if (EnableAdvancedUI)
            {
                InitializeUISystem();
            }
            
            if (EnableResponsiveLayout)
            {
                InitializeResponsiveSystem();
            }
            
            if (EnableRealTimeUpdates)
            {
                StartRealTimeUpdates();
            }
            
            SetupSystemIntegration();
        }
        
        protected override void OnManagerUpdate()
        {
            if (EnableAdvancedUI)
            {
                UpdateActivePanels();
                ProcessNotifications();
                UpdateWidgets();
            }
            
            if (EnableResponsiveLayout)
            {
                CheckResponsiveBreakpoints();
            }
            
            ProcessAnimations();
        }
        
        protected override void OnManagerShutdown()
        {
            SaveUILayout();
            CleanupNotifications();
            StopAllAnimations();
        }
        
        private void InitializeUISystem()
        {
            // Phase 4.4.a: Initialize comprehensive UI system
            CreateRootElements();
            LoadUITheme(CurrentTheme);
            SetupEventHandlers();
            CreateDefaultPanels();
            
            Debug.Log("Phase 4.4.a: Advanced UI system initialized");
        }
        
        private void CreateRootElements()
        {
            // Create main UI structure using UI Toolkit
            var uiDocument = FindObjectOfType<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogWarning("No UIDocument found. Advanced UI requires UI Toolkit setup.");
                return;
            }
            
            rootElement = uiDocument.rootVisualElement;
            
            // Create main container structure
            mainContainer = new VisualElement();
            mainContainer.name = "main-container";
            mainContainer.AddToClassList("main-container");
            rootElement.Add(mainContainer);
            
            // Header bar
            headerBar = new VisualElement();
            headerBar.name = "header-bar";
            headerBar.AddToClassList("header-bar");
            mainContainer.Add(headerBar);
            
            // Content area
            contentArea = new VisualElement();
            contentArea.name = "content-area";
            contentArea.AddToClassList("content-area");
            mainContainer.Add(contentArea);
            
            // Side panel
            sidePanel = new VisualElement();
            sidePanel.name = "side-panel";
            sidePanel.AddToClassList("side-panel");
            contentArea.Add(sidePanel);
            
            // Bottom bar
            bottomBar = new VisualElement();
            bottomBar.name = "bottom-bar";
            bottomBar.AddToClassList("bottom-bar");
            mainContainer.Add(bottomBar);
            
            CreateHeaderContent();
            CreateSidePanelContent();
            CreateBottomBarContent();
        }
        
        private void CreateHeaderContent()
        {
            // Logo and title
            var titleContainer = new VisualElement();
            titleContainer.AddToClassList("title-container");
            
            var titleLabel = new Label("Project Chimera");
            titleLabel.AddToClassList("main-title");
            titleContainer.Add(titleLabel);
            
            // Quick stats
            var statsContainer = new VisualElement();
            statsContainer.AddToClassList("stats-container");
            
            CreateQuickStatWidget("achievements", "Achievements", "0", statsContainer);
            CreateQuickStatWidget("competitions", "Competitions", "0", statsContainer);
            CreateQuickStatWidget("legacy-points", "Legacy Points", "0", statsContainer);
            
            // Navigation menu
            var navContainer = new VisualElement();
            navContainer.AddToClassList("nav-container");
            
            CreateNavButton("dashboard", "Dashboard", navContainer);
            CreateNavButton("achievements", "Achievements", navContainer);
            CreateNavButton("competitions", "Competitions", navContainer);
            CreateNavButton("narrative", "Stories", navContainer);
            CreateNavButton("equipment", "Equipment", navContainer);
            CreateNavButton("ipm", "Plant Health", navContainer);
            
            headerBar.Add(titleContainer);
            headerBar.Add(statsContainer);
            headerBar.Add(navContainer);
        }
        
        private void CreateQuickStatWidget(string id, string label, string value, VisualElement parent)
        {
            var widget = new VisualElement();
            widget.name = $"stat-{id}";
            widget.AddToClassList("quick-stat");
            
            var labelElement = new Label(label);
            labelElement.AddToClassList("stat-label");
            
            var valueElement = new Label(value);
            valueElement.AddToClassList("stat-value");
            
            widget.Add(labelElement);
            widget.Add(valueElement);
            parent.Add(widget);
            
            // Register as widget for updates
            activeWidgets[id] = new UIWidget
            {
                WidgetId = id,
                Title = label,
                Type = WidgetType.Metric,
                Element = widget,
                IsActive = true,
                AutoUpdate = true,
                UpdateInterval = 1f,
                LastUpdate = DateTime.Now,
                DataSources = new List<string> { id }
            };
        }
        
        private void CreateNavButton(string id, string label, VisualElement parent)
        {
            var button = new Button(() => NavigateToPanel(id));
            button.text = label;
            button.name = $"nav-{id}";
            button.AddToClassList("nav-button");
            parent.Add(button);
        }
        
        private void CreateSidePanelContent()
        {
            // Recent notifications
            var notificationsSection = new VisualElement();
            notificationsSection.AddToClassList("notifications-section");
            
            var notificationsTitle = new Label("Recent Activity");
            notificationsTitle.AddToClassList("section-title");
            notificationsSection.Add(notificationsTitle);
            
            var notificationsList = new VisualElement();
            notificationsList.name = "notifications-list";
            notificationsList.AddToClassList("notifications-list");
            notificationsSection.Add(notificationsList);
            
            // Quick actions
            var actionsSection = new VisualElement();
            actionsSection.AddToClassList("actions-section");
            
            var actionsTitle = new Label("Quick Actions");
            actionsTitle.AddToClassList("section-title");
            actionsSection.Add(actionsTitle);
            
            CreateQuickActionButton("plant-care", "Plant Care", actionsSection);
            CreateQuickActionButton("competition-enter", "Enter Competition", actionsSection);
            CreateQuickActionButton("breeding", "Start Breeding", actionsSection);
            CreateQuickActionButton("equipment-check", "Equipment Check", actionsSection);
            
            sidePanel.Add(notificationsSection);
            sidePanel.Add(actionsSection);
        }
        
        private void CreateQuickActionButton(string id, string label, VisualElement parent)
        {
            var button = new Button(() => ExecuteQuickAction(id));
            button.text = label;
            button.AddToClassList("quick-action-button");
            parent.Add(button);
        }
        
        private void CreateBottomBarContent()
        {
            // System status indicators
            var statusContainer = new VisualElement();
            statusContainer.AddToClassList("status-container");
            
            CreateStatusIndicator("pest-risk", "Pest Risk", StatusType.Good, statusContainer);
            CreateStatusIndicator("equipment-health", "Equipment", StatusType.Good, statusContainer);
            CreateStatusIndicator("plant-health", "Plants", StatusType.Good, statusContainer);
            CreateStatusIndicator("market-status", "Market", StatusType.Good, statusContainer);
            
            // Progress indicators
            var progressContainer = new VisualElement();
            progressContainer.AddToClassList("progress-container");
            
            CreateProgressIndicator("research", "Research Progress", 0.75f, progressContainer);
            CreateProgressIndicator("achievements", "Achievement Progress", 0.45f, progressContainer);
            
            bottomBar.Add(statusContainer);
            bottomBar.Add(progressContainer);
        }
        
        private void CreateStatusIndicator(string id, string label, StatusType status, VisualElement parent)
        {
            var indicator = new VisualElement();
            indicator.name = $"status-{id}";
            indicator.AddToClassList("status-indicator");
            indicator.AddToClassList($"status-{status.ToString().ToLower()}");
            
            var statusDot = new VisualElement();
            statusDot.AddToClassList("status-dot");
            
            var statusLabel = new Label(label);
            statusLabel.AddToClassList("status-label");
            
            indicator.Add(statusDot);
            indicator.Add(statusLabel);
            parent.Add(indicator);
        }
        
        private void CreateProgressIndicator(string id, string label, float progress, VisualElement parent)
        {
            var container = new VisualElement();
            container.AddToClassList("progress-indicator");
            
            var labelElement = new Label(label);
            labelElement.AddToClassList("progress-label");
            
            var progressBar = new ProgressBar();
            progressBar.value = progress * 100;
            progressBar.AddToClassList("progress-bar");
            
            container.Add(labelElement);
            container.Add(progressBar);
            parent.Add(container);
        }
        
        private void LoadUITheme(UITheme theme)
        {
            // Load theme-specific stylesheets
            var themeData = GetThemeData(theme);
            
            // Apply theme colors and styles
            rootElement.style.backgroundColor = themeData.BackgroundColor;
            
            // Apply theme colors directly to elements (CSS custom properties aren't supported in this way)
            // Instead, we'll apply the theme through direct style assignments and class-based styling
            ApplyThemeToElements(themeData);
        }
        
        private void ApplyThemeToElements(UIThemeData themeData)
        {
            // Apply theme colors to main UI elements
            if (headerBar != null)
            {
                headerBar.style.backgroundColor = themeData.SecondaryColor;
            }
            
            if (sidePanel != null)
            {
                sidePanel.style.backgroundColor = themeData.SecondaryColor;
            }
            
            if (bottomBar != null)
            {
                bottomBar.style.backgroundColor = themeData.SecondaryColor;
            }
            
            // Apply theme to all text elements
            var textElements = rootElement.Query<Label>().ToList();
            foreach (var label in textElements)
            {
                label.style.color = themeData.TextColor;
            }
            
            // Apply theme to buttons
            var buttons = rootElement.Query<Button>().ToList();
            foreach (var button in buttons)
            {
                button.style.backgroundColor = themeData.PrimaryColor;
                button.style.color = themeData.TextColor;
            }
        }
        
        private UIThemeData GetThemeData(UITheme theme)
        {
            return theme switch
            {
                UITheme.Dark => new UIThemeData
                {
                    ThemeName = "Dark",
                    PrimaryColor = new Color(0.1f, 0.1f, 0.1f, 1f),
                    SecondaryColor = new Color(0.2f, 0.2f, 0.2f, 1f),
                    BackgroundColor = new Color(0.05f, 0.05f, 0.05f, 1f),
                    TextColor = new Color(0.9f, 0.9f, 0.9f, 1f),
                    AccentColor = new Color(0.3f, 0.7f, 0.3f, 1f),
                    WarningColor = new Color(1f, 0.7f, 0.2f, 1f),
                    ErrorColor = new Color(0.8f, 0.2f, 0.2f, 1f),
                    SuccessColor = new Color(0.2f, 0.8f, 0.2f, 1f)
                },
                UITheme.Cannabis => new UIThemeData
                {
                    ThemeName = "Cannabis",
                    PrimaryColor = new Color(0.2f, 0.4f, 0.2f, 1f),
                    SecondaryColor = new Color(0.3f, 0.5f, 0.3f, 1f),
                    BackgroundColor = new Color(0.1f, 0.2f, 0.1f, 1f),
                    TextColor = new Color(0.9f, 1f, 0.9f, 1f),
                    AccentColor = new Color(0.5f, 0.8f, 0.3f, 1f),
                    WarningColor = new Color(1f, 0.8f, 0.2f, 1f),
                    ErrorColor = new Color(0.8f, 0.3f, 0.2f, 1f),
                    SuccessColor = new Color(0.3f, 0.9f, 0.3f, 1f)
                },
                _ => GetThemeData(UITheme.Dark) // Default to dark theme
            };
        }
        
        private void SetupEventHandlers()
        {
            // Listen to system events for UI updates
            if (achievementManager != null)
            {
                achievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
            }
            
            if (competitionManager != null)
            {
                // competitionManager.OnCompetitionUpdate += OnCompetitionUpdate;
            }
            
            if (eventManager != null)
            {
                // eventManager.OnEventTriggered += OnNarrativeEvent;
            }
        }
        
        private void CreateDefaultPanels()
        {
            // Create main dashboard panel
            CreatePanel("dashboard", "Dashboard", PanelType.Dashboard, new Vector2(0, 100), new Vector2(800, 600));
            CreatePanel("achievements", "Achievements", PanelType.Achievement, new Vector2(100, 150), new Vector2(600, 500));
            CreatePanel("competitions", "Competitions", PanelType.Competition, new Vector2(150, 200), new Vector2(700, 550));
            CreatePanel("narrative", "Story Events", PanelType.Narrative, new Vector2(200, 250), new Vector2(650, 500));
        }
        
        private void CreatePanel(string id, string title, PanelType type, Vector2 position, Vector2 size)
        {
            var panelElement = new VisualElement();
            panelElement.name = $"panel-{id}";
            panelElement.AddToClassList("ui-panel");
            panelElement.AddToClassList($"panel-{type.ToString().ToLower()}");
            
            // Panel header
            var header = new VisualElement();
            header.AddToClassList("panel-header");
            
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("panel-title");
            
            var closeButton = new Button(() => ClosePanel(id));
            closeButton.text = "×";
            closeButton.AddToClassList("panel-close");
            
            header.Add(titleLabel);
            header.Add(closeButton);
            
            // Panel content
            var content = new VisualElement();
            content.name = $"panel-content-{id}";
            content.AddToClassList("panel-content");
            
            panelElement.Add(header);
            panelElement.Add(content);
            
            // Initially hidden
            panelElement.style.display = DisplayStyle.None;
            contentArea.Add(panelElement);
            
            var panel = new UIPanel
            {
                PanelId = id,
                Title = title,
                Type = type,
                Element = panelElement,
                Position = position,
                Size = size,
                IsVisible = false,
                IsResizable = true,
                IsDraggable = true,
                Priority = PanelPriority.Normal,
                LastUpdate = DateTime.Now,
                PanelData = new Dictionary<string, object>(),
                Widgets = new List<UIWidget>()
            };
            
            activePanels[id] = panel;
            PopulatePanelContent(panel);
        }
        
        private void PopulatePanelContent(UIPanel panel)
        {
            var content = panel.Element.Q($"panel-content-{panel.PanelId}");
            
            switch (panel.Type)
            {
                case PanelType.Achievement:
                    CreateAchievementPanelContent(content, panel);
                    break;
                case PanelType.Competition:
                    CreateCompetitionPanelContent(content, panel);
                    break;
                case PanelType.Narrative:
                    CreateNarrativePanelContent(content, panel);
                    break;
                case PanelType.Dashboard:
                    CreateDashboardPanelContent(content, panel);
                    break;
            }
        }
        
        private void CreateAchievementPanelContent(VisualElement content, UIPanel panel)
        {
            // Achievement progress overview
            var progressSection = new VisualElement();
            progressSection.AddToClassList("achievement-progress");
            
            var progressTitle = new Label("Achievement Progress");
            progressTitle.AddToClassList("section-title");
            progressSection.Add(progressTitle);
            
            // Recent achievements list
            var recentSection = new VisualElement();
            recentSection.AddToClassList("recent-achievements");
            
            var recentTitle = new Label("Recent Achievements");
            recentTitle.AddToClassList("section-title");
            recentSection.Add(recentTitle);
            
            var achievementsList = new ListView();
            achievementsList.AddToClassList("achievements-list");
            recentSection.Add(achievementsList);
            
            content.Add(progressSection);
            content.Add(recentSection);
        }
        
        private void CreateCompetitionPanelContent(VisualElement content, UIPanel panel)
        {
            // Active competitions
            var activeSection = new VisualElement();
            activeSection.AddToClassList("active-competitions");
            
            var activeTitle = new Label("Active Competitions");
            activeTitle.AddToClassList("section-title");
            activeSection.Add(activeTitle);
            
            // Competition history
            var historySection = new VisualElement();
            historySection.AddToClassList("competition-history");
            
            var historyTitle = new Label("Competition History");
            historyTitle.AddToClassList("section-title");
            historySection.Add(historyTitle);
            
            content.Add(activeSection);
            content.Add(historySection);
        }
        
        private void CreateNarrativePanelContent(VisualElement content, UIPanel panel)
        {
            // Active events
            var eventsSection = new VisualElement();
            eventsSection.AddToClassList("narrative-events");
            
            var eventsTitle = new Label("Active Story Events");
            eventsTitle.AddToClassList("section-title");
            eventsSection.Add(eventsTitle);
            
            // NPC interactions
            var npcSection = new VisualElement();
            npcSection.AddToClassList("npc-interactions");
            
            var npcTitle = new Label("Character Interactions");
            npcTitle.AddToClassList("section-title");
            npcSection.Add(npcTitle);
            
            content.Add(eventsSection);
            content.Add(npcSection);
        }
        
        private void CreateDashboardPanelContent(VisualElement content, UIPanel panel)
        {
            // System overview widgets
            var overviewSection = new VisualElement();
            overviewSection.AddToClassList("dashboard-overview");
            
            CreateDashboardWidget("system-status", "System Status", WidgetType.Status, overviewSection);
            CreateDashboardWidget("recent-activity", "Recent Activity", WidgetType.List, overviewSection);
            CreateDashboardWidget("performance", "Performance Metrics", WidgetType.Chart, overviewSection);
            CreateDashboardWidget("alerts", "Active Alerts", WidgetType.Alert, overviewSection);
            
            content.Add(overviewSection);
        }
        
        private void CreateDashboardWidget(string id, string title, WidgetType type, VisualElement parent)
        {
            var widget = new VisualElement();
            widget.name = $"widget-{id}";
            widget.AddToClassList("dashboard-widget");
            widget.AddToClassList($"widget-{type.ToString().ToLower()}");
            
            var header = new VisualElement();
            header.AddToClassList("widget-header");
            
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("widget-title");
            header.Add(titleLabel);
            
            var body = new VisualElement();
            body.AddToClassList("widget-body");
            
            widget.Add(header);
            widget.Add(body);
            parent.Add(widget);
            
            activeWidgets[id] = new UIWidget
            {
                WidgetId = id,
                Title = title,
                Type = type,
                Element = widget,
                IsActive = true,
                AutoUpdate = true,
                UpdateInterval = 5f,
                LastUpdate = DateTime.Now
            };
        }
        
        private void InitializeResponsiveSystem()
        {
            // Setup responsive breakpoints for different screen sizes
            SetupResponsiveBreakpoints();
            CheckResponsiveBreakpoints();
        }
        
        private void SetupResponsiveBreakpoints()
        {
            // Define responsive breakpoints for different screen sizes
            // This would be expanded based on target platforms
        }
        
        private void StartRealTimeUpdates()
        {
            InvokeRepeating(nameof(UpdateRealTimeData), 0f, 1f / UpdateFrequency);
        }
        
        private void UpdateRealTimeData()
        {
            UpdateQuickStats();
            UpdateStatusIndicators();
            UpdateProgressBars();
        }
        
        private void UpdateQuickStats()
        {
            if (achievementManager != null)
            {
                var achievementWidget = activeWidgets.GetValueOrDefault("achievements");
                if (achievementWidget != null)
                {
                    var valueLabel = achievementWidget.Element.Q<Label>("stat-value");
                    if (valueLabel != null)
                    {
                        valueLabel.text = achievementManager.GetUnlockedAchievements().Count.ToString();
                    }
                }
                
                var legacyWidget = activeWidgets.GetValueOrDefault("legacy-points");
                if (legacyWidget != null)
                {
                    var valueLabel = legacyWidget.Element.Q<Label>("stat-value");
                    if (valueLabel != null)
                    {
                        valueLabel.text = achievementManager.GetPlayerLegacy().LegacyPoints.ToString();
                    }
                }
            }
            
            if (competitionManager != null)
            {
                var competitionWidget = activeWidgets.GetValueOrDefault("competitions");
                if (competitionWidget != null)
                {
                    var valueLabel = competitionWidget.Element.Q<Label>("stat-value");
                    if (valueLabel != null)
                    {
                        // valueLabel.text = competitionManager.GetActiveCompetitions().Count.ToString();
                    }
                }
            }
        }
        
        private void UpdateStatusIndicators()
        {
            // Update system status indicators based on manager states
            UpdatePestRiskStatus();
            UpdateEquipmentStatus();
            UpdatePlantHealthStatus();
        }
        
        private void UpdatePestRiskStatus()
        {
            if (pestManager != null)
            {
                var indicator = rootElement.Q("status-pest-risk");
                if (indicator != null)
                {
                    // Update based on current pest risk level
                    indicator.RemoveFromClassList("status-good");
                    indicator.RemoveFromClassList("status-warning");
                    indicator.RemoveFromClassList("status-danger");
                    
                    // This would be based on actual pest manager data
                    indicator.AddToClassList("status-good");
                }
            }
        }
        
        private void UpdateEquipmentStatus()
        {
            if (equipmentManager != null)
            {
                var indicator = rootElement.Q("status-equipment-health");
                if (indicator != null)
                {
                    // Update based on equipment health
                    indicator.RemoveFromClassList("status-good");
                    indicator.RemoveFromClassList("status-warning");
                    indicator.RemoveFromClassList("status-danger");
                    
                    // This would be based on actual equipment manager data
                    indicator.AddToClassList("status-good");
                }
            }
        }
        
        private void UpdatePlantHealthStatus()
        {
            // Update plant health indicator
            var indicator = rootElement.Q("status-plant-health");
            if (indicator != null)
            {
                indicator.RemoveFromClassList("status-good");
                indicator.RemoveFromClassList("status-warning");
                indicator.RemoveFromClassList("status-danger");
                
                // This would be based on actual plant health data
                indicator.AddToClassList("status-good");
            }
        }
        
        private void UpdateProgressBars()
        {
            // Update progress indicators with current data
            var researchProgress = rootElement.Q<ProgressBar>("progress-research");
            if (researchProgress != null)
            {
                // This would be based on actual research progress
                researchProgress.value = 75f;
            }
            
            var achievementProgress = rootElement.Q<ProgressBar>("progress-achievements");
            if (achievementProgress != null && achievementManager != null)
            {
                float progress = achievementManager.GetOverallProgress() * 100f;
                achievementProgress.value = progress;
            }
        }
        
        private void UpdateActivePanels()
        {
            foreach (var panel in activePanels.Values.Where(p => p.IsVisible))
            {
                UpdatePanelContent(panel);
            }
        }
        
        private void UpdatePanelContent(UIPanel panel)
        {
            switch (panel.Type)
            {
                case PanelType.Achievement:
                    UpdateAchievementPanel(panel);
                    break;
                case PanelType.Competition:
                    UpdateCompetitionPanel(panel);
                    break;
                case PanelType.Narrative:
                    UpdateNarrativePanel(panel);
                    break;
                case PanelType.Dashboard:
                    UpdateDashboardPanel(panel);
                    break;
            }
            
            panel.LastUpdate = DateTime.Now;
        }
        
        private void UpdateAchievementPanel(UIPanel panel)
        {
            if (achievementManager == null) return;
            
            // Update achievement list with latest data
            var achievementsList = panel.Element.Q<ListView>("achievements-list");
            if (achievementsList != null)
            {
                var recentAchievements = achievementManager.GetUnlockedAchievements().TakeLast(10);
                // Update list items
            }
        }
        
        private void UpdateCompetitionPanel(UIPanel panel)
        {
            if (competitionManager == null) return;
            
            // Update competition data
            // This would be expanded with actual competition manager integration
        }
        
        private void UpdateNarrativePanel(UIPanel panel)
        {
            if (eventManager == null) return;
            
            // Update narrative events and NPC interactions
            // This would be expanded with actual event manager integration
        }
        
        private void UpdateDashboardPanel(UIPanel panel)
        {
            // Update dashboard widgets with latest system data
            foreach (var widget in panel.Widgets.Where(w => w.AutoUpdate))
            {
                if (DateTime.Now.Subtract(widget.LastUpdate).TotalSeconds >= widget.UpdateInterval)
                {
                    UpdateWidget(widget);
                }
            }
        }
        
        private void UpdateWidget(UIWidget widget)
        {
            switch (widget.Type)
            {
                case WidgetType.Status:
                    UpdateStatusWidget(widget);
                    break;
                case WidgetType.Metric:
                    UpdateMetricWidget(widget);
                    break;
                case WidgetType.List:
                    UpdateListWidget(widget);
                    break;
                case WidgetType.Chart:
                    UpdateChartWidget(widget);
                    break;
            }
            
            widget.LastUpdate = DateTime.Now;
        }
        
        private void UpdateStatusWidget(UIWidget widget)
        {
            // Update status widget with current system status
        }
        
        private void UpdateMetricWidget(UIWidget widget)
        {
            // Update metric widget with current values
        }
        
        private void UpdateListWidget(UIWidget widget)
        {
            // Update list widget with current data
        }
        
        private void UpdateChartWidget(UIWidget widget)
        {
            // Update chart widget with current data
        }
        
        private void UpdateWidgets()
        {
            foreach (var widget in activeWidgets.Values.Where(w => w.AutoUpdate))
            {
                if (DateTime.Now.Subtract(widget.LastUpdate).TotalSeconds >= widget.UpdateInterval)
                {
                    UpdateWidget(widget);
                }
            }
        }
        
        private void ProcessNotifications()
        {
            // Process pending notifications
            while (pendingNotifications.Count > 0 && activeNotifications.Count < 5)
            {
                var notification = pendingNotifications.Dequeue();
                ShowNotification(notification);
            }
            
            // Remove expired notifications
            var expiredNotifications = activeNotifications
                .Where(n => !n.IsSticky && DateTime.Now.Subtract(n.CreatedTime).TotalSeconds > n.Duration)
                .ToList();
            
            foreach (var notification in expiredNotifications)
            {
                RemoveNotification(notification);
            }
        }
        
        private void ShowNotification(UINotification notification)
        {
            var notificationElement = CreateNotificationElement(notification);
            notification.Element = notificationElement;
            
            var notificationsList = rootElement.Q("notifications-list");
            notificationsList?.Add(notificationElement);
            
            activeNotifications.Add(notification);
            
            // Animate in
            AnimateNotificationIn(notificationElement);
        }
        
        private VisualElement CreateNotificationElement(UINotification notification)
        {
            var element = new VisualElement();
            element.AddToClassList("notification");
            element.AddToClassList($"notification-{notification.Type.ToString().ToLower()}");
            
            var header = new VisualElement();
            header.AddToClassList("notification-header");
            
            var title = new Label(notification.Title);
            title.AddToClassList("notification-title");
            
            var closeButton = new Button(() => RemoveNotification(notification));
            closeButton.text = "×";
            closeButton.AddToClassList("notification-close");
            
            header.Add(title);
            header.Add(closeButton);
            
            var message = new Label(notification.Message);
            message.AddToClassList("notification-message");
            
            element.Add(header);
            element.Add(message);
            
            // Add action buttons if any
            if (notification.Actions.Count > 0)
            {
                var actionsContainer = new VisualElement();
                actionsContainer.AddToClassList("notification-actions");
                
                foreach (var action in notification.Actions)
                {
                    var actionButton = new Button(() => ExecuteNotificationAction(action));
                    actionButton.text = action.Label;
                    actionButton.AddToClassList("notification-action");
                    actionsContainer.Add(actionButton);
                }
                
                element.Add(actionsContainer);
            }
            
            return element;
        }
        
        private void RemoveNotification(UINotification notification)
        {
            if (notification.Element != null)
            {
                AnimateNotificationOut(notification.Element, () =>
                {
                    notification.Element.RemoveFromHierarchy();
                    activeNotifications.Remove(notification);
                });
            }
        }
        
        private void AnimateNotificationIn(VisualElement element)
        {
            // Simple fade in animation
            element.style.opacity = 0f;
            StartCoroutine(FadeIn(element, AnimationSpeed));
        }
        
        private void AnimateNotificationOut(VisualElement element, System.Action onComplete)
        {
            // Simple fade out animation
            StartCoroutine(FadeOut(element, AnimationSpeed, onComplete));
        }
        
        private System.Collections.IEnumerator FadeIn(VisualElement element, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                element.style.opacity = alpha;
                elapsed += Time.deltaTime;
                yield return null;
            }
            element.style.opacity = 1f;
        }
        
        private System.Collections.IEnumerator FadeOut(VisualElement element, float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                element.style.opacity = alpha;
                elapsed += Time.deltaTime;
                yield return null;
            }
            element.style.opacity = 0f;
            onComplete?.Invoke();
        }
        
        private void ProcessAnimations()
        {
            // Process active UI animations
            // This would be expanded with a comprehensive animation system
        }
        
        private void CheckResponsiveBreakpoints()
        {
            // Check current screen size and apply appropriate layout
            var screenWidth = Screen.width;
            var screenHeight = Screen.height;
            
            // Adjust UI layout based on screen size
            if (screenWidth < 1024)
            {
                ApplyMobileLayout();
            }
            else if (screenWidth < 1440)
            {
                ApplyTabletLayout();
            }
            else
            {
                ApplyDesktopLayout();
            }
        }
        
        private void ApplyMobileLayout()
        {
            // Adjust UI for mobile screens
            if (sidePanel != null)
            {
                sidePanel.style.display = DisplayStyle.None;
            }
        }
        
        private void ApplyTabletLayout()
        {
            // Adjust UI for tablet screens
            if (sidePanel != null)
            {
                sidePanel.style.display = DisplayStyle.Flex;
                sidePanel.style.width = 200;
            }
        }
        
        private void ApplyDesktopLayout()
        {
            // Adjust UI for desktop screens
            if (sidePanel != null)
            {
                sidePanel.style.display = DisplayStyle.Flex;
                sidePanel.style.width = 300;
            }
        }
        
        private void SetupSystemIntegration()
        {
            // Setup integration with all Phase 4 systems
            FindSystemManagers();
            SubscribeToSystemEvents();
        }
        
        private void FindSystemManagers()
        {
            achievementManager = FindObjectOfType<LegacyAchievementManager>();
            competitionManager = FindObjectOfType<CannabisCupManager>();
            eventManager = FindObjectOfType<DynamicEventManager>();
            npcManager = FindObjectOfType<NPCInteractionManager>();
            pestManager = FindObjectOfType<CleanIPMManager>();
            equipmentManager = FindObjectOfType<EquipmentDegradationManager>();
        }
        
        private void SubscribeToSystemEvents()
        {
            // Subscribe to events from all integrated systems
            if (achievementManager != null)
            {
                achievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
                achievementManager.OnMilestoneCompleted += OnMilestoneCompleted;
            }
        }
        
        // Event handlers
        private void OnAchievementUnlocked(ProjectChimera.Data.Achievements.BaseAchievement achievement)
        {
            ShowAchievementNotification(achievement);
            UpdateAchievementDisplay();
        }
        
        private void OnMilestoneCompleted(LegacyAchievementManager.LegacyMilestone milestone)
        {
            ShowMilestoneNotification(milestone);
        }
        
        private void ShowAchievementNotification(ProjectChimera.Data.Achievements.BaseAchievement achievement)
        {
            var notification = new UINotification
            {
                NotificationId = Guid.NewGuid().ToString(),
                Title = "Achievement Unlocked!",
                Message = $"{achievement.Name}: {achievement.Description}",
                Type = NotificationType.Achievement,
                Priority = NotificationPriority.High,
                CreatedTime = DateTime.Now,
                Duration = 5f,
                IsSticky = false,
                Actions = new List<NotificationAction>
                {
                    new NotificationAction
                    {
                        ActionId = "view_achievement",
                        Label = "View",
                        Type = ActionType.Navigate,
                        Callback = (id) => NavigateToPanel("achievements")
                    }
                }
            };
            
            pendingNotifications.Enqueue(notification);
        }
        
        private void ShowMilestoneNotification(LegacyAchievementManager.LegacyMilestone milestone)
        {
            var notification = new UINotification
            {
                NotificationId = Guid.NewGuid().ToString(),
                Title = "Milestone Reached!",
                Message = $"{milestone.Title}: {milestone.Description}",
                Type = NotificationType.Success,
                Priority = NotificationPriority.High,
                CreatedTime = DateTime.Now,
                Duration = 8f,
                IsSticky = false
            };
            
            pendingNotifications.Enqueue(notification);
        }
        
        private void UpdateAchievementDisplay()
        {
            // Update achievement displays throughout the UI
            UpdateQuickStats();
        }
        
        // Navigation methods
        private void NavigateToPanel(string panelId)
        {
            if (activePanels.TryGetValue(panelId, out var panel))
            {
                ShowPanel(panel);
            }
        }
        
        private void ShowPanel(UIPanel panel)
        {
            panel.Element.style.display = DisplayStyle.Flex;
            panel.IsVisible = true;
            
            // Animate panel in
            AnimatePanelIn(panel.Element);
            
            // Update panel content
            UpdatePanelContent(panel);
        }
        
        private void ClosePanel(string panelId)
        {
            if (activePanels.TryGetValue(panelId, out var panel))
            {
                HidePanel(panel);
            }
        }
        
        private void HidePanel(UIPanel panel)
        {
            AnimatePanelOut(panel.Element, () =>
            {
                panel.Element.style.display = DisplayStyle.None;
                panel.IsVisible = false;
            });
        }
        
        private void AnimatePanelIn(VisualElement element)
        {
            // Simple scale and fade in animation
            element.style.scale = new Scale(Vector3.zero);
            element.style.opacity = 0f;
            
            StartCoroutine(ScaleAndFadeIn(element, AnimationSpeed));
        }
        
        private void AnimatePanelOut(VisualElement element, System.Action onComplete)
        {
            StartCoroutine(ScaleAndFadeOut(element, AnimationSpeed, onComplete));
        }
        
        private System.Collections.IEnumerator ScaleAndFadeIn(VisualElement element, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                float scale = Mathf.Lerp(0f, 1f, progress);
                float alpha = Mathf.Lerp(0f, 1f, progress);
                
                element.style.scale = new Scale(Vector3.one * scale);
                element.style.opacity = alpha;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            element.style.scale = new Scale(Vector3.one);
            element.style.opacity = 1f;
        }
        
        private System.Collections.IEnumerator ScaleAndFadeOut(VisualElement element, float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                float scale = Mathf.Lerp(1f, 0f, progress);
                float alpha = Mathf.Lerp(1f, 0f, progress);
                
                element.style.scale = new Scale(Vector3.one * scale);
                element.style.opacity = alpha;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            element.style.scale = new Scale(Vector3.zero);
            element.style.opacity = 0f;
            onComplete?.Invoke();
        }
        
        // Quick action handlers
        private void ExecuteQuickAction(string actionId)
        {
            switch (actionId)
            {
                case "plant-care":
                    // Navigate to plant care interface
                    break;
                case "competition-enter":
                    NavigateToPanel("competitions");
                    break;
                case "breeding":
                    // Navigate to breeding interface
                    break;
                case "equipment-check":
                    // Navigate to equipment interface
                    break;
            }
        }
        
        private void ExecuteNotificationAction(NotificationAction action)
        {
            action.Callback?.Invoke(action.ActionId);
        }
        
        private void SaveUILayout()
        {
            // Save current UI layout and preferences
            var layoutData = new Dictionary<string, object>();
            
            foreach (var panel in activePanels.Values)
            {
                layoutData[$"panel_{panel.PanelId}"] = new
                {
                    Position = panel.Position,
                    Size = panel.Size,
                    IsVisible = panel.IsVisible
                };
            }
            
            // Save to PlayerPrefs or save system
            var json = JsonUtility.ToJson(layoutData);
            PlayerPrefs.SetString("UI_Layout", json);
        }
        
        private void CleanupNotifications()
        {
            activeNotifications.Clear();
            pendingNotifications.Clear();
        }
        
        private void StopAllAnimations()
        {
            StopAllCoroutines();
        }
        
        // Public API for other systems
        public void ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 3f)
        {
            var notification = new UINotification
            {
                NotificationId = Guid.NewGuid().ToString(),
                Title = title,
                Message = message,
                Type = type,
                Priority = NotificationPriority.Normal,
                CreatedTime = DateTime.Now,
                Duration = duration,
                IsSticky = false
            };
            
            pendingNotifications.Enqueue(notification);
        }
        
        public void ShowPanel(string panelId)
        {
            NavigateToPanel(panelId);
        }
        
        public void SetTheme(UITheme theme)
        {
            CurrentTheme = theme;
            LoadUITheme(theme);
        }
        
        public void UpdateSystemStatus(string systemId, StatusType status)
        {
            var indicator = rootElement.Q($"status-{systemId}");
            if (indicator != null)
            {
                indicator.RemoveFromClassList("status-good");
                indicator.RemoveFromClassList("status-warning");
                indicator.RemoveFromClassList("status-danger");
                indicator.AddToClassList($"status-{status.ToString().ToLower()}");
            }
        }
        
        // Helper enums
        public enum StatusType
        {
            Good, Warning, Danger
        }
    }
}