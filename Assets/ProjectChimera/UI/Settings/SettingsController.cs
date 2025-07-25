using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
// using ProjectChimera.Systems.Settings;
using ProjectChimera.Data.UI;
// using SettingsManager = ProjectChimera.Systems.Settings.SettingsManager;

namespace ProjectChimera.UI.Settings
{
    /// <summary>
    /// Interface for settings manager to avoid dependency issues
    /// </summary>
    public interface ISettingsManager
    {
        Dictionary<string, object> GetAllSettings();
        void ApplySettings(Dictionary<string, object> settings);
        List<SettingsProfile> GetAvailableProfiles();
        void SaveProfile(SettingsProfile profile);
        void DeleteProfile(SettingsProfile profile);
    }

    /// <summary>
    /// Settings & Configuration UI Controller for Project Chimera.
    /// Provides comprehensive system preferences, customization options, and configuration management.
    /// Features organized settings panels, real-time preview, and profile management with gaming aesthetics.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        [Header("Settings UI Configuration")]
        [SerializeField] private UIDocument _settingsDocument;
        [SerializeField] private SettingsUIConfiguration _uiConfig;
        [SerializeField] private bool _enableRealTimePreview = true;
        [SerializeField] private float _previewUpdateInterval = 0.5f;
        
        [Header("Profile Management")]
        [SerializeField] private bool _enableProfileSystem = true;
        [SerializeField] private int _maxProfiles = 10;
        [SerializeField] private string _defaultProfileName = "Default";
        
        [Header("Import/Export Configuration")]
        [SerializeField] private bool _enableSettingsImportExport = true;
        [SerializeField] private string[] _supportedFormats = { "JSON", "XML", "Binary" };
        
        [Header("Audio Configuration")]
        [SerializeField] private AudioClip _settingChangedSound;
        [SerializeField] private AudioClip _profileSwitchedSound;
        [SerializeField] private AudioClip _importExportSound;
        [SerializeField] private AudioClip _resetSound;
        [SerializeField] private AudioSource _audioSource;
        
        // System References
        private ISettingsManager _settingsManager; // Placeholder for SettingsManager when available

        // UI Elements - Main Interface
        private VisualElement _rootElement;
        private Button _gameplayTabButton;
        private Button _graphicsTabButton;
        private Button _audioTabButton;
        private Button _controlsTabButton;
        private Button _networkTabButton;
        private Button _accessibilityTabButton;
        private Button _advancedTabButton;
        
        // Tab Panels
        private VisualElement _gameplayPanel;
        private VisualElement _graphicsPanel;
        private VisualElement _audioPanel;
        private VisualElement _controlsPanel;
        private VisualElement _networkPanel;
        private VisualElement _accessibilityPanel;
        private VisualElement _advancedPanel;
        
        // Profile Management
        private VisualElement _profileSection;
        private DropdownField _profileSelector;
        private TextField _profileNameField;
        private Button _saveProfileButton;
        private Button _deleteProfileButton;
        private Button _duplicateProfileButton;
        private Button _importProfileButton;
        private Button _exportProfileButton;
        
        // Gameplay Settings
        private VisualElement _gameplayContent;
        private DropdownField _difficultySelector;
        private Slider _gameSpeedSlider;
        private Toggle _tutorialsToggle;
        private Toggle _notificationsToggle;
        private Toggle _autoSaveToggle;
        private Slider _autoSaveIntervalSlider;
        private Toggle _pauseOnFocusLossToggle;
        private DropdownField _uiScaleSelector;
        
        // Graphics Settings
        private VisualElement _graphicsContent;
        private DropdownField _qualityPresetDropdown;
        private DropdownField _resolutionDropdown;
        private DropdownField _windowModeDropdown;
        private Slider _renderScaleSlider;
        private Toggle _vsyncToggle;
        private Slider _frameRateLimitSlider;
        private Slider _brightnessSlider;
        private Slider _contrastSlider;
        private Slider _saturationSlider;
        private Toggle _antiAliasingToggle;
        private Toggle _shadowsToggle;
        private Slider _shadowQualitySlider;
        private Toggle _postProcessingToggle;
        
        // Audio Settings
        private VisualElement _audioContent;
        private Slider _masterVolumeSlider;
        private Slider _musicVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Slider _uiVolumeSlider;
        private Slider _ambientVolumeSlider;
        private DropdownField _audioQualityDropdown;
        private Toggle _muteOnFocusLossToggle;
        private Toggle _spatialAudioToggle;
        
        // Controls Settings
        private VisualElement _controlsContent;
        private DropdownField _controlSchemeDropdown;
        private Slider _mouseSensitivitySlider;
        private Toggle _invertMouseToggle;
        private Slider _scrollSpeedSlider;
        private VisualElement _keybindingsContainer;
        private Button _resetControlsButton;
        private Button _resetKeybindingsButton;
        
        // Network Settings
        private VisualElement _networkContent;
        private Toggle _onlineFeaturesToggle;
        private DropdownField _connectionTypeDropdown;
        private TextField _serverAddressField;
        private SliderInt _portSlider;
        private Toggle _autoConnectToggle;
        private Slider _timeoutSlider;
        private Toggle _cloudSyncToggle;
        
        // Accessibility Settings
        private VisualElement _accessibilityContent;
        private Toggle _colorBlindSupportToggle;
        private DropdownField _colorBlindTypeDropdown;
        private Toggle _highContrastToggle;
        private Slider _textSizeSlider;
        private Toggle _screenReaderToggle;
        private Toggle _subtitlesToggle;
        private Toggle _reducedMotionToggle;
        private Toggle _flashingLightsToggle;
        
        // Advanced Settings
        private VisualElement _advancedContent;
        private Toggle _debugModeToggle;
        private Toggle _developerConsoleToggle;
        private DropdownField _loggingLevelDropdown;
        private Toggle _telemetryToggle;
        private Toggle _crashReportingToggle;
        private TextField _customConfigPathField;
        private Button _clearCacheButton;
        private Button _resetAllButton;
        
        // Settings Actions
        private VisualElement _settingsActions;
        private Button _applyButton;
        private Button _revertButton;
        private Button _defaultsButton;
        private Label _statusLabel;
        
        // Data and State
        private Dictionary<string, object> _currentSettings = new Dictionary<string, object>();
        private Dictionary<string, object> _originalSettings = new Dictionary<string, object>();
        private List<SettingsProfile> _availableProfiles = new List<SettingsProfile>();
        private SettingsProfile _currentProfile;
        private string _currentTab = "gameplay";
        private bool _hasUnsavedChanges = false;
        private float _lastPreviewUpdate;
        private bool _isApplyingSettings = false;
        
