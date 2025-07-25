using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.UI.Core;
using ProjectChimera.UI.Events;
using ProjectChimera.UI.Components;
using ProjectChimera.Data;
using ProjectChimera.Data.UI;
// using ProjectChimera.Systems.Environment;
// using ProjectChimera.Systems.Facilities;
// using ProjectChimera.Systems.Automation;

namespace ProjectChimera.UI.Panels
{
    /// <summary>
    /// Facility management panel for Project Chimera.
    /// Provides comprehensive environmental controls and monitoring for cultivation facilities.
    /// </summary>
    public class FacilityManagementPanel : UIPanel
    {
        [Header("Facility Events")]
        [SerializeField] private SimpleGameEventSO _onEnvironmentalControlChanged;
        [SerializeField] private SimpleGameEventSO _onFacilityAlertTriggered;
        [SerializeField] private UIButtonClickEventSO _onFacilityButtonClicked;
        [SerializeField] private UISliderEventSO _onEnvironmentalSliderChanged;
        
        [Header("Update Settings")]
        [SerializeField] private float _dataUpdateInterval = 2f;
        [SerializeField] private bool _enableRealTimeMonitoring = true;
        
        // Main layout containers
        private VisualElement _headerContainer;
        private VisualElement _contentContainer;
        private VisualElement _controlPanelContainer;
        private VisualElement _monitoringContainer;
        private VisualElement _alertsContainer;
        
        // Header elements
        private Label _titleLabel;
        private Button _closeButton;
        private DropdownField _facilitySelector;
        private UIStatusIndicator _overallStatus;
        
        // Environmental control sections
        private VisualElement _hvacControls;
        private VisualElement _lightingControls;
        private VisualElement _irrigationControls;
        private VisualElement _automationControls;
        
        // HVAC controls
        private Slider _temperatureSetpointSlider;
        private Slider _humiditySetpointSlider;
        private Slider _co2SetpointSlider;
        private Toggle _hvacSystemToggle;
        private Button _hvacPresetButton;
        private UIDataCard _currentTemperatureCard;
        private UIDataCard _currentHumidityCard;
        private UIDataCard _currentCO2Card;
        
        // Lighting controls
        private Slider _lightIntensitySlider;
        private Slider _lightScheduleSlider;
        private DropdownField _lightSpectrumDropdown;
        private Toggle _lightingSystemToggle;
        private UIProgressBar _dailyLightIntegralBar;
        private UISimpleChart _lightingScheduleChart;
        
        // Irrigation controls
        private Slider _waterFlowRateSlider;
        private Slider _nutrientConcentrationSlider;
        private DropdownField _irrigationScheduleDropdown;
        private Toggle _irrigationSystemToggle;
        private UIDataCard _waterConsumptionCard;
        private UIDataCard _nutrientLevelsCard;
        
        // Automation controls
        private Toggle _automationEnabledToggle;
        private DropdownField _automationModeDropdown;
        private Button _automationPresetButton;
        private UIStatusIndicator _automationStatus;
        
        // Monitoring displays
        private VisualElement _environmentalChartsContainer;
        private UISimpleChart _temperatureChart;
        private UISimpleChart _humidityChart;
        private UISimpleChart _co2Chart;
        private UISimpleChart _powerConsumptionChart;
        
        // Facility data
        private List<string> _availableFacilities;
        private string _selectedFacilityId;
        private Dictionary<string, float> _currentEnvironmentalData;
        private List<FacilityAlert> _activeAlerts;
        
        // Game managers
        // private EnvironmentalManager _environmentalManager;
        // private FacilityManager _facilityManager;
        // private AutomationManager _automationManager;
        
        protected override void SetupUIElements()
        {
            base.SetupUIElements();
            
            // Get manager references
            // _environmentalManager = GameManager.Instance?.GetManager<EnvironmentalManager>();
            // _facilityManager = GameManager.Instance?.GetManager<FacilityManager>();
            // _automationManager = GameManager.Instance?.GetManager<AutomationManager>();
            
            // Initialize data structures
            _availableFacilities = new List<string>();
            _currentEnvironmentalData = new Dictionary<string, float>();
            _activeAlerts = new List<FacilityAlert>();
            
            LoadFacilityData();
            
            CreateFacilityLayout();
            CreateHeader();
            CreateControlPanels();
            CreateMonitoringDisplays();
            CreateAlertsSection();
            
            StartDataUpdates();
        }
        
        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            
            // Header controls
            _closeButton?.RegisterCallback<ClickEvent>(OnCloseClicked);
            _facilitySelector?.RegisterCallback<ChangeEvent<string>>(OnFacilityChanged);
            
