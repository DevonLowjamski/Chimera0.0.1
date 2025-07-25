using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Systems.Gaming;

namespace ProjectChimera.Systems.Gaming
{
    /// <summary>
    /// Tracks player behavior patterns during mini-games
    /// </summary>
    public class PlayerBehaviorTracker
    {
        private Dictionary<string, List<PlayerAction>> _playerActions = new Dictionary<string, List<PlayerAction>>();
        private Dictionary<string, PlayerBehaviorPattern> _behaviorPatterns = new Dictionary<string, PlayerBehaviorPattern>();
        
        public void TrackAction(string gameId, PlayerAction action)
        {
            if (!_playerActions.ContainsKey(gameId))
            {
                _playerActions[gameId] = new List<PlayerAction>();
            }
            
            _playerActions[gameId].Add(action);
            UpdateBehaviorPattern(gameId, action);
        }
        
        public PlayerBehaviorPattern GetBehaviorPattern(string gameId)
        {
            return _behaviorPatterns.TryGetValue(gameId, out var pattern) ? pattern : new PlayerBehaviorPattern();
        }
        
        private void UpdateBehaviorPattern(string gameId, PlayerAction action)
        {
            if (!_behaviorPatterns.ContainsKey(gameId))
            {
                _behaviorPatterns[gameId] = new PlayerBehaviorPattern();
            }
            
            var pattern = _behaviorPatterns[gameId];
            pattern.TotalActions++;
            pattern.LastActionTime = DateTime.Now;
            
            // Update action type counts
            pattern.ActionTypeCounts[action.ActionType] = pattern.ActionTypeCounts.GetValueOrDefault(action.ActionType, 0) + 1;
        }
        
        public Dictionary<string, object> GetBehaviorAnalytics()
        {
            return new Dictionary<string, object>
            {
                {"total_tracked_games", _behaviorPatterns.Count},
                {"most_active_game", GetMostActiveGame()},
                {"average_actions_per_game", CalculateAverageActionsPerGame()}
            };
        }
        
        private string GetMostActiveGame()
        {
            return _behaviorPatterns.OrderByDescending(kvp => kvp.Value.TotalActions).FirstOrDefault().Key ?? "none";
        }
        
        private float CalculateAverageActionsPerGame()
        {
            return _behaviorPatterns.Count > 0 ? (float)_behaviorPatterns.Values.Average(p => p.TotalActions) : 0f;
        }
    }
    
    /// <summary>
    /// Calculates engagement metrics for mini-games
    /// </summary>
    public class EngagementMetricsCalculator
    {
        private Dictionary<string, EngagementData> _engagementData = new Dictionary<string, EngagementData>();
        
        public void RecordEngagement(string gameId, float sessionDuration, float interactionRate, bool completed)
        {
            if (!_engagementData.ContainsKey(gameId))
            {
                _engagementData[gameId] = new EngagementData();
            }
            
            var data = _engagementData[gameId];
            data.TotalSessions++;
            data.TotalDuration += sessionDuration;
            data.TotalInteractionRate += interactionRate;
            
            if (completed)
            {
                data.CompletedSessions++;
            }
            
            data.LastEngagement = DateTime.Now;
        }
        
        public EngagementMetrics CalculateMetrics(string gameId)
        {
            if (!_engagementData.TryGetValue(gameId, out var data))
            {
                return new EngagementMetrics();
            }
            
            return new EngagementMetrics
            {
                GameId = gameId,
                AverageSessionDuration = data.TotalSessions > 0 ? data.TotalDuration / data.TotalSessions : 0f,
                CompletionRate = data.TotalSessions > 0 ? (float)data.CompletedSessions / data.TotalSessions : 0f,
                AverageInteractionRate = data.TotalSessions > 0 ? data.TotalInteractionRate / data.TotalSessions : 0f,
                EngagementScore = CalculateEngagementScore(data),
                TotalSessions = data.TotalSessions
            };
        }
        
        public Dictionary<string, EngagementMetrics> GetAllMetrics()
        {
            var metrics = new Dictionary<string, EngagementMetrics>();
            foreach (var gameId in _engagementData.Keys)
            {
                metrics[gameId] = CalculateMetrics(gameId);
            }
            return metrics;
        }
        
        private float CalculateEngagementScore(EngagementData data)
        {
            if (data.TotalSessions == 0) return 0f;
            
            float completionScore = (float)data.CompletedSessions / data.TotalSessions;
            float durationScore = Mathf.Clamp01(data.TotalDuration / data.TotalSessions / 300f); // Normalize to 5 minutes
            float interactionScore = Mathf.Clamp01(data.TotalInteractionRate / data.TotalSessions);
            
            return (completionScore * 0.4f + durationScore * 0.3f + interactionScore * 0.3f) * 100f;
        }
        
        public void UpdateMetrics(MiniGameResult result)
        {
            // Update engagement metrics based on game result
            if (result == null) return;
            
            float sessionDuration = result.CompletionTime;
            float interactionRate = result.Accuracy / 100f; // Use accuracy as proxy for interaction rate
            bool completed = result.IsSuccess();
            
            RecordEngagement(result.GameId, sessionDuration, interactionRate, completed);
        }
    }
    
    /// <summary>
    /// Data structure for player actions
    /// </summary>
    [Serializable]
    public class PlayerAction
    {
        public string ActionType;
        public DateTime Timestamp;
        public Vector3 Position;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Player behavior pattern data
    /// </summary>
    [Serializable]
    public class PlayerBehaviorPattern
    {
        public int TotalActions;
        public DateTime LastActionTime;
        public Dictionary<string, int> ActionTypeCounts = new Dictionary<string, int>();
        public float AverageActionInterval => TotalActions > 1 ? CalculateAverageInterval() : 0f;
        
        private float CalculateAverageInterval()
        {
            // Simplified calculation - in real implementation would track actual intervals
            return 2.5f; // Average 2.5 seconds between actions
        }
    }
    
    /// <summary>
    /// Engagement data for a specific game
    /// </summary>
    [Serializable]
    public class EngagementData
    {
        public int TotalSessions;
        public int CompletedSessions;
        public float TotalDuration;
        public float TotalInteractionRate;
        public DateTime LastEngagement;
    }
    
    /// <summary>
    /// Calculated engagement metrics
    /// </summary>
    [Serializable]
    public class EngagementMetrics
    {
        public string GameId;
        public float AverageSessionDuration;
        public float CompletionRate;
        public float AverageInteractionRate;
        public float EngagementScore; // 0-100 composite score
        public int TotalSessions;
    }
} 