using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Economy;

namespace ProjectChimera.Systems.Economy
{
    /// <summary>
    /// Manages contract lifecycle, fulfillment tracking, and business relationship dynamics
    /// for all commercial agreements in the cannabis cultivation simulation.
    /// </summary>
    public class ContractManager : ChimeraManager
    {
        [Header("Contract Configuration")]
        [SerializeField] private List<ContractSO> _availableContracts = new List<ContractSO>();
        [SerializeField] private ContractGenerationSettings _generationSettings;
        [SerializeField] private float _contractUpdateInterval = 0.5f; // In-game days
        [SerializeField] private int _maxActiveContracts = 10;
        [SerializeField] private int _maxAvailableContracts = 20;
        
        [Header("Player Capabilities")]
        [SerializeField] private PlayerCapabilities _playerCapabilities;
        [SerializeField] private float _reputationDecayRate = 0.001f; // Per day
        
        [Header("Risk Management")]
        [SerializeField] private RiskAssessmentParameters _riskParameters;
        [SerializeField] private List<ContractRiskEvent> _possibleRiskEvents = new List<ContractRiskEvent>();
        
        [Header("Events")]
        [SerializeField] private SimpleGameEventSO _contractOfferedEvent;
        [SerializeField] private SimpleGameEventSO _contractCompletedEvent;
        [SerializeField] private SimpleGameEventSO _contractFailedEvent;
        [SerializeField] private SimpleGameEventSO _relationshipChangedEvent;
        
        // Runtime Data
        private List<ActiveContract> _activeContracts;
        private List<ContractOffer> _availableOffers;
        private Dictionary<NPCProfileSO, NPCRelationshipData> _npcRelationships;
        private Queue<ContractEvent> _recentEvents;
        private float _timeSinceLastUpdate;
        
        public List<ActiveContract> ActiveContracts => _activeContracts;
        public List<ContractOffer> AvailableOffers => _availableOffers;
        public PlayerCapabilities PlayerCapabilities => _playerCapabilities;
        
        // Events
        public System.Action<ContractOffer> OnContractOffered;
        public System.Action<ActiveContract> OnContractAccepted;
        public System.Action<ActiveContract, bool> OnContractCompleted; // contract, successful
        public System.Action<NPCProfileSO, float> OnRelationshipChanged; // npc, new relationship level
        
        protected override void OnManagerInitialize()
        {
            _activeContracts = new List<ActiveContract>();
            _availableOffers = new List<ContractOffer>();
            _npcRelationships = new Dictionary<NPCProfileSO, NPCRelationshipData>();
            _recentEvents = new Queue<ContractEvent>();
            
            InitializeNPCRelationships();
            GenerateInitialContracts();
            
            Debug.Log("ContractManager initialized successfully");
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
            
            if (_timeSinceLastUpdate >= _contractUpdateInterval * gameTimeDelta)
            {
                UpdateActiveContracts();
                UpdateAvailableOffers();
                ProcessContractEvents();
                UpdateNPCRelationships();
                GenerateNewContracts();
                
                _timeSinceLastUpdate = 0f;
            }
        }
        
        /// <summary>
        /// Accept a contract offer and begin tracking it as an active contract.
        /// </summary>
        public bool AcceptContract(ContractOffer offer)
        {
            if (_activeContracts.Count >= _maxActiveContracts)
            {
                Debug.LogWarning("Cannot accept contract: Maximum active contracts reached");
                return false;
            }
            
            var activeContract = new ActiveContract
            {
                ContractSO = offer.ContractSO,
                OfferDetails = offer,
                Status = ContractStatus.Active,
                StartDate = System.DateTime.Now,
                DaysRemaining = offer.ContractSO.Terms.DurationDays,
                ProgressTracking = InitializeProgressTracking(offer.ContractSO),
                RelationshipImpact = new RelationshipImpact()
            };
            
            _activeContracts.Add(activeContract);
            _availableOffers.Remove(offer);
            
            // Update relationship with contractor
            if (_npcRelationships.ContainsKey(offer.ContractSO.ContractorNPC))
            {
                var relationship = _npcRelationships[offer.ContractSO.ContractorNPC];
                relationship.TrustLevel += 0.05f; // Accepting contracts builds trust
                relationship.LastInteractionDate = System.DateTime.Now;
            }
            
            OnContractAccepted?.Invoke(activeContract);
            return true;
        }
        
