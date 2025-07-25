using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Narrative;

namespace ProjectChimera.Systems.Narrative
{
    
    /// <summary>
    /// Advanced consequence tracking system for Project Chimera's narrative engine.
    /// Features dynamic consequence generation, delayed effect processing, ripple effect modeling,
    /// and educational impact validation with scientific accuracy enforcement.
    /// Provides enterprise-grade performance optimization and cross-system integration.
    /// </summary>
    public class ConsequenceTrackingSystem : MonoBehaviour
    {
        [Header("Consequence Configuration")]
        [SerializeField] private bool _enableDelayedConsequences = true;
        [SerializeField] private bool _enableRippleEffects = true;
        [SerializeField] private bool _enableConsequencePrediction = true;
        [SerializeField] private bool _enableEducationalImpactTracking = true;
        [SerializeField] private float _consequenceUpdateInterval = 1.0f;
        
        [Header("Effect Processing")]
        [SerializeField] private int _maxActiveConsequences = 50;
        [SerializeField] private int _maxDelayedConsequences = 100;
        [SerializeField] private float _rippleEffectDecayRate = 0.1f;
        [SerializeField] private bool _enableConsequenceStacking = true;
        [SerializeField] private float _consequenceStackingThreshold = 0.5f;
        
        [Header("Performance Settings")]
        [SerializeField] private bool _enableConsequenceCaching = true;
        [SerializeField] private bool _enableBatchProcessing = true;
        [SerializeField] private int _maxConsequencesPerFrame = 5;
        [SerializeField] private float _cacheCleanupInterval = 30f;
        
        [Header("Educational Integration")]
        [SerializeField] private bool _trackEducationalConsequences = true;
        [SerializeField] private bool _validateScientificAccuracy = true;
        [SerializeField] private float _educationalImpactWeight = 0.8f;
        [SerializeField] private bool _enableLearningReinforcement = true;
        
        // Core consequence data
        private Dictionary<string, ActiveConsequence> _activeConsequences;
        private Queue<DelayedConsequence> _delayedConsequences;
        private Dictionary<string, ConsequenceTemplate> _consequenceTemplates;
        private Dictionary<string, List<RippleEffect>> _rippleEffectChains;
        
        // Prediction and analysis
        private ConsequencePredictionEngine _predictionEngine;
        private RippleEffectAnalyzer _rippleAnalyzer;
        private EducationalImpactTracker _educationalTracker;
        private ConsequenceValidator _consequenceValidator;
        
        // Performance optimization
        private Queue<ConsequenceProcessingTask> _processingQueue;
        private ConsequenceCache _consequenceCache;
        private ConsequencePerformanceMetrics _performanceMetrics;
        
        // Configuration references
        private CampaignConfigSO _campaignConfig;
        private CharacterDatabaseSO _characterDatabase;
        private StoryArcLibrarySO _storyLibrary;
        
        // Cross-system integration
        private CharacterRelationshipSystem _relationshipSystem;
        private BranchingNarrativeEngine _narrativeEngine;
        
        // Event integration
        private ConsequenceEventChannelSO _consequenceEventChannel;
        
        // Statistics and monitoring
        private float _lastUpdateTime;
        private float _lastCacheCleanup;
        private int _totalConsequencesProcessed;
        private float _averageProcessingTime;
        
        // Events
        public event Action<Consequence> OnConsequenceApplied;
        public event Action<DelayedConsequence> OnDelayedConsequenceTriggered;
        public event Action<RippleEffect> OnRippleEffectGenerated;
        public event Action<EducationalConsequence> OnEducationalImpact;
        
        // Properties
        public bool IsInitialized { get; private set; }
        public int ActiveConsequenceCount => _activeConsequences?.Count ?? 0;
        public int DelayedConsequenceCount => _delayedConsequences?.Count ?? 0;
        
