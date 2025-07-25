using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.UI
{
    /// <summary>
    /// ScriptableObject for managing UI state persistence configuration.
    /// Defines what UI states should be saved and how they should be stored.
    /// </summary>
    [CreateAssetMenu(fileName = "New UI State Persistence", menuName = "Project Chimera/UI/State Persistence")]
    public class UIStatePersistenceSO : ChimeraConfigSO
    {
        [Header("Persistence Configuration")]
        [SerializeField] private string _persistenceId = "ui-state";
        [SerializeField] private bool _enableAutosave = true;
        [SerializeField] private float _autosaveInterval = 30f;
        [SerializeField] private bool _saveOnSceneChange = true;
        [SerializeField] private bool _saveOnApplicationPause = true;
        
        [Header("Storage Settings")]
        [SerializeField] private UIStateStorageType _storageType = UIStateStorageType.PlayerPrefs;
        [SerializeField] private string _fileStoragePath = "UI/States";
        [SerializeField] private bool _compressData = true;
        [SerializeField] private bool _encryptData = false;
        [SerializeField] private int _maxBackupVersions = 3;
        
        [Header("Persistence Scopes")]
        [SerializeField] private List<UIPersistenceScope> _persistenceScopes = new List<UIPersistenceScope>();
        
        [Header("Dashboard Persistence")]
        [SerializeField] private bool _saveDashboardLayouts = true;
        [SerializeField] private bool _saveWidgetPositions = true;
        [SerializeField] private bool _saveWidgetSizes = true;
        [SerializeField] private bool _saveWidgetSettings = true;
        [SerializeField] private bool _saveCustomLayouts = true;
        
        [Header("Panel Persistence")]
        [SerializeField] private bool _savePanelStates = true;
        [SerializeField] private bool _saveSelectedTabs = true;
        [SerializeField] private bool _saveExpandedSections = true;
        [SerializeField] private bool _saveScrollPositions = true;
        [SerializeField] private bool _saveFilterSettings = true;
        
        [Header("Theme Persistence")]
        [SerializeField] private bool _saveThemeSelection = true;
        [SerializeField] private bool _saveColorPreferences = true;
        [SerializeField] private bool _saveFontSettings = true;
        [SerializeField] private bool _saveAnimationSettings = true;
        
        [Header("Window Management")]
        [SerializeField] private bool _saveWindowPositions = true;
        [SerializeField] private bool _saveWindowSizes = true;
        [SerializeField] private bool _saveWindowStates = true;
        [SerializeField] private bool _saveModalHistory = false;
        
        [Header("User Preferences")]
        [SerializeField] private bool _saveAccessibilitySettings = true;
        [SerializeField] private bool _saveNotificationSettings = true;
        [SerializeField] private bool _saveKeyboardShortcuts = true;
        [SerializeField] private bool _saveLanguageSettings = true;
        
        [Header("Performance Settings")]
        [SerializeField] private int _maxDataSize = 1024 * 1024; // 1MB
        [SerializeField] private bool _validateOnLoad = true;
        [SerializeField] private bool _cleanupOldData = true;
        [SerializeField] private int _dataRetentionDays = 30;
        
        // Properties
        public string PersistenceId => _persistenceId;
        public bool EnableAutosave => _enableAutosave;
        public float AutosaveInterval => _autosaveInterval;
        public bool SaveOnSceneChange => _saveOnSceneChange;
        public bool SaveOnApplicationPause => _saveOnApplicationPause;
        public UIStateStorageType StorageType => _storageType;
        public string FileStoragePath => _fileStoragePath;
        public bool CompressData => _compressData;
        public bool EncryptData => _encryptData;
        public int MaxBackupVersions => _maxBackupVersions;
        public List<UIPersistenceScope> PersistenceScopes => _persistenceScopes;
        
        // Dashboard Properties
        public bool SaveDashboardLayouts => _saveDashboardLayouts;
        public bool SaveWidgetPositions => _saveWidgetPositions;
        public bool SaveWidgetSizes => _saveWidgetSizes;
        public bool SaveWidgetSettings => _saveWidgetSettings;
        public bool SaveCustomLayouts => _saveCustomLayouts;
        
        // Panel Properties
        public bool SavePanelStates => _savePanelStates;
        public bool SaveSelectedTabs => _saveSelectedTabs;
        public bool SaveExpandedSections => _saveExpandedSections;
        public bool SaveScrollPositions => _saveScrollPositions;
        public bool SaveFilterSettings => _saveFilterSettings;
        
        // Theme Properties
        public bool SaveThemeSelection => _saveThemeSelection;
        public bool SaveColorPreferences => _saveColorPreferences;
        public bool SaveFontSettings => _saveFontSettings;
        public bool SaveAnimationSettings => _saveAnimationSettings;
        
        // Window Properties
        public bool SaveWindowPositions => _saveWindowPositions;
        public bool SaveWindowSizes => _saveWindowSizes;
        public bool SaveWindowStates => _saveWindowStates;
        public bool SaveModalHistory => _saveModalHistory;
        
        // User Preference Properties
        public bool SaveAccessibilitySettings => _saveAccessibilitySettings;
        public bool SaveNotificationSettings => _saveNotificationSettings;
        public bool SaveKeyboardShortcuts => _saveKeyboardShortcuts;
        public bool SaveLanguageSettings => _saveLanguageSettings;
        
        // Performance Properties
        public int MaxDataSize => _maxDataSize;
        public bool ValidateOnLoad => _validateOnLoad;
        public bool CleanupOldData => _cleanupOldData;
        public int DataRetentionDays => _dataRetentionDays;
        
        /// <summary>
        /// Check if a specific UI element should be persisted
        /// </summary>
        public bool ShouldPersist(string elementId, UIStateCategory category)
        {
            // Check if category is enabled
            if (!IsCategoryEnabled(category))
                return false;
            
            // Check specific scopes
            foreach (var scope in _persistenceScopes)
            {
                if (scope.ScopeId == elementId)
                {
                    return scope.EnablePersistence;
                }
                
                if (scope.IncludePattern != null && scope.IncludePattern.IsMatch(elementId))
                {
                    if (scope.ExcludePattern == null || !scope.ExcludePattern.IsMatch(elementId))
                    {
                        return scope.EnablePersistence;
                    }
                }
            }
            
            // Default to enabled if no specific scope found
            return true;
        }
        
        /// <summary>
        /// Check if a category is enabled for persistence
        /// </summary>
        private bool IsCategoryEnabled(UIStateCategory category)
        {
            return category switch
            {
                UIStateCategory.Dashboard => _saveDashboardLayouts,
                UIStateCategory.Widget => _saveWidgetPositions,
                UIStateCategory.Panel => _savePanelStates,
                UIStateCategory.Theme => _saveThemeSelection,
                UIStateCategory.Window => _saveWindowPositions,
                UIStateCategory.UserPreferences => _saveAccessibilitySettings,
                UIStateCategory.Filters => _saveFilterSettings,
                UIStateCategory.Scroll => _saveScrollPositions,
                UIStateCategory.Selection => _saveSelectedTabs,
                UIStateCategory.Expansion => _saveExpandedSections,
                _ => true
            };
        }
        
        /// <summary>
        /// Get persistence scope for element
        /// </summary>
        public UIPersistenceScope GetPersistenceScope(string elementId)
        {
            foreach (var scope in _persistenceScopes)
            {
                if (scope.ScopeId == elementId)
                {
                    return scope;
                }
                
                if (scope.IncludePattern != null && scope.IncludePattern.IsMatch(elementId))
                {
                    if (scope.ExcludePattern == null || !scope.ExcludePattern.IsMatch(elementId))
                    {
                        return scope;
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Add persistence scope
        /// </summary>
        public void AddPersistenceScope(UIPersistenceScope scope)
        {
            if (scope != null && !_persistenceScopes.Contains(scope))
            {
                _persistenceScopes.Add(scope);
            }
        }
        
        /// <summary>
        /// Remove persistence scope
        /// </summary>
        public void RemovePersistenceScope(string scopeId)
        {
            _persistenceScopes.RemoveAll(s => s.ScopeId == scopeId);
        }
        
        /// <summary>
        /// Get storage key for element
        /// </summary>
        public string GetStorageKey(string elementId, UIStateCategory category)
        {
            return $"{_persistenceId}.{category}.{elementId}";
        }
        
        /// <summary>
        /// Get file storage path for element
        /// </summary>
        public string GetFileStoragePath(string elementId, UIStateCategory category)
        {
            return $"{_fileStoragePath}/{_persistenceId}/{category}/{elementId}.json";
        }
        
        /// <summary>
        /// Check if data size is within limits
        /// </summary>
        public bool IsDataSizeValid(int dataSize)
        {
            return dataSize <= _maxDataSize;
        }
        
        /// <summary>
        /// Get configuration summary
        /// </summary>
        public UIStatePersistenceInfo GetPersistenceInfo()
        {
            return new UIStatePersistenceInfo
            {
                PersistenceId = _persistenceId,
                StorageType = _storageType,
                EnableAutosave = _enableAutosave,
                AutosaveInterval = _autosaveInterval,
                MaxDataSize = _maxDataSize,
                ScopeCount = _persistenceScopes.Count,
                DashboardPersistence = _saveDashboardLayouts,
                PanelPersistence = _savePanelStates,
                ThemePersistence = _saveThemeSelection,
                WindowPersistence = _saveWindowPositions,
                UserPreferencePersistence = _saveAccessibilitySettings
            };
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_persistenceId))
            {
                _persistenceId = name.ToLower().Replace(" ", "-");
            }
            
            _autosaveInterval = Mathf.Max(1f, _autosaveInterval);
            _maxDataSize = Mathf.Max(1024, _maxDataSize);
            _maxBackupVersions = Mathf.Max(0, _maxBackupVersions);
            _dataRetentionDays = Mathf.Max(1, _dataRetentionDays);
            
            if (string.IsNullOrEmpty(_fileStoragePath))
            {
                _fileStoragePath = "UI/States";
            }
        }
        
        public override bool ValidateData()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_persistenceId))
            {
                LogError("Persistence ID cannot be empty");
                isValid = false;
            }
            
            if (_autosaveInterval <= 0)
            {
                LogError("Autosave interval must be positive");
                isValid = false;
            }
            
            if (_maxDataSize <= 0)
            {
                LogError("Max data size must be positive");
                isValid = false;
            }
            
            // Validate persistence scopes
            var scopeIds = new HashSet<string>();
            foreach (var scope in _persistenceScopes)
            {
                if (string.IsNullOrEmpty(scope.ScopeId))
                {
                    LogWarning("Empty scope ID found in persistence scopes");
                    continue;
                }
                
                if (scopeIds.Contains(scope.ScopeId))
                {
                    LogError($"Duplicate scope ID: {scope.ScopeId}");
                    isValid = false;
                }
                else
                {
                    scopeIds.Add(scope.ScopeId);
                }
            }
            
            return isValid;
        }
    }
    
    /// <summary>
    /// UI persistence scope definition
    /// </summary>
    [System.Serializable]
    public class UIPersistenceScope
    {
        [SerializeField] public string ScopeId;
        [SerializeField] public string DisplayName;
        [SerializeField] public string Description;
        [SerializeField] public bool EnablePersistence = true;
        [SerializeField] public UIStateCategory Category = UIStateCategory.General;
        [SerializeField] public int Priority = 0;
        [SerializeField] public string IncludePatternString;
        [SerializeField] public string ExcludePatternString;
        
        private System.Text.RegularExpressions.Regex _includePattern;
        private System.Text.RegularExpressions.Regex _excludePattern;
        
        public System.Text.RegularExpressions.Regex IncludePattern
        {
            get
            {
                if (_includePattern == null && !string.IsNullOrEmpty(IncludePatternString))
                {
                    try
                    {
                        _includePattern = new System.Text.RegularExpressions.Regex(IncludePatternString);
                    }
                    catch
                    {
                        _includePattern = null;
                    }
                }
                return _includePattern;
            }
        }
        
        public System.Text.RegularExpressions.Regex ExcludePattern
        {
            get
            {
                if (_excludePattern == null && !string.IsNullOrEmpty(ExcludePatternString))
                {
                    try
                    {
                        _excludePattern = new System.Text.RegularExpressions.Regex(ExcludePatternString);
                    }
                    catch
                    {
                        _excludePattern = null;
                    }
                }
                return _excludePattern;
            }
        }
    }
    
    /// <summary>
    /// UI state storage types
    /// </summary>
    public enum UIStateStorageType
    {
        PlayerPrefs,
        FileSystem,
        Database,
        Cloud
    }
    
    /// <summary>
    /// UI state categories
    /// </summary>
    public enum UIStateCategory
    {
        General,
        Dashboard,
        Widget,
        Panel,
        Theme,
        Window,
        UserPreferences,
        Filters,
        Scroll,
        Selection,
        Expansion
    }
    
    /// <summary>
    /// UI state persistence information
    /// </summary>
    public struct UIStatePersistenceInfo
    {
        public string PersistenceId;
        public UIStateStorageType StorageType;
        public bool EnableAutosave;
        public float AutosaveInterval;
        public int MaxDataSize;
        public int ScopeCount;
        public bool DashboardPersistence;
        public bool PanelPersistence;
        public bool ThemePersistence;
        public bool WindowPersistence;
        public bool UserPreferencePersistence;
    }
}