        /// <summary>
        /// Submit product deliveries for a contract.
        /// </summary>
        public ContractDeliveryResult SubmitDelivery(ActiveContract contract, ContractDelivery delivery)
        {
            var result = new ContractDeliveryResult
            {
                Contract = contract,
                Delivery = delivery,
                Success = false,
                QualityScore = 0f,
                ComplianceScore = 0f,
                BonusesEarned = new List<BonusIncentive>(),
                PenaltiesApplied = new List<PenaltyClause>()
            };
            
            // Evaluate delivery quality
            result.QualityScore = EvaluateDeliveryQuality(contract, delivery);
            result.ComplianceScore = EvaluateDeliveryCompliance(contract, delivery);
            
            // Check if delivery meets minimum requirements
            float minQuality = contract.ContractSO.GetMinimumQualityRequirement();
            if (result.QualityScore >= minQuality && result.ComplianceScore >= 0.8f)
            {
                result.Success = true;
                
                // Update contract progress
                UpdateContractProgress(contract, delivery);
                
                // Check for bonuses
                result.BonusesEarned = EvaluateBonuses(contract, result);
                
                // Update relationship
                UpdateRelationshipFromDelivery(contract.ContractSO.ContractorNPC, result);
            }
            else
            {
                result.Success = false;
                
                // Apply penalties for poor delivery
                result.PenaltiesApplied = EvaluatePenalties(contract, result);
                
                // Negative relationship impact
                UpdateRelationshipFromFailure(contract.ContractSO.ContractorNPC, result);
            }
            
            return result;
        }
        
        /// <summary>
        /// Evaluates contract feasibility for the player's current capabilities.
        /// </summary>
        public ContractFeasibility EvaluateContractFeasibility(ContractSO contractSO)
        {
            return contractSO.EvaluateContractFeasibility(_playerCapabilities);
        }
        
        /// <summary>
        /// Gets all contracts available from a specific NPC.
        /// </summary>
        public List<ContractOffer> GetContractsFromNPC(NPCProfileSO npc)
        {
            return _availableOffers.Where(offer => offer.ContractSO.ContractorNPC == npc).ToList();
        }
        
        /// <summary>
        /// Gets relationship data for a specific NPC.
        /// </summary>
        public NPCRelationshipData GetRelationshipData(NPCProfileSO npc)
        {
            return _npcRelationships.ContainsKey(npc) ? _npcRelationships[npc] : null;
        }
        
        /// <summary>
        /// Updates player capabilities (called from other systems).
        /// </summary>
        public void UpdatePlayerCapabilities(PlayerCapabilities newCapabilities)
        {
            _playerCapabilities = newCapabilities;
            
            // Re-evaluate available contracts
            RefreshContractOffers();
        }
        
        /// <summary>
        /// Forces early contract termination.
        /// </summary>
        public void TerminateContract(ActiveContract contract, ContractTerminationReason reason)
        {
            contract.Status = ContractStatus.Terminated;
            contract.TerminationReason = reason;
            
            // Apply termination penalties
            if (contract.ContractSO.Terms.EarlyTerminationPenalty > 0f)
            {
                float penalty = contract.OfferDetails.TotalValue * contract.ContractSO.Terms.EarlyTerminationPenalty;
                // Apply financial penalty (would integrate with financial system)
            }
            
            // Negative relationship impact
            var relationship = _npcRelationships[contract.ContractSO.ContractorNPC];
            relationship.TrustLevel -= 0.2f;
            relationship.ReliabilityScore -= 0.15f;
            
            _activeContracts.Remove(contract);
            OnContractCompleted?.Invoke(contract, false);
        }
        
        private void InitializeNPCRelationships()
        {
            foreach (var contract in _availableContracts)
            {
                if (contract.ContractorNPC != null && !_npcRelationships.ContainsKey(contract.ContractorNPC))
                {
                    var relationshipData = new NPCRelationshipData
                    {
                        NPC = contract.ContractorNPC,
                        TrustLevel = Random.Range(0.3f, 0.7f),
                        ReliabilityScore = 0.5f,
                        ProfessionalismScore = 0.5f,
                        RelationshipHistory = new List<RelationshipEvent>(),
                        LastInteractionDate = System.DateTime.Now.AddDays(-Random.Range(1, 30))
                    };
                    
                    _npcRelationships[contract.ContractorNPC] = relationshipData;
                }
            }
        }
        
