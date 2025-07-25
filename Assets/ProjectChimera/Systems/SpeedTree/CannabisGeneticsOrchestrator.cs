using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Cannabis Genetics Orchestrator - Central coordination and manager interface for all genetics services
    /// Coordinates all extracted genetics services to provide unified genetics functionality
    /// Manages service initialization, communication, and provides high-level genetics operations
    /// Serves as the main interface for other systems to interact with genetics functionality
    /// </summary>
    public class CannabisGeneticsOrchestrator : MonoBehaviour
    {
        [Header("Orchestrator Configuration")]
        [SerializeField] private bool _enableGeneticsSystem = true;
        [SerializeField] private bool _enableAutoInitialization = true;
        [SerializeField] private bool _enableServiceValidation = true;
        [SerializeField] private bool _enableOrchestrationLogging = false;

        [Header("Service Management")]
        [SerializeField] private bool _enableGeneticAlgorithms = true;
        [SerializeField] private bool _enableBreedingSystem = true;
        [SerializeField] private bool _enablePhenotypeExpression = true;
        [SerializeField] private bool _enableEnvironmentalAdaptation = true;
        [SerializeField] private bool _enableSpeedTreeGenetics = true;
        [SerializeField] private bool _enableGeneticData = true;
        [SerializeField] private bool _enableGeneticsAnalytics = true;

        [Header("Performance Settings")]
        [SerializeField] private bool _enableBatchProcessing = true;
        [SerializeField] private int _maxOperationsPerFrame = 10;
        [SerializeField] private float _serviceUpdateInterval = 0.1f;
        [SerializeField] private bool _enableAsyncOperations = true;

        // Service state
        private bool _isInitialized = false;
        private Dictionary<Type, MonoBehaviour> _services = new Dictionary<Type, MonoBehaviour>();
        private Queue<GeneticsOperation> _operationQueue = new Queue<GeneticsOperation>();

        // Core genetics services
        private GeneticAlgorithmService _geneticAlgorithmService;
        private BreedingSystemService _breedingSystemService;
        private PhenotypeExpressionService _phenotypeExpressionService;
        private EnvironmentalAdaptationService _environmentalAdaptationService;
        private SpeedTreeGeneticsService _speedTreeGeneticsService;
        private GeneticDataService _geneticDataService;
        private GeneticsAnalyticsService _geneticsAnalyticsService;

        // Orchestration data
        private Dictionary<string, CannabisGenotype> _activeGenotypes = new Dictionary<string, CannabisGenotype>();
        private Dictionary<string, CannabisPhenotype> _activePhenotypes = new Dictionary<string, CannabisPhenotype>();
        private Dictionary<string, GeneticsOperation> _pendingOperations = new Dictionary<string, GeneticsOperation>();

        // Performance tracking
        private float _lastServiceUpdate = 0f;
        private int _totalOperations = 0;
        private int _successfulOperations = 0;
        private float _averageOperationTime = 0f;
        private GeneticsOrchestrationMetrics _orchestrationMetrics = new GeneticsOrchestrationMetrics();

        // Events
        public static event Action<CannabisGeneticsOrchestrator> OnOrchestratorInitialized;
        public static event Action<CannabisGenotype> OnGenotypeCreated;
        public static event Action<CannabisPhenotype> OnPhenotypeExpressed;
        public static event Action<BreedingResult> OnBreedingCompleted;
        public static event Action<string, string> OnGeneticsError;
        public static event Action<GeneticsOrchestrationMetrics> OnMetricsUpdated;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Cannabis Genetics Orchestrator";
        public int ActiveGenotypes => _activeGenotypes.Count;
        public int ActivePhenotypes => _activePhenotypes.Count;
        public int PendingOperations => _operationQueue.Count;
        public float SuccessRate => _totalOperations > 0 ? (float)_successfulOperations / _totalOperations : 0f;
        public GeneticsOrchestrationMetrics Metrics => _orchestrationMetrics;

        public void Initialize()
        {
            InitializeOrchestrator();
        }

        public void Shutdown()
        {
            ShutdownOrchestrator();
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_enableAutoInitialization)
            {
                InitializeOrchestrator();
            }
        }

        private void Update()
        {
            if (_isInitialized)
            {
                ProcessOperationQueue();
                UpdateServices();
                UpdateMetrics();
            }
        }

        private void OnDestroy()
        {
            ShutdownOrchestrator();
        }

        #endregion

        #region Orchestrator Management

        private void InitializeOrchestrator()
        {
            if (_isInitialized)
            {
                if (_enableOrchestrationLogging)
                    Debug.LogWarning("CannabisGeneticsOrchestrator already initialized");
                return;
            }

            if (!_enableGeneticsSystem)
            {
                if (_enableOrchestrationLogging)
                    Debug.Log("Genetics system disabled - skipping initialization");
                return;
            }

            try
            {
                InitializeServices();
                ConnectServiceEvents();
                InitializeOperationQueue();
                ValidateServices();

                _isInitialized = true;
                OnOrchestratorInitialized?.Invoke(this);

                if (_enableOrchestrationLogging)
                    Debug.Log("CannabisGeneticsOrchestrator initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize CannabisGeneticsOrchestrator: {ex.Message}");
                OnGeneticsError?.Invoke("Orchestrator", $"Initialization failed: {ex.Message}");
            }
        }

        private void InitializeServices()
        {
            // Initialize Genetic Algorithm Service
            if (_enableGeneticAlgorithms)
            {
                _geneticAlgorithmService = GetOrCreateService<GeneticAlgorithmService>();
                _geneticAlgorithmService.Initialize();
                _services[typeof(GeneticAlgorithmService)] = _geneticAlgorithmService;
            }

            // Initialize Breeding System Service
            if (_enableBreedingSystem)
            {
                _breedingSystemService = GetOrCreateService<BreedingSystemService>();
                _breedingSystemService.Initialize();
                _services[typeof(BreedingSystemService)] = _breedingSystemService;
            }

            // Initialize Phenotype Expression Service
            if (_enablePhenotypeExpression)
            {
                _phenotypeExpressionService = GetOrCreateService<PhenotypeExpressionService>();
                _phenotypeExpressionService.Initialize();
                _services[typeof(PhenotypeExpressionService)] = _phenotypeExpressionService;
            }

            // Initialize Environmental Adaptation Service
            if (_enableEnvironmentalAdaptation)
            {
                _environmentalAdaptationService = GetOrCreateService<EnvironmentalAdaptationService>();
                _environmentalAdaptationService.Initialize();
                _services[typeof(EnvironmentalAdaptationService)] = _environmentalAdaptationService;
            }

            // Initialize SpeedTree Genetics Service
            if (_enableSpeedTreeGenetics)
            {
                _speedTreeGeneticsService = GetOrCreateService<SpeedTreeGeneticsService>();
                _speedTreeGeneticsService.Initialize();
                _services[typeof(SpeedTreeGeneticsService)] = _speedTreeGeneticsService;
            }

            // Initialize Genetic Data Service
            if (_enableGeneticData)
            {
                _geneticDataService = GetOrCreateService<GeneticDataService>();
                _geneticDataService.Initialize();
                _services[typeof(GeneticDataService)] = _geneticDataService;
            }

            // Initialize Genetics Analytics Service
            if (_enableGeneticsAnalytics)
            {
                _geneticsAnalyticsService = GetOrCreateService<GeneticsAnalyticsService>();
                _geneticsAnalyticsService.Initialize();
                _services[typeof(GeneticsAnalyticsService)] = _geneticsAnalyticsService;
            }

            if (_enableOrchestrationLogging)
                Debug.Log($"Initialized {_services.Count} genetics services");
        }

        private T GetOrCreateService<T>() where T : MonoBehaviour
        {
            var service = GetComponent<T>();
            if (service == null)
            {
                service = gameObject.AddComponent<T>();
            }
            return service;
        }

        private void ConnectServiceEvents()
        {
            // Connect breeding events
            if (_breedingSystemService != null)
            {
                BreedingSystemService.OnBreedingCompleted += HandleBreedingCompleted;
                BreedingSystemService.OnBreedingFailed += HandleBreedingFailed;
            }

            // Connect phenotype events
            if (_phenotypeExpressionService != null)
            {
                PhenotypeExpressionService.OnPhenotypeCalculated += HandlePhenotypeCalculated;
                PhenotypeExpressionService.OnExpressionError += HandlePhenotypeError;
            }

            // Connect data service events
            if (_geneticDataService != null)
            {
                GeneticDataService.OnGenotypeAdded += HandleGenotypeAdded;
                GeneticDataService.OnDataError += HandleDataError;
            }

            // Connect analytics events
            if (_geneticsAnalyticsService != null)
            {
                GeneticsAnalyticsService.OnAnalyticsAlert += HandleAnalyticsAlert;
            }
        }

        private void InitializeOperationQueue()
        {
            _operationQueue.Clear();
            _pendingOperations.Clear();
            _totalOperations = 0;
            _successfulOperations = 0;
            _averageOperationTime = 0f;
        }

        private void ValidateServices()
        {
            if (!_enableServiceValidation) return;

            var validationResults = new List<string>();

            foreach (var service in _services.Values)
            {
                if (service == null)
                {
                    validationResults.Add($"Service {service.GetType().Name} is null");
                    continue;
                }

                // Check if service implements expected interface
                if (service is IService serviceInterface)
                {
                    if (!serviceInterface.IsInitialized)
                    {
                        validationResults.Add($"Service {service.GetType().Name} failed to initialize");
                    }
                }
            }

            if (validationResults.Count > 0)
            {
                var errorMessage = $"Service validation failed:\n{string.Join("\n", validationResults)}";
                Debug.LogError(errorMessage);
                OnGeneticsError?.Invoke("Validation", errorMessage);
            }
            else if (_enableOrchestrationLogging)
            {
                Debug.Log("All genetics services validated successfully");
            }
        }

        private void ShutdownOrchestrator()
        {
            if (!_isInitialized) return;

            try
            {
                DisconnectServiceEvents();
                ShutdownServices();
                ClearData();

                _isInitialized = false;

                if (_enableOrchestrationLogging)
                    Debug.Log("CannabisGeneticsOrchestrator shutdown completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during orchestrator shutdown: {ex.Message}");
            }
        }

        private void DisconnectServiceEvents()
        {
            if (_breedingSystemService != null)
            {
                BreedingSystemService.OnBreedingCompleted -= HandleBreedingCompleted;
                BreedingSystemService.OnBreedingFailed -= HandleBreedingFailed;
            }

            if (_phenotypeExpressionService != null)
            {
                PhenotypeExpressionService.OnPhenotypeCalculated -= HandlePhenotypeCalculated;
                PhenotypeExpressionService.OnExpressionError -= HandlePhenotypeError;
            }

            if (_geneticDataService != null)
            {
                GeneticDataService.OnGenotypeAdded -= HandleGenotypeAdded;
                GeneticDataService.OnDataError -= HandleDataError;
            }

            if (_geneticsAnalyticsService != null)
            {
                GeneticsAnalyticsService.OnAnalyticsAlert -= HandleAnalyticsAlert;
            }
        }

        private void ShutdownServices()
        {
            foreach (var service in _services.Values)
            {
                if (service is IService serviceInterface)
                {
                    serviceInterface.Shutdown();
                }
            }
            _services.Clear();
        }

        private void ClearData()
        {
            _activeGenotypes.Clear();
            _activePhenotypes.Clear();
            _pendingOperations.Clear();
            _operationQueue.Clear();
        }

        #endregion

        #region High-Level Genetics Operations

        /// <summary>
        /// Create a new genotype with specified traits
        /// </summary>
        public CannabisGenotype CreateGenotype(string strainName, Dictionary<string, float> traitValues = null)
        {
            try
            {
                var operation = new GeneticsOperation
                {
                    OperationId = Guid.NewGuid().ToString(),
                    OperationType = GeneticsOperationType.CreateGenotype,
                    Parameters = new Dictionary<string, object>
                    {
                        ["strainName"] = strainName,
                        ["traitValues"] = traitValues ?? new Dictionary<string, float>()
                    },
                    StartTime = DateTime.Now
                };

                QueueOperation(operation);
                return ProcessCreateGenotype(operation);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error creating genotype: {ex.Message}");
                OnGeneticsError?.Invoke("CreateGenotype", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Breed two genotypes to create offspring
        /// </summary>
        public BreedingResult BreedGenotypes(CannabisGenotype parent1, CannabisGenotype parent2, BreedingMethod method = BreedingMethod.StandardCross)
        {
            try
            {
                var operation = new GeneticsOperation
                {
                    OperationId = Guid.NewGuid().ToString(),
                    OperationType = GeneticsOperationType.Breeding,
                    Parameters = new Dictionary<string, object>
                    {
                        ["parent1"] = parent1,
                        ["parent2"] = parent2,
                        ["method"] = method
                    },
                    StartTime = DateTime.Now
                };

                QueueOperation(operation);
                return ProcessBreeding(operation);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error breeding genotypes: {ex.Message}");
                OnGeneticsError?.Invoke("Breeding", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Express phenotype from genotype and environmental conditions
        /// </summary>
        public CannabisPhenotype ExpressPhenotype(CannabisGenotype genotype, EnvironmentalConditions conditions)
        {
            try
            {
                var operation = new GeneticsOperation
                {
                    OperationId = Guid.NewGuid().ToString(),
                    OperationType = GeneticsOperationType.PhenotypeExpression,
                    Parameters = new Dictionary<string, object>
                    {
                        ["genotype"] = genotype,
                        ["conditions"] = conditions
                    },
                    StartTime = DateTime.Now
                };

                QueueOperation(operation);
                return ProcessPhenotypeExpression(operation);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error expressing phenotype: {ex.Message}");
                OnGeneticsError?.Invoke("PhenotypeExpression", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Apply genetics to SpeedTree plant instance
        /// </summary>
        public void ApplyGeneticsToPlant(SpeedTreePlantData plant, CannabisGenotype genotype)
        {
            try
            {
                if (_speedTreeGeneticsService != null)
                {
                    _speedTreeGeneticsService.ApplyGeneticsToSpeedTree(plant, genotype);
                }
                else
                {
                    Debug.LogWarning("SpeedTree Genetics Service not available");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error applying genetics to plant: {ex.Message}");
                OnGeneticsError?.Invoke("ApplyGenetics", ex.Message);
            }
        }

        /// <summary>
        /// Adapt genotype to environmental pressures
        /// </summary>
        public CannabisGenotype AdaptToEnvironment(CannabisGenotype genotype, EnvironmentalConditions conditions, float adaptationStrength = 1f)
        {
            try
            {
                if (_environmentalAdaptationService != null)
                {
                    return _environmentalAdaptationService.ApplyEnvironmentalPressure(genotype, conditions, adaptationStrength);
                }
                else
                {
                    Debug.LogWarning("Environmental Adaptation Service not available");
                    return genotype;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error adapting to environment: {ex.Message}");
                OnGeneticsError?.Invoke("EnvironmentalAdaptation", ex.Message);
                return genotype;
            }
        }

        #endregion

        #region Operation Processing

        private void ProcessOperationQueue()
        {
            if (!_enableBatchProcessing) return;

            var currentTime = Time.time;
            if (currentTime - _lastServiceUpdate < _serviceUpdateInterval) return;

            int processedCount = 0;
            while (_operationQueue.Count > 0 && processedCount < _maxOperationsPerFrame)
            {
                var operation = _operationQueue.Dequeue();
                ProcessOperation(operation);
                processedCount++;
            }

            _lastServiceUpdate = currentTime;
        }

        private void QueueOperation(GeneticsOperation operation)
        {
            _operationQueue.Enqueue(operation);
            _pendingOperations[operation.OperationId] = operation;
            _totalOperations++;
        }

        private void ProcessOperation(GeneticsOperation operation)
        {
            try
            {
                switch (operation.OperationType)
                {
                    case GeneticsOperationType.CreateGenotype:
                        ProcessCreateGenotype(operation);
                        break;
                    case GeneticsOperationType.Breeding:
                        ProcessBreeding(operation);
                        break;
                    case GeneticsOperationType.PhenotypeExpression:
                        ProcessPhenotypeExpression(operation);
                        break;
                    case GeneticsOperationType.EnvironmentalAdaptation:
                        ProcessEnvironmentalAdaptation(operation);
                        break;
                    default:
                        Debug.LogWarning($"Unknown operation type: {operation.OperationType}");
                        break;
                }

                _successfulOperations++;
                UpdateOperationMetrics(operation);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing operation {operation.OperationId}: {ex.Message}");
                OnGeneticsError?.Invoke(operation.OperationType.ToString(), ex.Message);
            }
            finally
            {
                _pendingOperations.Remove(operation.OperationId);
            }
        }

        private CannabisGenotype ProcessCreateGenotype(GeneticsOperation operation)
        {
            var strainName = (string)operation.Parameters["strainName"];
            var traitValues = (Dictionary<string, float>)operation.Parameters["traitValues"];

            CannabisGenotype genotype = null;

            if (_geneticAlgorithmService != null)
            {
                genotype = _geneticAlgorithmService.GenerateFounderGenotype(strainName);
                
                // Apply custom trait values if provided
                if (traitValues.Count > 0)
                {
                    genotype = _geneticAlgorithmService.ApplyGeneticModifications(genotype, traitValues);
                }
            }
            else
            {
                // Fallback: create basic genotype
                genotype = new CannabisGenotype(strainName, 1);
            }

            if (genotype != null)
            {
                _activeGenotypes[genotype.GenotypeId] = genotype;
                OnGenotypeCreated?.Invoke(genotype);
            }

            return genotype;
        }

        private BreedingResult ProcessBreeding(GeneticsOperation operation)
        {
            var parent1 = (CannabisGenotype)operation.Parameters["parent1"];
            var parent2 = (CannabisGenotype)operation.Parameters["parent2"];
            var method = (BreedingMethod)operation.Parameters["method"];

            BreedingResult result = null;

            if (_breedingSystemService != null)
            {
                result = _breedingSystemService.PerformBreeding(parent1, parent2, method);
            }
            else
            {
                Debug.LogWarning("Breeding System Service not available");
            }

            return result;
        }

        private CannabisPhenotype ProcessPhenotypeExpression(GeneticsOperation operation)
        {
            var genotype = (CannabisGenotype)operation.Parameters["genotype"];
            var conditions = (EnvironmentalConditions)operation.Parameters["conditions"];

            CannabisPhenotype phenotype = null;

            if (_phenotypeExpressionService != null)
            {
                phenotype = _phenotypeExpressionService.CalculatePhenotype(genotype, conditions);
            }
            else
            {
                Debug.LogWarning("Phenotype Expression Service not available");
            }

            if (phenotype != null)
            {
                _activePhenotypes[phenotype.PhenotypeId] = phenotype;
                OnPhenotypeExpressed?.Invoke(phenotype);
            }

            return phenotype;
        }

        private CannabisGenotype ProcessEnvironmentalAdaptation(GeneticsOperation operation)
        {
            var genotype = (CannabisGenotype)operation.Parameters["genotype"];
            var conditions = (EnvironmentalConditions)operation.Parameters["conditions"];
            var strength = (float)operation.Parameters["adaptationStrength"];

            CannabisGenotype adaptedGenotype = genotype;

            if (_environmentalAdaptationService != null)
            {
                adaptedGenotype = _environmentalAdaptationService.ApplyEnvironmentalPressure(genotype, conditions, strength);
            }

            return adaptedGenotype;
        }

        #endregion

        #region Service Updates

        private void UpdateServices()
        {
            // Update service coordination here if needed
        }

        private void UpdateMetrics()
        {
            _orchestrationMetrics.ActiveGenotypes = _activeGenotypes.Count;
            _orchestrationMetrics.ActivePhenotypes = _activePhenotypes.Count;
            _orchestrationMetrics.PendingOperations = _operationQueue.Count;
            _orchestrationMetrics.TotalOperations = _totalOperations;
            _orchestrationMetrics.SuccessfulOperations = _successfulOperations;
            _orchestrationMetrics.SuccessRate = SuccessRate;
            _orchestrationMetrics.AverageOperationTime = _averageOperationTime;
            _orchestrationMetrics.LastUpdate = DateTime.Now;

            OnMetricsUpdated?.Invoke(_orchestrationMetrics);
        }

        private void UpdateOperationMetrics(GeneticsOperation operation)
        {
            var operationTime = (float)(DateTime.Now - operation.StartTime).TotalSeconds;
            _averageOperationTime = (_averageOperationTime + operationTime) / 2f;
        }

        #endregion

        #region Event Handlers

        private void HandleBreedingCompleted(BreedingResult result)
        {
            if (result?.Offspring != null)
            {
                foreach (var offspring in result.Offspring)
                {
                    _activeGenotypes[offspring.GenotypeId] = offspring;
                }
            }
            OnBreedingCompleted?.Invoke(result);
        }

        private void HandleBreedingFailed(string errorMessage)
        {
            OnGeneticsError?.Invoke("Breeding", errorMessage);
        }

        private void HandlePhenotypeCalculated(CannabisPhenotype phenotype)
        {
            _activePhenotypes[phenotype.PhenotypeId] = phenotype;
            OnPhenotypeExpressed?.Invoke(phenotype);
        }

        private void HandlePhenotypeError(string errorMessage)
        {
            OnGeneticsError?.Invoke("Phenotype", errorMessage);
        }

        private void HandleGenotypeAdded(CannabisGenotype genotype)
        {
            _activeGenotypes[genotype.GenotypeId] = genotype;
            OnGenotypeCreated?.Invoke(genotype);
        }

        private void HandleDataError(string errorMessage)
        {
            OnGeneticsError?.Invoke("Data", errorMessage);
        }

        private void HandleAnalyticsAlert(AnalyticsAlert alert)
        {
            if (_enableOrchestrationLogging)
                Debug.Log($"Analytics Alert: {alert.AlertType} - {alert.Message}");
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get active genotype by ID
        /// </summary>
        public CannabisGenotype GetGenotype(string genotypeId)
        {
            return _activeGenotypes.TryGetValue(genotypeId, out var genotype) ? genotype : null;
        }

        /// <summary>
        /// Get active phenotype by ID
        /// </summary>
        public CannabisPhenotype GetPhenotype(string phenotypeId)
        {
            return _activePhenotypes.TryGetValue(phenotypeId, out var phenotype) ? phenotype : null;
        }

        /// <summary>
        /// Get all active genotypes
        /// </summary>
        public List<CannabisGenotype> GetAllGenotypes()
        {
            return _activeGenotypes.Values.ToList();
        }

        /// <summary>
        /// Get all active phenotypes
        /// </summary>
        public List<CannabisPhenotype> GetAllPhenotypes()
        {
            return _activePhenotypes.Values.ToList();
        }

        /// <summary>
        /// Get specific service
        /// </summary>
        public T GetService<T>() where T : MonoBehaviour
        {
            return _services.TryGetValue(typeof(T), out var service) ? service as T : null;
        }

        /// <summary>
        /// Check if specific service is available
        /// </summary>
        public bool HasService<T>() where T : MonoBehaviour
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Get orchestration metrics
        /// </summary>
        public GeneticsOrchestrationMetrics GetMetrics()
        {
            return _orchestrationMetrics;
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Genetics operation for queue processing
    /// </summary>
    [System.Serializable]
    public class GeneticsOperation
    {
        public string OperationId = "";
        public GeneticsOperationType OperationType = GeneticsOperationType.CreateGenotype;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public DateTime StartTime = DateTime.Now;
        public DateTime CompletionTime = DateTime.MinValue;
        public bool IsCompleted = false;
        public string ErrorMessage = "";
    }

    /// <summary>
    /// Types of genetics operations
    /// </summary>
    public enum GeneticsOperationType
    {
        CreateGenotype,
        Breeding,
        PhenotypeExpression,
        EnvironmentalAdaptation,
        Mutation,
        Analysis
    }

    /// <summary>
    /// Orchestration metrics
    /// </summary>
    [System.Serializable]
    public class GeneticsOrchestrationMetrics
    {
        public int ActiveGenotypes = 0;
        public int ActivePhenotypes = 0;
        public int PendingOperations = 0;
        public int TotalOperations = 0;
        public int SuccessfulOperations = 0;
        public float SuccessRate = 0f;
        public float AverageOperationTime = 0f;
        public DateTime LastUpdate = DateTime.Now;
    }

    /// <summary>
    /// Service interface for validation
    /// </summary>
    public interface IService
    {
        bool IsInitialized { get; }
        string ServiceName { get; }
        void Initialize();
        void Shutdown();
    }

    #endregion
}