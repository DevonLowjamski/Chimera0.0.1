using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;

namespace ProjectChimera.Systems.Performance
{
    /// <summary>
    /// Comprehensive ECS migration strategy planner for Project Chimera core systems.
    /// Part of PC016-2c: Plan migration strategy for core systems
    /// 
    /// This system creates detailed migration roadmaps, risk assessments, and implementation strategies
    /// for converting MonoBehaviour-based systems to Unity DOTS/ECS architecture.
    /// </summary>
    public class ECSMigrationStrategyPlanner : ChimeraManager
    {
        [Header("Migration Strategy Configuration")]
        [SerializeField] private bool _enableStrategyPlanning = true;
        [SerializeField] private bool _enableRiskAssessment = true;
        [SerializeField] private bool _enableTimelineGeneration = true;
        [SerializeField] private float _planningUpdateInterval = 10.0f;
        
        [Header("Migration Priorities")]
        [SerializeField] private bool _prioritizePlantSystems = true;
        [SerializeField] private bool _prioritizeGrowthSystems = true;
        [SerializeField] private bool _prioritizeEnvironmentalSystems = true;
        [SerializeField] private bool _prioritizeGeneticsSystems = true;
        [SerializeField] private bool _prioritizeRenderingSystems = true;
        
        [Header("Migration Constraints")]
        [SerializeField] private int _maxConcurrentMigrations = 2;
        [SerializeField] private float _minimumStabilityPeriod = 30.0f; // days
        [SerializeField] private float _maxAcceptableRisk = 0.7f; // 0-1 scale
        [SerializeField] private bool _requireBackwardCompatibility = true;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onStrategyGenerated;
        [SerializeField] private SimpleGameEventSO _onRiskAssessmentComplete;
        [SerializeField] private SimpleGameEventSO _onMigrationPlanReady;
        
        // Strategy planning data
        private Dictionary<string, MigrationStrategy> _migrationStrategies = new Dictionary<string, MigrationStrategy>();
        private List<MigrationPhase> _migrationPhases = new List<MigrationPhase>();
        private List<MigrationRisk> _identifiedRisks = new List<MigrationRisk>();
        private MigrationRoadmap _masterRoadmap;
        private float _lastPlanningUpdate;
        
        // System analysis data
        private Dictionary<string, SystemComplexity> _systemComplexities = new Dictionary<string, SystemComplexity>();
        private Dictionary<string, List<string>> _systemDependencies = new Dictionary<string, List<string>>();
        private Dictionary<string, float> _migrationPriorities = new Dictionary<string, float>();
        
        #region ChimeraManager Implementation
        
        protected override void OnManagerInitialize()
        {
            InitializeMigrationPlanning();
            AnalyzeSystemComplexities();
            MapSystemDependencies();
            GenerateMigrationStrategies();
            
            _lastPlanningUpdate = Time.time;
            _masterRoadmap = new MigrationRoadmap();
            
            Debug.Log($"[ECSMigrationStrategyPlanner] Migration strategy planner initialized");
            Debug.Log($"   - Strategy Planning: {_enableStrategyPlanning}");
            Debug.Log($"   - Risk Assessment: {_enableRiskAssessment}");
            Debug.Log($"   - Timeline Generation: {_enableTimelineGeneration}");
            Debug.Log($"   - Max Concurrent Migrations: {_maxConcurrentMigrations}");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!IsInitialized || !_enableStrategyPlanning) return;
            
            float deltaTime = Time.time - _lastPlanningUpdate;
            if (deltaTime >= _planningUpdateInterval)
            {
                UpdateMigrationProgress();
                ReassessRisks();
                OptimizeMigrationTimeline();
                
                _lastPlanningUpdate = Time.time;
            }
        }
        
