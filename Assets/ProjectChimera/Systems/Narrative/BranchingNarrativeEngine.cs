using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;
using ProjectChimera.Events;
// Type aliases to resolve namespace ambiguity
using DataNarrativeState = ProjectChimera.Data.Narrative.NarrativeState;
using DataPlayerChoiceData = ProjectChimera.Data.Narrative.PlayerChoiceData;
using DataBranchingNarrativeState = ProjectChimera.Data.Narrative.BranchingNarrativeState;
using DataCharacterRelationship = ProjectChimera.Data.Narrative.CharacterRelationship;
using DataNarrativeEvent = ProjectChimera.Data.Narrative.NarrativeEvent;
using DataPlayerDecision = ProjectChimera.Data.Narrative.PlayerDecision;

namespace ProjectChimera.Systems.Narrative
{
    /// <summary>
    /// Advanced branching narrative engine for Project Chimera's story campaign system.
    /// Features dynamic story path generation, consequence-driven branching, character relationship
    /// integration, and educational content validation with scientific accuracy enforcement.
    /// Provides enterprise-grade performance optimization and cross-system integration capabilities.
    /// </summary>
    public class BranchingNarrativeEngine : MonoBehaviour
    {
        [Header("Narrative Configuration")]
        [SerializeField] private bool _enableDynamicBranching = true;
        [SerializeField] private bool _enableConsequenceTracking = true;
        [SerializeField] private bool _enableCharacterRelationshipInfluence = true;
        [SerializeField] private bool _enableEducationalContentValidation = true;
        [SerializeField] private float _narrativeUpdateInterval = 0.5f;
        
        [Header("Branching Parameters")]
        [SerializeField] private int _maxActiveBranches = 10;
        [SerializeField] private int _maxBranchDepth = 5;
        [SerializeField] private float _branchingProbabilityThreshold = 0.7f;
        [SerializeField] private bool _enableAdaptiveBranching = true;
        [SerializeField] private float _playerChoiceWeight = 0.8f;
        
        [Header("Performance Settings")]
        [SerializeField] private bool _enableBranchCaching = true;
        [SerializeField] private int _maxCachedBranches = 50;
        [SerializeField] private bool _enableLazyBranchLoading = true;
        [SerializeField] private float _branchCleanupInterval = 10f;
        
        [Header("Educational Integration")]
        [SerializeField] private bool _enforceEducationalFlow = true;
        [SerializeField] private float _educationalContentRatio = 0.6f;
        [SerializeField] private bool _validateScientificAccuracy = true;
        [SerializeField] private bool _trackLearningProgression = true;
        
        // Core narrative state - using both Data layer state and Systems layer extended state
        private DataNarrativeState _currentState;
        private DataBranchingNarrativeState _branchingState;
        private Dictionary<string, NarrativeBranch> _activeBranches;
        private Dictionary<string, BranchNode> _narrativeGraph;
        private Queue<DataNarrativeEvent> _pendingEvents;
        
        // Configuration references
        private CampaignConfigSO _campaignConfig;
        private StoryArcLibrarySO _storyLibrary;
        private CharacterDatabaseSO _characterDatabase;
        
        // Branch management
        private Dictionary<string, CachedBranch> _branchCache;
        private BranchProbabilityCalculator _probabilityCalculator;
        private ConsequencePredictor _consequencePredictor;
        private EducationalFlowValidator _educationalValidator;
        
        // Cross-system integration
        private CharacterRelationshipSystem _relationshipSystem;
        private ConsequenceTrackingSystem _consequenceTracker;
        private DialogueProcessingEngine _dialogueEngine;
        
        // Performance monitoring
        private NarrativePerformanceMetrics _performanceMetrics;
        private float _lastCleanupTime;
        private int _totalBranchesGenerated;
        private float _averageBranchGenerationTime;
        
        // Events
        public event Action<NarrativeBranch> OnBranchGenerated;
        public event Action<DataPlayerDecision> OnDecisionProcessed;
        public event Action<DataNarrativeEvent> OnNarrativeEvent;
        public event Action<ProjectChimera.Data.Narrative.EducationalMilestone> OnEducationalMilestone;
        
        // Properties
        public bool IsInitialized { get; private set; }
        public int ActiveBranchCount => _activeBranches?.Count ?? 0;
        public DataNarrativeState CurrentState => _currentState;
        
