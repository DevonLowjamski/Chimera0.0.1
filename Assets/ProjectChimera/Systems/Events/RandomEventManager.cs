using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data;
using ProjectChimera.Data.Events;
using ProjectChimera.Data.Progression;
using System.Linq;

// Explicit namespace alias to resolve ambiguity
using EnvironmentalManager = ProjectChimera.Systems.Environment.EnvironmentalManager;
using SimpleGameEventSO = ProjectChimera.Core.SimpleGameEventSO;

// Resolve enum conflicts
using EventCategory = ProjectChimera.Data.Events.RandomEventCategory;

namespace ProjectChimera.Systems.Events
{
    /// <summary>
    /// Manages random events for Project Chimera to create dynamic, unpredictable gameplay.
    /// Transforms predictable simulation into engaging entertainment through emergent storytelling,
    /// strategic decision-making, and surprise elements that keep players engaged.
    /// </summary>
    public class RandomEventManager : ChimeraManager
    {
        [Header("Event Configuration")]
        [SerializeField] private bool _enableRandomEvents = true;
        [SerializeField] private float _baseEventInterval = 300f; // 5 minutes base
        [SerializeField] private float _eventIntervalVariance = 120f; // ±2 minutes
        [SerializeField] private int _maxActiveEvents = 3;
        [SerializeField] private float _eventDecisionTimeLimit = 180f; // 3 minutes to decide
        
        [Header("Event Probability")]
        [SerializeField] private float _positiveEventChance = 0.4f;
        [SerializeField] private float _neutralEventChance = 0.35f;
        [SerializeField] private float _negativeEventChance = 0.25f;
        [SerializeField] private float _marketEventChance = 0.3f;
        [SerializeField] private float _weatherEventChance = 0.25f;
        [SerializeField] private float _socialEventChance = 0.2f;
        
        [Header("Dynamic Scaling")]
        [SerializeField] private bool _enableDynamicDifficulty = true;
        [SerializeField] private float _playerLevelMultiplier = 0.1f;
        [SerializeField] private bool _enableStoryProgression = true;
        [SerializeField] private AnimationCurve _eventFrequencyCurve;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onEventTriggered;
        [SerializeField] private SimpleGameEventSO _onEventResolved;
        [SerializeField] private SimpleGameEventSO _onCrisisEvent;
        [SerializeField] private SimpleGameEventSO _onOpportunityEvent;
        [SerializeField] private SimpleGameEventSO _onStoryProgression;
        
        // Event state
        private List<ActiveRandomEvent> _activeEvents = new List<ActiveRandomEvent>();
        private List<RandomEventTemplate> _availableEvents = new List<RandomEventTemplate>();
        private Queue<StoryEvent> _storyEventQueue = new Queue<StoryEvent>();
        private Dictionary<EventCategory, float> _categoryProbabilities = new Dictionary<EventCategory, float>();
        
        // Timing and tracking
        private float _timeUntilNextEvent;
        private float _lastEventTime;
        private int _totalEventsTriggered = 0;
        private int _playerDecisionsMade = 0;
        private EventDifficultyLevel _currentDifficultyLevel = EventDifficultyLevel.Easy;
        
        // Story progression
        private List<string> _completedStoryArcs = new List<string>();
        private Dictionary<string, int> _storyProgress = new Dictionary<string, int>();
        private float _playerReputationScore = 50f; // 0-100 scale
        
        // Manager references (using object to avoid assembly dependency issues)
        private object _progressionManager;
        private object _plantManager;
        
        public override ManagerPriority Priority => ManagerPriority.Normal;
        
        // Public Properties
        public int ActiveEventCount => _activeEvents.Count;
        public int TotalEventsTriggered => _totalEventsTriggered;
        public float PlayerReputationScore => _playerReputationScore;
        public bool HasActiveEvents => _activeEvents.Count > 0;
        public List<ActiveRandomEvent> ActiveEvents => _activeEvents.ToList();
        
        // Events
        public System.Action<ActiveRandomEvent> OnEventStarted;
        public System.Action<ActiveRandomEvent, EventChoice> OnEventResolved;
        public System.Action<EventCategory> OnCategoryEventTriggered;
        public System.Action<float> OnReputationChanged;
        
        protected override void OnManagerInitialize()
        {
            InitializeSystemReferences();
            InitializeEventTemplates();
            InitializeCategoryProbabilities();
            InitializeStoryEvents();
            CalculateNextEventTime();
            
            if (_enableDynamicDifficulty)
            {
                UpdateDifficultyLevel();
            }
            
            LogInfo($"RandomEventManager initialized with {_availableEvents.Count} event templates");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!_enableRandomEvents) return;
            
