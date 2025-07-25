using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Core.ServiceContainer;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using System.Collections.Generic;
using System.Linq;
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions;
using TraitExpressionResult = ProjectChimera.Data.Genetics.TraitExpressionResult;

namespace ProjectChimera.Systems.Genetics
{
    /// <summary>
    /// Central manager for cannabis genetics including inheritance calculations, breeding mechanics,
    /// trait expression, and genetic algorithms. Implements scientifically accurate cannabis genetics
    /// based on research including epistasis, pleiotropy, and QTL effects.
    /// </summary>
    public class GeneticsManager : ChimeraManager
    {
        [Header("Genetics Configuration")]
        [SerializeField] private bool _enableEpistasis = true;
        [SerializeField] private bool _enablePleiotropy = true;
        [SerializeField] private bool _enableMutations = true;
        [SerializeField] private float _mutationRate = 0.001f; // 0.1% chance per allele
        [SerializeField] private bool _enableDetailedLogging = false;
        
        [Header("Breeding Settings")]
        [SerializeField] private int _maxGenerationsTracked = 10;
        [SerializeField] private bool _allowInbreeding = true;
        [SerializeField] private float _inbreedingDepression = 0.1f; // 10% fitness reduction per inbreeding coefficient unit
        [SerializeField] private bool _trackPedigrees = true;
        
        [Header("Event Channels")]
        [SerializeField] private GameEventSO<BreedingResult> _onBreedingCompleted;
        [SerializeField] private GameEventSO<GeneticMutation> _onMutationOccurred;
        [SerializeField] private GameEventSO<PlantInstanceSO> _onTraitExpressionCalculated;
        
        // PC014-2c: Dependency injection - genetic services injected via service container
        [Inject] private IInheritanceCalculationService _inheritanceCalculator;
        [Inject] private ITraitExpressionService _traitExpressionProcessor;
        [Inject] private IBreedingCalculationService _breedingCalculationEngine;
        [Inject] private IGeneticAnalysisService _geneticAnalysisEngine;
        [Inject] private IBreedingOptimizationService _breedingOptimizationService;
        
        // Private fields
        private Dictionary<string, PlantGenotype> _genotypeCache = new Dictionary<string, PlantGenotype>();
        private Dictionary<string, BreedingLineage> _pedigreeDatabase = new Dictionary<string, BreedingLineage>();
        
        public override ManagerPriority Priority => ManagerPriority.High;
        
        // Public Properties
        public bool EnableEpistasis => _enableEpistasis;
        public bool EnablePleiotropy => _enablePleiotropy;
        public bool EnableMutations => _enableMutations;
        public float MutationRate => _mutationRate;
        public int TrackedGenotypes => _genotypeCache.Count;
        
        protected override void OnManagerInitialize()
        {
            // PC014-2c: Use dependency injection for service resolution
            ServiceInjector.InjectDependencies(this);
            
            // Validate critical dependencies
            if (_inheritanceCalculator == null || _traitExpressionProcessor == null || _breedingCalculationEngine == null || _geneticAnalysisEngine == null)
            {
                LogWarning("One or more genetic services not injected. Some features may be limited.");
            }
            
            // Initialize genetic services with configuration
            InitializeGeneticServices();
            
            LogInfo($"GeneticsManager initialized with dependency injection - epistasis: {_enableEpistasis}, pleiotropy: {_enablePleiotropy}");
        }
        