        public void Initialize(CampaignConfigSO campaignConfig, StoryArcLibrarySO storyLibrary)
        {
            _campaignConfig = campaignConfig;
            _storyLibrary = storyLibrary;
            _characterDatabase = campaignConfig.CharacterDatabase;
            
            // Initialize core collections
            _activeBranches = new Dictionary<string, NarrativeBranch>();
            _narrativeGraph = new Dictionary<string, BranchNode>();
            _pendingEvents = new Queue<DataNarrativeEvent>();
            _branchCache = new Dictionary<string, CachedBranch>();
            
            // Initialize narrative state using Data layer structure
            _currentState = new DataNarrativeState
            {
                CurrentMainStory = "",
                ActiveStoryArcs = new List<string>(),
                AvailableChoices = new List<string>(),
                RecentEvents = new List<string>(),
                StateTimestamp = DateTime.Now
            };
            
            // Initialize extended branching state for Systems layer functionality
            _branchingState = new DataBranchingNarrativeState
            {
                StateId = Guid.NewGuid().ToString(),
                CurrentArcId = "",
                CurrentBeatId = "",
                PlayerChoiceHistory = new List<DataPlayerChoiceData>(),
                CharacterRelationshipStates = new Dictionary<string, DataCharacterRelationship>(),
                EducationalProgress = new ProjectChimera.Data.Narrative.EducationalProgressTracker(),
                NarrativeFlagsSet = new HashSet<string>(),
                Timestamp = DateTime.Now
            };
            
            // Initialize branch management systems
            _probabilityCalculator = new BranchProbabilityCalculator();
            _probabilityCalculator.Initialize(_campaignConfig);
            
            _consequencePredictor = new ConsequencePredictor();
            _consequencePredictor.Initialize(_campaignConfig);
            
            // Initialize educational validator
            if (_enableEducationalContentValidation)
            {
                _educationalValidator = new EducationalFlowValidator();
                _educationalValidator.Initialize(_characterDatabase, _campaignConfig);
            }
            
            // Initialize performance metrics
            _performanceMetrics = new NarrativePerformanceMetrics();
            
            // Build initial narrative graph
            BuildNarrativeGraph();
            
            IsInitialized = true;
            Debug.Log("[BranchingNarrativeEngine] Initialized with campaign configuration");
        }
        
        private void Update()
        {
            if (!IsInitialized) return;
            
            // Process pending narrative events
            ProcessPendingEvents();
            
            // Update active branches
            UpdateActiveBranches();
            
            // Perform periodic cleanup
            if (Time.time - _lastCleanupTime >= _branchCleanupInterval)
            {
                CleanupInactiveBranches();
                _lastCleanupTime = Time.time;
            }
            
            // Update performance metrics
            _performanceMetrics?.Update(Time.deltaTime);
        }
        
        private void BuildNarrativeGraph()
        {
            if (_storyLibrary == null) return;
            
            var allStoryArcs = _storyLibrary.GetAllStoryArcs();
            foreach (var storyArc in allStoryArcs)
            {
                BuildGraphFromStoryArc(storyArc);
            }
            
            Debug.Log($"[BranchingNarrativeEngine] Built narrative graph with {_narrativeGraph.Count} nodes");
        }
        
        private void BuildGraphFromStoryArc(StoryArcSO storyArc)
        {
            if (storyArc?.StoryBeats == null) return;
            
            foreach (var beat in storyArc.StoryBeats)
            {
                var node = new BranchNode
                {
                    NodeId = beat.BeatId,
                    ArcId = storyArc.ArcId,
                    BeatData = beat,
                    Connections = new List<BranchConnection>(),
                    Requirements = beat.Requirements ?? new List<string>(),
                    EducationalContent = ExtractEducationalContent(beat),
                    CharacterInfluences = ExtractCharacterInfluences(beat)
                };
                
                _narrativeGraph[beat.BeatId] = node;
            }
            
            // Build connections between beats
            BuildNodeConnections(storyArc);
        }
        
        private void BuildNodeConnections(StoryArcSO storyArc)
        {
            for (int i = 0; i < storyArc.StoryBeats.Count - 1; i++)
            {
                var currentBeat = storyArc.StoryBeats[i];
                var nextBeat = storyArc.StoryBeats[i + 1];
                
                if (_narrativeGraph.TryGetValue(currentBeat.BeatId, out var currentNode))
                {
                    var connection = new BranchConnection
                    {
                        ToNodeId = nextBeat.BeatId,
                        RequiredChoices = new List<string>(),
                        Probability = 1.0f,
                        ConsequenceWeight = 0.5f
                    };
                    
                    currentNode.Connections.Add(connection);
                }
            }
        }
        
