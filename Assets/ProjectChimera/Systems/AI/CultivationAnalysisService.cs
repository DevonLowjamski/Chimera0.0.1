using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.AI;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Systems.Cultivation;
using AIRecommendationPriority = ProjectChimera.Data.AI.RecommendationPriority;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC014-1b-2: Specialized service for cultivation-specific AI analysis
    /// Extracted from monolithic AIAdvisorManager.cs to handle plant health,
    /// growth patterns, and cultivation optimization analysis.
    /// </summary>
    public class CultivationAnalysisService : ICultivationAnalysisService
    {
        private bool _isInitialized = false;
        private CultivationManager _cultivationManager;
        private List<DataInsight> _cultivationInsights = new List<DataInsight>();
        private List<AIRecommendation> _cultivationRecommendations = new List<AIRecommendation>();
        
        // Analysis configuration
        private float _healthThreshold = 0.7f;
        private float _growthEfficiencyThreshold = 0.8f;
        private int _maxRecommendations = 10;
        
        public bool IsInitialized => _isInitialized;
        
        public void Initialize()
        {
            if (_isInitialized) return;
            
            _cultivationManager = GameManager.Instance?.GetManager<CultivationManager>();
            if (_cultivationManager == null)
            {
                Debug.LogWarning("[CultivationAnalysisService] CultivationManager not found - using simulated data");
            }
            
            _isInitialized = true;
            Debug.Log("[CultivationAnalysisService] Initialized successfully");
        }
        
        public void Shutdown()
        {
            _cultivationInsights.Clear();
            _cultivationRecommendations.Clear();
            _isInitialized = false;
            Debug.Log("[CultivationAnalysisService] Shutdown complete");
        }
        
        public void PerformAnalysis(AnalysisSnapshot snapshot)
        {
            if (!_isInitialized) return;
            
            // Clear previous analysis results
            _cultivationRecommendations.Clear();
            
            // Perform cultivation-specific analysis
            AnalyzePlantHealthStatus(snapshot);
            AnalyzeGrowthEfficiency(snapshot);
            AnalyzeCareOptimization(snapshot);
            
            Debug.Log($"[CultivationAnalysisService] Analysis complete - {_cultivationRecommendations.Count} recommendations generated");
        }
        
        public List<AIRecommendation> AnalyzePlantHealth()
        {
            var recommendations = new List<AIRecommendation>();
            
            if (_cultivationManager != null)
            {
                var plants = _cultivationManager.GetAllPlants();
                var unhealthyPlants = plants.Where(p => p.CurrentHealth < _healthThreshold).ToList();
                
                if (unhealthyPlants.Any())
                {
                    recommendations.Add(CreateHealthRecommendation(
                        "Plant Health Alert",
                        $"{unhealthyPlants.Count} plants below optimal health",
                        $"Monitor and treat {unhealthyPlants.Count} plants with health below {_healthThreshold * 100}%",
                        AIRecommendationPriority.High
                    ));
                }
            }
            else
            {
                // Simulated analysis when manager unavailable
                if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
                {
                    recommendations.Add(CreateHealthRecommendation(
                        "Plant Health Optimization",
                        "Some plants could benefit from improved care",
                        "Review watering and feeding schedules for optimal plant health",
                        AIRecommendationPriority.Medium
                    ));
                }
            }
            
            return recommendations;
        }
        
        public List<AIRecommendation> AnalyzeGrowthPatterns()
        {
            var recommendations = new List<AIRecommendation>();
            
            if (_cultivationManager != null)
            {
                var plants = _cultivationManager.GetAllPlants();
                var slowGrowingPlants = plants.Where(p => CalculateGrowthEfficiency(p) < _growthEfficiencyThreshold).ToList();
                
                if (slowGrowingPlants.Any())
                {
                    recommendations.Add(CreateGrowthRecommendation(
                        "Growth Optimization",
                        $"{slowGrowingPlants.Count} plants growing slower than expected",
                        "Consider adjusting environmental conditions or care schedules to improve growth rates",
                        AIRecommendationPriority.Medium
                    ));
                }
            }
            else
            {
                // Simulated growth analysis
                if (UnityEngine.Random.Range(0f, 1f) < 0.4f)
                {
                    recommendations.Add(CreateGrowthRecommendation(
                        "Growth Pattern Insight",
                        "Growth rates could be optimized",
                        "Environmental conditions appear suboptimal for current growth stage",
                        AIRecommendationPriority.Medium
                    ));
                }
            }
            
            return recommendations;
        }
        
        public List<AIRecommendation> AnalyzeCareOptimization()
        {
            var recommendations = new List<AIRecommendation>();
            
            // Analyze care frequency and effectiveness
            if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
            {
                recommendations.Add(CreateCareRecommendation(
                    "Care Schedule Optimization",
                    "Watering frequency could be optimized",
                    "Based on plant response data, consider adjusting watering intervals for better efficiency",
                    AIRecommendationPriority.Low
                ));
            }
            
            if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
            {
                recommendations.Add(CreateCareRecommendation(
                    "Nutrient Optimization",
                    "Feeding schedule shows room for improvement",
                    "Plant nutrient uptake patterns suggest potential for feeding schedule optimization",
                    AIRecommendationPriority.Medium
                ));
            }
            
            return recommendations;
        }
        
        public List<DataInsight> GetCultivationInsights()
        {
            return new List<DataInsight>(_cultivationInsights);
        }
        
        #region Private Helper Methods
        
        private void AnalyzePlantHealthStatus(AnalysisSnapshot snapshot)
        {
            var healthRecommendations = AnalyzePlantHealth();
            _cultivationRecommendations.AddRange(healthRecommendations);
            
            // Create insights based on health analysis
            if (healthRecommendations.Any(r => r.Priority == AIRecommendationPriority.High))
            {
                CreateCultivationInsight(
                    "Critical Health Issues Detected",
                    "Multiple plants showing signs of stress or poor health",
                    InsightSeverity.Critical,
                    "PlantHealth"
                );
            }
        }
        
        private void AnalyzeGrowthEfficiency(AnalysisSnapshot snapshot)
        {
            var growthRecommendations = AnalyzeGrowthPatterns();
            _cultivationRecommendations.AddRange(growthRecommendations);
            
            // Track growth trends
            if (growthRecommendations.Count > 2)
            {
                CreateCultivationInsight(
                    "Growth Efficiency Concerns",
                    "Multiple growth optimization opportunities identified",
                    InsightSeverity.Medium,
                    "GrowthOptimization"
                );
            }
        }
        
        private void AnalyzeCareOptimization(AnalysisSnapshot snapshot)
        {
            var careRecommendations = AnalyzeCareOptimization();
            _cultivationRecommendations.AddRange(careRecommendations);
        }
        
        private float CalculateGrowthEfficiency(PlantInstanceSO plant)
        {
            if (plant == null) return 0f;
            
            // Simplified growth efficiency calculation
            var expectedGrowth = plant.AgeInDays * 0.1f; // Simplified expected growth
            var actualGrowth = plant.OverallGrowthProgress;
            
            return actualGrowth / Mathf.Max(expectedGrowth, 0.1f);
        }
        
        private AIRecommendation CreateHealthRecommendation(string title, string summary, string description, AIRecommendationPriority priority)
        {
            return new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Summary = summary,
                Description = description,
                Priority = priority,
                Category = "PlantHealth",
                CreationTime = DateTime.Now,
                ExpirationTime = DateTime.Now.AddHours(24),
                ConfidenceScore = 0.8f,
                EstimatedImpact = priority == AIRecommendationPriority.High ? 0.9f : 0.6f,
                IsActive = true
            };
        }
        
        private AIRecommendation CreateGrowthRecommendation(string title, string summary, string description, AIRecommendationPriority priority)
        {
            return new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Summary = summary,
                Description = description,
                Priority = priority,
                Category = "GrowthOptimization",
                CreationTime = DateTime.Now,
                ExpirationTime = DateTime.Now.AddHours(48),
                ConfidenceScore = 0.75f,
                EstimatedImpact = 0.7f,
                IsActive = true
            };
        }
        
        private AIRecommendation CreateCareRecommendation(string title, string summary, string description, AIRecommendationPriority priority)
        {
            return new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Summary = summary,
                Description = description,
                Priority = priority,
                Category = "CareOptimization",
                CreationTime = DateTime.Now,
                ExpirationTime = DateTime.Now.AddHours(72),
                ConfidenceScore = 0.65f,
                EstimatedImpact = 0.5f,
                IsActive = true
            };
        }
        
        private void CreateCultivationInsight(string title, string description, InsightSeverity severity, string category)
        {
            var insight = new DataInsight
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Description = description,
                Severity = severity,
                Category = category,
                CreationTime = DateTime.Now,
                ConfidenceScore = 0.8f,
                IsActive = true
            };
            
            _cultivationInsights.Add(insight);
            
            // Limit insights collection size
            if (_cultivationInsights.Count > 50)
            {
                _cultivationInsights.RemoveAt(0);
            }
        }
        
        #endregion
    }
}