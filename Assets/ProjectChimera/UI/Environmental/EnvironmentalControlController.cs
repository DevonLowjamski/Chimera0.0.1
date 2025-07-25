using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
// using ProjectChimera.Systems.Environment;
// using ProjectChimera.Systems.Automation;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.UI;

namespace ProjectChimera.UI.Environmental
{
    /// <summary>
    /// Environmental Control UI Controller for Project Chimera.
    /// Provides intuitive, game-like controls for HVAC, lighting, and environmental systems.
    /// Features real-time monitoring, automated scheduling, and optimization tools.
    /// </summary>
    public class EnvironmentalControlController : MonoBehaviour
    {
        [Header("Environmental Control Configuration")]
        [SerializeField] private UIDocument _environmentalDocument;
        [SerializeField] private EnvironmentalControlSettings _controlSettings;
        [SerializeField] private bool _enableRealTimeUpdates = true;
        [SerializeField] private float _updateInterval = 2f;
        
        [Header("Zone Configuration")]
        [SerializeField] private List<string> _managedZones = new List<string> 
        { 
            "VegetativeRoom", "FloweringRoom", "DryingRoom", "NurseryRoom", "MotherRoom" 
        };
        [SerializeField] private string _selectedZone = "VegetativeRoom";
        
        [Header("Control Limits")]
        [SerializeField] private Vector2 _temperatureRange = new Vector2(15f, 35f);
        [SerializeField] private Vector2 _humidityRange = new Vector2(30f, 80f);
        [SerializeField] private Vector2 _co2Range = new Vector2(400f, 1800f);
        [SerializeField] private Vector2 _lightIntensityRange = new Vector2(0f, 1000f);
        
        [Header("Audio Feedback")]
        [SerializeField] private AudioClip _controlAdjustSound;
        [SerializeField] private AudioClip _zoneChangeSound;
        [SerializeField] private AudioClip _alertSound;
        [SerializeField] private AudioClip _notificationSound;
        [SerializeField] private AudioSource _audioSource;
        
        // System references
        private object _hvacManager; // Placeholder - will be replaced with actual HVACManager when assembly references are fixed
        // private LightingManager _lightingManager;
        // private AutomationManager _automationManager;
        
        // UI Elements - Main Controls
        private VisualElement _rootElement;
        private DropdownField _zoneSelector;
        private Label _selectedZoneLabel;
        private Button _optimizeZoneButton;
        private Button _copySettingsButton;
        private Button _emergencyStopButton;
        
        // Climate Control Elements
        private Slider _temperatureSlider;
        private FloatField _temperatureField;
        private Label _currentTemperatureLabel;
        private Button _temperatureAutoButton;
        private ProgressBar _temperatureStabilityBar;
        
        private Slider _humiditySlider;
        private FloatField _humidityField;
        private Label _currentHumidityLabel;
        private Button _humidityAutoButton;
        private ProgressBar _humidityStabilityBar;
        
        private Slider _co2Slider;
        private FloatField _co2Field;
        private Label _currentCo2Label;
        private Button _co2AutoButton;
        private ProgressBar _co2StabilityBar;
        
        // Lighting Control Elements
        private Slider _lightIntensitySlider;
        private FloatField _lightIntensityField;
        private Label _currentLightLabel;
        private Toggle _lightingOnToggle;
        private DropdownField _lightingModeDropdown;
        private Slider _photoperiodSlider;
        private Label _photoperiodLabel;
        private ProgressBar _dliProgressBar;
        
        // Equipment Status Elements
        private VisualElement _hvacStatusPanel;
        private VisualElement _lightingStatusPanel;
        private VisualElement _fanStatusPanel;
        private VisualElement _co2StatusPanel;
        
        // Scheduling Elements
        private VisualElement _schedulePanel;
        private Button _addScheduleButton;
        private VisualElement _scheduleList;
        private Toggle _automationEnabledToggle;
        
        // Advanced Controls
        private VisualElement _advancedPanel;
        private Slider _vpnSlider;
        private Label _vpnLabel;
        private Toggle _circulationFanToggle;
        private Toggle _exhaustFanToggle;
        private Slider _airflowSlider;
        
        // Data and State
        private Dictionary<string, EnvironmentalZoneData> _zoneData = new Dictionary<string, EnvironmentalZoneData>();
        private Dictionary<string, bool> _automationStates = new Dictionary<string, bool>();
        private List<EnvironmentalSchedule> _activeSchedules = new List<EnvironmentalSchedule>();
        private float _lastUpdateTime;
        private bool _isUpdating = false;
        
        // Events
        public System.Action<string> OnZoneChanged;
        public System.Action<string, float> OnParameterChanged;
        public System.Action<EnvironmentalSchedule> OnScheduleCreated;
        public System.Action<string> OnEmergencyAction;
        
        private void Start()
        {
            InitializeController();
            InitializeSystemReferences();
            SetupUIElements();
            SetupEventHandlers();
            LoadZoneData();
            
            if (_enableRealTimeUpdates)
            {
                InvokeRepeating(nameof(UpdateEnvironmentalData), 1f, _updateInterval);
            }
        }
        
        private void InitializeController()
        {
            if (_environmentalDocument == null)
            {
                Debug.LogError("Environmental UI Document not assigned!");
                return;
            }
            
            _rootElement = _environmentalDocument.rootVisualElement;
            _lastUpdateTime = Time.time;
            
            // Initialize zone data
            foreach (var zone in _managedZones)
            {
                _zoneData[zone] = new EnvironmentalZoneData
                {
                    ZoneId = zone,
                    ZoneName = GetFriendlyZoneName(zone),
                    Temperature = 22f,
                    Humidity = 55f,
                    CO2Level = 1000f,
                    LightIntensity = 400f,
                    IsLightingOn = true,
                    LightingMode = "Vegetative",
                    PhotoperiodHours = 18f
                };
                _automationStates[zone] = true;
            }
            
            Debug.Log("Environmental Control Controller initialized");
        }
        
        private void InitializeSystemReferences()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("GameManager not found - using simulation mode");
                return;
            }
            
            // _hvacManager = gameManager.GetManager<HVACManager>();
            // _lightingManager = gameManager.GetManager<LightingManager>();
            // _automationManager = gameManager.GetManager<AutomationManager>();
            
