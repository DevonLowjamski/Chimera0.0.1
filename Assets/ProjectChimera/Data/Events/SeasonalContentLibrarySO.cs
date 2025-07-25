using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Comprehensive seasonal content library ScriptableObject managing all seasonal variations,
    /// cultural events, and time-based content for Project Chimera's dynamic event system.
    /// </summary>
    [CreateAssetMenu(fileName = "New Seasonal Content Library", menuName = "Project Chimera/Events/Seasonal Content Library", order = 102)]
    public class SeasonalContentLibrarySO : ChimeraDataSO
    {
        [Header("Seasonal Event Collections")]
        [SerializeField] private List<SeasonalEventSO> _springEvents = new List<SeasonalEventSO>();
        [SerializeField] private List<SeasonalEventSO> _summerEvents = new List<SeasonalEventSO>();
        [SerializeField] private List<SeasonalEventSO> _autumnEvents = new List<SeasonalEventSO>();
        [SerializeField] private List<SeasonalEventSO> _winterEvents = new List<SeasonalEventSO>();
        
        [Header("Cultural Content")]
        [SerializeField] private List<CulturalEventSO> _culturalEvents = new List<CulturalEventSO>();
        [SerializeField] private List<CannabisHolidaySO> _cannabisHolidays = new List<CannabisHolidaySO>();
        [SerializeField] private List<HistoricalEventSO> _historicalEvents = new List<HistoricalEventSO>();
        
        [Header("Environmental Content")]
        [SerializeField] private List<WeatherEventSO> _weatherEvents = new List<WeatherEventSO>();
        [SerializeField] private List<ClimateModifierSO> _seasonalModifiers = new List<ClimateModifierSO>();
        [SerializeField] private List<EnvironmentalThemeSO> _seasonalThemes = new List<EnvironmentalThemeSO>();
        
        [Header("Regional Variations")]
        [SerializeField] private List<RegionalSeasonSO> _regionalSeasons = new List<RegionalSeasonSO>();
        [SerializeField] private List<LocalizedContentSO> _localizedContent = new List<LocalizedContentSO>();
        [SerializeField] private bool _enableRegionalVariations = true;
        
        [Header("Content Management")]
        [SerializeField] private bool _enableAutomaticSeasonalTransitions = true;
        [SerializeField] private bool _enableCulturalSensitivityCheck = true;
        [SerializeField] private bool _enableEducationalValidation = true;
        [SerializeField] private float _contentTransitionDuration = 3600f; // 1 hour
        
        // Cached lookups for performance
        private Dictionary<Season, List<SeasonalEventSO>> _seasonalEventLookup;
        private Dictionary<string, CulturalEventSO> _culturalEventLookup;
        private Dictionary<DateTime, List<CannabisHolidaySO>> _holidayLookup;
        private Dictionary<string, RegionalSeasonSO> _regionalLookup;
        
        // Properties
        public bool EnableAutomaticSeasonalTransitions => _enableAutomaticSeasonalTransitions;
        public bool EnableCulturalSensitivityCheck => _enableCulturalSensitivityCheck;
        public bool EnableEducationalValidation => _enableEducationalValidation;
        public bool EnableRegionalVariations => _enableRegionalVariations;
        public float ContentTransitionDuration => _contentTransitionDuration;
        
        public IReadOnlyList<SeasonalEventSO> SpringEvents => _springEvents;
        public IReadOnlyList<SeasonalEventSO> SummerEvents => _summerEvents;
        public IReadOnlyList<SeasonalEventSO> AutumnEvents => _autumnEvents;
        public IReadOnlyList<SeasonalEventSO> WinterEvents => _winterEvents;
        
        public IReadOnlyList<CulturalEventSO> CulturalEvents => _culturalEvents;
        public IReadOnlyList<CannabisHolidaySO> CannabisHolidays => _cannabisHolidays;
        public IReadOnlyList<HistoricalEventSO> HistoricalEvents => _historicalEvents;
        
        public IReadOnlyList<WeatherEventSO> WeatherEvents => _weatherEvents;
        public IReadOnlyList<ClimateModifierSO> SeasonalModifiers => _seasonalModifiers;
        public IReadOnlyList<EnvironmentalThemeSO> SeasonalThemes => _seasonalThemes;
        
        private void OnEnable()
        {
            BuildContentLookups();
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (Application.isPlaying)
            {
                BuildContentLookups();
                ValidateSeasonalContent();
            }
        }
        
        private void BuildContentLookups()
        {
            // Build seasonal event lookup
            _seasonalEventLookup = new Dictionary<Season, List<SeasonalEventSO>>
            {
                { Season.Spring, _springEvents },
                { Season.Summer, _summerEvents },
                { Season.Autumn, _autumnEvents },
                { Season.Winter, _winterEvents }
            };
            
            // Build cultural event lookup
            _culturalEventLookup = new Dictionary<string, CulturalEventSO>();
            foreach (var culturalEvent in _culturalEvents)
            {
                if (culturalEvent != null)
                {
                    _culturalEventLookup[culturalEvent.EventId] = culturalEvent;
                }
            }
            
            // Build holiday lookup by month/day
            _holidayLookup = new Dictionary<DateTime, List<CannabisHolidaySO>>();
            foreach (var holiday in _cannabisHolidays)
            {
                if (holiday != null)
                {
                    var holidayDate = new DateTime(2000, holiday.Date.Month, holiday.Date.Day); // Use year 2000 as base
                    if (!_holidayLookup.ContainsKey(holidayDate))
                    {
                        _holidayLookup[holidayDate] = new List<CannabisHolidaySO>();
                    }
                    _holidayLookup[holidayDate].Add(holiday);
                }
            }
            
            // Build regional lookup
            _regionalLookup = new Dictionary<string, RegionalSeasonSO>();
            foreach (var regional in _regionalSeasons)
            {
                if (regional != null)
                {
                    _regionalLookup[regional.RegionId] = regional;
                }
            }
        }
        
        public List<SeasonalEventSO> GetSeasonalEvents(Season season)
        {
            if (_seasonalEventLookup == null)
            {
                BuildContentLookups();
            }
            
            return _seasonalEventLookup.GetValueOrDefault(season, new List<SeasonalEventSO>());
        }
        
        public SeasonalContent GetSeasonalContent(Season season, string regionId = null)
        {
            var seasonalEvents = GetSeasonalEvents(season);
            var seasonalModifiers = GetSeasonalModifiers(season);
            var seasonalTheme = GetSeasonalTheme(season);
            
            var content = new SeasonalContent
            {
                Season = season,
                Events = seasonalEvents,
                ClimateModifiers = seasonalModifiers,
                EnvironmentalTheme = seasonalTheme,
                WeatherEvents = GetSeasonalWeatherEvents(season)
            };
            
            // Apply regional variations if enabled and available
            if (_enableRegionalVariations && !string.IsNullOrEmpty(regionId))
            {
                ApplyRegionalVariations(content, regionId);
            }
            
            return content;
        }
        
        public List<CulturalEventSO> GetActiveCulturalEvents(DateTime currentDate)
        {
            return _culturalEvents
                .Where(evt => IsEventActiveOnDate(evt, currentDate))
                .ToList();
        }
        
        public List<CannabisHolidaySO> GetHolidaysForDate(DateTime date)
        {
            if (_holidayLookup == null)
            {
                BuildContentLookups();
            }
            
            var lookupDate = new DateTime(2000, date.Month, date.Day);
            return _holidayLookup.GetValueOrDefault(lookupDate, new List<CannabisHolidaySO>());
        }
        
        public List<CannabisHolidaySO> GetUpcomingHolidays(DateTime currentDate, int daysAhead = 30)
        {
            var upcomingHolidays = new List<CannabisHolidaySO>();
            
            for (int i = 0; i <= daysAhead; i++)
            {
                var checkDate = currentDate.AddDays(i);
                var holidaysOnDate = GetHolidaysForDate(checkDate);
                upcomingHolidays.AddRange(holidaysOnDate);
            }
            
            return upcomingHolidays.OrderBy(h => GetNextOccurrence(h.Date, currentDate)).ToList();
        }
        
        public Season GetCurrentSeason(DateTime currentDate, string regionId = null)
        {
            // Apply regional variations if available
            if (_enableRegionalVariations && !string.IsNullOrEmpty(regionId))
            {
                var regionalSeason = GetRegionalSeason(regionId);
                if (regionalSeason != null)
                {
                    return regionalSeason.GetSeasonForDate(currentDate);
                }
            }
            
            // Default northern hemisphere seasons
            var month = currentDate.Month;
            return month switch
            {
                12 or 1 or 2 => Season.Winter,
                3 or 4 or 5 => Season.Spring,
                6 or 7 or 8 => Season.Summer,
                9 or 10 or 11 => Season.Autumn,
                _ => Season.Spring
            };
        }
        
        public CulturalPeriod GetCulturalPeriod(DateTime currentDate)
        {
            // Check for active cultural events
            var activeCulturalEvents = GetActiveCulturalEvents(currentDate);
            
            if (activeCulturalEvents.Any())
            {
                var primaryEvent = activeCulturalEvents.OrderByDescending(e => e.Importance).First();
                return new CulturalPeriod
                {
                    PeriodName = primaryEvent.EventName,
                    CulturalEvents = activeCulturalEvents,
                    StartDate = primaryEvent.Date,
                    EndDate = primaryEvent.Date.Add(primaryEvent.CelebrationDuration),
                    CulturalContext = primaryEvent.CulturalContext
                };
            }
            
            // Check for cannabis holidays
            var holidaysToday = GetHolidaysForDate(currentDate);
            if (holidaysToday.Any())
            {
                var primaryHoliday = holidaysToday.OrderByDescending(h => h.Significance).First();
                return new CulturalPeriod
                {
                    PeriodName = primaryHoliday.HolidayName,
                    CannabisHolidays = holidaysToday,
                    StartDate = primaryHoliday.Date,
                    EndDate = primaryHoliday.Date.AddDays(1),
                    CulturalContext = primaryHoliday.CulturalSignificance
                };
            }
            
            // Return seasonal cultural period
            var currentSeason = GetCurrentSeason(currentDate);
            return new CulturalPeriod
            {
                PeriodName = $"{currentSeason} Season",
                Season = currentSeason,
                StartDate = GetSeasonStartDate(currentSeason, currentDate.Year),
                EndDate = GetSeasonEndDate(currentSeason, currentDate.Year),
                CulturalContext = GetSeasonalCulturalContext(currentSeason)
            };
        }
        
        public List<ClimateModifierSO> GetSeasonalModifiers(Season season)
        {
            return _seasonalModifiers
                .Where(modifier => modifier.ApplicableSeasons.Contains(season))
                .ToList();
        }
        
        public EnvironmentalThemeSO GetSeasonalTheme(Season season)
        {
            return _seasonalThemes
                .FirstOrDefault(theme => theme.Season == season);
        }
        
        public List<WeatherEventSO> GetSeasonalWeatherEvents(Season season)
        {
            return _weatherEvents
                .Where(weather => weather.Season == season)
                .ToList();
        }
        
        public RegionalSeasonSO GetRegionalSeason(string regionId)
        {
            if (_regionalLookup == null)
            {
                BuildContentLookups();
            }
            
            return _regionalLookup.GetValueOrDefault(regionId);
        }
        
        public List<LocalizedContentSO> GetLocalizedContent(string regionId, string languageCode)
        {
            return _localizedContent
                .Where(content => 
                    content.RegionId == regionId && 
                    content.LanguageCode == languageCode)
                .ToList();
        }
        
        private void ApplyRegionalVariations(SeasonalContent content, string regionId)
        {
            var regionalSeason = GetRegionalSeason(regionId);
            if (regionalSeason == null) return;
            
            // Apply regional climate modifiers
            var regionalModifiers = regionalSeason.GetRegionalModifiers(content.Season);
            content.ClimateModifiers.AddRange(regionalModifiers);
            
            // Apply regional theme variations
            var regionalTheme = regionalSeason.GetRegionalTheme(content.Season);
            if (regionalTheme != null)
            {
                content.EnvironmentalTheme = regionalTheme;
            }
            
            // Apply localized events
            var localizedEvents = regionalSeason.GetLocalizedEvents(content.Season);
            content.Events.AddRange(localizedEvents);
        }
        
        private bool IsEventActiveOnDate(CulturalEventSO culturalEvent, DateTime date)
        {
            var eventStart = culturalEvent.Date;
            var eventEnd = culturalEvent.Date.Add(culturalEvent.CelebrationDuration);
            
            return date >= eventStart && date <= eventEnd;
        }
        
        private DateTime GetNextOccurrence(DateTime holidayDate, DateTime currentDate)
        {
            var thisYear = new DateTime(currentDate.Year, holidayDate.Month, holidayDate.Day);
            
            if (thisYear >= currentDate)
            {
                return thisYear;
            }
            else
            {
                return new DateTime(currentDate.Year + 1, holidayDate.Month, holidayDate.Day);
            }
        }
        
        private DateTime GetSeasonStartDate(Season season, int year)
        {
            return season switch
            {
                Season.Spring => new DateTime(year, 3, 20), // Spring Equinox (approximate)
                Season.Summer => new DateTime(year, 6, 21), // Summer Solstice (approximate)
                Season.Autumn => new DateTime(year, 9, 22), // Autumn Equinox (approximate)
                Season.Winter => new DateTime(year, 12, 21), // Winter Solstice (approximate)
                _ => new DateTime(year, 1, 1)
            };
        }
        
        private DateTime GetSeasonEndDate(Season season, int year)
        {
            var nextSeason = GetNextSeason(season);
            return GetSeasonStartDate(nextSeason, nextSeason == Season.Spring ? year + 1 : year).AddDays(-1);
        }
        
        private Season GetNextSeason(Season currentSeason)
        {
            return currentSeason switch
            {
                Season.Spring => Season.Summer,
                Season.Summer => Season.Autumn,
                Season.Autumn => Season.Winter,
                Season.Winter => Season.Spring,
                _ => Season.Spring
            };
        }
        
        private string GetSeasonalCulturalContext(Season season)
        {
            return season switch
            {
                Season.Spring => "Season of renewal, new growth, and fresh beginnings in cannabis cultivation",
                Season.Summer => "Season of vigorous growth, abundant light, and peak cultivation activity",
                Season.Autumn => "Season of harvest, reflection, and celebrating the year's cultivation achievements",
                Season.Winter => "Season of planning, education, and preparing for the next growing cycle",
                _ => "A time of cannabis cultivation and community"
            };
        }
        
        private void ValidateSeasonalContent()
        {
            if (!_enableEducationalValidation) return;
            
            var validationErrors = new List<string>();
            
            // Validate seasonal events
            foreach (var seasonalEventList in _seasonalEventLookup.Values)
            {
                foreach (var seasonalEvent in seasonalEventList)
                {
                    if (seasonalEvent == null)
                    {
                        validationErrors.Add("Null seasonal event found");
                        continue;
                    }
                    
                    var errors = ValidateSeasonalEvent(seasonalEvent);
                    validationErrors.AddRange(errors);
                }
            }
            
            // Validate cultural events
            foreach (var culturalEvent in _culturalEvents)
            {
                if (culturalEvent == null)
                {
                    validationErrors.Add("Null cultural event found");
                    continue;
                }
                
                var errors = ValidateCulturalEvent(culturalEvent);
                validationErrors.AddRange(errors);
            }
            
            // Log validation results
            if (validationErrors.Count > 0)
            {
                Debug.LogError($"[SeasonalContentLibrarySO] Seasonal content validation failed with {validationErrors.Count} errors:\n{string.Join("\n", validationErrors)}");
            }
            else
            {
                Debug.Log($"[SeasonalContentLibrarySO] Seasonal content validation passed");
            }
        }
        
        private List<string> ValidateSeasonalEvent(SeasonalEventSO seasonalEvent)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(seasonalEvent.EventId))
                errors.Add($"Seasonal event missing ID");
            
            if (string.IsNullOrEmpty(seasonalEvent.EventName))
                errors.Add($"Seasonal event {seasonalEvent.EventId} missing name");
            
            return errors;
        }
        
        private List<string> ValidateCulturalEvent(CulturalEventSO culturalEvent)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(culturalEvent.EventId))
                errors.Add($"Cultural event missing ID");
            
            if (string.IsNullOrEmpty(culturalEvent.EventName))
                errors.Add($"Cultural event {culturalEvent.EventId} missing name");
            
            // Cultural sensitivity validation
            if (_enableCulturalSensitivityCheck)
            {
                if (culturalEvent.RequiresCulturalAuthenticity && string.IsNullOrEmpty(culturalEvent.CulturalContext))
                {
                    errors.Add($"Cultural event {culturalEvent.EventId} requires cultural context");
                }
            }
            
            return errors;
        }
        
        public List<string> GetAvailableRegions()
        {
            var regions = new List<string>();
            
            // Add default region
            regions.Add("default");
            
            // Add all unique region IDs from regional seasons
            foreach (var regionalSeason in _regionalSeasons)
            {
                if (regionalSeason != null && !string.IsNullOrEmpty(regionalSeason.RegionId))
                {
                    if (!regions.Contains(regionalSeason.RegionId))
                    {
                        regions.Add(regionalSeason.RegionId);
                    }
                }
            }
            
            // Add unique region IDs from localized content
            foreach (var localizedContent in _localizedContent)
            {
                if (localizedContent != null && !string.IsNullOrEmpty(localizedContent.RegionId))
                {
                    if (!regions.Contains(localizedContent.RegionId))
                    {
                        regions.Add(localizedContent.RegionId);
                    }
                }
            }
            
            return regions;
        }

        public SeasonalContentStatistics GetContentStatistics()
        {
            return new SeasonalContentStatistics
            {
                TotalSeasonalEvents = _springEvents.Count + _summerEvents.Count + _autumnEvents.Count + _winterEvents.Count,
                SpringEvents = _springEvents.Count,
                SummerEvents = _summerEvents.Count,
                AutumnEvents = _autumnEvents.Count,
                WinterEvents = _winterEvents.Count,
                CulturalEvents = _culturalEvents.Count,
                CannabisHolidays = _cannabisHolidays.Count,
                HistoricalEvents = _historicalEvents.Count,
                WeatherEvents = _weatherEvents.Count,
                RegionalVariations = _regionalSeasons.Count,
                LocalizedContent = _localizedContent.Count,
                LastValidated = DateTime.Now
            };
        }
    }
    
    // Supporting data structures
    [Serializable]
    public class SeasonalContent
    {
        public Season Season;
        public List<SeasonalEventSO> Events = new List<SeasonalEventSO>();
        public List<ClimateModifierSO> ClimateModifiers = new List<ClimateModifierSO>();
        public EnvironmentalThemeSO EnvironmentalTheme;
        public List<WeatherEventSO> WeatherEvents = new List<WeatherEventSO>();
    }
    
    [Serializable]
    public class CulturalPeriod
    {
        public string PeriodName;
        public List<CulturalEventSO> CulturalEvents = new List<CulturalEventSO>();
        public List<CannabisHolidaySO> CannabisHolidays = new List<CannabisHolidaySO>();
        public Season? Season;
        public DateTime StartDate;
        public DateTime EndDate;
        public string CulturalContext;
    }
    
    [Serializable]
    public class SeasonalContentStatistics
    {
        public int TotalSeasonalEvents;
        public int SpringEvents;
        public int SummerEvents;
        public int AutumnEvents;
        public int WinterEvents;
        public int CulturalEvents;
        public int CannabisHolidays;
        public int HistoricalEvents;
        public int WeatherEvents;
        public int RegionalVariations;
        public int LocalizedContent;
        public DateTime LastValidated;
    }
}