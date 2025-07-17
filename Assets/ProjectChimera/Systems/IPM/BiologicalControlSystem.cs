using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.IPM;

namespace ProjectChimera.Systems.IPM
{
    /// <summary>
    /// Advanced biological control system for Integrated Pest Management.
    /// Manages beneficial organisms, predator-prey relationships, and natural pest control methods.
    /// </summary>
    public class BiologicalControlSystem : ChimeraManager
    {
        [Header("Biological Control Configuration")]
        [SerializeField] private bool _enableAutomaticRelease = true;
        [SerializeField] private bool _enablePredatorTracking = true;
        [SerializeField] private bool _enableEcosystemBalance = true;
        [SerializeField] private float _releaseEfficiency = 0.85f;
        [SerializeField] private float _establishmentRate = 0.75f;
        [SerializeField] private int _maxActiveBioControls = 20;
        
        [Header("Beneficial Organism Management")]
        [SerializeField] private bool _enablePopulationDynamics = true;
        [SerializeField] private bool _enableBreedingPrograms = true;
        [SerializeField] private bool _enableHabitatOptimization = true;
        [SerializeField] private float _populationGrowthRate = 0.15f;
        [SerializeField] private float _survivalRate = 0.80f;
        [SerializeField] private float _huntingEfficiency = 0.70f;
        
        [Header("Ecosystem Balance")]
        [SerializeField] private bool _enableCompetitionTracking = true;
        [SerializeField] private bool _enableNaturalSelection = true;
        [SerializeField] private bool _enableEnvironmentalStress = true;
        [SerializeField] private float _ecosystemStabilityThreshold = 0.75f;
        [SerializeField] private float _biodiversityIndex = 0.65f;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onBioControlReleased;
        [SerializeField] private SimpleGameEventSO _onPredatorSuccess;
        [SerializeField] private SimpleGameEventSO _onEcosystemBalance;
        [SerializeField] private SimpleGameEventSO _onPopulationUpdate;
        
        // Core biological control data
        private Dictionary<string, BiologicalControlData> _activeBioControls = new Dictionary<string, BiologicalControlData>();
        private Dictionary<string, PredatorPreyRelationship> _predatorPreyRelations = new Dictionary<string, PredatorPreyRelationship>();
        private Dictionary<string, EcosystemData> _ecosystemZones = new Dictionary<string, EcosystemData>();
        private Dictionary<BeneficialOrganismType, BiologicalControlProfile> _bioControlProfiles = new Dictionary<BeneficialOrganismType, BiologicalControlProfile>();
        
        // Biological control systems
        private OrganismReleaseSystem _releaseSystem;
        private PredatorTrackingSystem _predatorSystem;
        private EcosystemBalanceSystem _ecosystemSystem;
        private BiologicalControlAnalytics _analytics;
        
        // Performance metrics
        private BiologicalControlMetrics _metrics;
        private float _lastUpdateTime = 0f;
        private int _totalOrganismsReleased = 0;
        private int _successfulEstablishments = 0;
        private int _totalPestsControlled = 0;
        private float _averageControlEfficiency = 0f;
        
        // Events
        public System.Action<string, BiologicalControlData> OnBioControlReleased;
        public System.Action<string, PredatorSuccessData> OnPredatorSuccess;
        public System.Action<string, EcosystemBalanceData> OnEcosystemBalance;
        public System.Action<string, PopulationUpdateData> OnPopulationUpdate;
        
        // Properties
        public override string ManagerName => "Biological Control System";
        public override ManagerPriority Priority => ManagerPriority.High;
        public int TotalOrganismsReleased => _totalOrganismsReleased;
        public int SuccessfulEstablishments => _successfulEstablishments;
        public int TotalPestsControlled => _totalPestsControlled;
        public float AverageControlEfficiency => _averageControlEfficiency;
        public float EcosystemStability => CalculateEcosystemStability();
        public BiologicalControlMetrics Metrics => _metrics;
        
        protected override void OnManagerInitialize()
        {
            InitializeBiologicalControlSystems();
            InitializeBioControlProfiles();
            InitializeEcosystemZones();
            
            _metrics = new BiologicalControlMetrics();
            
            LogInfo("Biological Control System initialized successfully");
        }
        
