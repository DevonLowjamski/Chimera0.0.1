using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Economy;

namespace ProjectChimera.Systems.Economy
{
    /// <summary>
    /// Manages business relationships, reputation systems, and NPC behavioral modeling
    /// for the cannabis industry simulation. Handles complex relationship dynamics,
    /// reputation tracking, and AI-driven NPC interactions.
    /// </summary>
    public class NPCRelationshipManager : ChimeraManager
    {
        [Header("Relationship Configuration")]
        [SerializeField] private List<NPCProfileSO> _allNPCs = new List<NPCProfileSO>();
        [SerializeField] private PlayerReputation _playerReputation;
        [SerializeField] private RelationshipSettings _relationshipSettings;
        [SerializeField] private float _relationshipUpdateInterval = 1f; // In-game days
        
        [Header("Reputation System")]
        [SerializeField] private ReputationDecaySettings _reputationDecay;
        [SerializeField] private ReputationThresholds _reputationThresholds;
        [SerializeField] private List<ReputationModifier> _activeModifiers = new List<ReputationModifier>();
        
        [Header("Behavioral Modeling")]
        [SerializeField] private NPCBehaviorSettings _behaviorSettings;
        [SerializeField] private List<IndustryEvent> _recentIndustryEvents = new List<IndustryEvent>();
        
        [Header("Communication System")]
        [SerializeField] private CommunicationSettings _communicationSettings;
        [SerializeField] private List<NPCMessage> _pendingMessages = new List<NPCMessage>();
        [SerializeField] private List<NPCMessage> _messageHistory = new List<NPCMessage>();
        
        [Header("Events")]
        [SerializeField] private SimpleGameEventSO _relationshipChangedEvent;
        [SerializeField] private SimpleGameEventSO _reputationChangedEvent;
        [SerializeField] private SimpleGameEventSO _messageReceivedEvent;
        [SerializeField] private SimpleGameEventSO _industryEventEvent;
        
        // Runtime Data
        private Dictionary<NPCProfileSO, NPCRelationshipState> _npcRelationships;
        private Dictionary<NPCProfileSO, NPCBehaviorState> _npcBehaviors;
        private Queue<RelationshipEvent> _recentEvents;
        private float _timeSinceLastUpdate;
        private IndustryReputation _industryReputation;
        
        public PlayerReputation PlayerReputation => _playerReputation;
        public IndustryReputation IndustryReputation => _industryReputation;
        public List<NPCMessage> PendingMessages => _pendingMessages;
        
        // Events
        public System.Action<NPCProfileSO, float, float> OnRelationshipChanged; // npc, oldLevel, newLevel
        public System.Action<ReputationCategory, float, float> OnReputationChanged; // category, oldValue, newValue
        public System.Action<NPCMessage> OnMessageReceived;
        public System.Action<IndustryEvent> OnIndustryEventOccurred;
        
        protected override void OnManagerInitialize()
        {
            _npcRelationships = new Dictionary<NPCProfileSO, NPCRelationshipState>();
            _npcBehaviors = new Dictionary<NPCProfileSO, NPCBehaviorState>();
            _recentEvents = new Queue<RelationshipEvent>();
            _industryReputation = new IndustryReputation();
            
            InitializeNPCRelationships();
            InitializeNPCBehaviors();
            InitializePlayerReputation();
            
            Debug.Log("NPCRelationshipManager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Cleanup resources
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized) return;
            
            _timeSinceLastUpdate += Time.deltaTime;
            
            float gameTimeDelta = GameManager.Instance.GetManager<TimeManager>().GetScaledDeltaTime();
            
            if (_timeSinceLastUpdate >= _relationshipUpdateInterval * gameTimeDelta)
            {
                UpdateRelationshipStates();
                UpdateNPCBehaviors();
                ProcessReputationDecay();
                ProcessCommunications();
                ProcessIndustryEvents();
                GenerateNPCInteractions();
                
                _timeSinceLastUpdate = 0f;
            }
        }
        
        /// <summary>
        /// Gets the current relationship level with a specific NPC.
        /// </summary>
        public float GetRelationshipLevel(NPCProfileSO npc)
        {
            if (_npcRelationships.ContainsKey(npc))
            {
                return _npcRelationships[npc].CurrentTrustLevel;
            }
            return 0.5f; // Neutral default
        }
        
        /// <summary>
        /// Gets the relationship state with a specific NPC.
        /// </summary>
        public NPCRelationshipState GetRelationshipState(NPCProfileSO npc)
        {
            return _npcRelationships.ContainsKey(npc) ? _npcRelationships[npc] : null;
        }
        
        /// <summary>
        /// Records a business interaction with an NPC.
        /// </summary>
        public void RecordInteraction(NPCProfileSO npc, InteractionType interactionType, float qualityScore, float financialValue)
        {
            if (!_npcRelationships.ContainsKey(npc)) return;
            
            var relationship = _npcRelationships[npc];
            var interaction = new BusinessInteraction
            {
                NPC = npc,
                InteractionType = interactionType,
                QualityScore = qualityScore,
                FinancialValue = financialValue,
                Timestamp = System.DateTime.Now,
                ReputationImpact = CalculateReputationImpact(interactionType, qualityScore, financialValue)
            };
            
            relationship.InteractionHistory.Add(interaction);
            
            // Update relationship based on interaction
            UpdateRelationshipFromInteraction(npc, interaction);
            
            // Update player reputation
            UpdatePlayerReputationFromInteraction(interaction);
            
            // Generate potential response from NPC
            ConsiderNPCResponse(npc, interaction);
        }
        
        /// <summary>
        /// Sends a communication to an NPC.
        /// </summary>
        public void SendCommunication(NPCProfileSO npc, CommunicationType communicationType, string content, CommunicationUrgency urgency = CommunicationUrgency.Normal)
        {
            var communication = new NPCCommunication
            {
                FromPlayer = true,
                ToNPC = npc,
                CommunicationType = communicationType,
                Content = content,
                Urgency = urgency,
                Timestamp = System.DateTime.Now,
                RequiresResponse = true
            };
            
            ProcessCommunication(communication);
            
            // NPC may respond based on relationship and personality
            ScheduleNPCResponse(npc, communication);
        }
        
