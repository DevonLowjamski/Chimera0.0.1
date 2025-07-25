using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Core.Events;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Events; // For event data classes
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Events; // For PlayerChoiceEventData
// Type aliases to resolve ambiguities - comprehensive set for PlayerAgencyGamingSystem
using EventsPlayerChoiceEventData = ProjectChimera.Events.PlayerChoiceEventData;
using EventsChoiceConsequences = ProjectChimera.Events.ChoiceConsequences;
using EventsPendingConsequence = ProjectChimera.Events.PendingConsequence;
using EventsPlayerChoice = ProjectChimera.Events.PlayerChoice;
using EventsPlayerAgencyLevel = ProjectChimera.Events.PlayerAgencyLevel;
// using EventsConsequenceType = ProjectChimera.Events.ConsequenceType; // Using local ConsequenceType enum instead
using DataCultivationApproach = ProjectChimera.Data.Cultivation.CultivationApproach;
using DataFacilityDesignApproach = ProjectChimera.Data.Cultivation.FacilityDesignApproach;
using DataCultivationPathData = ProjectChimera.Data.Cultivation.CultivationPathData;
using DataFacilityDesignData = ProjectChimera.Data.Cultivation.FacilityDesignData;
using DataCultivationPathEffects = ProjectChimera.Data.Cultivation.CultivationPathEffects;
using DataFacilityDesignEffects = ProjectChimera.Data.Cultivation.FacilityDesignEffects;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Player Agency Gaming System - Multiple cultivation approaches and facility design choice systems
    /// Provides player creativity and expression within cultivation gaming framework
    /// Core component of Enhanced Cultivation Gaming System v2.0
    /// </summary>
    public class PlayerAgencyGamingSystem : MonoBehaviour
    {
        [Header("Player Agency Configuration")]
        [SerializeField] private PlayerAgencyGamingConfigSO _agencyConfig;
        [SerializeField] private CultivationPathLibrarySO _cultivationPaths;
        [SerializeField] private FacilityDesignLibrarySO _facilityDesignLibrary;
        
        [Header("Choice System Configuration")]
        [SerializeField] private PlayerChoiceManagerSO _choiceManager;
        [SerializeField] private ConsequenceCalculatorSO _consequenceCalculator;
        [SerializeField] private ExpressionSystemSO _expressionSystem;
        
        [Header("Agency Settings")]
        [Range(0.1f, 2.0f)] public float ChoiceImpactMultiplier = 1.0f;
        [Range(0.1f, 1.0f)] public float CreativityRewardBonus = 0.8f;
        [Range(1.0f, 5.0f)] public float ConsequenceDelayMultiplier = 1.5f;
        [Range(0.1f, 2.0f)] public float ExpressionValidationThreshold = 0.7f;
        
        // System State
        private bool _isInitialized = false;
        private PlayerAgencyState _currentAgencyState;
        private Dictionary<DataCultivationApproach, DataCultivationPathData> _activeCultivationPaths = new Dictionary<DataCultivationApproach, DataCultivationPathData>();
        private Dictionary<DataFacilityDesignApproach, DataFacilityDesignData> _activeFacilityDesigns = new Dictionary<DataFacilityDesignApproach, DataFacilityDesignData>();
        private List<PlayerChoice> _activeChoices = new List<PlayerChoice>();
        private List<EventsPendingConsequence> _pendingConsequences = new List<EventsPendingConsequence>();
        
        // Choice Management
        private CultivationPathManager _pathManager;
        private FacilityDesignGamingSystem _facilitySystem;
        private PlayerChoiceProcessor _choiceProcessor;
        private ConsequenceManager _consequenceManager;
        private CreativityExpressionEngine _expressionEngine;
        
        // Performance Tracking
        private int _totalChoicesMade = 0;
        private int _creativeSolutionsImplemented = 0;
        private int _consequencesProcessed = 0;
        private float _totalExpressionValue = 0f;
        private Dictionary<DataCultivationApproach, float> _approachSuccessRates = new Dictionary<DataCultivationApproach, float>();
        
        // Events
        private GameEventChannelSO _onPlayerChoiceMade;
        private GameEventChannelSO _onCultivationPathSelected;
        private GameEventChannelSO _onFacilityDesignChosen;
        private GameEventChannelSO _onCreativeSolutionImplemented;
        private GameEventChannelSO _onChoiceConsequenceRealized;
        private GameEventChannelSO _onExpressionValueIncreased;
        
        #region Initialization
        
        public void Initialize(PlayerAgencyGamingConfigSO config)
        {
            if (_isInitialized)
            {
                ChimeraLogger.LogWarning("PlayerAgencyGamingSystem already initialized", this);
                return;
            }
            
            _agencyConfig = config ?? _agencyConfig;
            
            if (_agencyConfig == null)
            {
                ChimeraLogger.LogError("PlayerAgencyGamingConfigSO is required for initialization", this);
                return;
            }
            
            InitializeAgencySystems();
            InitializeChoiceSystems();
            InitializeCultivationPaths();
            InitializeFacilityDesigns();
            InitializeExpressionEngine();
            SetupEventChannels();
            
            _isInitialized = true;
            ChimeraLogger.Log("PlayerAgencyGamingSystem initialized successfully", this);
        }
        
        private void InitializeAgencySystems()
        {
            // Initialize agency state
            _currentAgencyState = new PlayerAgencyState
            {
                CurrentCultivationApproach = DataCultivationApproach.OrganicTraditional,
                CurrentFacilityDesign = DataFacilityDesignApproach.MinimalistEfficient,
                AgencyLevel = PlayerAgencyLevel.Guided,
                ExpressionScore = 0f,
                CreativityRating = CreativityLevel.Beginner
            };
            
            // Clear collections
            _activeChoices.Clear();
            _pendingConsequences.Clear();
            _activeCultivationPaths.Clear();
            _activeFacilityDesigns.Clear();
        }
        
        private void InitializeChoiceSystems()
        {
            // Initialize choice processor if not already assigned
            if (_choiceProcessor == null)
            {
                var processorGO = new GameObject("PlayerChoiceProcessor");
                processorGO.transform.SetParent(transform);
                _choiceProcessor = processorGO.AddComponent<PlayerChoiceProcessor>();
            }
            
            // Initialize consequence manager if not already assigned
            if (_consequenceManager == null)
            {
                var consequenceGO = new GameObject("ConsequenceManager");
                consequenceGO.transform.SetParent(transform);
                _consequenceManager = consequenceGO.AddComponent<ConsequenceManager>();
            }
        }
        
        private void InitializeCultivationPaths()
        {
            if (_cultivationPaths == null) return;
            
            // Initialize all available cultivation approaches
            foreach (DataCultivationApproach approach in System.Enum.GetValues(typeof(DataCultivationApproach)))
            {
                var pathData = _cultivationPaths.GetPathData(approach);
                if (pathData != null)
                {
                    _activeCultivationPaths[approach] = pathData;
                }
            }
            
            ChimeraLogger.Log($"Initialized {_activeCultivationPaths.Count} cultivation paths", this);
        }
        
        private void InitializeFacilityDesigns()
        {
            if (_facilityDesignLibrary == null) return;
            
            // Initialize all available facility design approaches
            foreach (DataFacilityDesignApproach approach in System.Enum.GetValues(typeof(DataFacilityDesignApproach)))
            {
                var designData = _facilityDesignLibrary.GetDesignData(approach);
                if (designData != null)
                {
                    _activeFacilityDesigns[approach] = designData;
                }
            }
            
            ChimeraLogger.Log($"Initialized {_activeFacilityDesigns.Count} facility design approaches", this);
        }
        
        private void InitializeExpressionEngine()
        {
            // Initialize creativity expression engine if not already assigned
            if (_expressionEngine == null)
            {
                var expressionGO = new GameObject("CreativityExpressionEngine");
                expressionGO.transform.SetParent(transform);
                _expressionEngine = expressionGO.AddComponent<CreativityExpressionEngine>();
            }
            
            if (_expressionSystem != null)
            {
                _expressionEngine?.Initialize(_expressionSystem);
            }
        }
        
        private void SetupEventChannels()
        {
            // Event channels would be assigned in inspector or loaded from configuration
            if (_onPlayerChoiceMade != null)
                _onPlayerChoiceMade.OnEventRaisedWithData.AddListener(OnPlayerChoiceMadeEvent);
                
            if (_onCultivationPathSelected != null)
                _onCultivationPathSelected.OnEventRaisedWithData.AddListener(OnCultivationPathSelectedEvent);
                
            if (_onFacilityDesignChosen != null)
                _onFacilityDesignChosen.OnEventRaisedWithData.AddListener(OnFacilityDesignChosenEvent);
        }
        
        #endregion
        
        #region Public API - Player Choice Processing
        
        /// <summary>
        /// Process a player choice and return immediate consequences
        /// </summary>
        public EventsChoiceConsequences ProcessPlayerChoice(PlayerChoice choice)
        {
            if (!_isInitialized || choice == null)
                return EventsChoiceConsequences.None;
            
            _totalChoicesMade++;
            _activeChoices.Add(choice);
            
            var localConsequences = _choiceProcessor?.ProcessChoice(choice) ?? ChoiceConsequences.None;
            var consequences = ConvertToEventsChoiceConsequences(localConsequences);
            var delayedConsequences = CalculateDelayedConsequences(choice);
            
            // Add delayed consequences to pending list
            if (delayedConsequences.Count > 0)
            {
                _pendingConsequences.AddRange(delayedConsequences);
            }
            
            // Update agency state based on choice
            UpdateAgencyStateFromChoice(choice);
            
            // Trigger events
            _onPlayerChoiceMade?.RaiseEvent(new EventsPlayerChoiceEventData
            {
                ChoiceType = (ProjectChimera.Events.PlayerChoiceType)choice.ChoiceType,
                ChoiceDescription = choice.ChoiceDescription,
                ImpactLevel = choice.ImpactLevel,
                ChoiceParameters = choice.ChoiceParameters,
                ChoiceTimestamp = choice.ChoiceTimestamp,
                Consequences = consequences,
                Timestamp = Time.time
            });
            
            ChimeraLogger.Log($"Player choice processed: {choice.ChoiceType} with {consequences} consequences", this);
            return consequences;
        }
        
        /// <summary>
        /// Select cultivation approach and update player path
        /// </summary>
        public bool SelectCultivationApproach(DataCultivationApproach approach)
        {
            if (!_activeCultivationPaths.ContainsKey(approach))
            {
                ChimeraLogger.LogWarning($"Cultivation approach {approach} not available", this);
                return false;
            }
            
            var previousApproach = _currentAgencyState.CurrentCultivationApproach;
            _currentAgencyState.CurrentCultivationApproach = approach;
            
            var pathData = _activeCultivationPaths[approach];
            ApplyCultivationPathEffects(pathData);
            
            // Calculate creativity expression value
            var expressionValue = CalculateApproachExpressionValue(approach, previousApproach);
            _currentAgencyState.ExpressionScore += expressionValue;
            _totalExpressionValue += expressionValue;
            
            _onCultivationPathSelected?.RaiseEvent(new CultivationPathGamingEventData
            {
                NewApproach = approach,
                PreviousApproach = previousApproach,
                PathData = pathData,
                ExpressionValue = expressionValue
            });
            
            ChimeraLogger.Log($"Cultivation approach selected: {approach} (Expression: +{expressionValue:F2})", this);
            return true;
        }
        
        /// <summary>
        /// Choose facility design approach and update design path
        /// </summary>
        public bool ChooseFacilityDesignApproach(DataFacilityDesignApproach approach)
        {
            if (!_activeFacilityDesigns.ContainsKey(approach))
            {
                ChimeraLogger.LogWarning($"Facility design approach {approach} not available", this);
                return false;
            }
            
            var previousDesign = _currentAgencyState.CurrentFacilityDesign;
            _currentAgencyState.CurrentFacilityDesign = approach;
            
            var designData = _activeFacilityDesigns[approach];
            ApplyFacilityDesignEffects(designData);
            
            // Calculate creativity expression value
            var expressionValue = CalculateDesignExpressionValue(approach, previousDesign);
            _currentAgencyState.ExpressionScore += expressionValue;
            _totalExpressionValue += expressionValue;
            
            _onFacilityDesignChosen?.RaiseEvent(new FacilityDesignGamingEventData
            {
                NewApproach = approach,
                PreviousApproach = previousDesign,
                DesignData = designData,
                ExpressionValue = expressionValue
            });
            
            ChimeraLogger.Log($"Facility design chosen: {approach} (Expression: +{expressionValue:F2})", this);
            return true;
        }
        
        /// <summary>
        /// Implement creative solution that provides unique benefits
        /// </summary>
        public CreativeSolutionResult ImplementCreativeSolution(CreativeSolution solution)
        {
            if (solution == null || !ValidateCreativeSolution(solution))
                return CreativeSolutionResult.Invalid;
            
            var result = _expressionEngine?.ProcessCreativeSolution(solution) ?? CreativeSolutionResult.Failed;
            
            if (result == CreativeSolutionResult.Successful)
            {
                _creativeSolutionsImplemented++;
                
                // Apply creativity rewards
                var creativityBonus = CalculateCreativityBonus(solution);
                _currentAgencyState.ExpressionScore += creativityBonus;
                _totalExpressionValue += creativityBonus;
                
                // Update creativity level
                UpdateCreativityLevel();
                
                _onCreativeSolutionImplemented?.RaiseEvent(new CreativeSolutionEventData
                {
                    Solution = solution,
                    Result = result,
                    CreativityBonus = creativityBonus,
                    NewCreativityLevel = _currentAgencyState.CreativityRating
                });
                
                ChimeraLogger.Log($"Creative solution implemented: {solution.SolutionType} (+{creativityBonus:F2} expression)", this);
            }
            
            return result;
        }
        
        /// <summary>
        /// Apply choice consequences to player and cultivation system
        /// </summary>
        public void ApplyChoiceConsequences(EventsPlayerChoiceEventData choiceData)
        {
            if (choiceData == null) return;
            
            // Convert event data to local PlayerChoice for processing
            var localChoice = new PlayerChoice
            {
                ChoiceType = (PlayerChoiceType)choiceData.ChoiceType,
                ChoiceDescription = choiceData.ChoiceDescription,
                ImpactLevel = choiceData.ImpactLevel,
                ChoiceParameters = choiceData.ChoiceParameters,
                ChoiceTimestamp = choiceData.ChoiceTimestamp
            };
            
            // Apply immediate consequences
            ProcessImmediateConsequences(choiceData.Consequences);
            
            // Schedule delayed consequences (if available in event data)
            // Note: DelayedConsequences property may need to be added to EventsPlayerChoiceEventData
            
            // Update success rates
            UpdateApproachSuccessRates(localChoice);
        }
        
        #endregion
        
        #region Choice Processing and Consequences
        
        private List<EventsPendingConsequence> CalculateDelayedConsequences(PlayerChoice choice)
        {
            var consequences = new List<EventsPendingConsequence>();
            
            if (_consequenceCalculator == null) return consequences;
            
            // Calculate delayed consequences based on choice type and impact
            var delayTime = choice.ImpactLevel * ConsequenceDelayMultiplier;
            
            switch (choice.ChoiceType)
            {
                case PlayerChoiceType.CultivationMethod:
                    consequences.Add(new EventsPendingConsequence
                    {
                        Type = ConvertToEventsConsequenceType(ConsequenceType.YieldChange),
                        ImpactValue = choice.ImpactLevel,
                        DelayTime = delayTime,
                        TriggerTime = Time.time + delayTime
                    });
                    break;
                    
                case PlayerChoiceType.FacilityDesign:
                    consequences.Add(new EventsPendingConsequence
                    {
                        Type = ConvertToEventsConsequenceType(ConsequenceType.EfficiencyChange),
                        ImpactValue = choice.ImpactLevel * 0.8f,
                        DelayTime = delayTime * 1.5f,
                        TriggerTime = Time.time + (delayTime * 1.5f)
                    });
                    break;
                    
                case PlayerChoiceType.ResourceAllocation:
                    consequences.Add(new EventsPendingConsequence
                    {
                        Type = ConvertToEventsConsequenceType(ConsequenceType.CostChange),
                        ImpactValue = choice.ImpactLevel,
                        DelayTime = delayTime * 0.5f,
                        TriggerTime = Time.time + (delayTime * 0.5f)
                    });
                    break;
            }
            
            return consequences;
        }
        
        private void ProcessImmediateConsequences(EventsChoiceConsequences consequences)
        {
            if (consequences == EventsChoiceConsequences.None) return;
            
            // Process immediate consequences based on type
            if (consequences.HasFlag(EventsChoiceConsequences.YieldIncrease))
            {
                // Apply yield increase modifier
                ApplyYieldModifier(1.1f);
            }
            
            if (consequences.HasFlag(EventsChoiceConsequences.EfficiencyGain))
            {
                // Apply efficiency gain modifier
                ApplyEfficiencyModifier(1.15f);
            }
            
            if (consequences.HasFlag(EventsChoiceConsequences.CostReduction))
            {
                // Apply cost reduction modifier
                ApplyCostModifier(0.9f);
            }
            
            if (consequences.HasFlag(EventsChoiceConsequences.QualityImprovement))
            {
                // Apply quality improvement modifier
                ApplyQualityModifier(1.2f);
            }
        }
        
        private void ScheduleDelayedConsequence(EventsPendingConsequence consequence)
        {
            if (!_pendingConsequences.Contains(consequence))
            {
                _pendingConsequences.Add(consequence);
            }
        }
        
        private void ProcessPendingConsequences()
        {
            var currentTime = Time.time;
            var consequencesToProcess = _pendingConsequences.Where(c => c.TriggerTime <= currentTime).ToList();
            
            foreach (var consequence in consequencesToProcess)
            {
                ProcessConsequence(consequence);
                _pendingConsequences.Remove(consequence);
                _consequencesProcessed++;
                
                _onChoiceConsequenceRealized?.RaiseEvent(new ConsequenceEventData
                {
                    Consequence = ConvertToLocalPendingConsequence(consequence),
                    ProcessTime = currentTime
                });
            }
        }
        
        private void ProcessConsequence(EventsPendingConsequence consequence)
        {
            switch (consequence.Type)
            {
                case ProjectChimera.Events.ConsequenceType.Immediate:
                    ApplyYieldModifier(1.0f + (consequence.ImpactValue * 0.1f));
                    break;
                    
                case ProjectChimera.Events.ConsequenceType.Delayed:
                    ApplyEfficiencyModifier(1.0f + (consequence.ImpactValue * 0.08f));
                    break;
                    
                case ProjectChimera.Events.ConsequenceType.Educational:
                    ApplyCostModifier(1.0f - (consequence.ImpactValue * 0.05f));
                    break;
                    
                default:
                    ApplyQualityModifier(1.0f + (consequence.ImpactValue * 0.12f));
                    break;
            }
        }
        
        #endregion
        
        #region Expression and Creativity Management
        
        private float CalculateApproachExpressionValue(DataCultivationApproach newApproach, DataCultivationApproach previousApproach)
        {
            // Base expression value for changing approaches
            float baseValue = 5.0f;
            
            // Bonus for innovative approaches
            float innovationBonus = newApproach switch
            {
                DataCultivationApproach.ExperimentalInnovative => 15.0f,
                DataCultivationApproach.AeroponicCutting => 12.0f,
                DataCultivationApproach.BiodynamicHolistic => 10.0f,
                DataCultivationApproach.TechnologicalAutomated => 8.0f,
                DataCultivationApproach.HydroponicPrecision => 6.0f,
                DataCultivationApproach.EconomicOptimized => 4.0f,
                DataCultivationApproach.OrganicTraditional => 2.0f,
                _ => 0.0f
            };
            
            // Variety bonus for trying different approaches
            float varietyBonus = newApproach != previousApproach ? 3.0f : 0.0f;
            
            return (baseValue + innovationBonus + varietyBonus) * CreativityRewardBonus;
        }
        
        private float CalculateDesignExpressionValue(DataFacilityDesignApproach newDesign, DataFacilityDesignApproach previousDesign)
        {
            // Base expression value for changing designs
            float baseValue = 4.0f;
            
            // Bonus for creative designs
            float creativityBonus = newDesign switch
            {
                DataFacilityDesignApproach.CreativeInnovative => 12.0f,
                DataFacilityDesignApproach.AestheticShowcase => 10.0f,
                DataFacilityDesignApproach.TechnologicalCutting => 8.0f,
                DataFacilityDesignApproach.SustainableEcological => 7.0f,
                DataFacilityDesignApproach.ModularExpandable => 5.0f,
                DataFacilityDesignApproach.BudgetOptimized => 3.0f,
                DataFacilityDesignApproach.MinimalistEfficient => 2.0f,
                _ => 0.0f
            };
            
            // Variety bonus for trying different designs
            float varietyBonus = newDesign != previousDesign ? 2.0f : 0.0f;
            
            return (baseValue + creativityBonus + varietyBonus) * CreativityRewardBonus;
        }
        
        private float CalculateCreativityBonus(CreativeSolution solution)
        {
            float baseBonus = 10.0f;
            float complexityMultiplier = solution.ComplexityLevel * 0.5f;
            float innovationMultiplier = solution.InnovationLevel * 0.3f;
            
            return (baseBonus + complexityMultiplier + innovationMultiplier) * CreativityRewardBonus;
        }
        
        private bool ValidateCreativeSolution(CreativeSolution solution)
        {
            if (solution == null) return false;
            
            // Validate solution meets minimum requirements
            bool hasValidType = solution.SolutionType != CreativeSolutionType.None;
            bool hasValidComplexity = solution.ComplexityLevel >= ExpressionValidationThreshold;
            bool hasValidImplementation = !string.IsNullOrEmpty(solution.Implementation);
            
            return hasValidType && hasValidComplexity && hasValidImplementation;
        }
        
        private void UpdateCreativityLevel()
        {
            var newLevel = _currentAgencyState.ExpressionScore switch
            {
                >= 500f => CreativityLevel.Master,
                >= 300f => CreativityLevel.Expert,
                >= 150f => CreativityLevel.Advanced,
                >= 75f => CreativityLevel.Intermediate,
                >= 25f => CreativityLevel.Novice,
                _ => CreativityLevel.Beginner
            };
            
            if (newLevel != _currentAgencyState.CreativityRating)
            {
                var previousLevel = _currentAgencyState.CreativityRating;
                _currentAgencyState.CreativityRating = newLevel;
                
                _onExpressionValueIncreased?.RaiseEvent(new ExpressionLevelEventData
                {
                    PreviousLevel = previousLevel,
                    NewLevel = newLevel,
                    ExpressionScore = _currentAgencyState.ExpressionScore
                });
                
                ChimeraLogger.Log($"Creativity level increased: {previousLevel} → {newLevel}", this);
            }
        }
        
        #endregion
        
        #region System Effects Application
        
        private void ApplyCultivationPathEffects(DataCultivationPathData pathData)
        {
            if (pathData == null) return;
            
            // Apply path-specific modifiers to cultivation system
            // Apply cultivation path effects (implementation depends on CultivationPathManager)
            if (_pathManager != null)
            {
                // Note: ApplyPathEffects method may need to be implemented in CultivationPathManager
                ChimeraLogger.Log($"Cultivation path effects applied for path data", this);
            }
        }
        
        private void ApplyFacilityDesignEffects(DataFacilityDesignData designData)
        {
            if (designData == null) return;
            
            // Apply design-specific modifiers to facility system
            _facilitySystem?.ApplyDesignEffects(designData);
        }
        
        private void ApplyYieldModifier(float modifier)
        {
            // Apply yield modifier to cultivation system
            // This would integrate with existing plant management systems
        }
        
        private void ApplyEfficiencyModifier(float modifier)
        {
            // Apply efficiency modifier to facility systems
            // This would integrate with existing facility management systems
        }
        
        private void ApplyCostModifier(float modifier)
        {
            // Apply cost modifier to economic systems
            // This would integrate with existing economic management systems
        }
        
        private void ApplyQualityModifier(float modifier)
        {
            // Apply quality modifier to cultivation and harvest systems
            // This would integrate with existing quality management systems
        }
        
        #endregion
        
        #region State Management
        
        private void UpdateAgencyStateFromChoice(PlayerChoice choice)
        {
            // Increase agency level based on choice complexity
            if (choice.ImpactLevel > 0.7f)
            {
                var newLevel = _currentAgencyState.AgencyLevel switch
                {
                    PlayerAgencyLevel.Guided => PlayerAgencyLevel.Assisted,
                    PlayerAgencyLevel.Assisted => PlayerAgencyLevel.Independent,
                    PlayerAgencyLevel.Independent => PlayerAgencyLevel.Advanced,
                    PlayerAgencyLevel.Advanced => PlayerAgencyLevel.Expert,
                    _ => _currentAgencyState.AgencyLevel
                };
                
                if (newLevel != _currentAgencyState.AgencyLevel)
                {
                    _currentAgencyState.AgencyLevel = newLevel;
                    ChimeraLogger.Log($"Player agency level increased to {newLevel}", this);
                }
            }
        }
        
        private void UpdateApproachSuccessRates(PlayerChoice choice)
        {
            var approach = _currentAgencyState.CurrentCultivationApproach;
            var successRate = CalculateChoiceSuccessRate(choice);
            
            if (_approachSuccessRates.ContainsKey(approach))
            {
                _approachSuccessRates[approach] = (_approachSuccessRates[approach] + successRate) * 0.5f;
            }
            else
            {
                _approachSuccessRates[approach] = successRate;
            }
        }
        
        private float CalculateChoiceSuccessRate(PlayerChoice choice)
        {
            // Calculate success rate based on choice outcome
            return choice.ImpactLevel * ChoiceImpactMultiplier;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnPlayerChoiceMadeEvent(object eventData)
        {
            if (eventData is EventsPlayerChoiceEventData choiceData)
            {
                ApplyChoiceConsequences(choiceData);
            }
        }
        
        private void OnCultivationPathSelectedEvent(object eventData)
        {
            if (eventData is CultivationPathGamingEventData pathData)
            {
                // Handle cultivation path selection consequences
                ProcessCultivationPathConsequences(pathData);
            }
        }
        
        private void OnFacilityDesignChosenEvent(object eventData)
        {
            if (eventData is FacilityDesignGamingEventData designData)
            {
                // Handle facility design choice consequences
                ProcessFacilityDesignConsequences(designData);
            }
        }
        
        private void ProcessCultivationPathConsequences(CultivationPathGamingEventData pathData)
        {
            // Process long-term consequences of cultivation path changes
            var pathEffects = _cultivationPaths?.GetPathEffects(pathData.NewApproach);
            if (pathEffects != null)
            {
                // Apply path effects over time
                SchedulePathEffectApplication(pathEffects);
            }
        }
        
        private void ProcessFacilityDesignConsequences(FacilityDesignGamingEventData designData)
        {
            // Process long-term consequences of facility design changes
            var designEffects = _facilityDesignLibrary?.GetDesignEffects(designData.NewApproach);
            if (designEffects != null)
            {
                // Apply design effects over time
                ScheduleDesignEffectApplication(designEffects);
            }
        }
        
        private void SchedulePathEffectApplication(DataCultivationPathEffects effects)
        {
            // Schedule cultivation path effects to be applied over time
            // This would integrate with the existing cultivation system
        }
        
        private void ScheduleDesignEffectApplication(DataFacilityDesignEffects effects)
        {
            // Schedule facility design effects to be applied over time
            // This would integrate with the existing facility system
        }
        
        #endregion
        
        #region Public Properties and Getters
        
        /// <summary>
        /// Get current player agency state
        /// </summary>
        public PlayerAgencyState GetCurrentAgencyState()
        {
            return _currentAgencyState;
        }
        
        /// <summary>
        /// Get available cultivation approaches
        /// </summary>
        public IReadOnlyList<DataCultivationApproach> GetAvailableCultivationApproaches()
        {
            return _activeCultivationPaths.Keys.ToList();
        }
        
        /// <summary>
        /// Get available facility design approaches
        /// </summary>
        public IReadOnlyList<DataFacilityDesignApproach> GetAvailableFacilityDesigns()
        {
            return _activeFacilityDesigns.Keys.ToList();
        }
        
        /// <summary>
        /// Get success rate for specific cultivation approach
        /// </summary>
        public float GetApproachSuccessRate(DataCultivationApproach approach)
        {
            return _approachSuccessRates.TryGetValue(approach, out float rate) ? rate : 0f;
        }
        
        /// <summary>
        /// Get agency system performance metrics
        /// </summary>
        public PlayerAgencyMetrics GetAgencyMetrics()
        {
            return new PlayerAgencyMetrics
            {
                TotalChoicesMade = _totalChoicesMade,
                CreativeSolutionsImplemented = _creativeSolutionsImplemented,
                ConsequencesProcessed = _consequencesProcessed,
                TotalExpressionValue = _totalExpressionValue,
                CurrentAgencyLevel = _currentAgencyState.AgencyLevel,
                CurrentCreativityLevel = _currentAgencyState.CreativityRating,
                AverageSuccessRate = _approachSuccessRates.Values.DefaultIfEmpty(0).Average()
            };
        }
        
        #endregion
        
        #region System Updates
        
        public void UpdateSystem(float deltaTime)
        {
            if (!_isInitialized) return;
            
            // Process pending consequences
            ProcessPendingConsequences();
            
            // Update expression engine
            _expressionEngine?.UpdateSystem(deltaTime);
            
            // Update choice processor
            _choiceProcessor?.UpdateSystem(deltaTime);
            
            // Update consequence manager
            _consequenceManager?.UpdateSystem(deltaTime);
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void OnDestroy()
        {
            if (_onPlayerChoiceMade != null)
                _onPlayerChoiceMade.OnEventRaisedWithData.RemoveListener(OnPlayerChoiceMadeEvent);
                
            if (_onCultivationPathSelected != null)
                _onCultivationPathSelected.OnEventRaisedWithData.RemoveListener(OnCultivationPathSelectedEvent);
                
            if (_onFacilityDesignChosen != null)
                _onFacilityDesignChosen.OnEventRaisedWithData.RemoveListener(OnFacilityDesignChosenEvent);
        }
        
        /// <summary>
        /// Convert local ChoiceConsequences (flags) to Events namespace ChoiceConsequences (flags)
        /// </summary>
        private EventsChoiceConsequences ConvertToEventsChoiceConsequences(ChoiceConsequences localConsequences)
        {
            if (localConsequences == ChoiceConsequences.None)
                return EventsChoiceConsequences.None;
                
            EventsChoiceConsequences result = EventsChoiceConsequences.None;
            
            // Map specific flags from local to events namespace
            if ((localConsequences & ChoiceConsequences.EfficiencyGain) != 0)
                result |= EventsChoiceConsequences.EfficiencyGain;
            if ((localConsequences & ChoiceConsequences.EfficiencyLoss) != 0)
                result |= EventsChoiceConsequences.EfficiencyLoss;
            if ((localConsequences & ChoiceConsequences.QualityImprovement) != 0)
                result |= EventsChoiceConsequences.QualityImprovement;
            if ((localConsequences & ChoiceConsequences.QualityDegradation) != 0)
                result |= EventsChoiceConsequences.QualityDegradation;
            if ((localConsequences & ChoiceConsequences.CostSavings) != 0)
                result |= EventsChoiceConsequences.CostReduction;
            if ((localConsequences & ChoiceConsequences.CostIncrease) != 0)
                result |= EventsChoiceConsequences.CostIncrease;
            if ((localConsequences & ChoiceConsequences.AutomationUnlock) != 0)
                result |= EventsChoiceConsequences.UnlockNewOption;
            if ((localConsequences & ChoiceConsequences.AutomationLoss) != 0)
                result |= EventsChoiceConsequences.LockExistingOption;
                
            return result;
        }
        
        /// <summary>
        /// Convert local ConsequenceType to Events namespace ConsequenceType
        /// </summary>
        private ProjectChimera.Events.ConsequenceType ConvertToEventsConsequenceType(ConsequenceType localType)
        {
            return localType switch
            {
                ConsequenceType.YieldChange => ProjectChimera.Events.ConsequenceType.Immediate,
                ConsequenceType.EfficiencyChange => ProjectChimera.Events.ConsequenceType.Delayed,
                ConsequenceType.CostChange => ProjectChimera.Events.ConsequenceType.Educational,
                ConsequenceType.QualityChange => ProjectChimera.Events.ConsequenceType.Immediate,
                ConsequenceType.UnlockChange => ProjectChimera.Events.ConsequenceType.Delayed,
                ConsequenceType.RelationshipChange => ProjectChimera.Events.ConsequenceType.Educational,
                _ => ProjectChimera.Events.ConsequenceType.Immediate
            };
        }
        
        /// <summary>
        /// Convert Events namespace PendingConsequence to local PendingConsequence for processing
        /// </summary>
        private PendingConsequence ConvertToLocalPendingConsequence(EventsPendingConsequence eventsConsequence)
        {
            return new PendingConsequence
            {
                Type = ConvertFromEventsConsequenceType(eventsConsequence.Type),
                Impact = eventsConsequence.ImpactValue,
                DelayTime = eventsConsequence.DelayTime,
                TriggerTime = eventsConsequence.TriggerTime
            };
        }
        
        /// <summary>
        /// Convert Events namespace ConsequenceType to local ConsequenceType
        /// </summary>
        private ConsequenceType ConvertFromEventsConsequenceType(ProjectChimera.Events.ConsequenceType eventsType)
        {
            return eventsType switch
            {
                ProjectChimera.Events.ConsequenceType.Immediate => ConsequenceType.YieldChange,
                ProjectChimera.Events.ConsequenceType.Delayed => ConsequenceType.EfficiencyChange,
                ProjectChimera.Events.ConsequenceType.Educational => ConsequenceType.CostChange,
                _ => ConsequenceType.YieldChange
            };
        }
        
        #endregion
    }
    
    #region Data Structures
    
    [System.Serializable]
    public class PlayerAgencyState
    {
        public DataCultivationApproach CurrentCultivationApproach;
        public DataFacilityDesignApproach CurrentFacilityDesign;
        public PlayerAgencyLevel AgencyLevel;
        public float ExpressionScore;
        public CreativityLevel CreativityRating;
    }
    
    [System.Serializable]
    public class PlayerChoice
    {
        public PlayerChoiceType ChoiceType;
        public string ChoiceDescription;
        public float ImpactLevel;
        public Dictionary<string, object> ChoiceParameters;
        public float ChoiceTimestamp;
    }
    
    // PlayerChoiceEventData is now imported from ProjectChimera.Events namespace
    
    [System.Serializable]
    public class PendingConsequence
    {
        public ConsequenceType Type;
        public float Impact;
        public float DelayTime;
        public float TriggerTime;
    }
    
    [System.Serializable]
    public class CreativeSolution
    {
        public CreativeSolutionType SolutionType;
        public string Implementation;
        public float ComplexityLevel;
        public float InnovationLevel;
        public Dictionary<string, object> SolutionParameters;
    }
    
    [System.Serializable]
    public class PlayerAgencyMetrics
    {
        public int TotalChoicesMade;
        public int CreativeSolutionsImplemented;
        public int ConsequencesProcessed;
        public float TotalExpressionValue;
        public PlayerAgencyLevel CurrentAgencyLevel;
        public CreativityLevel CurrentCreativityLevel;
        public float AverageSuccessRate;
    }
    
    // Event Data Structures for Player Agency Gaming System
    [System.Serializable]
    public class CultivationPathGamingEventData
    {
        public DataCultivationApproach NewApproach;
        public DataCultivationApproach PreviousApproach;
        public DataCultivationPathData PathData;
        public float ExpressionValue;
        public System.DateTime Timestamp;
        public string PlayerId;
    }
    
    [System.Serializable]
    public class FacilityDesignGamingEventData
    {
        public DataFacilityDesignApproach NewApproach;
        public DataFacilityDesignApproach PreviousApproach;
        public DataFacilityDesignData DesignData;
        public float ExpressionValue;
        public System.DateTime Timestamp;
        public string PlayerId;
    }
    
    [System.Serializable]
    public class CreativeSolutionEventData
    {
        public CreativeSolution Solution;
        public CreativeSolutionResult Result;
        public float CreativityBonus;
        public CreativityLevel NewCreativityLevel;
        public System.DateTime Timestamp;
        public string PlayerId;
    }
    
    [System.Serializable]
    public class ConsequenceEventData
    {
        public PendingConsequence Consequence;
        public float ProcessTime;
        public System.DateTime Timestamp;
        public string PlayerId;
    }
    
    [System.Serializable]
    public class ExpressionLevelEventData
    {
        public CreativityLevel PreviousLevel;
        public CreativityLevel NewLevel;
        public float ExpressionScore;
        public System.DateTime Timestamp;
        public string PlayerId;
    }
    
    // Enums
    public enum PlayerChoiceType
    {
        CultivationMethod,
        FacilityDesign,
        ResourceAllocation,
        TechnologyAdoption,
        QualityStandard,
        MarketStrategy,
        EnvironmentalApproach
    }
    
    // ChoiceConsequences enum is defined in ProjectChimera.Events.EventDataStructures.cs to avoid CS0101 duplicate definition errors
    
    public enum ConsequenceType
    {
        YieldChange,
        EfficiencyChange,
        CostChange,
        QualityChange,
        UnlockChange,
        RelationshipChange
    }
    
    public enum PlayerAgencyLevel
    {
        Guided,
        Assisted,
        Independent,
        Advanced,
        Expert
    }
    
    public enum CreativityLevel
    {
        Beginner,
        Novice,
        Intermediate,
        Advanced,
        Expert,
        Master
    }
    
    public enum CreativeSolutionType
    {
        None,
        ProcessOptimization,
        EquipmentInnovation,
        SpaceUtilization,
        ResourceManagement,
        QualityEnhancement,
        CostReduction,
        EfficiencyGain
    }
    
    public enum CreativeSolutionResult
    {
        Invalid,
        Failed,
        Successful,
        Exceptional
    }
    
    #endregion
}