        protected override void OnManagerShutdown()
        {
            GenerateFinalMigrationPlan();
            ExportMigrationDocumentation();
            
            _migrationStrategies.Clear();
            _migrationPhases.Clear();
            _identifiedRisks.Clear();
            
            Debug.Log("[ECSMigrationStrategyPlanner] Migration strategy planning completed");
        }
        
        #endregion
        
        #region Migration Strategy Generation
        
        /// <summary>
        /// Generate comprehensive migration strategies for all core systems
        /// </summary>
        private void GenerateMigrationStrategies()
        {
            // Plant Systems Migration Strategy
            if (_prioritizePlantSystems)
                GeneratePlantSystemMigrationStrategy();
                
            // Growth Systems Migration Strategy
            if (_prioritizeGrowthSystems)
                GenerateGrowthSystemMigrationStrategy();
                
            // Environmental Systems Migration Strategy
            if (_prioritizeEnvironmentalSystems)
                GenerateEnvironmentalSystemMigrationStrategy();
                
            // Genetics Systems Migration Strategy
            if (_prioritizeGeneticsSystems)
                GenerateGeneticsSystemMigrationStrategy();
                
            // Rendering Systems Migration Strategy
            if (_prioritizeRenderingSystems)
                GenerateRenderingSystemMigrationStrategy();
                
            Debug.Log($"[ECSMigrationStrategyPlanner] Generated {_migrationStrategies.Count} migration strategies");
        }
        
        /// <summary>
        /// Generate migration strategy for plant systems
        /// </summary>
        private void GeneratePlantSystemMigrationStrategy()
        {
            var strategy = new MigrationStrategy
            {
                SystemName = "PlantSystems",
                Priority = MigrationPriority.High,
                Approach = MigrationApproach.PhaseBasedMigration,
                EstimatedDuration = TimeSpan.FromDays(42), // 6 weeks
                RiskLevel = MigrationRiskLevel.Medium,
                RequiredPrerequisites = new List<string>
                {
                    "DOTS packages installation",
                    "ECS architecture training",
                    "Plant data structure redesign",
                    "Component authoring tools setup"
                }
            };
            
            // Define migration phases for plant systems
            strategy.MigrationPhases = new List<MigrationPhase>
            {
                new MigrationPhase
                {
                    PhaseName = "Plant Data Conversion",
                    Description = "Convert PlantInstance to ECS entities and components",
                    EstimatedDuration = TimeSpan.FromDays(14),
                    RequiredResources = new List<string> { "Senior ECS Developer", "Plant System Designer" },
                    Deliverables = new List<string> 
                    { 
                        "PlantEntity component definitions",
                        "Plant archetype system",
                        "Data conversion utilities",
                        "Unit tests for plant components"
                    },
                    RiskFactors = new List<string>
                    {
                        "Complex plant state management",
                        "ScriptableObject integration challenges",
                        "Visual representation decoupling"
                    }
                },
                new MigrationPhase
                {
                    PhaseName = "Plant Systems Conversion",
                    Description = "Convert MonoBehaviour plant systems to ECS systems",
                    EstimatedDuration = TimeSpan.FromDays(21),
                    RequiredResources = new List<string> { "ECS Systems Developer", "Performance Engineer" },
                    Deliverables = new List<string>
                    {
                        "PlantGrowthSystem (ECS)",
                        "PlantHealthSystem (ECS)",
                        "PlantLifecycleSystem (ECS)",
                        "Performance benchmarks"
                    },
                    RiskFactors = new List<string>
                    {
                        "Performance regression during migration",
                        "System interdependency conflicts",
                        "Job system complexity"
                    }
                },
                new MigrationPhase
                {
                    PhaseName = "Integration and Testing",
                    Description = "Integrate ECS plant systems with existing codebase",
                    EstimatedDuration = TimeSpan.FromDays(7),
                    RequiredResources = new List<string> { "QA Engineer", "Systems Integrator" },
                    Deliverables = new List<string>
                    {
                        "Integration tests",
                        "Performance validation",
                        "Compatibility layer",
                        "Migration documentation"
                    },
                    RiskFactors = new List<string>
                    {
                        "Integration compatibility issues",
                        "Performance bottlenecks",
                        "Testing coverage gaps"
                    }
                }
            };
            
            strategy.SuccessMetrics = new List<string>
            {
                "50% reduction in plant update CPU time",
                "Support for 10,000+ concurrent plants at 60 FPS",
                "Zero regression in plant simulation accuracy",
                "Seamless integration with existing save system"
            };
            
            strategy.RollbackPlan = new List<string>
            {
                "Maintain MonoBehaviour systems during migration",
                "Feature flag system for ECS/MonoBehaviour switching",
                "Automated fallback testing",
                "Data migration rollback procedures"
            };
            
            _migrationStrategies["PlantSystems"] = strategy;
        }
        
