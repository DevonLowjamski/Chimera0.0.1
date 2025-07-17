using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data;
using ProjectChimera.Core.Events;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Comprehensive event database ScriptableObject containing all live event definitions,
    /// templates, and metadata for Project Chimera's dynamic event system.
    /// </summary>
    [CreateAssetMenu(fileName = "New Event Database", menuName = "Project Chimera/Events/Event Database", order = 101)]
    public class EventDatabaseSO : ChimeraDataSO
    {
        [Header("Global Competition Events")]
        [SerializeField] private List<GlobalCompetitionEventSO> _globalCompetitions = new List<GlobalCompetitionEventSO>();
        [SerializeField] private List<CompetitionTemplateSO> _competitionTemplates = new List<CompetitionTemplateSO>();
        
        [Header("Cultural Events")]
        [SerializeField] private List<CulturalEventSO> _culturalEvents = new List<CulturalEventSO>();
        [SerializeField] private List<CannabisHolidaySO> _cannabisHolidays = new List<CannabisHolidaySO>();
        
        [Header("Discovery Events")]
        [SerializeField] private List<DiscoveryEventSO> _discoveryEvents = new List<DiscoveryEventSO>();
        [SerializeField] private List<LimitedReleaseEventSO> _limitedReleases = new List<LimitedReleaseEventSO>();
        
        [Header("Crisis Response Events")]
        [SerializeField] private List<CrisisEventTemplateSO> _crisisTemplates = new List<CrisisEventTemplateSO>();
        [SerializeField] private List<EmergencyResponseSO> _emergencyResponses = new List<EmergencyResponseSO>();
        
        [Header("Seasonal Events")]
        [SerializeField] private List<SeasonalEventSO> _seasonalEvents = new List<SeasonalEventSO>();
        [SerializeField] private List<WeatherEventSO> _weatherEvents = new List<WeatherEventSO>();
        
        [Header("Event Metadata")]
        [SerializeField] private List<EventCategorySO> _eventCategories = new List<EventCategorySO>();
        [SerializeField] private List<ParticipationRequirementSO> _participationRequirements = new List<ParticipationRequirementSO>();
        [SerializeField] private List<RewardTemplateSO> _rewardTemplates = new List<RewardTemplateSO>();
        
        [Header("Validation and Quality Assurance")]
        [SerializeField] private bool _enableEventValidation = true;
        [SerializeField] private bool _enableCulturalSensitivityCheck = true;
        [SerializeField] private bool _enableEducationalContentValidation = true;
        [SerializeField] private bool _enableScientificAccuracyCheck = true;
        
        // Cached lookups for performance
        private Dictionary<string, ILiveEventDefinition> _eventLookup;
        private Dictionary<EventType, List<ILiveEventDefinition>> _eventsByType;
        private Dictionary<string, EventCategorySO> _categoryLookup;
        private Dictionary<string, RewardTemplateSO> _rewardLookup;
        
        // Properties
        public IReadOnlyList<GlobalCompetitionEventSO> GlobalCompetitions => _globalCompetitions;
        public IReadOnlyList<CulturalEventSO> CulturalEvents => _culturalEvents;
        public IReadOnlyList<DiscoveryEventSO> DiscoveryEvents => _discoveryEvents;
        public IReadOnlyList<CrisisEventTemplateSO> CrisisTemplates => _crisisTemplates;
        public IReadOnlyList<SeasonalEventSO> SeasonalEvents => _seasonalEvents;
        public IReadOnlyList<EventCategorySO> EventCategories => _eventCategories;
        public IReadOnlyList<RewardTemplateSO> RewardTemplates => _rewardTemplates;
        
        public bool EnableEventValidation => _enableEventValidation;
        public bool EnableCulturalSensitivityCheck => _enableCulturalSensitivityCheck;
        public bool EnableEducationalContentValidation => _enableEducationalContentValidation;
        public bool EnableScientificAccuracyCheck => _enableScientificAccuracyCheck;
        
        private void OnEnable()
        {
            BuildEventLookups();
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (Application.isPlaying)
            {
                BuildEventLookups();
                ValidateEventDatabase();
            }
        }
        
        private void BuildEventLookups()
        {
            _eventLookup = new Dictionary<string, ILiveEventDefinition>();
            _eventsByType = new Dictionary<EventType, List<ILiveEventDefinition>>();
            _categoryLookup = new Dictionary<string, EventCategorySO>();
            _rewardLookup = new Dictionary<string, RewardTemplateSO>();
            
            // Build event lookups
            foreach (var eventDef in GetAllEventDefinitions())
            {
                _eventLookup[eventDef.EventId] = eventDef;
                
                if (!_eventsByType.ContainsKey((EventType)eventDef.EventType))
                {
                    _eventsByType[(EventType)eventDef.EventType] = new List<ILiveEventDefinition>();
                }
                _eventsByType[(EventType)eventDef.EventType].Add(eventDef);
            }
            
            // Build category lookup
            foreach (var category in _eventCategories)
            {
                if (category != null)
                {
                    _categoryLookup[category.CategoryId] = category;
                }
            }
            
            // Build reward lookup
            foreach (var reward in _rewardTemplates)
            {
                if (reward != null)
                {
                    _rewardLookup[reward.RewardId] = reward;
                }
            }
        }
        
        public ILiveEventDefinition GetEventById(string eventId)
        {
            if (_eventLookup == null)
            {
                BuildEventLookups();
            }
            
            return _eventLookup.GetValueOrDefault(eventId);
        }
        
        public List<ILiveEventDefinition> GetEventsByType(EventType eventType)
        {
            if (_eventsByType == null)
            {
                BuildEventLookups();
            }
            
            return _eventsByType.GetValueOrDefault(eventType, new List<ILiveEventDefinition>());
        }
        
        public List<ILiveEventDefinition> GetEventsByCategory(string categoryId)
        {
            var category = GetCategoryById(categoryId);
            if (category == null) return new List<ILiveEventDefinition>();
            
            return GetAllEventDefinitions()
                .Where(e => category.EventIds.Contains(e.EventId))
                .ToList();
        }
        
        public List<ILiveEventDefinition> GetActiveEvents(DateTime currentTime)
        {
            return GetAllEventDefinitions()
                .Where(e => IsEventActive(e, currentTime))
                .ToList();
        }
        
        public List<ILiveEventDefinition> GetUpcomingEvents(DateTime currentTime, TimeSpan lookAhead)
        {
            var endTime = currentTime.Add(lookAhead);
            
            return GetAllEventDefinitions()
                .Where(e => IsEventUpcoming(e, currentTime, endTime))
                .OrderBy(e => e.StartTime)
                .ToList();
        }
        
        public List<ILiveEventDefinition> GetAvailableEvents(PlayerProfile playerProfile, DateTime currentTime)
        {
            return GetAllEventDefinitions()
                .Where(e => CanPlayerParticipate(e, playerProfile, currentTime))
                .ToList();
        }
        
        public List<GlobalCompetitionEventSO> GetGlobalCompetitionsByType(CompetitionType competitionType)
        {
            return _globalCompetitions
                .Where(comp => comp.CompetitionType == competitionType)
                .ToList();
        }
        
        public List<CulturalEventSO> GetCulturalEventsByDate(DateTime date)
        {
            return _culturalEvents
                .Where(evt => IsDateInEventWindow(evt, date))
                .ToList();
        }
        
        public List<CannabisHolidaySO> GetHolidaysByMonth(int month)
        {
            return _cannabisHolidays
                .Where(holiday => holiday.Date.Month == month)
                .ToList();
        }
        
        public List<DiscoveryEventSO> GetDiscoveryEventsByRarity(RarityTier rarity)
        {
            return _discoveryEvents
                .Where(evt => evt.RarityTier == rarity)
                .ToList();
        }
        
        public List<CrisisEventTemplateSO> GetCrisisTemplatesByType(CrisisType crisisType)
        {
            return _crisisTemplates
                .Where(template => template.CrisisType == crisisType)
                .ToList();
        }
        
        public List<SeasonalEventSO> GetSeasonalEventsBySeason(Season season)
        {
            return _seasonalEvents
                .Where(evt => evt.Season == season)
                .ToList();
        }
        
        public EventCategorySO GetCategoryById(string categoryId)
        {
            if (_categoryLookup == null)
            {
                BuildEventLookups();
            }
            
            return _categoryLookup.GetValueOrDefault(categoryId);
        }
        
        public RewardTemplateSO GetRewardTemplateById(string rewardId)
        {
            if (_rewardLookup == null)
            {
                BuildEventLookups();
            }
            
            return _rewardLookup.GetValueOrDefault(rewardId);
        }
        
        public List<ILiveEventDefinition> GetAllEventDefinitions()
        {
            var allEvents = new List<ILiveEventDefinition>();
            
            allEvents.AddRange(_globalCompetitions.Cast<ILiveEventDefinition>());
            allEvents.AddRange(_culturalEvents.Cast<ILiveEventDefinition>());
            allEvents.AddRange(_discoveryEvents.Cast<ILiveEventDefinition>());
            allEvents.AddRange(_seasonalEvents.Cast<ILiveEventDefinition>());
            
            return allEvents;
        }
        
        private bool IsEventActive(ILiveEventDefinition eventDef, DateTime currentTime)
        {
            return currentTime >= eventDef.StartTime && currentTime <= eventDef.EndTime;
        }
        
        private bool IsEventUpcoming(ILiveEventDefinition eventDef, DateTime currentTime, DateTime endTime)
        {
            return eventDef.StartTime > currentTime && eventDef.StartTime <= endTime;
        }
        
        private bool CanPlayerParticipate(ILiveEventDefinition eventDef, PlayerProfile playerProfile, DateTime currentTime)
        {
            // Check if event is active or upcoming
            if (currentTime > eventDef.EndTime) return false;
            
            // Check participation requirements
            foreach (var requirementId in eventDef.ParticipationRequirements)
            {
                var requirement = GetParticipationRequirement(requirementId);
                if (requirement != null && !requirement.IsMetBy(playerProfile))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private bool IsDateInEventWindow(CulturalEventSO culturalEvent, DateTime date)
        {
            // Check if date falls within the cultural event's celebration window
            var eventDate = culturalEvent.Date;
            var celebrationWindow = culturalEvent.CelebrationDuration;
            
            return date >= eventDate && date <= eventDate.Add(celebrationWindow);
        }
        
        private ParticipationRequirementSO GetParticipationRequirement(string requirementId)
        {
            return _participationRequirements.FirstOrDefault(req => req.RequirementId == requirementId);
        }
        
        private void ValidateEventDatabase()
        {
            if (!_enableEventValidation) return;
            
            var validationErrors = new List<string>();
            
            // Validate global competitions
            foreach (var competition in _globalCompetitions)
            {
                if (competition == null)
                {
                    validationErrors.Add("Null global competition found");
                    continue;
                }
                
                var errors = ValidateEventDefinition(competition);
                validationErrors.AddRange(errors);
            }
            
            // Validate cultural events
            foreach (var culturalEvent in _culturalEvents)
            {
                if (culturalEvent == null)
                {
                    validationErrors.Add("Null cultural event found");
                    continue;
                }
                
                var errors = ValidateEventDefinition(culturalEvent);
                validationErrors.AddRange(errors);
                
                // Additional cultural sensitivity validation
                if (_enableCulturalSensitivityCheck)
                {
                    var culturalErrors = ValidateCulturalSensitivity(culturalEvent);
                    validationErrors.AddRange(culturalErrors);
                }
            }
            
            // Validate discovery events
            foreach (var discoveryEvent in _discoveryEvents)
            {
                if (discoveryEvent == null)
                {
                    validationErrors.Add("Null discovery event found");
                    continue;
                }
                
                var errors = ValidateEventDefinition(discoveryEvent);
                validationErrors.AddRange(errors);
            }
            
            // Log validation results
            if (validationErrors.Count > 0)
            {
                Debug.LogError($"[EventDatabaseSO] Event database validation failed with {validationErrors.Count} errors:\n{string.Join("\n", validationErrors)}");
            }
            else
            {
                Debug.Log($"[EventDatabaseSO] Event database validation passed for {GetAllEventDefinitions().Count} events");
            }
        }
        
        private List<string> ValidateEventDefinition(ILiveEventDefinition eventDef)
        {
            var errors = new List<string>();
            
            // Basic validation
            if (string.IsNullOrEmpty(eventDef.EventId))
                errors.Add($"Event missing ID");
            
            if (string.IsNullOrEmpty(eventDef.EventName))
                errors.Add($"Event {eventDef.EventId} missing name");
            
            if (string.IsNullOrEmpty(eventDef.Description))
                errors.Add($"Event {eventDef.EventId} missing description");
            
            // Time validation
            if (eventDef.EndTime <= eventDef.StartTime)
                errors.Add($"Event {eventDef.EventId} has invalid time range");
            
            // Educational content validation
            if (_enableEducationalContentValidation && eventDef.HasEducationalContent)
            {
                var educationalErrors = ValidateEducationalContent(eventDef);
                errors.AddRange(educationalErrors);
            }
            
            return errors;
        }
        
        private List<string> ValidateCulturalSensitivity(CulturalEventSO culturalEvent)
        {
            var errors = new List<string>();
            
            // Check for appropriate cultural representation
            if (culturalEvent.RequiresCulturalAuthenticity && string.IsNullOrEmpty(culturalEvent.CulturalContext))
            {
                errors.Add($"Cultural event {culturalEvent.EventId} requires cultural context");
            }
            
            // Check for respectful representation
            if (culturalEvent.CulturalTags.Any(tag => IsInappropriateTag(tag)))
            {
                errors.Add($"Cultural event {culturalEvent.EventId} contains inappropriate cultural tags");
            }
            
            return errors;
        }
        
        private List<string> ValidateEducationalContent(ILiveEventDefinition eventDef)
        {
            var errors = new List<string>();
            
            // Validate scientific accuracy if required
            if (_enableScientificAccuracyCheck && eventDef.RequiresScientificAccuracy)
            {
                if (!eventDef.IsScientificallyValidated)
                {
                    errors.Add($"Event {eventDef.EventId} requires scientific validation");
                }
            }
            
            return errors;
        }
        
        private bool IsInappropriateTag(string tag)
        {
            // List of inappropriate or insensitive tags
            var inappropriateTags = new HashSet<string>
            {
                "stereotype", "appropriation", "misrepresentation"
            };
            
            return inappropriateTags.Contains(tag.ToLower());
        }
        
        public EventDatabaseStatistics GetDatabaseStatistics()
        {
            return new EventDatabaseStatistics
            {
                TotalEvents = GetAllEventDefinitions().Count,
                GlobalCompetitions = _globalCompetitions.Count,
                CulturalEvents = _culturalEvents.Count,
                DiscoveryEvents = _discoveryEvents.Count,
                CrisisTemplates = _crisisTemplates.Count,
                SeasonalEvents = _seasonalEvents.Count,
                EventCategories = _eventCategories.Count,
                RewardTemplates = _rewardTemplates.Count,
                LastValidated = DateTime.Now
            };
        }
    }
    
    [Serializable]
    public class EventDatabaseStatistics
    {
        public int TotalEvents;
        public int GlobalCompetitions;
        public int CulturalEvents;
        public int DiscoveryEvents;
        public int CrisisTemplates;
        public int SeasonalEvents;
        public int EventCategories;
        public int RewardTemplates;
        public DateTime LastValidated;
    }
    
    // Enums and supporting structures
    public enum EventType
    {
        GlobalCompetition,
        CulturalCelebration,
        DiscoveryEvent,
        CommunityChallenge,
        SeasonalEvent,
        CrisisResponse,
        LimitedTimeOffer,
        EducationalWorkshop,
        SocialGathering
    }
    
    public enum CompetitionType
    {
        BestStrain,
        HighestYield,
        BestTerpeneProfile,
        MostInnovative,
        SustainableGrowing,
        SpeedGrowing,
        ProblemSolving,
        CommunityContribution
    }
    
    public enum CrisisType
    {
        EnvironmentalDisaster,
        LegalChanges,
        IndustryDisruption,
        CommunityNeed,
        KnowledgeGap,
        TechnicalChallenge,
        HealthConcern,
        SupplyChainIssue
    }
    
    public enum RarityTier
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }
    
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter,
        // Additional seasonal categories for event filtering
        Seasonal,  // Events that span multiple seasons
        Cultural,  // Cultural season events
        Holiday,   // Holiday season events  
        Weather,   // Weather-based season events
        All        // All seasons filter
    }
}