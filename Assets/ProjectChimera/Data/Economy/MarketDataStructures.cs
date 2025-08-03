using System;
using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Data.Economy
{
    /// <summary>
    /// Market-focused data structures extracted from EconomicDataStructures.cs
    /// Contains market preferences, analysis, trends, opportunities, and intelligence systems
    /// Part of Phase 1 Foundation Data Structures refactoring
    /// </summary>

    #region Market Preferences and Behavior

    [System.Serializable]
    public class MarketPreferences
    {
        public List<ProductCategory> PreferredCategories = new List<ProductCategory>();
        public List<MarketTier> PreferredTiers = new List<MarketTier>();
        [Range(0f, 100f)] public float PriceSensitivity = 50f;
        [Range(0f, 100f)] public float QualitySensitivity = 70f;
        [Range(0f, 100f)] public float BrandLoyalty = 40f;
        public bool PrefersExclusiveDeals = false;
        public bool PrefersVolumeDiscounts = true;
    }

    [System.Serializable]
    public class ProductPreference
    {
        public ProductType ProductType;
        [Range(0f, 1f)] public float PreferenceStrength = 0.7f;
        public Vector2 PreferredPriceRange = new Vector2(10f, 30f);
        public Vector2 PreferredQualityRange = new Vector2(0.7f, 1f);
        public List<string> PreferredAttributes = new List<string>();
        public string PreferenceReason;
    }

    [System.Serializable]
    public class QualityExpectations
    {
        [Range(0f, 1f)] public float MinimumQualityThreshold = 0.7f;
        [Range(0f, 1f)] public float PreferredQualityLevel = 0.85f;
        public bool RequiresConsistentQuality = true;
        public bool AcceptsVariableQuality = false;
        [Range(0f, 1f)] public float QualityToleranceRange = 0.1f;
    }

    [System.Serializable]
    public class QualityAttribute
    {
        public string AttributeName;
        [Range(0f, 1f)] public float ImportanceWeight = 0.5f;
        [Range(0f, 1f)] public float MinimumAcceptableLevel = 0.6f;
        public bool IsCritical = false;
        public string AttributeDescription;
    }

    #endregion

    #region Market Analysis and Intelligence

    [System.Serializable]
    public class GlobalMarketAnalysis
    {
        public List<RegionalOpportunity> RegionalOpportunities = new List<RegionalOpportunity>();
        public List<GlobalTrend> GlobalTrends = new List<GlobalTrend>();
        public float OverallMarketHealth = 0.7f;
        public DateTime LastAnalysisUpdate;
        public AnalysisConfidence Confidence = AnalysisConfidence.Medium;
    }

    [System.Serializable]
    public class MarketOpportunity
    {
        public string OpportunityName;
        [Range(0f, 100f)] public float MarketSize = 50f;
        [Range(0f, 100f)] public float GrowthPotential = 40f;
        [Range(0f, 100f)] public float EntryBarriers = 30f;
        public Region TargetRegion = Region.NorthAmerica;
        public CompetitionIntensity Competition = CompetitionIntensity.Medium;
    }

    [System.Serializable]
    public class MarketTrend
    {
        public string TrendName;
        public TrendDirection Direction = TrendDirection.Neutral;
        [Range(-100f, 100f)] public float ChangePercentage = 0f;
        [Range(0f, 100f)] public float MarketShare = 25f;
        public bool IsEmergingTrend = false;
        public DateTime TrendStartDate;
        public float DurationInMonths = 6f;
    }

    [System.Serializable]
    public class MarketAnalysisScope
    {
        public List<Region> TargetRegions = new List<Region>();
        public List<ProductType> ProductTypes = new List<ProductType>();
        public AnalysisDepth AnalysisDepth = AnalysisDepth.Standard;
        public DateTime AnalysisStartDate;
        public DateTime AnalysisEndDate;
        public bool IncludeCompetitorAnalysis = true;
        public bool IncludeTrendAnalysis = true;
    }

    [System.Serializable]
    public class MarketData
    {
        public float CurrentPrice = 100f;
        public float OpeningPrice = 98f;
        public float HighPrice = 105f;
        public float LowPrice = 95f;
        public float Volume = 1000f;
        public DateTime LastUpdate;
        public MarketDataQuality DataQuality = MarketDataQuality.High;
    }

    [System.Serializable]
    public class MarketForecast
    {
        public float PredictedPrice = 110f;
        public Vector2 PriceRange = new Vector2(100f, 120f);
        [Range(0f, 1f)] public float ConfidenceLevel = 0.8f;
        public DateTime ForecastDate;
        public int ForecastHorizonDays = 30;
        public List<ForecastScenario> Scenarios = new List<ForecastScenario>();
    }

    [System.Serializable]
    public class ForecastParameters
    {
        public int TimeHorizonDays = 30;
        public bool IncludeSeasonality = true;
        public bool IncludeMarketEvents = true;
        public bool IncludeCompetitorImpact = false;
        public float VolatilityFactor = 1f;
        public List<string> InfluencingFactors = new List<string>();
    }

    [System.Serializable]
    public class MarketAnalysisTools
    {
        public List<AnalysisTool> AvailableTools = new List<AnalysisTool>();
        public AnalysisTool DefaultTool;
        public bool AutoSelectOptimalTool = true;
        public Dictionary<string, float> ToolEffectiveness = new Dictionary<string, float>();
    }

    [System.Serializable]
    public class MarketAnalysisReport
    {
        public string ReportTitle;
        public DateTime GeneratedDate;
        public List<string> KeyFindings = new List<string>();
        public List<string> Recommendations = new List<string>();
        public AnalysisConfidence OverallConfidence = AnalysisConfidence.Medium;
        public string ExecutiveSummary;
    }

    [System.Serializable]
    public class MarketKnowledge
    {
        public Region Region = Region.Global;
        [Range(0f, 1f)] public float ProductKnowledge = 0.5f;
        [Range(0f, 1f)] public float RegionalKnowledge = 0.5f;
        [Range(0f, 1f)] public float CompetitorKnowledge = 0.3f;
        public DateTime LastUpdated;
    }

    #endregion

    #region Market Trends and Conditions

    [System.Serializable]
    public class TrendAnalysisSystem
    {
        public List<TrendPattern> DetectedPatterns = new List<TrendPattern>();
        public float TrendDetectionSensitivity = 0.7f;
        public int MinimumDataPoints = 5;
        public bool EnableAutomaticDetection = true;
        public bool IsActive = true; // Added for backwards compatibility
        public DateTime LastAnalysis;
    }

    [System.Serializable]
    public class TrendPattern
    {
        public string PatternName;
        public TrendDirection Direction = TrendDirection.Neutral;
        [Range(0f, 1f)] public float PatternStrength = 0.5f;
        [Range(0f, 1f)] public float ConfidenceLevel = 0.7f;
        public int DurationDays = 30;
        public List<float> DataPoints = new List<float>();
    }

    [System.Serializable]
    public class MarketAnalyticsData
    {
        public float MarketVolatility = 0.2f;
        public float TrendStrength = 0.6f;
        public float MarketMomentum = 0.5f;
        public float MarketHealthIndicator = 0.8f;
        public DateTime LastCalculated;
        public List<string> AnalyticsMetrics = new List<string>();
    }

    [System.Serializable]
    public class MarketPrediction
    {
        public float PredictedValue = 100f;
        public Vector2 PredictionRange = new Vector2(90f, 110f);
        [Range(0f, 1f)] public float Accuracy = 0.75f;
        public DateTime PredictionDate;
        public int TimeFrameDays = 7;
        public string PredictionMethod;
        public List<string> InfluencingFactors = new List<string>();
    }

    [System.Serializable]
    public class PredictiveModel
    {
        public string ModelId;
        public string ModelName;
        public ModelType Type = ModelType.TechnicalAnalysis;
        [Range(0f, 1f)] public float ModelAccuracy = 0.8f;
        [Range(0f, 1f)] public float Accuracy = 0.8f; // Alias for ModelAccuracy for backwards compatibility
        public List<string> InputParameters = new List<string>();
        public DateTime LastTraining;
        public bool IsActive = true;
    }

    #endregion

    #region Market Competition and Positioning

    [System.Serializable]
    public class CompetitorAnalysis
    {
        public string CompetitorName;
        public MarketPosition Position = MarketPosition.Emerging;
        [Range(0f, 100f)] public float MarketShare = 15f;
        public List<string> Strengths = new List<string>();
        public List<string> Weaknesses = new List<string>();
        public List<string> Opportunities = new List<string>();
        public List<string> Threats = new List<string>();
        public ThreatLevel ThreatLevel = ThreatLevel.Medium;
    }

    [System.Serializable]
    public class CompetitorProfile
    {
        public string CompanyName;
        public float EstimatedRevenue = 1000000f;
        public int EmployeeCount = 50;
        [Range(0f, 100f)] public float MarketPresence = 30f;
        public List<ProductType> ProductPortfolio = new List<ProductType>();
        public CompetitiveThreat ThreatAssessment = CompetitiveThreat.Low;
        public DateTime LastUpdate;
    }

    [System.Serializable]
    public class MarketSegmentKnowledge
    {
        public MarketSegment Segment = MarketSegment.Premium_Buyers;
        [Range(0f, 1f)] public float SegmentUnderstanding = 0.6f;
        public List<string> KeyInsights = new List<string>();
        public List<CompetitorProfile> KnownCompetitors = new List<CompetitorProfile>();
        public float SegmentSize = 1000000f;
        public float GrowthRate = 0.1f;
    }

    #endregion

    #region Market Events and Impacts

    [System.Serializable]
    public class MarketOpening
    {
        public Region NewMarket = Region.Asia;
        public DateTime OpeningDate;
        public List<ProductType> AllowedProducts = new List<ProductType>();
        public List<string> EntryRequirements = new List<string>();
        public float EstimatedMarketSize = 500000f;
        public float EntryBarrierLevel = 0.5f;
    }

    [System.Serializable]
    public class MarketCrash
    {
        public string CrashName;
        public DateTime CrashDate;
        [Range(0f, 1f)] public float SeverityLevel = 0.7f;
        public List<ProductType> AffectedProducts = new List<ProductType>();
        public List<Region> AffectedRegions = new List<Region>();
        public float RecoveryTimeMonths = 6f;
        public bool IsRecovering = false;
    }

    [System.Serializable]
    public class MarketDataFeed
    {
        public string FeedName;
        public DataProvider Provider = DataProvider.Internal;
        public bool IsRealTime = true;
        public float UpdateFrequencySeconds = 1f;
        public List<string> DataTypes = new List<string>();
        public DataQuality Quality = DataQuality.High;
        public bool IsActive = true;
        public DateTime LastUpdate;
    }

    [System.Serializable]
    public class MarketAttackCampaign
    {
        public string CampaignName;
        public string TargetCompetitor;
        public AttackStrategy Strategy = AttackStrategy.PriceWar;
        public List<ProductType> TargetProducts = new List<ProductType>();
        [Range(0f, 100f)] public float ExpectedEffectiveness = 60f;
        public float BudgetAllocated = 100000f;
        public DateTime LaunchDate;
        public int DurationDays = 30;
        public bool IsActive = false;
    }

    [System.Serializable]
    public class MarketCaptureResult
    {
        public string CampaignId;
        [Range(0f, 100f)] public float MarketShareGained = 5f;
        [Range(0f, 100f)] public float RevenueIncrease = 10f;
        public float CostInvested = 50000f;
        public float ROI = 0.2f;
        public List<string> KeySuccessFactors = new List<string>();
        public List<string> LessonsLearned = new List<string>();
        public DateTime CampaignEndDate;
    }

    [System.Serializable]
    public class MarketProtectionSystem
    {
        public List<DefenseStrategy> ActiveDefenses = new List<DefenseStrategy>();
        [Range(0f, 1f)] public float ProtectionLevel = 0.7f;
        public float MonitoringBudget = 25000f;
        public bool AutoResponse = true;
        public List<string> ThreatIndicators = new List<string>();
        public DateTime LastAssessment;
    }

    [System.Serializable]
    public class MarketInsight
    {
        public string InsightTitle;
        public string InsightDescription;
        public InsightType Type = InsightType.TrendAnalysis;
        [Range(0f, 1f)] public float ConfidenceRating = 0.8f;
        public DateTime DiscoveredDate;
        public List<string> SupportingData = new List<string>();
        public bool IsActionable = true;
    }

    [System.Serializable]
    public class MarketResearch
    {
        public string ResearchTitle;
        public ResearchMethodology Methodology = ResearchMethodology.Survey;
        public int SampleSize = 1000;
        public List<string> KeyFindings = new List<string>();
        public float MarketSizeEstimate = 10000000f;
        [Range(0f, 1f)] public float DataReliability = 0.85f;
        public DateTime CompletionDate;
        public float ResearchCost = 15000f;
    }

    #endregion

    #region Supporting Enums and Data Types

    public enum MarketPosition
    {
        Startup,
        Emerging,
        Developing,
        Established,
        Market_Leader,
        Dominant,
        Declining,
        Niche_Player
    }

    public enum TrendDirection
    {
        Bullish,
        Bearish,
        Sideways,
        Volatile,
        Neutral,
        Emerging,
        Declining
    }

    public enum Region
    {
        Global,
        NorthAmerica,
        Europe,
        Asia,
        LatinAmerica,
        MiddleEast,
        Africa,
        Oceania
    }

    public enum CompetitionIntensity
    {
        Low,
        Medium,
        High,
        Extreme
    }

    public enum AnalysisDepth
    {
        Basic,
        Standard,
        Detailed,
        Comprehensive,
        Expert
    }

    public enum AnalysisConfidence
    {
        Low,
        Medium,
        High,
        VeryHigh
    }

    public enum MarketDataQuality
    {
        Low,
        Medium,
        High,
        Excellent
    }

    public enum ModelType
    {
        TechnicalAnalysis,
        FundamentalAnalysis,
        MachineLearning,
        Statistical,
        Hybrid
    }

    public enum ThreatLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum CompetitiveThreat
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum DataProvider
    {
        Internal,
        External,
        ThirdParty,
        Government,
        Industry
    }

    public enum DataQuality
    {
        Low,
        Medium,
        High,
        Premium
    }

    public enum AttackStrategy
    {
        PriceWar,
        QualityDifferentiation,
        MarketingBlitz,
        ProductLaunch,
        SupplyChainDisruption,
        TalentPoaching
    }

    public enum DefenseStrategy
    {
        PriceMatching,
        QualityImprovement,
        CustomerRetention,
        MarketExpansion,
        Innovation,
        Partnership
    }

    public enum InsightType
    {
        TrendAnalysis,
        CompetitorIntelligence,
        CustomerBehavior,
        MarketOpportunity,
        RiskAssessment
    }

    public enum ResearchMethodology
    {
        Survey,
        FocusGroup,
        Interview,
        Observation,
        Experiment,
        SecondaryData
    }

    // MarketSegment enum exists in MarketProductSO.cs with cannabis-specific segments

    #endregion

    #region Helper Data Structures

    [System.Serializable]
    public class RegionalOpportunity
    {
        public Region Region = Region.Global;
        [Range(0f, 100f)] public float OpportunityScore = 50f;
        public List<string> KeyDrivers = new List<string>();
        public float EstimatedMarketSize = 1000000f;
        public DateTime IdentifiedDate;
    }

    [System.Serializable]
    public class GlobalTrend
    {
        public string TrendName;
        public TrendDirection Direction = TrendDirection.Neutral;
        [Range(0f, 100f)] public float GlobalImpact = 50f;
        public List<Region> AffectedRegions = new List<Region>();
        public DateTime EmergenceDate;
    }

    [System.Serializable]
    public class ForecastScenario
    {
        public string ScenarioName;
        public float ProbabilityPercent = 33f;
        public float PredictedValue = 100f;
        public List<string> Assumptions = new List<string>();
        public string Description;
    }

    [System.Serializable]
    public class AnalysisTool
    {
        public string ToolName;
        public AnalysisType Type = AnalysisType.Market;
        [Range(0f, 1f)] public float Effectiveness = 0.8f;
        public float CostPerUse = 100f;
        public bool IsAvailable = true;
    }

    public enum AnalysisType
    {
        Market,
        Competitor,
        Trend,
        Financial,
        Risk,
        Opportunity
    }

    #endregion
}