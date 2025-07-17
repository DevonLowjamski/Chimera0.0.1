using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Narrative
{
    /// <summary>
    /// Advanced narrative analytics configuration ScriptableObject for Project Chimera's story campaign system.
    /// Features comprehensive player behavior tracking, emotional engagement metrics, and educational effectiveness
    /// measurement with scientific validation for cannabis cultivation learning outcomes.
    /// </summary>
    [CreateAssetMenu(fileName = "New Narrative Analytics Config", menuName = "Project Chimera/Narrative/Narrative Analytics Config", order = 106)]
    public class NarrativeAnalyticsConfigSO : ChimeraConfigSO
    {
        [Header("Core Analytics Settings")]
        [SerializeField] private bool _enableAnalytics = true;
        [SerializeField] private bool _enableRealTimeTracking = true;
        [SerializeField] private float _analyticsUpdateInterval = 1.0f;
        [SerializeField] private bool _enableDataPersistence = true;
        [SerializeField] private int _maxAnalyticsHistoryDays = 90;
        [SerializeField] private bool _enableAnonymousDataCollection = true;
        
        [Header("Player Behavior Tracking")]
        [SerializeField] private bool _trackDialogueChoices = true;
        [SerializeField] private bool _trackChoiceReactionTime = true;
        [SerializeField] private bool _trackDialogueSkipping = true;
        [SerializeField] private bool _trackCharacterInteractions = true;
        [SerializeField] private bool _trackStoryProgression = true;
        [SerializeField] private bool _trackPlayerPreferences = true;
        
        [Header("Emotional Engagement Tracking")]
        [SerializeField] private bool _trackEmotionalResponses = true;
        [SerializeField] private bool _trackCharacterAttachment = true;
        [SerializeField] private bool _trackStoryInvestment = true;
        [SerializeField] private bool _trackMoodChanges = true;
        [SerializeField] private float _emotionalSamplingRate = 5.0f;
        [SerializeField] private int _maxEmotionalDataPoints = 1000;
        
        [Header("Educational Effectiveness Tracking")]
        [SerializeField] private bool _trackLearningOutcomes = true;
        [SerializeField] private bool _trackKnowledgeRetention = true;
        [SerializeField] private bool _trackSkillProgression = true;
        [SerializeField] private bool _trackEducationalEngagement = true;
        [SerializeField] private bool _validateScientificAccuracy = true;
        [SerializeField] private float _knowledgeAssessmentInterval = 300f;
        
        [Header("Performance Analytics")]
        [SerializeField] private bool _trackSystemPerformance = true;
        [SerializeField] private bool _trackLoadTimes = true;
        [SerializeField] private bool _trackMemoryUsage = true;
        [SerializeField] private bool _trackFrameRate = true;
        [SerializeField] private float _performanceMonitoringInterval = 10.0f;
        [SerializeField] private bool _enablePerformanceAlerts = true;
        
        [Header("Story Flow Analytics")]
        [SerializeField] private bool _trackStoryPathTaken = true;
        [SerializeField] private bool _trackBranchingDecisions = true;
        [SerializeField] private bool _trackArcCompletion = true;
        [SerializeField] private bool _trackStoryDropoff = true;
        [SerializeField] private bool _trackReplayBehavior = true;
        [SerializeField] private bool _generateStoryHeatmaps = true;
        
        [Header("Character Relationship Analytics")]
        [SerializeField] private bool _trackRelationshipChanges = true;
        [SerializeField] private bool _trackTrustLevels = true;
        [SerializeField] private bool _trackInfluencePatterns = true;
        [SerializeField] private bool _trackCharacterPopularity = true;
        [SerializeField] private bool _trackConversationLength = true;
        [SerializeField] private float _relationshipSamplingRate = 30.0f;
        
        [Header("Data Processing and Analysis")]
        [SerializeField] private bool _enableMachineLearning = false;
        [SerializeField] private bool _enablePredictiveAnalytics = false;
        [SerializeField] private bool _enableClusterAnalysis = false;
        [SerializeField] private bool _enableABTesting = false;
        [SerializeField] private int _minDataPointsForAnalysis = 100;
        [SerializeField] private float _dataProcessingInterval = 60.0f;
        
        [Header("Reporting and Visualization")]
        [SerializeField] private bool _enableReportGeneration = true;
        [SerializeField] private bool _enableRealTimeDashboards = false;
        [SerializeField] private bool _enableDataExport = true;
        [SerializeField] private List<ReportType> _enabledReports = new List<ReportType>();
        [SerializeField] private float _reportGenerationInterval = 3600.0f;
        [SerializeField] private string _dataExportFormat = "JSON";
        
        [Header("Privacy and Compliance")]
        [SerializeField] private bool _enableDataEncryption = true;
        [SerializeField] private bool _enableUserConsent = true;
        [SerializeField] private bool _enableDataDeletion = true;
        [SerializeField] private bool _enableGDPRCompliance = true;
        [SerializeField] private int _dataRetentionDays = 365;
        [SerializeField] private bool _enableOptOut = true;
        
        [Header("Alert and Notification Settings")]
        [SerializeField] private bool _enableAnalyticsAlerts = true;
        [SerializeField] private float _engagementDropThreshold = 0.3f;
        [SerializeField] private float _performanceIssueThreshold = 30.0f;
        [SerializeField] private float _learningEffectivenessThreshold = 0.6f;
        [SerializeField] private bool _enableEmailReports = false;
        [SerializeField] private bool _enableSlackIntegration = false;
        
        // Public Properties
        public bool EnableAnalytics => _enableAnalytics;
        public bool EnableRealTimeTracking => _enableRealTimeTracking;
        public float AnalyticsUpdateInterval => _analyticsUpdateInterval;
        public bool EnableDataPersistence => _enableDataPersistence;
        public int MaxAnalyticsHistoryDays => _maxAnalyticsHistoryDays;
        public bool EnableAnonymousDataCollection => _enableAnonymousDataCollection;
        
        public bool TrackDialogueChoices => _trackDialogueChoices;
        public bool TrackChoiceReactionTime => _trackChoiceReactionTime;
        public bool TrackDialogueSkipping => _trackDialogueSkipping;
        public bool TrackCharacterInteractions => _trackCharacterInteractions;
        public bool TrackStoryProgression => _trackStoryProgression;
        public bool TrackPlayerPreferences => _trackPlayerPreferences;
        
        public bool TrackEmotionalResponses => _trackEmotionalResponses;
        public bool TrackCharacterAttachment => _trackCharacterAttachment;
        public bool TrackStoryInvestment => _trackStoryInvestment;
        public bool TrackMoodChanges => _trackMoodChanges;
        public float EmotionalSamplingRate => _emotionalSamplingRate;
        public int MaxEmotionalDataPoints => _maxEmotionalDataPoints;
        
        public bool TrackLearningOutcomes => _trackLearningOutcomes;
        public bool TrackKnowledgeRetention => _trackKnowledgeRetention;
        public bool TrackSkillProgression => _trackSkillProgression;
        public bool TrackEducationalEngagement => _trackEducationalEngagement;
        public bool ValidateScientificAccuracy => _validateScientificAccuracy;
        public float KnowledgeAssessmentInterval => _knowledgeAssessmentInterval;
        
        public bool TrackSystemPerformance => _trackSystemPerformance;
        public bool TrackLoadTimes => _trackLoadTimes;
        public bool TrackMemoryUsage => _trackMemoryUsage;
        public bool TrackFrameRate => _trackFrameRate;
        public float PerformanceMonitoringInterval => _performanceMonitoringInterval;
        public bool EnablePerformanceAlerts => _enablePerformanceAlerts;
        
        public bool TrackStoryPathTaken => _trackStoryPathTaken;
        public bool TrackBranchingDecisions => _trackBranchingDecisions;
        public bool TrackArcCompletion => _trackArcCompletion;
        public bool TrackStoryDropoff => _trackStoryDropoff;
        public bool TrackReplayBehavior => _trackReplayBehavior;
        public bool GenerateStoryHeatmaps => _generateStoryHeatmaps;
        
        public bool TrackRelationshipChanges => _trackRelationshipChanges;
        public bool TrackTrustLevels => _trackTrustLevels;
        public bool TrackInfluencePatterns => _trackInfluencePatterns;
        public bool TrackCharacterPopularity => _trackCharacterPopularity;
        public bool TrackConversationLength => _trackConversationLength;
        public float RelationshipSamplingRate => _relationshipSamplingRate;
        
        public bool EnableMachineLearning => _enableMachineLearning;
        public bool EnablePredictiveAnalytics => _enablePredictiveAnalytics;
        public bool EnableClusterAnalysis => _enableClusterAnalysis;
        public bool EnableABTesting => _enableABTesting;
        public int MinDataPointsForAnalysis => _minDataPointsForAnalysis;
        public float DataProcessingInterval => _dataProcessingInterval;
        
        public bool EnableReportGeneration => _enableReportGeneration;
        public bool EnableRealTimeDashboards => _enableRealTimeDashboards;
        public bool EnableDataExport => _enableDataExport;
        public IReadOnlyList<ReportType> EnabledReports => _enabledReports.AsReadOnly();
        public float ReportGenerationInterval => _reportGenerationInterval;
        public string DataExportFormat => _dataExportFormat;
        
        public bool EnableDataEncryption => _enableDataEncryption;
        public bool EnableUserConsent => _enableUserConsent;
        public bool EnableDataDeletion => _enableDataDeletion;
        public bool EnableGDPRCompliance => _enableGDPRCompliance;
        public int DataRetentionDays => _dataRetentionDays;
        public bool EnableOptOut => _enableOptOut;
        
        public bool EnableAnalyticsAlerts => _enableAnalyticsAlerts;
        public float EngagementDropThreshold => _engagementDropThreshold;
        public float PerformanceIssueThreshold => _performanceIssueThreshold;
        public float LearningEffectivenessThreshold => _learningEffectivenessThreshold;
        public bool EnableEmailReports => _enableEmailReports;
        public bool EnableSlackIntegration => _enableSlackIntegration;
        
        protected override bool ValidateDataSpecific()
        {
            var isValid = true;
            var validationErrors = new List<string>();
            
            // Validate core settings
            if (_analyticsUpdateInterval <= 0f)
            {
                validationErrors.Add("Analytics Update Interval must be greater than 0");
                isValid = false;
            }
            
            if (_maxAnalyticsHistoryDays <= 0)
            {
                validationErrors.Add("Max Analytics History Days must be greater than 0");
                isValid = false;
            }
            
            // Validate emotional tracking settings
            if (_emotionalSamplingRate <= 0f)
            {
                validationErrors.Add("Emotional Sampling Rate must be greater than 0");
                isValid = false;
            }
            
            if (_maxEmotionalDataPoints <= 0)
            {
                validationErrors.Add("Max Emotional Data Points must be greater than 0");
                isValid = false;
            }
            
            // Validate educational tracking settings
            if (_knowledgeAssessmentInterval <= 0f)
            {
                validationErrors.Add("Knowledge Assessment Interval must be greater than 0");
                isValid = false;
            }
            
            // Validate performance settings
            if (_performanceMonitoringInterval <= 0f)
            {
                validationErrors.Add("Performance Monitoring Interval must be greater than 0");
                isValid = false;
            }
            
            // Validate relationship tracking settings
            if (_relationshipSamplingRate <= 0f)
            {
                validationErrors.Add("Relationship Sampling Rate must be greater than 0");
                isValid = false;
            }
            
            // Validate data processing settings
            if (_minDataPointsForAnalysis <= 0)
            {
                validationErrors.Add("Min Data Points For Analysis must be greater than 0");
                isValid = false;
            }
            
            if (_dataProcessingInterval <= 0f)
            {
                validationErrors.Add("Data Processing Interval must be greater than 0");
                isValid = false;
            }
            
            // Validate reporting settings
            if (_reportGenerationInterval <= 0f)
            {
                validationErrors.Add("Report Generation Interval must be greater than 0");
                isValid = false;
            }
            
            if (_enableDataExport && string.IsNullOrEmpty(_dataExportFormat))
            {
                validationErrors.Add("Data Export Format cannot be empty when data export is enabled");
                isValid = false;
            }
            
            // Validate privacy settings
            if (_dataRetentionDays <= 0)
            {
                validationErrors.Add("Data Retention Days must be greater than 0");
                isValid = false;
            }
            
            // Validate alert thresholds
            if (_engagementDropThreshold < 0f || _engagementDropThreshold > 1f)
            {
                validationErrors.Add("Engagement Drop Threshold must be between 0 and 1");
                isValid = false;
            }
            
            if (_performanceIssueThreshold <= 0f)
            {
                validationErrors.Add("Performance Issue Threshold must be greater than 0");
                isValid = false;
            }
            
            if (_learningEffectivenessThreshold < 0f || _learningEffectivenessThreshold > 1f)
            {
                validationErrors.Add("Learning Effectiveness Threshold must be between 0 and 1");
                isValid = false;
            }
            
            // Validate report types
            if (_enableReportGeneration && _enabledReports.Count == 0)
            {
                validationErrors.Add("Report generation enabled but no report types specified");
                isValid = false;
            }
            
            // Log validation results
            if (!isValid)
            {
                Debug.LogError($"[NarrativeAnalyticsConfigSO] Validation failed for {name}: {string.Join(", ", validationErrors)}");
            }
            
            return base.ValidateDataSpecific() && isValid;
        }
        
        /// <summary>
        /// Get analytics configuration optimized for privacy mode
        /// </summary>
        public NarrativeAnalyticsConfigSO GetPrivacyOptimizedSettings()
        {
            var optimized = Instantiate(this);
            
            // Disable personally identifiable tracking
            optimized._enableAnonymousDataCollection = true;
            optimized._enableDataEncryption = true;
            optimized._enableUserConsent = true;
            optimized._enableGDPRCompliance = true;
            optimized._enableOptOut = true;
            
            // Reduce data collection scope
            optimized._trackChoiceReactionTime = false;
            optimized._trackPlayerPreferences = false;
            optimized._enableMachineLearning = false;
            optimized._enablePredictiveAnalytics = false;
            
            // Shorter retention period
            optimized._dataRetentionDays = Mathf.Min(_dataRetentionDays, 30);
            optimized._maxAnalyticsHistoryDays = Mathf.Min(_maxAnalyticsHistoryDays, 30);
            
            return optimized;
        }
        
        /// <summary>
        /// Get analytics configuration optimized for educational assessment
        /// </summary>
        public NarrativeAnalyticsConfigSO GetEducationalOptimizedSettings()
        {
            var optimized = Instantiate(this);
            
            // Focus on educational tracking
            optimized._trackLearningOutcomes = true;
            optimized._trackKnowledgeRetention = true;
            optimized._trackSkillProgression = true;
            optimized._trackEducationalEngagement = true;
            optimized._validateScientificAccuracy = true;
            
            // More frequent assessment
            optimized._knowledgeAssessmentInterval = Mathf.Min(_knowledgeAssessmentInterval, 180f);
            
            // Enable advanced analytics for education
            optimized._enableMachineLearning = true;
            optimized._enablePredictiveAnalytics = true;
            optimized._enableClusterAnalysis = true;
            
            // Educational reporting
            if (!optimized._enabledReports.Contains(ReportType.LearningOutcomes))
                optimized._enabledReports.Add(ReportType.LearningOutcomes);
            if (!optimized._enabledReports.Contains(ReportType.SkillProgression))
                optimized._enabledReports.Add(ReportType.SkillProgression);
            if (!optimized._enabledReports.Contains(ReportType.EducationalEffectiveness))
                optimized._enabledReports.Add(ReportType.EducationalEffectiveness);
            
            return optimized;
        }
    }
    
    /// <summary>
    /// Types of analytics reports that can be generated
    /// </summary>
    public enum ReportType
    {
        PlayerBehavior,
        EmotionalEngagement,
        LearningOutcomes,
        StoryFlowAnalysis,
        CharacterPopularity,
        PerformanceMetrics,
        SkillProgression,
        EducationalEffectiveness,
        UserJourney,
        DropoffAnalysis,
        ABTestResults,
        CustomAnalytics
    }
}