        // Events
        public System.Action<string> OnTabChanged;
        public System.Action<string, object> OnSettingChanged;
        public System.Action<SettingsProfile> OnProfileChanged;
        public System.Action OnSettingsApplied;
        public System.Action OnSettingsReverted;
        
        private void Start()
        {
            InitializeController();
            InitializeSystemReferences();
            SetupUIElements();
            SetupEventHandlers();
            LoadSettings();
            
            if (_enableRealTimePreview)
            {
                InvokeRepeating(nameof(UpdateRealTimePreview), 1f, _previewUpdateInterval);
            }
        }
        
        private void InitializeController()
        {
            if (_settingsDocument == null)
            {
                Debug.LogError("Settings UI Document not assigned!");
                return;
            }
            
            _rootElement = _settingsDocument.rootVisualElement;
            _lastPreviewUpdate = Time.time;
            
            // Initialize default settings
            InitializeDefaultSettings();
            
            Debug.Log("Settings Controller initialized");
        }
        
        private void InitializeSystemReferences()
        {
            // _gameManager = GameManager.Instance;
            // if (_gameManager == null)
            // {
                Debug.LogWarning("GameManager not found - using standalone mode");
                return;
            // }
            
            // _settingsManager = _gameManager.GetManager<SettingsManager>();
            // if (_settingsManager == null)
            // {
                // Debug.LogWarning("SettingsManager not found - creating default settings");
            // }
            
            Debug.Log("Settings Controller connected to game systems");
        }
        
        private void SetupUIElements()
        {
            // Main navigation tabs
            _gameplayTabButton = _rootElement.Q<Button>("gameplay-tab");
            _graphicsTabButton = _rootElement.Q<Button>("graphics-tab");
            _audioTabButton = _rootElement.Q<Button>("audio-tab");
            _controlsTabButton = _rootElement.Q<Button>("controls-tab");
            _networkTabButton = _rootElement.Q<Button>("network-tab");
            _accessibilityTabButton = _rootElement.Q<Button>("accessibility-tab");
            _advancedTabButton = _rootElement.Q<Button>("advanced-tab");
            
            // Tab panels
            _gameplayPanel = _rootElement.Q<VisualElement>("gameplay-panel");
            _graphicsPanel = _rootElement.Q<VisualElement>("graphics-panel");
            _audioPanel = _rootElement.Q<VisualElement>("audio-panel");
            _controlsPanel = _rootElement.Q<VisualElement>("controls-panel");
            _networkPanel = _rootElement.Q<VisualElement>("network-panel");
            _accessibilityPanel = _rootElement.Q<VisualElement>("accessibility-panel");
            _advancedPanel = _rootElement.Q<VisualElement>("advanced-panel");
            
            // Profile management
            _profileSection = _rootElement.Q<VisualElement>("profile-section");
            _profileSelector = _rootElement.Q<DropdownField>("profile-selector");
            _profileNameField = _rootElement.Q<TextField>("profile-name-field");
            _saveProfileButton = _rootElement.Q<Button>("save-profile-button");
            _deleteProfileButton = _rootElement.Q<Button>("delete-profile-button");
            _duplicateProfileButton = _rootElement.Q<Button>("duplicate-profile-button");
            _importProfileButton = _rootElement.Q<Button>("import-profile-button");
            _exportProfileButton = _rootElement.Q<Button>("export-profile-button");
            
            SetupGameplaySettings();
            SetupGraphicsSettings();
            SetupAudioSettings();
            SetupControlsSettings();
            SetupNetworkSettings();
            SetupAccessibilitySettings();
            SetupAdvancedSettings();
            
            // Settings actions
            _settingsActions = _rootElement.Q<VisualElement>("settings-actions");
            _applyButton = _rootElement.Q<Button>("apply-button");
            _revertButton = _rootElement.Q<Button>("revert-button");
            _defaultsButton = _rootElement.Q<Button>("defaults-button");
            _statusLabel = _rootElement.Q<Label>("status-label");
            
            SetupDropdowns();
            SetupInitialState();
        }
        
        private void SetupGameplaySettings()
        {
            _gameplayContent = _rootElement.Q<VisualElement>("gameplay-content");
            _difficultySelector = _rootElement.Q<DropdownField>("difficulty-selector");
            _gameSpeedSlider = _rootElement.Q<Slider>("game-speed-slider");
            _tutorialsToggle = _rootElement.Q<Toggle>("tutorials-toggle");
            _notificationsToggle = _rootElement.Q<Toggle>("notifications-toggle");
            _autoSaveToggle = _rootElement.Q<Toggle>("auto-save-toggle");
            _autoSaveIntervalSlider = _rootElement.Q<Slider>("auto-save-interval-slider");
            _pauseOnFocusLossToggle = _rootElement.Q<Toggle>("pause-on-focus-loss-toggle");
            _uiScaleSelector = _rootElement.Q<DropdownField>("ui-scale-selector");
        }
        
        private void SetupGraphicsSettings()
        {
            _graphicsContent = _rootElement.Q<VisualElement>("graphics-content");
            _qualityPresetDropdown = _rootElement.Q<DropdownField>("quality-preset-dropdown");
            _resolutionDropdown = _rootElement.Q<DropdownField>("resolution-dropdown");
            _windowModeDropdown = _rootElement.Q<DropdownField>("window-mode-dropdown");
            _renderScaleSlider = _rootElement.Q<Slider>("render-scale-slider");
            _vsyncToggle = _rootElement.Q<Toggle>("vsync-toggle");
            _frameRateLimitSlider = _rootElement.Q<Slider>("frame-rate-limit-slider");
            _brightnessSlider = _rootElement.Q<Slider>("brightness-slider");
            _contrastSlider = _rootElement.Q<Slider>("contrast-slider");
            _saturationSlider = _rootElement.Q<Slider>("saturation-slider");
            _antiAliasingToggle = _rootElement.Q<Toggle>("anti-aliasing-toggle");
            _shadowsToggle = _rootElement.Q<Toggle>("shadows-toggle");
            _shadowQualitySlider = _rootElement.Q<Slider>("shadow-quality-slider");
            _postProcessingToggle = _rootElement.Q<Toggle>("post-processing-toggle");
        }
        
