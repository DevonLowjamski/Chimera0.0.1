using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Breeding System Service - Handles cannabis breeding operations, Mendelian inheritance, and crossover mechanics
    /// Extracted from CannabisGeneticsEngine to provide focused breeding functionality
    /// Manages breeding simulations, genetic recombination, lineage tracking, and breeding analytics
    /// Implements scientific breeding principles with hybrid vigor, genetic diversity, and trait inheritance
    /// </summary>
    public class BreedingSystemService : MonoBehaviour
    {
        [Header("Breeding Configuration")]
        [SerializeField] private bool _enableMendelianInheritance = true;
        [SerializeField] private bool _enableAdvancedBreeding = true;
        [SerializeField] private bool _enableBackcrossing = true;
        [SerializeField] private bool _enableLineBreeding = true;
        [SerializeField] private int _maxBreedingGenerations = 10;

        [Header("Breeding Parameters")]
        [SerializeField] private float _hybridVigorMultiplier = 1.2f;
        [SerializeField] private float _inbreedingPenalty = 0.8f;
        [SerializeField] private float _outcrossBonus = 1.1f;
        [SerializeField] private bool _enableBreedingLogging = false;

        [Header("Genetic Variation Settings")]
        [SerializeField] private float _crossoverRate = 0.5f;
        [SerializeField] private float _geneRecombinationRate = 0.3f;
        [SerializeField] private bool _enableHybridVigor = true;
        [SerializeField] private bool _enableGeneticDrift = true;

        [Header("Breeding Analytics")]
        [SerializeField] private bool _enableBreedingAnalytics = true;
        [SerializeField] private bool _enableLineageTracking = true;
        [SerializeField] private bool _enableDiversityAnalysis = true;
        [SerializeField] private int _maxBreedingHistory = 1000;

        // Service state
        private bool _isInitialized = false;
        private ScriptableObject _breedingConfig;
        private ScriptableObject _geneLibrary;

        // Breeding data
        private Dictionary<string, BreedingRecord> _breedingHistory = new Dictionary<string, BreedingRecord>();
        private Dictionary<string, GeneticLineage> _lineageTracker = new Dictionary<string, GeneticLineage>();
        private Dictionary<string, BreedingOperation> _activeBreedingOperations = new Dictionary<string, BreedingOperation>();
        private Queue<BreedingOperation> _breedingQueue = new Queue<BreedingOperation>();

        // Analytics data
        private BreedingAnalytics _breedingAnalytics = new BreedingAnalytics();
        private Dictionary<string, float> _strainCompatibility = new Dictionary<string, float>();
        private Dictionary<string, Dictionary<string, float>> _traitCombinationResults = new Dictionary<string, Dictionary<string, float>>();

        // Performance tracking
        private int _totalBreedingOperations = 0;
        private int _successfulBreedings = 0;
        private int _failedBreedings = 0;
        private float _averageBreedingTime = 0f;

        // Events
        public static event Action<BreedingResult> OnBreedingCompleted;
        public static event Action<string> OnBreedingFailed;
        public static event Action<CannabisGenotype> OnOffspringGenerated;
        public static event Action<GeneticLineage> OnLineageUpdated;
        public static event Action<string> OnBreedingError;
        public static event Action<BreedingAnalytics> OnBreedingAnalyticsUpdated;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Breeding System Service";
        public int TotalBreedingOperations => _totalBreedingOperations;
        public int ActiveBreedingOperations => _activeBreedingOperations.Count;
        public int QueuedBreedings => _breedingQueue.Count;
        public int BreedingHistoryCount => _breedingHistory.Count;

        public void Initialize(ScriptableObject breedingConfig = null, ScriptableObject geneLibrary = null)
        {
            InitializeService(breedingConfig, geneLibrary);
        }

        public void Shutdown()
        {
            ShutdownService();
        }

        #endregion

        #region Service Lifecycle

        private void Awake()
        {
            InitializeDataStructures();
        }

        private void Start()
        {
            // Service will be initialized by orchestrator
        }

        private void Update()
        {
            if (_isInitialized)
            {
                ProcessBreedingQueue();
                UpdateActiveBreedingOperations();
                UpdateAnalytics();
            }
        }

        private void InitializeDataStructures()
        {
            _breedingHistory = new Dictionary<string, BreedingRecord>();
            _lineageTracker = new Dictionary<string, GeneticLineage>();
            _activeBreedingOperations = new Dictionary<string, BreedingOperation>();
            _breedingQueue = new Queue<BreedingOperation>();
            _breedingAnalytics = new BreedingAnalytics();
            _strainCompatibility = new Dictionary<string, float>();
            _traitCombinationResults = new Dictionary<string, Dictionary<string, float>>();
        }

        public void InitializeService(ScriptableObject breedingConfig = null, ScriptableObject geneLibrary = null)
        {
            if (_isInitialized)
            {
                if (_enableBreedingLogging)
                    Debug.LogWarning("BreedingSystemService already initialized");
                return;
            }

            try
            {
                _breedingConfig = breedingConfig;
                _geneLibrary = geneLibrary;

                InitializeBreedingAnalytics();
                
                _isInitialized = true;
                
                if (_enableBreedingLogging)
                    Debug.Log("BreedingSystemService initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize BreedingSystemService: {ex.Message}");
                OnBreedingError?.Invoke($"Service initialization failed: {ex.Message}");
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            try
            {
                // Complete any active breeding operations
                CompleteActiveBreedings();
                
                // Save breeding analytics
                if (_enableBreedingAnalytics)
                {
                    SaveBreedingData();
                }
                
                // Clear all data
                ClearAllData();
                
                _isInitialized = false;
                
                if (_enableBreedingLogging)
                    Debug.Log("BreedingSystemService shutdown completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during BreedingSystemService shutdown: {ex.Message}");
            }
        }

        #endregion

        #region Breeding Operations

        /// <summary>
        /// Perform breeding between two cannabis genotypes
        /// </summary>
        public BreedingResult PerformBreeding(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method = BreedingMethod.StandardCross)
        {
            if (!_isInitialized || parent1 == null || parent2 == null)
            {
                var errorResult = new BreedingResult
                {
                    Success = false,
                    ErrorMessage = "Invalid breeding parameters or service not initialized",
                    CompletionTime = DateTime.Now
                };
                OnBreedingError?.Invoke(errorResult.ErrorMessage);
                return errorResult;
            }

            try
            {
                var startTime = DateTime.Now;
                
                if (_enableBreedingLogging)
                    Debug.Log($"Starting breeding: {parent1.StrainName} x {parent2.StrainName} using {method}");

                // Validate breeding compatibility
                if (!ValidateBreedingCompatibility(parent1, parent2, method))
                {
                    var errorResult = new BreedingResult
                    {
                        Success = false,
                        ErrorMessage = "Breeding compatibility validation failed",
                        Parent1Genotype = parent1,
                        Parent2Genotype = parent2,
                        BreedingMethod = method,
                        CompletionTime = DateTime.Now
                    };
                    _failedBreedings++;
                    return errorResult;
                }

                // Generate offspring
                var offspring = GenerateOffspring(parent1, parent2, method);
                var breedingTime = (float)(DateTime.Now - startTime).TotalSeconds;

                // Create breeding result
                var result = new BreedingResult
                {
                    Success = true,
                    OffspringCount = offspring.Count,
                    OffspringGenotypes = offspring,
                    BreedingTime = breedingTime,
                    Parent1Genotype = parent1,
                    Parent2Genotype = parent2,
                    BreedingMethod = method,
                    HybridVigorFactor = CalculateHybridVigorFactor(parent1, parent2, method),
                    TraitPredictions = PredictOffspringTraits(parent1, parent2, method),
                    CompletionTime = DateTime.Now
                };

                // Record breeding operation
                RecordBreedingOperation(parent1, parent2, result, method);
                
                // Update lineage tracking
                if (_enableLineageTracking)
                {
                    UpdateLineageTracking(offspring, parent1, parent2);
                }

                // Update analytics
                _successfulBreedings++;
                _totalBreedingOperations++;
                _averageBreedingTime = (_averageBreedingTime * (_totalBreedingOperations - 1) + breedingTime) / _totalBreedingOperations;

                // Fire events
                OnBreedingCompleted?.Invoke(result);
                foreach (var offspringGenotype in offspring)
                {
                    OnOffspringGenerated?.Invoke(offspringGenotype);
                }

                if (_enableBreedingLogging)
                    Debug.Log($"Breeding completed successfully: {offspring.Count} offspring generated in {breedingTime:F2} seconds");

                return result;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during breeding operation: {ex.Message}");
                OnBreedingError?.Invoke($"Breeding failed: {ex.Message}");
                
                _failedBreedings++;
                _totalBreedingOperations++;
                
                return new BreedingResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Parent1Genotype = parent1,
                    Parent2Genotype = parent2,
                    BreedingMethod = method,
                    CompletionTime = DateTime.Now
                };
            }
        }

        /// <summary>
        /// Queue breeding operation for batch processing
        /// </summary>
        public string QueueBreeding(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method = BreedingMethod.StandardCross)
        {
            if (!_isInitialized || parent1 == null || parent2 == null)
            {
                OnBreedingError?.Invoke("Cannot queue breeding - invalid parameters");
                return null;
            }

            var operationId = Guid.NewGuid().ToString();
            var operation = new BreedingOperation
            {
                OperationId = operationId,
                Parent1 = parent1,
                Parent2 = parent2,
                Method = method,
                QueueTime = DateTime.Now,
                Status = BreedingStatus.Queued
            };

            _breedingQueue.Enqueue(operation);
            
            if (_enableBreedingLogging)
                Debug.Log($"Breeding operation queued: {operationId}");

            return operationId;
        }

        /// <summary>
        /// Generate offspring from two parent genotypes
        /// </summary>
        private List<CannabisGenotype> GenerateOffspring(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method)
        {
            var offspring = new List<CannabisGenotype>();
            
            // Determine number of offspring based on method
            int offspringCount = GetOffspringCount(method);
            
            for (int i = 0; i < offspringCount; i++)
            {
                var offspringGenotype = CreateOffspring(parent1, parent2, method, i);
                
                // Apply genetic recombination
                PerformGeneticRecombination(offspringGenotype, parent1, parent2, method);
                
                // Apply hybrid vigor if applicable
                if (_enableHybridVigor && IsOutcross(parent1, parent2))
                {
                    ApplyHybridVigor(offspringGenotype, _hybridVigorMultiplier);
                }
                
                // Apply inbreeding effects if applicable
                if (IsInbred(parent1, parent2))
                {
                    ApplyInbreedingEffects(offspringGenotype, _inbreedingPenalty);
                }
                
                offspring.Add(offspringGenotype);
            }
            
            return offspring;
        }

        /// <summary>
        /// Create a single offspring genotype
        /// </summary>
        private CannabisGenotype CreateOffspring(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method, int offspringIndex)
        {
            var offspring = new CannabisGenotype
            {
                GenotypeId = Guid.NewGuid().ToString(),
                StrainId = GenerateOffspringStrainId(parent1, parent2, method),
                StrainName = GenerateOffspringStrainName(parent1, parent2, method, offspringIndex),
                Generation = Math.Max(parent1.Generation, parent2.Generation) + 1,
                CreationDate = DateTime.Now,
                Origin = GenotypeOrigin.Bred,
                IsHybrid = parent1.StrainId != parent2.StrainId,
                ParentGenotypes = new List<string> { parent1.GenotypeId, parent2.GenotypeId }
            };

            // Default traits will be initialized by the constructor
            
            return offspring;
        }

        /// <summary>
        /// Perform genetic recombination between parent genotypes
        /// </summary>
        private void PerformGeneticRecombination(CannabisGenotype offspring, CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method)
        {
            if (!_enableMendelianInheritance)
                return;

            // Initialize offspring alleles dictionary
            offspring.Alleles = new Dictionary<string, List<object>>();

            // Get all gene IDs from both parents
            var allGeneIds = new HashSet<string>();
            if (parent1.Alleles != null) allGeneIds.UnionWith(parent1.Alleles.Keys);
            if (parent2.Alleles != null) allGeneIds.UnionWith(parent2.Alleles.Keys);

            foreach (var geneId in allGeneIds)
            {
                var parent1Alleles = GetAllelesForGene(parent1, geneId);
                var parent2Alleles = GetAllelesForGene(parent2, geneId);
                
                // Perform Mendelian inheritance
                var inheritedAlleles = PerformMendelianInheritance(parent1Alleles, parent2Alleles, geneId);
                
                // Apply crossover and recombination
                if (UnityEngine.Random.value < _crossoverRate)
                {
                    inheritedAlleles = ApplyCrossover(inheritedAlleles, geneId);
                }
                
                offspring.Alleles[geneId] = inheritedAlleles.Cast<object>().ToList();
            }

            // Update trait expressions based on inherited alleles
            UpdateTraitExpressions(offspring);
        }

        /// <summary>
        /// Perform Mendelian inheritance for a specific gene
        /// </summary>
        private List<Allele> PerformMendelianInheritance(List<Allele> parent1Alleles, List<Allele> parent2Alleles, string geneId)
        {
            var inheritedAlleles = new List<Allele>();

            // Simple Mendelian inheritance: take one allele from each parent
            if (parent1Alleles.Count > 0 && parent2Alleles.Count > 0)
            {
                // Select random allele from each parent
                var parent1Allele = parent1Alleles[UnityEngine.Random.Range(0, parent1Alleles.Count)];
                var parent2Allele = parent2Alleles[UnityEngine.Random.Range(0, parent2Alleles.Count)];
                
                // Create new allele instances for offspring
                var offspringAllele1 = CreateInheritedAllele(parent1Allele, geneId);
                var offspringAllele2 = CreateInheritedAllele(parent2Allele, geneId);
                
                inheritedAlleles.Add(offspringAllele1);
                inheritedAlleles.Add(offspringAllele2);
            }
            else if (parent1Alleles.Count > 0)
            {
                // Only parent 1 has alleles for this gene
                var inheritedAllele = CreateInheritedAllele(parent1Alleles[0], geneId);
                inheritedAlleles.Add(inheritedAllele);
            }
            else if (parent2Alleles.Count > 0)
            {
                // Only parent 2 has alleles for this gene
                var inheritedAllele = CreateInheritedAllele(parent2Alleles[0], geneId);
                inheritedAlleles.Add(inheritedAllele);
            }

            return inheritedAlleles;
        }

        /// <summary>
        /// Create an inherited allele from a parent allele
        /// </summary>
        private Allele CreateInheritedAllele(Allele parentAllele, string geneId)
        {
            return new Allele
            {
                AlleleId = Guid.NewGuid().ToString(),
                GeneId = geneId,
                AlleleName = $"Inherited_{parentAllele.AlleleName}",
                Expression = parentAllele.Expression,
                Dominance = parentAllele.Dominance,
                Stability = parentAllele.Stability,
                MutationRate = parentAllele.MutationRate,
                ColorValue = parentAllele.ColorValue,
                MorphologyValue = parentAllele.MorphologyValue,
                Properties = new Dictionary<string, float>(parentAllele.Properties),
                StrainTypeInfluence = new Dictionary<CannabisStrainType, float>(parentAllele.StrainTypeInfluence),
                CreationDate = DateTime.Now,
                Origin = $"Inherited from {parentAllele.AlleleId}"
            };
        }

        #endregion

        #region Breeding Analysis

        /// <summary>
        /// Validate breeding compatibility between two genotypes
        /// </summary>
        private bool ValidateBreedingCompatibility(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method)
        {
            // Basic validation checks
            if (parent1.GenotypeId == parent2.GenotypeId)
            {
                if (_enableBreedingLogging)
                    Debug.LogWarning("Cannot breed genotype with itself (unless self-pollination)");
                return method == BreedingMethod.SelfPollination;
            }

            // Check generation limits
            if (parent1.Generation >= _maxBreedingGenerations || parent2.Generation >= _maxBreedingGenerations)
            {
                if (_enableBreedingLogging)
                    Debug.LogWarning($"Breeding generation limit reached: {_maxBreedingGenerations}");
                return false;
            }

            // Check strain compatibility
            var compatibilityKey = $"{parent1.StrainId}_{parent2.StrainId}";
            if (_strainCompatibility.ContainsKey(compatibilityKey))
            {
                return _strainCompatibility[compatibilityKey] > 0.5f;
            }

            // Calculate and cache compatibility
            var compatibility = CalculateStrainCompatibility(parent1, parent2);
            _strainCompatibility[compatibilityKey] = compatibility;

            return compatibility > 0.3f; // Minimum compatibility threshold
        }

        /// <summary>
        /// Calculate compatibility between two strains
        /// </summary>
        private float CalculateStrainCompatibility(CannabisGenotype parent1, CannabisGenotype parent2)
        {
            float compatibility = 0.5f; // Base compatibility

            // Check genetic diversity
            var geneticDistance = CalculateGeneticDistance(parent1, parent2);
            if (geneticDistance > 0.2f && geneticDistance < 0.8f)
            {
                compatibility += 0.2f; // Good genetic diversity
            }

            // Check trait compatibility
            var traitSimilarity = CalculateTraitSimilarity(parent1, parent2);
            compatibility += traitSimilarity * 0.3f;

            return Mathf.Clamp01(compatibility);
        }

        /// <summary>
        /// Calculate genetic distance between two genotypes
        /// </summary>
        private float CalculateGeneticDistance(CannabisGenotype parent1, CannabisGenotype parent2)
        {
            if (parent1.Alleles == null || parent2.Alleles == null)
                return 0.5f;

            int sharedGenes = 0;
            int totalGenes = 0;

            var allGenes = new HashSet<string>();
            allGenes.UnionWith(parent1.Alleles.Keys);
            allGenes.UnionWith(parent2.Alleles.Keys);

            foreach (var geneId in allGenes)
            {
                totalGenes++;
                
                var parent1HasGene = parent1.Alleles.ContainsKey(geneId);
                var parent2HasGene = parent2.Alleles.ContainsKey(geneId);
                
                if (parent1HasGene && parent2HasGene)
                {
                    sharedGenes++;
                }
            }

            return totalGenes > 0 ? 1f - ((float)sharedGenes / totalGenes) : 0.5f;
        }

        /// <summary>
        /// Calculate trait similarity between two genotypes
        /// </summary>
        private float CalculateTraitSimilarity(CannabisGenotype parent1, CannabisGenotype parent2)
        {
            if (parent1.Traits == null || parent2.Traits == null)
                return 0.5f;

            float totalSimilarity = 0f;
            int comparedTraits = 0;

            foreach (var trait1 in parent1.Traits)
            {
                var matchingTrait = parent2.Traits.FirstOrDefault(t => t.TraitName == trait1.TraitName);
                if (matchingTrait != null)
                {
                    var similarity = 1f - Math.Abs(trait1.ExpressedValue - matchingTrait.ExpressedValue) / 2f;
                    totalSimilarity += similarity;
                    comparedTraits++;
                }
            }

            return comparedTraits > 0 ? totalSimilarity / comparedTraits : 0.5f;
        }

        /// <summary>
        /// Predict offspring traits based on parent genetics
        /// </summary>
        private Dictionary<string, float> PredictOffspringTraits(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method)
        {
            var predictions = new Dictionary<string, float>();

            if (parent1.Traits != null && parent2.Traits != null)
            {
                var allTraitNames = parent1.Traits.Select(t => t.TraitName)
                    .Union(parent2.Traits.Select(t => t.TraitName))
                    .Distinct();

                foreach (var traitName in allTraitNames)
                {
                    var parent1Trait = parent1.Traits.FirstOrDefault(t => t.TraitName == traitName);
                    var parent2Trait = parent2.Traits.FirstOrDefault(t => t.TraitName == traitName);

                    float predictedValue = PredictTraitValue(parent1Trait, parent2Trait, method);
                    predictions[traitName] = predictedValue;
                }
            }

            return predictions;
        }

        /// <summary>
        /// Predict trait value for offspring
        /// </summary>
        private float PredictTraitValue(GeneticTrait parent1Trait, GeneticTrait parent2Trait, BreedingMethod method)
        {
            if (parent1Trait == null && parent2Trait == null) return 1.0f;
            if (parent1Trait == null) return parent2Trait.ExpressedValue;
            if (parent2Trait == null) return parent1Trait.ExpressedValue;

            // Simple dominance-based prediction
            if (parent1Trait.Dominance == TraitDominance.Dominant)
                return parent1Trait.ExpressedValue;
            if (parent2Trait.Dominance == TraitDominance.Dominant)
                return parent2Trait.ExpressedValue;

            // Codominant or average inheritance
            return (parent1Trait.ExpressedValue + parent2Trait.ExpressedValue) / 2f;
        }

        #endregion

        #region Utility Methods

        private void ProcessBreedingQueue()
        {
            int processedCount = 0;
            int maxToProcess = 5; // Process up to 5 breeding operations per frame

            while (_breedingQueue.Count > 0 && processedCount < maxToProcess)
            {
                var operation = _breedingQueue.Dequeue();
                
                // Move to active operations
                _activeBreedingOperations[operation.OperationId] = operation;
                operation.Status = BreedingStatus.Processing;
                operation.StartTime = DateTime.Now;

                processedCount++;
            }
        }

        private void UpdateActiveBreedingOperations()
        {
            var completedOperations = new List<string>();

            foreach (var kvp in _activeBreedingOperations)
            {
                var operation = kvp.Value;
                
                // Simulate breeding time (in real implementation, this would be actual processing)
                var processingTime = (DateTime.Now - operation.StartTime).TotalSeconds;
                
                if (processingTime >= 0.1f) // Minimum processing time
                {
                    // Complete the breeding operation
                    var result = PerformBreeding(operation.Parent1, operation.Parent2, operation.Method);
                    operation.Status = result.Success ? BreedingStatus.Completed : BreedingStatus.Failed;
                    operation.Result = result;
                    
                    completedOperations.Add(operation.OperationId);
                }
            }

            // Remove completed operations
            foreach (var operationId in completedOperations)
            {
                _activeBreedingOperations.Remove(operationId);
            }
        }

        private void UpdateAnalytics()
        {
            if (!_enableBreedingAnalytics) return;

            var currentTime = DateTime.Now;
            if (_breedingAnalytics.LastAnalyticsUpdate == default || 
                (currentTime - _breedingAnalytics.LastAnalyticsUpdate).TotalMinutes >= 1)
            {
                UpdateBreedingAnalytics();
                _breedingAnalytics.LastAnalyticsUpdate = currentTime;
                
                OnBreedingAnalyticsUpdated?.Invoke(_breedingAnalytics);
            }
        }

        private void UpdateBreedingAnalytics()
        {
            _breedingAnalytics.TotalBreedingOperations = _totalBreedingOperations;
            _breedingAnalytics.SuccessfulBreedings = _successfulBreedings;
            _breedingAnalytics.FailedBreedings = _failedBreedings;
            _breedingAnalytics.AverageBreedingTime = _averageBreedingTime;
            
            // Update strain popularity
            _breedingAnalytics.StrainPopularity.Clear();
            foreach (var record in _breedingHistory.Values)
            {
                var parent1StrainKey = record.Parent1Id?.Split('_')[0] ?? "Unknown";
                var parent2StrainKey = record.Parent2Id?.Split('_')[0] ?? "Unknown";
                
                _breedingAnalytics.StrainPopularity[parent1StrainKey] = 
                    _breedingAnalytics.StrainPopularity.GetValueOrDefault(parent1StrainKey, 0) + 1;
                _breedingAnalytics.StrainPopularity[parent2StrainKey] = 
                    _breedingAnalytics.StrainPopularity.GetValueOrDefault(parent2StrainKey, 0) + 1;
            }
        }

        private void InitializeBreedingAnalytics()
        {
            _breedingAnalytics = new BreedingAnalytics
            {
                TotalBreedingOperations = 0,
                SuccessfulBreedings = 0,
                FailedBreedings = 0,
                AverageBreedingTime = 0f,
                StrainPopularity = new Dictionary<string, int>(),
                TraitFrequency = new Dictionary<string, float>(),
                GeneticDiversityTrend = 0f,
                LastAnalyticsUpdate = DateTime.Now
            };
        }

        private void RecordBreedingOperation(CannabisGenotype parent1, CannabisGenotype parent2, BreedingResult result, BreedingMethod method)
        {
            var record = new BreedingRecord
            {
                BreedingId = Guid.NewGuid().ToString(),
                Parent1Id = parent1.GenotypeId,
                Parent2Id = parent2.GenotypeId,
                Method = method,
                BreedingDate = DateTime.Now,
                OffspringIds = result.OffspringGenotypes?.Select(o => o.GenotypeId).ToList() ?? new List<string>(),
                Success = result.Success,
                BreedingTime = result.BreedingTime,
                Notes = $"Breeding using {method} method",
                TraitOutcomes = result.TraitPredictions ?? new Dictionary<string, float>()
            };

            _breedingHistory[record.BreedingId] = record;
            
            // Limit history size
            if (_breedingHistory.Count > _maxBreedingHistory)
            {
                var oldestRecord = _breedingHistory.Values.OrderBy(r => r.BreedingDate).First();
                _breedingHistory.Remove(oldestRecord.BreedingId);
            }
        }

        private void UpdateLineageTracking(List<CannabisGenotype> offspring, CannabisGenotype parent1, CannabisGenotype parent2)
        {
            foreach (var child in offspring)
            {
                var lineage = new GeneticLineage
                {
                    GenotypeId = child.GenotypeId,
                    ParentIds = new List<string> { parent1.GenotypeId, parent2.GenotypeId },
                    Generation = child.Generation,
                    CreationDate = child.CreationDate,
                    LineageDepth = CalculateLineageDepth(parent1, parent2) + 1
                };

                _lineageTracker[child.GenotypeId] = lineage;
                OnLineageUpdated?.Invoke(lineage);
            }
        }

        private int CalculateLineageDepth(CannabisGenotype parent1, CannabisGenotype parent2)
        {
            int parent1Depth = _lineageTracker.ContainsKey(parent1.GenotypeId) ? _lineageTracker[parent1.GenotypeId].LineageDepth : 0;
            int parent2Depth = _lineageTracker.ContainsKey(parent2.GenotypeId) ? _lineageTracker[parent2.GenotypeId].LineageDepth : 0;
            return Math.Max(parent1Depth, parent2Depth);
        }

        private float CalculateHybridVigorFactor(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method)
        {
            if (!_enableHybridVigor || !IsOutcross(parent1, parent2))
                return 1.0f;

            var geneticDistance = CalculateGeneticDistance(parent1, parent2);
            return 1.0f + (geneticDistance * _hybridVigorMultiplier - 1.0f);
        }

        private bool IsOutcross(CannabisGenotype parent1, CannabisGenotype parent2)
        {
            return parent1.StrainId != parent2.StrainId && 
                   CalculateGeneticDistance(parent1, parent2) > 0.3f;
        }

        private bool IsInbred(CannabisGenotype parent1, CannabisGenotype parent2)
        {
            return CalculateGeneticDistance(parent1, parent2) < 0.2f ||
                   parent1.ParentGenotypes.Any(p => parent2.ParentGenotypes.Contains(p));
        }

        private void ApplyHybridVigor(CannabisGenotype offspring, float vigorMultiplier)
        {
            if (offspring.Traits != null)
            {
                foreach (var trait in offspring.Traits)
                {
                    trait.ExpressedValue *= vigorMultiplier;
                    trait.ExpressedValue = Mathf.Clamp(trait.ExpressedValue, 0.1f, 2.0f);
                }
            }
        }

        private void ApplyInbreedingEffects(CannabisGenotype offspring, float inbreedingPenalty)
        {
            if (offspring.Traits != null)
            {
                foreach (var trait in offspring.Traits)
                {
                    trait.ExpressedValue *= inbreedingPenalty;
                    trait.ExpressedValue = Mathf.Clamp(trait.ExpressedValue, 0.1f, 2.0f);
                }
            }
        }

        private List<Allele> GetAllelesForGene(CannabisGenotype genotype, string geneId)
        {
            if (genotype.Alleles == null || !genotype.Alleles.ContainsKey(geneId))
                return new List<Allele>();

            return genotype.Alleles[geneId].OfType<Allele>().ToList();
        }

        private List<Allele> ApplyCrossover(List<Allele> alleles, string geneId)
        {
            // Simple crossover implementation
            if (alleles.Count >= 2 && UnityEngine.Random.value < _geneRecombinationRate)
            {
                // Swap properties between alleles
                var temp = alleles[0].Expression;
                alleles[0].Expression = alleles[1].Expression;
                alleles[1].Expression = temp;
            }
            
            return alleles;
        }

        private void UpdateTraitExpressions(CannabisGenotype offspring)
        {
            // Update the offspring's trait expressions based on inherited alleles
            offspring.CalculatePhenotypicExpression();
        }

        private int GetOffspringCount(BreedingMethod method)
        {
            return method switch
            {
                BreedingMethod.SelfPollination => UnityEngine.Random.Range(2, 5),
                BreedingMethod.StandardCross => UnityEngine.Random.Range(3, 6),
                BreedingMethod.HybridCross => UnityEngine.Random.Range(4, 8),
                _ => UnityEngine.Random.Range(2, 4)
            };
        }

        private string GenerateOffspringStrainId(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method)
        {
            return $"{parent1.StrainId}_x_{parent2.StrainId}_{method}_{DateTime.Now:yyyyMMdd}";
        }

        private string GenerateOffspringStrainName(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method, int offspringIndex)
        {
            var baseName = $"{parent1.StrainName} x {parent2.StrainName}";
            if (offspringIndex > 0)
                baseName += $" #{offspringIndex + 1}";
            return baseName;
        }

        private void CompleteActiveBreedings()
        {
            foreach (var operation in _activeBreedingOperations.Values)
            {
                if (operation.Status == BreedingStatus.Processing)
                {
                    operation.Status = BreedingStatus.Completed;
                }
            }
        }

        private void SaveBreedingData()
        {
            // Save breeding analytics and history - would integrate with data persistence service
            if (_enableBreedingLogging)
                Debug.Log($"Saving breeding data: {_breedingHistory.Count} records, {_totalBreedingOperations} total operations");
        }

        private void ClearAllData()
        {
            _breedingHistory.Clear();
            _lineageTracker.Clear();
            _activeBreedingOperations.Clear();
            _breedingQueue.Clear();
            _strainCompatibility.Clear();
            _traitCombinationResults.Clear();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get breeding record by ID
        /// </summary>
        public BreedingRecord GetBreedingRecord(string breedingId)
        {
            return _breedingHistory.TryGetValue(breedingId, out var record) ? record : null;
        }

        /// <summary>
        /// Get breeding history ordered by date
        /// </summary>
        public List<BreedingRecord> GetBreedingHistory(int maxRecords = 100)
        {
            return _breedingHistory.Values
                .OrderByDescending(r => r.BreedingDate)
                .Take(maxRecords)
                .ToList();
        }

        /// <summary>
        /// Get lineage information for a genotype
        /// </summary>
        public GeneticLineage GetLineage(string genotypeId)
        {
            return _lineageTracker.TryGetValue(genotypeId, out var lineage) ? lineage : null;
        }

        /// <summary>
        /// Get breeding analytics
        /// </summary>
        public BreedingAnalytics GetBreedingAnalytics()
        {
            return _breedingAnalytics;
        }

        /// <summary>
        /// Check if breeding is possible between two genotypes
        /// </summary>
        public bool CanBreed(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method = BreedingMethod.StandardCross)
        {
            if (!_isInitialized || parent1 == null || parent2 == null)
                return false;

            return ValidateBreedingCompatibility(parent1, parent2, method);
        }

        /// <summary>
        /// Get available breeding methods
        /// </summary>
        public List<BreedingMethod> GetAvailableBreedingMethods()
        {
            var methods = new List<BreedingMethod> { BreedingMethod.StandardCross };
            
            if (_enableAdvancedBreeding) methods.Add(BreedingMethod.HybridCross);
            if (_enableBackcrossing) methods.Add(BreedingMethod.Backcross);
            if (_enableLineBreeding) methods.Add(BreedingMethod.LineBreeding);
            
            methods.Add(BreedingMethod.SelfPollination);
            methods.Add(BreedingMethod.OutCross);
            
            return methods;
        }

        /// <summary>
        /// Update breeding settings at runtime
        /// </summary>
        public void UpdateBreedingSettings(bool enableMendelian, bool enableHybridVigor, float vigorMultiplier, float crossoverRate)
        {
            _enableMendelianInheritance = enableMendelian;
            _enableHybridVigor = enableHybridVigor;
            _hybridVigorMultiplier = vigorMultiplier;
            _crossoverRate = crossoverRate;
            
            if (_enableBreedingLogging)
                Debug.Log($"Breeding settings updated: Mendelian={enableMendelian}, HybridVigor={enableHybridVigor}, Multiplier={vigorMultiplier}, Crossover={crossoverRate}");
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    /// <summary>
    /// Breeding method enumeration
    /// </summary>
    public enum BreedingMethod
    {
        StandardCross,
        Backcross,
        SelfPollination,
        LineBreeding,
        OutCross,
        HybridCross
    }

    /// <summary>
    /// Breeding operation status
    /// </summary>
    public enum BreedingStatus
    {
        Queued,
        Processing,
        Completed,
        Failed
    }

    /// <summary>
    /// Cannabis strain types for breeding
    /// </summary>
    public enum CannabisStrainType
    {
        Sativa,
        Indica,
        Hybrid,
        Ruderalis,
        Industrial
    }

    /// <summary>
    /// Breeding operation data
    /// </summary>
    [System.Serializable]
    public class BreedingOperation
    {
        public string OperationId = "";
        public CannabisGenotype Parent1;
        public CannabisGenotype Parent2;
        public BreedingMethod Method = BreedingMethod.StandardCross;
        public BreedingStatus Status = BreedingStatus.Queued;
        public DateTime QueueTime = DateTime.Now;
        public DateTime StartTime = DateTime.Now;
        public BreedingResult Result;
    }

    /// <summary>
    /// Breeding result data structure
    /// </summary>
    [System.Serializable]
    public class BreedingResult
    {
        public bool Success = false;
        public int OffspringCount = 0;
        public List<CannabisGenotype> OffspringGenotypes = new List<CannabisGenotype>();
        public float BreedingTime = 0f;
        public string ErrorMessage = "";
        public Dictionary<string, float> TraitPredictions = new Dictionary<string, float>();
        public float HybridVigorFactor = 1.0f;
        public DateTime CompletionTime = DateTime.Now;
        public CannabisGenotype Parent1Genotype;
        public CannabisGenotype Parent2Genotype;
        public BreedingMethod BreedingMethod = BreedingMethod.StandardCross;
        
        // Alias property for compatibility with CannabisGeneticsOrchestrator
        public List<CannabisGenotype> Offspring => OffspringGenotypes;
    }

    /// <summary>
    /// Breeding record for history tracking
    /// </summary>
    [System.Serializable]
    public class BreedingRecord
    {
        public string BreedingId = "";
        public string Parent1Id = "";
        public string Parent2Id = "";
        public BreedingMethod Method = BreedingMethod.StandardCross;
        public DateTime BreedingDate = DateTime.Now;
        public List<string> OffspringIds = new List<string>();
        public bool Success = false;
        public float BreedingTime = 0f;
        public string Notes = "";
        public Dictionary<string, float> TraitOutcomes = new Dictionary<string, float>();
    }

    /// <summary>
    /// Genetic lineage tracking
    /// </summary>
    [System.Serializable]
    public class GeneticLineage
    {
        public string GenotypeId = "";
        public List<string> ParentIds = new List<string>();
        public int Generation = 1;
        public DateTime CreationDate = DateTime.Now;
        public int LineageDepth = 0;
        public Dictionary<string, string> AncestryMap = new Dictionary<string, string>();
    }

    /// <summary>
    /// Breeding analytics data
    /// </summary>
    [System.Serializable]
    public class BreedingAnalytics
    {
        public int TotalBreedingOperations = 0;
        public int SuccessfulBreedings = 0;
        public int FailedBreedings = 0;
        public float AverageBreedingTime = 0f;
        public Dictionary<string, int> StrainPopularity = new Dictionary<string, int>();
        public Dictionary<string, float> TraitFrequency = new Dictionary<string, float>();
        public float GeneticDiversityTrend = 0f;
        public DateTime LastAnalyticsUpdate = DateTime.Now;
    }

    /// <summary>
    /// Allele data structure for breeding system
    /// </summary>
    [System.Serializable]
    public class Allele
    {
        public string AlleleId = "";
        public string GeneId = "";
        public string AlleleName = "";
        public float Expression = 1.0f;
        public float Dominance = 0.5f;
        public float Stability = 0.8f;
        public float MutationRate = 0.001f;
        public Color ColorValue = Color.clear;
        public Vector3 MorphologyValue = Vector3.one;
        public Dictionary<string, float> Properties = new Dictionary<string, float>();
        public Dictionary<CannabisStrainType, float> StrainTypeInfluence = new Dictionary<CannabisStrainType, float>();
        public List<string> Mutations = new List<string>();
        public DateTime CreationDate = DateTime.Now;
        public string Origin = "";

        public Allele()
        {
            AlleleId = Guid.NewGuid().ToString();
            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            Properties = new Dictionary<string, float>();
            StrainTypeInfluence = new Dictionary<CannabisStrainType, float>
            {
                { CannabisStrainType.Sativa, 0.33f },
                { CannabisStrainType.Indica, 0.33f },
                { CannabisStrainType.Hybrid, 0.34f },
                { CannabisStrainType.Ruderalis, 0f },
                { CannabisStrainType.Industrial, 0f }
            };
            Mutations = new List<string>();
        }
    }
}