        /// <summary>
        /// Gets all NPCs with relationships above a certain threshold.
        /// </summary>
        public List<NPCProfileSO> GetAlliesAboveThreshold(float threshold = 0.7f)
        {
            return _npcRelationships
                .Where(kvp => kvp.Value.CurrentTrustLevel >= threshold)
                .Select(kvp => kvp.Key)
                .ToList();
        }
        
        /// <summary>
        /// Gets all NPCs by industry role.
        /// </summary>
        public List<NPCProfileSO> GetNPCsByRole(IndustryRole role)
        {
            return _allNPCs.Where(npc => npc.IndustryRole == role).ToList();
        }
        
        /// <summary>
        /// Gets reputation score in a specific category.
        /// </summary>
        public float GetReputationScore(ReputationCategory category)
        {
            switch (category)
            {
                case ReputationCategory.Quality: return _playerReputation.QualityScore;
                case ReputationCategory.Reliability: return _playerReputation.ReliabilityScore;
                case ReputationCategory.Innovation: return _playerReputation.InnovationScore;
                case ReputationCategory.Professionalism: return _playerReputation.ProfessionalismScore;
                case ReputationCategory.Compliance: return _playerReputation.ComplianceScore;
                default: return _playerReputation.OverallScore;
            }
        }
        
        /// <summary>
        /// Triggers an industry-wide event that affects relationships.
        /// </summary>
        public void TriggerIndustryEvent(IndustryEventType eventType, float intensity, string description)
        {
            var industryEvent = new IndustryEvent
            {
                EventType = eventType,
                Intensity = intensity,
                Description = description,
                Timestamp = System.DateTime.Now,
                Duration = CalculateEventDuration(eventType),
                AffectedRoles = GetAffectedRoles(eventType)
            };
            
            _recentIndustryEvents.Add(industryEvent);
            ProcessIndustryEvent(industryEvent);
            
            OnIndustryEventOccurred?.Invoke(industryEvent);
            _industryEventEvent?.Raise();
        }
        
        /// <summary>
        /// Gets NPCs who might be interested in a specific business opportunity.
        /// </summary>
        public List<NPCProfileSO> GetInterestedNPCs(BusinessOpportunity opportunity)
        {
            var interestedNPCs = new List<NPCProfileSO>();
            
            foreach (var npc in _allNPCs)
            {
                if (EvaluateNPCInterest(npc, opportunity))
                {
                    interestedNPCs.Add(npc);
                }
            }
            
            // Sort by relationship level
            return interestedNPCs.OrderByDescending(npc => GetRelationshipLevel(npc)).ToList();
        }
        
        private void InitializeNPCRelationships()
        {
            foreach (var npc in _allNPCs)
            {
                var relationshipState = new NPCRelationshipState
                {
                    NPC = npc,
                    CurrentTrustLevel = Random.Range(0.3f, 0.7f),
                    ReputationView = new NPCReputationView
                    {
                        QualityPerception = Random.Range(0.4f, 0.6f),
                        ReliabilityPerception = Random.Range(0.4f, 0.6f),
                        ProfessionalismPerception = Random.Range(0.4f, 0.6f)
                    },
                    LastInteractionDate = System.DateTime.Now.AddDays(-Random.Range(1, 30)),
                    InteractionHistory = new List<BusinessInteraction>(),
                    CurrentIssues = new List<RelationshipIssue>(),
                    CommunicationPreferences = InitializeCommunicationPreferences(npc)
                };
                
                _npcRelationships[npc] = relationshipState;
            }
        }
        
        private void InitializeNPCBehaviors()
        {
            foreach (var npc in _allNPCs)
            {
                var behaviorState = new NPCBehaviorState
                {
                    NPC = npc,
                    CurrentMood = Random.Range(0.3f, 0.8f),
                    StressLevel = Random.Range(0.1f, 0.5f),
                    BusinessPressure = Random.Range(0.2f, 0.7f),
                    MarketOutlook = Random.Range(0.4f, 0.8f),
                    RecentPerformance = Random.Range(0.5f, 0.9f),
                    DecisionMakingState = DecisionMakingState.Normal,
                    ActiveConcerns = new List<NPCConcern>(),
                    PendingDecisions = new List<NPCDecision>()
                };
                
                _npcBehaviors[npc] = behaviorState;
            }
        }
        
        private void InitializePlayerReputation()
        {
            if (_playerReputation == null)
            {
                _playerReputation = new PlayerReputation
                {
                    QualityScore = 0.5f,
                    ReliabilityScore = 0.5f,
                    InnovationScore = 0.5f,
                    ProfessionalismScore = 0.5f,
                    ComplianceScore = 0.5f
                };
            }
            
            UpdateOverallReputation();
        }
        
        private void UpdateRelationshipStates()
        {
            foreach (var kvp in _npcRelationships.ToList())
            {
                var npc = kvp.Key;
                var relationship = kvp.Value;
                
                // Apply natural relationship decay
                ApplyRelationshipDecay(relationship);
                
                // Update based on recent events
                UpdateRelationshipFromEvents(npc, relationship);
                
                // Update NPC's perception of player
                UpdateNPCPerceptions(npc, relationship);
                
                // Process any ongoing issues
                ProcessRelationshipIssues(npc, relationship);
            }
        }
        
        private void UpdateNPCBehaviors()
        {
            foreach (var kvp in _npcBehaviors.ToList())
            {
                var npc = kvp.Key;
                var behavior = kvp.Value;
                
                // Update mood and stress based on business performance
                UpdateNPCMoodAndStress(npc, behavior);
                
                // Update market outlook
                UpdateMarketOutlook(npc, behavior);
                
                // Process pending decisions
                ProcessNPCDecisions(npc, behavior);
                
                // Generate new concerns or opportunities
                GenerateNPCConcerns(npc, behavior);
            }
        }
        
