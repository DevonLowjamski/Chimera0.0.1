using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Environment;
using EnvironmentalConditions = ProjectChimera.Data.Environment.EnvironmentalConditions;

namespace ProjectChimera.Systems.SpeedTree
{
    /// <summary>
    /// Environmental Adaptation Service - Handles environmental pressure adaptation, epigenetic changes, and adaptation tracking
    /// Extracted from CannabisGeneticsEngine to provide focused environmental adaptation functionality
    /// Manages how cannabis plants adapt to environmental pressures over time, including epigenetic modifications
    /// Implements scientific adaptation mechanisms: phenotypic plasticity, epigenetic inheritance, and stress response
    /// </summary>
    public class EnvironmentalAdaptationService : MonoBehaviour
    {
        [Header("Adaptation Configuration")]
        [SerializeField] private bool _enableEnvironmentalAdaptation = true;
        [SerializeField] private bool _enableEpigenetics = true;
        [SerializeField] private bool _enableStressResponse = true;
        [SerializeField] private bool _enableAdaptationLogging = false;

        [Header("Adaptation Parameters")]
        [SerializeField] private float _adaptationRate = 0.01f;
        [SerializeField] private int _generationsForAdaptation = 5;
        [SerializeField] private float _epigeneticModificationRate = 0.005f;
        [SerializeField] private float _stressThreshold = 0.7f;

        [Header("Environmental Response")]
        [SerializeField] private float _temperatureAdaptationRate = 0.02f;
        [SerializeField] private float _humidityAdaptationRate = 0.015f;
        [SerializeField] private float _lightAdaptationRate = 0.01f;
        [SerializeField] private float _co2AdaptationRate = 0.008f;

        [Header("Adaptation Tracking")]
        [SerializeField] private bool _enableAdaptationTracking = true;
        [SerializeField] private bool _enableAdaptationHistory = true;
        [SerializeField] private int _maxAdaptationHistory = 1000;
        [SerializeField] private bool _enableAdaptationAnalytics = true;

        // Service state
        private bool _isInitialized = false;
        private bool _adaptationManagerEnabled = true;
        private ScriptableObject _adaptationConfig;

        // Adaptation tracking data
        private Dictionary<string, EnvironmentalAdaptation> _adaptationData = new Dictionary<string, EnvironmentalAdaptation>();
        private Dictionary<string, List<AdaptationHistoryEntry>> _adaptationHistory = new Dictionary<string, List<AdaptationHistoryEntry>>();
        private Dictionary<string, EpigeneticProfile> _epigeneticProfiles = new Dictionary<string, EpigeneticProfile>();

        // Environmental condition tracking
        private Dictionary<string, EnvironmentalConditions> _genotypeEnvironments = new Dictionary<string, EnvironmentalConditions>();
        private Dictionary<string, EnvironmentalStressProfile> _stressProfiles = new Dictionary<string, EnvironmentalStressProfile>();

        // Analytics data
        private EnvironmentalAdaptationAnalytics _adaptationAnalytics = new EnvironmentalAdaptationAnalytics();
        private Dictionary<string, float> _adaptationRates = new Dictionary<string, float>();
        private Dictionary<string, int> _adaptationCounts = new Dictionary<string, int>();

        // Performance tracking
        private int _totalAdaptations = 0;
        private int _totalEpigeneticChanges = 0;
        private float _lastUpdateTime = 0f;
        private float _adaptationProcessTime = 0f;

        // Events
        public static event Action<EnvironmentalAdaptation> OnAdaptationOccurred;
        public static event Action<EpigeneticModification> OnEpigeneticModification;
        public static event Action<EnvironmentalStressResponse> OnStressResponse;
        public static event Action<EnvironmentalAdaptationAnalytics> OnAdaptationAnalyticsUpdated;
        public static event Action<string> OnAdaptationError;

        #region Service Interface

        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Environmental Adaptation Service";
        public int TotalAdaptations => _totalAdaptations;
        public int TotalEpigeneticChanges => _totalEpigeneticChanges;
        public int TrackedGenotypes => _adaptationData.Count;
        public int ActiveAdaptations => _adaptationData.Values.Count(a => a.AdaptationProgress > 0f);

        public void Initialize(ScriptableObject adaptationConfig = null)
        {
            InitializeService(adaptationConfig);
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
                UpdateEnvironmentalAdaptation();
                UpdateAdaptationAnalytics();
            }
        }