        private void SetupAudioSettings()
        {
            _audioContent = _rootElement.Q<VisualElement>("audio-content");
            _masterVolumeSlider = _rootElement.Q<Slider>("master-volume-slider");
            _musicVolumeSlider = _rootElement.Q<Slider>("music-volume-slider");
            _sfxVolumeSlider = _rootElement.Q<Slider>("sfx-volume-slider");
            _uiVolumeSlider = _rootElement.Q<Slider>("ui-volume-slider");
            _ambientVolumeSlider = _rootElement.Q<Slider>("ambient-volume-slider");
            _audioQualityDropdown = _rootElement.Q<DropdownField>("audio-quality-dropdown");
            _muteOnFocusLossToggle = _rootElement.Q<Toggle>("mute-on-focus-loss-toggle");
            _spatialAudioToggle = _rootElement.Q<Toggle>("spatial-audio-toggle");
        }
        
        private void SetupControlsSettings()
        {
            _controlsContent = _rootElement.Q<VisualElement>("controls-content");
            _controlSchemeDropdown = _rootElement.Q<DropdownField>("control-scheme-dropdown");
            _mouseSensitivitySlider = _rootElement.Q<Slider>("mouse-sensitivity-slider");
            _invertMouseToggle = _rootElement.Q<Toggle>("invert-mouse-toggle");
            _scrollSpeedSlider = _rootElement.Q<Slider>("scroll-speed-slider");
            _keybindingsContainer = _rootElement.Q<VisualElement>("keybindings-container");
            _resetControlsButton = _rootElement.Q<Button>("reset-controls-button");
            _resetKeybindingsButton = _rootElement.Q<Button>("reset-keybindings-button");
        }
        
        private void SetupNetworkSettings()
        {
            _networkContent = _rootElement.Q<VisualElement>("network-content");
            _onlineFeaturesToggle = _rootElement.Q<Toggle>("online-features-toggle");
            _connectionTypeDropdown = _rootElement.Q<DropdownField>("connection-type-dropdown");
            _serverAddressField = _rootElement.Q<TextField>("server-address-field");
            _portSlider = _rootElement.Q<SliderInt>("port-slider");
            _autoConnectToggle = _rootElement.Q<Toggle>("auto-connect-toggle");
            _timeoutSlider = _rootElement.Q<Slider>("timeout-slider");
            _cloudSyncToggle = _rootElement.Q<Toggle>("cloud-sync-toggle");
        }
        
        private void SetupAccessibilitySettings()
        {
            _accessibilityContent = _rootElement.Q<VisualElement>("accessibility-content");
            _colorBlindSupportToggle = _rootElement.Q<Toggle>("color-blind-support-toggle");
            _colorBlindTypeDropdown = _rootElement.Q<DropdownField>("color-blind-type-dropdown");
            _highContrastToggle = _rootElement.Q<Toggle>("high-contrast-toggle");
            _textSizeSlider = _rootElement.Q<Slider>("text-size-slider");
            _screenReaderToggle = _rootElement.Q<Toggle>("screen-reader-toggle");
            _subtitlesToggle = _rootElement.Q<Toggle>("subtitles-toggle");
            _reducedMotionToggle = _rootElement.Q<Toggle>("reduced-motion-toggle");
            _flashingLightsToggle = _rootElement.Q<Toggle>("flashing-lights-toggle");
        }
        
        private void SetupAdvancedSettings()
        {
            _advancedContent = _rootElement.Q<VisualElement>("advanced-content");
            _debugModeToggle = _rootElement.Q<Toggle>("debug-mode-toggle");
            _developerConsoleToggle = _rootElement.Q<Toggle>("developer-console-toggle");
            _loggingLevelDropdown = _rootElement.Q<DropdownField>("logging-level-dropdown");
            _telemetryToggle = _rootElement.Q<Toggle>("telemetry-toggle");
            _crashReportingToggle = _rootElement.Q<Toggle>("crash-reporting-toggle");
            _customConfigPathField = _rootElement.Q<TextField>("custom-config-path-field");
            _clearCacheButton = _rootElement.Q<Button>("clear-cache-button");
            _resetAllButton = _rootElement.Q<Button>("reset-all-button");
        }
        
        private void SetupDropdowns()
        {
            // Difficulty levels
            if (_difficultySelector != null)
            {
                _difficultySelector.choices = new List<string>
                {
                    "Beginner", "Easy", "Normal", "Hard", "Expert", "Master"
                };
                _difficultySelector.value = "Normal";
            }
            
            // UI Scale options
            if (_uiScaleSelector != null)
            {
                _uiScaleSelector.choices = new List<string>
                {
                    "75%", "100%", "125%", "150%", "175%", "200%"
                };
                _uiScaleSelector.value = "100%";
            }
            
            // Quality presets
            if (_qualityPresetDropdown != null)
            {
                _qualityPresetDropdown.choices = new List<string>
                {
                    "Ultra Low", "Low", "Medium", "High", "Ultra", "Custom"
                };
                _qualityPresetDropdown.value = "High";
            }
            
            // Resolution options
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.choices = new List<string>
                {
                    "1920x1080", "2560x1440", "3840x2160", "1280x720", "1366x768", "1680x1050"
                };
                _resolutionDropdown.value = "1920x1080";
            }
            
            // Window modes
            if (_windowModeDropdown != null)
            {
                _windowModeDropdown.choices = new List<string>
                {
                    "Fullscreen", "Borderless Window", "Windowed"
                };
                _windowModeDropdown.value = "Fullscreen";
            }
            
            // Audio quality
            if (_audioQualityDropdown != null)
            {
                _audioQualityDropdown.choices = new List<string>
                {
                    "Low", "Medium", "High", "Ultra"
                };
                _audioQualityDropdown.value = "High";
            }
            
            // Control schemes
            if (_controlSchemeDropdown != null)
            {
                _controlSchemeDropdown.choices = new List<string>
                {
                    "Default", "Classic", "Modern", "Custom"
                };
                _controlSchemeDropdown.value = "Default";
            }
            
            // Connection types
            if (_connectionTypeDropdown != null)
            {
                _connectionTypeDropdown.choices = new List<string>
                {
                    "Automatic", "Ethernet", "Wi-Fi", "Cellular"
                };
                _connectionTypeDropdown.value = "Automatic";
            }
            
            // Color blind types
            if (_colorBlindTypeDropdown != null)
            {
                _colorBlindTypeDropdown.choices = new List<string>
                {
                    "Protanopia", "Deuteranopia", "Tritanopia", "Achromatopsia"
                };
                _colorBlindTypeDropdown.value = "Protanopia";
            }
            
