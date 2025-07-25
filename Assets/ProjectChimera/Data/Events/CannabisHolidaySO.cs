using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// ScriptableObject representing specific cannabis holidays and observances.
    /// Includes educational content about cannabis culture and traditions.
    /// </summary>
    [CreateAssetMenu(fileName = "New Cannabis Holiday", menuName = "Project Chimera/Events/Cannabis Holiday", order = 103)]
    public class CannabisHolidaySO : ChimeraDataSO
    {
        [Header("Holiday Identity")]
        [SerializeField] private string _holidayId;
        [SerializeField] private string _holidayName;
        [SerializeField] private string _description;
        [SerializeField] private DateTime _date;
        [SerializeField] private bool _isAnnualRecurring = true;
        
        [Header("Cultural Context")]
        [SerializeField] private string _culturalOrigin;
        [SerializeField] private string _historicalBackground;
        [SerializeField] private List<string> _traditions = new List<string>();
        [SerializeField] private List<string> _celebrationMethods = new List<string>();
        
        [Header("Global Recognition")]
        [SerializeField] private bool _isGloballyRecognized = false;
        [SerializeField] private List<string> _recognizedRegions = new List<string>();
        [SerializeField] private HolidaySignificance _significance = HolidaySignificance.Cultural;
        
        [Header("Educational Content")]
        [SerializeField] private bool _hasEducationalValue = true;
        [SerializeField] private string _educationalMessage;
        [SerializeField] private List<string> _keyLearningPoints = new List<string>();
        [SerializeField] private bool _promotesResponsibleUse = true;
        
        [Header("Community Activities")]
        [SerializeField] private List<string> _communityActivities = new List<string>();
        [SerializeField] private List<string> _virtualCelebrationIdeas = new List<string>();
        [SerializeField] private bool _supportsCharitableGiving = false;
        [SerializeField] private List<string> _charitableOrganizations = new List<string>();
        
        [Header("Game Integration")]
        [SerializeField] private bool _triggersSpecialEvents = true;
        [SerializeField] private List<string> _specialRewards = new List<string>();
        [SerializeField] private bool _hasDiscountOffers = false;
        [SerializeField] private float _discountPercentage = 0f;
        
        // Properties
        public string HolidayId => _holidayId;
        public string HolidayName => _holidayName;
        public string Description => _description;
        public DateTime Date => _date;
        public bool IsAnnualRecurring => _isAnnualRecurring;
        public string CulturalOrigin => _culturalOrigin;
        public string HistoricalBackground => _historicalBackground;
        public IReadOnlyList<string> Traditions => _traditions;
        public IReadOnlyList<string> CelebrationMethods => _celebrationMethods;
        public bool IsGloballyRecognized => _isGloballyRecognized;
        public IReadOnlyList<string> RecognizedRegions => _recognizedRegions;
        public HolidaySignificance Significance => _significance;
        public bool HasEducationalValue => _hasEducationalValue;
        public string EducationalMessage => _educationalMessage;
        public IReadOnlyList<string> KeyLearningPoints => _keyLearningPoints;
        public bool PromotesResponsibleUse => _promotesResponsibleUse;
        public IReadOnlyList<string> CommunityActivities => _communityActivities;
        public IReadOnlyList<string> VirtualCelebrationIdeas => _virtualCelebrationIdeas;
        public bool SupportsCharitableGiving => _supportsCharitableGiving;
        public IReadOnlyList<string> CharitableOrganizations => _charitableOrganizations;
        public bool TriggersSpecialEvents => _triggersSpecialEvents;
        public IReadOnlyList<string> SpecialRewards => _specialRewards;
        public bool HasDiscountOffers => _hasDiscountOffers;
        public float DiscountPercentage => _discountPercentage;
        
        // Additional compatibility properties
        public string CulturalSignificance => _historicalBackground + (!string.IsNullOrEmpty(_culturalOrigin) ? $" Origin: {_culturalOrigin}" : "");
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(_holidayId))
            {
                _holidayId = $"holiday_{name.ToLower().Replace(" ", "_")}_{DateTime.Now.Ticks}";
            }
            
            // Validate discount percentage
            _discountPercentage = Mathf.Clamp(_discountPercentage, 0f, 100f);
            
            // Ensure educational content is appropriate
            if (_hasEducationalValue && string.IsNullOrEmpty(_educationalMessage))
            {
                Debug.LogWarning($"Cannabis Holiday {_holidayName} has educational value but no educational message defined.");
            }
        }
        
        public bool IsActiveOn(DateTime date)
        {
            if (_isAnnualRecurring)
            {
                return date.Month == _date.Month && date.Day == _date.Day;
            }
            else
            {
                return date.Date == _date.Date;
            }
        }
        
        public bool IsActiveInRegion(string region)
        {
            if (_isGloballyRecognized) return true;
            return _recognizedRegions.Contains(region);
        }
        
        public TimeSpan GetTimeUntilHoliday(DateTime currentDate)
        {
            if (_isAnnualRecurring)
            {
                var thisYearDate = new DateTime(currentDate.Year, _date.Month, _date.Day);
                if (thisYearDate < currentDate)
                {
                    // Holiday already passed this year, calculate for next year
                    thisYearDate = new DateTime(currentDate.Year + 1, _date.Month, _date.Day);
                }
                return thisYearDate - currentDate;
            }
            else
            {
                return _date - currentDate;
            }
        }
    }
    
    public enum HolidaySignificance
    {
        Cultural,
        Historical,
        Advocacy,
        Educational,
        Community,
        International
    }
} 