        public void Initialize(CampaignConfigSO campaignConfig)
        {
            _campaignConfig = campaignConfig;
            _characterDatabase = campaignConfig.CharacterDatabase;
            _storyLibrary = campaignConfig.StoryArcLibrary;
            
            // Initialize core collections
            _activeConsequences = new Dictionary<string, ActiveConsequence>();
            _delayedConsequences = new Queue<DelayedConsequence>();
            _consequenceTemplates = new Dictionary<string, ConsequenceTemplate>();
            _rippleEffectChains = new Dictionary<string, List<RippleEffect>>();
            _processingQueue = new Queue<ConsequenceProcessingTask>();
            
            // Initialize analysis systems
            _predictionEngine = new ConsequencePredictionEngine();
            _predictionEngine.Initialize(_campaignConfig);
            
            _rippleAnalyzer = new RippleEffectAnalyzer();
            _rippleAnalyzer.Initialize(_campaignConfig);
            
            if (_enableEducationalImpactTracking)
            {
                _educationalTracker = new EducationalImpactTracker();
                _educationalTracker.Initialize(_campaignConfig);
            }
            
            _consequenceValidator = new ConsequenceValidator();
            _consequenceValidator.Initialize(_characterDatabase, _campaignConfig);
            
            // Initialize performance systems
            if (_enableConsequenceCaching)
            {
                _consequenceCache = new ConsequenceCache();
                _consequenceCache.Initialize(_campaignConfig.MaxCachedConsequences);
            }
            
            _performanceMetrics = new ConsequencePerformanceMetrics();
            
            // Load consequence templates
            LoadConsequenceTemplates();
            
            IsInitialized = true;
            Debug.Log("[ConsequenceTrackingSystem] Initialized with campaign configuration");
        }
        
        private void Update()
        {
            if (!IsInitialized) return;
            
            if (Time.time - _lastUpdateTime >= _consequenceUpdateInterval)
            {
                UpdateConsequences();
                _lastUpdateTime = Time.time;
            }
            
            if (_enableConsequenceCaching && Time.time - _lastCacheCleanup >= _cacheCleanupInterval)
            {
                CleanupCache();
                _lastCacheCleanup = Time.time;
            }
            
            _performanceMetrics?.Update(Time.deltaTime);
        }
        
        private void LoadConsequenceTemplates()
        {
            if (_storyLibrary == null) return;
            
            // Load templates from story arcs
            var allStoryArcs = _storyLibrary.GetAllStoryArcs();
            foreach (var storyArc in allStoryArcs)
            {
                LoadTemplatesFromStoryArc(storyArc);
            }
            
            Debug.Log($"[ConsequenceTrackingSystem] Loaded {_consequenceTemplates.Count} consequence templates");
        }
        
        private void LoadTemplatesFromStoryArc(StoryArcSO storyArc)
        {
            if (storyArc?.ConsequenceTemplates == null) return;
            
            foreach (var template in storyArc.ConsequenceTemplates)
            {
                _consequenceTemplates[template.TemplateId] = template;
            }
        }
        
        public List<Consequence> GetConsequencesForChoice(string choiceId)
        {
            var consequences = new List<Consequence>();
            
            try
            {
                // Get consequences from templates
                var templateConsequences = GetTemplateConsequences(choiceId);
                consequences.AddRange(templateConsequences);
                
                // Generate dynamic consequences if enabled
                if (_enableConsequencePrediction)
                {
                    var dynamicConsequences = _predictionEngine.GenerateDynamicConsequences(choiceId);
                    consequences.AddRange(dynamicConsequences);
                }
                
                // Validate consequences
                consequences = ValidateConsequences(consequences);
                
                // Cache consequences for performance
                if (_enableConsequenceCaching)
                {
                    _consequenceCache.CacheConsequences(choiceId, consequences);
                }
                
                return consequences;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ConsequenceTrackingSystem] Error getting consequences for choice {choiceId}: {ex.Message}");
                return new List<Consequence>();
            }
        }
        
        private List<Consequence> GetTemplateConsequences(string choiceId)
        {
            var consequences = new List<Consequence>();
            
            // Check cache first
            if (_enableConsequenceCaching && _consequenceCache.TryGetCachedConsequences(choiceId, out var cachedConsequences))
            {
                return cachedConsequences;
            }
            
            // Find templates that match this choice
            foreach (var template in _consequenceTemplates.Values)
            {
                if (template.TriggeredByChoices.Contains(choiceId))
                {
                    var consequence = CreateConsequenceFromTemplate(template, choiceId);
                    if (consequence != null)
                    {
                        consequences.Add(consequence);
                    }
                }
            }
            
            return consequences;
        }
        
