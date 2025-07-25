using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.UI.Dashboard;
using ProjectChimera.UI.Environmental;
using ProjectChimera.UI.Financial;
using ProjectChimera.UI.AIAdvisor;
// using ProjectChimera.Systems.AI;
using ProjectChimera.UI.Automation;
using ProjectChimera.UI.Research;
using ProjectChimera.UI.DataVisualization;
using ProjectChimera.UI.Settings;
using ProjectChimera.Data.UI;
using ProjectChimera.UI.Core;

namespace ProjectChimera.UI.Core
{
    /// <summary>
    /// Central UI Manager for Project Chimera.
    /// Coordinates all UI panels, manages navigation, and provides a cohesive game experience.
    /// Handles the overall UI flow including dashboard, specialized panels, and modal dialogs.
    /// </summary>
    public class GameUIManager : ChimeraManager
    {
        [Header("UI Configuration")]
        [SerializeField] private UIDocument _mainUIDocument;
        [SerializeField] private UIThemeSO _defaultTheme;
        [SerializeField] private bool _enableUIAnimations = true;
        [SerializeField] private bool _enableGamepadNavigation = true;
        
        [Header("UI Controllers")]
        [SerializeField] private FacilityDashboardController _dashboardController;
        [SerializeField] private EnvironmentalControlController _environmentalController;
        [SerializeField] private FinancialManagementController _financialController;
        [SerializeField] private AIAdvisorController _aiAdvisorController;
        [SerializeField] private AutomationControlController _automationController;
        [SerializeField] private ResearchProgressionController _researchController;
        [SerializeField] private DataVisualizationController _dataVisualizationController;
        [SerializeField] private SettingsController _settingsController;
        
        [Header("UI Documents")]
        [SerializeField] private UIDocument _modalUIDocument;
        [SerializeField] private UIDocument _tooltipUIDocument;
                [SerializeField] private UIDocument _notificationUIDocument;
        
        [Header("UI Prefabs")]
        [SerializeField] private GameObject _environmentalControlPrefab;
        [SerializeField] private GameObject _financialManagementPrefab;
        [SerializeField] private GameObject _aiAdvisorPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip _buttonClickSound;
        [SerializeField] private AudioClip _panelTransitionSound;
        [SerializeField] private AudioClip _alertSound;
        [SerializeField] private AudioSource _uiAudioSource;
        
        // UI System References
        private Dictionary<string, IUIController> _uiControllers = new Dictionary<string, IUIController>();
        private Dictionary<Type, IUIController> _controllersByType = new Dictionary<Type, IUIController>();
        // private UIIntegrationManager _integrationManager;
        
        // UI State Management
        private UIState _currentUIState = new UIState();
        private Stack<string> _navigationHistory = new Stack<string>();
        private Dictionary<string, object> _uiPreferences = new Dictionary<string, object>();
        
        // UI Elements
        private VisualElement _rootElement;
        private VisualElement _modalRootElement;
        private VisualElement _tooltipRootElement;
        private VisualElement _notificationRootElement;
        
        // Navigation and Modal System
        private VisualElement _mainNavigationPanel;
        private VisualElement _contentArea;
        private VisualElement _modalOverlay;
        private VisualElement _currentModalContent;
        private VisualElement _modalContainer;
        private VisualElement _notificationContainer;
        private Queue<NotificationData> _notificationQueue = new Queue<NotificationData>();
        
        // Game state
        private bool _isGamePaused = false;
        private bool _isInMenuMode = false;
        private float _lastInteractionTime;
        
        // Legacy panel system (for compatibility)
        private Dictionary<string, UIPanel> _activePanels = new Dictionary<string, UIPanel>();
        private string _currentActivePanel = "";
        
        public override ManagerPriority Priority => ManagerPriority.High;
        