        /// <summary>
        /// Generate migration strategy for growth systems
        /// </summary>
        private void GenerateGrowthSystemMigrationStrategy()
        {
            var strategy = new MigrationStrategy
            {
                SystemName = "GrowthSystems",
                Priority = MigrationPriority.High,
                Approach = MigrationApproach.HybridApproach,
                EstimatedDuration = TimeSpan.FromDays(35), // 5 weeks
                RiskLevel = MigrationRiskLevel.Medium,
                RequiredPrerequisites = new List<string>
                {
                    "Plant systems ECS migration complete",
                    "Burst compilation setup",
                    "Job system architecture defined",
                    "Growth calculation profiling complete"
                }
            };
            
            strategy.MigrationPhases = new List<MigrationPhase>
            {
                new MigrationPhase
                {
                    PhaseName = "Growth Calculation Jobs",
                    Description = "Convert growth calculations to Burst-compiled jobs",
                    EstimatedDuration = TimeSpan.FromDays(21),
                    RequiredResources = new List<string> { "Senior ECS Developer", "Mathematics Specialist" },
                    Deliverables = new List<string>
                    {
                        "GrowthCalculationJob (Burst)",
                        "BiomassAccumulationJob (Burst)",
                        "StageTransitionJob (Burst)", 
                        "Performance benchmarks"
                    }
                },
                new MigrationPhase
                {
                    PhaseName = "Environmental Integration",
                    Description = "Integrate growth jobs with environmental systems",
                    EstimatedDuration = TimeSpan.FromDays(14),
                    RequiredResources = new List<string> { "Systems Integration Developer" },
                    Deliverables = new List<string>
                    {
                        "Environmental data streaming jobs",
                        "GxE interaction system",
                        "Stress response calculations"
                    }
                }
            };
            
            _migrationStrategies["GrowthSystems"] = strategy;
        }
        
        /// <summary>
        /// Generate migration strategy for environmental systems
        /// </summary>
        private void GenerateEnvironmentalSystemMigrationStrategy()
        {
            var strategy = new MigrationStrategy
            {
                SystemName = "EnvironmentalSystems",
                Priority = MigrationPriority.Medium,
                Approach = MigrationApproach.HybridApproach,
                EstimatedDuration = TimeSpan.FromDays(28), // 4 weeks
                RiskLevel = MigrationRiskLevel.Low,
                RequiredPrerequisites = new List<string>
                {
                    "Spatial partitioning system design",
                    "Environmental zone component architecture",
                    "Sensor data streaming protocols"
                }
            };
            
            strategy.SuccessMetrics = new List<string>
            {
                "Real-time environmental processing for 10,000 plants",
                "Spatial query performance under 1ms",
                "Accurate environmental gradient calculations"
            };
            
            _migrationStrategies["EnvironmentalSystems"] = strategy;
        }
        