        private void GenerateInitialContracts()
        {
            int contractsToGenerate = Mathf.Min(_maxAvailableContracts, _availableContracts.Count);
            
            for (int i = 0; i < contractsToGenerate; i++)
            {
                if (i < _availableContracts.Count)
                {
                    var contractOffer = GenerateContractOffer(_availableContracts[i]);
                    if (contractOffer != null)
                    {
                        _availableOffers.Add(contractOffer);
                    }
                }
            }
        }
        
        private ContractOffer GenerateContractOffer(ContractSO contractSO)
        {
            // Check if player meets basic requirements
            var feasibility = EvaluateContractFeasibility(contractSO);
            if (feasibility.FeasibilityScore < 0.3f) // Below 30% feasibility
                return null;
            
            var offer = new ContractOffer
            {
                ContractSO = contractSO,
                OfferedDate = System.DateTime.Now,
                ExpirationDate = System.DateTime.Now.AddDays(Random.Range(7, 21)),
                TotalValue = contractSO.Terms.TotalContractValue,
                IsNegotiable = Random.Range(0f, 1f) < 0.3f, // 30% chance
                PriorityLevel = CalculateOfferPriority(contractSO),
                SpecialConditions = GenerateSpecialConditions(contractSO)
            };
            
            // Apply relationship modifier to value
            if (_npcRelationships.ContainsKey(contractSO.ContractorNPC))
            {
                var relationship = _npcRelationships[contractSO.ContractorNPC];
                float relationshipBonus = (relationship.TrustLevel - 0.5f) * 0.2f; // ±10% based on trust
                offer.TotalValue *= (1f + relationshipBonus);
            }
            
            return offer;
        }
        
        private void UpdateActiveContracts()
        {
            for (int i = _activeContracts.Count - 1; i >= 0; i--)
            {
                var contract = _activeContracts[i];
                
                // Update days remaining
                contract.DaysRemaining -= _contractUpdateInterval;
                
                // Check for contract expiration
                if (contract.DaysRemaining <= 0)
                {
                    if (contract.ProgressTracking.CompletionPercentage >= 1f)
                    {
                        CompleteContract(contract, true);
                    }
                    else
                    {
                        CompleteContract(contract, false);
                    }
                }
                
                // Check for risk events
                ProcessContractRisks(contract);
            }
        }
        
        private void UpdateAvailableOffers()
        {
            // Remove expired offers
            for (int i = _availableOffers.Count - 1; i >= 0; i--)
            {
                if (_availableOffers[i].ExpirationDate < System.DateTime.Now)
                {
                    _availableOffers.RemoveAt(i);
                }
            }
        }
        
        private void ProcessContractEvents()
        {
            // Clean up old events
            while (_recentEvents.Count > 0 && 
                   (System.DateTime.Now - _recentEvents.Peek().Timestamp).TotalDays > 30)
            {
                _recentEvents.Dequeue();
            }
        }
        
        private void UpdateNPCRelationships()
        {
            foreach (var kvp in _npcRelationships.ToList())
            {
                var relationship = kvp.Value;
                
                // Apply natural decay
                float daysSinceInteraction = (float)(System.DateTime.Now - relationship.LastInteractionDate).TotalDays;
                if (daysSinceInteraction > 7) // After a week of no contact
                {
                    float decay = _reputationDecayRate * daysSinceInteraction;
                    relationship.TrustLevel = Mathf.Max(0.1f, relationship.TrustLevel - decay);
                }
            }
        }
        
        private void GenerateNewContracts()
        {
            if (_availableOffers.Count < _maxAvailableContracts && Random.Range(0f, 1f) < 0.1f) // 10% chance
            {
                var randomContractTemplate = _availableContracts[Random.Range(0, _availableContracts.Count)];
                var newOffer = GenerateContractOffer(randomContractTemplate);
                
                if (newOffer != null)
                {
                    _availableOffers.Add(newOffer);
                    OnContractOffered?.Invoke(newOffer);
                    _contractOfferedEvent?.Raise();
                }
            }
        }
        
        private ContractProgressTracking InitializeProgressTracking(ContractSO contractSO)
        {
            return new ContractProgressTracking
            {
                TotalQuantityRequired = contractSO.Terms.QuantityRequired,
                QuantityDelivered = 0f,
                CompletionPercentage = 0f,
                QualityScores = new List<float>(),
                DeliveryDates = new List<System.DateTime>(),
                IssuesEncountered = new List<ContractIssue>()
            };
        }
        
