using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Systems.Settings
{
    /// <summary>
    /// Settings Manager for Project Chimera
    /// Provides centralized settings management and persistence
    /// </summary>
    public class SettingsManager : ChimeraManager
    {
        [Header("Settings Configuration")]
        [SerializeField] private bool _enableAutoSave = true;
        [SerializeField] private float _autoSaveInterval = 30f;
        [SerializeField] private string _settingsFileName = "ChimeraSettings.json";
        [SerializeField] private bool _enableCloudSync = false;
        
        // Settings storage
        private Dictionary<string, object> _settings = new Dictionary<string, object>();
        private Dictionary<string, object> _defaultSettings = new Dictionary<string, object>();
        
        // State
        private bool _hasUnsavedChanges = false;
        private float _lastSaveTime;
        
        // Events
        public System.Action<string, object> OnSettingChanged;
        public System.Action OnSettingsSaved;
        public System.Action OnSettingsLoaded;
        
        public string ManagerName => "Settings";
        
        protected override void OnManagerInitialize()
        {
            InitializeDefaultSettings();
            LoadSettings();
            
            if (_enableAutoSave)
            {
                InvokeRepeating(nameof(AutoSave), _autoSaveInterval, _autoSaveInterval);
            }
            
            LogDebug("Settings Manager initialized");
        }
        
        protected override void OnManagerShutdown()
        {
            if (_hasUnsavedChanges)
            {
                SaveSettings();
            }
            CancelInvoke();
            LogDebug("Settings Manager shutdown");
        }
        
        // Public interface methods
        public T GetSetting<T>(string key, T defaultValue = default(T))
        {
            if (_settings.ContainsKey(key))
            {
                try
                {
                    return (T)Convert.ChangeType(_settings[key], typeof(T));
                }
                catch (Exception)
                {
                    Debug.LogWarning($"Failed to convert setting '{key}' to type {typeof(T)}");
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }
        
        public void SetSetting(string key, object value)
        {
            var oldValue = _settings.ContainsKey(key) ? _settings[key] : null;
            _settings[key] = value;
            _hasUnsavedChanges = true;
            
            if (!object.Equals(oldValue, value))
            {
                OnSettingChanged?.Invoke(key, value);
            }
        }
        
        public Dictionary<string, object> GetAllSettings()
        {
            return new Dictionary<string, object>(_settings);
        }
        
        public void ApplySettings(Dictionary<string, object> settings)
        {
            foreach (var setting in settings)
            {
                SetSetting(setting.Key, setting.Value);
            }
            
            // Apply settings to Unity systems immediately
            ApplyUnitySettings();
        }
        
        public void LoadSettings()
        {
            try
            {
                string filePath = System.IO.Path.Combine(Application.persistentDataPath, _settingsFileName);
                
                if (System.IO.File.Exists(filePath))
                {
                    string json = System.IO.File.ReadAllText(filePath);
                    var loadedSettings = JsonUtility.FromJson<SettingsData>(json);
                    
                    _settings.Clear();
                    foreach (var setting in loadedSettings.Settings)
                    {
                        _settings[setting.Key] = setting.Value;
                    }
                    
                    OnSettingsLoaded?.Invoke();
                    Debug.Log("Settings loaded successfully");
                }
                else
                {
                    // Use default settings if no file exists
                    _settings = new Dictionary<string, object>(_defaultSettings);
                    Debug.Log("No settings file found, using defaults");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load settings: {ex.Message}");
                _settings = new Dictionary<string, object>(_defaultSettings);
            }
            
            _hasUnsavedChanges = false;
        }
        
        public void SaveSettings()
        {
            try
            {
                var settingsData = new SettingsData();
                settingsData.Settings = new List<SettingPair>();
                
                foreach (var setting in _settings)
                {
                    settingsData.Settings.Add(new SettingPair 
                    { 
                        Key = setting.Key, 
                        Value = setting.Value 
                    });
                }
                
                string json = JsonUtility.ToJson(settingsData, true);
                string filePath = System.IO.Path.Combine(Application.persistentDataPath, _settingsFileName);
                
                System.IO.File.WriteAllText(filePath, json);
                
                _hasUnsavedChanges = false;
                _lastSaveTime = Time.time;
                
                OnSettingsSaved?.Invoke();
                Debug.Log("Settings saved successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save settings: {ex.Message}");
            }
        }
        
        public void ResetToDefaults()
        {
            _settings.Clear();
            foreach (var defaultSetting in _defaultSettings)
            {
                _settings[defaultSetting.Key] = defaultSetting.Value;
            }
            
            _hasUnsavedChanges = true;
            Debug.Log("Settings reset to defaults");
        }
        
        private void InitializeDefaultSettings()
        {
            _defaultSettings.Clear();
            
            // Gameplay defaults
            _defaultSettings["difficulty"] = "Normal";
            _defaultSettings["gameSpeed"] = 1f;
            _defaultSettings["tutorials"] = true;
            _defaultSettings["autoSave"] = true;
            
            // Graphics defaults
            _defaultSettings["qualityLevel"] = QualitySettings.GetQualityLevel();
            _defaultSettings["fullscreen"] = Screen.fullScreen;
            _defaultSettings["resolution"] = Screen.currentResolution.ToString();
            
            // Audio defaults
            _defaultSettings["masterVolume"] = 0.8f;
            _defaultSettings["musicVolume"] = 0.7f;
            _defaultSettings["sfxVolume"] = 0.8f;
        }
        
        private void ApplyUnitySettings()
        {
            // Apply graphics settings
            if (_settings.ContainsKey("qualityLevel"))
            {
                QualitySettings.SetQualityLevel(GetSetting<int>("qualityLevel", 2));
            }
            
            if (_settings.ContainsKey("fullscreen"))
            {
                Screen.fullScreen = GetSetting<bool>("fullscreen", true);
            }
            
            // Apply audio settings
            if (_settings.ContainsKey("masterVolume"))
            {
                AudioListener.volume = GetSetting<float>("masterVolume", 0.8f);
            }
        }
        
        private void AutoSave()
        {
            if (_hasUnsavedChanges && Time.time - _lastSaveTime > _autoSaveInterval)
            {
                SaveSettings();
            }
        }
    }
    
    [System.Serializable]
    public class SettingsData
    {
        public List<SettingPair> Settings = new List<SettingPair>();
    }
    
    [System.Serializable]
    public class SettingPair
    {
        public string Key;
        public object Value;
    }
} 