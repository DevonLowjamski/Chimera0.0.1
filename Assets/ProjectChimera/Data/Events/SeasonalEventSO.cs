using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// ScriptableObject representing seasonal events tied to growing seasons and weather patterns.
    /// </summary>
    [CreateAssetMenu(fileName = "New Seasonal Event", menuName = "Project Chimera/Events/Seasonal Event", order = 105)]
    public class SeasonalEventSO : ChimeraDataSO, ILiveEventDefinition
    {
        [Header("Event Identity")]
        [SerializeField] private string _eventId;
        [SerializeField] private string _eventName;
        [SerializeField] private string _description;
        [SerializeField] private ProjectChimera.Data.Events.EventType _eventType = ProjectChimera.Data.Events.EventType.SeasonalEvent;
        [SerializeField] private EventScope _scope = EventScope.Global;
        
        [Header("Seasonal Details")]
        [SerializeField] private Season _season;
        [SerializeField] private bool _isRecurringAnnually = true;
        [SerializeField] private int _startMonth = 1;
        [SerializeField] private int _startDay = 1;
        [SerializeField] private int _durationDays = 30;
        
        [Header("Timing")]
        [SerializeField] private DateTime _startTime;
        [SerializeField] private DateTime _endTime;
        
        [Header("Requirements")]
        [SerializeField] private List<string> _participationRequirements = new List<string>();
        
        [Header("Educational Content")]
        [SerializeField] private bool _hasEducationalContent = true;
        [SerializeField] private bool _requiresScientificAccuracy = true;
        [SerializeField] private bool _isScientificallyValidated = true;
        
        // ILiveEventDefinition Implementation
        public string EventId => _eventId;
        public string EventName => _eventName;
        public string Description => _description;
        public object EventType => _eventType;
        public EventScope Scope => _scope;
        public DateTime StartTime => _startTime;
        public DateTime EndTime => _endTime;
        public TimeSpan Duration => _endTime - _startTime;
        public List<string> ParticipationRequirements => _participationRequirements;
        public bool HasEducationalContent => _hasEducationalContent;
        public bool RequiresScientificAccuracy => _requiresScientificAccuracy;
        public bool IsScientificallyValidated => _isScientificallyValidated;
        
        // Seasonal Event Properties
        public Season Season => _season;
        public bool IsRecurringAnnually => _isRecurringAnnually;
        public int StartMonth => _startMonth;
        public int StartDay => _startDay;
        public int DurationDays => _durationDays;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_eventId))
            {
                _eventId = $"seasonal_{name.ToLower().Replace(" ", "_")}_{DateTime.Now.Ticks}";
            }
            
            // Update start and end times based on seasonal settings
            if (_isRecurringAnnually)
            {
                var currentYear = DateTime.Now.Year;
                _startTime = new DateTime(currentYear, _startMonth, _startDay);
                _endTime = _startTime.AddDays(_durationDays);
            }
        }
    }
} 