        private void ProcessReputationDecay()
        {
            bool reputationChanged = false;
            
            // Apply decay to all reputation categories
            float oldQuality = _playerReputation.QualityScore;
            float oldReliability = _playerReputation.ReliabilityScore;
            float oldInnovation = _playerReputation.InnovationScore;
            float oldProfessionalism = _playerReputation.ProfessionalismScore;
            float oldCompliance = _playerReputation.ComplianceScore;
            
            _playerReputation.QualityScore = ApplyDecay(_playerReputation.QualityScore, _reputationDecay.QualityDecayRate);
            _playerReputation.ReliabilityScore = ApplyDecay(_playerReputation.ReliabilityScore, _reputationDecay.ReliabilityDecayRate);
            _playerReputation.InnovationScore = ApplyDecay(_playerReputation.InnovationScore, _reputationDecay.InnovationDecayRate);
            _playerReputation.ProfessionalismScore = ApplyDecay(_playerReputation.ProfessionalismScore, _reputationDecay.ProfessionalismDecayRate);
            _playerReputation.ComplianceScore = ApplyDecay(_playerReputation.ComplianceScore, _reputationDecay.ComplianceDecayRate);
            
            // Check for changes and fire events
            if (Mathf.Abs(oldQuality - _playerReputation.QualityScore) > 0.001f)
            {
                OnReputationChanged?.Invoke(ReputationCategory.Quality, oldQuality, _playerReputation.QualityScore);
                reputationChanged = true;
            }
            
            if (reputationChanged)
            {
                UpdateOverallReputation();
                _reputationChangedEvent?.Raise();
            }
        }
        
        private void ProcessCommunications()
        {
            // Process pending messages
            for (int i = _pendingMessages.Count - 1; i >= 0; i--)
            {
                var message = _pendingMessages[i];
                
                if (System.DateTime.Now >= message.ScheduledDelivery)
                {
                    DeliverMessage(message);
                    _pendingMessages.RemoveAt(i);
                }
            }
            
            // Clean up old message history
            var cutoffDate = System.DateTime.Now.AddDays(-30);
            _messageHistory.RemoveAll(msg => msg.Timestamp < cutoffDate);
        }
        
        private void ProcessIndustryEvents()
        {
            for (int i = _recentIndustryEvents.Count - 1; i >= 0; i--)
            {
                var industryEvent = _recentIndustryEvents[i];
                
                // Remove expired events
                if ((System.DateTime.Now - industryEvent.Timestamp).TotalDays > industryEvent.Duration)
                {
                    _recentIndustryEvents.RemoveAt(i);
                    continue;
                }
                
                // Continue applying event effects
                ApplyOngoingEventEffects(industryEvent);
            }
        }
        
        private void GenerateNPCInteractions()
        {
            // Randomly generate NPC-initiated interactions
            if (Random.Range(0f, 1f) < _behaviorSettings.BaseInteractionRate)
            {
                var randomNPC = _allNPCs[Random.Range(0, _allNPCs.Count)];
                GenerateRandomInteraction(randomNPC);
            }
        }
        
        private void UpdateRelationshipFromInteraction(NPCProfileSO npc, BusinessInteraction interaction)
        {
            if (!_npcRelationships.ContainsKey(npc)) return;
            
            var relationship = _npcRelationships[npc];
            float oldTrustLevel = relationship.CurrentTrustLevel;
            
            // Calculate trust change based on interaction
            float trustChange = CalculateTrustChange(npc, interaction);
            relationship.CurrentTrustLevel = Mathf.Clamp01(relationship.CurrentTrustLevel + trustChange);
            
            // Update last interaction date
            relationship.LastInteractionDate = interaction.Timestamp;
            
            // Fire event if trust changed significantly
            if (Mathf.Abs(oldTrustLevel - relationship.CurrentTrustLevel) > 0.05f)
            {
                OnRelationshipChanged?.Invoke(npc, oldTrustLevel, relationship.CurrentTrustLevel);
                _relationshipChangedEvent?.Raise();
            }
        }
        
        private void UpdatePlayerReputationFromInteraction(BusinessInteraction interaction)
        {
            float impact = interaction.ReputationImpact;
            
            switch (interaction.InteractionType)
            {
                case InteractionType.Contract_Delivery:
                    _playerReputation.QualityScore += impact * interaction.QualityScore;
                    _playerReputation.ReliabilityScore += impact * 0.5f;
                    break;
                case InteractionType.Payment:
                    _playerReputation.ReliabilityScore += impact;
                    _playerReputation.ProfessionalismScore += impact * 0.3f;
                    break;
                case InteractionType.Dispute_Resolution:
                    _playerReputation.ProfessionalismScore += impact;
                    break;
                case InteractionType.Innovation_Collaboration:
                    _playerReputation.InnovationScore += impact;
                    break;
            }
            
            // Clamp all values
            _playerReputation.QualityScore = Mathf.Clamp01(_playerReputation.QualityScore);
            _playerReputation.ReliabilityScore = Mathf.Clamp01(_playerReputation.ReliabilityScore);
            _playerReputation.InnovationScore = Mathf.Clamp01(_playerReputation.InnovationScore);
            _playerReputation.ProfessionalismScore = Mathf.Clamp01(_playerReputation.ProfessionalismScore);
            _playerReputation.ComplianceScore = Mathf.Clamp01(_playerReputation.ComplianceScore);
            
            UpdateOverallReputation();
        }
        
        private void ConsiderNPCResponse(NPCProfileSO npc, BusinessInteraction interaction)
        {
            if (!_npcBehaviors.ContainsKey(npc)) return;
            
            var behavior = _npcBehaviors[npc];
            
            // Check if NPC wants to respond based on personality and interaction
            float responseChance = CalculateResponseChance(npc, interaction);
            
            if (Random.Range(0f, 1f) < responseChance)
            {
                GenerateNPCResponse(npc, interaction);
            }
        }
        
        private void ScheduleNPCResponse(NPCProfileSO npc, NPCCommunication communication)
        {
            if (!_npcBehaviors.ContainsKey(npc)) return;
            
            var behavior = _npcBehaviors[npc];
            
            // Calculate response time based on NPC personality and relationship
            float responseDelayHours = CalculateResponseDelay(npc, communication);
            
            var responseMessage = new NPCMessage
            {
                FromNPC = npc,
                MessageType = NPCMessageType.Response,
                Content = GenerateResponseContent(npc, communication),
                Urgency = communication.Urgency,
                ScheduledDelivery = System.DateTime.Now.AddHours(responseDelayHours),
                RelatedCommunication = communication
            };
            
            _pendingMessages.Add(responseMessage);
        }
        
