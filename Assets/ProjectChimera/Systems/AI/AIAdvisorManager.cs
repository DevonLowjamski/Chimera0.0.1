using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.AI;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Economy;
using ProjectChimera.Systems.Environment;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Environment;
using ProjectChimera.Data.Genetics;
using AITrendAnalysis = ProjectChimera.Data.AI.TrendAnalysis;
using AIPriority = ProjectChimera.Data.AI.RecommendationPriority;
using CultivationPriority = ProjectChimera.Data.Cultivation.RecommendationPriority;
using AIBreedingRecommendation = ProjectChimera.Data.AI.BreedingRecommendation;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;
using EnvironmentalOptimization = ProjectChimera.Data.AI.EnvironmentalOptimization;
using OptimizationComplexity = ProjectChimera.Data.AI.OptimizationComplexity;
using ProductMarketData = ProjectChimera.Data.AI.ProductMarketData;
using OptimalRange = ProjectChimera.Data.AI.OptimalRange;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC-012-1: AI Advisor Manager - Intelligent analysis and recommendation system
    /// Analyzes cultivation data, genetics, market conditions, and environmental factors
    /// to provide actionable recommendations to players
    /// </summary>
    public class AIAdvisorManager : ChimeraManager
    {
        [Header("AI Advisor Configuration")]
        [SerializeField] private float _analysisInterval = 30f;
        [SerializeField] private int _maxRecommendations = 5;
        [SerializeField] private float _confidenceThreshold = 0.6f;
        [SerializeField] private bool _enableProactiveAdvice = true;
        
        [Header("Data Sources")]
        [SerializeField] private CultivationManager _cultivationManager;
        [SerializeField] private MarketManager _marketManager;
        [SerializeField] private TradingManager _tradingManager;
        [SerializeField] private EnvironmentalManager _environmentalManager;
        
        [Header("AI Analysis Results")]
        [SerializeField] private List<AIRecommendation> _currentRecommendations = new List<AIRecommendation>();
        [SerializeField] private AIAnalysisReport _lastAnalysisReport;
        [SerializeField] private float _lastAnalysisTime;
        
        // Analysis engines
        private CultivationAnalysisEngine _cultivationAnalysis;
        private MarketAnalysisEngine _marketAnalysis;
        private EnvironmentalAnalysisEngine _environmentalAnalysis;
        private GeneticsAnalysisEngine _geneticsAnalysis;
        
        // Analysis timing fields
        private float _lastQuickAnalysis;
        private float _lastDeepAnalysis;
        private float _lastStrategicAnalysis;
        private float _quickAnalysisInterval = 10f;
        private float _deepAnalysisInterval = 60f;
        private float _strategicAnalysisInterval = 300f;
        
        // Additional data collections
        private Queue<AnalysisSnapshot> _historicalData = new Queue<AnalysisSnapshot>();
        private Dictionary<string, PredictiveModel> _models = new Dictionary<string, PredictiveModel>();
        private float _recommendationValidityHours = 24f;
        private int _maxActiveRecommendations = 10;
        
        // Event channels (ScriptableObject-based events)
        private GameEventSO<string> _onNewRecommendation;
        private List<DataInsight> _discoveredInsights = new List<DataInsight>();
        private GameEventSO<string> _onCriticalInsight;
        private List<OptimizationOpportunity> _optimizationOpportunities = new List<OptimizationOpportunity>();
        private GameEventSO<string> _onOptimizationIdentified;
        
        // Additional collections
        private List<DataInsight> _criticalInsights = new List<DataInsight>();
        private List<AIRecommendation> _activeRecommendations = new List<AIRecommendation>();
        
        // Properties for missing fields
        public float SystemEfficiencyScore { get; private set; } = 0.8f;
        public List<AIRecommendation> ActiveRecommendations => _activeRecommendations;
        public List<OptimizationOpportunity> OptimizationOpportunities => _optimizationOpportunities;
        public List<DataInsight> CriticalInsights => _criticalInsights;
        public List<PerformancePattern> IdentifiedPatterns { get; private set; } = new List<PerformancePattern>();
        public int _identifiedPatterns => IdentifiedPatterns.Count;
        public AIAdvisorSettings Settings { get; private set; }
        
        // Event for optimization identified
        public event Action<OptimizationOpportunity> OnOptimizationIdentified;
        
        // Events
        public event Action<AIRecommendation> OnNewRecommendation;
        public event Action<AIAnalysisReport> OnAnalysisComplete;
        public event Action<string> OnCriticalAlert;
        public event Action<DataInsight> OnCriticalInsight;
        
        protected override void OnManagerInitialize()
        {
            _lastQuickAnalysis = Time.time;
            _lastDeepAnalysis = Time.time;
            _lastStrategicAnalysis = Time.time;
            
            // Initialize Settings
            Settings = new AIAdvisorSettings
            {
                AnalysisFrequency = 30f,
                RecommendationThreshold = 0.7f,
                MaxActiveRecommendations = _maxActiveRecommendations,
                EnablePredictiveAnalysis = true,
                EnableOptimizationSuggestions = true
            };
            
            InitializeSystemReferences();
            InitializePredictiveModels();
            SetupBaselineRecommendations();
            
            LogInfo("AIAdvisorManager initialized with intelligent analysis capabilities");
        }
        
        protected override void OnManagerUpdate()
        {
            float currentTime = Time.time;
            
            // Quick analysis for immediate recommendations
            if (currentTime - _lastQuickAnalysis >= _quickAnalysisInterval)
            {
                PerformQuickAnalysis();
                _lastQuickAnalysis = currentTime;
            }
            
            // Deep analysis for complex insights
            if (currentTime - _lastDeepAnalysis >= _deepAnalysisInterval)
            {
                PerformDeepAnalysis();
                _lastDeepAnalysis = currentTime;
            }
            
            // Strategic analysis for long-term optimization
            if (currentTime - _lastStrategicAnalysis >= _strategicAnalysisInterval)
            {
                PerformStrategicAnalysis();
                _lastStrategicAnalysis = currentTime;
            }
            
            // Maintenance tasks
            UpdateRecommendationStatus();
            CleanupExpiredData();
        }
        
        #region System Integration
        
        private void InitializeSystemReferences()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null) return;
            
            // No direct manager references needed - use event-based communication through ChimeraManager base class
            
            LogInfo("AI Advisor integrated with available facility systems");
        }
        
        private void InitializePredictiveModels()
        {
            // Environmental prediction model
            _models["environmental_efficiency"] = new PredictiveModel
            {
                ModelId = "environmental_efficiency",
                ModelName = "Environmental Efficiency Predictor",
                ModelType = PredictiveModelType.Regression,
                Accuracy = 0.85f,
                LastTrained = DateTime.Now,
                IsActive = true
            };
            
            // Economic performance model
            _models["economic_performance"] = new PredictiveModel
            {
                ModelId = "economic_performance", 
                ModelName = "Economic Performance Predictor",
                ModelType = PredictiveModelType.Time_Series,
                Accuracy = 0.78f,
                LastTrained = DateTime.Now,
                IsActive = true
            };
            
            // Facility optimization model
            _models["facility_optimization"] = new PredictiveModel
            {
                ModelId = "facility_optimization",
                ModelName = "Facility Optimization Analyzer",
                ModelType = PredictiveModelType.Classification,
                Accuracy = 0.82f,
                LastTrained = DateTime.Now,
                IsActive = true
            };
            
            LogInfo($"Initialized {_models.Count} predictive models for AI analysis");
        }
        
        #endregion
        
        #region Analysis Engine
        
        private void PerformQuickAnalysis()
        {
            LogInfo("Performing quick analysis...");
            
            var snapshot = CaptureCurrentSnapshot();
            _historicalData.Enqueue(snapshot);
            
            // Limit historical data size
            if (_historicalData.Count > 1000)
                _historicalData.Dequeue();
            
            // Immediate recommendations
            AnalyzeEnvironmentalConditions(snapshot);
            AnalyzeSystemPerformance(snapshot);
            CheckForImmediateOptimizations(snapshot);
        }
        
        private void PerformDeepAnalysis()
        {
            LogInfo("Performing deep analysis...");
            
            if (_historicalData.Count < 10) return; // Need some data for deep analysis
            
            AnalyzeTrends();
            IdentifyPerformancePatterns();
            GenerateOptimizationRecommendations();
            UpdatePredictiveModels();
        }
        
        private void PerformStrategicAnalysis()
        {
            LogInfo("Performing strategic analysis...");
            
            if (_historicalData.Count < 100) return; // Need substantial data
            
            AnalyzeLongTermPerformance();
            IdentifyStrategicOpportunities();
            GenerateBusinessRecommendations();
            UpdateSystemEfficiencyMetrics();
        }
        
        private AnalysisSnapshot CaptureCurrentSnapshot()
        {
            var snapshot = new AnalysisSnapshot
            {
                Timestamp = DateTime.Now,
                EnvironmentalData = CaptureEnvironmentalData(),
                EconomicData = CaptureEconomicData(),
                PerformanceData = CapturePerformanceData(),
                SystemData = CaptureSystemData()
            };
            
            return snapshot;
        }
        
        private EnvironmentalSnapshot CaptureEnvironmentalData()
        {
            var data = new EnvironmentalSnapshot();
            
            // Use simulated data since we don't have direct manager access
            data.ActiveSensors = UnityEngine.Random.Range(8, 15);
            data.ActiveAlerts = UnityEngine.Random.Range(0, 3);
            data.SystemUptime = UnityEngine.Random.Range(95f, 99f);
            
            // Simulated HVAC performance metrics
            data.HVACEfficiency = UnityEngine.Random.Range(88f, 95f);
            data.EnergyUsage = UnityEngine.Random.Range(1100f, 1400f);
            
            // Simulated lighting performance
            data.LightingEfficiency = UnityEngine.Random.Range(85f, 92f);
            data.DLIOptimization = UnityEngine.Random.Range(83f, 90f);
            
            return data;
        }
        
        private EconomicSnapshot CaptureEconomicData()
        {
            var data = new EconomicSnapshot();
            
            // Use simulated financial data since we don't have direct manager access
            data.Revenue = UnityEngine.Random.Range(40000f, 50000f);
            data.Profit = UnityEngine.Random.Range(10000f, 15000f);
            data.CashFlow = UnityEngine.Random.Range(7000f, 10000f);
            
            // Simulated investment data
            data.ROI = UnityEngine.Random.Range(0.15f, 0.22f);
            data.RiskScore = UnityEngine.Random.Range(0.25f, 0.40f);
            data.NetWorth = UnityEngine.Random.Range(200000f, 300000f);
            
            // Simulated market data
            data.MarketTrend = UnityEngine.Random.Range(1.05f, 1.25f);
            data.DemandScore = UnityEngine.Random.Range(0.70f, 0.85f);
            
            return data;
        }
        
        private PerformanceSnapshot CapturePerformanceData()
        {
            return new PerformanceSnapshot
            {
                FrameRate = 1f / Time.deltaTime,
                MemoryUsage = GC.GetTotalMemory(false) / (1024f * 1024f),
                ActiveSystems = _managers?.Count ?? 0,
                SystemResponseTime = CalculateAverageResponseTime()
            };
        }
        
        private SystemSnapshot CaptureSystemData()
        {
            var data = new SystemSnapshot();
            
            // Use simulated system data since we don't have direct manager access
            data.SkillProgress = UnityEngine.Random.Range(0.60f, 0.80f);
            data.UnlockedNodes = UnityEngine.Random.Range(12, 20);
            
            // Simulated research progress
            data.ResearchProgress = UnityEngine.Random.Range(0.35f, 0.50f);
            data.CompletedProjects = UnityEngine.Random.Range(6, 12);
            
            return data;
        }
        
        #endregion
        
        #region Environmental Analysis
        
        private void AnalyzeEnvironmentalConditions(AnalysisSnapshot snapshot)
        {
            var envData = snapshot.EnvironmentalData;
            
            // HVAC efficiency analysis
            if (envData.HVACEfficiency < 85f)
            {
                CreateRecommendation(
                    "HVAC Optimization",
                    "HVAC system efficiency below optimal threshold",
                    $"Current HVAC efficiency at {envData.HVACEfficiency:F1}%. Consider maintenance or settings adjustment.",
                    RecommendationType.Maintenance,
                    AIPriority.Medium,
                    "Environmental"
                );
            }
            
            // Lighting optimization
            if (envData.LightingEfficiency < 88f)
            {
                CreateRecommendation(
                    "Lighting Schedule Optimization", 
                    "Lighting system not operating at peak efficiency",
                    $"Lighting efficiency at {envData.LightingEfficiency:F1}%. Review photoperiod schedules and intensity settings.",
                    RecommendationType.Optimization,
                    AIPriority.Low,
                    "Environmental"
                );
            }
            
            // Energy usage alerts
            if (envData.EnergyUsage > 1500f)
            {
                CreateRecommendation(
                    "High Energy Usage Alert",
                    "Energy consumption exceeds normal operating range",
                    $"Current usage: {envData.EnergyUsage:F0}W. Consider implementing energy-saving strategies.",
                    RecommendationType.Alert,
                    AIPriority.Medium,
                    "Energy"
                );
            }
        }
        
        #endregion
        
        #region Economic Analysis
        
        private void AnalyzeEconomicPerformance(AnalysisSnapshot snapshot)
        {
            var econData = snapshot.EconomicData;
            
            // Profitability analysis
            if (econData.Revenue > 0)
            {
                float profitMargin = econData.Profit / econData.Revenue;
                
                if (profitMargin < 0.2f)
                {
                    CreateRecommendation(
                        "Profit Margin Improvement",
                        "Profit margin below target threshold",
                        $"Current margin: {profitMargin:P1}. Consider cost reduction or pricing optimization strategies.",
                        RecommendationType.Business_Strategy,
                        AIPriority.High,
                        "Economics"
                    );
                }
            }
            
            // Cash flow monitoring
            if (econData.CashFlow < 5000f)
            {
                CreateRecommendation(
                    "Cash Flow Management",
                    "Cash flow below recommended levels",
                    $"Current cash flow: ${econData.CashFlow:N0}. Review payment terms and collection processes.",
                    RecommendationType.Financial_Planning,
                    AIPriority.High,
                    "Finance"
                );
            }
            
            // Investment opportunities
            if (econData.ROI > 0.15f && econData.RiskScore < 0.4f)
            {
                CreateRecommendation(
                    "Investment Opportunity",
                    "Favorable conditions for facility expansion",
                    $"Strong ROI ({econData.ROI:P1}) with manageable risk. Consider expansion opportunities.",
                    RecommendationType.Investment,
                    AIPriority.Medium,
                    "Investment"
                );
            }
        }
        
        #endregion
        
        #region Pattern Analysis
        
        private void AnalyzeTrends()
        {
            if (_historicalData.Count < 20) return;
            
            var recentSnapshots = _historicalData.TakeLast(20).ToList();
            
            // Energy efficiency trend
            var energyTrend = AnalyzeEnergyTrend(recentSnapshots);
            if (energyTrend.IsSignificant)
            {
                CreateInsight(
                    "Energy Efficiency Trend",
                    $"Energy efficiency {(energyTrend.IsImproving ? "improving" : "declining")} by {energyTrend.ChangePercent:F1}% over recent period",
                    energyTrend.IsImproving ? InsightSeverity.Positive : InsightSeverity.Warning,
                    "Energy"
                );
            }
            
            // Economic performance trend
            var economicTrend = AnalyzeEconomicTrend(recentSnapshots);
            if (economicTrend.IsSignificant)
            {
                CreateInsight(
                    "Economic Performance Trend",
                    $"Revenue trend shows {economicTrend.ChangePercent:+F1}% change over recent period",
                    economicTrend.IsImproving ? InsightSeverity.Positive : InsightSeverity.Warning,
                    "Economics"
                );
            }
        }
        
        private TrendAnalysis AnalyzeEnergyTrend(List<AnalysisSnapshot> snapshots)
        {
            var energyValues = snapshots.Select(s => s.EnvironmentalData.EnergyUsage).ToList();
            
            if (energyValues.Count < 2) return new TrendAnalysis { IsSignificant = false };
            
            var firstHalf = energyValues.Take(energyValues.Count / 2).Average();
            var secondHalf = energyValues.Skip(energyValues.Count / 2).Average();
            
            var changePercent = ((secondHalf - firstHalf) / firstHalf) * 100f;
            
            return new TrendAnalysis
            {
                IsSignificant = Math.Abs(changePercent) > 5f,
                IsImproving = changePercent < 0, // Lower energy usage is better
                ChangePercent = Math.Abs(changePercent)
            };
        }
        
        private TrendAnalysis AnalyzeEconomicTrend(List<AnalysisSnapshot> snapshots)
        {
            var revenueValues = snapshots.Select(s => s.EconomicData.Revenue).ToList();
            
            if (revenueValues.Count < 2) return new TrendAnalysis { IsSignificant = false };
            
            var firstHalf = revenueValues.Take(revenueValues.Count / 2).Average();
            var secondHalf = revenueValues.Skip(revenueValues.Count / 2).Average();
            
            var changePercent = ((secondHalf - firstHalf) / firstHalf) * 100f;
            
            return new TrendAnalysis
            {
                IsSignificant = Math.Abs(changePercent) > 3f,
                IsImproving = changePercent > 0,
                ChangePercent = Math.Abs(changePercent)
            };
        }
        
        #endregion
        
        #region Optimization Engine
        
        private void CheckForImmediateOptimizations(AnalysisSnapshot snapshot)
        {
            // Environmental optimizations
            if (snapshot.EnvironmentalData.ActiveAlerts > 3)
            {
                CreateOptimizationOpportunity(
                    "Alert Reduction",
                    "Multiple environmental alerts detected",
                    "Review sensor thresholds and automation rules to reduce false alerts",
                    OptimizationType.Environmental,
                    15f, // Estimated benefit score
                    OptimizationComplexity.Low
                );
            }
            
            // Performance optimizations
            if (snapshot.PerformanceData.FrameRate < 55f)
            {
                CreateOptimizationOpportunity(
                    "Performance Optimization",
                    "System performance below target",
                    "Optimize update frequencies and reduce unnecessary calculations",
                    OptimizationType.Performance,
                    25f,
                    OptimizationComplexity.Medium
                );
            }
        }
        
        private void GenerateOptimizationRecommendations()
        {
            // Energy optimization opportunities
            if (_historicalData.Count > 50)
            {
                var avgEnergyUsage = _historicalData.TakeLast(50).Average(s => s.EnvironmentalData.EnergyUsage);
                if (avgEnergyUsage > 1200f)
                {
                    CreateOptimizationOpportunity(
                        "Energy Efficiency Program",
                        "Implement comprehensive energy optimization",
                        "Deploy smart scheduling and load balancing strategies",
                        OptimizationType.Energy,
                        40f,
                        OptimizationComplexity.High
                    );
                }
            }
        }
        
        #endregion
        
        #region Recommendation System
        
        private void CreateRecommendation(string title, string summary, string description, 
            RecommendationType type, AIPriority priority, string category)
        {
            // Check if similar recommendation already exists
            if (_activeRecommendations.Any(r => r.Title == title && r.Status == RecommendationStatus.Active))
                return;
            
            var recommendation = new AIRecommendation
            {
                RecommendationId = Guid.NewGuid().ToString(),
                Title = title,
                Summary = summary,
                Description = description,
                Type = type,
                Priority = priority,
                Category = category,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(_recommendationValidityHours),
                Status = RecommendationStatus.Active,
                ConfidenceScore = CalculateConfidenceScore(type, priority),
                ImpactScore = CalculateImpactScore(type, priority),
                ImplementationComplexity = EstimateComplexity(type),
                EstimatedBenefit = EstimateBenefit(type, priority)
            };
            
            _activeRecommendations.Add(recommendation);
            
            // Limit active recommendations
            if (_activeRecommendations.Count(r => r.Status == RecommendationStatus.Active) > _maxActiveRecommendations)
            {
                var oldestLowPriority = _activeRecommendations
                    .Where(r => r.Status == RecommendationStatus.Active && r.Priority == AIPriority.Low)
                    .OrderBy(r => r.CreatedAt)
                    .FirstOrDefault();
                
                if (oldestLowPriority != null)
                {
                    oldestLowPriority.Status = RecommendationStatus.Superseded;
                }
            }
            
            OnNewRecommendation?.Invoke(recommendation);
            _onNewRecommendation?.Raise("New Recommendation");
            
            LogInfo($"Generated recommendation: {title} ({priority})");
        }
        
        private void CreateInsight(string title, string description, InsightSeverity severity, string category)
        {
            var insight = new DataInsight
            {
                InsightId = Guid.NewGuid().ToString(),
                Title = title,
                Description = description,
                Severity = severity,
                Category = category,
                DiscoveredAt = DateTime.Now,
                DataPoints = _historicalData.Count,
                ConfidenceLevel = CalculateInsightConfidence(severity, _historicalData.Count)
            };
            
            _discoveredInsights.Add(insight);
            
            if (severity == InsightSeverity.Critical)
            {
                OnCriticalInsight?.Invoke(insight);
                _onCriticalInsight?.Raise("Critical Insight");
            }
            
            LogInfo($"Discovered insight: {title} ({severity})");
        }
        
        private void CreateOptimizationOpportunity(string title, string description, string implementation,
            OptimizationType type, float benefitScore, OptimizationComplexity complexity)
        {
            var opportunity = new OptimizationOpportunity
            {
                OpportunityId = Guid.NewGuid().ToString(),
                Title = title,
                Description = description,
                ImplementationPlan = implementation,
                Type = type,
                BenefitScore = benefitScore,
                Complexity = complexity,
                EstimatedROI = benefitScore * 0.1f, // Simplified ROI calculation
                DiscoveredAt = DateTime.Now,
                IsActive = true,
                RequiredSystems = GetRequiredSystemsForOptimization(type)
            };
            
            _optimizationOpportunities.Add(opportunity);
            
            OnOptimizationIdentified?.Invoke(opportunity);
            
            LogInfo($"Identified optimization opportunity: {title} (Benefit: {benefitScore:F1})");
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Gets all active recommendations for the player.
        /// </summary>
        public List<AIRecommendation> GetActiveRecommendations()
        {
            return _activeRecommendations
                .Where(r => r.Status == RecommendationStatus.Active)
                .OrderByDescending(r => r.Priority)
                .ThenByDescending(r => r.ImpactScore)
                .ToList();
        }
        
        /// <summary>
        /// Gets recommendations for a specific category.
        /// </summary>
        public List<AIRecommendation> GetRecommendationsByCategory(string category)
        {
            return _activeRecommendations
                .Where(r => r.Status == RecommendationStatus.Active && r.Category == category)
                .OrderByDescending(r => r.Priority)
                .ToList();
        }
        
        /// <summary>
        /// Marks a recommendation as implemented by the player.
        /// </summary>
        public bool ImplementRecommendation(string recommendationId)
        {
            var recommendation = _activeRecommendations.FirstOrDefault(r => r.RecommendationId == recommendationId);
            if (recommendation == null) return false;
            
            recommendation.Status = RecommendationStatus.Implemented;
            recommendation.ImplementedAt = DateTime.Now;
            
            // Track implementation for learning
            TrackRecommendationImplementation(recommendation);
            
            LogInfo($"Recommendation implemented: {recommendation.Title}");
            return true;
        }
        
        /// <summary>
        /// Dismisses a recommendation as not relevant or needed.
        /// </summary>
        public bool DismissRecommendation(string recommendationId, string reason = null)
        {
            var recommendation = _activeRecommendations.FirstOrDefault(r => r.RecommendationId == recommendationId);
            if (recommendation == null) return false;
            
            recommendation.Status = RecommendationStatus.Dismissed;
            recommendation.DismissalReason = reason;
            
            LogInfo($"Recommendation dismissed: {recommendation.Title}");
            return true;
        }
        
        /// <summary>
        /// Gets current optimization opportunities.
        /// </summary>
        public List<OptimizationOpportunity> GetOptimizationOpportunities()
        {
            return _optimizationOpportunities
                .Where(o => o.IsActive)
                .OrderByDescending(o => o.BenefitScore)
                .ToList();
        }
        
        /// <summary>
        /// Gets recent insights discovered by the AI system.
        /// </summary>
        public List<DataInsight> GetRecentInsights(int count = 10)
        {
            return _discoveredInsights
                .OrderByDescending(i => i.DiscoveredAt)
                .Take(count)
                .ToList();
        }
        
        /// <summary>
        /// Generates a comprehensive performance report.
        /// </summary>
        public AIPerformanceReport GeneratePerformanceReport()
        {
            var latestSnapshot = _historicalData.LastOrDefault();
            if (latestSnapshot == null) return null;
            
            return new AIPerformanceReport
            {
                ReportDate = DateTime.Now,
                OverallEfficiencyScore = SystemEfficiencyScore,
                EnvironmentalScore = CalculateEnvironmentalScore(latestSnapshot),
                EconomicScore = CalculateEconomicScore(latestSnapshot),
                PerformanceScore = CalculatePerformanceScore(latestSnapshot),
                ActiveRecommendations = ActiveRecommendations.Count,
                OptimizationOpportunities = OptimizationOpportunities.Count,
                CriticalInsights = CriticalInsights.Count,
                SystemStatus = GetSystemStatusSummary(),
                Trends = AnalyzeCurrentTrends(),
                Recommendations = GetTopRecommendations(5)
            };
        }
        
        /// <summary>
        /// Processes a user query and generates an AI response.
        /// </summary>
        public void ProcessUserQuery(string query, System.Action<string> responseCallback)
        {
            LogInfo($"Processing user query: {query}");
            
            // Simulate AI processing delay
            StartCoroutine(ProcessQueryCoroutine(query, responseCallback));
        }
        
        private System.Collections.IEnumerator ProcessQueryCoroutine(string query, System.Action<string> responseCallback)
        {
            yield return new WaitForSeconds(1f); // Simulate processing time
            
            string response = GenerateAIResponse(query);
            responseCallback?.Invoke(response);
        }
        
        private string GenerateAIResponse(string query)
        {
            // Simple response generation based on query keywords
            string lowerQuery = query.ToLower();
            
            if (lowerQuery.Contains("temperature") || lowerQuery.Contains("heat"))
                return "Based on current environmental data, I recommend adjusting the HVAC system to maintain optimal temperature ranges for your current growth stage.";
            else if (lowerQuery.Contains("profit") || lowerQuery.Contains("money"))
                return "Your facility is showing positive profit margins. Consider investing in automation upgrades to improve efficiency.";
            else if (lowerQuery.Contains("optimization") || lowerQuery.Contains("improve"))
                return "I've identified several optimization opportunities in your lighting and ventilation systems that could improve yields by 15-20%.";
            else
                return "I'm analyzing your facility data to provide the best recommendations. Please check the dashboard for detailed insights.";
        }
        
        /// <summary>
        /// Analyzes the current facility state and returns insights.
        /// </summary>
        public object AnalyzeFacilityState()
        {
            var snapshot = CaptureCurrentSnapshot();
            
            return new
            {
                OverallScore = CalculateOverallEfficiencyScore(),
                EnvironmentalScore = CalculateEnvironmentalScore(snapshot),
                EconomicScore = CalculateEconomicScore(snapshot),
                PerformanceScore = CalculatePerformanceScore(snapshot),
                CriticalIssues = _discoveredInsights.Where(i => i.Severity == InsightSeverity.Critical).Count(),
                Recommendations = _activeRecommendations.Count,
                SystemStatus = GetSystemStatusSummary(),
                LastAnalysis = DateTime.Now
            };
        }
        
        /// <summary>
        /// Generates predictions based on current data trends.
        /// </summary>
        public object GeneratePredictions()
        {
            return new
            {
                EnergyEfficiencyTrend = "Improving",
                ProfitabilityForecast = "Positive",
                MaintenanceNeeds = new List<string> { "HVAC filter replacement in 2 weeks", "Lighting system calibration recommended" },
                MarketOpportunities = new List<string> { "Cannabis futures showing upward trend", "Equipment upgrade financing available" },
                RiskFactors = new List<string> { "Humidity levels approaching critical threshold", "Energy costs increasing" },
                ConfidenceLevel = 0.85f,
                PredictionHorizon = "30 days",
                LastUpdated = DateTime.Now
            };
        }
        
        /// <summary>
        /// Gets AI data for dashboard display.
        /// </summary>
        public object GetAIData()
        {
            return new
            {
                Status = "Active",
                ProcessingLoad = UnityEngine.Random.Range(0.3f, 0.8f),
                ActiveAnalyses = _models.Count(m => m.Value.IsActive),
                RecommendationsGenerated = _activeRecommendations.Count,
                InsightsDiscovered = _discoveredInsights.Count,
                SystemUptime = "99.7%",
                LastUpdate = DateTime.Now,
                PerformanceMetrics = new
                {
                    ResponseTime = CalculateAverageResponseTime(),
                    Accuracy = _models.Values.Average(m => m.Accuracy),
                    Efficiency = SystemEfficiencyScore
                }
            };
        }
        
        #endregion
        
        #region Helper Methods
        
        private float CalculateOverallEfficiencyScore()
        {
            if (_historicalData.Count == 0) return 0.5f;
            
            var latest = _historicalData.Last();
            
            float environmentalScore = (latest.EnvironmentalData.HVACEfficiency + latest.EnvironmentalData.LightingEfficiency) / 200f;
            float economicScore = latest.EconomicData.Revenue > 0 ? Math.Min(1f, latest.EconomicData.Profit / latest.EconomicData.Revenue * 5f) : 0f;
            float performanceScore = Math.Min(1f, latest.PerformanceData.FrameRate / 60f);
            
            return (environmentalScore + economicScore + performanceScore) / 3f;
        }
        
        private float CalculateConfidenceScore(RecommendationType type, AIPriority priority)
        {
            float baseConfidence = 0.7f;
            
            baseConfidence += (int)priority * 0.1f;
            baseConfidence += _historicalData.Count > 100 ? 0.2f : 0f;
            
            return Math.Min(1f, baseConfidence);
        }
        
        private float CalculateImpactScore(RecommendationType type, AIPriority priority)
        {
            return type switch
            {
                RecommendationType.Critical_Action => 100f,
                RecommendationType.Optimization => 75f,
                RecommendationType.Maintenance => 50f,
                RecommendationType.Business_Strategy => 85f,
                RecommendationType.Investment => 70f,
                _ => 25f
            } * ((int)priority + 1) / 4f;
        }
        
        private string EstimateComplexity(RecommendationType type)
        {
            return type switch
            {
                RecommendationType.Critical_Action => "Low",
                RecommendationType.Alert => "Low", 
                RecommendationType.Maintenance => "Medium",
                RecommendationType.Optimization => "Medium",
                RecommendationType.Business_Strategy => "High",
                RecommendationType.Investment => "High",
                _ => "Medium"
            };
        }
        
        private float EstimateBenefit(RecommendationType type, AIPriority priority)
        {
            float baseBenefit = type switch
            {
                RecommendationType.Critical_Action => 500f,
                RecommendationType.Business_Strategy => 1000f,
                RecommendationType.Investment => 800f,
                RecommendationType.Optimization => 300f,
                RecommendationType.Maintenance => 150f,
                _ => 100f
            };
            
            return baseBenefit * ((int)priority + 1) / 4f;
        }
        
        private float CalculateInsightConfidence(InsightSeverity severity, int dataPoints)
        {
            float baseConfidence = Math.Min(1f, dataPoints / 100f);
            
            return severity switch
            {
                InsightSeverity.Critical => baseConfidence * 0.9f,
                InsightSeverity.Warning => baseConfidence * 0.8f,
                InsightSeverity.Info => baseConfidence * 0.7f,
                InsightSeverity.Positive => baseConfidence * 0.75f,
                _ => baseConfidence * 0.6f
            };
        }
        
        private List<string> GetRequiredSystemsForOptimization(OptimizationType type)
        {
            return type switch
            {
                OptimizationType.Environmental => new List<string> { "HVAC", "Lighting", "Automation" },
                OptimizationType.Economic => new List<string> { "Trading", "Market", "Investment" },
                OptimizationType.Energy => new List<string> { "HVAC", "Lighting", "Automation" },
                OptimizationType.Performance => new List<string> { "All Systems" },
                _ => new List<string>()
            };
        }
        
        private float CalculateAverageResponseTime()
        {
            // Simplified response time calculation
            return UnityEngine.Random.Range(5f, 25f);
        }
        
        private void TrackRecommendationImplementation(AIRecommendation recommendation)
        {
            // Track for machine learning improvements
            // This would update model weights and preferences
        }
        
        private void SetupBaselineRecommendations()
        {
            // Create initial helpful recommendations
            CreateRecommendation(
                "Welcome to AI Advisor",
                "Your intelligent facility management assistant",
                "The AI Advisor will monitor your facility and provide personalized recommendations to optimize operations, reduce costs, and improve efficiency.",
                RecommendationType.Information,
                AIPriority.Low,
                "System"
            );
        }
        
        private void UpdateRecommendationStatus()
        {
            var now = DateTime.Now;
            
            foreach (var recommendation in _activeRecommendations.Where(r => r.Status == RecommendationStatus.Active))
            {
                if (now > recommendation.ExpiresAt)
                {
                    recommendation.Status = RecommendationStatus.Expired;
                }
            }
        }
        
        private void CleanupExpiredData()
        {
            // Remove old recommendations
            _activeRecommendations.RemoveAll(r => r.Status == RecommendationStatus.Expired && 
                (DateTime.Now - r.ExpiresAt).TotalDays > 7);
            
            // Remove old insights
            _discoveredInsights.RemoveAll(i => (DateTime.Now - i.DiscoveredAt).TotalDays > 30);
            
            // Remove inactive optimization opportunities
            _optimizationOpportunities.RemoveAll(o => !o.IsActive && 
                (DateTime.Now - o.DiscoveredAt).TotalDays > 14);
        }
        
        private void UpdatePredictiveModels()
        {
            foreach (var model in _models.Values.Where(m => m.IsActive))
            {
                // Update model with recent data
                model.TrainingDataPoints = _historicalData.Count;
                model.LastTrained = DateTime.Now;
                
                // Simulate accuracy improvement with more data
                if (_historicalData.Count > 500)
                {
                    model.Accuracy = Math.Min(0.95f, model.Accuracy + 0.001f);
                }
            }
        }
        
        private float CalculateEnvironmentalScore(AnalysisSnapshot snapshot)
        {
            var env = snapshot.EnvironmentalData;
            return (env.HVACEfficiency + env.LightingEfficiency + env.SystemUptime) / 300f;
        }
        
        private float CalculateEconomicScore(AnalysisSnapshot snapshot)
        {
            var econ = snapshot.EconomicData;
            if (econ.Revenue <= 0) return 0f;
            
            float profitMargin = econ.Profit / econ.Revenue;
            return Math.Min(1f, profitMargin * 5f); // Normalize to 0-1 scale
        }
        
        private float CalculatePerformanceScore(AnalysisSnapshot snapshot)
        {
            var perf = snapshot.PerformanceData;
            float frameRateScore = Math.Min(1f, perf.FrameRate / 60f);
            float memoryScore = Math.Max(0f, 1f - (perf.MemoryUsage / 1024f));
            
            return (frameRateScore + memoryScore) / 2f;
        }
        
        private string GetSystemStatusSummary()
        {
            // Use simulated system count since we don't have direct manager access
            int availableSystems = UnityEngine.Random.Range(3, 6);
            
            return $"{availableSystems} systems operational";
        }
        
        private List<string> AnalyzeCurrentTrends()
        {
            var trends = new List<string>();
            
            if (_historicalData.Count > 10)
            {
                var energyTrend = AnalyzeEnergyTrend(_historicalData.TakeLast(10).ToList());
                if (energyTrend.IsSignificant)
                {
                    trends.Add($"Energy usage {(energyTrend.IsImproving ? "decreasing" : "increasing")} by {energyTrend.ChangePercent:F1}%");
                }
                
                var economicTrend = AnalyzeEconomicTrend(_historicalData.TakeLast(10).ToList());
                if (economicTrend.IsSignificant)
                {
                    trends.Add($"Revenue {(economicTrend.IsImproving ? "increasing" : "decreasing")} by {economicTrend.ChangePercent:F1}%");
                }
            }
            
            return trends;
        }
        
        private List<AIRecommendation> GetTopRecommendations(int count)
        {
            return GetActiveRecommendations().Take(count).ToList();
        }
        
        /// <summary>
        /// PC-012-2: Generate breeding recommendations based on genetic analysis of available plants
        /// </summary>
        public List<AIBreedingRecommendation> GenerateBreedingRecommendations(List<PlantInstanceSO> availablePlants, BreedingGoals goals = null)
        {
            var recommendations = new List<AIBreedingRecommendation>();
            
            if (availablePlants == null || availablePlants.Count < 2)
            {
                LogWarning("Cannot generate breeding recommendations: Need at least 2 plants");
                return recommendations;
            }
            
            try
            {
                // Analyze genetic diversity of available plants
                var geneticProfiles = AnalyzePlantGenetics(availablePlants);
                
                // Find optimal breeding pairs based on goals
                var breedingPairs = FindOptimalBreedingPairs(geneticProfiles, goals);
                
                // Generate detailed recommendations for each pair
                foreach (var pair in breedingPairs)
                {
                    var recommendation = CreateBreedingRecommendation(pair, goals);
                    if (recommendation != null)
                    {
                        recommendations.Add(recommendation);
                    }
                }
                
                // Add general breeding strategy recommendations
                recommendations.AddRange(GenerateStrategicBreedingAdvice(geneticProfiles, goals));
                
                LogInfo($"Generated {recommendations.Count} breeding recommendations from {availablePlants.Count} available plants");
                
                // Create AI recommendation for the top breeding suggestion
                if (recommendations.Count > 0)
                {
                    CreateBreedingAIRecommendation(recommendations.First());
                }
                
                return recommendations.OrderByDescending(r => r.ExpectedBenefit).ToList();
            }
            catch (System.Exception ex)
            {
                LogError($"Error generating breeding recommendations: {ex.Message}");
                return recommendations;
            }
        }
        
        private List<PlantGeneticProfile> AnalyzePlantGenetics(List<PlantInstanceSO> plants)
        {
            var profiles = new List<PlantGeneticProfile>();
            
            foreach (var plant in plants)
            {
                var profile = new PlantGeneticProfile
                {
                    PlantId = plant.PlantID,
                    PlantName = plant.PlantName,
                    StrainName = plant.Strain?.StrainName ?? "Unknown",
                    MaturityLevel = plant.MaturityLevel,
                    GeneticDiversity = CalculateGeneticDiversity(plant),
                    TraitPotential = CalculateTraitPotential(plant),
                    TraitPotentials = AnalyzeTraitPotentials(plant),
                    DiseaseResistance = CalculateDiseaseResistance(plant),
                    EnvironmentalAdaptability = CalculateEnvironmentalAdaptability(plant),
                    InbreedingRisk = CalculateInbreedingRisk(plant),
                    RarityScore = CalculateRarityScore(plant)
                };
                profiles.Add(profile);
            }
            
            return profiles;
        }
        
        private List<BreedingPairAnalysis> FindOptimalBreedingPairs(List<PlantGeneticProfile> profiles, BreedingGoals goals)
        {
            var pairs = new List<BreedingPairAnalysis>();
            
            // Analyze all possible breeding combinations
            for (int i = 0; i < profiles.Count; i++)
            {
                for (int j = i + 1; j < profiles.Count; j++)
                {
                    var parent1 = profiles[i];
                    var parent2 = profiles[j];
                    
                    // Skip if inbreeding risk is too high
                    if (IsInbreedingRiskTooHigh(parent1, parent2))
                        continue;
                    
                    var analysis = AnalyzeBreedingPair(parent1, parent2, goals);
                    if (analysis.CompatibilityScore > 0.6f) // Only recommend good matches
                    {
                        pairs.Add(analysis);
                    }
                }
            }
            
            return pairs.OrderByDescending(p => p.OverallScore).Take(10).ToList(); // Top 10 pairs
        }
        
        private BreedingPairAnalysis AnalyzeBreedingPair(PlantGeneticProfile parent1, PlantGeneticProfile parent2, BreedingGoals goals)
        {
            var analysis = new BreedingPairAnalysis
            {
                Parent1 = parent1,
                Parent2 = parent2,
                CompatibilityScore = CalculateCompatibilityScore(parent1, parent2),
                GeneticDiversityGain = CalculateGeneticDiversityGain(parent1, parent2),
                TraitImprovementPotential = CalculateTraitImprovementPotential(parent1, parent2, goals),
                HybridVigor = CalculateHybridVigor(parent1, parent2),
                InbreedingRisk = CalculatePairInbreedingRisk(parent1, parent2),
                OutcomePrediction = PredictBreedingOutcomes(parent1, parent2, goals)
            };
            
            // Set expected outcomes as a float value
            analysis.ExpectedOutcomes = analysis.OutcomePrediction != null ? analysis.OutcomePrediction.SuccessProbability : 0.5f;
            
            // Calculate overall score
            analysis.OverallScore = (
                analysis.CompatibilityScore * 0.3f +
                analysis.GeneticDiversityGain * 0.25f +
                analysis.TraitImprovementPotential * 0.25f +
                analysis.HybridVigor * 0.2f
            ) * (1f - analysis.InbreedingRisk * 0.5f); // Penalty for inbreeding risk
            
            return analysis;
        }
        
        private AIBreedingRecommendation CreateBreedingRecommendation(BreedingPairAnalysis analysis, BreedingGoals goals)
        {
            var recommendation = new AIBreedingRecommendation
            {
                RecommendationId = System.Guid.NewGuid().ToString(),
                Parent1Id = analysis.Parent1.PlantId,
                Parent2Id = analysis.Parent2.PlantId,
                Parent1Name = analysis.Parent1.PlantName,
                Parent2Name = analysis.Parent2.PlantName,
                CompatibilityScore = analysis.CompatibilityScore,
                ExpectedBenefit = analysis.OverallScore,
                ConfidenceLevel = CalculateConfidenceLevel(analysis),
                RecommendationType = DetermineRecommendationType(analysis, goals),
                ExpectedTrait = analysis.ExpectedOutcomes,
                ExpectedTraits = analysis.OutcomePrediction?.PredictedTraits ?? new Dictionary<string, float>(),
                PotentialRisks = IdentifyPotentialRisks(analysis),
                RecommendedTiming = DetermineOptimalTiming(analysis),
                EstimatedDuration = "8-12 weeks to maturity",
                SpecialConsiderations = GenerateSpecialConsiderations(analysis, goals),
                PredictedOutcomes = analysis.OutcomePrediction,
                CreatedAt = System.DateTime.Now,
                Breeding = "Standard Cross-Pollination"
            };
            
            // Generate detailed reasoning
            recommendation.Reasoning = GenerateBreedingReasoning(analysis, goals);
            
            return recommendation;
        }
        
        private List<AIBreedingRecommendation> GenerateStrategicBreedingAdvice(List<PlantGeneticProfile> profiles, BreedingGoals goals)
        {
            var strategicAdvice = new List<AIBreedingRecommendation>();
            
            // Genetic diversity recommendations
            if (IsGeneticDiversityLow(profiles))
            {
                strategicAdvice.Add(CreateDiversityRecommendation(profiles));
            }
            
            // Trait improvement focus recommendations
            if (goals != null)
            {
                strategicAdvice.AddRange(CreateTraitFocusRecommendations(profiles, goals));
            }
            
            // Long-term breeding program recommendations
            strategicAdvice.Add(CreateLongTermBreedingStrategy(profiles, goals));
            
            return strategicAdvice;
        }
        
        private void CreateBreedingAIRecommendation(AIBreedingRecommendation breedingRec)
        {
            var aiRecommendation = new AIRecommendation
            {
                RecommendationId = System.Guid.NewGuid().ToString(),
                Title = $"Optimal Breeding Opportunity: {breedingRec.Parent1Name} × {breedingRec.Parent2Name}",
                Summary = $"High-potential breeding pair identified with {breedingRec.CompatibilityScore:P1} compatibility",
                Description = $"Analysis indicates this breeding combination could yield excellent results. {breedingRec.Reasoning}",
                Type = RecommendationType.Breeding,
                Priority = breedingRec.ExpectedBenefit > 0.8f ? AIPriority.High : AIPriority.Medium,
                Category = "Genetics",
                CreatedAt = System.DateTime.Now,
                ExpiresAt = System.DateTime.Now.AddDays(7),
                Status = RecommendationStatus.Active,
                ConfidenceScore = breedingRec.ConfidenceLevel,
                ImpactScore = breedingRec.ExpectedBenefit,
                ImplementationComplexity = "Medium",
                EstimatedBenefit = breedingRec.ExpectedBenefit,
                RequiredActions = new List<string>
                {
                    $"Select {breedingRec.Parent1Name} as parent 1",
                    $"Select {breedingRec.Parent2Name} as parent 2",
                    "Initiate controlled breeding process",
                    "Monitor offspring development",
                    "Record genetic outcomes"
                },
                RelatedSystems = new List<string> { "Genetics", "Breeding", "Cultivation" }
            };
            
            // Add metadata
            aiRecommendation.Metadata["breeding_pair_id"] = breedingRec.RecommendationId;
            aiRecommendation.Metadata["parent1_id"] = breedingRec.Parent1Id;
            aiRecommendation.Metadata["parent2_id"] = breedingRec.Parent2Id;
            aiRecommendation.Metadata["compatibility_score"] = breedingRec.CompatibilityScore;
            
            _activeRecommendations.Add(aiRecommendation);
        }
        
        #region Breeding Analysis Helper Methods
        
        private float CalculateMaturityLevel(PlantInstanceSO plant)
        {
            // Simplified maturity calculation using age in days
            return Mathf.Clamp01(plant.AgeInDays / 90f); // Assume 90 days to full maturity
        }
        
        private float CalculateGeneticDiversity(PlantInstanceSO plant)
        {
            // Simplified genetic diversity - in real system would analyze actual genetic markers
            return UnityEngine.Random.Range(0.4f, 1.0f);
        }
        
        private float CalculateTraitPotential(PlantInstanceSO plant)
        {
            // Calculate trait potential based on plant characteristics
            return (plant.OverallHealth + plant.Vigor + plant.MaturityLevel) / 3f;
        }
        
        private Dictionary<string, float> AnalyzeTraitPotentials(PlantInstanceSO plant)
        {
            return new Dictionary<string, float>
            {
                {"Overall_Health", plant.OverallHealth},
                {"Vigor", plant.Vigor},
                {"Maturity", plant.MaturityLevel},
                {"Stress_Tolerance", 1f - plant.StressLevel},
                {"Immune_Response", plant.ImmuneResponse},
                {"Growth_Rate", plant.DailyGrowthRate},
                {"Biomass_Accumulation", plant.BiomassAccumulation}
            };
        }
        
        private float CalculateDiseaseResistance(PlantInstanceSO plant)
        {
            // Disease resistance based on immune response and overall health
            return (plant.ImmuneResponse + plant.OverallHealth) / 2f;
        }
        
        private float CalculateEnvironmentalAdaptability(PlantInstanceSO plant)
        {
            // Environmental adaptability based on stress tolerance and vigor
            return (plant.Vigor + (1f - plant.StressLevel)) / 2f;
        }
        
        private float CalculateInbreedingRisk(PlantInstanceSO plant)
        {
            // Simplified - would check actual genetic lineage in real system
            return UnityEngine.Random.Range(0f, 0.3f);
        }
        
        private float CalculateRarityScore(PlantInstanceSO plant)
        {
            // Factor in strain rarity and unique trait combinations
            return plant.Strain?.RarityScore ?? 0.5f;
        }
        
        private bool IsInbreedingRiskTooHigh(PlantGeneticProfile parent1, PlantGeneticProfile parent2)
        {
            return (parent1.InbreedingRisk + parent2.InbreedingRisk) > 0.6f;
        }
        
        private float CalculateCompatibilityScore(PlantGeneticProfile parent1, PlantGeneticProfile parent2)
        {
            // Calculate genetic compatibility based on diversity and complementary traits
            float diversityScore = (parent1.GeneticDiversity + parent2.GeneticDiversity) / 2f;
            float traitComplementarity = CalculateTraitComplementarity(parent1.TraitPotentials, parent2.TraitPotentials);
            return (diversityScore + traitComplementarity) / 2f;
        }
        
        private float CalculateTraitComplementarity(Dictionary<string, float> traits1, Dictionary<string, float> traits2)
        {
            float complementarity = 0f;
            int traitCount = 0;
            
            foreach (var trait in traits1.Keys)
            {
                if (traits2.ContainsKey(trait))
                {
                    // Higher complementarity when traits balance each other out
                    float difference = Mathf.Abs(traits1[trait] - traits2[trait]);
                    complementarity += 1f - (difference / 2f); // Normalize difference
                    traitCount++;
                }
            }
            
            return traitCount > 0 ? complementarity / traitCount : 0f;
        }
        
        private float CalculateGeneticDiversityGain(PlantGeneticProfile parent1, PlantGeneticProfile parent2)
        {
            return Mathf.Abs(parent1.GeneticDiversity - parent2.GeneticDiversity);
        }
        
        private float CalculateTraitImprovementPotential(PlantGeneticProfile parent1, PlantGeneticProfile parent2, BreedingGoals goals)
        {
            if (goals == null) return 0.5f;
            
            float potential = 0f;
            int goalCount = 0;
            
            foreach (var goal in goals.TargetTraits)
            {
                if (parent1.TraitPotentials.ContainsKey(goal) && parent2.TraitPotentials.ContainsKey(goal))
                {
                    float combinedPotential = (parent1.TraitPotentials[goal] + parent2.TraitPotentials[goal]) / 2f;
                    float targetValue = goals.TraitPriorities.ContainsKey(goal) ? goals.TraitPriorities[goal] : 1f;
                    float goalAlignment = 1f - Mathf.Abs(combinedPotential - targetValue);
                    potential += goalAlignment;
                    goalCount++;
                }
            }
            
            return goalCount > 0 ? potential / goalCount : 0.5f;
        }
        
        private float CalculateHybridVigor(PlantGeneticProfile parent1, PlantGeneticProfile parent2)
        {
            // Hybrid vigor is higher when parents are genetically diverse but compatible
            float diversityDifference = Mathf.Abs(parent1.GeneticDiversity - parent2.GeneticDiversity);
            return Mathf.Clamp01(diversityDifference * 2f); // Scale to 0-1 range
        }
        
        private float CalculatePairInbreedingRisk(PlantGeneticProfile parent1, PlantGeneticProfile parent2)
        {
            return (parent1.InbreedingRisk + parent2.InbreedingRisk) / 2f;
        }
        
        private BreedingOutcomePrediction PredictBreedingOutcomes(PlantGeneticProfile parent1, PlantGeneticProfile parent2, BreedingGoals goals)
        {
            var prediction = new BreedingOutcomePrediction
            {
                PredictedTraits = new Dictionary<string, float>(),
                SuccessProbability = CalculateCompatibilityScore(parent1, parent2),
                ExpectedVariation = CalculateExpectedVariation(parent1, parent2),
                GenerationTime = 10f // Average 10 weeks
            };
            
            // Predict trait outcomes
            foreach (var trait in parent1.TraitPotentials.Keys)
            {
                if (parent2.TraitPotentials.ContainsKey(trait))
                {
                    float avgTrait = (parent1.TraitPotentials[trait] + parent2.TraitPotentials[trait]) / 2f;
                    float hybridBonus = CalculateHybridVigor(parent1, parent2) * 0.1f;
                    prediction.PredictedTraits[trait] = Mathf.Clamp01(avgTrait + hybridBonus);
                }
            }
            
            return prediction;
        }
        
        private float CalculateExpectedVariation(PlantGeneticProfile parent1, PlantGeneticProfile parent2)
        {
            return (parent1.GeneticDiversity + parent2.GeneticDiversity) / 2f;
        }
        
        private float CalculateConfidenceLevel(BreedingPairAnalysis analysis)
        {
            // Confidence based on data quality and genetic stability
            float baseConfidence = 0.7f;
            float diversityBonus = analysis.GeneticDiversityGain * 0.2f;
            float compatibilityBonus = analysis.CompatibilityScore * 0.1f;
            return Mathf.Clamp01(baseConfidence + diversityBonus + compatibilityBonus);
        }
        
        private string DetermineRecommendationType(BreedingPairAnalysis analysis, BreedingGoals goals)
        {
            if (analysis.HybridVigor > 0.8f) return "Hybrid Vigor Enhancement";
            if (analysis.TraitImprovementPotential > 0.8f) return "Trait Optimization";
            if (analysis.GeneticDiversityGain > 0.6f) return "Genetic Diversity Expansion";
            return "General Improvement";
        }
        
        private List<string> IdentifyPotentialRisks(BreedingPairAnalysis analysis)
        {
            var risks = new List<string>();
            
            if (analysis.InbreedingRisk > 0.3f)
                risks.Add("Moderate inbreeding risk - monitor offspring closely");
            
            if (analysis.CompatibilityScore < 0.7f)
                risks.Add("Lower compatibility may result in inconsistent offspring");
            
            if (analysis.GeneticDiversityGain < 0.2f)
                risks.Add("Limited genetic diversity gain expected");
            
            return risks;
        }
        
        private string DetermineOptimalTiming(BreedingPairAnalysis analysis)
        {
            // Factor in plant maturity and breeding seasons
            return "Next available breeding cycle";
        }
        
        private List<string> GenerateSpecialConsiderations(BreedingPairAnalysis analysis, BreedingGoals goals)
        {
            var considerations = new List<string>();
            
            if (analysis.HybridVigor > 0.7f)
                considerations.Add("High hybrid vigor expected - consider large breeding batch");
            
            if (goals?.Priority.ContainsKey("THC") == true && goals.Priority["THC"] > 0.8f)
                considerations.Add("Focus on high-THC parent selection for optimal results");
            
            considerations.Add("Monitor environmental conditions closely during breeding");
            considerations.Add("Document all genetic outcomes for future reference");
            
            return considerations;
        }
        
        private string GenerateBreedingReasoning(BreedingPairAnalysis analysis, BreedingGoals goals)
        {
            var reasoning = $"This pairing shows {analysis.CompatibilityScore:P1} genetic compatibility with excellent potential for ";
            
            if (analysis.HybridVigor > 0.7f)
                reasoning += "hybrid vigor enhancement. ";
            else if (analysis.TraitImprovementPotential > 0.7f)
                reasoning += "significant trait improvements. ";
            else
                reasoning += "steady genetic progress. ";
            
            if (analysis.GeneticDiversityGain > 0.5f)
                reasoning += "The combination will also expand genetic diversity in your breeding program.";
            
            return reasoning;
        }
        
        private bool IsGeneticDiversityLow(List<PlantGeneticProfile> profiles)
        {
            return profiles.Average(p => p.GeneticDiversity) < 0.6f;
        }
        
        private AIBreedingRecommendation CreateDiversityRecommendation(List<PlantGeneticProfile> profiles)
        {
            return new AIBreedingRecommendation
            {
                RecommendationId = System.Guid.NewGuid().ToString(),
                RecommendationType = "Genetic Diversity Enhancement",
                Reasoning = "Current breeding population shows limited genetic diversity. Consider introducing new genetic lines or focusing on outcrossing.",
                ExpectedBenefit = 0.8f,
                ConfidenceLevel = 0.9f,
                RecommendedTiming = "Immediate attention recommended",
                SpecialConsiderations = new List<string>
                {
                    "Acquire new breeding stock from different genetic lineages",
                    "Avoid close relative breeding for next 2-3 generations",
                    "Document genetic markers to track diversity improvements"
                }
            };
        }
        
        private List<AIBreedingRecommendation> CreateTraitFocusRecommendations(List<PlantGeneticProfile> profiles, BreedingGoals goals)
        {
            var recommendations = new List<AIBreedingRecommendation>();
            
            foreach (var targetTrait in goals.TargetTraits)
            {
                var topPerformers = profiles
                    .Where(p => p.TraitPotentials.ContainsKey(targetTrait))
                    .OrderByDescending(p => p.TraitPotentials[targetTrait])
                    .Take(3)
                    .ToList();
                
                if (topPerformers.Count >= 2)
                {
                    recommendations.Add(new AIBreedingRecommendation
                    {
                        RecommendationId = System.Guid.NewGuid().ToString(),
                        RecommendationType = $"{targetTrait} Optimization",
                        Reasoning = $"Focus breeding efforts on top {targetTrait} performers to achieve breeding goals.",
                        ExpectedBenefit = 0.75f,
                        ConfidenceLevel = 0.8f,
                        Parent1Name = topPerformers[0].PlantName,
                        Parent2Name = topPerformers[1].PlantName,
                        ExpectedTraits = new Dictionary<string, float> { { targetTrait, 0.85f } }
                    });
                }
            }
            
            return recommendations;
        }
        
        private AIBreedingRecommendation CreateLongTermBreedingStrategy(List<PlantGeneticProfile> profiles, BreedingGoals goals)
        {
            return new AIBreedingRecommendation
            {
                RecommendationId = System.Guid.NewGuid().ToString(),
                RecommendationType = "Long-term Breeding Strategy",
                Reasoning = "Establish a systematic breeding program with clear genetic objectives and population management.",
                ExpectedBenefit = 0.9f,
                ConfidenceLevel = 0.85f,
                EstimatedDuration = "6-12 months for full program establishment",
                SpecialConsiderations = new List<string>
                {
                    "Establish breeding population of 10-15 genetically diverse individuals",
                    "Implement rotation breeding to prevent inbreeding depression",
                    "Set up genetic tracking and record-keeping system",
                    "Plan for 3-4 generation advancement program",
                    "Regular genetic evaluation and selection pressure application"
                }
            };
        }
        
        #endregion
        
        protected override void OnManagerShutdown()
        {
            _activeRecommendations.Clear();
            _discoveredInsights.Clear();
            IdentifiedPatterns.Clear();
            _optimizationOpportunities.Clear();
            _models.Clear();
            _historicalData.Clear();
            
            LogInfo("AIAdvisorManager shutdown complete");
        }
        
        #endregion
        
        #region System Performance Analysis
        
        private void AnalyzeSystemPerformance(AnalysisSnapshot snapshot)
        {
            var perfData = snapshot.PerformanceData;
            
            // Frame rate analysis
            if (perfData.FrameRate < 30f)
            {
                CreateRecommendation(
                    "Performance Optimization",
                    "Frame rate below optimal threshold",
                    $"Current FPS: {perfData.FrameRate:F1}. Consider reducing visual complexity or optimizing systems.",
                    RecommendationType.Performance,
                    AIPriority.High,
                    "Performance"
                );
            }
            
            // Memory usage analysis
            if (perfData.MemoryUsage > 1024f) // Over 1GB
            {
                CreateRecommendation(
                    "Memory Usage Alert",
                    "High memory consumption detected",
                    $"Memory usage: {perfData.MemoryUsage:F1}MB. Review system efficiency and data cleanup.",
                    RecommendationType.Optimization,
                    AIPriority.Medium,
                    "Performance"
                );
            }
        }
        
        private void IdentifyPerformancePatterns()
        {
            LogInfo("Analyzing performance patterns...");
            
            if (_historicalData.Count < 5) return;
            
            var recentSnapshots = _historicalData.TakeLast(10).ToList();
            
            // Identify declining performance trends
            var avgFrameRate = recentSnapshots.Average(s => s.PerformanceData.FrameRate);
            var avgMemoryUsage = recentSnapshots.Average(s => s.PerformanceData.MemoryUsage);
            
            if (avgFrameRate < 45f)
            {
                CreateInsight(
                    "Performance Degradation Pattern",
                    $"Average frame rate trending downward: {avgFrameRate:F1} FPS",
                    InsightSeverity.Warning,
                    "Performance"
                );
            }
            
            if (avgMemoryUsage > 768f)
            {
                CreateInsight(
                    "Memory Usage Pattern",
                    $"Memory usage consistently high: {avgMemoryUsage:F1}MB",
                    InsightSeverity.Info,
                    "Performance"
                );
            }
        }
        
        private void AnalyzeLongTermPerformance()
        {
            LogInfo("Analyzing long-term performance trends...");
            
            if (_historicalData.Count < 50) return;
            
            var allSnapshots = _historicalData.ToList();
            var recentSnapshots = allSnapshots.TakeLast(20).ToList();
            var olderSnapshots = allSnapshots.Take(20).ToList();
            
            // Compare recent vs historical performance
            var recentAvgFPS = recentSnapshots.Average(s => s.PerformanceData.FrameRate);
            var historicalAvgFPS = olderSnapshots.Average(s => s.PerformanceData.FrameRate);
            
            var performanceChange = (recentAvgFPS - historicalAvgFPS) / historicalAvgFPS;
            
            if (performanceChange < -0.1f) // 10% decline
            {
                CreateInsight(
                    "Long-term Performance Decline",
                    $"Performance has declined by {Math.Abs(performanceChange * 100):F1}% over time",
                    InsightSeverity.Warning,
                    "Performance"
                );
            }
            else if (performanceChange > 0.1f) // 10% improvement
            {
                CreateInsight(
                    "Performance Improvement",
                    $"Performance has improved by {performanceChange * 100:F1}% over time",
                    InsightSeverity.Info,
                    "Performance"
                );
            }
        }
        
        private void IdentifyStrategicOpportunities()
        {
            LogInfo("Identifying strategic opportunities...");
            
            // Use simulated data to identify opportunities
            float simulatedFinancialHealth = UnityEngine.Random.Range(0.6f, 0.9f);
            
            if (simulatedFinancialHealth > 0.8f)
            {
                CreateOptimizationOpportunity(
                    "Expansion Opportunity",
                    "Strong financial position indicates potential for facility expansion",
                    "Consider investing in additional cultivation areas or advanced equipment",
                    OptimizationType.Strategic,
                    0.85f,
                    OptimizationComplexity.High
                );
            }
            
            // Simulate automation opportunity analysis
            int simulatedSensorCount = UnityEngine.Random.Range(5, 15);
            if (simulatedSensorCount < 10)
            {
                CreateOptimizationOpportunity(
                    "Automation Enhancement",
                    "Low sensor density presents automation expansion opportunity",
                    "Add environmental sensors for improved monitoring and control",
                    OptimizationType.Automation,
                    0.65f,
                    OptimizationComplexity.Medium
                );
            }
        }
        
        private void GenerateBusinessRecommendations()
        {
            LogInfo("Generating business recommendations...");
            
            // Generate general business recommendations since we don't have direct manager access
            CreateRecommendation(
                "Market Analysis Review",
                "Regular market analysis recommended",
                "Review current market trends and adjust production accordingly",
                RecommendationType.Strategic,
                AIPriority.Low,
                "Business"
            );
            
            CreateRecommendation(
                "Skill Development Focus",
                "Strategic skill development recommended",
                "Focus on unlocking skills that complement current facility capabilities",
                RecommendationType.Development,
                AIPriority.Low,
                "Business"
            );
        }
        
        private void UpdateSystemEfficiencyMetrics()
        {
            LogInfo("Updating system efficiency metrics...");
            
            // Use simulated efficiency metrics since we don't have direct manager access
            float automationEfficiency = UnityEngine.Random.Range(0.85f, 0.95f);
            float hvacEfficiency = UnityEngine.Random.Range(0.80f, 0.90f);
            float lightingEfficiency = UnityEngine.Random.Range(0.83f, 0.93f);
            
            float avgEfficiency = (automationEfficiency + hvacEfficiency + lightingEfficiency) / 3f;
            
            if (avgEfficiency < 0.8f)
            {
                CreateInsight(
                    "System Efficiency Alert",
                    $"Overall system efficiency below target: {avgEfficiency * 100:F1}%",
                    InsightSeverity.Warning,
                    "Efficiency"
                );
            }
        }
        
        #endregion

        #region Environmental Optimization - PC-012-3
        
        /// <summary>
        /// PC-012-3: Generate environmental optimization suggestions based on current conditions and plant needs
        /// </summary>
        public List<EnvironmentalOptimization> GenerateEnvironmentalOptimizations(List<PlantInstanceSO> plants = null, EnvironmentalConditions currentConditions = null)
        {
            var optimizations = new List<EnvironmentalOptimization>();
            
            if (currentConditions == null)
            {
                LogWarning("No environmental conditions provided - using simulated data");
                currentConditions = GetCurrentEnvironmentalConditions();
            }
            
            try
            {
                // Analyze current environmental conditions
                var environmentalAnalysis = AnalyzeEnvironmentalConditions(currentConditions, plants);
                
                // Generate temperature optimizations
                optimizations.AddRange(GenerateTemperatureOptimizations(environmentalAnalysis));
                
                // Generate humidity optimizations
                optimizations.AddRange(GenerateHumidityOptimizations(environmentalAnalysis));
                
                // Generate lighting optimizations
                optimizations.AddRange(GenerateLightingOptimizations(environmentalAnalysis));
                
                // Generate airflow optimizations
                optimizations.AddRange(GenerateAirflowOptimizations(environmentalAnalysis));
                
                // Generate CO2 optimizations
                optimizations.AddRange(GenerateCO2Optimizations(environmentalAnalysis));
                
                // Generate nutrient optimizations
                optimizations.AddRange(GenerateNutrientOptimizations(environmentalAnalysis));
                
                // Generate VPD optimizations
                optimizations.AddRange(GenerateVPDOptimizations(environmentalAnalysis));
                
                // Create AI recommendations for top optimizations
                var topOptimizations = optimizations
                    .Where(o => o.Priority >= EnvironmentalPriority.High)
                    .OrderByDescending(o => o.ImpactScore)
                    .Take(3);
                
                foreach (var optimization in topOptimizations)
                {
                    CreateEnvironmentalAIRecommendation(optimization);
                }
                
                LogInfo($"Generated {optimizations.Count} environmental optimizations");
                return optimizations.OrderByDescending(o => o.ImpactScore).ToList();
            }
            catch (System.Exception ex)
            {
                LogError($"Error generating environmental optimizations: {ex.Message}");
                return optimizations;
            }
        }
        
        private EnvironmentalAnalysis AnalyzeEnvironmentalConditions(EnvironmentalConditions conditions, List<PlantInstanceSO> plants)
        {
            var analysis = new EnvironmentalAnalysis
            {
                CurrentConditions = conditions,
                PlantCount = plants?.Count ?? 0,
                AnalysisTime = System.DateTime.Now
            };
            
            // Analyze temperature conditions
            analysis.TemperatureStatus = AnalyzeTemperatureStatus(conditions, plants);
            analysis.HumidityStatus = AnalyzeHumidityStatus(conditions, plants);
            analysis.LightingStatus = AnalyzeLightingStatus(conditions, plants);
            analysis.AirflowStatus = AnalyzeAirflowStatus(conditions, plants);
            analysis.CO2Status = AnalyzeCO2Status(conditions, plants);
            analysis.NutrientStatus = AnalyzeNutrientStatus(conditions, plants);
            
            // Calculate overall environmental efficiency
            var statusScores = new float[]
            {
                analysis.TemperatureStatus.EfficiencyScore,
                analysis.HumidityStatus.EfficiencyScore,
                analysis.LightingStatus.EfficiencyScore,
                analysis.AirflowStatus.EfficiencyScore,
                analysis.CO2Status.EfficiencyScore,
                analysis.NutrientStatus.EfficiencyScore
            };
            
            analysis.OverallEfficiency = statusScores.Average();
            analysis.CriticalIssues = statusScores.Count(s => s < 0.6f);
            analysis.OptimizationPotential = CalculateOptimizationPotential(statusScores);
            
            return analysis;
        }
        
        private List<EnvironmentalOptimization> GenerateTemperatureOptimizations(EnvironmentalAnalysis analysis)
        {
            var optimizations = new List<EnvironmentalOptimization>();
            var tempStatus = analysis.TemperatureStatus;
            
            if (tempStatus.EfficiencyScore < 0.8f)
            {
                var optimization = new EnvironmentalOptimization
                {
                    OptimizationId = System.Guid.NewGuid().ToString(),
                    Category = EnvironmentalCategory.Temperature,
                    Title = "Temperature Control Optimization",
                    Description = $"Current temperature ({tempStatus.CurrentValue:F1}°C) is not optimal for current growth stage",
                    RecommendedAction = DetermineTemperatureAction(tempStatus),
                    Priority = DetermineTemperaturePriority(tempStatus),
                    ImpactScore = CalculateTemperatureImpact(tempStatus),
                    ImplementationComplexity = EnvironmentalComplexity.Medium,
                    EstimatedCost = CalculateTemperatureCost(tempStatus),
                    ExpectedImprovement = CalculateTemperatureImprovement(tempStatus),
                    TimeToImplement = "15-30 minutes",
                    RequiredEquipment = GetTemperatureEquipment(tempStatus)
                };
                
                optimizations.Add(optimization);
            }
            
            return optimizations;
        }
        
        private List<EnvironmentalOptimization> GenerateHumidityOptimizations(EnvironmentalAnalysis analysis)
        {
            var optimizations = new List<EnvironmentalOptimization>();
            var humidityStatus = analysis.HumidityStatus;
            
            if (humidityStatus.EfficiencyScore < 0.8f)
            {
                optimizations.Add(new EnvironmentalOptimization
                {
                    OptimizationId = System.Guid.NewGuid().ToString(),
                    Category = EnvironmentalCategory.Humidity,
                    Title = "Humidity Level Adjustment",
                    Description = $"Current RH ({humidityStatus.CurrentValue:F1}%) requires adjustment for optimal plant health",
                    RecommendedAction = DetermineHumidityAction(humidityStatus),
                    Priority = DetermineHumidityPriority(humidityStatus),
                    ImpactScore = CalculateHumidityImpact(humidityStatus),
                    ImplementationComplexity = EnvironmentalComplexity.Low,
                    EstimatedCost = CalculateHumidityCost(humidityStatus),
                    ExpectedImprovement = CalculateHumidityImprovement(humidityStatus),
                    TimeToImplement = "10-20 minutes",
                    RequiredEquipment = GetHumidityEquipment(humidityStatus)
                });
            }
            
            return optimizations;
        }
        
        private List<EnvironmentalOptimization> GenerateLightingOptimizations(EnvironmentalAnalysis analysis)
        {
            var optimizations = new List<EnvironmentalOptimization>();
            var lightStatus = analysis.LightingStatus;
            
            if (lightStatus.EfficiencyScore < 0.85f)
            {
                optimizations.Add(new EnvironmentalOptimization
                {
                    OptimizationId = System.Guid.NewGuid().ToString(),
                    Category = EnvironmentalCategory.Lighting,
                    Title = "Lighting Schedule Optimization",
                    Description = $"Current PPFD ({lightStatus.CurrentValue:F0} μmol/m²/s) can be optimized for better growth",
                    RecommendedAction = DetermineLightingAction(lightStatus),
                    Priority = DetermineLightingPriority(lightStatus),
                    ImpactScore = CalculateLightingImpact(lightStatus),
                    ImplementationComplexity = EnvironmentalComplexity.Medium,
                    EstimatedCost = CalculateLightingCost(lightStatus),
                    ExpectedImprovement = CalculateLightingImprovement(lightStatus),
                    TimeToImplement = "30-45 minutes",
                    RequiredEquipment = GetLightingEquipment(lightStatus)
                });
            }
            
            return optimizations;
        }
        
        private List<EnvironmentalOptimization> GenerateAirflowOptimizations(EnvironmentalAnalysis analysis)
        {
            var optimizations = new List<EnvironmentalOptimization>();
            var airflowStatus = analysis.AirflowStatus;
            
            if (airflowStatus.EfficiencyScore < 0.75f)
            {
                optimizations.Add(new EnvironmentalOptimization
                {
                    OptimizationId = System.Guid.NewGuid().ToString(),
                    Category = EnvironmentalCategory.Airflow,
                    Title = "Air Circulation Enhancement",
                    Description = "Improve air circulation to prevent hot spots and humidity pockets",
                    RecommendedAction = DetermineAirflowAction(airflowStatus),
                    Priority = DetermineAirflowPriority(airflowStatus),
                    ImpactScore = CalculateAirflowImpact(airflowStatus),
                    ImplementationComplexity = EnvironmentalComplexity.Low,
                    EstimatedCost = CalculateAirflowCost(airflowStatus),
                    ExpectedImprovement = CalculateAirflowImprovement(airflowStatus),
                    TimeToImplement = "15-25 minutes",
                    RequiredEquipment = GetAirflowEquipment(airflowStatus)
                });
            }
            
            return optimizations;
        }
        
        private List<EnvironmentalOptimization> GenerateCO2Optimizations(EnvironmentalAnalysis analysis)
        {
            var optimizations = new List<EnvironmentalOptimization>();
            var co2Status = analysis.CO2Status;
            
            if (co2Status.EfficiencyScore < 0.7f)
            {
                optimizations.Add(new EnvironmentalOptimization
                {
                    OptimizationId = System.Guid.NewGuid().ToString(),
                    Category = EnvironmentalCategory.CO2,
                    Title = "CO2 Level Optimization",
                    Description = $"Current CO2 ({co2Status.CurrentValue:F0} ppm) is below optimal for photosynthesis",
                    RecommendedAction = DetermineCO2Action(co2Status),
                    Priority = DetermineCO2Priority(co2Status),
                    ImpactScore = CalculateCO2Impact(co2Status),
                    ImplementationComplexity = EnvironmentalComplexity.High,
                    EstimatedCost = CalculateCO2Cost(co2Status),
                    ExpectedImprovement = CalculateCO2Improvement(co2Status),
                    TimeToImplement = "1-2 hours",
                    RequiredEquipment = GetCO2Equipment(co2Status)
                });
            }
            
            return optimizations;
        }
        
        private List<EnvironmentalOptimization> GenerateNutrientOptimizations(EnvironmentalAnalysis analysis)
        {
            var optimizations = new List<EnvironmentalOptimization>();
            var nutrientStatus = analysis.NutrientStatus;
            
            if (nutrientStatus.EfficiencyScore < 0.8f)
            {
                optimizations.Add(new EnvironmentalOptimization
                {
                    OptimizationId = System.Guid.NewGuid().ToString(),
                    Category = EnvironmentalCategory.Nutrients,
                    Title = "Nutrient Solution Adjustment",
                    Description = "Optimize nutrient concentrations for current growth stage",
                    RecommendedAction = DetermineNutrientAction(nutrientStatus),
                    Priority = DetermineNutrientPriority(nutrientStatus),
                    ImpactScore = CalculateNutrientImpact(nutrientStatus),
                    ImplementationComplexity = EnvironmentalComplexity.Medium,
                    EstimatedCost = CalculateNutrientCost(nutrientStatus),
                    ExpectedImprovement = CalculateNutrientImprovement(nutrientStatus),
                    TimeToImplement = "20-30 minutes",
                    RequiredEquipment = GetNutrientEquipment(nutrientStatus)
                });
            }
            
            return optimizations;
        }
        
        private List<EnvironmentalOptimization> GenerateVPDOptimizations(EnvironmentalAnalysis analysis)
        {
            var optimizations = new List<EnvironmentalOptimization>();
            
            // Calculate current VPD
            var currentVPD = CalculateVPD(analysis.CurrentConditions.Temperature, analysis.CurrentConditions.Humidity);
            var optimalVPD = GetOptimalVPD(analysis.PlantCount > 0 ? PlantGrowthStage.Vegetative : PlantGrowthStage.Vegetative);
            var vpdDeviation = Mathf.Abs(currentVPD - optimalVPD);
            
            if (vpdDeviation > 0.3f)
            {
                optimizations.Add(new EnvironmentalOptimization
                {
                    OptimizationId = System.Guid.NewGuid().ToString(),
                    Category = EnvironmentalCategory.VPD,
                    Title = "Vapor Pressure Deficit Optimization",
                    Description = $"Current VPD ({currentVPD:F2} kPa) deviates from optimal range",
                    RecommendedAction = DetermineVPDAction(currentVPD, optimalVPD),
                    Priority = vpdDeviation > 0.5f ? EnvironmentalPriority.High : EnvironmentalPriority.Medium,
                    ImpactScore = CalculateVPDImpact(vpdDeviation),
                    ImplementationComplexity = EnvironmentalComplexity.Medium,
                    EstimatedCost = 25f,
                    ExpectedImprovement = CalculateVPDImprovement(vpdDeviation),
                    TimeToImplement = "15-30 minutes",
                    RequiredEquipment = new List<string> { "Temperature control", "Humidity control" }
                });
            }
            
            return optimizations;
        }
        
        private void CreateEnvironmentalAIRecommendation(EnvironmentalOptimization optimization)
        {
            var aiRecommendation = new AIRecommendation
            {
                RecommendationId = System.Guid.NewGuid().ToString(),
                Title = $"Environment: {optimization.Title}",
                Summary = optimization.Description,
                Description = $"{optimization.Description}\n\nRecommended Action: {optimization.RecommendedAction}\n\nExpected Improvement: {optimization.ExpectedImprovement:P1}",
                Type = RecommendationType.Optimization,
                Priority = ConvertEnvironmentalPriority(optimization.Priority),
                Category = "Environmental",
                CreatedAt = System.DateTime.Now,
                ExpiresAt = System.DateTime.Now.AddHours(4), // Environmental conditions change frequently
                Status = RecommendationStatus.Active,
                ConfidenceScore = 0.9f, // High confidence in environmental data
                ImpactScore = optimization.ImpactScore,
                EstimatedBenefit = optimization.ExpectedImprovement,
                ImplementationComplexity = optimization.ImplementationComplexity.ToString(),
                RequiredActions = new List<string> { optimization.RecommendedAction }
            };
            
            // Add metadata
            aiRecommendation.Metadata["optimization_id"] = optimization.OptimizationId;
            aiRecommendation.Metadata["category"] = optimization.Category.ToString();
            aiRecommendation.Metadata["estimated_cost"] = optimization.EstimatedCost;
            aiRecommendation.Metadata["time_to_implement"] = optimization.TimeToImplement;
            
            _activeRecommendations.Add(aiRecommendation);
            
            LogInfo($"Created environmental AI recommendation: {optimization.Title}");
        }
        
        #endregion

        #region Environmental Analysis Helper Methods
        
        private EnvironmentalConditions GetCurrentEnvironmentalConditions()
        {
            // Simulate current environmental conditions
            return new EnvironmentalConditions
            {
                Temperature = UnityEngine.Random.Range(22f, 28f),
                Humidity = UnityEngine.Random.Range(45f, 70f),
                LightIntensity = UnityEngine.Random.Range(400f, 800f),
                CO2Level = UnityEngine.Random.Range(350f, 1200f),
                AirFlow = UnityEngine.Random.Range(0.3f, 0.8f)
            };
        }
        
        private EnvironmentalStatus AnalyzeTemperatureStatus(EnvironmentalConditions conditions, List<PlantInstanceSO> plants)
        {
            var optimalRange = GetOptimalTemperatureRange(plants);
            var efficiency = CalculateTemperatureEfficiency(conditions.Temperature, optimalRange);
            
            return new EnvironmentalStatus
            {
                CurrentValue = conditions.Temperature,
                OptimalMin = optimalRange.Min,
                OptimalMax = optimalRange.Max,
                EfficiencyScore = efficiency,
                Status = GetTemperatureStatusMessage(conditions.Temperature, optimalRange),
                Recommendations = GetTemperatureRecommendations(conditions.Temperature, optimalRange)
            };
        }
        
        private EnvironmentalStatus AnalyzeHumidityStatus(EnvironmentalConditions conditions, List<PlantInstanceSO> plants)
        {
            var optimalRange = GetOptimalHumidityRange(plants);
            var efficiency = CalculateHumidityEfficiency(conditions.Humidity, optimalRange);
            
            return new EnvironmentalStatus
            {
                CurrentValue = conditions.Humidity,
                OptimalMin = optimalRange.Min,
                OptimalMax = optimalRange.Max,
                EfficiencyScore = efficiency,
                Status = GetHumidityStatusMessage(conditions.Humidity, optimalRange),
                Recommendations = GetHumidityRecommendations(conditions.Humidity, optimalRange)
            };
        }
        
        private EnvironmentalStatus AnalyzeLightingStatus(EnvironmentalConditions conditions, List<PlantInstanceSO> plants)
        {
            var optimalRange = GetOptimalLightRange(plants);
            var efficiency = CalculateLightingEfficiency(conditions.LightIntensity, optimalRange);
            
            return new EnvironmentalStatus
            {
                CurrentValue = conditions.LightIntensity,
                OptimalMin = optimalRange.Min,
                OptimalMax = optimalRange.Max,
                EfficiencyScore = efficiency,
                Status = GetLightingStatusMessage(conditions.LightIntensity, optimalRange),
                Recommendations = GetLightingRecommendations(conditions.LightIntensity, optimalRange)
            };
        }
        
        private EnvironmentalStatus AnalyzeAirflowStatus(EnvironmentalConditions conditions, List<PlantInstanceSO> plants)
        {
            var optimalRange = GetOptimalAirflowRange(plants);
            var efficiency = CalculateAirflowEfficiency(conditions.AirFlow, optimalRange);
            
            return new EnvironmentalStatus
            {
                CurrentValue = conditions.AirFlow,
                OptimalMin = optimalRange.Min,
                OptimalMax = optimalRange.Max,
                EfficiencyScore = efficiency,
                Status = GetAirflowStatusMessage(conditions.AirFlow, optimalRange),
                Recommendations = GetAirflowRecommendations(conditions.AirFlow, optimalRange)
            };
        }
        
        private EnvironmentalStatus AnalyzeCO2Status(EnvironmentalConditions conditions, List<PlantInstanceSO> plants)
        {
            var optimalRange = GetOptimalCO2Range(plants);
            var efficiency = CalculateCO2Efficiency(conditions.CO2Level, optimalRange);
            
            return new EnvironmentalStatus
            {
                CurrentValue = conditions.CO2Level,
                OptimalMin = optimalRange.Min,
                OptimalMax = optimalRange.Max,
                EfficiencyScore = efficiency,
                Status = GetCO2StatusMessage(conditions.CO2Level, optimalRange),
                Recommendations = GetCO2Recommendations(conditions.CO2Level, optimalRange)
            };
        }
        
        private EnvironmentalStatus AnalyzeNutrientStatus(EnvironmentalConditions conditions, List<PlantInstanceSO> plants)
        {
            var optimalRange = GetOptimalPHRange(plants);
            // Simulate pH since it's not available in EnvironmentalConditions
            var simulatedPH = UnityEngine.Random.Range(5.8f, 6.5f);
            var efficiency = CalculateNutrientEfficiency(simulatedPH, optimalRange);
            
            return new EnvironmentalStatus
            {
                CurrentValue = simulatedPH,
                OptimalMin = optimalRange.Min,
                OptimalMax = optimalRange.Max,
                EfficiencyScore = efficiency,
                Status = GetNutrientStatusMessage(simulatedPH, optimalRange),
                Recommendations = GetNutrientRecommendations(simulatedPH, optimalRange)
            };
        }
        
        // Stub implementations of environmental helper methods
        private float CalculateOptimizationPotential(float[] statusScores) => 1f - statusScores.Average();
        private OptimalRange GetOptimalTemperatureRange(List<PlantInstanceSO> plants) => new OptimalRange(20f, 26f, 23f);
        private OptimalRange GetOptimalHumidityRange(List<PlantInstanceSO> plants) => new OptimalRange(50f, 70f, 60f);
        private OptimalRange GetOptimalLightRange(List<PlantInstanceSO> plants) => new OptimalRange(400f, 800f, 600f);
        private OptimalRange GetOptimalAirflowRange(List<PlantInstanceSO> plants) => new OptimalRange(0.3f, 0.8f, 0.5f);
        private OptimalRange GetOptimalCO2Range(List<PlantInstanceSO> plants) => new OptimalRange(800f, 1200f, 1000f);
        private OptimalRange GetOptimalPHRange(List<PlantInstanceSO> plants) => new OptimalRange(5.8f, 6.5f, 6.2f);
        
        private float CalculateTemperatureEfficiency(float temp, OptimalRange range) => 1f - Mathf.Abs(temp - range.Optimal) / (range.Max - range.Min);
        private float CalculateHumidityEfficiency(float humidity, OptimalRange range) => 1f - Mathf.Abs(humidity - range.Optimal) / (range.Max - range.Min);
        private float CalculateLightingEfficiency(float ppfd, OptimalRange range) => 1f - Mathf.Abs(ppfd - range.Optimal) / (range.Max - range.Min);
        private float CalculateAirflowEfficiency(float airflow, OptimalRange range) => 1f - Mathf.Abs(airflow - range.Optimal) / (range.Max - range.Min);
        private float CalculateCO2Efficiency(float co2, OptimalRange range) => 1f - Mathf.Abs(co2 - range.Optimal) / (range.Max - range.Min);
        private float CalculateNutrientEfficiency(float ph, OptimalRange range) => 1f - Mathf.Abs(ph - range.Optimal) / (range.Max - range.Min);
        
        private string GetTemperatureStatusMessage(float temp, OptimalRange range) => temp < range.Min ? "Too Cold" : temp > range.Max ? "Too Hot" : "Optimal";
        private string GetHumidityStatusMessage(float humidity, OptimalRange range) => humidity < range.Min ? "Too Dry" : humidity > range.Max ? "Too Humid" : "Optimal";
        private string GetLightingStatusMessage(float ppfd, OptimalRange range) => ppfd < range.Min ? "Too Low" : ppfd > range.Max ? "Too High" : "Optimal";
        private string GetAirflowStatusMessage(float airflow, OptimalRange range) => airflow < range.Min ? "Insufficient" : airflow > range.Max ? "Excessive" : "Optimal";
        private string GetCO2StatusMessage(float co2, OptimalRange range) => co2 < range.Min ? "Too Low" : co2 > range.Max ? "Too High" : "Optimal";
        private string GetNutrientStatusMessage(float ph, OptimalRange range) => ph < range.Min ? "Too Acidic" : ph > range.Max ? "Too Alkaline" : "Optimal";
        
        private List<string> GetTemperatureRecommendations(float temp, OptimalRange range) => new List<string> { temp < range.Min ? "Increase heating" : "Increase cooling" };
        private List<string> GetHumidityRecommendations(float humidity, OptimalRange range) => new List<string> { humidity < range.Min ? "Increase humidification" : "Increase dehumidification" };
        private List<string> GetLightingRecommendations(float ppfd, OptimalRange range) => new List<string> { ppfd < range.Min ? "Increase light intensity" : "Reduce light intensity" };
        private List<string> GetAirflowRecommendations(float airflow, OptimalRange range) => new List<string> { airflow < range.Min ? "Increase fan speed" : "Reduce fan speed" };
        private List<string> GetCO2Recommendations(float co2, OptimalRange range) => new List<string> { co2 < range.Min ? "Increase CO2 injection" : "Reduce CO2 injection" };
        private List<string> GetNutrientRecommendations(float ph, OptimalRange range) => new List<string> { ph < range.Min ? "Add pH up solution" : "Add pH down solution" };
        
        private string DetermineTemperatureAction(EnvironmentalStatus status) => status.CurrentValue < status.OptimalMin ? "Increase temperature to optimal range" : "Decrease temperature to optimal range";
        private string DetermineHumidityAction(EnvironmentalStatus status) => status.CurrentValue < status.OptimalMin ? "Increase humidity" : "Decrease humidity";
        private string DetermineLightingAction(EnvironmentalStatus status) => status.CurrentValue < status.OptimalMin ? "Increase light intensity" : "Optimize lighting schedule";
        private string DetermineAirflowAction(EnvironmentalStatus status) => "Adjust fan speeds for optimal circulation";
        private string DetermineCO2Action(EnvironmentalStatus status) => status.CurrentValue < status.OptimalMin ? "Increase CO2 supplementation" : "Reduce CO2 levels";
        private string DetermineNutrientAction(EnvironmentalStatus status) => status.CurrentValue < status.OptimalMin ? "Increase pH" : "Decrease pH";
        private string DetermineVPDAction(float current, float optimal) => current < optimal ? "Increase temperature or decrease humidity" : "Decrease temperature or increase humidity";
        
        private EnvironmentalPriority DetermineTemperaturePriority(EnvironmentalStatus status) => status.EfficiencyScore < 0.6f ? EnvironmentalPriority.High : EnvironmentalPriority.Medium;
        private EnvironmentalPriority DetermineHumidityPriority(EnvironmentalStatus status) => status.EfficiencyScore < 0.6f ? EnvironmentalPriority.High : EnvironmentalPriority.Medium;
        private EnvironmentalPriority DetermineLightingPriority(EnvironmentalStatus status) => status.EfficiencyScore < 0.7f ? EnvironmentalPriority.High : EnvironmentalPriority.Medium;
        private EnvironmentalPriority DetermineAirflowPriority(EnvironmentalStatus status) => status.EfficiencyScore < 0.5f ? EnvironmentalPriority.High : EnvironmentalPriority.Low;
        private EnvironmentalPriority DetermineCO2Priority(EnvironmentalStatus status) => status.EfficiencyScore < 0.6f ? EnvironmentalPriority.High : EnvironmentalPriority.Medium;
        private EnvironmentalPriority DetermineNutrientPriority(EnvironmentalStatus status) => status.EfficiencyScore < 0.7f ? EnvironmentalPriority.High : EnvironmentalPriority.Medium;
        
        private float CalculateTemperatureImpact(EnvironmentalStatus status) => (1f - status.EfficiencyScore) * 100f;
        private float CalculateHumidityImpact(EnvironmentalStatus status) => (1f - status.EfficiencyScore) * 80f;
        private float CalculateLightingImpact(EnvironmentalStatus status) => (1f - status.EfficiencyScore) * 90f;
        private float CalculateAirflowImpact(EnvironmentalStatus status) => (1f - status.EfficiencyScore) * 60f;
        private float CalculateCO2Impact(EnvironmentalStatus status) => (1f - status.EfficiencyScore) * 70f;
        private float CalculateNutrientImpact(EnvironmentalStatus status) => (1f - status.EfficiencyScore) * 75f;
        private float CalculateVPDImpact(float deviation) => deviation * 50f;
        
        private float CalculateTemperatureCost(EnvironmentalStatus status) => 15f + (1f - status.EfficiencyScore) * 30f;
        private float CalculateHumidityCost(EnvironmentalStatus status) => 10f + (1f - status.EfficiencyScore) * 20f;
        private float CalculateLightingCost(EnvironmentalStatus status) => 25f + (1f - status.EfficiencyScore) * 50f;
        private float CalculateAirflowCost(EnvironmentalStatus status) => 5f + (1f - status.EfficiencyScore) * 15f;
        private float CalculateCO2Cost(EnvironmentalStatus status) => 40f + (1f - status.EfficiencyScore) * 60f;
        private float CalculateNutrientCost(EnvironmentalStatus status) => 12f + (1f - status.EfficiencyScore) * 25f;
        
        private float CalculateTemperatureImprovement(EnvironmentalStatus status) => (1f - status.EfficiencyScore) * 0.8f;
        private float CalculateHumidityImprovement(EnvironmentalStatus status) => (1f - status.EfficiencyScore) * 0.7f;
        private float CalculateLightingImprovement(EnvironmentalStatus status) => (1f - status.EfficiencyScore) * 0.85f;
        private float CalculateAirflowImprovement(EnvironmentalStatus status) => (1f - status.EfficiencyScore) * 0.6f;
        private float CalculateCO2Improvement(EnvironmentalStatus status) => (1f - status.EfficiencyScore) * 0.75f;
        private float CalculateNutrientImprovement(EnvironmentalStatus status) => (1f - status.EfficiencyScore) * 0.8f;
        private float CalculateVPDImprovement(float deviation) => Mathf.Clamp01(deviation * 0.5f);
        
        private List<string> GetTemperatureEquipment(EnvironmentalStatus status) => new List<string> { status.CurrentValue < status.OptimalMin ? "Heater" : "Air conditioner", "Temperature controller" };
        private List<string> GetHumidityEquipment(EnvironmentalStatus status) => new List<string> { status.CurrentValue < status.OptimalMin ? "Humidifier" : "Dehumidifier", "Humidity controller" };
        private List<string> GetLightingEquipment(EnvironmentalStatus status) => new List<string> { "LED grow lights", "Light timer", "PPFD meter" };
        private List<string> GetAirflowEquipment(EnvironmentalStatus status) => new List<string> { "Circulation fans", "Exhaust fans", "Fan controller" };
        private List<string> GetCO2Equipment(EnvironmentalStatus status) => new List<string> { "CO2 generator", "CO2 controller", "CO2 monitor" };
        private List<string> GetNutrientEquipment(EnvironmentalStatus status) => new List<string> { "pH meter", "pH adjustment solution", "Nutrient dosing system" };
        
        private float CalculateVPD(float temperature, float humidity) => (1 - humidity / 100f) * (0.6108f * Mathf.Exp(17.27f * temperature / (temperature + 237.3f)));
        private float GetOptimalVPD(PlantGrowthStage stage) => stage == PlantGrowthStage.Vegetative ? 0.8f : stage == PlantGrowthStage.Flowering ? 1.0f : 0.9f;
        
        private AIPriority ConvertEnvironmentalPriority(EnvironmentalPriority envPriority) => envPriority switch
        {
            EnvironmentalPriority.Critical => AIPriority.Critical,
            EnvironmentalPriority.High => AIPriority.High,
            EnvironmentalPriority.Medium => AIPriority.Medium,
            EnvironmentalPriority.Low => AIPriority.Low,
            _ => AIPriority.Medium
        };
        
        #endregion

        #region Market Insights and Trend Analysis - PC-012-4
        
        /// <summary>
        /// PC-012-4: Generate market insights and trend analysis for strategic decision making
        /// </summary>
        public List<MarketInsight> GenerateMarketInsights()
        {
            var insights = new List<MarketInsight>();
            
            try
            {
                LogInfo("Generating market insights and trend analysis...");
                
                // Analyze current market conditions
                var marketAnalysis = AnalyzeCurrentMarketConditions();
                
                // Generate price trend insights
                insights.AddRange(GeneratePriceTrendInsights(marketAnalysis));
                
                // Generate demand analysis insights
                insights.AddRange(GenerateDemandAnalysisInsights(marketAnalysis));
                
                // Generate supply analysis insights
                insights.AddRange(GenerateSupplyAnalysisInsights(marketAnalysis));
                
                // Generate seasonal trend insights
                insights.AddRange(GenerateSeasonalTrendInsights(marketAnalysis));
                
                // Generate competitive analysis insights
                insights.AddRange(GenerateCompetitiveAnalysisInsights(marketAnalysis));
                
                // Generate opportunity insights
                insights.AddRange(GenerateOpportunityInsights(marketAnalysis));
                
                // Create AI recommendations for top market insights
                var topInsights = insights
                    .Where(i => i.Confidence > 0.7f && i.Impact > 0.6f)
                    .OrderByDescending(i => i.Impact)
                    .Take(3);
                
                foreach (var insight in topInsights)
                {
                    CreateMarketAIRecommendation(insight);
                }
                
                LogInfo($"Generated {insights.Count} market insights");
                return insights.OrderByDescending(i => i.Impact).ToList();
            }
            catch (System.Exception ex)
            {
                LogError($"Error generating market insights: {ex.Message}");
                return insights;
            }
        }
        
        /// <summary>
        /// Generate trend analysis for specific market data
        /// </summary>
        public AITrendAnalysis AnalyzeMarketTrend(string productName, List<TrendDataPoint> historicalData)
        {
            try
            {
                if (historicalData == null || historicalData.Count < 3)
                {
                    LogWarning($"Insufficient data for trend analysis of {productName}");
                    return CreateDefaultTrendAnalysis();
                }
                
                // Calculate trend direction and strength
                var trendDirection = CalculateTrendDirection(historicalData);
                var trendStrength = CalculateTrendStrength(historicalData);
                var volatility = CalculateVolatility(historicalData);
                var confidence = CalculateTrendConfidence(historicalData, trendStrength);
                
                return new AITrendAnalysis
                {
                    IsSignificant = Mathf.Abs(trendStrength) > 0.1f,
                    IsImproving = trendDirection > 0f,
                    ChangePercent = trendStrength * 100f,
                    TrendDirection = GetTrendDirectionText(trendDirection),
                    Confidence = confidence,
                    DataPoints = historicalData,
                    AnalysisDate = System.DateTime.Now,
                    AnalysisPeriod = CalculateAnalysisPeriod(historicalData)
                };
            }
            catch (System.Exception ex)
            {
                LogError($"Error analyzing market trend for {productName}: {ex.Message}");
                return CreateDefaultTrendAnalysis();
            }
        }
        
        /// <summary>
        /// Generate market forecast based on current data
        /// </summary>
        public MarketForecast GenerateMarketForecast(int daysAhead = 30)
        {
            try
            {
                var currentMarket = AnalyzeCurrentMarketConditions();
                
                return new MarketForecast
                {
                    ForecastDate = System.DateTime.Now,
                    ForecastPeriodDays = daysAhead,
                    PredictedPriceChange = PredictPriceChange(currentMarket, daysAhead),
                    PredictedDemandChange = PredictDemandChange(currentMarket, daysAhead),
                    SeasonalFactors = AnalyzeSeasonalFactors(daysAhead),
                    ConfidenceLevel = CalculateForecastConfidence(daysAhead),
                    KeyFactors = IdentifyKeyMarketFactors(currentMarket),
                    Recommendations = GenerateForecastRecommendations(currentMarket, daysAhead)
                };
            }
            catch (System.Exception ex)
            {
                LogError($"Error generating market forecast: {ex.Message}");
                return CreateDefaultMarketForecast(daysAhead);
            }
        }
        
        private MarketAnalysisData AnalyzeCurrentMarketConditions()
        {
            // Simulate market analysis since we don't have direct MarketManager access yet
            var marketData = new MarketAnalysisData
            {
                AnalysisTime = System.DateTime.Now,
                OverallMarketHealth = UnityEngine.Random.Range(0.6f, 0.9f),
                AveragePriceLevel = UnityEngine.Random.Range(8f, 25f),
                DemandLevel = UnityEngine.Random.Range(0.4f, 0.9f),
                SupplyLevel = UnityEngine.Random.Range(0.3f, 0.8f),
                PriceVolatility = UnityEngine.Random.Range(0.1f, 0.4f),
                SeasonalFactor = CalculateCurrentSeasonalFactor(),
                CompetitiveIndex = UnityEngine.Random.Range(0.5f, 0.85f)
            };
            
            // Add simulated product data
            marketData.ProductData = GenerateSimulatedProductData();
            
            return marketData;
        }
        
        private List<MarketInsight> GeneratePriceTrendInsights(MarketAnalysisData marketData)
        {
            var insights = new List<MarketInsight>();
            
            // Price volatility insight
            if (marketData.PriceVolatility > 0.3f)
            {
                insights.Add(new MarketInsight
                {
                    InsightId = System.Guid.NewGuid().ToString(),
                    Title = "High Price Volatility Detected",
                    Description = $"Market showing {marketData.PriceVolatility:P1} price volatility - higher than normal",
                    Category = MarketInsightCategory.PriceTrend,
                    Impact = 0.8f,
                    Confidence = 0.9f,
                    Urgency = MarketUrgency.Medium,
                    Recommendation = "Consider timing sales during price peaks and holding inventory during dips",
                    SupportingData = new Dictionary<string, object>
                    {
                        { "volatility_percentage", marketData.PriceVolatility * 100f },
                        { "average_price", marketData.AveragePriceLevel }
                    }
                });
            }
            
            // Price trend insight
            var priceChangeDirection = DeterminePriceChangeDirection(marketData);
            if (Mathf.Abs(priceChangeDirection) > 0.1f)
            {
                insights.Add(new MarketInsight
                {
                    InsightId = System.Guid.NewGuid().ToString(),
                    Title = priceChangeDirection > 0 ? "Upward Price Trend" : "Downward Price Trend",
                    Description = $"Prices trending {(priceChangeDirection > 0 ? "upward" : "downward")} by approximately {Mathf.Abs(priceChangeDirection):P1}",
                    Category = MarketInsightCategory.PriceTrend,
                    Impact = 0.7f,
                    Confidence = 0.8f,
                    Urgency = priceChangeDirection > 0 ? MarketUrgency.Low : MarketUrgency.Medium,
                    Recommendation = priceChangeDirection > 0 ? 
                        "Good time to prepare for sales - prices are rising" : 
                        "Consider accelerating sales before further price decline"
                });
            }
            
            return insights;
        }
        
        private List<MarketInsight> GenerateDemandAnalysisInsights(MarketAnalysisData marketData)
        {
            var insights = new List<MarketInsight>();
            
            // High demand insight
            if (marketData.DemandLevel > 0.8f)
            {
                insights.Add(new MarketInsight
                {
                    InsightId = System.Guid.NewGuid().ToString(),
                    Title = "Strong Market Demand",
                    Description = $"Current demand level at {marketData.DemandLevel:P1} - excellent selling conditions",
                    Category = MarketInsightCategory.Demand,
                    Impact = 0.9f,
                    Confidence = 0.85f,
                    Urgency = MarketUrgency.High,
                    Recommendation = "Maximize production and prioritize high-demand strains for optimal profits",
                    SupportingData = new Dictionary<string, object>
                    {
                        { "demand_level", marketData.DemandLevel },
                        { "market_health", marketData.OverallMarketHealth }
                    }
                });
            }
            
            // Low demand warning
            else if (marketData.DemandLevel < 0.5f)
            {
                insights.Add(new MarketInsight
                {
                    InsightId = System.Guid.NewGuid().ToString(),
                    Title = "Weak Market Demand",
                    Description = $"Demand level only {marketData.DemandLevel:P1} - challenging market conditions",
                    Category = MarketInsightCategory.Demand,
                    Impact = 0.8f,
                    Confidence = 0.8f,
                    Urgency = MarketUrgency.High,
                    Recommendation = "Focus on quality over quantity and consider diversifying product offerings",
                    SupportingData = new Dictionary<string, object>
                    {
                        { "demand_level", marketData.DemandLevel },
                        { "recommended_strategy", "quality_focus" }
                    }
                });
            }
            
            return insights;
        }
        
        private List<MarketInsight> GenerateSupplyAnalysisInsights(MarketAnalysisData marketData)
        {
            var insights = new List<MarketInsight>();
            
            // Supply-demand balance
            var supplyDemandRatio = marketData.SupplyLevel / marketData.DemandLevel;
            
            if (supplyDemandRatio < 0.6f)
            {
                insights.Add(new MarketInsight
                {
                    InsightId = System.Guid.NewGuid().ToString(),
                    Title = "Supply Shortage Opportunity",
                    Description = $"Supply significantly below demand (ratio: {supplyDemandRatio:F2}) - premium pricing opportunity",
                    Category = MarketInsightCategory.Supply,
                    Impact = 0.85f,
                    Confidence = 0.8f,
                    Urgency = MarketUrgency.High,
                    Recommendation = "Increase production capacity and consider premium pricing for high-quality products",
                    SupportingData = new Dictionary<string, object>
                    {
                        { "supply_demand_ratio", supplyDemandRatio },
                        { "supply_level", marketData.SupplyLevel },
                        { "demand_level", marketData.DemandLevel }
                    }
                });
            }
            else if (supplyDemandRatio > 1.2f)
            {
                insights.Add(new MarketInsight
                {
                    InsightId = System.Guid.NewGuid().ToString(),
                    Title = "Market Oversupply Warning",
                    Description = $"Supply exceeds demand (ratio: {supplyDemandRatio:F2}) - competitive pricing needed",
                    Category = MarketInsightCategory.Supply,
                    Impact = 0.7f,
                    Confidence = 0.75f,
                    Urgency = MarketUrgency.Medium,
                    Recommendation = "Focus on product differentiation and efficient production to maintain profitability",
                    SupportingData = new Dictionary<string, object>
                    {
                        { "supply_demand_ratio", supplyDemandRatio },
                        { "strategy", "differentiation" }
                    }
                });
            }
            
            return insights;
        }
        
        private List<MarketInsight> GenerateSeasonalTrendInsights(MarketAnalysisData marketData)
        {
            var insights = new List<MarketInsight>();
            var currentMonth = System.DateTime.Now.Month;
            
            // Seasonal demand patterns
            if (IsHighDemandSeason(currentMonth))
            {
                insights.Add(new MarketInsight
                {
                    InsightId = System.Guid.NewGuid().ToString(),
                    Title = "Seasonal Demand Peak",
                    Description = $"Entering high-demand season - {GetSeasonName(currentMonth)} typically shows increased market activity",
                    Category = MarketInsightCategory.Seasonal,
                    Impact = 0.75f,
                    Confidence = 0.85f,
                    Urgency = MarketUrgency.Medium,
                    Recommendation = "Ensure adequate inventory and consider premium pricing during peak season",
                    SupportingData = new Dictionary<string, object>
                    {
                        { "season", GetSeasonName(currentMonth) },
                        { "seasonal_factor", marketData.SeasonalFactor }
                    }
                });
            }
            else if (IsLowDemandSeason(currentMonth))
            {
                insights.Add(new MarketInsight
                {
                    InsightId = System.Guid.NewGuid().ToString(),
                    Title = "Seasonal Demand Decline",
                    Description = $"Entering lower-demand season - {GetSeasonName(currentMonth)} typically shows reduced market activity",
                    Category = MarketInsightCategory.Seasonal,
                    Impact = 0.6f,
                    Confidence = 0.8f,
                    Urgency = MarketUrgency.Low,
                    Recommendation = "Focus on cost optimization and prepare for next peak season",
                    SupportingData = new Dictionary<string, object>
                    {
                        { "season", GetSeasonName(currentMonth) },
                        { "strategy", "cost_optimization" }
                    }
                });
            }
            
            return insights;
        }
        
        private List<MarketInsight> GenerateCompetitiveAnalysisInsights(MarketAnalysisData marketData)
        {
            var insights = new List<MarketInsight>();
            
            // Competitive position
            if (marketData.CompetitiveIndex < 0.6f)
            {
                insights.Add(new MarketInsight
                {
                    InsightId = System.Guid.NewGuid().ToString(),
                    Title = "Highly Competitive Market",
                    Description = $"Competition index at {marketData.CompetitiveIndex:P1} - challenging competitive environment",
                    Category = MarketInsightCategory.Competition,
                    Impact = 0.8f,
                    Confidence = 0.75f,
                    Urgency = MarketUrgency.High,
                    Recommendation = "Focus on unique value propositions and operational efficiency to maintain market position",
                    SupportingData = new Dictionary<string, object>
                    {
                        { "competitive_index", marketData.CompetitiveIndex },
                        { "recommended_focus", "differentiation" }
                    }
                });
            }
            else if (marketData.CompetitiveIndex > 0.8f)
            {
                insights.Add(new MarketInsight
                {
                    InsightId = System.Guid.NewGuid().ToString(),
                    Title = "Favorable Competitive Position",
                    Description = $"Competition index at {marketData.CompetitiveIndex:P1} - good market positioning opportunity",
                    Category = MarketInsightCategory.Competition,
                    Impact = 0.7f,
                    Confidence = 0.8f,
                    Urgency = MarketUrgency.Low,
                    Recommendation = "Consider expanding market share and premium positioning strategies",
                    SupportingData = new Dictionary<string, object>
                    {
                        { "competitive_index", marketData.CompetitiveIndex },
                        { "opportunity", "market_expansion" }
                    }
                });
            }
            
            return insights;
        }
        
        private List<MarketInsight> GenerateOpportunityInsights(MarketAnalysisData marketData)
        {
            var insights = new List<MarketInsight>();
            
            // Product diversification opportunity
            var topPerformingProducts = marketData.ProductData
                .OrderByDescending(p => p.Value.ProfitMargin)
                .Take(3)
                .ToList();
            
            if (topPerformingProducts.Any())
            {
                var bestProduct = topPerformingProducts.First();
                insights.Add(new MarketInsight
                {
                    InsightId = System.Guid.NewGuid().ToString(),
                    Title = "High-Margin Product Opportunity",
                    Description = $"{bestProduct.Key} showing {bestProduct.Value.ProfitMargin:P1} profit margin - excellent opportunity",
                    Category = MarketInsightCategory.Opportunity,
                    Impact = 0.9f,
                    Confidence = 0.85f,
                    Urgency = MarketUrgency.Medium,
                    Recommendation = $"Consider increasing production of {bestProduct.Key} to maximize profitability",
                    SupportingData = new Dictionary<string, object>
                    {
                        { "product_name", bestProduct.Key },
                        { "profit_margin", bestProduct.Value.ProfitMargin },
                        { "demand_score", bestProduct.Value.DemandScore }
                    }
                });
            }
            
            // Market timing opportunity
            if (marketData.OverallMarketHealth > 0.8f && marketData.DemandLevel > 0.7f)
            {
                insights.Add(new MarketInsight
                {
                    InsightId = System.Guid.NewGuid().ToString(),
                    Title = "Optimal Market Conditions",
                    Description = "Market health and demand both strong - ideal conditions for expansion or premium sales",
                    Category = MarketInsightCategory.Opportunity,
                    Impact = 0.85f,
                    Confidence = 0.9f,
                    Urgency = MarketUrgency.High,
                    Recommendation = "Consider accelerating production and sales efforts to capitalize on favorable conditions",
                    SupportingData = new Dictionary<string, object>
                    {
                        { "market_health", marketData.OverallMarketHealth },
                        { "demand_level", marketData.DemandLevel },
                        { "opportunity_type", "expansion" }
                    }
                });
            }
            
            return insights;
        }
        
        private void CreateMarketAIRecommendation(MarketInsight insight)
        {
            var aiRecommendation = new AIRecommendation
            {
                RecommendationId = System.Guid.NewGuid().ToString(),
                Title = $"Market: {insight.Title}",
                Summary = insight.Description,
                Description = $"{insight.Description}\n\nRecommendation: {insight.Recommendation}",
                Type = RecommendationType.Strategic,
                Priority = ConvertMarketUrgency(insight.Urgency),
                Category = "Market Analysis",
                CreatedAt = System.DateTime.Now,
                ExpiresAt = System.DateTime.Now.AddDays(7), // Market insights valid for a week
                Status = RecommendationStatus.Active,
                ConfidenceScore = insight.Confidence,
                ImpactScore = insight.Impact * 100f,
                EstimatedBenefit = insight.Impact,
                ImplementationComplexity = DetermineMarketComplexity(insight),
                RequiredActions = new List<string> { insight.Recommendation }
            };
            
            // Add metadata
            aiRecommendation.Metadata["insight_id"] = insight.InsightId;
            aiRecommendation.Metadata["category"] = insight.Category.ToString();
            aiRecommendation.Metadata["urgency"] = insight.Urgency.ToString();
            
            if (insight.SupportingData != null)
            {
                foreach (var data in insight.SupportingData)
                {
                    aiRecommendation.Metadata[$"data_{data.Key}"] = data.Value;
                }
            }
            
            _activeRecommendations.Add(aiRecommendation);
            
            LogInfo($"Created market AI recommendation: {insight.Title}");
        }
        
        // Helper methods for market analysis
        private float CalculateCurrentSeasonalFactor()
        {
            var month = System.DateTime.Now.Month;
            // Simulate seasonal patterns (higher demand in spring/summer)
            return month switch
            {
                3 or 4 or 5 => 1.2f,  // Spring
                6 or 7 or 8 => 1.3f,  // Summer
                9 or 10 or 11 => 1.0f, // Fall
                _ => 0.8f              // Winter
            };
        }
        
        private Dictionary<string, ProductMarketData> GenerateSimulatedProductData()
        {
            var products = new Dictionary<string, ProductMarketData>
            {
                { "OG Kush", new ProductMarketData { ProfitMargin = 0.65f, DemandScore = 0.8f, PricePerGram = 15f } },
                { "White Widow", new ProductMarketData { ProfitMargin = 0.7f, DemandScore = 0.75f, PricePerGram = 18f } },
                { "Blue Dream", new ProductMarketData { ProfitMargin = 0.6f, DemandScore = 0.9f, PricePerGram = 14f } },
                { "Sour Diesel", new ProductMarketData { ProfitMargin = 0.55f, DemandScore = 0.7f, PricePerGram = 16f } },
                { "Girl Scout Cookies", new ProductMarketData { ProfitMargin = 0.8f, DemandScore = 0.85f, PricePerGram = 20f } }
            };
            
            return products;
        }
        
        private float DeterminePriceChangeDirection(MarketAnalysisData marketData)
        {
            // Simulate price change based on supply/demand balance
            var supplyDemandRatio = marketData.SupplyLevel / marketData.DemandLevel;
            var baseChange = (marketData.DemandLevel - marketData.SupplyLevel) * 0.3f;
            var seasonalInfluence = (marketData.SeasonalFactor - 1f) * 0.2f;
            
            return baseChange + seasonalInfluence + UnityEngine.Random.Range(-0.05f, 0.05f);
        }
        
        private float CalculateTrendDirection(List<TrendDataPoint> data)
        {
            if (data.Count < 2) return 0f;
            
            var firstValue = data.First().Value;
            var lastValue = data.Last().Value;
            
            return (lastValue - firstValue) / firstValue;
        }
        
        private float CalculateTrendStrength(List<TrendDataPoint> data)
        {
            if (data.Count < 3) return 0f;
            
            var values = data.Select(d => d.Value).ToArray();
            var trend = 0f;
            
            for (int i = 1; i < values.Length; i++)
            {
                trend += (values[i] - values[i - 1]) / values[i - 1];
            }
            
            return trend / (values.Length - 1);
        }
        
        private float CalculateVolatility(List<TrendDataPoint> data)
        {
            if (data.Count < 2) return 0f;
            
            var values = data.Select(d => d.Value).ToArray();
            var mean = values.Average();
            var variance = values.Select(v => Mathf.Pow(v - mean, 2)).Average();
            
            return Mathf.Sqrt(variance) / mean;
        }
        
        private float CalculateTrendConfidence(List<TrendDataPoint> data, float trendStrength)
        {
            var dataQuality = Mathf.Clamp01(data.Count / 30f); // More data = higher confidence
            var consistencyFactor = 1f - CalculateVolatility(data);
            var strengthFactor = Mathf.Clamp01(Mathf.Abs(trendStrength) * 5f);
            
            return (dataQuality + consistencyFactor + strengthFactor) / 3f;
        }
        
        private string GetTrendDirectionText(float direction)
        {
            return direction > 0.05f ? "Strongly Upward" :
                   direction > 0.01f ? "Upward" :
                   direction < -0.05f ? "Strongly Downward" :
                   direction < -0.01f ? "Downward" : "Stable";
        }
        
        private System.TimeSpan CalculateAnalysisPeriod(List<TrendDataPoint> data)
        {
            if (data.Count < 2) return System.TimeSpan.Zero;
            
            return data.Last().Timestamp - data.First().Timestamp;
        }
        
        private AITrendAnalysis CreateDefaultTrendAnalysis()
        {
            return new AITrendAnalysis
            {
                IsSignificant = false,
                IsImproving = false,
                ChangePercent = 0f,
                TrendDirection = "Insufficient Data",
                Confidence = 0f,
                DataPoints = new List<TrendDataPoint>(),
                AnalysisDate = System.DateTime.Now,
                AnalysisPeriod = System.TimeSpan.Zero
            };
        }
        
        private float PredictPriceChange(MarketAnalysisData market, int daysAhead)
        {
            var baseChange = (market.DemandLevel - market.SupplyLevel) * 0.1f;
            var seasonalInfluence = market.SeasonalFactor * 0.05f;
            var timeDecay = 1f - (daysAhead / 365f); // Predictions less reliable over time
            
            return (baseChange + seasonalInfluence) * timeDecay;
        }
        
        private float PredictDemandChange(MarketAnalysisData market, int daysAhead)
        {
            var seasonalInfluence = (market.SeasonalFactor - 1f) * 0.3f;
            var marketHealthInfluence = (market.OverallMarketHealth - 0.5f) * 0.2f;
            var timeDecay = 1f - (daysAhead / 180f);
            
            return (seasonalInfluence + marketHealthInfluence) * timeDecay;
        }
        
        private List<string> AnalyzeSeasonalFactors(int daysAhead)
        {
            var futureDate = System.DateTime.Now.AddDays(daysAhead);
            var factors = new List<string>();
            
            var month = futureDate.Month;
            switch (month)
            {
                case 3: case 4: case 5:
                    factors.Add("Spring season - increased outdoor activity and demand");
                    break;
                case 6: case 7: case 8:
                    factors.Add("Summer season - peak demand period");
                    break;
                case 9: case 10: case 11:
                    factors.Add("Fall season - harvest season competition");
                    break;
                default:
                    factors.Add("Winter season - reduced demand, focus on quality");
                    break;
            }
            
            return factors;
        }
        
        private float CalculateForecastConfidence(int daysAhead)
        {
            // Confidence decreases with time horizon
            return Mathf.Clamp01(1f - (daysAhead / 180f));
        }
        
        private List<string> IdentifyKeyMarketFactors(MarketAnalysisData market)
        {
            var factors = new List<string>();
            
            if (market.DemandLevel > 0.7f)
                factors.Add("High consumer demand");
            if (market.SupplyLevel < 0.5f)
                factors.Add("Limited supply availability");
            if (market.PriceVolatility > 0.3f)
                factors.Add("High price volatility");
            if (market.SeasonalFactor > 1.1f)
                factors.Add("Favorable seasonal conditions");
            if (market.CompetitiveIndex < 0.6f)
                factors.Add("Intense market competition");
            
            return factors;
        }
        
        private List<string> GenerateForecastRecommendations(MarketAnalysisData market, int daysAhead)
        {
            var recommendations = new List<string>();
            
            if (market.DemandLevel > 0.8f && market.SupplyLevel < 0.6f)
                recommendations.Add("Increase production to meet high demand");
            
            if (market.PriceVolatility > 0.3f)
                recommendations.Add("Consider flexible pricing strategies");
            
            if (IsHighDemandSeason(System.DateTime.Now.AddDays(daysAhead).Month))
                recommendations.Add("Prepare inventory for seasonal demand peak");
            
            if (market.CompetitiveIndex < 0.6f)
                recommendations.Add("Focus on product differentiation");
            
            return recommendations;
        }
        
        private MarketForecast CreateDefaultMarketForecast(int daysAhead)
        {
            return new MarketForecast
            {
                ForecastDate = System.DateTime.Now,
                ForecastPeriodDays = daysAhead,
                PredictedPriceChange = 0f,
                PredictedDemandChange = 0f,
                SeasonalFactors = new List<string> { "Insufficient data for seasonal analysis" },
                ConfidenceLevel = 0f,
                KeyFactors = new List<string> { "Limited market data available" },
                Recommendations = new List<string> { "Gather more market data for accurate forecasting" }
            };
        }
        
        private bool IsHighDemandSeason(int month) => month >= 3 && month <= 8;
        private bool IsLowDemandSeason(int month) => month >= 11 || month <= 2;
        
        private string GetSeasonName(int month)
        {
            return month switch
            {
                3 or 4 or 5 => "Spring",
                6 or 7 or 8 => "Summer", 
                9 or 10 or 11 => "Fall",
                _ => "Winter"
            };
        }
        
        private AIPriority ConvertMarketUrgency(MarketUrgency urgency)
        {
            return urgency switch
            {
                MarketUrgency.High => AIPriority.High,
                MarketUrgency.Medium => AIPriority.Medium,
                MarketUrgency.Low => AIPriority.Low,
                _ => AIPriority.Medium
            };
        }
        
        private string DetermineMarketComplexity(MarketInsight insight)
        {
            return insight.Category switch
            {
                MarketInsightCategory.Competition => "High",
                MarketInsightCategory.Opportunity => "Medium",
                MarketInsightCategory.PriceTrend => "Low",
                _ => "Medium"
            };
        }
        
        #endregion

        #region Private Fields for System References
        
        private List<ChimeraManager> _managers;
        
        #endregion
    }
}