        protected override void OnManagerUpdate()
        {
            float currentTime = Time.time;
            
            // Update biological control populations
            UpdateBioControlPopulations();
            
            // Update predator-prey relationships
            UpdatePredatorPreyRelations();
            
            // Update ecosystem balance
            if (_enableEcosystemBalance)
            {
                UpdateEcosystemBalance();
            }
            
            // Update population dynamics
            if (_enablePopulationDynamics)
            {
                UpdatePopulationDynamics();
            }
            
            UpdateMetrics();
            _lastUpdateTime = currentTime;
        }
        
        protected override void OnManagerShutdown()
        {
            _activeBioControls.Clear();
            _predatorPreyRelations.Clear();
            _ecosystemZones.Clear();
            
            LogInfo("Biological Control System shutdown completed");
        }
        
        /// <summary>
        /// Release beneficial organisms for pest control
        /// </summary>
        public BiologicalControlResult ReleaseBeneficialOrganisms(BeneficialOrganismType organismType, Vector3 releaseLocation, int quantity)
        {
            var bioControl = new BiologicalControlData
            {
                ControlId = Guid.NewGuid().ToString(),
                OrganismType = organismType,
                ReleaseLocation = releaseLocation,
                InitialPopulation = quantity,
                CurrentPopulation = quantity,
                ReleaseTime = DateTime.Now,
                IsEstablished = false,
                ControlEfficiency = 0f,
                TargetPests = GetTargetPests(organismType),
                HuntingRadius = GetHuntingRadius(organismType),
                SurvivalRate = _survivalRate,
                ReproductionRate = GetReproductionRate(organismType)
            };
            
            var result = _releaseSystem.ReleaseOrganisms(bioControl);
            
            if (result.IsSuccessful)
            {
                _activeBioControls[bioControl.ControlId] = bioControl;
                _totalOrganismsReleased += quantity;
                
                // Create predator-prey relationships
                CreatePredatorPreyRelations(bioControl);
                
                OnBioControlReleased?.Invoke(bioControl.ControlId, bioControl);
                _onBioControlReleased?.Raise();
                
                LogInfo($"Released {quantity} {organismType} organisms for biological control");
            }
            
            return result;
        }
        
        /// <summary>
        /// Apply targeted biological control to specific pest outbreak
        /// </summary>
        public BiologicalControlResult ApplyTargetedBioControl(string pestId, PestType pestType, Vector3 location)
        {
            var targetOrganism = GetOptimalBioControlForPest(pestType);
            if (targetOrganism == BeneficialOrganismType.Ladybugs) // Default fallback
            {
                return new BiologicalControlResult
                {
                    ControlId = "",
                    IsSuccessful = false,
                    ResultMessage = "No suitable biological control available for pest type",
                    EstimatedEffectiveness = 0f
                };
            }
            
            int optimalQuantity = CalculateOptimalReleaseQuantity(pestType, location);
            return ReleaseBeneficialOrganisms(targetOrganism, location, optimalQuantity);
        }
        
        /// <summary>
        /// Establish breeding programs for beneficial organisms
        /// </summary>
        public BreedingProgramResult EstablishBreedingProgram(BeneficialOrganismType organismType, BreedingProgramConfiguration config)
        {
            var program = new BreedingProgramData
            {
                ProgramId = Guid.NewGuid().ToString(),
                OrganismType = organismType,
                Configuration = config,
                StartTime = DateTime.Now,
                IsActive = true,
                BreedingCycles = 0,
                TotalOffspring = 0,
                QualityScore = 0.75f,
                GeneticDiversity = 0.85f
            };
            
            var result = new BreedingProgramResult
            {
                ProgramId = program.ProgramId,
                IsSuccessful = true,
                ResultMessage = $"Breeding program established for {organismType}",
                EstimatedProductionRate = config.ProductionRate,
                EstimatedQuality = program.QualityScore
            };
            
            LogInfo($"Established breeding program for {organismType}");
            return result;
        }
        