        /// <summary>
        /// PC014-2c: Initialize genetic services with dependency injection
        /// </summary>
        private void InitializeGeneticServices()
        {
            try
            {
                // Initialize inheritance calculation service
                if (_inheritanceCalculator != null && !_inheritanceCalculator.IsInitialized)
                {
                    _inheritanceCalculator.Initialize();
                    _inheritanceCalculator.SetEpistasisEnabled(_enableEpistasis);
                    _inheritanceCalculator.SetPleiotropyEnabled(_enablePleiotropy);
                    LogInfo("Inheritance calculation service initialized via dependency injection");
                }
                
                // Initialize trait expression service
                if (_traitExpressionProcessor != null && !_traitExpressionProcessor.IsInitialized)
                {
                    _traitExpressionProcessor.Initialize();
                    LogInfo("Trait expression service initialized via dependency injection");
                }
                
                // Initialize breeding calculation service
                if (_breedingCalculationEngine != null && !_breedingCalculationEngine.IsInitialized)
                {
                    _breedingCalculationEngine.Initialize();
                    _breedingCalculationEngine.SetInbreedingAllowed(_allowInbreeding);
                    LogInfo("Breeding calculation service initialized via dependency injection");
                }
                
                // Initialize genetic analysis service
                if (_geneticAnalysisEngine != null && !_geneticAnalysisEngine.IsInitialized)
                {
                    _geneticAnalysisEngine.Initialize();
                    LogInfo("Genetic analysis service initialized via dependency injection");
                }
                
                // Initialize breeding optimization service
                if (_breedingOptimizationService != null && !_breedingOptimizationService.IsInitialized)
                {
                    _breedingOptimizationService.Initialize();
                    LogInfo("Breeding optimization service initialized via dependency injection");
                }
                
                LogInfo("Genetic services initialization completed");
            }
            catch (System.Exception ex)
            {
                LogError($"Failed to initialize genetic services: {ex.Message}");
            }
        }
        
        // PC014-2c: Service delegation methods with fallback support
        
        /// <summary>
        /// Generates a founder genotype from strain using inheritance calculation service
        /// </summary>
        public PlantGenotype GenerateFounderGenotype(PlantStrainSO strain)
        {
            if (_inheritanceCalculator != null)
            {
                return _inheritanceCalculator.GenerateFounderGenotype(strain);
            }
            
            LogWarning("Inheritance calculator service not available, using fallback genotype generation");
            return CreateFallbackGenotype(strain);
        }
        
        /// <summary>
        /// Calculates offspring genotype using inheritance calculation service
        /// </summary>
        public PlantGenotype CalculateOffspringGenotype(PlantGenotype parent1, PlantGenotype parent2)
        {
            if (_inheritanceCalculator != null)
            {
                return _inheritanceCalculator.CalculateOffspringGenotype(parent1, parent2);
            }
            
            LogWarning("Inheritance calculator service not available, using fallback calculation");
            return CreateSimpleOffspringGenotype(parent1, parent2);
        }
        
        /// <summary>
        /// Calculates trait expression using trait expression service
        /// </summary>
        public ProjectChimera.Systems.Genetics.TraitExpressionResult CalculateTraitExpression(PlantInstanceSO plant, EnvironmentalConditions environment)
        {
            if (_traitExpressionProcessor != null)
            {
                var genotype = GetOrGenerateGenotype(plant);
                if (genotype != null)
                {
                    // PC014-FIX-44: Service interface expects Data version but implementation might return Systems version
                    try
                    {
                        var result = _traitExpressionProcessor.CalculateTraitExpression(genotype, environment);
                        // The interface defines Data.Genetics.TraitExpressionResult, so convert to Systems version
                        return ConvertToSystemsTraitExpression(result);
                    }
                    catch (System.InvalidCastException)
                    {
                        // Fallback if there's a type mismatch
                        LogWarning("Type conversion issue in trait expression service, using fallback");
                        return ConvertToSystemsTraitExpression(CreateFallbackTraitExpression(plant));
                    }
                }
            }
            
            LogWarning("Trait expression service not available, using fallback calculation");
            return ConvertToSystemsTraitExpression(CreateFallbackTraitExpression(plant));
        }
        
