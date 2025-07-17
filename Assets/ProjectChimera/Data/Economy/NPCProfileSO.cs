using UnityEngine;
using ProjectChimera.Core;
using System.Collections.Generic;

namespace ProjectChimera.Data.Economy
{
    /// <summary>
    /// Defines NPC characters in the cannabis industry ecosystem including buyers, suppliers, 
    /// regulators, and other stakeholders. Includes relationship dynamics, reputation systems,
    /// and complex business interactions.
    /// </summary>
    [CreateAssetMenu(fileName = "New NPC Profile", menuName = "Project Chimera/Economy/NPC Profile")]
    public class NPCProfileSO : ChimeraDataSO
    {
        [Header("NPC Identity")]
        [SerializeField] private string _npcName;
        [SerializeField] private string _businessName;
        [SerializeField] private NPCType _npcType = NPCType.Buyer;
        [SerializeField] private IndustryRole _industryRole = IndustryRole.Retailer;
        [SerializeField, TextArea(3, 5)] private string _background;
        [SerializeField] private Sprite _portrait;
        
        [Header("Business Profile")]
        [SerializeField] private BusinessProfile _businessProfile;
        [SerializeField] private List<BusinessActivity> _businessActivities = new List<BusinessActivity>();
        [SerializeField] private MarketPosition _marketPosition = MarketPosition.Established;
        [SerializeField] private BusinessPhilosophy _businessPhilosophy = BusinessPhilosophy.Quality_Focused;
        
        [Header("Financial Characteristics")]
        [SerializeField] private FinancialProfile _financialProfile;
        [SerializeField] private PaymentBehavior _paymentBehavior = PaymentBehavior.Reliable;
        [SerializeField] private NegotiationStyle _negotiationStyle = NegotiationStyle.Collaborative;
        [SerializeField] private RiskTolerance _riskTolerance = RiskTolerance.Moderate;
        
        [Header("Relationship Dynamics")]
        [SerializeField] private RelationshipProfile _relationshipProfile;
        [SerializeField] private List<RelationshipModifier> _relationshipModifiers = new List<RelationshipModifier>();
        [SerializeField] private CommunicationStyle _communicationStyle = CommunicationStyle.Professional;
        [SerializeField] private TrustLevel _baseTrustLevel = TrustLevel.Neutral;
        
        [Header("Market Preferences")]
        [SerializeField] private MarketPreferences _marketPreferences;
        [SerializeField] private List<ProductPreference> _productPreferences = new List<ProductPreference>();
        [SerializeField] private QualityExpectations _qualityExpectations;
        [SerializeField] private List<ComplianceExpectation> _complianceExpectations = new List<ComplianceExpectation>();
        
        [Header("Reputation and Influence")]
        [SerializeField] private ReputationProfile _reputationProfile;
        [SerializeField] private List<IndustryConnection> _industryConnections = new List<IndustryConnection>();
        [SerializeField] private InfluenceLevel _influenceLevel = InfluenceLevel.Local;
        [SerializeField] private List<ReputationFactor> _reputationFactors = new List<ReputationFactor>();
        
        [Header("Behavioral Patterns")]
        [SerializeField] private BehavioralProfile _behavioralProfile;
        [SerializeField] private List<BusinessCycle> _businessCycles = new List<BusinessCycle>();
        [SerializeField] private DecisionMakingStyle _decisionMaking = DecisionMakingStyle.Data_Driven;
        [SerializeField] private Personality _personality;
        
        [Header("Problems and Issues Generation")]
        [SerializeField] private ProblemProfile _problemProfile;
        [SerializeField] private List<PotentialIssue> _potentialIssues = new List<PotentialIssue>();
        [SerializeField] private ConflictTendency _conflictTendency = ConflictTendency.Low;
        [SerializeField] private CrisisManagement _crisisManagement = CrisisManagement.Competent;
        
