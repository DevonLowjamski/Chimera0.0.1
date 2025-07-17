using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// ScriptableObject representing cultural events and cannabis holidays in Project Chimera.
    /// Includes educational content about cannabis culture, traditions, and community celebrations.
    /// </summary>
    [CreateAssetMenu(fileName = "New Cultural Event", menuName = "Project Chimera/Events/Cultural Event", order = 102)]
    public class CulturalEventSO : ChimeraDataSO, ILiveEventDefinition
    {
        [Header("Event Identity")]
        [SerializeField] private string _eventId;
        [SerializeField] private string _eventName;
        [SerializeField] private string _description;
        [SerializeField] private ProjectChimera.Data.Events.EventType _eventType = ProjectChimera.Data.Events.EventType.CulturalCelebration;
        [SerializeField] private EventScope _scope = EventScope.Global;
        
        [Header("Cultural Context")]
        [SerializeField] private string _culturalOrigin;
        [SerializeField] private string _historicalSignificance;
        [SerializeField] private List<string> _culturalTraditions = new List<string>();
        [SerializeField] private List<string> _celebrationActivities = new List<string>();
        
        [Header("Timing")]
        [SerializeField] private DateTime _startTime;
        [SerializeField] private DateTime _endTime;
        [SerializeField] private bool _isRecurringAnnually = true;
        [SerializeField] private int _durationDays = 1;
        
        [Header("Participation")]
        [SerializeField] private List<string> _participationRequirements = new List<string>();
        [SerializeField] private bool _isGlobalEvent = true;
        [SerializeField] private List<string> _eligibleRegions = new List<string>();
        
        [Header("Educational Content")]
        [SerializeField] private bool _hasEducationalContent = true;
        [SerializeField] private string _educationalObjective;
        [SerializeField] private List<string> _learningPoints = new List<string>();
        [SerializeField] private bool _requiresScientificAccuracy = false;
        [SerializeField] private bool _isScientificallyValidated = true;
        
        [Header("Cultural Sensitivity")]
        [SerializeField] private bool _culturalSensitivityChecked = true;
        [SerializeField] private List<string> _culturalConsiderations = new List<string>();
        [SerializeField] private bool _respectsCulturalTraditions = true;
        [SerializeField] private bool _requiresCulturalAuthenticity = true;
        [SerializeField] private string _culturalContext;
        [SerializeField] private float _importance = 1.0f;
        
        [Header("Event Rewards")]
        [SerializeField] private List<string> _rewardTemplateIds = new List<string>();
        [SerializeField] private bool _hasSpecialRewards = true;
        
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
        
        // Cultural Event Specific Properties
        public string CulturalOrigin => _culturalOrigin;
        public string HistoricalSignificance => _historicalSignificance;
        public IReadOnlyList<string> CulturalTraditions => _culturalTraditions;
        public IReadOnlyList<string> CulturalTags => _culturalTraditions;
        public IReadOnlyList<string> CelebrationActivities => _celebrationActivities;
        public bool IsRecurringAnnually => _isRecurringAnnually;
        public int DurationDays => _durationDays;
        public bool IsGlobalEvent => _isGlobalEvent;
        public IReadOnlyList<string> EligibleRegions => _eligibleRegions;
        public string EducationalObjective => _educationalObjective;
        public IReadOnlyList<string> LearningPoints => _learningPoints;
        public bool CulturalSensitivityChecked => _culturalSensitivityChecked;
        public IReadOnlyList<string> CulturalConsiderations => _culturalConsiderations;
        public bool RespectsCulturalTraditions => _respectsCulturalTraditions;
        public IReadOnlyList<string> RewardTemplateIds => _rewardTemplateIds;
        public bool HasSpecialRewards => _hasSpecialRewards;
        
        // Additional compatibility properties
        public bool RequiresCulturalAuthenticity => _requiresCulturalAuthenticity;
        public string CulturalContext => _culturalContext;
        public float Importance => _importance;
        public TimeSpan CelebrationDuration => TimeSpan.FromDays(_durationDays);
        public bool CulturalSensitivityChecker => _culturalSensitivityChecked;
        
        public DateTime Date => _startTime;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(_eventId))
            {
                _eventId = $"cultural_{name.ToLower().Replace(" ", "_")}_{DateTime.Now.Ticks}";
            }
            
            // Validate cultural sensitivity
            if (_hasEducationalContent && string.IsNullOrEmpty(_educationalObjective))
            {
                Debug.LogWarning($"Cultural Event {_eventName} has educational content but no educational objective defined.");
            }
            
            // Ensure end time is after start time
            if (_endTime <= _startTime)
            {
                _endTime = _startTime.AddDays(_durationDays);
            }
        }
        
        public bool IsActiveOn(DateTime date)
        {
            if (_isRecurringAnnually)
            {
                // Check if the date falls within the annual recurrence window
                var eventStart = new DateTime(date.Year, _startTime.Month, _startTime.Day);
                var eventEnd = eventStart.AddDays(_durationDays);
                return date >= eventStart && date <= eventEnd;
            }
            else
            {
                return date >= _startTime && date <= _endTime;
            }
        }
        
        public List<string> GetCulturalValidationErrors()
        {
            var errors = new List<string>();
            
            if (!_culturalSensitivityChecked)
            {
                errors.Add("Cultural sensitivity has not been checked.");
            }
            
            if (!_respectsCulturalTraditions)
            {
                errors.Add("Event may not respect cultural traditions.");
            }
            
            if (string.IsNullOrEmpty(_culturalOrigin))
            {
                errors.Add("Cultural origin is not specified.");
            }
            
            if (_culturalTraditions.Count == 0)
            {
                errors.Add("No cultural traditions specified.");
            }
            
            return errors;
        }
    }
} 