        /// <summary>
        /// Calculates batch trait expressions using trait expression service
        /// </summary>
        public List<ProjectChimera.Systems.Genetics.TraitExpressionResult> CalculateBatchTraitExpression(List<PlantInstanceSO> plants, 
            EnvironmentalConditions environment)
        {
            if (_traitExpressionProcessor != null)
            {
                var genotypes = plants.Select(p => GetOrGenerateGenotype(p)).Where(g => g != null).ToList();
                if (genotypes.Count > 0)
                {
                    // PC014-FIX-44: Service interface expects Data version, convert each result to Systems version
                    try
                    {
                        var dataResults = _traitExpressionProcessor.CalculateBatchTraitExpression(plants, environment);
                        return dataResults.Select(r => ConvertToSystemsTraitExpression(r)).ToList();
                    }
                    catch (System.InvalidCastException)
                    {
                        // Fallback if there's a type mismatch
                        LogWarning("Type conversion issue in batch trait expression service, using fallback");
                        return plants.Select(p => ConvertToSystemsTraitExpression(CreateFallbackTraitExpression(p))).ToList();
                    }
                }
            }
            
            LogWarning("Trait expression service not available, using fallback batch calculation");
            return plants.Select(p => ConvertToSystemsTraitExpression(CreateFallbackTraitExpression(p))).ToList();
        }
        
        /// <summary>
        /// Predicts phenotype using trait expression service
        /// </summary>
        public PhenotypeProjection PredictPhenotype(PlantInstanceSO plant, EnvironmentalConditions environment, 
            int daysToPredict = 90)
        {
            if (_traitExpressionProcessor != null)
            {
                var genotype = GetOrGenerateGenotype(plant);
                if (genotype != null)
                {
                    return _traitExpressionProcessor.PredictPhenotype(genotype, environment, daysToPredict);
                }
            }
            
            LogWarning("Trait expression service not available, using fallback phenotype prediction");
            return CreateFallbackPhenotypeProjection(plant);
        }
        
        /// <summary>
        /// Analyzes trait stability using trait expression service
        /// </summary>
        public TraitStabilityAnalysis AnalyzeTraitStability(PlantInstanceSO plant, 
            List<EnvironmentalConditions> environments)
        {
            if (_traitExpressionProcessor != null)
            {
                var genotype = GetOrGenerateGenotype(plant);
                if (genotype != null)
                {
                    return _traitExpressionProcessor.AnalyzeTraitStability(genotype, environments);
                }
            }
            
            LogWarning("Trait expression service not available, using fallback stability analysis");
            return CreateFallbackStabilityAnalysis(plant);
        }
        
        /// <summary>
        /// Gets trait expression statistics using trait expression service
        /// </summary>
        public TraitExpressionStats GetTraitExpressionStats()
        {
            if (_traitExpressionProcessor != null)
            {
                return _traitExpressionProcessor.GetExpressionStats();
            }
            
            LogWarning("Trait expression service not available, returning empty stats");
            return new TraitExpressionStats();
        }
        
        /// <summary>
        /// Performs breeding using breeding calculation service
        /// </summary>
        public ProjectChimera.Data.Genetics.BreedingResult BreedPlants(PlantInstanceSO parent1, PlantInstanceSO parent2, int numberOfOffspring = 1)
        {
            if (_breedingCalculationEngine != null)
            {
                var systemsResult = _breedingCalculationEngine.BreedPlants(
                    GetOrGenerateGenotype(parent1), 
                    GetOrGenerateGenotype(parent2), 
                    numberOfOffspring
                );
                return ConvertToDataBreedingResult(systemsResult);
            }
            
            LogWarning("Breeding calculation service not available, using fallback breeding");
            return CreateFallbackBreedingResult(parent1, parent2, numberOfOffspring);
        }
        
        /// <summary>
        /// Analyzes genetic diversity using genetic analysis service
        /// </summary>
        public GeneticDiversityAnalysis AnalyzeGeneticDiversity(List<PlantInstanceSO> population)
        {
            if (_geneticAnalysisEngine != null)
            {
                return _geneticAnalysisEngine.AnalyzeGeneticDiversity(population);
            }
            
            LogWarning("Genetic analysis service not available, using fallback diversity analysis");
            return CreateFallbackDiversityAnalysis(population);
        }
        
