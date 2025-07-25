using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectChimera.Data.UI
{
    /// <summary>
    /// Data structures for the Facility Dashboard UI system.
    /// Supports comprehensive monitoring and gamification of facility management.
    /// </summary>
    
    [System.Serializable]
    public class DashboardData
    {
        public DateTime Timestamp;
        public string FacilityName;
        public SystemStatus OverallStatus;
        public EnvironmentalDashboardData Environmental;
        public EconomicDashboardData Economic;
        public AutomationDashboardData Automation;
        public AIAdvisorDashboardData AIAdvisor;
        public PerformanceDashboardData Performance;
        public List<Achievement> RecentAchievements = new List<Achievement>();
        public PlayerProgressData PlayerProgress;
    }
    
    [System.Serializable]
    public class EnvironmentalDashboardData
    {
        public float AverageTemperature;
        public float AverageHumidity;
        public float AverageCO2;
        public float AverageVPD;
        public float DailyLightIntegral;
        public string LightingStatus;
        public float HVACEfficiency;
        public float LightingEfficiency;
        public float EnergyConsumption;
        public int ActiveSensors;
        public int ActiveAlerts;
        public List<EnvironmentalZone> Zones = new List<EnvironmentalZone>();
        public QualityScore EnvironmentalQuality;
    }
    
    [System.Serializable]
    public class EconomicDashboardData
    {
        public float NetWorth;
        public float DailyRevenue;
        public float WeeklyRevenue;
        public float MonthlyRevenue;
        public float CashFlow;
        public float ProfitMargin;
        public float FinancialHealthScore;
        public TrendDirection TrendDirection;
        public List<MarketOpportunity> MarketOpportunities = new List<MarketOpportunity>();
        public List<InvestmentSummary> ActiveInvestments = new List<InvestmentSummary>();
        public float RiskScore;
        public float ROIProjection;
    }
    
    [System.Serializable]
    public class AutomationDashboardData
    {
        public int ActiveSensors;
        public int ConnectedDevices;
        public int ActiveRules;
        public int ActiveAlerts;
        public float SystemUptime;
        public float EnergyOptimization;
        public int AutomationLevel; // 1-5 scale for gamification
        public List<AutomationMilestone> UnlockedMilestones = new List<AutomationMilestone>();
        public float EfficiencyScore;
        public Dictionary<string, float> ZonePerformance = new Dictionary<string, float>();
    }
    
    [System.Serializable]
    public class AIAdvisorDashboardData
    {
        public int ActiveRecommendations;
        public int CriticalInsights;
        public int OptimizationOpportunities;
        public float SystemEfficiencyScore;
        public List<string> TopRecommendations = new List<string>();
        public string AIStatus;
        public float LearningProgress; // 0-1 scale showing AI improvement
        public int InsightStreak; // Consecutive days with implemented recommendations
        public List<OptimizationResult> RecentOptimizations = new List<OptimizationResult>();
    }
    
    [System.Serializable]
    public class PerformanceDashboardData
    {
        public float FrameRate;
        public float MemoryUsage;
        public int ActiveSystems;
        public float LastUpdateTime;
        public float NetworkLatency;
        public float DatabaseResponseTime;
        public QualityScore PerformanceGrade;
    }
    
    [System.Serializable]
    public class PlayerProgressData
    {
        public int PlayerLevel;
        public float ExperiencePoints;
        public float ExperienceToNextLevel;
        public List<string> UnlockedSkills = new List<string>();
        public List<string> AvailableResearch = new List<string>();
        public Dictionary<string, float> SkillProgress = new Dictionary<string, float>();
        public int PlaytimeHours;
        public DateTime FirstPlayDate;
        public int FacilitiesOwned;
        public float MasteryRating; // Overall player skill 0-1
    }
    
    [System.Serializable]
    public class EnvironmentalZone
    {
        public string ZoneId;
        public string ZoneName;
        public string ZoneType; // Vegetative, Flowering, Drying, etc.
        public float Temperature;
        public float Humidity;
        public float CO2Level;
        public float LightIntensity;
        public int PlantCount;
        public ZoneStatus Status;
        public float HealthScore; // 0-1 scale
        public List<string> ActiveIssues = new List<string>();
        public float ProductivityScore;
    }
    
    [System.Serializable]
    public class MarketOpportunity
    {
        public string ProductName;
        public float PotentialProfit;
        public float Demand;
        public float Supply;
        public DateTime ExpiryDate;
        public DifficultyLevel Difficulty;
        public List<string> RequiredSkills = new List<string>();
    }
    
    [System.Serializable]
    public class InvestmentSummary
    {
        public string InvestmentName;
        public float AmountInvested;
        public float CurrentValue;
        public float ROI;
        public DateTime InvestmentDate;
        public DateTime MaturityDate;
        public RiskLevel Risk;
        public TrendDirection Performance;
    }
    
    [System.Serializable]
    public class AutomationMilestone
    {
        public string MilestoneName;
        public string Description;
        public DateTime UnlockedDate;
        public int RewardPoints;
        public List<string> UnlockedFeatures = new List<string>();
        public MilestoneCategory Category;
    }
    
    [System.Serializable]
    public class OptimizationResult
    {
        public string OptimizationName;
        public float EfficiencyGain;
        public float CostSavings;
        public DateTime ImplementedDate;
        public string Description;
        public OptimizationImpact Impact;
    }
    
    [System.Serializable]
    public class Achievement
    {
        public string AchievementId;
        public string Title;
        public string Description;
        public DateTime UnlockedDate;
        public AchievementRarity Rarity;
        public int Points;
        public string IconPath;
        public bool IsSecret;
        public List<string> Prerequisites = new List<string>();
    }
    
    [System.Serializable]
    public class DashboardAlert
    {
        public string AlertId;
        public string Title;
        public string Description;
        public AlertSeverity Severity;
        public DateTime Timestamp;
        public string Source; // System that generated the alert
        public bool RequiresAction;
        public List<string> SuggestedActions = new List<string>();
        public float UrgencyScore; // 0-1 scale
    }
    
    [System.Serializable]
    public class QualityScore
    {
        public float Score; // 0-1 scale
        public QualityGrade Grade; // F, D, C, B, A, S
        public string Description;
        public List<string> ImprovementSuggestions = new List<string>();
        public float TrendChange; // Positive/negative change
    }
    
    // Enums for UI data classification
    public enum SystemStatus
    {
        Optimal,
        Warning,
        Critical,
        Offline,
        Maintenance
    }
    
    public enum TrendDirection
    {
        Up,
        Down,
        Stable,
        Volatile
    }
    
    public enum ZoneStatus
    {
        Healthy,
        Warning,
        Critical,
        Inactive,
        Maintenance,
        Optimizing
    }
    
    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical,
        Emergency
    }
    
    public enum DifficultyLevel
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        Master
    }
    
    public enum RiskLevel
    {
        VeryLow,
        Low,
        Moderate,
        High,
        VeryHigh
    }
    
    public enum MilestoneCategory
    {
        Environmental,
        Economic,
        Automation,
        Research,
        Production,
        Efficiency,
        Achievement
    }
    
    public enum OptimizationImpact
    {
        Minor,
        Moderate,
        Significant,
        Major,
        Revolutionary
    }
    
    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythical
    }
    
    public enum QualityGrade
    {
        F, D, C, B, A, S
    }
    
    /// <summary>
    /// Settings configuration for the dashboard system.
    /// </summary>
    [System.Serializable]
    public class DashboardSettings
    {
        [Header("Facility Information")]
        public string FacilityName = "Project Chimera Facility";
        public string OwnerName = "Player";
        public string FacilityLicense = "CUL-001";
        
        [Header("Display Preferences")]
        public bool ShowAdvancedMetrics = true;
        public bool EnableRealTimeUpdates = true;
        public bool ShowPredictiveAnalytics = true;
        public bool EnableSoundNotifications = true;
        public bool ShowTooltips = true;
        
        [Header("Gamification Settings")]
        public bool EnableAchievements = true;
        public bool EnableLevelProgression = true;
        public bool ShowRankings = true;
        public bool EnableChallenges = true;
        public bool ShowMilestoneProgress = true;
        
        [Header("Alert Configuration")]
        public AlertSeverity MinimumAlertLevel = AlertSeverity.Warning;
        public float AlertDisplayDuration = 10f;
        public int MaxSimultaneousAlerts = 5;
        public bool AutoDismissInfoAlerts = true;
        
        [Header("Performance Settings")]
        public float RefreshRate = 5f;
        public bool EnableAnimations = true;
        public bool ReduceMotionForPerformance = false;
        public int MaxHistoryPoints = 1000;
        
        [Header("Data Visualization")]
        public bool ShowTrendLines = true;
        public bool EnableInteractiveCharts = true;
        public int ChartDataPoints = 50;
        public bool ShowComparativeData = true;
        
        [Header("Quick Actions")]
        public List<string> EnabledQuickActions = new List<string>
        {
            "emergency-stop",
            "optimize-all",
            "refresh-data",
            "export-report",
            "backup-settings"
        };
        
        public Dictionary<string, bool> PanelVisibility = new Dictionary<string, bool>
        {
            { "environmental", true },
            { "economic", true },
            { "automation", true },
            { "ai-advisor", true },
            { "alerts", true },
            { "achievements", true },
            { "performance", false }
        };
    }
}