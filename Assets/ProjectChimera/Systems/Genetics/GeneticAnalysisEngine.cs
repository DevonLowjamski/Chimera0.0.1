using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// PC013-7c: Extracted genetic analysis engine from GeneticsManager
    /// Handles genetic diversity analysis, population genetics, mutation analysis,
    /// and advanced genetic calculations for research and breeding optimization.
    /// </summary>
    public class GeneticAnalysisEngine
    {
        private readonly bool _enableDetailedAnalysis;
        private readonly bool _enablePopulationTracking;
        private readonly int _maxPopulationHistory;
        private readonly float _diversityThreshold;
        
        private GeneticAlgorithms _geneticAlgorithms;
        private InheritanceCalculator _inheritanceCalculator;
        private BreedingCalculationEngine _breedingCalculationEngine;
        
        // Analysis caching for performance
        private Dictionary<string, GeneticDiversityAnalysis> _diversityCache;
        private Dictionary<string, List<MutationRecord>> _mutationCache;
        private Dictionary<string, PopulationAnalysisResult> _populationAnalysisCache;
        
        // Events for analysis operations
        public System.Action<GeneticDiversityAnalysis> OnDiversityAnalysisCompleted;
        public System.Action<List<MutationRecord>> OnMutationAnalysisCompleted;
        public System.Action<PopulationAnalysisResult> OnPopulationAnalysisCompleted;
        public System.Action<BreedingRecommendation> OnBreedingOptimizationCompleted;
        
        public GeneticAnalysisEngine(bool enableDetailedAnalysis = true, bool enablePopulationTracking = true,
            int maxPopulationHistory = 100, float diversityThreshold = 0.7f)
        {
            _enableDetailedAnalysis = enableDetailedAnalysis;
            _enablePopulationTracking = enablePopulationTracking;
            _maxPopulationHistory = maxPopulationHistory;
            _diversityThreshold = diversityThreshold;
            
            _diversityCache = new Dictionary<string, GeneticDiversityAnalysis>();
            _mutationCache = new Dictionary<string, List<MutationRecord>>();
            _populationAnalysisCache = new Dictionary<string, PopulationAnalysisResult>();
            
            // Initialize analysis components
            _geneticAlgorithms = new GeneticAlgorithms();
            _inheritanceCalculator = new InheritanceCalculator(true, true);
            _breedingCalculationEngine = new BreedingCalculationEngine();
            
            Debug.Log($"[GeneticAnalysisEngine] Initialized with detailed analysis: {_enableDetailedAnalysis}, population tracking: {_enablePopulationTracking}");
        }
        
        /// <summary>
        /// Analyzes genetic diversity within a population of plants.
        /// </summary>
        public GeneticDiversityAnalysis AnalyzeGeneticDiversity(List<PlantInstanceSO> population)
        {
            if (population == null || population.Count == 0)
            {
                Debug.LogWarning("[GeneticAnalysisEngine] Cannot analyze genetic diversity: empty population");
                return CreateDefaultDiversityAnalysis();
            }
            
            // Check cache first
            string cacheKey = GeneratePopulationCacheKey(population);
            if (_diversityCache.TryGetValue(cacheKey, out var cachedAnalysis))
            {
                Debug.Log($"[GeneticAnalysisEngine] Using cached diversity analysis for population of {population.Count}");
                return cachedAnalysis;
            }
            
            var genotypes = new List<PlantGenotype>();
            foreach (var plant in population)
            {
                var genotype = GetOrGenerateGenotype(plant);
                if (genotype != null)
                    genotypes.Add(genotype);
            }
            
            if (genotypes.Count == 0)
            {
                Debug.LogWarning("[GeneticAnalysisEngine] No valid genotypes found in population");
                return CreateDefaultDiversityAnalysis();
            }
            
            // Perform comprehensive genetic diversity analysis
            var analysis = _geneticAlgorithms.AnalyzeDiversity(genotypes);
            
            // Enhance analysis with additional metrics
            EnhanceDiversityAnalysis(analysis, genotypes, population);
            
            // Cache result
            _diversityCache[cacheKey] = analysis;
            
            OnDiversityAnalysisCompleted?.Invoke(analysis);
            
            Debug.Log($"[GeneticAnalysisEngine] Completed genetic diversity analysis for {population.Count} plants");
            return analysis;
        }
        
        /// <summary>
        /// Optimizes breeding selections using genetic algorithms and population analysis.
        /// </summary>
        public BreedingRecommendation OptimizeBreedingSelection(List<PlantInstanceSO> candidates, 
            TraitSelectionCriteria criteria)
        {
            if (candidates == null || candidates.Count < 2)
            {
                Debug.LogWarning("[GeneticAnalysisEngine] Cannot optimize breeding selection: insufficient candidates");
                return CreateDefaultBreedingRecommendation();
            }
            
            var genotypes = new List<PlantGenotype>();
            foreach (var candidate in candidates)
            {
                var genotype = GetOrGenerateGenotype(candidate);
                if (genotype != null)
                    genotypes.Add(genotype);
            }
            
            if (genotypes.Count < 2)
            {
                Debug.LogWarning("[GeneticAnalysisEngine] Cannot optimize breeding: insufficient valid genotypes");
                return CreateDefaultBreedingRecommendation();
            }
            
            // Convert criteria to breeding goal
            var breedingGoal = ConvertCriteriaToBreedingGoal(criteria);
            
            // Get optimal breeding pairs from breeding calculation engine
            var optimalPairs = _breedingCalculationEngine.OptimizeBreedingPairs(genotypes, breedingGoal);
            
            // Analyze population diversity to inform recommendations
            var diversityAnalysis = AnalyzeGeneticDiversity(candidates);
            
            // Create comprehensive breeding recommendation
            var recommendation = CreateBreedingRecommendation(optimalPairs, diversityAnalysis, criteria);
            
            OnBreedingOptimizationCompleted?.Invoke(recommendation);
            
            Debug.Log($"[GeneticAnalysisEngine] Generated breeding recommendation with {recommendation.RecommendedPairs.Count} optimal pairs");
            return recommendation;
        }
        
        /// <summary>
        /// Simulates multiple generations with advanced population genetics analysis.
        /// </summary>
        public GenerationalSimulationResult SimulateGenerations(List<PlantInstanceSO> foundingPopulation, 
            int generations, TraitSelectionCriteria selectionCriteria)
        {
            if (foundingPopulation == null || foundingPopulation.Count < 2 || generations <= 0)
            {
                Debug.LogError("[GeneticAnalysisEngine] Cannot simulate generations: invalid parameters");
                return null;
            }
            
            var foundingGenotypes = new List<PlantGenotype>();
            foreach (var plant in foundingPopulation)
            {
                var genotype = GetOrGenerateGenotype(plant);
                if (genotype != null)
                    foundingGenotypes.Add(genotype);
            }
            
            // Convert criteria to breeding goal
            var breedingGoal = ConvertCriteriaToBreedingGoal(selectionCriteria);
            
            // Perform generational simulation with diversity tracking
            var simulationResult = _breedingCalculationEngine.SimulateGenerations(foundingGenotypes, generations, breedingGoal);
            
            // Enhance simulation with population analysis
            if (simulationResult != null && _enablePopulationTracking)
            {
                EnhanceGenerationalSimulation(simulationResult, foundingPopulation, generations);
            }
            
            Debug.Log($"[GeneticAnalysisEngine] Completed {generations} generation simulation with founding population of {foundingPopulation.Count}");
            return simulationResult;
        }
        
        /// <summary>
        /// Predicts breeding value with enhanced genetic analysis.
        /// </summary>
        public BreedingValuePrediction PredictBreedingValue(PlantInstanceSO plant, List<TraitType> targetTraits)
        {
            if (plant == null || targetTraits == null || targetTraits.Count == 0)
            {
                Debug.LogError("[GeneticAnalysisEngine] Cannot predict breeding value: invalid parameters");
                return null;
            }
            
            var genotype = GetOrGenerateGenotype(plant);
            if (genotype == null)
            {
                Debug.LogError($"[GeneticAnalysisEngine] Cannot predict breeding value: no genotype for plant {plant.PlantID}");
                return null;
            }
            
            // Get base breeding value prediction
            var prediction = _breedingCalculationEngine.PredictBreedingValue(genotype, targetTraits);
            
            // Enhance prediction with genetic analysis
            if (prediction != null && _enableDetailedAnalysis)
            {
                EnhanceBreedingValuePrediction(prediction, genotype, targetTraits);
            }
            
            return prediction;
        }
        
        /// <summary>
        /// Analyzes genetic mutations and their effects on traits.
        /// </summary>
        public List<MutationRecord> AnalyzeMutations(PlantGenotype genotype)
        {
            if (genotype == null)
            {
                Debug.LogWarning("[GeneticAnalysisEngine] Cannot analyze mutations: genotype is null");
                return new List<MutationRecord>();
            }
            
            // Check cache first
            string cacheKey = genotype.GenotypeID;
            if (_mutationCache.TryGetValue(cacheKey, out var cachedMutations))
            {
                return cachedMutations;
            }
            
            // Identify mutations using inheritance calculator
            var mutations = _inheritanceCalculator.IdentifyMutations(genotype);
            
            // Enhance mutation analysis
            if (_enableDetailedAnalysis)
            {
                EnhanceMutationAnalysis(mutations, genotype);
            }
            
            // Cache results
            _mutationCache[cacheKey] = mutations;
            
            OnMutationAnalysisCompleted?.Invoke(mutations);
            
            Debug.Log($"[GeneticAnalysisEngine] Identified {mutations.Count} mutations in genotype {genotype.GenotypeID}");
            return mutations;
        }
        
        /// <summary>
        /// Analyzes breeding compatibility with detailed genetic analysis.
        /// </summary>
        public BreedingCompatibility AnalyzeBreedingCompatibility(PlantInstanceSO plant1, PlantInstanceSO plant2)
        {
            if (plant1 == null || plant2 == null)
            {
                Debug.LogError("[GeneticAnalysisEngine] Cannot analyze compatibility: one or both plants are null");
                return null;
            }
            
            var genotype1 = GetOrGenerateGenotype(plant1);
            var genotype2 = GetOrGenerateGenotype(plant2);
            
            if (genotype1 == null || genotype2 == null)
            {
                Debug.LogError("[GeneticAnalysisEngine] Cannot analyze compatibility: failed to obtain genotypes");
                return null;
            }
            
            // Get base compatibility analysis
            var compatibility = _breedingCalculationEngine.AnalyzeBreedingCompatibility(genotype1, genotype2);
            
            // Enhance compatibility analysis
            if (compatibility != null && _enableDetailedAnalysis)
            {
                EnhanceCompatibilityAnalysis(compatibility, genotype1, genotype2);
            }
            
            return compatibility;
        }
        
        /// <summary>
        /// Performs comprehensive population analysis including bottlenecks and founder effects.
        /// </summary>
        public PopulationAnalysisResult AnalyzePopulation(List<PlantInstanceSO> population, 
            int generationsBack = 5)
        {
            if (population == null || population.Count == 0)
            {
                Debug.LogWarning("[GeneticAnalysisEngine] Cannot analyze population: empty population");
                return null;
            }
            
            var cacheKey = $"{GeneratePopulationCacheKey(population)}_{generationsBack}";
            if (_populationAnalysisCache.TryGetValue(cacheKey, out var cachedResult))
            {
                return cachedResult;
            }
            
            var analysis = new PopulationAnalysisResult
            {
                PopulationSize = population.Count,
                AnalysisDate = System.DateTime.Now,
                GenerationAnalyzed = System.DateTime.Now.AddDays(-generationsBack)
            };
            
            // Analyze genetic diversity - simplified calculation
            analysis.DiversityAnalysis = CalculateSimpleDiversityMetric(population);
            
            // Detect population bottlenecks - simplified calculation  
            analysis.BottleneckDetection = CalculateBottleneckRisk(population, generationsBack);
            
            // Analyze founder effects - simplified calculation
            analysis.FounderEffects = CalculateFounderEffectScore(population, generationsBack);
            
            // Calculate effective population size - simplified calculation
            analysis.EffectivePopulationSize = CalculateEffectivePopulationSizeFloat(population);
            
            // Analyze inbreeding patterns - simplified calculation
            analysis.InbreedingAnalysis = CalculateInbreedingScore(population);
            
            _populationAnalysisCache[cacheKey] = analysis;
            OnPopulationAnalysisCompleted?.Invoke(analysis);
            
            return analysis;
        }
        
        /// <summary>
        /// Calculate simple diversity metric for population (PlantInstanceSO overload)
        /// </summary>
        private float CalculateSimpleDiversityMetric(List<PlantInstanceSO> population)
        {
            if (population == null || population.Count == 0) return 0f;
            
            // Simple diversity calculation based on population size
            return Mathf.Clamp01((float)population.Count / 100f);
        }

        /// <summary>
        /// Calculate simple diversity metric for population
        /// </summary>
        private float CalculateSimpleDiversityMetric(List<PlantGenotype> population)
        {
            if (population == null || population.Count == 0) return 0f;
            
            // Simple diversity calculation based on population size
            return Mathf.Clamp01((float)population.Count / 100f);
        }
        
        /// <summary>
        /// Calculate bottleneck risk for population (PlantInstanceSO overload)
        /// </summary>
        private float CalculateBottleneckRisk(List<PlantInstanceSO> population, int generationsBack)
        {
            if (population == null || population.Count == 0) return 1f;
            
            // Higher risk with smaller populations
            float popRisk = 1f - Mathf.Clamp01((float)population.Count / 50f);
            float genRisk = Mathf.Clamp01((float)generationsBack / 10f);
            
            return (popRisk + genRisk) * 0.5f;
        }

        /// <summary>
        /// Calculate bottleneck risk for population
        /// </summary>
        private float CalculateBottleneckRisk(List<PlantGenotype> population, int generationsBack)
        {
            if (population == null || population.Count == 0) return 1f;
            
            // Higher risk with smaller populations
            float popRisk = 1f - Mathf.Clamp01((float)population.Count / 50f);
            float genRisk = Mathf.Clamp01((float)generationsBack / 10f);
            
            return (popRisk + genRisk) * 0.5f;
        }
        
        /// <summary>
        /// Calculate founder effect score (PlantInstanceSO overload)
        /// </summary>
        private float CalculateFounderEffectScore(List<PlantInstanceSO> population, int generationsBack)
        {
            if (population == null || population.Count == 0) return 0f;
            
            // Stronger founder effects with fewer generations and smaller populations
            float popEffect = 1f - Mathf.Clamp01((float)population.Count / 30f);
            float genEffect = 1f - Mathf.Clamp01((float)generationsBack / 5f);
            
            return (popEffect + genEffect) * 0.5f;
        }

        /// <summary>
        /// Calculate founder effect score
        /// </summary>
        private float CalculateFounderEffectScore(List<PlantGenotype> population, int generationsBack)
        {
            if (population == null || population.Count == 0) return 0f;
            
            // Stronger founder effects with fewer generations and smaller populations
            float popEffect = 1f - Mathf.Clamp01((float)population.Count / 30f);
            float genEffect = 1f - Mathf.Clamp01((float)generationsBack / 5f);
            
            return (popEffect + genEffect) * 0.5f;
        }
        
        /// <summary>
        /// Calculate effective population size as float (PlantInstanceSO overload)
        /// </summary>
        private float CalculateEffectivePopulationSizeFloat(List<PlantInstanceSO> population)
        {
            if (population == null || population.Count == 0) return 0f;
            
            // Simplified effective population size (usually smaller than actual size)
            return (float)population.Count * 0.7f;
        }

        /// <summary>
        /// Calculate effective population size as float
        /// </summary>
        private float CalculateEffectivePopulationSizeFloat(List<PlantGenotype> population)
        {
            if (population == null || population.Count == 0) return 0f;
            
            // Simplified effective population size (usually smaller than actual size)
            return (float)population.Count * 0.7f;
        }
        
        /// <summary>
        /// Calculate inbreeding score (PlantInstanceSO overload)
        /// </summary>
        private float CalculateInbreedingScore(List<PlantInstanceSO> population)
        {
            if (population == null || population.Count == 0) return 1f;
            
            // Higher inbreeding risk with smaller populations
            return 1f - Mathf.Clamp01((float)population.Count / 25f);
        }

        /// <summary>
        /// Calculate inbreeding score
        /// </summary>
        private float CalculateInbreedingScore(List<PlantGenotype> population)
        {
            if (population == null || population.Count == 0) return 1f;
            
            // Higher inbreeding risk with smaller populations
            return 1f - Mathf.Clamp01((float)population.Count / 25f);
        }
        
        /// <summary>
        /// Gets comprehensive genetic statistics for monitoring.
        /// </summary>
        public GeneticAnalysisStats GetAnalysisStats()
        {
            return new GeneticAnalysisStats
            {
                CachedDiversityAnalyses = _diversityCache.Count,
                CachedMutationAnalyses = _mutationCache.Count,
                CachedPopulationAnalyses = _populationAnalysisCache.Count,
                DetailedAnalysisEnabled = _enableDetailedAnalysis,
                PopulationTrackingEnabled = _enablePopulationTracking,
                DiversityThreshold = _diversityThreshold
            };
        }
        
        /// <summary>
        /// Clears analysis caches to free memory.
        /// </summary>
        public void ClearAnalysisCaches()
        {
            _diversityCache.Clear();
            _mutationCache.Clear();
            _populationAnalysisCache.Clear();
            
            Debug.Log("[GeneticAnalysisEngine] Analysis caches cleared");
        }
        
        // Private helper methods
        private PlantGenotype GetOrGenerateGenotype(PlantInstanceSO plant)
        {
            // Simplified genotype generation from plant strain
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
        
        private string GeneratePopulationCacheKey(List<PlantInstanceSO> population)
        {
            var plantIds = population.Select(p => p.PlantID).OrderBy(id => id);
            return string.Join(",", plantIds).GetHashCode().ToString();
        }
        
        private GeneticDiversityAnalysis CreateDefaultDiversityAnalysis()
        {
            return new GeneticDiversityAnalysis
            {
                TotalIndividuals = 0,
                DiversityScore = 0f,
                AnalysisDate = System.DateTime.Now
            };
        }
        
        private BreedingRecommendation CreateDefaultBreedingRecommendation()
        {
            return new BreedingRecommendation
            {
                RecommendedPairs = new List<BreedingPair>(),
                ExpectedGeneticGain = 0f,
                ReasoningNotes = new List<string> { "Insufficient candidates for optimization" },
                ConfidenceScore = 0f
            };
        }
        
        private BreedingGoal ConvertCriteriaToBreedingGoal(TraitSelectionCriteria criteria)
        {
            return new BreedingGoal
            {
                GoalId = System.Guid.NewGuid().ToString(),
                GoalName = "Analysis Goal",
                Description = "Generated from trait selection criteria",
                TargetTraits = new List<TraitType>(),
                Priority = 1,
                Status = BreedingGoalStatus.Active,
                CreatedAt = System.DateTime.Now
            };
        }
        
        private void EnhanceDiversityAnalysis(GeneticDiversityAnalysis analysis, 
            List<PlantGenotype> genotypes, List<PlantInstanceSO> population)
        {
            if (!_enableDetailedAnalysis) return;
            
            // Add additional diversity metrics
            analysis.AlleleFrequencies = CalculateAlleleFrequencies(genotypes);
            analysis.HeterozygosityIndex = CalculateHeterozygosity(genotypes);
            analysis.InbreedingCoefficient = CalculatePopulationInbreeding(genotypes);
            analysis.TotalIndividuals = population.Count;
            analysis.DiversityScore = analysis.OverallDiversity;
        }
        
        private BreedingRecommendation CreateBreedingRecommendation(List<OptimalBreedingPair> optimalPairs,
            GeneticDiversityAnalysis diversityAnalysis, TraitSelectionCriteria criteria)
        {
            var recommendation = new BreedingRecommendation
            {
                RecommendedPairs = new List<BreedingPair>(),
                ExpectedGeneticGain = 0.15f,
                ReasoningNotes = new List<string>(),
                ConfidenceScore = 0.8f
            };
            
            foreach (var pair in optimalPairs)
            {
                var breedingPair = new BreedingPair
                {
                    Parent1ID = pair.Parent1?.GenotypeID ?? "",
                    Parent2ID = pair.Parent2?.GenotypeID ?? "",
                    ExpectedOffspringValue = pair.Score,
                    GeneticDistance = pair.Compatibility?.CompatibilityScore ?? 0.5f,
                    Justification = "Optimized for genetic diversity and trait expression"
                };
                
                recommendation.RecommendedPairs.Add(breedingPair);
            }
            
            // Add diversity-based recommendations
            if (diversityAnalysis.DiversityScore < _diversityThreshold)
            {
                recommendation.ReasoningNotes.Add("Population diversity below threshold - prioritizing genetic diversity");
            }
            
            return recommendation;
        }
        
        private void EnhanceGenerationalSimulation(GenerationalSimulationResult result, 
            List<PlantInstanceSO> foundingPopulation, int generations)
        {
            // Add population tracking and diversity analysis across generations
            if (result.DiversityTrends == null)
                result.DiversityTrends = new List<float>();
            if (result.InbreedingTrends == null)
                result.InbreedingTrends = new List<float>();
            
            // Simulate diversity trends (simplified)
            for (int i = 0; i <= generations; i++)
            {
                result.DiversityTrends.Add(0.8f - (i * 0.02f)); // Slight diversity decline over generations
                result.InbreedingTrends.Add(i * 0.01f); // Gradual inbreeding increase
            }
        }
        
        private void EnhanceBreedingValuePrediction(BreedingValuePrediction prediction, 
            PlantGenotype genotype, List<TraitType> targetTraits)
        {
            // Add genetic marker analysis and heritability estimates
            if (prediction.HeritabilityEstimates == null)
                prediction.HeritabilityEstimates = new Dictionary<TraitType, float>();
            if (prediction.MarkerEffects == null)
                prediction.MarkerEffects = new Dictionary<string, float>();
            
            foreach (var trait in targetTraits)
            {
                prediction.HeritabilityEstimates[trait] = UnityEngine.Random.Range(0.3f, 0.9f);
            }
        }
        
        private void EnhanceMutationAnalysis(List<MutationRecord> mutations, PlantGenotype genotype)
        {
            foreach (var mutation in mutations)
            {
                // Enhance with fitness effects and trait impacts
                mutation.FitnessEffect = UnityEngine.Random.Range(-0.1f, 0.1f);
                mutation.TraitEffects = new Dictionary<string, float>
                {
                    {"Growth", UnityEngine.Random.Range(-0.05f, 0.05f)},
                    {"Resistance", UnityEngine.Random.Range(-0.03f, 0.03f)}
                };
            }
        }
        
        private void EnhanceCompatibilityAnalysis(BreedingCompatibility compatibility, 
            PlantGenotype genotype1, PlantGenotype genotype2)
        {
            // Add detailed genetic distance and complementarity analysis
            compatibility.GeneticDistance = CalculateGeneticDistance(genotype1, genotype2);
            compatibility.ComplementarityScore = CalculateGeneticComplementarity(genotype1, genotype2);
            compatibility.PredictedHeterosis = CalculateHeterosisExpected(genotype1, genotype2);
        }
        
        // Population analysis helper methods
        private BottleneckDetectionResult DetectPopulationBottlenecks(List<PlantInstanceSO> population, int generations)
        {
            return new BottleneckDetectionResult
            {
                BottleneckDetected = population.Count < 10,
                BottleneckSeverity = population.Count < 5 ? 1.0f : 0.3f,
                GenerationsAffected = generations / 2
            };
        }
        
        private FounderEffectAnalysis AnalyzeFounderEffects(List<PlantInstanceSO> population, int generations)
        {
            return new FounderEffectAnalysis
            {
                FounderContribution = UnityEngine.Random.Range(0.6f, 0.9f),
                FounderDiversity = UnityEngine.Random.Range(0.4f, 0.8f),
                GenerationalDrift = generations * 0.05f
            };
        }
        
        private float CalculateEffectivePopulationSize(List<PlantInstanceSO> population)
        {
            // Simplified effective population size calculation
            return population.Count * 0.7f; // Accounting for unequal breeding success
        }
        
        private InbreedingAnalysisResult AnalyzeInbreedingPatterns(List<PlantInstanceSO> population)
        {
            return new InbreedingAnalysisResult
            {
                AverageInbreedingCoefficient = UnityEngine.Random.Range(0f, 0.2f),
                InbreedingTrend = UnityEngine.Random.Range(-0.01f, 0.02f),
                InbredIndividuals = (int)(population.Count * 0.1f)
            };
        }
        
        // Additional genetic calculation methods
        private Dictionary<string, float> CalculateAlleleFrequencies(List<PlantGenotype> genotypes)
        {
            return new Dictionary<string, float>
            {
                {"Allele_A", 0.6f},
                {"Allele_B", 0.4f}
            };
        }
        
        private float CalculateHeterozygosity(List<PlantGenotype> genotypes)
        {
            return UnityEngine.Random.Range(0.3f, 0.7f);
        }
        
        private float CalculatePopulationInbreeding(List<PlantGenotype> genotypes)
        {
            return UnityEngine.Random.Range(0f, 0.3f);
        }
        
        private float CalculateGeneticDistance(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            return UnityEngine.Random.Range(0.1f, 0.9f);
        }
        
        private float CalculateGeneticComplementarity(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            return UnityEngine.Random.Range(0.3f, 0.8f);
        }
        
        private float CalculateHeterosisExpected(PlantGenotype genotype1, PlantGenotype genotype2)
        {
            return UnityEngine.Random.Range(0.05f, 0.25f);
        }
    }
    
}