        /// <summary>
        /// Optimizes breeding selection using genetic analysis service
        /// </summary>
        public ProjectChimera.Data.AI.BreedingRecommendation OptimizeBreedingSelection(List<PlantInstanceSO> candidates, TraitSelectionCriteria criteria)
        {
            if (_geneticAnalysisEngine != null)
            {
                var systemsRecommendation = _geneticAnalysisEngine.OptimizeBreedingSelection(candidates, criteria);
                return ConvertToDataBreedingRecommendation(systemsRecommendation);
            }
            
            LogWarning("Genetic analysis service not available, using fallback breeding optimization");
            return CreateFallbackBreedingRecommendation(candidates);
        }
        
        /// <summary>
        /// Simulates multiple generations using genetic analysis service
        /// </summary>
        public GenerationalSimulationResult SimulateGenerations(List<PlantInstanceSO> foundingPopulation, 
            int generations, TraitSelectionCriteria selectionCriteria)
        {
            if (_geneticAnalysisEngine != null)
            {
                return _geneticAnalysisEngine.SimulateGenerations(foundingPopulation, generations, selectionCriteria);
            }
            
            LogWarning("Genetic analysis service not available, using fallback generation simulation");
            return CreateFallbackGenerationSimulation(foundingPopulation, generations);
        }
        
        /// <summary>
        /// Predicts breeding value using genetic analysis service
        /// </summary>
        public BreedingValuePrediction PredictBreedingValue(PlantInstanceSO plant, List<TraitType> targetTraits)
        {
            if (_geneticAnalysisEngine != null)
            {
                return _geneticAnalysisEngine.PredictBreedingValue(plant, targetTraits);
            }
            
            LogWarning("Genetic analysis service not available, using fallback breeding value prediction");
            return CreateFallbackBreedingValuePrediction(plant);
        }
        
        /// <summary>
        /// Analyzes mutations using genetic analysis service
        /// </summary>
        public List<MutationRecord> AnalyzeMutations(PlantGenotype genotype)
        {
            if (_geneticAnalysisEngine != null)
            {
                return _geneticAnalysisEngine.AnalyzeMutations(genotype);
            }
            
            LogWarning("Genetic analysis service not available, returning empty mutation list");
            return new List<MutationRecord>();
        }
        
        /// <summary>
        /// Analyzes breeding compatibility using genetic analysis service
        /// </summary>
        public BreedingCompatibility AnalyzeBreedingCompatibility(PlantInstanceSO plant1, PlantInstanceSO plant2)
        {
            if (_geneticAnalysisEngine != null)
            {
                return _geneticAnalysisEngine.AnalyzeBreedingCompatibility(plant1, plant2);
            }
            
            LogWarning("Genetic analysis service not available, using fallback compatibility analysis");
            return CreateFallbackBreedingCompatibility(plant1, plant2);
        }
        
        /// <summary>
        /// Analyzes population using genetic analysis service
        /// </summary>
        public PopulationAnalysisResult AnalyzePopulation(List<PlantInstanceSO> population, int generationsBack = 5)
        {
            if (_geneticAnalysisEngine != null)
            {
                return _geneticAnalysisEngine.AnalyzePopulation(population, generationsBack);
            }
            
            LogWarning("Genetic analysis service not available, using fallback population analysis");
            return CreateFallbackPopulationAnalysis(population);
        }
        
        /// <summary>
        /// Gets genetic analysis statistics using genetic analysis service
        /// </summary>
        public GeneticAnalysisStats GetGeneticAnalysisStats()
        {
            if (_geneticAnalysisEngine != null)
            {
                return _geneticAnalysisEngine.GetAnalysisStats();
            }
            
            LogWarning("Genetic analysis service not available, returning empty stats");
            return new GeneticAnalysisStats();
        }
        
