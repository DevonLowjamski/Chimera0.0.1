using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Economy;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Data.AI
{
    /// <summary>
    /// PC-012-1: AI Analysis Data Structures for real data analysis
    /// Defines specific data structures needed for PC-012-1 AI analysis that don't conflict with existing types
    /// </summary>

    [System.Serializable]
    public class AIAnalysisReport
    {
        public string AnalysisId;
        public DateTime Timestamp;
        public AIAnalysisType AnalysisType;
        public CultivationAnalysisResult CultivationAnalysis;
        public MarketAnalysisResult MarketAnalysis;
        public EnvironmentalAnalysisResult EnvironmentalAnalysis;
        public GeneticsAnalysisResult GeneticsAnalysis;
        public List<AIRecommendation> GeneratedRecommendations;
        public float OverallScore;
        public string Summary;
        
        // Additional properties for RefactoredAIAdvisorManager compatibility
        public float SystemEfficiencyScore;
        public int ActiveRecommendationCount;
        public int CriticalInsightCount;
        public int OptimizationOpportunityCount;
        public float OverallSystemHealth;
    }

    public enum AIAnalysisType
    {
        Quick,
        Comprehensive,
        Strategic,
        Emergency
    }

    [System.Serializable]
    public class CultivationAnalysisResult
    {
        public int TotalPlants;
        public int ActivePlants;
        public float AverageHealth;
        public Dictionary<PlantGrowthStage, int> GrowthStageDistribution;
        public float PredictedYield;
        public float OptimalYield;
        public List<string> HealthIssues;
        public List<string> GrowthRecommendations;
        public float EfficiencyScore;
    }

    [System.Serializable]
    public class MarketAnalysisResult
    {
        public float CurrentPriceIndex;
        public float PriceVolatility;
        public float QualityPremium;
        public Dictionary<string, DemandTrend> DemandTrends;
        public List<string> MarketOpportunities;
        public List<string> PriceAlerts;
        public float MarketScore;
    }

    [System.Serializable]
    public class DemandTrend
    {
        public string ProductCategory;
        public float CurrentDemand;
        public float GrowthRate;
        public float Volatility;
        public bool IsIncreasing;
    }

    [System.Serializable]
    public class EnvironmentalAnalysisResult
    {
        public Dictionary<string, ZoneAnalysis> ZoneAnalysis;
        public float EnergyEfficiency;
        public float OverallStability;
        public List<string> EnvironmentalAlerts;
        public List<string> OptimizationSuggestions;
        public float EnvironmentalScore;
        
        // Additional properties for comprehensive environmental analysis
        public float TemperatureScore;
        public float HumidityScore;
        public float CO2Score;
        public float LightScore;
        public float OverallScore;
        public List<string> Recommendations;
        public Dictionary<string, (float min, float max)> OptimalRanges;
    }

    [System.Serializable]
    public class ZoneAnalysis
    {
        public string ZoneId;
        public float EfficiencyScore;
        public float TemperatureStability;
        public float HumidityStability;
        public float LightOptimization;
        public List<string> Issues;
    }

    [System.Serializable]
    public class GeneticsAnalysisResult
    {
        public float GeneticDiversity;
        public List<BreedingOpportunity> BreedingOpportunities;
        public List<string> GeneticRecommendations;
        public Dictionary<string, float> TraitPerformance;
        public float GeneticsScore;
        
        // Additional properties for comprehensive genetics analysis
        public int TotalStrains;
        public float BreedingPotential;
        public List<string> OptimalCrosses;
        public Dictionary<string, float> TraitDistribution;
        public List<string> HybridizationOpportunities;
    }

    [System.Serializable]
    public class BreedingOpportunity
    {
        public string OpportunityId;
        public string Parent1;
        public string Parent2;
        public string TargetTrait;
        public float PredictedImprovement;
        public float Confidence;
        public string Description;
    }

    [System.Serializable]
    public class BatchTrackingInfo
    {
        public string SourcePlantId;
        public string StrainName;
        public DateTime HarvestDate;
        public float InitialQuality;
        public string ProcessingMethod;
    }

    [System.Serializable]
    public class StorageEnvironment
    {
        public float Temperature;
        public float Humidity;
        public bool AirCirculation;
        public float LightExposure;
    }
}