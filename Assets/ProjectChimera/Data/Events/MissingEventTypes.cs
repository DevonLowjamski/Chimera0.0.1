using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;
using ProjectChimera.Data;
using System.Linq;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// Season enumeration already defined in EventDatabaseSO.cs
    /// Removed duplicate definition to resolve CS0101 error
    /// </summary>
    /// <summary>
    /// Collection of missing ScriptableObject types needed for EventDatabaseSO compilation.
    /// These are placeholder implementations that can be expanded later.
    /// </summary>
    
    public enum UrgencyLevel
    {
        Low,
        Medium,
        High,
        Critical,
        Emergency
    }
    
    [CreateAssetMenu(fileName = "New Competition Template", menuName = "Project Chimera/Events/Competition Template", order = 108)]
    public class CompetitionTemplateSO : ChimeraDataSO
    {
        [SerializeField] private string _templateId;
        [SerializeField] private string _templateName;
        [SerializeField] private CompetitionType _competitionType;
        
        public string TemplateId => _templateId;
        public string TemplateName => _templateName;
        public CompetitionType CompetitionType => _competitionType;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_templateId))
                _templateId = $"template_{name.ToLower().Replace(" ", "_")}";
        }
    }
    
    [CreateAssetMenu(fileName = "New Limited Release Event", menuName = "Project Chimera/Events/Limited Release Event", order = 109)]
    public class LimitedReleaseEventSO : ChimeraDataSO, ILiveEventDefinition
    {
        [SerializeField] private string _eventId;
        [SerializeField] private string _eventName;
        [SerializeField] private string _description;
        [SerializeField] private DateTime _startTime;
        [SerializeField] private DateTime _endTime;
        [SerializeField] private List<string> _participationRequirements = new List<string>();
        
        public string EventId => _eventId;
        public string EventName => _eventName;
        public string Description => _description;
        public object EventType => ProjectChimera.Data.Events.EventType.LimitedTimeOffer;
        public EventScope Scope => EventScope.Global;
        public DateTime StartTime => _startTime;
        public DateTime EndTime => _endTime;
        public TimeSpan Duration => _endTime - _startTime;
        public List<string> ParticipationRequirements => _participationRequirements;
        public bool HasEducationalContent => false;
        public bool RequiresScientificAccuracy => false;
        public bool IsScientificallyValidated => true;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_eventId))
                _eventId = $"limited_{name.ToLower().Replace(" ", "_")}";
        }
    }
    
    [CreateAssetMenu(fileName = "New Crisis Event Template", menuName = "Project Chimera/Events/Crisis Event Template", order = 110)]
    public class CrisisEventTemplateSO : ChimeraDataSO, ILiveEventDefinition
    {
        [SerializeField] private string _eventId;
        [SerializeField] private string _eventName;
        [SerializeField] private string _description;
        [SerializeField] private CrisisType _crisisType;
        [SerializeField] private UrgencyLevel _urgencyLevel = UrgencyLevel.Medium;
        [SerializeField] private DateTime _startTime;
        [SerializeField] private DateTime _endTime;
        [SerializeField] private List<string> _participationRequirements = new List<string>();
        
        public string EventId => _eventId;
        public string EventName => _eventName;
        public string Description => _description;
        public object EventType => ProjectChimera.Data.Events.EventType.CrisisResponse;
        public EventScope Scope => EventScope.Global;
        public DateTime StartTime => _startTime;
        public DateTime EndTime => _endTime;
        public TimeSpan Duration => _endTime - _startTime;
        public List<string> ParticipationRequirements => _participationRequirements;
        public bool HasEducationalContent => true;
        public bool RequiresScientificAccuracy => true;
        public bool IsScientificallyValidated => false;
        public CrisisType CrisisType => _crisisType;
        public UrgencyLevel UrgencyLevel => _urgencyLevel;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_eventId))
                _eventId = $"crisis_{name.ToLower().Replace(" ", "_")}";
        }
    }
    
    [CreateAssetMenu(fileName = "New Emergency Response", menuName = "Project Chimera/Events/Emergency Response", order = 111)]
    public class EmergencyResponseSO : ChimeraDataSO
    {
        [SerializeField] private string _responseId;
        [SerializeField] private string _responseName;
        [SerializeField] private string _description;
        [SerializeField] private CrisisType _applicableCrisisType;
        
        public string ResponseId => _responseId;
        public string ResponseName => _responseName;
        public string Description => _description;
        public CrisisType ApplicableCrisisType => _applicableCrisisType;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_responseId))
                _responseId = $"response_{name.ToLower().Replace(" ", "_")}";
        }
    }
    
    [CreateAssetMenu(fileName = "New Weather Event", menuName = "Project Chimera/Events/Weather Event", order = 112)]
    public class WeatherEventSO : ChimeraDataSO, ILiveEventDefinition
    {
        [SerializeField] private string _eventId;
        [SerializeField] private string _eventName;
        [SerializeField] private string _description;
        [SerializeField] private DateTime _startTime;
        [SerializeField] private DateTime _endTime;
        [SerializeField] private Season _season = Season.Spring;
        [SerializeField] private List<string> _participationRequirements = new List<string>();
        
        public string EventId => _eventId;
        public string EventName => _eventName;
        public string Description => _description;
        public object EventType => ProjectChimera.Data.Events.EventType.SeasonalEvent;
        public EventScope Scope => EventScope.Regional;
        public DateTime StartTime => _startTime;
        public DateTime EndTime => _endTime;
        public Season Season => _season;
        public TimeSpan Duration => _endTime - _startTime;
        public List<string> ParticipationRequirements => _participationRequirements;
        public bool HasEducationalContent => true;
        public bool RequiresScientificAccuracy => true;
        public bool IsScientificallyValidated => true;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_eventId))
                _eventId = $"weather_{name.ToLower().Replace(" ", "_")}";
        }
    }
    
    [CreateAssetMenu(fileName = "New Participation Requirement", menuName = "Project Chimera/Events/Participation Requirement", order = 113)]
    public class ParticipationRequirementSO : ChimeraDataSO
    {
        [SerializeField] private string _requirementId;
        [SerializeField] private string _requirementName;
        [SerializeField] private string _description;
        [SerializeField] private RequirementType _requirementType;
        [SerializeField] private int _minimumValue = 0;
        [SerializeField] private bool _isMinify = false;
        
        public string RequirementId => _requirementId;
        public string RequirementName => _requirementName;
        public string Description => _description;
        public RequirementType RequirementType => _requirementType;
        public int MinimumValue => _minimumValue;
        public bool IsMinify => _isMinify;
        
        public bool IsMetBy(PlayerProfile playerProfile)
        {
            // This would typically check against player data based on RequirementType
            // For now, return true as placeholder implementation
            return true;
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_requirementId))
                _requirementId = $"req_{name.ToLower().Replace(" ", "_")}";
        }
    }
    
    public enum RequirementType
    {
        PlayerLevel,
        SkillLevel,
        CompletedEvents,
        OwnedItems,
        PlantCount,
        HarvestCount,
        StrainCount,
        Achievement
    }
    
    // Additional event types for Community Challenges
    


    [Serializable]
    public class EventReward
    {
        [Header("Reward Configuration")]
        public string RewardId;
        public string RewardName;
        public string Description;
        public RewardType RewardType;
        public int Quantity = 1;
        public float Value = 100f;
        public RarityTier Rarity = RarityTier.Common;
        public bool IsUnique = false;
        public string IconPath;
    }

    public enum ChallengeType
    {
        CollectiveGrowing,
        SkillSharing,
        KnowledgeQuest,
        CommunityBuilding,
        Research,
        Innovation,
        Conservation,
        CompetitiveGrowing,
        ProblemSolving,
        Mentorship,
        KnowledgeSharing,
        ResourceSharing,
        Environmental
    }

    public enum CollaborationMode
    {
        FreeForm,
        TeamBased,
        Structured,
        Mentorship,
        PeerToPeer,
        Competitive,
        Cooperative,
        Sequential,
        Parallel
    }

    public enum RewardDistributionType
    {
        Community,
        Individual,
        ProportionalContribution,
        EqualDistribution,
        TopPerformers,
        Participation,
        Milestone,
        RandomDraw
    }

    [CreateAssetMenu(fileName = "New Competition Category", menuName = "Project Chimera/Events/Competition Category", order = 115)]
    public class CompetitionCategorySO : ChimeraDataSO
    {
        [SerializeField] private string _categoryId;
        [SerializeField] private string _categoryName;
        [SerializeField] private string _description;
        [SerializeField] private int _maxParticipants = 100;
        [SerializeField] private bool _allowTeams = false;

        public string CategoryId => _categoryId;
        public string CategoryName => _categoryName;
        public string Description => _description;
        public int MaxParticipants => _maxParticipants;
        public bool AllowTeams => _allowTeams;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_categoryId))
                _categoryId = $"category_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [CreateAssetMenu(fileName = "New Judging Criteria", menuName = "Project Chimera/Events/Judging Criteria", order = 116)]
    public class JudgingCriteriaSO : ChimeraDataSO
    {
        [SerializeField] private string _criteriaId;
        [SerializeField] private string _criteriaName;
        [SerializeField] private List<JudgingCriterion> _criteria = new List<JudgingCriterion>();

        public string CriteriaId => _criteriaId;
        public string CriteriaName => _criteriaName;
        public List<JudgingCriterion> Criteria => _criteria;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_criteriaId))
                _criteriaId = $"criteria_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [CreateAssetMenu(fileName = "New Leaderboard Config", menuName = "Project Chimera/Events/Leaderboard Config", order = 117)]
    public class LeaderboardConfigSO : ChimeraDataSO
    {
        [SerializeField] private string _configId;
        [SerializeField] private string _configName;
        [SerializeField] private bool _enableRealTimeUpdates = true;
        [SerializeField] private int _maxEntries = 100;

        public string ConfigId => _configId;
        public string ConfigName => _configName;
        public bool EnableRealTimeUpdates => _enableRealTimeUpdates;
        public int MaxEntries => _maxEntries;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_configId))
                _configId = $"config_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [CreateAssetMenu(fileName = "New Tiered Reward Structure", menuName = "Project Chimera/Events/Tiered Reward Structure", order = 118)]
    public class TieredRewardStructureSO : ChimeraDataSO
    {
        [SerializeField] private string _structureId;
        [SerializeField] private string _structureName;
        [SerializeField] private List<RewardTier> _tiers = new List<RewardTier>();

        public string StructureId => _structureId;
        public string StructureName => _structureName;
        public List<RewardTier> Tiers => _tiers;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_structureId))
                _structureId = $"structure_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [CreateAssetMenu(fileName = "New Participation Reward", menuName = "Project Chimera/Events/Participation Reward", order = 119)]
    public class ParticipationRewardSO : ChimeraDataSO
    {
        [SerializeField] private string _rewardId;
        [SerializeField] private string _rewardName;
        [SerializeField] private EventReward _reward;

        public string RewardId => _rewardId;
        public string RewardName => _rewardName;
        public EventReward Reward => _reward;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_rewardId))
                _rewardId = $"reward_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [CreateAssetMenu(fileName = "New Special Recognition", menuName = "Project Chimera/Events/Special Recognition", order = 120)]
    public class SpecialRecognitionSO : ChimeraDataSO
    {
        [SerializeField] private string _recognitionId;
        [SerializeField] private string _recognitionName;
        [SerializeField] private string _description;
        [SerializeField] private string _badgeIcon;

        public string RecognitionId => _recognitionId;
        public string RecognitionName => _recognitionName;
        public string Description => _description;
        public string BadgeIcon => _badgeIcon;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_recognitionId))
                _recognitionId = $"recognition_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [CreateAssetMenu(fileName = "New Educational Content", menuName = "Project Chimera/Events/Educational Content", order = 121)]
    public class EducationalContentSO : ChimeraDataSO
    {
        [SerializeField] private string _contentId;
        [SerializeField] private string _contentName;
        [SerializeField] private string _description;
        [SerializeField] private List<string> _learningObjectives = new List<string>();

        public string ContentId => _contentId;
        public string ContentName => _contentName;
        public string Description => _description;
        public List<string> LearningObjectives => _learningObjectives;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_contentId))
                _contentId = $"content_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [CreateAssetMenu(fileName = "New Community Features", menuName = "Project Chimera/Events/Community Features", order = 122)]
    public class CommunityFeaturesSO : ChimeraDataSO
    {
        [SerializeField] private string _featuresId;
        [SerializeField] private string _featuresName;
        [SerializeField] private bool _enableChat = true;
        [SerializeField] private bool _enableSharing = true;

        public string FeaturesId => _featuresId;
        public string FeaturesName => _featuresName;
        public bool EnableChat => _enableChat;
        public bool EnableSharing => _enableSharing;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_featuresId))
                _featuresId = $"features_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [CreateAssetMenu(fileName = "New Submission Requirements", menuName = "Project Chimera/Events/Submission Requirements", order = 123)]
    public class SubmissionRequirementsSO : ChimeraDataSO
    {
        [SerializeField] private string _requirementsId;
        [SerializeField] private string _requirementsName;
        [SerializeField] private List<string> _requirements = new List<string>();

        public string RequirementsId => _requirementsId;
        public string RequirementsName => _requirementsName;
        public List<string> Requirements => _requirements;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_requirementsId))
                _requirementsId = $"requirements_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [Serializable]
    public class JudgingCriterion
    {
        [Header("Criterion Details")]
        public string CriterionId;
        public string Name;
        public string Description;
        public float Weight = 1.0f;
        public int MaxScore = 100;
    }

    [Serializable]
    public class RewardTier
    {
        [Header("Tier Configuration")]
        public int Tier;
        public string TierName;
        public List<EventReward> Rewards = new List<EventReward>();
        public int MinimumRank = 1;
        public int MaximumRank = 1;
    }

    [CreateAssetMenu(fileName = "New Cultural Event Library", menuName = "Project Chimera/Events/Cultural Event Library", order = 201)]
    public class CulturalEventLibrarySO : ChimeraDataSO
    {
        [Header("Cultural Event Configuration")]
        [SerializeField] private string _libraryId;
        [SerializeField] private string _libraryName;
        [SerializeField] private List<CulturalEventData> _culturalEvents = new List<CulturalEventData>();

        public string LibraryId => _libraryId;
        public string LibraryName => _libraryName;
        public List<CulturalEventData> CulturalEvents => _culturalEvents;
    }

    [CreateAssetMenu(fileName = "New Community Challenge Database", menuName = "Project Chimera/Events/Community Challenge Database", order = 202)]
    public class CommunityChallengeDatabaseSO : ChimeraDataSO
    {
        [Header("Community Challenge Configuration")]
        [SerializeField] private string _databaseId;
        [SerializeField] private string _databaseName;
        [SerializeField] private List<CommunityChallenge> _challenges = new List<CommunityChallenge>();

        public string DatabaseId => _databaseId;
        public string DatabaseName => _databaseName;
        public List<CommunityChallenge> Challenges => _challenges;
    }

    [CreateAssetMenu(fileName = "New Global Event Template", menuName = "Project Chimera/Events/Global Event Template", order = 203)]
    public class GlobalEventTemplateSO : ChimeraDataSO
    {
        [Header("Global Event Template Configuration")]
        [SerializeField] private string _templateId;
        [SerializeField] private string _templateName;
        [SerializeField] private string _description;
        [SerializeField] private EventScope _eventScope = EventScope.Global;
        [SerializeField] private EventPriority _priority = EventPriority.Medium;

        public string TemplateId => _templateId;
        public string TemplateName => _templateName;
        public string Description => _description;
        public EventScope EventScope => _eventScope;
        public EventPriority Priority => _priority;
    }

    [Serializable]
    public class CulturalEventData
    {
        [Header("Cultural Event Information")]
        public string EventId;
        public string EventName;
        public string Description;
        public string CulturalContext;
        public DateTime EventDate;
        public bool IsRecurring;
    }

    [Serializable]
    public class CommunityChallenge
    {
        [Header("Challenge Details")]
        public string ChallengeId;
        public string ChallengeName;
        public string Description;
        public DateTime StartDate;
        public DateTime EndDate;
        public int ParticipantLimit;
        public bool IsActive;
    }

    // Character and event tracking - using CharacterEventData from ProjectChimera.Events.Narrative

    [CreateAssetMenu(fileName = "New Historical Event", menuName = "Project Chimera/Events/Historical Event", order = 204)]
    public class HistoricalEventSO : ChimeraDataSO
    {
        [Header("Historical Event Configuration")]
        [SerializeField] private string _eventId;
        [SerializeField] private string _eventName;
        [SerializeField] private string _description;
        [SerializeField] private DateTime _historicalDate;
        [SerializeField] private string _significance;

        public string EventId => _eventId;
        public string EventName => _eventName;
        public string Description => _description;
        public DateTime HistoricalDate => _historicalDate;
        public string Significance => _significance;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_eventId))
                _eventId = $"historical_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [CreateAssetMenu(fileName = "New Climate Modifier", menuName = "Project Chimera/Events/Climate Modifier", order = 205)]
    public class ClimateModifierSO : ChimeraDataSO
    {
        [Header("Climate Modifier Configuration")]
        [SerializeField] private string _modifierId;
        [SerializeField] private string _modifierName;
        [SerializeField] private float _temperatureEffect;
        [SerializeField] private float _humidityEffect;
        [SerializeField] private float _lightEffect;
        [SerializeField] private List<Season> _applicableSeasons = new List<Season>();

        public string ModifierId => _modifierId;
        public string ModifierName => _modifierName;
        public float TemperatureEffect => _temperatureEffect;
        public float HumidityEffect => _humidityEffect;
        public float LightEffect => _lightEffect;
        public List<Season> ApplicableSeasons => _applicableSeasons;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_modifierId))
                _modifierId = $"climate_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [CreateAssetMenu(fileName = "New Environmental Theme", menuName = "Project Chimera/Events/Environmental Theme", order = 206)]
    public class EnvironmentalThemeSO : ChimeraDataSO
    {
        [Header("Environmental Theme Configuration")]
        [SerializeField] private string _themeId;
        [SerializeField] private string _themeName;
        [SerializeField] private Season _season = Season.Spring;
        [SerializeField] private Color _primaryColor;
        [SerializeField] private Color _secondaryColor;
        [SerializeField] private Texture2D _backgroundTexture;

        public string ThemeId => _themeId;
        public string ThemeName => _themeName;
        public Season Season => _season;
        public Color PrimaryColor => _primaryColor;
        public Color SecondaryColor => _secondaryColor;
        public Texture2D BackgroundTexture => _backgroundTexture;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_themeId))
                _themeId = $"theme_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [CreateAssetMenu(fileName = "New Regional Season", menuName = "Project Chimera/Events/Regional Season", order = 207)]
    public class RegionalSeasonSO : ChimeraDataSO
    {
        [Header("Regional Season Configuration")]
        [SerializeField] private string _regionId;
        [SerializeField] private string _regionName;
        [SerializeField] private Season _season;
        [SerializeField] private DateTime _startDate;
        [SerializeField] private DateTime _endDate;

        public string RegionId => _regionId;
        public string RegionName => _regionName;
        public Season Season => _season;
        public DateTime StartDate => _startDate;
        public DateTime EndDate => _endDate;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_regionId))
                _regionId = $"region_{name.ToLower().Replace(" ", "_")}";
        }
        
        // Additional methods required by the system
        public Season GetSeasonForDate(DateTime date)
        {
            if (date >= _startDate && date <= _endDate)
                return _season;
            return Season.Spring; // Default fallback
        }
        
        public List<ClimateModifierSO> GetRegionalModifiers(Season season)
        {
            // Return empty list as placeholder - this would typically be populated from a database
            return new List<ClimateModifierSO>();
        }
        
        public EnvironmentalThemeSO GetRegionalTheme(Season season)
        {
            // Return null as placeholder - this would typically be populated from a database
            return null;
        }
        
        public List<SeasonalEventSO> GetLocalizedEvents(Season season)
        {
            // Return empty list as placeholder - this would typically be populated from a database
            return new List<SeasonalEventSO>();
        }
    }

    [CreateAssetMenu(fileName = "New Localized Content", menuName = "Project Chimera/Events/Localized Content", order = 208)]
    public class LocalizedContentSO : ChimeraDataSO
    {
        [Header("Localized Content Configuration")]
        [SerializeField] private string _contentId;
        [SerializeField] private string _regionId;
        [SerializeField] private string _languageCode;
        [SerializeField] private string _localizedText;
        [SerializeField] private List<CulturalEventSO> _regionalEvents = new List<CulturalEventSO>();

        public string ContentId => _contentId;
        public string RegionId => _regionId;
        public string LanguageCode => _languageCode;
        public string LocalizedText => _localizedText;
        public List<CulturalEventSO> RegionalEvents => _regionalEvents;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_contentId))
                _contentId = $"content_{name.ToLower().Replace(" ", "_")}";
        }
    }

    // Weather and environmental enums
    public enum WeatherType
    {
        Sunny,
        Cloudy,
        Rainy,
        Stormy,
        Snowy,
        Foggy,
        Windy,
        Humid,
        Drought,
        Heatwave,
        Freeze
    }

    // Note: PlayerProfile is now defined in SharedDataStructures.cs

    public enum SeasonalEventType
    {
        // General seasonal categories
        Seasonal,
        Cultural,
        Holiday,
        Weather,
        // Specific spring events
        Spring_PlantingBonus,
        Spring_NewCropAvailable,
        Spring_WeatherBonus,
        // Specific summer events
        Summer_GrowthBonus,
        Summer_HeatWave,
        Summer_ExtraHarvest,
        // Specific fall events
        Fall_HarvestBonus,
        Fall_MarketPriceIncrease,
        Fall_StormWeather,
        // Specific winter events
        Winter_IndoorGrowing,
        Winter_EquipmentMaintenance,
        Winter_PlanningPhase,
        // Special events
        Equinox_SpecialEvent,
        Solstice_CelebrationEvent,
        RandomWeather_Event,
        Pest_Management_Event,
        Disease_Prevention_Event,
        Nutrient_Deficiency_Event,
        Equipment_Failure_Event,
        Market_Fluctuation_Event
    }

    // Player Agency and Cultivation Gaming Event Data
    [System.Serializable]
    public class PlayerChoiceEventData
    {
        [Header("Choice Information")]
        public string ChoiceId;
        public string ChoiceName;
        public string Description;
        public string PlayerId;
        public DateTime ChoiceTimestamp;
        
        [Header("Choice Details")]
        public string ChoiceCategory;
        public int ChoiceValue;
        public Dictionary<string, object> ChoiceParameters = new Dictionary<string, object>();
        public List<string> ConsequenceIds = new List<string>();
        
        [Header("Context")]
        public string PlantId;
        public string FacilityId;
        public string SystemContext;
        public object ChoiceContext;
    }

    // Event Channel ScriptableObjects for different systems
    [CreateAssetMenu(fileName = "New Consequence Event Channel", menuName = "Project Chimera/Events/Consequence Event Channel", order = 301)]
    public class ConsequenceEventChannelSO : GameEventChannelSO
    {
        [Header("Consequence Event Channel Configuration")]
        [SerializeField] private string _channelId;
        [SerializeField] private bool _trackConsequenceHistory = true;
        
        public string ChannelId => _channelId;
        public bool TrackConsequenceHistory => _trackConsequenceHistory;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_channelId))
                _channelId = $"consequence_channel_{name.ToLower().Replace(" ", "_")}";
        }
    }

    [CreateAssetMenu(fileName = "New Character Event Channel", menuName = "Project Chimera/Events/Character Event Channel", order = 302)]
    public class CharacterEventChannelSO : GameEventChannelSO
    {
        [Header("Character Event Channel Configuration")]
        [SerializeField] private string _channelId;
        [SerializeField] private bool _trackCharacterInteractions = true;
        
        public string ChannelId => _channelId;
        public bool TrackCharacterInteractions => _trackCharacterInteractions;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(_channelId))
                _channelId = $"character_channel_{name.ToLower().Replace(" ", "_")}";
        }
    }

    // Character relationship and effectiveness data structures
    [System.Serializable]
    public class EffectivenessDataPoint
    {
        [Header("Effectiveness Measurement")]
        public string DataPointId;
        public DateTime Timestamp;
        public float EffectivenessValue; // 0.0 to 1.0
        public string Context;
        public string RelatedCharacterId;
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
    }

    [System.Serializable]
    public class EffectivenessTrend
    {
        [Header("Effectiveness Trend Analysis")]
        public string TrendId;
        public List<EffectivenessDataPoint> DataPoints = new List<EffectivenessDataPoint>();
        public float TrendSlope; // Positive = improving, Negative = declining
        public DateTime AnalysisDate;
        public string TrendCategory;
    }
} 