        private float CalculateTrustChange(NPCProfileSO npc, BusinessInteraction interaction)
        {
            float baseChange = 0f;
            
            switch (interaction.InteractionType)
            {
                case InteractionType.Contract_Delivery:
                    baseChange = (interaction.QualityScore - 0.7f) * 0.1f; // Quality above 0.7 improves trust
                    break;
                case InteractionType.Payment:
                    baseChange = 0.02f; // Reliable payments always improve trust
                    break;
                case InteractionType.Dispute_Resolution:
                    baseChange = interaction.QualityScore > 0.7f ? 0.05f : -0.05f;
                    break;
                case InteractionType.Communication:
                    baseChange = 0.01f; // Good communication builds trust slowly
                    break;
                case InteractionType.Breach_Of_Contract:
                    baseChange = -0.15f; // Major trust damage
                    break;
            }
            
            // Apply NPC personality modifiers
            if (npc.BehavioralProfile.Patience < 0.5f)
                baseChange *= 1.5f; // Impatient NPCs react more strongly
            
            if (npc.RelationshipProfile.TrustBuildingRate > 1f)
                baseChange *= npc.RelationshipProfile.TrustBuildingRate;
            
            return baseChange;
        }
        
        private float CalculateReputationImpact(InteractionType interactionType, float qualityScore, float financialValue)
        {
            float impact = 0.01f; // Base impact
            
            // Scale by financial value (larger deals have more impact)
            float valueMultiplier = Mathf.Min(financialValue / 10000f, 3f); // Cap at 3x
            impact *= valueMultiplier;
            
            // Quality affects impact magnitude
            if (qualityScore > 0.8f)
                impact *= 1.5f;
            else if (qualityScore < 0.6f)
                impact *= -1f; // Negative impact for poor quality
            
            return impact;
        }
        
        private void ApplyRelationshipDecay(NPCRelationshipState relationship)
        {
            float daysSinceInteraction = (float)(System.DateTime.Now - relationship.LastInteractionDate).TotalDays;
            
            if (daysSinceInteraction > 7) // After a week
            {
                float decayRate = _relationshipSettings.BaseDecayRate;
                
                // NPCs with high loyalty tendency decay slower
                if (relationship.NPC.RelationshipProfile.LoyaltyTendency > 0.5f)
                    decayRate *= 0.5f;
                
                float decay = decayRate * (daysSinceInteraction / 30f); // Normalize to monthly decay
                relationship.CurrentTrustLevel = Mathf.Max(0.1f, relationship.CurrentTrustLevel - decay);
            }
        }
        
        private void UpdateRelationshipFromEvents(NPCProfileSO npc, NPCRelationshipState relationship)
        {
            // Industry events can affect relationships
            foreach (var industryEvent in _recentIndustryEvents)
            {
                if (industryEvent.AffectedRoles.Contains(npc.IndustryRole))
                {
                    float eventImpact = CalculateEventRelationshipImpact(industryEvent, npc);
                    relationship.CurrentTrustLevel = Mathf.Clamp01(relationship.CurrentTrustLevel + eventImpact);
                }
            }
        }
        
        private void UpdateNPCPerceptions(NPCProfileSO npc, NPCRelationshipState relationship)
        {
            // NPCs update their perception of player based on overall reputation
            float convergenceRate = 0.05f; // How quickly NPC perceptions align with actual reputation
            
            relationship.ReputationView.QualityPerception = Mathf.Lerp(
                relationship.ReputationView.QualityPerception,
                _playerReputation.QualityScore,
                convergenceRate
            );
            
            relationship.ReputationView.ReliabilityPerception = Mathf.Lerp(
                relationship.ReputationView.ReliabilityPerception,
                _playerReputation.ReliabilityScore,
                convergenceRate
            );
            
            relationship.ReputationView.ProfessionalismPerception = Mathf.Lerp(
                relationship.ReputationView.ProfessionalismPerception,
                _playerReputation.ProfessionalismScore,
                convergenceRate
            );
        }
        
        private void ProcessRelationshipIssues(NPCProfileSO npc, NPCRelationshipState relationship)
        {
            for (int i = relationship.CurrentIssues.Count - 1; i >= 0; i--)
            {
                var issue = relationship.CurrentIssues[i];
                issue.TimeToResolve -= _relationshipUpdateInterval;
                
                if (issue.TimeToResolve <= 0)
                {
                    // Issue resolved automatically over time
                    relationship.CurrentTrustLevel += issue.ResolutionBenefit;
                    relationship.CurrentIssues.RemoveAt(i);
                }
            }
        }
        
        private void UpdateNPCMoodAndStress(NPCProfileSO npc, NPCBehaviorState behavior)
        {
            // Business performance affects mood
            if (behavior.RecentPerformance > 0.8f)
                behavior.CurrentMood = Mathf.Min(1f, behavior.CurrentMood + 0.02f);
            else if (behavior.RecentPerformance < 0.4f)
                behavior.CurrentMood = Mathf.Max(0f, behavior.CurrentMood - 0.02f);
            
            // Market conditions affect stress
            var marketManager = GameManager.Instance.GetManager<MarketManager>();
            if (marketManager != null)
            {
                float marketHealth = marketManager.CurrentMarketConditions.EconomicHealth;
                if (marketHealth < 0.5f)
                    behavior.StressLevel = Mathf.Min(1f, behavior.StressLevel + 0.01f);
                else if (marketHealth > 0.8f)
                    behavior.StressLevel = Mathf.Max(0f, behavior.StressLevel - 0.01f);
            }
        }
        
        private void UpdateMarketOutlook(NPCProfileSO npc, NPCBehaviorState behavior)
        {
            // Market outlook based on industry events and personal experience
            float outlookChange = 0f;
            
            foreach (var industryEvent in _recentIndustryEvents)
            {
                if (industryEvent.AffectedRoles.Contains(npc.IndustryRole))
                {
                    switch (industryEvent.EventType)
                    {
                        case IndustryEventType.Market_Growth:
                            outlookChange += 0.05f;
                            break;
                        case IndustryEventType.Regulatory_Crackdown:
                            outlookChange -= 0.1f;
                            break;
                        case IndustryEventType.Technology_Breakthrough:
                            outlookChange += 0.03f;
                            break;
                    }
                }
            }
            
            behavior.MarketOutlook = Mathf.Clamp01(behavior.MarketOutlook + outlookChange);
        }
        
