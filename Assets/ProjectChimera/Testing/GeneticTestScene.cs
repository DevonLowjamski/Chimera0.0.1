using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Systems.Genetics;
using System.Collections.Generic;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Phase 0.2: Test scene with three plants demonstrating genetic inheritance
    /// Creates HH (homozygous dominant), Hh (heterozygous), hh (homozygous recessive) genotypes
    /// for Height trait expression validation
    /// </summary>
    public class GeneticTestScene : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool _autoCreateTestPlantsOnStart = true;
        [SerializeField] private bool _enableDebugLogging = true;
        [SerializeField] private float _testPlantSpacing = 2f;
        [SerializeField] private Vector3 _testAreaCenter = Vector3.zero;
        
        [Header("Genetic Test Parameters")]
        [SerializeField] private string _testGeneLocusName = "HEIGHT_MAIN";
        [SerializeField] private float _dominantAlleleEffect = 1.5f; // +50% height
        [SerializeField] private float _recessiveAlleleEffect = -0.3f; // -30% height
        [SerializeField] private string _testStrainName = "Test Strain";
        
        [Header("System References")]
        [SerializeField] private DataManager _dataManager;
        [SerializeField] private GeneticsManager _geneticsManager;
        
        // Test plants
        private PlantInstanceSO _plantHH; // Homozygous dominant (tallest)
        private PlantInstanceSO _plantHh; // Heterozygous (medium height)
        private PlantInstanceSO _planthh; // Homozygous recessive (shortest)
        
        // Test genetic components
        private GeneDefinitionSO _heightGene;
        private AlleleSO _dominantAllele;
        private AlleleSO _recessiveAllele;
        private PlantStrainSO _testStrain;
        private GenotypeDataSO _genotypeHH;
        private GenotypeDataSO _genotypeHh;
        private GenotypeDataSO _genotypehh;
        
        void Start()
        {
            if (_autoCreateTestPlantsOnStart)
            {
                InitializeTestScene();
            }
        }
        
        /// <summary>
        /// Initialize the complete genetic test scene
        /// </summary>
        [ContextMenu("Initialize Test Scene")]
        public void InitializeTestScene()
        {
            LogDebug("=== INITIALIZING GENETIC TEST SCENE ===");
            
            // Find system references if not assigned
            FindSystemReferences();
            
            // Create genetic test data
            CreateTestGeneticData();
            
            // Create test plants
            CreateTestPlants();
            
            // Run initial trait calculations
            RunInitialGeneticTests();
            
            LogDebug("=== GENETIC TEST SCENE INITIALIZED ===");
        }
        
        /// <summary>
        /// Find required system references
        /// </summary>
        private void FindSystemReferences()
        {
            if (_dataManager == null)
                _dataManager = FindObjectOfType<DataManager>();
            
            if (_geneticsManager == null)
                _geneticsManager = FindObjectOfType<GeneticsManager>();
            
            if (_dataManager == null)
                LogError("DataManager not found! Please assign or ensure one exists in the scene.");
            
            if (_geneticsManager == null)
                LogError("GeneticsManager not found! Please assign or ensure one exists in the scene.");
        }
        
        /// <summary>
        /// Create the genetic test data (genes, alleles, strain, genotypes)
        /// </summary>
        private void CreateTestGeneticData()
        {
            LogDebug("Creating test genetic data...");
            
            // Create height gene definition
            _heightGene = CreateHeightGene();
            
            // Create dominant and recessive alleles
            _dominantAllele = CreateDominantAllele();
            _recessiveAllele = CreateRecessiveAllele();
            
            // Create test strain
            _testStrain = CreateTestStrain();
            
            // Create three different genotypes
            _genotypeHH = CreateGenotype("HH", _dominantAllele, _dominantAllele);
            _genotypeHh = CreateGenotype("Hh", _dominantAllele, _recessiveAllele);
            _genotypehh = CreateGenotype("hh", _recessiveAllele, _recessiveAllele);
            
            LogDebug("Test genetic data created successfully");
        }
        
        /// <summary>
        /// Create the height gene definition
        /// </summary>
        private GeneDefinitionSO CreateHeightGene()
        {
            var gene = ScriptableObject.CreateInstance<GeneDefinitionSO>();
            
            // Use reflection to set private fields since properties are read-only
            var geneType = typeof(GeneDefinitionSO);
            geneType.GetField("_geneName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gene, "Height Main Gene");
            geneType.GetField("_geneSymbol", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gene, "HGT1");
            geneType.GetField("_geneCode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gene, _testGeneLocusName);
            geneType.GetField("_dominanceType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gene, DominanceType.Incomplete);
            geneType.GetField("_inheritancePattern", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gene, InheritancePattern.Mendelian);
            geneType.GetField("_environmentallyRegulated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gene, true);
            
            // Create influenced traits list
            var influencedTraits = new List<TraitInfluence>
            {
                new TraitInfluence
                {
                    TraitType = PlantTrait.Height,
                    InfluenceStrength = 1.0f,
                    IsPositiveEffect = true,
                    InfluenceDescription = "Primary height control gene"
                }
            };
            geneType.GetField("_influencedTraits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gene, influencedTraits);
            
            LogDebug($"Created height gene: {gene.GeneName}");
            return gene;
        }
        
        /// <summary>
        /// Create dominant allele (increases height)
        /// </summary>
        private AlleleSO CreateDominantAllele()
        {
            var allele = ScriptableObject.CreateInstance<AlleleSO>();
            
            // Use reflection to set private fields since properties are read-only
            var alleleType = typeof(AlleleSO);
            alleleType.GetField("_alleleCode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, "H");
            alleleType.GetField("_alleleName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, "Tall Height Allele");
            alleleType.GetField("_parentGene", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, _heightGene);
            alleleType.GetField("_isDominant", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, true);
            alleleType.GetField("_isRecessive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, false);
            alleleType.GetField("_effectStrength", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, _dominantAlleleEffect);
            
            // Create trait effect for height
            var heightEffect = new TraitEffect
            {
                AffectedTrait = PlantTrait.Height,
                EffectMagnitude = _dominantAlleleEffect,
                IsMainEffect = true
            };
            
            var traitEffects = new List<TraitEffect> { heightEffect };
            alleleType.GetField("_traitEffects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, traitEffects);
            
            LogDebug($"Created dominant allele: {allele.AlleleCode} (effect: +{_dominantAlleleEffect * 100:F0}%)");
            return allele;
        }
        
        /// <summary>
        /// Create recessive allele (decreases height)
        /// </summary>
        private AlleleSO CreateRecessiveAllele()
        {
            var allele = ScriptableObject.CreateInstance<AlleleSO>();
            
            // Use reflection to set private fields since properties are read-only
            var alleleType = typeof(AlleleSO);
            alleleType.GetField("_alleleCode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, "h");
            alleleType.GetField("_alleleName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, "Short Height Allele");
            alleleType.GetField("_parentGene", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, _heightGene);
            alleleType.GetField("_isDominant", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, false);
            alleleType.GetField("_isRecessive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, true);
            alleleType.GetField("_effectStrength", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, _recessiveAlleleEffect);
            
            // Create trait effect for height
            var heightEffect = new TraitEffect
            {
                AffectedTrait = PlantTrait.Height,
                EffectMagnitude = _recessiveAlleleEffect,
                IsMainEffect = true
            };
            
            var traitEffects = new List<TraitEffect> { heightEffect };
            alleleType.GetField("_traitEffects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(allele, traitEffects);
            
            LogDebug($"Created recessive allele: {allele.AlleleCode} (effect: {_recessiveAlleleEffect * 100:F0}%)");
            return allele;
        }
        
        /// <summary>
        /// Create test plant strain
        /// </summary>
        private PlantStrainSO CreateTestStrain()
        {
            var strain = ScriptableObject.CreateInstance<PlantStrainSO>();
            strain.name = _testStrainName;
            strain.StrainName = _testStrainName;
            strain.StrainType = StrainType.Hybrid;
            strain.BaseHeight = 150f; // 150cm base height
            strain.BaseWidth = 90f;
            strain.BaseBiomass = 100f;
            strain.FloweringTimeWeeks = 8;
            
            LogDebug($"Created test strain: {strain.StrainName} (base height: {strain.BaseHeight}cm)");
            return strain;
        }
        
        /// <summary>
        /// Create a genotype with specific allele combination
        /// </summary>
        private GenotypeDataSO CreateGenotype(string genotypeId, AlleleSO allele1, AlleleSO allele2)
        {
            var genotype = ScriptableObject.CreateInstance<GenotypeDataSO>();
            genotype.IndividualID = $"TEST_{genotypeId}";
            genotype.Species = "Cannabis sativa";
            genotype.ParentStrain = _testStrain;
            genotype.IsViable = true;
            genotype.OverallFitness = CalculateFitnessFromAlleles(allele1, allele2);
            genotype.GeneticDiversity = 0.5f;
            
            // Create gene pair for height
            var genePair = new GenePair
            {
                Gene = _heightGene,
                Allele1 = allele1,
                Allele2 = allele2
            };
            
            genotype.GenePairs = new List<GenePair> { genePair };
            
            // Calculate predicted height trait
            var predictedHeight = CalculatePredictedHeight(allele1, allele2);
            var predictedTrait = new PredictedTrait
            {
                Trait = PlantTrait.Height,
                PredictedValue = predictedHeight,
                Confidence = 0.9f,
                EnvironmentDependent = true,
                PredictionRange = new Vector2(predictedHeight * 0.8f, predictedHeight * 1.2f)
            };
            
            genotype.PredictedTraits = new List<PredictedTrait> { predictedTrait };
            
            LogDebug($"Created genotype {genotypeId}: predicted height {predictedHeight:F1}cm (fitness: {genotype.OverallFitness:F2})");
            return genotype;
        }
        
        /// <summary>
        /// Calculate fitness based on allele combination
        /// </summary>
        private float CalculateFitnessFromAlleles(AlleleSO allele1, AlleleSO allele2)
        {
            float baseFitness = 0.8f;
            
            // Heterozygote advantage for genetic diversity
            if (allele1.AlleleCode != allele2.AlleleCode)
            {
                baseFitness += 0.1f; // Heterozygote bonus
            }
            
            // Extreme heights might have lower fitness
            float avgEffect = (allele1.EffectStrength + allele2.EffectStrength) * 0.5f;
            float extremePenalty = Mathf.Abs(avgEffect) > 1.0f ? 0.1f : 0f;
            
            return Mathf.Clamp(baseFitness - extremePenalty, 0.3f, 1.0f);
        }
        
        /// <summary>
        /// Calculate predicted height from allele combination
        /// </summary>
        private float CalculatePredictedHeight(AlleleSO allele1, AlleleSO allele2)
        {
            float baseHeight = _testStrain.BaseHeight;
            
            // Use incomplete dominance (intermediate expression)
            float combinedEffect = (allele1.EffectStrength + allele2.EffectStrength) * 0.5f;
            float modifiedHeight = baseHeight * (1f + combinedEffect);
            
            return Mathf.Clamp(modifiedHeight, baseHeight * 0.5f, baseHeight * 2.0f);
        }
        
        /// <summary>
        /// Create the three test plants with different genotypes
        /// </summary>
        private void CreateTestPlants()
        {
            LogDebug("Creating test plants...");
            
            // Create plant positions
            Vector3 plantHHPos = _testAreaCenter + Vector3.left * _testPlantSpacing;
            Vector3 plantHhPos = _testAreaCenter;
            Vector3 planthhPos = _testAreaCenter + Vector3.right * _testPlantSpacing;
            
            // Create HH plant (homozygous dominant - tallest)
            _plantHH = CreateTestPlant("Plant_HH", _genotypeHH, plantHHPos);
            
            // Create Hh plant (heterozygous - medium height)
            _plantHh = CreateTestPlant("Plant_Hh", _genotypeHh, plantHhPos);
            
            // Create hh plant (homozygous recessive - shortest)
            _planthh = CreateTestPlant("Plant_hh", _genotypehh, planthhPos);
            
            LogDebug("Test plants created successfully");
        }
        
        /// <summary>
        /// Create a single test plant with specified genotype
        /// </summary>
        private PlantInstanceSO CreateTestPlant(string plantName, GenotypeDataSO genotype, Vector3 position)
        {
            var plant = ScriptableObject.CreateInstance<PlantInstanceSO>();
            
            // Initialize the plant
            plant.InitializePlant(
                plantID: System.Guid.NewGuid().ToString(),
                plantName: plantName,
                strain: _testStrain,
                genotype: genotype,
                worldPosition: position
            );
            
            LogDebug($"Created test plant: {plantName} at {position} with genotype {genotype.IndividualID}");
            return plant;
        }
        
        /// <summary>
        /// Run initial genetic tests to demonstrate trait expression
        /// </summary>
        [ContextMenu("Run Genetic Tests")]
        public void RunInitialGeneticTests()
        {
            LogDebug("=== RUNNING GENETIC TESTS ===");
            
            if (_plantHH == null || _plantHh == null || _planthh == null)
            {
                LogError("Test plants not created. Run InitializeTestScene first.");
                return;
            }
            
            // Create test environment
            var testEnvironment = EnvironmentalConditions.CreateIndoorDefault();
            testEnvironment.Temperature = 24f;
            testEnvironment.LightIntensity = 600f;
            testEnvironment.CO2Level = 1200f;
            
            LogDebug($"Test environment: {testEnvironment.Temperature}°C, {testEnvironment.LightIntensity} PPFD, {testEnvironment.CO2Level} ppm");
            
            // Process growth for each plant to trigger genetic calculations
            ProcessPlantGrowth(_plantHH, testEnvironment, "HH (Homozygous Dominant)");
            ProcessPlantGrowth(_plantHh, testEnvironment, "Hh (Heterozygous)");
            ProcessPlantGrowth(_planthh, testEnvironment, "hh (Homozygous Recessive)");
            
            // Compare results
            CompareGeneticResults();
            
            LogDebug("=== GENETIC TESTS COMPLETED ===");
        }
        
        /// <summary>
        /// Process growth for a plant and log genetic calculations
        /// </summary>
        private void ProcessPlantGrowth(PlantInstanceSO plant, EnvironmentalConditions environment, string description)
        {
            LogDebug($"\n--- Testing {description} ---");
            
            // Process daily growth (this will trigger genetic trait calculations)
            plant.ProcessDailyGrowth(environment, 1f);
            
            // Log results
            LogDebug($"Plant: {plant.PlantName}");
            LogDebug($"  Genotype: {plant.Genotype?.IndividualID}");
            LogDebug($"  Current Height: {plant.CurrentHeight:F2}cm");
            LogDebug($"  Calculated Max Height: {plant.CalculatedMaxHeight:F2}cm");
            LogDebug($"  Overall Health: {plant.OverallHealth:F2}");
            LogDebug($"  Age: {plant.AgeInDays:F1} days");
            
            // Check if genetic expression was calculated
            if (plant.LastTraitExpression != null)
            {
                LogDebug($"  Genetic Expression: {plant.LastTraitExpression.HeightExpression:F2}m");
                LogDebug($"  Genetic Fitness: {plant.LastTraitExpression.OverallFitness:F2}");
                LogDebug($"  Environment Factors: {plant.LastTraitExpression.EnvironmentalFactors}");
            }
            else
            {
                LogDebug("  No genetic expression calculated yet");
            }
        }
        
        /// <summary>
        /// Compare and summarize genetic test results
        /// </summary>
        private void CompareGeneticResults()
        {
            LogDebug("\n=== GENETIC INHERITANCE COMPARISON ===");
            
            float heightHH = _plantHH.CalculatedMaxHeight;
            float heightHh = _plantHh.CalculatedMaxHeight;
            float heighthh = _planthh.CalculatedMaxHeight;
            
            LogDebug($"Expected Height Ranking: HH > Hh > hh");
            LogDebug($"Actual Heights:");
            LogDebug($"  HH (Dom/Dom): {heightHH:F1}cm");
            LogDebug($"  Hh (Dom/Rec): {heightHh:F1}cm");
            LogDebug($"  hh (Rec/Rec): {heighthh:F1}cm");
            
            // Validate genetic inheritance
            bool inheritanceValid = heightHH >= heightHh && heightHh >= heighthh;
            LogDebug($"Genetic Inheritance Valid: {inheritanceValid}");
            
            if (inheritanceValid)
            {
                LogDebug("✓ Genetic inheritance working correctly!");
                LogDebug($"  Height difference HH-hh: {heightHH - heighthh:F1}cm");
                LogDebug($"  Heterozygote intermediate: {(heightHh > heighthh && heightHh < heightHH)}");
            }
            else
            {
                LogDebug("✗ Genetic inheritance needs adjustment");
            }
            
            // Calculate fitness comparison
            float fitnessHH = _plantHH.LastTraitExpression?.OverallFitness ?? 0f;
            float fitnessHh = _plantHh.LastTraitExpression?.OverallFitness ?? 0f;
            float fitnesshh = _planthh.LastTraitExpression?.OverallFitness ?? 0f;
            
            LogDebug($"Fitness Comparison:");
            LogDebug($"  HH: {fitnessHH:F3}");
            LogDebug($"  Hh: {fitnessHh:F3} (heterozygote advantage: {fitnessHh > fitnessHH && fitnessHh > fitnesshh})");
            LogDebug($"  hh: {fitnesshh:F3}");
        }
        
        /// <summary>
        /// Simulate growth over multiple days to see genetic expression over time
        /// </summary>
        [ContextMenu("Simulate 30 Days Growth")]
        public void SimulateGrowthOverTime()
        {
            if (_plantHH == null || _plantHh == null || _planthh == null)
            {
                LogError("Test plants not created. Run InitializeTestScene first.");
                return;
            }
            
            LogDebug("=== SIMULATING 30 DAYS OF GROWTH ===");
            
            var environment = EnvironmentalConditions.CreateIndoorDefault();
            environment.Temperature = 24f;
            environment.LightIntensity = 600f;
            environment.CO2Level = 1200f;
            
            // Simulate 30 days of growth
            for (int day = 1; day <= 30; day++)
            {
                _plantHH.ProcessDailyGrowth(environment, 1f);
                _plantHh.ProcessDailyGrowth(environment, 1f);
                _planthh.ProcessDailyGrowth(environment, 1f);
                
                // Log every 5 days
                if (day % 5 == 0)
                {
                    LogDebug($"Day {day}: HH={_plantHH.CurrentHeight:F1}cm, Hh={_plantHh.CurrentHeight:F1}cm, hh={_planthh.CurrentHeight:F1}cm");
                }
            }
            
            LogDebug("=== 30-DAY SIMULATION COMPLETE ===");
            CompareGeneticResults();
        }
        
        /// <summary>
        /// Get test results for external systems
        /// </summary>
        public GeneticTestResults GetTestResults()
        {
            if (_plantHH == null || _plantHh == null || _planthh == null)
                return null;
            
            return new GeneticTestResults
            {
                PlantHH = _plantHH,
                PlantHh = _plantHh,
                Planthh = _planthh,
                HeightHH = _plantHH.CalculatedMaxHeight,
                HeightHh = _plantHh.CalculatedMaxHeight,
                Heighthh = _planthh.CalculatedMaxHeight,
                InheritanceValid = _plantHH.CalculatedMaxHeight >= _plantHh.CalculatedMaxHeight && 
                                  _plantHh.CalculatedMaxHeight >= _planthh.CalculatedMaxHeight,
                TestStrain = _testStrain,
                TestEnvironment = EnvironmentalConditions.CreateIndoorDefault()
            };
        }
        
        private void LogDebug(string message)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[GeneticTestScene] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[GeneticTestScene] {message}");
        }
    }
    
    /// <summary>
    /// Results from genetic test scene
    /// </summary>
    [System.Serializable]
    public class GeneticTestResults
    {
        public PlantInstanceSO PlantHH;
        public PlantInstanceSO PlantHh;
        public PlantInstanceSO Planthh;
        public float HeightHH;
        public float HeightHh;
        public float Heighthh;
        public bool InheritanceValid;
        public PlantStrainSO TestStrain;
        public EnvironmentalConditions TestEnvironment;
    }
}