        /// <summary>
        /// Optimize habitat conditions for beneficial organisms
        /// </summary>
        public HabitatOptimizationResult OptimizeHabitat(string areaId, HabitatOptimizationPlan plan)
        {
            var habitat = new HabitatData
            {
                HabitatId = Guid.NewGuid().ToString(),
                AreaId = areaId,
                OptimizationPlan = plan,
                CreationTime = DateTime.Now,
                IsOptimized = false,
                BiodiversityIndex = _biodiversityIndex,
                CarryingCapacity = plan.TargetCapacity,
                EnvironmentalStability = 0.65f
            };
            
            var result = new HabitatOptimizationResult
            {
                HabitatId = habitat.HabitatId,
                IsSuccessful = true,
                ResultMessage = "Habitat optimization initiated",
                EstimatedImprovement = 0.25f,
                EstimatedCompletionTime = TimeSpan.FromDays(plan.EstimatedDays)
            };
            
            LogInfo($"Optimizing habitat for area {areaId}");
            return result;
        }
        
        /// <summary>
        /// Get comprehensive biological control status
        /// </summary>
        public BiologicalControlStatusReport GetSystemStatus()
        {
            return new BiologicalControlStatusReport
            {
                TotalActiveBioControls = _activeBioControls.Count,
                TotalOrganismsReleased = _totalOrganismsReleased,
                SuccessfulEstablishments = _successfulEstablishments,
                TotalPestsControlled = _totalPestsControlled,
                AverageControlEfficiency = _averageControlEfficiency,
                EcosystemStability = CalculateEcosystemStability(),
                BiodiversityIndex = _biodiversityIndex,
                ActivePredatorPreyRelations = _predatorPreyRelations.Count,
                EcosystemZones = _ecosystemZones.Count,
                LastUpdated = DateTime.Now
            };
        }
        
        /// <summary>
        /// Configure biological control parameters
        /// </summary>
        public void ConfigureBiologicalControl(BiologicalControlConfiguration config)
        {
            _releaseEfficiency = config.ReleaseEfficiency;
            _establishmentRate = config.EstablishmentRate;
            _populationGrowthRate = config.PopulationGrowthRate;
            _survivalRate = config.SurvivalRate;
            _huntingEfficiency = config.HuntingEfficiency;
            
            LogInfo("Biological control configuration updated");
        }
        
        #region Private Implementation
        
        private void InitializeBiologicalControlSystems()
        {
            _releaseSystem = new OrganismReleaseSystem(_releaseEfficiency);
            _predatorSystem = new PredatorTrackingSystem(_huntingEfficiency);
            _ecosystemSystem = new EcosystemBalanceSystem(_ecosystemStabilityThreshold);
            _analytics = new BiologicalControlAnalytics();
        }
        
        private void InitializeBioControlProfiles()
        {
            var organismTypes = Enum.GetValues(typeof(BeneficialOrganismType));
            foreach (BeneficialOrganismType organismType in organismTypes)
            {
                _bioControlProfiles[organismType] = new BiologicalControlProfile
                {
                    OrganismType = organismType,
                    ControlEfficiency = GetBaseControlEfficiency(organismType),
                    TargetPests = GetTargetPests(organismType),
                    HuntingRadius = GetHuntingRadius(organismType),
                    ReproductionRate = GetReproductionRate(organismType),
                    SurvivalRate = GetBaseSurvivalRate(organismType),
                    EnvironmentalRequirements = GetEnvironmentalRequirements(organismType)
                };
            }
        }
        
        private void InitializeEcosystemZones()
        {
            // Initialize default ecosystem zones
            for (int i = 0; i < 3; i++)
            {
                var zoneId = $"ecosystem_zone_{i}";
                _ecosystemZones[zoneId] = new EcosystemData
                {
                    ZoneId = zoneId,
                    BiodiversityIndex = _biodiversityIndex,
                    StabilityScore = 0.70f,
                    CarryingCapacity = 1000,
                    CurrentPopulation = 0,
                    PredatorPreyBalance = 0.65f,
                    EnvironmentalStress = 0.15f,
                    HabitatQuality = 0.80f
                };
            }
        }
        
        private void UpdateBioControlPopulations()
        {
            foreach (var bioControl in _activeBioControls.Values.ToList())
            {
                UpdatePopulation(bioControl);
                UpdateControlEfficiency(bioControl);
                CheckEstablishment(bioControl);
            }
        }
        
