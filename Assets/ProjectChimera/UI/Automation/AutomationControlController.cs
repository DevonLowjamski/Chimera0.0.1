using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
// using ProjectChimera.Systems.Automation;
using ProjectChimera.Data.Automation;
using ProjectChimera.Data.UI;

namespace ProjectChimera.UI.Automation
{
    /// <summary>
    /// Automation Control UI Controller for Project Chimera.
    /// Provides comprehensive automation management including sensor networks, rules, and IoT devices.
    /// Features intuitive drag-and-drop rule creation and real-time monitoring capabilities.
    /// </summary>
    public class AutomationControlController : MonoBehaviour
    {
        [Header("Automation UI Configuration")]
        [SerializeField] private UIDocument _automationDocument;
        [SerializeField] private AutomationUISettings _uiSettings;
        [SerializeField] private bool _enableRealTimeMonitoring = true;
        [SerializeField] private float _monitoringInterval = 2f;
        
        [Header("Rule Builder Configuration")]
        [SerializeField] private bool _enableDragDropRules = true;
        [SerializeField] private int _maxActiveRules = 50;
        [SerializeField] private bool _enableRuleTemplates = true;
        
        [Header("Sensor Network Configuration")]
        [SerializeField] private int _maxSensorsPerZone = 20;
        [SerializeField] private float _sensorUpdateInterval = 1f;
        [SerializeField] private bool _enableSensorAlerts = true;
        
        [Header("Audio Configuration")]
        [SerializeField] private AudioClip _ruleCreatedSound;
        [SerializeField] private AudioClip _ruleTriggeredSound;
        [SerializeField] private AudioClip _sensorAlertSound;
        [SerializeField] private AudioClip _deviceConnectedSound;
        [SerializeField] private AudioSource _audioSource;
        
        // System references (commented out to avoid circular dependencies)
        // private AutomationManager _automationManager;
        // private SensorManager _sensorManager;
        // private DeviceManager _deviceManager;
        // private ScheduleManager _scheduleManager;
        private object _hvacManager; // Placeholder
        private object _lightingManager; // Placeholder
        private List<AutomationRule> savedRules = new List<AutomationRule>(); // Placeholder
        
        // UI Elements - Main Navigation
        private VisualElement _rootElement;
        private Button _rulesTabButton;
        private Button _sensorsTabButton;
        private Button _devicesTabButton;
        private Button _schedulesTabButton;
        private Button _templatesTabButton;
        
        // Tab Panels
        private VisualElement _rulesPanel;
        private VisualElement _sensorsPanel;
        private VisualElement _devicesPanel;
        private VisualElement _schedulesPanel;
        private VisualElement _templatesPanel;
        
        // Rules Panel Elements
        private VisualElement _activeRulesList;
        private VisualElement _ruleBuilderPanel;
        private Button _createRuleButton;
        private Button _importRulesButton;
        private Button _exportRulesButton;
        private Label _activeRulesCountLabel;
        private VisualElement _ruleStatsContainer;
        
        // Rule Builder Elements
        private VisualElement _triggerSection;
        private VisualElement _conditionsSection;
        private VisualElement _actionsSection;
        private DropdownField _triggerTypeDropdown;
        private DropdownField _sensorSourceDropdown;
        private FloatField _triggerValueField;
        private DropdownField _operatorDropdown;
        private TextField _ruleNameField;
        private TextField _ruleDescriptionField;
        private Toggle _ruleEnabledToggle;
        private Slider _rulePrioritySlider;
        private Button _addConditionButton;
        private Button _addActionButton;
        private Button _saveRuleButton;
        private Button _testRuleButton;
        
        // Sensors Panel Elements
        private VisualElement _sensorNetworkMap;
        private VisualElement _sensorsList;
        private VisualElement _sensorDetailsPanel;
        private Button _addSensorButton;
        private Button _calibrateSensorsButton;
        private Button _refreshSensorsButton;
        private Label _onlineSensorsCountLabel;
        private ProgressBar _networkHealthBar;
        private VisualElement _sensorAlertsContainer;
        
        // Devices Panel Elements
        private VisualElement _devicesList;
        private VisualElement _deviceGroupsContainer;
        private Button _scanDevicesButton;
        private Button _addDeviceButton;
        private Button _createGroupButton;
        private Label _connectedDevicesCountLabel;
        private ProgressBar _deviceNetworkBar;
        private VisualElement _deviceControlsPanel;
        
        // Schedules Panel Elements
        private VisualElement _schedulesList;
        private VisualElement _scheduleCalendar;
        private Button _createScheduleButton;
        private Button _importScheduleButton;
        private Label _activeSchedulesCountLabel;
        private VisualElement _upcomingEventsContainer;
        