        private void InitializeDataStructures()
        {
            _adaptationData = new Dictionary<string, EnvironmentalAdaptation>();
            _adaptationHistory = new Dictionary<string, List<AdaptationHistoryEntry>>();
            _epigeneticProfiles = new Dictionary<string, EpigeneticProfile>();
            _genotypeEnvironments = new Dictionary<string, EnvironmentalConditions>();
            _stressProfiles = new Dictionary<string, EnvironmentalStressProfile>();
            _adaptationAnalytics = new EnvironmentalAdaptationAnalytics();
            _adaptationRates = new Dictionary<string, float>();
            _adaptationCounts = new Dictionary<string, int>();
        }

        public void InitializeService(ScriptableObject adaptationConfig = null)
        {
            if (_isInitialized)
            {
                if (_enableAdaptationLogging)
                    Debug.LogWarning("EnvironmentalAdaptationService already initialized");
                return;
            }

            try
            {
                _adaptationConfig = adaptationConfig;
                _adaptationManagerEnabled = _enableEnvironmentalAdaptation;

                InitializeAdaptationParameters();
                InitializeEpigeneticSystem();
                InitializeStressResponseSystem();
                InitializeAnalytics();
                
                _isInitialized = true;
                
                if (_enableAdaptationLogging)
                    Debug.Log("EnvironmentalAdaptationService initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to initialize EnvironmentalAdaptationService: {ex.Message}");
                OnAdaptationError?.Invoke($"Service initialization failed: {ex.Message}");
            }
        }

        public void ShutdownService()
        {
            if (!_isInitialized) return;

            try
            {
                // Save adaptation data
                if (_enableAdaptationTracking)
                {
                    SaveAdaptationData();
                }
                
                // Clear all data
                ClearAllData();
                
                _isInitialized = false;
                
                if (_enableAdaptationLogging)
                    Debug.Log("EnvironmentalAdaptationService shutdown completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during EnvironmentalAdaptationService shutdown: {ex.Message}");
            }
        }

        #endregion

        #region Environmental Adaptation