            // HVAC controls
            _temperatureSetpointSlider?.BindToEventChannel("temperature_setpoint", PanelId, _onEnvironmentalSliderChanged);
            _humiditySetpointSlider?.BindToEventChannel("humidity_setpoint", PanelId, _onEnvironmentalSliderChanged);
            _co2SetpointSlider?.BindToEventChannel("co2_setpoint", PanelId, _onEnvironmentalSliderChanged);
            _hvacSystemToggle?.RegisterCallback<ChangeEvent<bool>>(OnHVACSystemToggled);
            _hvacPresetButton?.RegisterCallback<ClickEvent>(OnHVACPresetClicked);
            
            // Lighting controls  
            _lightIntensitySlider?.BindToEventChannel("light_intensity", PanelId, _onEnvironmentalSliderChanged);
            _lightScheduleSlider?.BindToEventChannel("light_schedule", PanelId, _onEnvironmentalSliderChanged);
            _lightSpectrumDropdown?.RegisterCallback<ChangeEvent<string>>(OnLightSpectrumChanged);
            _lightingSystemToggle?.RegisterCallback<ChangeEvent<bool>>(OnLightingSystemToggled);
            
            // Irrigation controls
            _waterFlowRateSlider?.BindToEventChannel("water_flow_rate", PanelId, _onEnvironmentalSliderChanged);
            _nutrientConcentrationSlider?.BindToEventChannel("nutrient_concentration", PanelId, _onEnvironmentalSliderChanged);
            _irrigationScheduleDropdown?.RegisterCallback<ChangeEvent<string>>(OnIrrigationScheduleChanged);
            _irrigationSystemToggle?.RegisterCallback<ChangeEvent<bool>>(OnIrrigationSystemToggled);
            
            // Automation controls
            _automationEnabledToggle?.RegisterCallback<ChangeEvent<bool>>(OnAutomationToggled);
            _automationModeDropdown?.RegisterCallback<ChangeEvent<string>>(OnAutomationModeChanged);
            _automationPresetButton?.RegisterCallback<ClickEvent>(OnAutomationPresetClicked);
        }
        
        /// <summary>
        /// Create the main facility layout
        /// </summary>
        private void CreateFacilityLayout()
        {
            _rootElement.Clear();
            
            var mainContainer = new VisualElement();
            mainContainer.name = "facility-main-container";
            mainContainer.style.flexGrow = 1;
            // mainContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.BackgroundDark;
            mainContainer.style.flexDirection = FlexDirection.Column;
            
            // Header
            _headerContainer = new VisualElement();
            _headerContainer.name = "facility-header";
            _headerContainer.style.height = 80;
            // _headerContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            _headerContainer.style.flexDirection = FlexDirection.Row;
            _headerContainer.style.alignItems = Align.Center;
            _headerContainer.style.justifyContent = Justify.SpaceBetween;
            _headerContainer.style.paddingLeft = 24;
            _headerContainer.style.paddingRight = 24;
            _headerContainer.style.borderBottomWidth = 2;
            // _headerContainer.style.borderBottomColor = _uiManager.DesignSystem.ColorPalette.PrimaryGreen;
            
            // Content area
            _contentContainer = new VisualElement();
            _contentContainer.name = "facility-content";
            _contentContainer.style.flexGrow = 1;
            _contentContainer.style.flexDirection = FlexDirection.Row;
            _contentContainer.style.paddingTop = 20;
            _contentContainer.style.paddingBottom = 20;
            _contentContainer.style.paddingLeft = 20;
            _contentContainer.style.paddingRight = 20;
            
            // Control panels (left side)
            _controlPanelContainer = new VisualElement();
            _controlPanelContainer.name = "control-panels";
            _controlPanelContainer.style.width = 400;
            _controlPanelContainer.style.marginRight = 20;
            
            // Monitoring displays (right side)
            _monitoringContainer = new VisualElement();
            _monitoringContainer.name = "monitoring-displays";
            _monitoringContainer.style.flexGrow = 1;
            
            _contentContainer.Add(_controlPanelContainer);
            _contentContainer.Add(_monitoringContainer);
            
            mainContainer.Add(_headerContainer);
            mainContainer.Add(_contentContainer);
            
            _rootElement.Add(mainContainer);
        }
        