            float currentTime = Time.time;
            
            // Check for new random events
            if (currentTime >= _lastEventTime + _timeUntilNextEvent)
            {
                if (_activeEvents.Count < _maxActiveEvents)
                {
                    TriggerRandomEvent();
                }
                CalculateNextEventTime();
            }
            
            // Update active events
            UpdateActiveEvents();
            
            // Process story events
            if (_enableStoryProgression)
            {
                ProcessStoryEvents();
            }
            
            // Update difficulty scaling
            if (_enableDynamicDifficulty)
            {
                UpdateDifficultyLevel();
            }
        }
        
        /// <summary>
        /// Manually trigger a specific event category for testing or story purposes
        /// </summary>
        public void TriggerEventByCategory(EventCategory category)
        {
            var categoryEvents = _availableEvents.Where(e => e.Category == category).ToList();
            if (categoryEvents.Count > 0)
            {
                var selectedEvent = categoryEvents[UnityEngine.Random.Range(0, categoryEvents.Count)];
                CreateActiveEvent(selectedEvent);
            }
        }
        
        /// <summary>
        /// Make a decision on an active event
        /// </summary>
        public void MakeEventDecision(string eventId, int choiceIndex)
        {
            var activeEvent = _activeEvents.FirstOrDefault(e => e.EventId == eventId);
            if (activeEvent == null)
            {
                LogError($"Cannot find active event: {eventId}");
                return;
            }
            
            if (choiceIndex < 0 || choiceIndex >= activeEvent.Choices.Count)
            {
                LogError($"Invalid choice index {choiceIndex} for event {eventId}");
                return;
            }
            
            var choice = activeEvent.Choices[choiceIndex];
            ResolveEvent(activeEvent, choice);
        }
        
        /// <summary>
        /// Get current event display data for UI
        /// </summary>
        public List<EventDisplayData> GetEventDisplayData()
        {
            var displayData = new List<EventDisplayData>();
            
            foreach (var activeEvent in _activeEvents)
            {
                displayData.Add(new EventDisplayData
                {
                    EventId = activeEvent.EventId,
                    Title = activeEvent.Title,
                    Description = activeEvent.Description,
                    Category = activeEvent.Category,
                    Severity = activeEvent.Severity,
                    TimeRemaining = activeEvent.ExpirationTime - DateTime.Now,
                    Choices = activeEvent.Choices.Select(c => c.ChoiceText).ToList(),
                    CategoryIcon = GetCategoryIcon(activeEvent.Category),
                    SeverityColor = GetSeverityColor(activeEvent.Severity),
                    HasTimeLimit = activeEvent.HasTimeLimit,
                    StoryContext = activeEvent.StoryContext
                });
            }
            
            return displayData;
        }
        