        private void UpdatePopulation(BiologicalControlData bioControl)
        {
            if (!bioControl.IsEstablished)
            {
                // Population decline during establishment phase
                float mortalityRate = (1f - bioControl.SurvivalRate) * Time.deltaTime;
                bioControl.CurrentPopulation = Mathf.Max(0, bioControl.CurrentPopulation - (int)(bioControl.CurrentPopulation * mortalityRate));
            }
            else
            {
                // Population growth after establishment
                float growthRate = bioControl.ReproductionRate * Time.deltaTime;
                int newPopulation = Mathf.RoundToInt(bioControl.CurrentPopulation * (1f + growthRate));
                bioControl.CurrentPopulation = Mathf.Min(newPopulation, bioControl.InitialPopulation * 3); // Cap at 3x initial
            }
        }
        
        private void UpdateControlEfficiency(BiologicalControlData bioControl)
        {
            if (bioControl.IsEstablished)
            {
                float baseEfficiency = GetBaseControlEfficiency(bioControl.OrganismType);
                float populationFactor = bioControl.CurrentPopulation / (float)bioControl.InitialPopulation;
                float environmentalFactor = GetEnvironmentalFactor(bioControl.ReleaseLocation);
                
                bioControl.ControlEfficiency = baseEfficiency * populationFactor * environmentalFactor;
            }
        }
        
        private void CheckEstablishment(BiologicalControlData bioControl)
        {
            if (!bioControl.IsEstablished)
            {
                TimeSpan elapsedTime = DateTime.Now - bioControl.ReleaseTime;
                float establishmentThreshold = _establishmentRate * (bioControl.CurrentPopulation / (float)bioControl.InitialPopulation);
                
                if (elapsedTime.TotalDays >= 7 && establishmentThreshold > 0.5f)
                {
                    bioControl.IsEstablished = true;
                    _successfulEstablishments++;
                    
                    LogInfo($"Biological control {bioControl.OrganismType} successfully established");
                }
            }
        }
        
        private void UpdatePredatorPreyRelations()
        {
            foreach (var relation in _predatorPreyRelations.Values.ToList())
            {
                UpdatePredatorPreyDynamics(relation);
            }
        }
        
        private void UpdatePredatorPreyDynamics(PredatorPreyRelationship relation)
        {
            // Simplified Lotka-Volterra dynamics
            float predatorGrowth = relation.PredationRate * relation.PreyPopulation * relation.PredatorPopulation;
            float preyDeath = relation.PredationRate * relation.PreyPopulation * relation.PredatorPopulation;
            
            relation.PredatorPopulation += predatorGrowth * Time.deltaTime;
            relation.PreyPopulation -= preyDeath * Time.deltaTime;
            
            // Ensure populations don't go negative
            relation.PredatorPopulation = Mathf.Max(0, relation.PredatorPopulation);
            relation.PreyPopulation = Mathf.Max(0, relation.PreyPopulation);
            
            // Update control effectiveness
            if (relation.PreyPopulation > 0)
            {
                relation.ControlEffectiveness = 1f - (relation.PreyPopulation / relation.InitialPreyPopulation);
                _totalPestsControlled += Mathf.RoundToInt(preyDeath * Time.deltaTime);
            }
        }
        
        private void UpdateEcosystemBalance()
        {
            foreach (var ecosystem in _ecosystemZones.Values.ToList())
            {
                UpdateEcosystemStability(ecosystem);
                UpdateBiodiversity(ecosystem);
                CheckEcosystemThresholds(ecosystem);
            }
        }
        
        private void UpdateEcosystemStability(EcosystemData ecosystem)
        {
            float predatorPreyBalance = CalculatePredatorPreyBalance(ecosystem);
            float populationPressure = ecosystem.CurrentPopulation / (float)ecosystem.CarryingCapacity;
            float environmentalStress = ecosystem.EnvironmentalStress;
            
            ecosystem.StabilityScore = (predatorPreyBalance * 0.4f + (1f - populationPressure) * 0.3f + (1f - environmentalStress) * 0.3f);
            ecosystem.PredatorPreyBalance = predatorPreyBalance;
        }
        
