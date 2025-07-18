using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.AI;
using ProjectChimera.Data.Environment;
using ProjectChimera.Systems.Environment;
using AIRecommendationPriority = ProjectChimera.Data.AI.RecommendationPriority;
using AIOptimizationComplexity = ProjectChimera.Data.AI.OptimizationComplexity;

namespace ProjectChimera.Systems.AI
{
    /// <summary>
    /// PC014-1b-4: Specialized service for environmental AI analysis
    /// Extracted from monolithic AIAdvisorManager.cs to handle environmental
    /// optimization, climate control, and HVAC efficiency analysis.
    /// </summary>
    public class EnvironmentalAnalysisService : IEnvironmentalAnalysisService
    {
        private bool _isInitialized = false;
        private EnvironmentalManager _environmentalManager;
        private List<DataInsight> _environmentalInsights = new List<DataInsight>();
        private List<AIRecommendation> _environmentalRecommendations = new List<AIRecommendation>();
        private List<OptimizationOpportunity> _environmentalOptimizations = new List<OptimizationOpportunity>();
        
        // Analysis thresholds
        private float _efficiencyThreshold = 0.85f;
        private float _energyWasteThreshold = 1200f;
        private float _temperatureVarianceThreshold = 2f;
        private float _humidityVarianceThreshold = 5f;
        
        // Environmental data tracking
        private Queue<EnvironmentalSnapshot> _environmentalHistory = new Queue<EnvironmentalSnapshot>();
        private DateTime _lastOptimizationCheck;
        
        public bool IsInitialized => _isInitialized;
        
        public void Initialize()
        {
            if (_isInitialized) return;
            
            _environmentalManager = GameManager.Instance?.GetManager<EnvironmentalManager>();
            if (_environmentalManager == null)
            {
                Debug.LogWarning("[EnvironmentalAnalysisService] EnvironmentalManager not found - using simulated data");
            }
            
            _lastOptimizationCheck = DateTime.Now;
            
            _isInitialized = true;
            Debug.Log("[EnvironmentalAnalysisService] Initialized successfully");
        }
        
        public void Shutdown()
        {
            _environmentalInsights.Clear();
            _environmentalRecommendations.Clear();
            _environmentalOptimizations.Clear();
            _environmentalHistory.Clear();
            _isInitialized = false;
            Debug.Log("[EnvironmentalAnalysisService] Shutdown complete");
        }
        
        public void PerformAnalysis(AnalysisSnapshot snapshot)
        {
            if (!_isInitialized) return;
            
            // Clear previous analysis results
            _environmentalRecommendations.Clear();
            
            // Store environmental history
            if (snapshot?.EnvironmentalData != null)
            {
                _environmentalHistory.Enqueue(snapshot.EnvironmentalData);
                if (_environmentalHistory.Count > 100)
                    _environmentalHistory.Dequeue();
            }
            
            // Perform environmental-specific analysis
            AnalyzeHVACEfficiency(snapshot);
            AnalyzeEnergyConsumption(snapshot);
            AnalyzeClimateStability(snapshot);
            AnalyzeLightingOptimization(snapshot);
            
            Debug.Log($"[EnvironmentalAnalysisService] Analysis complete - {_environmentalRecommendations.Count} recommendations generated");
        }
        