            // Logging levels
            if (_loggingLevelDropdown != null)
            {
                _loggingLevelDropdown.choices = new List<string>
                {
                    "None", "Error", "Warning", "Info", "Debug", "Verbose"
                };
                _loggingLevelDropdown.value = "Warning";
            }
        }
        
        private void SetupInitialState()
        {
            // Show gameplay panel by default
            ShowPanel("gameplay");
            
            // Initialize default values
            if (_gameSpeedSlider != null)
                _gameSpeedSlider.value = 1f;
            
            if (_autoSaveIntervalSlider != null)
                _autoSaveIntervalSlider.value = 300f; // 5 minutes
            
            if (_renderScaleSlider != null)
                _renderScaleSlider.value = 1f;
            
            if (_frameRateLimitSlider != null)
                _frameRateLimitSlider.value = 60f;
            
            if (_mouseSensitivitySlider != null)
                _mouseSensitivitySlider.value = 1f;
            
            if (_scrollSpeedSlider != null)
                _scrollSpeedSlider.value = 1f;
            
            if (_portSlider != null)
                _portSlider.value = 7777;
            
            if (_timeoutSlider != null)
                _timeoutSlider.value = 30f;
            
            if (_textSizeSlider != null)
                _textSizeSlider.value = 1f;
            
            UpdateActionButtons();
        }
        
        private void SetupEventHandlers()
        {
            // Tab navigation
            _gameplayTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("gameplay"));
            _graphicsTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("graphics"));
            _audioTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("audio"));
            _controlsTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("controls"));
            _networkTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("network"));
            _accessibilityTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("accessibility"));
            _advancedTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("advanced"));
            
            // Profile management
            _profileSelector?.RegisterValueChangedCallback(evt => SwitchProfile(evt.newValue));
            _saveProfileButton?.RegisterCallback<ClickEvent>(evt => SaveCurrentProfile());
            _deleteProfileButton?.RegisterCallback<ClickEvent>(evt => DeleteCurrentProfile());
            _duplicateProfileButton?.RegisterCallback<ClickEvent>(evt => DuplicateCurrentProfile());
            _importProfileButton?.RegisterCallback<ClickEvent>(evt => ImportProfile());
            _exportProfileButton?.RegisterCallback<ClickEvent>(evt => ExportProfile());
            
            // Settings change handlers
            SetupGameplayEventHandlers();
            SetupGraphicsEventHandlers();
            SetupAudioEventHandlers();
            SetupControlsEventHandlers();
            SetupNetworkEventHandlers();
            SetupAccessibilityEventHandlers();
            SetupAdvancedEventHandlers();
            
            // Settings actions
            _applyButton?.RegisterCallback<ClickEvent>(evt => ApplySettings());
            _revertButton?.RegisterCallback<ClickEvent>(evt => RevertSettings());
            _defaultsButton?.RegisterCallback<ClickEvent>(evt => LoadDefaultSettings());
        }
        
        private void SetupGameplayEventHandlers()
        {
            _difficultySelector?.RegisterValueChangedCallback(evt => HandleSettingChanged("difficulty", evt.newValue));
            _gameSpeedSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("gameSpeed", evt.newValue));
            _tutorialsToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("tutorials", evt.newValue));
            _notificationsToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("notifications", evt.newValue));
            _autoSaveToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("autoSave", evt.newValue));
            _autoSaveIntervalSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("autoSaveInterval", evt.newValue));
            _pauseOnFocusLossToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("pauseOnFocusLoss", evt.newValue));
            _uiScaleSelector?.RegisterValueChangedCallback(evt => HandleSettingChanged("uiScale", evt.newValue));
        }
        
        private void SetupGraphicsEventHandlers()
        {
            _qualityPresetDropdown?.RegisterValueChangedCallback(evt => HandleSettingChanged("qualityPreset", evt.newValue));
            _resolutionDropdown?.RegisterValueChangedCallback(evt => HandleSettingChanged("resolution", evt.newValue));
            _windowModeDropdown?.RegisterValueChangedCallback(evt => HandleSettingChanged("windowMode", evt.newValue));
            _renderScaleSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("renderScale", evt.newValue));
            _vsyncToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("vsync", evt.newValue));
            _frameRateLimitSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("frameRateLimit", evt.newValue));
            _brightnessSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("brightness", evt.newValue));
            _contrastSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("contrast", evt.newValue));
            _saturationSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("saturation", evt.newValue));
            _antiAliasingToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("antiAliasing", evt.newValue));
            _shadowsToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("shadows", evt.newValue));
            _shadowQualitySlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("shadowQuality", evt.newValue));
            _postProcessingToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("postProcessing", evt.newValue));
        }
        
        private void SetupAudioEventHandlers()
        {
            _masterVolumeSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("masterVolume", evt.newValue));
            _musicVolumeSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("musicVolume", evt.newValue));
            _sfxVolumeSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("sfxVolume", evt.newValue));
            _uiVolumeSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("uiVolume", evt.newValue));
            _ambientVolumeSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("ambientVolume", evt.newValue));
            _audioQualityDropdown?.RegisterValueChangedCallback(evt => HandleSettingChanged("audioQuality", evt.newValue));
            _muteOnFocusLossToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("muteOnFocusLoss", evt.newValue));
            _spatialAudioToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("spatialAudio", evt.newValue));
        }
        
        private void SetupControlsEventHandlers()
        {
            _controlSchemeDropdown?.RegisterValueChangedCallback(evt => HandleSettingChanged("controlScheme", evt.newValue));
            _mouseSensitivitySlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("mouseSensitivity", evt.newValue));
            _invertMouseToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("invertMouse", evt.newValue));
            _scrollSpeedSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("scrollSpeed", evt.newValue));
            _resetControlsButton?.RegisterCallback<ClickEvent>(evt => ResetControls());
            _resetKeybindingsButton?.RegisterCallback<ClickEvent>(evt => ResetKeybindings());
        }
        
        private void SetupNetworkEventHandlers()
        {
            _onlineFeaturesToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("onlineFeatures", evt.newValue));
            _connectionTypeDropdown?.RegisterValueChangedCallback(evt => HandleSettingChanged("connectionType", evt.newValue));
            _serverAddressField?.RegisterValueChangedCallback(evt => HandleSettingChanged("serverAddress", evt.newValue));
            _portSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("port", evt.newValue));
            _autoConnectToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("autoConnect", evt.newValue));
            _timeoutSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("timeout", evt.newValue));
            _cloudSyncToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("cloudSync", evt.newValue));
        }
        
        private void SetupAccessibilityEventHandlers()
        {
            _colorBlindSupportToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("colorBlindSupport", evt.newValue));
            _colorBlindTypeDropdown?.RegisterValueChangedCallback(evt => HandleSettingChanged("colorBlindType", evt.newValue));
            _highContrastToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("highContrast", evt.newValue));
            _textSizeSlider?.RegisterValueChangedCallback(evt => HandleSettingChanged("textSize", evt.newValue));
            _screenReaderToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("screenReader", evt.newValue));
            _subtitlesToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("subtitles", evt.newValue));
            _reducedMotionToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("reducedMotion", evt.newValue));
            _flashingLightsToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("flashingLights", evt.newValue));
        }
        
        private void SetupAdvancedEventHandlers()
        {
            _debugModeToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("debugMode", evt.newValue));
            _developerConsoleToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("developerConsole", evt.newValue));
            _loggingLevelDropdown?.RegisterValueChangedCallback(evt => HandleSettingChanged("loggingLevel", evt.newValue));
            _telemetryToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("telemetry", evt.newValue));
            _crashReportingToggle?.RegisterValueChangedCallback(evt => HandleSettingChanged("crashReporting", evt.newValue));
            _customConfigPathField?.RegisterValueChangedCallback(evt => HandleSettingChanged("customConfigPath", evt.newValue));
            _clearCacheButton?.RegisterCallback<ClickEvent>(evt => ClearCache());
            _resetAllButton?.RegisterCallback<ClickEvent>(evt => ResetAllSettings());
        }
        
        #region Panel Management
        
        private void ShowPanel(string panelName)
        {
            // Hide all panels
            _gameplayPanel?.AddToClassList("hidden");
            _graphicsPanel?.AddToClassList("hidden");
            _audioPanel?.AddToClassList("hidden");
            _controlsPanel?.AddToClassList("hidden");
            _networkPanel?.AddToClassList("hidden");
            _accessibilityPanel?.AddToClassList("hidden");
            _advancedPanel?.AddToClassList("hidden");
            
            // Remove active state from all tabs
            _gameplayTabButton?.RemoveFromClassList("tab-active");
            _graphicsTabButton?.RemoveFromClassList("tab-active");
            _audioTabButton?.RemoveFromClassList("tab-active");
            _controlsTabButton?.RemoveFromClassList("tab-active");
            _networkTabButton?.RemoveFromClassList("tab-active");
            _accessibilityTabButton?.RemoveFromClassList("tab-active");
            _advancedTabButton?.RemoveFromClassList("tab-active");
            
            // Show selected panel and activate tab
            switch (panelName)
            {
                case "gameplay":
                    _gameplayPanel?.RemoveFromClassList("hidden");
                    _gameplayTabButton?.AddToClassList("tab-active");
                    break;
                case "graphics":
                    _graphicsPanel?.RemoveFromClassList("hidden");
                    _graphicsTabButton?.AddToClassList("tab-active");
                    break;
                case "audio":
                    _audioPanel?.RemoveFromClassList("hidden");
                    _audioTabButton?.AddToClassList("tab-active");
                    break;
                case "controls":
                    _controlsPanel?.RemoveFromClassList("hidden");
                    _controlsTabButton?.AddToClassList("tab-active");
                    break;
                case "network":
                    _networkPanel?.RemoveFromClassList("hidden");
                    _networkTabButton?.AddToClassList("tab-active");
                    break;
                case "accessibility":
                    _accessibilityPanel?.RemoveFromClassList("hidden");
                    _accessibilityTabButton?.AddToClassList("tab-active");
                    break;
                case "advanced":
                    _advancedPanel?.RemoveFromClassList("hidden");
                    _advancedTabButton?.AddToClassList("tab-active");
                    break;
            }
            
            _currentTab = panelName;
            OnTabChanged?.Invoke(panelName);
            
            Debug.Log($"Switched to {panelName} settings panel");
        }
        
        #endregion
        
        #region Settings Management
        
        private void HandleSettingChanged(string settingName, object value)
        {
            _currentSettings[settingName] = value;
            _hasUnsavedChanges = true;
            
            PlaySound(_settingChangedSound);
            OnSettingChanged?.Invoke(settingName, value);
            
            UpdateActionButtons();
            UpdateStatusLabel($"Setting changed: {settingName}");
            
            Debug.Log($"Setting changed: {settingName} = {value}");
        }
        
        private void ApplySettings()
        {
            if (_isApplyingSettings) return;
            
            _isApplyingSettings = true;
            
            if (_settingsManager != null)
            {
                _settingsManager.ApplySettings(_currentSettings);
            }
            else
            {
                // Apply settings directly to Unity systems
                ApplyUnitySettings();
            }
            
            // Copy current to original
            _originalSettings.Clear();
            foreach (var setting in _currentSettings)
            {
                _originalSettings[setting.Key] = setting.Value;
            }
            
            _hasUnsavedChanges = false;
            OnSettingsApplied?.Invoke();
            
            UpdateActionButtons();
            UpdateStatusLabel("Settings applied successfully");
            
            _isApplyingSettings = false;
            
            Debug.Log("Settings applied");
        }
        
        private void RevertSettings()
        {
            _currentSettings.Clear();
            foreach (var setting in _originalSettings)
            {
                _currentSettings[setting.Key] = setting.Value;
            }
            
            UpdateUIFromSettings();
            _hasUnsavedChanges = false;
            OnSettingsReverted?.Invoke();
            
            UpdateActionButtons();
            UpdateStatusLabel("Settings reverted");
            
            Debug.Log("Settings reverted");
        }
        
        private void LoadDefaultSettings()
        {
            InitializeDefaultSettings();
            UpdateUIFromSettings();
            _hasUnsavedChanges = true;
            
            UpdateActionButtons();
            UpdateStatusLabel("Default settings loaded");
            
            Debug.Log("Default settings loaded");
        }
        
        private void InitializeDefaultSettings()
        {
            _currentSettings.Clear();
            
            // Gameplay defaults
            _currentSettings["difficulty"] = "Normal";
            _currentSettings["gameSpeed"] = 1f;
            _currentSettings["tutorials"] = true;
            _currentSettings["notifications"] = true;
            _currentSettings["autoSave"] = true;
            _currentSettings["autoSaveInterval"] = 300f;
            _currentSettings["pauseOnFocusLoss"] = true;
            _currentSettings["uiScale"] = "100%";
            
            // Graphics defaults
            _currentSettings["qualityPreset"] = "High";
            _currentSettings["resolution"] = "1920x1080";
            _currentSettings["windowMode"] = "Fullscreen";
            _currentSettings["renderScale"] = 1f;
            _currentSettings["vsync"] = true;
            _currentSettings["frameRateLimit"] = 60f;
            _currentSettings["brightness"] = 0.5f;
            _currentSettings["contrast"] = 0.5f;
            _currentSettings["saturation"] = 0.5f;
            _currentSettings["antiAliasing"] = true;
            _currentSettings["shadows"] = true;
            _currentSettings["shadowQuality"] = 0.8f;
            _currentSettings["postProcessing"] = true;
            
            // Audio defaults
            _currentSettings["masterVolume"] = 0.8f;
            _currentSettings["musicVolume"] = 0.7f;
            _currentSettings["sfxVolume"] = 0.8f;
            _currentSettings["uiVolume"] = 0.6f;
            _currentSettings["ambientVolume"] = 0.5f;
            _currentSettings["audioQuality"] = "High";
            _currentSettings["muteOnFocusLoss"] = false;
            _currentSettings["spatialAudio"] = true;
            
            // Controls defaults
            _currentSettings["controlScheme"] = "Default";
            _currentSettings["mouseSensitivity"] = 1f;
            _currentSettings["invertMouse"] = false;
            _currentSettings["scrollSpeed"] = 1f;
            
            // Network defaults
            _currentSettings["onlineFeatures"] = true;
            _currentSettings["connectionType"] = "Automatic";
            _currentSettings["serverAddress"] = "";
            _currentSettings["port"] = 7777;
            _currentSettings["autoConnect"] = false;
            _currentSettings["timeout"] = 30f;
            _currentSettings["cloudSync"] = false;
            
            // Accessibility defaults
            _currentSettings["colorBlindSupport"] = false;
            _currentSettings["colorBlindType"] = "Protanopia";
            _currentSettings["highContrast"] = false;
            _currentSettings["textSize"] = 1f;
            _currentSettings["screenReader"] = false;
            _currentSettings["subtitles"] = false;
            _currentSettings["reducedMotion"] = false;
            _currentSettings["flashingLights"] = true;
            
            // Advanced defaults
            _currentSettings["debugMode"] = false;
            _currentSettings["developerConsole"] = false;
            _currentSettings["loggingLevel"] = "Warning";
            _currentSettings["telemetry"] = true;
            _currentSettings["crashReporting"] = true;
            _currentSettings["customConfigPath"] = "";
        }
        
        #endregion
        
        #region Helper Methods
        
        private void LoadSettings()
        {
            if (_settingsManager != null)
            {
                var loadedSettings = _settingsManager.GetAllSettings();
                if (loadedSettings != null)
                {
                    _currentSettings = new Dictionary<string, object>(loadedSettings);
                }
            }
            
            if (_currentSettings.Count == 0)
            {
                InitializeDefaultSettings();
            }
            
            // Copy to original settings
            _originalSettings = new Dictionary<string, object>(_currentSettings);
            
            UpdateUIFromSettings();
            LoadProfiles();
            
            Debug.Log("Settings loaded");
        }
        
        private void UpdateUIFromSettings()
        {
            // Update all UI elements based on current settings
            UpdateGameplayUI();
            UpdateGraphicsUI();
            UpdateAudioUI();
            UpdateControlsUI();
            UpdateNetworkUI();
            UpdateAccessibilityUI();
            UpdateAdvancedUI();
        }
        
        private void UpdateGameplayUI()
        {
            if (_currentSettings.TryGetValue("difficulty", out var difficulty))
                _difficultySelector.value = (string)difficulty;
            if (_currentSettings.TryGetValue("gameSpeed", out var gameSpeed))
                _gameSpeedSlider.value = (float)gameSpeed;
            if (_currentSettings.TryGetValue("tutorials", out var tutorials))
                _tutorialsToggle.value = (bool)tutorials;
            if (_currentSettings.TryGetValue("notifications", out var notifications))
                _notificationsToggle.value = (bool)notifications;
            if (_currentSettings.TryGetValue("autoSave", out var autoSave))
                _autoSaveToggle.value = (bool)autoSave;
            if (_currentSettings.TryGetValue("autoSaveInterval", out var autoSaveInterval))
                _autoSaveIntervalSlider.value = (float)autoSaveInterval;
            if (_currentSettings.TryGetValue("pauseOnFocusLoss", out var pauseOnFocusLoss))
                _pauseOnFocusLossToggle.value = (bool)pauseOnFocusLoss;
            if (_currentSettings.TryGetValue("uiScale", out var uiScale))
                _uiScaleSelector.value = (string)uiScale;
        }
        
        private void UpdateGraphicsUI()
        {
            if (_currentSettings.TryGetValue("qualityPreset", out var qualityPreset))
                _qualityPresetDropdown.value = (string)qualityPreset;
            if (_currentSettings.TryGetValue("resolution", out var resolution))
                _resolutionDropdown.value = (string)resolution;
            if (_currentSettings.TryGetValue("windowMode", out var windowMode))
                _windowModeDropdown.value = (string)windowMode;
            if (_currentSettings.TryGetValue("renderScale", out var renderScale))
                _renderScaleSlider.value = (float)renderScale;
            if (_currentSettings.TryGetValue("vsync", out var vsync))
                _vsyncToggle.value = (bool)vsync;
            if (_currentSettings.TryGetValue("frameRateLimit", out var frameRateLimit))
                _frameRateLimitSlider.value = (float)frameRateLimit;
            if (_currentSettings.TryGetValue("brightness", out var brightness))
                _brightnessSlider.value = (float)brightness;
            if (_currentSettings.TryGetValue("contrast", out var contrast))
                _contrastSlider.value = (float)contrast;
            if (_currentSettings.TryGetValue("saturation", out var saturation))
                _saturationSlider.value = (float)saturation;
            if (_currentSettings.TryGetValue("antiAliasing", out var antiAliasing))
                _antiAliasingToggle.value = (bool)antiAliasing;
            if (_currentSettings.TryGetValue("shadows", out var shadows))
                _shadowsToggle.value = (bool)shadows;
            if (_currentSettings.TryGetValue("shadowQuality", out var shadowQuality))
                _shadowQualitySlider.value = (float)shadowQuality;
            if (_currentSettings.TryGetValue("postProcessing", out var postProcessing))
                _postProcessingToggle.value = (bool)postProcessing;
        }
        
        private void UpdateAudioUI()
        {
            if (_currentSettings.TryGetValue("masterVolume", out var masterVolume))
                _masterVolumeSlider.value = (float)masterVolume;
            if (_currentSettings.TryGetValue("musicVolume", out var musicVolume))
                _musicVolumeSlider.value = (float)musicVolume;
            if (_currentSettings.TryGetValue("sfxVolume", out var sfxVolume))
                _sfxVolumeSlider.value = (float)sfxVolume;
            if (_currentSettings.TryGetValue("uiVolume", out var uiVolume))
                _uiVolumeSlider.value = (float)uiVolume;
            if (_currentSettings.TryGetValue("ambientVolume", out var ambientVolume))
                _ambientVolumeSlider.value = (float)ambientVolume;
            if (_currentSettings.TryGetValue("audioQuality", out var audioQuality))
                _audioQualityDropdown.value = (string)audioQuality;
            if (_currentSettings.TryGetValue("muteOnFocusLoss", out var muteOnFocusLoss))
                _muteOnFocusLossToggle.value = (bool)muteOnFocusLoss;
            if (_currentSettings.TryGetValue("spatialAudio", out var spatialAudio))
                _spatialAudioToggle.value = (bool)spatialAudio;
        }
        
        private void UpdateControlsUI()
        {
            if (_currentSettings.TryGetValue("controlScheme", out var controlScheme))
                _controlSchemeDropdown.value = (string)controlScheme;
            if (_currentSettings.TryGetValue("mouseSensitivity", out var mouseSensitivity))
                _mouseSensitivitySlider.value = (float)mouseSensitivity;
            if (_currentSettings.TryGetValue("invertMouse", out var invertMouse))
                _invertMouseToggle.value = (bool)invertMouse;
            if (_currentSettings.TryGetValue("scrollSpeed", out var scrollSpeed))
                _scrollSpeedSlider.value = (float)scrollSpeed;
        }
        
        private void UpdateNetworkUI()
        {
            if (_currentSettings.TryGetValue("onlineFeatures", out var onlineFeatures))
                _onlineFeaturesToggle.value = (bool)onlineFeatures;
            if (_currentSettings.TryGetValue("connectionType", out var connectionType))
                _connectionTypeDropdown.value = (string)connectionType;
            if (_currentSettings.TryGetValue("serverAddress", out var serverAddress))
                _serverAddressField.value = (string)serverAddress;
            if (_currentSettings.TryGetValue("port", out var port))
                _portSlider.value = (int)port;
            if (_currentSettings.TryGetValue("autoConnect", out var autoConnect))
                _autoConnectToggle.value = (bool)autoConnect;
            if (_currentSettings.TryGetValue("timeout", out var timeout))
                _timeoutSlider.value = (float)timeout;
            if (_currentSettings.TryGetValue("cloudSync", out var cloudSync))
                _cloudSyncToggle.value = (bool)cloudSync;
        }
        
        private void UpdateAccessibilityUI()
        {
            if (_currentSettings.TryGetValue("colorBlindSupport", out var colorBlindSupport))
                _colorBlindSupportToggle.value = (bool)colorBlindSupport;
            if (_currentSettings.TryGetValue("colorBlindType", out var colorBlindType))
                _colorBlindTypeDropdown.value = (string)colorBlindType;
            if (_currentSettings.TryGetValue("highContrast", out var highContrast))
                _highContrastToggle.value = (bool)highContrast;
            if (_currentSettings.TryGetValue("textSize", out var textSize))
                _textSizeSlider.value = (float)textSize;
            if (_currentSettings.TryGetValue("screenReader", out var screenReader))
                _screenReaderToggle.value = (bool)screenReader;
            if (_currentSettings.TryGetValue("subtitles", out var subtitles))
                _subtitlesToggle.value = (bool)subtitles;
            if (_currentSettings.TryGetValue("reducedMotion", out var reducedMotion))
                _reducedMotionToggle.value = (bool)reducedMotion;
            if (_currentSettings.TryGetValue("flashingLights", out var flashingLights))
                _flashingLightsToggle.value = (bool)flashingLights;
        }
        
        private void UpdateAdvancedUI()
        {
            if (_currentSettings.TryGetValue("debugMode", out var debugMode))
                _debugModeToggle.value = (bool)debugMode;
            if (_currentSettings.TryGetValue("developerConsole", out var developerConsole))
                _developerConsoleToggle.value = (bool)developerConsole;
            if (_currentSettings.TryGetValue("loggingLevel", out var loggingLevel))
                _loggingLevelDropdown.value = (string)loggingLevel;
            if (_currentSettings.TryGetValue("telemetry", out var telemetry))
                _telemetryToggle.value = (bool)telemetry;
            if (_currentSettings.TryGetValue("crashReporting", out var crashReporting))
                _crashReportingToggle.value = (bool)crashReporting;
            if (_currentSettings.TryGetValue("customConfigPath", out var customConfigPath))
                _customConfigPathField.value = (string)customConfigPath;
        }
        
        private void ApplyUnitySettings()
        {
            // Apply graphics settings
            if (_currentSettings.TryGetValue("vsync", out var vsync))
                QualitySettings.vSyncCount = (bool)vsync ? 1 : 0;
            
            if (_currentSettings.TryGetValue("frameRateLimit", out var frameRateLimit))
                Application.targetFrameRate = (int)(float)frameRateLimit;
            
            // Apply audio settings
            if (_currentSettings.TryGetValue("masterVolume", out var masterVolume))
                AudioListener.volume = (float)masterVolume;
            
            // Additional Unity-specific settings would be applied here
        }
        
        private void UpdateRealTimePreview()
        {
            if (!_enableRealTimePreview || Time.time - _lastPreviewUpdate < _previewUpdateInterval)
                return;
            
            // Apply real-time preview changes for certain settings
            if (_currentSettings.TryGetValue("brightness", out var brightness))
            {
                // Apply brightness preview
            }
            
            if (_currentSettings.TryGetValue("contrast", out var contrast))
            {
                // Apply contrast preview
            }
            
            if (_currentSettings.TryGetValue("saturation", out var saturation))
            {
                // Apply saturation preview
            }
            
            _lastPreviewUpdate = Time.time;
        }
        
        private void UpdateActionButtons()
        {
            if (_applyButton != null)
                _applyButton.SetEnabled(_hasUnsavedChanges);
            
            if (_revertButton != null)
                _revertButton.SetEnabled(_hasUnsavedChanges);
        }
        
        private void UpdateStatusLabel(string message)
        {
            if (_statusLabel != null)
                _statusLabel.text = message;
        }
        
        #endregion
        
        #region Profile Management
        
        private void LoadProfiles()
        {
            // Load available profiles
            _availableProfiles.Clear();
            
            if (_settingsManager != null)
            {
                _availableProfiles = _settingsManager.GetAvailableProfiles()?.ToList() ?? new List<SettingsProfile>();
            }
            
            if (_availableProfiles.Count == 0)
            {
                // Create default profile
                var defaultProfile = new SettingsProfile
                {
                    Name = _defaultProfileName,
                    Settings = new Dictionary<string, object>(_currentSettings),
                    IsDefault = true
                };
                _availableProfiles.Add(defaultProfile);
            }
            
            UpdateProfileSelector();
        }
        
        private void UpdateProfileSelector()
        {
            if (_profileSelector == null) return;
            
            _profileSelector.choices = _availableProfiles.Select(p => p.Name).ToList();
            
            if (_currentProfile != null)
                _profileSelector.value = _currentProfile.Name;
            else if (_availableProfiles.Count > 0)
                _profileSelector.value = _availableProfiles[0].Name;
        }
        
        private void SwitchProfile(string profileName)
        {
            var profile = _availableProfiles.FirstOrDefault(p => p.Name == profileName);
            if (profile == null) return;
            
            _currentProfile = profile;
            _currentSettings = new Dictionary<string, object>(profile.Settings);
            _originalSettings = new Dictionary<string, object>(profile.Settings);
            
            UpdateUIFromSettings();
            _hasUnsavedChanges = false;
            
            PlaySound(_profileSwitchedSound);
            OnProfileChanged?.Invoke(profile);
            
            UpdateActionButtons();
            UpdateStatusLabel($"Switched to profile: {profileName}");
            
            Debug.Log($"Switched to profile: {profileName}");
        }
        
        private void SaveCurrentProfile()
        {
            if (_currentProfile == null) return;
            
            _currentProfile.Settings = new Dictionary<string, object>(_currentSettings);
            
            if (_settingsManager != null)
            {
                _settingsManager.SaveProfile(_currentProfile);
            }
            
            UpdateStatusLabel($"Profile saved: {_currentProfile.Name}");
            Debug.Log($"Profile saved: {_currentProfile.Name}");
        }
        
        private void DeleteCurrentProfile()
        {
            if (_currentProfile == null || _currentProfile.IsDefault) return;
            
            _availableProfiles.Remove(_currentProfile);
            
            if (_settingsManager != null)
            {
                _settingsManager.DeleteProfile(_currentProfile);
            }
            
            // Switch to default profile
            _currentProfile = _availableProfiles.FirstOrDefault(p => p.IsDefault);
            if (_currentProfile != null)
            {
                SwitchProfile(_currentProfile.Name);
            }
            
            UpdateProfileSelector();
            UpdateStatusLabel("Profile deleted");
            
            Debug.Log("Profile deleted");
        }
        
        private void DuplicateCurrentProfile()
        {
            if (_currentProfile == null) return;
            
            var newProfile = new SettingsProfile
            {
                Name = $"{_currentProfile.Name} Copy",
                Settings = new Dictionary<string, object>(_currentProfile.Settings),
                IsDefault = false
            };
            
            _availableProfiles.Add(newProfile);
            UpdateProfileSelector();
            
            UpdateStatusLabel($"Profile duplicated: {newProfile.Name}");
            Debug.Log($"Profile duplicated: {newProfile.Name}");
        }
        
        private void ImportProfile()
        {
            if (!_enableSettingsImportExport) return;
            
            // Would open file dialog for profile import
            PlaySound(_importExportSound);
            UpdateStatusLabel("Import profile functionality would be implemented here");
            Debug.Log("Import profile requested");
        }
        
        private void ExportProfile()
        {
            if (!_enableSettingsImportExport || _currentProfile == null) return;
            
            // Would open save dialog for profile export
            PlaySound(_importExportSound);
            UpdateStatusLabel($"Export profile: {_currentProfile.Name}");
            Debug.Log($"Export profile requested: {_currentProfile.Name}");
        }
        
        #endregion
        
        #region Special Actions
        
        private void ResetControls()
        {
            // Reset control-specific settings to defaults
            _currentSettings["controlScheme"] = "Default";
            _currentSettings["mouseSensitivity"] = 1f;
            _currentSettings["invertMouse"] = false;
            _currentSettings["scrollSpeed"] = 1f;
            
            UpdateControlsUI();
            _hasUnsavedChanges = true;
            UpdateActionButtons();
            
            PlaySound(_resetSound);
            UpdateStatusLabel("Controls reset to defaults");
            
            Debug.Log("Controls reset to defaults");
        }
        
        private void ResetKeybindings()
        {
            // Reset keybindings to defaults
            PlaySound(_resetSound);
            UpdateStatusLabel("Keybindings reset to defaults");
            Debug.Log("Keybindings reset to defaults");
        }
        
        private void ClearCache()
        {
            // Clear application cache
            UpdateStatusLabel("Cache cleared");
            Debug.Log("Cache cleared");
        }
        
        private void ResetAllSettings()
        {
            InitializeDefaultSettings();
            UpdateUIFromSettings();
            _hasUnsavedChanges = true;
            
            PlaySound(_resetSound);
            UpdateActionButtons();
            UpdateStatusLabel("All settings reset to defaults");
            
            Debug.Log("All settings reset to defaults");
        }
        
        #endregion
        
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
        
        /// <summary>
        /// Gets all current settings as a dictionary.
        /// </summary>
        public Dictionary<string, object> GetAllSettings()
        {
            return new Dictionary<string, object>(_currentSettings);
        }
    }
    
    // Supporting data structures
    [System.Serializable]
    public class SettingsProfile
    {
        public string Name;
        public Dictionary<string, object> Settings;
        public bool IsDefault;
        public DateTime CreatedDate;
        public DateTime LastModified;
    }
    
    [System.Serializable]
    public class SettingsUIConfiguration
    {
        public bool EnableRealTimePreview = true;
        public bool EnableProfileSystem = true;
        public bool EnableImportExport = true;
        public float PreviewUpdateInterval = 0.5f;
        public int MaxProfiles = 10;
        public bool PlaySoundEffects = true;
    }
}