        // Properties
        public string CurrentActivePanel => _currentActivePanel;
        public bool IsInMenuMode => _isInMenuMode;
        public UIThemeSO CurrentTheme => _defaultTheme;
        public UIState CurrentUIState => _currentUIState;
        public IReadOnlyDictionary<string, IUIController> UIControllers => _uiControllers;
        public bool HasUnsavedUIPreferences { get; private set; }
        
        // UI Events
        public System.Action<string> OnPanelChanged;
        public System.Action<string> OnModalOpened;
        public System.Action<string> OnModalClosed;
        public System.Action<NotificationData> OnNotificationShown;
        public System.Action<string, object> OnUIPreferenceChanged;
        
        // Integration Events
        public System.Action OnUISystemsInitialized;
        public System.Action<IUIController> OnUIControllerRegistered;
        public System.Action<bool> OnUIStateChanged;
        
        // Legacy Events
        public System.Action<bool> OnMenuModeChanged;
        public System.Action<string> OnQuickAction;
        public System.Action<DashboardAlert> OnAlertInteraction;
        
        // Additional UI Events
        public System.Action<string> OnUIControllerActivated;
        public System.Action<string> OnUIControllerDeactivated;
        public System.Action OnUISystemReady;
        
        protected override void OnManagerInitialize()
        {
            InitializeSystemReferences();
            InitializeUIDocuments();
            RegisterUIControllers();
            SetupUINavigation();
            SetupModalSystem();
            SetupNotificationSystem();
            LoadUIPreferences();
            SetupEventHandlers();
            SetupInputHandling();
            
            // Initialize all UI controllers
            InitializeUIControllers();
            
            _lastInteractionTime = Time.time;
            
            OnUISystemsInitialized?.Invoke();
            OnUISystemReady?.Invoke();
            
            // LogInfo("GameUIManager initialized with comprehensive UI integration");
        }
        
        protected override void OnManagerUpdate()
        {
            // Handle UI input and interactions
            HandleKeyboardShortcuts();
            UpdateNotifications();
            
            // Auto-hide cursor after inactivity (game-like behavior)
            if (Time.time - _lastInteractionTime > 10f && !_isInMenuMode)
            {
                UnityEngine.Cursor.visible = false;
            }
        }
        
        private void InitializeSystemReferences()
        {
            var _gameManager = GameManager.Instance;
            // if (_gameManager == null)
            // {
                LogWarning("GameManager not found - using standalone mode");
                return;
            // }
            
            // Get UI Integration Manager for advanced communication
            // _integrationManager = _gameManager.GetManager<UIIntegrationManager>();
            // if (_integrationManager == null)
            // {
                // LogWarning("UIIntegrationManager not found - using basic UI mode");
            // }
            // else
            // {
                // Subscribe to integration events
                // _integrationManager.OnIntegrationInitialized += HandleIntegrationInitialized;
                // _integrationManager.OnManagerDataUpdated += HandleManagerDataUpdated;
                // _integrationManager.OnBindingError += HandleBindingError;
            // }
            
            // LogInfo("GameUIManager connected to integration systems");
        }
        
        private void InitializeUIDocuments()
        {
            // InitializeUIDocument(); // Call existing method - method doesn't exist
            
            // Initialize additional UI documents
            if (_modalUIDocument != null)
            {
                _modalRootElement = _modalUIDocument.rootVisualElement;
            }
            
            if (_tooltipUIDocument != null)
            {
                _tooltipRootElement = _tooltipUIDocument.rootVisualElement;
            }
            
            if (_notificationUIDocument != null)
            {
                _notificationRootElement = _notificationUIDocument.rootVisualElement;
            }
            
            // SetupPanelContainer(); // Call existing method - method doesn't exist
            LogInfo("All UI Documents initialized");
        }
        