        // Public Properties
        public string NPCName => _npcName;
        public string BusinessName => _businessName;
        public NPCType NPCType => _npcType;
        public IndustryRole IndustryRole => _industryRole;
        public string Background => _background;
        public Sprite Portrait => _portrait;
        public BusinessProfile BusinessProfile => _businessProfile;
        public List<BusinessActivity> BusinessActivities => _businessActivities;
        public MarketPosition MarketPosition => _marketPosition;
        public BusinessPhilosophy BusinessPhilosophy => _businessPhilosophy;
        public FinancialProfile FinancialProfile => _financialProfile;
        public PaymentBehavior PaymentBehavior => _paymentBehavior;
        public NegotiationStyle NegotiationStyle => _negotiationStyle;
        public RiskTolerance RiskTolerance => _riskTolerance;
        public RelationshipProfile RelationshipProfile => _relationshipProfile;
        public List<RelationshipModifier> RelationshipModifiers => _relationshipModifiers;
        public CommunicationStyle CommunicationStyle => _communicationStyle;
        public TrustLevel BaseTrustLevel => _baseTrustLevel;
        public MarketPreferences MarketPreferences => _marketPreferences;
        public List<ProductPreference> ProductPreferences => _productPreferences;
        public QualityExpectations QualityExpectations => _qualityExpectations;
        public List<ComplianceExpectation> ComplianceExpectations => _complianceExpectations;
        public ReputationProfile ReputationProfile => _reputationProfile;
        public List<IndustryConnection> IndustryConnections => _industryConnections;
        public InfluenceLevel InfluenceLevel => _influenceLevel;
        public List<ReputationFactor> ReputationFactors => _reputationFactors;
        public BehavioralProfile BehavioralProfile => _behavioralProfile;
        public List<BusinessCycle> BusinessCycles => _businessCycles;
        public DecisionMakingStyle DecisionMaking => _decisionMaking;
        public Personality Personality => _personality;
        public ProblemProfile ProblemProfile => _problemProfile;
        public List<PotentialIssue> PotentialIssues => _potentialIssues;
        public ConflictTendency ConflictTendency => _conflictTendency;
        public CrisisManagement CrisisManagement => _crisisManagement;
        
        /// <summary>
        /// Calculates the current relationship score with the player.
        /// </summary>
        public float CalculateRelationshipScore(PlayerReputation playerReputation, List<RelationshipEvent> relationshipHistory)
        {
            float baseScore = GetBaseTrustScore();
            
            // Apply reputation influence
            float reputationBonus = CalculateReputationInfluence(playerReputation);
            baseScore += reputationBonus;
            
            // Apply relationship history
            float historyModifier = CalculateHistoryModifier(relationshipHistory);
            baseScore += historyModifier;
            
            // Apply personality compatibility
            float personalityModifier = CalculatePersonalityCompatibility(playerReputation.PlayerPersonality);
            baseScore += personalityModifier;
            
            return Mathf.Clamp(baseScore, -1f, 1f);
        }
        
        /// <summary>
        /// Determines the likelihood of this NPC creating problems or issues.
        /// </summary>
        public float CalculateProblemProbability(MarketConditions marketConditions, float relationshipScore)
        {
            float baseProbability = _problemProfile.BaseProblemRate;
            
            // Market stress increases problem probability
            if (marketConditions.EconomicHealth < 0.5f)
                baseProbability *= 1.5f;
            
            // Poor relationships increase problems
            if (relationshipScore < 0f)
                baseProbability *= (1f - relationshipScore);
            
            // Conflict tendency affects probability
            baseProbability *= GetConflictMultiplier();
            
            // Business stress factors
            if (_businessProfile.BusinessStressLevel > 0.7f)
                baseProbability *= 1.3f;
            
            return Mathf.Clamp01(baseProbability);
        }
        
        /// <summary>
        /// Evaluates this NPC's interest in a specific contract offer.
        /// </summary>
        public ContractInterest EvaluateContractInterest(ContractSO contract, MarketConditions marketConditions)
        {
            var interest = new ContractInterest();
            
            // Base interest from business activities
            interest.BaseInterest = EvaluateBusinessAlignment(contract);
            
            // Financial capability assessment
            interest.FinancialViability = EvaluateFinancialCapability(contract);
            
            // Quality expectations alignment
            interest.QualityAlignment = EvaluateQualityAlignment(contract);
            
            // Market timing considerations
            interest.MarketTiming = EvaluateMarketTiming(contract, marketConditions);
            
            // Calculate overall interest score
            interest.OverallScore = (interest.BaseInterest * 0.3f + 
                                   interest.FinancialViability * 0.25f + 
                                   interest.QualityAlignment * 0.25f + 
                                   interest.MarketTiming * 0.2f);
            
            return interest;
        }
        