        /// <summary>
        /// Generate migration strategy for genetics systems
        /// </summary>
        private void GenerateGeneticsSystemMigrationStrategy()
        {
            var strategy = new MigrationStrategy
            {
                SystemName = "GeneticsSystems", 
                Priority = MigrationPriority.Medium,
                Approach = MigrationApproach.FullMigration,
                EstimatedDuration = TimeSpan.FromDays(49), // 7 weeks
                RiskLevel = MigrationRiskLevel.High,
                RequiredPrerequisites = new List<string>
                {
                    "Genetic algorithm ECS architecture design",
                    "Trait expression component system",
                    "Breeding calculation job definitions"
                }
            };
            
            _migrationStrategies["GeneticsSystems"] = strategy;
        }
        
        /// <summary>
        /// Generate migration strategy for rendering systems  
        /// </summary>
        private void GenerateRenderingSystemMigrationStrategy()
        {
            var strategy = new MigrationStrategy
            {
                SystemName = "RenderingSystems",
                Priority = MigrationPriority.Low,
                Approach = MigrationApproach.HybridApproach,
                EstimatedDuration = TimeSpan.FromDays(21), // 3 weeks
                RiskLevel = MigrationRiskLevel.Low,
                RequiredPrerequisites = new List<string>
                {
                    "GPU instancing architecture",
                    "LOD system ECS integration",
                    "Rendering job definitions"
                }
            };
            
            _migrationStrategies["RenderingSystems"] = strategy;
        }
        
        #endregion
        
        #region Risk Assessment
        
        /// <summary>
        /// Perform comprehensive risk assessment for ECS migration
        /// </summary>
        private void PerformRiskAssessment()
        {
            if (!_enableRiskAssessment) return;
            
            _identifiedRisks.Clear();
            
            // Technical risks
            AddTechnicalRisks();
            
            // Timeline risks
            AddTimelineRisks();
            
            // Resource risks
            AddResourceRisks();
            
            // Integration risks
            AddIntegrationRisks();
            
            // Performance risks
            AddPerformanceRisks();
            
            Debug.Log($"[ECSMigrationStrategyPlanner] Identified {_identifiedRisks.Count} migration risks");
            
            _onRiskAssessmentComplete?.Raise();
        }
        
        /// <summary>
        /// Add technical risks to assessment
        /// </summary>
        private void AddTechnicalRisks()
        {
            _identifiedRisks.Add(new MigrationRisk
            {
                RiskName = "DOTS Package Compatibility",
                Category = MigrationRiskCategory.Technical,
                Severity = MigrationRiskSeverity.High,
                Probability = 0.6f,
                Impact = "Unity DOTS packages may have compatibility issues with current Unity version",
                MitigationStrategy = "Thorough compatibility testing, package version pinning, fallback plans",
                EstimatedImpact = TimeSpan.FromDays(7)
            });
            
            _identifiedRisks.Add(new MigrationRisk
            {
                RiskName = "Burst Compilation Issues",
                Category = MigrationRiskCategory.Technical,
                Severity = MigrationRiskSeverity.Medium,
                Probability = 0.4f,
                Impact = "Complex plant calculations may not compile with Burst",
                MitigationStrategy = "Burst-friendly code patterns, incremental migration approach",
                EstimatedImpact = TimeSpan.FromDays(3)
            });
            
            _identifiedRisks.Add(new MigrationRisk
            {
                RiskName = "ScriptableObject Integration",
                Category = MigrationRiskCategory.Technical,
                Severity = MigrationRiskSeverity.Medium,
                Probability = 0.5f,
                Impact = "Current ScriptableObject data architecture may not integrate well with ECS",
                MitigationStrategy = "Hybrid data approach, SO to blob asset conversion utilities",
                EstimatedImpact = TimeSpan.FromDays(10)
            });
        }
        