        private void ProcessNPCDecisions(NPCProfileSO npc, NPCBehaviorState behavior)
        {
            for (int i = behavior.PendingDecisions.Count - 1; i >= 0; i--)
            {
                var decision = behavior.PendingDecisions[i];
                decision.TimeToDecide -= _relationshipUpdateInterval;
                
                if (decision.TimeToDecide <= 0)
                {
                    // Make decision based on NPC personality and current state
                    bool decisionMade = MakeNPCDecision(npc, decision);
                    behavior.PendingDecisions.RemoveAt(i);
                    
                    if (decisionMade)
                    {
                        ApplyDecisionConsequences(npc, decision);
                    }
                }
            }
        }
        
        private void GenerateNPCConcerns(NPCProfileSO npc, NPCBehaviorState behavior)
        {
            // Generate concerns based on current state
            if (behavior.StressLevel > 0.7f && Random.Range(0f, 1f) < 0.1f)
            {
                var concern = new NPCConcern
                {
                    ConcernType = NPCConcernType.Business_Pressure,
                    Description = "High stress levels affecting business decisions",
                    Intensity = behavior.StressLevel,
                    Duration = Random.Range(5f, 15f)
                };
                
                behavior.ActiveConcerns.Add(concern);
            }
        }
        
        private float ApplyDecay(float currentValue, float decayRate)
        {
            return Mathf.Max(0.1f, currentValue - (decayRate * _relationshipUpdateInterval));
        }
        
        private void UpdateOverallReputation()
        {
            // OverallScore is read-only, calculated automatically by PlayerReputation
            // No need to manually set it
        }
        
        private CommunicationPreferences InitializeCommunicationPreferences(NPCProfileSO npc)
        {
            return new CommunicationPreferences
            {
                PreferredMethod = (CommunicationType)Random.Range(0, System.Enum.GetValues(typeof(CommunicationType)).Length),
                ResponseTimeExpectation = Random.Range(1f, 48f), // 1-48 hours
                FormalityLevel = Random.Range(0.3f, 1f),
                PrefersBrief = npc.CommunicationStyle == CommunicationStyle.Direct
            };
        }
        
        private float CalculateEventDuration(IndustryEventType eventType)
        {
            switch (eventType)
            {
                case IndustryEventType.Regulatory_Crackdown: return Random.Range(30f, 90f);
                case IndustryEventType.Market_Growth: return Random.Range(60f, 180f);
                case IndustryEventType.Technology_Breakthrough: return Random.Range(90f, 365f);
                case IndustryEventType.Supply_Shortage: return Random.Range(14f, 60f);
                default: return Random.Range(7f, 30f);
            }
        }
        
        private List<IndustryRole> GetAffectedRoles(IndustryEventType eventType)
        {
            switch (eventType)
            {
                case IndustryEventType.Regulatory_Crackdown:
                    return new List<IndustryRole> { IndustryRole.Grower, IndustryRole.Processor, IndustryRole.Distributor };
                case IndustryEventType.Technology_Breakthrough:
                    return new List<IndustryRole> { IndustryRole.Grower, IndustryRole.Technology_Provider };
                case IndustryEventType.Supply_Shortage:
                    return new List<IndustryRole> { IndustryRole.Grower, IndustryRole.Distributor };
                default:
                    return new List<IndustryRole> { IndustryRole.Grower };
            }
        }
        
        private void ProcessIndustryEvent(IndustryEvent industryEvent)
        {
            // Apply event effects to all affected NPCs
            foreach (var npc in _allNPCs)
            {
                if (industryEvent.AffectedRoles.Contains(npc.IndustryRole))
                {
                    ApplyEventEffectsToNPC(npc, industryEvent);
                }
            }
        }
        
        private void ApplyEventEffectsToNPC(NPCProfileSO npc, IndustryEvent industryEvent)
        {
            if (!_npcBehaviors.ContainsKey(npc)) return;
            
            var behavior = _npcBehaviors[npc];
            
            switch (industryEvent.EventType)
            {
                case IndustryEventType.Regulatory_Crackdown:
                    behavior.StressLevel += industryEvent.Intensity * 0.3f;
                    behavior.MarketOutlook -= industryEvent.Intensity * 0.2f;
                    break;
                case IndustryEventType.Market_Growth:
                    behavior.CurrentMood += industryEvent.Intensity * 0.2f;
                    behavior.MarketOutlook += industryEvent.Intensity * 0.3f;
                    break;
                case IndustryEventType.Supply_Shortage:
                    behavior.BusinessPressure += industryEvent.Intensity * 0.4f;
                    break;
            }
            
            // Clamp values
            behavior.StressLevel = Mathf.Clamp01(behavior.StressLevel);
            behavior.CurrentMood = Mathf.Clamp01(behavior.CurrentMood);
            behavior.MarketOutlook = Mathf.Clamp01(behavior.MarketOutlook);
            behavior.BusinessPressure = Mathf.Clamp01(behavior.BusinessPressure);
        }
        
        private void ApplyOngoingEventEffects(IndustryEvent industryEvent)
        {
            // Continuous effects while event is active
            float dailyEffect = industryEvent.Intensity * 0.01f; // Small daily effect
            
            foreach (var npc in _allNPCs)
            {
                if (industryEvent.AffectedRoles.Contains(npc.IndustryRole) && _npcBehaviors.ContainsKey(npc))
                {
                    var behavior = _npcBehaviors[npc];
                    
                    switch (industryEvent.EventType)
                    {
                        case IndustryEventType.Regulatory_Crackdown:
                            behavior.StressLevel = Mathf.Min(1f, behavior.StressLevel + dailyEffect);
                            break;
                        case IndustryEventType.Market_Growth:
                            behavior.CurrentMood = Mathf.Min(1f, behavior.CurrentMood + dailyEffect);
                            break;
                    }
                }
            }
        }
        