        private float EvaluateDeliveryQuality(ActiveContract contract, ContractDelivery delivery)
        {
            float qualityScore = 0f;
            int evaluatedSpecs = 0;
            
            // Evaluate against product specifications
            foreach (var spec in contract.ContractSO.ProductSpecs)
            {
                if (spec.ProductType == delivery.ProductType)
                {
                    // Check THC content
                    if (delivery.THCContent >= spec.THCRange.x && delivery.THCContent <= spec.THCRange.y)
                        qualityScore += 0.3f;
                    
                    // Check CBD content
                    if (delivery.CBDContent >= spec.CBDRange.x && delivery.CBDContent <= spec.CBDRange.y)
                        qualityScore += 0.2f;
                    
                    // Check moisture content
                    if (delivery.MoistureContent >= spec.MoistureRange.x && delivery.MoistureContent <= spec.MoistureRange.y)
                        qualityScore += 0.2f;
                    
                    // Overall quality assessment
                    qualityScore += delivery.OverallQuality * 0.3f;
                    
                    evaluatedSpecs++;
                    break;
                }
            }
            
            return evaluatedSpecs > 0 ? Mathf.Clamp01(qualityScore) : delivery.OverallQuality;
        }
        
        private float EvaluateDeliveryCompliance(ActiveContract contract, ContractDelivery delivery)
        {
            float complianceScore = 1f;
            
            // Check packaging compliance
            if (contract.ContractSO.ProcessingRequirements.PackagingRequirement != delivery.PackagingType)
                complianceScore -= 0.2f;
            
            // Check testing compliance
            if (contract.ContractSO.RequiresThirdPartyTesting && !delivery.HasThirdPartyTesting)
                complianceScore -= 0.3f;
            
            // Check chain of custody
            if (contract.ContractSO.RequiresChainOfCustody && !delivery.HasChainOfCustody)
                complianceScore -= 0.25f;
            
            // Check delivery timing
            if (delivery.DeliveryDate > contract.OfferDetails.ExpirationDate)
                complianceScore -= 0.25f;
            
            return Mathf.Clamp01(complianceScore);
        }
        
        private void UpdateContractProgress(ActiveContract contract, ContractDelivery delivery)
        {
            contract.ProgressTracking.QuantityDelivered += delivery.Quantity;
            contract.ProgressTracking.CompletionPercentage = 
                contract.ProgressTracking.QuantityDelivered / contract.ProgressTracking.TotalQuantityRequired;
            contract.ProgressTracking.QualityScores.Add(delivery.OverallQuality);
            contract.ProgressTracking.DeliveryDates.Add(delivery.DeliveryDate);
        }
        
        private List<BonusIncentive> EvaluateBonuses(ActiveContract contract, ContractDeliveryResult result)
        {
            var earnedBonuses = new List<BonusIncentive>();
            
            foreach (var bonus in contract.ContractSO.BonusIncentives)
            {
                bool bonusEarned = false;
                
                // Quality bonuses
                if (bonus.IncentiveName.Contains("Quality") && result.QualityScore >= 0.9f)
                    bonusEarned = true;
                
                // Early delivery bonuses
                if (bonus.IncentiveName.Contains("Early") && result.Delivery.DeliveryDate < contract.OfferDetails.ExpirationDate.AddDays(-7))
                    bonusEarned = true;
                
                if (bonusEarned)
                {
                    earnedBonuses.Add(bonus);
                }
            }
            
            return earnedBonuses;
        }
        
        private List<PenaltyClause> EvaluatePenalties(ActiveContract contract, ContractDeliveryResult result)
        {
            var appliedPenalties = new List<PenaltyClause>();
            
            foreach (var penalty in contract.ContractSO.PenaltyClauses)
            {
                bool penaltyApplies = false;
                
                // Quality penalties
                if (penalty.ViolationType.Contains("Quality") && result.QualityScore < 0.7f)
                    penaltyApplies = true;
                
                // Compliance penalties
                if (penalty.ViolationType.Contains("Compliance") && result.ComplianceScore < 0.8f)
                    penaltyApplies = true;
                
                if (penaltyApplies)
                {
                    appliedPenalties.Add(penalty);
                }
            }
            
            return appliedPenalties;
        }
        
        private void UpdateRelationshipFromDelivery(NPCProfileSO npc, ContractDeliveryResult result)
        {
            if (!_npcRelationships.ContainsKey(npc)) return;
            
            var relationship = _npcRelationships[npc];
            
            // Positive impact from successful delivery
            if (result.Success)
            {
                relationship.ReliabilityScore += 0.02f;
                relationship.ProfessionalismScore += 0.01f;
                
                if (result.QualityScore >= 0.9f)
                {
                    relationship.TrustLevel += 0.03f;
                }
            }
            
            // Cap values
            relationship.TrustLevel = Mathf.Clamp01(relationship.TrustLevel);
            relationship.ReliabilityScore = Mathf.Clamp01(relationship.ReliabilityScore);
            relationship.ProfessionalismScore = Mathf.Clamp01(relationship.ProfessionalismScore);
            
            relationship.LastInteractionDate = System.DateTime.Now;
            
            OnRelationshipChanged?.Invoke(npc, relationship.TrustLevel);
        }
        