        /// <summary>
        /// Add timeline risks to assessment
        /// </summary>  
        private void AddTimelineRisks()
        {
            _identifiedRisks.Add(new MigrationRisk
            {
                RiskName = "Learning Curve Underestimation",
                Category = MigrationRiskCategory.Timeline,
                Severity = MigrationRiskSeverity.High,
                Probability = 0.7f,
                Impact = "Team may need more time to learn ECS patterns effectively",
                MitigationStrategy = "Dedicated training phase, ECS mentorship, gradual complexity increase",
                EstimatedImpact = TimeSpan.FromDays(14)
            });
            
            _identifiedRisks.Add(new MigrationRisk
            {
                RiskName = "Debugging Complexity",
                Category = MigrationRiskCategory.Timeline,
                Severity = MigrationRiskSeverity.Medium,
                Probability = 0.6f,
                Impact = "ECS debugging is more complex than MonoBehaviour debugging",
                MitigationStrategy = "ECS debugging tools setup, debugging methodology training",
                EstimatedImpact = TimeSpan.FromDays(5)
            });
        }
        
        /// <summary>
        /// Add resource risks to assessment
        /// </summary>
        private void AddResourceRisks()
        {
            _identifiedRisks.Add(new MigrationRisk
            {
                RiskName = "ECS Expertise Shortage",
                Category = MigrationRiskCategory.Resource,
                Severity = MigrationRiskSeverity.High,
                Probability = 0.8f,
                Impact = "Limited team members with deep ECS/DOTS experience",
                MitigationStrategy = "External consultant engagement, intensive training program",
                EstimatedImpact = TimeSpan.FromDays(21)
            });
        }
        
        /// <summary>
        /// Add integration risks to assessment
        /// </summary>
        private void AddIntegrationRisks()
        {
            _identifiedRisks.Add(new MigrationRisk
            {
                RiskName = "Save System Compatibility",
                Category = MigrationRiskCategory.Integration,
                Severity = MigrationRiskSeverity.High,
                Probability = 0.5f,
                Impact = "ECS entities may not serialize properly with existing save system",
                MitigationStrategy = "ECS serialization research, save system adaptation layer",
                EstimatedImpact = TimeSpan.FromDays(14)
            });
        }
        
        /// <summary>
        /// Add performance risks to assessment
        /// </summary>
        private void AddPerformanceRisks()
        {
            _identifiedRisks.Add(new MigrationRisk
            {
                RiskName = "Performance Regression",
                Category = MigrationRiskCategory.Performance,
                Severity = MigrationRiskSeverity.Medium,
                Probability = 0.4f,
                Impact = "Initial ECS implementation may perform worse than MonoBehaviour",
                MitigationStrategy = "Performance benchmarking, profiling-driven optimization",
                EstimatedImpact = TimeSpan.FromDays(7)
            });
        }
        
        #endregion
        
        #region Migration Timeline Generation
        
        /// <summary>
        /// Generate comprehensive migration timeline
        /// </summary>
        private void GenerateMigrationTimeline()
        {
            if (!_enableTimelineGeneration) return;
            
            _migrationPhases.Clear();
            
            // Sort strategies by priority and dependencies
            var sortedStrategies = _migrationStrategies.Values
                .OrderByDescending(s => (int)s.Priority)
                .ThenBy(s => s.EstimatedDuration)
                .ToList();
                
            DateTime currentDate = DateTime.Now;
            
            foreach (var strategy in sortedStrategies)
            {
                foreach (var phase in strategy.MigrationPhases)
                {
                    phase.PlannedStartDate = currentDate;
                    phase.PlannedEndDate = currentDate.Add(phase.EstimatedDuration);
                    currentDate = phase.PlannedEndDate.AddDays(_minimumStabilityPeriod);
                    
                    _migrationPhases.Add(phase);
                }
            }
            
            Debug.Log($"[ECSMigrationStrategyPlanner] Generated migration timeline with {_migrationPhases.Count} phases");
            Debug.Log($"   - Total Migration Duration: {(currentDate - DateTime.Now).TotalDays:F0} days");
        }
        
        #endregion
        
        #region System Analysis
        