        private Consequence CreateConsequenceFromTemplate(ConsequenceTemplate template, string choiceId)
        {
            try
            {
                var consequence = new Consequence
                {
                    ConsequenceId = Guid.NewGuid().ToString(),
                    Type = template.Type,
                    Description = template.Description,
                    Severity = template.Severity,
                    IsImmediate = template.IsImmediate,
                    DelayTime = template.DelayTime,
                    Duration = template.Duration,
                    CharacterTargets = new List<string>(template.CharacterTargets),
                    NarrativeFlags = new List<string>(template.NarrativeFlags),
                    Desirability = template.Desirability,
                    EducationalImpact = template.EducationalImpact,
                    TriggeringChoiceId = choiceId,
                    Timestamp = DateTime.Now
                };
                
                // Apply template-specific modifications
                ApplyTemplateModifications(consequence, template);
                
                return consequence;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ConsequenceTrackingSystem] Error creating consequence from template {template.TemplateId}: {ex.Message}");
                return null;
            }
        }
        
        private void ApplyTemplateModifications(Consequence consequence, ConsequenceTemplate template)
        {
            // Apply relationship modifiers
            foreach (var modifier in template.RelationshipModifiers)
            {
                consequence.RelationshipImpacts[modifier.CharacterId] = modifier.ImpactValue;
            }
            
            // Apply educational content
            if (template.HasEducationalContent)
            {
                consequence.EducationalContent = new EducationalConsequence
                {
                    Topic = template.EducationalTopic,
                    LearningValue = template.EducationalImpact,
                    IsScientificallyAccurate = template.IsScientificallyAccurate,
                    ReinforcementLevel = template.LearningReinforcementLevel
                };
            }
        }
        
        private List<Consequence> ValidateConsequences(List<Consequence> consequences)
        {
            return consequences.Where(c => _consequenceValidator.ValidateConsequence(c)).ToList();
        }
        
        public void ApplyConsequence(Consequence consequence)
        {
            if (!IsInitialized || consequence == null) return;
            
            var startTime = Time.realtimeSinceStartup;
            
            try
            {
                if (consequence.IsImmediate)
                {
                    ProcessImmediateConsequence(consequence);
                }
                else
                {
                    QueueDelayedConsequence(consequence);
                }
                
                // Generate ripple effects
                if (_enableRippleEffects)
                {
                    GenerateRippleEffects(consequence);
                }
                
                // Track educational impact
                if (_trackEducationalConsequences && consequence.EducationalContent != null)
                {
                    TrackEducationalImpact(consequence);
                }
                
                // Update statistics
                _totalConsequencesProcessed++;
                
                // Raise event
                OnConsequenceApplied?.Invoke(consequence);
                
                Debug.Log($"[ConsequenceTrackingSystem] Applied consequence: {consequence.ConsequenceId} - {consequence.Description}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ConsequenceTrackingSystem] Error applying consequence {consequence.ConsequenceId}: {ex.Message}");
            }
            finally
            {
                var processingTime = Time.realtimeSinceStartup - startTime;
                UpdatePerformanceMetrics(processingTime);
            }
        }
        
        private void ProcessImmediateConsequence(Consequence consequence)
        {
            // Apply immediate effects
            ApplyConsequenceEffects(consequence);
            
            // Add to active consequences for tracking
            var activeConsequence = new ActiveConsequence
            {
                ConsequenceData = consequence,
                ActivationTime = Time.time,
                IsActive = true,
                RemainingDuration = consequence.Duration
            };
            
            _activeConsequences[consequence.ConsequenceId] = activeConsequence;
        }
        
        private void QueueDelayedConsequence(Consequence consequence)
        {
            var delayedConsequence = new DelayedConsequence
            {
                ConsequenceData = consequence,
                TriggerTime = Time.time + consequence.DelayTime,
                IsQueued = true
            };
            
            _delayedConsequences.Enqueue(delayedConsequence);
            
            Debug.Log($"[ConsequenceTrackingSystem] Queued delayed consequence: {consequence.ConsequenceId} (delay: {consequence.DelayTime}s)");
        }
        
        private void ApplyConsequenceEffects(Consequence consequence)
        {
            // Apply character relationship impacts
            ApplyRelationshipImpacts(consequence);
            
            // Set narrative flags
            SetNarrativeFlags(consequence);
            
            // Apply system-specific effects
            ApplySystemEffects(consequence);
            
            // Apply educational effects
            if (consequence.EducationalContent != null)
            {
                ApplyEducationalEffects(consequence);
            }
        }
        
        private void ApplyRelationshipImpacts(Consequence consequence)
        {
            if (_relationshipSystem == null || consequence.RelationshipImpacts == null) return;
            
            foreach (var impact in consequence.RelationshipImpacts)
            {
                var characterId = impact.Key;
                var impactValue = impact.Value;
                
                var relationship = _relationshipSystem.GetRelationship(characterId);
                if (relationship != null)
                {
                    // Apply relationship change based on consequence type
                    switch (consequence.Type)
                    {
                        case ConsequenceType.TrustChange:
                            relationship.ModifyTrust(impactValue);
                            break;
                        case ConsequenceType.RespectChange:
                            relationship.ModifyRespect(impactValue);
                            break;
                        case ConsequenceType.InfluenceChange:
                            relationship.ModifyInfluence(impactValue);
                            break;
                        case ConsequenceType.RelationshipStatus:
                            // Modify overall relationship
                            relationship.ModifyTrust(impactValue * 0.4f);
                            relationship.ModifyRespect(impactValue * 0.4f);
                            relationship.ModifyInfluence(impactValue * 0.2f);
                            break;
                    }
                }
            }
        }
        
        private void SetNarrativeFlags(Consequence consequence)
        {
            if (consequence.NarrativeFlags == null) return;
            
            // Set flags in narrative engine
            if (_narrativeEngine != null)
            {
                var currentState = _narrativeEngine.GetCurrentNarrativeState();
                foreach (var flag in consequence.NarrativeFlags)
                {
                    currentState.NarrativeFlagsSet.Add(flag);
                }
            }
        }
        
        private void ApplySystemEffects(Consequence consequence)
        {
            // Apply consequence effects to other game systems
            switch (consequence.Type)
            {
                case ConsequenceType.EconomicImpact:
                    ApplyEconomicConsequence(consequence);
                    break;
                case ConsequenceType.EnvironmentalChange:
                    ApplyEnvironmentalConsequence(consequence);
                    break;
                case ConsequenceType.CultivationEffect:
                    ApplyCultivationConsequence(consequence);
                    break;
                case ConsequenceType.ProgressionUnlock:
                    ApplyProgressionConsequence(consequence);
                    break;
            }
        }
        
        private void ApplyEconomicConsequence(Consequence consequence)
        {
            // Integration with economy system
            // EconomyManager.Instance?.ApplyConsequence(consequence);
        }
        
        private void ApplyEnvironmentalConsequence(Consequence consequence)
        {
            // Integration with environmental system
            // EnvironmentalManager.Instance?.ApplyConsequence(consequence);
        }
        
        private void ApplyCultivationConsequence(Consequence consequence)
        {
            // Integration with cultivation system
            // PlantManager.Instance?.ApplyConsequence(consequence);
        }
        
        private void ApplyProgressionConsequence(Consequence consequence)
        {
            // Integration with progression system
            // ProgressionManager.Instance?.ApplyConsequence(consequence);
        }
        
        private void ApplyEducationalEffects(Consequence consequence)
        {
            if (_educationalTracker == null || consequence.EducationalContent == null) return;
            
            var educationalConsequence = consequence.EducationalContent;
            
            // Validate scientific accuracy
            if (_validateScientificAccuracy && !educationalConsequence.IsScientificallyAccurate)
            {
                Debug.LogWarning($"[ConsequenceTrackingSystem] Scientifically inaccurate educational consequence: {consequence.ConsequenceId}");
                return;
            }
            
            // Track educational impact
            _educationalTracker.TrackEducationalConsequence(educationalConsequence);
            
            // Apply learning reinforcement if enabled
            if (_enableLearningReinforcement)
            {
                ApplyLearningReinforcement(educationalConsequence);
            }
            
            // Raise educational impact event
            OnEducationalImpact?.Invoke(educationalConsequence);
        }
        
        private void ApplyLearningReinforcement(EducationalConsequence educationalConsequence)
        {
            // Reinforce learning based on consequence impact
            var reinforcementStrength = educationalConsequence.LearningValue * educationalConsequence.ReinforcementLevel;
            
            // Apply reinforcement to player's educational progress
            if (_narrativeEngine != null)
            {
                var currentState = _narrativeEngine.GetCurrentNarrativeState();
                currentState.EducationalProgress.AddLearningMoment(new ProjectChimera.Data.Narrative.LearningMoment
                {
                    MomentId = Guid.NewGuid().ToString(),
                    Topic = educationalConsequence.Topic,
                    EducationalValue = reinforcementStrength,
                    Timestamp = DateTime.Now,
                    IsReinforcement = true
                });
            }
        }
        
        private void GenerateRippleEffects(Consequence consequence)
        {
            if (_rippleAnalyzer == null) return;
            
            var rippleEffects = _rippleAnalyzer.GenerateRippleEffects(consequence);
            
            foreach (var rippleEffect in rippleEffects)
            {
                // Add to ripple effect chains
                if (!_rippleEffectChains.ContainsKey(consequence.ConsequenceId))
                {
                    _rippleEffectChains[consequence.ConsequenceId] = new List<RippleEffect>();
                }
                
                _rippleEffectChains[consequence.ConsequenceId].Add(rippleEffect);
                
                // Queue ripple effect for processing
                QueueRippleEffect(rippleEffect);
                
                // Raise event
                OnRippleEffectGenerated?.Invoke(rippleEffect);
            }
        }
        
        private void QueueRippleEffect(RippleEffect rippleEffect)
        {
            var rippleConsequence = new Consequence
            {
                ConsequenceId = Guid.NewGuid().ToString(),
                Type = rippleEffect.ConsequenceType,
                Description = rippleEffect.Description,
                Severity = rippleEffect.Severity,
                IsImmediate = false,
                DelayTime = rippleEffect.DelayTime,
                Duration = rippleEffect.Duration,
                Desirability = rippleEffect.Desirability,
                RelationshipImpacts = rippleEffect.RelationshipImpacts,
                Timestamp = DateTime.Now
            };
            
            QueueDelayedConsequence(rippleConsequence);
        }
        
        private void TrackEducationalImpact(Consequence consequence)
        {
            if (_educationalTracker == null || consequence.EducationalContent == null) return;
            
            _educationalTracker.TrackEducationalConsequence(consequence.EducationalContent);
        }
        
        private void UpdateConsequences()
        {
            // Process delayed consequences
            ProcessDelayedConsequences();
            
            // Update active consequences
            UpdateActiveConsequences();
            
            // Process ripple effect decay
            ProcessRippleEffectDecay();
            
            // Process batch operations if enabled
            if (_enableBatchProcessing)
            {
                ProcessBatchOperations();
            }
        }
        
        private void ProcessDelayedConsequences()
        {
            var currentTime = Time.time;
            var processedCount = 0;
            var maxProcessPerFrame = _enableBatchProcessing ? _maxConsequencesPerFrame : int.MaxValue;
            
            while (_delayedConsequences.Count > 0 && processedCount < maxProcessPerFrame)
            {
                var delayedConsequence = _delayedConsequences.Peek();
                
                if (delayedConsequence.TriggerTime <= currentTime)
                {
                    _delayedConsequences.Dequeue();
                    
                    // Process the delayed consequence
                    ProcessImmediateConsequence(delayedConsequence.ConsequenceData);
                    
                    // Raise event
                    OnDelayedConsequenceTriggered?.Invoke(delayedConsequence);
                    
                    processedCount++;
                    
                    Debug.Log($"[ConsequenceTrackingSystem] Triggered delayed consequence: {delayedConsequence.ConsequenceData.ConsequenceId}");
                }
                else
                {
                    break; // Queue is ordered by trigger time
                }
            }
        }
        
        private void UpdateActiveConsequences()
        {
            var consequencesToRemove = new List<string>();
            
            foreach (var kvp in _activeConsequences)
            {
                var activeConsequence = kvp.Value;
                var deltaTime = Time.deltaTime;
                
                // Update duration
                activeConsequence.RemainingDuration -= deltaTime;
                
                // Check if consequence should expire
                if (activeConsequence.RemainingDuration <= 0)
                {
                    // Apply end effects if any
                    ApplyConsequenceEndEffects(activeConsequence.ConsequenceData);
                    
                    consequencesToRemove.Add(kvp.Key);
                }
                else
                {
                    // Update ongoing effects
                    UpdateOngoingConsequenceEffects(activeConsequence, deltaTime);
                }
            }
            
            // Remove expired consequences
            foreach (var consequenceId in consequencesToRemove)
            {
                _activeConsequences.Remove(consequenceId);
            }
        }
        
        private void ApplyConsequenceEndEffects(Consequence consequence)
        {
            // Apply any effects that should occur when consequence ends
            Debug.Log($"[ConsequenceTrackingSystem] Consequence ended: {consequence.ConsequenceId}");
        }
        
        private void UpdateOngoingConsequenceEffects(ActiveConsequence activeConsequence, float deltaTime)
        {
            // Update ongoing effects for active consequences
            var consequence = activeConsequence.ConsequenceData;
            
            // Example: Gradually apply relationship changes over time
            if (consequence.Type == ConsequenceType.GradualRelationshipChange)
            {
                ApplyGradualRelationshipChanges(consequence, deltaTime);
            }
        }
        
        private void ApplyGradualRelationshipChanges(Consequence consequence, float deltaTime)
        {
            if (_relationshipSystem == null || consequence.RelationshipImpacts == null) return;
            
            foreach (var impact in consequence.RelationshipImpacts)
            {
                var characterId = impact.Key;
                var totalImpact = impact.Value;
                var gradualImpact = totalImpact * (deltaTime / consequence.Duration);
                
                var relationship = _relationshipSystem.GetRelationship(characterId);
                relationship?.ModifyTrust(gradualImpact * 0.4f);
                relationship?.ModifyRespect(gradualImpact * 0.4f);
                relationship?.ModifyInfluence(gradualImpact * 0.2f);
            }
        }
        
        private void ProcessRippleEffectDecay()
        {
            var chainsToRemove = new List<string>();
            
            foreach (var kvp in _rippleEffectChains)
            {
                var chainId = kvp.Key;
                var rippleEffects = kvp.Value;
                
                for (int i = rippleEffects.Count - 1; i >= 0; i--)
                {
                    var rippleEffect = rippleEffects[i];
                    rippleEffect.Intensity -= _rippleEffectDecayRate * Time.deltaTime;
                    
                    if (rippleEffect.Intensity <= 0f)
                    {
                        rippleEffects.RemoveAt(i);
                    }
                }
                
                if (rippleEffects.Count == 0)
                {
                    chainsToRemove.Add(chainId);
                }
            }
            
            // Remove empty chains
            foreach (var chainId in chainsToRemove)
            {
                _rippleEffectChains.Remove(chainId);
            }
        }
        
        private void ProcessBatchOperations()
        {
            // Process queued operations in batches for performance
            var processedCount = 0;
            
            while (_processingQueue.Count > 0 && processedCount < _maxConsequencesPerFrame)
            {
                var task = _processingQueue.Dequeue();
                ProcessConsequenceTask(task);
                processedCount++;
            }
        }
        
        private void ProcessConsequenceTask(ConsequenceProcessingTask task)
        {
            // Process individual consequence tasks
            switch (task.TaskType)
            {
                case ConsequenceTaskType.Apply:
                    ApplyConsequence(task.Consequence);
                    break;
                case ConsequenceTaskType.Validate:
                    ValidateConsequence(task.Consequence);
                    break;
                case ConsequenceTaskType.Predict:
                    PredictConsequenceOutcome(task.Consequence);
                    break;
            }
        }
        
        private void ValidateConsequence(Consequence consequence)
        {
            _consequenceValidator.ValidateConsequence(consequence);
        }
        
        private void PredictConsequenceOutcome(Consequence consequence)
        {
            _predictionEngine.PredictConsequenceOutcome(consequence);
        }
        
        private void CleanupCache()
        {
            _consequenceCache?.CleanupExpiredEntries();
        }
        
        private void UpdatePerformanceMetrics(float processingTime)
        {
            // Update rolling average
            var weight = 0.1f;
            _averageProcessingTime = (_averageProcessingTime * (1f - weight)) + (processingTime * weight);
            
            // Update performance metrics
            _performanceMetrics?.RecordProcessingTime(processingTime);
        }
        
        public void UpdateDelayedConsequences(float deltaTime)
        {
            // Public method for external system calls
            if (!IsInitialized) return;
            
            // Update is handled by MonoBehaviour Update method
        }
        
        public ConsequenceSystemStatistics GetStatistics()
        {
            return new ConsequenceSystemStatistics
            {
                TotalConsequencesProcessed = _totalConsequencesProcessed,
                ActiveConsequenceCount = _activeConsequences.Count,
                DelayedConsequenceCount = _delayedConsequences.Count,
                RippleEffectChainCount = _rippleEffectChains.Count,
                AverageProcessingTime = _averageProcessingTime,
                CachedConsequenceCount = _consequenceCache?.GetCacheSize() ?? 0,
                TemplateCount = _consequenceTemplates.Count
            };
        }
        
        public List<ActiveConsequence> GetActiveConsequences()
        {
            return _activeConsequences.Values.ToList();
        }
        
        public List<DelayedConsequence> GetDelayedConsequences()
        {
            return _delayedConsequences.ToList();
        }
        
        public void SetRelationshipSystem(CharacterRelationshipSystem relationshipSystem)
        {
            _relationshipSystem = relationshipSystem;
        }
        
        public void SetNarrativeEngine(BranchingNarrativeEngine narrativeEngine)
        {
            _narrativeEngine = narrativeEngine;
        }
        
        private void OnDestroy()
        {
            // Cleanup resources
            _activeConsequences?.Clear();
            _delayedConsequences?.Clear();
            _rippleEffectChains?.Clear();
            _processingQueue?.Clear();
            _consequenceCache?.Dispose();
        }
    }
    
    // Supporting data structures
    [Serializable]
    public class ActiveConsequence
    {
        public Consequence ConsequenceData;
        public float ActivationTime;
        public bool IsActive;
        public float RemainingDuration;
    }
    
    [Serializable]
    public class DelayedConsequence
    {
        public Consequence ConsequenceData;
        public float TriggerTime;
        public bool IsQueued;
    }
    
    [Serializable]
    public class RippleEffect
    {
        public string RippleId;
        public string SourceConsequenceId;
        public ConsequenceType ConsequenceType;
        public string Description;
        public ConsequenceSeverity Severity;
        public float Intensity;
        public float DelayTime;
        public float Duration;
        public float Desirability;
        public Dictionary<string, float> RelationshipImpacts = new Dictionary<string, float>();
        public DateTime Timestamp;
    }
    
    
    [Serializable]
    public class ConsequenceProcessingTask
    {
        public ConsequenceTaskType TaskType;
        public Consequence Consequence;
        public DateTime QueueTime;
        public float Priority;
    }
    
    [Serializable]
    public class ConsequenceSystemStatistics
    {
        public int TotalConsequencesProcessed;
        public int ActiveConsequenceCount;
        public int DelayedConsequenceCount;
        public int RippleEffectChainCount;
        public float AverageProcessingTime;
        public int CachedConsequenceCount;
        public int TemplateCount;
    }
    
    [Serializable]
    public class ConsequencePerformanceMetrics
    {
        public float AverageProcessingTime;
        public float PeakProcessingTime;
        public int TotalProcessed;
        
        public void Update(float deltaTime)
        {
            TotalProcessed++;
        }
        
        public void RecordProcessingTime(float time)
        {
            PeakProcessingTime = Mathf.Max(PeakProcessingTime, time);
            
            // Update rolling average
            var weight = 0.1f;
            AverageProcessingTime = (AverageProcessingTime * (1f - weight)) + (time * weight);
        }
    }
    
    // Enums
    public enum ConsequenceTaskType
    {
        Apply,
        Validate,
        Predict,
        Cache
    }
    
    // Placeholder classes for compilation
    public class ConsequencePredictionEngine
    {
        public void Initialize(CampaignConfigSO config) { }
        public List<Consequence> GenerateDynamicConsequences(string choiceId) => new List<Consequence>();
        public void PredictConsequenceOutcome(Consequence consequence) { }
    }
    
    public class RippleEffectAnalyzer
    {
        public void Initialize(CampaignConfigSO config) { }
        public List<RippleEffect> GenerateRippleEffects(Consequence consequence) => new List<RippleEffect>();
    }
    
    public class EducationalImpactTracker
    {
        public void Initialize(CampaignConfigSO config) { }
        public void TrackEducationalConsequence(EducationalConsequence consequence) { }
    }
    
    public class ConsequenceValidator
    {
        public void Initialize(CharacterDatabaseSO database, CampaignConfigSO config) { }
        public bool ValidateConsequence(Consequence consequence) => true;
    }
    
    public class ConsequenceCache
    {
        private Dictionary<string, List<Consequence>> _cache = new Dictionary<string, List<Consequence>>();
        
        public void Initialize(int maxEntries) { }
        public bool TryGetCachedConsequences(string choiceId, out List<Consequence> consequences)
        {
            return _cache.TryGetValue(choiceId, out consequences);
        }
        public void CacheConsequences(string choiceId, List<Consequence> consequences)
        {
            _cache[choiceId] = consequences;
        }
        public void CleanupExpiredEntries() { }
        public int GetCacheSize() => _cache.Count;
        public void Dispose() => _cache.Clear();
    }
}