        private void GenerateRandomInteraction(NPCProfileSO npc)
        {
            if (!_npcBehaviors.ContainsKey(npc)) return;
            
            var behavior = _npcBehaviors[npc];
            
            // Generate different types of interactions based on NPC state
            if (behavior.StressLevel > 0.6f)
            {
                // Stressed NPCs might reach out for help or express concerns
                GenerateStressedNPCCommunication(npc);
            }
            else if (behavior.CurrentMood > 0.8f)
            {
                // Happy NPCs might offer opportunities
                GenerateOpportunisticCommunication(npc);
            }
            else if (Random.Range(0f, 1f) < 0.3f)
            {
                // General business communication
                GenerateRoutineCommunication(npc);
            }
        }
        
        private void GenerateStressedNPCCommunication(NPCProfileSO npc)
        {
            var message = new NPCMessage
            {
                FromNPC = npc,
                MessageType = NPCMessageType.Concern,
                Content = "I'm facing some challenges in my business. Would you be interested in discussing potential collaboration?",
                Urgency = CommunicationUrgency.High,
                ScheduledDelivery = System.DateTime.Now,
                RequiresResponse = true
            };
            
            _pendingMessages.Add(message);
        }
        
        private void GenerateOpportunisticCommunication(NPCProfileSO npc)
        {
            var message = new NPCMessage
            {
                FromNPC = npc,
                MessageType = NPCMessageType.Opportunity,
                Content = "I have an exciting opportunity that might interest you. Let's talk!",
                Urgency = CommunicationUrgency.Normal,
                ScheduledDelivery = System.DateTime.Now,
                RequiresResponse = false
            };
            
            _pendingMessages.Add(message);
        }
        
        private void GenerateRoutineCommunication(NPCProfileSO npc)
        {
            var message = new NPCMessage
            {
                FromNPC = npc,
                MessageType = NPCMessageType.General,
                Content = "Hope business is going well. Let me know if you need anything.",
                Urgency = CommunicationUrgency.Low,
                ScheduledDelivery = System.DateTime.Now,
                RequiresResponse = false
            };
            
            _pendingMessages.Add(message);
        }
        
        private bool EvaluateNPCInterest(NPCProfileSO npc, BusinessOpportunity opportunity)
        {
            // Check if NPC's business interests align with opportunity
            if (!opportunity.RelevantRoles.Contains(npc.IndustryRole))
                return false;
            
            // Check financial capacity
            if (opportunity.RequiredInvestment > npc.FinancialProfile.MaxContractValue)
                return false;
            
            // Check relationship level
            float relationshipLevel = GetRelationshipLevel(npc);
            if (relationshipLevel < opportunity.MinimumRelationshipLevel)
                return false;
            
            return true;
        }
        
        private float CalculateResponseChance(NPCProfileSO npc, BusinessInteraction interaction)
        {
            float baseChance = 0.3f;
            
            // Higher relationship = more likely to respond
            float relationshipLevel = GetRelationshipLevel(npc);
            baseChance += relationshipLevel * 0.3f;
            
            // Significant interactions more likely to get response
            if (interaction.FinancialValue > 10000f)
                baseChance += 0.2f;
            
            // NPC personality affects response likelihood
            if (npc.BehavioralProfile.IsProactive)
                baseChance += 0.2f;
            
            return Mathf.Clamp01(baseChance);
        }
        
        private void GenerateNPCResponse(NPCProfileSO npc, BusinessInteraction interaction)
        {
            NPCMessageType messageType = NPCMessageType.General;
            string content = "Thank you for the recent business.";
            
            if (interaction.QualityScore > 0.8f)
            {
                messageType = NPCMessageType.Praise;
                content = "Excellent quality on that last delivery! I'm impressed.";
            }
            else if (interaction.QualityScore < 0.6f)
            {
                messageType = NPCMessageType.Concern;
                content = "I have some concerns about the quality of the recent delivery. Can we discuss?";
            }
            
            var message = new NPCMessage
            {
                FromNPC = npc,
                MessageType = messageType,
                Content = content,
                Urgency = CommunicationUrgency.Normal,
                ScheduledDelivery = System.DateTime.Now.AddHours(Random.Range(1f, 24f)),
                RequiresResponse = messageType == NPCMessageType.Concern
            };
            
            _pendingMessages.Add(message);
        }
        
        private float CalculateResponseDelay(NPCProfileSO npc, NPCCommunication communication)
        {
            float baseDelay = 4f; // 4 hours default
            
            // Urgent messages get faster response
            switch (communication.Urgency)
            {
                case CommunicationUrgency.Urgent:
                    baseDelay *= 0.25f;
                    break;
                case CommunicationUrgency.High:
                    baseDelay *= 0.5f;
                    break;
                case CommunicationUrgency.Low:
                    baseDelay *= 2f;
                    break;
            }
            
            // Relationship affects response time
            float relationshipLevel = GetRelationshipLevel(npc);
            baseDelay *= (1.5f - relationshipLevel); // Better relationships = faster response
            
            // NPC personality affects response time
            if (npc.BehavioralProfile.IsProactive)
                baseDelay *= 0.7f;
            
            return Mathf.Max(0.5f, baseDelay);
        }
        
        private string GenerateResponseContent(NPCProfileSO npc, NPCCommunication communication)
        {
            // Generate appropriate response based on communication type and NPC personality
            switch (communication.CommunicationType)
            {
                case CommunicationType.Business_Proposal:
                    return GetRelationshipLevel(npc) > 0.6f ? 
                        "I'm interested in your proposal. Let's discuss the details." :
                        "Thank you for the proposal. I'll need to review it carefully.";
                
                case CommunicationType.Complaint:
                    return npc.ConflictTendency == ConflictTendency.Low ?
                        "I apologize for any issues. Let's work together to resolve this." :
                        "I need to understand your concerns better before we can move forward.";
                
                case CommunicationType.General_Inquiry:
                    return npc.CommunicationStyle == CommunicationStyle.Professional ?
                        "Thank you for your inquiry. I'll provide the information you requested." :
                        "Hey! Thanks for reaching out. Here's what you need to know...";
                
                default:
                    return "Thank you for your message. I'll get back to you soon.";
            }
        }
        