        public List<AIRecommendation> AnalyzeEnvironmentalOptimization()
        {
            var recommendations = new List<AIRecommendation>();
            
            if (_environmentalHistory.Count > 5)
            {
                var recent = _environmentalHistory.TakeLast(5).ToList();
                var avgEfficiency = recent.Average(e => e.HVACEfficiency);
                var avgEnergy = recent.Average(e => e.EnergyUsage);
                
                if (avgEfficiency < _efficiencyThreshold * 100f)
                {
                    recommendations.Add(CreateEnvironmentalRecommendation(
                        "HVAC Efficiency Alert",
                        $"HVAC efficiency below optimal ({avgEfficiency:F1}%)",
                        "Consider maintenance or calibration to improve HVAC system efficiency",
                        AIRecommendationPriority.Medium
                    ));
                }
                
                if (avgEnergy > _energyWasteThreshold)
                {
                    recommendations.Add(CreateEnvironmentalRecommendation(
                        "High Energy Consumption",
                        $"Energy usage above normal ({avgEnergy:F0} units)",
                        "Review environmental settings for potential energy savings",
                        AIRecommendationPriority.High
                    ));
                }
            }
            else
            {
                // Simulated analysis when insufficient data
                if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
                {
                    recommendations.Add(CreateEnvironmentalRecommendation(
                        "Environmental Optimization Opportunity",
                        "Potential improvements in environmental efficiency detected",
                        "System analysis suggests optimization opportunities in climate control",
                        AIRecommendationPriority.Medium
                    ));
                }
            }
            
            return recommendations;
        }
        
        public List<AIRecommendation> AnalyzeClimateControl()
        {
            var recommendations = new List<AIRecommendation>();
            
            if (_environmentalHistory.Count >= 10)
            {
                var recent = _environmentalHistory.TakeLast(10).ToList();
                
                // Analyze temperature stability
                var temperatures = recent.Select(e => GetSimulatedTemperature()).ToList();
                var tempVariance = CalculateVariance(temperatures);
                
                if (tempVariance > _temperatureVarianceThreshold)
                {
                    recommendations.Add(CreateClimateRecommendation(
                        "Temperature Instability",
                        $"Temperature variance high ({tempVariance:F1}°C)",
                        "Consider adjusting HVAC controls for better temperature stability",
                        AIRecommendationPriority.Medium
                    ));
                }
                
                // Analyze humidity stability
                var humidities = recent.Select(e => GetSimulatedHumidity()).ToList();
                var humidityVariance = CalculateVariance(humidities);
                
                if (humidityVariance > _humidityVarianceThreshold)
                {
                    recommendations.Add(CreateClimateRecommendation(
                        "Humidity Fluctuation",
                        $"Humidity variance above optimal ({humidityVariance:F1}%)",
                        "Review humidity controls for improved climate stability",
                        AIRecommendationPriority.Medium
                    ));
                }
            }
            else
            {
                // Simulated climate analysis
                if (UnityEngine.Random.Range(0f, 1f) < 0.4f)
                {
                    recommendations.Add(CreateClimateRecommendation(
                        "Climate Control Optimization",
                        "Climate patterns could be optimized",
                        "Environmental data suggests potential for improved climate control efficiency",
                        AIRecommendationPriority.Low
                    ));
                }
            }
            
            return recommendations;
        }
        
        public List<OptimizationOpportunity> GetEnvironmentalOptimizations()
        {
            return new List<OptimizationOpportunity>(_environmentalOptimizations);
        }
        
        public List<DataInsight> GetEnvironmentalInsights()
        {
            return new List<DataInsight>(_environmentalInsights);
        }
        
        #region Private Analysis Methods
        
        private void AnalyzeHVACEfficiency(AnalysisSnapshot snapshot)
        {
            if (snapshot?.EnvironmentalData != null)
            {
                var efficiency = snapshot.EnvironmentalData.HVACEfficiency;
                
                if (efficiency < _efficiencyThreshold * 100f)
                {
                    CreateEnvironmentalOptimization(
                        "HVAC Efficiency Improvement",
                        $"HVAC system operating at {efficiency:F1}% efficiency",
                        "Implement preventive maintenance schedule and control calibration",
                        AIOptimizationComplexity.Medium,
                        0.8f
                    );
                }
            }
        }
        
        private void AnalyzeEnergyConsumption(AnalysisSnapshot snapshot)
        {
            if (snapshot?.EnvironmentalData != null)
            {
                var energyUsage = snapshot.EnvironmentalData.EnergyUsage;
                
                if (energyUsage > _energyWasteThreshold)
                {
                    CreateEnvironmentalOptimization(
                        "Energy Consumption Reduction",
                        $"Energy usage at {energyUsage:F0} units - above optimal range",
                        "Implement smart scheduling and efficiency controls",
                        AIOptimizationComplexity.High,
                        0.9f
                    );
                }
            }
        }
        