        private void UpdateRelationshipFromFailure(NPCProfileSO npc, ContractDeliveryResult result)
        {
            if (!_npcRelationships.ContainsKey(npc)) return;
            
            var relationship = _npcRelationships[npc];
            
            relationship.TrustLevel -= 0.05f;
            relationship.ReliabilityScore -= 0.03f;
            
            // Clamp to minimum values
            relationship.TrustLevel = Mathf.Max(0.1f, relationship.TrustLevel);
            relationship.ReliabilityScore = Mathf.Max(0.1f, relationship.ReliabilityScore);
            
            relationship.LastInteractionDate = System.DateTime.Now;
            
            OnRelationshipChanged?.Invoke(npc, relationship.TrustLevel);
        }
        
        private void CompleteContract(ActiveContract contract, bool successful)
        {
            contract.Status = successful ? ContractStatus.Completed : ContractStatus.Failed;
            contract.CompletionDate = System.DateTime.Now;
            
            if (successful)
            {
                // Positive relationship impact
                var relationship = _npcRelationships[contract.ContractSO.ContractorNPC];
                relationship.TrustLevel += 0.05f;
                relationship.ReliabilityScore += 0.03f;
                
                _contractCompletedEvent?.Raise();
            }
            else
            {
                // Negative relationship impact
                var relationship = _npcRelationships[contract.ContractSO.ContractorNPC];
                relationship.TrustLevel -= 0.1f;
                relationship.ReliabilityScore -= 0.05f;
                
                _contractFailedEvent?.Raise();
            }
            
            _activeContracts.Remove(contract);
            OnContractCompleted?.Invoke(contract, successful);
        }
        
        private void ProcessContractRisks(ActiveContract contract)
        {
            foreach (var risk in contract.ContractSO.ContractRisks)
            {
                if (Random.Range(0f, 1f) < risk.Probability * 0.1f) // Scale down probability for per-update checks
                {
                    var riskEvent = new ContractEvent
                    {
                        Contract = contract,
                        EventType = ContractEventType.Risk_Materialized,
                        Description = $"Risk materialized: {risk.RiskName}",
                        Impact = risk.PotentialLoss,
                        Timestamp = System.DateTime.Now
                    };
                    
                    _recentEvents.Enqueue(riskEvent);
                    
                    // Apply risk consequences
                    contract.ProgressTracking.IssuesEncountered.Add(new ContractIssue
                    {
                        IssueType = IssueType.Technical_Problem,
                        Description = risk.RiskName,
                        FinancialImpact = risk.PotentialLoss,
                        OccurrenceDate = System.DateTime.Now
                    });
                }
            }
        }
        
        private ContractPriority CalculateOfferPriority(ContractSO contractSO)
        {
            float value = contractSO.Terms.TotalContractValue;
            int duration = contractSO.Terms.DurationDays;
            
            if (value > 50000f && duration < 60)
                return ContractPriority.High;
            else if (value > 20000f || duration < 30)
                return ContractPriority.Medium;
            else
                return ContractPriority.Low;
        }
        
        private List<string> GenerateSpecialConditions(ContractSO contractSO)
        {
            var conditions = new List<string>();
            
            if (contractSO.RequiresOrganicCertification)
                conditions.Add("Organic certification required");
            
            if (contractSO.RequiresSpecializedEquipment)
                conditions.Add("Specialized processing equipment required");
            
            if (contractSO.CarbonFootprintLimit < 50f)
                conditions.Add("Low carbon footprint required");
            
            return conditions;
        }
        
        private void RefreshContractOffers()
        {
            // Re-evaluate all available offers based on updated capabilities
            for (int i = _availableOffers.Count - 1; i >= 0; i--)
            {
                var feasibility = EvaluateContractFeasibility(_availableOffers[i].ContractSO);
                if (feasibility.FeasibilityScore < 0.2f) // Too low feasibility
                {
                    _availableOffers.RemoveAt(i);
                }
            }
        }
    }
    
