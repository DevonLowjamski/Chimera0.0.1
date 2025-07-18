using System;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.AI;
using AIOptimizationComplexity = ProjectChimera.Data.AI.OptimizationComplexity;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// Base interface for all AI analysis services in the advisor system
    /// </summary>
    public interface IAIAnalysisService
    {
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
        void PerformAnalysis(AnalysisSnapshot snapshot);
    }

    /// <summary>
    /// Interface for cultivation-specific analysis
    /// </summary>
    public interface ICultivationAnalysisService : IAIAnalysisService
    {
        List<AIRecommendation> AnalyzePlantHealth();
        List<AIRecommendation> AnalyzeGrowthPatterns();
        List<AIRecommendation> AnalyzeCareOptimization();
        List<DataInsight> GetCultivationInsights();
    }

    /// <summary>
    /// Interface for market and economic analysis
    /// </summary>
    public interface IMarketAnalysisService : IAIAnalysisService
    {
        List<AIRecommendation> AnalyzeMarketTrends();
        List<AIRecommendation> AnalyzePricingOpportunities();
        List<AIRecommendation> AnalyzeProductDemand();
        List<DataInsight> GetMarketInsights();
        ProjectChimera.Data.AI.ProductMarketData GetMarketData();
    }

    /// <summary>
    /// Interface for environmental analysis
    /// </summary>
    public interface IEnvironmentalAnalysisService : IAIAnalysisService
    {
        List<AIRecommendation> AnalyzeEnvironmentalOptimization();
        List<AIRecommendation> AnalyzeClimateControl();
        List<OptimizationOpportunity> GetEnvironmentalOptimizations();
        List<DataInsight> GetEnvironmentalInsights();
    }

    /// <summary>
    /// Interface for genetics and breeding analysis
    /// </summary>
    public interface IGeneticsAnalysisService : IAIAnalysisService
    {
        List<AIRecommendation> AnalyzeBreedingOpportunities();
        List<AIRecommendation> AnalyzeGeneticDiversity();
        List<AIRecommendation> AnalyzeTraitOptimization();
        List<DataInsight> GetGeneticsInsights();
    }

    /// <summary>
    /// Interface for recommendation management
    /// </summary>
    public interface IRecommendationService
    {
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
        
        List<AIRecommendation> GetActiveRecommendations();
        List<AIRecommendation> GetRecommendationsByCategory(string category);
        void AddRecommendation(AIRecommendation recommendation);
        void MarkAsImplemented(string recommendationId);
        void DismissRecommendation(string recommendationId);
        void UpdateRecommendationStatus();
        void CleanupExpiredRecommendations();
        
        event Action<AIRecommendation> OnNewRecommendation;
        event Action<AIRecommendation> OnRecommendationUpdated;
    }

    /// <summary>
    /// Interface for optimization opportunity detection
    /// </summary>
    public interface IOptimizationService
    {
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
        
        List<OptimizationOpportunity> GetOptimizationOpportunities();
        void AnalyzeOptimizations(AnalysisSnapshot snapshot);
        void CreateOptimizationOpportunity(string title, string description, string implementation, AIOptimizationComplexity complexity, float potentialImpact);
        
        event Action<OptimizationOpportunity> OnOptimizationIdentified;
    }

    /// <summary>
    /// Interface for data insights management
    /// </summary>
    public interface IInsightService
    {
        bool IsInitialized { get; }
        void Initialize();
        void Shutdown();
        
        List<DataInsight> GetCriticalInsights();
        List<DataInsight> GetInsightsByCategory(string category);
        void AddInsight(DataInsight insight);
        void CreateInsight(string title, string description, InsightSeverity severity, string category);
        
        event Action<DataInsight> OnCriticalInsight;
    }
}