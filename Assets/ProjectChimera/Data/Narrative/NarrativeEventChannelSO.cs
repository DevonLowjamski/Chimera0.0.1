using System;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Narrative Event Channel ScriptableObject for Project Chimera narrative system.
    /// Manages event broadcasting for narrative events.
    /// </summary>
    [CreateAssetMenu(fileName = "New Narrative Event Channel", menuName = "Project Chimera/Narrative/Event Channel", order = 250)]
    public class NarrativeEventChannelSO : ChimeraDataSO
    {
        [Header("Event Channel Configuration")]
        [SerializeField] private string _channelId;
        [SerializeField] private string _channelName;
        [SerializeField] private string _description;
        
        [Header("Event Settings")]
        [SerializeField] private bool _enableEventLogging = true;
        [SerializeField] private bool _enableEventPersistence = false;
        [SerializeField] private int _maxEventHistory = 100;
        
        // Event Action
        public event Action<NarrativeEventMessage> OnEventRaised;
        
        // Properties
        public string ChannelId => _channelId;
        public string ChannelName => _channelName;
        public string Description => _description;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_channelId))
            {
                _channelId = name.Replace(" ", "").Replace("NarrativeEventChannel", "").ToLower();
            }
            
            if (string.IsNullOrEmpty(_channelName))
            {
                _channelName = name.Replace("NarrativeEventChannel", "").Trim();
            }
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_channelId))
            {
                LogError("Channel ID cannot be empty");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(_channelName))
            {
                LogError("Channel name cannot be empty");
                isValid = false;
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Raise an event on this channel
        /// </summary>
        public void RaiseEvent(NarrativeEventMessage eventMessage)
        {
            if (eventMessage == null)
            {
                LogWarning("Attempted to raise null event message");
                return;
            }
            
            if (_enableEventLogging)
            {
                LogInfo($"[{_channelName}] Raising event: {eventMessage.EventType}");
            }
            
            try
            {
                OnEventRaised?.Invoke(eventMessage);
            }
            catch (Exception ex)
            {
                LogError($"Error raising narrative event: {ex.Message}");
            }
        }
    }
}