        /// <summary>
        /// Create the header section
        /// </summary>
        private void CreateHeader()
        {
            // Left section
            var leftSection = new VisualElement();
            leftSection.style.flexDirection = FlexDirection.Row;
            leftSection.style.alignItems = Align.Center;
            
            _titleLabel = new Label("Facility Management");
            _titleLabel.style.fontSize = 24;
            // _titleLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.marginRight = 20;
            
            // Facility selector
            _facilitySelector = new DropdownField("Select Facility", _availableFacilities, 0);
            _facilitySelector.style.minWidth = 200;
            _facilitySelector.style.marginRight = 20;
            
            // Overall status
            _overallStatus = new UIStatusIndicator(UIStatus.Success, "All Systems Operational");
            
            leftSection.Add(_titleLabel);
            leftSection.Add(_facilitySelector);
            leftSection.Add(_overallStatus);
            
            // Close button
            _closeButton = new Button();
            _closeButton.name = "facility-close-button";
            _closeButton.text = "✕";
            _closeButton.style.width = 40;
            _closeButton.style.height = 40;
            _closeButton.style.fontSize = 20;
            // _uiManager.ApplyDesignSystemStyle(_closeButton, UIStyleToken.SecondaryButton);
            
            _headerContainer.Add(leftSection);
            _headerContainer.Add(_closeButton);
        }
        
        /// <summary>
        /// Create environmental control panels
        /// </summary>
        private void CreateControlPanels()
        {
            CreateHVACControls();
            CreateLightingControls();
            CreateIrrigationControls();
            CreateAutomationControls();
        }
        
        /// <summary>
        /// Create HVAC control panel
        /// </summary>
        private void CreateHVACControls()
        {
            _hvacControls = CreateControlSection("HVAC System", "🌡️");
            
            // System toggle
            var systemContainer = new VisualElement();
            systemContainer.style.flexDirection = FlexDirection.Row;
            systemContainer.style.justifyContent = Justify.SpaceBetween;
            systemContainer.style.alignItems = Align.Center;
            systemContainer.style.marginBottom = 16;
            
            var systemLabel = new Label("System Enabled");
            // systemLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            
            _hvacSystemToggle = new Toggle();
            _hvacSystemToggle.value = true;
            
            systemContainer.Add(systemLabel);
            systemContainer.Add(_hvacSystemToggle);
            _hvacControls.Add(systemContainer);
            
            // Temperature control
            var tempContainer = CreateSliderControl("Temperature Setpoint", "°C", 22f, 15f, 35f);
            _temperatureSetpointSlider = tempContainer.Q<Slider>("control-slider");
            _hvacControls.Add(tempContainer);
            
            // Humidity control
            var humidityContainer = CreateSliderControl("Humidity Setpoint", "%", 60f, 30f, 90f);
            _humiditySetpointSlider = humidityContainer.Q<Slider>("control-slider");
            _hvacControls.Add(humidityContainer);
            
            // CO2 control
            var co2Container = CreateSliderControl("CO₂ Setpoint", "ppm", 1200f, 400f, 2000f);
            _co2SetpointSlider = co2Container.Q<Slider>("control-slider");
            _hvacControls.Add(co2Container);
            
            // Current readings
            var readingsContainer = new VisualElement();
            readingsContainer.name = "current-readings";
            readingsContainer.style.flexDirection = FlexDirection.Row;
            readingsContainer.style.justifyContent = Justify.SpaceBetween;
            readingsContainer.style.marginTop = 16;
            
            _currentTemperatureCard = new UIDataCard("Current Temp", "22.5", "°C");
            _currentTemperatureCard.style.width = 90;
            
            _currentHumidityCard = new UIDataCard("Current RH", "58", "%");
            _currentHumidityCard.style.width = 90;
            
            _currentCO2Card = new UIDataCard("Current CO₂", "1150", "ppm");
            _currentCO2Card.style.width = 90;
            
            readingsContainer.Add(_currentTemperatureCard);
            readingsContainer.Add(_currentHumidityCard);
            readingsContainer.Add(_currentCO2Card);
            _hvacControls.Add(readingsContainer);
            
            // Preset button
            _hvacPresetButton = new Button();
            _hvacPresetButton.text = "Load Presets";
            _hvacPresetButton.style.marginTop = 12;
            // _uiManager.ApplyDesignSystemStyle(_hvacPresetButton, UIStyleToken.SecondaryButton);
            _hvacControls.Add(_hvacPresetButton);
            
            _controlPanelContainer.Add(_hvacControls);
        }
        
