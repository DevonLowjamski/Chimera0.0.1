using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Events.Core;
using ProjectChimera.Data.Events;
using PlantCareEventData = ProjectChimera.Core.Events.PlantCareEventData;
using EventsPlayerChoiceEventData = ProjectChimera.Data.Events.PlayerChoiceEventData;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Service responsible for orchestrating gaming events
    /// Handles event coordination and routing for all cultivation gaming event channels
    /// </summary>
    public class GamingEventOrchestrator
    {
        // Event channels (15+ channels as referenced in original)
        private readonly GameEventChannelSO _onPlantCarePerformed;
        private readonly GameEventChannelSO _onAutomationUnlocked;
        private readonly GameEventChannelSO _onSkillNodeUnlocked;
        private readonly GameEventChannelSO _onTimeScaleChanged;
        private readonly GameEventChannelSO _onPlayerChoiceMade;
        private readonly GameEventChannelSO _onFacilityDesignCompleted;
        private readonly GameEventChannelSO _onCultivationPathSelected;
        private readonly GameEventChannelSO _onManualTaskBurdenIncreased;
        private readonly GameEventChannelSO _onAutomationBenefitRealized;
        private readonly GameEventChannelSO _onSkillTreeVisualizationUpdated;
        
        // Additional event channels for comprehensive coverage
        private readonly GameEventChannelSO _onPlantGrowthStageChanged;
        private readonly GameEventChannelSO _onEnvironmentalConditionsChanged;
        private readonly GameEventChannelSO _onHarvestCompleted;
        private readonly GameEventChannelSO _onResearchCompleted;
        private readonly GameEventChannelSO _onAchievementUnlocked;
        
        // Event routing and management
        private readonly Dictionary<string, List<Action<object>>> _eventSubscriptions = new Dictionary<string, List<Action<object>>>();
        private readonly Dictionary<string, GameEventChannelSO> _eventChannels = new Dictionary<string, GameEventChannelSO>();
        private readonly Queue<EventQueueItem> _eventQueue = new Queue<EventQueueItem>();
        
        // State
        private bool _isInitialized = false;
        private bool _isProcessingEvents = false;
        private int _eventsProcessedThisFrame = 0;
        private const int MaxEventsPerFrame = 10;
        
        // Statistics
        private readonly Dictionary<string, int> _eventCounts = new Dictionary<string, int>();
        private float _totalEventsProcessed = 0;
        
        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Gaming Event Orchestrator";
        
        public GamingEventOrchestrator(
            GameEventChannelSO onPlantCarePerformed,
            GameEventChannelSO onAutomationUnlocked,
            GameEventChannelSO onSkillNodeUnlocked,
            GameEventChannelSO onTimeScaleChanged,
            GameEventChannelSO onPlayerChoiceMade,
            GameEventChannelSO onFacilityDesignCompleted,
            GameEventChannelSO onCultivationPathSelected,
            GameEventChannelSO onManualTaskBurdenIncreased,
            GameEventChannelSO onAutomationBenefitRealized,
            GameEventChannelSO onSkillTreeVisualizationUpdated,
            GameEventChannelSO onPlantGrowthStageChanged = null,
            GameEventChannelSO onEnvironmentalConditionsChanged = null,
            GameEventChannelSO onHarvestCompleted = null,
            GameEventChannelSO onResearchCompleted = null,
            GameEventChannelSO onAchievementUnlocked = null)
        {
            _onPlantCarePerformed = onPlantCarePerformed;
            _onAutomationUnlocked = onAutomationUnlocked;
            _onSkillNodeUnlocked = onSkillNodeUnlocked;
            _onTimeScaleChanged = onTimeScaleChanged;
            _onPlayerChoiceMade = onPlayerChoiceMade;
            _onFacilityDesignCompleted = onFacilityDesignCompleted;
            _onCultivationPathSelected = onCultivationPathSelected;
            _onManualTaskBurdenIncreased = onManualTaskBurdenIncreased;
            _onAutomationBenefitRealized = onAutomationBenefitRealized;
            _onSkillTreeVisualizationUpdated = onSkillTreeVisualizationUpdated;
            
            // Optional channels
            _onPlantGrowthStageChanged = onPlantGrowthStageChanged;
            _onEnvironmentalConditionsChanged = onEnvironmentalConditionsChanged;
            _onHarvestCompleted = onHarvestCompleted;
            _onResearchCompleted = onResearchCompleted;
            _onAchievementUnlocked = onAchievementUnlocked;
        }
        
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[GamingEventOrchestrator] Already initialized");
                return;
            }
            
            try
            {
                RegisterEventChannels();
                RegisterEventHandlers();
                InitializeEventCounts();
                
                _isInitialized = true;
                Debug.Log("GamingEventOrchestrator initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize GamingEventOrchestrator: {ex.Message}");
                throw;
            }
        }
        
        public void Shutdown()
        {
            if (!_isInitialized) return;
            
            UnregisterEventHandlers();
            ProcessRemainingEvents();
            LogEventStatistics();
            
            _eventChannels.Clear();
            _eventSubscriptions.Clear();
            _eventQueue.Clear();
            
            _isInitialized = false;
            Debug.Log("GamingEventOrchestrator shutdown completed");
        }
        
        public void Update(float deltaTime)
        {
            if (!_isInitialized) return;
            
            ProcessEventQueue();
            _eventsProcessedThisFrame = 0;
        }
        
        private void RegisterEventChannels()
        {
            // Register all event channels with string keys for easy lookup
            RegisterChannel("PlantCarePerformed", _onPlantCarePerformed);
            RegisterChannel("AutomationUnlocked", _onAutomationUnlocked);
            RegisterChannel("SkillNodeUnlocked", _onSkillNodeUnlocked);
            RegisterChannel("TimeScaleChanged", _onTimeScaleChanged);
            RegisterChannel("PlayerChoiceMade", _onPlayerChoiceMade);
            RegisterChannel("FacilityDesignCompleted", _onFacilityDesignCompleted);
            RegisterChannel("CultivationPathSelected", _onCultivationPathSelected);
            RegisterChannel("ManualTaskBurdenIncreased", _onManualTaskBurdenIncreased);
            RegisterChannel("AutomationBenefitRealized", _onAutomationBenefitRealized);
            RegisterChannel("SkillTreeVisualizationUpdated", _onSkillTreeVisualizationUpdated);
            
            // Register optional channels
            RegisterChannel("PlantGrowthStageChanged", _onPlantGrowthStageChanged);
            RegisterChannel("EnvironmentalConditionsChanged", _onEnvironmentalConditionsChanged);
            RegisterChannel("HarvestCompleted", _onHarvestCompleted);
            RegisterChannel("ResearchCompleted", _onResearchCompleted);
            RegisterChannel("AchievementUnlocked", _onAchievementUnlocked);
        }
        
        private void RegisterChannel(string eventName, GameEventChannelSO channel)
        {
            if (channel != null)
            {
                _eventChannels[eventName] = channel;
                Debug.Log($"Registered event channel: {eventName}");
            }
        }
        
        private void RegisterEventHandlers()
        {
            foreach (var channel in _eventChannels)
            {
                if (channel.Value != null)
                {
                    channel.Value.OnEventRaisedWithData.AddListener(data => OnEventReceived(channel.Key, data));
                }
            }
        }
        
        private void UnregisterEventHandlers()
        {
            foreach (var channel in _eventChannels)
            {
                if (channel.Value != null)
                {
                    channel.Value.OnEventRaisedWithData.RemoveListener(data => OnEventReceived(channel.Key, data));
                }
            }
        }
        
        private void InitializeEventCounts()
        {
            foreach (var eventName in _eventChannels.Keys)
            {
                _eventCounts[eventName] = 0;
            }
        }
        
        private void OnEventReceived(string eventName, object eventData)
        {
            if (!_isInitialized) return;
            
            // Queue event for processing
            _eventQueue.Enqueue(new EventQueueItem
            {
                EventName = eventName,
                EventData = eventData,
                Timestamp = Time.time
            });
            
            // Update statistics
            if (_eventCounts.ContainsKey(eventName))
            {
                _eventCounts[eventName]++;
            }
            
            _totalEventsProcessed++;
        }
        
        private void ProcessEventQueue()
        {
            if (_isProcessingEvents) return;
            
            _isProcessingEvents = true;
            
            while (_eventQueue.Count > 0 && _eventsProcessedThisFrame < MaxEventsPerFrame)
            {
                var eventItem = _eventQueue.Dequeue();
                ProcessEvent(eventItem);
                _eventsProcessedThisFrame++;
            }
            
            _isProcessingEvents = false;
        }
        
        private void ProcessEvent(EventQueueItem eventItem)
        {
            try
            {
                // Route event to subscribed handlers
                if (_eventSubscriptions.ContainsKey(eventItem.EventName))
                {
                    var handlers = _eventSubscriptions[eventItem.EventName];
                    foreach (var handler in handlers)
                    {
                        handler?.Invoke(eventItem.EventData);
                    }
                }
                
                // Process specific event types
                ProcessSpecificEvent(eventItem);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing event {eventItem.EventName}: {ex.Message}");
            }
        }
        
        private void ProcessSpecificEvent(EventQueueItem eventItem)
        {
            switch (eventItem.EventName)
            {
                case "PlantCarePerformed":
                    if (eventItem.EventData is PlantCareEventData careData)
                        ProcessPlantCareEvent(careData);
                    break;
                    
                case "PlayerChoiceMade":
                    if (eventItem.EventData is EventsPlayerChoiceEventData choiceData)
                        ProcessPlayerChoiceEvent(choiceData);
                    break;
                    
                case "TimeScaleChanged":
                    if (eventItem.EventData is TimeScaleEventData timeData)
                        ProcessTimeScaleEvent(timeData);
                    break;
                    
                // Add more specific event processing as needed
            }
        }
        
        private void ProcessPlantCareEvent(PlantCareEventData careData)
        {
            // Coordinate plant care event effects
            Debug.Log($"Processing plant care event: {careData.CareType} with quality {careData.CareQuality}");
        }
        
        private void ProcessPlayerChoiceEvent(EventsPlayerChoiceEventData choiceData)
        {
            // Coordinate player choice event effects
            Debug.Log($"Processing player choice event: {choiceData.ChoiceId}");
        }
        
        private void ProcessTimeScaleEvent(TimeScaleEventData timeData)
        {
            // Coordinate time scale change effects
            Debug.Log($"Processing time scale change: {timeData.PreviousTimeScale} -> {timeData.NewTimeScale}");
        }
        
        private void ProcessRemainingEvents()
        {
            while (_eventQueue.Count > 0)
            {
                var eventItem = _eventQueue.Dequeue();
                ProcessEvent(eventItem);
            }
        }
        
        private void LogEventStatistics()
        {
            Debug.Log($"Event Statistics - Total Events: {_totalEventsProcessed}");
            
            foreach (var eventCount in _eventCounts)
            {
                if (eventCount.Value > 0)
                {
                    Debug.Log($"  {eventCount.Key}: {eventCount.Value} events");
                }
            }
        }
        
        // Public API
        public void SubscribeToEvent(string eventName, Action<object> handler)
        {
            if (!_isInitialized || string.IsNullOrEmpty(eventName) || handler == null) return;
            
            if (!_eventSubscriptions.ContainsKey(eventName))
            {
                _eventSubscriptions[eventName] = new List<Action<object>>();
            }
            
            _eventSubscriptions[eventName].Add(handler);
        }
        
        public void UnsubscribeFromEvent(string eventName, Action<object> handler)
        {
            if (!_isInitialized || string.IsNullOrEmpty(eventName) || handler == null) return;
            
            if (_eventSubscriptions.ContainsKey(eventName))
            {
                _eventSubscriptions[eventName].Remove(handler);
            }
        }
        
        public bool RaiseEvent(string eventName, object eventData = null)
        {
            if (!_isInitialized || string.IsNullOrEmpty(eventName)) return false;
            
            if (_eventChannels.ContainsKey(eventName) && _eventChannels[eventName] != null)
            {
                _eventChannels[eventName].RaiseEvent(eventData);
                return true;
            }
            
            return false;
        }
        
        public Dictionary<string, int> GetEventStatistics()
        {
            return new Dictionary<string, int>(_eventCounts);
        }
        
        public int GetEventCount(string eventName)
        {
            return _eventCounts.ContainsKey(eventName) ? _eventCounts[eventName] : 0;
        }
        
        public float GetTotalEventsProcessed()
        {
            return _totalEventsProcessed;
        }
        
        public int GetQueuedEventCount()
        {
            return _eventQueue.Count;
        }
        
        public List<string> GetRegisteredEventNames()
        {
            return new List<string>(_eventChannels.Keys);
        }
        
        public bool IsEventRegistered(string eventName)
        {
            return _eventChannels.ContainsKey(eventName) && _eventChannels[eventName] != null;
        }
        
        public void ClearEventQueue()
        {
            _eventQueue.Clear();
            Debug.Log("Event queue cleared");
        }
        
        public void ResetEventStatistics()
        {
            foreach (var key in _eventCounts.Keys.ToArray())
            {
                _eventCounts[key] = 0;
            }
            _totalEventsProcessed = 0;
            
            Debug.Log("Event statistics reset");
        }
    }
    
    // Supporting data structures
    [System.Serializable]
    public class EventQueueItem
    {
        public string EventName;
        public object EventData;
        public float Timestamp;
    }
}