using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Genetics Analytics Manager - Advanced analytics and intelligence for genetics gaming systems
    /// Tracks player behavior, competition performance, research patterns, and system optimization
    /// Uses only verified types from ScientificGamingDataStructures to prevent compilation errors
    /// Operates independently within Genetics assembly to avoid assembly dependency issues
    /// 
    /// ABSTRACT METHOD VERIFICATION COMPLETE:
    /// ✅ OnManagerInitialize() - implemented
    /// ✅ OnManagerShutdown() - implemented
    /// </summary>
    public class GeneticsAnalyticsManager : ChimeraManager
    {
        [Header("Analytics Configuration")]
        public bool EnableAnalytics = true;
        public bool EnablePerformanceTracking = true;
        public bool EnableBehaviorAnalysis = true;
        public float AnalyticsUpdateInterval = 60f;
        
        [Header("Intelligence Settings")]
        public bool EnablePredictiveAnalytics = true;
        public bool EnableRecommendationEngine = true;
        public bool EnableBalancingInsights = true;
        public int MaxAnalyticsHistory = 1000;
        
        [Header("Data Collection")]
        public bool TrackCompetitionMetrics = true;
        public bool TrackResearchMetrics = true;
        public bool TrackPlayerProgressionMetrics = true;
        public bool TrackSystemPerformanceMetrics = true;
        
        [Header("Analytics Collections")]
        [SerializeField] private List<CleanAnalyticsEvent> analyticsEvents = new List<CleanAnalyticsEvent>();
        [SerializeField] private List<CleanPlayerBehaviorProfile> playerBehaviorProfiles = new List<CleanPlayerBehaviorProfile>();
        [SerializeField] private List<CleanSystemMetrics> systemMetrics = new List<CleanSystemMetrics>();
        [SerializeField] private List<CleanGameBalanceInsight> balanceInsights = new List<CleanGameBalanceInsight>();
        
        [Header("Analytics State")]
        [SerializeField] private DateTime lastAnalyticsUpdate = DateTime.Now;
        [SerializeField] private int totalEventsTracked = 0;
        [SerializeField] private float analyticsProcessingTime = 0f;
        [SerializeField] private Dictionary<string, float> metricAverages = new Dictionary<string, float>();
        
        // Events using verified event patterns (CS0070 prevention)
        public static event Action<CleanAnalyticsEvent> OnAnalyticsEventRecorded;
        public static event Action<CleanPlayerBehaviorProfile> OnBehaviorProfileUpdated;
        public static event Action<CleanGameBalanceInsight> OnBalanceInsightGenerated;
        public static event Action<CleanSystemMetrics> OnSystemMetricsUpdated;
        
        protected override void OnManagerInitialize()
        {
            // Register with GameManager using verified pattern
            GameManager.Instance?.RegisterManager(this);
            
            // Note: No cross-assembly dependencies to avoid CS0246 errors
            // Analytics system operates independently within Genetics assembly
            
            // Initialize analytics system
            InitializeAnalyticsSystem();
            
            if (EnableAnalytics)
            {
                StartAnalyticsTracking();
            }
            
            Debug.Log("✅ GeneticsAnalyticsManager initialized successfully");
        }
        
        protected override void OnManagerShutdown()
        {
            // Clean up analytics tracking
            if (EnableAnalytics)
            {
                StopAnalyticsTracking();
            }
            
            // Clear all events to prevent memory leaks
            OnAnalyticsEventRecorded = null;
            OnBehaviorProfileUpdated = null;
            OnBalanceInsightGenerated = null;
            OnSystemMetricsUpdated = null;
            
            Debug.Log("✅ GeneticsAnalyticsManager shutdown successfully");
        }
        
        private void InitializeAnalyticsSystem()
        {
            // Initialize collections if empty (LINQ prevention with proper using)
            if (analyticsEvents == null) analyticsEvents = new List<CleanAnalyticsEvent>();
            if (playerBehaviorProfiles == null) playerBehaviorProfiles = new List<CleanPlayerBehaviorProfile>();
            if (systemMetrics == null) systemMetrics = new List<CleanSystemMetrics>();
            if (balanceInsights == null) balanceInsights = new List<CleanGameBalanceInsight>();
            if (metricAverages == null) metricAverages = new Dictionary<string, float>();
            
            // Initialize analytics categories
            InitializeAnalyticsCategories();
            
            // Setup event subscriptions for other managers
            SubscribeToManagerEvents();
        }
        
        private void InitializeAnalyticsCategories()
        {
            // Initialize metric tracking categories
            metricAverages["competition_participation_rate"] = 0f;
            metricAverages["research_completion_rate"] = 0f;
            metricAverages["average_skill_progression"] = 0f;
            metricAverages["tournament_engagement"] = 0f;
            metricAverages["system_performance"] = 1f;
            
            Debug.Log("✅ Analytics categories initialized");
        }
        
        private void SubscribeToManagerEvents()
        {
            // Subscribe to other manager events for data collection (proper += syntax)
            if (TrackCompetitionMetrics)
            {
                ScientificCompetitionManager.OnCompetitionStarted += OnCompetitionStartedAnalytics;
                ScientificCompetitionManager.OnCompetitionCompleted += OnCompetitionCompletedAnalytics;
                AdvancedTournamentSystem.OnTournamentStarted += OnTournamentStartedAnalytics;
                AdvancedTournamentSystem.OnMatchCompleted += OnMatchCompletedAnalytics;
            }
            
            if (TrackResearchMetrics)
            {
                GeneticResearchManager.OnResearchProjectStarted += OnResearchStartedAnalytics;
                GeneticResearchManager.OnResearchProjectCompleted += OnResearchCompletedAnalytics;
            }
            
            Debug.Log("✅ Analytics event subscriptions established");
        }
        
        private void StartAnalyticsTracking()
        {
            // Start analytics data collection and processing
            lastAnalyticsUpdate = DateTime.Now;
            
            Debug.Log("✅ Analytics tracking started - operating independently");
        }
        
        private void StopAnalyticsTracking()
        {
            // Unsubscribe from manager events (proper -= syntax)
            if (TrackCompetitionMetrics)
            {
                ScientificCompetitionManager.OnCompetitionStarted -= OnCompetitionStartedAnalytics;
                ScientificCompetitionManager.OnCompetitionCompleted -= OnCompetitionCompletedAnalytics;
                AdvancedTournamentSystem.OnTournamentStarted -= OnTournamentStartedAnalytics;
                AdvancedTournamentSystem.OnMatchCompleted -= OnMatchCompletedAnalytics;
            }
            
            if (TrackResearchMetrics)
            {
                GeneticResearchManager.OnResearchProjectStarted -= OnResearchStartedAnalytics;
                GeneticResearchManager.OnResearchProjectCompleted -= OnResearchCompletedAnalytics;
            }
            
            Debug.Log("✅ Analytics tracking stopped");
        }
        
        private void Update()
        {
            if (!EnableAnalytics) return;
            
            // Process analytics data periodically
            if ((DateTime.Now - lastAnalyticsUpdate).TotalSeconds >= AnalyticsUpdateInterval)
            {
                ProcessAnalyticsData();
                lastAnalyticsUpdate = DateTime.Now;
            }
        }
        
        private void ProcessAnalyticsData()
        {
            var processingStartTime = Time.realtimeSinceStartup;
            
            // Update system metrics
            if (EnablePerformanceTracking)
            {
                UpdateSystemMetrics();
            }
            
            // Analyze player behavior patterns
            if (EnableBehaviorAnalysis)
            {
                AnalyzePlayerBehavior();
            }
            
            // Generate balance insights
            if (EnableBalancingInsights)
            {
                GenerateBalanceInsights();
            }
            
            // Generate recommendations
            if (EnableRecommendationEngine)
            {
                GenerateRecommendations();
            }
            
            analyticsProcessingTime = Time.realtimeSinceStartup - processingStartTime;
        }
        
        #region Event Handlers
        
        private void OnCompetitionStartedAnalytics(CleanScientificCompetition competition)
        {
            RecordAnalyticsEvent("competition_started", new Dictionary<string, object>
            {
                { "competition_type", competition.CompetitionType.ToString() },
                { "competition_id", competition.CompetitionID },
                { "participant_count", competition.Entries.Count }
            });
        }
        
        private void OnCompetitionCompletedAnalytics(CleanScientificCompetition competition)
        {
            RecordAnalyticsEvent("competition_completed", new Dictionary<string, object>
            {
                { "competition_type", competition.CompetitionType.ToString() },
                { "competition_id", competition.CompetitionID },
                { "final_participant_count", competition.Entries.Count },
                { "duration_days", (DateTime.Now - competition.StartDate).TotalDays }
            });
        }
        
        private void OnTournamentStartedAnalytics(CleanTournamentData tournament)
        {
            RecordAnalyticsEvent("tournament_started", new Dictionary<string, object>
            {
                { "tournament_type", tournament.TournamentType },
                { "tournament_id", tournament.TournamentID },
                { "participant_count", tournament.Participants.Count },
                { "prize_pool", tournament.PrizePool }
            });
        }
        
        private void OnMatchCompletedAnalytics(CleanTournamentMatch match)
        {
            RecordAnalyticsEvent("match_completed", new Dictionary<string, object>
            {
                { "tournament_id", match.TournamentID },
                { "round", match.Round },
                { "match_duration", match.IsCompleted ? (DateTime.Now - match.ScheduledDate).TotalMinutes : 0 }
            });
        }
        
        private void OnResearchStartedAnalytics(CleanGeneticResearchProject project)
        {
            RecordAnalyticsEvent("research_started", new Dictionary<string, object>
            {
                { "research_type", project.ResearchType.ToString() },
                { "project_id", project.ProjectID },
                { "estimated_duration", project.Requirements?.EstimatedTimeHours ?? 0f }
            });
        }
        
        private void OnResearchCompletedAnalytics(CleanGeneticResearchProject project)
        {
            RecordAnalyticsEvent("research_completed", new Dictionary<string, object>
            {
                { "research_type", project.ResearchType.ToString() },
                { "project_id", project.ProjectID },
                { "completion_progress", project.Progress },
                { "actual_duration", (DateTime.Now - project.StartDate).TotalHours }
            });
        }
        
        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// Record a custom analytics event
        /// </summary>
        public void RecordAnalyticsEvent(string eventType, Dictionary<string, object> eventData = null)
        {
            if (!EnableAnalytics) return;
            
            var analyticsEvent = new CleanAnalyticsEvent
            {
                EventID = $"event_{DateTime.Now.Ticks}",
                EventType = eventType,
                Timestamp = DateTime.Now,
                EventData = eventData ?? new Dictionary<string, object>(),
                SessionID = "current_session", // Would be actual session ID
                PlayerID = "current_player" // Would be actual player ID
            };
            
            analyticsEvents.Add(analyticsEvent);
            totalEventsTracked++;
            
            // Maintain history limit
            if (analyticsEvents.Count > MaxAnalyticsHistory)
            {
                analyticsEvents.RemoveAt(0);
            }
            
            OnAnalyticsEventRecorded?.Invoke(analyticsEvent);
        }
        
        /// <summary>
        /// Get analytics summary for a specific time period
        /// </summary>
        public CleanAnalyticsSummary GetAnalyticsSummary(DateTime startDate, DateTime endDate)
        {
            var eventsInPeriod = analyticsEvents.Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate).ToList();
            
            var summary = new CleanAnalyticsSummary
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalEvents = eventsInPeriod.Count,
                UniqueEventTypes = eventsInPeriod.Select(e => e.EventType).Distinct().Count(),
                EventTypeBreakdown = eventsInPeriod.GroupBy(e => e.EventType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                GeneratedDate = DateTime.Now
            };
            
            return summary;
        }
        
        /// <summary>
        /// Get player behavior profile
        /// </summary>
        public CleanPlayerBehaviorProfile GetPlayerBehaviorProfile(string playerId)
        {
            return playerBehaviorProfiles.FirstOrDefault(p => p.PlayerID == playerId);
        }
        
        /// <summary>
        /// Get system performance metrics
        /// </summary>
        public List<CleanSystemMetrics> GetSystemMetrics(int count = 10)
        {
            return systemMetrics.OrderByDescending(m => m.Timestamp).Take(count).ToList();
        }
        
        /// <summary>
        /// Get balance insights
        /// </summary>
        public List<CleanGameBalanceInsight> GetBalanceInsights()
        {
            return new List<CleanGameBalanceInsight>(balanceInsights);
        }
        
        /// <summary>
        /// Get metric averages
        /// </summary>
        public Dictionary<string, float> GetMetricAverages()
        {
            return new Dictionary<string, float>(metricAverages);
        }
        
        #endregion
        
        #region Private Analytics Methods
        
        private void UpdateSystemMetrics()
        {
            var metrics = new CleanSystemMetrics
            {
                MetricsID = $"metrics_{DateTime.Now.Ticks}",
                Timestamp = DateTime.Now,
                FrameRate = 1f / Time.deltaTime,
                MemoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f), // MB
                ActiveManagers = 6, // Our current manager count
                AnalyticsProcessingTime = analyticsProcessingTime,
                SystemHealth = CalculateSystemHealth()
            };
            
            systemMetrics.Add(metrics);
            
            // Maintain metrics history
            if (systemMetrics.Count > MaxAnalyticsHistory / 10)
            {
                systemMetrics.RemoveAt(0);
            }
            
            OnSystemMetricsUpdated?.Invoke(metrics);
        }
        
        private void AnalyzePlayerBehavior()
        {
            // Analyze player behavior patterns from recent events
            var recentEvents = analyticsEvents.Where(e => e.Timestamp >= DateTime.Now.AddHours(-24)).ToList();
            
            if (recentEvents.Count == 0) return;
            
            var playerGroups = recentEvents.GroupBy(e => e.PlayerID);
            
            foreach (var playerGroup in playerGroups)
            {
                var playerId = playerGroup.Key;
                var playerEvents = playerGroup.ToList();
                
                var behaviorProfile = playerBehaviorProfiles.FirstOrDefault(p => p.PlayerID == playerId) 
                    ?? new CleanPlayerBehaviorProfile { PlayerID = playerId };
                
                // Update behavior metrics
                behaviorProfile.SessionCount++;
                behaviorProfile.TotalEvents += playerEvents.Count;
                behaviorProfile.LastActivity = DateTime.Now;
                behaviorProfile.PreferredGameModes = CalculatePreferredGameModes(playerEvents);
                behaviorProfile.EngagementScore = CalculateEngagementScore(playerEvents);
                behaviorProfile.SkillProgressionRate = CalculateSkillProgressionRate(playerEvents);
                
                // Add or update profile
                if (!playerBehaviorProfiles.Contains(behaviorProfile))
                {
                    playerBehaviorProfiles.Add(behaviorProfile);
                }
                
                OnBehaviorProfileUpdated?.Invoke(behaviorProfile);
            }
        }
        
        private void GenerateBalanceInsights()
        {
            // Generate insights about game balance from analytics data
            var competitionEvents = analyticsEvents.Where(e => e.EventType.Contains("competition") || e.EventType.Contains("tournament")).ToList();
            
            if (competitionEvents.Count < 10) return; // Need sufficient data
            
            // Analyze competition participation rates
            var participationInsight = new CleanGameBalanceInsight
            {
                InsightID = $"insight_{DateTime.Now.Ticks}",
                InsightType = "Competition Participation",
                InsightTitle = "Competition Participation Analysis",
                InsightDescription = AnalyzeCompetitionParticipation(competitionEvents),
                Severity = CalculateInsightSeverity(competitionEvents.Count),
                Timestamp = DateTime.Now,
                RecommendedActions = GenerateParticipationRecommendations(competitionEvents)
            };
            
            balanceInsights.Add(participationInsight);
            
            // Maintain insights history
            if (balanceInsights.Count > 50)
            {
                balanceInsights.RemoveAt(0);
            }
            
            OnBalanceInsightGenerated?.Invoke(participationInsight);
        }
        
        private void GenerateRecommendations()
        {
            // Generate intelligent recommendations based on analytics
            if (EnablePredictiveAnalytics && totalEventsTracked > 100)
            {
                // Simple recommendation logic - can be expanded
                var recentCompetitions = analyticsEvents.Where(e => 
                    e.EventType == "competition_started" && 
                    e.Timestamp >= DateTime.Now.AddDays(-7)).Count();
                
                if (recentCompetitions < 3)
                {
                    RecordAnalyticsEvent("recommendation_generated", new Dictionary<string, object>
                    {
                        { "recommendation_type", "increase_competition_frequency" },
                        { "reason", "low_recent_competition_activity" },
                        { "suggested_action", "create_weekly_tournament_schedule" }
                    });
                }
            }
        }
        
        private float CalculateSystemHealth()
        {
            // Calculate overall system health score
            float health = 1f;
            
            // Factor in frame rate
            float currentFPS = 1f / Time.deltaTime;
            if (currentFPS < 30f) health *= 0.7f;
            else if (currentFPS < 60f) health *= 0.9f;
            
            // Factor in memory usage
            float memoryMB = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            if (memoryMB > 1000f) health *= 0.8f;
            else if (memoryMB > 500f) health *= 0.95f;
            
            // Factor in processing time
            if (analyticsProcessingTime > 0.1f) health *= 0.9f;
            
            return Mathf.Clamp01(health);
        }
        
        private List<string> CalculatePreferredGameModes(List<CleanAnalyticsEvent> playerEvents)
        {
            var gameModes = new List<string>();
            
            var competitionEvents = playerEvents.Count(e => e.EventType.Contains("competition"));
            var researchEvents = playerEvents.Count(e => e.EventType.Contains("research"));
            var tournamentEvents = playerEvents.Count(e => e.EventType.Contains("tournament"));
            
            if (competitionEvents > researchEvents && competitionEvents > tournamentEvents)
                gameModes.Add("Competitive");
            else if (researchEvents > tournamentEvents)
                gameModes.Add("Research-Focused");
            else
                gameModes.Add("Tournament");
            
            return gameModes;
        }
        
        private float CalculateEngagementScore(List<CleanAnalyticsEvent> playerEvents)
        {
            // Simple engagement score calculation
            float baseScore = Mathf.Min(playerEvents.Count / 10f, 1f); // Events per day
            
            // Bonus for diversity
            float diversityBonus = playerEvents.Select(e => e.EventType).Distinct().Count() / 10f;
            
            return Mathf.Clamp01(baseScore + diversityBonus);
        }
        
        private float CalculateSkillProgressionRate(List<CleanAnalyticsEvent> playerEvents)
        {
            // Calculate skill progression based on completion events
            var completionEvents = playerEvents.Count(e => e.EventType.Contains("completed"));
            return Mathf.Clamp01(completionEvents / 5f); // Normalize to 0-1
        }
        
        private string AnalyzeCompetitionParticipation(List<CleanAnalyticsEvent> competitionEvents)
        {
            var participationRate = competitionEvents.Count / Math.Max(1f, analyticsEvents.Count);
            
            if (participationRate > 0.3f)
                return "High competition participation indicates strong competitive engagement";
            else if (participationRate > 0.1f)
                return "Moderate competition participation suggests balanced player interests";
            else
                return "Low competition participation may indicate need for better incentives";
        }
        
        private string CalculateInsightSeverity(int eventCount)
        {
            if (eventCount > 100) return "High";
            if (eventCount > 50) return "Medium";
            return "Low";
        }
        
        private List<string> GenerateParticipationRecommendations(List<CleanAnalyticsEvent> events)
        {
            var recommendations = new List<string>();
            
            if (events.Count < 20)
            {
                recommendations.Add("Increase competition frequency");
                recommendations.Add("Add more tournament types");
                recommendations.Add("Improve competition rewards");
            }
            else
            {
                recommendations.Add("Maintain current competition schedule");
                recommendations.Add("Focus on competition quality improvements");
            }
            
            return recommendations;
        }
        
        #endregion
        
        #region Testing and Validation Methods
        
        /// <summary>
        /// Test method to validate analytics system functionality
        /// </summary>
        public void TestAnalyticsSystem()
        {
            Debug.Log("=== Testing Genetics Analytics System ===");
            Debug.Log($"Analytics Enabled: {EnableAnalytics}");
            Debug.Log($"Performance Tracking: {EnablePerformanceTracking}");
            Debug.Log($"Behavior Analysis: {EnableBehaviorAnalysis}");
            Debug.Log($"Total Events Tracked: {totalEventsTracked}");
            Debug.Log($"Player Behavior Profiles: {playerBehaviorProfiles.Count}");
            Debug.Log($"System Metrics Records: {systemMetrics.Count}");
            Debug.Log($"Balance Insights: {balanceInsights.Count}");
            Debug.Log($"Processing Time: {analyticsProcessingTime:F4}s");
            
            // Test analytics event recording
            if (EnableAnalytics)
            {
                RecordAnalyticsEvent("test_event", new Dictionary<string, object>
                {
                    { "test_parameter", "test_value" },
                    { "test_number", 42 }
                });
                Debug.Log("✓ Test analytics event recorded");
                
                // Test analytics summary
                var summary = GetAnalyticsSummary(DateTime.Now.AddHours(-1), DateTime.Now);
                Debug.Log($"✓ Analytics summary: {summary.TotalEvents} events in last hour");
            }
            
            Debug.Log("✅ Genetics analytics system test completed");
        }
        
        #endregion
    }
    
    #region Supporting Analytics Data Structures
    
    [System.Serializable]
    public class CleanAnalyticsEvent
    {
        public string EventID;
        public string EventType;
        public DateTime Timestamp;
        public Dictionary<string, object> EventData = new Dictionary<string, object>();
        public string SessionID;
        public string PlayerID;
    }
    
    [System.Serializable]
    public class CleanPlayerBehaviorProfile
    {
        public string PlayerID;
        public int SessionCount;
        public int TotalEvents;
        public DateTime LastActivity;
        public List<string> PreferredGameModes = new List<string>();
        public float EngagementScore;
        public float SkillProgressionRate;
        public DateTime ProfileCreated = DateTime.Now;
    }
    
    [System.Serializable]
    public class CleanSystemMetrics
    {
        public string MetricsID;
        public DateTime Timestamp;
        public float FrameRate;
        public float MemoryUsage;
        public int ActiveManagers;
        public float AnalyticsProcessingTime;
        public float SystemHealth;
    }
    
    [System.Serializable]
    public class CleanGameBalanceInsight
    {
        public string InsightID;
        public string InsightType;
        public string InsightTitle;
        public string InsightDescription;
        public string Severity;
        public DateTime Timestamp;
        public List<string> RecommendedActions = new List<string>();
    }
    
    [System.Serializable]
    public class CleanAnalyticsSummary
    {
        public DateTime StartDate;
        public DateTime EndDate;
        public int TotalEvents;
        public int UniqueEventTypes;
        public Dictionary<string, int> EventTypeBreakdown = new Dictionary<string, int>();
        public DateTime GeneratedDate;
    }
    
    #endregion
}