        /// <summary>
        /// Analyze complexity of systems for migration planning
        /// </summary>
        private void AnalyzeSystemComplexities()
        {
            // Analyze plant systems complexity
            _systemComplexities["PlantSystems"] = new SystemComplexity
            {
                SystemName = "PlantSystems",
                ComponentCount = 150,
                StateComplexity = ComplexityLevel.High,
                DataDependencies = 25,
                IntegrationPoints = 40,
                OverallComplexity = ComplexityLevel.High
            };
            
            // Analyze growth systems complexity
            _systemComplexities["GrowthSystems"] = new SystemComplexity
            {
                SystemName = "GrowthSystems", 
                ComponentCount = 80,
                StateComplexity = ComplexityLevel.Medium,
                DataDependencies = 15,
                IntegrationPoints = 20,
                OverallComplexity = ComplexityLevel.Medium
            };
            
            // Additional system complexity analysis...
            Debug.Log($"[ECSMigrationStrategyPlanner] Analyzed complexity for {_systemComplexities.Count} systems");
        }
        
        /// <summary>
        /// Map dependencies between systems
        /// </summary>
        private void MapSystemDependencies()
        {
            _systemDependencies["PlantSystems"] = new List<string>
            {
                "GrowthSystems", "EnvironmentalSystems", "GeneticsSystems"
            };
            
            _systemDependencies["GrowthSystems"] = new List<string>
            {
                "EnvironmentalSystems", "GeneticsSystems"  
            };
            
            _systemDependencies["EnvironmentalSystems"] = new List<string>();
            _systemDependencies["GeneticsSystems"] = new List<string>();
            _systemDependencies["RenderingSystems"] = new List<string> { "PlantSystems" };
            
            Debug.Log($"[ECSMigrationStrategyPlanner] Mapped dependencies for {_systemDependencies.Count} systems");
        }
        
        #endregion
        
        #region Public API Methods
        
        /// <summary>
        /// Get complete migration roadmap
        /// </summary>
        public MigrationRoadmap GetMigrationRoadmap()
        {
            GenerateMigrationTimeline();
            PerformRiskAssessment();
            
            _masterRoadmap.MigrationStrategies = _migrationStrategies.Values.ToList();
            _masterRoadmap.MigrationPhases = _migrationPhases;
            _masterRoadmap.IdentifiedRisks = _identifiedRisks;
            _masterRoadmap.TotalEstimatedDuration = CalculateTotalDuration();
            _masterRoadmap.OverallRiskLevel = CalculateOverallRisk();
            _masterRoadmap.GenerationTimestamp = DateTime.Now;
            
            return _masterRoadmap;
        }
        
        /// <summary>
        /// Get migration strategy for specific system
        /// </summary>
        public MigrationStrategy GetSystemMigrationStrategy(string systemName)
        {
            _migrationStrategies.TryGetValue(systemName, out MigrationStrategy strategy);
            return strategy;
        }
        
        /// <summary>
        /// Force regeneration of all migration strategies
        /// </summary>
        public void RegenerateMigrationStrategies()
        {
            _migrationStrategies.Clear();
            _migrationPhases.Clear();
            _identifiedRisks.Clear();
            
            GenerateMigrationStrategies();
            GenerateMigrationTimeline();
            PerformRiskAssessment();
            
            _onMigrationPlanReady?.Raise();
            
            Debug.Log("[ECSMigrationStrategyPlanner] Regenerated all migration strategies");
        }
        
        #endregion
        
        #region Private Helper Methods
        
        private void InitializeMigrationPlanning()
        {
            _migrationStrategies.Clear();
            _migrationPhases.Clear();
            _identifiedRisks.Clear();
            _systemComplexities.Clear();
            _systemDependencies.Clear();
            _migrationPriorities.Clear();
        }
        