        /// <summary>
        /// Create lighting control panel
        /// </summary>
        private void CreateLightingControls()
        {
            _lightingControls = CreateControlSection("Lighting System", "💡");
            
            // System toggle
            var systemContainer = new VisualElement();
            systemContainer.style.flexDirection = FlexDirection.Row;
            systemContainer.style.justifyContent = Justify.SpaceBetween;
            systemContainer.style.alignItems = Align.Center;
            systemContainer.style.marginBottom = 16;
            
            var systemLabel = new Label("Lighting Enabled");
            // systemLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            
            _lightingSystemToggle = new Toggle();
            _lightingSystemToggle.value = true;
            
            systemContainer.Add(systemLabel);
            systemContainer.Add(_lightingSystemToggle);
            _lightingControls.Add(systemContainer);
            
            // Light intensity
            var intensityContainer = CreateSliderControl("Light Intensity", "%", 85f, 0f, 100f);
            _lightIntensitySlider = intensityContainer.Q<Slider>("control-slider");
            _lightingControls.Add(intensityContainer);
            
            // Light spectrum
            var spectrumContainer = new VisualElement();
            spectrumContainer.style.marginBottom = 16;
            
            var spectrumLabel = new Label("Light Spectrum");
            // spectrumLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            spectrumLabel.style.marginBottom = 8;
            
            _lightSpectrumDropdown = new DropdownField(new List<string> { "Full Spectrum", "Vegetative", "Flowering", "Custom" }, 0);
            
            spectrumContainer.Add(spectrumLabel);
            spectrumContainer.Add(_lightSpectrumDropdown);
            _lightingControls.Add(spectrumContainer);
            
            // Schedule control
            var scheduleContainer = CreateSliderControl("Daily Duration", "hours", 18f, 12f, 24f);
            _lightScheduleSlider = scheduleContainer.Q<Slider>("control-slider");
            _lightingControls.Add(scheduleContainer);
            
            // Daily Light Integral progress
            _dailyLightIntegralBar = new UIProgressBar(60f);
            _dailyLightIntegralBar.Value = 42f;
            _dailyLightIntegralBar.Format = "DLI: {0:F1} mol/m²/day";
            // _dailyLightIntegralBar.SetColor(_uiManager.DesignSystem.ColorPalette.AccentGold);
            _dailyLightIntegralBar.style.marginTop = 16;
            _lightingControls.Add(_dailyLightIntegralBar);
            
            _controlPanelContainer.Add(_lightingControls);
        }
        
        /// <summary>
        /// Create irrigation control panel
        /// </summary>
        private void CreateIrrigationControls()
        {
            _irrigationControls = CreateControlSection("Irrigation System", "💧");
            
            // System toggle
            var systemContainer = new VisualElement();
            systemContainer.style.flexDirection = FlexDirection.Row;
            systemContainer.style.justifyContent = Justify.SpaceBetween;
            systemContainer.style.alignItems = Align.Center;
            systemContainer.style.marginBottom = 16;
            
            var systemLabel = new Label("Irrigation Enabled");
            // systemLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            
            _irrigationSystemToggle = new Toggle();
            _irrigationSystemToggle.value = true;
            
            systemContainer.Add(systemLabel);
            systemContainer.Add(_irrigationSystemToggle);
            _irrigationControls.Add(systemContainer);
            
            // Water flow rate
            var flowRateContainer = CreateSliderControl("Flow Rate", "L/min", 2.5f, 0.5f, 10f);
            _waterFlowRateSlider = flowRateContainer.Q<Slider>("control-slider");
            _irrigationControls.Add(flowRateContainer);
            
            // Nutrient concentration
            var nutrientContainer = CreateSliderControl("Nutrient EC", "mS/cm", 1.8f, 0.5f, 3.0f);
            _nutrientConcentrationSlider = nutrientContainer.Q<Slider>("control-slider");
            _irrigationControls.Add(nutrientContainer);
            
            // Irrigation schedule
            var scheduleContainer = new VisualElement();
            scheduleContainer.style.marginBottom = 16;
            
            var scheduleLabel = new Label("Schedule");
            // scheduleLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            scheduleLabel.style.marginBottom = 8;
            
            _irrigationScheduleDropdown = new DropdownField(new List<string> { "Every 4 hours", "Every 6 hours", "Daily", "Custom" }, 1);
            
            scheduleContainer.Add(scheduleLabel);
            scheduleContainer.Add(_irrigationScheduleDropdown);
            _irrigationControls.Add(scheduleContainer);
            
            // Water consumption and nutrient levels
            var statusContainer = new VisualElement();
            statusContainer.style.flexDirection = FlexDirection.Row;
            statusContainer.style.justifyContent = Justify.SpaceBetween;
            statusContainer.style.marginTop = 16;
            
            _waterConsumptionCard = new UIDataCard("Water Usage", "128", "L/day");
            _waterConsumptionCard.style.width = 120;
            
            _nutrientLevelsCard = new UIDataCard("Nutrient Level", "78", "%");
            _nutrientLevelsCard.style.width = 120;
            
            statusContainer.Add(_waterConsumptionCard);
            statusContainer.Add(_nutrientLevelsCard);
            _irrigationControls.Add(statusContainer);
            
            _controlPanelContainer.Add(_irrigationControls);
        }
        