        private EducationalContent ExtractEducationalContent(StoryBeatSO beat)
        {
            if (beat.Tags?.Any(tag => tag.StartsWith("educational:")) == true)
            {
                return new EducationalContent
                {
                    ContentId = Guid.NewGuid().ToString(),
                    Topic = ExtractEducationalTopic(beat),
                    Content = beat.Description,
                    EducationalValue = CalculateEducationalValue(beat),
                    IsScientificallyValidated = ValidateScientificContent(beat)
                };
            }
            
            return null;
        }
        
        private string ExtractEducationalTopic(StoryBeatSO beat)
        {
            var educationalTag = beat.Tags?.FirstOrDefault(tag => tag.StartsWith("educational:"));
            return educationalTag?.Substring(12) ?? "general"; // Remove "educational:" prefix
        }
        
        private float CalculateEducationalValue(StoryBeatSO beat)
        {
            var baseValue = 0.5f;
            
            // Increase value for detailed content
            if (beat.Description?.Length > 100) baseValue += 0.2f;
            
            // Increase value for scientific accuracy tags
            if (beat.Tags?.Contains("scientifically_accurate") == true) baseValue += 0.3f;
            
            return Mathf.Clamp01(baseValue);
        }
        
        private bool ValidateScientificContent(StoryBeatSO beat)
        {
            if (!_validateScientificAccuracy) return true;
            
            // Check for scientific accuracy validation tags
            return beat.Tags?.Contains("scientifically_accurate") == true;
        }
        
        private Dictionary<string, float> ExtractCharacterInfluences(StoryBeatSO beat)
        {
            var influences = new Dictionary<string, float>();
            
            if (beat.InvolvedCharacters != null)
            {
                foreach (var character in beat.InvolvedCharacters)
                {
                    influences[character.CharacterId] = CalculateCharacterInfluence(beat, character.CharacterId);
                }
            }
            
            return influences;
        }
        
        private float CalculateCharacterInfluence(StoryBeatSO beat, string characterId)
        {
            // Base influence from beat importance
            var baseInfluence = beat.Importance * 0.3f;
            
            // Adjust based on character role in beat
            var roleInfluence = beat.Tags?.Contains($"protagonist:{characterId}") == true ? 0.5f : 0.2f;
            
            return Mathf.Clamp01(baseInfluence + roleInfluence);
        }
        
        public NarrativeBranch GenerateBranch(DataPlayerDecision decision)
        {
            if (!IsInitialized) return null;
            
            var startTime = Time.realtimeSinceStartup;
            
            try
            {
                // Get current node
                var currentNode = GetCurrentNode();
                if (currentNode == null) return null;
                
                // Calculate branch probability
                var branchProbability = _probabilityCalculator.CalculateBranchProbability(decision, _currentState);
                
                // Check if branch should be generated
                if (branchProbability < _branchingProbabilityThreshold && !_enableAdaptiveBranching)
                {
                    return null;
                }
                
                // Generate branch options
                var branchOptions = GenerateBranchOptions(currentNode, decision);
                
                // Select best branch based on various factors
                var selectedBranch = SelectOptimalBranch(branchOptions, decision);
                
                if (selectedBranch != null)
                {
                    // Validate educational flow if enabled
                    if (_enforceEducationalFlow && !ValidateEducationalFlow(selectedBranch))
                    {
                        selectedBranch = AdjustBranchForEducationalFlow(selectedBranch);
                    }
                    
                    // Add to active branches
                    _activeBranches[selectedBranch.BranchId] = selectedBranch;
                    
                    // Raise event
                    OnBranchGenerated?.Invoke(selectedBranch);
                    
                    // Update statistics
                    _totalBranchesGenerated++;
                }
                
                return selectedBranch;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BranchingNarrativeEngine] Error generating branch: {ex.Message}");
                return null;
            }
            finally
            {
                var generationTime = Time.realtimeSinceStartup - startTime;
                UpdateBranchGenerationMetrics(generationTime);
            }
        }
        
        private BranchNode GetCurrentNode()
        {
            if (string.IsNullOrEmpty(_branchingState.CurrentBeatId))
            {
                return null;
            }
            
            return _narrativeGraph.GetValueOrDefault(_branchingState.CurrentBeatId);
        }
        