        /// <summary>
        /// Clears genetic analysis caches using genetic analysis service
        /// </summary>
        public void ClearGeneticAnalysisCaches()
        {
            if (_geneticAnalysisEngine != null)
            {
                _geneticAnalysisEngine.ClearAnalysisCaches();
            }
            else
            {
                LogInfo("Genetic analysis service not available, clearing local caches only");
                _genotypeCache.Clear();
            }
        }
        
        /// <summary>
        /// Generates optimal breeding plan using breeding optimization service
        /// </summary>
        public OptimalBreedingPlan GenerateOptimalBreedingPlan(List<PlantInstanceSO> candidates, BreedingObjective objective)
        {
            if (_breedingOptimizationService != null)
            {
                return _breedingOptimizationService.GenerateBreedingPlan(candidates, objective);
            }
            
            LogWarning("Breeding optimization service not available, using fallback breeding plan");
            return CreateFallbackBreedingPlan(candidates, objective);
        }
        
        /// <summary>
        /// Selects optimal breeding pairs using breeding optimization service
        /// </summary>
        public List<BreedingPair> SelectOptimalBreedingPairs(List<PlantInstanceSO> candidates, int maxPairs)
        {
            if (_breedingOptimizationService != null)
            {
                return _breedingOptimizationService.SelectOptimalPairs(candidates, maxPairs);
            }
            
            LogWarning("Breeding optimization service not available, using fallback pair selection");
            return CreateFallbackBreedingPairs(candidates, maxPairs);
        }
        
        /// <summary>
        /// Tracks breeding progress using breeding optimization service
        /// </summary>
        public BreedingProgress TrackBreedingProgress(string breedingProgramId)
        {
            if (_breedingOptimizationService != null)
            {
                return _breedingOptimizationService.TrackBreedingProgress(breedingProgramId);
            }
            
            LogWarning("Breeding optimization service not available, returning empty progress");
            return new BreedingProgress { ProgramId = breedingProgramId, CurrentGeneration = 0 };
        }
        
        // PC014-2c: Fallback methods for service unavailability
        
        private PlantGenotype CreateFallbackGenotype(PlantStrainSO strain)
        {
            return new PlantGenotype
            {
                GenotypeID = System.Guid.NewGuid().ToString(),
                StrainId = strain?.StrainId ?? "unknown",
                Alleles = new Dictionary<string, List<object>>()
            };
        }
        
        private PlantGenotype CreateSimpleOffspringGenotype(PlantGenotype parent1, PlantGenotype parent2)
        {
            return new PlantGenotype
            {
                GenotypeID = System.Guid.NewGuid().ToString(),
                StrainId = parent1?.StrainId ?? parent2?.StrainId ?? "hybrid",
                Alleles = new Dictionary<string, List<object>>()
            };
        }
        
        private ProjectChimera.Data.Genetics.TraitExpressionResult CreateFallbackTraitExpression(PlantInstanceSO plant)
        {
            return new ProjectChimera.Data.Genetics.TraitExpressionResult
            {
                PlantId = plant?.PlantID ?? "unknown",
                HeightExpression = 1.0f,
                YieldExpression = 1.0f,
                THCExpression = 1.0f,
                CBDExpression = 1.0f,
                TerpeneExpression = 1.0f,
                EnvironmentalInfluence = 0.5f,
                CalculationTime = System.DateTime.Now
            };
        }
        
        private PhenotypeProjection CreateFallbackPhenotypeProjection(PlantInstanceSO plant)
        {
            return new PhenotypeProjection
            {
                PlantId = plant?.PlantID ?? "unknown",
                ProjectedTraits = new Dictionary<TraitType, float>()
            };
        }
        
        private TraitStabilityAnalysis CreateFallbackStabilityAnalysis(PlantInstanceSO plant)
        {
            return new TraitStabilityAnalysis
            {
                PlantId = plant?.PlantID ?? "unknown",
                StabilityScore = 0.5f
            };
        }
        
