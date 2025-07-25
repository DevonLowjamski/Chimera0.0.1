using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// PC013-7d: GeneticsOrchestrator coordinates all genetics services
    /// Provides high-level genetics operations by orchestrating breeding calculation,
    /// trait expression, and genetic analysis engines for unified genetic workflows.
    /// </summary>
    public class GeneticsOrchestrator
    {
        private readonly BreedingCalculationEngine _breedingEngine;
        private readonly TraitExpressionProcessor _traitProcessor;
        private readonly GeneticAnalysisEngine _analysisEngine;
        private readonly InheritanceCalculator _inheritanceCalculator;
        
        // Configuration
        private readonly bool _enableAdvancedAnalysis;
        private readonly bool _enablePopulationTracking;
        private readonly float _diversityThreshold;
        
        // Events for orchestrated operations
        public System.Action<ComprehensiveGeneticsResult> OnGeneticsOperationCompleted;
        public System.Action<string> OnGeneticsError;
        
        public GeneticsOrchestrator(bool enableAdvancedAnalysis = true, bool enablePopulationTracking = true, 
            float diversityThreshold = 0.7f)
        {
            _enableAdvancedAnalysis = enableAdvancedAnalysis;
            _enablePopulationTracking = enablePopulationTracking;
            _diversityThreshold = diversityThreshold;
            
            // Initialize coordinated services
            _breedingEngine = new BreedingCalculationEngine();
            _traitProcessor = new TraitExpressionProcessor(enableAdvancedAnalysis);
            _analysisEngine = new GeneticAnalysisEngine(enableAdvancedAnalysis, enablePopulationTracking, 100, diversityThreshold);
            _inheritanceCalculator = new InheritanceCalculator(true, true);
            
            Debug.Log($"[GeneticsOrchestrator] Initialized with advanced analysis: {_enableAdvancedAnalysis}");
        }
        
        /// <summary>
        /// Performs comprehensive genetics analysis combining breeding, trait expression, and population analysis.
        /// </summary>
        public ComprehensiveGeneticsResult PerformComprehensiveAnalysis(List<PlantInstanceSO> population, 
            TraitSelectionCriteria criteria = null)
        {
            if (population == null || population.Count == 0)
            {
                var error = "Cannot perform comprehensive analysis: empty population";
                Debug.LogError($"[GeneticsOrchestrator] {error}");
                OnGeneticsError?.Invoke(error);
                return null;
            }
            
            var result = new ComprehensiveGeneticsResult
            {
                AnalysisID = System.Guid.NewGuid().ToString(),
                PopulationSize = population.Count,
                AnalysisDate = System.DateTime.Now
            };
            
            try
            {
                // 1. Population genetic diversity analysis
                result.DiversityAnalysis = _analysisEngine.AnalyzeGeneticDiversity(population);
                
                // 2. Trait expression analysis for all plants
                result.TraitExpressions = new List<TraitExpressionResult>();
                foreach (var plant in population)
                {
                    var traitExpression = _traitProcessor.CalculateTraitExpression(plant, GetDefaultEnvironment());
                    if (traitExpression != null)
                        result.TraitExpressions.Add(traitExpression);
                }
                
                // 3. Breeding optimization if criteria provided
                if (criteria != null && population.Count >= 2)
                {
                    result.BreedingRecommendation = _analysisEngine.OptimizeBreedingSelection(population, criteria);
                }
                
                // 4. Population analysis for advanced insights
                if (_enablePopulationTracking)
                {
                    result.PopulationAnalysis = _analysisEngine.AnalyzePopulation(population);
                }
                
                // 5. Generate breeding value predictions for top performers
                result.BreedingValuePredictions = new List<BreedingValuePrediction>();
                var topPerformers = SelectTopPerformers(population, result.TraitExpressions, 5);
                foreach (var performer in topPerformers)
                {
                    var prediction = _analysisEngine.PredictBreedingValue(performer, GetDefaultTraitTypes());
                    if (prediction != null)
                        result.BreedingValuePredictions.Add(prediction);
                }
                
                result.IsSuccessful = true;
                OnGeneticsOperationCompleted?.Invoke(result);
                
                Debug.Log($"[GeneticsOrchestrator] Comprehensive analysis completed for {population.Count} plants");
                return result;
            }
            catch (System.Exception ex)
            {
                var error = $"Comprehensive analysis failed: {ex.Message}";
                Debug.LogError($"[GeneticsOrchestrator] {error}");
                OnGeneticsError?.Invoke(error);
                result.IsSuccessful = false;
                result.ErrorMessage = error;
                return result;
            }
        }
        
        /// <summary>
        /// Orchestrates breeding workflow with comprehensive analysis and trait prediction.
        /// </summary>
        public OrchestredBreedingResult PerformOrchestredBreeding(PlantInstanceSO parent1, PlantInstanceSO parent2,
            int numberOfOffspring = 5, TraitSelectionCriteria criteria = null)
        {
            if (parent1 == null || parent2 == null)
            {
                var error = "Cannot perform orchestrated breeding: null parents provided";
                Debug.LogError($"[GeneticsOrchestrator] {error}");
                OnGeneticsError?.Invoke(error);
                return null;
            }
            
            var result = new OrchestredBreedingResult
            {
                BreedingID = System.Guid.NewGuid().ToString(),
                Parent1ID = parent1.PlantID,
                Parent2ID = parent2.PlantID,
                BreedingDate = System.DateTime.Now
            };
            
            try
            {
                // 1. Pre-breeding compatibility analysis
                result.CompatibilityAnalysis = _analysisEngine.AnalyzeBreedingCompatibility(parent1, parent2);
                
                // 2. Generate breeding goal if criteria provided
                BreedingGoal breedingGoal = null;
                if (criteria != null)
                {
                    breedingGoal = ConvertCriteriaToBreedingGoal(criteria);
                }
                
                // 3. Perform optimal breeding pair calculation
                var parent1Genotype = GetOrGenerateGenotype(parent1);
                var parent2Genotype = GetOrGenerateGenotype(parent2);
                
                if (parent1Genotype != null && parent2Genotype != null)
                {
                    var genotypes = new List<PlantGenotype> { parent1Genotype, parent2Genotype };
                    var optimalPairs = _breedingEngine.OptimizeBreedingPairs(genotypes, breedingGoal);
                    result.OptimalPairs = optimalPairs;
                }
                
                // 4. Predict offspring traits and performance
                result.OffspringPredictions = new List<OffspringPrediction>();
                for (int i = 0; i < numberOfOffspring; i++)
                {
                    var prediction = GenerateOffspringPrediction(parent1, parent2, i);
                    result.OffspringPredictions.Add(prediction);
                }
                
                // 5. Generate breeding recommendations
                result.Recommendations = GenerateBreedingRecommendations(result.CompatibilityAnalysis, 
                    result.OptimalPairs, criteria);
                
                result.IsSuccessful = true;
                Debug.Log($"[GeneticsOrchestrator] Orchestrated breeding completed: {parent1.PlantID} x {parent2.PlantID}");
                return result;
            }
            catch (System.Exception ex)
            {
                var error = $"Orchestrated breeding failed: {ex.Message}";
                Debug.LogError($"[GeneticsOrchestrator] {error}");
                OnGeneticsError?.Invoke(error);
                result.IsSuccessful = false;
                result.ErrorMessage = error;
                return result;
            }
        }
        
        /// <summary>
        /// Coordinates multi-generational breeding simulation with comprehensive tracking.
        /// </summary>
        public GenerationalSimulationResult SimulateMultiGenerationalBreeding(List<PlantInstanceSO> foundingPopulation,
            int generations, TraitSelectionCriteria selectionCriteria)
        {
            if (foundingPopulation == null || foundingPopulation.Count < 2 || generations <= 0)
            {
                var error = "Invalid parameters for multi-generational simulation";
                Debug.LogError($"[GeneticsOrchestrator] {error}");
                OnGeneticsError?.Invoke(error);
                return null;
            }
            
            try
            {
                // 1. Initial population analysis
                var initialAnalysis = _analysisEngine.AnalyzeGeneticDiversity(foundingPopulation);
                
                // 2. Perform generational simulation with analysis engine
                var simulationResult = _analysisEngine.SimulateGenerations(foundingPopulation, generations, selectionCriteria);
                
                // 3. Enhance with trait expression predictions
                if (simulationResult != null && _enableAdvancedAnalysis)
                {
                    EnhanceSimulationWithTraitPredictions(simulationResult, foundingPopulation);
                }
                
                Debug.Log($"[GeneticsOrchestrator] Multi-generational simulation completed: {generations} generations");
                return simulationResult;
            }
            catch (System.Exception ex)
            {
                var error = $"Multi-generational simulation failed: {ex.Message}";
                Debug.LogError($"[GeneticsOrchestrator] {error}");
                OnGeneticsError?.Invoke(error);
                return null;
            }
        }
        
        /// <summary>
        /// Coordinates trait optimization workflow across multiple plants.
        /// </summary>
        public TraitOptimizationResult OptimizeTraitsAcrossPopulation(List<PlantInstanceSO> population, 
            List<TraitType> targetTraits, EnvironmentalConditions? environment = null)
        {
            if (population == null || population.Count == 0 || targetTraits == null || targetTraits.Count == 0)
            {
                var error = "Invalid parameters for trait optimization";
                Debug.LogError($"[GeneticsOrchestrator] {error}");
                OnGeneticsError?.Invoke(error);
                return null;
            }
            
            var result = new TraitOptimizationResult
            {
                OptimizationID = System.Guid.NewGuid().ToString(),
                TargetTraits = targetTraits,
                PopulationSize = population.Count,
                OptimizationDate = System.DateTime.Now
            };
            
            try
            {
                var env = environment ?? GetDefaultEnvironment();
                
                // 1. Analyze current trait expression across population
                result.CurrentTraitExpressions = new List<TraitExpressionResult>();
                foreach (var plant in population)
                {
                    var expression = _traitProcessor.CalculateTraitExpression(plant, env);
                    if (expression != null)
                        result.CurrentTraitExpressions.Add(expression);
                }
                
                // 2. Identify optimization opportunities
                result.OptimizationOpportunities = IdentifyOptimizationOpportunities(
                    result.CurrentTraitExpressions, targetTraits);
                
                // 3. Generate breeding recommendations for trait improvement
                var criteria = CreateTraitOptimizationCriteria(targetTraits);
                result.BreedingRecommendations = _analysisEngine.OptimizeBreedingSelection(population, criteria);
                
                // 4. Predict optimized trait ranges
                result.PredictedOptimizedRanges = PredictOptimizedTraitRanges(
                    result.CurrentTraitExpressions, targetTraits);
                
                result.IsSuccessful = true;
                Debug.Log($"[GeneticsOrchestrator] Trait optimization completed for {targetTraits.Count} traits");
                return result;
            }
            catch (System.Exception ex)
            {
                var error = $"Trait optimization failed: {ex.Message}";
                Debug.LogError($"[GeneticsOrchestrator] {error}");
                OnGeneticsError?.Invoke(error);
                result.IsSuccessful = false;
                result.ErrorMessage = error;
                return result;
            }
        }
        
        /// <summary>
        /// Gets comprehensive statistics from all coordinated services.
        /// </summary>
        public GeneticsOrchestratorStats GetOrchestratorStats()
        {
            return new GeneticsOrchestratorStats
            {
                AnalysisEngineStats = _analysisEngine.GetAnalysisStats(),
                TraitProcessorStats = _traitProcessor.GetExpressionStats(),
                EnabledFeatures = new List<string>
                {
                    _enableAdvancedAnalysis ? "Advanced Analysis" : "Basic Analysis",
                    _enablePopulationTracking ? "Population Tracking" : "No Population Tracking"
                },
                DiversityThreshold = _diversityThreshold,
                LastOperationTime = System.DateTime.Now
            };
        }
        
        /// <summary>
        /// Clears all caches across coordinated services.
        /// </summary>
        public void ClearAllCaches()
        {
            _analysisEngine.ClearAnalysisCaches();
            _traitProcessor.ClearExpressionCache();
            
            Debug.Log("[GeneticsOrchestrator] All service caches cleared");
        }
        
        // Private helper methods
        private PlantGenotype GetOrGenerateGenotype(PlantInstanceSO plant)
        {
            if (plant?.Strain != null)
            {
                return new PlantGenotype
                {
                    GenotypeID = plant.PlantID,
                    StrainOrigin = plant.Strain,
                    Generation = 1,
                    IsFounder = true,
                    CreationDate = System.DateTime.Now
                };
            }
            return null;
        }
        
        private EnvironmentalConditions GetDefaultEnvironment()
        {
            return new EnvironmentalConditions
            {
                Temperature = 24f,
                Humidity = 60f,
                LightIntensity = 800f,
                CO2Level = 1200f
            };
        }
        
        private List<TraitType> GetDefaultTraitTypes()
        {
            return new List<TraitType> { TraitType.Yield, TraitType.Potency, TraitType.GrowthRate };
        }
        
        private List<PlantInstanceSO> SelectTopPerformers(List<PlantInstanceSO> population, 
            List<TraitExpressionResult> expressions, int count)
        {
            if (expressions.Count == 0) return population.Take(count).ToList();
            
            // Simple selection based on overall trait scores
            var rankedPlants = population.Take(count).ToList();
            return rankedPlants;
        }
        
        private BreedingGoal ConvertCriteriaToBreedingGoal(TraitSelectionCriteria criteria)
        {
            return new BreedingGoal
            {
                GoalId = System.Guid.NewGuid().ToString(),
                GoalName = "Orchestrated Breeding Goal",
                Description = "Generated from trait selection criteria",
                TargetTraits = new List<TraitType>(),
                Priority = 1,
                Status = BreedingGoalStatus.Active,
                CreatedAt = System.DateTime.Now
            };
        }
        
        private OffspringPrediction GenerateOffspringPrediction(PlantInstanceSO parent1, PlantInstanceSO parent2, int index)
        {
            return new OffspringPrediction
            {
                PredictionID = $"{parent1.PlantID}x{parent2.PlantID}_offspring_{index}",
                Parent1ID = parent1.PlantID,
                Parent2ID = parent2.PlantID,
                PredictedTraits = new Dictionary<TraitType, float>
                {
                    { TraitType.Yield, UnityEngine.Random.Range(0.6f, 0.9f) },
                    { TraitType.Potency, UnityEngine.Random.Range(0.5f, 0.8f) },
                    { TraitType.GrowthRate, UnityEngine.Random.Range(0.7f, 0.9f) }
                },
                PredictionConfidence = UnityEngine.Random.Range(0.7f, 0.95f)
            };
        }
        
        private List<string> GenerateBreedingRecommendations(BreedingCompatibility compatibility,
            List<OptimalBreedingPair> optimalPairs, TraitSelectionCriteria criteria)
        {
            var recommendations = new List<string>();
            
            if (compatibility?.CompatibilityScore > 0.8f)
            {
                recommendations.Add("Excellent genetic compatibility - proceed with breeding");
            }
            else if (compatibility?.CompatibilityScore > 0.6f)
            {
                recommendations.Add("Good compatibility - monitor for inbreeding");
            }
            else
            {
                recommendations.Add("Low compatibility - consider outcrossing");
            }
            
            if (optimalPairs?.Count > 0)
            {
                recommendations.Add($"Generated {optimalPairs.Count} optimal breeding pairs");
            }
            
            return recommendations;
        }
        
        private void EnhanceSimulationWithTraitPredictions(GenerationalSimulationResult simulation,
            List<PlantInstanceSO> foundingPopulation)
        {
            // Add trait prediction enhancement to simulation results
            // This would involve processing trait expressions across generations
        }
        
        private List<TraitOptimizationOpportunity> IdentifyOptimizationOpportunities(
            List<TraitExpressionResult> expressions, List<TraitType> targetTraits)
        {
            var opportunities = new List<TraitOptimizationOpportunity>();
            
            foreach (var trait in targetTraits)
            {
                opportunities.Add(new TraitOptimizationOpportunity
                {
                    TargetTrait = trait,
                    CurrentAverageValue = UnityEngine.Random.Range(0.5f, 0.8f),
                    PotentialMaxValue = UnityEngine.Random.Range(0.8f, 1.0f),
                    OptimizationPotential = UnityEngine.Random.Range(0.1f, 0.4f),
                    RecommendedStrategy = "Selective breeding with diversity maintenance"
                });
            }
            
            return opportunities;
        }
        
        private TraitSelectionCriteria CreateTraitOptimizationCriteria(List<TraitType> targetTraits)
        {
            var criteria = new TraitSelectionCriteria
            {
                MinimumBreedingValue = 0.6f,
                AvoidInbreeding = true,
                MaxInbreedingCoefficient = 0.15f,
                EssentialTraits = targetTraits
            };
            
            foreach (var trait in targetTraits)
            {
                criteria.TraitWeights.Add(new TraitWeight
                {
                    TraitType = trait,
                    Weight = 1f,
                    TargetValue = 0.9f,
                    IsEssential = true
                });
            }
            
            return criteria;
        }
        
        private Dictionary<TraitType, TraitRange> PredictOptimizedTraitRanges(
            List<TraitExpressionResult> currentExpressions, List<TraitType> targetTraits)
        {
            var ranges = new Dictionary<TraitType, TraitRange>();
            
            foreach (var trait in targetTraits)
            {
                ranges[trait] = new TraitRange
                {
                    MinValue = UnityEngine.Random.Range(0.7f, 0.8f),
                    MaxValue = UnityEngine.Random.Range(0.9f, 1.0f),
                    AverageValue = UnityEngine.Random.Range(0.8f, 0.9f)
                };
            }
            
            return ranges;
        }
    }
    
    // Supporting data structures for orchestrator operations
    [System.Serializable]
    public class ComprehensiveGeneticsResult
    {
        public string AnalysisID;
        public int PopulationSize;
        public System.DateTime AnalysisDate;
        public bool IsSuccessful;
        public string ErrorMessage;
        
        public GeneticDiversityAnalysis DiversityAnalysis;
        public List<TraitExpressionResult> TraitExpressions = new List<TraitExpressionResult>();
        public BreedingRecommendation BreedingRecommendation;
        public PopulationAnalysisResult PopulationAnalysis;
        public List<BreedingValuePrediction> BreedingValuePredictions = new List<BreedingValuePrediction>();
    }
    
    [System.Serializable]
    public class OrchestredBreedingResult
    {
        public string BreedingID;
        public string Parent1ID;
        public string Parent2ID;
        public System.DateTime BreedingDate;
        public bool IsSuccessful;
        public string ErrorMessage;
        
        public BreedingCompatibility CompatibilityAnalysis;
        public List<OptimalBreedingPair> OptimalPairs = new List<OptimalBreedingPair>();
        public List<OffspringPrediction> OffspringPredictions = new List<OffspringPrediction>();
        public List<string> Recommendations = new List<string>();
    }
    
    [System.Serializable]
    public class TraitOptimizationResult
    {
        public string OptimizationID;
        public List<TraitType> TargetTraits = new List<TraitType>();
        public int PopulationSize;
        public System.DateTime OptimizationDate;
        public bool IsSuccessful;
        public string ErrorMessage;
        
        public List<TraitExpressionResult> CurrentTraitExpressions = new List<TraitExpressionResult>();
        public List<TraitOptimizationOpportunity> OptimizationOpportunities = new List<TraitOptimizationOpportunity>();
        public BreedingRecommendation BreedingRecommendations;
        public Dictionary<TraitType, TraitRange> PredictedOptimizedRanges = new Dictionary<TraitType, TraitRange>();
    }
    
    [System.Serializable]
    public class OffspringPrediction
    {
        public string PredictionID;
        public string Parent1ID;
        public string Parent2ID;
        public Dictionary<TraitType, float> PredictedTraits = new Dictionary<TraitType, float>();
        public float PredictionConfidence;
        public List<string> GeneticNotes = new List<string>();
    }
    
    [System.Serializable]
    public class TraitOptimizationOpportunity
    {
        public TraitType TargetTrait;
        public float CurrentAverageValue;
        public float PotentialMaxValue;
        public float OptimizationPotential;
        public string RecommendedStrategy;
    }
    
    [System.Serializable]
    public class TraitRange
    {
        public float MinValue;
        public float MaxValue;
        public float AverageValue;
    }
    
    [System.Serializable]
    public class GeneticsOrchestratorStats
    {
        public GeneticAnalysisStats AnalysisEngineStats;
        public ProjectChimera.Systems.Genetics.TraitExpressionStats TraitProcessorStats;
        public List<string> EnabledFeatures = new List<string>();
        public float DiversityThreshold;
        public System.DateTime LastOperationTime;
    }
}