    [System.Serializable]
    public class ContractGenerationSettings
    {
        [Range(0f, 1f)] public float BaseOfferRate = 0.1f;
        [Range(1, 10)] public int MinContractsAvailable = 3;
        [Range(10, 50)] public int MaxContractsAvailable = 20;
        [Range(0f, 0.5f)] public float RelationshipInfluence = 0.2f;
        [Range(0f, 0.3f)] public float RandomVariation = 0.1f;
    }
    
    [System.Serializable]
    public class RiskAssessmentParameters
    {
        [Range(0f, 1f)] public float BaseRiskLevel = 0.1f;
        [Range(0f, 1f)] public float ComplexityRiskMultiplier = 0.5f;
        [Range(0f, 1f)] public float RelationshipRiskReduction = 0.3f;
        [Range(0f, 1f)] public float ExperienceRiskReduction = 0.2f;
    }
    
    [System.Serializable]
    public class ActiveContract
    {
        public ContractSO ContractSO;
        public ContractOffer OfferDetails;
        public ContractStatus Status;
        public System.DateTime StartDate;
        public System.DateTime CompletionDate;
        public float DaysRemaining;
        public ContractProgressTracking ProgressTracking;
        public RelationshipImpact RelationshipImpact;
        public ContractTerminationReason TerminationReason;
    }
    
    [System.Serializable]
    public class ContractOffer
    {
        public ContractSO ContractSO;
        public System.DateTime OfferedDate;
        public System.DateTime ExpirationDate;
        public float TotalValue;
        public bool IsNegotiable;
        public ContractPriority PriorityLevel;
        public List<string> SpecialConditions;
    }
    
    [System.Serializable]
    public class NPCRelationshipData
    {
        public NPCProfileSO NPC;
        [Range(0f, 1f)] public float TrustLevel;
        [Range(0f, 1f)] public float ReliabilityScore;
        [Range(0f, 1f)] public float ProfessionalismScore;
        public List<RelationshipEvent> RelationshipHistory;
        public System.DateTime LastInteractionDate;
    }
    
    [System.Serializable]
    public class ContractProgressTracking
    {
        public float TotalQuantityRequired;
        public float QuantityDelivered;
        public float CompletionPercentage;
        public List<float> QualityScores;
        public List<System.DateTime> DeliveryDates;
        public List<ContractIssue> IssuesEncountered;
    }
    
    [System.Serializable]
    public class ContractDelivery
    {
        public ProductType ProductType;
        public float Quantity;
        public float THCContent;
        public float CBDContent;
        public float MoistureContent;
        public float OverallQuality;
        public PackagingRequirement PackagingType;
        public bool HasThirdPartyTesting;
        public bool HasChainOfCustody;
        public System.DateTime DeliveryDate;
    }
    
    [System.Serializable]
    public class ContractDeliveryResult
    {
        public ActiveContract Contract;
        public ContractDelivery Delivery;
        public bool Success;
        public float QualityScore;
        public float ComplianceScore;
        public List<BonusIncentive> BonusesEarned;
        public List<PenaltyClause> PenaltiesApplied;
    }
    
    [System.Serializable]
    public class RelationshipImpact
    {
        public float TrustChange;
        public float ReputationChange;
        public float ProfessionalismChange;
    }
    
    [System.Serializable]
    public class ContractEvent
    {
        public ActiveContract Contract;
        public ContractEventType EventType;
        public string Description;
        public float Impact;
        public System.DateTime Timestamp;
    }
    
    [System.Serializable]
    public class ContractIssue
    {
        public IssueType IssueType;
        public string Description;
        public float FinancialImpact;
        public System.DateTime OccurrenceDate;
    }
    
    [System.Serializable]
    public class ContractRiskEvent
    {
        public string RiskName;
        public RiskType RiskType;
        public float Probability;
        public float Impact;
        public string Description;
    }
    
    public enum ContractStatus
    {
        Offered,
        Active,
        Completed,
        Failed,
        Terminated,
        Disputed
    }
    
    public enum ContractPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }
    
    public enum ContractEventType
    {
        Contract_Offered,
        Contract_Accepted,
        Delivery_Made,
        Quality_Issue,
        Compliance_Violation,
        Bonus_Earned,
        Penalty_Applied,
        Risk_Materialized,
        Contract_Completed,
        Contract_Failed
    }
    
    public enum ContractTerminationReason
    {
        Player_Request,
        NPC_Request,
        Breach_Of_Contract,
        Force_Majeure,
        Mutual_Agreement,
        Regulatory_Change
    }
}