        private void UpdateBiodiversity(EcosystemData ecosystem)
        {
            int speciesCount = GetSpeciesCount(ecosystem.ZoneId);
            float diversityIndex = CalculateShannonDiversity(ecosystem.ZoneId);
            
            ecosystem.BiodiversityIndex = diversityIndex;
            ecosystem.SpeciesCount = speciesCount;
        }
        
        private void CheckEcosystemThresholds(EcosystemData ecosystem)
        {
            if (ecosystem.StabilityScore < _ecosystemStabilityThreshold)
            {
                // Trigger ecosystem intervention
                OnEcosystemBalance?.Invoke(ecosystem.ZoneId, new EcosystemBalanceData
                {
                    ZoneId = ecosystem.ZoneId,
                    StabilityScore = ecosystem.StabilityScore,
                    RequiresIntervention = true,
                    RecommendedActions = GetEcosystemInterventions(ecosystem)
                });
            }
        }
        
        private void UpdatePopulationDynamics()
        {
            foreach (var bioControl in _activeBioControls.Values.ToList())
            {
                var populationData = new PopulationUpdateData
                {
                    ControlId = bioControl.ControlId,
                    OrganismType = bioControl.OrganismType,
                    CurrentPopulation = bioControl.CurrentPopulation,
                    PopulationChange = bioControl.CurrentPopulation - bioControl.InitialPopulation,
                    IsEstablished = bioControl.IsEstablished,
                    ControlEfficiency = bioControl.ControlEfficiency
                };
                
                OnPopulationUpdate?.Invoke(bioControl.ControlId, populationData);
            }
        }
        
        private void UpdateMetrics()
        {
            _averageControlEfficiency = _activeBioControls.Values.Count > 0 ? 
                _activeBioControls.Values.Average(b => b.ControlEfficiency) : 0f;
            
            _metrics.TotalOrganismsReleased = _totalOrganismsReleased;
            _metrics.SuccessfulEstablishments = _successfulEstablishments;
            _metrics.TotalPestsControlled = _totalPestsControlled;
            _metrics.AverageControlEfficiency = _averageControlEfficiency;
            _metrics.EcosystemStability = CalculateEcosystemStability();
            _metrics.LastUpdated = DateTime.Now;
        }
        
        private void CreatePredatorPreyRelations(BiologicalControlData bioControl)
        {
            foreach (var targetPest in bioControl.TargetPests)
            {
                var relationId = $"{bioControl.ControlId}_{targetPest}";
                _predatorPreyRelations[relationId] = new PredatorPreyRelationship
                {
                    RelationshipId = relationId,
                    PredatorType = bioControl.OrganismType,
                    PreyType = targetPest,
                    PredatorPopulation = bioControl.CurrentPopulation,
                    PreyPopulation = GetEstimatedPestPopulation(targetPest),
                    InitialPreyPopulation = GetEstimatedPestPopulation(targetPest),
                    PredationRate = GetPredationRate(bioControl.OrganismType, targetPest),
                    ControlEffectiveness = 0f,
                    CreationTime = DateTime.Now
                };
            }
        }
        
        private List<PestType> GetTargetPests(BeneficialOrganismType organismType)
        {
            switch (organismType)
            {
                case BeneficialOrganismType.Ladybugs:
                    return new List<PestType> { PestType.Aphids, PestType.Mealybugs, PestType.Scale };
                case BeneficialOrganismType.PredatoryMites:
                    return new List<PestType> { PestType.SpiderMites, PestType.Thrips };
                case BeneficialOrganismType.LacewingLarvae:
                    return new List<PestType> { PestType.Aphids, PestType.Whiteflies, PestType.Thrips };
                case BeneficialOrganismType.Parasitoids:
                    return new List<PestType> { PestType.Whiteflies, PestType.Leafminers, PestType.Caterpillars };
                case BeneficialOrganismType.PredatoryBeetles:
                    return new List<PestType> { PestType.Fungusgnats, PestType.Caterpillars };
                default:
                    return new List<PestType> { PestType.Aphids };
            }
        }
        
        private float GetHuntingRadius(BeneficialOrganismType organismType)
        {
            switch (organismType)
            {
                case BeneficialOrganismType.Ladybugs: return 0.5f;
                case BeneficialOrganismType.PredatoryMites: return 0.2f;
                case BeneficialOrganismType.LacewingLarvae: return 0.3f;
                case BeneficialOrganismType.Parasitoids: return 1.0f;
                case BeneficialOrganismType.PredatoryBeetles: return 0.8f;
                default: return 0.5f;
            }
        }
        