        // Templates Panel Elements
        private VisualElement _templatesList;
        private VisualElement _templatePreview;
        private Button _createTemplateButton;
        private Button _importTemplateButton;
        private DropdownField _templateCategoryDropdown;
        private TextField _templateSearchField;
        
        // Data and State
        private List<AutomationRule> _activeRules = new List<AutomationRule>();
        private List<SensorReading> _sensorReadings = new List<SensorReading>();
        private List<IoTDevice> _connectedDevices = new List<IoTDevice>();
        private List<AutomationSchedule> _activeSchedules = new List<AutomationSchedule>();
        private List<RuleTemplate> _ruleTemplates = new List<RuleTemplate>();
        private AutomationRule _currentRule = new AutomationRule();
        private string _currentTab = "rules";
        private SensorData _selectedSensor = null;
        private IoTDevice _selectedDevice = null;
        private float _lastMonitoringTime;
        private bool _isUpdating = false;
        
        // Events
        public System.Action<AutomationRule> OnRuleCreated;
        public System.Action<AutomationRule> OnRuleTriggered;
        public System.Action<SensorReading> OnSensorAlert;
        public System.Action<IoTDevice> OnDeviceConnected;
        public System.Action<string> OnTabChanged;
        
        private void Start()
        {
            InitializeController();
            InitializeSystemReferences();
            SetupUIElements();
            SetupEventHandlers();
            LoadAutomationData();
            
            if (_enableRealTimeMonitoring)
            {
                InvokeRepeating(nameof(UpdateMonitoring), 1f, _monitoringInterval);
                InvokeRepeating(nameof(UpdateSensorReadings), 0.5f, _sensorUpdateInterval);
            }
        }
        
        private void InitializeController()
        {
            if (_automationDocument == null)
            {
                Debug.LogError("Automation UI Document not assigned!");
                return;
            }
            
            _rootElement = _automationDocument.rootVisualElement;
            _lastMonitoringTime = Time.time;
            
            // Initialize current rule
            _currentRule = new AutomationRule
            {
                RuleId = Guid.NewGuid().ToString(),
                RuleName = "New Rule",
                IsEnabled = true,
                Priority = 5,
                Trigger = new AutomationTrigger(),
                Condition = new AutomationCondition(),
                Actions = new List<AutomationAction>()
            };
            
            Debug.Log("Automation Control Controller initialized");
        }
        
        private void InitializeSystemReferences()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("GameManager not found - using simulation mode");
                return;
            }
            
            // _automationManager = gameManager.GetManager<AutomationManager>();
            // _sensorManager = gameManager.GetManager<SensorManager>();
            // _iotManager = gameManager.GetManager<IoTDeviceManager>();
            
