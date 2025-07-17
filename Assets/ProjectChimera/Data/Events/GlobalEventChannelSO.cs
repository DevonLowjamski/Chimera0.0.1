using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Global event channel ScriptableObject for Project Chimera's worldwide event messaging system.
    /// Provides type-safe event channels for global synchronization, coordination, and conflict resolution
    /// across multiple regions and time zones.
    /// </summary>
    [CreateAssetMenu(fileName = "New Global Event Channel", menuName = "Project Chimera/Events/Global Event Channel", order = 121)]
    public class GlobalEventChannelSO : ChimeraDataSO
    {
        [Header("Channel Configuration")]
        [SerializeField] private string _channelId;
        [SerializeField] private string _channelName;
        [SerializeField] private string _description;
        [SerializeField] private GlobalEventChannelType _channelType = GlobalEventChannelType.Synchronization;
        [SerializeField] private EventScope _scope = EventScope.Global;
        [SerializeField] private EventPriority _defaultPriority = EventPriority.Medium;
        
        [Header("Channel Settings")]
        [SerializeField] private bool _enableBroadcast = true;
        [SerializeField] private bool _enableRegionalFiltering = true;
        [SerializeField] private bool _enablePriorityFiltering = true;
        [SerializeField] private bool _enableEventHistory = true;
        [SerializeField] private int _maxHistorySize = 1000;
        
        [Header("Subscriber Management")]
        [SerializeField] private bool _enableSubscriberValidation = true;
        [SerializeField] private bool _requireSubscriberAuthentication = false;
        [SerializeField] private List<string> _authorizedSubscribers = new List<string>();
        [SerializeField] private List<string> _blockedSubscribers = new List<string>();
        
        [Header("Message Filtering")]
        [SerializeField] private bool _enableContentFiltering = false;
        [SerializeField] private List<string> _allowedMessageTypes = new List<string>();
        [SerializeField] private List<string> _blockedMessageTypes = new List<string>();
        [SerializeField] private bool _enableSpamPrevention = true;
        [SerializeField] private float _messageRateLimit = 10f; // messages per second
        
        // Event system
        public event Action<LiveEventMessage> OnGlobalEventMessage;
        public event Action<string, Dictionary<string, object>> OnGlobalSynchronization;
        public event Action<string, GlobalEventState> OnGlobalStateChange;
        public event Action<string, ConflictData> OnGlobalConflict;
        
        // Runtime state
        private List<LiveEventMessage> _messageHistory = new List<LiveEventMessage>();
        private Dictionary<string, DateTime> _lastMessageTime = new Dictionary<string, DateTime>();
        private Dictionary<string, int> _messageCount = new Dictionary<string, int>();
        private bool _isChannelActive = true;
        
        // Properties
        public string ChannelId => _channelId;
        public string ChannelName => _channelName;
        public string Description => _description;
        public GlobalEventChannelType ChannelType => _channelType;
        public EventScope Scope => _scope;
        public EventPriority DefaultPriority => _defaultPriority;
        public bool EnableBroadcast => _enableBroadcast;
        public bool EnableRegionalFiltering => _enableRegionalFiltering;
        public bool EnablePriorityFiltering => _enablePriorityFiltering;
        public bool EnableEventHistory => _enableEventHistory;
        public int MaxHistorySize => _maxHistorySize;
        public bool IsChannelActive => _isChannelActive;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Generate channel ID if empty
            if (string.IsNullOrEmpty(_channelId))
            {
                _channelId = $"global_channel_{_channelType}_{Guid.NewGuid():N}";
            }
            
            // Validate settings
            _maxHistorySize = Mathf.Max(1, _maxHistorySize);
            _messageRateLimit = Mathf.Max(0.1f, _messageRateLimit);
            
            // Initialize default name if empty
            if (string.IsNullOrEmpty(_channelName))
            {
                _channelName = $"Global {_channelType} Channel";
            }
        }
        
        #region Event Raising Methods
        
        public void RaiseEvent(LiveEventMessage message)
        {
            if (!_isChannelActive || message == null)
                return;
            
            // Validate message
            if (!ValidateMessage(message))
            {
                Debug.LogWarning($"[GlobalEventChannelSO] Message validation failed for channel: {_channelId}");
                return;
            }
            
            // Check rate limiting
            if (!CheckRateLimit(message.SourceId))
            {
                Debug.LogWarning($"[GlobalEventChannelSO] Rate limit exceeded for source: {message.SourceId}");
                return;
            }
            
            // Add to history if enabled
            if (_enableEventHistory)
            {
                AddToHistory(message);
            }
            
            // Set default priority if not set
            if (message.Priority == EventPriority.Medium && _defaultPriority != EventPriority.Medium)
            {
                message.Priority = _defaultPriority;
            }
            
            // Broadcast to subscribers
            try
            {
                OnGlobalEventMessage?.Invoke(message);
                Debug.Log($"[GlobalEventChannelSO] Broadcast message: {message.MessageId} on channel: {_channelId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GlobalEventChannelSO] Error broadcasting message: {ex.Message}");
            }
        }
        
        public void RaiseGlobalSynchronization(string eventId, Dictionary<string, object> syncData)
        {
            if (!_isChannelActive || _channelType != GlobalEventChannelType.Synchronization)
                return;
            
            try
            {
                OnGlobalSynchronization?.Invoke(eventId, syncData);
                
                // Also create a standard message
                var message = new LiveEventMessage(LiveEventMessageType.SystemNotification, "Global Synchronization")
                {
                    SourceId = _channelId,
                    Scope = EventScope.Global,
                    Priority = EventPriority.High
                };
                message.SetData("eventId", eventId);
                message.SetData("syncData", syncData);
                
                RaiseEvent(message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GlobalEventChannelSO] Error raising global synchronization: {ex.Message}");
            }
        }
        
        public void RaiseGlobalStateChange(string eventId, GlobalEventState newState)
        {
            if (!_isChannelActive)
                return;
            
            try
            {
                OnGlobalStateChange?.Invoke(eventId, newState);
                
                // Create state change message
                var message = new LiveEventMessage(LiveEventMessageType.GameStateChange, "Global State Change")
                {
                    SourceId = _channelId,
                    Scope = EventScope.Global,
                    Priority = EventPriority.Medium
                };
                message.SetData("eventId", eventId);
                message.SetData("globalState", newState);
                
                RaiseEvent(message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GlobalEventChannelSO] Error raising global state change: {ex.Message}");
            }
        }
        
        public void RaiseGlobalConflict(string conflictId, ConflictData conflictData)
        {
            if (!_isChannelActive || _channelType != GlobalEventChannelType.ConflictResolution)
                return;
            
            try
            {
                OnGlobalConflict?.Invoke(conflictId, conflictData);
                
                // Create conflict message
                var message = new LiveEventMessage(LiveEventMessageType.Alert, "Global Event Conflict")
                {
                    SourceId = _channelId,
                    Scope = EventScope.Global,
                    Priority = GetConflictPriority(conflictData.Severity)
                };
                message.SetData("conflictId", conflictId);
                message.SetData("conflictData", conflictData);
                message.AddTag("conflict");
                message.AddTag("global");
                
                RaiseEvent(message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GlobalEventChannelSO] Error raising global conflict: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Channel Management
        
        public void ActivateChannel()
        {
            _isChannelActive = true;
            Debug.Log($"[GlobalEventChannelSO] Channel activated: {_channelId}");
        }
        
        public void DeactivateChannel()
        {
            _isChannelActive = false;
            Debug.Log($"[GlobalEventChannelSO] Channel deactivated: {_channelId}");
        }
        
        public void ClearHistory()
        {
            if (_enableEventHistory)
            {
                _messageHistory.Clear();
                Debug.Log($"[GlobalEventChannelSO] History cleared for channel: {_channelId}");
            }
        }
        
        public void ResetRateLimits()
        {
            _lastMessageTime.Clear();
            _messageCount.Clear();
            Debug.Log($"[GlobalEventChannelSO] Rate limits reset for channel: {_channelId}");
        }
        
        public bool AddAuthorizedSubscriber(string subscriberId)
        {
            if (!_authorizedSubscribers.Contains(subscriberId))
            {
                _authorizedSubscribers.Add(subscriberId);
                return true;
            }
            return false;
        }
        
        public bool RemoveAuthorizedSubscriber(string subscriberId)
        {
            return _authorizedSubscribers.Remove(subscriberId);
        }
        
        public bool IsSubscriberAuthorized(string subscriberId)
        {
            if (!_enableSubscriberValidation)
                return true;
            
            if (_blockedSubscribers.Contains(subscriberId))
                return false;
            
            if (_requireSubscriberAuthentication)
                return _authorizedSubscribers.Contains(subscriberId);
            
            return true;
        }
        
        #endregion
        
        #region Message Validation and Filtering
        
        private bool ValidateMessage(LiveEventMessage message)
        {
            // Check if message is valid
            if (string.IsNullOrEmpty(message.MessageId) || string.IsNullOrEmpty(message.Title))
                return false;
            
            // Check source authorization
            if (!IsSubscriberAuthorized(message.SourceId))
                return false;
            
            // Check content filtering
            if (_enableContentFiltering)
            {
                if (_blockedMessageTypes.Contains(message.MessageType.ToString()))
                    return false;
                
                if (_allowedMessageTypes.Count > 0 && !_allowedMessageTypes.Contains(message.MessageType.ToString()))
                    return false;
            }
            
            // Check message expiration
            if (message.IsExpired())
                return false;
            
            return true;
        }
        
        private bool CheckRateLimit(string sourceId)
        {
            if (!_enableSpamPrevention)
                return true;
            
            var currentTime = DateTime.Now;
            
            // Initialize tracking for new sources
            if (!_lastMessageTime.ContainsKey(sourceId))
            {
                _lastMessageTime[sourceId] = currentTime;
                _messageCount[sourceId] = 1;
                return true;
            }
            
            // Check time window (1 second)
            var timeDiff = (currentTime - _lastMessageTime[sourceId]).TotalSeconds;
            if (timeDiff >= 1.0)
            {
                // Reset counter for new time window
                _messageCount[sourceId] = 1;
                _lastMessageTime[sourceId] = currentTime;
                return true;
            }
            
            // Check if within rate limit
            if (_messageCount[sourceId] < _messageRateLimit)
            {
                _messageCount[sourceId]++;
                return true;
            }
            
            return false;
        }
        
        private void AddToHistory(LiveEventMessage message)
        {
            _messageHistory.Add(message);
            
            // Maintain history size limit
            while (_messageHistory.Count > _maxHistorySize)
            {
                _messageHistory.RemoveAt(0);
            }
        }
        
        private EventPriority GetConflictPriority(ConflictSeverity severity)
        {
            return severity switch
            {
                ConflictSeverity.Critical => EventPriority.Critical,
                ConflictSeverity.High => EventPriority.High,
                ConflictSeverity.Medium => EventPriority.Medium,
                ConflictSeverity.Low => EventPriority.Low,
                _ => EventPriority.Medium
            };
        }
        
        #endregion
        
        #region Public API
        
        public List<LiveEventMessage> GetMessageHistory()
        {
            return new List<LiveEventMessage>(_messageHistory);
        }
        
        public List<LiveEventMessage> GetMessageHistory(EventPriority minPriority)
        {
            return _messageHistory.Where(m => m.Priority <= minPriority).ToList();
        }
        
        public List<LiveEventMessage> GetMessageHistory(DateTime since)
        {
            return _messageHistory.Where(m => m.Timestamp >= since).ToList();
        }
        
        public int GetSubscriberCount()
        {
            return OnGlobalEventMessage?.GetInvocationList().Length ?? 0;
        }
        
        public ChannelStatistics GetChannelStatistics()
        {
            return new ChannelStatistics
            {
                ChannelId = _channelId,
                TotalMessages = _messageHistory.Count,
                ActiveSubscribers = GetSubscriberCount(),
                IsActive = _isChannelActive,
                LastActivity = _messageHistory.LastOrDefault()?.Timestamp ?? DateTime.MinValue,
                MessageTypes = _messageHistory.GroupBy(m => m.MessageType)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count())
            };
        }
        
        #endregion
    }
    
    // Supporting enums and data structures
    public enum GlobalEventChannelType
    {
        Synchronization,
        Coordination,
        ConflictResolution,
        StateManagement,
        Analytics,
        Monitoring,
        Debug
    }
    
    public enum ConflictSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    [Serializable]
    public class ConflictData
    {
        public string ConflictId;
        public ConflictSeverity Severity;
        public string ConflictType;
        public string Description;
        public DateTime DetectionTime;
        public List<string> AffectedEvents = new List<string>();
        public List<string> AffectedRegions = new List<string>();
    }
    
    [Serializable]
    public class ChannelStatistics
    {
        public string ChannelId;
        public int TotalMessages;
        public int ActiveSubscribers;
        public bool IsActive;
        public DateTime LastActivity;
        public Dictionary<string, int> MessageTypes = new Dictionary<string, int>();
    }
}