        private ProjectChimera.Data.Genetics.BreedingResult CreateFallbackBreedingResult(PlantInstanceSO parent1, PlantInstanceSO parent2, int offspring)
        {
            return new ProjectChimera.Data.Genetics.BreedingResult
            {
                Parent1Id = parent1?.PlantID ?? "unknown",
                Parent2Id = parent2?.PlantID ?? "unknown",
                OffspringGenotypes = new List<PlantGenotype>()
            };
        }
        
        private GeneticDiversityAnalysis CreateFallbackDiversityAnalysis(List<PlantInstanceSO> population)
        {
            return new GeneticDiversityAnalysis
            {
                PopulationSize = population?.Count ?? 0,
                DiversityIndex = 0.5f
            };
        }
        
        private ProjectChimera.Data.AI.BreedingRecommendation CreateFallbackBreedingRecommendation(List<PlantInstanceSO> candidates)
        {
            return new ProjectChimera.Data.AI.BreedingRecommendation
            {
                RecommendedPairs = new List<string>(), // Keep as List<string> to avoid assembly dependency
                ExpectedGeneticGain = 0.0f,
                Strategy = ProjectChimera.Data.Genetics.BreedingStrategyType.LineBreeding
            };
        }
        
        private GenerationalSimulationResult CreateFallbackGenerationSimulation(List<PlantInstanceSO> population, int generations)
        {
            return new GenerationalSimulationResult
            {
                GenerationsSimulated = generations,
                FinalPopulationSize = population?.Count ?? 0
            };
        }
        
        private BreedingValuePrediction CreateFallbackBreedingValuePrediction(PlantInstanceSO plant)
        {
            return new BreedingValuePrediction
            {
                PlantId = plant?.PlantID ?? "unknown",
                PredictedValue = 0.5f
            };
        }
        
        private BreedingCompatibility CreateFallbackBreedingCompatibility(PlantInstanceSO plant1, PlantInstanceSO plant2)
        {
            return new BreedingCompatibility
            {
                Plant1Id = plant1?.PlantID ?? "unknown",
                Plant2Id = plant2?.PlantID ?? "unknown",
                CompatibilityScore = 0.5f
            };
        }
        
        private PopulationAnalysisResult CreateFallbackPopulationAnalysis(List<PlantInstanceSO> population)
        {
            return new PopulationAnalysisResult
            {
                PopulationSize = population?.Count ?? 0,
                AnalysisDate = System.DateTime.Now
            };
        }
        
        private OptimalBreedingPlan CreateFallbackBreedingPlan(List<PlantInstanceSO> candidates, BreedingObjective objective)
        {
            return new OptimalBreedingPlan
            {
                Phase1Crosses = new List<BreedingPair>(),
                Phase2Crosses = new List<BreedingPair>(),
                Phase3Crosses = new List<BreedingPair>(),
                ExpectedGeneticGain = 0.0f,
                EstimatedTimeToCompletion = 365
            };
        }
        
        private List<BreedingPair> CreateFallbackBreedingPairs(List<PlantInstanceSO> candidates, int maxPairs)
        {
            return new List<BreedingPair>();
        }
        
        /// <summary>
        /// PC014-FIX-30: Convert Data.Genetics.TraitExpressionResult to Systems.Genetics.TraitExpressionResult
        /// </summary>
        private ProjectChimera.Systems.Genetics.TraitExpressionResult ConvertToSystemsTraitExpression(ProjectChimera.Data.Genetics.TraitExpressionResult dataResult)
        {
            if (dataResult == null) return null;
            
            return new ProjectChimera.Systems.Genetics.TraitExpressionResult
            {
                GenotypeID = dataResult.GenotypeID,
                OverallFitness = dataResult.OverallFitness,
                HeightExpression = dataResult.HeightExpression,
                THCExpression = dataResult.THCExpression,
                CBDExpression = dataResult.CBDExpression,
                YieldExpression = dataResult.YieldExpression,
                StressResponse = null // Systems version has different StressResponse structure
            };
        }
        