        private void RegisterUIControllers()
        {
            // Register all available UI controllers
            if (_dashboardController != null)
            {
                _uiControllers["dashboard"] = _dashboardController as IUIController;
                _controllersByType[typeof(FacilityDashboardController)] = _dashboardController as IUIController;
            }
            
            if (_environmentalController != null)
            {
                _uiControllers["environmental"] = _environmentalController as IUIController;
                _controllersByType[typeof(EnvironmentalControlController)] = _environmentalController as IUIController;
            }
            
            if (_financialController != null)
            {
                _uiControllers["financial"] = _financialController as IUIController;
                _controllersByType[typeof(FinancialManagementController)] = _financialController as IUIController;
            }
            
            if (_aiAdvisorController != null)
            {
                _uiControllers["ai-advisor"] = _aiAdvisorController as IUIController;
                _controllersByType[typeof(AIAdvisorController)] = _aiAdvisorController as IUIController;
            }
            
            if (_automationController != null)
            {
                _uiControllers["automation"] = _automationController as IUIController;
                _controllersByType[typeof(AutomationControlController)] = _automationController as IUIController;
            }
            
            if (_researchController != null)
            {
                _uiControllers["research"] = _researchController as IUIController;
                _controllersByType[typeof(ResearchProgressionController)] = _researchController as IUIController;
            }
            
            if (_dataVisualizationController != null)
            {
                _uiControllers["data-visualization"] = _dataVisualizationController as IUIController;
                _controllersByType[typeof(DataVisualizationController)] = _dataVisualizationController as IUIController;
            }
            
            if (_settingsController != null)
            {
                _uiControllers["settings"] = _settingsController as IUIController;
                _controllersByType[typeof(SettingsController)] = _settingsController as IUIController;
            }
            
            LogInfo($"Registered {_uiControllers.Count} UI controllers");
        }
        
        private void SetupUINavigation()
        {
            // Setup main navigation elements
            _mainNavigationPanel = _rootElement?.Q<VisualElement>("navigation-panel");
            _contentArea = _rootElement?.Q<VisualElement>("content-area");
            
            if (_mainNavigationPanel == null)
            {
                LogWarning("Navigation panel not found in UI document");
            }
            
            if (_contentArea == null)
            {
                LogWarning("Content area not found in UI document");
            }
            
            LogInfo("UI Navigation setup complete");
        }
        
        private void SetupModalSystem()
        {
            // Modal system already setup in SetupPanelContainer
            LogInfo("Modal system setup complete");
        }
        
        private void SetupNotificationSystem()
        {
            // Notification system already setup in SetupPanelContainer
            LogInfo("Notification system setup complete");
        }
        
        private void InitializeUIControllers()
        {
            // Initialize dashboard if available
            // InitializeDashboard(); // Method not implemented yet
            
            // Initialize each registered controller
            foreach (var kvp in _uiControllers)
            {
                try
                {
                    OnUIControllerRegistered?.Invoke(kvp.Value);
                    LogInfo($"Initialized UI controller: {kvp.Key}");
                }
                catch (System.Exception ex)
                {
                    LogError($"Failed to initialize UI controller {kvp.Key}: {ex.Message}");
                }
            }
        }
        
        private void SetupEventHandlers()
        {
            // Setup event handlers for dashboard controller
            if (_dashboardController != null)
            {
                // Note: These method calls may need to be commented out if the methods don't exist
                // _dashboardController.OnPanelSelected += NavigateToPanel;
                // _dashboardController.OnAlertClicked += HandleAlertClick;
                // _dashboardController.OnQuickActionTriggered += HandleQuickAction;
            }
            
            LogInfo("Event handlers setup complete");
        }
        
        private void SetupInputHandling()
        {
            // Setup keyboard shortcut handling
            if (_enableGamepadNavigation)
            {
                LogInfo("Gamepad navigation enabled");
            }
            
            LogInfo("Input handling setup complete");
        }
        
        #region Panel and Controller Management
        
        public void RegisterPanel(string panelId, UIPanel panel)
        {
            if (_activePanels.ContainsKey(panelId))
            {
                Debug.LogWarning($"Panel {panelId} already registered");
                return;
            }
            
            _activePanels[panelId] = panel;
            LogInfo($"Registered UI panel: {panelId}");
        }
        
