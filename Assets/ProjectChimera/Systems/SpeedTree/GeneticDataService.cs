using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Genetic Data Service - Handles genotype database management, lineage tracking, and genetic data persistence
    /// Extracted from CannabisGeneticsEngine to provide focused genetic data management functionality
    /// Manages comprehensive genetic database operations: genotype storage, lineage trees, breeding history
    /// Implements scientific data management: genetic diversity metrics, population genetics, breeding analytics
    /// </summary>
    public class GeneticDataService : MonoBehaviour
    {
        [Header("Data Management Configuration")]
        [SerializeField] private bool _enableDataPersistence = true;
        [SerializeField] private bool _enableLineageTracking = true;
        [SerializeField] private bool _enableBreedingHistory = true;
        [SerializeField] private bool _enableDataLogging = false;

        [Header("Database Parameters")]
        [SerializeField] private int _maxGenotypeStorage = 10000;
        [SerializeField] private int _maxLineageDepth = 50;
        [SerializeField] private int _maxBreedingHistory = 1000;
        [SerializeField] private float _autoSaveInterval = 300f; // 5 minutes

        [Header("Data Validation")]
        [SerializeField] private bool _enableDataValidation = true;
        [SerializeField] private bool _enableIntegrityChecks = true;
        [SerializeField] private bool _enableBackupGeneration = true;
        [SerializeField] private int _maxBackupFiles = 10;

        [Header("Performance Settings")]
        [SerializeField] private bool _enableLazyLoading = true;
        [SerializeField] private bool _enableDataCompression = true;
        [SerializeField] private int _maxConcurrentOperations = 5;
        [SerializeField] private float _cacheEvictionThreshold = 0.8f;

        // Service state
        private bool _isInitialized = false;
        private DataManager _dataManager;
        private ScriptableObject _dataConfig;

        // Genetic database
        private Dictionary<string, CannabisGenotype> _genotypeDatabase = new Dictionary<string, CannabisGenotype>();
        private Dictionary<string, LineageEntry> _lineageDatabase = new Dictionary<string, LineageEntry>();
        private Dictionary<string, BreedingRecord> _breedingHistory = new Dictionary<string, BreedingRecord>();

        // Population tracking
        private Dictionary<string, PopulationData> _populationDatabase = new Dictionary<string, PopulationData>();
        private Dictionary<string, StrainLineage> _strainLineages = new Dictionary<string, StrainLineage>();
        private Dictionary<string, GeneticDiversityMetrics> _diversityMetrics = new Dictionary<string, GeneticDiversityMetrics>();

        // Data caching and performance
        private Dictionary<string, GenotypeQueryResult> _queryCache = new Dictionary<string, GenotypeQueryResult>();
        private Dictionary<string, LineageQueryResult> _lineageCache = new Dictionary<string, LineageQueryResult>();
        private Queue<DatabaseOperation> _operationQueue = new Queue<DatabaseOperation>();

        // Analytics and metrics
        private GeneticDatabaseAnalytics _databaseAnalytics = new GeneticDatabaseAnalytics();
        private Dictionary<string, float> _performanceMetrics = new Dictionary<string, float>();
        private Dictionary<string, int> _operationCounts = new Dictionary<string, int>();

        // Performance tracking
        private float _lastAutoSave = 0f;
        private float _lastIntegrityCheck = 0f;
        private int _totalOperations = 0;
        private int _successfulOperations = 0;
        private float _averageOperationTime = 0f;

        // Events
        public static event Action<CannabisGenotype> OnGenotypeAdded;
        public static event Action<string> OnGenotypeRemoved;
        public static event Action<BreedingRecord> OnBreedingRecorded;
        public static event Action<LineageEntry> OnLineageUpdated;
        public static event Action<GeneticDatabaseAnalytics> OnAnalyticsUpdated;
        public static event Action<string> OnDataError;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Genetic Data Service";
        public int TotalGenotypes => _genotypeDatabase.Count;
        public int TotalLineageEntries => _lineageDatabase.Count;
        public int TotalBreedingRecords => _breedingHistory.Count;
        public int TotalPopulations => _populationDatabase.Count;
        public float DatabaseSize => CalculateDatabaseSize();
        public float CacheHitRate => _totalOperations > 0 ? _queryCache.Count / (float)_totalOperations : 0f;

        public void Initialize(ScriptableObject dataConfig = null)
        {
            InitializeService(dataConfig);
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
                ProcessOperationQueue();
                CheckAutoSave();
                CheckIntegritySchedule();
                UpdateAnalytics();
            }
        }

        private void InitializeDataStructures()
        {
            _genotypeDatabase = new Dictionary<string, CannabisGenotype>();
            _lineageDatabase = new Dictionary<string, LineageEntry>();
            _breedingHistory = new Dictionary<string, BreedingRecord>();
            _populationDatabase = new Dictionary<string, PopulationData>();
            _strainLineages = new Dictionary<string, StrainLineage>();
            _diversityMetrics = new Dictionary<string, GeneticDiversityMetrics>();
            _queryCache = new Dictionary<string, GenotypeQueryResult>();
            _lineageCache = new Dictionary<string, LineageQueryResult>();
            _operationQueue = new Queue<DatabaseOperation>();
            _databaseAnalytics = new GeneticDatabaseAnalytics();
            _performanceMetrics = new Dictionary<string, float>();
            _operationCounts = new Dictionary<string, int>();
        }

        public void InitializeService(ScriptableObject dataConfig = null)
        {
            if (_isInitialized)
            {
                if (_enableDataLogging)
                    Debug.LogWarning("GeneticDataService already initialized");
                return;
            }

            try
            {
                _dataConfig = dataConfig;

                InitializeDataManager();
                InitializeDatabaseSystem();
                LoadExistingData();
                InitializeAnalytics();
                
                _isInitialized = true;
                _lastAutoSave = Time.time;
                _lastIntegrityCheck = Time.time;
                
                if (_enableDataLogging)
                    Debug.Log("GeneticDataService initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize GeneticDataService: {ex.Message}");
                OnDataError?.Invoke($"Service initialization failed: {ex.Message}");
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            try
            {
                // Save all pending data
                if (_enableDataPersistence)
                {
                    SaveAllData();
                }

                // Process remaining operations
                ProcessAllPendingOperations();
                
                // Clear all data
                ClearAllData();
                
                _isInitialized = false;
                
                if (_enableDataLogging)
                    Debug.Log("GeneticDataService shutdown completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during GeneticDataService shutdown: {ex.Message}");
            }
        }

        #endregion

        #region Genotype Database Management

        /// <summary>
        /// Add genotype to database
        /// </summary>
        public bool AddGenotype(CannabisGenotype genotype)
        {
            if (!_isInitialized || genotype == null)
                return false;

            try
            {
                var startTime = DateTime.Now;
                
                if (_enableDataLogging)
                    Debug.Log($"Adding genotype to database: {genotype.StrainName} ({genotype.GenotypeId})");

                // Validate genotype data
                if (_enableDataValidation && !ValidateGenotype(genotype))
                {
                    OnDataError?.Invoke($"Invalid genotype data: {genotype.GenotypeId}");
                    return false;
                }

                // Check storage limits
                if (_genotypeDatabase.Count >= _maxGenotypeStorage)
                {
                    PerformDatabaseMaintenance();
                }

                // Add to database
                _genotypeDatabase[genotype.GenotypeId] = genotype;

                // Update lineage if tracking enabled
                if (_enableLineageTracking)
                {
                    UpdateLineageEntry(genotype);
                }

                // Update population data
                UpdatePopulationData(genotype);

                // Invalidate relevant caches
                InvalidateQueryCache(genotype.GenotypeId);

                // Track performance
                var operationTime = (float)(DateTime.Now - startTime).TotalSeconds;
                UpdatePerformanceMetrics("AddGenotype", operationTime);

                OnGenotypeAdded?.Invoke(genotype);
                
                if (_enableDataLogging)
                    Debug.Log($"Genotype added successfully: {genotype.GenotypeId} in {operationTime:F3}s");

                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error adding genotype: {ex.Message}");
                OnDataError?.Invoke($"Failed to add genotype: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get genotype from database
        /// </summary>
        public CannabisGenotype GetGenotype(string genotypeId)
        {
            if (!_isInitialized || string.IsNullOrEmpty(genotypeId))
                return null;

            try
            {
                // Check cache first
                var cacheKey = $"genotype_{genotypeId}";
                if (_queryCache.TryGetValue(cacheKey, out var cachedResult) && cachedResult.IsValid())
                {
                    return cachedResult.Genotype;
                }

                // Query database
                if (_genotypeDatabase.TryGetValue(genotypeId, out var genotype))
                {
                    // Cache result
                    _queryCache[cacheKey] = new GenotypeQueryResult
                    {
                        Genotype = genotype,
                        QueryTime = DateTime.Now,
                        IsValid = () => true
                    };

                    return genotype;
                }

                return null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error retrieving genotype: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Remove genotype from database
        /// </summary>
        public bool RemoveGenotype(string genotypeId)
        {
            if (!_isInitialized || string.IsNullOrEmpty(genotypeId))
                return false;

            try
            {
                if (_genotypeDatabase.Remove(genotypeId))
                {
                    // Remove from lineage
                    _lineageDatabase.Remove(genotypeId);

                    // Update population data
                    UpdatePopulationDataRemoval(genotypeId);

                    // Clear caches
                    ClearRelatedCaches(genotypeId);

                    OnGenotypeRemoved?.Invoke(genotypeId);
                    
                    if (_enableDataLogging)
                        Debug.Log($"Genotype removed: {genotypeId}");

                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error removing genotype: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Query genotypes by criteria
        /// </summary>
        public List<CannabisGenotype> QueryGenotypes(GenotypeQueryCriteria criteria)
        {
            if (!_isInitialized || criteria == null)
                return new List<CannabisGenotype>();

            try
            {
                var startTime = DateTime.Now;
                var results = new List<CannabisGenotype>();

                // Check cache for this query
                var cacheKey = criteria.GetCacheKey();
                if (_queryCache.TryGetValue(cacheKey, out var cachedResult) && cachedResult.IsValid())
                {
                    return cachedResult.Results;
                }

                // Perform query
                foreach (var genotype in _genotypeDatabase.Values)
                {
                    if (criteria.Matches(genotype))
                    {
                        results.Add(genotype);
                    }
                }

                // Apply sorting and pagination
                results = ApplyQueryFilters(results, criteria);

                // Cache results
                _queryCache[cacheKey] = new GenotypeQueryResult
                {
                    Results = results,
                    QueryTime = DateTime.Now,
                    IsValid = () => (DateTime.Now - DateTime.Now).TotalMinutes < 5
                };

                var operationTime = (float)(DateTime.Now - startTime).TotalSeconds;
                UpdatePerformanceMetrics("QueryGenotypes", operationTime);

                if (_enableDataLogging)
                    Debug.Log($"Query completed: {results.Count} results in {operationTime:F3}s");

                return results;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error querying genotypes: {ex.Message}");
                return new List<CannabisGenotype>();
            }
        }

        #endregion

        #region Lineage Tracking

        /// <summary>
        /// Update lineage entry for genotype
        /// </summary>
        private void UpdateLineageEntry(CannabisGenotype genotype)
        {
            try
            {
                var lineageEntry = new LineageEntry
                {
                    GenotypeId = genotype.GenotypeId,
                    StrainName = genotype.StrainName,
                    Generation = genotype.Generation,
                    ParentIds = genotype.ParentIDs?.ToList() ?? new List<string>(),
                    CreationDate = genotype.CreationDate,
                    LastUpdate = DateTime.Now
                };

                _lineageDatabase[genotype.GenotypeId] = lineageEntry;

                // Update strain lineage
                UpdateStrainLineage(genotype);

                OnLineageUpdated?.Invoke(lineageEntry);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error updating lineage: {ex.Message}");
            }
        }

        /// <summary>
        /// Get complete lineage for genotype
        /// </summary>
        public LineageTree GetLineageTree(string genotypeId, int maxDepth = -1)
        {
            if (!_isInitialized || string.IsNullOrEmpty(genotypeId))
                return null;

            try
            {
                var cacheKey = $"lineage_{genotypeId}_{maxDepth}";
                if (_lineageCache.TryGetValue(cacheKey, out var cachedResult) && cachedResult.IsValid())
                {
                    return cachedResult.LineageTree;
                }

                var tree = BuildLineageTree(genotypeId, maxDepth == -1 ? _maxLineageDepth : maxDepth);
                
                // Cache result
                _lineageCache[cacheKey] = new LineageQueryResult
                {
                    LineageTree = tree,
                    QueryTime = DateTime.Now,
                    IsValid = () => (DateTime.Now - DateTime.Now).TotalMinutes < 10
                };

                return tree;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error building lineage tree: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Build lineage tree recursively
        /// </summary>
        private LineageTree BuildLineageTree(string genotypeId, int remainingDepth)
        {
            if (remainingDepth <= 0 || !_lineageDatabase.TryGetValue(genotypeId, out var entry))
                return null;

            var tree = new LineageTree
            {
                GenotypeId = genotypeId,
                StrainName = entry.StrainName,
                Generation = entry.Generation,
                CreationDate = entry.CreationDate,
                Children = new List<LineageTree>()
            };

            // Build parent trees
            foreach (var parentId in entry.ParentIds)
            {
                var parentTree = BuildLineageTree(parentId, remainingDepth - 1);
                if (parentTree != null)
                {
                    tree.Children.Add(parentTree);
                }
            }

            return tree;
        }

        #endregion

        #region Breeding History

        /// <summary>
        /// Record breeding operation
        /// </summary>
        public void RecordBreeding(BreedingRecord record)
        {
            if (!_isInitialized || record == null || !_enableBreedingHistory)
                return;

            try
            {
                if (_enableDataLogging)
                    Debug.Log($"Recording breeding: {record.BreedingId}");

                // Validate breeding record
                if (_enableDataValidation && !ValidateBreedingRecord(record))
                {
                    OnDataError?.Invoke($"Invalid breeding record: {record.BreedingId}");
                    return;
                }

                // Check storage limits
                if (_breedingHistory.Count >= _maxBreedingHistory)
                {
                    RemoveOldestBreedingRecords(100); // Remove oldest 100 records
                }

                _breedingHistory[record.BreedingId] = record;

                // Update related lineage entries
                if (_enableLineageTracking)
                {
                    UpdateBreedingLineage(record);
                }

                OnBreedingRecorded?.Invoke(record);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error recording breeding: {ex.Message}");
                OnDataError?.Invoke($"Failed to record breeding: {ex.Message}");
            }
        }

        /// <summary>
        /// Get breeding history for genotype
        /// </summary>
        public List<BreedingRecord> GetBreedingHistory(string genotypeId)
        {
            if (!_isInitialized || string.IsNullOrEmpty(genotypeId))
                return new List<BreedingRecord>();

            try
            {
                return _breedingHistory.Values
                    .Where(record => record.Parent1Id == genotypeId || record.Parent2Id == genotypeId || record.OffspringIds.Contains(genotypeId))
                    .OrderByDescending(record => record.BreedingDate)
                    .ToList();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error retrieving breeding history: {ex.Message}");
                return new List<BreedingRecord>();
            }
        }

        #endregion

        #region Population and Diversity Tracking

        /// <summary>
        /// Update population data for genotype
        /// </summary>
        private void UpdatePopulationData(CannabisGenotype genotype)
        {
            try
            {
                var populationKey = genotype.StrainName;
                
                if (!_populationDatabase.TryGetValue(populationKey, out var population))
                {
                    population = new PopulationData
                    {
                        StrainName = genotype.StrainName,
                        GenotypeIds = new List<string>(),
                        CreationDate = DateTime.Now,
                        LastUpdate = DateTime.Now
                    };
                    _populationDatabase[populationKey] = population;
                }

                if (!population.GenotypeIds.Contains(genotype.GenotypeId))
                {
                    population.GenotypeIds.Add(genotype.GenotypeId);
                    population.TotalIndividuals = population.GenotypeIds.Count;
                    population.LastUpdate = DateTime.Now;

                    // Update diversity metrics
                    UpdateDiversityMetrics(populationKey);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error updating population data: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate genetic diversity metrics
        /// </summary>
        private void UpdateDiversityMetrics(string populationKey)
        {
            try
            {
                if (!_populationDatabase.TryGetValue(populationKey, out var population))
                    return;

                var genotypes = population.GenotypeIds
                    .Select(id => GetGenotype(id))
                    .Where(g => g != null)
                    .ToList();

                if (genotypes.Count == 0)
                    return;

                var metrics = new GeneticDiversityMetrics
                {
                    PopulationName = populationKey,
                    TotalIndividuals = genotypes.Count,
                    UniqueGenotypes = genotypes.Count,
                    AlleleFrequencies = CalculateAlleleFrequencies(genotypes),
                    HeterozygosityIndex = CalculateHeterozygosity(genotypes),
                    GeneticDistance = CalculateGeneticDistance(genotypes),
                    CalculationDate = DateTime.Now
                };

                _diversityMetrics[populationKey] = metrics;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error calculating diversity metrics: {ex.Message}");
            }
        }

        /// <summary>
        /// Get genetic diversity metrics for population
        /// </summary>
        public GeneticDiversityMetrics GetDiversityMetrics(string populationName)
        {
            return _diversityMetrics.TryGetValue(populationName, out var metrics) ? metrics : null;
        }

        #endregion

        #region Utility Methods

        private void InitializeDataManager()
        {
            try
            {
                _dataManager = GameManager.Instance.GetManager<DataManager>();
                
                if (_dataManager == null && _enableDataLogging)
                {
                    Debug.LogWarning("DataManager not found - data persistence disabled");
                    _enableDataPersistence = false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize data manager: {ex.Message}");
                _enableDataPersistence = false;
            }
        }

        private void InitializeDatabaseSystem()
        {
            // Initialize database optimization settings
            InitializePerformanceMetrics();
        }

        private void LoadExistingData()
        {
            if (!_enableDataPersistence || _dataManager == null)
                return;

            try
            {
                // Load genetic data from persistent storage
                // This would integrate with the actual data persistence system
                if (_enableDataLogging)
                    Debug.Log("Loading existing genetic data...");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load existing data: {ex.Message}");
            }
        }

        private void InitializeAnalytics()
        {
            _databaseAnalytics = new GeneticDatabaseAnalytics
            {
                TotalGenotypes = 0,
                TotalLineageEntries = 0,
                TotalBreedingRecords = 0,
                TotalOperations = 0,
                SuccessfulOperations = 0,
                DatabaseSize = 0f,
                AverageOperationTime = 0f,
                CacheHitRate = 0f,
                LastAnalyticsUpdate = DateTime.Now
            };
        }

        private void InitializePerformanceMetrics()
        {
            _performanceMetrics["AddGenotype"] = 0f;
            _performanceMetrics["GetGenotype"] = 0f;
            _performanceMetrics["QueryGenotypes"] = 0f;
            _performanceMetrics["RemoveGenotype"] = 0f;
            _performanceMetrics["LineageQuery"] = 0f;
            
            _operationCounts["AddGenotype"] = 0;
            _operationCounts["GetGenotype"] = 0;
            _operationCounts["QueryGenotypes"] = 0;
            _operationCounts["RemoveGenotype"] = 0;
            _operationCounts["LineageQuery"] = 0;
        }

        private bool ValidateGenotype(CannabisGenotype genotype)
        {
            return !string.IsNullOrEmpty(genotype.GenotypeId) &&
                   !string.IsNullOrEmpty(genotype.StrainName) &&
                   genotype.Generation >= 0;
        }

        private bool ValidateBreedingRecord(BreedingRecord record)
        {
            return !string.IsNullOrEmpty(record.BreedingId) &&
                   (!string.IsNullOrEmpty(record.Parent1Id) || !string.IsNullOrEmpty(record.Parent2Id)) &&
                   record.OffspringIds != null && record.OffspringIds.Count >= 1;
        }

        private void PerformDatabaseMaintenance()
        {
            try
            {
                // Remove oldest genotypes if at capacity
                var oldestGenotypes = _genotypeDatabase.Values
                    .OrderBy(g => g.CreationDate)
                    .Take(100)
                    .ToList();

                foreach (var genotype in oldestGenotypes)
                {
                    RemoveGenotype(genotype.GenotypeId);
                }

                if (_enableDataLogging)
                    Debug.Log($"Database maintenance: removed {oldestGenotypes.Count} old genotypes");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during database maintenance: {ex.Message}");
            }
        }

        private void UpdateStrainLineage(CannabisGenotype genotype)
        {
            if (!_strainLineages.TryGetValue(genotype.StrainName, out var lineage))
            {
                lineage = new StrainLineage
                {
                    StrainName = genotype.StrainName,
                    GenotypeIds = new List<string>(),
                    GenerationHistory = new Dictionary<int, List<string>>(),
                    CreationDate = DateTime.Now
                };
                _strainLineages[genotype.StrainName] = lineage;
            }

            if (!lineage.GenotypeIds.Contains(genotype.GenotypeId))
            {
                lineage.GenotypeIds.Add(genotype.GenotypeId);
                
                if (!lineage.GenerationHistory.ContainsKey(genotype.Generation))
                {
                    lineage.GenerationHistory[genotype.Generation] = new List<string>();
                }
                lineage.GenerationHistory[genotype.Generation].Add(genotype.GenotypeId);
                
                lineage.LastUpdate = DateTime.Now;
            }
        }

        private void UpdateBreedingLineage(BreedingRecord record)
        {
            // Update lineage entries for offspring
            foreach (var offspringId in record.OffspringIds)
            {
                if (_lineageDatabase.TryGetValue(offspringId, out var lineage))
                {
                    // Build ParentIds list from individual parent properties
                    var parentIds = new List<string>();
                    if (!string.IsNullOrEmpty(record.Parent1Id)) parentIds.Add(record.Parent1Id);
                    if (!string.IsNullOrEmpty(record.Parent2Id)) parentIds.Add(record.Parent2Id);
                    
                    lineage.ParentIds = parentIds;
                    lineage.LastUpdate = DateTime.Now;
                }
            }
        }

        private void RemoveOldestBreedingRecords(int count)
        {
            var oldestRecords = _breedingHistory.Values
                .OrderBy(r => r.BreedingDate)
                .Take(count)
                .ToList();

            foreach (var record in oldestRecords)
            {
                _breedingHistory.Remove(record.BreedingId);
            }
        }

        private void UpdatePopulationDataRemoval(string genotypeId)
        {
            foreach (var population in _populationDatabase.Values)
            {
                if (population.GenotypeIds.Remove(genotypeId))
                {
                    population.TotalIndividuals = population.GenotypeIds.Count;
                    population.LastUpdate = DateTime.Now;
                    
                    // Update diversity metrics
                    UpdateDiversityMetrics(population.StrainName);
                }
            }
        }

        private List<CannabisGenotype> ApplyQueryFilters(List<CannabisGenotype> results, GenotypeQueryCriteria criteria)
        {
            // Apply sorting
            switch (criteria.SortBy)
            {
                case GenotypeSortOrder.CreationDate:
                    results = criteria.SortDescending ? 
                        results.OrderByDescending(g => g.CreationDate).ToList() :
                        results.OrderBy(g => g.CreationDate).ToList();
                    break;
                case GenotypeSortOrder.Generation:
                    results = criteria.SortDescending ?
                        results.OrderByDescending(g => g.Generation).ToList() :
                        results.OrderBy(g => g.Generation).ToList();
                    break;
                case GenotypeSortOrder.StrainName:
                    results = criteria.SortDescending ?
                        results.OrderByDescending(g => g.StrainName).ToList() :
                        results.OrderBy(g => g.StrainName).ToList();
                    break;
            }

            // Apply pagination
            if (criteria.MaxResults > 0)
            {
                results = results.Take(criteria.MaxResults).ToList();
            }

            return results;
        }

        private Dictionary<string, float> CalculateAlleleFrequencies(List<CannabisGenotype> genotypes)
        {
            var frequencies = new Dictionary<string, float>();
            
            // Simplified allele frequency calculation
            // In a real implementation, this would analyze actual genetic data
            var totalAlleles = genotypes.Count * 2; // Diploid assumption
            
            foreach (var genotype in genotypes)
            {
                if (genotype.Traits != null)
                {
                    foreach (var trait in genotype.Traits)
                    {
                        var alleleKey = $"{trait.TraitName}_{trait.ExpressedValue:F2}";
                        frequencies[alleleKey] = frequencies.GetValueOrDefault(alleleKey, 0f) + 2f / totalAlleles;
                    }
                }
            }

            return frequencies;
        }

        private float CalculateHeterozygosity(List<CannabisGenotype> genotypes)
        {
            // Simplified heterozygosity calculation
            // In a real implementation, this would analyze actual genetic diversity
            return genotypes.Count > 1 ? 0.7f : 0f;
        }

        private float CalculateGeneticDistance(List<CannabisGenotype> genotypes)
        {
            // Simplified genetic distance calculation
            // In a real implementation, this would compare genetic markers
            return genotypes.Count > 1 ? 0.3f : 0f;
        }

        private void InvalidateQueryCache(string genotypeId)
        {
            var keysToRemove = _queryCache.Keys
                .Where(key => key.Contains(genotypeId))
                .ToList();
            
            foreach (var key in keysToRemove)
            {
                _queryCache.Remove(key);
            }
        }

        private void ClearRelatedCaches(string genotypeId)
        {
            InvalidateQueryCache(genotypeId);
            
            var lineageKeysToRemove = _lineageCache.Keys
                .Where(key => key.Contains(genotypeId))
                .ToList();
            
            foreach (var key in lineageKeysToRemove)
            {
                _lineageCache.Remove(key);
            }
        }

        private void ProcessOperationQueue()
        {
            int processedCount = 0;
            
            while (_operationQueue.Count > 0 && processedCount < _maxConcurrentOperations)
            {
                var operation = _operationQueue.Dequeue();
                ProcessDatabaseOperation(operation);
                processedCount++;
            }
        }

        private void ProcessDatabaseOperation(DatabaseOperation operation)
        {
            try
            {
                switch (operation.OperationType)
                {
                    case DatabaseOperationType.AddGenotype:
                        AddGenotype(operation.Genotype);
                        break;
                    case DatabaseOperationType.RemoveGenotype:
                        RemoveGenotype(operation.GenotypeId);
                        break;
                    case DatabaseOperationType.UpdateLineage:
                        UpdateLineageEntry(operation.Genotype);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing database operation: {ex.Message}");
            }
        }

        private void ProcessAllPendingOperations()
        {
            while (_operationQueue.Count > 0)
            {
                var operation = _operationQueue.Dequeue();
                ProcessDatabaseOperation(operation);
            }
        }

        private void CheckAutoSave()
        {
            if (!_enableDataPersistence || Time.time - _lastAutoSave < _autoSaveInterval)
                return;

            SaveAllData();
            _lastAutoSave = Time.time;
        }

        private void CheckIntegritySchedule()
        {
            if (!_enableIntegrityChecks || Time.time - _lastIntegrityCheck < 3600f) // 1 hour
                return;

            PerformIntegrityCheck();
            _lastIntegrityCheck = Time.time;
        }

        private void PerformIntegrityCheck()
        {
            try
            {
                int inconsistencies = 0;
                
                // Check genotype-lineage consistency
                foreach (var genotype in _genotypeDatabase.Values)
                {
                    if (_enableLineageTracking && !_lineageDatabase.ContainsKey(genotype.GenotypeId))
                    {
                        UpdateLineageEntry(genotype);
                        inconsistencies++;
                    }
                }

                if (_enableDataLogging && inconsistencies > 0)
                    Debug.Log($"Integrity check completed: fixed {inconsistencies} inconsistencies");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during integrity check: {ex.Message}");
            }
        }

        private void UpdatePerformanceMetrics(string operationType, float operationTime)
        {
            _totalOperations++;
            _successfulOperations++;
            
            _performanceMetrics[operationType] = (_performanceMetrics[operationType] + operationTime) / 2f;
            _operationCounts[operationType] = _operationCounts.GetValueOrDefault(operationType, 0) + 1;
            
            _averageOperationTime = (_averageOperationTime + operationTime) / 2f;
        }

        private void UpdateAnalytics()
        {
            var currentTime = DateTime.Now;
            if (_databaseAnalytics.LastAnalyticsUpdate == default ||
                (currentTime - _databaseAnalytics.LastAnalyticsUpdate).TotalMinutes >= 1)
            {
                UpdateAnalyticsData();
                _databaseAnalytics.LastAnalyticsUpdate = currentTime;
                
                OnAnalyticsUpdated?.Invoke(_databaseAnalytics);
            }
        }

        private void UpdateAnalyticsData()
        {
            _databaseAnalytics.TotalGenotypes = _genotypeDatabase.Count;
            _databaseAnalytics.TotalLineageEntries = _lineageDatabase.Count;
            _databaseAnalytics.TotalBreedingRecords = _breedingHistory.Count;
            _databaseAnalytics.TotalOperations = _totalOperations;
            _databaseAnalytics.SuccessfulOperations = _successfulOperations;
            _databaseAnalytics.DatabaseSize = CalculateDatabaseSize();
            _databaseAnalytics.AverageOperationTime = _averageOperationTime;
            _databaseAnalytics.CacheHitRate = CacheHitRate;
            _databaseAnalytics.PerformanceMetrics = new Dictionary<string, float>(_performanceMetrics);
        }

        private float CalculateDatabaseSize()
        {
            // Simplified database size calculation in MB
            float sizeEstimate = 0f;
            
            sizeEstimate += _genotypeDatabase.Count * 0.001f; // ~1KB per genotype
            sizeEstimate += _lineageDatabase.Count * 0.0005f; // ~0.5KB per lineage entry
            sizeEstimate += _breedingHistory.Count * 0.0002f; // ~0.2KB per breeding record
            
            return sizeEstimate;
        }

        private void SaveAllData()
        {
            try
            {
                if (_dataManager != null && _enableDataPersistence)
                {
                    // Save genetic database
                    // This would integrate with the actual data persistence system
                    
                    if (_enableDataLogging)
                        Debug.Log($"Saved genetic database: {_genotypeDatabase.Count} genotypes, {_lineageDatabase.Count} lineage entries");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error saving data: {ex.Message}");
            }
        }

        private void ClearAllData()
        {
            _genotypeDatabase.Clear();
            _lineageDatabase.Clear();
            _breedingHistory.Clear();
            _populationDatabase.Clear();
            _strainLineages.Clear();
            _diversityMetrics.Clear();
            _queryCache.Clear();
            _lineageCache.Clear();
            _operationQueue.Clear();
            _performanceMetrics.Clear();
            _operationCounts.Clear();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get all genotypes for a strain
        /// </summary>
        public List<CannabisGenotype> GetStrainGenotypes(string strainName)
        {
            var criteria = new GenotypeQueryCriteria
            {
                StrainName = strainName,
                SortBy = GenotypeSortOrder.Generation,
                SortDescending = false
            };
            return QueryGenotypes(criteria);
        }

        /// <summary>
        /// Get genotypes by generation
        /// </summary>
        public List<CannabisGenotype> GetGenotypesByGeneration(int generation)
        {
            var criteria = new GenotypeQueryCriteria
            {
                Generation = generation,
                SortBy = GenotypeSortOrder.StrainName,
                SortDescending = false
            };
            return QueryGenotypes(criteria);
        }

        /// <summary>
        /// Get strain lineage data
        /// </summary>
        public StrainLineage GetStrainLineage(string strainName)
        {
            return _strainLineages.TryGetValue(strainName, out var lineage) ? lineage : null;
        }

        /// <summary>
        /// Get population data for strain
        /// </summary>
        public PopulationData GetPopulationData(string strainName)
        {
            return _populationDatabase.TryGetValue(strainName, out var data) ? data : null;
        }

        /// <summary>
        /// Get database analytics
        /// </summary>
        public GeneticDatabaseAnalytics GetAnalytics()
        {
            return _databaseAnalytics;
        }

        /// <summary>
        /// Clear all cached data
        /// </summary>
        public void ClearAllCaches()
        {
            _queryCache.Clear();
            _lineageCache.Clear();
            
            if (_enableDataLogging)
                Debug.Log("All caches cleared");
        }

        /// <summary>
        /// Perform manual database maintenance
        /// </summary>
        public void PerformMaintenance()
        {
            PerformDatabaseMaintenance();
            PerformIntegrityCheck();
            
            if (_enableDataPersistence)
            {
                SaveAllData();
            }
        }

        /// <summary>
        /// Update data persistence settings
        /// </summary>
        public void UpdateDataSettings(bool enablePersistence, bool enableLineage, bool enableHistory, float saveInterval)
        {
            _enableDataPersistence = enablePersistence;
            _enableLineageTracking = enableLineage;
            _enableBreedingHistory = enableHistory;
            _autoSaveInterval = saveInterval;
            
            if (_enableDataLogging)
                Debug.Log($"Data settings updated: Persistence={enablePersistence}, Lineage={enableLineage}, History={enableHistory}, SaveInterval={saveInterval}");
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
    /// Database operation for queue processing
    /// </summary>
    [System.Serializable]
    public class DatabaseOperation
    {
        public DatabaseOperationType OperationType = DatabaseOperationType.AddGenotype;
        public CannabisGenotype Genotype;
        public string GenotypeId = "";
        public DateTime RequestTime = DateTime.Now;
        public int Priority = 0;
    }

    /// <summary>
    /// Database operation types
    /// </summary>
    public enum DatabaseOperationType
    {
        AddGenotype,
        RemoveGenotype,
        UpdateGenotype,
        UpdateLineage,
        AddBreedingRecord,
        Maintenance
    }

    /// <summary>
    /// Genotype query criteria
    /// </summary>
    [System.Serializable]
    public class GenotypeQueryCriteria
    {
        public string StrainName = "";
        public int? Generation;
        public List<string> TraitNames = new List<string>();
        public DateTime? CreatedAfter;
        public DateTime? CreatedBefore;
        public GenotypeSortOrder SortBy = GenotypeSortOrder.CreationDate;
        public bool SortDescending = true;
        public int MaxResults = 100;

        public string GetCacheKey()
        {
            return $"{StrainName}_{Generation}_{string.Join(",", TraitNames)}_{CreatedAfter}_{CreatedBefore}_{SortBy}_{SortDescending}_{MaxResults}";
        }

        public bool Matches(CannabisGenotype genotype)
        {
            if (!string.IsNullOrEmpty(StrainName) && genotype.StrainName != StrainName)
                return false;
            
            if (Generation.HasValue && genotype.Generation != Generation.Value)
                return false;
            
            if (CreatedAfter.HasValue && genotype.CreationDate < CreatedAfter.Value)
                return false;
            
            if (CreatedBefore.HasValue && genotype.CreationDate > CreatedBefore.Value)
                return false;

            if (TraitNames.Count > 0 && genotype.Traits != null)
            {
                var genotypeTraitNames = genotype.Traits.Select(t => t.TraitName).ToHashSet();
                if (!TraitNames.Any(name => genotypeTraitNames.Contains(name)))
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Genotype sort orders
    /// </summary>
    public enum GenotypeSortOrder
    {
        CreationDate,
        Generation,
        StrainName,
        GenotypeId
    }

    /// <summary>
    /// Query result caching
    /// </summary>
    [System.Serializable]
    public class GenotypeQueryResult
    {
        public CannabisGenotype Genotype;
        public List<CannabisGenotype> Results = new List<CannabisGenotype>();
        public DateTime QueryTime = DateTime.Now;
        public System.Func<bool> IsValid = () => true;
    }

    /// <summary>
    /// Lineage query result caching
    /// </summary>
    [System.Serializable]
    public class LineageQueryResult
    {
        public LineageTree LineageTree;
        public DateTime QueryTime = DateTime.Now;
        public System.Func<bool> IsValid = () => true;
    }

    /// <summary>
    /// Lineage entry for tracking
    /// </summary>
    [System.Serializable]
    public class LineageEntry
    {
        public string GenotypeId = "";
        public string StrainName = "";
        public int Generation = 0;
        public List<string> ParentIds = new List<string>();
        public DateTime CreationDate = DateTime.Now;
        public DateTime LastUpdate = DateTime.Now;
    }

    /// <summary>
    /// Lineage tree structure
    /// </summary>
    [System.Serializable]
    public class LineageTree
    {
        public string GenotypeId = "";
        public string StrainName = "";
        public int Generation = 0;
        public DateTime CreationDate = DateTime.Now;
        public List<LineageTree> Children = new List<LineageTree>();
    }

    /// <summary>
    /// Population data tracking
    /// </summary>
    [System.Serializable]
    public class PopulationData
    {
        public string StrainName = "";
        public List<string> GenotypeIds = new List<string>();
        public int TotalIndividuals = 0;
        public DateTime CreationDate = DateTime.Now;
        public DateTime LastUpdate = DateTime.Now;
    }

    /// <summary>
    /// Strain lineage tracking
    /// </summary>
    [System.Serializable]
    public class StrainLineage
    {
        public string StrainName = "";
        public List<string> GenotypeIds = new List<string>();
        public Dictionary<int, List<string>> GenerationHistory = new Dictionary<int, List<string>>();
        public DateTime CreationDate = DateTime.Now;
        public DateTime LastUpdate = DateTime.Now;
    }

    /// <summary>
    /// Genetic diversity metrics
    /// </summary>
    [System.Serializable]
    public class GeneticDiversityMetrics
    {
        public string PopulationName = "";
        public int TotalIndividuals = 0;
        public int UniqueGenotypes = 0;
        public Dictionary<string, float> AlleleFrequencies = new Dictionary<string, float>();
        public float HeterozygosityIndex = 0f;
        public float GeneticDistance = 0f;
        public DateTime CalculationDate = DateTime.Now;
    }

    /// <summary>
    /// Genetic database analytics
    /// </summary>
    [System.Serializable]
    public class GeneticDatabaseAnalytics
    {
        public int TotalGenotypes = 0;
        public int TotalLineageEntries = 0;
        public int TotalBreedingRecords = 0;
        public int TotalOperations = 0;
        public int SuccessfulOperations = 0;
        public float DatabaseSize = 0f;
        public float AverageOperationTime = 0f;
        public float CacheHitRate = 0f;
        public Dictionary<string, float> PerformanceMetrics = new Dictionary<string, float>();
        public DateTime LastAnalyticsUpdate = DateTime.Now;
    }
}