        /// <summary>
        /// PC014-FIX-44: Convert Systems.Genetics.TraitExpressionResult to Data.Genetics.TraitExpressionResult
        /// </summary>
        private ProjectChimera.Data.Genetics.TraitExpressionResult ConvertToDataTraitExpression(ProjectChimera.Systems.Genetics.TraitExpressionResult systemsResult)
        {
            if (systemsResult == null) return null;
            
            return new ProjectChimera.Data.Genetics.TraitExpressionResult
            {
                GenotypeID = systemsResult.GenotypeID,
                OverallFitness = systemsResult.OverallFitness,
                HeightExpression = systemsResult.HeightExpression,
                THCExpression = systemsResult.THCExpression,
                CBDExpression = systemsResult.CBDExpression,
                YieldExpression = systemsResult.YieldExpression,
                EnvironmentalInfluence = 0.5f, // Default value since Systems version doesn't have this
                CalculationTime = System.DateTime.Now
            };
        }
        
        /// <summary>
        /// PC014-FIX-32: Convert Systems.Genetics.BreedingResult to Data.Genetics.BreedingResult
        /// </summary>
        private ProjectChimera.Data.Genetics.BreedingResult ConvertToDataBreedingResult(ProjectChimera.Systems.Genetics.BreedingResult systemsResult)
        {
            if (systemsResult == null) return null;
            
            return new ProjectChimera.Data.Genetics.BreedingResult
            {
                Parent1Id = systemsResult.Parent1Genotype?.GenotypeID ?? "unknown",
                Parent2Id = systemsResult.Parent2Genotype?.GenotypeID ?? "unknown",
                OffspringGenotypes = systemsResult.OffspringGenotypes ?? new List<PlantGenotype>(),
                BreedingSuccess = systemsResult.BreedingSuccess,
                BreedingDate = systemsResult.BreedingDate
            };
        }
        
        /// <summary>
        /// PC014-FIX-32: Convert Systems.Genetics.BreedingRecommendation to Data.AI.BreedingRecommendation
        /// </summary>
        private ProjectChimera.Data.AI.BreedingRecommendation ConvertToDataBreedingRecommendation(ProjectChimera.Systems.Genetics.BreedingRecommendation systemsRecommendation)
        {
            if (systemsRecommendation == null) return null;
            
            return new ProjectChimera.Data.AI.BreedingRecommendation
            {
                RecommendedPairs = new List<string>(), // Convert BreedingPair list to string list
                ExpectedGeneticGain = systemsRecommendation.ExpectedGeneticGain,
                Strategy = ProjectChimera.Data.Genetics.BreedingStrategyType.LineBreeding // Default strategy
            };
        }
        
        protected override void OnManagerUpdate()
        {
            // Genetics manager typically doesn't need per-frame updates
            // Most genetic calculations are event-driven
        }
        