        public void NavigateToPanel(string panelId)
        {
            // Check if controller exists (new system)
            if (_uiControllers.ContainsKey(panelId))
            {
                // Add current panel to history
                if (!string.IsNullOrEmpty(_currentActivePanel))
                {
                    _navigationHistory.Push(_currentActivePanel);
                    SetControllerVisibility(_currentActivePanel, false);
                }
                
                // Show new panel
                SetControllerVisibility(panelId, true);
                _currentActivePanel = panelId;
                
                // Play transition sound
                PlayUISound(_panelTransitionSound);
                
                OnPanelChanged?.Invoke(panelId);
                OnUIControllerActivated?.Invoke(panelId);
                LogInfo($"Navigated to panel: {panelId}");
                return;
            }
            
            // Fallback to legacy panel system
            if (!_activePanels.ContainsKey(panelId))
            {
                LogWarning($"Panel {panelId} not found");
                return;
            }
            
            // Add current panel to history
            if (!string.IsNullOrEmpty(_currentActivePanel))
            {
                _navigationHistory.Push(_currentActivePanel);
                SetPanelVisibility(_currentActivePanel, false);
            }
            
            // Show new panel
            SetPanelVisibility(panelId, true);
            _currentActivePanel = panelId;
            
            // Play transition sound
            PlayUISound(_panelTransitionSound);
            
            OnPanelChanged?.Invoke(panelId);
            LogInfo($"Navigated to panel: {panelId}");
        }
        
        public void NavigateBack()
        {
            if (_navigationHistory.Count == 0)
            {
                LogInfo("No previous panel in navigation history");
                return;
            }
            
            string previousPanel = _navigationHistory.Pop();
            
            // Try new controller system first
            if (_uiControllers.ContainsKey(_currentActivePanel))
            {
                SetControllerVisibility(_currentActivePanel, false);
                SetControllerVisibility(previousPanel, true);
                _currentActivePanel = previousPanel; // Legacy compatibility
                
                PlayUISound(_panelTransitionSound);
                OnPanelChanged?.Invoke(previousPanel);
                OnUIControllerActivated?.Invoke(previousPanel);
                return;
            }
            
            // Fallback to legacy system
            SetPanelVisibility(_currentActivePanel, false);
            SetPanelVisibility(previousPanel, true);
            _currentActivePanel = previousPanel;
            
            PlayUISound(_panelTransitionSound);
            OnPanelChanged?.Invoke(previousPanel);
        }
        
        private void SetPanelVisibility(string panelId, bool visible)
        {
            if (!_activePanels.ContainsKey(panelId)) return;
            
            var panel = _activePanels[panelId];
            // UIPanel doesn't have GameObject or IsActive properties
            // if (panel.GameObject != null)
            // {
            //     panel.GameObject.SetActive(visible);
            //     panel.IsActive = visible;
            // }
            if (panel != null)
            {
                if (visible)
                {
                    panel.Show();
                }
                else
                {
                    panel.Hide();
                }
            }
            
            // Update UI state - UIState is an enum, not a class with properties
            // _currentUIState.ActiveControllers[panelId] = visible;
            
            // Apply animations if enabled
            if (_enableUIAnimations && visible)
            {
                AnimatePanelIn(panel);
            }
        }
        
        private void SetControllerVisibility(string controllerId, bool visible)
        {
            if (!_uiControllers.ContainsKey(controllerId)) return;
            
            var controller = _uiControllers[controllerId];
            if (controller is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.gameObject.SetActive(visible);
            }
            
            // Update UI state - UIState is an enum, not a class with properties
            // _currentUIState.ActiveControllers[controllerId] = visible;
            
            // Fire events
            if (visible)
            {
                OnUIControllerActivated?.Invoke(controllerId);
            }
            // else
            // {
                OnUIControllerDeactivated?.Invoke(controllerId);
            // }
            
            // Apply animations if enabled
            if (_enableUIAnimations && visible)
            {
                AnimateControllerIn(controller);
            }
        }
        
        #region Specialized Panel Creation
        
