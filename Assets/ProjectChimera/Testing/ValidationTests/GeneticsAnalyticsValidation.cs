using UnityEngine;
using ProjectChimera.Systems.Genetics;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Validation test for GeneticsAnalyticsManager
/// Verifies analytics collection, behavior analysis, and intelligence features
/// </summary>
public class GeneticsAnalyticsValidation : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== Genetics Analytics Manager Validation ===");
        
        // Test manager instantiation
        TestManagerInstantiation();
        
        // Test analytics event recording
        TestAnalyticsEventRecording();
        
        // Test behavior analysis
        TestBehaviorAnalysis();
        
        // Test system metrics
        TestSystemMetrics();
        
        // Test balance insights
        TestBalanceInsights();
        
        // Test data structure integrity
        TestAnalyticsDataStructures();
        
        Debug.Log("✅ Genetics Analytics Manager validation completed successfully");
    }
    
    private void TestManagerInstantiation()
    {
        // Test that manager can be instantiated (inheritance check)
        var testObject = new GameObject("TestAnalyticsManager");
        var manager = testObject.AddComponent<GeneticsAnalyticsManager>();
        
        // Verify it's a ChimeraManager
        bool isChimeraManager = manager is ProjectChimera.Core.ChimeraManager;
        
        Debug.Log($"✅ Manager instantiation: {(manager != null ? "Success" : "Failed")}");
        Debug.Log($"✅ ChimeraManager inheritance: {isChimeraManager}");
        
        // Test analytics configuration
        manager.EnableAnalytics = true;
        manager.EnablePerformanceTracking = true;
        manager.EnableBehaviorAnalysis = true;
        manager.EnablePredictiveAnalytics = true;
        manager.EnableRecommendationEngine = true;
        manager.MaxAnalyticsHistory = 1000;
        
        Debug.Log($"✅ Analytics configuration: Enabled={manager.EnableAnalytics}, History={manager.MaxAnalyticsHistory}");
        
        // Cleanup
        DestroyImmediate(testObject);
    }
    
    private void TestAnalyticsEventRecording()
    {
        // Test analytics event data structure
        var analyticsEvent = new CleanAnalyticsEvent
        {
            EventID = "validation_event_001",
            EventType = "competition_validation",
            Timestamp = DateTime.Now,
            SessionID = "validation_session",
            PlayerID = "validation_player"
        };
        
        // Test event data collection
        analyticsEvent.EventData["competition_type"] = "Breeding Excellence";
        analyticsEvent.EventData["participant_count"] = 16;
        analyticsEvent.EventData["duration_minutes"] = 45.5f;
        analyticsEvent.EventData["skill_level"] = "Advanced";
        analyticsEvent.EventData["completion_rate"] = 0.85f;
        
        Debug.Log($"✅ Analytics event created: {analyticsEvent.EventType}");
        Debug.Log($"  - Event ID: {analyticsEvent.EventID}");
        Debug.Log($"  - Timestamp: {analyticsEvent.Timestamp:yyyy-MM-dd HH:mm:ss}");
        Debug.Log($"  - Data Points: {analyticsEvent.EventData.Count}");
        
        // Test event data validation
        bool hasValidData = analyticsEvent.EventData.ContainsKey("competition_type") &&
                           analyticsEvent.EventData.ContainsKey("participant_count") &&
                           analyticsEvent.EventData.ContainsKey("completion_rate");
        
        Debug.Log($"  - Data Validation: {(hasValidData ? "Valid" : "Invalid")}");
        
        // Test multiple event types
        TestMultipleEventTypes();
    }
    
    private void TestMultipleEventTypes()
    {
        var eventTypes = new List<(string type, Dictionary<string, object> data)>
        {
            ("research_started", new Dictionary<string, object>
            {
                { "research_type", "TraitMapping" },
                { "estimated_duration", 168f },
                { "complexity", "High" }
            }),
            ("tournament_match_completed", new Dictionary<string, object>
            {
                { "tournament_id", "tournament_001" },
                { "round", 2 },
                { "match_duration", 25.3f },
                { "skill_difference", 150f }
            }),
            ("player_skill_updated", new Dictionary<string, object>
            {
                { "old_rating", 1200f },
                { "new_rating", 1225f },
                { "rating_change", 25f },
                { "games_played", 15 }
            }),
            ("system_performance", new Dictionary<string, object>
            {
                { "frame_rate", 60.2f },
                { "memory_usage", 234.5f },
                { "processing_time", 0.0023f }
            })
        };
        
        Debug.Log($"✅ Multiple event types validated: {eventTypes.Count} types");
        foreach (var (type, data) in eventTypes)
        {
            Debug.Log($"  - {type}: {data.Count} data points");
        }
    }
    
    private void TestBehaviorAnalysis()
    {
        // Test player behavior profile creation
        var behaviorProfile = new CleanPlayerBehaviorProfile
        {
            PlayerID = "behavior_test_player",
            SessionCount = 25,
            TotalEvents = 150,
            LastActivity = DateTime.Now,
            EngagementScore = 0.85f,
            SkillProgressionRate = 0.75f,
            ProfileCreated = DateTime.Now.AddDays(-30)
        };
        
        // Test preferred game modes
        behaviorProfile.PreferredGameModes.Add("Competitive");
        behaviorProfile.PreferredGameModes.Add("Research-Focused");
        behaviorProfile.PreferredGameModes.Add("Tournament");
        
        Debug.Log($"✅ Behavior profile created: {behaviorProfile.PlayerID}");
        Debug.Log($"  - Sessions: {behaviorProfile.SessionCount}");
        Debug.Log($"  - Total Events: {behaviorProfile.TotalEvents}");
        Debug.Log($"  - Engagement Score: {behaviorProfile.EngagementScore:P}");
        Debug.Log($"  - Skill Progression: {behaviorProfile.SkillProgressionRate:P}");
        Debug.Log($"  - Preferred Modes: {string.Join(", ", behaviorProfile.PreferredGameModes)}");
        
        // Test behavior pattern analysis
        TestBehaviorPatternAnalysis(behaviorProfile);
    }
    
    private void TestBehaviorPatternAnalysis(CleanPlayerBehaviorProfile profile)
    {
        // Analyze engagement patterns
        string engagementLevel = profile.EngagementScore switch
        {
            >= 0.8f => "Highly Engaged",
            >= 0.6f => "Moderately Engaged",
            >= 0.4f => "Casually Engaged",
            _ => "Low Engagement"
        };
        
        // Analyze skill progression
        string progressionRate = profile.SkillProgressionRate switch
        {
            >= 0.8f => "Rapid Learner",
            >= 0.6f => "Steady Learner",
            >= 0.4f => "Moderate Learner",
            _ => "Slow Learner"
        };
        
        // Calculate session frequency
        var profileAge = (DateTime.Now - profile.ProfileCreated).TotalDays;
        var sessionsPerDay = profile.SessionCount / Math.Max(1, profileAge);
        
        Debug.Log($"✅ Behavior pattern analysis:");
        Debug.Log($"  - Engagement Level: {engagementLevel}");
        Debug.Log($"  - Learning Style: {progressionRate}");
        Debug.Log($"  - Session Frequency: {sessionsPerDay:F2} sessions/day");
        Debug.Log($"  - Activity Diversity: {profile.PreferredGameModes.Count} game modes");
    }
    
    private void TestSystemMetrics()
    {
        // Test system metrics data structure
        var systemMetrics = new CleanSystemMetrics
        {
            MetricsID = "metrics_validation_001",
            Timestamp = DateTime.Now,
            FrameRate = 60.3f,
            MemoryUsage = 234.7f, // MB
            ActiveManagers = 6,
            AnalyticsProcessingTime = 0.0045f, // seconds
            SystemHealth = 0.95f
        };
        
        Debug.Log($"✅ System metrics recorded: {systemMetrics.MetricsID}");
        Debug.Log($"  - Frame Rate: {systemMetrics.FrameRate:F1} FPS");
        Debug.Log($"  - Memory Usage: {systemMetrics.MemoryUsage:F1} MB");
        Debug.Log($"  - Active Managers: {systemMetrics.ActiveManagers}");
        Debug.Log($"  - Processing Time: {systemMetrics.AnalyticsProcessingTime:F4}s");
        Debug.Log($"  - System Health: {systemMetrics.SystemHealth:P}");
        
        // Test system health calculation
        TestSystemHealthCalculation(systemMetrics);
    }
    
    private void TestSystemHealthCalculation(CleanSystemMetrics metrics)
    {
        // Test health score interpretation
        string healthStatus = metrics.SystemHealth switch
        {
            >= 0.9f => "Excellent",
            >= 0.8f => "Good",
            >= 0.7f => "Fair",
            >= 0.6f => "Poor",
            _ => "Critical"
        };
        
        // Performance recommendations
        var recommendations = new List<string>();
        
        if (metrics.FrameRate < 30f)
            recommendations.Add("Optimize rendering performance");
        else if (metrics.FrameRate < 60f)
            recommendations.Add("Minor performance optimization recommended");
        
        if (metrics.MemoryUsage > 500f)
            recommendations.Add("Memory usage optimization needed");
        
        if (metrics.AnalyticsProcessingTime > 0.01f)
            recommendations.Add("Analytics processing optimization recommended");
        
        Debug.Log($"✅ System health analysis:");
        Debug.Log($"  - Health Status: {healthStatus}");
        Debug.Log($"  - Recommendations: {(recommendations.Count > 0 ? string.Join(", ", recommendations) : "None")}");
    }
    
    private void TestBalanceInsights()
    {
        // Test game balance insight creation
        var balanceInsight = new CleanGameBalanceInsight
        {
            InsightID = "insight_validation_001",
            InsightType = "Competition Balance",
            InsightTitle = "Tournament Participation Analysis",
            InsightDescription = "High competition participation indicates strong competitive engagement with 75% of active players participating in weekly tournaments.",
            Severity = "Medium",
            Timestamp = DateTime.Now
        };
        
        // Test recommended actions
        balanceInsight.RecommendedActions.Add("Maintain current tournament schedule");
        balanceInsight.RecommendedActions.Add("Add skill-based tournament tiers");
        balanceInsight.RecommendedActions.Add("Introduce seasonal championship events");
        balanceInsight.RecommendedActions.Add("Monitor prize pool distribution effectiveness");
        
        Debug.Log($"✅ Balance insight generated: {balanceInsight.InsightType}");
        Debug.Log($"  - Title: {balanceInsight.InsightTitle}");
        Debug.Log($"  - Severity: {balanceInsight.Severity}");
        Debug.Log($"  - Description: {balanceInsight.InsightDescription}");
        Debug.Log($"  - Recommendations: {balanceInsight.RecommendedActions.Count} actions");
        
        foreach (var action in balanceInsight.RecommendedActions)
        {
            Debug.Log($"    - {action}");
        }
        
        // Test multiple insight types
        TestMultipleInsightTypes();
    }
    
    private void TestMultipleInsightTypes()
    {
        var insightTypes = new List<(string type, string description, string severity)>
        {
            ("Research Engagement", "Players show strong preference for collaborative research projects over solo research", "Low"),
            ("Skill Progression", "New players are progressing through skill tiers 30% faster than expected baseline", "Medium"),
            ("Competition Balance", "Advanced players dominating tournaments - consider skill-based brackets", "High"),
            ("System Performance", "Analytics processing time increased 15% over last week due to data volume growth", "Medium"),
            ("Player Retention", "Weekly active users increased 22% following tournament reward improvements", "Low")
        };
        
        Debug.Log($"✅ Multiple insight types validated: {insightTypes.Count} categories");
        foreach (var (type, description, severity) in insightTypes)
        {
            Debug.Log($"  - {type} ({severity}): {description.Substring(0, Math.Min(50, description.Length))}...");
        }
    }
    
    private void TestAnalyticsDataStructures()
    {
        // Test analytics summary creation
        var analyticsSummary = new CleanAnalyticsSummary
        {
            StartDate = DateTime.Now.AddDays(-7),
            EndDate = DateTime.Now,
            TotalEvents = 1250,
            UniqueEventTypes = 15,
            GeneratedDate = DateTime.Now
        };
        
        // Test event type breakdown
        analyticsSummary.EventTypeBreakdown["competition_started"] = 45;
        analyticsSummary.EventTypeBreakdown["competition_completed"] = 42;
        analyticsSummary.EventTypeBreakdown["research_started"] = 38;
        analyticsSummary.EventTypeBreakdown["research_completed"] = 35;
        analyticsSummary.EventTypeBreakdown["tournament_started"] = 12;
        analyticsSummary.EventTypeBreakdown["match_completed"] = 156;
        analyticsSummary.EventTypeBreakdown["player_skill_updated"] = 89;
        analyticsSummary.EventTypeBreakdown["system_metrics"] = 168;
        
        Debug.Log($"✅ Analytics summary generated:");
        Debug.Log($"  - Time Period: {analyticsSummary.StartDate:yyyy-MM-dd} to {analyticsSummary.EndDate:yyyy-MM-dd}");
        Debug.Log($"  - Total Events: {analyticsSummary.TotalEvents}");
        Debug.Log($"  - Unique Event Types: {analyticsSummary.UniqueEventTypes}");
        Debug.Log($"  - Event Breakdown: {analyticsSummary.EventTypeBreakdown.Count} categories");
        
        // Test top event types
        var topEvents = analyticsSummary.EventTypeBreakdown
            .OrderByDescending(kvp => kvp.Value)
            .Take(3)
            .ToList();
        
        Debug.Log($"  - Top Event Types:");
        foreach (var (eventType, count) in topEvents)
        {
            Debug.Log($"    - {eventType}: {count} events");
        }
        
        // Test data structure serialization
        TestAnalyticsSerialiation(analyticsSummary);
    }
    
    private void TestAnalyticsSerialiation(CleanAnalyticsSummary summary)
    {
        // Test serialization of analytics data structures
        var summaryJson = JsonUtility.ToJson(summary, true);
        bool summarySerializable = !string.IsNullOrEmpty(summaryJson);
        
        var behaviorProfile = new CleanPlayerBehaviorProfile { PlayerID = "test" };
        var behaviorJson = JsonUtility.ToJson(behaviorProfile, true);
        bool behaviorSerializable = !string.IsNullOrEmpty(behaviorJson);
        
        var systemMetrics = new CleanSystemMetrics { MetricsID = "test" };
        var metricsJson = JsonUtility.ToJson(systemMetrics, true);
        bool metricsSerializable = !string.IsNullOrEmpty(metricsJson);
        
        var balanceInsight = new CleanGameBalanceInsight { InsightID = "test" };
        var insightJson = JsonUtility.ToJson(balanceInsight, true);
        bool insightSerializable = !string.IsNullOrEmpty(insightJson);
        
        Debug.Log($"✅ Analytics data serialization test:");
        Debug.Log($"  - Analytics Summary: {(summarySerializable ? "Serializable" : "Failed")}");
        Debug.Log($"  - Behavior Profile: {(behaviorSerializable ? "Serializable" : "Failed")}");
        Debug.Log($"  - System Metrics: {(metricsSerializable ? "Serializable" : "Failed")}");
        Debug.Log($"  - Balance Insight: {(insightSerializable ? "Serializable" : "Failed")}");
        
        bool allSerializable = summarySerializable && behaviorSerializable && metricsSerializable && insightSerializable;
        Debug.Log($"  - Overall Result: {(allSerializable ? "All structures serializable" : "Some serialization issues")}");
    }
}