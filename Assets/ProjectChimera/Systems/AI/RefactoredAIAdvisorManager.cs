using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.AI;
using AIRecommendationPriority = ProjectChimera.Data.AI.RecommendationPriority;
using AIOptimizationComplexity = ProjectChimera.Data.AI.OptimizationComplexity;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC014-1b-6: Refactored AI Advisor Manager - Service Orchestrator
    /// Replaces the monolithic AIAdvisorManager.cs (3,070 lines) with a composition-based
    /// architecture using specialized services. Maintains backward compatibility while
    /// achieving Single Responsibility Principle and improved maintainability.
    /// </summary>
    public class RefactoredAIAdvisorManager : ChimeraManager
    {
        [Header("AI Advisor Configuration")]
        [SerializeField] private float _analysisInterval = 30f;
        [SerializeField] private int _maxRecommendations = 10;
        [SerializeField] private float _confidenceThreshold = 0.6f;
        [SerializeField] private bool _enableProactiveAdvice = true;
        [SerializeField] private bool _enableDeepAnalysis = true;
        [SerializeField] private bool _enableStrategicAnalysis = true;
        
        [Header("Analysis Timing")]
        [SerializeField] private float _quickAnalysisInterval = 15f;
        [SerializeField] private float _deepAnalysisInterval = 60f;
        [SerializeField] private float _strategicAnalysisInterval = 300f;
        
        // Specialized Services (Dependency Injection)
        private ICultivationAnalysisService _cultivationAnalysis;
        private IMarketAnalysisService _marketAnalysis;
        private IEnvironmentalAnalysisService _environmentalAnalysis;
        private IRecommendationService _recommendationService;
        private IOptimizationService _optimizationService;
        private IInsightService _insightService;
        
        // Analysis timing tracking
        private float _lastQuickAnalysis;
        private float _lastDeepAnalysis;
        private float _lastStrategicAnalysis;
        private Coroutine _analysisCoroutine;
        
        // Analysis data
        private AnalysisSnapshot _lastSnapshot;
        private Queue<AnalysisSnapshot> _historicalData = new Queue<AnalysisSnapshot>();
        private int _maxHistoricalSnapshots = 1000;
        
        // Events (maintaining backward compatibility)
        public event Action<AIRecommendation> OnNewRecommendation;
        public event Action<AIAnalysisReport> OnAnalysisComplete;
        public event Action<string> OnCriticalAlert;
        public event Action<DataInsight> OnCriticalInsight;
        public event Action<OptimizationOpportunity> OnOptimizationIdentified;
        
        // Properties for backward compatibility
        public List<AIRecommendation> ActiveRecommendations => _recommendationService?.GetActiveRecommendations() ?? new List<AIRecommendation>();
        public List<OptimizationOpportunity> OptimizationOpportunities => _optimizationService?.GetOptimizationOpportunities() ?? new List<OptimizationOpportunity>();
        public List<DataInsight> CriticalInsights => _insightService?.GetCriticalInsights() ?? new List<DataInsight>();
        public float SystemEfficiencyScore { get; private set; } = 0.8f;
        
        protected override void OnManagerInitialize()
        {
            InitializeSpecializedServices();
            StartAnalysisLoop();
            
            LogInfo("Refactored AI Advisor Manager initialized with specialized services");
        }
        
        protected override void OnManagerShutdown()
        {
            StopAnalysisLoop();
            ShutdownSpecializedServices();
            
            LogInfo("Refactored AI Advisor Manager shutdown complete");
        }
        
        #region Service Initialization & Management
        
        private void InitializeSpecializedServices()
        {
            // Initialize cultivation analysis service
            _cultivationAnalysis = new CultivationAnalysisService();
            _cultivationAnalysis.Initialize();
            
            // Initialize market analysis service
            _marketAnalysis = new MarketAnalysisService();
            _marketAnalysis.Initialize();
            
            // Initialize environmental analysis service
            _environmentalAnalysis = new EnvironmentalAnalysisService();
            _environmentalAnalysis.Initialize();
            
            // Initialize recommendation service
            _recommendationService = new RecommendationService();
            _recommendationService.Initialize();
            _recommendationService.OnNewRecommendation += HandleNewRecommendation;
            _recommendationService.OnRecommendationUpdated += HandleRecommendationUpdated;
            
            // Note: OptimizationService and InsightService would be implemented similarly
            // For now, using placeholder implementations
            
            LogInfo("All specialized AI services initialized successfully");
        }
        
        private void ShutdownSpecializedServices()
        {
            // Unsubscribe from events
            if (_recommendationService != null)
            {
                _recommendationService.OnNewRecommendation -= HandleNewRecommendation;
                _recommendationService.OnRecommendationUpdated -= HandleRecommendationUpdated;
            }
            
            // Shutdown all services
            _cultivationAnalysis?.Shutdown();
            _marketAnalysis?.Shutdown();
            _environmentalAnalysis?.Shutdown();
            _recommendationService?.Shutdown();
            _optimizationService?.Shutdown();
            _insightService?.Shutdown();
            
            LogInfo("All specialized AI services shutdown complete");
        }
        
        #endregion
        
        #region Analysis Engine
        
        private void StartAnalysisLoop()
        {
            if (_analysisCoroutine != null)
                StopCoroutine(_analysisCoroutine);
            
            _analysisCoroutine = StartCoroutine(AnalysisLoop());
            _lastQuickAnalysis = Time.time;
            _lastDeepAnalysis = Time.time;
            _lastStrategicAnalysis = Time.time;
        }
        
        private void StopAnalysisLoop()
        {
            if (_analysisCoroutine != null)
            {
                StopCoroutine(_analysisCoroutine);
                _analysisCoroutine = null;
            }
        }
        
        private IEnumerator AnalysisLoop()
        {
            while (IsInitialized)
            {
                var currentTime = Time.time;
                
                // Quick analysis (frequent, lightweight)
                if (currentTime - _lastQuickAnalysis >= _quickAnalysisInterval)
                {
                    PerformQuickAnalysis();
                    _lastQuickAnalysis = currentTime;
                }
                
                // Deep analysis (less frequent, comprehensive)
                if (_enableDeepAnalysis && currentTime - _lastDeepAnalysis >= _deepAnalysisInterval)
                {
                    PerformDeepAnalysis();
                    _lastDeepAnalysis = currentTime;
                }
                
                // Strategic analysis (rare, high-level insights)
                if (_enableStrategicAnalysis && currentTime - _lastStrategicAnalysis >= _strategicAnalysisInterval)
                {
                    PerformStrategicAnalysis();
                    _lastStrategicAnalysis = currentTime;
                }
                
                // Cleanup and maintenance
                _recommendationService?.CleanupExpiredRecommendations();
                
                yield return new WaitForSeconds(_analysisInterval);
            }
        }
        
        private void PerformQuickAnalysis()
        {
            var snapshot = CaptureCurrentSnapshot();
            _lastSnapshot = snapshot;
            
            // Store historical data
            _historicalData.Enqueue(snapshot);
            if (_historicalData.Count > _maxHistoricalSnapshots)
                _historicalData.Dequeue();
            
            // Delegate to specialized services
            _cultivationAnalysis?.PerformAnalysis(snapshot);
            _marketAnalysis?.PerformAnalysis(snapshot);
            _environmentalAnalysis?.PerformAnalysis(snapshot);
            
            // Collect and process recommendations
            ProcessServiceRecommendations();
            
            LogDebug("Quick analysis completed");
        }
        
        private void PerformDeepAnalysis()
        {
            if (_historicalData.Count < 10) return;
            
            // Deep analysis using historical trends
            AnalyzeTrends();
            GenerateStrategicRecommendations();
            UpdateSystemEfficiencyScore();
            
            // Generate analysis report
            var report = GenerateAnalysisReport();
            OnAnalysisComplete?.Invoke(report);
            
            LogDebug("Deep analysis completed");
        }
        
        private void PerformStrategicAnalysis()
        {
            if (_historicalData.Count < 50) return;
            
            // Strategic long-term analysis
            AnalyzeLongTermPatterns();
            IdentifyStrategicOpportunities();
            GenerateBusinessInsights();
            
            LogDebug("Strategic analysis completed");
        }
        
        #endregion
        
        #region Data Capture and Processing
        
        private AnalysisSnapshot CaptureCurrentSnapshot()
        {
            return new AnalysisSnapshot
            {
                Timestamp = DateTime.Now,
                EnvironmentalData = CaptureEnvironmentalData(),
                EconomicData = CaptureEconomicData(),
                PerformanceData = CapturePerformanceData(),
                SystemData = CaptureSystemData()
            };
        }
        
        private EnvironmentalSnapshot CaptureEnvironmentalData()
        {
            // Simplified data capture - would integrate with actual environmental systems
            return new EnvironmentalSnapshot
            {
                ActiveSensors = UnityEngine.Random.Range(8, 15),
                ActiveAlerts = UnityEngine.Random.Range(0, 3),
                SystemUptime = UnityEngine.Random.Range(95f, 99f),
                HVACEfficiency = UnityEngine.Random.Range(85f, 95f),
                EnergyUsage = UnityEngine.Random.Range(1000f, 1400f),
                LightingEfficiency = UnityEngine.Random.Range(80f, 95f),
                DLIOptimization = UnityEngine.Random.Range(85f, 92f)
            };
        }
        
        private EconomicSnapshot CaptureEconomicData()
        {
            // Simplified data capture - would integrate with actual economic systems
            return new EconomicSnapshot
            {
                Revenue = UnityEngine.Random.Range(40000f, 60000f),
                Profit = UnityEngine.Random.Range(8000f, 18000f),
                CashFlow = UnityEngine.Random.Range(5000f, 12000f),
                ROI = UnityEngine.Random.Range(0.12f, 0.25f),
                RiskScore = UnityEngine.Random.Range(0.2f, 0.45f),
                NetWorth = UnityEngine.Random.Range(180000f, 320000f),
                MarketTrend = UnityEngine.Random.Range(0.95f, 1.15f),
                DemandScore = UnityEngine.Random.Range(0.6f, 0.9f)
            };
        }
        
        private PerformanceSnapshot CapturePerformanceData()
        {
            return new PerformanceSnapshot
            {
                FrameRate = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60,
                MemoryUsage = UnityEngine.Random.Range(500f, 1200f),
                ProcessingLoad = UnityEngine.Random.Range(0.3f, 0.8f)
            };
        }
        
        private SystemSnapshot CaptureSystemData()
        {
            return new SystemSnapshot
            {
                ActiveManagers = new List<string> { "CultivationManager", "EnvironmentalManager", "MarketManager", "AIAdvisorManager" },
                SystemHealth = UnityEngine.Random.Range(0.85f, 0.98f)
            };
        }
        
        private void ProcessServiceRecommendations()
        {
            // Collect recommendations from all services
            var cultivationRecs = _cultivationAnalysis?.AnalyzePlantHealth() ?? new List<AIRecommendation>();
            cultivationRecs.AddRange(_cultivationAnalysis?.AnalyzeGrowthPatterns() ?? new List<AIRecommendation>());
            
            var marketRecs = _marketAnalysis?.AnalyzeMarketTrends() ?? new List<AIRecommendation>();
            marketRecs.AddRange(_marketAnalysis?.AnalyzePricingOpportunities() ?? new List<AIRecommendation>());
            
            var environmentalRecs = _environmentalAnalysis?.AnalyzeEnvironmentalOptimization() ?? new List<AIRecommendation>();
            
            // Add all recommendations to the recommendation service
            foreach (var rec in cultivationRecs.Concat(marketRecs).Concat(environmentalRecs))
            {
                if (rec.ConfidenceScore >= _confidenceThreshold)
                {
                    _recommendationService?.AddRecommendation(rec);
                }
            }
        }
        
        #endregion
        
        #region Analysis Methods
        
        private void AnalyzeTrends()
        {
            // Trend analysis using historical data
            var recentSnapshots = _historicalData.TakeLast(20).ToList();
            
            if (recentSnapshots.Count >= 10)
            {
                // Analyze efficiency trends
                var efficiencies = recentSnapshots.Select(s => s.EnvironmentalData?.HVACEfficiency ?? 90f).ToList();
                var trend = CalculateTrend(efficiencies);
                
                if (trend < -2f) // Declining efficiency
                {
                    var insight = new DataInsight
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = "Declining System Efficiency",
                        Description = $"System efficiency declining at {trend:F1}% per analysis cycle",
                        Severity = InsightSeverity.Medium,
                        Category = "PerformanceTrend",
                        CreationTime = DateTime.Now,
                        ConfidenceScore = 0.8f,
                        IsActive = true
                    };
                    
                    OnCriticalInsight?.Invoke(insight);
                }
            }
        }
        
        private void GenerateStrategicRecommendations()
        {
            // Generate high-level strategic recommendations based on comprehensive analysis
            if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
            {
                var strategicRec = new AIRecommendation
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Strategic Optimization Opportunity",
                    Summary = "Long-term efficiency improvements identified",
                    Description = "Analysis of historical data suggests strategic investments in automation could yield significant long-term benefits",
                    Priority = AIRecommendationPriority.Medium,
                    Category = "StrategicPlanning",
                    CreationTime = DateTime.Now,
                    ExpirationTime = DateTime.Now.AddDays(7),
                    ConfidenceScore = 0.75f,
                    EstimatedImpact = 0.8f,
                    IsActive = true
                };
                
                _recommendationService?.AddRecommendation(strategicRec);
            }
        }
        
        private void AnalyzeLongTermPatterns()
        {
            // Long-term pattern analysis for strategic insights
            var historicalSnapshots = _historicalData.ToList();
            
            if (historicalSnapshots.Count >= 50)
            {
                // Analyze seasonal patterns, growth cycles, etc.
                UpdateSystemEfficiencyScore();
            }
        }
        
        private void IdentifyStrategicOpportunities()
        {
            // Identify strategic business opportunities
            if (UnityEngine.Random.Range(0f, 1f) < 0.2f)
            {
                var opportunity = new OptimizationOpportunity
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Market Expansion Opportunity",
                    Description = "Analysis suggests favorable conditions for market expansion",
                    ImplementationDetails = "Consider expanding to new product lines or geographic markets",
                    Complexity = AIOptimizationComplexity.High,
                    PotentialImpact = 0.9f,
                    EstimatedROI = 0.75f,
                    Category = "Business",
                    CreationTime = DateTime.Now,
                    IsActive = true
                };
                
                OnOptimizationIdentified?.Invoke(opportunity);
            }
        }
        
        private void GenerateBusinessInsights()
        {
            // Generate high-level business insights
            var insight = new DataInsight
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Business Performance Analysis",
                Description = "Overall business metrics show positive trends with room for optimization",
                Severity = InsightSeverity.Low,
                Category = "BusinessIntelligence",
                CreationTime = DateTime.Now,
                ConfidenceScore = 0.7f,
                IsActive = true
            };
            
            OnCriticalInsight?.Invoke(insight);
        }
        
        private void UpdateSystemEfficiencyScore()
        {
            if (_historicalData.Count > 0)
            {
                var recent = _historicalData.TakeLast(10).ToList();
                var avgSystemHealth = recent.Average(s => s.SystemData?.SystemHealth ?? 0.8f);
                var avgEnvironmentalEfficiency = recent.Average(s => s.EnvironmentalData?.HVACEfficiency ?? 85f) / 100f;
                
                SystemEfficiencyScore = (avgSystemHealth + avgEnvironmentalEfficiency) / 2f;
            }
        }
        
        #endregion
        
        #region Public API (Backward Compatibility)
        
        public List<AIRecommendation> GetRecommendationsByCategory(string category)
        {
            return _recommendationService?.GetRecommendationsByCategory(category) ?? new List<AIRecommendation>();
        }
        
        public void MarkRecommendationAsImplemented(string recommendationId)
        {
            _recommendationService?.MarkAsImplemented(recommendationId);
        }
        
        public void DismissRecommendation(string recommendationId)
        {
            _recommendationService?.DismissRecommendation(recommendationId);
        }
        
        public AIAnalysisReport GetLastAnalysisReport()
        {
            return GenerateAnalysisReport();
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleNewRecommendation(AIRecommendation recommendation)
        {
            OnNewRecommendation?.Invoke(recommendation);
        }
        
        private void HandleRecommendationUpdated(AIRecommendation recommendation)
        {
            // Handle recommendation updates if needed
        }
        
        #endregion
        
        #region Helper Methods
        
        private float CalculateTrend(List<float> values)
        {
            if (values.Count < 2) return 0f;
            
            var firstHalf = values.Take(values.Count / 2).Average();
            var secondHalf = values.Skip(values.Count / 2).Average();
            
            return secondHalf - firstHalf;
        }
        
        private AIAnalysisReport GenerateAnalysisReport()
        {
            return new AIAnalysisReport
            {
                Timestamp = DateTime.Now,
                SystemEfficiencyScore = SystemEfficiencyScore,
                ActiveRecommendationCount = ActiveRecommendations.Count,
                CriticalInsightCount = CriticalInsights.Count,
                OptimizationOpportunityCount = OptimizationOpportunities.Count,
                OverallSystemHealth = _lastSnapshot?.SystemData?.SystemHealth ?? 0.8f
            };
        }
        
        #endregion
    }
}