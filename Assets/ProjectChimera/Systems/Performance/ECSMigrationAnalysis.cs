using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Systems.Performance
{
    /// <summary>
    /// Analysis and evaluation of Unity DOTS/ECS migration for Project Chimera plant simulation systems.
    /// Part of PC016-2: Evaluate and prepare for ECS migration
    /// </summary>
    public class ECSMigrationAnalysis : ChimeraManager
    {
        [Header("ECS Migration Configuration")]
        [SerializeField] private bool _enableECSAnalysis = true;
        [SerializeField] private bool _enablePerformanceProfiling = true;
        [SerializeField] private bool _enableMemoryProfiling = true;
        [SerializeField] private float _analysisInterval = 5.0f;
        [SerializeField] private int _maxPlantsForAnalysis = 1000;
        
        [Header("Migration Target Systems")]
        [SerializeField] private bool _analyzePlantSimulation = true;
        [SerializeField] private bool _analyzeGrowthProcessing = true;
        [SerializeField] private bool _analyzeEnvironmentalEffects = true;
        [SerializeField] private bool _analyzeGeneticsProcessing = true;
        [SerializeField] private bool _analyzeRenderingOptimization = true;
        
        [Header("Performance Baselines")]
        [SerializeField] private float _targetFrameRate = 60.0f;
        [SerializeField] private int _maxPlantCount = 10000;
        [SerializeField] private float _memoryBudgetMB = 512.0f;
        [SerializeField] private float _updateFrequencyHz = 30.0f;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onECSAnalysisComplete;
        [SerializeField] private SimpleGameEventSO _onPerformanceBaseline;
        [SerializeField] private SimpleGameEventSO _onMigrationRecommendation;
        
        // Analysis data
        private Dictionary<string, SystemAnalysis> _systemAnalyses = new Dictionary<string, SystemAnalysis>();
        private List<PerformanceBaseline> _performanceBaselines = new List<PerformanceBaseline>();
        private ECSMigrationReport _migrationReport;
        private float _lastAnalysisTime;
        
        // Performance tracking
        private PerformanceProfiler _performanceProfiler;
        private MemoryProfiler _memoryProfiler;
        private int _currentPlantCount;
        private float _currentFrameRate;
        private float _currentMemoryUsage;
        
        #region ChimeraManager Implementation
        
        protected override void OnManagerInitialize()
        {
            InitializeECSAnalysis();
            InitializeProfilers();
            EstablishPerformanceBaselines();
            
            _lastAnalysisTime = Time.time;
            _migrationReport = new ECSMigrationReport();
            
            Debug.Log($"[ECSMigrationAnalysis] Initialized ECS migration analysis system");
            Debug.Log($"   - Target frame rate: {_targetFrameRate} FPS");
            Debug.Log($"   - Max plant count: {_maxPlantCount}");
            Debug.Log($"   - Memory budget: {_memoryBudgetMB} MB");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized || !_enableECSAnalysis) return;
            
            float deltaTime = Time.time - _lastAnalysisTime;
            if (deltaTime >= _analysisInterval)
            {
                UpdatePerformanceMetrics();
                AnalyzeCurrentSystems();
                EvaluateECSOpportunities();
                UpdateMigrationReport();
                
                _lastAnalysisTime = Time.time;
            }
        }
        
        protected override void OnManagerShutdown()
        {
            GenerateFinalMigrationReport();
            SaveAnalysisResults();
            
            _systemAnalyses.Clear();
            _performanceBaselines.Clear();
            
            Debug.Log("[ECSMigrationAnalysis] ECS migration analysis completed and results saved");
        }
        
        #endregion
        
        #region System Analysis
        
        /// <summary>
        /// Analyze current MonoBehaviour-based systems for ECS migration potential
        /// </summary>
        private void AnalyzeCurrentSystems()
        {
            if (_analyzePlantSimulation)
                AnalyzePlantSimulationSystem();
                
            if (_analyzeGrowthProcessing)
                AnalyzeGrowthProcessingSystem();
                
            if (_analyzeEnvironmentalEffects)
                AnalyzeEnvironmentalEffectsSystem();
                
            if (_analyzeGeneticsProcessing)
                AnalyzeGeneticsProcessingSystem();
                
            if (_analyzeRenderingOptimization)
                AnalyzeRenderingOptimizationSystem();
        }
        
        /// <summary>
        /// Analyze plant simulation system for ECS conversion
        /// </summary>
        private void AnalyzePlantSimulationSystem()
        {
            var analysis = new SystemAnalysis
            {
                SystemName = "PlantSimulation",
                CurrentArchitecture = "MonoBehaviour",
                ComponentCount = CountPlantComponents(),
                UpdateFrequency = CalculateUpdateFrequency("Plant"),
                MemoryUsage = CalculateMemoryUsage("Plant"),
                CPUUsage = CalculateCPUUsage("Plant"),
                ECSMigrationScore = CalculateECSMigrationScore("Plant")
            };
            
            // Analyze specific plant simulation aspects
            analysis.MigrationBenefits.Add("Massive parallelization of plant growth calculations");
            analysis.MigrationBenefits.Add("Reduced memory overhead through component stripping");
            analysis.MigrationBenefits.Add("Burst compilation for mathematical plant operations");
            analysis.MigrationBenefits.Add("Job system utilization for environmental processing");
            
            analysis.MigrationChallenges.Add("Complex plant state management");
            analysis.MigrationChallenges.Add("Visual representation coupling");
            analysis.MigrationChallenges.Add("ScriptableObject data integration");
            analysis.MigrationChallenges.Add("Custom editor tool compatibility");
            
            analysis.RecommendedApproach = DetermineMigrationApproach(analysis);
            
            _systemAnalyses["PlantSimulation"] = analysis;
            
            Debug.Log($"[ECSMigrationAnalysis] Plant Simulation Analysis:");
            Debug.Log($"   - Components: {analysis.ComponentCount}");
            Debug.Log($"   - Memory Usage: {analysis.MemoryUsage:F2} MB");
            Debug.Log($"   - ECS Score: {analysis.ECSMigrationScore:F2}/10");
            Debug.Log($"   - Approach: {analysis.RecommendedApproach}");
        }
        
        /// <summary>
        /// Analyze growth processing system for ECS conversion
        /// </summary>
        private void AnalyzeGrowthProcessingSystem()
        {
            var analysis = new SystemAnalysis
            {
                SystemName = "GrowthProcessing",
                CurrentArchitecture = "MonoBehaviour",
                ComponentCount = CountGrowthComponents(),
                UpdateFrequency = CalculateUpdateFrequency("Growth"),
                MemoryUsage = CalculateMemoryUsage("Growth"),
                CPUUsage = CalculateCPUUsage("Growth"),
                ECSMigrationScore = CalculateECSMigrationScore("Growth")
            };
            
            // Growth-specific analysis
            analysis.MigrationBenefits.Add("Highly parallel growth calculations across thousands of plants");
            analysis.MigrationBenefits.Add("SIMD optimization for mathematical growth formulas");
            analysis.MigrationBenefits.Add("Memory-efficient growth stage transitions");
            analysis.MigrationBenefits.Add("Batch processing of environmental effects");
            
            analysis.MigrationChallenges.Add("Complex growth stage state machines");
            analysis.MigrationChallenges.Add("Dynamic mesh generation integration");
            analysis.MigrationChallenges.Add("Animation system compatibility");
            
            analysis.RecommendedApproach = DetermineMigrationApproach(analysis);
            _systemAnalyses["GrowthProcessing"] = analysis;
        }
        
        /// <summary>
        /// Analyze environmental effects system for ECS conversion
        /// </summary>
        private void AnalyzeEnvironmentalEffectsSystem()
        {
            var analysis = new SystemAnalysis
            {
                SystemName = "EnvironmentalEffects",
                CurrentArchitecture = "MonoBehaviour",
                ComponentCount = CountEnvironmentalComponents(),
                UpdateFrequency = CalculateUpdateFrequency("Environmental"),
                MemoryUsage = CalculateMemoryUsage("Environmental"),
                CPUUsage = CalculateCPUUsage("Environmental"),
                ECSMigrationScore = CalculateECSMigrationScore("Environmental")
            };
            
            // Environmental-specific analysis
            analysis.MigrationBenefits.Add("Spatial queries for environmental zones");
            analysis.MigrationBenefits.Add("Parallel processing of climate effects");
            analysis.MigrationBenefits.Add("Efficient neighbor detection for plant interactions");
            analysis.MigrationBenefits.Add("Real-time environmental gradient calculations");
            
            analysis.MigrationChallenges.Add("Complex environmental zone management");
            analysis.MigrationChallenges.Add("Sensor network integration");
            analysis.MigrationChallenges.Add("Real-time environmental data streaming");
            
            analysis.RecommendedApproach = DetermineMigrationApproach(analysis);
            _systemAnalyses["EnvironmentalEffects"] = analysis;
        }
        
        /// <summary>
        /// Analyze genetics processing system for ECS conversion
        /// </summary>
        private void AnalyzeGeneticsProcessingSystem()
        {
            var analysis = new SystemAnalysis
            {
                SystemName = "GeneticsProcessing",
                CurrentArchitecture = "MonoBehaviour",
                ComponentCount = CountGeneticComponents(),
                UpdateFrequency = CalculateUpdateFrequency("Genetics"),
                MemoryUsage = CalculateMemoryUsage("Genetics"),
                CPUUsage = CalculateCPUUsage("Genetics"),
                ECSMigrationScore = CalculateECSMigrationScore("Genetics")
            };
            
            // Genetics-specific analysis
            analysis.MigrationBenefits.Add("Parallel genetic expression calculations");
            analysis.MigrationBenefits.Add("Efficient breeding simulation processing");
            analysis.MigrationBenefits.Add("Memory-optimized genetic data storage");
            analysis.MigrationBenefits.Add("Batch processing of trait calculations");
            
            analysis.MigrationChallenges.Add("Complex genetic inheritance algorithms");
            analysis.MigrationChallenges.Add("Phenotype-genotype mapping complexity");
            analysis.MigrationChallenges.Add("Breeding history data management");
            
            analysis.RecommendedApproach = DetermineMigrationApproach(analysis);
            _systemAnalyses["GeneticsProcessing"] = analysis;
        }
        
        /// <summary>
        /// Analyze rendering optimization potential for ECS
        /// </summary>
        private void AnalyzeRenderingOptimizationSystem()
        {
            var analysis = new SystemAnalysis
            {
                SystemName = "RenderingOptimization",
                CurrentArchitecture = "MonoBehaviour",
                ComponentCount = CountRenderingComponents(),
                UpdateFrequency = CalculateUpdateFrequency("Rendering"),
                MemoryUsage = CalculateMemoryUsage("Rendering"),
                CPUUsage = CalculateCPUUsage("Rendering"),
                ECSMigrationScore = CalculateECSMigrationScore("Rendering")
            };
            
            // Rendering-specific analysis
            analysis.MigrationBenefits.Add("GPU instancing for thousands of plants");
            analysis.MigrationBenefits.Add("Efficient culling and LOD management");
            analysis.MigrationBenefits.Add("Parallel transform updates");
            analysis.MigrationBenefits.Add("Optimized material property blocks");
            
            analysis.MigrationChallenges.Add("Custom rendering pipeline integration");
            analysis.MigrationChallenges.Add("SpeedTree compatibility");
            analysis.MigrationChallenges.Add("Dynamic mesh updates");
            
            analysis.RecommendedApproach = DetermineMigrationApproach(analysis);
            _systemAnalyses["RenderingOptimization"] = analysis;
        }
        
        #endregion
        
        #region ECS Opportunity Evaluation
        
        /// <summary>
        /// Evaluate specific ECS opportunities and benefits
        /// </summary>
        private void EvaluateECSOpportunities()
        {
            var opportunities = new List<ECSOpportunity>();
            
            // Plant simulation opportunities
            opportunities.Add(new ECSOpportunity
            {
                SystemName = "PlantLifecycleSystem",
                ExpectedPerformanceGain = 5.0f, // 5x improvement
                ImplementationComplexity = ComplexityLevel.High,
                Description = "Convert plant lifecycle management to ECS for massive parallelization",
                RequiredComponents = new List<string> 
                { 
                    "PlantLifecycleComponent", 
                    "GrowthStageComponent", 
                    "PlantHealthComponent",
                    "EnvironmentalResponseComponent"
                }
            });
            
            // Growth processing opportunities
            opportunities.Add(new ECSOpportunity
            {
                SystemName = "GrowthCalculationSystem",
                ExpectedPerformanceGain = 8.0f, // 8x improvement
                ImplementationComplexity = ComplexityLevel.Medium,
                Description = "Parallel growth calculations with Burst compilation",
                RequiredComponents = new List<string>
                {
                    "GrowthRateComponent",
                    "BiomassComponent",
                    "NutrientUptakeComponent"
                }
            });
            
            // Environmental processing opportunities
            opportunities.Add(new ECSOpportunity
            {
                SystemName = "EnvironmentalProcessingSystem",
                ExpectedPerformanceGain = 3.0f, // 3x improvement
                ImplementationComplexity = ComplexityLevel.Medium,
                Description = "Spatial queries and environmental effect processing",
                RequiredComponents = new List<string>
                {
                    "EnvironmentalZoneComponent",
                    "ClimateEffectComponent",
                    "SpatialPositionComponent"
                }
            });
            
            // Genetics processing opportunities
            opportunities.Add(new ECSOpportunity
            {
                SystemName = "GeneticExpressionSystem",
                ExpectedPerformanceGain = 4.0f, // 4x improvement
                ImplementationComplexity = ComplexityLevel.High,
                Description = "Parallel genetic trait expression and inheritance calculations",
                RequiredComponents = new List<string>
                {
                    "GenotypeComponent",
                    "PhenotypeComponent",
                    "TraitExpressionComponent"
                }
            });
            
            // Rendering optimization opportunities
            opportunities.Add(new ECSOpportunity
            {
                SystemName = "RenderingBatchingSystem",
                ExpectedPerformanceGain = 10.0f, // 10x improvement
                ImplementationComplexity = ComplexityLevel.Low,
                Description = "GPU instancing and efficient rendering batching",
                RequiredComponents = new List<string>
                {
                    "RenderDataComponent",
                    "TransformComponent",
                    "MaterialPropertyComponent"
                }
            });
            
            _migrationReport.ECSOpportunities = opportunities;
            
            Debug.Log($"[ECSMigrationAnalysis] Identified {opportunities.Count} ECS opportunities:");
            foreach (var opportunity in opportunities)
            {
                Debug.Log($"   - {opportunity.SystemName}: {opportunity.ExpectedPerformanceGain}x gain ({opportunity.ImplementationComplexity} complexity)");
            }
        }
        
        #endregion
        
        #region Performance Profiling
        
        /// <summary>
        /// Update current performance metrics
        /// </summary>
        private void UpdatePerformanceMetrics()
        {
            if (_enablePerformanceProfiling)
            {
                _currentFrameRate = 1.0f / Time.unscaledDeltaTime;
                _currentPlantCount = CountTotalPlants();
                
                if (_enableMemoryProfiling)
                {
                    _currentMemoryUsage = UnityEngine.Profiling.Profiler.GetTotalReservedMemory() / (1024f * 1024f);
                }
                
                // Record baseline if significant change
                if (ShouldRecordBaseline())
                {
                    RecordPerformanceBaseline();
                }
            }
        }
        
        /// <summary>
        /// Record current performance as baseline
        /// </summary>
        private void RecordPerformanceBaseline()
        {
            var baseline = new PerformanceBaseline
            {
                Timestamp = DateTime.Now,
                PlantCount = _currentPlantCount,
                FrameRate = _currentFrameRate,
                MemoryUsage = _currentMemoryUsage,
                CPUUsage = GetCurrentCPUUsage(),
                SystemLoad = CalculateSystemLoad()
            };
            
            _performanceBaselines.Add(baseline);
            _onPerformanceBaseline?.Raise();
            
            Debug.Log($"[ECSMigrationAnalysis] Performance baseline recorded:");
            Debug.Log($"   - Plants: {baseline.PlantCount}");
            Debug.Log($"   - FPS: {baseline.FrameRate:F1}");
            Debug.Log($"   - Memory: {baseline.MemoryUsage:F1} MB");
        }
        
        #endregion
        
        #region Migration Report Generation
        
        /// <summary>
        /// Update migration report with current analysis
        /// </summary>
        private void UpdateMigrationReport()
        {
            _migrationReport.AnalysisTimestamp = DateTime.Now;
            _migrationReport.SystemAnalyses = new List<SystemAnalysis>(_systemAnalyses.Values);
            _migrationReport.PerformanceBaselines = _performanceBaselines;
            _migrationReport.OverallMigrationScore = CalculateOverallMigrationScore();
            _migrationReport.RecommendedPriority = DetermineRecommendedPriority();
            
            // Update recommendations
            UpdateMigrationRecommendations();
        }
        
        /// <summary>
        /// Generate final comprehensive migration report
        /// </summary>
        private void GenerateFinalMigrationReport()
        {
            _migrationReport.FinalRecommendations = GenerateFinalRecommendations();
            _migrationReport.ImplementationRoadmap = GenerateImplementationRoadmap();
            _migrationReport.RiskAssessment = GenerateRiskAssessment();
            
            _onMigrationRecommendation?.Raise();
            
            Debug.Log("[ECSMigrationAnalysis] Final migration report generated:");
            Debug.Log($"   - Overall Score: {_migrationReport.OverallMigrationScore:F1}/10");
            Debug.Log($"   - Recommended Priority: {_migrationReport.RecommendedPriority}");
            Debug.Log($"   - Systems Analyzed: {_migrationReport.SystemAnalyses.Count}");
        }
        
        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// Get current ECS migration report
        /// </summary>
        public ECSMigrationReport GetMigrationReport()
        {
            return _migrationReport;
        }
        
        /// <summary>
        /// Get system analysis for specific system
        /// </summary>
        public SystemAnalysis GetSystemAnalysis(string systemName)
        {
            _systemAnalyses.TryGetValue(systemName, out SystemAnalysis analysis);
            return analysis;
        }
        
        /// <summary>
        /// Force immediate analysis update
        /// </summary>
        public void ForceAnalysisUpdate()
        {
            UpdatePerformanceMetrics();
            AnalyzeCurrentSystems();
            EvaluateECSOpportunities();
            UpdateMigrationReport();
            
            Debug.Log("[ECSMigrationAnalysis] Forced analysis update completed");
        }
        
        /// <summary>
        /// Get performance improvement estimates for ECS migration
        /// </summary>
        public Dictionary<string, float> GetPerformanceEstimates()
        {
            var estimates = new Dictionary<string, float>();
            
            foreach (var analysis in _systemAnalyses.Values)
            {
                estimates[analysis.SystemName] = analysis.ECSMigrationScore * 0.5f; // Conservative estimate
            }
            
            return estimates;
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private void InitializeECSAnalysis()
        {
            _systemAnalyses.Clear();
            _performanceBaselines.Clear();
            _migrationReport = new ECSMigrationReport();
        }
        
        private void InitializeProfilers()
        {
            _performanceProfiler = new PerformanceProfiler();
            _memoryProfiler = new MemoryProfiler();
        }
        
        private void EstablishPerformanceBaselines()
        {
            // Record initial baseline
            RecordPerformanceBaseline();
        }
        
        // Analysis helper methods (placeholder implementations)
        private int CountPlantComponents() => UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Length;
        private int CountGrowthComponents() => 50; // Placeholder
        private int CountEnvironmentalComponents() => 30; // Placeholder
        private int CountGeneticComponents() => 25; // Placeholder
        private int CountRenderingComponents() => 100; // Placeholder
        private int CountTotalPlants() => UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length;
        
        private float CalculateUpdateFrequency(string systemType) => _updateFrequencyHz;
        private float CalculateMemoryUsage(string systemType) => UnityEngine.Random.Range(10f, 50f);
        private float CalculateCPUUsage(string systemType) => UnityEngine.Random.Range(5f, 25f);
        private float CalculateECSMigrationScore(string systemType) => UnityEngine.Random.Range(6f, 9f);
        private float GetCurrentCPUUsage() => UnityEngine.Random.Range(20f, 60f);
        private float CalculateSystemLoad() => _currentPlantCount / (float)_maxPlantCount;
        
        private bool ShouldRecordBaseline()
        {
            if (_performanceBaselines.Count == 0) return true;
            var lastBaseline = _performanceBaselines[_performanceBaselines.Count - 1];
            return Mathf.Abs(lastBaseline.PlantCount - _currentPlantCount) > 50 ||
                   Mathf.Abs(lastBaseline.FrameRate - _currentFrameRate) > 5f;
        }
        
        private MigrationApproach DetermineMigrationApproach(SystemAnalysis analysis)
        {
            if (analysis.ECSMigrationScore >= 8.0f) return MigrationApproach.FullMigration;
            if (analysis.ECSMigrationScore >= 6.0f) return MigrationApproach.HybridApproach;
            return MigrationApproach.KeepCurrent;
        }
        
        private float CalculateOverallMigrationScore()
        {
            if (_systemAnalyses.Count == 0) return 0f;
            float totalScore = 0f;
            foreach (var analysis in _systemAnalyses.Values)
            {
                totalScore += analysis.ECSMigrationScore;
            }
            return totalScore / _systemAnalyses.Count;
        }
        
        private MigrationPriority DetermineRecommendedPriority()
        {
            float overallScore = CalculateOverallMigrationScore();
            if (overallScore >= 8.0f) return MigrationPriority.High;
            if (overallScore >= 6.0f) return MigrationPriority.Medium;
            return MigrationPriority.Low;
        }
        
        private void UpdateMigrationRecommendations() { }
        private List<string> GenerateFinalRecommendations() => new List<string>();
        private List<string> GenerateImplementationRoadmap() => new List<string>();
        private List<string> GenerateRiskAssessment() => new List<string>();
        private void SaveAnalysisResults() { }
        
        #endregion
    }
    
    // Supporting data structures for ECS migration analysis
    [Serializable]
    public class SystemAnalysis
    {
        public string SystemName;
        public string CurrentArchitecture;
        public int ComponentCount;
        public float UpdateFrequency;
        public float MemoryUsage; // MB
        public float CPUUsage; // Percentage
        public float ECSMigrationScore; // 0-10 scale
        public MigrationApproach RecommendedApproach;
        public List<string> MigrationBenefits = new List<string>();
        public List<string> MigrationChallenges = new List<string>();
        public Dictionary<string, float> PerformanceMetrics = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class ECSMigrationReport
    {
        public DateTime AnalysisTimestamp;
        public List<SystemAnalysis> SystemAnalyses = new List<SystemAnalysis>();
        public List<PerformanceBaseline> PerformanceBaselines = new List<PerformanceBaseline>();
        public List<ECSOpportunity> ECSOpportunities = new List<ECSOpportunity>();
        public float OverallMigrationScore;
        public MigrationPriority RecommendedPriority;
        public List<string> FinalRecommendations = new List<string>();
        public List<string> ImplementationRoadmap = new List<string>();
        public List<string> RiskAssessment = new List<string>();
        public Dictionary<string, object> AdditionalData = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class PerformanceBaseline
    {
        public DateTime Timestamp;
        public int PlantCount;
        public float FrameRate;
        public float MemoryUsage; // MB
        public float CPUUsage; // Percentage
        public float SystemLoad; // 0-1 scale
        public Dictionary<string, float> DetailedMetrics = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class ECSOpportunity
    {
        public string SystemName;
        public string Description;
        public float ExpectedPerformanceGain; // Multiplier
        public ComplexityLevel ImplementationComplexity;
        public List<string> RequiredComponents = new List<string>();
        public List<string> Dependencies = new List<string>();
        public TimeSpan EstimatedImplementationTime;
        public Dictionary<string, object> TechnicalDetails = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class PerformanceProfiler
    {
        public bool IsActive = true;
        public float ProfilingInterval = 1.0f;
        public Dictionary<string, float> SystemPerformance = new Dictionary<string, float>();
        public Queue<PerformanceSnapshot> PerformanceHistory = new Queue<PerformanceSnapshot>();
        public int MaxHistorySize = 100;
    }
    
    [Serializable]
    public class MemoryProfiler
    {
        public bool IsActive = true;
        public float ProfilingInterval = 5.0f;
        public Dictionary<string, long> MemoryUsageBySystem = new Dictionary<string, long>();
        public Queue<MemorySnapshot> MemoryHistory = new Queue<MemorySnapshot>();
        public int MaxHistorySize = 50;
    }
    
    [Serializable]
    public class PerformanceSnapshot
    {
        public DateTime Timestamp;
        public float FrameRate;
        public float CPUUsage;
        public float MemoryUsage;
        public int ActivePlants;
        public Dictionary<string, float> SystemTimes = new Dictionary<string, float>();
    }
    
    [Serializable]
    public class MemorySnapshot
    {
        public DateTime Timestamp;
        public long TotalMemory;
        public long UsedMemory;
        public long AvailableMemory;
        public Dictionary<string, long> SystemMemory = new Dictionary<string, long>();
    }
    
    // Enums for migration analysis
    public enum MigrationApproach
    {
        KeepCurrent,
        HybridApproach,
        FullMigration,
        PhaseBasedMigration
    }
    
    public enum MigrationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
}