        /// <summary>
        /// Create automation control panel
        /// </summary>
        private void CreateAutomationControls()
        {
            _automationControls = CreateControlSection("Automation System", "🤖");
            
            // Automation toggle
            var automationContainer = new VisualElement();
            automationContainer.style.flexDirection = FlexDirection.Row;
            automationContainer.style.justifyContent = Justify.SpaceBetween;
            automationContainer.style.alignItems = Align.Center;
            automationContainer.style.marginBottom = 16;
            
            var automationLabel = new Label("Automation Enabled");
            // automationLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            
            _automationEnabledToggle = new Toggle();
            _automationEnabledToggle.value = true;
            
            automationContainer.Add(automationLabel);
            automationContainer.Add(_automationEnabledToggle);
            _automationControls.Add(automationContainer);
            
            // Automation mode
            var modeContainer = new VisualElement();
            modeContainer.style.marginBottom = 16;
            
            var modeLabel = new Label("Automation Mode");
            // modeLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            modeLabel.style.marginBottom = 8;
            
            _automationModeDropdown = new DropdownField(new List<string> { "Basic", "Advanced", "AI Optimized", "Manual Override" }, 1);
            
            modeContainer.Add(modeLabel);
            modeContainer.Add(_automationModeDropdown);
            _automationControls.Add(modeContainer);
            
            // Automation status
            _automationStatus = new UIStatusIndicator(UIStatus.Success, "Automation Active");
            _automationStatus.style.marginBottom = 16;
            _automationControls.Add(_automationStatus);
            
            // Preset button
            _automationPresetButton = new Button();
            _automationPresetButton.text = "Configure Presets";
            // _uiManager.ApplyDesignSystemStyle(_automationPresetButton, UIStyleToken.SecondaryButton);
            _automationControls.Add(_automationPresetButton);
            
            _controlPanelContainer.Add(_automationControls);
        }
        
        /// <summary>
        /// Create control section container
        /// </summary>
        private VisualElement CreateControlSection(string title, string icon)
        {
            var section = new VisualElement();
            section.name = title.ToLower().Replace(" ", "-") + "-section";
            // section.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            section.style.borderTopLeftRadius = 12;
            section.style.borderTopRightRadius = 12;
            section.style.borderBottomLeftRadius = 12;
            section.style.borderBottomRightRadius = 12;
            section.style.paddingTop = 16;
            section.style.paddingBottom = 16;
            section.style.paddingLeft = 16;
            section.style.paddingRight = 16;
            section.style.marginBottom = 16;
            
            // Header
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 16;
            
            var iconLabel = new Label(icon);
            iconLabel.style.fontSize = 20;
            iconLabel.style.marginRight = 8;
            
            var titleLabel = new Label(title);
            titleLabel.style.fontSize = 16;
            // titleLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            header.Add(iconLabel);
            header.Add(titleLabel);
            section.Add(header);
            
            return section;
        }
        
        /// <summary>
        /// Create slider control
        /// </summary>
        private VisualElement CreateSliderControl(string label, string unit, float defaultValue, float min, float max)
        {
            var container = new VisualElement();
            container.style.marginBottom = 16;
            
            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.marginBottom = 8;
            
            var labelElement = new Label(label);
            // labelElement.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            
            var valueLabel = new Label($"{defaultValue:F1} {unit}");
            valueLabel.name = "value-label";
            // valueLabel.style.color = _uiManager.DesignSystem.ColorPalette.AccentGold;
            
            headerContainer.Add(labelElement);
            headerContainer.Add(valueLabel);
            
            var slider = new Slider(min, max);
            slider.name = "control-slider";
            slider.value = defaultValue;
            slider.RegisterCallback<ChangeEvent<float>>(evt => {
                valueLabel.text = $"{evt.newValue:F1} {unit}";
            });
            
            container.Add(headerContainer);
            container.Add(slider);
            
            return container;
        }
        