        /// <summary>
        /// Generates a potential business problem or issue from this NPC.
        /// </summary>
        public BusinessIssue GeneratePotentialIssue(PlayerReputation playerReputation, MarketConditions marketConditions)
        {
            if (_potentialIssues.Count == 0) return null;
            
            // Weight issues by probability and current conditions
            var weightedIssues = new List<(PotentialIssue issue, float weight)>();
            
            foreach (var issue in _potentialIssues)
            {
                float weight = CalculateIssueWeight(issue, playerReputation, marketConditions);
                weightedIssues.Add((issue, weight));
            }
            
            // Select issue based on weighted probability
            var selectedIssue = SelectWeightedIssue(weightedIssues);
            if (selectedIssue == null) return null;
            
            return new BusinessIssue
            {
                IssueType = selectedIssue.IssueType,
                Severity = selectedIssue.Severity,
                Description = selectedIssue.Description,
                TimeToResolve = selectedIssue.ExpectedResolutionTime,
                FinancialImpact = selectedIssue.FinancialImpact,
                ReputationImpact = selectedIssue.ReputationImpact,
                SourceNPC = this
            };
        }
        
        /// <summary>
        /// Calculates the price modifier this NPC applies based on relationship and market position.
        /// </summary>
        public float CalculatePriceModifier(float relationshipScore, MarketConditions marketConditions)
        {
            float modifier = 1f;
            
            // Relationship influence on pricing
            if (relationshipScore > 0.5f)
                modifier *= 0.95f; // 5% discount for good relationships
            else if (relationshipScore < -0.3f)
                modifier *= 1.1f; // 10% premium for poor relationships
            
            // Market position influence
            switch (_marketPosition)
            {
                case MarketPosition.Dominant:
                    modifier *= 1.05f; // Premium pricing
                    break;
                case MarketPosition.Struggling:
                    modifier *= 0.9f; // Discount pricing
                    break;
            }
            
            // Negotiation style influence
            if (_negotiationStyle == NegotiationStyle.Aggressive)
                modifier *= 1.02f;
            else if (_negotiationStyle == NegotiationStyle.Accommodating)
                modifier *= 0.98f;
            
            return modifier;
        }
        
        private float GetBaseTrustScore()
        {
            switch (_baseTrustLevel)
            {
                case TrustLevel.Hostile: return -0.8f;
                case TrustLevel.Suspicious: return -0.4f;
                case TrustLevel.Neutral: return 0f;
                case TrustLevel.Friendly: return 0.4f;
                case TrustLevel.Allied: return 0.8f;
                default: return 0f;
            }
        }
        
        private float CalculateReputationInfluence(PlayerReputation playerReputation)
        {
            float influence = 0f;
            
            // Quality reputation influence
            if (_businessPhilosophy == BusinessPhilosophy.Quality_Focused)
                influence += (playerReputation.QualityScore - 0.5f) * 0.4f;
            
            // Reliability reputation influence
            influence += (playerReputation.ReliabilityScore - 0.5f) * 0.3f;
            
            // Innovation reputation influence
            if (_businessPhilosophy == BusinessPhilosophy.Innovation_Driven)
                influence += (playerReputation.InnovationScore - 0.5f) * 0.3f;
            
            return influence;
        }
        
        private float CalculateHistoryModifier(List<RelationshipEvent> relationshipHistory)
        {
            if (relationshipHistory == null || relationshipHistory.Count == 0) return 0f;
            
            float totalImpact = 0f;
            float decayFactor = 0.95f; // Recent events have more impact
            
            for (int i = relationshipHistory.Count - 1; i >= 0; i--)
            {
                var eventObj = relationshipHistory[i];
                totalImpact += eventObj.RelationshipImpact * Mathf.Pow(decayFactor, relationshipHistory.Count - 1 - i);
            }
            
            return Mathf.Clamp(totalImpact, -0.5f, 0.5f);
        }
        