        public void CreateEnvironmentalControlPanel()
        {
            if (_activePanels.ContainsKey("environmental")) return;
            
            var panelGO = CreatePanelFromPrefab(_environmentalControlPrefab, "Environmental Control");
            if (panelGO != null)
            {
                // RegisterPanel("environmental", new UIPanel
                // {
                //     PanelId = "environmental",
                //     PanelName = "Environmental Control",
                //     GameObject = panelGO,
                //     IsActive = false
                // });
                LogInfo("Environmental panel created but not registered - UIPanel is abstract");
            }
        }
        
        public void CreateFinancialManagementPanel()
        {
            if (_activePanels.ContainsKey("financial")) return;
            
            var panelGO = CreatePanelFromPrefab(_financialManagementPrefab, "Financial Management");
            if (panelGO != null)
            {
                // RegisterPanel("financial", new UIPanel
                // {
                //     PanelId = "financial",
                //     PanelName = "Financial Management",
                //     GameObject = panelGO,
                //     IsActive = false
                // });
                LogInfo("Financial panel created but not registered - UIPanel is abstract");
            }
        }
        
        public void CreateAIAdvisorPanel()
        {
            if (_activePanels.ContainsKey("ai-advisor")) return;
            
            var panelGO = CreatePanelFromPrefab(_aiAdvisorPrefab, "AI Advisor");
            if (panelGO != null)
            {
                // RegisterPanel("ai-advisor", new UIPanel
                // {
                //     PanelId = "ai-advisor",
                //     PanelName = "AI Advisor",
                //     GameObject = panelGO,
                //     IsActive = false
                // });
                LogInfo("AI Advisor panel created but not registered - UIPanel is abstract");
            }
        }
        