        /// <summary>
        /// Create monitoring displays
        /// </summary>
        private void CreateMonitoringDisplays()
        {
            // Environmental charts container
            _environmentalChartsContainer = new VisualElement();
            _environmentalChartsContainer.name = "environmental-charts";
            _environmentalChartsContainer.style.flexDirection = FlexDirection.Column;
            
            // Charts grid
            var chartsGrid = new VisualElement();
            chartsGrid.style.flexDirection = FlexDirection.Row;
            chartsGrid.style.flexWrap = Wrap.Wrap;
            chartsGrid.style.justifyContent = Justify.SpaceBetween;
            
            // Temperature chart
            _temperatureChart = new UISimpleChart("Temperature Trend");
            _temperatureChart.style.width = Length.Percent(48);
            _temperatureChart.style.marginBottom = 16;
            _temperatureChart.SetRange(15f, 35f);
            // _temperatureChart.LineColor = _uiManager.DesignSystem.ColorPalette.Error;
            
            // Humidity chart
            _humidityChart = new UISimpleChart("Humidity Trend");
            _humidityChart.style.width = Length.Percent(48);
            _humidityChart.style.marginBottom = 16;
            _humidityChart.SetRange(30f, 90f);
            // _humidityChart.LineColor = _uiManager.DesignSystem.ColorPalette.Info;
            
            // CO2 chart
            _co2Chart = new UISimpleChart("CO₂ Trend");
            _co2Chart.style.width = Length.Percent(48);
            _co2Chart.style.marginBottom = 16;
            _co2Chart.SetRange(400f, 2000f);
            // _co2Chart.LineColor = _uiManager.DesignSystem.ColorPalette.Success;
            
            // Power consumption chart
            _powerConsumptionChart = new UISimpleChart("Power Consumption");
            _powerConsumptionChart.style.width = Length.Percent(48);
            _powerConsumptionChart.style.marginBottom = 16;
            _powerConsumptionChart.SetRange(0f, 5000f);
            // _powerConsumptionChart.LineColor = _uiManager.DesignSystem.ColorPalette.Warning;
            
            // Sample data
            PopulateChartsWithSampleData();
            
            chartsGrid.Add(_temperatureChart);
            chartsGrid.Add(_humidityChart);
            chartsGrid.Add(_co2Chart);
            chartsGrid.Add(_powerConsumptionChart);
            
            _environmentalChartsContainer.Add(chartsGrid);
            _monitoringContainer.Add(_environmentalChartsContainer);
        }
        
        /// <summary>
        /// Create alerts section
        /// </summary>
        private void CreateAlertsSection()
        {
            _alertsContainer = new VisualElement();
            _alertsContainer.name = "alerts-container";
            _alertsContainer.style.marginTop = 20;
            // _alertsContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            _alertsContainer.style.borderTopLeftRadius = 12;
            _alertsContainer.style.borderTopRightRadius = 12;
            _alertsContainer.style.borderBottomLeftRadius = 12;
            _alertsContainer.style.borderBottomRightRadius = 12;
            _alertsContainer.style.paddingTop = 16;
            _alertsContainer.style.paddingBottom = 16;
            _alertsContainer.style.paddingLeft = 16;
            _alertsContainer.style.paddingRight = 16;
            
            var alertsTitle = new Label("System Alerts");
            alertsTitle.style.fontSize = 16;
            // alertsTitle.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            alertsTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            alertsTitle.style.marginBottom = 12;
            
            _alertsContainer.Add(alertsTitle);
            
            // Sample alerts
            AddSampleAlerts();
            
            _monitoringContainer.Add(_alertsContainer);
        }
        
        /// <summary>
        /// Load facility data
        /// </summary>
        private void LoadFacilityData()
        {
            // This would integrate with facility manager
            _availableFacilities.Clear();
            _availableFacilities.AddRange(new[] { "Greenhouse A", "Greenhouse B", "Indoor Facility 1", "Outdoor Plot 1" });
            
            if (_availableFacilities.Count > 0)
            {
                _selectedFacilityId = _availableFacilities[0];
            }
        }
        
        /// <summary>
        /// Populate charts with sample data
        /// </summary>
        private void PopulateChartsWithSampleData()
        {
            // Temperature data (21-25°C)
            var tempData = new List<float> { 22.5f, 23.1f, 22.8f, 23.5f, 24.2f, 23.9f, 23.2f, 22.7f };
            _temperatureChart.SetData(tempData);
            
            // Humidity data (55-65%)
            var humidityData = new List<float> { 58f, 62f, 59f, 61f, 63f, 60f, 57f, 59f };
            _humidityChart.SetData(humidityData);
            
            // CO2 data (1000-1400 ppm)
            var co2Data = new List<float> { 1200f, 1150f, 1300f, 1250f, 1180f, 1220f, 1280f, 1240f };
            _co2Chart.SetData(co2Data);
            
            // Power consumption data (2000-4500W)
            var powerData = new List<float> { 3200f, 3800f, 4200f, 4100f, 3900f, 3500f, 3100f, 3400f };
            _powerConsumptionChart.SetData(powerData);
        }
        
