using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using System.Collections.Generic;
using System.Linq;
using System;
using TraitType = ProjectChimera.Data.Genetics.TraitType;
using TraitGap = ProjectChimera.Systems.Genetics.TraitGap;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Phase 3.3: Advanced breeding goal management system.
    /// Handles breeding objectives, strategy optimization, and progress tracking.
    /// </summary>
    public class BreedingGoalManager : ChimeraManager
    {
        [Header("Goal Management Settings")]
        [SerializeField] private int _maxActiveGoals = 5;
        [SerializeField] private float _goalProgressUpdateInterval = 1.0f;
        [SerializeField] private bool _enableGoalSuggestions = true;
        [SerializeField] private bool _enableAdaptiveStrategies = true;
        
        [Header("Strategy Optimization")]
        [SerializeField] private float _strategyEvaluationInterval = 5.0f;
        [SerializeField] private int _generationLookAhead = 3;
        [SerializeField] private float _successThreshold = 0.8f;
        
        // Active goals and strategies
        private Dictionary<string, BreedingGoal> _activeGoals = new Dictionary<string, BreedingGoal>();
        private Dictionary<string, BreedingStrategyData> _strategies = new Dictionary<string, BreedingStrategyData>();
        private List<BreedingGoalTemplate> _goalTemplates = new List<BreedingGoalTemplate>();
        
        // Progress tracking
        private Dictionary<string, BreedingProgress> _goalProgress = new Dictionary<string, BreedingProgress>();
        private Dictionary<string, List<BreedingAttempt>> _breedingHistory = new Dictionary<string, List<BreedingAttempt>>();
        
        // Performance monitoring
        private float _lastGoalUpdate;
        private float _lastStrategyEvaluation;
        
        // Dependencies
        private ProjectChimera.Systems.Genetics.BreedingSimulator _breedingSimulator;
        private ProjectChimera.Systems.Genetics.TraitExpressionEngine _traitEngine;
        
        // Events
        public event Action<BreedingGoal> OnGoalAdded;
        public event Action<BreedingGoal> OnGoalCompleted;
        public event Action<BreedingGoal> OnGoalProgress;
        public event Action<BreedingStrategyData> OnStrategyOptimized;
        
        public override ManagerPriority Priority => ManagerPriority.Normal;
        
        protected override void OnManagerInitialize()
        {
            LoadGoalTemplates();
            InitializeDefaultStrategies();
            
            // Initialize dependencies directly instead of FindObjectOfType
            _breedingSimulator = new ProjectChimera.Systems.Genetics.BreedingSimulator(true, 0.1f);
            _traitEngine = new ProjectChimera.Systems.Genetics.TraitExpressionEngine(true, true);
            
            _lastGoalUpdate = Time.time;
            _lastStrategyEvaluation = Time.time;
            
            Debug.Log("BreedingGoalManager initialized with advanced strategy optimization");
        }
        
        protected override void OnManagerUpdate()
        {
            // Update goal progress
            if (Time.time - _lastGoalUpdate >= _goalProgressUpdateInterval)
            {
                UpdateGoalProgress();
                _lastGoalUpdate = Time.time;
            }
            
            // Evaluate and optimize strategies
            if (Time.time - _lastStrategyEvaluation >= _strategyEvaluationInterval)
            {
                EvaluateStrategies();
                _lastStrategyEvaluation = Time.time;
            }
        }
        
        /// <summary>
        /// Create a new breeding goal with specific objectives.
        /// </summary>
        public string CreateBreedingGoal(BreedingGoalConfiguration config)
        {
            var goalId = Guid.NewGuid().ToString();
            var goal = new BreedingGoal
            {
                GoalId = goalId,
                GoalName = config.GoalName,
                Description = config.Description,
                TargetTraits = config.TargetTraits,
                Priority = config.Priority,
                Deadline = config.Deadline,
                CreatedAt = DateTime.Now,
                Status = BreedingGoalStatus.Active
            };
            
            _activeGoals[goalId] = goal;
            _goalProgress[goalId] = new BreedingProgress();
            _breedingHistory[goalId] = new List<BreedingAttempt>();
            
            // Generate optimal strategy for this goal
            var strategy = GenerateOptimalStrategy(goal);
            _strategies[goalId] = strategy;
            
            OnGoalAdded?.Invoke(goal);
            Debug.Log($"Created breeding goal: {goal.GoalName} ({goalId})");
            
            return goalId;
        }
        
        /// <summary>
        /// Add a breeding goal from a predefined template.
        /// </summary>
        public string CreateGoalFromTemplate(string templateId, Dictionary<string, object> parameters = null)
        {
            var template = _goalTemplates.FirstOrDefault(t => t.TemplateId == templateId);
            if (template == null)
            {
                Debug.LogWarning($"[BreedingGoalManager] Breeding goal template not found: {templateId}");
                return null;
            }
            
            var maxGoals = parameters.ContainsKey("maxGoals") ? (int)parameters["maxGoals"] : 5;
            var config = template.CreateConfiguration(maxGoals);
            return CreateBreedingGoal(config);
        }
        
        /// <summary>
        /// Evaluate a potential breeding cross against active goals.
        /// </summary>
        public BreedingEvaluation EvaluateBreedingCross(PlantStrainSO parent1, PlantStrainSO parent2)
        {
            var evaluation = new BreedingEvaluation
            {
                Parent1 = parent1,
                Parent2 = parent2,
                EvaluatedAt = (float)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds,
                GoalScores = new Dictionary<string, float>()
            };
            
            foreach (var goal in _activeGoals.Values)
            {
                var score = CalculateGoalScore(goal, parent1, parent2);
                evaluation.GoalScores[goal.GoalId] = score;
            }
            
            // Calculate overall score
            evaluation.OverallScore = evaluation.GoalScores.Count > 0 ? evaluation.GoalScores.Values.ToList().Average() : 0f;
            
            // Get strategy recommendations
            evaluation.StrategyRecommendations = GetStrategyRecommendations(evaluation);
            
            return evaluation;
        }
        
        /// <summary>
        /// Record the result of a breeding attempt.
        /// </summary>
        public void RecordBreedingAttempt(string goalId, BreedingAttempt attempt)
        {
            if (!_breedingHistory.ContainsKey(goalId))
                _breedingHistory[goalId] = new List<BreedingAttempt>();
            
            _breedingHistory[goalId].Add(attempt);
            
            // Update goal progress
            UpdateGoalProgress(goalId, attempt);
            
            // Learn from the attempt to improve strategies
            if (_enableAdaptiveStrategies)
            {
                AdaptStrategy(goalId, attempt);
            }
        }
        
        /// <summary>
        /// Get suggestions for new breeding goals based on current genetic diversity.
        /// </summary>
        public List<BreedingGoalSuggestion> GetGoalSuggestions(List<PlantStrainSO> availableStrains)
        {
            var suggestions = new List<BreedingGoalSuggestion>();
            
            if (!_enableGoalSuggestions || availableStrains.Count < 2)
                return suggestions;
            
            // Analyze genetic diversity
            var diversityAnalysis = AnalyzeGeneticDiversity(availableStrains);
            
            // Suggest goals based on gaps in trait coverage
            foreach (var gap in diversityAnalysis.TraitGaps)
            {
                var suggestion = new BreedingGoalSuggestion
                {
                    SuggestionId = Guid.NewGuid().ToString(),
                    GoalName = $"Develop {gap.TraitName} Variety",
                    Description = $"Create strains with enhanced {gap.TraitName} expression",
                    Priority = (int)CalculateSuggestionPriority(gap),
                    EstimatedGenerations = EstimateGenerationsRequired(gap),
                    SuccessProbability = EstimateSuccessProbability(gap, availableStrains)
                };
                
                suggestions.Add(suggestion);
            }
            
            // Suggest hybrid vigor opportunities
            var hybridOpportunities = FindHybridVigorOpportunities(availableStrains);
            foreach (var opportunity in hybridOpportunities)
            {
                var suggestion = new BreedingGoalSuggestion
                {
                    SuggestionId = Guid.NewGuid().ToString(),
                    GoalName = $"Hybrid Vigor: {opportunity.Parent1Name} × {opportunity.Parent2Name}",
                    Description = $"Exploit heterosis between genetically distant strains",
                    Priority = (int)BreedingPriorityLevel.High,
                    EstimatedGenerations = 1,
                    SuccessProbability = opportunity.ExpectedHeterosis
                };
                
                suggestions.Add(suggestion);
            }
            
            return suggestions.OrderByDescending(s => s.Priority).ThenByDescending(s => s.SuccessProbability).ToList();
        }
        
        /// <summary>
        /// Get the optimal breeding strategy for a specific goal.
        /// </summary>
        public BreedingStrategyData? GetOptimalStrategy(string goalId)
        {
            return _strategies.TryGetValue(goalId, out var strategy) ? strategy : null;
        }
        
        /// <summary>
        /// Update the progress of all active goals.
        /// </summary>
        private void UpdateGoalProgress()
        {
            foreach (var goalId in _activeGoals.Keys.ToList())
            {
                UpdateGoalProgress(goalId);
            }
        }
        
        /// <summary>
        /// Update progress for a specific goal.
        /// </summary>
        private void UpdateGoalProgress(string goalId, BreedingAttempt latestAttempt = null)
        {
            if (!_activeGoals.TryGetValue(goalId, out var goal) || 
                !_goalProgress.TryGetValue(goalId, out var progress))
                return;
            
            var attempts = _breedingHistory[goalId];
            
            // Calculate progress metrics
            progress.TotalAttempts = attempts.Count;
            progress.SuccessfulAttempts = attempts.Count(a => a.Success);
            progress.SuccessRate = progress.TotalAttempts > 0 ? (float)progress.SuccessfulAttempts / progress.TotalAttempts : 0f;
            
            // Calculate trait progress
            var traitProgress = new Dictionary<TraitType, float>();
            foreach (var targetTrait in goal.TargetTraits)
            {
                var bestValue = attempts
                    .Where(a => a.OffspringTraits.ContainsKey(targetTrait))
                    .Select(a => a.OffspringTraits[targetTrait])
                    .DefaultIfEmpty(0f)
                    .Max();
                
                // Use a default target value since BreedingGoal doesn't store target values per trait
                var targetValue = 1.0f; // Default target value for normalized progress
                var progressValue = bestValue / targetValue;
                traitProgress[targetTrait] = Mathf.Clamp01(progressValue);
            }
            
            progress.TraitProgress = traitProgress;
            progress.OverallProgress = traitProgress.Values.Average();
            progress.LastUpdated = DateTime.Now;
            
            // Check for goal completion
            if (progress.OverallProgress >= _successThreshold && goal.Status == BreedingGoalStatus.Active)
            {
                goal.Status = BreedingGoalStatus.Completed;
                goal.CompletedAt = DateTime.Now;
                OnGoalCompleted?.Invoke(goal);
                Debug.Log($"Breeding goal completed: {goal.GoalName}");
            }
            else
            {
                OnGoalProgress?.Invoke(goal);
            }
        }
        
        /// <summary>
        /// Calculate how well a breeding cross aligns with a specific goal.
        /// </summary>
        private float CalculateGoalScore(BreedingGoal goal, PlantStrainSO parent1, PlantStrainSO parent2)
        {
            if (_breedingSimulator == null || _traitEngine == null)
                return 0f;
            
            // Simulate offspring traits
            var compatibility = _breedingSimulator.AnalyzeBreedingCompatibility(parent1, parent2);
            
            float totalScore = 0f;
            int traitCount = 0;
            
            foreach (var targetTrait in goal.TargetTraits)
            {
                // Predict trait expression for offspring
                var predictedValue = PredictTraitValue(targetTrait, parent1, parent2);
                
                // Calculate how close to target (using default target value)
                var targetValue = 1.0f; // Default target value since BreedingGoal doesn't store individual trait targets
                var score = 1f - Mathf.Abs(predictedValue - targetValue) / Mathf.Max(targetValue, 1f);
                
                // Weight by trait importance (using default weight)
                var weight = 1.0f; // Default weight since BreedingGoal doesn't store trait weights
                totalScore += score * weight;
                traitCount++;
            }
            
            var baseScore = traitCount > 0 ? totalScore / traitCount : 0f;
            
            // Apply modifiers
            var diversityModifier = 1f + (compatibility.GeneticDistance * 0.2f); // Reward genetic diversity
            var vigorModifier = 1f + compatibility.ExpectedHeterosis; // Reward hybrid vigor
            
            return Mathf.Clamp01(baseScore * diversityModifier * vigorModifier);
        }
        
        /// <summary>
        /// Predict the value of a specific trait for offspring.
        /// </summary>
        private float PredictTraitValue(TraitType trait, PlantStrainSO parent1, PlantStrainSO parent2)
        {
            // This is a simplified prediction - in a full implementation,
            // you would use the trait expression engine with simulated offspring genotypes
            
            var parent1Value = GetStrainTraitValue(parent1, trait);
            var parent2Value = GetStrainTraitValue(parent2, trait);
            
            // Simple average with some random variation
            var average = (parent1Value + parent2Value) / 2f;
            var variation = UnityEngine.Random.Range(-0.1f, 0.1f);
            
            return Mathf.Clamp01(average + variation);
        }
        
        /// <summary>
        /// Get trait value from a plant strain.
        /// </summary>
        private float GetStrainTraitValue(PlantStrainSO strain, TraitType trait)
        {
            // Map trait to strain properties
            switch (trait)
            {
                case TraitType.THCContent:
                    return strain.THCContent() / 35f; // Normalize to 0-1
                case TraitType.CBDContent:
                    return strain.CBDContent() / 25f; // Normalize to 0-1
                case TraitType.YieldPotential:
                    return strain.BaseYield() / 1000f; // Normalize to 0-1 (assuming max ~1000g)
                case TraitType.PlantHeight:
                    return strain.BaseHeight / 3f; // Normalize to 0-1 (assuming max ~3m)
                default:
                    return 0.5f;
            }
        }
        
        /// <summary>
        /// Generate optimal breeding strategy for a goal.
        /// </summary>
        private BreedingStrategyData GenerateOptimalStrategy(BreedingGoal goal)
        {
            var strategy = new BreedingStrategyData
            {
                StrategyId = Guid.NewGuid().ToString(),
                GoalId = goal.GoalId,
                StrategyName = $"Optimal Strategy for {goal.GoalName}",
                CreatedAt = DateTime.Now
            };
            
            // Determine strategy type based on goal characteristics
            if (goal.TargetTraits.Count == 1)
            {
                strategy.StrategyType = BreedingStrategyType.SingleTrait;
                strategy.RecommendedCrosses = GenerateSingleTraitCrosses(goal.TargetTraits[0]);
            }
            else if (goal.TargetTraits.Count <= 3) // Multi-trait balanced for small number of traits
            {
                strategy.StrategyType = BreedingStrategyType.MultiTraitBalanced;
                strategy.RecommendedCrosses = GenerateBalancedCrosses(goal.TargetTraits);
            }
            else
            {
                strategy.StrategyType = BreedingStrategyType.HybridVigor;
                strategy.RecommendedCrosses = GenerateHybridVigorCrosses(goal.TargetTraits);
            }
            
            // Set general strategy parameters
            strategy.GenerationsPlanned = _generationLookAhead;
            strategy.MutationRate = 0.02f; // Slightly elevated for goal-directed breeding
            strategy.SelectionPressure = 0.8f; // High selection pressure
            
            return strategy;
        }
        
        /// <summary>
        /// Generate recommended crosses for single trait optimization.
        /// </summary>
        private List<RecommendedCross> GenerateSingleTraitCrosses(TraitType targetTrait)
        {
            // Placeholder implementation - would use actual strain database
            return new List<RecommendedCross>
            {
                new RecommendedCross
                {
                    CrossName = $"High {targetTrait} Cross",
                    EstimatedSuccessRate = 0.7f,
                    GenerationsRequired = 2,
                    Notes = $"Optimized for maximum {targetTrait} expression"
                }
            };
        }
        
        /// <summary>
        /// Generate recommended crosses for balanced multi-trait optimization.
        /// </summary>
        private List<RecommendedCross> GenerateBalancedCrosses(List<TraitType> targetTraits)
        {
            // Placeholder implementation
            return new List<RecommendedCross>
            {
                new RecommendedCross
                {
                    CrossName = "Balanced Multi-Trait Cross",
                    EstimatedSuccessRate = 0.6f,
                    GenerationsRequired = 3,
                    Notes = "Optimized for balanced expression across all target traits"
                }
            };
        }
        
        /// <summary>
        /// Generate recommended crosses for hybrid vigor exploitation.
        /// </summary>
        private List<RecommendedCross> GenerateHybridVigorCrosses(List<TraitType> targetTraits)
        {
            // Placeholder implementation
            return new List<RecommendedCross>
            {
                new RecommendedCross
                {
                    CrossName = "Hybrid Vigor Cross",
                    EstimatedSuccessRate = 0.8f,
                    GenerationsRequired = 1,
                    Notes = "Exploits heterosis between genetically distant parents"
                }
            };
        }
        
        /// <summary>
        /// Evaluate and optimize existing strategies.
        /// </summary>
        private void EvaluateStrategies()
        {
            foreach (var strategy in _strategies.Values.ToList())
            {
                if (_breedingHistory.TryGetValue(strategy.GoalId, out var attempts))
                {
                    var recentAttempts = attempts.Where(a => a.AttemptTime > DateTime.Now.AddDays(-7)).ToList();
                    
                    if (recentAttempts.Count >= 3)
                    {
                        var successRate = recentAttempts.Count(a => a.Success) / (float)recentAttempts.Count;
                        
                        if (successRate < 0.3f) // Strategy is underperforming
                        {
                            OptimizeStrategy(strategy);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Optimize an underperforming strategy.
        /// </summary>
        private void OptimizeStrategy(BreedingStrategyData strategy)
        {
            // Adjust strategy parameters based on historical performance
            strategy.MutationRate = Mathf.Clamp(strategy.MutationRate * 1.2f, 0.01f, 0.1f);
            strategy.SelectionPressure = Mathf.Clamp(strategy.SelectionPressure * 0.9f, 0.5f, 1.0f);
            
            strategy.LastOptimized = DateTime.Now;
            strategy.OptimizationCount++;
            
            OnStrategyOptimized?.Invoke(strategy);
            Debug.Log($"Optimized breeding strategy: {strategy.StrategyName}");
        }
        
        /// <summary>
        /// Adapt strategy based on breeding attempt results.
        /// </summary>
        private void AdaptStrategy(string goalId, BreedingAttempt attempt)
        {
            if (!_strategies.TryGetValue(goalId, out var strategy))
                return;
            
            // Learn from successful attempts
            if (attempt.Success)
            {
                // Reinforce successful strategy elements
                strategy.SelectionPressure = Mathf.Min(strategy.SelectionPressure * 1.05f, 1.0f);
            }
            else
            {
                // Adapt unsuccessful strategy
                strategy.MutationRate = Mathf.Clamp(strategy.MutationRate * 1.1f, 0.01f, 0.1f);
            }
        }
        
        /// <summary>
        /// Load predefined breeding goal templates.
        /// </summary>
        private void LoadGoalTemplates()
        {
            _goalTemplates.Add(new BreedingGoalTemplate
            {
                TemplateId = "maximize-thc",
                TemplateName = "Maximize THC",
                Description = "Create strains with maximum THC content",
                DefaultTargetTraits = new List<TargetTrait>
                {
                    new TargetTrait { Trait = TraitType.THCContent, TargetValue = 0.9f, Weight = 1.0f }
                }
            });
            
            _goalTemplates.Add(new BreedingGoalTemplate
            {
                TemplateId = "cbd-focus",
                TemplateName = "CBD Focus",
                Description = "Develop high-CBD medicinal strains",
                DefaultTargetTraits = new List<TargetTrait>
                {
                    new TargetTrait { Trait = TraitType.CBDContent, TargetValue = 0.8f, Weight = 1.0f },
                    new TargetTrait { Trait = TraitType.THCContent, TargetValue = 0.1f, Weight = 0.5f }
                }
            });
            
            _goalTemplates.Add(new BreedingGoalTemplate
            {
                TemplateId = "balanced-hybrid",
                TemplateName = "Balanced Hybrid",
                Description = "Create well-balanced hybrid strains",
                DefaultTargetTraits = new List<TargetTrait>
                {
                    new TargetTrait { Trait = TraitType.THCContent, TargetValue = 0.6f, Weight = 0.8f },
                    new TargetTrait { Trait = TraitType.CBDContent, TargetValue = 0.4f, Weight = 0.8f },
                    new TargetTrait { Trait = TraitType.YieldPotential, TargetValue = 0.8f, Weight = 0.6f }
                }
            });
            
            Debug.Log($"Loaded {_goalTemplates.Count} breeding goal templates");
        }
        
        /// <summary>
        /// Initialize default breeding strategies.
        /// </summary>
        private void InitializeDefaultStrategies()
        {
            // Default strategies are created when goals are added
            Debug.Log("Default breeding strategies initialized");
        }
        
        /// <summary>
        /// Analyze genetic diversity in available strains.
        /// </summary>
        private GeneticDiversityAnalysis AnalyzeGeneticDiversity(List<PlantStrainSO> strains)
        {
            var analysis = new GeneticDiversityAnalysis
            {
                TotalStrains = strains.Count,
                TraitGaps = new List<TraitGap>(),
                DiversityScore = CalculateDiversityScore(strains)
            };
            
            // Identify trait gaps
            var traitCoverage = new Dictionary<TraitType, float>();
            foreach (TraitType trait in Enum.GetValues(typeof(TraitType)))
            {
                var values = strains.Select(s => GetStrainTraitValue(s, trait)).ToList();
                var coverage = values.Max() - values.Min();
                traitCoverage[trait] = coverage;
                
                if (coverage < 0.5f) // Gap in trait coverage
                {
                    analysis.TraitGaps.Add(new TraitGap
                    {
                        TraitName = trait.ToString(),
                        CurrentRange = coverage,
                        OptimalRange = 1.0f,
                        Priority = 1.0f - coverage
                    });
                }
            }
            
            return analysis;
        }
        
        /// <summary>
        /// Calculate overall genetic diversity score.
        /// </summary>
        private float CalculateDiversityScore(List<PlantStrainSO> strains)
        {
            if (strains.Count < 2)
                return 0f;
            
            // Calculate average genetic distance between all strain pairs
            float totalDistance = 0f;
            int pairs = 0;
            
            for (int i = 0; i < strains.Count; i++)
            {
                for (int j = i + 1; j < strains.Count; j++)
                {
                    var distance = CalculateGeneticDistance(strains[i], strains[j]);
                    totalDistance += distance;
                    pairs++;
                }
            }
            
            return pairs > 0 ? totalDistance / pairs : 0f;
        }
        
        /// <summary>
        /// Calculate genetic distance between two strains.
        /// </summary>
        private float CalculateGeneticDistance(PlantStrainSO strain1, PlantStrainSO strain2)
        {
            // Simplified genetic distance calculation
            var thcDiff = Mathf.Abs(strain1.THCContent() - strain2.THCContent()) / 35f;
            var cbdDiff = Mathf.Abs(strain1.CBDContent() - strain2.CBDContent()) / 25f;
            var yieldDiff = Mathf.Abs(strain1.BaseYield() - strain2.BaseYield()) / 1000f;
            var heightDiff = Mathf.Abs(strain1.BaseHeight - strain2.BaseHeight) / 3f;
            
            return (thcDiff + cbdDiff + yieldDiff + heightDiff) / 4f;
        }
        
        /// <summary>
        /// Find opportunities for hybrid vigor exploitation.
        /// </summary>
        private List<HybridVigorOpportunity> FindHybridVigorOpportunities(List<PlantStrainSO> strains)
        {
            var opportunities = new List<HybridVigorOpportunity>();
            
            for (int i = 0; i < strains.Count; i++)
            {
                for (int j = i + 1; j < strains.Count; j++)
                {
                    var distance = CalculateGeneticDistance(strains[i], strains[j]);
                    
                    if (distance > 0.6f) // High genetic distance suggests good hybrid vigor potential
                    {
                        opportunities.Add(new HybridVigorOpportunity
                        {
                            Parent1Name = strains[i].StrainName,
                            Parent2Name = strains[j].StrainName,
                            GeneticDistance = distance,
                            ExpectedHeterosis = distance * 0.8f // Simplified heterosis calculation
                        });
                    }
                }
            }
            
            return opportunities.OrderByDescending(o => o.ExpectedHeterosis).ToList();
        }
        
        /// <summary>
        /// Get strategy recommendations for a breeding evaluation.
        /// </summary>
        private List<string> GetStrategyRecommendations(BreedingEvaluation evaluation)
        {
            var recommendations = new List<string>();
            
            if (evaluation.OverallScore > 0.8f)
            {
                recommendations.Add("Excellent cross - proceed with breeding");
            }
            else if (evaluation.OverallScore > 0.6f)
            {
                recommendations.Add("Good potential - consider for breeding program");
            }
            else if (evaluation.OverallScore > 0.4f)
            {
                recommendations.Add("Moderate potential - may require multiple generations");
            }
            else
            {
                recommendations.Add("Low compatibility - consider alternative parents");
            }
            
            return recommendations;
        }
        
        /// <summary>
        /// Calculate priority for a breeding goal suggestion.
        /// </summary>
        private BreedingPriorityLevel CalculateSuggestionPriority(TraitGap gap)
        {
            if (gap.Priority > 0.8f)
                return BreedingPriorityLevel.Critical;
            else if (gap.Priority > 0.6f)
                return BreedingPriorityLevel.High;
            else if (gap.Priority > 0.4f)
                return BreedingPriorityLevel.Medium;
            else
                return BreedingPriorityLevel.Low;
        }
        
        /// <summary>
        /// Estimate generations required for a trait gap.
        /// </summary>
        private int EstimateGenerationsRequired(TraitGap gap)
        {
            // Simple estimation based on gap size
            if (gap.Priority > 0.8f)
                return 4; // Large gap requires more generations
            else if (gap.Priority > 0.6f)
                return 3;
            else if (gap.Priority > 0.4f)
                return 2;
            else
                return 1;
        }
        
        /// <summary>
        /// Estimate success probability for addressing a trait gap.
        /// </summary>
        private float EstimateSuccessProbability(TraitGap gap, List<PlantStrainSO> availableStrains)
        {
            // Base probability decreases with gap size
            var baseProbability = 1.0f - gap.Priority;
            
            // Increase probability with more diverse strain pool
            var diversityBonus = Mathf.Min(availableStrains.Count / 10f, 0.3f);
            
            return Mathf.Clamp01(baseProbability + diversityBonus);
        }
        
        /// <summary>
        /// Get all active breeding goals.
        /// </summary>
        public List<BreedingGoal> GetActiveGoals()
        {
            return _activeGoals.Values.Where(g => g.Status == BreedingGoalStatus.Active).ToList();
        }
        
        /// <summary>
        /// Get progress for a specific goal.
        /// </summary>
        public BreedingProgress GetGoalProgress(string goalId)
        {
            return _goalProgress.TryGetValue(goalId, out var progress) ? progress : null;
        }
        
        /// <summary>
        /// Get breeding history for a specific goal.
        /// </summary>
        public List<BreedingAttempt> GetBreedingHistory(string goalId)
        {
            return _breedingHistory.TryGetValue(goalId, out var history) ? history : new List<BreedingAttempt>();
        }
        
        protected override void OnManagerShutdown()
        {
            _activeGoals.Clear();
            _strategies.Clear();
            _goalProgress.Clear();
            _breedingHistory.Clear();
            
            Debug.Log("BreedingGoalManager shutdown complete");
        }
    }
}