        private void DeliverMessage(NPCMessage message)
        {
            _messageHistory.Add(message);
            OnMessageReceived?.Invoke(message);
            _messageReceivedEvent?.Raise();
        }
        
        private void ProcessCommunication(NPCCommunication communication)
        {
            // Log communication in relationship history
            if (_npcRelationships.ContainsKey(communication.ToNPC))
            {
                var interaction = new BusinessInteraction
                {
                    NPC = communication.ToNPC,
                    InteractionType = InteractionType.Communication,
                    QualityScore = 0.7f, // Assume good communication
                    FinancialValue = 0f,
                    Timestamp = communication.Timestamp,
                    ReputationImpact = 0.01f
                };
                
                var relationship = _npcRelationships[communication.ToNPC];
                relationship.InteractionHistory.Add(interaction);
                relationship.LastInteractionDate = communication.Timestamp;
            }
        }
        
        private float CalculateEventRelationshipImpact(IndustryEvent industryEvent, NPCProfileSO npc)
        {
            float impact = 0f;
            
            switch (industryEvent.EventType)
            {
                case IndustryEventType.Regulatory_Crackdown:
                    // Good compliance reputation helps during crackdowns
                    if (_playerReputation.ComplianceScore > 0.8f)
                        impact = 0.05f;
                    break;
                
                case IndustryEventType.Technology_Breakthrough:
                    // Innovation reputation helps with tech breakthroughs
                    if (_playerReputation.InnovationScore > 0.7f)
                        impact = 0.03f;
                    break;
                
                case IndustryEventType.Market_Growth:
                    // General positive sentiment
                    impact = 0.02f;
                    break;
            }
            
            return impact * industryEvent.Intensity;
        }
        
        private bool MakeNPCDecision(NPCProfileSO npc, NPCDecision decision)
        {
            // Decision making based on NPC personality and current state
            float decisionProbability = 0.5f; // Base probability
            
            // Modify based on NPC decision making style
            switch (npc.DecisionMaking)
            {
                case DecisionMakingStyle.Impulsive:
                    decisionProbability += 0.3f;
                    break;
                case DecisionMakingStyle.Analytical:
                    decisionProbability -= 0.1f; // Analytical NPCs are more cautious
                    break;
                case DecisionMakingStyle.Collaborative:
                    // Depends on relationship with player
                    if (GetRelationshipLevel(npc) > 0.7f)
                        decisionProbability += 0.2f;
                    break;
            }
            
            // Modify based on current state
            if (_npcBehaviors.ContainsKey(npc))
            {
                var behavior = _npcBehaviors[npc];
                if (behavior.StressLevel > 0.7f)
                    decisionProbability -= 0.2f; // Stressed NPCs delay decisions
                if (behavior.CurrentMood > 0.8f)
                    decisionProbability += 0.1f; // Happy NPCs are more decisive
            }
            
            return Random.Range(0f, 1f) < Mathf.Clamp01(decisionProbability);
        }
        
        private void ApplyDecisionConsequences(NPCProfileSO npc, NPCDecision decision)
        {
            // Apply the consequences of the NPC's decision
            switch (decision.DecisionType)
            {
                case NPCDecisionType.Contract_Offer:
                    // NPC decided to offer a contract
                    var contractManager = GameManager.Instance.GetManager<ContractManager>();
                    // Would integrate with contract system
                    break;
                
                case NPCDecisionType.Business_Partnership:
                    // NPC decided to form partnership
                    if (_npcRelationships.ContainsKey(npc))
                    {
                        _npcRelationships[npc].CurrentTrustLevel += 0.1f;
                    }
                    break;
                
                case NPCDecisionType.Market_Entry:
                    // NPC decided to enter new market
                    if (_npcBehaviors.ContainsKey(npc))
                    {
                        _npcBehaviors[npc].BusinessPressure += 0.1f;
                        _npcBehaviors[npc].MarketOutlook += 0.05f;
                    }
                    break;
            }
        }
    }
    
    [System.Serializable]
    public class RelationshipSettings
    {
        [Range(0f, 0.1f)] public float BaseDecayRate = 0.01f;
        [Range(0f, 1f)] public float TrustVolatility = 0.3f;
        [Range(0f, 1f)] public float ReputationInfluence = 0.4f;
        public bool EnablePersonalityEffects = true;
    }
    
    [System.Serializable]
    public class ReputationDecaySettings
    {
        [Range(0f, 0.01f)] public float QualityDecayRate = 0.002f;
        [Range(0f, 0.01f)] public float ReliabilityDecayRate = 0.001f;
        [Range(0f, 0.01f)] public float InnovationDecayRate = 0.003f;
        [Range(0f, 0.01f)] public float ProfessionalismDecayRate = 0.001f;
        [Range(0f, 0.01f)] public float ComplianceDecayRate = 0.002f;
    }
    
    [System.Serializable]
    public class ReputationThresholds
    {
        [Range(0f, 1f)] public float ExcellentThreshold = 0.9f;
        [Range(0f, 1f)] public float GoodThreshold = 0.7f;
        [Range(0f, 1f)] public float AverageThreshold = 0.5f;
        [Range(0f, 1f)] public float PoorThreshold = 0.3f;
    }
    
    [System.Serializable]
    public class NPCBehaviorSettings
    {
        [Range(0f, 1f)] public float BaseInteractionRate = 0.1f;
        [Range(0f, 1f)] public float StressResponseSensitivity = 0.5f;
        [Range(0f, 1f)] public float MoodVolatility = 0.3f;
        public bool EnableEmotionalResponses = true;
    }
    
    [System.Serializable]
    public class CommunicationSettings
    {
        [Range(1f, 168f)] public float MaxResponseTime = 48f; // Hours
        [Range(0f, 1f)] public float BaseResponseRate = 0.7f;
        public bool EnableUrgencyModifiers = true;
        public bool EnablePersonalityEffects = true;
    }
    
    [System.Serializable]
    public class NPCRelationshipState
    {
        public NPCProfileSO NPC;
        [Range(0f, 1f)] public float CurrentTrustLevel;
        public NPCReputationView ReputationView;
        public System.DateTime LastInteractionDate;
        public List<BusinessInteraction> InteractionHistory;
        public List<RelationshipIssue> CurrentIssues;
        public CommunicationPreferences CommunicationPreferences;
    }
    
