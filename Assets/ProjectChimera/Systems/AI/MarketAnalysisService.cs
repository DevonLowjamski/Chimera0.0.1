using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.AI;
using ProjectChimera.Systems.Economy;
using AIRecommendationPriority = ProjectChimera.Data.AI.RecommendationPriority;
using AIProductMarketData = ProjectChimera.Data.AI.ProductMarketData;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC014-1b-3: Specialized service for market and economic AI analysis
    /// Extracted from monolithic AIAdvisorManager.cs to handle market trends,
    /// pricing opportunities, and product demand analysis.
    /// </summary>
    public class MarketAnalysisService : IMarketAnalysisService
    {
        private bool _isInitialized = false;
        private MarketManager _marketManager;
        private TradingManager _tradingManager;
        private List<DataInsight> _marketInsights = new List<DataInsight>();
        private List<AIRecommendation> _marketRecommendations = new List<AIRecommendation>();
        private AIProductMarketData _cachedMarketData;
        
        // Analysis configuration
        private float _profitabilityThreshold = 0.15f;
        private float _demandThreshold = 0.7f;
        private float _volatilityThreshold = 0.3f;
        private DateTime _lastMarketDataUpdate;
        
        public bool IsInitialized => _isInitialized;
        
        public void Initialize()
        {
            if (_isInitialized) return;
            
            _marketManager = GameManager.Instance?.GetManager<MarketManager>();
            _tradingManager = GameManager.Instance?.GetManager<TradingManager>();
            
            if (_marketManager == null)
            {
                Debug.LogWarning("[MarketAnalysisService] MarketManager not found - using simulated data");
            }
            
            if (_tradingManager == null)
            {
                Debug.LogWarning("[MarketAnalysisService] TradingManager not found - using simulated data");
            }
            
            _cachedMarketData = GenerateSimulatedMarketData();
            _lastMarketDataUpdate = DateTime.Now;
            
            _isInitialized = true;
            Debug.Log("[MarketAnalysisService] Initialized successfully");
        }
        
        public void Shutdown()
        {
            _marketInsights.Clear();
            _marketRecommendations.Clear();
            _cachedMarketData = null;
            _isInitialized = false;
            Debug.Log("[MarketAnalysisService] Shutdown complete");
        }
        
        public void PerformAnalysis(AnalysisSnapshot snapshot)
        {
            if (!_isInitialized) return;
            
            // Clear previous analysis results
            _marketRecommendations.Clear();
            
            // Update market data if needed
            if ((DateTime.Now - _lastMarketDataUpdate).TotalMinutes > 15)
            {
                _cachedMarketData = GenerateSimulatedMarketData();
                _lastMarketDataUpdate = DateTime.Now;
            }
            
            // Perform market-specific analysis
            AnalyzeMarketTrendData(snapshot);
            AnalyzeProfitabilityOpportunities(snapshot);
            AnalyzeSupplyDemandBalance(snapshot);
            
            Debug.Log($"[MarketAnalysisService] Analysis complete - {_marketRecommendations.Count} recommendations generated");
        }
        
        public List<AIRecommendation> AnalyzeMarketTrends()
        {
            var recommendations = new List<AIRecommendation>();
            
            if (_cachedMarketData != null)
            {
                // Analyze price trends
                foreach (var product in _cachedMarketData.ProductPrices)
                {
                    if (product.Value.TrendDirection > 1.05f) // Rising trend
                    {
                        recommendations.Add(CreateMarketRecommendation(
                            $"Rising Demand: {product.Key}",
                            $"{product.Key} prices trending upward (+{(product.Value.TrendDirection - 1) * 100:F1}%)",
                            $"Consider increasing production of {product.Key} to capitalize on rising prices",
                            AIRecommendationPriority.Medium
                        ));
                    }
                    else if (product.Value.TrendDirection < 0.95f) // Declining trend
                    {
                        recommendations.Add(CreateMarketRecommendation(
                            $"Price Decline: {product.Key}",
                            $"{product.Key} prices declining ({(product.Value.TrendDirection - 1) * 100:F1}%)",
                            $"Consider reducing {product.Key} production or finding alternative markets",
                            AIRecommendationPriority.Low
                        ));
                    }
                }
                
                // Analyze overall market volatility
                if (_cachedMarketData.MarketVolatility > _volatilityThreshold)
                {
                    recommendations.Add(CreateMarketRecommendation(
                        "High Market Volatility",
                        "Market showing increased volatility",
                        "Consider diversifying product portfolio to reduce risk during volatile periods",
                        AIRecommendationPriority.High
                    ));
                }
            }
            
            return recommendations;
        }
        
        public List<AIRecommendation> AnalyzePricingOpportunities()
        {
            var recommendations = new List<AIRecommendation>();
            
            if (_cachedMarketData != null)
            {
                // Find underpriced opportunities
                foreach (var product in _cachedMarketData.ProductPrices)
                {
                    if (product.Value.CurrentPrice < product.Value.OptimalPrice * 0.9f)
                    {
                        var potentialGain = (product.Value.OptimalPrice - product.Value.CurrentPrice) / product.Value.CurrentPrice;
                        
                        recommendations.Add(CreatePricingRecommendation(
                            $"Undervalued Product: {product.Key}",
                            $"{product.Key} trading below optimal price ({potentialGain * 100:F1}% upside)",
                            $"Current market conditions suggest {product.Key} could support higher pricing",
                            AIRecommendationPriority.Medium
                        ));
                    }
                }
                
                // Analyze competitive positioning
                if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
                {
                    recommendations.Add(CreatePricingRecommendation(
                        "Competitive Pricing Analysis",
                        "Opportunity for premium positioning identified",
                        "Market analysis suggests room for premium pricing on high-quality products",
                        AIRecommendationPriority.Low
                    ));
                }
            }
            
            return recommendations;
        }
        
        public List<AIRecommendation> AnalyzeProductDemand()
        {
            var recommendations = new List<AIRecommendation>();
            
            if (_cachedMarketData != null)
            {
                // Analyze demand patterns
                foreach (var demand in _cachedMarketData.DemandData)
                {
                    if (demand.Value.CurrentDemand > _demandThreshold)
                    {
                        recommendations.Add(CreateDemandRecommendation(
                            $"High Demand: {demand.Key}",
                            $"{demand.Key} showing strong demand ({demand.Value.CurrentDemand * 100:F0}%)",
                            $"Consider increasing {demand.Key} production to meet strong market demand",
                            AIRecommendationPriority.High
                        ));
                    }
                    else if (demand.Value.CurrentDemand < 0.4f)
                    {
                        recommendations.Add(CreateDemandRecommendation(
                            $"Low Demand: {demand.Key}",
                            $"{demand.Key} demand below expectations ({demand.Value.CurrentDemand * 100:F0}%)",
                            $"Consider reducing {demand.Key} production or exploring new markets",
                            AIRecommendationPriority.Medium
                        ));
                    }
                }
                
                // Seasonal demand analysis
                if (UnityEngine.Random.Range(0f, 1f) < 0.4f)
                {
                    recommendations.Add(CreateDemandRecommendation(
                        "Seasonal Demand Pattern",
                        "Seasonal demand shifts detected",
                        "Adjust production planning to align with seasonal demand patterns",
                        AIRecommendationPriority.Medium
                    ));
                }
            }
            
            return recommendations;
        }
        
        public List<DataInsight> GetMarketInsights()
        {
            return new List<DataInsight>(_marketInsights);
        }
        
        public AIProductMarketData GetMarketData()
        {
            return _cachedMarketData;
        }
        
        #region Private Helper Methods
        
        private void AnalyzeMarketTrendData(AnalysisSnapshot snapshot)
        {
            var trendRecommendations = AnalyzeMarketTrends();
            _marketRecommendations.AddRange(trendRecommendations);
            
            // Create insights based on trend analysis
            if (trendRecommendations.Any(r => r.Priority == AIRecommendationPriority.High))
            {
                CreateMarketInsight(
                    "Significant Market Trends",
                    "Multiple high-impact market trends identified",
                    InsightSeverity.High,
                    "MarketTrends"
                );
            }
        }
        
        private void AnalyzeProfitabilityOpportunities(AnalysisSnapshot snapshot)
        {
            var pricingRecommendations = AnalyzePricingOpportunities();
            _marketRecommendations.AddRange(pricingRecommendations);
            
            // Analyze profitability metrics
            if (snapshot?.EconomicData != null && snapshot.EconomicData.ROI < _profitabilityThreshold)
            {
                CreateMarketInsight(
                    "Profitability Concern",
                    $"ROI below threshold ({snapshot.EconomicData.ROI * 100:F1}% vs {_profitabilityThreshold * 100:F1}%)",
                    InsightSeverity.Medium,
                    "Profitability"
                );
            }
        }
        
        private void AnalyzeSupplyDemandBalance(AnalysisSnapshot snapshot)
        {
            var demandRecommendations = AnalyzeProductDemand();
            _marketRecommendations.AddRange(demandRecommendations);
        }
        
        private AIProductMarketData GenerateSimulatedMarketData()
        {
            var marketData = new AIProductMarketData
            {
                MarketVolatility = UnityEngine.Random.Range(0.1f, 0.5f),
                OverallTrend = UnityEngine.Random.Range(0.9f, 1.2f),
                ProductPrices = new Dictionary<string, PriceData>(),
                DemandData = new Dictionary<string, DemandData>()
            };
            
            // Generate sample product data
            var products = new[] { "Premium Flower", "Concentrates", "Edibles", "Pre-Rolls", "Seeds" };
            
            foreach (var product in products)
            {
                marketData.ProductPrices[product] = new PriceData
                {
                    CurrentPrice = UnityEngine.Random.Range(10f, 100f),
                    OptimalPrice = UnityEngine.Random.Range(15f, 110f),
                    TrendDirection = UnityEngine.Random.Range(0.85f, 1.25f)
                };
                
                marketData.DemandData[product] = new DemandData
                {
                    CurrentDemand = UnityEngine.Random.Range(0.3f, 0.9f),
                    SeasonalFactor = UnityEngine.Random.Range(0.8f, 1.3f)
                };
            }
            
            return marketData;
        }
        
        private AIRecommendation CreateMarketRecommendation(string title, string summary, string description, AIRecommendationPriority priority)
        {
            return new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Summary = summary,
                Description = description,
                Priority = priority,
                Category = "MarketTrends",
                CreationTime = DateTime.Now,
                ExpirationTime = DateTime.Now.AddHours(12),
                ConfidenceScore = 0.75f,
                EstimatedImpact = priority == AIRecommendationPriority.High ? 0.8f : 0.6f,
                IsActive = true
            };
        }
        
        private AIRecommendation CreatePricingRecommendation(string title, string summary, string description, AIRecommendationPriority priority)
        {
            return new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Summary = summary,
                Description = description,
                Priority = priority,
                Category = "PricingOptimization",
                CreationTime = DateTime.Now,
                ExpirationTime = DateTime.Now.AddHours(8),
                ConfidenceScore = 0.7f,
                EstimatedImpact = 0.7f,
                IsActive = true
            };
        }
        
        private AIRecommendation CreateDemandRecommendation(string title, string summary, string description, AIRecommendationPriority priority)
        {
            return new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Summary = summary,
                Description = description,
                Priority = priority,
                Category = "DemandAnalysis",
                CreationTime = DateTime.Now,
                ExpirationTime = DateTime.Now.AddHours(24),
                ConfidenceScore = 0.8f,
                EstimatedImpact = priority == AIRecommendationPriority.High ? 0.9f : 0.6f,
                IsActive = true
            };
        }
        
        private void CreateMarketInsight(string title, string description, InsightSeverity severity, string category)
        {
            var insight = new DataInsight
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Description = description,
                Severity = severity,
                Category = category,
                CreationTime = DateTime.Now,
                ConfidenceScore = 0.8f,
                IsActive = true
            };
            
            _marketInsights.Add(insight);
            
            // Limit insights collection size
            if (_marketInsights.Count > 30)
            {
                _marketInsights.RemoveAt(0);
            }
        }
        
        #endregion
    }
}