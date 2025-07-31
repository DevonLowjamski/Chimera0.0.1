using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectChimera.Data.AI;
using AIEnvironmentalAnalysisResult = ProjectChimera.Data.AI.EnvironmentalAnalysisResult;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// Extended interface for comprehensive AI analysis functionality
    /// Defines the complete contract for the AIAnalysisService
    /// </summary>
    public interface IAIAnalysisServiceExtended : IAIAnalysisService, ICultivationAnalysisService, IEnvironmentalAnalysisService, IGeneticsAnalysisService
    {
        // Analysis results properties
        CultivationAnalysisResult LastCultivationAnalysis { get; }
        AIEnvironmentalAnalysisResult LastEnvironmentalAnalysis { get; }
        GeneticsAnalysisResult LastGeneticsAnalysis { get; }
        List<DataInsight> CriticalInsights { get; }
        
        // Comprehensive analysis methods
        Task<CultivationAnalysisResult> AnalyzeCultivationDataAsync();
        Task<AIEnvironmentalAnalysisResult> AnalyzeEnvironmentalDataAsync();
        Task<GeneticsAnalysisResult> AnalyzeGeneticsDataAsync();
        
        // Configuration
        bool EnableRealTimeAnalysis { get; set; }
        bool EnablePredictiveAnalysis { get; set; }
        float ConfidenceThreshold { get; set; }
        
        // Events
        event Action<CultivationAnalysisResult> OnCultivationAnalysisComplete;
        event Action<AIEnvironmentalAnalysisResult> OnEnvironmentalAnalysisComplete;
        event Action<GeneticsAnalysisResult> OnGeneticsAnalysisComplete;
        event Action<DataInsight> OnCriticalInsight;
        event Action<OptimizationOpportunity> OnOptimizationIdentified;
    }
    
    /// <summary>
    /// Settings for AI analysis configuration
    /// </summary>
    [System.Serializable]
    public class AnalysisSettings
    {
        [UnityEngine.Header("Analysis Intervals")]
        public float QuickAnalysisInterval = 10f;
        public float DeepAnalysisInterval = 60f;
        public float StrategicAnalysisInterval = 300f;
        
        [UnityEngine.Header("Thresholds")]
        public float ConfidenceThreshold = 0.6f;
        public float EmergencyAnalysisThreshold = 0.3f;
        public float HealthAlertThreshold = 0.6f;
        
        [UnityEngine.Header("Data Management")]
        public int MaxHistoricalSnapshots = 1000;
        public int MinDataPointsForDeepAnalysis = 10;
        public int MinDataPointsForStrategicAnalysis = 100;
        
        [UnityEngine.Header("Features")]
        public bool EnableRealTimeAnalysis = true;
        public bool EnablePredictiveAnalysis = true;
        public bool EnableEmergencyDetection = true;
        public bool EnableTrendAnalysis = true;
    }
}