        /// <summary>
        /// Add sample alerts
        /// </summary>
        private void AddSampleAlerts()
        {
            var alertContainer = new VisualElement();
            alertContainer.style.flexDirection = FlexDirection.Column;
            
            // High humidity alert
            var humidityAlert = new UIStatusIndicator(UIStatus.Warning, "Humidity levels elevated in Zone 2");
            humidityAlert.style.marginBottom = 8;
            alertContainer.Add(humidityAlert);
            
            // Nutrient level alert
            var nutrientAlert = new UIStatusIndicator(UIStatus.Info, "Nutrient reservoir requires refill within 24 hours");
            nutrientAlert.style.marginBottom = 8;
            alertContainer.Add(nutrientAlert);
            
            // Filter replacement alert
            var filterAlert = new UIStatusIndicator(UIStatus.Warning, "HEPA filter replacement recommended");
            alertContainer.Add(filterAlert);
            
            _alertsContainer.Add(alertContainer);
        }
        
        /// <summary>
        /// Start data update cycle
        /// </summary>
        private void StartDataUpdates()
        {
            if (_enableRealTimeMonitoring)
            {
                InvokeRepeating(nameof(UpdateEnvironmentalData), 0f, _dataUpdateInterval);
            }
        }
        
        /// <summary>
        /// Update environmental data from managers
        /// </summary>
        private void UpdateEnvironmentalData()
        {
            // if (_environmentalManager == null) return;
            
            // Update current readings
            UpdateCurrentReadings();
            
            // Update charts with new data points
            UpdateChartsData();
            
            // Update system status
            UpdateSystemStatus();
        }
        
        /// <summary>
        /// Update current environmental readings
        /// </summary>
        private void UpdateCurrentReadings()
        {
            // This would get real data from environmental manager
            var currentTemp = 23.2f + Random.Range(-0.5f, 0.5f);
            var currentHumidity = 59f + Random.Range(-2f, 2f);
            var currentCO2 = 1180f + Random.Range(-50f, 50f);
            
            _currentTemperatureCard.Value = currentTemp.ToString("F1");
            _currentHumidityCard.Value = currentHumidity.ToString("F0");
            _currentCO2Card.Value = currentCO2.ToString("F0");
            
            // Update color based on setpoints
            UpdateReadingColors(currentTemp, currentHumidity, currentCO2);
        }
        
        /// <summary>
        /// Update reading colors based on setpoints
        /// </summary>
        private void UpdateReadingColors(float temp, float humidity, float co2)
        {
            var tempSetpoint = _temperatureSetpointSlider?.value ?? 22f;
            var humiditySetpoint = _humiditySetpointSlider?.value ?? 60f;
            var co2Setpoint = _co2SetpointSlider?.value ?? 1200f;
            
            // Temperature color
            var tempDiff = Mathf.Abs(temp - tempSetpoint);
            // if (tempDiff < 1f)
                // _currentTemperatureCard.SetValueColor(_uiManager.DesignSystem.ColorPalette.Success);
            // else if (tempDiff < 2f)
                // _currentTemperatureCard.SetValueColor(_uiManager.DesignSystem.ColorPalette.Warning);
            // else
                // _currentTemperatureCard.SetValueColor(_uiManager.DesignSystem.ColorPalette.Error);
            
            // Humidity color
            var humidityDiff = Mathf.Abs(humidity - humiditySetpoint);
            // if (humidityDiff < 3f)
                // _currentHumidityCard.SetValueColor(_uiManager.DesignSystem.ColorPalette.Success);
            // else if (humidityDiff < 5f)
                // _currentHumidityCard.SetValueColor(_uiManager.DesignSystem.ColorPalette.Warning);
            // else
                // _currentHumidityCard.SetValueColor(_uiManager.DesignSystem.ColorPalette.Error);
            
            // CO2 color
            var co2Diff = Mathf.Abs(co2 - co2Setpoint);
            // if (co2Diff < 50f)
                // _currentCO2Card.SetValueColor(_uiManager.DesignSystem.ColorPalette.Success);
            // else if (co2Diff < 100f)
                // _currentCO2Card.SetValueColor(_uiManager.DesignSystem.ColorPalette.Warning);
            // else
                // _currentCO2Card.SetValueColor(_uiManager.DesignSystem.ColorPalette.Error);
        }
        
