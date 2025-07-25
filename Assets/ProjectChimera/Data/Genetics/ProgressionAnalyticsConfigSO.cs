using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Genetics
{
    /// <summary>
    /// Progression Analytics Configuration - Configuration for progression tracking and analytics
    /// Defines metrics collection, analysis parameters, and reporting systems
    /// Part of Enhanced Scientific Gaming System v2.0
    /// </summary>
    [CreateAssetMenu(fileName = "New Progression Analytics Config", menuName = "Project Chimera/Gaming/Progression Analytics Config")]
    public class ProgressionAnalyticsConfigSO : ChimeraDataSO
    {
        [Header("Analytics Settings")]
        [Range(1, 365)] public int DataRetentionDays = 90;
        [Range(1, 60)] public int AnalyticsUpdateInterval = 30;
        public bool EnableRealTimeTracking = true;
        public bool EnablePredictiveAnalysis = true;
        
        [Header("Metrics Collection")]
        public List<ProgressionMetricConfig> TrackedMetrics = new List<ProgressionMetricConfig>();
        public List<AnalyticsEventConfig> TrackedEvents = new List<AnalyticsEventConfig>();
        
        [Header("Reporting Configuration")]
        public List<ProgressionReportConfig> ReportConfigurations = new List<ProgressionReportConfig>();
        [Range(0.1f, 5.0f)] public float EfficiencyThreshold = 1.0f;
        
        #region Runtime Methods
        
        public ProgressionMetricConfig GetMetricConfig(ProgressionMetricType metricType)
        {
            return TrackedMetrics.Find(m => m.MetricType == metricType);
        }
        
        public List<AnalyticsEventConfig> GetEventConfigs(AnalyticsEventCategory category)
        {
            return TrackedEvents.FindAll(e => e.Category == category);
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class ProgressionMetricConfig
    {
        public string MetricName;
        public ProgressionMetricType MetricType;
        public float SamplingRate = 1.0f;
        public bool EnableTrending = true;
        public string Description;
    }
    
    [System.Serializable]
    public class AnalyticsEventConfig
    {
        public string EventName;
        public AnalyticsEventCategory Category;
        public bool EnableTracking = true;
        public float Weight = 1.0f;
        public string Description;
    }
    
    [System.Serializable]
    public class ProgressionReportConfig
    {
        public string ReportName;
        public ProgressionReportType ReportType;
        [Range(1, 365)] public int ReportingInterval = 7;
        public List<ProgressionMetricType> IncludedMetrics = new List<ProgressionMetricType>();
        public string Description;
    }
    
    public enum ProgressionMetricType
    {
        ExperienceGain,
        SkillUnlocks,
        AchievementEarned,
        SessionDuration,
        EngagementScore,
        ProgressionEfficiency,
        CrossSystemSynergy,
        CommunityParticipation
    }
    
    public enum AnalyticsEventCategory
    {
        Progression,
        Achievement,
        Social,
        Competition,
        Innovation,
        System
    }
    
    public enum ProgressionReportType
    {
        Daily,
        Weekly,
        Monthly,
        Seasonal,
        Custom
    }
}