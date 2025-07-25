using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Genetics Analytics Service - Handles performance monitoring, diversity tracking, and breeding analytics
    /// Extracted from CannabisGeneticsEngine to provide focused genetics analytics functionality
    /// Manages comprehensive genetic analytics: population diversity, breeding success, performance tracking
    /// Implements scientific analytics: Shannon diversity index, genetic drift analysis, breeding efficiency metrics
    /// </summary>
    public class GeneticsAnalyticsService : MonoBehaviour
    {
        [Header("Analytics Configuration")]
        [SerializeField] private bool _enableAnalytics = true;
        [SerializeField] private bool _enablePerformanceTracking = true;
        [SerializeField] private bool _enableDiversityTracking = true;
        [SerializeField] private bool _enableAnalyticsLogging = false;

        [Header("Analytics Parameters")]
        [SerializeField] private float _analyticsUpdateInterval = 60f; // 1 minute
        [SerializeField] private int _maxDataPoints = 1000;
        [SerializeField] private int _trendAnalysisWindow = 100;
        [SerializeField] private float _diversityThreshold = 0.5f;

        [Header("Reporting Settings")]
        [SerializeField] private bool _enableReporting = true;
        [SerializeField] private float _reportGenerationInterval = 300f; // 5 minutes
        [SerializeField] private bool _enableTrendAnalysis = true;
        [SerializeField] private bool _enablePredictiveAnalytics = true;

        [Header("Performance Monitoring")]
        [SerializeField] private bool _enablePerformanceAlerts = true;
        [SerializeField] private float _performanceThreshold = 100f; // ms
        [SerializeField] private int _maxPerformanceSamples = 500;
        [SerializeField] private float _alertCooldown = 30f;

        // Service state
        private bool _isInitialized = false;
        private GeneticDataService _dataService;
        private ScriptableObject _analyticsConfig;

        // Analytics data storage
        private Dictionary<string, PopulationDiversityMetrics> _diversityMetrics = new Dictionary<string, PopulationDiversityMetrics>();
        private Dictionary<string, BreedingSuccessMetrics> _breedingMetrics = new Dictionary<string, BreedingSuccessMetrics>();
        private Dictionary<string, GeneticsPerformanceMetrics> _performanceMetrics = new Dictionary<string, GeneticsPerformanceMetrics>();

        // Time series data
        private Dictionary<string, List<DiversityDataPoint>> _diversityTimeSeries = new Dictionary<string, List<DiversityDataPoint>>();
        private Dictionary<string, List<PerformanceDataPoint>> _performanceTimeSeries = new Dictionary<string, List<PerformanceDataPoint>>();
        private Dictionary<string, List<BreedingDataPoint>> _breedingTimeSeries = new Dictionary<string, List<BreedingDataPoint>>();

        // Trend analysis
        private Dictionary<string, TrendAnalysis> _trendAnalysis = new Dictionary<string, TrendAnalysis>();
        private Dictionary<string, PredictiveModel> _predictiveModels = new Dictionary<string, PredictiveModel>();

        // Reporting system
        private List<AnalyticsReport> _generatedReports = new List<AnalyticsReport>();
        private Dictionary<string, AlertStatus> _alertStatus = new Dictionary<string, AlertStatus>();

        // Performance tracking
        private float _lastAnalyticsUpdate = 0f;
        private float _lastReportGeneration = 0f;
        private int _totalAnalyticsOperations = 0;
        private float _averageProcessingTime = 0f;
        private GeneticsAnalyticsData _comprehensiveAnalytics = new GeneticsAnalyticsData();

        // Events
        public static event Action<PopulationDiversityMetrics> OnDiversityMetricsUpdated;
        public static event Action<BreedingSuccessMetrics> OnBreedingMetricsUpdated;
        public static event Action<GeneticsPerformanceMetrics> OnPerformanceMetricsUpdated;
        public static event Action<AnalyticsReport> OnReportGenerated;
        public static event Action<AnalyticsAlert> OnAnalyticsAlert;
        public static event Action<TrendAnalysis> OnTrendAnalysisUpdated;
        public static event Action<string> OnAnalyticsError;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Genetics Analytics Service";
        public int TrackedPopulations => _diversityMetrics.Count;
        public int GeneratedReports => _generatedReports.Count;
        public int ActiveAlerts => _alertStatus.Values.Count(a => a.IsActive);
        public float AnalyticsProcessingTime => _averageProcessingTime;
        public int TotalOperations => _totalAnalyticsOperations;

        public void Initialize(ScriptableObject analyticsConfig = null)
        {
            InitializeService(analyticsConfig);
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
                UpdateAnalytics();
                ProcessTrendAnalysis();
                CheckReportGeneration();
                UpdatePerformanceTracking();
            }
        }

        private void InitializeDataStructures()
        {
            _diversityMetrics = new Dictionary<string, PopulationDiversityMetrics>();
            _breedingMetrics = new Dictionary<string, BreedingSuccessMetrics>();
            _performanceMetrics = new Dictionary<string, GeneticsPerformanceMetrics>();
            _diversityTimeSeries = new Dictionary<string, List<DiversityDataPoint>>();
            _performanceTimeSeries = new Dictionary<string, List<PerformanceDataPoint>>();
            _breedingTimeSeries = new Dictionary<string, List<BreedingDataPoint>>();
            _trendAnalysis = new Dictionary<string, TrendAnalysis>();
            _predictiveModels = new Dictionary<string, PredictiveModel>();
            _generatedReports = new List<AnalyticsReport>();
            _alertStatus = new Dictionary<string, AlertStatus>();
            _comprehensiveAnalytics = new GeneticsAnalyticsData();
        }

        public void InitializeService(ScriptableObject analyticsConfig = null)
        {
            if (_isInitialized)
            {
                if (_enableAnalyticsLogging)
                    Debug.LogWarning("GeneticsAnalyticsService already initialized");
                return;
            }

            try
            {
                _analyticsConfig = analyticsConfig;

                InitializeDataService();
                InitializeAnalyticsSystem();
                InitializeTrendAnalysis();
                InitializeReportingSystem();
                
                _isInitialized = true;
                _lastAnalyticsUpdate = Time.time;
                _lastReportGeneration = Time.time;
                
                if (_enableAnalyticsLogging)
                    Debug.Log("GeneticsAnalyticsService initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize GeneticsAnalyticsService: {ex.Message}");
                OnAnalyticsError?.Invoke($"Service initialization failed: {ex.Message}");
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            try
            {
                // Generate final report
                if (_enableReporting)
                {
                    GenerateComprehensiveReport();
                }
                
                // Clear all data
                ClearAllData();
                
                _isInitialized = false;
                
                if (_enableAnalyticsLogging)
                    Debug.Log("GeneticsAnalyticsService shutdown completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during GeneticsAnalyticsService shutdown: {ex.Message}");
            }
        }

        #endregion

        #region Diversity Analytics

        /// <summary>
        /// Calculate population diversity metrics
        /// </summary>
        public void AnalyzePopulationDiversity(string populationName, List<CannabisGenotype> genotypes)
        {
            if (!_isInitialized || !_enableDiversityTracking || genotypes == null || genotypes.Count == 0)
                return;

            try
            {
                var startTime = DateTime.Now;
                
                if (_enableAnalyticsLogging)
                    Debug.Log($"Analyzing population diversity: {populationName} ({genotypes.Count} genotypes)");

                var metrics = new PopulationDiversityMetrics
                {
                    PopulationName = populationName,
                    SampleSize = genotypes.Count,
                    AnalysisDate = DateTime.Now
                };

                // Calculate Shannon Diversity Index
                metrics.ShannonDiversityIndex = CalculateShannonDiversityIndex(genotypes);

                // Calculate Simpson's Diversity Index
                metrics.SimpsonDiversityIndex = CalculateSimpsonDiversityIndex(genotypes);

                // Calculate Genetic Richness
                metrics.GeneticRichness = CalculateGeneticRichness(genotypes);

                // Calculate Evenness
                metrics.Evenness = CalculateEvenness(genotypes);

                // Calculate Allelic Diversity
                metrics.AllelicDiversity = CalculateAllelicDiversity(genotypes);

                // Calculate Effective Population Size
                metrics.EffectivePopulationSize = CalculateEffectivePopulationSize(genotypes);

                // Identify unique genotypes
                metrics.UniqueGenotypes = IdentifyUniqueGenotypes(genotypes);

                // Calculate trait diversity
                metrics.TraitDiversity = CalculateTraitDiversity(genotypes);

                // Store metrics
                _diversityMetrics[populationName] = metrics;

                // Add to time series
                AddDiversityDataPoint(populationName, metrics);

                // Check for diversity alerts
                CheckDiversityAlerts(populationName, metrics);

                var processingTime = (float)(DateTime.Now - startTime).TotalMilliseconds;
                UpdatePerformanceMetrics("DiversityAnalysis", processingTime);

                OnDiversityMetricsUpdated?.Invoke(metrics);
                
                if (_enableAnalyticsLogging)
                    Debug.Log($"Population diversity analysis completed: {populationName} (Shannon: {metrics.ShannonDiversityIndex:F3}, Simpson: {metrics.SimpsonDiversityIndex:F3})");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error analyzing population diversity: {ex.Message}");
                OnAnalyticsError?.Invoke($"Diversity analysis failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate Shannon Diversity Index
        /// </summary>
        private float CalculateShannonDiversityIndex(List<CannabisGenotype> genotypes)
        {
            var traitFrequencies = CalculateTraitFrequencies(genotypes);
            float shannonIndex = 0f;

            foreach (var frequency in traitFrequencies.Values)
            {
                if (frequency > 0f)
                {
                    shannonIndex -= frequency * Mathf.Log(frequency);
                }
            }

            return shannonIndex;
        }

        /// <summary>
        /// Calculate Simpson's Diversity Index
        /// </summary>
        private float CalculateSimpsonDiversityIndex(List<CannabisGenotype> genotypes)
        {
            var traitFrequencies = CalculateTraitFrequencies(genotypes);
            float simpsonIndex = 0f;

            foreach (var frequency in traitFrequencies.Values)
            {
                simpsonIndex += frequency * frequency;
            }

            return 1f - simpsonIndex;
        }

        /// <summary>
        /// Calculate trait frequencies for diversity calculations
        /// </summary>
        private Dictionary<string, float> CalculateTraitFrequencies(List<CannabisGenotype> genotypes)
        {
            var traitCounts = new Dictionary<string, int>();
            var totalTraits = 0;

            foreach (var genotype in genotypes)
            {
                if (genotype.Traits != null)
                {
                    foreach (var trait in genotype.Traits)
                    {
                        var traitKey = $"{trait.TraitName}_{trait.ExpressedValue:F2}";
                        traitCounts[traitKey] = traitCounts.GetValueOrDefault(traitKey, 0) + 1;
                        totalTraits++;
                    }
                }
            }

            var frequencies = new Dictionary<string, float>();
            foreach (var kvp in traitCounts)
            {
                frequencies[kvp.Key] = (float)kvp.Value / totalTraits;
            }

            return frequencies;
        }

        /// <summary>
        /// Calculate genetic richness
        /// </summary>
        private float CalculateGeneticRichness(List<CannabisGenotype> genotypes)
        {
            var uniqueAlleles = new HashSet<string>();

            foreach (var genotype in genotypes)
            {
                if (genotype.Traits != null)
                {
                    foreach (var trait in genotype.Traits)
                    {
                        uniqueAlleles.Add($"{trait.TraitName}_{trait.ExpressedValue:F2}");
                    }
                }
            }

            return uniqueAlleles.Count;
        }

        /// <summary>
        /// Calculate evenness measure
        /// </summary>
        private float CalculateEvenness(List<CannabisGenotype> genotypes)
        {
            var shannonIndex = CalculateShannonDiversityIndex(genotypes);
            var richness = CalculateGeneticRichness(genotypes);
            
            if (richness <= 1) return 1f;
            
            var maxShannonIndex = Mathf.Log(richness);
            return maxShannonIndex > 0 ? shannonIndex / maxShannonIndex : 0f;
        }

        /// <summary>
        /// Calculate allelic diversity
        /// </summary>
        private float CalculateAllelicDiversity(List<CannabisGenotype> genotypes)
        {
            var alleleCounts = new Dictionary<string, Dictionary<string, int>>();

            foreach (var genotype in genotypes)
            {
                if (genotype.Traits != null)
                {
                    foreach (var trait in genotype.Traits)
                    {
                        if (!alleleCounts.ContainsKey(trait.TraitName))
                        {
                            alleleCounts[trait.TraitName] = new Dictionary<string, int>();
                        }

                        var alleleKey = trait.ExpressedValue.ToString("F2");
                        alleleCounts[trait.TraitName][alleleKey] = 
                            alleleCounts[trait.TraitName].GetValueOrDefault(alleleKey, 0) + 1;
                    }
                }
            }

            float totalDiversity = 0f;
            int traitCount = 0;

            foreach (var traitAlleles in alleleCounts.Values)
            {
                float traitDiversity = 1f - traitAlleles.Values.Sum(count => {
                    float frequency = (float)count / genotypes.Count;
                    return frequency * frequency;
                });
                totalDiversity += traitDiversity;
                traitCount++;
            }

            return traitCount > 0 ? totalDiversity / traitCount : 0f;
        }

        /// <summary>
        /// Calculate effective population size
        /// </summary>
        private float CalculateEffectivePopulationSize(List<CannabisGenotype> genotypes)
        {
            // Simplified calculation based on genetic diversity
            var diversity = CalculateShannonDiversityIndex(genotypes);
            return genotypes.Count * Mathf.Clamp01(diversity);
        }

        /// <summary>
        /// Identify unique genotypes in population
        /// </summary>
        private int IdentifyUniqueGenotypes(List<CannabisGenotype> genotypes)
        {
            var uniqueGenotypes = new HashSet<string>();

            foreach (var genotype in genotypes)
            {
                var genotypeSignature = GenerateGenotypeSignature(genotype);
                uniqueGenotypes.Add(genotypeSignature);
            }

            return uniqueGenotypes.Count;
        }

        /// <summary>
        /// Generate unique signature for genotype comparison
        /// </summary>
        private string GenerateGenotypeSignature(CannabisGenotype genotype)
        {
            if (genotype.Traits == null || genotype.Traits.Count == 0)
                return genotype.GenotypeId;

            var sortedTraits = genotype.Traits
                .OrderBy(t => t.TraitName)
                .Select(t => $"{t.TraitName}:{t.ExpressedValue:F3}")
                .ToList();

            return string.Join("|", sortedTraits);
        }

        /// <summary>
        /// Calculate trait-specific diversity
        /// </summary>
        private Dictionary<string, float> CalculateTraitDiversity(List<CannabisGenotype> genotypes)
        {
            var traitDiversity = new Dictionary<string, float>();
            var traitValues = new Dictionary<string, List<float>>();

            // Collect trait values
            foreach (var genotype in genotypes)
            {
                if (genotype.Traits != null)
                {
                    foreach (var trait in genotype.Traits)
                    {
                        if (!traitValues.ContainsKey(trait.TraitName))
                        {
                            traitValues[trait.TraitName] = new List<float>();
                        }
                        traitValues[trait.TraitName].Add(trait.ExpressedValue);
                    }
                }
            }

            // Calculate diversity for each trait
            foreach (var kvp in traitValues)
            {
                var values = kvp.Value;
                if (values.Count > 1)
                {
                    var mean = (float)values.Average();
                    var variance = (float)values.Sum(v => (v - mean) * (v - mean)) / values.Count;
                    traitDiversity[kvp.Key] = Mathf.Sqrt(variance);
                }
                else
                {
                    traitDiversity[kvp.Key] = 0f;
                }
            }

            return traitDiversity;
        }

        #endregion

        #region Breeding Analytics

        /// <summary>
        /// Analyze breeding success metrics
        /// </summary>
        public void AnalyzeBreedingSuccess(string breedingProgramName, List<BreedingRecord> breedingRecords)
        {
            if (!_isInitialized || breedingRecords == null || breedingRecords.Count == 0)
                return;

            try
            {
                var startTime = DateTime.Now;
                
                if (_enableAnalyticsLogging)
                    Debug.Log($"Analyzing breeding success: {breedingProgramName} ({breedingRecords.Count} records)");

                var metrics = new BreedingSuccessMetrics
                {
                    ProgramName = breedingProgramName,
                    TotalBreedingAttempts = breedingRecords.Count,
                    AnalysisDate = DateTime.Now
                };

                // Calculate success rates
                metrics.SuccessRate = CalculateBreedingSuccessRate(breedingRecords);

                // Calculate generation progression
                metrics.GenerationProgression = CalculateGenerationProgression(breedingRecords);

                // Calculate trait improvement
                metrics.TraitImprovement = CalculateTraitImprovement(breedingRecords);

                // Calculate breeding efficiency
                metrics.BreedingEfficiency = CalculateBreedingEfficiency(breedingRecords);

                // Calculate genetic gain
                metrics.GeneticGain = CalculateGeneticGain(breedingRecords);

                // Calculate inbreeding coefficient
                metrics.InbreedingCoefficient = CalculateInbreedingCoefficient(breedingRecords);

                // Store metrics
                _breedingMetrics[breedingProgramName] = metrics;

                // Add to time series
                AddBreedingDataPoint(breedingProgramName, metrics);

                var processingTime = (float)(DateTime.Now - startTime).TotalMilliseconds;
                UpdatePerformanceMetrics("BreedingAnalysis", processingTime);

                OnBreedingMetricsUpdated?.Invoke(metrics);
                
                if (_enableAnalyticsLogging)
                    Debug.Log($"Breeding success analysis completed: {breedingProgramName} (Success Rate: {metrics.SuccessRate:F2}%)");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error analyzing breeding success: {ex.Message}");
                OnAnalyticsError?.Invoke($"Breeding analysis failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate breeding success rate
        /// </summary>
        private float CalculateBreedingSuccessRate(List<BreedingRecord> records)
        {
            if (records.Count == 0) return 0f;

            int successfulBreedings = records.Count(r => r.OffspringIds != null && r.OffspringIds.Count > 0);
            return (float)successfulBreedings / records.Count * 100f;
        }

        /// <summary>
        /// Calculate generation progression metrics
        /// </summary>
        private Dictionary<string, float> CalculateGenerationProgression(List<BreedingRecord> records)
        {
            var progression = new Dictionary<string, float>();
            var dateSpan = new Dictionary<DateTime, int>();

            foreach (var record in records)
            {
                var date = record.BreedingDate.Date;
                dateSpan[date] = dateSpan.GetValueOrDefault(date, 0) + 1;
            }

            if (dateSpan.Count > 0)
            {
                var dates = dateSpan.Keys.OrderBy(d => d).ToList();
                progression["FirstBreeding"] = (float)(dates.First() - DateTime.MinValue).TotalDays;
                progression["LastBreeding"] = (float)(dates.Last() - DateTime.MinValue).TotalDays;
                progression["BreedingTimespan"] = (float)(dates.Last() - dates.First()).TotalDays;
                progression["BreedingFrequency"] = records.Count / Math.Max(1f, (float)(dates.Last() - dates.First()).TotalDays / 30f); // per month
            }

            return progression;
        }

        /// <summary>
        /// Calculate trait improvement over generations
        /// </summary>
        private Dictionary<string, float> CalculateTraitImprovement(List<BreedingRecord> records)
        {
            var improvement = new Dictionary<string, float>();
            
            // This would require genotype data to calculate actual trait improvements
            // For now, provide a simplified metric based on breeding success
            improvement["OverallImprovement"] = CalculateBreedingSuccessRate(records) / 100f;
            
            return improvement;
        }

        /// <summary>
        /// Calculate breeding efficiency metrics
        /// </summary>
        private float CalculateBreedingEfficiency(List<BreedingRecord> records)
        {
            if (records.Count == 0) return 0f;

            // Calculate average offspring per breeding attempt
            float totalOffspring = (float)records.Sum(r => r.OffspringIds?.Count ?? 0);
            return totalOffspring / records.Count;
        }

        /// <summary>
        /// Calculate genetic gain over time
        /// </summary>
        private float CalculateGeneticGain(List<BreedingRecord> records)
        {
            // Simplified genetic gain calculation
            var chronologicalRecords = records.OrderBy(r => r.BreedingDate).ToList();
            
            if (chronologicalRecords.Count < 2) return 0f;

            // Calculate improvement trend (simplified)
            float initialEfficiency = (float)chronologicalRecords.Take(chronologicalRecords.Count / 2)
                .Average(r => r.OffspringIds?.Count ?? 0);
            float laterEfficiency = (float)chronologicalRecords.Skip(chronologicalRecords.Count / 2)
                .Average(r => r.OffspringIds?.Count ?? 0);

            return laterEfficiency - initialEfficiency;
        }

        /// <summary>
        /// Calculate inbreeding coefficient
        /// </summary>
        private float CalculateInbreedingCoefficient(List<BreedingRecord> records)
        {
            // Simplified inbreeding calculation
            // In reality, this would require detailed pedigree analysis
            var parentPairs = records.Select(r => $"{r.Parent1Id}_{r.Parent2Id}").ToList();
            var uniquePairs = parentPairs.Distinct().Count();
            
            if (parentPairs.Count == 0) return 0f;
            
            return 1f - ((float)uniquePairs / parentPairs.Count);
        }

        #endregion

        #region Utility Methods

        private void InitializeDataService()
        {
            try
            {
                _dataService = FindObjectOfType<GeneticDataService>();
                
                if (_dataService == null && _enableAnalyticsLogging)
                {
                    Debug.LogWarning("GeneticDataService not found - some analytics features may be limited");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize data service: {ex.Message}");
            }
        }

        private void InitializeAnalyticsSystem()
        {
            // Initialize analytics tracking systems
            InitializePerformanceTracking();
        }

        private void InitializeTrendAnalysis()
        {
            // Initialize trend analysis algorithms
            if (_enableTrendAnalysis)
            {
                InitializePredictiveModels();
            }
        }

        private void InitializeReportingSystem()
        {
            // Initialize automated reporting systems
            if (_enableReporting)
            {
                InitializeReportTemplates();
            }
        }

        private void InitializePerformanceTracking()
        {
            _performanceMetrics["DiversityAnalysis"] = new GeneticsPerformanceMetrics { OperationType = "DiversityAnalysis" };
            _performanceMetrics["BreedingAnalysis"] = new GeneticsPerformanceMetrics { OperationType = "BreedingAnalysis" };
            _performanceMetrics["TrendAnalysis"] = new GeneticsPerformanceMetrics { OperationType = "TrendAnalysis" };
            _performanceMetrics["ReportGeneration"] = new GeneticsPerformanceMetrics { OperationType = "ReportGeneration" };
        }

        private void InitializePredictiveModels()
        {
            // Initialize machine learning models for predictive analytics
        }

        private void InitializeReportTemplates()
        {
            // Initialize report templates and formatting
        }

        private void UpdateAnalytics()
        {
            if (!_enableAnalytics || Time.time - _lastAnalyticsUpdate < _analyticsUpdateInterval)
                return;

            try
            {
                // Update comprehensive analytics data
                UpdateComprehensiveAnalytics();
                
                _lastAnalyticsUpdate = Time.time;
                _totalAnalyticsOperations++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error updating analytics: {ex.Message}");
                OnAnalyticsError?.Invoke($"Analytics update failed: {ex.Message}");
            }
        }

        private void UpdateComprehensiveAnalytics()
        {
            _comprehensiveAnalytics.TotalPopulations = _diversityMetrics.Count;
            _comprehensiveAnalytics.TotalBreedingPrograms = _breedingMetrics.Count;
            _comprehensiveAnalytics.TotalReports = _generatedReports.Count;
            _comprehensiveAnalytics.ActiveAlerts = ActiveAlerts;
            _comprehensiveAnalytics.AverageProcessingTime = _averageProcessingTime;
            _comprehensiveAnalytics.LastUpdate = DateTime.Now;

            // Calculate overall diversity score
            if (_diversityMetrics.Count > 0)
            {
                _comprehensiveAnalytics.OverallDiversityScore = 
                    (float)_diversityMetrics.Values.Average(d => d.ShannonDiversityIndex);
            }

            // Calculate overall breeding success
            if (_breedingMetrics.Count > 0)
            {
                _comprehensiveAnalytics.OverallBreedingSuccess = 
                    (float)_breedingMetrics.Values.Average(b => b.SuccessRate);
            }
        }

        private void ProcessTrendAnalysis()
        {
            if (!_enableTrendAnalysis) return;

            try
            {
                // Process trend analysis for diversity metrics
                ProcessDiversityTrends();

                // Process trend analysis for breeding metrics
                ProcessBreedingTrends();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing trend analysis: {ex.Message}");
            }
        }

        private void ProcessDiversityTrends()
        {
            foreach (var kvp in _diversityTimeSeries)
            {
                var populationName = kvp.Key;
                var dataPoints = kvp.Value;

                if (dataPoints.Count >= _trendAnalysisWindow)
                {
                    var trendAnalysis = CalculateTrend(dataPoints);
                    _trendAnalysis[$"diversity_{populationName}"] = trendAnalysis;
                    OnTrendAnalysisUpdated?.Invoke(trendAnalysis);
                }
            }
        }

        private void ProcessBreedingTrends()
        {
            foreach (var kvp in _breedingTimeSeries)
            {
                var programName = kvp.Key;
                var dataPoints = kvp.Value;

                if (dataPoints.Count >= _trendAnalysisWindow)
                {
                    var trendAnalysis = CalculateBreedingTrend(dataPoints);
                    _trendAnalysis[$"breeding_{programName}"] = trendAnalysis;
                    OnTrendAnalysisUpdated?.Invoke(trendAnalysis);
                }
            }
        }

        private TrendAnalysis CalculateTrend(List<DiversityDataPoint> dataPoints)
        {
            var recentPoints = dataPoints.TakeLast(_trendAnalysisWindow).ToList();
            
            var trend = new TrendAnalysis
            {
                TrendType = "Diversity",
                DataPoints = recentPoints.Count,
                AnalysisDate = DateTime.Now
            };

            // Calculate trend direction
            if (recentPoints.Count >= 2)
            {
                var firstHalf = (float)recentPoints.Take(recentPoints.Count / 2).Average(p => p.ShannonIndex);
                var secondHalf = (float)recentPoints.Skip(recentPoints.Count / 2).Average(p => p.ShannonIndex);
                
                trend.TrendDirection = secondHalf > firstHalf ? "Increasing" : 
                                     secondHalf < firstHalf ? "Decreasing" : "Stable";
                trend.TrendStrength = Math.Abs(secondHalf - firstHalf);
            }

            return trend;
        }

        private TrendAnalysis CalculateBreedingTrend(List<BreedingDataPoint> dataPoints)
        {
            var recentPoints = dataPoints.TakeLast(_trendAnalysisWindow).ToList();
            
            var trend = new TrendAnalysis
            {
                TrendType = "Breeding",
                DataPoints = recentPoints.Count,
                AnalysisDate = DateTime.Now
            };

            // Calculate trend direction
            if (recentPoints.Count >= 2)
            {
                var firstHalf = (float)recentPoints.Take(recentPoints.Count / 2).Average(p => p.SuccessRate);
                var secondHalf = (float)recentPoints.Skip(recentPoints.Count / 2).Average(p => p.SuccessRate);
                
                trend.TrendDirection = secondHalf > firstHalf ? "Improving" : 
                                     secondHalf < firstHalf ? "Declining" : "Stable";
                trend.TrendStrength = Math.Abs(secondHalf - firstHalf);
            }

            return trend;
        }

        private void CheckReportGeneration()
        {
            if (!_enableReporting || Time.time - _lastReportGeneration < _reportGenerationInterval)
                return;

            try
            {
                GeneratePeriodicReport();
                _lastReportGeneration = Time.time;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error generating report: {ex.Message}");
            }
        }

        private void GeneratePeriodicReport()
        {
            var report = new AnalyticsReport
            {
                ReportId = Guid.NewGuid().ToString(),
                ReportType = "Periodic Analytics",
                GenerationDate = DateTime.Now,
                Summary = GenerateReportSummary()
            };

            _generatedReports.Add(report);

            // Limit report history
            if (_generatedReports.Count > 100)
            {
                _generatedReports.RemoveAt(0);
            }

            OnReportGenerated?.Invoke(report);
        }

        private void GenerateComprehensiveReport()
        {
            var report = new AnalyticsReport
            {
                ReportId = Guid.NewGuid().ToString(),
                ReportType = "Comprehensive Final Report",
                GenerationDate = DateTime.Now,
                Summary = GenerateComprehensiveReportSummary()
            };

            _generatedReports.Add(report);
            OnReportGenerated?.Invoke(report);

            if (_enableAnalyticsLogging)
                Debug.Log("Comprehensive analytics report generated");
        }

        private string GenerateReportSummary()
        {
            return $"Analytics Summary - Populations: {_diversityMetrics.Count}, " +
                   $"Breeding Programs: {_breedingMetrics.Count}, " +
                   $"Active Alerts: {ActiveAlerts}";
        }

        private string GenerateComprehensiveReportSummary()
        {
            return $"Comprehensive Analytics Report - " +
                   $"Total Operations: {_totalAnalyticsOperations}, " +
                   $"Average Processing Time: {_averageProcessingTime:F2}ms, " +
                   $"Overall Diversity Score: {_comprehensiveAnalytics.OverallDiversityScore:F3}, " +
                   $"Overall Breeding Success: {_comprehensiveAnalytics.OverallBreedingSuccess:F2}%";
        }

        private void UpdatePerformanceTracking()
        {
            if (!_enablePerformanceTracking) return;

            // Update performance metrics for each operation type
            foreach (var metric in _performanceMetrics.Values)
            {
                metric.LastUpdate = DateTime.Now;
                
                // Check for performance alerts
                if (_enablePerformanceAlerts && metric.AverageExecutionTime > _performanceThreshold)
                {
                    CheckPerformanceAlert(metric);
                }
            }
        }

        private void UpdatePerformanceMetrics(string operationType, float executionTime)
        {
            if (_performanceMetrics.TryGetValue(operationType, out var metrics))
            {
                metrics.TotalExecutions++;
                metrics.TotalExecutionTime += executionTime;
                metrics.AverageExecutionTime = metrics.TotalExecutionTime / metrics.TotalExecutions;
                metrics.LastExecutionTime = executionTime;
                metrics.LastUpdate = DateTime.Now;

                // Update overall average
                _averageProcessingTime = (_averageProcessingTime + executionTime) / 2f;

                OnPerformanceMetricsUpdated?.Invoke(metrics);
            }
        }

        private void AddDiversityDataPoint(string populationName, PopulationDiversityMetrics metrics)
        {
            if (!_diversityTimeSeries.ContainsKey(populationName))
            {
                _diversityTimeSeries[populationName] = new List<DiversityDataPoint>();
            }

            var dataPoint = new DiversityDataPoint
            {
                Timestamp = DateTime.Now,
                ShannonIndex = metrics.ShannonDiversityIndex,
                SimpsonIndex = metrics.SimpsonDiversityIndex,
                GeneticRichness = metrics.GeneticRichness,
                Evenness = metrics.Evenness
            };

            _diversityTimeSeries[populationName].Add(dataPoint);

            // Limit data points
            if (_diversityTimeSeries[populationName].Count > _maxDataPoints)
            {
                _diversityTimeSeries[populationName].RemoveAt(0);
            }
        }

        private void AddBreedingDataPoint(string programName, BreedingSuccessMetrics metrics)
        {
            if (!_breedingTimeSeries.ContainsKey(programName))
            {
                _breedingTimeSeries[programName] = new List<BreedingDataPoint>();
            }

            var dataPoint = new BreedingDataPoint
            {
                Timestamp = DateTime.Now,
                SuccessRate = metrics.SuccessRate,
                BreedingEfficiency = metrics.BreedingEfficiency,
                GeneticGain = metrics.GeneticGain,
                InbreedingCoefficient = metrics.InbreedingCoefficient
            };

            _breedingTimeSeries[programName].Add(dataPoint);

            // Limit data points
            if (_breedingTimeSeries[programName].Count > _maxDataPoints)
            {
                _breedingTimeSeries[programName].RemoveAt(0);
            }
        }

        private void CheckDiversityAlerts(string populationName, PopulationDiversityMetrics metrics)
        {
            // Check if diversity falls below threshold
            if (metrics.ShannonDiversityIndex < _diversityThreshold)
            {
                var alert = new AnalyticsAlert
                {
                    AlertId = Guid.NewGuid().ToString(),
                    AlertType = "Low Diversity",
                    PopulationName = populationName,
                    AlertMessage = $"Population {populationName} diversity ({metrics.ShannonDiversityIndex:F3}) below threshold ({_diversityThreshold})",
                    Severity = AlertSeverity.Warning,
                    Timestamp = DateTime.Now
                };

                TriggerAlert(alert);
            }
        }

        private void CheckPerformanceAlert(GeneticsPerformanceMetrics metrics)
        {
            var alertKey = $"performance_{metrics.OperationType}";
            
            // Check cooldown
            if (_alertStatus.TryGetValue(alertKey, out var status) && 
                (DateTime.Now - status.LastAlert).TotalSeconds < _alertCooldown)
            {
                return;
            }

            var alert = new AnalyticsAlert
            {
                AlertId = Guid.NewGuid().ToString(),
                AlertType = "Performance",
                AlertMessage = $"Operation {metrics.OperationType} exceeding performance threshold: {metrics.AverageExecutionTime:F2}ms",
                Severity = AlertSeverity.Warning,
                Timestamp = DateTime.Now
            };

            TriggerAlert(alert);

            // Update alert status
            _alertStatus[alertKey] = new AlertStatus
            {
                IsActive = true,
                LastAlert = DateTime.Now,
                AlertCount = (status?.AlertCount ?? 0) + 1
            };
        }

        private void TriggerAlert(AnalyticsAlert alert)
        {
            OnAnalyticsAlert?.Invoke(alert);
            
            if (_enableAnalyticsLogging)
                Debug.LogWarning($"Analytics Alert: {alert.AlertMessage}");
        }

        private void ClearAllData()
        {
            _diversityMetrics.Clear();
            _breedingMetrics.Clear();
            _performanceMetrics.Clear();
            _diversityTimeSeries.Clear();
            _performanceTimeSeries.Clear();
            _breedingTimeSeries.Clear();
            _trendAnalysis.Clear();
            _predictiveModels.Clear();
            _generatedReports.Clear();
            _alertStatus.Clear();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get population diversity metrics
        /// </summary>
        public PopulationDiversityMetrics GetDiversityMetrics(string populationName)
        {
            return _diversityMetrics.TryGetValue(populationName, out var metrics) ? metrics : null;
        }

        /// <summary>
        /// Get breeding success metrics
        /// </summary>
        public BreedingSuccessMetrics GetBreedingMetrics(string programName)
        {
            return _breedingMetrics.TryGetValue(programName, out var metrics) ? metrics : null;
        }

        /// <summary>
        /// Get performance metrics for operation type
        /// </summary>
        public GeneticsPerformanceMetrics GetPerformanceMetrics(string operationType)
        {
            return _performanceMetrics.TryGetValue(operationType, out var metrics) ? metrics : null;
        }

        /// <summary>
        /// Get trend analysis
        /// </summary>
        public TrendAnalysis GetTrendAnalysis(string analysisKey)
        {
            return _trendAnalysis.TryGetValue(analysisKey, out var trend) ? trend : null;
        }

        /// <summary>
        /// Get comprehensive analytics data
        /// </summary>
        public GeneticsAnalyticsData GetComprehensiveAnalytics()
        {
            return _comprehensiveAnalytics;
        }

        /// <summary>
        /// Get all generated reports
        /// </summary>
        public List<AnalyticsReport> GetAllReports()
        {
            return new List<AnalyticsReport>(_generatedReports);
        }

        /// <summary>
        /// Generate on-demand report
        /// </summary>
        public void GenerateOnDemandReport(string reportType)
        {
            try
            {
                var report = new AnalyticsReport
                {
                    ReportId = Guid.NewGuid().ToString(),
                    ReportType = $"On-Demand {reportType}",
                    GenerationDate = DateTime.Now,
                    Summary = GenerateCustomReportSummary(reportType)
                };

                _generatedReports.Add(report);
                OnReportGenerated?.Invoke(report);

                if (_enableAnalyticsLogging)
                    Debug.Log($"On-demand report generated: {reportType}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error generating on-demand report: {ex.Message}");
                OnAnalyticsError?.Invoke($"Report generation failed: {ex.Message}");
            }
        }

        private string GenerateCustomReportSummary(string reportType)
        {
            switch (reportType.ToLower())
            {
                case "diversity":
                    return $"Diversity Report - {_diversityMetrics.Count} populations analyzed";
                case "breeding":
                    return $"Breeding Report - {_breedingMetrics.Count} programs analyzed";
                case "performance":
                    return $"Performance Report - Average processing time: {_averageProcessingTime:F2}ms";
                default:
                    return GenerateReportSummary();
            }
        }

        /// <summary>
        /// Update analytics settings at runtime
        /// </summary>
        public void UpdateAnalyticsSettings(bool enableAnalytics, bool enableDiversity, bool enableBreeding, float updateInterval)
        {
            _enableAnalytics = enableAnalytics;
            _enableDiversityTracking = enableDiversity;
            _enablePerformanceTracking = enableBreeding;
            _analyticsUpdateInterval = updateInterval;
            
            if (_enableAnalyticsLogging)
                Debug.Log($"Analytics settings updated: Analytics={enableAnalytics}, Diversity={enableDiversity}, Performance={enableBreeding}, Interval={updateInterval}s");
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        #endregion
    }

    // Data structures for analytics
    [System.Serializable]
    public class PopulationDiversityMetrics
    {
        public string PopulationName = "";
        public int SampleSize = 0;
        public float ShannonDiversityIndex = 0f;
        public float SimpsonDiversityIndex = 0f;
        public float GeneticRichness = 0f;
        public float Evenness = 0f;
        public float AllelicDiversity = 0f;
        public float EffectivePopulationSize = 0f;
        public int UniqueGenotypes = 0;
        public Dictionary<string, float> TraitDiversity = new Dictionary<string, float>();
        public DateTime AnalysisDate = DateTime.Now;
    }

    [System.Serializable]
    public class BreedingSuccessMetrics
    {
        public string ProgramName = "";
        public int TotalBreedingAttempts = 0;
        public float SuccessRate = 0f;
        public Dictionary<string, float> GenerationProgression = new Dictionary<string, float>();
        public Dictionary<string, float> TraitImprovement = new Dictionary<string, float>();
        public float BreedingEfficiency = 0f;
        public float GeneticGain = 0f;
        public float InbreedingCoefficient = 0f;
        public DateTime AnalysisDate = DateTime.Now;
    }

    [System.Serializable]
    public class GeneticsPerformanceMetrics
    {
        public string OperationType = "";
        public int TotalExecutions = 0;
        public float TotalExecutionTime = 0f;
        public float AverageExecutionTime = 0f;
        public float LastExecutionTime = 0f;
        public DateTime LastUpdate = DateTime.Now;
    }

    [System.Serializable]
    public class DiversityDataPoint
    {
        public DateTime Timestamp = DateTime.Now;
        public float ShannonIndex = 0f;
        public float SimpsonIndex = 0f;
        public float GeneticRichness = 0f;
        public float Evenness = 0f;
    }

    [System.Serializable]
    public class PerformanceDataPoint
    {
        public DateTime Timestamp = DateTime.Now;
        public string OperationType = "";
        public float ExecutionTime = 0f;
        public int OperationCount = 0;
    }

    [System.Serializable]
    public class BreedingDataPoint
    {
        public DateTime Timestamp = DateTime.Now;
        public float SuccessRate = 0f;
        public float BreedingEfficiency = 0f;
        public float GeneticGain = 0f;
        public float InbreedingCoefficient = 0f;
    }

    [System.Serializable]
    public class TrendAnalysis
    {
        public string TrendType = "";
        public string TrendDirection = "";
        public float TrendStrength = 0f;
        public int DataPoints = 0;
        public DateTime AnalysisDate = DateTime.Now;
    }

    [System.Serializable]
    public class PredictiveModel
    {
        public string ModelType = "";
        public Dictionary<string, float> ModelParameters = new Dictionary<string, float>();
        public float Accuracy = 0f;
        public DateTime LastTrained = DateTime.Now;
    }

    [System.Serializable]
    public class AnalyticsReport
    {
        public string ReportId = "";
        public string ReportType = "";
        public string Summary = "";
        public DateTime GenerationDate = DateTime.Now;
        public Dictionary<string, object> DetailedData = new Dictionary<string, object>();
    }

    [System.Serializable]
    public class AnalyticsAlert
    {
        public string AlertId = "";
        public string AlertType = "";
        public string PopulationName = "";
        public string AlertMessage = "";
        public AlertSeverity Severity = AlertSeverity.Info;
        public DateTime Timestamp = DateTime.Now;
        public bool IsAcknowledged = false;
        
        // Alias property for CannabisGeneticsOrchestrator compatibility
        public string Message => AlertMessage;
    }

    [System.Serializable]
    public class AlertStatus
    {
        public bool IsActive = false;
        public DateTime LastAlert = DateTime.Now;
        public int AlertCount = 0;
    }

    [System.Serializable]
    public class GeneticsAnalyticsData
    {
        public int TotalPopulations = 0;
        public int TotalBreedingPrograms = 0;
        public int TotalReports = 0;
        public int ActiveAlerts = 0;
        public float AverageProcessingTime = 0f;
        public float OverallDiversityScore = 0f;
        public float OverallBreedingSuccess = 0f;
        public DateTime LastUpdate = DateTime.Now;
    }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }
}