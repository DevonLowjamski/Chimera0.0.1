using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;

namespace ProjectChimera.Data.Events
{
    /// <summary>
    /// ScriptableObject representing discovery events for new strains, techniques, or knowledge.
    /// </summary>
    [CreateAssetMenu(fileName = "New Discovery Event", menuName = "Project Chimera/Events/Discovery Event", order = 104)]
    public class DiscoveryEventSO : ChimeraDataSO, ILiveEventDefinition
    {
        [Header("Event Identity")]
        [SerializeField] private string _eventId;
        [SerializeField] private string _eventName;
        [SerializeField] private string _description;
        [SerializeField] private ProjectChimera.Data.Events.EventType _eventType = ProjectChimera.Data.Events.EventType.DiscoveryEvent;
        [SerializeField] private EventScope _scope = EventScope.Global;
        
        [Header("Discovery Details")]
        [SerializeField] private DiscoveryType _discoveryType;
        [SerializeField] private RarityTier _rarityTier = RarityTier.Common;
        [SerializeField] private string _discoveryName;
        [SerializeField] private string _scientificName;
        
        [Header("Timing")]
        [SerializeField] private DateTime _startTime;
        [SerializeField] private DateTime _endTime;
        [SerializeField] private bool _isLimitedTime = true;
        
        [Header("Requirements")]
        [SerializeField] private List<string> _participationRequirements = new List<string>();
        [SerializeField] private int _minimumPlayerLevel = 1;
        [SerializeField] private List<string> _prerequisiteDiscoveries = new List<string>();
        
        [Header("Educational Content")]
        [SerializeField] private bool _hasEducationalContent = true;
        [SerializeField] private bool _requiresScientificAccuracy = true;
        [SerializeField] private bool _isScientificallyValidated = false;
        [SerializeField] private string _scientificBasis;
        [SerializeField] private List<string> _researchSources = new List<string>();
        
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
        
        // Discovery Event Properties
        public DiscoveryType DiscoveryType => _discoveryType;
        public RarityTier RarityTier => _rarityTier;
        public string DiscoveryName => _discoveryName;
        public string ScientificName => _scientificName;
        public bool IsLimitedTime => _isLimitedTime;
        public int MinimumPlayerLevel => _minimumPlayerLevel;
        public IReadOnlyList<string> PrerequisiteDiscoveries => _prerequisiteDiscoveries;
        public string ScientificBasis => _scientificBasis;
        public IReadOnlyList<string> ResearchSources => _researchSources;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (string.IsNullOrEmpty(_eventId))
            {
                _eventId = $"discovery_{name.ToLower().Replace(" ", "_")}_{DateTime.Now.Ticks}";
            }
        }
    }
    
    public enum DiscoveryType
    {
        NewStrain,
        GrowingTechnique,
        ScientificBreakthrough,
        CulturalKnowledge,
        TerpeneProfile,
        GeneticMarker,
        EnvironmentalFactor,
        ProcessingMethod
    }
} 