        private float CalculatePersonalityCompatibility(PlayerPersonality playerPersonality)
        {
            // Simplified personality compatibility calculation
            float compatibility = 0f;
            
            if (_personality.IsExtroverted == playerPersonality.IsExtroverted)
                compatibility += 0.1f;
            
            if (_personality.IsAnalytical == playerPersonality.IsAnalytical)
                compatibility += 0.1f;
            
            if (Mathf.Abs(_personality.RiskTaking - playerPersonality.RiskTaking) < 0.3f)
                compatibility += 0.1f;
            
            return compatibility;
        }
        
        private float GetConflictMultiplier()
        {
            switch (_conflictTendency)
            {
                case ConflictTendency.Very_Low: return 0.5f;
                case ConflictTendency.Low: return 0.8f;
                case ConflictTendency.Moderate: return 1f;
                case ConflictTendency.High: return 1.5f;
                case ConflictTendency.Very_High: return 2f;
                default: return 1f;
            }
        }
        
        private float EvaluateBusinessAlignment(ContractSO contract)
        {
            float alignment = 0.5f; // Base alignment
            
            // Check if contract type matches business activities
            foreach (var activity in _businessActivities)
            {
                if (IsContractRelevantToActivity(contract, activity))
                {
                    alignment += 0.3f;
                    break;
                }
            }
            
            return Mathf.Clamp01(alignment);
        }
        
        private float EvaluateFinancialCapability(ContractSO contract)
        {
            float contractValue = contract.Terms.TotalContractValue;
            float capability = _financialProfile.LiquidityRatio;
            
            if (contractValue <= _financialProfile.MaxContractValue)
                capability *= 1.2f;
            else
                capability *= 0.6f;
            
            return Mathf.Clamp01(capability);
        }
        
        private float EvaluateQualityAlignment(ContractSO contract)
        {
            float minQuality = contract.GetMinimumQualityRequirement();
            float expectedQuality = _qualityExpectations.MinimumQualityThreshold;
            
            return Mathf.Clamp01(1f - Mathf.Abs(minQuality - expectedQuality));
        }
        
        private float EvaluateMarketTiming(ContractSO contract, MarketConditions marketConditions)
        {
            float timing = marketConditions.EconomicHealth;
            
            // Adjust based on business philosophy
            if (_businessPhilosophy == BusinessPhilosophy.Opportunistic && marketConditions.EconomicHealth < 0.5f)
                timing += 0.3f; // Opportunistic NPCs like market downturns
            
            return Mathf.Clamp01(timing);
        }
        
        private bool IsContractRelevantToActivity(ContractSO contract, BusinessActivity activity)
        {
            // Simplified relevance check - could be more sophisticated
            return activity.ActivityType.ToString().Contains(contract.Category.ToString());
        }
        
        private float CalculateIssueWeight(PotentialIssue issue, PlayerReputation playerReputation, MarketConditions marketConditions)
        {
            float weight = issue.BaseProbability;
            
            // Market conditions influence
            if (marketConditions.EconomicHealth < 0.4f && issue.IssueType == IssueType.Financial_Dispute)
                weight *= 2f;
            
            // Reputation influence
            if (playerReputation.ReliabilityScore < 0.5f && issue.IssueType == IssueType.Delivery_Delay)
                weight *= 1.5f;
            
            return weight;
        }
        
        private PotentialIssue SelectWeightedIssue(List<(PotentialIssue issue, float weight)> weightedIssues)
        {
            float totalWeight = 0f;
            foreach (var (_, weight) in weightedIssues)
                totalWeight += weight;
            
            if (totalWeight <= 0f) return null;
            
            float randomValue = Random.value * totalWeight;
            float currentWeight = 0f;
            
            foreach (var (issue, weight) in weightedIssues)
            {
                currentWeight += weight;
                if (randomValue <= currentWeight)
                    return issue;
            }
            
            return weightedIssues[0].issue;
        }
        
        protected override bool ValidateDataSpecific()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(_npcName))
            {
                Debug.LogError($"NPCProfileSO '{name}' has no NPC name assigned.", this);
                isValid = false;
            }
            
            return isValid;
        }
    }
    
    // Supporting data structures continue in the next comment due to length...
}