        private void AnalyzeClimateStability(AnalysisSnapshot snapshot)
        {
            var climateRecommendations = AnalyzeClimateControl();
            _environmentalRecommendations.AddRange(climateRecommendations);
            
            if (climateRecommendations.Count >= 2)
            {
                CreateEnvironmentalInsight(
                    "Climate Stability Issues",
                    "Multiple climate control issues detected requiring attention",
                    InsightSeverity.Medium,
                    "ClimateControl"
                );
            }
        }
        
        private void AnalyzeLightingOptimization(AnalysisSnapshot snapshot)
        {
            if (snapshot?.EnvironmentalData != null)
            {
                var lightingEfficiency = snapshot.EnvironmentalData.LightingEfficiency;
                var dliOptimization = snapshot.EnvironmentalData.DLIOptimization;
                
                if (lightingEfficiency < 85f || dliOptimization < 85f)
                {
                    CreateEnvironmentalOptimization(
                        "Lighting System Optimization",
                        $"Lighting efficiency: {lightingEfficiency:F1}%, DLI optimization: {dliOptimization:F1}%",
                        "Optimize lighting schedules and intensity for better efficiency",
                        AIOptimizationComplexity.Medium,
                        0.7f
                    );
                }
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private float GetSimulatedTemperature()
        {
            return UnityEngine.Random.Range(22f, 28f);
        }
        
        private float GetSimulatedHumidity()
        {
            return UnityEngine.Random.Range(45f, 65f);
        }
        
        private float CalculateVariance(List<float> values)
        {
            if (values.Count < 2) return 0f;
            
            var mean = values.Average();
            var sumSquaredDifferences = values.Sum(v => Mathf.Pow(v - mean, 2));
            return Mathf.Sqrt(sumSquaredDifferences / values.Count);
        }
        
        private AIRecommendation CreateEnvironmentalRecommendation(string title, string summary, string description, AIRecommendationPriority priority)
        {
            return new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Summary = summary,
                Description = description,
                Priority = priority,
                Category = "EnvironmentalOptimization",
                CreationTime = DateTime.Now,
                ExpirationTime = DateTime.Now.AddHours(36),
                ConfidenceScore = 0.8f,
                EstimatedImpact = priority == AIRecommendationPriority.High ? 0.85f : 0.65f,
                IsActive = true
            };
        }
        
        private AIRecommendation CreateClimateRecommendation(string title, string summary, string description, AIRecommendationPriority priority)
        {
            return new AIRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Summary = summary,
                Description = description,
                Priority = priority,
                Category = "ClimateControl",
                CreationTime = DateTime.Now,
                ExpirationTime = DateTime.Now.AddHours(24),
                ConfidenceScore = 0.75f,
                EstimatedImpact = 0.7f,
                IsActive = true
            };
        }
        
        private void CreateEnvironmentalOptimization(string title, string description, string implementation, AIOptimizationComplexity complexity, float potentialImpact)
        {
            var optimization = new OptimizationOpportunity
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Description = description,
                ImplementationDetails = implementation,
                Complexity = complexity,
                PotentialImpact = potentialImpact,
                EstimatedROI = potentialImpact * 0.8f,
                Category = "Environmental",
                CreationTime = DateTime.Now,
                IsActive = true
            };
            
            _environmentalOptimizations.Add(optimization);
            
            // Limit optimizations collection size
            if (_environmentalOptimizations.Count > 20)
            {
                _environmentalOptimizations.RemoveAt(0);
            }
        }
        
        private void CreateEnvironmentalInsight(string title, string description, InsightSeverity severity, string category)
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
            
            _environmentalInsights.Add(insight);
            
            // Limit insights collection size
            if (_environmentalInsights.Count > 40)
            {
                _environmentalInsights.RemoveAt(0);
            }
        }
        
        #endregion
    }
}