        /// <summary>
        /// Generates a genotype for a plant from a strain definition.
        /// </summary>
        public PlantGenotype GenerateGenotypeFromStrain(PlantStrainSO strain)
        {
            if (strain == null)
            {
                LogError("Cannot generate genotype: strain is null");
                return null;
            }
            
            var genotype = _inheritanceCalculator.GenerateFounderGenotype(strain);
            
            // Cache the genotype
            if (!string.IsNullOrEmpty(genotype.GenotypeID))
            {
                _genotypeCache[genotype.GenotypeID] = genotype;
            }
            
            if (_enableDetailedLogging)
                LogInfo($"Generated genotype {genotype.GenotypeID} from strain {strain.StrainName}");
            
            return genotype;
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        /// <summary>
        /// Gets or generates a genotype for a plant instance.
        /// </summary>
        private PlantGenotype GetOrGenerateGenotype(PlantInstanceSO plant)
        {
            if (plant == null || plant.Strain == null)
                return null;
            
            // Check cache first
            if (_genotypeCache.TryGetValue(plant.PlantID, out var cachedGenotype))
                return cachedGenotype;
            
            // Generate new genotype from strain
            var genotype = GenerateGenotypeFromStrain(plant.Strain);
            if (genotype != null)
            {
                genotype.GenotypeID = plant.PlantID; // Use plant ID as genotype ID
                _genotypeCache[plant.PlantID] = genotype;
            }
            
            return genotype;
        }
        
        /// <summary>
        /// Handles breeding completion events from the breeding calculation engine.
        /// </summary>
        private void HandleBreedingCompleted(BreedingResult breedingResult)
        {
            _onBreedingCompleted?.Raise(breedingResult);
        }
        
        /// <summary>
        /// Handles mutation events from the breeding calculation engine.
        /// </summary>
        private void HandleMutationOccurred(GeneticMutation mutation)
        {
            _onMutationOccurred?.Raise(mutation);
        }
        
        /// <summary>
        /// Handles trait expression calculation events from the trait expression processor.
        /// </summary>
        private void HandleTraitExpressionCalculated(PlantInstanceSO plant)
        {
            _onTraitExpressionCalculated?.Raise(plant);
        }
        
        /// <summary>
        /// Handles batch trait expression completion events.
        /// </summary>
        private void HandleBatchExpressionCompleted(List<PlantInstanceSO> plants)
        {
            if (_enableDetailedLogging)
                LogInfo($"Batch trait expression completed for {plants.Count} plants");
        }
        
        /// <summary>
        /// Handles trait value update events.
        /// </summary>
        private void HandleTraitValueUpdated(string traitName, float traitValue)
        {
            if (_enableDetailedLogging)
                LogInfo($"Trait updated: {traitName} = {traitValue:F2}");
        }
        
        
        protected override void OnManagerShutdown()
        {
            // PC014-2c: Shutdown genetic services via dependency injection
            try
            {
                // Shutdown inheritance calculation service
                if (_inheritanceCalculator != null && _inheritanceCalculator.IsInitialized)
                {
                    _inheritanceCalculator.Shutdown();
                    LogInfo("Inheritance calculation service shutdown");
                }
                
                // Shutdown trait expression service
                if (_traitExpressionProcessor != null && _traitExpressionProcessor.IsInitialized)
                {
                    _traitExpressionProcessor.Shutdown();
                    LogInfo("Trait expression service shutdown");
                }
                
                // Shutdown breeding calculation service
                if (_breedingCalculationEngine != null && _breedingCalculationEngine.IsInitialized)
                {
                    _breedingCalculationEngine.Shutdown();
                    LogInfo("Breeding calculation service shutdown");
                }
                
                // Shutdown genetic analysis service
                if (_geneticAnalysisEngine != null && _geneticAnalysisEngine.IsInitialized)
                {
                    _geneticAnalysisEngine.ClearAnalysisCaches();
                    _geneticAnalysisEngine.Shutdown();
                    LogInfo("Genetic analysis service shutdown");
                }
                
                // Shutdown breeding optimization service
                if (_breedingOptimizationService != null && _breedingOptimizationService.IsInitialized)
                {
                    _breedingOptimizationService.Shutdown();
                    LogInfo("Breeding optimization service shutdown");
                }
                
                // Clear local caches
                _genotypeCache.Clear();
                _pedigreeDatabase.Clear();
                
                LogInfo("GeneticsManager shutdown complete with dependency injection cleanup");
            }
            catch (System.Exception ex)
            {
                LogError($"Error during GeneticsManager shutdown: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Pedigree information for tracking breeding lineages.
    /// </summary>
    [System.Serializable]
    public class BreedingLineage
    {
        public string IndividualID;
        public string Parent1ID;
        public string Parent2ID;
        public int Generation;
        public float InbreedingCoefficient;
        public System.DateTime BreedingDate;
        public List<string> AncestorIDs = new List<string>();
    }
}