        private void InitializeSystemReferences()
        {
            // Initialize manager references for progression tracking
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                // Use reflection to avoid assembly dependency issues
                var progressionManagerType = System.Type.GetType("ProjectChimera.Systems.Progression.ProgressionManager, ProjectChimera.Systems.Progression");
                if (progressionManagerType != null)
                {
                    _progressionManager = gameManager.GetType().GetMethod("GetManager").MakeGenericMethod(progressionManagerType).Invoke(gameManager, null);
                }
            }
        }
        
        private int GetPlayerLevel()
        {
            if (_progressionManager != null)
            {
                var playerLevelProperty = _progressionManager.GetType().GetProperty("PlayerLevel");
                if (playerLevelProperty != null)
                {
                    return (int)playerLevelProperty.GetValue(_progressionManager);
                }
            }
            return 1; // Default level
        }
        
        private void GainExperience(float experienceGain, object experienceSource)
        {
            if (_progressionManager != null)
            {
                var gainExperienceMethod = _progressionManager.GetType().GetMethod("GainExperience");
                if (gainExperienceMethod != null)
                {
                    gainExperienceMethod.Invoke(_progressionManager, new object[] { experienceGain, experienceSource });
                }
            }
        }
        
        private void InitializeEventTemplates()
        {
            CreateCultivationEvents();
            CreateMarketEvents();
            CreateWeatherEvents();
            CreateSocialEvents();
            CreateTechnologyEvents();
            CreateCrisisEvents();
            CreateOpportunityEvents();
            
            LogInfo($"Initialized {_availableEvents.Count} random event templates");
        }
        
        private void CreateCultivationEvents()
        {
            var cultivationEvents = new List<RandomEventTemplate>
            {
                new RandomEventTemplate
                {
                    EventId = "pest_outbreak",
                    Title = "Pest Outbreak Detected!",
                    Description = "Spider mites have been spotted in one of your grow rooms. Quick action is needed to prevent spread to your entire crop.",
                    Category = EventCategory.Cultivation,
                    Severity = EventSeverity.High,
                    Probability = 0.15f,
                    HasTimeLimit = true,
                    TimeLimit = TimeSpan.FromHours(6),
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            ChoiceText = "Apply immediate pesticide treatment ($500)",
                            CostCurrency = 500f,
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.PlantHealth, Value = -0.1f, Duration = 24f },
                                new EventConsequence { Type = ConsequenceType.Currency, Value = -500f },
                                new EventConsequence { Type = ConsequenceType.Reputation, Value = 2f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Use organic predator insects ($800)",
                            CostCurrency = 800f,
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.PlantHealth, Value = 0.05f, Duration = 72f },
                                new EventConsequence { Type = ConsequenceType.Currency, Value = -800f },
                                new EventConsequence { Type = ConsequenceType.Reputation, Value = 5f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Quarantine affected plants (lose 1-3 plants)",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.PlantLoss, Value = 2f },
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 100f }
                            }
                        }
                    }
                },
                new RandomEventTemplate
                {
                    EventId = "genetic_mutation",
                    Title = "Rare Genetic Mutation Discovered",
                    Description = "One of your plants is showing unusual traits - enhanced trichome production and unique terpene profiles. This could be valuable!",
                    Category = EventCategory.Cultivation,
                    Severity = EventSeverity.Positive,
                    Probability = 0.05f,
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            ChoiceText = "Carefully clone and study the mutation",
                            CostTime = 48f,
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.UnlockStrain, Value = 1f },
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 500f },
                                new EventConsequence { Type = ConsequenceType.Reputation, Value = 10f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Sell information to genetics company ($2000)",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Currency, Value = 2000f },
                                new EventConsequence { Type = ConsequenceType.Reputation, Value = -5f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Continue normal cultivation",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 50f }
                            }
                        }
                    }
                },
                new RandomEventTemplate
                {
                    EventId = "nutrient_deficiency",
                    Title = "Widespread Nutrient Deficiency",
                    Description = "Your plants are showing signs of magnesium deficiency. The nutrient solution needs immediate adjustment.",
                    Category = EventCategory.Cultivation,
                    Severity = EventSeverity.Medium,
                    Probability = 0.2f,
                    HasTimeLimit = true,
                    TimeLimit = TimeSpan.FromHours(12),
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            ChoiceText = "Purchase premium nutrient supplements ($300)",
                            CostCurrency = 300f,
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.PlantHealth, Value = 0.15f, Duration = 48f },
                                new EventConsequence { Type = ConsequenceType.Currency, Value = -300f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Research and mix custom solution (takes time)",
                            CostTime = 24f,
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.PlantHealth, Value = 0.1f, Duration = 72f },
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 200f },
                                new EventConsequence { Type = ConsequenceType.SkillPoints, Value = 1f }
                            }
                        }
                    }
                }
            };
            
            _availableEvents.AddRange(cultivationEvents);
        }
        
        private void CreateMarketEvents()
        {
            var marketEvents = new List<RandomEventTemplate>
            {
                new RandomEventTemplate
                {
                    EventId = "market_crash",
                    Title = "Cannabis Market Crash",
                    Description = "Oversupply has caused cannabis prices to plummet 40%. Dispensaries are refusing new contracts.",
                    Category = EventCategory.Market,
                    Severity = EventSeverity.Critical,
                    Probability = 0.08f,
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            ChoiceText = "Hold inventory and wait for recovery",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.MarketPrices, Value = -0.4f, Duration = 168f },
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 100f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Sell everything at reduced prices",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Currency, Value = -2000f },
                                new EventConsequence { Type = ConsequenceType.InventoryClear, Value = 1f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Pivot to hemp/CBD production",
                            CostCurrency = 1000f,
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Currency, Value = -1000f },
                                new EventConsequence { Type = ConsequenceType.UnlockFeature, Description = "Hemp Production" },
                                new EventConsequence { Type = ConsequenceType.Reputation, Value = 5f }
                            }
                        }
                    }
                },
                new RandomEventTemplate
                {
                    EventId = "premium_contract",
                    Title = "Exclusive Premium Contract Opportunity",
                    Description = "A high-end dispensary wants to offer you an exclusive contract for premium flower at 2x market rate, but they require consistent quality.",
                    Category = EventCategory.Market,
                    Severity = EventSeverity.Positive,
                    Probability = 0.12f,
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            ChoiceText = "Accept the contract (quality requirements)",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.MarketPrices, Value = 1.0f, Duration = 336f },
                                new EventConsequence { Type = ConsequenceType.QualityRequirement, Value = 0.9f, Duration = 336f },
                                new EventConsequence { Type = ConsequenceType.Reputation, Value = 15f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Negotiate for more flexible terms",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.MarketPrices, Value = 0.5f, Duration = 168f },
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 150f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Decline and maintain independence",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 50f }
                            }
                        }
                    }
                }
            };
            
            _availableEvents.AddRange(marketEvents);
        }
        
        private void CreateWeatherEvents()
        {
            var weatherEvents = new List<RandomEventTemplate>
            {
                new RandomEventTemplate
                {
                    EventId = "power_outage",
                    Title = "Extended Power Outage",
                    Description = "A severe storm has knocked out power to your facility. Your plants need light and climate control!",
                    Category = EventCategory.Weather,
                    Severity = EventSeverity.High,
                    Probability = 0.1f,
                    HasTimeLimit = true,
                    TimeLimit = TimeSpan.FromHours(8),
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            ChoiceText = "Rent emergency generators ($1200)",
                            CostCurrency = 1200f,
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Currency, Value = -1200f },
                                new EventConsequence { Type = ConsequenceType.PlantHealth, Value = 0f } // No health loss
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Move plants to backup greenhouse",
                            CostTime = 12f,
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.PlantStress, Value = 0.3f, Duration = 48f },
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 100f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Wait it out and hope for the best",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.PlantHealth, Value = -0.2f },
                                new EventConsequence { Type = ConsequenceType.PlantStress, Value = 0.5f, Duration = 72f }
                            }
                        }
                    }
                },
                new RandomEventTemplate
                {
                    EventId = "heatwave",
                    Title = "Extreme Heatwave Warning",
                    Description = "Temperatures are expected to reach record highs. Your HVAC system is struggling to maintain optimal conditions.",
                    Category = EventCategory.Weather,
                    Severity = EventSeverity.Medium,
                    Probability = 0.15f,
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            ChoiceText = "Install emergency cooling ($800)",
                            CostCurrency = 800f,
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Currency, Value = -800f },
                                new EventConsequence { Type = ConsequenceType.Temperature, Value = -5f, Duration = 72f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Increase ventilation and reduce lighting",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.GrowthRate, Value = -0.1f, Duration = 72f },
                                new EventConsequence { Type = ConsequenceType.Temperature, Value = -3f, Duration = 72f }
                            }
                        }
                    }
                }
            };
            
            _availableEvents.AddRange(weatherEvents);
        }
        
        private void CreateSocialEvents()
        {
            var socialEvents = new List<RandomEventTemplate>
            {
                new RandomEventTemplate
                {
                    EventId = "community_event",
                    Title = "Cannabis Education Event",
                    Description = "The local community is hosting a cannabis education event. Participating could boost your reputation but costs time.",
                    Category = EventCategory.Social,
                    Severity = EventSeverity.Positive,
                    Probability = 0.1f,
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            ChoiceText = "Participate as guest speaker",
                            CostTime = 24f,
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Reputation, Value = 10f },
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 200f },
                                new EventConsequence { Type = ConsequenceType.NetworkContacts, Value = 3f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Sponsor the event ($500)",
                            CostCurrency = 500f,
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Currency, Value = -500f },
                                new EventConsequence { Type = ConsequenceType.Reputation, Value = 5f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Focus on cultivation work",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 50f }
                            }
                        }
                    }
                },
                new RandomEventTemplate
                {
                    EventId = "media_attention",
                    Title = "Media Coverage Opportunity",
                    Description = "A cannabis magazine wants to feature your operation in an article about innovative growing techniques.",
                    Category = EventCategory.Social,
                    Severity = EventSeverity.Positive,
                    Probability = 0.08f,
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            ChoiceText = "Accept the feature story",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Reputation, Value = 20f },
                                new EventConsequence { Type = ConsequenceType.MarketDemand, Value = 0.3f, Duration = 168f },
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 300f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Decline to maintain privacy",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 25f }
                            }
                        }
                    }
                }
            };
            
            _availableEvents.AddRange(socialEvents);
        }
        
        private void CreateTechnologyEvents()
        {
            var technologyEvents = new List<RandomEventTemplate>
            {
                new RandomEventTemplate
                {
                    EventId = "tech_breakthrough",
                    Title = "New Growing Technology Available",
                    Description = "A revolutionary LED system with spectrum optimization has become available, promising 25% better growth rates.",
                    Category = EventCategory.Technology,
                    Severity = EventSeverity.Positive,
                    Probability = 0.06f,
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            ChoiceText = "Invest in the new technology ($3000)",
                            CostCurrency = 3000f,
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Currency, Value = -3000f },
                                new EventConsequence { Type = ConsequenceType.GrowthRate, Value = 0.25f, Duration = 720f },
                                new EventConsequence { Type = ConsequenceType.UnlockFeature, Description = "Advanced LED Systems" }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Wait for technology to mature",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 100f }
                            }
                        }
                    }
                }
            };
            
            _availableEvents.AddRange(technologyEvents);
        }
        
        private void CreateCrisisEvents()
        {
            var crisisEvents = new List<RandomEventTemplate>
            {
                new RandomEventTemplate
                {
                    EventId = "facility_fire",
                    Title = "Facility Fire Emergency!",
                    Description = "A small electrical fire has started in your grow room! Quick action is needed to save your crops and equipment.",
                    Category = EventCategory.Crisis,
                    Severity = EventSeverity.Critical,
                    Probability = 0.02f,
                    HasTimeLimit = true,
                    TimeLimit = TimeSpan.FromMinutes(30),
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            ChoiceText = "Evacuate plants immediately (save plants, lose equipment)",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.EquipmentLoss, Value = 5000f },
                                new EventConsequence { Type = ConsequenceType.PlantStress, Value = 0.8f, Duration = 168f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Fight fire with equipment (risk plant loss)",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.PlantLoss, Value = 5f },
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 300f }
                            }
                        }
                    }
                }
            };
            
            _availableEvents.AddRange(crisisEvents);
        }
        
        private void CreateOpportunityEvents()
        {
            var opportunityEvents = new List<RandomEventTemplate>
            {
                new RandomEventTemplate
                {
                    EventId = "investment_offer",
                    Title = "Angel Investor Interest",
                    Description = "A cannabis industry investor has noticed your operation and wants to offer $15,000 for 20% equity in your business.",
                    Category = EventCategory.Opportunity,
                    Severity = EventSeverity.Positive,
                    Probability = 0.04f,
                    Choices = new List<EventChoice>
                    {
                        new EventChoice
                        {
                            ChoiceText = "Accept the investment deal",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Currency, Value = 15000f },
                                new EventConsequence { Type = ConsequenceType.BusinessEquity, Value = -20f },
                                new EventConsequence { Type = ConsequenceType.NetworkContacts, Value = 5f },
                                new EventConsequence { Type = ConsequenceType.MarketAccess, Value = 1f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Negotiate for better terms",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Experience, Value = 200f },
                                new EventConsequence { Type = ConsequenceType.DelayedOpportunity, Value = 1f }
                            }
                        },
                        new EventChoice
                        {
                            ChoiceText = "Decline and maintain full ownership",
                            Consequences = new List<EventConsequence>
                            {
                                new EventConsequence { Type = ConsequenceType.Reputation, Value = 5f }
                            }
                        }
                    }
                }
            };
            
            _availableEvents.AddRange(opportunityEvents);
        }
        
        private void InitializeCategoryProbabilities()
        {
            _categoryProbabilities[EventCategory.Cultivation] = 0.35f;
            _categoryProbabilities[EventCategory.Market] = _marketEventChance;
            _categoryProbabilities[EventCategory.Weather] = _weatherEventChance;
            _categoryProbabilities[EventCategory.Social] = _socialEventChance;
            _categoryProbabilities[EventCategory.Technology] = 0.1f;
            _categoryProbabilities[EventCategory.Crisis] = 0.05f;
            _categoryProbabilities[EventCategory.Opportunity] = 0.1f;
        }
        
        private void InitializeStoryEvents()
        {
            if (!_enableStoryProgression) return;
            
            // Initialize story progression tracking
            _storyProgress["beginner_arc"] = 0;
            _storyProgress["business_growth"] = 0;
            _storyProgress["industry_recognition"] = 0;
            _storyProgress["master_cultivator"] = 0;
        }
        
        private void TriggerRandomEvent()
        {
            var availableEvents = GetAvailableEventsForCurrentState();
            if (availableEvents.Count == 0) return;
            
            var selectedEvent = SelectEventByProbability(availableEvents);
            if (selectedEvent != null)
            {
                CreateActiveEvent(selectedEvent);
            }
        }
        
        private List<RandomEventTemplate> GetAvailableEventsForCurrentState()
        {
            var playerLevel = GetPlayerLevel();
            var availableEvents = new List<RandomEventTemplate>();
            
            foreach (var eventTemplate in _availableEvents)
            {
                // Filter by difficulty level
                if (IsEventAppropriateForLevel(eventTemplate, playerLevel))
                {
                    // Check if event is already active
                    if (!_activeEvents.Any(ae => ae.EventId == eventTemplate.EventId))
                    {
                        availableEvents.Add(eventTemplate);
                    }
                }
            }
            
            return availableEvents;
        }
        
        private bool IsEventAppropriateForLevel(RandomEventTemplate eventTemplate, int playerLevel)
        {
            return eventTemplate.Category switch
            {
                EventCategory.Crisis => playerLevel >= 5,
                EventCategory.Opportunity => playerLevel >= 3,
                EventCategory.Technology => playerLevel >= 8,
                _ => true
            };
        }
        
        private RandomEventTemplate SelectEventByProbability(List<RandomEventTemplate> availableEvents)
        {
            // First select category based on probabilities
            var selectedCategory = SelectEventCategory();
            
            // Then select specific event from that category
            var categoryEvents = availableEvents.Where(e => e.Category == selectedCategory).ToList();
            if (categoryEvents.Count == 0)
            {
                // Fallback to any available event
                categoryEvents = availableEvents;
            }
            
            if (categoryEvents.Count == 0) return null;
            
            // Weight by event probability and current difficulty
            var weightedEvents = categoryEvents.Select(e => new
            {
                Event = e,
                Weight = e.Probability * GetDifficultyMultiplier(e.Severity)
            }).ToList();
            
            float totalWeight = weightedEvents.Sum(we => we.Weight);
            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            
            float currentWeight = 0f;
            foreach (var weightedEvent in weightedEvents)
            {
                currentWeight += weightedEvent.Weight;
                if (randomValue <= currentWeight)
                {
                    return weightedEvent.Event;
                }
            }
            
            return categoryEvents[UnityEngine.Random.Range(0, categoryEvents.Count)];
        }
        
        private EventCategory SelectEventCategory()
        {
            float randomValue = UnityEngine.Random.Range(0f, 1f);
            float currentProbability = 0f;
            
            foreach (var kvp in _categoryProbabilities)
            {
                currentProbability += kvp.Value;
                if (randomValue <= currentProbability)
                {
                    return kvp.Key;
                }
            }
            
            return EventCategory.Cultivation; // Fallback
        }
        
        private float GetDifficultyMultiplier(EventSeverity severity)
        {
            return _currentDifficultyLevel switch
            {
                EventDifficultyLevel.Easy => severity == EventSeverity.Critical ? 0.3f : 1f,
                EventDifficultyLevel.Medium => 1f,
                EventDifficultyLevel.Hard => severity == EventSeverity.Critical ? 1.5f : 1f,
                _ => 1f
            };
        }
        
        private void CreateActiveEvent(RandomEventTemplate template)
        {
            var activeEvent = new ActiveRandomEvent
            {
                EventId = template.EventId,
                Title = template.Title,
                Description = template.Description,
                Category = template.Category,
                Severity = template.Severity,
                HasTimeLimit = template.HasTimeLimit,
                TimeLimit = template.TimeLimit,
                StartTime = DateTime.Now,
                ExpirationTime = template.HasTimeLimit ? DateTime.Now.Add(template.TimeLimit) : DateTime.MaxValue,
                Choices = template.Choices,
                StoryContext = GetStoryContext(template),
                IsResolved = false
            };
            
            _activeEvents.Add(activeEvent);
            _totalEventsTriggered++;
            
            // Trigger events
            _onEventTriggered?.Raise();
            OnEventStarted?.Invoke(activeEvent);
            OnCategoryEventTriggered?.Invoke(activeEvent.Category);
            
            if (activeEvent.Severity == EventSeverity.Critical)
            {
                _onCrisisEvent?.Raise();
            }
            else if (activeEvent.Severity == EventSeverity.Positive)
            {
                _onOpportunityEvent?.Raise();
            }
            
            LogInfo($"🎭 Random Event Triggered: {activeEvent.Title} (Category: {activeEvent.Category}, Severity: {activeEvent.Severity})");
        }
        
        private void ResolveEvent(ActiveRandomEvent activeEvent, EventChoice choice)
        {
            _playerDecisionsMade++;
            
            // Apply consequences
            foreach (var consequence in choice.Consequences)
            {
                ApplyConsequence(consequence);
            }
            
            // Apply costs
            if (choice.CostCurrency > 0)
            {
                // Would integrate with economy system
                LogInfo($"💰 Event Cost: ${choice.CostCurrency}");
            }
            
            if (choice.CostTime > 0)
            {
                // Would integrate with time management
                LogInfo($"⏰ Event Time Cost: {choice.CostTime} hours");
            }
            
            activeEvent.IsResolved = true;
            activeEvent.SelectedChoice = choice;
            activeEvent.ResolutionTime = DateTime.Now;
            
            _activeEvents.Remove(activeEvent);
            
            // Update story progression
            if (_enableStoryProgression)
            {
                UpdateStoryProgression(activeEvent, choice);
            }
            
            // Trigger events
            _onEventResolved?.Raise();
            OnEventResolved?.Invoke(activeEvent, choice);
            
            LogInfo($"✅ Event Resolved: {activeEvent.Title} - Choice: {choice.ChoiceText}");
        }
        
        private void ApplyConsequence(EventConsequence consequence)
        {
            switch (consequence.Type)
            {
                case ConsequenceType.PlantHealth:
                    ApplyPlantHealthConsequence(consequence.Value);
                    break;
                case ConsequenceType.Currency:
                    ApplyCurrencyConsequence(consequence.Value);
                    break;
                case ConsequenceType.Experience:
                    ApplyExperienceConsequence(consequence.Value);
                    break;
                case ConsequenceType.Reputation:
                    ApplyReputationConsequence(consequence.Value);
                    break;
                case ConsequenceType.MarketPrices:
                    ApplyMarketPriceConsequence(consequence.Value, consequence.Duration);
                    break;
                case ConsequenceType.PlantLoss:
                    ApplyPlantLossConsequence((int)consequence.Value);
                    break;
                case ConsequenceType.UnlockFeature:
                    ApplyFeatureUnlock(consequence.Description);
                    break;
                case ConsequenceType.SkillPoints:
                    ApplySkillPointsConsequence(consequence.Value);
                    break;
                default:
                    LogInfo($"Applied consequence: {consequence.Type} = {consequence.Value}");
                    break;
            }
        }
        
        private void ApplyPlantHealthConsequence(float healthChange)
        {
            if (_plantManager != null)
            {
                // Would apply health change to all plants or specific plants
                LogInfo($"🌱 Plant Health Effect: {healthChange:+0.0%}");
            }
        }
        
        private void ApplyCurrencyConsequence(float currencyChange)
        {
            // Would integrate with economy system
            LogInfo($"💰 Currency Effect: ${currencyChange:+0}");
        }
        
        private void ApplyExperienceConsequence(float experienceGain)
        {
            // Note: ExperienceSource.Achievement would need to be resolved via reflection too
            // For now, pass null as a placeholder
            GainExperience(experienceGain, null);
            LogInfo($"⭐ Experience Gained: +{experienceGain}");
        }
        
        private void ApplyReputationConsequence(float reputationChange)
        {
            _playerReputationScore = Mathf.Clamp(_playerReputationScore + reputationChange, 0f, 100f);
            OnReputationChanged?.Invoke(_playerReputationScore);
            LogInfo($"🎭 Reputation Change: {reputationChange:+0} (Total: {_playerReputationScore:F1})");
        }
        
        private void ApplyMarketPriceConsequence(float priceMultiplier, float durationHours)
        {
            // Would integrate with market system
            LogInfo($"📈 Market Price Effect: {priceMultiplier:+0.0%} for {durationHours}h");
        }
        
        private void ApplyPlantLossConsequence(int plantsLost)
        {
            if (_plantManager != null)
            {
                // Would remove specified number of plants
                LogInfo($"💀 Plants Lost: {plantsLost}");
            }
        }
        
        private void ApplyFeatureUnlock(string feature)
        {
            LogInfo($"🔓 Feature Unlocked: {feature}");
        }
        
        private void ApplySkillPointsConsequence(float skillPoints)
        {
            // Would integrate with progression system
            LogInfo($"🎯 Skill Points Gained: +{skillPoints}");
        }
        
        private void UpdateActiveEvents()
        {
            var expiredEvents = _activeEvents.Where(e => e.HasTimeLimit && DateTime.Now > e.ExpirationTime).ToList();
            
            foreach (var expiredEvent in expiredEvents)
            {
                // Auto-resolve with default choice (usually the safest/neutral option)
                var defaultChoice = expiredEvent.Choices.LastOrDefault();
                if (defaultChoice != null)
                {
                    LogInfo($"⏰ Event Auto-Resolved (Time Expired): {expiredEvent.Title}");
                    ResolveEvent(expiredEvent, defaultChoice);
                }
                else
                {
                    _activeEvents.Remove(expiredEvent);
                }
            }
        }
        
        private void ProcessStoryEvents()
        {
            if (_storyEventQueue.Count > 0)
            {
                var storyEvent = _storyEventQueue.Dequeue();
                // Process story events
                _onStoryProgression?.Raise();
            }
        }
        
        private void UpdateStoryProgression(ActiveRandomEvent resolvedEvent, EventChoice choice)
        {
            // Update story arcs based on player choices and outcomes
            var playerLevel = GetPlayerLevel();
            
            if (playerLevel <= 5 && !_completedStoryArcs.Contains("beginner_arc"))
            {
                _storyProgress["beginner_arc"]++;
                if (_storyProgress["beginner_arc"] >= 3)
                {
                    _completedStoryArcs.Add("beginner_arc");
                    LogInfo("📖 Story Arc Completed: Beginner Cultivator");
                }
            }
        }
        
        private void UpdateDifficultyLevel()
        {
            var playerLevel = GetPlayerLevel();
            var newDifficulty = playerLevel switch
            {
                <= 5 => EventDifficultyLevel.Easy,
                <= 15 => EventDifficultyLevel.Medium,
                _ => EventDifficultyLevel.Hard
            };
            
            if (newDifficulty != _currentDifficultyLevel)
            {
                _currentDifficultyLevel = newDifficulty;
                LogInfo($"📊 Event Difficulty Updated: {_currentDifficultyLevel}");
            }
        }
        
        private void CalculateNextEventTime()
        {
            float variance = UnityEngine.Random.Range(-_eventIntervalVariance, _eventIntervalVariance);
            _timeUntilNextEvent = _baseEventInterval + variance;
            
            // Apply difficulty scaling
            float difficultyMultiplier = _currentDifficultyLevel switch
            {
                EventDifficultyLevel.Easy => 1.5f,
                EventDifficultyLevel.Medium => 1f,
                EventDifficultyLevel.Hard => 0.7f,
                _ => 1f
            };
            
            _timeUntilNextEvent *= difficultyMultiplier;
            _lastEventTime = Time.time;
            
            LogInfo($"⏱️ Next event in {_timeUntilNextEvent:F0} seconds");
        }
        
        private string GetStoryContext(RandomEventTemplate template)
        {
            // Generate contextual flavor text based on player progress
            var playerLevel = GetPlayerLevel();
            
            return template.Category switch
            {
                EventCategory.Cultivation when playerLevel <= 5 => "As a new cultivator, every challenge is a learning opportunity.",
                EventCategory.Market when _playerReputationScore >= 70 => "Your reputation in the industry opens new doors.",
                EventCategory.Crisis when playerLevel >= 10 => "Experience has taught you to stay calm under pressure.",
                _ => ""
            };
        }
        
        private string GetCategoryIcon(EventCategory category)
        {
            return category switch
            {
                EventCategory.Cultivation => "🌱",
                EventCategory.Market => "📈",
                EventCategory.Weather => "🌤️",
                EventCategory.Social => "👥",
                EventCategory.Technology => "⚡",
                EventCategory.Crisis => "🚨",
                EventCategory.Opportunity => "💎",
                _ => "📋"
            };
        }
        
        private Color GetSeverityColor(EventSeverity severity)
        {
            return severity switch
            {
                EventSeverity.Positive => new Color(0.2f, 0.8f, 0.2f),
                EventSeverity.Low => new Color(0.7f, 0.7f, 0.7f),
                EventSeverity.Medium => new Color(0.9f, 0.7f, 0.2f),
                EventSeverity.High => new Color(0.9f, 0.4f, 0.2f),
                EventSeverity.Critical => new Color(0.9f, 0.1f, 0.1f),
                _ => Color.white
            };
        }
        
        protected override void OnManagerShutdown()
        {
            LogInfo($"RandomEventManager shutdown - {_totalEventsTriggered} total events triggered");
        }
    }
}