        /// <summary>
        /// Process environmental adaptation for a genotype under specific conditions
        /// </summary>
        public void ProcessAdaptation(CannabisGenotype genotype, EnvironmentalConditions conditions)
        {
            if (!_isInitialized || genotype == null || conditions == null)
                return;

            try
            {
                var startTime = DateTime.Now;
                
                if (_enableAdaptationLogging)
                    Debug.Log($"Processing adaptation for genotype: {genotype.StrainName}");

                // Update genotype environment tracking
                _genotypeEnvironments[genotype.GenotypeId] = conditions;

                // Process environmental adaptation
                if (_enableEnvironmentalAdaptation)
                {
                    ProcessGenotypeAdaptation(genotype, conditions);
                }

                // Process epigenetic changes
                if (_enableEpigenetics)
                {
                    ProcessEpigeneticChanges(genotype, conditions);
                }

                // Process stress responses
                if (_enableStressResponse)
                {
                    ProcessStressResponse(genotype, conditions);
                }

                var processingTime = (float)(DateTime.Now - startTime).TotalSeconds;
                _adaptationProcessTime = (_adaptationProcessTime + processingTime) / 2f;

                if (_enableAdaptationLogging)
                    Debug.Log($"Adaptation processing completed for {genotype.StrainName} in {processingTime:F3} seconds");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing adaptation: {ex.Message}");
                OnAdaptationError?.Invoke($"Adaptation processing failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Process genotype adaptation to environmental conditions
        /// </summary>
        private void ProcessGenotypeAdaptation(CannabisGenotype genotype, EnvironmentalConditions conditions)
        {
            var adaptationKey = $"{genotype.GenotypeId}_{GetEnvironmentalHash(conditions)}";

            if (!_adaptationData.TryGetValue(adaptationKey, out var adaptation))
            {
                adaptation = new EnvironmentalAdaptation
                {
                    GenotypeId = genotype.GenotypeId,
                    EnvironmentalConditions = conditions,
                    StartDate = DateTime.Now,
                    AdaptationProgress = 0f,
                    AdaptationApplied = false,
                    AdaptationConditions = conditions
                };

                _adaptationData[adaptationKey] = adaptation;
            }

            // Update adaptation progress
            var deltaTime = Time.deltaTime;
            var adaptationIncrement = _adaptationRate * deltaTime;
            
            // Factor in environmental stress for faster adaptation under stress
            var stressLevel = CalculateEnvironmentalStress(conditions);
            if (stressLevel > _stressThreshold)
            {
                adaptationIncrement *= (1f + stressLevel);
            }

            adaptation.AdaptationProgress += adaptationIncrement;
            adaptation.AdaptationProgress = Mathf.Clamp01(adaptation.AdaptationProgress);

            // Apply adaptation when threshold is reached
            if (adaptation.AdaptationProgress > 0.5f && !adaptation.AdaptationApplied)
            {
                ApplyAdaptiveChanges(genotype, adaptation);
                adaptation.AdaptationApplied = true;
                // Adaptation completed successfully
                
                _totalAdaptations++;
                
                OnAdaptationOccurred?.Invoke(adaptation);
                
                // Record adaptation history
                if (_enableAdaptationHistory)
                {
                    RecordAdaptationHistory(genotype, adaptation);
                }
                
                if (_enableAdaptationLogging)
                    Debug.Log($"Adaptation applied to {genotype.StrainName}: {adaptation.AdaptationProgress:F2}");
            }
        }

        /// <summary>
        /// Apply adaptive changes to genotype
        /// </summary>
        private void ApplyAdaptiveChanges(CannabisGenotype genotype, EnvironmentalAdaptation adaptation)
        {
            var conditions = adaptation.EnvironmentalConditions;
            var adaptationStrength = adaptation.AdaptationProgress;

            // Temperature adaptation
            if (Math.Abs(conditions.Temperature - 22f) > 5f)
            {
                ModifyEnvironmentalTolerance(genotype, "heat_tolerance", 
                    conditions.Temperature > 22f ? adaptationStrength * _temperatureAdaptationRate : 0f);
                ModifyEnvironmentalTolerance(genotype, "cold_tolerance", 
                    conditions.Temperature < 22f ? adaptationStrength * _temperatureAdaptationRate : 0f);
            }

            // Humidity adaptation
            if (Math.Abs(conditions.Humidity - 55f) > 15f)
            {
                ModifyEnvironmentalTolerance(genotype, "humidity_tolerance", 
                    adaptationStrength * _humidityAdaptationRate);
            }

            // Light adaptation
            if (Math.Abs(conditions.LightIntensity - 400f) > 200f)
            {
                ModifyEnvironmentalTolerance(genotype, "light_adaptation", 
                    adaptationStrength * _lightAdaptationRate);
            }

            // CO2 adaptation
            if (Math.Abs(conditions.CO2Level - 400f) > 200f)
            {
                ModifyEnvironmentalTolerance(genotype, "co2_utilization", 
                    adaptationStrength * _co2AdaptationRate);
            }

            // Update adaptation analytics
            UpdateAdaptationCounts(conditions);
        }

        /// <summary>
        /// Modify environmental tolerance traits
        /// </summary>
        private void ModifyEnvironmentalTolerance(CannabisGenotype genotype, string toleranceType, float modification)
        {
            if (genotype.Traits == null) return;

            var trait = genotype.Traits.FirstOrDefault(t => t.TraitName.ToLower().Contains(toleranceType.ToLower()));
            if (trait != null)
            {
                trait.ExpressedValue += modification;
                trait.ExpressedValue = Mathf.Clamp(trait.ExpressedValue, 0.1f, 2.0f);
            }
            else
            {
                // Add new tolerance trait
                genotype.Traits.Add(new GeneticTrait
                {
                    TraitName = toleranceType,
                    ExpressedValue = 1.0f + modification,
                    Dominance = TraitDominance.Codominant,
                    HeritabilityIndex = 0.7f,
                    EnvironmentalSensitivity = 0.5f
                });
            }
        }

        #endregion

        #region Epigenetic System

        /// <summary>
        /// Process epigenetic changes based on environmental conditions
        /// </summary>
        private void ProcessEpigeneticChanges(CannabisGenotype genotype, EnvironmentalConditions conditions)
        {
            if (!_epigeneticProfiles.TryGetValue(genotype.GenotypeId, out var profile))
            {
                profile = new EpigeneticProfile
                {
                    GenotypeId = genotype.GenotypeId,
                    Modifications = new Dictionary<string, EpigeneticModification>(),
                    LastUpdate = DateTime.Now
                };
                _epigeneticProfiles[genotype.GenotypeId] = profile;
            }

            // Apply epigenetic modifications based on environmental stress
            ApplyEpigeneticModifications(genotype, conditions, profile);
        }

        /// <summary>
        /// Apply epigenetic modifications based on environmental conditions
        /// </summary>
        private void ApplyEpigeneticModifications(CannabisGenotype genotype, EnvironmentalConditions conditions, EpigeneticProfile profile)
        {
            var stressLevel = CalculateEnvironmentalStress(conditions);
            
            if (stressLevel > _stressThreshold)
            {
                // Temperature stress triggers heat shock protein expression
                if (Math.Abs(conditions.Temperature - 22f) > 8f)
                {
                    ApplyEpigeneticModification(genotype, profile, "heat_shock_response", 
                        _epigeneticModificationRate * stressLevel, conditions);
                }

                // Light stress triggers photoprotection mechanisms
                if (conditions.LightIntensity > 800f)
                {
                    ApplyEpigeneticModification(genotype, profile, "photoprotection", 
                        _epigeneticModificationRate * stressLevel, conditions);
                }

                // Drought stress triggers water conservation
                if (conditions.Humidity < 40f)
                {
                    ApplyEpigeneticModification(genotype, profile, "drought_response", 
                        _epigeneticModificationRate * stressLevel, conditions);
                }
            }

            profile.LastUpdate = DateTime.Now;
        }

        /// <summary>
        /// Apply specific epigenetic modification
        /// </summary>
        private void ApplyEpigeneticModification(CannabisGenotype genotype, EpigeneticProfile profile, 
            string modificationType, float strength, EnvironmentalConditions conditions)
        {
            if (!profile.Modifications.TryGetValue(modificationType, out var modification))
            {
                modification = new EpigeneticModification
                {
                    ModificationId = Guid.NewGuid().ToString(),
                    ModificationType = modificationType,
                    Strength = 0f,
                    AppliedTime = DateTime.Now,
                    TriggeringConditions = conditions,
                    IsActive = true
                };
                profile.Modifications[modificationType] = modification;
            }

            modification.Strength += strength;
            modification.Strength = Mathf.Clamp01(modification.Strength);
            modification.LastUpdate = DateTime.Now;

            // Update genotype's epigenetic modifications
            if (genotype.EpigeneticModifications == null)
            {
                genotype.EpigeneticModifications = new Dictionary<string, float>();
            }
            genotype.EpigeneticModifications[modificationType] = modification.Strength;

            _totalEpigeneticChanges++;
            OnEpigeneticModification?.Invoke(modification);

            if (_enableAdaptationLogging)
                Debug.Log($"Applied epigenetic modification {modificationType} to {genotype.StrainName}: {modification.Strength:F3}");
        }

        #endregion

        #region Stress Response System

        /// <summary>
        /// Process stress response to environmental conditions
        /// </summary>
        private void ProcessStressResponse(CannabisGenotype genotype, EnvironmentalConditions conditions)
        {
            var stressLevel = CalculateEnvironmentalStress(conditions);
            
            if (!_stressProfiles.TryGetValue(genotype.GenotypeId, out var stressProfile))
            {
                stressProfile = new EnvironmentalStressProfile
                {
                    GenotypeId = genotype.GenotypeId,
                    StressHistory = new List<StressEvent>(),
                    AdaptationCapacity = 1.0f,
                    RecoveryRate = 0.1f
                };
                _stressProfiles[genotype.GenotypeId] = stressProfile;
            }

            // Record stress event if threshold exceeded
            if (stressLevel > _stressThreshold)
            {
                var stressEvent = new StressEvent
                {
                    StressType = DetermineStressType(conditions),
                    StressLevel = stressLevel,
                    EventTime = DateTime.Now,
                    Conditions = conditions,
                    Duration = Time.deltaTime
                };

                stressProfile.StressHistory.Add(stressEvent);
                
                // Limit stress history size
                if (stressProfile.StressHistory.Count > 100)
                {
                    stressProfile.StressHistory.RemoveAt(0);
                }

                // Create stress response
                var stressResponse = new EnvironmentalStressResponse
                {
                    ResponseId = Guid.NewGuid().ToString(),
                    GenotypeId = genotype.GenotypeId,
                    StressEvent = stressEvent,
                    ResponseStrength = CalculateStressResponseStrength(stressProfile, stressLevel),
                    ResponseTime = DateTime.Now
                };

                ApplyStressResponse(genotype, stressResponse);
                OnStressResponse?.Invoke(stressResponse);
            }
        }

        /// <summary>
        /// Determine primary stress type from conditions
        /// </summary>
        private StressType DetermineStressType(EnvironmentalConditions conditions)
        {
            if (Math.Abs(conditions.Temperature - 22f) > 8f)
                return conditions.Temperature > 22f ? StressType.Heat : StressType.Cold;
                
            if (conditions.Humidity < 40f)
                return StressType.Drought;
                
            if (conditions.LightIntensity > 800f)
                return StressType.Light;
                
            if (conditions.CO2Level < 300f)
                return StressType.CO2;
                
            return StressType.General;
        }

        /// <summary>
        /// Calculate stress response strength based on stress profile and current stress
        /// </summary>
        private float CalculateStressResponseStrength(EnvironmentalStressProfile profile, float currentStress)
        {
            // Base response strength
            float responseStrength = currentStress * profile.AdaptationCapacity;
            
            // Factor in previous stress exposure (hormesis effect)
            int recentStressEvents = profile.StressHistory.Count(s => 
                (DateTime.Now - s.EventTime).TotalHours < 24);
                
            if (recentStressEvents > 3)
            {
                responseStrength *= 0.8f; // Reduced response due to stress exhaustion
            }
            else if (recentStressEvents > 1)
            {
                responseStrength *= 1.2f; // Enhanced response due to priming
            }

            return Mathf.Clamp01(responseStrength);
        }

        /// <summary>
        /// Apply stress response modifications to genotype
        /// </summary>
        private void ApplyStressResponse(CannabisGenotype genotype, EnvironmentalStressResponse response)
        {
            var stressType = response.StressEvent.StressType;
            var responseStrength = response.ResponseStrength;

            // Apply stress-specific modifications
            switch (stressType)
            {
                case StressType.Heat:
                    ModifyEnvironmentalTolerance(genotype, "heat_tolerance", responseStrength * 0.1f);
                    break;
                case StressType.Cold:
                    ModifyEnvironmentalTolerance(genotype, "cold_tolerance", responseStrength * 0.1f);
                    break;
                case StressType.Drought:
                    ModifyEnvironmentalTolerance(genotype, "drought_tolerance", responseStrength * 0.1f);
                    break;
                case StressType.Light:
                    ModifyEnvironmentalTolerance(genotype, "light_adaptation", responseStrength * 0.1f);
                    break;
                case StressType.CO2:
                    ModifyEnvironmentalTolerance(genotype, "co2_utilization", responseStrength * 0.1f);
                    break;
            }
        }

        #endregion

        #region Utility Methods

        private void UpdateEnvironmentalAdaptation()
        {
            if (!_enableEnvironmentalAdaptation) return;

            // Process adaptation for all tracked genotypes
            var currentTime = Time.time;
            if (currentTime - _lastUpdateTime >= 1f)
            {
                foreach (var adaptation in _adaptationData.Values.Where(a => !a.AdaptationApplied))
                {
                    // Continue adaptation progress for incomplete adaptations
                    if (adaptation.AdaptationProgress < 1f)
                    {
                        adaptation.AdaptationProgress += _adaptationRate * (currentTime - _lastUpdateTime);
                        adaptation.AdaptationProgress = Mathf.Clamp01(adaptation.AdaptationProgress);
                    }
                }
                
                _lastUpdateTime = currentTime;
            }
        }

        private string GetEnvironmentalHash(EnvironmentalConditions conditions)
        {
            // Create a hash representing the environmental conditions for adaptation tracking
            return $"{conditions.Temperature:F1}_{conditions.Humidity:F1}_{conditions.LightIntensity:F0}_{conditions.CO2Level:F0}";
        }

        private float CalculateEnvironmentalStress(EnvironmentalConditions conditions)
        {
            float stressSum = 0f;
            int stressFactors = 0;

            // Temperature stress
            var tempStress = Math.Abs(conditions.Temperature - 22f) / 10f;
            stressSum += Mathf.Clamp01(tempStress);
            stressFactors++;

            // Humidity stress
            var humidityStress = Math.Abs(conditions.Humidity - 55f) / 30f;
            stressSum += Mathf.Clamp01(humidityStress);
            stressFactors++;

            // Light stress
            var lightStress = Math.Abs(conditions.LightIntensity - 400f) / 400f;
            stressSum += Mathf.Clamp01(lightStress);
            stressFactors++;

            // CO2 stress
            var co2Stress = Math.Abs(conditions.CO2Level - 400f) / 400f;
            stressSum += Mathf.Clamp01(co2Stress);
            stressFactors++;

            return stressFactors > 0 ? stressSum / stressFactors : 0f;
        }

        private void RecordAdaptationHistory(CannabisGenotype genotype, EnvironmentalAdaptation adaptation)
        {
            if (!_adaptationHistory.TryGetValue(genotype.GenotypeId, out var history))
            {
                history = new List<AdaptationHistoryEntry>();
                _adaptationHistory[genotype.GenotypeId] = history;
            }

            var historyEntry = new AdaptationHistoryEntry
            {
                AdaptationId = Guid.NewGuid().ToString(),
                GenotypeId = genotype.GenotypeId,
                EnvironmentalConditions = adaptation.EnvironmentalConditions,
                AdaptationTime = DateTime.Now,
                AdaptationType = DetermineAdaptationType(adaptation.EnvironmentalConditions),
                AdaptationStrength = adaptation.AdaptationProgress
            };

            history.Add(historyEntry);

            // Limit history size
            if (history.Count > _maxAdaptationHistory)
            {
                history.RemoveAt(0);
            }
        }

        private string DetermineAdaptationType(EnvironmentalConditions conditions)
        {
            if (Math.Abs(conditions.Temperature - 22f) > 5f)
                return conditions.Temperature > 22f ? "Heat Adaptation" : "Cold Adaptation";
                
            if (Math.Abs(conditions.Humidity - 55f) > 15f)
                return "Humidity Adaptation";
                
            if (Math.Abs(conditions.LightIntensity - 400f) > 200f)
                return "Light Adaptation";
                
            if (Math.Abs(conditions.CO2Level - 400f) > 200f)
                return "CO2 Adaptation";
                
            return "General Adaptation";
        }

        private void UpdateAdaptationCounts(EnvironmentalConditions conditions)
        {
            var adaptationType = DetermineAdaptationType(conditions);
            _adaptationCounts[adaptationType] = _adaptationCounts.GetValueOrDefault(adaptationType, 0) + 1;
        }

        private void UpdateAdaptationAnalytics()
        {
            if (!_enableAdaptationAnalytics) return;

            var currentTime = DateTime.Now;
            if (_adaptationAnalytics.LastAnalyticsUpdate == default ||
                (currentTime - _adaptationAnalytics.LastAnalyticsUpdate).TotalMinutes >= 1)
            {
                UpdateAnalyticsData();
                _adaptationAnalytics.LastAnalyticsUpdate = currentTime;
                
                OnAdaptationAnalyticsUpdated?.Invoke(_adaptationAnalytics);
            }
        }

        private void UpdateAnalyticsData()
        {
            _adaptationAnalytics.TotalAdaptations = _totalAdaptations;
            _adaptationAnalytics.TotalEpigeneticChanges = _totalEpigeneticChanges;
            _adaptationAnalytics.TrackedGenotypes = _adaptationData.Count;
            _adaptationAnalytics.ActiveAdaptations = ActiveAdaptations;
            _adaptationAnalytics.AdaptationProcessTime = _adaptationProcessTime;
            _adaptationAnalytics.AdaptationCounts = new Dictionary<string, int>(_adaptationCounts);
        }

        private void InitializeAdaptationParameters()
        {
            // Initialize adaptation parameter mappings and thresholds
        }

        private void InitializeEpigeneticSystem()
        {
            // Initialize epigenetic modification patterns and inheritance rules
        }

        private void InitializeStressResponseSystem()
        {
            // Initialize stress response mechanisms and recovery patterns
        }

        private void InitializeAnalytics()
        {
            _adaptationAnalytics = new EnvironmentalAdaptationAnalytics
            {
                TotalAdaptations = 0,
                TotalEpigeneticChanges = 0,
                TrackedGenotypes = 0,
                ActiveAdaptations = 0,
                AdaptationProcessTime = 0f,
                AdaptationCounts = new Dictionary<string, int>(),
                LastAnalyticsUpdate = DateTime.Now
            };
        }

        private void SaveAdaptationData()
        {
            // Save adaptation analytics and history - would integrate with data persistence service
            if (_enableAdaptationLogging)
                Debug.Log($"Saving adaptation data: {_adaptationData.Count} adaptations, {_totalEpigeneticChanges} epigenetic changes");
        }

        private void ClearAllData()
        {
            _adaptationData.Clear();
            _adaptationHistory.Clear();
            _epigeneticProfiles.Clear();
            _genotypeEnvironments.Clear();
            _stressProfiles.Clear();
            _adaptationRates.Clear();
            _adaptationCounts.Clear();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Get adaptation data for a genotype
        /// </summary>
        public EnvironmentalAdaptation GetAdaptationData(string genotypeId, EnvironmentalConditions conditions)
        {
            var adaptationKey = $"{genotypeId}_{GetEnvironmentalHash(conditions)}";
            return _adaptationData.TryGetValue(adaptationKey, out var adaptation) ? adaptation : null;
        }

        /// <summary>
        /// Get epigenetic profile for a genotype
        /// </summary>
        public EpigeneticProfile GetEpigeneticProfile(string genotypeId)
        {
            return _epigeneticProfiles.TryGetValue(genotypeId, out var profile) ? profile : null;
        }

        /// <summary>
        /// Get stress profile for a genotype
        /// </summary>
        public EnvironmentalStressProfile GetStressProfile(string genotypeId)
        {
            return _stressProfiles.TryGetValue(genotypeId, out var profile) ? profile : null;
        }

        /// <summary>
        /// Get adaptation history for a genotype
        /// </summary>
        public List<AdaptationHistoryEntry> GetAdaptationHistory(string genotypeId)
        {
            return _adaptationHistory.TryGetValue(genotypeId, out var history) ? 
                   new List<AdaptationHistoryEntry>(history) : new List<AdaptationHistoryEntry>();
        }

        /// <summary>
        /// Get adaptation analytics
        /// </summary>
        public EnvironmentalAdaptationAnalytics GetAdaptationAnalytics()
        {
            return _adaptationAnalytics;
        }

        /// <summary>
        /// Update adaptation settings at runtime
        /// </summary>
        public void UpdateAdaptationSettings(bool enableAdaptation, bool enableEpigenetics, float adaptationRate, float epigeneticRate)
        {
            _enableEnvironmentalAdaptation = enableAdaptation;
            _enableEpigenetics = enableEpigenetics;
            _adaptationRate = adaptationRate;
            _epigeneticModificationRate = epigeneticRate;
            
            if (_enableAdaptationLogging)
                Debug.Log($"Adaptation settings updated: Adaptation={enableAdaptation}, Epigenetics={enableEpigenetics}, Rate={adaptationRate}, EpiRate={epigeneticRate}");
        }

        /// <summary>
        /// Clear adaptation data for specific genotype
        /// </summary>
        public void ClearAdaptationData(string genotypeId)
        {
            var keysToRemove = _adaptationData.Keys.Where(k => k.StartsWith(genotypeId)).ToList();
            foreach (var key in keysToRemove)
            {
                _adaptationData.Remove(key);
            }
            
            _adaptationHistory.Remove(genotypeId);
            _epigeneticProfiles.Remove(genotypeId);
            _stressProfiles.Remove(genotypeId);
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            ShutdownService();
        }

        /// <summary>
        /// Apply environmental pressure to genotype with adaptation strength
        /// Overloaded method to match the expected signature from CannabisGeneticsOrchestrator
        /// </summary>
        public CannabisGenotype ApplyEnvironmentalPressure(CannabisGenotype genotype, EnvironmentalConditions conditions, float adaptationStrength)
        {
            if (!_isInitialized || genotype == null || conditions == null)
            {
                return genotype;
            }

            try
            {
                // Create a copy of the genotype to avoid modifying the original
                var adaptedGenotype = new CannabisGenotype(genotype.StrainName, genotype.Generation)
                {
                    GenotypeId = genotype.GenotypeId,
                    StrainId = genotype.StrainId,
                    IsFounderStrain = genotype.IsFounderStrain,
                    Origin = genotype.Origin,
                    ParentGenotypes = new List<string>(genotype.ParentGenotypes ?? new List<string>()),
                    CreationDate = genotype.CreationDate
                };
                
                // Copy traits from original genotype
                foreach (var trait in genotype.Traits)
                {
                    adaptedGenotype.AddTrait(trait.TraitName, trait.ExpressedValue, trait.Dominance);
                }
                
                // Process the adaptation (modifies the genotype in place)
                ProcessAdaptation(adaptedGenotype, conditions);
                
                // Apply the adaptation strength modifier to the results
                if (adaptationStrength != 1.0f)
                {
                    // Modify adaptation effects based on strength parameter
                    foreach (var trait in adaptedGenotype.Traits)
                    {
                        // Scale the trait expression change by adaptation strength
                        float baseExpression = genotype.GetTrait(trait.TraitName)?.ExpressedValue ?? trait.ExpressedValue;
                        float change = (trait.ExpressedValue - baseExpression) * adaptationStrength;
                        trait.ExpressedValue = baseExpression + change;
                    }
                }

                return adaptedGenotype;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in ApplyEnvironmentalPressure: {ex.Message}");
                OnAdaptationError?.Invoke($"Environmental pressure application failed: {ex.Message}");
                return genotype;
            }
        }

        #endregion
    }

    // EnvironmentalAdaptationManager class is defined in CannabisGeneticsEngine.cs

    // EnvironmentalAdaptation class is defined in EnvironmentalDataStructures.cs

    /// <summary>
    /// Epigenetic profile for tracking modifications
    /// </summary>
    [System.Serializable]
    public class EpigeneticProfile
    {
        public string GenotypeId = "";
        public Dictionary<string, EpigeneticModification> Modifications = new Dictionary<string, EpigeneticModification>();
        public DateTime LastUpdate = DateTime.Now;
        public int TotalModifications => Modifications.Count;
        public int ActiveModifications => Modifications.Values.Count(m => m.IsActive);
    }

    /// <summary>
    /// Individual epigenetic modification
    /// </summary>
    [System.Serializable]
    public class EpigeneticModification
    {
        public string ModificationId = "";
        public string ModificationType = "";
        public float Strength = 0f;
        public DateTime AppliedTime = DateTime.Now;
        public DateTime LastUpdate = DateTime.Now;
        public EnvironmentalConditions TriggeringConditions = new EnvironmentalConditions();
        public bool IsActive = true;
        public bool IsInheritable = false;
        public int GenerationPersistence = 1;
    }

    /// <summary>
    /// Environmental stress profile
    /// </summary>
    [System.Serializable]
    public class EnvironmentalStressProfile
    {
        public string GenotypeId = "";
        public List<StressEvent> StressHistory = new List<StressEvent>();
        public float AdaptationCapacity = 1.0f;
        public float RecoveryRate = 0.1f;
        public float StressResistance = 0.5f;
        public DateTime LastStressEvent = DateTime.Now;
    }

    /// <summary>
    /// Individual stress event
    /// </summary>
    [System.Serializable]
    public class StressEvent
    {
        public StressType StressType = StressType.General;
        public float StressLevel = 0f;
        public DateTime EventTime = DateTime.Now;
        public EnvironmentalConditions Conditions = new EnvironmentalConditions();
        public float Duration = 0f;
        public bool RecoveryComplete = false;
    }

    /// <summary>
    /// Environmental stress response
    /// </summary>
    [System.Serializable]
    public class EnvironmentalStressResponse
    {
        public string ResponseId = "";
        public string GenotypeId = "";
        public StressEvent StressEvent = new StressEvent();
        public float ResponseStrength = 0f;
        public DateTime ResponseTime = DateTime.Now;
        public Dictionary<string, float> ResponseEffects = new Dictionary<string, float>();
    }

    /// <summary>
    /// Adaptation history entry
    /// </summary>
    [System.Serializable]
    public class AdaptationHistoryEntry
    {
        public string AdaptationId = "";
        public string GenotypeId = "";
        public EnvironmentalConditions EnvironmentalConditions = new EnvironmentalConditions();
        public DateTime AdaptationTime = DateTime.Now;
        public string AdaptationType = "";
        public float AdaptationStrength = 0f;
    }

    /// <summary>
    /// Environmental adaptation analytics
    /// </summary>
    [System.Serializable]
    public class EnvironmentalAdaptationAnalytics
    {
        public int TotalAdaptations = 0;
        public int TotalEpigeneticChanges = 0;
        public int TrackedGenotypes = 0;
        public int ActiveAdaptations = 0;
        public float AdaptationProcessTime = 0f;
        public Dictionary<string, int> AdaptationCounts = new Dictionary<string, int>();
        public DateTime LastAnalyticsUpdate = DateTime.Now;
    }

    /// <summary>
    /// Types of environmental stress
    /// </summary>
    public enum StressType
    {
        General,
        Heat,
        Cold,
        Drought,
        Flood,
        Light,
        Darkness,
        CO2,
        Nutrient,
        Disease,
        Physical
    }
}