        private float GetReproductionRate(BeneficialOrganismType organismType)
        {
            switch (organismType)
            {
                case BeneficialOrganismType.Ladybugs: return 0.15f;
                case BeneficialOrganismType.PredatoryMites: return 0.25f;
                case BeneficialOrganismType.LacewingLarvae: return 0.20f;
                case BeneficialOrganismType.Parasitoids: return 0.30f;
                case BeneficialOrganismType.PredatoryBeetles: return 0.10f;
                default: return 0.15f;
            }
        }
        
        private float GetBaseControlEfficiency(BeneficialOrganismType organismType)
        {
            switch (organismType)
            {
                case BeneficialOrganismType.Ladybugs: return 0.75f;
                case BeneficialOrganismType.PredatoryMites: return 0.85f;
                case BeneficialOrganismType.LacewingLarvae: return 0.70f;
                case BeneficialOrganismType.Parasitoids: return 0.80f;
                case BeneficialOrganismType.PredatoryBeetles: return 0.65f;
                default: return 0.70f;
            }
        }
        
        private float GetBaseSurvivalRate(BeneficialOrganismType organismType)
        {
            switch (organismType)
            {
                case BeneficialOrganismType.Ladybugs: return 0.80f;
                case BeneficialOrganismType.PredatoryMites: return 0.75f;
                case BeneficialOrganismType.LacewingLarvae: return 0.70f;
                case BeneficialOrganismType.Parasitoids: return 0.85f;
                case BeneficialOrganismType.PredatoryBeetles: return 0.75f;
                default: return 0.75f;
            }
        }
        
        private Dictionary<string, float> GetEnvironmentalRequirements(BeneficialOrganismType organismType)
        {
            var requirements = new Dictionary<string, float>();
            
            switch (organismType)
            {
                case BeneficialOrganismType.Ladybugs:
                    requirements["Temperature"] = 25f;
                    requirements["Humidity"] = 0.6f;
                    requirements["Light"] = 600f;
                    break;
                case BeneficialOrganismType.PredatoryMites:
                    requirements["Temperature"] = 26f;
                    requirements["Humidity"] = 0.7f;
                    requirements["Light"] = 500f;
                    break;
                default:
                    requirements["Temperature"] = 24f;
                    requirements["Humidity"] = 0.65f;
                    requirements["Light"] = 550f;
                    break;
            }
            
            return requirements;
        }
        
        private BeneficialOrganismType GetOptimalBioControlForPest(PestType pestType)
        {
            switch (pestType)
            {
                case PestType.Aphids: return BeneficialOrganismType.Ladybugs;
                case PestType.SpiderMites: return BeneficialOrganismType.PredatoryMites;
                case PestType.Thrips: return BeneficialOrganismType.PredatoryMites;
                case PestType.Whiteflies: return BeneficialOrganismType.LacewingLarvae;
                case PestType.Fungusgnats: return BeneficialOrganismType.PredatoryBeetles;
                case PestType.Caterpillars: return BeneficialOrganismType.Parasitoids;
                default: return BeneficialOrganismType.Ladybugs;
            }
        }
        
        private int CalculateOptimalReleaseQuantity(PestType pestType, Vector3 location)
        {
            // Base quantity calculation
            int baseQuantity = 50;
            
            // Adjust based on pest type severity
            switch (pestType)
            {
                case PestType.Aphids: baseQuantity = 100; break;
                case PestType.SpiderMites: baseQuantity = 200; break;
                case PestType.Thrips: baseQuantity = 150; break;
                case PestType.Whiteflies: baseQuantity = 120; break;
                case PestType.Fungusgnats: baseQuantity = 80; break;
                case PestType.Caterpillars: baseQuantity = 60; break;
            }
            
            // Environmental factors would be calculated here
            float environmentalFactor = GetEnvironmentalFactor(location);
            
            return Mathf.RoundToInt(baseQuantity * environmentalFactor);
        }
        
        private float GetEnvironmentalFactor(Vector3 location)
        {
            // Simplified environmental assessment
            return UnityEngine.Random.Range(0.8f, 1.2f);
        }
        
