using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.AI;
using AIRecommendationPriority = ProjectChimera.Data.AI.RecommendationPriority;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC014-1b-5: Specialized service for AI recommendation management
    /// Extracted from monolithic AIAdvisorManager.cs to handle recommendation
    /// lifecycle, prioritization, tracking, and user interaction.
    /// </summary>
    public class RecommendationService : IRecommendationService
    {
        private bool _isInitialized = false;
        private List<AIRecommendation> _activeRecommendations = new List<AIRecommendation>();
        private Dictionary<string, AIRecommendation> _recommendationRegistry = new Dictionary<string, AIRecommendation>();
        private Queue<AIRecommendation> _recommendationHistory = new Queue<AIRecommendation>();
        
        // Configuration
        private int _maxActiveRecommendations = 15;
        private int _maxHistorySize = 100;
        private float _defaultExpirationHours = 24f;
        
        // Tracking and metrics
        private Dictionary<string, int> _implementationCount = new Dictionary<string, int>();
        private Dictionary<string, int> _dismissalCount = new Dictionary<string, int>();
        private DateTime _lastCleanupTime;
        private float _cleanupIntervalHours = 6f;
        
        // Events
        public event Action<AIRecommendation> OnNewRecommendation;
        public event Action<AIRecommendation> OnRecommendationUpdated;
        
        public bool IsInitialized => _isInitialized;
        
        public void Initialize()
        {
            if (_isInitialized) return;
            
            _lastCleanupTime = DateTime.Now;
            
            _isInitialized = true;
            Debug.Log("[RecommendationService] Initialized successfully");
        }
        
        public void Shutdown()
        {
            _activeRecommendations.Clear();
            _recommendationRegistry.Clear();
            _recommendationHistory.Clear();
            _implementationCount.Clear();
            _dismissalCount.Clear();
            
            _isInitialized = false;
            Debug.Log("[RecommendationService] Shutdown complete");
        }
        
        public List<AIRecommendation> GetActiveRecommendations()
        {
            if (!_isInitialized) return new List<AIRecommendation>();
            
            // Return sorted by priority and creation time
            return _activeRecommendations
                .Where(r => r.IsActive && !IsExpired(r))
                .OrderByDescending(r => r.Priority)
                .ThenByDescending(r => r.CreationTime)
                .ToList();
        }
        
        public List<AIRecommendation> GetRecommendationsByCategory(string category)
        {
            if (!_isInitialized) return new List<AIRecommendation>();
            
            return _activeRecommendations
                .Where(r => r.IsActive && !IsExpired(r) && 
                           string.Equals(r.Category, category, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.Priority)
                .ThenByDescending(r => r.CreationTime)
                .ToList();
        }
        
        public void AddRecommendation(AIRecommendation recommendation)
        {
            if (!_isInitialized || recommendation == null) return;
            
            // Ensure unique ID
            if (string.IsNullOrEmpty(recommendation.Id))
            {
                recommendation.Id = Guid.NewGuid().ToString();
            }
            
            // Set default expiration if not set
            if (recommendation.ExpirationTime == default)
            {
                recommendation.ExpirationTime = DateTime.Now.AddHours(_defaultExpirationHours);
            }
            
            // Check for duplicates
            if (_recommendationRegistry.ContainsKey(recommendation.Id))
            {
                Debug.LogWarning($"[RecommendationService] Duplicate recommendation ID: {recommendation.Id}");
                return;
            }
            
            // Add to collections
            _activeRecommendations.Add(recommendation);
            _recommendationRegistry[recommendation.Id] = recommendation;
            
            // Enforce size limits
            EnforceSizeLimits();
            
            // Trigger event
            OnNewRecommendation?.Invoke(recommendation);
            
            Debug.Log($"[RecommendationService] Added recommendation: {recommendation.Title} (Priority: {recommendation.Priority})");
        }
        
        public void MarkAsImplemented(string recommendationId)
        {
            if (!_isInitialized || string.IsNullOrEmpty(recommendationId)) return;
            
            if (_recommendationRegistry.TryGetValue(recommendationId, out var recommendation))
            {
                recommendation.Status = RecommendationStatus.Implemented;
                recommendation.IsActive = false;
                recommendation.CompletionTime = DateTime.Now;
                
                // Track implementation statistics
                if (!_implementationCount.ContainsKey(recommendation.Category))
                    _implementationCount[recommendation.Category] = 0;
                _implementationCount[recommendation.Category]++;
                
                // Move to history
                MoveToHistory(recommendation);
                
                OnRecommendationUpdated?.Invoke(recommendation);
                
                Debug.Log($"[RecommendationService] Recommendation implemented: {recommendation.Title}");
            }
            else
            {
                Debug.LogWarning($"[RecommendationService] Recommendation not found: {recommendationId}");
            }
        }
        
        public void DismissRecommendation(string recommendationId)
        {
            if (!_isInitialized || string.IsNullOrEmpty(recommendationId)) return;
            
            if (_recommendationRegistry.TryGetValue(recommendationId, out var recommendation))
            {
                recommendation.Status = RecommendationStatus.Dismissed;
                recommendation.IsActive = false;
                recommendation.CompletionTime = DateTime.Now;
                
                // Track dismissal statistics
                if (!_dismissalCount.ContainsKey(recommendation.Category))
                    _dismissalCount[recommendation.Category] = 0;
                _dismissalCount[recommendation.Category]++;
                
                // Move to history
                MoveToHistory(recommendation);
                
                OnRecommendationUpdated?.Invoke(recommendation);
                
                Debug.Log($"[RecommendationService] Recommendation dismissed: {recommendation.Title}");
            }
            else
            {
                Debug.LogWarning($"[RecommendationService] Recommendation not found: {recommendationId}");
            }
        }
        
        public void UpdateRecommendationStatus()
        {
            if (!_isInitialized) return;
            
            var expiredRecommendations = _activeRecommendations
                .Where(r => r.IsActive && IsExpired(r))
                .ToList();
            
            foreach (var recommendation in expiredRecommendations)
            {
                recommendation.Status = RecommendationStatus.Expired;
                recommendation.IsActive = false;
                MoveToHistory(recommendation);
                
                OnRecommendationUpdated?.Invoke(recommendation);
            }
            
            if (expiredRecommendations.Count > 0)
            {
                Debug.Log($"[RecommendationService] Expired {expiredRecommendations.Count} recommendations");
            }
        }
        
        public void CleanupExpiredRecommendations()
        {
            if (!_isInitialized) return;
            
            // Check if cleanup is needed
            if ((DateTime.Now - _lastCleanupTime).TotalHours < _cleanupIntervalHours)
                return;
            
            UpdateRecommendationStatus();
            
            // Remove inactive recommendations from active list
            var toRemove = _activeRecommendations
                .Where(r => !r.IsActive)
                .ToList();
            
            foreach (var recommendation in toRemove)
            {
                _activeRecommendations.Remove(recommendation);
            }
            
            _lastCleanupTime = DateTime.Now;
            
            if (toRemove.Count > 0)
            {
                Debug.Log($"[RecommendationService] Cleaned up {toRemove.Count} inactive recommendations");
            }
        }
        
        #region Public Query Methods
        
        public int GetActiveRecommendationCount()
        {
            return GetActiveRecommendations().Count;
        }
        
        public List<AIRecommendation> GetHighPriorityRecommendations()
        {
            return GetActiveRecommendations()
                .Where(r => r.Priority == AIRecommendationPriority.High)
                .ToList();
        }
        
        public Dictionary<string, int> GetImplementationStatistics()
        {
            return new Dictionary<string, int>(_implementationCount);
        }
        
        public Dictionary<string, int> GetDismissalStatistics()
        {
            return new Dictionary<string, int>(_dismissalCount);
        }
        
        public float GetImplementationRate(string category = null)
        {
            var relevantHistory = string.IsNullOrEmpty(category) 
                ? _recommendationHistory.ToList()
                : _recommendationHistory.Where(r => r.Category == category).ToList();
            
            if (relevantHistory.Count == 0) return 0f;
            
            var implementedCount = relevantHistory.Count(r => r.Status == RecommendationStatus.Implemented);
            return (float)implementedCount / relevantHistory.Count;
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private bool IsExpired(AIRecommendation recommendation)
        {
            return DateTime.Now > recommendation.ExpirationTime;
        }
        
        private void MoveToHistory(AIRecommendation recommendation)
        {
            // Remove from active list and registry
            _activeRecommendations.Remove(recommendation);
            _recommendationRegistry.Remove(recommendation.Id);
            
            // Add to history
            _recommendationHistory.Enqueue(recommendation);
            
            // Limit history size
            if (_recommendationHistory.Count > _maxHistorySize)
            {
                _recommendationHistory.Dequeue();
            }
        }
        
        private void EnforceSizeLimits()
        {
            // If we exceed max active recommendations, remove oldest low-priority ones
            while (_activeRecommendations.Count > _maxActiveRecommendations)
            {
                var oldestLowPriority = _activeRecommendations
                    .Where(r => r.Priority == AIRecommendationPriority.Low)
                    .OrderBy(r => r.CreationTime)
                    .FirstOrDefault();
                
                if (oldestLowPriority != null)
                {
                    oldestLowPriority.Status = RecommendationStatus.Superseded;
                    oldestLowPriority.IsActive = false;
                    MoveToHistory(oldestLowPriority);
                }
                else
                {
                    // If no low priority items, remove oldest medium priority
                    var oldestMediumPriority = _activeRecommendations
                        .Where(r => r.Priority == AIRecommendationPriority.Medium)
                        .OrderBy(r => r.CreationTime)
                        .FirstOrDefault();
                    
                    if (oldestMediumPriority != null)
                    {
                        oldestMediumPriority.Status = RecommendationStatus.Superseded;
                        oldestMediumPriority.IsActive = false;
                        MoveToHistory(oldestMediumPriority);
                    }
                    else
                    {
                        break; // Don't remove high priority recommendations
                    }
                }
            }
        }
        
        #endregion
    }
}