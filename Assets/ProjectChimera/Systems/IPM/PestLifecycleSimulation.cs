using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;
using ProjectChimera.Data.Environment;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Advanced pest lifecycle simulation system for realistic IPM management.
    /// Simulates pest populations, reproduction, lifecycle stages, and environmental responses.
    /// Part of PC017-1a: Create pest simulation and lifecycle management
    /// </summary>
    public class PestLifecycleSimulation : ChimeraManager
    {
        [Header("Simulation Configuration")]
        [SerializeField] private bool _enableRealTimeSimulation = true;
        [SerializeField] private bool _enableEnvironmentalFactors = true;
        [SerializeField] private bool _enableSeasonalVariation = true;
        [SerializeField] private float _simulationSpeed = 1.0f;
        [SerializeField] private float _updateInterval = 5.0f; // seconds
        [SerializeField] private int _maxPopulationPerSpecies = 10000;
        
        [Header("Environmental Sensitivity")]
        [SerializeField] private float _temperatureInfluence = 0.8f;
        [SerializeField] private float _humidityInfluence = 0.6f;
        [SerializeField] private float _lightInfluence = 0.4f;
        [SerializeField] private float _co2Influence = 0.3f;
        [SerializeField] private float _nutritionalInfluence = 0.5f;
        
        [Header("Population Dynamics")]
        [SerializeField] private float _carryingCapacityMultiplier = 1.0f;
        [SerializeField] private float _migrationRate = 0.1f;
        [SerializeField] private float _mortalityBaseRate = 0.05f;
        [SerializeField] private bool _enableDensityDependentEffects = true;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onPestLifecycleAdvanced;
        [SerializeField] private SimpleGameEventSO _onPopulationOutbreak;
        [SerializeField] private SimpleGameEventSO _onSpeciesIntroduced;
        [SerializeField] private SimpleGameEventSO _onSpeciesExtinct;
        
        // Core simulation data
        private Dictionary<string, PestPopulation> _activePopulations = new Dictionary<string, PestPopulation>();
        private Dictionary<PestType, PestLifecycleProfile> _lifecycleProfiles = new Dictionary<PestType, PestLifecycleProfile>();
        private Dictionary<string, EnvironmentalZone> _environmentalZones = new Dictionary<string, EnvironmentalZone>();
        private Dictionary<PestType, List<PestInteraction>> _speciesInteractions = new Dictionary<PestType, List<PestInteraction>>();
        
        // Simulation state
        private float _lastUpdateTime = 0f;
        private float _simulationTime = 0f;
        private int _totalPopulationCount = 0;
        private int _activeSpeciesCount = 0;
        private SeasonalState _currentSeason = SeasonalState.Spring;
        
        // Performance tracking
        private SimulationMetrics _metrics;
        private Queue<float> _performanceHistory = new Queue<float>();
        
        // Events
        public System.Action<string, PestPopulation> OnPopulationChanged;
        public System.Action<PestType, LifecycleStage> OnLifecycleAdvanced;
        public System.Action<string, PopulationOutbreakData> OnPopulationOutbreak;
        public System.Action<PestType, string> OnSpeciesIntroduced;
        public System.Action<PestType, string> OnSpeciesExtinct;
        
        // Properties
        public override string ManagerName => "Pest Lifecycle Simulation";
        public override ManagerPriority Priority => ManagerPriority.High;
        public int TotalPopulationCount => _totalPopulationCount;
        public int ActiveSpeciesCount => _activeSpeciesCount;
        public float SimulationTime => _simulationTime;
        public SeasonalState CurrentSeason => _currentSeason;
        public SimulationMetrics Metrics => _metrics;
        
        protected override void OnManagerInitialize()
        {
            InitializeLifecycleProfiles();
            InitializeEnvironmentalZones();
            InitializeSpeciesInteractions();
            
            _metrics = new SimulationMetrics();
            
            LogInfo("Pest Lifecycle Simulation initialized successfully");
        }
        
        protected override void OnManagerUpdate()
        {
            if (!_enableRealTimeSimulation) return;
            
            float currentTime = Time.time;
            float deltaTime = Time.deltaTime * _simulationSpeed;
            
            _simulationTime += deltaTime;
            
            // Update simulation at specified intervals
            if (currentTime - _lastUpdateTime >= _updateInterval)
            {
                UpdateSimulationCycle(deltaTime);
                _lastUpdateTime = currentTime;
            }
            
            UpdateMetrics();
        }
        
        protected override void OnManagerShutdown()
        {
            _activePopulations.Clear();
            _lifecycleProfiles.Clear();
            _environmentalZones.Clear();
            _speciesInteractions.Clear();
            
            LogInfo("Pest Lifecycle Simulation shutdown completed");
        }
        
        /// <summary>
        /// Introduce a new pest species to a specific environmental zone
        /// </summary>
        public bool IntroducePestSpecies(PestType pestType, string zoneId, int initialPopulation, LifecycleStage initialStage = LifecycleStage.Adult)
        {
            if (!_environmentalZones.ContainsKey(zoneId))
            {
                LogError($"Environmental zone {zoneId} not found");
                return false;
            }
            
            if (!_lifecycleProfiles.ContainsKey(pestType))
            {
                LogError($"Lifecycle profile for {pestType} not found");
                return false;
            }
            
            var populationId = $"{zoneId}_{pestType}";
            
            if (_activePopulations.ContainsKey(populationId))
            {
                LogWarning($"Population {populationId} already exists, adding to existing population");
                _activePopulations[populationId].AddIndividuals(initialStage, initialPopulation);
                return true;
            }
            
            var population = new PestPopulation
            {
                PopulationId = populationId,
                PestType = pestType,
                ZoneId = zoneId,
                LifecycleProfile = _lifecycleProfiles[pestType],
                LifecycleStages = InitializeLifecycleStages(pestType, initialStage, initialPopulation),
                EstablishmentDate = DateTime.Now,
                LastUpdateTime = _simulationTime,
                IsActive = true
            };
            
            _activePopulations[populationId] = population;
            _activeSpeciesCount = _activePopulations.Values.Count(p => p.IsActive);
            
            OnSpeciesIntroduced?.Invoke(pestType, zoneId);
            _onSpeciesIntroduced?.Raise();
            
            LogInfo($"Introduced {pestType} to zone {zoneId} with {initialPopulation} individuals at {initialStage} stage");
            
            return true;
        }
        
        /// <summary>
        /// Get current population data for a specific species in a zone
        /// </summary>
        public PestPopulation GetPopulation(PestType pestType, string zoneId)
        {
            var populationId = $"{zoneId}_{pestType}";
            return _activePopulations.ContainsKey(populationId) ? _activePopulations[populationId] : null;
        }
        
        /// <summary>
        /// Get all active populations in the simulation
        /// </summary>
        public List<PestPopulation> GetAllActivePopulations()
        {
            return _activePopulations.Values.Where(p => p.IsActive).ToList();
        }
        
        /// <summary>
        /// Get comprehensive lifecycle simulation report
        /// </summary>
        public LifecycleSimulationReport GetSimulationReport()
        {
            var activePopulations = GetAllActivePopulations();
            
            return new LifecycleSimulationReport
            {
                TotalPopulationCount = _totalPopulationCount,
                ActiveSpeciesCount = _activeSpeciesCount,
                SimulationTime = _simulationTime,
                CurrentSeason = _currentSeason,
                PopulationsBySpecies = GetPopulationsBySpecies(),
                PopulationsByZone = GetPopulationsByZone(),
                LifecycleDistribution = GetLifecycleDistribution(),
                EnvironmentalFactors = GetEnvironmentalFactorsSummary(),
                OutbreakWarnings = GetOutbreakWarnings(),
                ExtinctionRisks = GetExtinctionRisks(),
                ReportGeneratedAt = DateTime.Now
            };
        }
        
        /// <summary>
        /// Apply environmental treatment to affect pest populations
        /// </summary>
        public void ApplyEnvironmentalTreatment(string zoneId, EnvironmentalTreatment treatment)
        {
            if (!_environmentalZones.ContainsKey(zoneId))
            {
                LogError($"Environmental zone {zoneId} not found");
                return;
            }
            
            var zone = _environmentalZones[zoneId];
            var affectedPopulations = _activePopulations.Values.Where(p => p.ZoneId == zoneId).ToList();
            
            foreach (var population in affectedPopulations)
            {
                ApplyTreatmentToPopulation(population, treatment);
            }
            
            // Update zone environmental conditions
            zone.ApplyTreatment(treatment);
            
            LogInfo($"Applied {treatment.TreatmentType} treatment to zone {zoneId}, affecting {affectedPopulations.Count} populations");
        }
        
        #region Private Implementation
        
        private void UpdateSimulationCycle(float deltaTime)
        {
            var performanceStartTime = Time.realtimeSinceStartup;
            
            // Update seasonal state
            UpdateSeasonalState();
            
            // Update all active populations
            var populationsToUpdate = _activePopulations.Values.Where(p => p.IsActive).ToList();
            
            foreach (var population in populationsToUpdate)
            {
                UpdatePopulationLifecycle(population, deltaTime);
                UpdatePopulationDynamics(population, deltaTime);
                ApplyEnvironmentalEffects(population, deltaTime);
                ProcessSpeciesInteractions(population, deltaTime);
            }
            
            // Check for outbreaks and extinctions
            CheckForPopulationEvents();
            
            // Update totals
            _totalPopulationCount = _activePopulations.Values.Sum(p => p.GetTotalPopulation());
            
            // Record performance
            var performanceTime = (Time.realtimeSinceStartup - performanceStartTime) * 1000f;
            RecordPerformance(performanceTime);
        }
        
        private void UpdatePopulationLifecycle(PestPopulation population, float deltaTime)
        {
            var profile = population.LifecycleProfile;
            var zone = _environmentalZones[population.ZoneId];
            
            // Process each lifecycle stage
            foreach (var stage in Enum.GetValues(typeof(LifecycleStage)).Cast<LifecycleStage>())
            {
                if (!population.LifecycleStages.ContainsKey(stage)) continue;
                
                var stageData = population.LifecycleStages[stage];
                
                // Apply development rate based on environmental conditions
                var developmentRate = CalculateDevelopmentRate(profile, stage, zone.Conditions);
                stageData.DevelopmentProgress += developmentRate * deltaTime;
                
                // Check for stage advancement
                if (stageData.DevelopmentProgress >= profile.StageDurations[stage])
                {
                    AdvanceLifecycleStage(population, stage);
                }
                
                // Apply mortality
                ApplyMortality(stageData, profile, stage, zone.Conditions, deltaTime);
            }
            
            population.LastUpdateTime = _simulationTime;
        }
        
        private void UpdatePopulationDynamics(PestPopulation population, float deltaTime)
        {
            var profile = population.LifecycleProfile;
            var zone = _environmentalZones[population.ZoneId];
            var totalPopulation = population.GetTotalPopulation();
            
            // Calculate carrying capacity for this zone
            var carryingCapacity = CalculateCarryingCapacity(population.PestType, zone);
            
            // Apply density-dependent effects
            if (_enableDensityDependentEffects && totalPopulation > carryingCapacity * 0.8f)
            {
                var densityStress = (float)totalPopulation / carryingCapacity;
                ApplyDensityStress(population, densityStress, deltaTime);
            }
            
            // Process reproduction for reproductive stages
            ProcessReproduction(population, zone.Conditions, deltaTime);
            
            // Handle migration if population exceeds carrying capacity
            if (totalPopulation > carryingCapacity && _migrationRate > 0f)
            {
                ProcessMigration(population, deltaTime);
            }
        }
        
        private void ApplyEnvironmentalEffects(PestPopulation population, float deltaTime)
        {
            if (!_enableEnvironmentalFactors) return;
            
            var zone = _environmentalZones[population.ZoneId];
            var profile = population.LifecycleProfile;
            var conditions = zone.Conditions;
            
            // Temperature effects
            var tempEffect = CalculateTemperatureEffect(profile, conditions.Temperature);
            
            // Humidity effects
            var humidityEffect = CalculateHumidityEffect(profile, conditions.Humidity);
            
            // Light effects (for photosensitive species)
            var lightEffect = CalculateLightEffect(profile, conditions.LightIntensity);
            
            // Apply combined environmental effects
            var combinedEffect = (tempEffect * _temperatureInfluence + 
                                humidityEffect * _humidityInfluence + 
                                lightEffect * _lightInfluence) / 3f;
            
            ApplyEnvironmentalMultiplier(population, combinedEffect, deltaTime);
        }
        
        private void ProcessSpeciesInteractions(PestPopulation population, float deltaTime)
        {
            if (!_speciesInteractions.ContainsKey(population.PestType)) return;
            
            var interactions = _speciesInteractions[population.PestType];
            var zone = _environmentalZones[population.ZoneId];
            
            foreach (var interaction in interactions)
            {
                var otherPopulationId = $"{population.ZoneId}_{interaction.TargetSpecies}";
                if (!_activePopulations.ContainsKey(otherPopulationId)) continue;
                
                var otherPopulation = _activePopulations[otherPopulationId];
                ProcessInteraction(population, otherPopulation, interaction, deltaTime);
            }
        }
        
        private void AdvanceLifecycleStage(PestPopulation population, LifecycleStage currentStage)
        {
            var nextStage = GetNextLifecycleStage(currentStage);
            if (nextStage == LifecycleStage.None) return;
            
            var currentStageData = population.LifecycleStages[currentStage];
            var individualsToAdvance = currentStageData.PopulationCount;
            
            // Apply survival rate for stage transition
            var survivalRate = population.LifecycleProfile.StageSurvivalRates[currentStage];
            var survivors = Mathf.RoundToInt(individualsToAdvance * survivalRate);
            
            // Remove from current stage
            currentStageData.PopulationCount = 0;
            currentStageData.DevelopmentProgress = 0f;
            
            // Add to next stage
            if (!population.LifecycleStages.ContainsKey(nextStage))
            {
                population.LifecycleStages[nextStage] = new LifecycleStageData
                {
                    Stage = nextStage,
                    PopulationCount = 0,
                    DevelopmentProgress = 0f
                };
            }
            
            population.LifecycleStages[nextStage].PopulationCount += survivors;
            
            OnLifecycleAdvanced?.Invoke(population.PestType, nextStage);
            _onPestLifecycleAdvanced?.Raise();
            
            LogInfo($"{population.PestType} in {population.ZoneId}: {survivors} individuals advanced from {currentStage} to {nextStage}");
        }
        
        private void ProcessReproduction(PestPopulation population, EnvironmentalConditions conditions, float deltaTime)
        {
            var profile = population.LifecycleProfile;
            
            // Only adults can reproduce
            if (!population.LifecycleStages.ContainsKey(LifecycleStage.Adult)) return;
            
            var adultStage = population.LifecycleStages[LifecycleStage.Adult];
            if (adultStage.PopulationCount == 0) return;
            
            // Calculate reproduction rate based on environmental conditions
            var baseReproductionRate = profile.ReproductionRate;
            var environmentalModifier = CalculateReproductionModifier(profile, conditions);
            var effectiveRate = baseReproductionRate * environmentalModifier * deltaTime;
            
            // Calculate number of offspring
            var reproductiveAdults = adultStage.PopulationCount / 2; // Assume 50% are female
            var offspring = Mathf.RoundToInt(reproductiveAdults * effectiveRate);
            
            if (offspring > 0)
            {
                // Add eggs (first stage of lifecycle)
                if (!population.LifecycleStages.ContainsKey(LifecycleStage.Egg))
                {
                    population.LifecycleStages[LifecycleStage.Egg] = new LifecycleStageData
                    {
                        Stage = LifecycleStage.Egg,
                        PopulationCount = 0,
                        DevelopmentProgress = 0f
                    };
                }
                
                population.LifecycleStages[LifecycleStage.Egg].PopulationCount += offspring;
                
                LogInfo($"{population.PestType} reproduction: {offspring} eggs laid by {reproductiveAdults} adults");
            }
        }
        
        private void CheckForPopulationEvents()
        {
            var populationsToRemove = new List<string>();
            
            foreach (var kvp in _activePopulations)
            {
                var population = kvp.Value;
                var totalPop = population.GetTotalPopulation();
                
                // Check for extinction
                if (totalPop == 0)
                {
                    population.IsActive = false;
                    populationsToRemove.Add(kvp.Key);
                    
                    OnSpeciesExtinct?.Invoke(population.PestType, population.ZoneId);
                    _onSpeciesExtinct?.Raise();
                    
                    LogWarning($"{population.PestType} has gone extinct in zone {population.ZoneId}");
                }
                // Check for outbreak
                else if (totalPop > _maxPopulationPerSpecies * 0.8f)
                {
                    var outbreakData = new PopulationOutbreakData
                    {
                        PestType = population.PestType,
                        ZoneId = population.ZoneId,
                        PopulationCount = totalPop,
                        OutbreakSeverity = CalculateOutbreakSeverity(totalPop),
                        DetectedAt = DateTime.Now
                    };
                    
                    OnPopulationOutbreak?.Invoke(population.ZoneId, outbreakData);
                    _onPopulationOutbreak?.Raise();
                    
                    LogWarning($"Population outbreak detected: {population.PestType} in {population.ZoneId} has {totalPop} individuals");
                }
            }
            
            // Remove extinct populations
            foreach (var popId in populationsToRemove)
            {
                _activePopulations.Remove(popId);
            }
            
            _activeSpeciesCount = _activePopulations.Values.Count(p => p.IsActive);
        }
        
        private void InitializeLifecycleProfiles()
        {
            // Initialize lifecycle profiles for each pest type
            foreach (PestType pestType in Enum.GetValues(typeof(PestType)))
            {
                _lifecycleProfiles[pestType] = CreateLifecycleProfile(pestType);
            }
        }
        
        private PestLifecycleProfile CreateLifecycleProfile(PestType pestType)
        {
            return pestType switch
            {
                PestType.Aphids => new PestLifecycleProfile
                {
                    PestType = pestType,
                    StageDurations = new Dictionary<LifecycleStage, float>
                    {
                        { LifecycleStage.Egg, 3f },
                        { LifecycleStage.Larva, 7f },
                        { LifecycleStage.Nymph, 5f },
                        { LifecycleStage.Adult, 30f }
                    },
                    StageSurvivalRates = new Dictionary<LifecycleStage, float>
                    {
                        { LifecycleStage.Egg, 0.8f },
                        { LifecycleStage.Larva, 0.7f },
                        { LifecycleStage.Nymph, 0.9f },
                        { LifecycleStage.Adult, 0.6f }
                    },
                    ReproductionRate = 15f,
                    OptimalTemperature = 22f,
                    TemperatureTolerance = 5f,
                    OptimalHumidity = 0.6f,
                    HumidityTolerance = 0.2f,
                    GenerationsPerYear = 12
                },
                PestType.SpiderMites => new PestLifecycleProfile
                {
                    PestType = pestType,
                    StageDurations = new Dictionary<LifecycleStage, float>
                    {
                        { LifecycleStage.Egg, 5f },
                        { LifecycleStage.Larva, 3f },
                        { LifecycleStage.Nymph, 4f },
                        { LifecycleStage.Adult, 25f }
                    },
                    StageSurvivalRates = new Dictionary<LifecycleStage, float>
                    {
                        { LifecycleStage.Egg, 0.9f },
                        { LifecycleStage.Larva, 0.8f },
                        { LifecycleStage.Nymph, 0.85f },
                        { LifecycleStage.Adult, 0.7f }
                    },
                    ReproductionRate = 25f,
                    OptimalTemperature = 27f,
                    TemperatureTolerance = 4f,
                    OptimalHumidity = 0.4f,
                    HumidityTolerance = 0.15f,
                    GenerationsPerYear = 15
                },
                PestType.Thrips => new PestLifecycleProfile
                {
                    PestType = pestType,
                    StageDurations = new Dictionary<LifecycleStage, float>
                    {
                        { LifecycleStage.Egg, 4f },
                        { LifecycleStage.Larva, 8f },
                        { LifecycleStage.Pupa, 6f },
                        { LifecycleStage.Adult, 20f }
                    },
                    StageSurvivalRates = new Dictionary<LifecycleStage, float>
                    {
                        { LifecycleStage.Egg, 0.75f },
                        { LifecycleStage.Larva, 0.65f },
                        { LifecycleStage.Pupa, 0.8f },
                        { LifecycleStage.Adult, 0.5f }
                    },
                    ReproductionRate = 20f,
                    OptimalTemperature = 25f,
                    TemperatureTolerance = 6f,
                    OptimalHumidity = 0.5f,
                    HumidityTolerance = 0.25f,
                    GenerationsPerYear = 8
                },
                PestType.Whiteflies => new PestLifecycleProfile
                {
                    PestType = pestType,
                    StageDurations = new Dictionary<LifecycleStage, float>
                    {
                        { LifecycleStage.Egg, 7f },
                        { LifecycleStage.Larva, 14f },
                        { LifecycleStage.Pupa, 5f },
                        { LifecycleStage.Adult, 30f }
                    },
                    StageSurvivalRates = new Dictionary<LifecycleStage, float>
                    {
                        { LifecycleStage.Egg, 0.85f },
                        { LifecycleStage.Larva, 0.7f },
                        { LifecycleStage.Pupa, 0.9f },
                        { LifecycleStage.Adult, 0.6f }
                    },
                    ReproductionRate = 18f,
                    OptimalTemperature = 26f,
                    TemperatureTolerance = 5f,
                    OptimalHumidity = 0.7f,
                    HumidityTolerance = 0.15f,
                    GenerationsPerYear = 10
                },
                PestType.Fungusgnats => new PestLifecycleProfile
                {
                    PestType = pestType,
                    StageDurations = new Dictionary<LifecycleStage, float>
                    {
                        { LifecycleStage.Egg, 6f },
                        { LifecycleStage.Larva, 21f },
                        { LifecycleStage.Pupa, 4f },
                        { LifecycleStage.Adult, 10f }
                    },
                    StageSurvivalRates = new Dictionary<LifecycleStage, float>
                    {
                        { LifecycleStage.Egg, 0.9f },
                        { LifecycleStage.Larva, 0.6f },
                        { LifecycleStage.Pupa, 0.8f },
                        { LifecycleStage.Adult, 0.7f }
                    },
                    ReproductionRate = 12f,
                    OptimalTemperature = 24f,
                    TemperatureTolerance = 4f,
                    OptimalHumidity = 0.8f,
                    HumidityTolerance = 0.1f,
                    GenerationsPerYear = 6
                },
                _ => CreateDefaultLifecycleProfile(pestType)
            };
        }
        
        private PestLifecycleProfile CreateDefaultLifecycleProfile(PestType pestType)
        {
            return new PestLifecycleProfile
            {
                PestType = pestType,
                StageDurations = new Dictionary<LifecycleStage, float>
                {
                    { LifecycleStage.Egg, 5f },
                    { LifecycleStage.Larva, 10f },
                    { LifecycleStage.Pupa, 7f },
                    { LifecycleStage.Adult, 25f }
                },
                StageSurvivalRates = new Dictionary<LifecycleStage, float>
                {
                    { LifecycleStage.Egg, 0.8f },
                    { LifecycleStage.Larva, 0.7f },
                    { LifecycleStage.Pupa, 0.8f },
                    { LifecycleStage.Adult, 0.6f }
                },
                ReproductionRate = 15f,
                OptimalTemperature = 25f,
                TemperatureTolerance = 5f,
                OptimalHumidity = 0.6f,
                HumidityTolerance = 0.2f,
                GenerationsPerYear = 8
            };
        }
        
        private void InitializeEnvironmentalZones()
        {
            // Create default environmental zones
            _environmentalZones["grow_room_1"] = new EnvironmentalZone
            {
                ZoneId = "grow_room_1",
                ZoneName = "Primary Grow Room",
                Conditions = new EnvironmentalConditions
                {
                    Temperature = 24f,
                    Humidity = 0.6f,
                    LightIntensity = 600f,
                    CO2Level = 1200f,
                    AirFlow = 0.7f
                },
                CarryingCapacityMultiplier = 1.0f
            };
            
            _environmentalZones["propagation_room"] = new EnvironmentalZone
            {
                ZoneId = "propagation_room",
                ZoneName = "Propagation Room",
                Conditions = new EnvironmentalConditions
                {
                    Temperature = 26f,
                    Humidity = 0.8f,
                    LightIntensity = 400f,
                    CO2Level = 1000f,
                    AirFlow = 0.5f
                },
                CarryingCapacityMultiplier = 0.5f
            };
        }
        
        private void InitializeSpeciesInteractions()
        {
            // Define predator-prey relationships and competition
            _speciesInteractions[PestType.Aphids] = new List<PestInteraction>
            {
                new PestInteraction
                {
                    InteractionType = InteractionType.Competition,
                    TargetSpecies = PestType.Whiteflies,
                    EffectStrength = 0.3f
                }
            };
            
            _speciesInteractions[PestType.SpiderMites] = new List<PestInteraction>
            {
                new PestInteraction
                {
                    InteractionType = InteractionType.Competition,
                    TargetSpecies = PestType.Thrips,
                    EffectStrength = 0.2f
                }
            };
        }
        
        private Dictionary<LifecycleStage, LifecycleStageData> InitializeLifecycleStages(PestType pestType, LifecycleStage initialStage, int initialPopulation)
        {
            var stages = new Dictionary<LifecycleStage, LifecycleStageData>();
            
            foreach (LifecycleStage stage in Enum.GetValues(typeof(LifecycleStage)))
            {
                if (stage == LifecycleStage.None) continue;
                
                stages[stage] = new LifecycleStageData
                {
                    Stage = stage,
                    PopulationCount = stage == initialStage ? initialPopulation : 0,
                    DevelopmentProgress = 0f
                };
            }
            
            return stages;
        }
        
        // Additional helper methods would continue here...
        // (Implementation continues with calculation methods, environmental effects, etc.)
        
        private float CalculateDevelopmentRate(PestLifecycleProfile profile, LifecycleStage stage, EnvironmentalConditions conditions)
        {
            var baseRate = 1.0f / profile.StageDurations[stage];
            var tempEffect = CalculateTemperatureEffect(profile, conditions.Temperature);
            var humidityEffect = CalculateHumidityEffect(profile, conditions.Humidity);
            
            return baseRate * tempEffect * humidityEffect;
        }
        
        private float CalculateTemperatureEffect(PestLifecycleProfile profile, float temperature)
        {
            var deviation = Mathf.Abs(temperature - profile.OptimalTemperature);
            if (deviation <= profile.TemperatureTolerance)
            {
                return 1.0f;
            }
            else
            {
                var penalty = (deviation - profile.TemperatureTolerance) / profile.TemperatureTolerance;
                return Mathf.Max(0.1f, 1.0f - penalty * 0.5f);
            }
        }
        
        private float CalculateHumidityEffect(PestLifecycleProfile profile, float humidity)
        {
            var deviation = Mathf.Abs(humidity - profile.OptimalHumidity);
            if (deviation <= profile.HumidityTolerance)
            {
                return 1.0f;
            }
            else
            {
                var penalty = (deviation - profile.HumidityTolerance) / profile.HumidityTolerance;
                return Mathf.Max(0.1f, 1.0f - penalty * 0.3f);
            }
        }
        
        private float CalculateLightEffect(PestLifecycleProfile profile, float lightIntensity)
        {
            // Most pests prefer lower light conditions
            return Mathf.Clamp(1.0f - (lightIntensity / 1000f) * 0.2f, 0.5f, 1.0f);
        }
        
        private void UpdateMetrics()
        {
            _metrics.TotalPopulationCount = _totalPopulationCount;
            _metrics.ActiveSpeciesCount = _activeSpeciesCount;
            _metrics.SimulationTime = _simulationTime;
            _metrics.CurrentSeason = _currentSeason;
            _metrics.ActiveZones = _environmentalZones.Count;
            _metrics.LastUpdated = DateTime.Now;
        }
        
        private void RecordPerformance(float performanceTime)
        {
            _performanceHistory.Enqueue(performanceTime);
            if (_performanceHistory.Count > 100)
            {
                _performanceHistory.Dequeue();
            }
            
            _metrics.AverageUpdateTime = _performanceHistory.Average();
        }
        
        #endregion
        
        private void UpdateSeasonalState()
        {
            // Simple seasonal progression based on simulation time
            var seasonCycleDuration = 365f; // days
            var seasonProgress = (_simulationTime % seasonCycleDuration) / seasonCycleDuration;
            
            _currentSeason = seasonProgress switch
            {
                < 0.25f => SeasonalState.Spring,
                < 0.5f => SeasonalState.Summer,
                < 0.75f => SeasonalState.Fall,
                _ => SeasonalState.Winter
            };
        }
        
        private void ApplyMortality(LifecycleStageData stageData, PestLifecycleProfile profile, LifecycleStage stage, EnvironmentalConditions conditions, float deltaTime)
        {
            var baseMortalityRate = (1.0f - profile.StageSurvivalRates[stage]) * _mortalityBaseRate;
            var environmentalStress = CalculateEnvironmentalStress(profile, conditions);
            var adjustedMortalityRate = baseMortalityRate * (1.0f + environmentalStress) * deltaTime;
            
            var deaths = Mathf.RoundToInt(stageData.PopulationCount * adjustedMortalityRate);
            stageData.PopulationCount = Mathf.Max(0, stageData.PopulationCount - deaths);
        }
        
        private float CalculateEnvironmentalStress(PestLifecycleProfile profile, EnvironmentalConditions conditions)
        {
            var tempStress = Mathf.Abs(conditions.Temperature - profile.OptimalTemperature) / profile.TemperatureTolerance;
            var humidityStress = Mathf.Abs(conditions.Humidity - profile.OptimalHumidity) / profile.HumidityTolerance;
            
            return Mathf.Max(0f, (tempStress + humidityStress) * 0.5f - 1.0f);
        }
        
        private int CalculateCarryingCapacity(PestType pestType, EnvironmentalZone zone)
        {
            var baseCapacity = pestType switch
            {
                PestType.Aphids => 500,
                PestType.SpiderMites => 1000,
                PestType.Thrips => 300,
                PestType.Whiteflies => 400,
                PestType.Fungusgnats => 200,
                _ => 250
            };
            
            return Mathf.RoundToInt(baseCapacity * zone.CarryingCapacityMultiplier * _carryingCapacityMultiplier);
        }
        
        private void ApplyDensityStress(PestPopulation population, float densityStress, float deltaTime)
        {
            var stressMultiplier = Mathf.Min(2.0f, densityStress);
            
            foreach (var stageData in population.LifecycleStages.Values)
            {
                var stressDeaths = Mathf.RoundToInt(stageData.PopulationCount * stressMultiplier * 0.1f * deltaTime);
                stageData.PopulationCount = Mathf.Max(0, stageData.PopulationCount - stressDeaths);
            }
        }
        
        private void ProcessMigration(PestPopulation population, float deltaTime)
        {
            var totalPop = population.GetTotalPopulation();
            var migrantsCount = Mathf.RoundToInt(totalPop * _migrationRate * deltaTime);
            
            if (migrantsCount > 0)
            {
                // Remove migrants from current population (simplified - usually they'd move to adjacent zones)
                var adultStage = population.LifecycleStages[LifecycleStage.Adult];
                var actualMigrants = Mathf.Min(migrantsCount, adultStage.PopulationCount);
                adultStage.PopulationCount -= actualMigrants;
                
                LogInfo($"Migration: {actualMigrants} {population.PestType} adults migrated from {population.ZoneId}");
            }
        }
        
        private void ApplyEnvironmentalMultiplier(PestPopulation population, float multiplier, float deltaTime)
        {
            if (multiplier >= 1.0f) return; // No negative effects
            
            var negativeEffect = 1.0f - multiplier;
            
            foreach (var stageData in population.LifecycleStages.Values)
            {
                var affectedCount = Mathf.RoundToInt(stageData.PopulationCount * negativeEffect * deltaTime * 0.1f);
                stageData.PopulationCount = Mathf.Max(0, stageData.PopulationCount - affectedCount);
            }
        }
        
        private void ProcessInteraction(PestPopulation population1, PestPopulation population2, PestInteraction interaction, float deltaTime)
        {
            switch (interaction.InteractionType)
            {
                case InteractionType.Competition:
                    ProcessCompetition(population1, population2, interaction.EffectStrength, deltaTime);
                    break;
                case InteractionType.Predation:
                    ProcessPredation(population1, population2, interaction.EffectStrength, deltaTime);
                    break;
            }
        }
        
        private void ProcessCompetition(PestPopulation pop1, PestPopulation pop2, float strength, float deltaTime)
        {
            var pop1Total = pop1.GetTotalPopulation();
            var pop2Total = pop2.GetTotalPopulation();
            
            if (pop1Total == 0 || pop2Total == 0) return;
            
            var competitionEffect = (float)pop2Total / (pop1Total + pop2Total) * strength * deltaTime;
            
            // Reduce pop1 based on competition from pop2
            foreach (var stageData in pop1.LifecycleStages.Values)
            {
                var losses = Mathf.RoundToInt(stageData.PopulationCount * competitionEffect * 0.05f);
                stageData.PopulationCount = Mathf.Max(0, stageData.PopulationCount - losses);
            }
        }
        
        private void ProcessPredation(PestPopulation predator, PestPopulation prey, float strength, float deltaTime)
        {
            var predatorCount = predator.GetTotalPopulation();
            var preyCount = prey.GetTotalPopulation();
            
            if (predatorCount == 0 || preyCount == 0) return;
            
            var predationRate = strength * deltaTime * 0.1f;
            var preyKilled = Mathf.RoundToInt(Mathf.Min(preyCount * predationRate, predatorCount * 2f));
            
            // Remove prey
            var preyStages = prey.LifecycleStages.Values.Where(s => s.PopulationCount > 0).ToList();
            if (preyStages.Any())
            {
                var targetStage = preyStages[UnityEngine.Random.Range(0, preyStages.Count)];
                var actualKilled = Mathf.Min(preyKilled, targetStage.PopulationCount);
                targetStage.PopulationCount -= actualKilled;
            }
        }
        
        private LifecycleStage GetNextLifecycleStage(LifecycleStage currentStage)
        {
            return currentStage switch
            {
                LifecycleStage.Egg => LifecycleStage.Larva,
                LifecycleStage.Larva => LifecycleStage.Nymph,
                LifecycleStage.Nymph => LifecycleStage.Adult,
                LifecycleStage.Pupa => LifecycleStage.Adult,
                _ => LifecycleStage.None
            };
        }
        
        private float CalculateReproductionModifier(PestLifecycleProfile profile, EnvironmentalConditions conditions)
        {
            var tempEffect = CalculateTemperatureEffect(profile, conditions.Temperature);
            var humidityEffect = CalculateHumidityEffect(profile, conditions.Humidity);
            
            return (tempEffect + humidityEffect) * 0.5f;
        }
        
        private OutbreakSeverity CalculateOutbreakSeverity(int populationCount)
        {
            var threshold = _maxPopulationPerSpecies;
            var ratio = (float)populationCount / threshold;
            
            return ratio switch
            {
                > 0.95f => OutbreakSeverity.Critical,
                > 0.85f => OutbreakSeverity.Major,
                > 0.7f => OutbreakSeverity.Moderate,
                _ => OutbreakSeverity.Minor
            };
        }
        
        private void ApplyTreatmentToPopulation(PestPopulation population, EnvironmentalTreatment treatment)
        {
            var effectiveness = treatment.Effectiveness;
            
            switch (treatment.TreatmentType)
            {
                case TreatmentType.EnvironmentalAdjustment:
                    // Environmental treatments affect all stages
                    foreach (var stageData in population.LifecycleStages.Values)
                    {
                        var affected = Mathf.RoundToInt(stageData.PopulationCount * effectiveness);
                        stageData.PopulationCount = Mathf.Max(0, stageData.PopulationCount - affected);
                    }
                    break;
                    
                case TreatmentType.BiologicalControl:
                    // Biological controls primarily affect specific stages
                    if (population.LifecycleStages.ContainsKey(LifecycleStage.Larva))
                    {
                        var larvaStage = population.LifecycleStages[LifecycleStage.Larva];
                        var affected = Mathf.RoundToInt(larvaStage.PopulationCount * effectiveness);
                        larvaStage.PopulationCount = Mathf.Max(0, larvaStage.PopulationCount - affected);
                    }
                    break;
            }
        }
        
        private Dictionary<PestType, int> GetPopulationsBySpecies()
        {
            return _activePopulations.Values
                .Where(p => p.IsActive)
                .GroupBy(p => p.PestType)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.GetTotalPopulation()));
        }
        
        private Dictionary<string, int> GetPopulationsByZone()
        {
            return _activePopulations.Values
                .Where(p => p.IsActive)
                .GroupBy(p => p.ZoneId)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.GetTotalPopulation()));
        }
        
        private Dictionary<LifecycleStage, int> GetLifecycleDistribution()
        {
            var distribution = new Dictionary<LifecycleStage, int>();
            
            foreach (LifecycleStage stage in Enum.GetValues(typeof(LifecycleStage)))
            {
                if (stage == LifecycleStage.None) continue;
                
                distribution[stage] = _activePopulations.Values
                    .Where(p => p.IsActive && p.LifecycleStages.ContainsKey(stage))
                    .Sum(p => p.LifecycleStages[stage].PopulationCount);
            }
            
            return distribution;
        }
        
        private string GetEnvironmentalFactorsSummary()
        {
            var zones = _environmentalZones.Values;
            var avgTemp = zones.Average(z => z.Conditions.Temperature);
            var avgHumidity = zones.Average(z => z.Conditions.Humidity);
            var avgLight = zones.Average(z => z.Conditions.LightIntensity);
            
            return $"Avg Temp: {avgTemp:F1}°C, Avg Humidity: {avgHumidity:P0}, Avg Light: {avgLight:F0} PPFD";
        }
        
        private List<string> GetOutbreakWarnings()
        {
            var warnings = new List<string>();
            
            foreach (var population in _activePopulations.Values.Where(p => p.IsActive))
            {
                var totalPop = population.GetTotalPopulation();
                var threshold = _maxPopulationPerSpecies * 0.7f;
                
                if (totalPop > threshold)
                {
                    warnings.Add($"{population.PestType} in {population.ZoneId}: {totalPop} individuals (approaching outbreak)");
                }
            }
            
            return warnings;
        }
        
        private List<string> GetExtinctionRisks()
        {
            var risks = new List<string>();
            
            foreach (var population in _activePopulations.Values.Where(p => p.IsActive))
            {
                var totalPop = population.GetTotalPopulation();
                
                if (totalPop < 10)
                {
                    risks.Add($"{population.PestType} in {population.ZoneId}: {totalPop} individuals (extinction risk)");
                }
            }
            
            return risks;
        }
    }
    
    // Data structures for the lifecycle simulation
    
    [System.Serializable]
    public class PestPopulation
    {
        public string PopulationId;
        public PestType PestType;
        public string ZoneId;
        public PestLifecycleProfile LifecycleProfile;
        public Dictionary<LifecycleStage, LifecycleStageData> LifecycleStages = new Dictionary<LifecycleStage, LifecycleStageData>();
        public DateTime EstablishmentDate;
        public float LastUpdateTime;
        public bool IsActive = true;
        
        public int GetTotalPopulation()
        {
            return LifecycleStages.Values.Sum(stage => stage.PopulationCount);
        }
        
        public void AddIndividuals(LifecycleStage stage, int count)
        {
            if (LifecycleStages.ContainsKey(stage))
            {
                LifecycleStages[stage].PopulationCount += count;
            }
        }
    }
    
    [System.Serializable]
    public class PestLifecycleProfile
    {
        public PestType PestType;
        public Dictionary<LifecycleStage, float> StageDurations = new Dictionary<LifecycleStage, float>();
        public Dictionary<LifecycleStage, float> StageSurvivalRates = new Dictionary<LifecycleStage, float>();
        public float ReproductionRate;
        public float OptimalTemperature;
        public float TemperatureTolerance;
        public float OptimalHumidity;
        public float HumidityTolerance;
        public int GenerationsPerYear;
    }
    
    [System.Serializable]
    public class LifecycleStageData
    {
        public LifecycleStage Stage;
        public int PopulationCount;
        public float DevelopmentProgress;
    }
    
    [System.Serializable]
    public class EnvironmentalZone
    {
        public string ZoneId;
        public string ZoneName;
        public EnvironmentalConditions Conditions;
        public float CarryingCapacityMultiplier = 1.0f;
        
        public void ApplyTreatment(EnvironmentalTreatment treatment)
        {
            // Apply treatment effects to environmental conditions
            switch (treatment.TreatmentType)
            {
                case TreatmentType.EnvironmentalAdjustment:
                    Conditions.Temperature += treatment.TemperatureChange;
                    Conditions.Humidity += treatment.HumidityChange;
                    break;
            }
        }
    }
    
    [System.Serializable]
    public class PestInteraction
    {
        public InteractionType InteractionType;
        public PestType TargetSpecies;
        public float EffectStrength;
    }
    
    [System.Serializable]
    public class EnvironmentalTreatment
    {
        public TreatmentType TreatmentType;
        public float TemperatureChange;
        public float HumidityChange;
        public float Duration;
        public float Effectiveness;
    }
    
    [System.Serializable]
    public class PopulationOutbreakData
    {
        public PestType PestType;
        public string ZoneId;
        public int PopulationCount;
        public OutbreakSeverity OutbreakSeverity;
        public DateTime DetectedAt;
    }
    
    [System.Serializable]
    public class LifecycleSimulationReport
    {
        public int TotalPopulationCount;
        public int ActiveSpeciesCount;
        public float SimulationTime;
        public SeasonalState CurrentSeason;
        public Dictionary<PestType, int> PopulationsBySpecies = new Dictionary<PestType, int>();
        public Dictionary<string, int> PopulationsByZone = new Dictionary<string, int>();
        public Dictionary<LifecycleStage, int> LifecycleDistribution = new Dictionary<LifecycleStage, int>();
        public string EnvironmentalFactors;
        public List<string> OutbreakWarnings = new List<string>();
        public List<string> ExtinctionRisks = new List<string>();
        public DateTime ReportGeneratedAt;
    }
    
    [System.Serializable]
    public class SimulationMetrics
    {
        public int TotalPopulationCount;
        public int ActiveSpeciesCount;
        public float SimulationTime;
        public SeasonalState CurrentSeason;
        public int ActiveZones;
        public float AverageUpdateTime;
        public DateTime LastUpdated;
        
        public SimulationMetrics()
        {
            LastUpdated = DateTime.Now;
        }
    }
    
    // Enums for lifecycle simulation
    public enum LifecycleStage
    {
        None,
        Egg,
        Larva,
        Nymph,
        Pupa,
        Adult
    }
    
    public enum SeasonalState
    {
        Spring,
        Summer,
        Fall,
        Winter
    }
    
    public enum InteractionType
    {
        Competition,
        Predation,
        Mutualism,
        Parasitism
    }
}