        /// <summary>
        /// Update charts with new data
        /// </summary>
        private void UpdateChartsData()
        {
            // Add new data points to charts (simplified)
            // In a real implementation, this would add actual time-series data
        }
        
        /// <summary>
        /// Update system status indicators
        /// </summary>
        private void UpdateSystemStatus()
        {
            // Update overall status based on system health
            var allSystemsOperational = _hvacSystemToggle.value && _lightingSystemToggle.value && _irrigationSystemToggle.value;
            
            if (allSystemsOperational)
            {
                _overallStatus.Status = UIStatus.Success;
                _overallStatus.Label = "All Systems Operational";
            }
            // else
            // {
                _overallStatus.Status = UIStatus.Warning;
                _overallStatus.Label = "Some Systems Offline";
            // }
            
            // Update automation status
            if (_automationEnabledToggle.value)
            {
                _automationStatus.Status = UIStatus.Success;
                _automationStatus.Label = "Automation Active";
            }
            // else
            // {
                _automationStatus.Status = UIStatus.Info;
                _automationStatus.Label = "Manual Control";
            // }
        }
        
        // Event handlers
        private void OnCloseClicked(ClickEvent evt)
        {
            Hide();
        }
        
        private void OnFacilityChanged(ChangeEvent<string> evt)
        {
            _selectedFacilityId = evt.newValue;
            LoadFacilitySettings();
            _onFacilityButtonClicked?.RaiseButtonClick("facility-changed", PanelId, Vector2.zero);
        }
        
        private void OnHVACSystemToggled(ChangeEvent<bool> evt)
        {
            // Handle HVAC system toggle
            _onEnvironmentalControlChanged?.Raise();
            LogInfo($"HVAC system {(evt.newValue ? "enabled" : "disabled")}");
        }
        
        private void OnHVACPresetClicked(ClickEvent evt)
        {
            // Show HVAC preset selection dialog
            _onFacilityButtonClicked?.RaiseButtonClick("hvac-preset", PanelId, evt.position);
        }
        
        private void OnLightingSystemToggled(ChangeEvent<bool> evt)
        {
            _onEnvironmentalControlChanged?.Raise();
            LogInfo($"Lighting system {(evt.newValue ? "enabled" : "disabled")}");
        }
        
        private void OnLightSpectrumChanged(ChangeEvent<string> evt)
        {
            _onEnvironmentalControlChanged?.Raise();
            LogInfo($"Light spectrum changed to: {evt.newValue}");
        }
        
        private void OnIrrigationSystemToggled(ChangeEvent<bool> evt)
        {
            _onEnvironmentalControlChanged?.Raise();
            LogInfo($"Irrigation system {(evt.newValue ? "enabled" : "disabled")}");
        }
        
        private void OnIrrigationScheduleChanged(ChangeEvent<string> evt)
        {
            _onEnvironmentalControlChanged?.Raise();
            LogInfo($"Irrigation schedule changed to: {evt.newValue}");
        }
        
        private void OnAutomationToggled(ChangeEvent<bool> evt)
        {
            _onEnvironmentalControlChanged?.Raise();
            LogInfo($"Automation {(evt.newValue ? "enabled" : "disabled")}");
        }
        
        private void OnAutomationModeChanged(ChangeEvent<string> evt)
        {
            _onEnvironmentalControlChanged?.Raise();
            LogInfo($"Automation mode changed to: {evt.newValue}");
        }
        
        private void OnAutomationPresetClicked(ClickEvent evt)
        {
            _onFacilityButtonClicked?.RaiseButtonClick("automation-preset", PanelId, evt.position);
        }
        
        /// <summary>
        /// Load settings for selected facility
        /// </summary>
        private void LoadFacilitySettings()
        {
            // This would load facility-specific settings from the facility manager
            LogInfo($"Loading settings for facility: {_selectedFacilityId}");
        }
        
        protected override void OnAfterShow()
        {
            base.OnAfterShow();
            LoadFacilityData();
            UpdateEnvironmentalData();
        }
        
        protected virtual void OnDestroy()
        {
            // Clean up facility management panel
            CancelInvoke(nameof(UpdateEnvironmentalData));
        }
    }
    
    /// <summary>
    /// Facility alert data structure
    /// </summary>
    [System.Serializable]
    public struct FacilityAlert
    {
        public string AlertId;
        public string Message;
        public UIStatus Severity;
        public float Timestamp;
        
        public FacilityAlert(string alertId, string message, UIStatus severity)
        {
            AlertId = alertId;
            Message = message;
            Severity = severity;
            Timestamp = Time.time;
        }
    }
}