        private float CalculateEcosystemStability()
        {
            if (_ecosystemZones.Count == 0) return 0f;
            return _ecosystemZones.Values.Average(e => e.StabilityScore);
        }
        
        private float CalculatePredatorPreyBalance(EcosystemData ecosystem)
        {
            // Simplified predator-prey balance calculation
            return UnityEngine.Random.Range(0.5f, 0.9f);
        }
        
        private int GetSpeciesCount(string zoneId)
        {
            return _activeBioControls.Values.Count(b => Vector3.Distance(b.ReleaseLocation, Vector3.zero) < 10f);
        }
        
        private float CalculateShannonDiversity(string zoneId)
        {
            // Simplified Shannon diversity calculation
            return UnityEngine.Random.Range(0.6f, 0.9f);
        }
        
        private int GetEstimatedPestPopulation(PestType pestType)
        {
            // Simplified pest population estimation
            return UnityEngine.Random.Range(50, 200);
        }
        
        private float GetPredationRate(BeneficialOrganismType predator, PestType prey)
        {
            // Simplified predation rate calculation
            return UnityEngine.Random.Range(0.1f, 0.3f);
        }
        
        private List<string> GetEcosystemInterventions(EcosystemData ecosystem)
        {
            var interventions = new List<string>();
            
            if (ecosystem.StabilityScore < 0.5f)
            {
                interventions.Add("Increase habitat quality");
                interventions.Add("Reduce environmental stress");
            }
            
            if (ecosystem.BiodiversityIndex < 0.6f)
            {
                interventions.Add("Introduce additional species");
                interventions.Add("Create habitat corridors");
            }
            
            return interventions;
        }
        
        #endregion
    }
    
    // Supporting data structures
    [System.Serializable]
    public class BiologicalControlData
    {
        public string ControlId;
        public BeneficialOrganismType OrganismType;
        public Vector3 ReleaseLocation;
        public int InitialPopulation;
        public int CurrentPopulation;
        public DateTime ReleaseTime;
        public bool IsEstablished;
        public float ControlEfficiency;
        public List<PestType> TargetPests;
        public float HuntingRadius;
        public float SurvivalRate;
        public float ReproductionRate;
    }
    
    [System.Serializable]
    public class BiologicalControlProfile
    {
        public BeneficialOrganismType OrganismType;
        public float ControlEfficiency;
        public List<PestType> TargetPests;
        public float HuntingRadius;
        public float ReproductionRate;
        public float SurvivalRate;
        public Dictionary<string, float> EnvironmentalRequirements;
    }
    
    [System.Serializable]
    public class PredatorPreyRelationship
    {
        public string RelationshipId;
        public BeneficialOrganismType PredatorType;
        public PestType PreyType;
        public float PredatorPopulation;
        public float PreyPopulation;
        public float InitialPreyPopulation;
        public float PredationRate;
        public float ControlEffectiveness;
        public DateTime CreationTime;
    }
    
    [System.Serializable]
    public class EcosystemData
    {
        public string ZoneId;
        public float BiodiversityIndex;
        public float StabilityScore;
        public int CarryingCapacity;
        public int CurrentPopulation;
        public float PredatorPreyBalance;
        public float EnvironmentalStress;
        public float HabitatQuality;
        public int SpeciesCount;
    }
    
    [System.Serializable]
    public class BiologicalControlResult
    {
        public string ControlId;
        public bool IsSuccessful;
        public string ResultMessage;
        public float EstimatedEffectiveness;
        public DateTime ReleaseTime;
        public int OrganismsReleased;
    }
    
    [System.Serializable]
    public class BreedingProgramData
    {
        public string ProgramId;
        public BeneficialOrganismType OrganismType;
        public BreedingProgramConfiguration Configuration;
        public DateTime StartTime;
        public bool IsActive;
        public int BreedingCycles;
        public int TotalOffspring;
        public float QualityScore;
        public float GeneticDiversity;
    }
    
    [System.Serializable]
    public class BreedingProgramConfiguration
    {
        public float ProductionRate;
        public float QualityTarget;
        public int MaxCycles;
        public bool EnableGeneticDiversity;
        public float ResourceAllocation;
    }
    