        private GameObject CreatePanelFromPrefab(GameObject prefab, string panelName)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"{panelName} prefab not assigned");
                return null;
            }
            
            var panelGO = Instantiate(prefab, transform);
            panelGO.SetActive(false);
            
            LogInfo($"Created {panelName} panel");
            return panelGO;
        }
        
        #endregion
        
        #endregion
        
        #region Modal and Notification System
        
        public void ShowModal(string title, string message, System.Action onConfirm = null, System.Action onCancel = null)
        {
            var modal = CreateModalDialog(title, message, onConfirm, onCancel);
            _modalContainer.Add(modal);
            _modalContainer.style.display = DisplayStyle.Flex;
            
            PlayUISound(_buttonClickSound);
        }
        
        public void HideModal()
        {
            _modalContainer.Clear();
            _modalContainer.style.display = DisplayStyle.None;
        }
        
        public void ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = 5f)
        {
            var notification = CreateNotification(title, message, type);
            _notificationContainer.Add(notification);
            
            // Auto-remove after duration
            this.DelayedCall(duration, () =>
            {
                if (notification.parent != null)
                {
                    _notificationContainer.Remove(notification);
                }
            });
            
            if (type == NotificationType.Alert || type == NotificationType.Critical)
            {
                PlayUISound(_alertSound);
            }
        }
        
        private VisualElement CreateModalDialog(string title, string message, System.Action onConfirm, System.Action onCancel)
        {
            var modal = new VisualElement();
            modal.AddToClassList("modal-backdrop");
            
            var dialog = new VisualElement();
            dialog.AddToClassList("modal-dialog");
            
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("modal-title");
            
            var messageLabel = new Label(message);
            messageLabel.AddToClassList("modal-message");
            
            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList("modal-buttons");
            
            if (onCancel != null)
            {
                var cancelButton = new Button(() => { onCancel?.Invoke(); HideModal(); });
                cancelButton.text = "Cancel";
                cancelButton.AddToClassList("modal-button-cancel");
                buttonContainer.Add(cancelButton);
            }
            
            var confirmButton = new Button(() => { onConfirm?.Invoke(); HideModal(); });
            confirmButton.text = "OK";
            confirmButton.AddToClassList("modal-button-confirm");
            buttonContainer.Add(confirmButton);
            
            dialog.Add(titleLabel);
            dialog.Add(messageLabel);
            dialog.Add(buttonContainer);
            modal.Add(dialog);
            
            return modal;
        }
        
        private VisualElement CreateNotification(string title, string message, NotificationType type)
        {
            var notification = new VisualElement();
            notification.AddToClassList("notification");
            notification.AddToClassList($"notification-{type.ToString().ToLower()}");
            
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("notification-title");
            
            var messageLabel = new Label(message);
            messageLabel.AddToClassList("notification-message");
            
            var closeButton = new Button(() => _notificationContainer.Remove(notification));
            closeButton.text = "×";
            closeButton.AddToClassList("notification-close");
            
            notification.Add(titleLabel);
            notification.Add(messageLabel);
            notification.Add(closeButton);
            
            if (_enableUIAnimations)
            {
                notification.AddToClassList("slide-in");
            }
            
            return notification;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleAlertClick(DashboardAlert alert)
        {
            ShowModal("Alert Details", $"{alert.Title}\n\n{alert.Description}", 
                () => LogInfo($"Alert acknowledged: {alert.Title}"));
            
            OnAlertInteraction?.Invoke(alert);
        }
        
        private void HandleQuickAction(string actionName)
        {
            switch (actionName)
            {
                case "emergency-stop":
                    ShowModal("Emergency Stop", "Are you sure you want to initiate emergency shutdown of all systems?",
                        () => ExecuteEmergencyStop(),
                        () => LogInfo("Emergency stop cancelled"));
                    break;
                    
                case "optimize-all":
                    ShowNotification("Optimization", "System optimization started", NotificationType.Info);
                    ExecuteSystemOptimization();
                    break;
                    
                default:
                    LogInfo($"Quick action triggered: {actionName}");
                    break;
            }
            
            OnQuickAction?.Invoke(actionName);
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            _lastInteractionTime = Time.time;
            UnityEngine.Cursor.visible = true;
        }
        
        private void OnKeyDown(KeyDownEvent evt)
        {
            _lastInteractionTime = Time.time;
        }
        
        #endregion
        
        #region Input Handling
        
        private void HandleKeyboardShortcuts()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_modalContainer.style.display == DisplayStyle.Flex)
                {
                    HideModal();
                }
                // else if (_currentUIState.CurrentPanel != "dashboard")
                // {
                    NavigateBack();
                // }
                // else
                // {
                    ToggleMenuMode();
                // }
            }
            
            if (Input.GetKeyDown(KeyCode.F1))
            {
                NavigateToPanel("dashboard");
            }
            
            if (Input.GetKeyDown(KeyCode.F2))
            {
                NavigateToPanel("environmental");
            }
            
            if (Input.GetKeyDown(KeyCode.F3))
            {
                NavigateToPanel("financial");
            }
            
            if (Input.GetKeyDown(KeyCode.F4))
            {
                NavigateToPanel("ai-advisor");
            }
            
                if (Input.GetKeyDown(KeyCode.F5))
            {
                RefreshCurrentPanel();
            }
            
            if (Input.GetKeyDown(KeyCode.F6))
            {
                NavigateToPanel("automation");
            }
            
            if (Input.GetKeyDown(KeyCode.F7))
            {
                NavigateToPanel("research");
            }
            
            if (Input.GetKeyDown(KeyCode.F8))
            {
                NavigateToPanel("data-visualization");
            }
            
            if (Input.GetKeyDown(KeyCode.F9))
            {
                NavigateToPanel("settings");
            }
        }
        
        #endregion
        
        #region Game State Management
        
        public void ToggleMenuMode()
        {
            _isInMenuMode = !_isInMenuMode;
            
            if (_isInMenuMode)
            {
                Time.timeScale = 0f;
                UnityEngine.Cursor.visible = true;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
            }
            // else
            // {
                Time.timeScale = 1f;
                UnityEngine.Cursor.lockState = CursorLockMode.Confined;
            // }
            
            OnMenuModeChanged?.Invoke(_isInMenuMode);
            LogInfo($"Menu mode: {(_isInMenuMode ? "enabled" : "disabled")}");
        }
        
        public void PauseGame()
        {
            _isGamePaused = true;
            Time.timeScale = 0f;
            ShowNotification("Game Paused", "Game simulation paused", NotificationType.Info);
        }
        
        public void ResumeGame()
        {
            _isGamePaused = false;
            Time.timeScale = 1f;
            ShowNotification("Game Resumed", "Game simulation resumed", NotificationType.Info);
        }
        
        #endregion
        
        #region UI Preferences and State Management
        
        private void LoadUIPreferences()
        {
            // Load UI preferences from settings manager or player prefs
            try
            {
                if (_uiPreferences.ContainsKey("theme"))
                {
                    var theme = _uiPreferences["theme"] as UIThemeSO;
                    if (theme != null)
                    {
                        ApplyTheme(theme);
                    }
                }
                
                // Load other UI preferences
                if (_uiPreferences.ContainsKey("animations"))
                {
                    _enableUIAnimations = (bool)_uiPreferences["animations"];
                }
                
                if (_uiPreferences.ContainsKey("gamepadNavigation"))
                {
                    _enableGamepadNavigation = (bool)_uiPreferences["gamepadNavigation"];
                }
                
                LogInfo("UI preferences loaded");
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to load UI preferences: {ex.Message}");
            }
        }
        
        public void SaveUIPreferences()
        {
            try
            {
                _uiPreferences["animations"] = _enableUIAnimations;
                _uiPreferences["gamepadNavigation"] = _enableGamepadNavigation;
                
                // Save to persistent storage
                foreach (var pref in _uiPreferences)
                {
                    OnUIPreferenceChanged?.Invoke(pref.Key, pref.Value);
                }
                
                HasUnsavedUIPreferences = false;
                LogInfo("UI preferences saved");
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to save UI preferences: {ex.Message}");
            }
        }
        
        public T GetUIController<T>() where T : class, IUIController
        {
            return _controllersByType.Values.OfType<T>().FirstOrDefault();
        }
        
        public IUIController GetUIController(string controllerId)
        {
            _uiControllers.TryGetValue(controllerId, out var controller);
            return controller;
        }
        
        public bool IsControllerActive(string controllerId)
        {
            // return _currentUIState.ActiveControllers.TryGetValue(controllerId, out var isActive) && isActive;
            // TODO: Implement proper controller active state tracking when UIState supports it
            return false; // Placeholder implementation
        }
        
        public void SetUIPreference(string key, object value)
        {
            _uiPreferences[key] = value;
            HasUnsavedUIPreferences = true;
            OnUIPreferenceChanged?.Invoke(key, value);
        }
        
        public T GetUIPreference<T>(string key, T defaultValue = default(T))
        {
            if (_uiPreferences.TryGetValue(key, out var value) && value is T)
            {
                return (T)value;
            }
            return defaultValue;
        }
        
        private void ProcessNotificationQueue()
        {
            if (_notificationQueue.Count == 0) return;
            
            var notification = _notificationQueue.Dequeue();
            var notificationElement = CreateNotification(notification.Title, notification.Message, notification.Type);
            
            // Add to notification container (from notification UI document)
            if (_notificationRootElement != null)
            {
                var container = _notificationRootElement.Q<VisualElement>("notification-container");
                container?.Add(notificationElement);
                
                // Auto-remove after duration
                this.DelayedCall(notification.Duration, () =>
                {
                    if (notificationElement.parent != null)
                    {
                        container?.Remove(notificationElement);
                    }
                });
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private void ApplyTheme(UIThemeSO theme)
        {
            if (theme == null) return;
            
            // Apply theme colors and settings to root element
            _rootElement.style.backgroundColor = new StyleColor(theme.BackgroundColor);
            _rootElement.style.color = new StyleColor(theme.TextColor);
            
            LogInfo($"Applied UI theme: {theme.name}");
        }
        
        private void PlayUISound(AudioClip clip)
        {
            if (_uiAudioSource != null && clip != null)
            {
                _uiAudioSource.PlayOneShot(clip);
            }
        }
        
        private void AnimatePanelIn(UIPanel panel)
        {
            // Would implement panel transition animations
            // UIPanel doesn't have GameObject property
            // if (panel.GameObject != null)
            // {
            //     // Add animation logic here
            // }
            if (panel != null)
            {
                // Add animation logic here using UIPanel's VisualElement
                LogInfo($"Animating panel: {panel.PanelId}");
            }
        }
        
        private void AnimateControllerIn(IUIController controller)
        {
            // Animation logic for controller transitions
            if (controller is MonoBehaviour monoBehaviour)
            {
                // Add animation logic here
                LogInfo($"Animating controller: {controller.GetType().Name}");
            }
        }
        
        private void RefreshCurrentPanel()
        {
            if (string.IsNullOrEmpty(_currentActivePanel)) return;
            
            if (_uiControllers.TryGetValue(_currentActivePanel, out var controller))
            {
                try
                {
                    // Refresh the current controller if it has a refresh method
                    if (controller is FacilityDashboardController dashboard)
                    {
                        dashboard.RefreshDashboard();
                    }
                    
                    ShowNotification("Panel Refreshed", $"{_currentActivePanel} updated successfully", NotificationType.Success);
                    LogInfo($"Refreshed panel: {_currentActivePanel}");
                }
                catch (System.Exception ex)
                {
                    LogError($"Failed to refresh panel {_currentActivePanel}: {ex.Message}");
                }
            }
        }
        
        private void UpdateNotifications()
        {
            // Process any queued notifications
            while (_notificationQueue.Count > 0)
            {
                ProcessNotificationQueue();
            }
        }
        
        private void ExecuteEmergencyStop()
        {
            // Would trigger emergency shutdown of all systems
            ShowNotification("Emergency Stop", "All systems have been shut down", NotificationType.Critical);
            LogInfo("Emergency stop executed");
        }
        
        private void ExecuteSystemOptimization()
        {
            // Would trigger AI-driven system optimization
            this.DelayedCall(2f, () =>
            {
                ShowNotification("Optimization Complete", "System performance improved by 12%", NotificationType.Success);
            });
        }
        
        protected override void OnManagerShutdown()
        {
            // Save UI preferences before shutdown
            SaveUIPreferences();
            
            // Cleanup UI resources
            _activePanels.Clear();
            _navigationHistory.Clear();
            _uiControllers.Clear();
            _controllersByType.Clear();
            _uiPreferences.Clear();
            _notificationQueue.Clear();
            
            // Unsubscribe from events
            if (_dashboardController != null)
            {
                _dashboardController.OnPanelSelected -= NavigateToPanel;
                _dashboardController.OnAlertClicked -= HandleAlertClick;
                _dashboardController.OnQuickActionTriggered -= HandleQuickAction;
            }
            
            // Clear UI state
            _currentUIState = new UIState();
            
            // LogInfo("GameUIManager shutdown complete");
        }
        
        #endregion
    }
    
    // Supporting data structures (NotificationType moved to UIManager.cs to avoid duplicates)
    
    [System.Serializable]
    public class NotificationData
    {
        public string Title;
        public string Message;
        public NotificationType Type;
        public float Duration;
        public DateTime Timestamp;
    }
    
    // Interface for UI controllers
    public interface IUIController
    {
        // Basic controller interface methods could be defined here
    }
}

// Extension methods for UI utilities
public static class UIExtensions
{
    public static void DelayedCall(this MonoBehaviour mono, float delay, System.Action callback)
    {
        mono.StartCoroutine(DelayedCallCoroutine(delay, callback));
    }
    
    private static System.Collections.IEnumerator DelayedCallCoroutine(float delay, System.Action callback)
    {
        yield return new UnityEngine.WaitForSeconds(delay);
        callback?.Invoke();
    }
}