using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.AI;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Economy;
using ProjectChimera.Systems.Environment;
using DataEnvironmentalAnalysisResult = ProjectChimera.Data.AI.EnvironmentalAnalysisResult;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Genetics;
using AIPriority = ProjectChimera.Data.AI.RecommendationPriority;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC-012-1: AI Advisor Manager - Intelligent analysis and recommendation system
    /// Analyzes cultivation data, genetics, market conditions, and environmental factors
    /// to provide actionable recommendations to players
    /// </summary>
    public class PC012AIAdvisorManager : ChimeraManager
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
        
        // Events
        public event Action<AIRecommendation> OnNewRecommendation;
        public event Action<AIAnalysisReport> OnAnalysisComplete;
        public event Action<string> OnCriticalAlert;
        
        protected override void OnManagerInitialize()
        {
            InitializeAnalysisEngines();
            StartCoroutine(PeriodicAnalysisCoroutine());
            
            Debug.Log("[PC012AIAdvisorManager] AI Advisor initialized with real data analysis capabilities");
        }
        
        private void InitializeAnalysisEngines()
        {
            _cultivationAnalysis = new CultivationAnalysisEngine();
            _marketAnalysis = new MarketAnalysisEngine();
            _environmentalAnalysis = new EnvironmentalAnalysisEngine();
            _geneticsAnalysis = new GeneticsAnalysisEngine();
        }
        
        private System.Collections.IEnumerator PeriodicAnalysisCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_analysisInterval);
                
                if (IsInitialized && _enableProactiveAdvice)
                {
                    PerformComprehensiveAnalysis();
                }
            }
        }
        
        /// <summary>
        /// Perform comprehensive analysis of all game systems
        /// </summary>
        public AIAnalysisReport PerformComprehensiveAnalysis()
        {
            var report = new AIAnalysisReport
            {
                AnalysisId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                AnalysisType = AIAnalysisType.Comprehensive,
                GeneratedRecommendations = new List<AIRecommendation>()
            };
            
            // Find managers if not assigned
            if (_cultivationManager == null)
                _cultivationManager = FindObjectOfType<CultivationManager>();
            if (_marketManager == null)
                _marketManager = FindObjectOfType<MarketManager>();
            if (_tradingManager == null)
                _tradingManager = FindObjectOfType<TradingManager>();
            if (_environmentalManager == null)
                _environmentalManager = FindObjectOfType<EnvironmentalManager>();
            
            // Analyze cultivation data
            if (_cultivationManager != null && _cultivationManager.IsInitialized)
            {
                report.CultivationAnalysis = _cultivationAnalysis.AnalyzeCultivationData(_cultivationManager);
                GenerateCultivationRecommendations(report.CultivationAnalysis);
            }
            
            // Analyze market conditions
            if (_marketManager != null && _marketManager.IsInitialized)
            {
                report.MarketAnalysis = _marketAnalysis.AnalyzeMarketConditions(_marketManager);
                GenerateMarketRecommendations(report.MarketAnalysis);
            }
            
            // Analyze environmental conditions
            if (_environmentalManager != null && _environmentalManager.IsInitialized)
            {
                report.EnvironmentalAnalysis = _environmentalAnalysis.AnalyzeEnvironmentalData(_environmentalManager);
                GenerateEnvironmentalRecommendations(report.EnvironmentalAnalysis);
            }
            
            // Analyze genetics and breeding opportunities
            if (_cultivationManager != null)
            {
                report.GeneticsAnalysis = _geneticsAnalysis.AnalyzeGeneticsData(_cultivationManager);
                GenerateGeneticsRecommendations(report.GeneticsAnalysis);
            }
            
            // Calculate overall score
            report.OverallScore = CalculateOverallScore(report);
            report.Summary = GenerateAnalysisSummary(report);
            
            _lastAnalysisReport = report;
            _lastAnalysisTime = Time.time;
            
            OnAnalysisComplete?.Invoke(report);
            
            Debug.Log($"[PC012AIAdvisorManager] Comprehensive analysis complete. Generated {_currentRecommendations.Count} recommendations");
            
            return report;
        }
        
        private void GenerateCultivationRecommendations(CultivationAnalysisResult analysis)
        {
            if (analysis == null) return;
            
            // Plant health recommendations
            if (analysis.AverageHealth < 0.7f)
            {
                AddRecommendation(CreateRecommendation(
                    "Plant Health Alert",
                    "Plant health needs attention",
                    $"Average plant health is {analysis.AverageHealth:P1}. Consider improving environmental conditions or checking for pests.",
                    RecommendationType.Critical_Action,
                    AIPriority.High,
                    "Cultivation",
                    new List<string>
                    {
                        "Check temperature and humidity levels",
                        "Inspect plants for pests or diseases",
                        "Review nutrient solution composition",
                        "Adjust lighting schedule if needed"
                    },
                    0.85f,
                    8.5f
                ));
            }
            
            // Growth stage distribution recommendations
            if (analysis.GrowthStageDistribution != null && 
                analysis.GrowthStageDistribution.ContainsKey(PlantGrowthStage.Harvest) && 
                analysis.GrowthStageDistribution[PlantGrowthStage.Harvest] > 5)
            {
                AddRecommendation(CreateRecommendation(
                    "Harvest Ready Plants",
                    "Plants ready for harvest",
                    $"{analysis.GrowthStageDistribution[PlantGrowthStage.Harvest]} plants are ready for harvest.",
                    RecommendationType.Alert,
                    AIPriority.Medium,
                    "Cultivation",
                    new List<string>
                    {
                        "Harvest mature plants to prevent over-ripening",
                        "Prepare drying and curing facilities",
                        "Plan next planting cycle"
                    },
                    0.95f,
                    7.0f
                ));
            }
        }
        
        private void GenerateMarketRecommendations(MarketAnalysisResult analysis)
        {
            if (analysis == null) return;
            
            // Price opportunity recommendations
            if (analysis.PriceVolatility > 0.15f)
            {
                AddRecommendation(CreateRecommendation(
                    "Market Volatility Alert",
                    "High price volatility detected",
                    $"High price volatility detected ({analysis.PriceVolatility:P1}). Consider timing sales carefully.",
                    RecommendationType.Alert,
                    AIPriority.High,
                    "Market",
                    new List<string>
                    {
                        "Monitor market trends daily",
                        "Consider selling premium products during price peaks",
                        "Hold lower-grade inventory for better prices"
                    },
                    0.8f,
                    7.5f
                ));
            }
        }
        
        private void GenerateEnvironmentalRecommendations(DataEnvironmentalAnalysisResult analysis)
        {
            if (analysis == null) return;
            
            // Energy efficiency recommendations
            if (analysis.EnergyEfficiency < 0.75f)
            {
                AddRecommendation(CreateRecommendation(
                    "Energy Efficiency Improvement",
                    "Energy efficiency below target",
                    $"Overall energy efficiency is {analysis.EnergyEfficiency:P1}. Consider optimizations.",
                    RecommendationType.Optimization,
                    AIPriority.Medium,
                    "Environmental",
                    new List<string>
                    {
                        "Upgrade to LED lighting systems",
                        "Implement smart scheduling for equipment",
                        "Improve insulation in grow rooms"
                    },
                    0.65f,
                    5.5f
                ));
            }
        }
        
        private void GenerateGeneticsRecommendations(GeneticsAnalysisResult analysis)
        {
            if (analysis == null) return;
            
            // Genetic diversity recommendations
            if (analysis.GeneticDiversity < 0.6f)
            {
                AddRecommendation(CreateRecommendation(
                    "Genetic Diversity Warning",
                    "Low genetic diversity detected",
                    $"Genetic diversity is low ({analysis.GeneticDiversity:P1}). Consider introducing new genetics.",
                    RecommendationType.Alert,
                    AIPriority.Medium,
                    "Genetics",
                    new List<string>
                    {
                        "Source new strains from reputable breeders",
                        "Avoid inbreeding in current stock",
                        "Maintain detailed breeding records"
                    },
                    0.85f,
                    6.5f
                ));
            }
        }
        
        private AIRecommendation CreateRecommendation(string title, string summary, string description, 
            RecommendationType type, AIPriority priority, string category, 
            List<string> actions, float confidence, float impact)
        {
            return new AIRecommendation
            {
                RecommendationId = Guid.NewGuid().ToString(),
                Title = title,
                Summary = summary,
                Description = description,
                Type = type,
                Priority = priority,
                Category = category,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddHours(24),
                Status = RecommendationStatus.Active,
                ConfidenceScore = confidence,
                ImpactScore = impact,
                RequiredActions = actions ?? new List<string>()
            };
        }
        
        private void AddRecommendation(AIRecommendation recommendation)
        {
            if (recommendation.ConfidenceScore >= _confidenceThreshold)
            {
                _currentRecommendations.Add(recommendation);
                
                // Limit recommendations to max count
                if (_currentRecommendations.Count > _maxRecommendations)
                {
                    _currentRecommendations = _currentRecommendations
                        .OrderByDescending(x => x.Priority)
                        .ThenByDescending(x => x.ImpactScore)
                        .Take(_maxRecommendations)
                        .ToList();
                }
                
                OnNewRecommendation?.Invoke(recommendation);
                
                if (recommendation.Priority == AIPriority.High || recommendation.Priority == AIPriority.Critical)
                {
                    OnCriticalAlert?.Invoke(recommendation.Title);
                }
            }
        }
        
        private float CalculateOverallScore(AIAnalysisReport report)
        {
            float cultivationScore = report.CultivationAnalysis?.EfficiencyScore ?? 0.5f;
            float marketScore = report.MarketAnalysis?.MarketScore ?? 0.5f;
            float environmentalScore = report.EnvironmentalAnalysis?.EnvironmentalScore ?? 0.5f;
            float geneticsScore = report.GeneticsAnalysis?.GeneticsScore ?? 0.5f;
            
            return (cultivationScore + marketScore + environmentalScore + geneticsScore) / 4f;
        }
        
        private string GenerateAnalysisSummary(AIAnalysisReport report)
        {
            var summary = $"Analysis completed at {report.Timestamp:HH:mm}. ";
            summary += $"Overall facility score: {report.OverallScore:P1}. ";
            summary += $"Generated {report.GeneratedRecommendations?.Count ?? 0} recommendations.";
            
            return summary;
        }
        
        /// <summary>
        /// Get current active recommendations
        /// </summary>
        public List<AIRecommendation> GetCurrentRecommendations()
        {
            return new List<AIRecommendation>(_currentRecommendations);
        }
        
        /// <summary>
        /// Get recommendations by type
        /// </summary>
        public List<AIRecommendation> GetRecommendationsByType(RecommendationType type)
        {
            return _currentRecommendations.Where(x => x.Type == type).ToList();
        }
        
        /// <summary>
        /// Mark recommendation as completed
        /// </summary>
        public void CompleteRecommendation(string recommendationId)
        {
            var recommendation = _currentRecommendations.FirstOrDefault(x => x.RecommendationId == recommendationId);
            if (recommendation != null)
            {
                recommendation.Status = RecommendationStatus.Implemented;
                recommendation.ImplementedAt = DateTime.Now;
                
                Debug.Log($"[PC012AIAdvisorManager] Recommendation completed: {recommendation.Title}");
            }
        }
        
        /// <summary>
        /// Dismiss recommendation
        /// </summary>
        public void DismissRecommendation(string recommendationId)
        {
            _currentRecommendations.RemoveAll(x => x.RecommendationId == recommendationId);
        }
        
        /// <summary>
        /// Get last analysis report
        /// </summary>
        public AIAnalysisReport GetLastAnalysisReport()
        {
            return _lastAnalysisReport;
        }
        
        /// <summary>
        /// Force immediate analysis
        /// </summary>
        [ContextMenu("Force Analysis")]
        public void ForceAnalysis()
        {
            PerformComprehensiveAnalysis();
        }
        
        /// <summary>
        /// Clear all recommendations
        /// </summary>
        [ContextMenu("Clear Recommendations")]
        public void ClearRecommendations()
        {
            _currentRecommendations.Clear();
            Debug.Log("[PC012AIAdvisorManager] All recommendations cleared");
        }
        
        /// <summary>
        /// Get AI advisor statistics for display
        /// </summary>
        public object GetAIAdvisorStats()
        {
            return new
            {
                TotalRecommendations = _currentRecommendations.Count,
                HighPriorityCount = _currentRecommendations.Count(r => r.Priority == AIPriority.High || r.Priority == AIPriority.Critical),
                LastAnalysisTime = _lastAnalysisTime > 0 ? DateTime.Now.AddSeconds(-(Time.time - _lastAnalysisTime)) : (DateTime?)null,
                OverallScore = _lastAnalysisReport?.OverallScore ?? 0f,
                IsActive = IsInitialized && _enableProactiveAdvice,
                AnalysisInterval = _analysisInterval
            };
        }
        
        protected override void OnManagerShutdown()
        {
            StopAllCoroutines();
            _currentRecommendations.Clear();
        }
    }
}