            Debug.Log("Automation Control connected to automation systems");
        }
        
        private void SetupUIElements()
        {
            // Main navigation tabs
            _rulesTabButton = _rootElement.Q<Button>("rules-tab");
            _sensorsTabButton = _rootElement.Q<Button>("sensors-tab");
            _devicesTabButton = _rootElement.Q<Button>("devices-tab");
            _schedulesTabButton = _rootElement.Q<Button>("schedules-tab");
            _templatesTabButton = _rootElement.Q<Button>("templates-tab");
            
            // Tab panels
            _rulesPanel = _rootElement.Q<VisualElement>("rules-panel");
            _sensorsPanel = _rootElement.Q<VisualElement>("sensors-panel");
            _devicesPanel = _rootElement.Q<VisualElement>("devices-panel");
            _schedulesPanel = _rootElement.Q<VisualElement>("schedules-panel");
            _templatesPanel = _rootElement.Q<VisualElement>("templates-panel");
            
            // Rules panel elements
            _activeRulesList = _rootElement.Q<VisualElement>("active-rules-list");
            _ruleBuilderPanel = _rootElement.Q<VisualElement>("rule-builder-panel");
            _createRuleButton = _rootElement.Q<Button>("create-rule-button");
            _importRulesButton = _rootElement.Q<Button>("import-rules-button");
            _exportRulesButton = _rootElement.Q<Button>("export-rules-button");
            _activeRulesCountLabel = _rootElement.Q<Label>("active-rules-count");
            _ruleStatsContainer = _rootElement.Q<VisualElement>("rule-stats-container");
            
            // Rule builder elements
            _triggerSection = _rootElement.Q<VisualElement>("trigger-section");
            _conditionsSection = _rootElement.Q<VisualElement>("conditions-section");
            _actionsSection = _rootElement.Q<VisualElement>("actions-section");
            _triggerTypeDropdown = _rootElement.Q<DropdownField>("trigger-type-dropdown");
            _sensorSourceDropdown = _rootElement.Q<DropdownField>("sensor-source-dropdown");
            _triggerValueField = _rootElement.Q<FloatField>("trigger-value-field");
            _operatorDropdown = _rootElement.Q<DropdownField>("operator-dropdown");
            _ruleNameField = _rootElement.Q<TextField>("rule-name-field");
            _ruleDescriptionField = _rootElement.Q<TextField>("rule-description-field");
            _ruleEnabledToggle = _rootElement.Q<Toggle>("rule-enabled-toggle");
            _rulePrioritySlider = _rootElement.Q<Slider>("rule-priority-slider");
            _addConditionButton = _rootElement.Q<Button>("add-condition-button");
            _addActionButton = _rootElement.Q<Button>("add-action-button");
            _saveRuleButton = _rootElement.Q<Button>("save-rule-button");
            _testRuleButton = _rootElement.Q<Button>("test-rule-button");
            
            // Sensors panel elements
            _sensorNetworkMap = _rootElement.Q<VisualElement>("sensor-network-map");
            _sensorsList = _rootElement.Q<VisualElement>("sensors-list");
            _sensorDetailsPanel = _rootElement.Q<VisualElement>("sensor-details-panel");
            _addSensorButton = _rootElement.Q<Button>("add-sensor-button");
            _calibrateSensorsButton = _rootElement.Q<Button>("calibrate-sensors-button");
            _refreshSensorsButton = _rootElement.Q<Button>("refresh-sensors-button");
            _onlineSensorsCountLabel = _rootElement.Q<Label>("online-sensors-count");
            _networkHealthBar = _rootElement.Q<ProgressBar>("network-health-bar");
            _sensorAlertsContainer = _rootElement.Q<VisualElement>("sensor-alerts-container");
            
            // Devices panel elements
            _devicesList = _rootElement.Q<VisualElement>("devices-list");
            _deviceGroupsContainer = _rootElement.Q<VisualElement>("device-groups-container");
            _scanDevicesButton = _rootElement.Q<Button>("scan-devices-button");
            _addDeviceButton = _rootElement.Q<Button>("add-device-button");
            _createGroupButton = _rootElement.Q<Button>("create-group-button");
            _connectedDevicesCountLabel = _rootElement.Q<Label>("connected-devices-count");
            _deviceNetworkBar = _rootElement.Q<ProgressBar>("device-network-bar");
            _deviceControlsPanel = _rootElement.Q<VisualElement>("device-controls-panel");
            
            // Schedules panel elements
            _schedulesList = _rootElement.Q<VisualElement>("schedules-list");
            _scheduleCalendar = _rootElement.Q<VisualElement>("schedule-calendar");
            _createScheduleButton = _rootElement.Q<Button>("create-schedule-button");
            _importScheduleButton = _rootElement.Q<Button>("import-schedule-button");
            _activeSchedulesCountLabel = _rootElement.Q<Label>("active-schedules-count");
            _upcomingEventsContainer = _rootElement.Q<VisualElement>("upcoming-events-container");
            
            // Templates panel elements
            _templatesList = _rootElement.Q<VisualElement>("templates-list");
            _templatePreview = _rootElement.Q<VisualElement>("template-preview");
            _createTemplateButton = _rootElement.Q<Button>("create-template-button");
            _importTemplateButton = _rootElement.Q<Button>("import-template-button");
            _templateCategoryDropdown = _rootElement.Q<DropdownField>("template-category-dropdown");
            _templateSearchField = _rootElement.Q<TextField>("template-search-field");
            
            SetupDropdowns();
            SetupInitialState();
        }
        
        private void SetupDropdowns()
        {
            // Trigger types
            if (_triggerTypeDropdown != null)
            {
                _triggerTypeDropdown.choices = new List<string>
                {
                    "Threshold Exceeded", "Threshold Below", "Value Changed", 
                    "Time Based", "Device Status", "Manual Trigger"
                };
                _triggerTypeDropdown.value = "Threshold Exceeded";
            }
            
            // Operators
            if (_operatorDropdown != null)
            {
                _operatorDropdown.choices = new List<string>
                {
                    "Greater Than", "Less Than", "Equal To", "Not Equal To",
                    "Greater Than or Equal", "Less Than or Equal", "Between", "Outside Range"
                };
                _operatorDropdown.value = "Greater Than";
            }
            
            // Sensor sources
            if (_sensorSourceDropdown != null)
            {
                _sensorSourceDropdown.choices = new List<string>
                {
                    "Temperature Sensor", "Humidity Sensor", "CO2 Sensor", 
                    "Light Sensor", "pH Sensor", "EC Sensor", "Pressure Sensor"
                };
                _sensorSourceDropdown.value = "Temperature Sensor";
            }
            
            // Template categories
            if (_templateCategoryDropdown != null)
            {
                _templateCategoryDropdown.choices = new List<string>
                {
                    "All", "Environmental", "Security", "Maintenance", 
                    "Optimization", "Emergency", "Custom"
                };
                _templateCategoryDropdown.value = "All";
            }
        }
        
        private void SetupInitialState()
        {
            // Show rules panel by default
            ShowPanel("rules");
            
            // Initialize rule builder
            if (_ruleNameField != null)
                _ruleNameField.value = "New Automation Rule";
            
            if (_ruleEnabledToggle != null)
                _ruleEnabledToggle.value = true;
            
            if (_rulePrioritySlider != null)
                _rulePrioritySlider.value = 5f;
        }
        
        private void SetupEventHandlers()
        {
            // Tab navigation
            _rulesTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("rules"));
            _sensorsTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("sensors"));
            _devicesTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("devices"));
            _schedulesTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("schedules"));
            _templatesTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("templates"));
            
            // Rules panel controls
            _createRuleButton?.RegisterCallback<ClickEvent>(evt => CreateNewRule());
            _importRulesButton?.RegisterCallback<ClickEvent>(evt => ImportRules());
            _exportRulesButton?.RegisterCallback<ClickEvent>(evt => ExportRules());
            
            // Rule builder controls
            _addConditionButton?.RegisterCallback<ClickEvent>(evt => AddCondition());
            _addActionButton?.RegisterCallback<ClickEvent>(evt => AddAction());
            _saveRuleButton?.RegisterCallback<ClickEvent>(evt => SaveRule());
            _testRuleButton?.RegisterCallback<ClickEvent>(evt => TestRule());
            _ruleNameField?.RegisterValueChangedCallback(evt => UpdateRuleName(evt.newValue));
            _ruleDescriptionField?.RegisterValueChangedCallback(evt => UpdateRuleDescription(evt.newValue));
            _ruleEnabledToggle?.RegisterValueChangedCallback(evt => UpdateRuleEnabled(evt.newValue));
            _rulePrioritySlider?.RegisterValueChangedCallback(evt => UpdateRulePriority(evt.newValue));
            
            // Sensors panel controls
            _addSensorButton?.RegisterCallback<ClickEvent>(evt => AddSensor());
            _calibrateSensorsButton?.RegisterCallback<ClickEvent>(evt => CalibrateSensors());
            _refreshSensorsButton?.RegisterCallback<ClickEvent>(evt => RefreshSensors());
            
            // Devices panel controls
            _scanDevicesButton?.RegisterCallback<ClickEvent>(evt => ScanDevices());
            _addDeviceButton?.RegisterCallback<ClickEvent>(evt => AddDevice());
            _createGroupButton?.RegisterCallback<ClickEvent>(evt => CreateDeviceGroup());
            
            // Schedules panel controls
            _createScheduleButton?.RegisterCallback<ClickEvent>(evt => CreateSchedule());
            _importScheduleButton?.RegisterCallback<ClickEvent>(evt => ImportSchedule());
            
            // Templates panel controls
            _createTemplateButton?.RegisterCallback<ClickEvent>(evt => CreateTemplate());
            _importTemplateButton?.RegisterCallback<ClickEvent>(evt => ImportTemplate());
            _templateCategoryDropdown?.RegisterValueChangedCallback(evt => FilterTemplates(evt.newValue));
            _templateSearchField?.RegisterValueChangedCallback(evt => SearchTemplates(evt.newValue));
        }
        
        #region Panel Management
        
        private void ShowPanel(string panelName)
        {
            // Hide all panels
            _rulesPanel?.AddToClassList("hidden");
            _sensorsPanel?.AddToClassList("hidden");
            _devicesPanel?.AddToClassList("hidden");
            _schedulesPanel?.AddToClassList("hidden");
            _templatesPanel?.AddToClassList("hidden");
            
            // Remove active state from all tabs
            _rulesTabButton?.RemoveFromClassList("tab-active");
            _sensorsTabButton?.RemoveFromClassList("tab-active");
            _devicesTabButton?.RemoveFromClassList("tab-active");
            _schedulesTabButton?.RemoveFromClassList("tab-active");
            _templatesTabButton?.RemoveFromClassList("tab-active");
            
            // Show selected panel and activate tab
            switch (panelName)
            {
                case "rules":
                    _rulesPanel?.RemoveFromClassList("hidden");
                    _rulesTabButton?.AddToClassList("tab-active");
                    RefreshRulesData();
                    break;
                case "sensors":
                    _sensorsPanel?.RemoveFromClassList("hidden");
                    _sensorsTabButton?.AddToClassList("tab-active");
                    RefreshSensorsData();
                    break;
                case "devices":
                    _devicesPanel?.RemoveFromClassList("hidden");
                    _devicesTabButton?.AddToClassList("tab-active");
                    RefreshDevicesData();
                    break;
                case "schedules":
                    _schedulesPanel?.RemoveFromClassList("hidden");
                    _schedulesTabButton?.AddToClassList("tab-active");
                    RefreshSchedulesData();
                    break;
                case "templates":
                    _templatesPanel?.RemoveFromClassList("hidden");
                    _templatesTabButton?.AddToClassList("tab-active");
                    RefreshTemplatesData();
                    break;
            }
            
            _currentTab = panelName;
            OnTabChanged?.Invoke(panelName);
            
            Debug.Log($"Switched to {panelName} panel");
        }
        
        #endregion
        
        #region Rule Management
        
        private void CreateNewRule()
        {
            _currentRule = new AutomationRule
            {
                RuleId = Guid.NewGuid().ToString(),
                RuleName = "New Automation Rule",
                IsEnabled = true,
                Priority = 5,
                Trigger = new AutomationTrigger(),
                Condition = new AutomationCondition(),
                Actions = new List<AutomationAction>(),
                CreatedDate = DateTime.Now
            };
            
            UpdateRuleBuilder();
            Debug.Log("Created new automation rule");
        }
        
        private void SaveRule()
        {
            if (ValidateRule(_currentRule))
            {
                // if (_automationManager != null)
                // {
                    // _automationManager.CreateAutomationRule(_currentRule.RuleName, 
                        // _currentRule.Trigger, 
                        // _currentRule.Actions);
                // }
                
                _activeRules.Add(_currentRule);
                RefreshRulesDisplay();
                PlaySound(_ruleCreatedSound);
                OnRuleCreated?.Invoke(_currentRule);
                
                Debug.Log($"Saved automation rule: {_currentRule.RuleName}");
            }
            // else
            // {
                Debug.LogWarning("Rule validation failed");
            // }
        }
        
        private void TestRule()
        {
            // if (_automationManager != null && _currentRule != null)
            // {
                // bool testResult = _automationManager.TestAutomationRule(_currentRule);
                bool testResult = true; // Placeholder for testing
                Debug.Log($"Rule test result: {(testResult ? "PASSED" : "FAILED")}");
            // }
        }
        
        private bool ValidateRule(AutomationRule rule)
        {
            return !string.IsNullOrEmpty(rule.RuleName) && 
                   rule.Trigger != null && 
                   rule.Actions.Count > 0;
        }
        
        private void AddCondition()
        {
            var conditionRule = new ConditionRule
            {
                SensorId = _sensorSourceDropdown?.value ?? "default_sensor",
                Value = _triggerValueField?.value ?? 0f,
                Operator = GetOperatorFromDropdown()
            };
            
            if (_currentRule.Condition == null)
            {
                _currentRule.Condition = new AutomationCondition();
            }
            
            _currentRule.Condition.Rules.Add(conditionRule);
            UpdateConditionsDisplay();
        }
        
        private void AddAction()
        {
            var action = new AutomationAction
            {
                ActionId = Guid.NewGuid().ToString(),
                ActionType = ProjectChimera.Data.Automation.ActionType.SetTemperature,
                TargetZoneId = "default",
                Parameters = new Dictionary<string, object>()
            };
            
            _currentRule.Actions.Add(action);
            UpdateActionsDisplay();
        }
        
        #endregion
        
        #region Monitoring and Updates
        
        [ContextMenu("Update Monitoring")]
        public void UpdateMonitoring()
        {
            if (_isUpdating) return;
            
            _isUpdating = true;
            
            // Update automation statistics
            UpdateAutomationStats();
            
            // Check for triggered rules
            CheckTriggeredRules();
            
            // Update current tab data
            switch (_currentTab)
            {
                case "rules":
                    RefreshRulesData();
                    break;
                case "sensors":
                    RefreshSensorsData();
                    break;
                case "devices":
                    RefreshDevicesData();
                    break;
                case "schedules":
                    RefreshSchedulesData();
                    break;
            }
            
            _lastMonitoringTime = Time.time;
            _isUpdating = false;
        }
        
        private void UpdateSensorReadings()
        {
            // if (_sensorManager != null)
            // {
                // _sensorReadings = _sensorManager.GetAllSensorReadings();
            // }
            // else
            // {
                // Simulate sensor readings
                SimulateSensorReadings();
            // }
            
            // Update sensor displays if on sensors tab
            if (_currentTab == "sensors")
            {
                UpdateSensorsDisplay();
            }
        }
        
        private void SimulateSensorReadings()
        {
            var sensorTypes = new[] { "temperature", "humidity", "co2", "light", "ph", "ec" };
            _sensorReadings.Clear();
            
            foreach (var sensorType in sensorTypes)
            {
                for (int i = 1; i <= 3; i++)
                {
                    var reading = new SensorReading
                    {
                        SensorId = $"{sensorType}_sensor_{i}",
                        SensorType = sensorType,
                        Value = GenerateRealisticSensorValue(sensorType),
                        Timestamp = DateTime.Now,
                        ZoneId = $"zone_{i}",
                        IsOnline = UnityEngine.Random.Range(0f, 1f) > 0.05f // 95% uptime
                    };
                    
                    _sensorReadings.Add(reading);
                }
            }
        }
        
        private float GenerateRealisticSensorValue(string sensorType)
        {
            return sensorType switch
            {
                "temperature" => UnityEngine.Random.Range(20f, 28f),
                "humidity" => UnityEngine.Random.Range(45f, 65f),
                "co2" => UnityEngine.Random.Range(800f, 1200f),
                "light" => UnityEngine.Random.Range(200f, 600f),
                "ph" => UnityEngine.Random.Range(5.5f, 7.0f),
                "ec" => UnityEngine.Random.Range(1.0f, 2.5f),
                _ => UnityEngine.Random.Range(0f, 100f)
            };
        }
        
        #endregion
        
        #region UI Updates
        
        private void RefreshRulesData()
        {
            RefreshRulesDisplay();
            UpdateAutomationStats();
        }
        
        private void RefreshSensorsData()
        {
            UpdateSensorsDisplay();
            UpdateSensorNetworkHealth();
        }
        
        private void RefreshDevicesData()
        {
            UpdateDevicesDisplay();
            UpdateDeviceNetworkHealth();
        }
        
        private void RefreshSchedulesData()
        {
            UpdateSchedulesDisplay();
        }
        
        private void RefreshTemplatesData()
        {
            UpdateTemplatesDisplay();
        }
        
        private void RefreshRulesDisplay()
        {
            if (_activeRulesList == null) return;
            
            _activeRulesList.Clear();
            
            foreach (var rule in _activeRules)
            {
                var ruleElement = CreateRuleElement(rule);
                _activeRulesList.Add(ruleElement);
            }
            
            if (_activeRulesCountLabel != null)
            {
                _activeRulesCountLabel.text = $"{_activeRules.Count} Active Rules";
            }
        }
        
        private void UpdateSensorsDisplay()
        {
            if (_sensorsList == null) return;
            
            _sensorsList.Clear();
            
            var groupedSensors = _sensorReadings.GroupBy(s => s.ZoneId);
            
            foreach (var zone in groupedSensors)
            {
                var zoneElement = CreateSensorZoneElement(zone.Key, zone.ToList());
                _sensorsList.Add(zoneElement);
            }
            
            if (_onlineSensorsCountLabel != null)
            {
                int onlineCount = _sensorReadings.Count(s => s.IsOnline);
                _onlineSensorsCountLabel.text = $"{onlineCount}/{_sensorReadings.Count} Online";
            }
        }
        
        private void UpdateDevicesDisplay()
        {
            if (_devicesList == null) return;
            
            _devicesList.Clear();
            
            foreach (var device in _connectedDevices)
            {
                var deviceElement = CreateDeviceElement(device);
                _devicesList.Add(deviceElement);
            }
            
            if (_connectedDevicesCountLabel != null)
            {
                _connectedDevicesCountLabel.text = $"{_connectedDevices.Count} Connected";
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void LoadAutomationData()
        {
            // Load saved automation data
            // if (_automationManager != null)
            // {
                // var savedRules = _automationManager.GetActiveRules();
                _activeRules = savedRules?.ToList() ?? new List<AutomationRule>();
            // }
            
            // Load sensors data
            // if (_sensorManager != null)
            // {
                // _sensorReadings = _sensorManager.GetAllSensorReadings();
            // }
            
            // Load devices data
            // if (_iotManager != null)
            // {
                // _connectedDevices = _iotManager.GetConnectedDevices();
            // }
            
            Debug.Log("Automation data loaded");
        }
        
        private void UpdateRuleBuilder()
        {
            if (_ruleNameField != null)
                _ruleNameField.value = _currentRule.RuleName;
            
            if (_ruleEnabledToggle != null)
                _ruleEnabledToggle.value = _currentRule.IsEnabled;
            
            if (_rulePrioritySlider != null)
                _rulePrioritySlider.value = _currentRule.Priority;
            
            UpdateTriggersDisplay();
            UpdateConditionsDisplay();
            UpdateActionsDisplay();
        }
        
        private void UpdateTriggersDisplay()
        {
            // Update triggers display in rule builder
        }
        
        private void UpdateConditionsDisplay()
        {
            // Update conditions display in rule builder
        }
        
        private void UpdateActionsDisplay()
        {
            // Update actions display in rule builder
        }
        
        private void UpdateAutomationStats()
        {
            // Update automation statistics
        }
        
        private void CheckTriggeredRules()
        {
            foreach (var rule in _activeRules.Where(r => r.IsEnabled))
            {
                if (EvaluateRule(rule))
                {
                    TriggerRule(rule);
                }
            }
        }
        
        private bool EvaluateRule(AutomationRule rule)
        {
            // Simplified rule evaluation
            return UnityEngine.Random.Range(0f, 1f) < 0.01f; // 1% chance per check
        }
        
        private void TriggerRule(AutomationRule rule)
        {
            PlaySound(_ruleTriggeredSound);
            OnRuleTriggered?.Invoke(rule);
            Debug.Log($"Rule triggered: {rule.RuleName}");
        }
        
        private VisualElement CreateRuleElement(AutomationRule rule)
        {
            var element = new VisualElement();
            element.AddToClassList("rule-item");
            element.AddToClassList(rule.IsEnabled ? "rule-enabled" : "rule-disabled");
            
            var nameLabel = new Label(rule.RuleName);
            nameLabel.AddToClassList("rule-name");
            
            var statusLabel = new Label(rule.IsEnabled ? "Enabled" : "Disabled");
            statusLabel.AddToClassList("rule-status");
            
            var priorityLabel = new Label($"Priority: {rule.Priority}");
            priorityLabel.AddToClassList("rule-priority");
            
            var editButton = new Button(() => EditRule(rule));
            editButton.text = "Edit";
            editButton.AddToClassList("rule-edit-btn");
            
            var toggleButton = new Button(() => ToggleRule(rule));
            toggleButton.text = rule.IsEnabled ? "Disable" : "Enable";
            toggleButton.AddToClassList("rule-toggle-btn");
            
            element.Add(nameLabel);
            element.Add(statusLabel);
            element.Add(priorityLabel);
            element.Add(editButton);
            element.Add(toggleButton);
            
            return element;
        }
        
        private VisualElement CreateSensorZoneElement(string zoneId, List<SensorReading> sensors)
        {
            var zoneElement = new VisualElement();
            zoneElement.AddToClassList("sensor-zone");
            
            var zoneHeader = new Label($"Zone: {zoneId}");
            zoneHeader.AddToClassList("zone-header");
            zoneElement.Add(zoneHeader);
            
            foreach (var sensor in sensors)
            {
                var sensorElement = CreateSensorElement(sensor);
                zoneElement.Add(sensorElement);
            }
            
            return zoneElement;
        }
        
        private VisualElement CreateSensorElement(SensorReading sensor)
        {
            var element = new VisualElement();
            element.AddToClassList("sensor-item");
            element.AddToClassList(sensor.IsOnline ? "sensor-online" : "sensor-offline");
            
            var nameLabel = new Label(sensor.SensorId);
            nameLabel.AddToClassList("sensor-name");
            
            var valueLabel = new Label($"{sensor.Value:F2}");
            valueLabel.AddToClassList("sensor-value");
            
            var statusIndicator = new VisualElement();
            statusIndicator.AddToClassList("sensor-status-indicator");
            statusIndicator.AddToClassList(sensor.IsOnline ? "status-online" : "status-offline");
            
            element.Add(nameLabel);
            element.Add(valueLabel);
            element.Add(statusIndicator);
            
            return element;
        }
        
        private VisualElement CreateDeviceElement(IoTDevice device)
        {
            var element = new VisualElement();
            element.AddToClassList("device-item");
            
            var nameLabel = new Label(device.DeviceName);
            nameLabel.AddToClassList("device-name");
            
            var typeLabel = new Label(device.DeviceType.ToString());
            typeLabel.AddToClassList("device-type");
            
            var statusLabel = new Label(device.IsOnline ? "Online" : "Offline");
            statusLabel.AddToClassList("device-status");
            statusLabel.AddToClassList(device.IsOnline ? "status-online" : "status-offline");
            
            element.Add(nameLabel);
            element.Add(typeLabel);
            element.Add(statusLabel);
            
            return element;
        }
        
        private void UpdateSensorNetworkHealth()
        {
            if (_networkHealthBar != null)
            {
                float health = _sensorReadings.Count > 0 ? 
                    (float)_sensorReadings.Count(s => s.IsOnline) / _sensorReadings.Count : 0f;
                _networkHealthBar.value = health;
                _networkHealthBar.title = $"Network Health: {health:P0}";
            }
        }
        
        private void UpdateDeviceNetworkHealth()
        {
            if (_deviceNetworkBar != null)
            {
                float health = _connectedDevices.Count > 0 ? 
                    (float)_connectedDevices.Count(d => d.IsOnline) / _connectedDevices.Count : 0f;
                _deviceNetworkBar.value = health;
                _deviceNetworkBar.title = $"Device Network: {health:P0}";
            }
        }
        
        private void UpdateSchedulesDisplay()
        {
            // Update schedules display
        }
        
        private void UpdateTemplatesDisplay()
        {
            // Update templates display
        }
        
        private ComparisonOperator GetOperatorFromDropdown()
        {
            var operatorText = _operatorDropdown?.value ?? "Greater Than";
            return operatorText switch
            {
                "Greater Than" => ComparisonOperator.GreaterThan,
                "Less Than" => ComparisonOperator.LessThan,
                "Equal To" => ComparisonOperator.Equals,
                "Not Equal To" => ComparisonOperator.NotEquals,
                _ => ComparisonOperator.GreaterThan
            };
        }
        
        // Event handlers for UI controls
        private void UpdateRuleName(string newName)
        {
            _currentRule.RuleName = newName;
        }
        
        private void UpdateRuleDescription(string newDescription)
        {
            if (_currentRule != null)
                _currentRule.Description = newDescription;
        }
        
        private void UpdateRuleEnabled(bool enabled)
        {
            _currentRule.IsEnabled = enabled;
        }
        
        private void UpdateRulePriority(float priority)
        {
            _currentRule.Priority = (int)priority;
        }
        
        private void EditRule(AutomationRule rule)
        {
            _currentRule = rule;
            UpdateRuleBuilder();
        }
        
        private void ToggleRule(AutomationRule rule)
        {
            rule.IsEnabled = !rule.IsEnabled;
            RefreshRulesDisplay();
        }
        
        private void ImportRules()
        {
            Debug.Log("Import rules functionality");
        }
        
        private void ExportRules()
        {
            Debug.Log("Export rules functionality");
        }
        
        private void AddSensor()
        {
            Debug.Log("Add sensor functionality");
        }
        
        private void CalibrateSensors()
        {
            Debug.Log("Calibrate sensors functionality");
        }
        
        private void RefreshSensors()
        {
            UpdateSensorReadings();
            RefreshSensorsData();
        }
        
        private void ScanDevices()
        {
            Debug.Log("Scan devices functionality");
        }
        
        private void AddDevice()
        {
            Debug.Log("Add device functionality");
        }
        
        private void CreateDeviceGroup()
        {
            Debug.Log("Create device group functionality");
        }
        
        private void CreateSchedule()
        {
            Debug.Log("Create schedule functionality");
        }
        
        private void ImportSchedule()
        {
            Debug.Log("Import schedule functionality");
        }
        
        private void CreateTemplate()
        {
            Debug.Log("Create template functionality");
        }
        
        private void ImportTemplate()
        {
            Debug.Log("Import template functionality");
        }
        
        private void FilterTemplates(string category)
        {
            Debug.Log($"Filter templates by category: {category}");
        }
        
        private void SearchTemplates(string searchTerm)
        {
            Debug.Log($"Search templates: {searchTerm}");
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
        }
        
        #endregion
    }
    
    // Supporting data structures
    [System.Serializable]
    public class SensorReading
    {
        public string SensorId;
        public string SensorType;
        public float Value;
        public DateTime Timestamp;
        public string ZoneId;
        public bool IsOnline;
    }
    
    [System.Serializable]
    public class SensorData
    {
        public string SensorId;
        public string SensorName;
        public string SensorType;
        public string ZoneId;
        public bool IsOnline;
        public float LastValue;
        public DateTime LastReading;
        public List<SensorReading> History;
    }
    
    [System.Serializable]
    public class IoTDevice
    {
        public string DeviceId;
        public string DeviceName;
        public IoTDeviceType DeviceType;
        public bool IsOnline;
        public string IpAddress;
        public DateTime LastSeen;
        public Dictionary<string, object> Properties;
    }
    
    [System.Serializable]
    public class RuleTemplate
    {
        public string TemplateId;
        public string Name;
        public string Description;
        public string Category;
        public AutomationRule RuleDefinition;
        public List<string> Tags;
    }
    
    public enum IoTDeviceType
    {
        Sensor,
        Actuator,
        Controller,
        Display,
        Camera,
        Switch,
        Other
    }
    
    [System.Serializable]
    public class AutomationUISettings
    {
        public bool EnableDragDropRules = true;
        public bool EnableRuleTemplates = true;
        public bool EnableSensorAlerts = true;
        public float MonitoringInterval = 2f;
        public int MaxActiveRules = 50;
        public bool PlaySoundEffects = true;
    }
}