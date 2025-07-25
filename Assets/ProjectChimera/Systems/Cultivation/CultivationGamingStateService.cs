using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Construction;
using DataCultivationApproach = ProjectChimera.Data.Cultivation.CultivationApproach;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Service responsible for cultivation gaming state management
    /// Handles session tracking, state persistence, and state transitions
    /// </summary>
    public class CultivationGamingStateService     {
        private readonly EnhancedCultivationGamingConfigSO _config;
        
        // State management
        private bool _isInitialized = false;
        private CultivationGamingState _currentGamingState;
        private readonly Dictionary<string, object> _persistentState = new Dictionary<string, object>();
        private readonly List<CultivationGamingStateSnapshot> _stateHistory = new List<CultivationGamingStateSnapshot>();
        
        // Session tracking
        private float _sessionStartTime;
        private string _currentSessionId;
        
        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Cultivation Gaming State Service";
        
        public CultivationGamingStateService(EnhancedCultivationGamingConfigSO config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }
        
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[CultivationGamingStateService] Already initialized");
                return;
            }
            
            try
            {
                InitializeGamingState();
                StartNewSession();
                LoadPersistedState();
                
                _isInitialized = true;
                Debug.Log("CultivationGamingStateService initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize CultivationGamingStateService: {ex.Message}");
                throw;
            }
        }
        
        public void Shutdown()
        {
            if (!_isInitialized) return;
            
            SaveCurrentState();
            EndCurrentSession();
            
            _isInitialized = false;
            Debug.Log("CultivationGamingStateService shutdown completed");
        }
        
        public void Update(float deltaTime)
        {
            if (!_isInitialized) return;
            
            // Update session time and periodic state saves
            UpdateSessionTime();
            CheckPeriodicStateSave();
        }
        
        private void InitializeGamingState()
        {
            _currentGamingState = new CultivationGamingState
            {
                SessionStartTime = Time.time,
                CurrentTimeScale = GameTimeScale.Baseline,
                AutomationLevel = AutomationLevel.FullyManual,
                PlayerSkillLevel = SkillLevel.Beginner,
                CultivationApproach = DataCultivationApproach.OrganicTraditional,
                FacilityDesignApproach = FacilityDesignApproach.MinimalistEfficient
            };
        }
        
        private void StartNewSession()
        {
            _currentSessionId = Guid.NewGuid().ToString();
            _sessionStartTime = Time.time;
            
            Debug.Log($"Started new gaming session: {_currentSessionId}");
        }
        
        private void LoadPersistedState()
        {
            // Load state from persistent storage
            // This would integrate with save system
            Debug.Log("Loading persisted gaming state");
        }
        
        private void SaveCurrentState()
        {
            if (_currentGamingState == null) return;
            
            var snapshot = CreateStateSnapshot();
            _stateHistory.Add(snapshot);
            
            // Persist to storage
            PersistStateToStorage(snapshot);
            
            Debug.Log("Current gaming state saved");
        }
        
        private void EndCurrentSession()
        {
            var sessionDuration = Time.time - _sessionStartTime;
            Debug.Log($"Ended gaming session {_currentSessionId}, Duration: {sessionDuration:F1}s");
        }
        
        private void UpdateSessionTime()
        {
            // Update any time-based state changes
        }
        
        private void CheckPeriodicStateSave()
        {
            // Save state periodically (every 5 minutes)
            var timeSinceLastSave = Time.time - _sessionStartTime;
            if (timeSinceLastSave > 300f) // 5 minutes
            {
                SaveCurrentState();
                _sessionStartTime = Time.time; // Reset timer
            }
        }
        
        private CultivationGamingStateSnapshot CreateStateSnapshot()
        {
            return new CultivationGamingStateSnapshot
            {
                Timestamp = Time.time,
                SessionId = _currentSessionId,
                GameState = CloneGamingState(_currentGamingState),
                PersistentData = new Dictionary<string, object>(_persistentState)
            };
        }
        
        private CultivationGamingState CloneGamingState(CultivationGamingState original)
        {
            return new CultivationGamingState
            {
                SessionStartTime = original.SessionStartTime,
                CurrentTimeScale = original.CurrentTimeScale,
                AutomationLevel = original.AutomationLevel,
                PlayerSkillLevel = original.PlayerSkillLevel,
                CultivationApproach = original.CultivationApproach,
                FacilityDesignApproach = original.FacilityDesignApproach
            };
        }
        
        private void PersistStateToStorage(CultivationGamingStateSnapshot snapshot)
        {
            // Implementation for persistent storage
            // This would integrate with the game's save system
        }
        
        // Public API
        public CultivationGamingState GetCurrentGamingState()
        {
            return _currentGamingState;
        }
        
        public bool UpdateGamingState(Action<CultivationGamingState> stateUpdater)
        {
            if (!_isInitialized || stateUpdater == null || _currentGamingState == null)
                return false;
            
            stateUpdater.Invoke(_currentGamingState);
            return true;
        }
        
        public bool SetTimeScale(GameTimeScale timeScale)
        {
            if (!_isInitialized || _currentGamingState == null) return false;
            
            _currentGamingState.CurrentTimeScale = timeScale;
            return true;
        }
        
        public bool SetAutomationLevel(AutomationLevel automationLevel)
        {
            if (!_isInitialized || _currentGamingState == null) return false;
            
            _currentGamingState.AutomationLevel = automationLevel;
            return true;
        }
        
        public bool SetPlayerSkillLevel(SkillLevel skillLevel)
        {
            if (!_isInitialized || _currentGamingState == null) return false;
            
            _currentGamingState.PlayerSkillLevel = skillLevel;
            return true;
        }
        
        public bool SetCultivationApproach(DataCultivationApproach approach)
        {
            if (!_isInitialized || _currentGamingState == null) return false;
            
            _currentGamingState.CultivationApproach = approach;
            return true;
        }
        
        public bool SetFacilityDesignApproach(FacilityDesignApproach approach)
        {
            if (!_isInitialized || _currentGamingState == null) return false;
            
            _currentGamingState.FacilityDesignApproach = approach;
            return true;
        }
        
        public void SetPersistentValue(string key, object value)
        {
            if (!_isInitialized || string.IsNullOrEmpty(key)) return;
            
            _persistentState[key] = value;
        }
        
        public T GetPersistentValue<T>(string key, T defaultValue = default(T))
        {
            if (!_isInitialized || string.IsNullOrEmpty(key) || !_persistentState.ContainsKey(key))
                return defaultValue;
            
            try
            {
                return (T)_persistentState[key];
            }
            catch
            {
                return defaultValue;
            }
        }
        
        public CultivationGamingStateSnapshot GetStateSnapshot(int index = -1)
        {
            if (!_isInitialized || _stateHistory.Count == 0) return null;
            
            if (index < 0) // Get latest
                return _stateHistory[_stateHistory.Count - 1];
            
            if (index >= _stateHistory.Count) return null;
            
            return _stateHistory[index];
        }
        
        public List<CultivationGamingStateSnapshot> GetStateHistory()
        {
            return new List<CultivationGamingStateSnapshot>(_stateHistory);
        }
        
        public void ClearStateHistory()
        {
            _stateHistory.Clear();
            Debug.Log("Gaming state history cleared");
        }
        
        public string GetCurrentSessionId()
        {
            return _currentSessionId;
        }
        
        public float GetSessionDuration()
        {
            return Time.time - _sessionStartTime;
        }
        
        public bool RestoreFromSnapshot(CultivationGamingStateSnapshot snapshot)
        {
            if (!_isInitialized || snapshot?.GameState == null) return false;
            
            _currentGamingState = CloneGamingState(snapshot.GameState);
            _persistentState.Clear();
            
            if (snapshot.PersistentData != null)
            {
                foreach (var kvp in snapshot.PersistentData)
                {
                    _persistentState[kvp.Key] = kvp.Value;
                }
            }
            
            Debug.Log($"Restored gaming state from snapshot at {snapshot.Timestamp}");
            return true;
        }
    }
    
    // Supporting data structures
    [System.Serializable]
    public class CultivationGamingStateSnapshot
    {
        public float Timestamp;
        public string SessionId;
        public CultivationGamingState GameState;
        public Dictionary<string, object> PersistentData;
    }
}