        private void UpdateMigrationProgress() { }
        private void ReassessRisks() { }
        private void OptimizeMigrationTimeline() { }
        private void GenerateFinalMigrationPlan() { }
        private void ExportMigrationDocumentation() { }
        
        private TimeSpan CalculateTotalDuration()
        {
            return _migrationPhases.Count > 0 
                ? _migrationPhases.Max(p => p.PlannedEndDate) - DateTime.Now
                : TimeSpan.Zero;
        }
        
        private MigrationRiskLevel CalculateOverallRisk()
        {
            if (_identifiedRisks.Count == 0) return MigrationRiskLevel.Low;
            
            var highRiskCount = _identifiedRisks.Count(r => r.Severity == MigrationRiskSeverity.High);
            if (highRiskCount > 2) return MigrationRiskLevel.High;
            
            var mediumRiskCount = _identifiedRisks.Count(r => r.Severity == MigrationRiskSeverity.Medium);
            if (mediumRiskCount > 3) return MigrationRiskLevel.Medium;
            
            return MigrationRiskLevel.Low;
        }
        
        #endregion
    }
    
    // Supporting data structures for migration strategy planning
    [Serializable]
    public class MigrationStrategy
    {
        public string SystemName;
        public MigrationPriority Priority;
        public MigrationApproach Approach;
        public TimeSpan EstimatedDuration;
        public MigrationRiskLevel RiskLevel;
        public List<string> RequiredPrerequisites = new List<string>();
        public List<MigrationPhase> MigrationPhases = new List<MigrationPhase>();
        public List<string> SuccessMetrics = new List<string>();
        public List<string> RollbackPlan = new List<string>();
        public Dictionary<string, object> AdditionalData = new Dictionary<string, object>();
    }
    
    [Serializable] 
    public class MigrationPhase
    {
        public string PhaseName;
        public string Description;
        public TimeSpan EstimatedDuration;
        public DateTime PlannedStartDate;
        public DateTime PlannedEndDate;
        public List<string> RequiredResources = new List<string>();
        public List<string> Deliverables = new List<string>();
        public List<string> RiskFactors = new List<string>();
        public MigrationPhaseStatus Status = MigrationPhaseStatus.Planned;
        public float CompletionPercentage = 0f;
    }
    
    [Serializable]
    public class MigrationRisk
    {
        public string RiskName;
        public MigrationRiskCategory Category;
        public MigrationRiskSeverity Severity;
        public float Probability; // 0-1 scale
        public string Impact;
        public string MitigationStrategy;
        public TimeSpan EstimatedImpact;
        public MigrationRiskStatus Status = MigrationRiskStatus.Identified;
    }
    
    [Serializable]
    public class MigrationRoadmap
    {
        public DateTime GenerationTimestamp;
        public List<MigrationStrategy> MigrationStrategies = new List<MigrationStrategy>();
        public List<MigrationPhase> MigrationPhases = new List<MigrationPhase>();
        public List<MigrationRisk> IdentifiedRisks = new List<MigrationRisk>();
        public TimeSpan TotalEstimatedDuration;
        public MigrationRiskLevel OverallRiskLevel;
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class SystemComplexity
    {
        public string SystemName;
        public int ComponentCount;
        public ComplexityLevel StateComplexity;
        public int DataDependencies;
        public int IntegrationPoints;
        public ComplexityLevel OverallComplexity;
    }
    
    // Note: MigrationPriority enum is defined in ECSMigrationAnalysis.cs to avoid duplication
    
    public enum MigrationRiskLevel
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    public enum MigrationRiskCategory
    {
        Technical,
        Timeline,
        Resource,
        Integration,
        Performance,
        Business
    }
    
    public enum MigrationRiskSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    public enum MigrationRiskStatus
    {
        Identified,
        Mitigated,
        Resolved,
        Accepted
    }
    
    public enum MigrationPhaseStatus
    {
        Planned,
        InProgress,
        Completed,
        Blocked,
        Cancelled
    }
}