        private List<NarrativeBranch> GenerateBranchOptions(BranchNode currentNode, DataPlayerDecision decision)
        {
            var branchOptions = new List<NarrativeBranch>();
            
            foreach (var connection in currentNode.Connections)
            {
                if (!_narrativeGraph.TryGetValue(connection.ToNodeId, out var targetNode))
                    continue;
                
                // Check if connection requirements are met
                if (!AreRequirementsMet(connection.RequiredChoices, decision))
                    continue;
                
                // Generate branch
                var branch = new NarrativeBranch
                {
                    BranchId = Guid.NewGuid().ToString(),
                    FromNodeId = currentNode.NodeId,
                    ToNodeId = targetNode.NodeId,
                    TriggeringDecision = decision,
                    BranchProbability = connection.Probability,
                    ConsequenceWeight = connection.ConsequenceWeight,
                    EstimatedConsequences = PredictConsequences(decision, targetNode),
                    EducationalValue = targetNode.EducationalContent?.EducationalValue ?? 0f,
                    CharacterImpacts = CalculateCharacterImpacts(targetNode, decision),
                    Timestamp = DateTime.Now
                };
                
                branchOptions.Add(branch);
            }
            
            return branchOptions;
        }
        
        private bool AreRequirementsMet(List<string> requiredChoices, DataPlayerDecision decision)
        {
            if (requiredChoices == null || requiredChoices.Count == 0)
                return true;
            
            // Check if player decision satisfies requirements
            foreach (var requirement in requiredChoices)
            {
                if (!DoesDecisionMeetRequirement(decision, requirement))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        private bool DoesDecisionMeetRequirement(DataPlayerDecision decision, string requirement)
        {
            // Parse requirement (e.g., "choice:help_character", "flag:completed_tutorial")
            var parts = requirement.Split(':');
            if (parts.Length != 2) return false;
            
            var requirementType = parts[0];
            var requirementValue = parts[1];
            
            return requirementType switch
            {
                "choice" => decision.Choice.ChoiceId == requirementValue,
                "flag" => _branchingState.NarrativeFlagsSet.Contains(requirementValue),
                "relationship" => CheckRelationshipRequirement(requirementValue),
                _ => false
            };
        }
        
        private bool CheckRelationshipRequirement(string requirement)
        {
            // Parse relationship requirement (e.g., "character_id:trust:50")
            var parts = requirement.Split(':');
            if (parts.Length != 3) return false;
            
            var characterId = parts[0];
            var relationshipType = parts[1];
            var threshold = float.Parse(parts[2]);
            
            if (!_branchingState.CharacterRelationshipStates.TryGetValue(characterId, out var currentRelationship))
                return false;
            
            var currentLevel = currentRelationship.GetOverallRelationshipLevel();
            return currentLevel >= threshold;
        }
        
        private List<Consequence> PredictConsequences(DataPlayerDecision decision, BranchNode targetNode)
        {
            return _consequencePredictor.PredictConsequences(decision, targetNode, _currentState);
        }
        
        private Dictionary<string, float> CalculateCharacterImpacts(BranchNode targetNode, DataPlayerDecision decision)
        {
            var impacts = new Dictionary<string, float>();
            
            foreach (var kvp in targetNode.CharacterInfluences)
            {
                var characterId = kvp.Key;
                var baseImpact = kvp.Value;
                
                // Adjust impact based on player decision and current relationship
                var adjustedImpact = AdjustImpactForRelationship(characterId, baseImpact, decision);
                impacts[characterId] = adjustedImpact;
            }
            
            return impacts;
        }
        
        private float AdjustImpactForRelationship(string characterId, float baseImpact, DataPlayerDecision decision)
        {
            if (_branchingState.CharacterRelationshipStates.TryGetValue(characterId, out var currentRelationship))
            {
                // Higher relationships amplify positive impacts, reduce negative impacts
                var relationshipLevel = currentRelationship.GetOverallRelationshipLevel();
                var relationshipModifier = Mathf.Lerp(0.5f, 1.5f, relationshipLevel / 100f);
                return baseImpact * relationshipModifier;
            }
            
            return baseImpact;
        }
        
        private NarrativeBranch SelectOptimalBranch(List<NarrativeBranch> branchOptions, DataPlayerDecision decision)
        {
            if (branchOptions.Count == 0) return null;
            if (branchOptions.Count == 1) return branchOptions[0];
            
            // Score each branch based on multiple factors
            var scoredBranches = branchOptions.Select(branch => new
            {
                Branch = branch,
                Score = CalculateBranchScore(branch, decision)
            }).OrderByDescending(x => x.Score).ToList();
            
            return scoredBranches.First().Branch;
        }
        
        private float CalculateBranchScore(NarrativeBranch branch, DataPlayerDecision decision)
        {
            var score = 0f;
            
            // Player choice weight
            score += branch.BranchProbability * _playerChoiceWeight;
            
            // Educational value
            if (_enforceEducationalFlow)
            {
                score += branch.EducationalValue * 0.3f;
            }
            
            // Character relationship impact
            var relationshipImpact = branch.CharacterImpacts.Values.Sum() / Math.Max(1, branch.CharacterImpacts.Count);
            score += relationshipImpact * 0.2f;
            
            // Consequence desirability (positive consequences score higher)
            var consequenceScore = CalculateConsequenceScore(branch.EstimatedConsequences);
            score += consequenceScore * 0.2f;
            
            // Narrative variety (prefer less traveled paths)
            var varietyBonus = CalculateVarietyBonus(branch);
            score += varietyBonus * 0.1f;
            
            return Mathf.Clamp01(score);
        }
        
        private float CalculateConsequenceScore(List<Consequence> consequences)
        {
            if (consequences == null || consequences.Count == 0) return 0f;
            
            var totalScore = consequences.Sum(c => c.Desirability);
            return totalScore / consequences.Count;
        }
        
        private float CalculateVarietyBonus(NarrativeBranch branch)
        {
            // Check how often this path has been taken
            var pathFrequency = GetPathFrequency(branch.ToNodeId);
            return Mathf.Lerp(0.2f, 0f, pathFrequency); // Bonus for less traveled paths
        }
        
        private float GetPathFrequency(string nodeId)
        {
            // Calculate how frequently this node has been visited
            var totalDecisions = _branchingState.PlayerChoiceHistory.Count;
            if (totalDecisions == 0) return 0f;
            
            var nodeVisits = _branchingState.PlayerChoiceHistory.Count(d => d.ResultingNodeId == nodeId);
            return (float)nodeVisits / totalDecisions;
        }
        
        private bool ValidateEducationalFlow(NarrativeBranch branch)
        {
            if (_educationalValidator == null) return true;
            
            return _educationalValidator.ValidateBranchEducationalFlow(branch, _currentState);
        }
        
        private NarrativeBranch AdjustBranchForEducationalFlow(NarrativeBranch branch)
        {
            // Enhance educational content in the branch
            if (_educationalValidator != null)
            {
                return _educationalValidator.EnhanceBranchEducationalContent(branch);
            }
            
            return branch;
        }
        
        public void ProcessPlayerDecision(DataPlayerDecision decision)
        {
            if (!IsInitialized) return;
            
            try
            {
                // Add to player choice history
                var choiceData = new DataPlayerChoiceData
                {
                    ChoiceId = decision.Choice?.ChoiceId ?? decision.DecisionId,
                    SelectedOptionIndex = 0, // Default or derive from decision
                    Timestamp = decision.Timestamp
                };
                _branchingState.PlayerChoiceHistory.Add(choiceData);
                
                // Generate and process branch
                var branch = GenerateBranch(decision);
                if (branch != null)
                {
                    ExecuteBranch(branch);
                }
                
                // Update narrative state
                UpdateNarrativeState(decision);
                
                // Raise event
                OnDecisionProcessed?.Invoke(decision);
                
                Debug.Log($"[BranchingNarrativeEngine] Processed player decision: {decision.Choice.ChoiceId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BranchingNarrativeEngine] Error processing player decision: {ex.Message}");
            }
        }
        
        private void ExecuteBranch(NarrativeBranch branch)
        {
            // Update current state to reflect branch execution
            _branchingState.CurrentBeatId = branch.ToNodeId;
            _currentState.PreviousBeatId = branch.FromNodeId;
            
            // Apply character impacts
            foreach (var impact in branch.CharacterImpacts)
            {
                var characterId = impact.Key;
                var impactValue = impact.Value;
                
                if (!_branchingState.CharacterRelationshipStates.ContainsKey(characterId))
                {
                    _branchingState.CharacterRelationshipStates[characterId] = new DataCharacterRelationship(characterId);
                }
                
                var relationship = _branchingState.CharacterRelationshipStates[characterId];
                relationship.RelationshipLevel += impactValue;
                relationship.RelationshipLevel = Mathf.Clamp(relationship.RelationshipLevel, 0f, 100f);
            }
            
            // Apply consequences
            foreach (var consequence in branch.EstimatedConsequences)
            {
                ApplyConsequence(consequence);
            }
            
            // Update educational progress
            if (branch.EducationalValue > 0)
            {
                UpdateEducationalProgress(branch);
            }
        }
        
        private void ApplyConsequence(Consequence consequence)
        {
            // Add narrative flags based on consequence
            foreach (var flag in consequence.NarrativeFlags)
            {
                _branchingState.NarrativeFlagsSet.Add(flag);
            }
            
            // Apply other consequence effects
            _consequenceTracker?.ApplyConsequence(consequence);
        }
        
        private void UpdateEducationalProgress(NarrativeBranch branch)
        {
            if (branch.EducationalValue > 0)
            {
                _branchingState.EducationalProgress.AddLearningMoment(new ProjectChimera.Data.Narrative.LearningMoment
                {
                    MomentId = Guid.NewGuid().ToString(),
                    Topic = branch.ToNodeId, // Using node ID as topic for simplicity
                    EducationalValue = branch.EducationalValue,
                    Timestamp = DateTime.Now,
                    BranchId = branch.BranchId
                });
                
                // Check for educational milestones
                CheckEducationalMilestones();
            }
        }
        
        private void CheckEducationalMilestones()
        {
            var progress = _branchingState.EducationalProgress;
            var totalEducationalValue = progress.GetTotalEducationalValue();
            
            // Check if player has reached new educational milestones
            if (totalEducationalValue >= 10f && !progress.HasMilestone("basic_understanding"))
            {
                var milestone = new ProjectChimera.Data.Narrative.EducationalMilestone
                {
                    MilestoneId = "basic_understanding",
                    Description = "Achieved basic understanding of cultivation concepts",
                    AchievedTime = DateTime.Now,
                    EducationalValue = totalEducationalValue
                };
                
                progress.AddMilestone(milestone);
                OnEducationalMilestone?.Invoke(milestone);
            }
            
            // Additional milestone checks...
        }
        
        private void UpdateNarrativeState(DataPlayerDecision decision)
        {
            _branchingState.LastDecisionTime = DateTime.Now;
            _branchingState.DecisionCount++;
            
            // Update state based on decision context
            if (decision.Context?.AdditionalData() != null)
            {
                foreach (var kvp in decision.Context.AdditionalData())
                {
                    if (kvp.Key.StartsWith("flag:"))
                    {
                        _branchingState.NarrativeFlagsSet.Add(kvp.Key.Substring(5));
                    }
                }
            }
        }
        
        private void ProcessPendingEvents()
        {
            while (_pendingEvents.Count > 0)
            {
                var narrativeEvent = _pendingEvents.Dequeue();
                ProcessNarrativeEvent(narrativeEvent);
            }
        }
        
        private void ProcessNarrativeEvent(DataNarrativeEvent narrativeEvent)
        {
            // Process different types of narrative events
            switch (narrativeEvent.EventType)
            {
                case NarrativeEventType.CharacterEncounter:
                    ProcessCharacterEncounter(narrativeEvent);
                    break;
                case NarrativeEventType.ConsequenceTriggered:
                    ProcessConsequenceEvent(narrativeEvent);
                    break;
                case NarrativeEventType.EducationalMoment:
                    ProcessEducationalEvent(narrativeEvent);
                    break;
                default:
                    Debug.LogWarning($"[BranchingNarrativeEngine] Unknown narrative event type: {narrativeEvent.EventType}");
                    break;
            }
            
            OnNarrativeEvent?.Invoke(narrativeEvent);
        }
        
        private void ProcessCharacterEncounter(DataNarrativeEvent narrativeEvent)
        {
            // Handle character encounter logic
            if (narrativeEvent.Data is CharacterEncounterData encounterData)
            {
                // Update character relationship states
                if (!_branchingState.CharacterRelationshipStates.ContainsKey(encounterData.CharacterId))
                {
                    var relationship = new DataCharacterRelationship(encounterData.CharacterId);
                    relationship.RelationshipLevel = encounterData.InitialRelationshipLevel;
                    _branchingState.CharacterRelationshipStates[encounterData.CharacterId] = relationship;
                }
            }
        }
        
        private void ProcessConsequenceEvent(DataNarrativeEvent narrativeEvent)
        {
            // Handle consequence-triggered events
            if (narrativeEvent.Data is ConsequenceEventData consequenceData)
            {
                ApplyConsequence(consequenceData.Consequence);
            }
        }
        
        private void ProcessEducationalEvent(DataNarrativeEvent narrativeEvent)
        {
            // Handle educational events
            if (narrativeEvent.Data is ProjectChimera.Data.Narrative.LearningMoment learningMoment)
            {
                _branchingState.EducationalProgress.AddLearningMoment(learningMoment);
            }
        }
        
        private void UpdateActiveBranches()
        {
            var branchesToRemove = new List<string>();
            
            foreach (var kvp in _activeBranches)
            {
                var branch = kvp.Value;
                
                // Update branch state
                branch.Update(Time.deltaTime);
                
                // Check if branch should be deactivated
                if (branch.IsExpired || branch.IsCompleted)
                {
                    branchesToRemove.Add(kvp.Key);
                }
            }
            
            // Remove expired branches
            foreach (var branchId in branchesToRemove)
            {
                _activeBranches.Remove(branchId);
            }
        }
        
        private void CleanupInactiveBranches()
        {
            // Cache cleanup
            if (_enableBranchCaching)
            {
                var cacheKeysToRemove = new List<string>();
                var cutoffTime = DateTime.Now.AddMinutes(-30); // 30 minute cache lifetime
                
                foreach (var kvp in _branchCache)
                {
                    if (kvp.Value.Timestamp < cutoffTime)
                    {
                        cacheKeysToRemove.Add(kvp.Key);
                    }
                }
                
                foreach (var key in cacheKeysToRemove)
                {
                    _branchCache.Remove(key);
                }
                
                if (cacheKeysToRemove.Count > 0)
                {
                    Debug.Log($"[BranchingNarrativeEngine] Cleaned up {cacheKeysToRemove.Count} cached branches");
                }
            }
        }
        
        private void UpdateBranchGenerationMetrics(float generationTime)
        {
            // Update rolling average
            var weight = 0.1f;
            _averageBranchGenerationTime = (_averageBranchGenerationTime * (1f - weight)) + (generationTime * weight);
            
            // Update performance metrics
            _performanceMetrics?.RecordBranchGeneration(generationTime);
        }
        
        public void QueueNarrativeEvent(DataNarrativeEvent narrativeEvent)
        {
            _pendingEvents.Enqueue(narrativeEvent);
        }
        
        public DataNarrativeState GetCurrentNarrativeState()
        {
            return _currentState;
        }
        
        public List<NarrativeBranch> GetActiveBranches()
        {
            return _activeBranches.Values.ToList();
        }
        
        public NarrativeEngineStatistics GetStatistics()
        {
            return new NarrativeEngineStatistics
            {
                TotalBranchesGenerated = _totalBranchesGenerated,
                ActiveBranchCount = _activeBranches.Count,
                AverageBranchGenerationTime = _averageBranchGenerationTime,
                CachedBranchCount = _branchCache.Count,
                NarrativeGraphSize = _narrativeGraph.Count,
                TotalDecisionsProcessed = _branchingState.DecisionCount,
                EducationalMilestonesReached = _branchingState.EducationalProgress.GetMilestoneCount()
            };
        }
        
        public void Update(float deltaTime)
        {
            // Public update method for external system calls
            if (!IsInitialized) return;
            
            // Update is handled by MonoBehaviour Update method
        }
        
        private void OnDestroy()
        {
            // Cleanup resources
            _activeBranches?.Clear();
            _branchCache?.Clear();
            _pendingEvents?.Clear();
        }
    }
    
    // Supporting data structures for Systems layer only (not duplicating Data layer)
    [Serializable]
    public class BranchingNarrativeState
    {
        public string StateId;
        public string CurrentArcId;
        public string CurrentBeatId;
        public string PreviousBeatId;
        public List<DataPlayerChoiceData> PlayerChoiceHistory = new List<DataPlayerChoiceData>();
        public Dictionary<string, DataCharacterRelationship> CharacterRelationshipStates = new Dictionary<string, DataCharacterRelationship>();
        public ProjectChimera.Data.Narrative.EducationalProgressTracker EducationalProgress = new ProjectChimera.Data.Narrative.EducationalProgressTracker();
        public HashSet<string> NarrativeFlagsSet = new HashSet<string>();
        public DateTime Timestamp;
        public DateTime LastDecisionTime;
        public int DecisionCount;
    }
    
    [Serializable]
    public class NarrativeBranch
    {
        public string BranchId;
        public string FromNodeId;
        public string ToNodeId;
        public DataPlayerDecision TriggeringDecision;
        public float BranchProbability;
        public float ConsequenceWeight;
        public List<Consequence> EstimatedConsequences = new List<Consequence>();
        public float EducationalValue;
        public Dictionary<string, float> CharacterImpacts = new Dictionary<string, float>();
        public DateTime Timestamp;
        public bool IsExpired;
        public bool IsCompleted;
        public float Duration;
        
        public void Update(float deltaTime)
        {
            Duration += deltaTime;
            
            // Check expiration conditions
            if (Duration > 300f) // 5 minutes
            {
                IsExpired = true;
            }
        }
    }
    
    [Serializable]
    public class BranchNode
    {
        public string NodeId;
        public string ArcId;
        public StoryBeatSO BeatData;
        public List<BranchConnection> Connections = new List<BranchConnection>();
        public List<string> Requirements = new List<string>();
        public EducationalContent EducationalContent;
        public Dictionary<string, float> CharacterInfluences = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class BranchConnection
    {
        public string ToNodeId;
        public List<string> RequiredChoices = new List<string>();
        public float Probability = 1.0f;
        public float ConsequenceWeight = 0.5f;
    }
    
    // Note: Using NarrativeState and NarrativeEvent from ProjectChimera.Data.Narrative
    
    // Note: Event data classes are defined in ProjectChimera.Data.Narrative to avoid duplicates
    // Removed duplicate definitions to prevent namespace conflicts
    
    [Serializable]
    public class CachedBranch
    {
        public string BranchId;
        public NarrativeBranch Branch;
        public DateTime Timestamp;
        public int AccessCount;
    }
    
    
    [Serializable]
    public class EducationalMilestone
    {
        public string MilestoneId;
        public string Description;
        public DateTime Timestamp;
        public float EducationalValue;
    }
    
    [Serializable]
    public class NarrativeEngineStatistics
    {
        public int TotalBranchesGenerated;
        public int ActiveBranchCount;
        public float AverageBranchGenerationTime;
        public int CachedBranchCount;
        public int NarrativeGraphSize;
        public int TotalDecisionsProcessed;
        public int EducationalMilestonesReached;
    }
    
    [Serializable]
    public class NarrativePerformanceMetrics
    {
        public float AverageUpdateTime;
        public float PeakUpdateTime;
        public int TotalUpdates;
        
        public void Update(float deltaTime)
        {
            TotalUpdates++;
            
            // Update rolling average
            var weight = 0.1f;
            AverageUpdateTime = (AverageUpdateTime * (1f - weight)) + (deltaTime * weight);
            
            // Track peak time
            PeakUpdateTime = Mathf.Max(PeakUpdateTime, deltaTime);
        }
        
        public void RecordBranchGeneration(float generationTime)
        {
            // Track branch generation performance
        }
    }
    
    // Event data classes
    public class CharacterEncounterData
    {
        public string CharacterId;
        public float InitialRelationshipLevel;
    }
    
    // ConsequenceEventData is defined in ProjectChimera.Events.CultivationGamingEventData to avoid CS0101 duplicate definition errors
    // EducationalEventData: Using LearningMoment directly instead of separate event data class
    
    
    // Placeholder classes for compilation
    public class BranchProbabilityCalculator
    {
        public void Initialize(CampaignConfigSO config) { }
        public float CalculateBranchProbability(DataPlayerDecision decision, DataNarrativeState state) => 0.8f;
    }
    
    public class ConsequencePredictor
    {
        public void Initialize(CampaignConfigSO config) { }
        public List<Consequence> PredictConsequences(DataPlayerDecision decision, BranchNode node, DataNarrativeState state) => new List<Consequence>();
    }
    
    public class EducationalFlowValidator
    {
        public void Initialize(CharacterDatabaseSO database, CampaignConfigSO config) { }
        public bool ValidateBranchEducationalFlow(NarrativeBranch branch, DataNarrativeState state) => true;
        public NarrativeBranch EnhanceBranchEducationalContent(NarrativeBranch branch) => branch;
    }
}