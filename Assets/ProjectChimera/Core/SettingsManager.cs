using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectChimera.Core
{
    /// <summary>
    /// Manages player preferences and game settings for Project Chimera.
    /// Handles graphics, audio, gameplay, and accessibility settings with real-time application.
    /// </summary>
    public class SettingsManager : ChimeraManager, IGameStateListener
    {
        [Header("Settings Configuration")]
        [SerializeField] private SettingsManagerConfigSO _config;
        [SerializeField] private bool _autoApplySettings = true;
        [SerializeField] private bool _validateSettings = true;

        [Header("Settings Events")]
        [SerializeField] private StringGameEventSO _onSettingChanged;
        [SerializeField] private SimpleGameEventSO _onSettingsLoaded;
        [SerializeField] private SimpleGameEventSO _onSettingsReset;
        [SerializeField] private StringGameEventSO _onSettingsError;

        // Settings categories
        private readonly Dictionary<SettingsCategory, Dictionary<string, object>> _settings = new Dictionary<SettingsCategory, Dictionary<string, object>>();
        private readonly Dictionary<string, SettingDefinition> _settingDefinitions = new Dictionary<string, SettingDefinition>();
        private readonly List<ISettingsListener> _settingsListeners = new List<ISettingsListener>();

        // Performance tracking
        private int _settingsChangedThisSession = 0;
        private float _lastSettingsAppliedTime = 0.0f;

        /// <summary>
        /// Number of settings changed this session.
        /// </summary>
        public int SettingsChangedThisSession => _settingsChangedThisSession;

        /// <summary>
        /// Time when settings were last applied.
        /// </summary>
        public float LastSettingsAppliedTime => _lastSettingsAppliedTime;

        /// <summary>
        /// Whether settings are automatically applied when changed.
        /// </summary>
        public bool AutoApplySettings => _autoApplySettings;

        protected override void OnManagerInitialize()
        {
            LogDebug("Initializing Settings Manager");

            // Load configuration
            if (_config != null)
            {
                _autoApplySettings = _config.AutoApplySettings;
                _validateSettings = _config.ValidateSettings;
            }

            // Initialize settings categories
            InitializeSettingsCategories();

            // Define default settings
            DefineDefaultSettings();

            // Load settings from PlayerPrefs
            LoadSettings();

            // Apply initial settings
            if (_autoApplySettings)
            {
                ApplyAllSettings();
            }

            _onSettingsLoaded?.Raise();
            LogDebug($"Settings Manager initialized - {_settingDefinitions.Count} settings defined");
        }

        protected override void OnManagerShutdown()
        {
            LogDebug("Shutting down Settings Manager");

            // Save current settings
            SaveSettings();

            // Clear all data
            _settings.Clear();
            _settingDefinitions.Clear();
            _settingsListeners.Clear();

            _settingsChangedThisSession = 0;
        }

        /// <summary>
        /// Initializes all settings categories.
        /// </summary>
        private void InitializeSettingsCategories()
        {
            foreach (SettingsCategory category in Enum.GetValues(typeof(SettingsCategory)))
            {
                _settings[category] = new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Defines all default game settings.
        /// </summary>
        private void DefineDefaultSettings()
        {
            // Graphics Settings
            DefineSettingFloat("graphics.quality", 1.0f, 0.0f, 1.0f, SettingsCategory.Graphics, "Overall graphics quality");
            DefineSettingInt("graphics.resolution_width", 1920, 800, 3840, SettingsCategory.Graphics, "Screen resolution width");
            DefineSettingInt("graphics.resolution_height", 1080, 600, 2160, SettingsCategory.Graphics, "Screen resolution height");
            DefineSettingBool("graphics.fullscreen", true, SettingsCategory.Graphics, "Fullscreen mode");
            DefineSettingBool("graphics.vsync", true, SettingsCategory.Graphics, "Vertical synchronization");
            DefineSettingFloat("graphics.render_scale", 1.0f, 0.5f, 2.0f, SettingsCategory.Graphics, "Render scale multiplier");
            DefineSettingInt("graphics.target_framerate", 60, 30, 120, SettingsCategory.Graphics, "Target frame rate");

            // Audio Settings
            DefineSettingFloat("audio.master_volume", 1.0f, 0.0f, 1.0f, SettingsCategory.Audio, "Master volume");
            DefineSettingFloat("audio.music_volume", 0.8f, 0.0f, 1.0f, SettingsCategory.Audio, "Music volume");
            DefineSettingFloat("audio.sfx_volume", 1.0f, 0.0f, 1.0f, SettingsCategory.Audio, "Sound effects volume");
            DefineSettingFloat("audio.ui_volume", 1.0f, 0.0f, 1.0f, SettingsCategory.Audio, "UI sound volume");
            DefineSettingBool("audio.mute_on_focus_loss", false, SettingsCategory.Audio, "Mute audio when game loses focus");

            // Gameplay Settings
            DefineSettingFloat("gameplay.time_scale_default", 1.0f, 0.1f, 10.0f, SettingsCategory.Gameplay, "Default time scale");
            DefineSettingFloat("gameplay.time_scale_max", 100.0f, 1.0f, 1000.0f, SettingsCategory.Gameplay, "Maximum time scale");
            DefineSettingBool("gameplay.auto_save", true, SettingsCategory.Gameplay, "Enable auto-save");
            DefineSettingFloat("gameplay.auto_save_interval", 300.0f, 60.0f, 1800.0f, SettingsCategory.Gameplay, "Auto-save interval in seconds");
            DefineSettingBool("gameplay.pause_on_focus_loss", true, SettingsCategory.Gameplay, "Pause game when it loses focus");
            DefineSettingBool("gameplay.show_tooltips", true, SettingsCategory.Gameplay, "Show gameplay tooltips");

            // Control Settings
            DefineSettingFloat("controls.mouse_sensitivity", 1.0f, 0.1f, 5.0f, SettingsCategory.Controls, "Mouse sensitivity");
            DefineSettingBool("controls.invert_mouse_y", false, SettingsCategory.Controls, "Invert mouse Y axis");
            DefineSettingFloat("controls.scroll_speed", 1.0f, 0.1f, 3.0f, SettingsCategory.Controls, "Scroll wheel speed");
            DefineSettingBool("controls.edge_scrolling", true, SettingsCategory.Controls, "Enable edge scrolling");

            // Accessibility Settings
            DefineSettingFloat("accessibility.ui_scale", 1.0f, 0.8f, 2.0f, SettingsCategory.Accessibility, "UI scale factor");
            DefineSettingBool("accessibility.high_contrast", false, SettingsCategory.Accessibility, "High contrast mode");
            DefineSettingBool("accessibility.colorblind_support", false, SettingsCategory.Accessibility, "Colorblind accessibility");
            DefineSettingFloat("accessibility.font_size_multiplier", 1.0f, 0.8f, 2.0f, SettingsCategory.Accessibility, "Font size multiplier");
            DefineSettingBool("accessibility.reduce_motion", false, SettingsCategory.Accessibility, "Reduce motion effects");

            // Debug Settings
            DefineSettingBool("debug.show_fps", false, SettingsCategory.Debug, "Show FPS counter");
            DefineSettingBool("debug.show_memory", false, SettingsCategory.Debug, "Show memory usage");
            DefineSettingBool("debug.enable_logging", true, SettingsCategory.Debug, "Enable debug logging");
            DefineSettingInt("debug.log_level", 3, 0, 5, SettingsCategory.Debug, "Debug log level");
        }

        /// <summary>
        /// Defines a boolean setting.
        /// </summary>
        private void DefineSettingBool(string key, bool defaultValue, SettingsCategory category, string description)
        {
            var definition = new SettingDefinition
            {
                Key = key,
                Type = typeof(bool),
                DefaultValue = defaultValue,
                Category = category,
                Description = description
            };

            _settingDefinitions[key] = definition;
            _settings[category][key] = defaultValue;
        }

        /// <summary>
        /// Defines an integer setting with min/max bounds.
        /// </summary>
        private void DefineSettingInt(string key, int defaultValue, int minValue, int maxValue, SettingsCategory category, string description)
        {
            var definition = new SettingDefinition
            {
                Key = key,
                Type = typeof(int),
                DefaultValue = defaultValue,
                MinValue = minValue,
                MaxValue = maxValue,
                Category = category,
                Description = description
            };

            _settingDefinitions[key] = definition;
            _settings[category][key] = defaultValue;
        }

        /// <summary>
        /// Defines a float setting with min/max bounds.
        /// </summary>
        private void DefineSettingFloat(string key, float defaultValue, float minValue, float maxValue, SettingsCategory category, string description)
        {
            var definition = new SettingDefinition
            {
                Key = key,
                Type = typeof(float),
                DefaultValue = defaultValue,
                MinValue = minValue,
                MaxValue = maxValue,
                Category = category,
                Description = description
            };

            _settingDefinitions[key] = definition;
            _settings[category][key] = defaultValue;
        }

        /// <summary>
        /// Gets a boolean setting value.
        /// </summary>
        public bool GetBool(string key, bool defaultValue = false)
        {
            if (TryGetSetting(key, out object value) && value is bool boolValue)
            {
                return boolValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets an integer setting value.
        /// </summary>
        public int GetInt(string key, int defaultValue = 0)
        {
            if (TryGetSetting(key, out object value) && value is int intValue)
            {
                return intValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a float setting value.
        /// </summary>
        public float GetFloat(string key, float defaultValue = 0.0f)
        {
            if (TryGetSetting(key, out object value) && value is float floatValue)
            {
                return floatValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a string setting value.
        /// </summary>
        public string GetString(string key, string defaultValue = "")
        {
            if (TryGetSetting(key, out object value) && value is string stringValue)
            {
                return stringValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Sets a setting value and optionally applies it immediately.
        /// </summary>
        public void SetSetting(string key, object value, bool applyImmediately = true)
        {
            if (!_settingDefinitions.TryGetValue(key, out SettingDefinition definition))
            {
                LogWarning($"Unknown setting key: {key}");
                return;
            }

            // Validate and clamp value
            object validatedValue = ValidateAndClampValue(value, definition);
            
            // Update the setting
            _settings[definition.Category][key] = validatedValue;
            _settingsChangedThisSession++;

            LogDebug($"Setting changed: {key} = {validatedValue}");

            // Notify listeners
            NotifySettingsListeners(key, validatedValue);
            _onSettingChanged?.Raise($"{key}:{validatedValue}");

            // Apply the setting if requested
            if (applyImmediately && _autoApplySettings)
            {
                ApplySetting(key, validatedValue);
            }
        }

        /// <summary>
        /// Tries to get a setting value.
        /// </summary>
        private bool TryGetSetting(string key, out object value)
        {
            value = null;

            if (!_settingDefinitions.TryGetValue(key, out SettingDefinition definition))
            {
                return false;
            }

            return _settings[definition.Category].TryGetValue(key, out value);
        }

        /// <summary>
        /// Validates and clamps a setting value according to its definition.
        /// </summary>
        private object ValidateAndClampValue(object value, SettingDefinition definition)
        {
            if (value == null) return definition.DefaultValue;

            try
            {
                // Convert to the correct type
                object convertedValue = Convert.ChangeType(value, definition.Type);

                // Apply bounds for numeric types
                if (definition.Type == typeof(int))
                {
                    int intValue = (int)convertedValue;
                    if (definition.MinValue.HasValue) intValue = Mathf.Max(intValue, (int)definition.MinValue.Value);
                    if (definition.MaxValue.HasValue) intValue = Mathf.Min(intValue, (int)definition.MaxValue.Value);
                    return intValue;
                }
                else if (definition.Type == typeof(float))
                {
                    float floatValue = (float)convertedValue;
                    if (definition.MinValue.HasValue) floatValue = Mathf.Max(floatValue, definition.MinValue.Value);
                    if (definition.MaxValue.HasValue) floatValue = Mathf.Min(floatValue, definition.MaxValue.Value);
                    return floatValue;
                }

                return convertedValue;
            }
            catch (Exception e)
            {
                LogError($"Error validating setting value for {definition.Key}: {e.Message}");
                return definition.DefaultValue;
            }
        }

        /// <summary>
        /// Applies a specific setting to the game systems.
        /// </summary>
        private void ApplySetting(string key, object value)
        {
            try
            {
                switch (key)
                {
                    // Graphics settings
                    case "graphics.quality":
                        QualitySettings.SetQualityLevel(Mathf.RoundToInt(GetFloat(key) * (QualitySettings.names.Length - 1)));
                        break;
                    case "graphics.resolution_width":
                    case "graphics.resolution_height":
                    case "graphics.fullscreen":
                        ApplyResolutionSettings();
                        break;
                    case "graphics.vsync":
                        QualitySettings.vSyncCount = GetBool(key) ? 1 : 0;
                        break;
                    case "graphics.target_framerate":
                        Application.targetFrameRate = GetInt(key);
                        break;

                    // Audio settings
                    case "audio.master_volume":
                        AudioListener.volume = GetFloat(key);
                        break;

                    // Gameplay settings
                    case "gameplay.time_scale_default":
                        var timeManager = GameManager.Instance?.GetManager<TimeManager>();
                        if (timeManager != null)
                        {
                            timeManager.ResetTimeScale();
                        }
                        break;

                    default:
                        // Custom setting application can be handled by listeners
                        break;
                }

                _lastSettingsAppliedTime = Time.time;
            }
            catch (Exception e)
            {
                LogError($"Error applying setting {key}: {e.Message}");
                _onSettingsError?.Raise($"Failed to apply {key}: {e.Message}");
            }
        }

        /// <summary>
        /// Applies resolution settings.
        /// </summary>
        private void ApplyResolutionSettings()
        {
            int width = GetInt("graphics.resolution_width");
            int height = GetInt("graphics.resolution_height");
            bool fullscreen = GetBool("graphics.fullscreen");

            Screen.SetResolution(width, height, fullscreen);
        }

        /// <summary>
        /// Applies all settings to their respective systems.
        /// </summary>
        public void ApplyAllSettings()
        {
            LogDebug("Applying all settings");

            foreach (var definition in _settingDefinitions.Values)
            {
                if (TryGetSetting(definition.Key, out object value))
                {
                    ApplySetting(definition.Key, value);
                }
            }

            _lastSettingsAppliedTime = Time.time;
            LogDebug("All settings applied");
        }

        /// <summary>
        /// Loads settings from PlayerPrefs.
        /// </summary>
        private void LoadSettings()
        {
            LogDebug("Loading settings from PlayerPrefs");

            foreach (var definition in _settingDefinitions.Values)
            {
                string key = definition.Key;
                
                if (PlayerPrefs.HasKey(key))
                {
                    object value = null;

                    if (definition.Type == typeof(bool))
                    {
                        value = PlayerPrefs.GetInt(key, 0) == 1;
                    }
                    else if (definition.Type == typeof(int))
                    {
                        value = PlayerPrefs.GetInt(key, (int)definition.DefaultValue);
                    }
                    else if (definition.Type == typeof(float))
                    {
                        value = PlayerPrefs.GetFloat(key, (float)definition.DefaultValue);
                    }
                    else if (definition.Type == typeof(string))
                    {
                        value = PlayerPrefs.GetString(key, (string)definition.DefaultValue);
                    }

                    if (value != null)
                    {
                        _settings[definition.Category][key] = ValidateAndClampValue(value, definition);
                    }
                }
            }

            LogDebug("Settings loaded from PlayerPrefs");
        }

        /// <summary>
        /// Saves settings to PlayerPrefs.
        /// </summary>
        private void SaveSettings()
        {
            LogDebug("Saving settings to PlayerPrefs");

            foreach (var definition in _settingDefinitions.Values)
            {
                string key = definition.Key;
                
                if (TryGetSetting(key, out object value))
                {
                    if (definition.Type == typeof(bool))
                    {
                        PlayerPrefs.SetInt(key, (bool)value ? 1 : 0);
                    }
                    else if (definition.Type == typeof(int))
                    {
                        PlayerPrefs.SetInt(key, (int)value);
                    }
                    else if (definition.Type == typeof(float))
                    {
                        PlayerPrefs.SetFloat(key, (float)value);
                    }
                    else if (definition.Type == typeof(string))
                    {
                        PlayerPrefs.SetString(key, (string)value);
                    }
                }
            }

            PlayerPrefs.Save();
            LogDebug("Settings saved to PlayerPrefs");
        }

        /// <summary>
        /// Resets all settings to their default values.
        /// </summary>
        public void ResetAllSettings()
        {
            LogDebug("Resetting all settings to defaults");

            foreach (var definition in _settingDefinitions.Values)
            {
                _settings[definition.Category][definition.Key] = definition.DefaultValue;
            }

            _settingsChangedThisSession = 0;

            if (_autoApplySettings)
            {
                ApplyAllSettings();
            }

            _onSettingsReset?.Raise();
            LogDebug("All settings reset to defaults");
        }

        /// <summary>
        /// Resets settings for a specific category.
        /// </summary>
        public void ResetCategorySettings(SettingsCategory category)
        {
            LogDebug($"Resetting {category} settings to defaults");

            var categoryDefinitions = _settingDefinitions.Values.Where(d => d.Category == category);
            foreach (var definition in categoryDefinitions)
            {
                _settings[category][definition.Key] = definition.DefaultValue;
            }

            if (_autoApplySettings)
            {
                ApplyAllSettings();
            }
        }

        /// <summary>
        /// Gets all settings for a specific category.
        /// </summary>
        public Dictionary<string, object> GetCategorySettings(SettingsCategory category)
        {
            return new Dictionary<string, object>(_settings[category]);
        }

        /// <summary>
        /// Gets all setting definitions for a category.
        /// </summary>
        public List<SettingDefinition> GetCategoryDefinitions(SettingsCategory category)
        {
            return _settingDefinitions.Values.Where(d => d.Category == category).ToList();
        }

        /// <summary>
        /// Registers a settings listener.
        /// </summary>
        public void RegisterSettingsListener(ISettingsListener listener)
        {
            if (listener != null && !_settingsListeners.Contains(listener))
            {
                _settingsListeners.Add(listener);
            }
        }

        /// <summary>
        /// Unregisters a settings listener.
        /// </summary>
        public void UnregisterSettingsListener(ISettingsListener listener)
        {
            _settingsListeners.Remove(listener);
        }

        /// <summary>
        /// Notifies all settings listeners of a change.
        /// </summary>
        private void NotifySettingsListeners(string key, object value)
        {
            for (int i = _settingsListeners.Count - 1; i >= 0; i--)
            {
                try
                {
                    _settingsListeners[i]?.OnSettingChanged(key, value);
                }
                catch (Exception e)
                {
                    LogError($"Error notifying settings listener: {e.Message}");
                    _settingsListeners.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Handles game state changes.
        /// </summary>
        public void OnGameStateChanged(GameState previousState, GameState newState)
        {
            switch (newState)
            {
                case GameState.Shutdown:
                    SaveSettings();
                    break;
            }
        }
    }

    /// <summary>
    /// Settings categories for organization.
    /// </summary>
    public enum SettingsCategory
    {
        Graphics,
        Audio,
        Gameplay,
        Controls,
        Accessibility,
        Debug
    }

    /// <summary>
    /// Definition of a setting with metadata.
    /// </summary>
    [Serializable]
    public struct SettingDefinition
    {
        public string Key;
        public Type Type;
        public object DefaultValue;
        public float? MinValue;
        public float? MaxValue;
        public SettingsCategory Category;
        public string Description;
    }

    /// <summary>
    /// Interface for components that need to respond to settings changes.
    /// </summary>
    public interface ISettingsListener
    {
        void OnSettingChanged(string key, object value);
    }

    /// <summary>
    /// Configuration for Settings Manager behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "Settings Manager Config", menuName = "Project Chimera/Core/Settings Manager Config")]
    public class SettingsManagerConfigSO : ChimeraConfigSO
    {
        [Header("General Settings")]
        public bool AutoApplySettings = true;
        public bool ValidateSettings = true;
        public bool SaveOnChange = false;

        [Header("Performance Settings")]
        public bool EnableSettingsCache = true;
        public bool EnableSettingsEvents = true;
        
        [Range(0.1f, 5.0f)]
        public float SettingsApplyDelay = 0.1f;

        [Header("Debug Settings")]
        public bool LogSettingChanges = false;
        public bool EnableSettingsValidation = true;
    }
}