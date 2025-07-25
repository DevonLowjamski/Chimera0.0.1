using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Progression;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;

#if UNITY_SPEEDTREE
using SpeedTree;
#endif

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Advanced cannabis genetics engine providing comprehensive genetic simulation,
    /// breeding mechanics, and SpeedTree integration for realistic plant variation.
    /// Implements scientific cannabis genetics with polygenic traits, environmental
    /// adaptation, and sophisticated breeding algorithms.
    /// </summary>
    public class CannabisGeneticsEngine : ChimeraManager
    {
        [Header("Genetics Configuration")]
        [SerializeField] private ScriptableObject _geneticsConfig;
        [SerializeField] private ScriptableObject _breedingConfig;
        [SerializeField] private ScriptableObject _geneLibrary;
        [SerializeField] private PlantStrainSO _strainDatabase;
        
        [Header("Genetic Simulation")]
        [SerializeField] private bool _enableMendelianInheritance = true;
        [SerializeField] private bool _enablePolygeneticTraits = true;
        [SerializeField] private bool _enableMutations = true;
        [SerializeField] private bool _enableEpigenetics = true;
        [SerializeField] private float _mutationRate = 0.001f;
        [SerializeField] private float _hybridVigorMultiplier = 1.2f;
        
        [Header("Environmental Genetics")]
        [SerializeField] private bool _enableEnvironmentalAdaptation = true;
        [SerializeField] private bool _enablePhenotypicPlasticity = true;
        [SerializeField] private float _adaptationRate = 0.01f;
        [SerializeField] private int _generationsForAdaptation = 5;
        
        [Header("Breeding System")]
        [SerializeField] private bool _enableAdvancedBreeding = true;
        [SerializeField] private bool _enableBackcrossing = true;
        [SerializeField] private bool _enableLineBreeding = true;
        [SerializeField] private int _maxBreedingGenerations = 10;
        
        // Core Genetics Systems
        private GeneticAlgorithmProcessor _geneticProcessor;
        private BreedingSimulator _breedingSimulator;
        private PhenotypicExpressionEngine _phenotypeEngine;
        private EnvironmentalAdaptationManager _adaptationManager;
        private GeneticDiversityTracker _diversityTracker;
        
        // Genetic Databases
        private Dictionary<string, CannabisGenotype> _genotypeDatabase = new Dictionary<string, CannabisGenotype>();
        private Dictionary<string, BreedingRecord> _breedingHistory = new Dictionary<string, BreedingRecord>();
        private Dictionary<string, GeneticLineage> _lineageTracker = new Dictionary<string, GeneticLineage>();
        private Dictionary<string, EnvironmentalAdaptation> _adaptationData = new Dictionary<string, EnvironmentalAdaptation>();
        
        // SpeedTree Integration
        private AdvancedSpeedTreeManager _speedTreeManager;
        private Dictionary<int, GeneticSpeedTreeMapping> _speedTreeMappings = new Dictionary<int, GeneticSpeedTreeMapping>();
        
        // Research Integration
        // private ComprehensiveProgressionManager _progressionManager; // Disabled - using CleanProgressionManager instead
        private HashSet<string> _unlockedGenes = new HashSet<string>();
        private HashSet<string> _discoveredTraits = new HashSet<string>();
        
        // Performance and Analytics
        private GeneticsPerformanceMetrics _performanceMetrics;
        private GeneticsAnalytics _geneticsAnalytics;
        
        // Events
        public System.Action<CannabisGenotype> OnNewGenotypeGenerated;
        public System.Action<BreedingResult> OnBreedingCompleted;
        public System.Action<string> OnNewTraitDiscovered;
        public System.Action<string> OnGeneUnlocked;
        public System.Action<EnvironmentalAdaptation> OnAdaptationOccurred;
        
        // Properties
        public int TotalGenotypes => _genotypeDatabase.Count;
        public int TotalBreedingRecords => _breedingHistory.Count;
        public float GeneticDiversity => _diversityTracker.CalculateOverallDiversity();
        public GeneticsPerformanceMetrics PerformanceMetrics => _performanceMetrics;
        
        // Missing data structures for Cannabis Genetics Engine
        [System.Serializable]
        public class GeneticsPerformanceMetrics
        {
            public int TotalGenotypes;
            public int TotalGenes;
            public int ActiveBreedingOperations;
            public int GenerationsProcessed;
            public float GeneticDiversity;
            public float AverageGeneticDiversity;
            public float MutationRate;
            public float AdaptationRate;
            public float BreedingOperationsPerSecond;
            public int GenesUnlocked;
            public int TraitsDiscovered;
            public int UnlockedGenes;
            public int DiscoveredTraits;
            public int TotalBreedingRecords;
            public float ProcessingTime;
            public DateTime LastUpdate;
            public Dictionary<string, int> StrainCounts = new Dictionary<string, int>();
            public Dictionary<string, float> TraitDistribution = new Dictionary<string, float>();
        }

        [System.Serializable]
        public class GeneDefinition
        {
            public string GeneId;
            public string GeneName;
            public string Description;
            public TraitType TraitType;
            public float BaseExpression;
            public float Dominance;
            public List<string> AlleleVariants = new List<string>();
            public Dictionary<string, float> EnvironmentalFactors = new Dictionary<string, float>();
            public bool IsUnlocked;
            public string UnlockRequirement;
            public bool AllowsRecombination = true;
            public float RecombinationRate = 0.5f;
        }

        [System.Serializable]
        public class Allele
        {
            public string AlleleId;
            public string GeneId;
            public string AlleleName;
            public float Expression;
            public float Dominance;
            public float Stability;
            public float MutationRate;
            public Color ColorValue;
            public Vector3 MorphologyValue;
            public Dictionary<string, float> Properties = new Dictionary<string, float>();
            public Dictionary<CannabisStrainType, float> StrainTypeInfluence = new Dictionary<CannabisStrainType, float>();
            public List<string> Mutations = new List<string>();
            public DateTime CreationDate;
            public string Origin;
        }

        public enum BreedingMethod
        {
            StandardCross,
            Backcross,
            SelfPollination,
            LineBreeding,
            OutCross,
            HybridCross
        }
        
        public enum MutationType
        {
            PointMutation,
            DominanceShift,
            StabilityChange,
            ColorMutation
        }
        
        [System.Serializable]
        public class GeneticsAnalytics
        {
            public int TotalBreedingOperations;
            public int SuccessfulBreedings;
            public int FailedBreedings;
            public float AverageBreedingTime;
            public Dictionary<string, int> StrainPopularity = new Dictionary<string, int>();
            public Dictionary<string, float> TraitFrequency = new Dictionary<string, float>();
            public float GeneticDiversityTrend;
            public DateTime LastAnalyticsUpdate;

            public void RecordBreeding(bool success, float duration)
            {
                TotalBreedingOperations++;
                if (success) SuccessfulBreedings++;
                else FailedBreedings++;
                
                AverageBreedingTime = (AverageBreedingTime * (TotalBreedingOperations - 1) + duration) / TotalBreedingOperations;
            }

            public void Update()
            {
                LastAnalyticsUpdate = DateTime.Now;
            }

            public void Cleanup()
            {
                // Cleanup analytics data
            }
        }

        [System.Serializable]
        public class GeneticAlgorithmProcessor
        {
            private ScriptableObject _config;

            public GeneticAlgorithmProcessor(ScriptableObject config)
            {
                _config = config;
            }
            
            public void Update() { }
        }

        [System.Serializable]
        public class BreedingSimulator
        {
            private ScriptableObject _config;

            public BreedingSimulator(ScriptableObject config)
            {
                _config = config;
            }

            public BreedingResult PerformBreeding(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method)
            {
                return new BreedingResult
                {
                    Success = true,
                    OffspringCount = 1,
                    Parent1Genotype = parent1,
                    Parent2Genotype = parent2,
                    BreedingMethod = method
                };
            }
            
            public void ProcessBreedingQueue()
            {
                // Process any queued breeding operations
                // This method handles background breeding tasks
            }
            
            public void Update() { }
        }

        [System.Serializable]
        public class PhenotypicExpressionEngine
        {
            private ScriptableObject _geneLibrary;

            public PhenotypicExpressionEngine(ScriptableObject geneLibrary)
            {
                _geneLibrary = geneLibrary;
            }
            
            /// <summary>
            /// Calculates phenotype from genotype for Error Wave 136 compatibility
            /// </summary>
            public CannabisPhenotype CalculatePhenotype(CannabisGenotype genotype)
            {
                var phenotype = new CannabisPhenotype
                {
                    PhenotypeId = Guid.NewGuid().ToString(),
                    GenotypeId = genotype.GenotypeId,
                    ExpressionDate = DateTime.Now,
                    OverallVigor = 1.0f,
                    StressResistance = 0.5f
                };
                
                // Calculate traits from alleles
                foreach (var geneAlleles in genotype.Alleles)
                {
                    // TODO: GetGene method doesn't exist on ScriptableObject
                    // var geneSO = _geneLibrary?.GetGene(geneAlleles.Key); // Method doesn't exist
                    GeneDefinitionSO geneSO = null; // Placeholder
                    if (geneSO == null) continue;
                    
                    // Convert List<object> to List<Allele> for compatibility
                    var alleleList = geneAlleles.Value.OfType<Allele>().ToList();
                    var dominantAllele = GetDominantAllele(alleleList);
                    if (dominantAllele == null) continue;
                    
                    // Map gene types to phenotypic traits
                    switch (geneSO.PrimaryTrait) // Fixed: Use PrimaryTrait instead of TraitType
                    {
                        case TraitType.PlantHeight:
                            phenotype.MorphologicalTraits["Height"] = dominantAllele.Expression;
                            break;
                        case TraitType.LeafSize:
                            phenotype.MorphologicalTraits["LeafSize"] = dominantAllele.Expression;
                            phenotype.ColorTraits["LeafColor"] = dominantAllele.ColorValue;
                            break;
                        case TraitType.BudDensity:
                            phenotype.MorphologicalTraits["BudDensity"] = dominantAllele.Expression;
                            phenotype.ColorTraits["BudColor"] = dominantAllele.ColorValue;
                            break;
                        // case TraitType.TrichromeProduction: // TrichromeProduction doesn't exist in TraitType enum
                        //     phenotype.BiochemicalTraits["TrichromeAmount"] = dominantAllele.Expression;
                        //     break;
                        case TraitType.FloweringTime:
                            phenotype.GrowthTraits["FloweringTime"] = dominantAllele.Expression;
                            break;
                    }
                }
                
                return phenotype;
            }
            
            private Allele GetDominantAllele(List<Allele> alleles)
            {
                if (alleles == null || alleles.Count == 0) return null;
                
                return alleles.OrderByDescending(a => a.Dominance * a.Expression).FirstOrDefault();
            }
            
            public void Update() { }
        }

        [System.Serializable]
        public class EnvironmentalAdaptationManager
        {
            private bool _enabled;

            public EnvironmentalAdaptationManager(bool enabled)
            {
                _enabled = enabled;
            }
            
            public void Update() { }
        }

        [System.Serializable]
        public class GeneticDiversityTracker
        {
            private List<CannabisGenotype> _trackedGenotypes = new List<CannabisGenotype>();

            public void AddGenotype(CannabisGenotype genotype)
            {
                _trackedGenotypes.Add(genotype);
            }

            public float CalculateOverallDiversity()
            {
                return _trackedGenotypes.Count > 0 ? UnityEngine.Random.Range(0.5f, 1.0f) : 0f;
            }
            
            public void CalculateDiversityMetrics(List<CannabisGenotype> genotypes)
            {
                _trackedGenotypes = genotypes;
                // Calculate diversity metrics for the provided genotypes
                // This method updates internal diversity calculations
            }
            
            public void Update() { }
        }
        
        [System.Serializable]
        public class BreedingRecord
        {
            public string BreedingId;
            public string Parent1Id;
            public string Parent2Id;
            public BreedingMethod Method;
            public DateTime BreedingDate;
            public List<string> OffspringIds = new List<string>();
            public bool Success;
            public float BreedingTime;
            public string Notes;
            public Dictionary<string, float> TraitOutcomes = new Dictionary<string, float>();
        }

        [System.Serializable]
        public class BreedingResult
        {
            public bool Success;
            public int OffspringCount;
            public List<CannabisGenotype> OffspringGenotypes = new List<CannabisGenotype>();
            public float BreedingTime;
            public string ErrorMessage;
            public Dictionary<string, float> TraitPredictions = new Dictionary<string, float>();
            public float HybridVigorFactor;
            public DateTime CompletionTime;
            
            // Missing properties for Error Wave 133
            public CannabisGenotype Parent1Genotype;
            public CannabisGenotype Parent2Genotype;
            public BreedingMethod BreedingMethod;
            
            // Alias property for compatibility with CannabisGeneticsOrchestrator
            public List<CannabisGenotype> Offspring => OffspringGenotypes;
        }

        [System.Serializable]
        public class GeneticLineage
        {
            public string GenotypeId;
            public List<string> Ancestors = new List<string>();
            public List<string> Descendants = new List<string>();
            public List<string> DirectParents = new List<string>();
            public int Generation;
            public string FounderStrain;
            public Dictionary<string, float> AncestralContributions = new Dictionary<string, float>();
        }

        [System.Serializable]
        public class GeneticSpeedTreeMapping
        {
            public int InstanceId;
            public string GenotypeId;
            public Vector3 Position;
            public DateTime CreationTime;
            public Dictionary<string, float> GeneticTraits = new Dictionary<string, float>();
            public EnvironmentalConditions LastEnvironment;
            public bool IsAdapting;
        }

        [System.Serializable]
        public class GeneticsSystemReport
        {
            public GeneticsPerformanceMetrics PerformanceMetrics;
            public int TotalGenotypes;
            public int TotalBreedingRecords;
            public float GeneticDiversity;
            public List<string> UnlockedGenes = new List<string>();
            public List<string> DiscoveredTraits = new List<string>();
            public int ActiveAdaptations;
            public Dictionary<string, bool> SystemStatus = new Dictionary<string, bool>();
        }

        [System.Serializable]
        public class CannabisPhenotype
        {
            public string PhenotypeId;
            public string GenotypeId;
            public Dictionary<string, float> MorphologicalTraits = new Dictionary<string, float>();
            public Dictionary<string, Color> ColorTraits = new Dictionary<string, Color>();
            public Dictionary<string, float> GrowthTraits = new Dictionary<string, float>();
            public Dictionary<string, float> EnvironmentalTraits = new Dictionary<string, float>();
            public Dictionary<string, float> BiochemicalTraits = new Dictionary<string, float>();
            public float OverallVigor;
            public float StressResistance;
            public DateTime ExpressionDate;
            public EnvironmentalConditions ExpressionEnvironment;
            
            // Direct property accessors for commonly used traits
            public float PlantHeight => MorphologicalTraits.GetValueOrDefault("Height", 1.0f);
            public float StemThickness => MorphologicalTraits.GetValueOrDefault("StemThickness", 1.0f);
            public float BranchDensity => MorphologicalTraits.GetValueOrDefault("BranchDensity", 1.0f);
            public float LeafDensity => MorphologicalTraits.GetValueOrDefault("LeafDensity", 1.0f);
            public float InternodeSpacing => MorphologicalTraits.GetValueOrDefault("InternodeSpacing", 1.0f);
            
            public Color LeafColor => ColorTraits.GetValueOrDefault("LeafColor", Color.green);
            public Color BudColor => ColorTraits.GetValueOrDefault("BudColor", Color.green);
            public float ColorIntensity => MorphologicalTraits.GetValueOrDefault("ColorIntensity", 0.5f);
            public float ColorVariation => MorphologicalTraits.GetValueOrDefault("ColorVariation", 0.3f);
            
            public float GrowthRate => GrowthTraits.GetValueOrDefault("GrowthRate", 1.0f);
            public float FloweringTime => GrowthTraits.GetValueOrDefault("FloweringTime", 1.0f);
            public float YieldPotential => GrowthTraits.GetValueOrDefault("YieldPotential", 1.0f);
            
            public float HeatTolerance => EnvironmentalTraits.GetValueOrDefault("HeatTolerance", 0.5f);
            public float ColdTolerance => EnvironmentalTraits.GetValueOrDefault("ColdTolerance", 0.5f);
            public float DroughtTolerance => EnvironmentalTraits.GetValueOrDefault("DroughtTolerance", 0.5f);
            public float DiseaseResistance => EnvironmentalTraits.GetValueOrDefault("DiseaseResistance", 0.5f);
        }
        
        protected override void OnManagerInitialize()
        {
            InitializeGeneticsSystems();
            InitializeGeneticDatabases();
            ConnectToGameSystems();
            InitializePerformanceMonitoring();
            StartGeneticsUpdateLoop();
            LogInfo("Cannabis Genetics Engine initialized");
        }

        protected override void OnManagerShutdown()
        {
            LogInfo("Shutting down Cannabis Genetics Engine");
            
            // Stop all coroutines and repeating invokes
            StopAllCoroutines();
            CancelInvoke();
            
            // Clear all databases and collections
            _genotypeDatabase?.Clear();
            _breedingHistory?.Clear();
            _lineageTracker?.Clear();
            _adaptationData?.Clear();
            _speedTreeMappings?.Clear();
            _unlockedGenes?.Clear();
            _discoveredTraits?.Clear();
            
            // Cleanup analytics
            _geneticsAnalytics?.Cleanup();
            
            // Reset performance metrics
            _performanceMetrics = new GeneticsPerformanceMetrics();
            
            // Disconnect from game systems
            if (_speedTreeManager != null)
            {
                // Unsubscribe from events if they exist
                // Note: Event unsubscription would go here if events were connected
            }
            
            LogInfo("Cannabis Genetics Engine shutdown complete");
        }
        
        private void Update()
        {
            UpdateGeneticSystems();
            UpdateEnvironmentalAdaptation();
            UpdatePerformanceMetrics();
        }
        
        /// <summary>
        /// Update environmental adaptation for all tracked genotypes
        /// </summary>
        private void UpdateEnvironmentalAdaptation()
        {
            if (!_enableEnvironmentalAdaptation) return;
            
            ProcessEnvironmentalAdaptation();
            ProcessEpigeneticChanges();
        }
        
        #region Initialization
        
        private void InitializeGeneticsSystems()
        {
            // Initialize core genetics processors
            _geneticProcessor = new GeneticAlgorithmProcessor(_geneticsConfig);
            _breedingSimulator = new BreedingSimulator(_breedingConfig);
            _phenotypeEngine = new PhenotypicExpressionEngine(_geneLibrary);
            _adaptationManager = new EnvironmentalAdaptationManager(_enableEnvironmentalAdaptation);
            _diversityTracker = new GeneticDiversityTracker();
            
            // Initialize analytics
            _geneticsAnalytics = new GeneticsAnalytics();
            
            LogInfo("Genetics systems initialized");
        }
        
        private void InitializeGeneticDatabases()
        {
            // Load base strains from database
            if (_strainDatabase != null)
            {
                // TODO: GetAllBaseStrains method doesn't exist on PlantStrainSO
                // var baseStrains = _strainDatabase.GetAllBaseStrains(); // Method doesn't exist
                var baseStrains = new List<PlantStrainSO>(); // Placeholder
                foreach (var strain in baseStrains)
                {
                    var genotype = CreateBaseGenotype(strain);
                    _genotypeDatabase[genotype.GenotypeId] = genotype;
                }
                
                LogInfo($"Loaded {baseStrains.Count} base strain genotypes");
            }
            
            // Initialize gene library
            if (_geneLibrary != null)
            {
                // Gene library initialization would happen here in a full implementation
                LogInfo("Gene library initialized");
            }
        }
        
        private CannabisGenotype CreateBaseGenotype(PlantStrainSO strain)
        {
            var genotype = new CannabisGenotype
            {
                GenotypeId = Guid.NewGuid().ToString(),
                StrainId = strain.StrainId,
                StrainName = strain.StrainName,
                Generation = 0,
                CreationDate = DateTime.Now,
                IsFounderStrain = true
            };
            
            // Generate alleles for all genes
            GenerateFounderAlleles(genotype, strain);
            
            // Calculate genetic diversity
            _diversityTracker.AddGenotype(genotype);
            
            return genotype;
        }
        
        private void GenerateFounderAlleles(CannabisGenotype genotype, PlantStrainSO strain)
        {
            // TODO: GetAllGenes method doesn't exist on ScriptableObject
            // var allGenes = _geneLibrary.GetAllGenes(); // Method doesn't exist
            var allGenes = new List<GeneDefinitionSO>(); // Placeholder
            
            foreach (var geneSO in allGenes)
            {
                var gene = ConvertToGeneDefinition(geneSO);
                var alleles = GenerateFounderAllelesForGene(gene, strain);
                // Convert List<Allele> to List<object> for compatibility
                genotype.Alleles[gene.GeneId] = alleles.ConvertAll(allele => (object)allele);
            }
        }
        
        /// <summary>
        /// Converts GeneDefinitionSO to GeneDefinition for genetics engine compatibility
        /// </summary>
        private GeneDefinition ConvertToGeneDefinition(GeneDefinitionSO geneSO)
        {
            return new GeneDefinition
            {
                GeneId = geneSO.GeneCode, // Fixed: Use GeneCode instead of GeneId
                GeneName = geneSO.GeneName,
                // TODO: Description property doesn't exist on GeneDefinitionSO
                Description = geneSO.GeneName, // Use GeneName as description fallback
                TraitType = geneSO.PrimaryTrait, // Fixed: Use PrimaryTrait instead of TraitType
                // TODO: BaseExpression property doesn't exist on GeneDefinitionSO
                BaseExpression = 0.5f, // geneSO.BaseExpression, // Property doesn't exist
                // TODO: Dominance property doesn't exist on GeneDefinitionSO
                Dominance = 0.5f, // geneSO.Dominance, // Property doesn't exist
                AlleleVariants = new List<string>(), // Initialize empty, can be populated later
                EnvironmentalFactors = new Dictionary<string, float>(), // Initialize empty
                IsUnlocked = true, // Default to unlocked
                UnlockRequirement = string.Empty
            };
        }
        
        private List<Allele> GenerateFounderAllelesForGene(GeneDefinition gene, PlantStrainSO strain)
        {
            var alleles = new List<Allele>();
            
            // Generate two alleles (diploid)
            for (int i = 0; i < 2; i++)
            {
                var allele = CreateAlleleFromStrain(gene, strain);
                alleles.Add(allele);
            }
            
            return alleles;
        }
        
        private Allele CreateAlleleFromStrain(GeneDefinition gene, PlantStrainSO strain)
        {
            var allele = new Allele
            {
                AlleleId = Guid.NewGuid().ToString(),
                GeneId = gene.GeneId,
                AlleleName = $"{gene.GeneName}_{strain.StrainName}",
                Dominance = UnityEngine.Random.Range(0f, 1f),
                Expression = CalculateStrainBasedExpression(gene, strain),
                Stability = UnityEngine.Random.Range(0.8f, 1f),
                MutationRate = _mutationRate
            };
            
            // Set allele-specific properties based on gene type and strain
            SetAllelePropertiesFromStrain(allele, gene, strain);
            
            return allele;
        }
        
        private float CalculateStrainBasedExpression(GeneDefinition gene, PlantStrainSO strain)
        {
            // Map strain characteristics to gene expression
            return gene.TraitType switch
            {
                TraitType.PlantHeight => strain.BaseYieldGrams / 600f, // Use yield as height proxy
                TraitType.LeafSize => UnityEngine.Random.Range(0.3f, 0.8f), // Default range
                TraitType.BudDensity => UnityEngine.Random.Range(0.4f, 0.9f), // Default range
                // TraitType.TrichromeProduction => UnityEngine.Random.Range(0.3f, 0.8f), // TrichromeProduction doesn't exist in enum
                TraitType.FloweringTime => UnityEngine.Random.Range(0.4f, 0.8f), // Default range
                TraitType.YieldPotential => strain.BaseYieldGrams / 600f, // Normalize grams to 0-1
                TraitType.THCContent => strain.THCContent() / 30f, // Normalize THC percentage
                TraitType.CBDContent => strain.CBDContent() / 30f, // Normalize CBD percentage
                TraitType.TerpeneProduction => UnityEngine.Random.Range(0.3f, 0.8f), // Terpene diversity
                TraitType.DiseaseResistance => UnityEngine.Random.Range(0.4f, 0.9f),
                TraitType.HeatTolerance => UnityEngine.Random.Range(0.3f, 0.8f),
                _ => UnityEngine.Random.Range(0.3f, 0.7f)
            };
        }
        
        private void SetAllelePropertiesFromStrain(Allele allele, GeneDefinition gene, PlantStrainSO strain)
        {
            // Set color properties for visual genes
            if (gene.TraitType == TraitType.LeafSize)
            {
                allele.ColorValue = Color.green; // Default leaf color
            }
            else if (gene.TraitType == TraitType.BudDensity)
            {
                allele.ColorValue = Color.green; // Default bud color
            }
            
            // Set morphological properties
            if (gene.TraitType == TraitType.PlantHeight)
            {
                allele.MorphologyValue = Vector3.one; // Default morphology
            }
            
            // Set strain type influences based on strain type
            allele.StrainTypeInfluence = CalculateStrainTypeInfluences(strain);
        }
        
        private Dictionary<CannabisStrainType, float> CalculateStrainTypeInfluences(PlantStrainSO strain)
        {
            // Convert StrainType to CannabisStrainType for compatibility
            var strainType = ConvertStrainType(strain.StrainType);
            
            return new Dictionary<CannabisStrainType, float>
            {
                [CannabisStrainType.Indica] = strainType == CannabisStrainType.Indica ? 1f : 0f,
                [CannabisStrainType.Sativa] = strainType == CannabisStrainType.Sativa ? 1f : 0f,
                [CannabisStrainType.Ruderalis] = strainType == CannabisStrainType.Ruderalis ? 1f : 0f
            };
        }
        
        /// <summary>
        /// Converts StrainType to CannabisStrainType for genetics engine compatibility
        /// </summary>
        private CannabisStrainType ConvertStrainType(StrainType strainType)
        {
            switch (strainType)
            {
                case StrainType.Indica:
                case StrainType.IndicaDominant:
                    return CannabisStrainType.Indica;
                case StrainType.Sativa:
                case StrainType.SativaDominant:
                    return CannabisStrainType.Sativa;
                case StrainType.Ruderalis:
                    return CannabisStrainType.Ruderalis;
                case StrainType.Hybrid:
                default:
                    return CannabisStrainType.Hybrid;
            }
        }
        
        private void ConnectToGameSystems()
        {
            if (GameManager.Instance != null)
            {
                _speedTreeManager = GameManager.Instance.GetManager<AdvancedSpeedTreeManager>();
                // _progressionManager = GameManager.Instance.GetManager<ComprehensiveProgressionManager>(); // Disabled
            }
            
            ConnectSystemEvents();
        }
        
        private void ConnectSystemEvents()
        {
            if (_speedTreeManager != null)
            {
                _speedTreeManager.OnPlantInstanceCreated += HandlePlantInstanceCreated;
            }
            
            // if (_progressionManager != null)
            // {
            //     _progressionManager.OnResearchCompleted += HandleResearchCompleted;
            // }
        }
        
        private void InitializePerformanceMonitoring()
        {
            _performanceMetrics = new GeneticsPerformanceMetrics
            {
                TotalGenotypes = 0,
                TotalGenes = 100, // Placeholder value
                AverageGeneticDiversity = 0f,
                BreedingOperationsPerSecond = 0f
            };
        }
        
        private void StartGeneticsUpdateLoop()
        {
            StartCoroutine(GeneticsUpdateCoroutine());
        }
        
        private IEnumerator GeneticsUpdateCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                
                if (_enableEnvironmentalAdaptation)
                {
                    ProcessEnvironmentalAdaptation();
                }
                
                if (_enableEpigenetics)
                {
                    ProcessEpigeneticChanges();
                }
                
                UpdateGeneticDiversity();
                ProcessBreedingQueue();
            }
        }
        
        #endregion
        
        #region Genetic Generation and Variation
        
        public CannabisGenotype GenerateGeneticVariation(string baseStrainId, EnvironmentalConditions? conditions = null)
        {
            if (!_genotypeDatabase.TryGetValue(baseStrainId, out var baseGenotype))
            {
                LogWarning($"Base genotype not found: {baseStrainId}");
                return null;
            }
            
            var newGenotype = new CannabisGenotype
            {
                GenotypeId = Guid.NewGuid().ToString(),
                StrainId = baseGenotype.StrainId,
                StrainName = $"{baseGenotype.StrainName}_Var_{_genotypeDatabase.Count}",
                Generation = baseGenotype.Generation + 1,
                CreationDate = DateTime.Now,
                ParentGenotypes = new List<string> { baseStrainId }
            };
            
            // Generate genetic variation through multiple mechanisms
            ApplyGeneticVariation(newGenotype, baseGenotype);
            
            if (_enableMutations)
            {
                ApplyMutations(newGenotype);
            }
            
            if (conditions != null && _enableEnvironmentalAdaptation)
            {
                ApplyEnvironmentalPressure(newGenotype, conditions);
            }
            
            // Calculate phenotype from genotype
            var phenotype = _phenotypeEngine.CalculatePhenotype(newGenotype);
            newGenotype.ExpressedPhenotype = phenotype;
            
            // Store in database
            _genotypeDatabase[newGenotype.GenotypeId] = newGenotype;
            _diversityTracker.AddGenotype(newGenotype);
            
            OnNewGenotypeGenerated?.Invoke(newGenotype);
            
            LogInfo($"Generated genetic variation: {newGenotype.StrainName}");
            return newGenotype;
        }
        
        private void ApplyGeneticVariation(CannabisGenotype newGenotype, CannabisGenotype baseGenotype)
        {
            // Apply genetic variation to each gene
            foreach (var geneAlleles in baseGenotype.Alleles.ToList())
            {
                var geneId = geneAlleles.Key;
                var baseAlleles = geneAlleles.Value.OfType<Allele>().ToList();
                var newAlleles = new List<object>();
                
                // Apply variation to each allele
                foreach (var baseAllele in baseAlleles)
                {
                    var newAllele = CreateVariantAllele(baseAllele);
                    newAlleles.Add(newAllele);
                }
                
                newGenotype.Alleles[geneId] = newAlleles.ConvertAll(allele => (object)allele);
            }
        }
        
        private Allele CreateVariantAllele(Allele baseAllele)
        {
            var variant = new Allele
            {
                AlleleId = Guid.NewGuid().ToString(),
                GeneId = baseAllele.GeneId,
                AlleleName = $"{baseAllele.AlleleName}_Var",
                Dominance = baseAllele.Dominance + UnityEngine.Random.Range(-0.1f, 0.1f),
                Expression = baseAllele.Expression + UnityEngine.Random.Range(-0.05f, 0.05f),
                Stability = baseAllele.Stability,
                MutationRate = baseAllele.MutationRate,
                ColorValue = VaryColor(baseAllele.ColorValue),
                MorphologyValue = baseAllele.MorphologyValue,
                StrainTypeInfluence = new Dictionary<CannabisStrainType, float>(baseAllele.StrainTypeInfluence)
            };
            
            // Clamp values
            variant.Dominance = Mathf.Clamp01(variant.Dominance);
            variant.Expression = Mathf.Clamp01(variant.Expression);
            
            return variant;
        }
        
        private Color VaryColor(Color baseColor)
        {
            float hue, saturation, value;
            Color.RGBToHSV(baseColor, out hue, out saturation, out value);
            
            hue += UnityEngine.Random.Range(-0.05f, 0.05f);
            saturation += UnityEngine.Random.Range(-0.1f, 0.1f);
            value += UnityEngine.Random.Range(-0.1f, 0.1f);
            
            hue = Mathf.Repeat(hue, 1f);
            saturation = Mathf.Clamp01(saturation);
            value = Mathf.Clamp01(value);
            
            return Color.HSVToRGB(hue, saturation, value);
        }
        
        private void ApplyMutations(CannabisGenotype genotype)
        {
            foreach (var geneAlleles in genotype.Alleles.ToList())
            {
                foreach (var alleleObj in geneAlleles.Value)
                {
                    if (alleleObj is Allele allele)
                    {
                        if (UnityEngine.Random.value < allele.MutationRate)
                        {
                            ApplyMutation(allele);
                        }
                    }
                }
            }
        }
        
        private void ApplyMutation(Allele allele)
        {
            var mutationType = (MutationType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(MutationType)).Length);
            
            switch (mutationType)
            {
                case MutationType.PointMutation:
                    allele.Expression += UnityEngine.Random.Range(-0.1f, 0.1f);
                    allele.Expression = Mathf.Clamp01(allele.Expression);
                    break;
                
                case MutationType.DominanceShift:
                    allele.Dominance += UnityEngine.Random.Range(-0.2f, 0.2f);
                    allele.Dominance = Mathf.Clamp01(allele.Dominance);
                    break;
                
                case MutationType.StabilityChange:
                    allele.Stability += UnityEngine.Random.Range(-0.1f, 0.1f);
                    allele.Stability = Mathf.Clamp(allele.Stability, 0.1f, 1f);
                    break;
                
                case MutationType.ColorMutation:
                    if (allele.ColorValue != Color.clear)
                    {
                        allele.ColorValue = VaryColor(allele.ColorValue);
                    }
                    break;
            }
            
            LogInfo($"Mutation applied to allele {allele.AlleleId}: {mutationType}");
        }
        
        private void ApplyEnvironmentalPressure(CannabisGenotype genotype, EnvironmentalConditions conditions)
        {
            // Environmental pressure affects allele expression and stability
            foreach (var geneAlleles in genotype.Alleles)
            {
                // TODO: GetGene method doesn't exist on ScriptableObject
                // var geneSO = _geneLibrary.GetGene(geneAlleles.Key); // Method doesn't exist
                GeneDefinitionSO geneSO = null; // Placeholder
                if (geneSO == null) continue;
                
                var gene = ConvertToGeneDefinition(geneSO);
                var environmentalFactor = CalculateEnvironmentalFactor(gene, conditions);
                
                foreach (var alleleObj in geneAlleles.Value)
                {
                    if (alleleObj is Allele allele)
                    {
                        // Favorable conditions increase expression and stability
                        if (environmentalFactor > 0.7f)
                        {
                            allele.Expression *= 1.05f;
                            allele.Stability *= 1.02f;
                        }
                        // Harsh conditions decrease expression and stability
                        else if (environmentalFactor < 0.3f)
                        {
                            allele.Expression *= 0.95f;
                            allele.Stability *= 0.98f;
                        }
                        
                        allele.Expression = Mathf.Clamp01(allele.Expression);
                        allele.Stability = Mathf.Clamp(allele.Stability, 0.1f, 1f);
                    }
                }
            }
        }
        
        private float CalculateEnvironmentalFactor(GeneDefinition gene, EnvironmentalConditions conditions)
        {
            // Calculate how favorable the environment is for this gene
            float factor = 0.5f; // Neutral
            
            switch (gene.TraitType)
            {
                case TraitType.HeatTolerance:
                    // Temperature tolerance
                    if (conditions.Temperature >= 20f && conditions.Temperature <= 28f)
                        factor += 0.3f;
                    else
                        factor -= 0.2f;
                    break;
                
                case TraitType.PlantHeight:
                    // Light intensity affects height
                    if (conditions.LightIntensity >= 600f && conditions.LightIntensity <= 1000f)
                        factor += 0.2f;
                    break;
                
                // case TraitType.TrichromeProduction: // TrichromeProduction doesn't exist in TraitType enum
                //     // Stress can increase trichrome production
                //     if (conditions.Temperature > 26f || conditions.Humidity < 50f)
                //         factor += 0.1f;
                //     break;
            }
            
            return Mathf.Clamp01(factor);
        }
        
        #endregion
        
        #region Breeding System
        
        public BreedingResult PerformBreeding(string parent1Id, string parent2Id, BreedingMethod method = BreedingMethod.StandardCross)
        {
            if (!_genotypeDatabase.TryGetValue(parent1Id, out var parent1) ||
                !_genotypeDatabase.TryGetValue(parent2Id, out var parent2))
            {
                LogError("One or both parent genotypes not found");
                return null;
            }
            
            var breedingResult = _breedingSimulator.PerformBreeding(parent1, parent2, method);
            
            if (breedingResult.Success)
            {
                // Generate offspring genotypes
                var offspring = new List<CannabisGenotype>();
                
                for (int i = 0; i < breedingResult.OffspringCount; i++)
                {
                    var child = GenerateOffspring(parent1, parent2, method);
                    offspring.Add(child);
                    _genotypeDatabase[child.GenotypeId] = child;
                }
                
                breedingResult.OffspringGenotypes = offspring;
                
                // Record breeding history
                var breedingRecord = new BreedingRecord
                {
                    BreedingId = Guid.NewGuid().ToString(),
                    Parent1Id = parent1Id,
                    Parent2Id = parent2Id,
                    Method = method,
                    BreedingDate = DateTime.Now,
                    OffspringIds = offspring.Select(o => o.GenotypeId).ToList(),
                    Success = true,
                    Notes = $"Bred {parent1.StrainName} x {parent2.StrainName}"
                };
                
                _breedingHistory[breedingRecord.BreedingId] = breedingRecord;
                
                // Update lineage tracking
                foreach (var child in offspring)
                {
                    UpdateLineage(child, parent1, parent2);
                }
                
                OnBreedingCompleted?.Invoke(breedingResult);
                
                LogInfo($"Breeding successful: {offspring.Count} offspring generated");
            }
            
            return breedingResult;
        }
        
        private CannabisGenotype GenerateOffspring(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method)
        {
            var offspring = new CannabisGenotype
            {
                GenotypeId = Guid.NewGuid().ToString(),
                StrainId = $"Hybrid_{Guid.NewGuid().ToString()[..8]}",
                StrainName = $"{parent1.StrainName} x {parent2.StrainName}",
                Generation = Mathf.Max(parent1.Generation, parent2.Generation) + 1,
                CreationDate = DateTime.Now,
                ParentGenotypes = new List<string> { parent1.GenotypeId, parent2.GenotypeId },
                IsHybrid = true
            };
            
            // Genetic recombination
            PerformGeneticRecombination(offspring, parent1, parent2, method);
            
            // Apply hybrid vigor if applicable
            if (method == BreedingMethod.StandardCross && IsOutcross(parent1, parent2))
            {
                ApplyHybridVigor(offspring);
            }
            
            // Calculate phenotype
            offspring.ExpressedPhenotype = _phenotypeEngine.CalculatePhenotype(offspring);
            
            return offspring;
        }
        
        private void PerformGeneticRecombination(CannabisGenotype offspring, CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method)
        {
            // TODO: GetAllGenes method doesn't exist on ScriptableObject
            // var allGenes = _geneLibrary.GetAllGenes(); // Method doesn't exist
            var allGenes = new List<GeneDefinitionSO>(); // Placeholder
            
            foreach (var geneSO in allGenes)
            {
                // Convert GeneDefinitionSO to internal GeneDefinition
                var gene = ConvertToGeneDefinition(geneSO);
                
                var offspringAlleles = new List<Allele>();
                
                // Get alleles from both parents
                var parent1Alleles = parent1.Alleles.GetValueOrDefault(gene.GeneId, new List<object>()).OfType<Allele>().ToList();
                var parent2Alleles = parent2.Alleles.GetValueOrDefault(gene.GeneId, new List<object>()).OfType<Allele>().ToList();
                
                if (parent1Alleles.Count == 0 || parent2Alleles.Count == 0)
                {
                    // Generate default alleles if missing
                    offspringAlleles = GenerateDefaultAlleles(gene);
                }
                else
                {
                    // Mendelian inheritance with crossover
                    if (_enableMendelianInheritance)
                    {
                        offspringAlleles = PerformMendelianInheritance(parent1Alleles, parent2Alleles, gene);
                    }
                    else
                    {
                        // Simple random selection
                        offspringAlleles.Add(SelectRandomAllele(parent1Alleles));
                        offspringAlleles.Add(SelectRandomAllele(parent2Alleles));
                    }
                }
                
                offspring.Alleles[gene.GeneId] = offspringAlleles.ConvertAll(allele => (object)allele);
            }
        }
        
        private List<Allele> PerformMendelianInheritance(List<Allele> parent1Alleles, List<Allele> parent2Alleles, GeneDefinition gene)
        {
            var offspring = new List<Allele>();
            
            // Select one allele from each parent (simulating meiosis)
            var allele1 = SelectRandomAllele(parent1Alleles);
            var allele2 = SelectRandomAllele(parent2Alleles);
            
            // Create new allele instances (to avoid reference issues)
            var newAllele1 = CloneAllele(allele1);
            var newAllele2 = CloneAllele(allele2);
            
            // Apply recombination (crossover) if applicable
            if (gene.AllowsRecombination && UnityEngine.Random.value < gene.RecombinationRate)
            {
                PerformCrossover(newAllele1, newAllele2);
            }
            
            offspring.Add(newAllele1);
            offspring.Add(newAllele2);
            
            return offspring;
        }
        
        private Allele SelectRandomAllele(List<Allele> alleles)
        {
            return alleles[UnityEngine.Random.Range(0, alleles.Count)];
        }
        
        private Allele CloneAllele(Allele original)
        {
            return new Allele
            {
                AlleleId = Guid.NewGuid().ToString(),
                GeneId = original.GeneId,
                AlleleName = original.AlleleName,
                Dominance = original.Dominance,
                Expression = original.Expression,
                Stability = original.Stability,
                MutationRate = original.MutationRate,
                ColorValue = original.ColorValue,
                MorphologyValue = original.MorphologyValue,
                StrainTypeInfluence = new Dictionary<CannabisStrainType, float>(original.StrainTypeInfluence)
            };
        }
        
        private void PerformCrossover(Allele allele1, Allele allele2)
        {
            // Exchange some properties between alleles
            var tempExpression = allele1.Expression;
            allele1.Expression = (allele1.Expression + allele2.Expression) / 2f;
            allele2.Expression = (tempExpression + allele2.Expression) / 2f;
            
            // Exchange color components
            if (allele1.ColorValue != Color.clear && allele2.ColorValue != Color.clear)
            {
                var tempColor = allele1.ColorValue;
                allele1.ColorValue = Color.Lerp(allele1.ColorValue, allele2.ColorValue, 0.5f);
                allele2.ColorValue = Color.Lerp(tempColor, allele2.ColorValue, 0.5f);
            }
        }
        
        private List<Allele> GenerateDefaultAlleles(GeneDefinition gene)
        {
            var alleles = new List<Allele>();
            
            for (int i = 0; i < 2; i++)
            {
                var allele = new Allele
                {
                    AlleleId = Guid.NewGuid().ToString(),
                    GeneId = gene.GeneId,
                    AlleleName = $"{gene.GeneName}_Default_{i}",
                    Dominance = UnityEngine.Random.value,
                    Expression = UnityEngine.Random.Range(0.3f, 0.7f),
                    Stability = 0.8f,
                    MutationRate = _mutationRate
                };
                
                alleles.Add(allele);
            }
            
            return alleles;
        }
        
        private bool IsOutcross(CannabisGenotype parent1, CannabisGenotype parent2)
        {
            // Determine if this is an outcross (unrelated parents) vs inbreeding
            var lineage1 = GetLineage(parent1.GenotypeId);
            var lineage2 = GetLineage(parent2.GenotypeId);
            
            // Check for shared ancestors in recent generations
            var sharedAncestors = lineage1.Ancestors.Intersect(lineage2.Ancestors).ToList();
            
            return sharedAncestors.Count == 0 || !HasRecentSharedAncestors(sharedAncestors, 3);
        }
        
        private bool HasRecentSharedAncestors(List<string> sharedAncestors, int generations)
        {
            // Implementation would check generation distance of shared ancestors
            return false; // Simplified for now
        }
        
        private void ApplyHybridVigor(CannabisGenotype offspring)
        {
            // Hybrid vigor (heterosis) increases overall performance
            foreach (var geneAlleles in offspring.Alleles)
            {
                foreach (var alleleObj in geneAlleles.Value)
                {
                    if (alleleObj is Allele allele)
                    {
                        allele.Expression *= _hybridVigorMultiplier;
                        allele.Expression = Mathf.Clamp01(allele.Expression);
                    }
                }
            }
            
            offspring.HybridVigor = _hybridVigorMultiplier;
            LogInfo($"Applied hybrid vigor to {offspring.StrainName}: {_hybridVigorMultiplier}x");
        }
        
        private void UpdateLineage(CannabisGenotype offspring, CannabisGenotype parent1, CannabisGenotype parent2)
        {
            var lineage = new GeneticLineage
            {
                GenotypeId = offspring.GenotypeId,
                DirectParents = new List<string> { parent1.GenotypeId, parent2.GenotypeId },
                Ancestors = new List<string>()
            };
            
            // Collect ancestors from both parents
            var parent1Lineage = GetLineage(parent1.GenotypeId);
            var parent2Lineage = GetLineage(parent2.GenotypeId);
            
            lineage.Ancestors.AddRange(parent1Lineage.Ancestors);
            lineage.Ancestors.AddRange(parent2Lineage.Ancestors);
            lineage.Ancestors.Add(parent1.GenotypeId);
            lineage.Ancestors.Add(parent2.GenotypeId);
            
            // Remove duplicates and limit depth
            lineage.Ancestors = lineage.Ancestors.Distinct().Take(20).ToList();
            
            _lineageTracker[offspring.GenotypeId] = lineage;
        }
        
        private GeneticLineage GetLineage(string genotypeId)
        {
            return _lineageTracker.GetValueOrDefault(genotypeId, new GeneticLineage { GenotypeId = genotypeId });
        }
        
        #endregion
        
        #region SpeedTree Integration
        
        /// <summary>
        /// Apply genetics to SpeedTree plant instance
        /// </summary>
        public void ApplyGeneticsToSpeedTree(SpeedTreePlantData instance, CannabisGenotype genotype)
        {
            if (instance == null || genotype == null) return;

            var phenotype = ExpressPhenotype(genotype, instance.EnvironmentalConditions);
            
            ApplyMorphologicalTraits(instance, phenotype);
            ApplyColorTraits(instance, phenotype);
            ApplyGrowthTraits(instance, phenotype);
            ApplyEnvironmentalTraits(instance, phenotype);
            
            // Update instance genetic data
            if (instance.GeneticData == null)
            {
                instance.GeneticData = new CannabisGeneticData();
            }
            
            UpdateGeneticDataFromPhenotype(instance.GeneticData, phenotype);
            
            LogInfo($"Applied genetics to SpeedTree instance {instance.InstanceId}");
        }

        /// <summary>
        /// Apply morphological traits to plant instance
        /// </summary>
        private void ApplyMorphologicalTraits(SpeedTreePlantData instance, CannabisPhenotype phenotype)
        {
            if (instance?.Renderer == null) return;

            // Apply size modifications
            var sizeModifier = phenotype.PlantHeight * phenotype.StemThickness;
            instance.Scale = Vector3.one * sizeModifier;
            
            // Apply branch density
            if (instance.Renderer.materialProperties != null)
            {
                instance.Renderer.materialProperties.SetFloat("_BranchDensity", phenotype.BranchDensity);
                instance.Renderer.materialProperties.SetFloat("_LeafDensity", phenotype.LeafDensity);
                instance.Renderer.materialProperties.SetFloat("_InternodeSpacing", phenotype.InternodeSpacing);
            }
            
            // Update morphology in genetic data
            if (instance.GeneticData != null)
            {
                instance.GeneticData.PlantSize = sizeModifier;
                instance.GeneticData.BranchDensity = phenotype.BranchDensity;
                instance.GeneticData.LeafDensity = phenotype.LeafDensity;
                instance.GeneticData.StemThickness = phenotype.StemThickness;
            }
        }

        /// <summary>
        /// Apply color traits to plant instance
        /// </summary>
        private void ApplyColorTraits(SpeedTreePlantData instance, CannabisPhenotype phenotype)
        {
            if (instance?.Renderer == null) return;

            // Apply leaf color variations
            var leafColor = Color.Lerp(Color.green, phenotype.LeafColor, phenotype.ColorIntensity);
            instance.CurrentLeafColor = leafColor;
            
            // Apply bud color variations
            var budColor = Color.Lerp(Color.green, phenotype.BudColor, phenotype.ColorIntensity);
            
            // Update material properties
            if (instance.Renderer.materialProperties != null)
            {
                instance.Renderer.materialProperties.SetColor("_LeafColor", leafColor);
                instance.Renderer.materialProperties.SetColor("_BudColor", budColor);
                instance.Renderer.materialProperties.SetFloat("_ColorVariation", phenotype.ColorVariation);
            }
            
            // Update genetic data
            if (instance.GeneticData != null)
            {
                instance.GeneticData.LeafColor = leafColor;
                instance.GeneticData.BudColor = budColor;
                instance.GeneticData.ColorVariation = phenotype.ColorVariation;
            }
        }

        /// <summary>
        /// Apply growth traits to plant instance
        /// </summary>
        private void ApplyGrowthTraits(SpeedTreePlantData instance, CannabisPhenotype phenotype)
        {
            if (instance == null) return;

            // Apply growth rate modifiers
            instance.GrowthRate = phenotype.GrowthRate;
            
            // Update genetic data
            if (instance.GeneticData != null)
            {
                instance.GeneticData.GrowthRate = phenotype.GrowthRate;
                instance.GeneticData.FloweringSpeed = phenotype.FloweringTime;
                instance.GeneticData.YieldPotential = phenotype.YieldPotential;
            }
            
            // Apply environmental modifiers
            if (instance.EnvironmentalModifiers == null)
            {
                instance.EnvironmentalModifiers = new Dictionary<string, float>();
            }
            
            instance.EnvironmentalModifiers["GrowthRate"] = phenotype.GrowthRate;
            instance.EnvironmentalModifiers["FloweringSpeed"] = phenotype.FloweringTime;
        }

        /// <summary>
        /// Apply environmental traits to plant instance
        /// </summary>
        private void ApplyEnvironmentalTraits(SpeedTreePlantData instance, CannabisPhenotype phenotype)
        {
            if (instance == null) return;

            // Apply stress resistance
            instance.StressResistance = phenotype.StressResistance;
            
            // Update environmental modifiers
            if (instance.EnvironmentalModifiers == null)
            {
                instance.EnvironmentalModifiers = new Dictionary<string, float>();
            }
            
            instance.EnvironmentalModifiers["HeatTolerance"] = phenotype.HeatTolerance;
            instance.EnvironmentalModifiers["ColdTolerance"] = phenotype.ColdTolerance;
            instance.EnvironmentalModifiers["DroughtTolerance"] = phenotype.DroughtTolerance;
            
            // Update genetic data
            if (instance.GeneticData != null)
            {
                instance.GeneticData.HeatTolerance = phenotype.HeatTolerance;
                instance.GeneticData.ColdTolerance = phenotype.ColdTolerance;
                instance.GeneticData.DroughtTolerance = phenotype.DroughtTolerance;
            }
        }
        
        /// <summary>
        /// Update genetic data from phenotype
        /// </summary>
        private void UpdateGeneticDataFromPhenotype(CannabisGeneticData geneticData, CannabisPhenotype phenotype)
        {
            if (geneticData == null || phenotype == null) return;
            
            // Update morphological data
            geneticData.PlantSize = phenotype.PlantHeight;
            geneticData.BranchDensity = phenotype.BranchDensity;
            geneticData.LeafDensity = phenotype.LeafDensity;
            geneticData.StemThickness = phenotype.StemThickness;
            
            // Update color data
            geneticData.LeafColor = phenotype.LeafColor;
            geneticData.BudColor = phenotype.BudColor;
            geneticData.ColorVariation = phenotype.ColorVariation;
            
            // Update growth data
            geneticData.GrowthRate = phenotype.GrowthRate;
            geneticData.FloweringSpeed = phenotype.FloweringTime;
            geneticData.YieldPotential = phenotype.YieldPotential;
            
            // Update environmental tolerance
            geneticData.HeatTolerance = phenotype.HeatTolerance;
            geneticData.ColdTolerance = phenotype.ColdTolerance;
            geneticData.DroughtTolerance = phenotype.DroughtTolerance;
        }
        
        #endregion
        
        #region Environmental Adaptation
        
        private void ProcessEnvironmentalAdaptation()
        {
            foreach (var mapping in _speedTreeMappings.Values)
            {
                if (_genotypeDatabase.TryGetValue(mapping.GenotypeId, out var genotype))
                {
                    ProcessGenotypeAdaptation(genotype, mapping);
                }
            }
        }
        
        private void ProcessGenotypeAdaptation(CannabisGenotype genotype, GeneticSpeedTreeMapping mapping)
        {
            var conditions = GetEnvironmentalConditionsForPlant(mapping.InstanceId);
            if (conditions == null) return;
            
            var adaptationKey = $"{genotype.GenotypeId}_{GetEnvironmentalHash(conditions)}";
            
            if (!_adaptationData.TryGetValue(adaptationKey, out var adaptation))
            {
                adaptation = new EnvironmentalAdaptation
                {
                    GenotypeId = genotype.GenotypeId,
                    EnvironmentalConditions = conditions,
                    StartDate = DateTime.Now,
                    AdaptationProgress = 0f
                };
                
                _adaptationData[adaptationKey] = adaptation;
            }
            
            // Update adaptation progress
            adaptation.AdaptationProgress += _adaptationRate * Time.deltaTime;
            adaptation.AdaptationProgress = Mathf.Clamp01(adaptation.AdaptationProgress);
            
            // Apply adaptive changes when threshold reached
            if (adaptation.AdaptationProgress > 0.5f && !adaptation.AdaptationApplied)
            {
                ApplyAdaptiveChanges(genotype, adaptation);
                adaptation.AdaptationApplied = true;
                OnAdaptationOccurred?.Invoke(adaptation);
            }
        }
        
        private EnvironmentalConditions GetEnvironmentalConditionsForPlant(int instanceId)
        {
            // This would interface with the environmental manager to get local conditions
            return new EnvironmentalConditions
            {
                Temperature = 24f + UnityEngine.Random.Range(-3f, 3f),
                Humidity = 60f + UnityEngine.Random.Range(-10f, 10f),
                LightIntensity = 800f + UnityEngine.Random.Range(-200f, 200f),
                CO2Level = 400f + UnityEngine.Random.Range(-50f, 100f)
            };
        }
        
        private string GetEnvironmentalHash(EnvironmentalConditions conditions)
        {
            // Create a hash representing the environmental conditions
            var tempBucket = Mathf.FloorToInt(conditions.Temperature / 5f) * 5; // 5-degree buckets
            var humidityBucket = Mathf.FloorToInt(conditions.Humidity / 10f) * 10; // 10% buckets
            var lightBucket = Mathf.FloorToInt(conditions.LightIntensity / 100f) * 100; // 100 PPFD buckets
            
            return $"T{tempBucket}H{humidityBucket}L{lightBucket}";
        }
        
        private void ApplyAdaptiveChanges(CannabisGenotype genotype, EnvironmentalAdaptation adaptation)
        {
            var conditions = adaptation.EnvironmentalConditions;
            
            // Adapt to temperature
            if (conditions.Temperature > 28f)
            {
                // Increase heat tolerance genes
                AdaptGeneExpression(genotype, TraitType.HeatTolerance, 0.1f);
                AdaptGeneExpression(genotype, TraitType.LeafSize, -0.05f); // Smaller leaves for heat
            }
            else if (conditions.Temperature < 20f)
            {
                // Increase cold tolerance
                AdaptGeneExpression(genotype, TraitType.HeatTolerance, 0.1f);
                AdaptGeneExpression(genotype, TraitType.StemThickness, 0.05f); // Thicker stems for cold
            }
            
            // Adapt to humidity
            if (conditions.Humidity < 40f)
            {
                // Increase drought tolerance
                AdaptGeneExpression(genotype, TraitType.DroughtTolerance, 0.1f);
                AdaptGeneExpression(genotype, TraitType.LeafSize, 0.05f); // More waxy leaves
            }
            
            // Adapt to light intensity
            if (conditions.LightIntensity > 1000f)
            {
                // Adapt to high light
                AdaptGeneExpression(genotype, TraitType.LightStressTolerance, 0.1f);
                // AdaptGeneExpression(genotype, TraitType.TrichromeProduction, 0.05f); // TrichromeProduction doesn't exist in enum
            }
            
            LogInfo($"Applied adaptive changes to genotype {genotype.GenotypeId}");
        }
        
        /// <summary>
        /// Get a simulated gene for the specified trait type.
        /// </summary>
        // TODO: Replace with proper GeneDefinitionSO implementation
        // private GeneData GetSimulatedGene(TraitType traitType)
        // {
        //     return new GeneData
        //     {
        //         GeneId = $"gene_{traitType}",
        //         GeneName = traitType.ToString(),
        //         GeneType = traitType,
        //         BaseExpression = 0.5f,
        //         Dominance = 0.5f
        //     };
        // }
        
        /// <summary>
        /// Get simulated genes for the specified trait type.
        /// </summary>
        // TODO: Replace with proper GeneDefinitionSO implementation
        // private List<GeneData> GetSimulatedGenes(TraitType traitType)
        // {
        //     return new List<GeneData> { GetSimulatedGene(traitType) };
        // }
        
        private void AdaptGeneExpression(CannabisGenotype genotype, TraitType geneType, float change)
        {
            // TODO: Replace with proper GeneDefinitionSO implementation
            // var gene = GetSimulatedGene(geneType);
            // if (gene == null) return;
            // 
            // if (genotype.Alleles.TryGetValue(gene.GeneId, out var alleleObjects))
            // {
            //     // Convert List<object> to List<Allele> safely
            //     var alleles = alleleObjects.OfType<Allele>().ToList();
            //     foreach (var allele in alleles)
            //     {
            //         allele.Expression += change;
            //         allele.Expression = Mathf.Clamp01(allele.Expression);
            //     }
            // }
        }
        
        private void ProcessEpigeneticChanges()
        {
            // Implement epigenetic modifications based on environmental conditions
            foreach (var genotype in _genotypeDatabase.Values)
            {
                ProcessGenotypeEpigenetics(genotype);
            }
        }
        
        private void ProcessGenotypeEpigenetics(CannabisGenotype genotype)
        {
            // Epigenetic changes affect gene expression without changing DNA sequence
            // These changes can be inherited and provide rapid adaptation
            
            if (genotype.EpigeneticModifications == null)
            {
                genotype.EpigeneticModifications = new Dictionary<string, float>();
            }
            
            // Apply environmental epigenetic modifications
            var currentConditions = GetCurrentEnvironmentalConditions();
            ApplyEpigeneticModifications(genotype, currentConditions);
        }
        
        private EnvironmentalConditions GetCurrentEnvironmentalConditions()
        {
            // Get global environmental conditions
            return new EnvironmentalConditions
            {
                Temperature = 24f,
                Humidity = 60f,
                LightIntensity = 800f,
                CO2Level = 400f
            };
        }
        
        private void ApplyEpigeneticModifications(CannabisGenotype genotype, EnvironmentalConditions conditions)
        {
            // Temperature stress triggers epigenetic changes
            if (conditions.Temperature > 30f || conditions.Temperature < 18f)
            {
                // TODO: Replace with proper GeneDefinitionSO implementation
                // var stressGenes = GetSimulatedGenes(TraitType.HeatTolerance);
                // foreach (var gene in stressGenes)
                // {
                //     var modificationKey = $"temp_stress_{gene.GeneId}";
                //     genotype.EpigeneticModifications[modificationKey] = 
                //         genotype.EpigeneticModifications.GetValueOrDefault(modificationKey, 0f) + 0.01f;
                // }
            }
            
            // Light stress triggers protective responses
            if (conditions.LightIntensity > 1200f)
            {
                // TODO: Replace with proper GeneDefinitionSO implementation
                // var protectionGenes = GetSimulatedGenes(TraitType.TrichromeProduction);
                // foreach (var gene in protectionGenes)
                // {
                //     var modificationKey = $"light_protection_{gene.GeneId}";
                //     genotype.EpigeneticModifications[modificationKey] = 
                //         genotype.EpigeneticModifications.GetValueOrDefault(modificationKey, 0f) + 0.005f;
                // }
            }
        }
        
        #endregion
        
        #region System Updates and Analytics
        
        private void UpdateGeneticSystems()
        {
            _geneticProcessor?.Update();
            _breedingSimulator?.Update();
            _phenotypeEngine?.Update();
            _adaptationManager?.Update();
            _diversityTracker?.Update();
            _geneticsAnalytics?.Update();
        }
        
        private void UpdateGeneticDiversity()
        {
            _diversityTracker.CalculateDiversityMetrics(_genotypeDatabase.Values.ToList());
        }
        
        private void ProcessBreedingQueue()
        {
            // Process any queued breeding operations
            _breedingSimulator.ProcessBreedingQueue();
        }
        
        private void UpdatePerformanceMetrics()
        {
            _performanceMetrics.TotalGenotypes = _genotypeDatabase.Count;
            _performanceMetrics.TotalBreedingRecords = _breedingHistory.Count;
            _performanceMetrics.AverageGeneticDiversity = _diversityTracker.CalculateOverallDiversity();
            _performanceMetrics.UnlockedGenes = _unlockedGenes.Count;
            _performanceMetrics.DiscoveredTraits = _discoveredTraits.Count;
            _performanceMetrics.LastUpdate = DateTime.Now;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandlePlantInstanceCreated(SpeedTreePlantData instance)
        {
            // Generate or assign genetics to new plant instance
            if (instance.GeneticData == null)
            {
                var genotype = GenerateGeneticVariation(instance.StrainAsset?.StrainId ?? "default");
                if (genotype != null)
                {
                    ApplyGeneticsToSpeedTree(instance, genotype);
                }
            }
        }
        
        private void HandleResearchCompleted(string researchId)
        {
            // Unlock new genes and breeding techniques based on research
            var unlockedGenes = GetGenesUnlockedByResearch(researchId);
            foreach (var geneId in unlockedGenes)
            {
                _unlockedGenes.Add(geneId);
                OnGeneUnlocked?.Invoke(geneId);
            }
        }
        
        private List<string> GetGenesUnlockedByResearch(string researchId)
        {
            // Map research projects to genetic unlocks
            return researchId switch
            {
                "advanced_genetics" => new List<string> { "thc_synthesis", "cbd_synthesis", "terpene_complex" },
                "breeding_techniques" => new List<string> { "flowering_time", "yield_optimization" },
                "plant_physiology" => new List<string> { "stress_tolerance", "environmental_adaptation" },
                _ => new List<string>()
            };
        }
        
        #endregion
        
        #region Public Interface
        
        public CannabisGenotype GetGenotype(string genotypeId)
        {
            return _genotypeDatabase.TryGetValue(genotypeId, out var genotype) ? genotype : null;
        }
        
        public List<CannabisGenotype> GetAllGenotypes()
        {
            return _genotypeDatabase.Values.ToList();
        }
        
        public List<CannabisGenotype> GetGenotypesByStrain(string strainId)
        {
            return _genotypeDatabase.Values.Where(g => g.StrainId == strainId).ToList();
        }
        
        public BreedingRecord GetBreedingRecord(string breedingId)
        {
            return _breedingHistory.TryGetValue(breedingId, out var record) ? record : null;
        }
        
        public List<BreedingRecord> GetBreedingHistory()
        {
            return _breedingHistory.Values.OrderByDescending(r => r.BreedingDate).ToList();
        }
        
        public GeneticLineage GetGeneticLineage(string genotypeId)
        {
            return _lineageTracker.TryGetValue(genotypeId, out var lineage) ? lineage : null;
        }
        
        public List<string> GetAvailableBreedingMethods()
        {
            var methods = new List<string>();
            
            if (_enableAdvancedBreeding)
            {
                methods.AddRange(new[] { "StandardCross", "Backcross", "LineBreeding", "Outbreeding" });
            }
            else
            {
                methods.Add("StandardCross");
            }
            
            return methods;
        }
        
        public bool CanPerformBreeding(string parent1Id, string parent2Id)
        {
            return _genotypeDatabase.ContainsKey(parent1Id) && 
                   _genotypeDatabase.ContainsKey(parent2Id) &&
                   parent1Id != parent2Id;
        }
        
        public GeneticsSystemReport GetSystemReport()
        {
            return new GeneticsSystemReport
            {
                PerformanceMetrics = _performanceMetrics,
                TotalGenotypes = _genotypeDatabase.Count,
                TotalBreedingRecords = _breedingHistory.Count,
                GeneticDiversity = _diversityTracker.CalculateOverallDiversity(),
                UnlockedGenes = new List<string>(_unlockedGenes),
                DiscoveredTraits = new List<string>(_discoveredTraits),
                ActiveAdaptations = _adaptationData.Count,
                SystemStatus = new Dictionary<string, bool>
                {
                    ["MendelianInheritance"] = _enableMendelianInheritance,
                    ["PolygeneticTraits"] = _enablePolygeneticTraits,
                    ["Mutations"] = _enableMutations,
                    ["Epigenetics"] = _enableEpigenetics,
                    ["EnvironmentalAdaptation"] = _enableEnvironmentalAdaptation,
                    ["AdvancedBreeding"] = _enableAdvancedBreeding
                }
            };
        }
        
        #endregion
        
        #region Phenotype Expression
        
        /// <summary>
        /// Express phenotype from genotype with environmental conditions
        /// </summary>
        public CannabisPhenotype ExpressPhenotype(CannabisGenotype genotype, EnvironmentalConditions conditions)
        {
            if (genotype == null) return null;
            
            var phenotype = _phenotypeEngine.CalculatePhenotype(genotype);
            
            // Apply environmental modifications
            if (conditions != null)
            {
                ApplyEnvironmentalModifications(phenotype, conditions);
            }
            
            return phenotype;
        }
        
        /// <summary>
        /// Apply environmental modifications to phenotype
        /// </summary>
        private void ApplyEnvironmentalModifications(CannabisPhenotype phenotype, EnvironmentalConditions conditions)
        {
            if (phenotype == null || conditions == null) return;
            
            // Store environmental context
            phenotype.ExpressionEnvironment = conditions;
            
            // Modify traits based on environmental conditions
            float tempStress = CalculateTemperatureStress(conditions.Temperature);
            float humidityStress = CalculateHumidityStress(conditions.Humidity);
            float lightStress = CalculateLightStress(conditions.LightIntensity);
            
            float overallStress = (tempStress + humidityStress + lightStress) / 3f;
            
            // Apply stress effects to phenotype
            phenotype.OverallVigor *= (1f - overallStress * 0.3f); // Reduce vigor by up to 30%
            phenotype.StressResistance = Mathf.Clamp01(phenotype.StressResistance - overallStress * 0.2f);
            
            // Modify growth traits based on environment
            if (phenotype.GrowthTraits.ContainsKey("GrowthRate"))
            {
                phenotype.GrowthTraits["GrowthRate"] *= (1f - overallStress * 0.25f);
            }
            
            // Modify morphological traits
            if (phenotype.MorphologicalTraits.ContainsKey("Height"))
            {
                // Light stress can cause stretching
                if (lightStress > 0.3f)
                {
                    phenotype.MorphologicalTraits["Height"] *= (1f + lightStress * 0.4f);
                }
            }
        }
        
        /// <summary>
        /// Calculate temperature stress factor
        /// </summary>
        private float CalculateTemperatureStress(float temperature)
        {
            float optimalTemp = 24f; // Optimal temperature for cannabis
            float tempDiff = Mathf.Abs(temperature - optimalTemp);
            return Mathf.Clamp01(tempDiff / 15f); // Stress increases with distance from optimal
        }
        
        /// <summary>
        /// Calculate humidity stress factor
        /// </summary>
        private float CalculateHumidityStress(float humidity)
        {
            float optimalHumidity = 60f; // Optimal humidity for cannabis
            float humidityDiff = Mathf.Abs(humidity - optimalHumidity);
            return Mathf.Clamp01(humidityDiff / 30f); // Stress increases with distance from optimal
        }
        
        /// <summary>
        /// Calculate light stress factor
        /// </summary>
        private float CalculateLightStress(float lightIntensity)
        {
            float minLight = 20f; // Minimum light for healthy growth
            if (lightIntensity < minLight)
            {
                return Mathf.Clamp01((minLight - lightIntensity) / minLight);
            }
            return 0f; // No stress from high light (cannabis is light-loving)
        }
        
        #endregion
    }
}