            Debug.Log($"Environmental Control connected to available systems");
        }
        
        private void SetupUIElements()
        {
            // Main control elements
            _zoneSelector = _rootElement.Q<DropdownField>("zone-selector");
            _selectedZoneLabel = _rootElement.Q<Label>("selected-zone-label");
            _optimizeZoneButton = _rootElement.Q<Button>("optimize-zone-btn");
            _copySettingsButton = _rootElement.Q<Button>("copy-settings-btn");
            _emergencyStopButton = _rootElement.Q<Button>("emergency-stop-btn");
            
            // Temperature controls
            _temperatureSlider = _rootElement.Q<Slider>("temperature-slider");
            _temperatureField = _rootElement.Q<FloatField>("temperature-field");
            _currentTemperatureLabel = _rootElement.Q<Label>("current-temperature");
            _temperatureAutoButton = _rootElement.Q<Button>("temperature-auto-btn");
            _temperatureStabilityBar = _rootElement.Q<ProgressBar>("temperature-stability");
            
            // Humidity controls
            _humiditySlider = _rootElement.Q<Slider>("humidity-slider");
            _humidityField = _rootElement.Q<FloatField>("humidity-field");
            _currentHumidityLabel = _rootElement.Q<Label>("current-humidity");
            _humidityAutoButton = _rootElement.Q<Button>("humidity-auto-btn");
            _humidityStabilityBar = _rootElement.Q<ProgressBar>("humidity-stability");
            
            // CO2 controls
            _co2Slider = _rootElement.Q<Slider>("co2-slider");
            _co2Field = _rootElement.Q<FloatField>("co2-field");
            _currentCo2Label = _rootElement.Q<Label>("current-co2");
            _co2AutoButton = _rootElement.Q<Button>("co2-auto-btn");
            _co2StabilityBar = _rootElement.Q<ProgressBar>("co2-stability");
            
            // Lighting controls
            _lightIntensitySlider = _rootElement.Q<Slider>("light-intensity-slider");
            _lightIntensityField = _rootElement.Q<FloatField>("light-intensity-field");
            _currentLightLabel = _rootElement.Q<Label>("current-light");
            _lightingOnToggle = _rootElement.Q<Toggle>("lighting-on-toggle");
            _lightingModeDropdown = _rootElement.Q<DropdownField>("lighting-mode-dropdown");
            _photoperiodSlider = _rootElement.Q<Slider>("photoperiod-slider");
            _photoperiodLabel = _rootElement.Q<Label>("photoperiod-label");
            _dliProgressBar = _rootElement.Q<ProgressBar>("dli-progress");
            
            // Equipment status panels
            _hvacStatusPanel = _rootElement.Q<VisualElement>("hvac-status-panel");
            _lightingStatusPanel = _rootElement.Q<VisualElement>("lighting-status-panel");
            _fanStatusPanel = _rootElement.Q<VisualElement>("fan-status-panel");
            _co2StatusPanel = _rootElement.Q<VisualElement>("co2-status-panel");
            
            // Scheduling
            _schedulePanel = _rootElement.Q<VisualElement>("schedule-panel");
            _addScheduleButton = _rootElement.Q<Button>("add-schedule-btn");
            _scheduleList = _rootElement.Q<VisualElement>("schedule-list");
            _automationEnabledToggle = _rootElement.Q<Toggle>("automation-enabled-toggle");
            
            // Advanced controls
            _advancedPanel = _rootElement.Q<VisualElement>("advanced-panel");
            _vpnSlider = _rootElement.Q<Slider>("vpd-slider");
            _vpnLabel = _rootElement.Q<Label>("vpd-label");
            _circulationFanToggle = _rootElement.Q<Toggle>("circulation-fan-toggle");
            _exhaustFanToggle = _rootElement.Q<Toggle>("exhaust-fan-toggle");
            _airflowSlider = _rootElement.Q<Slider>("airflow-slider");
            
            SetupControlRanges();
            SetupDropdownOptions();
        }
        
        private void SetupControlRanges()
        {
            // Set slider ranges
            if (_temperatureSlider != null)
            {
                _temperatureSlider.lowValue = _temperatureRange.x;
                _temperatureSlider.highValue = _temperatureRange.y;
                _temperatureSlider.value = 22f;
            }
            
            if (_humiditySlider != null)
            {
                _humiditySlider.lowValue = _humidityRange.x;
                _humiditySlider.highValue = _humidityRange.y;
                _humiditySlider.value = 55f;
            }
            
            if (_co2Slider != null)
            {
                _co2Slider.lowValue = _co2Range.x;
                _co2Slider.highValue = _co2Range.y;
                _co2Slider.value = 1000f;
            }
            
            if (_lightIntensitySlider != null)
            {
                _lightIntensitySlider.lowValue = _lightIntensityRange.x;
                _lightIntensitySlider.highValue = _lightIntensityRange.y;
                _lightIntensitySlider.value = 400f;
            }
            
            if (_photoperiodSlider != null)
            {
                _photoperiodSlider.lowValue = 8f;
                _photoperiodSlider.highValue = 24f;
                _photoperiodSlider.value = 18f;
            }
            
            if (_vpnSlider != null)
            {
                _vpnSlider.lowValue = 0.4f;
                _vpnSlider.highValue = 1.6f;
                _vpnSlider.value = 1.0f;
            }
            
            if (_airflowSlider != null)
            {
                _airflowSlider.lowValue = 0f;
                _airflowSlider.highValue = 100f;
                _airflowSlider.value = 75f;
            }
        }
        
        private void SetupDropdownOptions()
        {
            // Zone selector
            if (_zoneSelector != null)
            {
                _zoneSelector.choices = _managedZones.Select(GetFriendlyZoneName).ToList();
                _zoneSelector.value = GetFriendlyZoneName(_selectedZone);
            }
            
            // Lighting mode dropdown
            if (_lightingModeDropdown != null)
            {
                _lightingModeDropdown.choices = new List<string>
                {
                    "Seedling", "Vegetative", "Flowering", "Drying", "Custom"
                };
                _lightingModeDropdown.value = "Vegetative";
            }
        }
        
        private void SetupEventHandlers()
        {
            // Zone selection
            _zoneSelector?.RegisterValueChangedCallback(evt => OnZoneSelectionChanged(evt.newValue));
            
            // Main action buttons
            _optimizeZoneButton?.RegisterCallback<ClickEvent>(evt => OptimizeCurrentZone());
            _copySettingsButton?.RegisterCallback<ClickEvent>(evt => CopySettingsToOtherZones());
            _emergencyStopButton?.RegisterCallback<ClickEvent>(evt => EmergencyStopAll());
            
            // Temperature controls
            _temperatureSlider?.RegisterValueChangedCallback(evt => OnTemperatureChanged(evt.newValue));
            _temperatureField?.RegisterValueChangedCallback(evt => OnTemperatureFieldChanged(evt.newValue));
            _temperatureAutoButton?.RegisterCallback<ClickEvent>(evt => ToggleTemperatureAuto());
            
            // Humidity controls
            _humiditySlider?.RegisterValueChangedCallback(evt => OnHumidityChanged(evt.newValue));
            _humidityField?.RegisterValueChangedCallback(evt => OnHumidityFieldChanged(evt.newValue));
            _humidityAutoButton?.RegisterCallback<ClickEvent>(evt => ToggleHumidityAuto());
            
            // CO2 controls
            _co2Slider?.RegisterValueChangedCallback(evt => OnCO2Changed(evt.newValue));
            _co2Field?.RegisterValueChangedCallback(evt => OnCO2FieldChanged(evt.newValue));
            _co2AutoButton?.RegisterCallback<ClickEvent>(evt => ToggleCO2Auto());
            
            // Lighting controls
            _lightIntensitySlider?.RegisterValueChangedCallback(evt => OnLightIntensityChanged(evt.newValue));
            _lightIntensityField?.RegisterValueChangedCallback(evt => OnLightIntensityFieldChanged(evt.newValue));
            _lightingOnToggle?.RegisterValueChangedCallback(evt => OnLightingToggled(evt.newValue));
            _lightingModeDropdown?.RegisterValueChangedCallback(evt => OnLightingModeChanged(evt.newValue));
            _photoperiodSlider?.RegisterValueChangedCallback(evt => OnPhotoperiodChanged(evt.newValue));
            
            // Advanced controls
            _vpnSlider?.RegisterValueChangedCallback(evt => OnVPDChanged(evt.newValue));
            _circulationFanToggle?.RegisterValueChangedCallback(evt => OnCirculationFanToggled(evt.newValue));
            _exhaustFanToggle?.RegisterValueChangedCallback(evt => OnExhaustFanToggled(evt.newValue));
            _airflowSlider?.RegisterValueChangedCallback(evt => OnAirflowChanged(evt.newValue));
            
            // Scheduling
            _addScheduleButton?.RegisterCallback<ClickEvent>(evt => CreateNewSchedule());
            _automationEnabledToggle?.RegisterValueChangedCallback(evt => OnAutomationToggled(evt.newValue));
        }
        
        #region Event Handlers
        
        private void OnZoneSelectionChanged(string friendlyZoneName)
        {
            string zoneId = GetZoneIdFromFriendlyName(friendlyZoneName);
            if (zoneId != _selectedZone)
            {
                _selectedZone = zoneId;
                LoadZoneSettings();
                PlaySound(_zoneChangeSound);
                OnZoneChanged?.Invoke(zoneId);
            }
        }
        
        private void OnTemperatureChanged(float value)
        {
            if (_isUpdating) return;
            
            UpdateZoneData(_selectedZone, zd => zd.TargetTemperature = value);
            UpdateTemperatureField(value);
            ApplyTemperatureChange(value);
            PlaySound(_controlAdjustSound);
            OnParameterChanged?.Invoke("temperature", value);
        }
        
        private void OnTemperatureFieldChanged(float value)
        {
            if (_isUpdating) return;
            
            value = Mathf.Clamp(value, _temperatureRange.x, _temperatureRange.y);
            UpdateZoneData(_selectedZone, zd => zd.TargetTemperature = value);
            UpdateTemperatureSlider(value);
            ApplyTemperatureChange(value);
        }
        
        private void OnHumidityChanged(float value)
        {
            if (_isUpdating) return;
            
            UpdateZoneData(_selectedZone, zd => zd.TargetHumidity = value);
            UpdateHumidityField(value);
            ApplyHumidityChange(value);
            PlaySound(_controlAdjustSound);
            OnParameterChanged?.Invoke("humidity", value);
        }
        
        private void OnHumidityFieldChanged(float value)
        {
            if (_isUpdating) return;
            
            value = Mathf.Clamp(value, _humidityRange.x, _humidityRange.y);
            UpdateZoneData(_selectedZone, zd => zd.TargetHumidity = value);
            UpdateHumiditySlider(value);
            ApplyHumidityChange(value);
        }
        
        private void OnCO2Changed(float value)
        {
            if (_isUpdating) return;
            
            UpdateZoneData(_selectedZone, zd => zd.TargetCO2 = value);
            UpdateCO2Field(value);
            ApplyCO2Change(value);
            PlaySound(_controlAdjustSound);
            OnParameterChanged?.Invoke("co2", value);
        }
        
        private void OnCO2FieldChanged(float value)
        {
            if (_isUpdating) return;
            
            value = Mathf.Clamp(value, _co2Range.x, _co2Range.y);
            UpdateZoneData(_selectedZone, zd => zd.TargetCO2 = value);
            UpdateCO2Slider(value);
            ApplyCO2Change(value);
        }
        
        private void OnLightIntensityChanged(float value)
        {
            if (_isUpdating) return;
            
            UpdateZoneData(_selectedZone, zd => zd.TargetLightIntensity = value);
            UpdateLightIntensityField(value);
            ApplyLightIntensityChange(value);
            PlaySound(_controlAdjustSound);
            OnParameterChanged?.Invoke("light_intensity", value);
        }
        
        private void OnLightIntensityFieldChanged(float value)
        {
            if (_isUpdating) return;
            
            value = Mathf.Clamp(value, _lightIntensityRange.x, _lightIntensityRange.y);
            UpdateZoneData(_selectedZone, zd => zd.TargetLightIntensity = value);
            UpdateLightIntensitySlider(value);
            ApplyLightIntensityChange(value);
        }
        
        private void OnLightingToggled(bool isOn)
        {
            if (_isUpdating) return;
            
            UpdateZoneData(_selectedZone, zd => zd.IsLightingOn = isOn);
            ApplyLightingToggle(isOn);
            PlaySound(_controlAdjustSound);
            OnParameterChanged?.Invoke("lighting_on", isOn ? 1f : 0f);
        }
        
        private void OnLightingModeChanged(string mode)
        {
            if (_isUpdating) return;
            
            UpdateZoneData(_selectedZone, zd => zd.LightingMode = mode);
            ApplyLightingModePreset(mode);
            PlaySound(_controlAdjustSound);
        }
        
        private void OnPhotoperiodChanged(float hours)
        {
            if (_isUpdating) return;
            
            UpdateZoneData(_selectedZone, zd => zd.PhotoperiodHours = hours);
            UpdatePhotoperiodLabel(hours);
            ApplyPhotoperiodChange(hours);
            PlaySound(_controlAdjustSound);
            OnParameterChanged?.Invoke("photoperiod", hours);
        }
        
        private void OnVPDChanged(float value)
        {
            if (_isUpdating) return;
            
            UpdateZoneData(_selectedZone, zd => zd.TargetVPD = value);
            UpdateVPDLabel(value);
            ApplyVPDChange(value);
            PlaySound(_controlAdjustSound);
            OnParameterChanged?.Invoke("vpd", value);
        }
        
        private void OnCirculationFanToggled(bool isOn)
        {
            UpdateZoneData(_selectedZone, zd => zd.CirculationFanOn = isOn);
            ApplyCirculationFanChange(isOn);
            PlaySound(_controlAdjustSound);
        }
        
        private void OnExhaustFanToggled(bool isOn)
        {
            UpdateZoneData(_selectedZone, zd => zd.ExhaustFanOn = isOn);
            ApplyExhaustFanChange(isOn);
            PlaySound(_controlAdjustSound);
        }
        
        private void OnAirflowChanged(float value)
        {
            if (_isUpdating) return;
            
            UpdateZoneData(_selectedZone, zd => zd.AirflowRate = value);
            ApplyAirflowChange(value);
            PlaySound(_controlAdjustSound);
            OnParameterChanged?.Invoke("airflow", value);
        }
        
        private void OnAutomationToggled(bool enabled)
        {
            _automationStates[_selectedZone] = enabled;
            UpdateAutomationState(enabled);
        }
        
        #endregion
        
        #region Control Application Methods
        
        private void ApplyTemperatureChange(float temperature)
        {
            // if (_hvacManager != null)
            // {
                // Would integrate with actual HVAC system
                Debug.Log($"Setting temperature to {temperature}°C in {_selectedZone}");
            // }
            
            // Update current temperature with some simulation
            UpdateZoneData(_selectedZone, zd => 
            {
                float diff = temperature - zd.Temperature;
                zd.Temperature += diff * 0.1f; // Gradual change simulation
            });
        }
        
        private void ApplyHumidityChange(float humidity)
        {
            // if (_hvacManager != null)
            // {
                Debug.Log($"Setting humidity to {humidity}% in {_selectedZone}");
            // }
            
            UpdateZoneData(_selectedZone, zd => 
            {
                float diff = humidity - zd.Humidity;
                zd.Humidity += diff * 0.1f;
            });
        }
        
        private void ApplyCO2Change(float co2)
        {
            Debug.Log($"Setting CO2 to {co2} ppm in {_selectedZone}");
            
            UpdateZoneData(_selectedZone, zd => 
            {
                float diff = co2 - zd.CO2Level;
                zd.CO2Level += diff * 0.15f;
            });
        }
        
        private void ApplyLightIntensityChange(float intensity)
        {
            // if (_lightingManager != null)
            // {
                Debug.Log($"Setting light intensity to {intensity} PPFD in {_selectedZone}");
            // }
            
            UpdateZoneData(_selectedZone, zd => zd.LightIntensity = intensity);
        }
        
        private void ApplyLightingToggle(bool isOn)
        {
            // if (_lightingManager != null)
            // {
                Debug.Log($"{(isOn ? "Enabling" : "Disabling")} lighting in {_selectedZone}");
            // }
            
            UpdateZoneData(_selectedZone, zd => 
            {
                zd.IsLightingOn = isOn;
                if (!isOn) zd.LightIntensity = 0f;
            });
        }
        
        private void ApplyLightingModePreset(string mode)
        {
            var preset = GetLightingPreset(mode);
            if (preset != null)
            {
                _isUpdating = true;
                
                _lightIntensitySlider.value = preset.Intensity;
                _photoperiodSlider.value = preset.PhotoperiodHours;
                
                UpdateZoneData(_selectedZone, zd => 
                {
                    zd.TargetLightIntensity = preset.Intensity;
                    zd.PhotoperiodHours = preset.PhotoperiodHours;
                    zd.LightingMode = mode;
                });
                
                _isUpdating = false;
                
                UpdatePhotoperiodLabel(preset.PhotoperiodHours);
                Debug.Log($"Applied {mode} lighting preset to {_selectedZone}");
            }
        }
        
        private void ApplyPhotoperiodChange(float hours)
        {
            Debug.Log($"Setting photoperiod to {hours} hours in {_selectedZone}");
        }
        
        private void ApplyVPDChange(float vpd)
        {
            Debug.Log($"Setting VPD target to {vpd} kPa in {_selectedZone}");
        }
        
        private void ApplyCirculationFanChange(bool isOn)
        {
            Debug.Log($"{(isOn ? "Enabling" : "Disabling")} circulation fan in {_selectedZone}");
        }
        
        private void ApplyExhaustFanChange(bool isOn)
        {
            Debug.Log($"{(isOn ? "Enabling" : "Disabling")} exhaust fan in {_selectedZone}");
        }
        
        private void ApplyAirflowChange(float rate)
        {
            Debug.Log($"Setting airflow rate to {rate}% in {_selectedZone}");
        }
        
        #endregion
        
        #region Action Methods
        
        private void OptimizeCurrentZone()
        {
            var zoneData = _zoneData[_selectedZone];
            var optimizedSettings = CalculateOptimalSettings(zoneData);
            
            ApplyOptimizedSettings(optimizedSettings);
            
            PlaySound(_controlAdjustSound);
            ShowOptimizationMessage();
            
            Debug.Log($"Optimized settings applied to {_selectedZone}");
        }
        
        private void CopySettingsToOtherZones()
        {
            var sourceData = _zoneData[_selectedZone];
            
            foreach (var zone in _managedZones.Where(z => z != _selectedZone))
            {
                CopySettingsToZone(sourceData, zone);
            }
            
            ShowCopyMessage();
            Debug.Log($"Settings copied from {_selectedZone} to all other zones");
        }
        
        private void EmergencyStopAll()
        {
            foreach (var zone in _managedZones)
            {
                EmergencyStopZone(zone);
            }
            
            PlaySound(_alertSound);
            ShowEmergencyMessage();
            OnEmergencyAction?.Invoke("emergency_stop_all");
            
            Debug.Log("Emergency stop activated for all zones");
        }
        
        private void CreateNewSchedule()
        {
            var schedule = new EnvironmentalSchedule
            {
                ScheduleId = Guid.NewGuid().ToString(),
                ZoneId = _selectedZone,
                Name = $"Schedule for {GetFriendlyZoneName(_selectedZone)}",
                StartTime = new TimeSpan(6, 0, 0),
                EndTime = new TimeSpan(22, 0, 0),
                IsActive = true,
                TargetTemperature = _zoneData[_selectedZone].TargetTemperature,
                TargetHumidity = _zoneData[_selectedZone].TargetHumidity,
                TargetCO2 = _zoneData[_selectedZone].TargetCO2,
                LightIntensity = _zoneData[_selectedZone].TargetLightIntensity
            };
            
            _activeSchedules.Add(schedule);
            UpdateScheduleList();
            OnScheduleCreated?.Invoke(schedule);
            
            Debug.Log($"Created new schedule for {_selectedZone}");
        }
        
        private void ToggleTemperatureAuto()
        {
            bool isAuto = !GetAutomationState("temperature");
            SetAutomationState("temperature", isAuto);
            UpdateTemperatureAutoButton(isAuto);
            
            // if (isAuto && _automationManager != null)
            // {
                // Create automation rule for temperature
                CreateTemperatureAutomationRule();
            // }
        }
        
        private void ToggleHumidityAuto()
        {
            bool isAuto = !GetAutomationState("humidity");
            SetAutomationState("humidity", isAuto);
            UpdateHumidityAutoButton(isAuto);
            
            // if (isAuto && _automationManager != null)
            // {
                CreateHumidityAutomationRule();
            // }
        }
        
        private void ToggleCO2Auto()
        {
            bool isAuto = !GetAutomationState("co2");
            SetAutomationState("co2", isAuto);
            UpdateCO2AutoButton(isAuto);
            
            // if (isAuto && _automationManager != null)
            // {
                CreateCO2AutomationRule();
            // }
        }
        
        #endregion
        
        #region Data Management
        
        private void LoadZoneData()
        {
            // Load initial zone data from systems or defaults
            foreach (var zone in _managedZones)
            {
                if (!_zoneData.ContainsKey(zone))
                {
                    _zoneData[zone] = CreateDefaultZoneData(zone);
                }
            }
            
            LoadZoneSettings();
        }
        
        private void LoadZoneSettings()
        {
            if (!_zoneData.ContainsKey(_selectedZone)) return;
            
            var zoneData = _zoneData[_selectedZone];
            _isUpdating = true;
            
            // Update temperature controls
            _temperatureSlider.value = zoneData.TargetTemperature;
            _temperatureField.value = zoneData.TargetTemperature;
            
            // Update humidity controls
            _humiditySlider.value = zoneData.TargetHumidity;
            _humidityField.value = zoneData.TargetHumidity;
            
            // Update CO2 controls
            _co2Slider.value = zoneData.TargetCO2;
            _co2Field.value = zoneData.TargetCO2;
            
            // Update lighting controls
            _lightIntensitySlider.value = zoneData.TargetLightIntensity;
            _lightIntensityField.value = zoneData.TargetLightIntensity;
            _lightingOnToggle.value = zoneData.IsLightingOn;
            _lightingModeDropdown.value = zoneData.LightingMode;
            _photoperiodSlider.value = zoneData.PhotoperiodHours;
            
            // Update advanced controls
            _vpnSlider.value = zoneData.TargetVPD;
            _circulationFanToggle.value = zoneData.CirculationFanOn;
            _exhaustFanToggle.value = zoneData.ExhaustFanOn;
            _airflowSlider.value = zoneData.AirflowRate;
            
            _isUpdating = false;
            
            // Update labels
            UpdateSelectedZoneLabel();
            UpdatePhotoperiodLabel(zoneData.PhotoperiodHours);
            UpdateVPDLabel(zoneData.TargetVPD);
        }
        
        [ContextMenu("Update Environmental Data")]
        public void UpdateEnvironmentalData()
        {
            if (_isUpdating) return;
            
            // Update current readings from sensors/systems
            foreach (var zone in _managedZones)
            {
                UpdateZoneReadings(zone);
            }
            
            // Update UI displays
            UpdateCurrentReadingsDisplay();
            UpdateEquipmentStatus();
            UpdateStabilityBars();
            
            // Check for achievement triggers
            TriggerOptimalEnvironmentAchievement();
            
            _lastUpdateTime = Time.time;
        }
        
        private void UpdateZoneReadings(string zoneId)
        {
            // if (_automationManager != null)
            // {
                // var readings = _automationManager.GetZoneSensorReadings(zoneId);
                
            // Create empty readings list when automation manager is not available
            var readings = new List<SensorReading>();
                
            foreach (var reading in readings)
            {
                UpdateZoneData(zoneId, zd =>
                {
                    if (reading.SensorId.Contains("temp"))
                        zd.Temperature = reading.Value;
                    else if (reading.SensorId.Contains("humidity"))
                        zd.Humidity = reading.Value;
                    else if (reading.SensorId.Contains("co2"))
                        zd.CO2Level = reading.Value;
                    else if (reading.SensorId.Contains("light"))
                        zd.LightIntensity = reading.Value;
                });
            }
            // }
            // else
            // {
                // Simulate gradual changes toward targets
                UpdateZoneData(zoneId, zd => SimulateEnvironmentalChanges(zd));
            // }
        }
        
        private void UpdateZoneData(string zoneId, System.Action<EnvironmentalZoneData> updateAction)
        {
            if (_zoneData.ContainsKey(zoneId))
            {
                updateAction(_zoneData[zoneId]);
            }
        }
        
        #endregion
        
        #region UI Update Methods
        
        private void UpdateCurrentReadingsDisplay()
        {
            if (!_zoneData.ContainsKey(_selectedZone)) return;
            
            var zoneData = _zoneData[_selectedZone];
            
            // Enhanced temperature display with visual feedback
            if (_currentTemperatureLabel != null)
            {
                string trend = GetTrendArrow(zoneData.Temperature, zoneData.PreviousTemperature);
                string colorClass = GetOptimalityColorClass(zoneData.Temperature, zoneData.TargetTemperature, 2f);
                _currentTemperatureLabel.text = $"{zoneData.Temperature:F1}°C {trend}";
                ApplyVisualFeedback(_currentTemperatureLabel, colorClass);
            }
            
            // Enhanced humidity display with visual feedback
            if (_currentHumidityLabel != null)
            {
                string trend = GetTrendArrow(zoneData.Humidity, zoneData.PreviousHumidity);
                string colorClass = GetOptimalityColorClass(zoneData.Humidity, zoneData.TargetHumidity, 5f);
                _currentHumidityLabel.text = $"{zoneData.Humidity:F1}% {trend}";
                ApplyVisualFeedback(_currentHumidityLabel, colorClass);
            }
            
            // Enhanced CO2 display with visual feedback
            if (_currentCo2Label != null)
            {
                string trend = GetTrendArrow(zoneData.CO2Level, zoneData.PreviousCO2);
                string colorClass = GetOptimalityColorClass(zoneData.CO2Level, zoneData.TargetCO2, 100f);
                _currentCo2Label.text = $"{zoneData.CO2Level:F0} ppm {trend}";
                ApplyVisualFeedback(_currentCo2Label, colorClass);
            }
            
            // Enhanced light display with visual feedback
            if (_currentLightLabel != null)
            {
                string trend = GetTrendArrow(zoneData.LightIntensity, zoneData.PreviousLightIntensity);
                string colorClass = GetOptimalityColorClass(zoneData.LightIntensity, zoneData.TargetLightIntensity, 50f);
                _currentLightLabel.text = $"{zoneData.LightIntensity:F0} PPFD {trend}";
                ApplyVisualFeedback(_currentLightLabel, colorClass);
            }
            
            // Enhanced DLI progress with game-like feedback
            if (_dliProgressBar != null)
            {
                float dli = CalculateDLI(zoneData.LightIntensity, zoneData.PhotoperiodHours);
                float dliProgress = dli / 50f; // Assuming 50 mol/m²/day max
                
                _dliProgressBar.value = dliProgress;
                _dliProgressBar.title = $"DLI: {dli:F1} mol/m²/day";
                
                // Add visual feedback for optimal DLI ranges
                if (dli >= 35f && dli <= 45f) // Optimal range
                {
                    _dliProgressBar.AddToClassList("optimal-range");
                    _dliProgressBar.RemoveFromClassList("suboptimal-range");
                }
                // else
                // {
                    _dliProgressBar.AddToClassList("suboptimal-range");
                    _dliProgressBar.RemoveFromClassList("optimal-range");
                // }
            }
            
            // Store previous values for next trend calculation
            zoneData.PreviousTemperature = zoneData.Temperature;
            zoneData.PreviousHumidity = zoneData.Humidity;
            zoneData.PreviousCO2 = zoneData.CO2Level;
            zoneData.PreviousLightIntensity = zoneData.LightIntensity;
        }
        
        private void UpdateEquipmentStatus()
        {
            // Update equipment status indicators
            UpdateHVACStatus();
            UpdateLightingStatus();
            UpdateFanStatus();
            UpdateCO2Status();
        }
        
        private void UpdateStabilityBars()
        {
            if (!_zoneData.ContainsKey(_selectedZone)) return;
            
            var zoneData = _zoneData[_selectedZone];
            
            // Calculate stability based on how close current values are to targets
            float tempStability = CalculateStability(zoneData.Temperature, zoneData.TargetTemperature, 2f);
            float humidityStability = CalculateStability(zoneData.Humidity, zoneData.TargetHumidity, 5f);
            float co2Stability = CalculateStability(zoneData.CO2Level, zoneData.TargetCO2, 100f);
            
            if (_temperatureStabilityBar != null)
            {
                _temperatureStabilityBar.value = tempStability;
                UpdateStabilityBarColor(_temperatureStabilityBar, tempStability);
            }
            
            if (_humidityStabilityBar != null)
            {
                _humidityStabilityBar.value = humidityStability;
                UpdateStabilityBarColor(_humidityStabilityBar, humidityStability);
            }
            
            if (_co2StabilityBar != null)
            {
                _co2StabilityBar.value = co2Stability;
                UpdateStabilityBarColor(_co2StabilityBar, co2Stability);
            }
        }
        
        private void UpdateTemperatureField(float value)
        {
            if (_temperatureField != null && !_isUpdating)
            {
                _isUpdating = true;
                _temperatureField.value = value;
                _isUpdating = false;
            }
        }
        
        private void UpdateTemperatureSlider(float value)
        {
            if (_temperatureSlider != null && !_isUpdating)
            {
                _isUpdating = true;
                _temperatureSlider.value = value;
                _isUpdating = false;
            }
        }
        
        private void UpdateHumidityField(float value)
        {
            if (_humidityField != null && !_isUpdating)
            {
                _isUpdating = true;
                _humidityField.value = value;
                _isUpdating = false;
            }
        }
        
        private void UpdateHumiditySlider(float value)
        {
            if (_humiditySlider != null && !_isUpdating)
            {
                _isUpdating = true;
                _humiditySlider.value = value;
                _isUpdating = false;
            }
        }
        
        private void UpdateCO2Field(float value)
        {
            if (_co2Field != null && !_isUpdating)
            {
                _isUpdating = true;
                _co2Field.value = value;
                _isUpdating = false;
            }
        }
        
        private void UpdateCO2Slider(float value)
        {
            if (_co2Slider != null && !_isUpdating)
            {
                _isUpdating = true;
                _co2Slider.value = value;
                _isUpdating = false;
            }
        }
        
        private void UpdateLightIntensityField(float value)
        {
            if (_lightIntensityField != null && !_isUpdating)
            {
                _isUpdating = true;
                _lightIntensityField.value = value;
                _isUpdating = false;
            }
        }
        
        private void UpdateLightIntensitySlider(float value)
        {
            if (_lightIntensitySlider != null && !_isUpdating)
            {
                _isUpdating = true;
                _lightIntensitySlider.value = value;
                _isUpdating = false;
            }
        }
        
        private void UpdateSelectedZoneLabel()
        {
            if (_selectedZoneLabel != null)
            {
                _selectedZoneLabel.text = GetFriendlyZoneName(_selectedZone);
            }
        }
        
        private void UpdatePhotoperiodLabel(float hours)
        {
            if (_photoperiodLabel != null)
            {
                int totalHours = (int)hours;
                int minutes = (int)((hours - totalHours) * 60);
                _photoperiodLabel.text = $"{totalHours}h {minutes}m";
            }
        }
        
        private void UpdateVPDLabel(float vpd)
        {
            if (_vpnLabel != null)
            {
                _vpnLabel.text = $"{vpd:F2} kPa";
            }
        }
        
        private void UpdateScheduleList()
        {
            if (_scheduleList == null) return;
            
            _scheduleList.Clear();
            
            var zoneSchedules = _activeSchedules.Where(s => s.ZoneId == _selectedZone).ToList();
            
            foreach (var schedule in zoneSchedules)
            {
                var scheduleElement = CreateScheduleElement(schedule);
                _scheduleList.Add(scheduleElement);
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private string GetFriendlyZoneName(string zoneId)
        {
            return zoneId switch
            {
                "VegetativeRoom" => "Vegetative Room",
                "FloweringRoom" => "Flowering Room",
                "DryingRoom" => "Drying Room",
                "NurseryRoom" => "Nursery Room",
                "MotherRoom" => "Mother Plant Room",
                _ => zoneId
            };
        }
        
        private string GetZoneIdFromFriendlyName(string friendlyName)
        {
            return friendlyName switch
            {
                "Vegetative Room" => "VegetativeRoom",
                "Flowering Room" => "FloweringRoom",
                "Drying Room" => "DryingRoom",
                "Nursery Room" => "NurseryRoom",
                "Mother Plant Room" => "MotherRoom",
                _ => friendlyName.Replace(" ", "")
            };
        }
        
        private LightingPreset GetLightingPreset(string mode)
        {
            return mode switch
            {
                "Seedling" => new LightingPreset { Intensity = 200f, PhotoperiodHours = 18f },
                "Vegetative" => new LightingPreset { Intensity = 400f, PhotoperiodHours = 18f },
                "Flowering" => new LightingPreset { Intensity = 600f, PhotoperiodHours = 12f },
                "Drying" => new LightingPreset { Intensity = 0f, PhotoperiodHours = 0f },
                _ => null
            };
        }
        
        private EnvironmentalZoneData CreateDefaultZoneData(string zoneId)
        {
            return new EnvironmentalZoneData
            {
                ZoneId = zoneId,
                ZoneName = GetFriendlyZoneName(zoneId),
                Temperature = 22f,
                Humidity = 55f,
                CO2Level = 1000f,
                LightIntensity = 400f,
                TargetTemperature = 22f,
                TargetHumidity = 55f,
                TargetCO2 = 1000f,
                TargetLightIntensity = 400f,
                TargetVPD = 1.0f,
                IsLightingOn = true,
                LightingMode = "Vegetative",
                PhotoperiodHours = 18f,
                CirculationFanOn = true,
                ExhaustFanOn = true,
                AirflowRate = 75f
            };
        }
        
        private OptimizedSettings CalculateOptimalSettings(EnvironmentalZoneData zoneData)
        {
            // AI-driven optimization logic would go here
            return new OptimizedSettings
            {
                Temperature = 24f,
                Humidity = 60f,
                CO2 = 1200f,
                LightIntensity = 500f,
                VPD = 1.1f
            };
        }
        
        private void ApplyOptimizedSettings(OptimizedSettings settings)
        {
            _isUpdating = true;
            
            _temperatureSlider.value = settings.Temperature;
            _humiditySlider.value = settings.Humidity;
            _co2Slider.value = settings.CO2;
            _lightIntensitySlider.value = settings.LightIntensity;
            _vpnSlider.value = settings.VPD;
            
            UpdateZoneData(_selectedZone, zd =>
            {
                zd.TargetTemperature = settings.Temperature;
                zd.TargetHumidity = settings.Humidity;
                zd.TargetCO2 = settings.CO2;
                zd.TargetLightIntensity = settings.LightIntensity;
                zd.TargetVPD = settings.VPD;
            });
            
            _isUpdating = false;
        }
        
        private void CopySettingsToZone(EnvironmentalZoneData sourceData, string targetZone)
        {
            UpdateZoneData(targetZone, zd =>
            {
                zd.TargetTemperature = sourceData.TargetTemperature;
                zd.TargetHumidity = sourceData.TargetHumidity;
                zd.TargetCO2 = sourceData.TargetCO2;
                zd.TargetLightIntensity = sourceData.TargetLightIntensity;
                zd.TargetVPD = sourceData.TargetVPD;
                zd.LightingMode = sourceData.LightingMode;
                zd.PhotoperiodHours = sourceData.PhotoperiodHours;
                zd.CirculationFanOn = sourceData.CirculationFanOn;
                zd.ExhaustFanOn = sourceData.ExhaustFanOn;
                zd.AirflowRate = sourceData.AirflowRate;
            });
        }
        
        private void EmergencyStopZone(string zoneId)
        {
            UpdateZoneData(zoneId, zd =>
            {
                zd.IsLightingOn = false;
                zd.LightIntensity = 0f;
                zd.CirculationFanOn = false;
                zd.ExhaustFanOn = true; // Keep exhaust for safety
                zd.TargetCO2 = 400f; // Return to ambient
            });
        }
        
        private void SimulateEnvironmentalChanges(EnvironmentalZoneData zoneData)
        {
            float deltaTime = Time.deltaTime;
            float changeRate = 0.5f; // How quickly environment responds
            
            // Gradual movement toward targets
            zoneData.Temperature = Mathf.MoveTowards(zoneData.Temperature, zoneData.TargetTemperature, changeRate * deltaTime);
            zoneData.Humidity = Mathf.MoveTowards(zoneData.Humidity, zoneData.TargetHumidity, changeRate * deltaTime);
            zoneData.CO2Level = Mathf.MoveTowards(zoneData.CO2Level, zoneData.TargetCO2, changeRate * 10f * deltaTime);
            
            if (zoneData.IsLightingOn)
            {
                zoneData.LightIntensity = Mathf.MoveTowards(zoneData.LightIntensity, zoneData.TargetLightIntensity, changeRate * 50f * deltaTime);
            }
            // else
            // {
                zoneData.LightIntensity = Mathf.MoveTowards(zoneData.LightIntensity, 0f, changeRate * 100f * deltaTime);
            // }
        }
        
        private float CalculateStability(float current, float target, float tolerance)
        {
            float difference = Mathf.Abs(current - target);
            float stability = Mathf.Max(0f, 1f - (difference / tolerance));
            return stability;
        }
        
        private float CalculateDLI(float intensity, float hours)
        {
            // Simplified DLI calculation: PPFD * hours * 0.0036
            return intensity * hours * 0.0036f;
        }
        
        private void UpdateStabilityBarColor(ProgressBar bar, float stability)
        {
            bar.RemoveFromClassList("stability-excellent");
            bar.RemoveFromClassList("stability-good");
            bar.RemoveFromClassList("stability-poor");
            
            if (stability > 0.9f)
                bar.AddToClassList("stability-excellent");
            else if (stability > 0.7f)
                bar.AddToClassList("stability-good");
            else
                bar.AddToClassList("stability-poor");
        }
        
        private VisualElement CreateScheduleElement(EnvironmentalSchedule schedule)
        {
            var element = new VisualElement();
            element.AddToClassList("schedule-item");
            
            var nameLabel = new Label(schedule.Name);
            nameLabel.AddToClassList("schedule-name");
            
            var timeLabel = new Label($"{schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm}");
            timeLabel.AddToClassList("schedule-time");
            
            var deleteButton = new Button(() => DeleteSchedule(schedule.ScheduleId));
            deleteButton.text = "×";
            deleteButton.AddToClassList("schedule-delete");
            
            element.Add(nameLabel);
            element.Add(timeLabel);
            element.Add(deleteButton);
            
            return element;
        }
        
        private void DeleteSchedule(string scheduleId)
        {
            _activeSchedules.RemoveAll(s => s.ScheduleId == scheduleId);
            UpdateScheduleList();
        }
        
        private void UpdateHVACStatus()
        {
            // Update HVAC status indicators
            if (_hvacStatusPanel != null)
            {
                _hvacStatusPanel.RemoveFromClassList("status-online");
                _hvacStatusPanel.RemoveFromClassList("status-offline");
                _hvacStatusPanel.AddToClassList(_hvacManager != null ? "status-online" : "status-offline");
            }
        }
        
        private void UpdateLightingStatus()
        {
            if (_lightingStatusPanel != null)
            {
                var zoneData = _zoneData[_selectedZone];
                _lightingStatusPanel.RemoveFromClassList("status-on");
                _lightingStatusPanel.RemoveFromClassList("status-off");
                _lightingStatusPanel.AddToClassList(zoneData.IsLightingOn ? "status-on" : "status-off");
            }
        }
        
        private void UpdateFanStatus()
        {
            if (_fanStatusPanel != null)
            {
                var zoneData = _zoneData[_selectedZone];
                _fanStatusPanel.RemoveFromClassList("status-on");
                _fanStatusPanel.RemoveFromClassList("status-off");
                _fanStatusPanel.AddToClassList((zoneData.CirculationFanOn || zoneData.ExhaustFanOn) ? "status-on" : "status-off");
            }
        }
        
        private void UpdateCO2Status()
        {
            if (_co2StatusPanel != null)
            {
                var zoneData = _zoneData[_selectedZone];
                _co2StatusPanel.RemoveFromClassList("status-normal");
                _co2StatusPanel.RemoveFromClassList("status-high");
                _co2StatusPanel.RemoveFromClassList("status-low");
                
                if (zoneData.CO2Level < 600f)
                    _co2StatusPanel.AddToClassList("status-low");
                else if (zoneData.CO2Level > 1500f)
                    _co2StatusPanel.AddToClassList("status-high");
                else
                    _co2StatusPanel.AddToClassList("status-normal");
            }
        }
        
        private void UpdateAutomationState(bool enabled)
        {
            // Update automation UI elements based on state
            Debug.Log($"Automation {(enabled ? "enabled" : "disabled")} for {_selectedZone}");
        }
        
        private void UpdateTemperatureAutoButton(bool isAuto)
        {
            if (_temperatureAutoButton != null)
            {
                _temperatureAutoButton.text = isAuto ? "AUTO ON" : "AUTO OFF";
                _temperatureAutoButton.RemoveFromClassList("auto-on");
                _temperatureAutoButton.RemoveFromClassList("auto-off");
                _temperatureAutoButton.AddToClassList(isAuto ? "auto-on" : "auto-off");
            }
        }
        
        private void UpdateHumidityAutoButton(bool isAuto)
        {
            if (_humidityAutoButton != null)
            {
                _humidityAutoButton.text = isAuto ? "AUTO ON" : "AUTO OFF";
                _humidityAutoButton.RemoveFromClassList("auto-on");
                _humidityAutoButton.RemoveFromClassList("auto-off");
                _humidityAutoButton.AddToClassList(isAuto ? "auto-on" : "auto-off");
            }
        }
        
        private void UpdateCO2AutoButton(bool isAuto)
        {
            if (_co2AutoButton != null)
            {
                _co2AutoButton.text = isAuto ? "AUTO ON" : "AUTO OFF";
                _co2AutoButton.RemoveFromClassList("auto-on");
                _co2AutoButton.RemoveFromClassList("auto-off");
                _co2AutoButton.AddToClassList(isAuto ? "auto-on" : "auto-off");
            }
        }
        
        private bool GetAutomationState(string parameter)
        {
            return _automationStates.GetValueOrDefault($"{_selectedZone}_{parameter}", false);
        }
        
        private void SetAutomationState(string parameter, bool state)
        {
            _automationStates[$"{_selectedZone}_{parameter}"] = state;
        }
        
        private void CreateTemperatureAutomationRule()
        {
            // if (_automationManager == null) return;
            
            var trigger = new ProjectChimera.Data.Automation.AutomationTrigger
            {
                TriggerType = ProjectChimera.Data.Automation.TriggerType.Threshold_Exceeded,
                SourceSensorId = $"temp_sensor_{_selectedZone.ToLower()}",
                TriggerValue = _zoneData[_selectedZone].TargetTemperature + 2f,
                Operator = ProjectChimera.Data.Automation.ComparisonOperator.GreaterThan
            };
            
            var actions = new List<ProjectChimera.Data.Automation.AutomationAction>
            {
                new ProjectChimera.Data.Automation.AutomationAction
                {
                    ActionId = Guid.NewGuid().ToString(),
                    ActionType = ProjectChimera.Data.Automation.ActionType.SetTemperature,
                    TargetZoneId = _selectedZone,
                    Parameters = new Dictionary<string, object> { { "temperature", _zoneData[_selectedZone].TargetTemperature } }
                }
            };
            
            // _automationManager.CreateAutomationRule($"Auto Temperature Control - {_selectedZone}", trigger, actions);
        }
        
        private void CreateHumidityAutomationRule()
        {
            // if (_automationManager == null) return;
            
            var trigger = new ProjectChimera.Data.Automation.AutomationTrigger
            {
                TriggerType = ProjectChimera.Data.Automation.TriggerType.Threshold_Below,
                SourceSensorId = $"humidity_sensor_{_selectedZone.ToLower()}",
                TriggerValue = _zoneData[_selectedZone].TargetHumidity - 5f,
                Operator = ProjectChimera.Data.Automation.ComparisonOperator.LessThan
            };
            
            var actions = new List<ProjectChimera.Data.Automation.AutomationAction>
            {
                new ProjectChimera.Data.Automation.AutomationAction
                {
                    ActionId = Guid.NewGuid().ToString(),
                    ActionType = ProjectChimera.Data.Automation.ActionType.SetHumidity,
                    TargetZoneId = _selectedZone,
                    Parameters = new Dictionary<string, object> { { "humidity", _zoneData[_selectedZone].TargetHumidity } }
                }
            };
            
            // _automationManager.CreateAutomationRule($"Auto Humidity Control - {_selectedZone}", trigger, actions);
        }
        
        private void CreateCO2AutomationRule()
        {
            // if (_automationManager == null) return;
            
            var trigger = new ProjectChimera.Data.Automation.AutomationTrigger
            {
                TriggerType = ProjectChimera.Data.Automation.TriggerType.Threshold_Below,
                SourceSensorId = $"co2_sensor_{_selectedZone.ToLower()}",
                TriggerValue = _zoneData[_selectedZone].TargetCO2 - 100f,
                Operator = ProjectChimera.Data.Automation.ComparisonOperator.LessThan
            };
            
            var actions = new List<ProjectChimera.Data.Automation.AutomationAction>
            {
                new ProjectChimera.Data.Automation.AutomationAction
                {
                    ActionId = Guid.NewGuid().ToString(),
                    ActionType = ProjectChimera.Data.Automation.ActionType.LogEvent,
                    Parameters = new Dictionary<string, object> { { "message", $"CO2 adjustment needed in {_selectedZone}" } }
                }
            };
            
            // _automationManager.CreateAutomationRule($"Auto CO2 Control - {_selectedZone}", trigger, actions);
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
        
        private void ShowOptimizationMessage()
        {
            // Would show UI notification about optimization
            Debug.Log("Zone optimization applied successfully!");
        }
        
        private void ShowCopyMessage()
        {
            // Would show UI notification about copying settings
            Debug.Log("Settings copied to all zones successfully!");
        }
        
        private void ShowEmergencyMessage()
        {
            // Would show emergency UI notification
            Debug.Log("Emergency stop activated!");
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
        
        #endregion
        
        #region Visual Feedback Enhancement Methods
        
        /// <summary>
        /// Gets trend arrow indicator based on value change
        /// </summary>
        private string GetTrendArrow(float currentValue, float previousValue)
        {
            float difference = currentValue - previousValue;
            float threshold = 0.1f; // Minimum change to show trend
            
            if (Mathf.Abs(difference) < threshold)
                return "→"; // Stable
            else if (difference > 0)
                return "↗"; // Increasing
            else
                return "↘"; // Decreasing
        }
        
        /// <summary>
        /// Gets color class based on how close value is to optimal target
        /// </summary>
        private string GetOptimalityColorClass(float currentValue, float targetValue, float tolerance)
        {
            float difference = Mathf.Abs(currentValue - targetValue);
            
            if (difference <= tolerance * 0.5f)
                return "optimal-value"; // Perfect range - green
            else if (difference <= tolerance)
                return "good-value"; // Close to target - yellow
            else if (difference <= tolerance * 2f)
                return "warning-value"; // Getting off target - orange
            else
                return "critical-value"; // Far from target - red
        }
        
        /// <summary>
        /// Applies visual feedback styling to UI elements
        /// </summary>
        private void ApplyVisualFeedback(VisualElement element, string feedbackClass)
        {
            if (element == null) return;
            
            // Remove all feedback classes first
            element.RemoveFromClassList("optimal-value");
            element.RemoveFromClassList("good-value");
            element.RemoveFromClassList("warning-value");
            element.RemoveFromClassList("critical-value");
            
            // Add the new feedback class
            element.AddToClassList(feedbackClass);
            
            // Add subtle animation class for visual appeal
            element.AddToClassList("value-updated");
            
            // Remove animation class after brief delay to allow re-triggering
            element.schedule.Execute(() => element.RemoveFromClassList("value-updated")).StartingIn(300);
        }
        
        /// <summary>
        /// Creates achievement-style notification for optimal environmental control
        /// </summary>
        private void TriggerOptimalEnvironmentAchievement()
        {
            if (!_zoneData.ContainsKey(_selectedZone)) return;
            
            var zoneData = _zoneData[_selectedZone];
            
            // Check if all parameters are in optimal ranges
            bool tempOptimal = Mathf.Abs(zoneData.Temperature - zoneData.TargetTemperature) <= 1f;
            bool humidityOptimal = Mathf.Abs(zoneData.Humidity - zoneData.TargetHumidity) <= 2.5f;
            bool co2Optimal = Mathf.Abs(zoneData.CO2Level - zoneData.TargetCO2) <= 50f;
            bool lightOptimal = Mathf.Abs(zoneData.LightIntensity - zoneData.TargetLightIntensity) <= 25f;
            
            if (tempOptimal && humidityOptimal && co2Optimal && lightOptimal)
            {
                // Trigger achievement notification (would integrate with achievement system)
                Debug.Log($"🏆 Perfect Environment Achievement! Zone {_selectedZone} is optimally tuned!");
                PlaySound(_notificationSound);
                
                // Add visual celebration effect
                _rootElement?.AddToClassList("achievement-celebration");
                _rootElement?.schedule.Execute(() => _rootElement.RemoveFromClassList("achievement-celebration")).StartingIn(2000);
            }
        }
        
        #endregion
    }
    
    // Supporting data structures
    [System.Serializable]
    public class EnvironmentalZoneData
    {
        public string ZoneId;
        public string ZoneName;
        
        // Current readings
        public float Temperature;
        public float Humidity;
        public float CO2Level;
        public float LightIntensity;
        
        // Previous readings for trend calculation
        public float PreviousTemperature;
        public float PreviousHumidity;
        public float PreviousCO2;
        public float PreviousLightIntensity;
        
        // Target settings
        public float TargetTemperature;
        public float TargetHumidity;
        public float TargetCO2;
        public float TargetLightIntensity;
        public float TargetVPD;
        
        // Lighting settings
        public bool IsLightingOn;
        public string LightingMode;
        public float PhotoperiodHours;
        
        // Fan settings
        public bool CirculationFanOn;
        public bool ExhaustFanOn;
        public float AirflowRate;
        
        // Status
        public DateTime LastUpdated;
        public bool IsOnline;
    }
    
    [System.Serializable]
    public class EnvironmentalSchedule
    {
        public string ScheduleId;
        public string ZoneId;
        public string Name;
        public TimeSpan StartTime;
        public TimeSpan EndTime;
        public bool IsActive;
        public float TargetTemperature;
        public float TargetHumidity;
        public float TargetCO2;
        public float LightIntensity;
        public List<DayOfWeek> ActiveDays = new List<DayOfWeek>();
    }
    
    [System.Serializable]
    public class LightingPreset
    {
        public float Intensity;
        public float PhotoperiodHours;
    }
    
    [System.Serializable]
    public class OptimizedSettings
    {
        public float Temperature;
        public float Humidity;
        public float CO2;
        public float LightIntensity;
        public float VPD;
    }
    
    [System.Serializable]
    public class EnvironmentalControlSettings
    {
        public bool EnableAdvancedControls = true;
        public bool EnableScheduling = true;
        public bool EnableAutomation = true;
        public float DefaultUpdateInterval = 2f;
        public bool PlaySoundEffects = true;
        public bool ShowTooltips = true;
    }
    
    // Temporary SensorReading class for compatibility
    [System.Serializable]
    public class SensorReading
    {
        public string SensorId;
        public float Value;
        public DateTime Timestamp;
        public string Units;
    }
}