    [System.Serializable]
    public class BreedingProgramResult
    {
        public string ProgramId;
        public bool IsSuccessful;
        public string ResultMessage;
        public float EstimatedProductionRate;
        public float EstimatedQuality;
    }
    
    [System.Serializable]
    public class HabitatData
    {
        public string HabitatId;
        public string AreaId;
        public HabitatOptimizationPlan OptimizationPlan;
        public DateTime CreationTime;
        public bool IsOptimized;
        public float BiodiversityIndex;
        public int CarryingCapacity;
        public float EnvironmentalStability;
    }
    
    [System.Serializable]
    public class HabitatOptimizationPlan
    {
        public int TargetCapacity;
        public float EnvironmentalQuality;
        public int EstimatedDays;
        public List<string> ImprovementActions;
        public float ResourceRequirement;
    }
    
    [System.Serializable]
    public class HabitatOptimizationResult
    {
        public string HabitatId;
        public bool IsSuccessful;
        public string ResultMessage;
        public float EstimatedImprovement;
        public TimeSpan EstimatedCompletionTime;
    }
    
    [System.Serializable]
    public class BiologicalControlConfiguration
    {
        public float ReleaseEfficiency;
        public float EstablishmentRate;
        public float PopulationGrowthRate;
        public float SurvivalRate;
        public float HuntingEfficiency;
        public bool EnableAutomaticRelease;
        public bool EnableEcosystemBalance;
    }
    
    [System.Serializable]
    public class BiologicalControlStatusReport
    {
        public int TotalActiveBioControls;
        public int TotalOrganismsReleased;
        public int SuccessfulEstablishments;
        public int TotalPestsControlled;
        public float AverageControlEfficiency;
        public float EcosystemStability;
        public float BiodiversityIndex;
        public int ActivePredatorPreyRelations;
        public int EcosystemZones;
        public DateTime LastUpdated;
    }
    
    [System.Serializable]
    public class BiologicalControlMetrics
    {
        public int TotalOrganismsReleased;
        public int SuccessfulEstablishments;
        public int TotalPestsControlled;
        public float AverageControlEfficiency;
        public float EcosystemStability;
        public DateTime LastUpdated;
    }
    
    [System.Serializable]
    public class PredatorSuccessData
    {
        public string PredatorId;
        public BeneficialOrganismType PredatorType;
        public PestType PreyType;
        public int PestsControlled;
        public float ControlEfficiency;
        public DateTime SuccessTime;
    }
    
    [System.Serializable]
    public class EcosystemBalanceData
    {
        public string ZoneId;
        public float StabilityScore;
        public bool RequiresIntervention;
        public List<string> RecommendedActions;
    }
    
    [System.Serializable]
    public class PopulationUpdateData
    {
        public string ControlId;
        public BeneficialOrganismType OrganismType;
        public int CurrentPopulation;
        public int PopulationChange;
        public bool IsEstablished;
        public float ControlEfficiency;
    }
    
    // Supporting system classes
    public class OrganismReleaseSystem
    {
        private float _efficiency;
        
        public OrganismReleaseSystem(float efficiency)
        {
            _efficiency = efficiency;
        }
        
        public BiologicalControlResult ReleaseOrganisms(BiologicalControlData bioControl)
        {
            return new BiologicalControlResult
            {
                ControlId = bioControl.ControlId,
                IsSuccessful = true,
                ResultMessage = "Organisms released successfully",
                EstimatedEffectiveness = _efficiency,
                ReleaseTime = bioControl.ReleaseTime,
                OrganismsReleased = bioControl.InitialPopulation
            };
        }
    }
    
    public class PredatorTrackingSystem
    {
        private float _huntingEfficiency;
        
        public PredatorTrackingSystem(float huntingEfficiency)
        {
            _huntingEfficiency = huntingEfficiency;
        }
    }
    
    public class EcosystemBalanceSystem
    {
        private float _stabilityThreshold;
        
        public EcosystemBalanceSystem(float stabilityThreshold)
        {
            _stabilityThreshold = stabilityThreshold;
        }
    }
    
    public class BiologicalControlAnalytics
    {
        public BiologicalControlAnalytics()
        {
            // Initialize analytics
        }
    }
}