    [System.Serializable]
    public class NPCBehaviorState
    {
        public NPCProfileSO NPC;
        [Range(0f, 1f)] public float CurrentMood;
        [Range(0f, 1f)] public float StressLevel;
        [Range(0f, 1f)] public float BusinessPressure;
        [Range(0f, 1f)] public float MarketOutlook;
        [Range(0f, 1f)] public float RecentPerformance;
        public DecisionMakingState DecisionMakingState;
        public List<NPCConcern> ActiveConcerns;
        public List<NPCDecision> PendingDecisions;
    }
    
    [System.Serializable]
    public class NPCReputationView
    {
        [Range(0f, 1f)] public float QualityPerception;
        [Range(0f, 1f)] public float ReliabilityPerception;
        [Range(0f, 1f)] public float ProfessionalismPerception;
        [Range(0f, 1f)] public float InnovationPerception;
        [Range(0f, 1f)] public float CompliancePerception;
    }
    
    [System.Serializable]
    public class BusinessInteraction
    {
        public NPCProfileSO NPC;
        public InteractionType InteractionType;
        public float QualityScore;
        public float FinancialValue;
        public System.DateTime Timestamp;
        public float ReputationImpact;
        public string Notes;
    }
    
    [System.Serializable]
    public class RelationshipIssue
    {
        public string IssueName;
        public IssueType IssueType;
        public float Severity;
        public float TimeToResolve;
        public float ResolutionBenefit;
        public string Description;
    }
    
    [System.Serializable]
    public class CommunicationPreferences
    {
        public CommunicationType PreferredMethod;
        public float ResponseTimeExpectation; // Hours
        [Range(0f, 1f)] public float FormalityLevel;
        public bool PrefersBrief;
        public bool RequiresAcknowledgment;
    }
    
    [System.Serializable]
    public class NPCCommunication
    {
        public bool FromPlayer;
        public NPCProfileSO ToNPC;
        public CommunicationType CommunicationType;
        public string Content;
        public CommunicationUrgency Urgency;
        public System.DateTime Timestamp;
        public bool RequiresResponse;
    }
    
    [System.Serializable]
    public class NPCMessage
    {
        public NPCProfileSO FromNPC;
        public NPCMessageType MessageType;
        public string Content;
        public CommunicationUrgency Urgency;
        public System.DateTime Timestamp;
        public System.DateTime ScheduledDelivery;
        public bool RequiresResponse;
        public NPCCommunication RelatedCommunication;
    }
    
    [System.Serializable]
    public class IndustryEvent
    {
        public IndustryEventType EventType;
        public float Intensity;
        public string Description;
        public System.DateTime Timestamp;
        public float Duration; // Days
        public List<IndustryRole> AffectedRoles;
    }
    
    [System.Serializable]
    public class BusinessOpportunity
    {
        public string OpportunityName;
        public List<IndustryRole> RelevantRoles;
        public float RequiredInvestment;
        public float MinimumRelationshipLevel;
        public float PotentialReturn;
        public string Description;
    }
    
    [System.Serializable]
    public class NPCConcern
    {
        public NPCConcernType ConcernType;
        public string Description;
        public float Intensity;
        public float Duration;
        public System.DateTime StartDate;
    }
    
    [System.Serializable]
    public class NPCDecision
    {
        public NPCDecisionType DecisionType;
        public string Description;
        public float TimeToDecide;
        public float DecisionWeight;
        public Dictionary<string, object> DecisionParameters;
    }
    
    [System.Serializable]
    public class IndustryReputation
    {
        [Range(0f, 1f)] public float OverallStanding;
        [Range(0f, 1f)] public float MarketInfluence;
        [Range(0f, 1f)] public float InnovationLeadership;
        [Range(0f, 1f)] public float RegulatoryStanding;
        public List<string> KnownFor;
        public List<string> IndustryConnections;
    }
    
    public enum InteractionType
    {
        Contract_Delivery,
        Payment,
        Communication,
        Dispute_Resolution,
        Innovation_Collaboration,
        Market_Intelligence_Sharing,
        Joint_Venture,
        Breach_Of_Contract,
        Regulatory_Compliance,
        Emergency_Assistance
    }
    
    public enum CommunicationType
    {
        Email,
        Phone_Call,
        Video_Conference,
        In_Person_Meeting,
        Text_Message,
        Business_Proposal,
        Complaint,
        General_Inquiry,
        Contract_Discussion,
        Market_Update
    }
    
    public enum CommunicationUrgency
    {
        Low,
        Normal,
        High,
        Urgent,
        Emergency
    }
    
    public enum NPCMessageType
    {
        General,
        Response,
        Opportunity,
        Concern,
        Praise,
        Complaint,
        Market_Information,
        Contract_Offer,
        Partnership_Proposal
    }
    
    public enum IndustryEventType
    {
        Regulatory_Crackdown,
        Market_Growth,
        Technology_Breakthrough,
        Supply_Shortage,
        Demand_Surge,
        Competition_Entry,
        Economic_Downturn,
        Policy_Change
    }
    
    public enum ReputationCategory
    {
        Quality,
        Reliability,
        Innovation,
        Professionalism,
        Compliance,
        Overall
    }
    
    public enum DecisionMakingState
    {
        Normal,
        Stressed,
        Rushed,
        Cautious,
        Optimistic,
        Pessimistic
    }
    
    public enum NPCConcernType
    {
        Financial_Pressure,
        Market_Competition,
        Regulatory_Compliance,
        Supply_Chain_Issues,
        Quality_Control,
        Business_Pressure,
        Relationship_Strain
    }
    
    public enum NPCDecisionType
    {
        Contract_Offer,
        Business_Partnership,
        Market_Entry,
        Technology_Investment,
        Relationship_Change,
        Risk_Assessment,
        Strategic_Planning
    }
    
    [System.Serializable]
    public class ReputationModifier
    {
        public string ModifierName;
        public ReputationCategory Category;
        public float ModifierValue;
        public bool IsPercentage = true;
        public float Duration = -1f; // -1 for permanent
        public System.DateTime StartDate;
        public string ModifierDescription;
    }
}