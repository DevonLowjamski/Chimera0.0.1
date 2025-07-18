using UnityEngine;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Systems.Genetics; // Added for advanced TraitExpressionEngine
using TraitExpressionEngine = ProjectChimera.Systems.Genetics.TraitExpressionEngine; // Use Systems version
using TraitExpressionResult = ProjectChimera.Systems.Genetics.TraitExpressionResult; // Resolve ambiguous reference
using StressResponse = ProjectChimera.Systems.Genetics.StressResponse; // Resolve ambiguous reference  
using StressFactor = ProjectChimera.Systems.Genetics.StressFactor; // Resolve ambiguous reference
using EnvironmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions; // Use Cultivation version to match PlantInstance
using PlantGenotype = ProjectChimera.Data.Genetics.PlantGenotype;
using EnvironmentalManager = ProjectChimera.Systems.Environment.EnvironmentalManager; // Add EnvironmentalManager alias
using GxE_ProfileSO = ProjectChimera.Data.Environment.GxE_ProfileSO; // Add specific alias for GxE_ProfileSO to avoid namespace conflicts
using GameManager = ProjectChimera.Core.GameManager; // Add GameManager for accessing managers
using GeneticPerformanceStats = ProjectChimera.Systems.Cultivation.GeneticPerformanceStats; // Use Systems version
using GeneticsPerformanceStats = ProjectChimera.Systems.Genetics.GeneticsPerformanceStats; // Use Genetics version
using HarvestResults = ProjectChimera.Systems.Cultivation.HarvestResults; // Use Systems version
using SystemsHarvestResults = ProjectChimera.Systems.Cultivation.SystemsHarvestResults; // Use Systems version
using CultivationEnvironmentalConditions = ProjectChimera.Systems.Cultivation.CultivationEnvironmentalConditions; // Use Systems version
using System.Collections.Generic;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Phase 3.1: Enhanced Plant Update Processor with TraitExpressionEngine integration.
    /// Processes plant updates including growth calculations, health management,
    /// environmental response processing, and advanced genetic trait expression.
    /// </summary>
    public class PlantUpdateProcessor
    {
        private readonly bool _enableStressSystem;
        private readonly bool _enableGxEInteractions;
        private readonly bool _enableAdvancedGenetics;
        private readonly TraitExpressionEngine _traitExpressionEngine;
        
        // Performance optimization
        private readonly Dictionary<string, TraitExpressionResult> _traitExpressionCache = new Dictionary<string, TraitExpressionResult>();
        private float _lastCacheUpdate = 0f;
        private const float CACHE_UPDATE_INTERVAL = 5f; // Update cache every 5 seconds
        
        public PlantUpdateProcessor(bool enableStressSystem = true, bool enableGxEInteractions = true, bool enableAdvancedGenetics = true)
        {
            _enableStressSystem = enableStressSystem;
            _enableGxEInteractions = enableGxEInteractions;
            _enableAdvancedGenetics = enableAdvancedGenetics;
            
            // Initialize advanced trait expression engine with performance optimization
            if (_enableAdvancedGenetics)
            {
                _traitExpressionEngine = new TraitExpressionEngine(
                    enableEpistasis: true, 
                    enablePleiotropy: true, 
                    enableGPUAcceleration: true // Phase 3.2: Enable GPU acceleration
                );
            }
        }
        
        /// <summary>
        /// Phase 3.1: Enhanced plant update with genetic trait expression integration.
        /// Updates a single plant's state including growth, health, environmental responses, and genetic trait expression.
        /// </summary>
        public void UpdatePlant(PlantInstance plant, float deltaTime, float globalGrowthModifier)
        {
            if (plant == null || !plant.IsActive)
                return;
            
            // Get current environmental conditions for the plant
            var environmentalConditions = GetPlantEnvironmentalConditions(plant);
            
            // Calculate genetic trait expression if advanced genetics is enabled
            TraitExpressionResult traitExpression = null;
            if (_enableAdvancedGenetics && plant.Genotype != null)
            {
                traitExpression = CalculateTraitExpression(plant, environmentalConditions);
            }
            
            // Update plant with genetic trait expression data
            if (traitExpression != null)
            {
                UpdatePlantWithGeneticTraits(plant, traitExpression, deltaTime, globalGrowthModifier);
            }
            else
            {
                // Fallback to basic update if no genetic data available
                plant.UpdatePlant(deltaTime, globalGrowthModifier);
            }
        }
        
        /// <summary>
        /// Phase 3.2: Enhanced trait expression calculation with performance optimization.
        /// Uses the high-performance TraitExpressionEngine with automatic optimization selection.
        /// </summary>
        private TraitExpressionResult CalculateTraitExpression(PlantInstance plant, ProjectChimera.Data.Cultivation.EnvironmentalConditions environment)
        {
            string cacheKey = $"{plant.PlantID}_{environment.GetHashCode()}";
            
            // Check cache first for performance optimization
            if (_traitExpressionCache.TryGetValue(cacheKey, out var cachedResult))
            {
                if (Time.time - _lastCacheUpdate < CACHE_UPDATE_INTERVAL)
                {
                    return cachedResult;
                }
            }
            
            // Calculate new trait expression using optimized engine
            var plantGenotype = CreatePlantGenotypeFromInstance(plant);
            if (plantGenotype == null)
                return null;
            
            var traitExpression = _traitExpressionEngine.CalculateExpression(plantGenotype, environment);
            
            // Update cache
            _traitExpressionCache[cacheKey] = traitExpression;
            _lastCacheUpdate = Time.time;
            
            return traitExpression;
        }
        
        /// <summary>
        /// Phase 3.2: Batch processing for multiple plants using optimized genetic calculations.
        /// Automatically selects optimal processing method based on batch size.
        /// </summary>
        public void UpdatePlantsBatch(List<PlantInstance> plants, float deltaTime, float globalGrowthModifier)
        {
            if (plants == null || plants.Count == 0)
                return;
            
            // Prepare batch data for genetic calculations
            var batchData = new List<(PlantGenotype, ProjectChimera.Data.Cultivation.EnvironmentalConditions)>();
            var validPlants = new List<PlantInstance>();
            
            foreach (var plant in plants)
            {
                if (plant == null || !plant.IsActive)
                    continue;
                
                var environmentalConditions = GetPlantEnvironmentalConditions(plant);
                var plantGenotype = CreatePlantGenotypeFromInstance(plant);
                
                if (plantGenotype != null)
                {
                    batchData.Add((plantGenotype, environmentalConditions));
                    validPlants.Add(plant);
                }
            }
            
            if (batchData.Count == 0)
                return;
            
            // Use batch processing for genetic calculations
            List<TraitExpressionResult> batchResults;
            if (_enableAdvancedGenetics && batchData.Count > 1)
            {
                batchResults = _traitExpressionEngine.CalculateExpressionBatch(batchData);
            }
            else
            {
                // Fallback to individual calculations
                batchResults = new List<TraitExpressionResult>();
                foreach (var (genotype, environment) in batchData)
                {
                    batchResults.Add(_traitExpressionEngine.CalculateExpression(genotype, environment));
                }
            }
            
            // Apply results to plants
            for (int i = 0; i < validPlants.Count && i < batchResults.Count; i++)
            {
                var plant = validPlants[i];
                var traitExpression = batchResults[i];
                
                if (traitExpression != null)
                {
                    UpdatePlantWithGeneticTraits(plant, traitExpression, deltaTime, globalGrowthModifier);
                }
                else
                {
                    // Fallback to basic update
                    plant.UpdatePlant(deltaTime, globalGrowthModifier);
                }
            }
        }
        
        /// <summary>
        /// Update plant state using genetic trait expression results.
        /// </summary>
        private void UpdatePlantWithGeneticTraits(PlantInstance plant, TraitExpressionResult traitExpression, 
            float deltaTime, float globalGrowthModifier)
        {
            // Store the trait expression result in the plant for other systems to access
            plant.SetLastTraitExpression(traitExpression);
            
            // Apply genetic trait effects to plant growth
            ApplyGeneticTraitEffects(plant, traitExpression, deltaTime, globalGrowthModifier);
            
            // Update plant's basic systems
            plant.UpdatePlant(deltaTime, globalGrowthModifier);
            
            // Apply stress response effects if present
            if (traitExpression.StressResponse != null)
            {
                ApplyStressResponseEffects(plant, traitExpression.StressResponse, deltaTime);
            }
        }
        
        /// <summary>
        /// Apply genetic trait effects to plant growth and development.
        /// </summary>
        private void ApplyGeneticTraitEffects(PlantInstance plant, TraitExpressionResult traitExpression, 
            float deltaTime, float globalGrowthModifier)
        {
            // Apply height trait effects
            if (traitExpression.HeightExpression > 0f)
            {
                float heightGrowthModifier = traitExpression.HeightExpression;
                plant.ApplyHeightGrowthModifier(heightGrowthModifier, deltaTime);
            }
            
            // Apply THC trait effects (affects potency)
            if (traitExpression.THCExpression > 0f)
            {
                plant.ApplyPotencyModifier(traitExpression.THCExpression);
            }
            
            // Apply CBD trait effects (affects medicinal value)
            if (traitExpression.CBDExpression > 0f)
            {
                plant.ApplyCBDModifier(traitExpression.CBDExpression);
            }
            
            // Apply yield trait effects
            if (traitExpression.YieldExpression > 0f)
            {
                plant.ApplyYieldModifier(traitExpression.YieldExpression);
            }
            
            // Apply overall genetic fitness effects
            plant.ApplyGeneticFitnessModifier(traitExpression.OverallFitness);
        }
        
        /// <summary>
        /// Apply environmental stress response effects to the plant.
        /// </summary>
        private void ApplyStressResponseEffects(PlantInstance plant, StressResponse stressResponse, float deltaTime)
        {
            // Apply overall stress level effects
            if (stressResponse.OverallStressLevel > 0.1f)
            {
                float stressHealthPenalty = stressResponse.OverallStressLevel * 0.1f * deltaTime;
                plant.ApplyHealthChange(-stressHealthPenalty);
                
                // Apply stress-specific effects
                foreach (var stressFactor in stressResponse.ActiveStresses)
                {
                    ApplySpecificStressEffect(plant, stressFactor, deltaTime);
                }
            }
            
            // Apply adaptive capacity benefits
            if (stressResponse.AdaptiveCapacity > 0.7f)
            {
                float adaptiveBonus = (stressResponse.AdaptiveCapacity - 0.7f) * 0.05f * deltaTime;
                plant.ApplyHealthChange(adaptiveBonus);
            }
        }
        
        /// <summary>
        /// Apply specific stress factor effects to the plant.
        /// </summary>
        private void ApplySpecificStressEffect(PlantInstance plant, StressFactor stressFactor, float deltaTime)
        {
            // Convert Systems.Genetics.StressType to the appropriate plant stress methods
            switch (stressFactor.StressType)
            {
                case ProjectChimera.Systems.Genetics.StressType.Temperature:
                case ProjectChimera.Systems.Genetics.StressType.Heat:
                case ProjectChimera.Systems.Genetics.StressType.Cold:
                    plant.ApplyTemperatureStress(stressFactor.Severity, deltaTime);
                    break;
                case ProjectChimera.Systems.Genetics.StressType.Light:
                    plant.ApplyLightStress(stressFactor.Severity, deltaTime);
                    break;
                case ProjectChimera.Systems.Genetics.StressType.Water:
                case ProjectChimera.Systems.Genetics.StressType.Drought:
                case ProjectChimera.Systems.Genetics.StressType.Flood:
                    plant.ApplyWaterStress(stressFactor.Severity, deltaTime);
                    break;
                case ProjectChimera.Systems.Genetics.StressType.Nutrient:
                    plant.ApplyNutrientStress(stressFactor.Severity, deltaTime);
                    break;
                case ProjectChimera.Systems.Genetics.StressType.Atmospheric:
                    plant.ApplyAtmosphericStress(stressFactor.Severity, deltaTime);
                    break;
                default:
                    // For unmapped stress types, apply as general health stress
                    plant.ApplyHealthChange(-stressFactor.Severity * 0.1f * deltaTime);
                    break;
            }
        }
        
        /// <summary>
        /// Get environmental conditions for a plant (returns Data.Cultivation.EnvironmentalConditions directly).
        /// </summary>
        private ProjectChimera.Data.Cultivation.EnvironmentalConditions GetPlantEnvironmentalConditions(PlantInstance plant)
        {
            // Get environmental conditions directly from the plant (returns Data.Cultivation.EnvironmentalConditions)
            ProjectChimera.Data.Cultivation.EnvironmentalConditions dataConditions = plant.GetCurrentEnvironmentalConditions();
            
            // Validate that the conditions are initialized (struct cannot be null)
            if (dataConditions.IsInitialized())
            {
                return dataConditions;
            }
            
            // Fallback to environmental manager if plant conditions are not available
            var environmentalManager = GameManager.Instance?.GetManager<ProjectChimera.Systems.Environment.EnvironmentalManager>();
            if (environmentalManager != null)
            {
                ProjectChimera.Data.Cultivation.EnvironmentalConditions cultivationConditions = environmentalManager.GetCultivationConditions(plant.transform.position);
                return cultivationConditions;
            }
            
            // Final fallback to default indoor conditions
            return ProjectChimera.Data.Cultivation.EnvironmentalConditions.CreateIndoorDefault();
        }
        
        /// <summary>
        /// Convert from Environment.EnvironmentalConditions to Data.Cultivation.EnvironmentalConditions.
        /// </summary>
        private ProjectChimera.Data.Cultivation.EnvironmentalConditions ConvertEnvironmentToCultivationConditions(ProjectChimera.Data.Environment.EnvironmentalConditions envConditions)
        {
            var cultivationConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions.CreateIndoorDefault();
            
            // Map basic environmental properties
            cultivationConditions.Temperature = envConditions.Temperature;
            cultivationConditions.Humidity = envConditions.Humidity;
            cultivationConditions.CO2Level = envConditions.CO2Level;
            cultivationConditions.LightIntensity = envConditions.LightIntensity;
            
            return cultivationConditions;
        }
        
        /// <summary>
        /// Convert from Systems.Cultivation.EnvironmentalConditions to Data.Cultivation.EnvironmentalConditions.
        /// Note: This method is currently not used in the main flow since PlantInstance.GetCurrentEnvironmentalConditions() 
        /// already returns Data.Cultivation.EnvironmentalConditions, but kept for future use cases where conversion is needed.
        /// </summary>
        private ProjectChimera.Data.Cultivation.EnvironmentalConditions ConvertToDataCultivationConditions(CultivationEnvironmentalConditions systemsConditions)
        {
            // Create a new Data.Cultivation.EnvironmentalConditions struct using the static factory method
            var dataConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions.CreateIndoorDefault();
            
            // Assign basic environmental properties that exist in both versions (using property setters)
            dataConditions.Temperature = systemsConditions.Temperature;
            dataConditions.Humidity = systemsConditions.Humidity;
            dataConditions.LightIntensity = systemsConditions.LightIntensity;
            dataConditions.CO2Level = systemsConditions.CO2Level;
            dataConditions.AirFlow = systemsConditions.AirCirculation / 100f; // Systems uses AirCirculation (percentage), Data uses AirFlow (factor)
            
            // Map property names that differ between Systems and Data versions
            // Note: Only setting properties that have public setters, avoiding SerializeField fields
            dataConditions.pH = systemsConditions.pH;
            dataConditions.PhotoperiodHours = systemsConditions.PhotoperiodHours;
            dataConditions.WaterAvailability = systemsConditions.MoisureLevel; // Systems uses MoisureLevel, Data uses WaterAvailability
            
            // Calculate VPD from temperature and humidity since Systems version doesn't have VPD
            dataConditions.VPD = ProjectChimera.Data.Cultivation.EnvironmentalConditions.CalculateVPD(
                systemsConditions.Temperature, 
                systemsConditions.Humidity);
            
            // Only set the ElectricalConductivity if it has a public setter
            dataConditions.ElectricalConductivity = systemsConditions.EC; // Systems uses EC, Data uses ElectricalConductivity
            
            // Note: Avoiding setting SerializeField fields like ColorTemperature, OxygenLevel, etc.
            // These will keep their default values from CreateIndoorDefault()
            
            // Calculate any derived values
            dataConditions.CalculateDerivedValues();
            
            return dataConditions;
        }
        
        /// <summary>
        /// Create a PlantGenotype from a PlantInstance for trait expression calculations.
        /// </summary>
        private PlantGenotype CreatePlantGenotypeFromInstance(PlantInstance plant)
        {
            if (plant.Genotype == null)
                return null;
            
            return new PlantGenotype
            {
                GenotypeID = plant.PlantID,
                StrainOrigin = plant.Strain,
                Generation = 1, // Would be calculated from breeding history
                IsFounder = true, // Would be determined from breeding history
                CreationDate = System.DateTime.Now,
                ParentIDs = new List<string>(),
                Genotype = ConvertGenotypeDataToGenotype(plant.Genotype),
                OverallFitness = plant.Genotype.OverallFitness,
                InbreedingCoefficient = 0f, // Would be calculated from breeding history
                Mutations = new List<MutationRecord>()
            };
        }
        
        /// <summary>
        /// Convert GenotypeDataSO to the format required by TraitExpressionEngine.
        /// </summary>
        private Dictionary<string, AlleleCouple> ConvertGenotypeDataToGenotype(GenotypeDataSO genotypeData)
        {
            var genotype = new Dictionary<string, AlleleCouple>();
            
            if (genotypeData.GenePairs != null)
            {
                foreach (var genePair in genotypeData.GenePairs)
                {
                    if (genePair?.Gene != null)
                    {
                        var alleleCouple = new AlleleCouple(genePair.Allele1, genePair.Allele2);
                        genotype[genePair.Gene.GeneCode] = alleleCouple;
                    }
                }
            }
            
            return genotype;
        }
        
        /// <summary>
        /// Clear trait expression cache to free memory.
        /// </summary>
        public void ClearTraitExpressionCache()
        {
            _traitExpressionCache.Clear();
        }
        
        /// <summary>
        /// Phase 3.2: Enhanced performance monitoring with detailed genetic calculation metrics.
        /// </summary>
        public (int cacheSize, float lastUpdate) GetCacheStatistics()
        {
            return (_traitExpressionCache.Count, _lastCacheUpdate);
        }
        
        /// <summary>
        /// Get comprehensive performance metrics from the trait expression engine.
        /// </summary>
        public GeneticPerformanceStats GetPerformanceMetrics()
        {
            if (_enableAdvancedGenetics && _traitExpressionEngine != null)
            {
                var geneticsStats = _traitExpressionEngine.GetPerformanceMetrics().GetStats();
                // Convert GeneticsPerformanceStats to GeneticPerformanceStats
                return new GeneticPerformanceStats
                {
                    TotalCalculations = geneticsStats.TotalCalculations,
                    AverageCalculationTimeMs = geneticsStats.AverageCalculationTimeMs,
                    CacheHitRatio = geneticsStats.CacheHitRatio,
                    BatchCalculations = geneticsStats.BatchCalculations,
                    AverageBatchTimeMs = geneticsStats.AverageBatchTimeMs,
                    AverageUpdateTimeMs = 0.0 // Default value
                };
            }
            
            return new GeneticPerformanceStats
            {
                TotalCalculations = 0,
                AverageCalculationTimeMs = 0.0,
                CacheHitRatio = 0.0,
                BatchCalculations = 0,
                AverageBatchTimeMs = 0.0,
                AverageUpdateTimeMs = 0.0
            };
        }
        
        /// <summary>
        /// Optimize performance by clearing caches and resetting metrics.
        /// </summary>
        public void OptimizePerformance()
        {
            ClearTraitExpressionCache();
            
            if (_enableAdvancedGenetics && _traitExpressionEngine != null)
            {
                _traitExpressionEngine.ClearCache();
            }
            
            // Force garbage collection to clean up pooled objects
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            
            Debug.Log("PlantUpdateProcessor: Performance optimization completed");
        }
    }
    
    /// <summary>
    /// Calculates plant growth rates and progression based on multiple factors.
    /// </summary>
    public class PlantGrowthCalculator
    {
        private PlantStrainSO _strain;
        private PhenotypicTraits _traits;
        private AnimationCurve _growthCurve;
        
        public void Initialize(PlantStrainSO strain, PhenotypicTraits traits)
        {
            _strain = strain;
            _traits = traits;
            
            // Initialize growth curve from strain data or use default
            _growthCurve = strain.GrowthCurve ?? CreateDefaultGrowthCurve();
        }
        
        /// <summary>
        /// Calculates the growth rate for the current frame.
        /// </summary>
        public float CalculateGrowthRate(PlantGrowthStage stage, float environmentalFitness, float health, float globalModifier)
        {
            // Base growth rate from strain genetics
            float baseRate = GetBaseGrowthRateForStage(stage);
            
            // Apply environmental fitness
            float environmentalModifier = Mathf.Lerp(0.2f, 1.5f, environmentalFitness);
            
            // Apply health modifier
            float healthModifier = Mathf.Lerp(0.1f, 1.2f, health);
            
            // Apply strain-specific growth characteristics
            float strainModifier = _strain?.GrowthRateModifier ?? 1f;
            
            // Apply phenotypic expression
            float phenotypeModifier = CalculatePhenotypeGrowthModifier(stage);
            
            // Combine all modifiers
            float finalRate = baseRate * environmentalModifier * healthModifier * strainModifier * phenotypeModifier * globalModifier;
            
            return Mathf.Max(0f, finalRate);
        }
        
        /// <summary>
        /// Calculates harvest results based on plant's final state.
        /// </summary>
        public SystemsHarvestResults CalculateHarvestResults(float finalHealth, float qualityPotential, PhenotypicTraits traits)
        {
            var results = new SystemsHarvestResults
            {
                FinalHealth = finalHealth,
                QualityScore = CalculateQualityScore(finalHealth, qualityPotential),
                FloweringDays = traits.FloweringTime,
                HarvestDate = System.DateTime.Now
            };
            
            // Calculate yield based on multiple factors
            results.TotalYieldGrams = CalculateYield(finalHealth, traits);
            
            // Calculate cannabinoid and terpene profiles
            results.Cannabinoids = CalculateCannabinoidProfile(finalHealth, qualityPotential);
            results.Terpenes = CalculateTerpeneProfile(finalHealth, qualityPotential);
            
            return results;
        }
        
        private float GetBaseGrowthRateForStage(PlantGrowthStage stage)
        {
            // Different stages have different base growth rates
            switch (stage)
            {
                case PlantGrowthStage.Seed:
                    return 0.005f; // Very slow initial growth
                case PlantGrowthStage.Germination:
                    return 0.01f;
                case PlantGrowthStage.Seedling:
                    return 0.015f;
                case PlantGrowthStage.Vegetative:
                    return 0.02f; // Peak growth rate
                case PlantGrowthStage.Flowering:
                    return 0.012f; // Slower during flowering
                case PlantGrowthStage.Harvest:
                    return 0f; // No growth when ready for harvest
                default:
                    return 0.01f;
            }
        }
        
        private float CalculatePhenotypeGrowthModifier(PlantGrowthStage stage)
        {
            if (_traits == null)
                return 1f;
            
            // Different traits affect growth at different stages
            switch (stage)
            {
                case PlantGrowthStage.Vegetative:
                    return Mathf.Lerp(0.8f, 1.3f, _traits.YieldMultiplier);
                case PlantGrowthStage.Flowering:
                    return Mathf.Lerp(0.9f, 1.2f, _traits.PotencyMultiplier);
                default:
                    return 1f;
            }
        }
        
        private float CalculateYield(float health, PhenotypicTraits traits)
        {
            float baseYield = _strain?.BaseYieldGrams ?? 100f;
            float healthModifier = Mathf.Lerp(0.3f, 1.2f, health);
            float traitModifier = traits.YieldMultiplier;
            
            return baseYield * healthModifier * traitModifier;
        }
        
        private float CalculateQualityScore(float health, float qualityPotential)
        {
            float healthComponent = health * 0.4f;
            float potentialComponent = qualityPotential * 0.6f;
            
            return Mathf.Clamp01(healthComponent + potentialComponent);
        }
        
        private CannabinoidProfile CalculateCannabinoidProfile(float health, float quality)
        {
            if (_strain?.CannabinoidProfile == null)
                return new CannabinoidProfile();
            
            var profile = new CannabinoidProfile
            {
                THC = _strain.CannabinoidProfile.ThcPercentage * health * quality,
                CBD = _strain.CannabinoidProfile.CbdPercentage * health * quality,
                CBG = _strain.CannabinoidProfile.CbgPercentage * health * quality,
                CBN = _strain.CannabinoidProfile.CbnPercentage * health * quality
            };
            
            return profile;
        }
        
        private TerpeneProfile CalculateTerpeneProfile(float health, float quality)
        {
            if (_strain?.TerpeneProfile == null)
                return new TerpeneProfile();
            
            var profile = new TerpeneProfile
            {
                Myrcene = _strain.TerpeneProfile.Myrcene * health * quality,
                Limonene = _strain.TerpeneProfile.Limonene * health * quality,
                Pinene = _strain.TerpeneProfile.Pinene * health * quality,
                Linalool = _strain.TerpeneProfile.Linalool * health * quality,
                Caryophyllene = _strain.TerpeneProfile.Caryophyllene * health * quality
            };
            
            return profile;
        }
        
        private AnimationCurve CreateDefaultGrowthCurve()
        {
            var curve = new AnimationCurve();
            curve.AddKey(0f, 0f);
            curve.AddKey(0.2f, 0.1f);
            curve.AddKey(0.5f, 0.5f);
            curve.AddKey(0.8f, 0.9f);
            curve.AddKey(1f, 1f);
            
            return curve;
        }
    }
    
    /// <summary>
    /// Manages plant health, stress responses, and disease resistance.
    /// </summary>
    public class PlantHealthSystem
    {
        private PlantStrainSO _strain;
        private float _currentHealth = 1f;
        private float _maxHealth = 1f;
        private float _stressLevel = 0f;
        private float _diseaseResistance = 1f;
        private float _recoveryRate = 0.1f;
        
        public void Initialize(PlantStrainSO strain, float diseaseResistance)
        {
            _strain = strain;
            _maxHealth = strain?.BaseHealthModifier ?? 1f;
            _currentHealth = _maxHealth;
            _diseaseResistance = diseaseResistance;
            _recoveryRate = strain?.HealthRecoveryRate ?? 0.1f;
        }
        
        /// <summary>
        /// Updates health based on stressors and environmental conditions.
        /// </summary>
        public void UpdateHealth(float deltaTime, List<ActiveStressor> stressors, float environmentalFitness)
        {
            // Calculate stress damage
            float stressDamage = CalculateStressDamage(stressors, deltaTime);
            
            // Calculate environmental health effects
            float environmentalEffect = CalculateEnvironmentalHealthEffect(environmentalFitness, deltaTime);
            
            // Apply natural recovery
            float naturalRecovery = _recoveryRate * deltaTime;
            
            // Update health
            float healthChange = environmentalEffect + naturalRecovery - stressDamage;
            _currentHealth = Mathf.Clamp(_currentHealth + healthChange, 0f, _maxHealth);
            
            // Update stress level
            UpdateStressLevel(stressors);
        }
        
        public float GetCurrentHealth() => _currentHealth;
        public float GetStressLevel() => _stressLevel;
        public float GetHealthPercentage() => _currentHealth / _maxHealth;
        
        private float CalculateStressDamage(List<ActiveStressor> stressors, float deltaTime)
        {
            float totalDamage = 0f;
            
            foreach (var stressor in stressors)
            {
                if (!stressor.IsActive)
                    continue;
                
                float damage = stressor.Intensity * stressor.StressSource.DamagePerSecond * deltaTime;
                
                // Apply disease resistance
                if (stressor.StressSource.StressType == ProjectChimera.Data.Environment.StressType.Biotic)
                {
                    damage *= (1f - _diseaseResistance);
                }
                
                totalDamage += damage;
            }
            
            return totalDamage;
        }
        
        private float CalculateEnvironmentalHealthEffect(float environmentalFitness, float deltaTime)
        {
            // Good environmental conditions promote health recovery
            if (environmentalFitness > 0.8f)
            {
                return (environmentalFitness - 0.8f) * 0.5f * deltaTime;
            }
            // Poor conditions cause slow health decline
            else if (environmentalFitness < 0.4f)
            {
                return (environmentalFitness - 0.4f) * 0.2f * deltaTime;
            }
            
            return 0f;
        }
        
        private void UpdateStressLevel(List<ActiveStressor> stressors)
        {
            _stressLevel = 0f;
            
            foreach (var stressor in stressors)
            {
                if (stressor.IsActive)
                {
                    _stressLevel += stressor.Intensity * stressor.StressSource.StressMultiplier;
                }
            }
            
            _stressLevel = Mathf.Clamp01(_stressLevel);
        }
    }
    
    /// <summary>
    /// Handles environmental response calculations and GxE interactions.
    /// </summary>
    public class EnvironmentalResponseSystem
    {
        private PlantStrainSO _strain;
        private GxE_ProfileSO _gxeProfile;
        private float _environmentalFitness = 1f;
        private ProjectChimera.Data.Cultivation.EnvironmentalConditions _currentConditions;
        
        public void Initialize(PlantStrainSO strain)
        {
            _strain = strain;
            _gxeProfile = strain?.GxEProfile;
        }
        
        /// <summary>
        /// Updates environmental responses and calculates fitness.
        /// </summary>
        public void UpdateEnvironmentalResponse(ProjectChimera.Data.Cultivation.EnvironmentalConditions conditions, float deltaTime)
        {
            _currentConditions = conditions;
            _environmentalFitness = CalculateEnvironmentalFitness(conditions);
        }
        
        /// <summary>
        /// Processes changes in environmental conditions.
        /// </summary>
        public void ProcessEnvironmentalChange(ProjectChimera.Data.Cultivation.EnvironmentalConditions previous, ProjectChimera.Data.Cultivation.EnvironmentalConditions current)
        {
            // Calculate stress from rapid environmental changes
            if (previous.Temperature != 0f) // Valid previous conditions
            {
                float tempChange = Mathf.Abs(current.Temperature - previous.Temperature);
                float humidityChange = Mathf.Abs(current.Humidity - previous.Humidity);
                
                // Rapid changes can cause stress
                if (tempChange > 5f || humidityChange > 20f)
                {
                    // This would trigger stress responses
                    Debug.Log($"Rapid environmental change detected - Temp: {tempChange:F1}°C, Humidity: {humidityChange:F1}%");
                }
            }
        }
        
        /// <summary>
        /// Processes environmental adaptation for the plant.
        /// </summary>
        public void ProcessAdaptation(ProjectChimera.Data.Cultivation.EnvironmentalConditions conditions, float adaptationRate)
        {
            if (_strain?.GxEProfile == null)
                return;
            
            // Update current conditions
            _currentConditions = conditions;
            
            // Calculate adaptation effects based on environmental fitness and adaptation rate
            float currentFitness = CalculateEnvironmentalFitness(conditions);
            
            // Apply adaptation over time - plants gradually adapt to their environment
            if (currentFitness < _environmentalFitness)
            {
                // Adapting to worse conditions - slower process
                _environmentalFitness = Mathf.Lerp(_environmentalFitness, currentFitness, adaptationRate * 0.5f);
            }
            else
            {
                // Adapting to better conditions - faster process
                _environmentalFitness = Mathf.Lerp(_environmentalFitness, currentFitness, adaptationRate);
            }
            
            // Clamp to ensure valid range
            _environmentalFitness = Mathf.Clamp01(_environmentalFitness);
            
            // Log significant adaptation changes
            if (Mathf.Abs(_environmentalFitness - currentFitness) > 0.1f)
            {
                Debug.Log($"Plant adapting to environment - Current fitness: {currentFitness:F2}, Adapted fitness: {_environmentalFitness:F2}");
            }
        }
        
        public float GetEnvironmentalFitness() => _environmentalFitness;
        
        private float CalculateEnvironmentalFitness(ProjectChimera.Data.Cultivation.EnvironmentalConditions conditions)
        {
            if (_strain?.BaseSpecies == null)
                return 1f;
            
            float tempFitness = CalculateTemperatureFitness(conditions.Temperature);
            float humidityFitness = CalculateHumidityFitness(conditions.Humidity);
            float lightFitness = CalculateLightFitness(conditions.LightIntensity);
            float co2Fitness = CalculateCO2Fitness(conditions.CO2Level);
            
            // Weighted average of all environmental factors
            float fitness = (tempFitness * 0.3f) + (humidityFitness * 0.25f) + 
                           (lightFitness * 0.3f) + (co2Fitness * 0.15f);
            
            return Mathf.Clamp01(fitness);
        }
        
        private float CalculateTemperatureFitness(float temperature)
        {
            var optimalConditions = _strain.BaseSpecies.GetOptimalEnvironment();
            var optimal = optimalConditions.Temperature;
            var temperatureRange = _strain.BaseSpecies.TemperatureRange;
            
            // Check if temperature is within optimal range
            if (temperature >= temperatureRange.x && temperature <= temperatureRange.y)
                return 1f;
            
            // Calculate distance from nearest edge of optimal range
            float distance = Mathf.Min(Mathf.Abs(temperature - temperatureRange.x), 
                                     Mathf.Abs(temperature - temperatureRange.y));
            
            // Linear falloff beyond optimal range
            float rangeSize = temperatureRange.y - temperatureRange.x;
            float falloffRange = rangeSize * 0.5f;
            float fitness = 1f - (distance / falloffRange);
            
            return Mathf.Clamp01(fitness);
        }
        
        private float CalculateHumidityFitness(float humidity)
        {
            var optimalConditions = _strain.BaseSpecies.GetOptimalEnvironment();
            var optimal = optimalConditions.Humidity;
            var humidityRange = _strain.BaseSpecies.HumidityRange;
            
            // Check if humidity is within optimal range
            if (humidity >= humidityRange.x && humidity <= humidityRange.y)
                return 1f;
            
            // Calculate distance from nearest edge of optimal range
            float distance = Mathf.Min(Mathf.Abs(humidity - humidityRange.x), 
                                     Mathf.Abs(humidity - humidityRange.y));
            
            // Linear falloff beyond optimal range
            float rangeSize = humidityRange.y - humidityRange.x;
            float falloffRange = rangeSize * 0.5f;
            float fitness = 1f - (distance / falloffRange);
            
            return Mathf.Clamp01(fitness);
        }
        
        private float CalculateLightFitness(float lightIntensity)
        {
            var optimalConditions = _strain.BaseSpecies.GetOptimalEnvironment();
            var optimal = optimalConditions.LightIntensity;
            var lightRange = _strain.BaseSpecies.LightIntensityRange;
            
            // Check if light intensity is within optimal range
            if (lightIntensity >= lightRange.x && lightIntensity <= lightRange.y)
                return 1f;
            
            // Calculate distance from nearest edge of optimal range
            float distance = Mathf.Min(Mathf.Abs(lightIntensity - lightRange.x), 
                                     Mathf.Abs(lightIntensity - lightRange.y));
            
            // Linear falloff beyond optimal range
            float rangeSize = lightRange.y - lightRange.x;
            float falloffRange = rangeSize * 0.5f;
            float fitness = 1f - (distance / falloffRange);
            
            return Mathf.Clamp01(fitness);
        }
        
        private float CalculateCO2Fitness(float co2Level)
        {
            var optimalConditions = _strain.BaseSpecies.GetOptimalEnvironment();
            var optimal = optimalConditions.CO2Level;
            var co2Range = _strain.BaseSpecies.Co2Range;
            
            // Check if CO2 level is within optimal range
            if (co2Level >= co2Range.x && co2Level <= co2Range.y)
                return 1f;
            
            // Calculate distance from nearest edge of optimal range
            float distance = Mathf.Min(Mathf.Abs(co2Level - co2Range.x), 
                                     Mathf.Abs(co2Level - co2Range.y));
            
            // Linear falloff beyond optimal range
            float rangeSize = co2Range.y - co2Range.x;
            float falloffRange = rangeSize * 0.5f;
            float fitness = 1f - (distance / falloffRange);
            
            return Mathf.Clamp01(fitness);
        }
    }
}