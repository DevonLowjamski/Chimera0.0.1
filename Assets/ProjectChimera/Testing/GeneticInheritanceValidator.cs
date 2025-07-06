using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Systems.Genetics;
using ProjectChimera.UI;

namespace ProjectChimera.Testing
{
    /// <summary>
    /// Phase 1.1: Genetic Inheritance Validator
    /// Validates Mendelian inheritance through Debug Inspector integration.
    /// Creates test breeding scenarios and verifies genetic principles.
    /// </summary>
    public class GeneticInheritanceValidator : MonoBehaviour
    {
        [Header("Validation Configuration")]
        [SerializeField] private bool _autoRunValidationOnStart = true;
        [SerializeField] private bool _enableContinuousValidation = false;
        [SerializeField] private float _validationInterval = 10f;
        [SerializeField] private bool _enableDetailedLogging = true;
        
        [Header("Test Parameters")]
        [SerializeField] private int _testOffspringCount = 20;
        [SerializeField] private int _testGenerations = 3;
        [SerializeField] private bool _enableMutationsInTests = false;
        [SerializeField] private float _testMutationRate = 0f;
        
        [Header("Validation Thresholds")]
        [SerializeField] private float _mendelianRatioTolerance = 0.15f; // 15% tolerance for Mendelian ratios
        [SerializeField] private float _inheritanceConfidenceThreshold = 0.8f;
        [SerializeField] private float _fitnessVariationThreshold = 0.3f;
        
        [Header("System References")]
        [SerializeField] private DataManager _dataManager;
        [SerializeField] private GeneticsManager _geneticsManager;
        [SerializeField] private DebugInspector _debugInspector;
        [SerializeField] private BreedingUI _breedingUI;
        
        // Test Data
        private GeneticTestScene _testScene;
        private BreedingSimulator _breedingSimulator;
        private List<ValidationResult> _validationResults = new List<ValidationResult>();
        private Dictionary<string, TestGenotype> _testGenotypes = new Dictionary<string, TestGenotype>();
        
        // Validation State
        private bool _isValidationRunning = false;
        private bool _isInitialized = false;
        private DateTime _lastValidationTime;
        private int _totalValidationRuns = 0;
        private int _passedValidations = 0;
        private int _failedValidations = 0;
        
        void Start()
        {
            Initialize();
            
            if (_autoRunValidationOnStart)
            {
                RunComprehensiveValidation();
            }
            
            if (_enableContinuousValidation)
            {
                InvokeRepeating(nameof(RunInheritanceValidation), _validationInterval, _validationInterval);
            }
        }
        
        private void Initialize()
        {
            LogInfo("Initializing Genetic Inheritance Validator...");
            
            FindSystemReferences();
            SetupBreedingSimulator();
            SetupTestScene();
            CreateTestGenotypes();
            
            _isInitialized = true;
            LogInfo("Genetic Inheritance Validator initialized successfully");
        }
        
        private void FindSystemReferences()
        {
            if (_dataManager == null)
                _dataManager = FindObjectOfType<DataManager>();
            
            if (_geneticsManager == null)
                _geneticsManager = FindObjectOfType<GeneticsManager>();
            
            if (_debugInspector == null)
                _debugInspector = FindObjectOfType<DebugInspector>();
            
            if (_breedingUI == null)
                _breedingUI = FindObjectOfType<BreedingUI>();
            
            if (_testScene == null)
                _testScene = FindObjectOfType<GeneticTestScene>();
        }
        
        private void SetupBreedingSimulator()
        {
            _breedingSimulator = new BreedingSimulator(allowInbreeding: true, inbreedingDepression: 0.1f);
        }
        
        private void SetupTestScene()
        {
            if (_testScene != null)
            {
                _testScene.InitializeTestScene();
            }
        }
        
        private void CreateTestGenotypes()
        {
            LogInfo("Creating test genotypes for validation...");
            
            // Create standardized test genotypes for validation
            _testGenotypes["HH"] = CreateTestGenotype("HH", "H", "H", 1.5f, 1.5f);
            _testGenotypes["Hh"] = CreateTestGenotype("Hh", "H", "h", 1.5f, -0.3f);
            _testGenotypes["hh"] = CreateTestGenotype("hh", "h", "h", -0.3f, -0.3f);
            
            LogInfo($"Created {_testGenotypes.Count} test genotypes");
        }
        
        private TestGenotype CreateTestGenotype(string id, string allele1Code, string allele2Code, 
            float allele1Effect, float allele2Effect)
        {
            var testGenotype = new TestGenotype
            {
                GenotypeID = id,
                ExpectedHeightMultiplier = 1f + (allele1Effect + allele2Effect) * 0.5f,
                AlleleCodesExpected = new List<string> { allele1Code, allele2Code }
            };
            
            // Create the actual PlantGenotype for breeding
            testGenotype.PlantGenotype = new PlantGenotype
            {
                GenotypeID = $"TEST_{id}",
                StrainOrigin = CreateTestStrain(),
                Generation = 0,
                IsFounder = true,
                CreationDate = DateTime.Now,
                ParentIDs = new List<string>(),
                Genotype = new Dictionary<string, AlleleCouple>(),
                OverallFitness = 0.8f,
                InbreedingCoefficient = 0f,
                Mutations = new List<GeneticMutation>()
            };
            
            // Create height gene alleles
            var allele1 = CreateTestAllele(allele1Code, allele1Effect);
            var allele2 = CreateTestAllele(allele2Code, allele2Effect);
            var alleleCouple = new AlleleCouple(allele1, allele2);
            
            testGenotype.PlantGenotype.Genotype["HEIGHT_MAIN"] = alleleCouple;
            
            return testGenotype;
        }
        
        private AlleleSO CreateTestAllele(string code, float effect)
        {
            var allele = ScriptableObject.CreateInstance<AlleleSO>();
            allele.AlleleCode = code;
            allele.AlleleName = $"Test Allele {code}";
            allele.EffectStrength = effect;
            allele.IsDominant = code == "H";
            allele.IsRecessive = code == "h";
            
            var heightEffect = new TraitEffect
            {
                AffectedTrait = PlantTrait.Height,
                EffectMagnitude = effect,
                IsMainEffect = true
            };
            
            allele.TraitEffects = new List<TraitEffect> { heightEffect };
            
            return allele;
        }
        
        private PlantStrainSO CreateTestStrain()
        {
            var strain = ScriptableObject.CreateInstance<PlantStrainSO>();
            strain.name = "Test Validation Strain";
            strain.StrainName = "Test Validation Strain";
            strain.StrainType = StrainType.Hybrid;
            strain.BaseHeight = 150f;
            strain.BaseWidth = 90f;
            strain.BaseBiomass = 100f;
            strain.FloweringTimeWeeks = 8;
            
            return strain;
        }
        
        /// <summary>
        /// Run comprehensive genetic inheritance validation
        /// </summary>
        [ContextMenu("Run Comprehensive Validation")]
        public void RunComprehensiveValidation()
        {
            if (!_isInitialized || _isValidationRunning)
            {
                LogWarning("Cannot run validation - system not ready or validation already running");
                return;
            }
            
            LogInfo("=== STARTING COMPREHENSIVE GENETIC INHERITANCE VALIDATION ===");
            _isValidationRunning = true;
            _lastValidationTime = DateTime.Now;
            _totalValidationRuns++;
            
            try
            {
                var allTestsPassed = true;
                
                // Test 1: Basic Mendelian Inheritance (Monohybrid Cross)
                allTestsPassed &= ValidateMonohybridCross();
                
                // Test 2: Heterozygote x Heterozygote Cross
                allTestsPassed &= ValidateHeterozygoteCross();
                
                // Test 3: Backcross Validation
                allTestsPassed &= ValidateBackcross();
                
                // Test 4: Fitness Inheritance
                allTestsPassed &= ValidateFitnessInheritance();
                
                // Test 5: Compatibility Analysis Validation
                allTestsPassed &= ValidateCompatibilityAnalysis();
                
                // Test 6: Integration with Debug Inspector
                allTestsPassed &= ValidateDebugInspectorIntegration();
                
                // Update validation statistics
                if (allTestsPassed)
                {
                    _passedValidations++;
                    LogInfo("✓ ALL VALIDATION TESTS PASSED");
                }
                else
                {
                    _failedValidations++;
                    LogError("✗ SOME VALIDATION TESTS FAILED");
                }
                
                // Generate validation report
                GenerateValidationReport();
            }
            catch (Exception ex)
            {
                LogError($"Error during comprehensive validation: {ex.Message}");
                _failedValidations++;
            }
            finally
            {
                _isValidationRunning = false;
                LogInfo("=== GENETIC INHERITANCE VALIDATION COMPLETE ===");
            }
        }
        
        /// <summary>
        /// Validate basic Mendelian inheritance with monohybrid cross
        /// </summary>
        private bool ValidateMonohybridCross()
        {
            LogInfo("Testing Monohybrid Cross (HH x hh)...");
            
            try
            {
                var parentHH = _testGenotypes["HH"].PlantGenotype;
                var parenthh = _testGenotypes["hh"].PlantGenotype;
                
                var breedingResult = _breedingSimulator.PerformBreeding(
                    parentHH, parenthh, _testOffspringCount, _enableMutationsInTests, _testMutationRate);
                
                if (breedingResult.OffspringGenotypes.Count == 0)
                {
                    LogError("Monohybrid cross failed - no offspring generated");
                    return false;
                }
                
                // All F1 offspring should be Hh (heterozygous)
                var allHeterozygous = true;
                foreach (var offspring in breedingResult.OffspringGenotypes)
                {
                    var heightAlleles = offspring.Genotype.GetValueOrDefault("HEIGHT_MAIN");
                    if (heightAlleles == null)
                    {
                        LogError($"Offspring {offspring.GenotypeID} missing HEIGHT_MAIN alleles");
                        allHeterozygous = false;
                        continue;
                    }
                    
                    var allele1Code = heightAlleles.Allele1?.AlleleCode ?? "";
                    var allele2Code = heightAlleles.Allele2?.AlleleCode ?? "";
                    
                    var isHeterozygous = (allele1Code == "H" && allele2Code == "h") || 
                                        (allele1Code == "h" && allele2Code == "H");
                    
                    if (!isHeterozygous)
                    {
                        LogError($"Offspring {offspring.GenotypeID} not heterozygous: {allele1Code}{allele2Code}");
                        allHeterozygous = false;
                    }
                }
                
                if (allHeterozygous)
                {
                    LogInfo("✓ Monohybrid cross validation passed - all F1 are heterozygous");
                    AddValidationResult("Monohybrid Cross", true, "All F1 offspring are heterozygous as expected");
                    return true;
                }
                else
                {
                    LogError("✗ Monohybrid cross validation failed");
                    AddValidationResult("Monohybrid Cross", false, "Not all F1 offspring are heterozygous");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in monohybrid cross validation: {ex.Message}");
                AddValidationResult("Monohybrid Cross", false, $"Exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validate heterozygote cross (Hh x Hh) showing 3:1 ratio
        /// </summary>
        private bool ValidateHeterozygoteCross()
        {
            LogInfo("Testing Heterozygote Cross (Hh x Hh)...");
            
            try
            {
                var parentHh1 = _testGenotypes["Hh"].PlantGenotype;
                var parentHh2 = _testGenotypes["Hh"].PlantGenotype;
                
                var breedingResult = _breedingSimulator.PerformBreeding(
                    parentHh1, parentHh2, _testOffspringCount, _enableMutationsInTests, _testMutationRate);
                
                if (breedingResult.OffspringGenotypes.Count == 0)
                {
                    LogError("Heterozygote cross failed - no offspring generated");
                    return false;
                }
                
                // Count genotype ratios
                int hhCount = 0, HhCount = 0, HHCount = 0;
                
                foreach (var offspring in breedingResult.OffspringGenotypes)
                {
                    var heightAlleles = offspring.Genotype.GetValueOrDefault("HEIGHT_MAIN");
                    if (heightAlleles == null) continue;
                    
                    var allele1Code = heightAlleles.Allele1?.AlleleCode ?? "";
                    var allele2Code = heightAlleles.Allele2?.AlleleCode ?? "";
                    
                    if ((allele1Code == "H" && allele2Code == "H"))
                        HHCount++;
                    else if ((allele1Code == "H" && allele2Code == "h") || (allele1Code == "h" && allele2Code == "H"))
                        HhCount++;
                    else if ((allele1Code == "h" && allele2Code == "h"))
                        hhCount++;
                }
                
                var totalOffspring = HHCount + HhCount + hhCount;
                if (totalOffspring == 0)
                {
                    LogError("No valid offspring with HEIGHT_MAIN alleles found");
                    return false;
                }
                
                // Expected Mendelian ratio: 1 HH : 2 Hh : 1 hh (or 25% : 50% : 25%)
                var expectedHH = totalOffspring * 0.25f;
                var expectedHh = totalOffspring * 0.50f;
                var expectedhh = totalOffspring * 0.25f;
                
                var hhRatio = Math.Abs((HHCount / (float)totalOffspring) - 0.25f);
                var HhRatio = Math.Abs((HhCount / (float)totalOffspring) - 0.50f);
                var hhRatioActual = Math.Abs((hhCount / (float)totalOffspring) - 0.25f);
                
                bool ratiosValid = hhRatio <= _mendelianRatioTolerance && 
                                  HhRatio <= _mendelianRatioTolerance && 
                                  hhRatioActual <= _mendelianRatioTolerance;
                
                LogInfo($"Heterozygote cross results: HH={HHCount} ({HHCount/(float)totalOffspring:P1}), " +
                       $"Hh={HhCount} ({HhCount/(float)totalOffspring:P1}), " +
                       $"hh={hhCount} ({hhCount/(float)totalOffspring:P1})");
                LogInfo($"Expected ratios: HH=25%, Hh=50%, hh=25% (tolerance: {_mendelianRatioTolerance:P1})");
                
                if (ratiosValid)
                {
                    LogInfo("✓ Heterozygote cross validation passed - ratios within tolerance");
                    AddValidationResult("Heterozygote Cross", true, $"Mendelian ratios within {_mendelianRatioTolerance:P1} tolerance");
                    return true;
                }
                else
                {
                    LogError("✗ Heterozygote cross validation failed - ratios outside tolerance");
                    AddValidationResult("Heterozygote Cross", false, "Mendelian ratios outside tolerance");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in heterozygote cross validation: {ex.Message}");
                AddValidationResult("Heterozygote Cross", false, $"Exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validate backcross inheritance patterns
        /// </summary>
        private bool ValidateBackcross()
        {
            LogInfo("Testing Backcross (Hh x HH)...");
            
            try
            {
                var parentHh = _testGenotypes["Hh"].PlantGenotype;
                var parentHH = _testGenotypes["HH"].PlantGenotype;
                
                var breedingResult = _breedingSimulator.PerformBreeding(
                    parentHh, parentHH, _testOffspringCount, _enableMutationsInTests, _testMutationRate);
                
                if (breedingResult.OffspringGenotypes.Count == 0)
                {
                    LogError("Backcross failed - no offspring generated");
                    return false;
                }
                
                // Count genotype ratios for backcross
                int HhCount = 0, HHCount = 0, otherCount = 0;
                
                foreach (var offspring in breedingResult.OffspringGenotypes)
                {
                    var heightAlleles = offspring.Genotype.GetValueOrDefault("HEIGHT_MAIN");
                    if (heightAlleles == null) continue;
                    
                    var allele1Code = heightAlleles.Allele1?.AlleleCode ?? "";
                    var allele2Code = heightAlleles.Allele2?.AlleleCode ?? "";
                    
                    if ((allele1Code == "H" && allele2Code == "H"))
                        HHCount++;
                    else if ((allele1Code == "H" && allele2Code == "h") || (allele1Code == "h" && allele2Code == "H"))
                        HhCount++;
                    else
                        otherCount++;
                }
                
                var totalOffspring = HHCount + HhCount + otherCount;
                if (totalOffspring == 0)
                {
                    LogError("No valid offspring found in backcross");
                    return false;
                }
                
                // Expected backcross ratio: 1 HH : 1 Hh (50% : 50%), no hh expected
                var HHRatio = Math.Abs((HHCount / (float)totalOffspring) - 0.50f);
                var HhRatio = Math.Abs((HhCount / (float)totalOffspring) - 0.50f);
                
                bool ratiosValid = HHRatio <= _mendelianRatioTolerance && 
                                  HhRatio <= _mendelianRatioTolerance && 
                                  otherCount == 0;
                
                LogInfo($"Backcross results: HH={HHCount} ({HHCount/(float)totalOffspring:P1}), " +
                       $"Hh={HhCount} ({HhCount/(float)totalOffspring:P1}), Other={otherCount}");
                
                if (ratiosValid)
                {
                    LogInfo("✓ Backcross validation passed");
                    AddValidationResult("Backcross", true, "Expected 1:1 ratio achieved");
                    return true;
                }
                else
                {
                    LogError("✗ Backcross validation failed");
                    AddValidationResult("Backcross", false, "Expected 1:1 ratio not achieved");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in backcross validation: {ex.Message}");
                AddValidationResult("Backcross", false, $"Exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validate fitness inheritance and calculation
        /// </summary>
        private bool ValidateFitnessInheritance()
        {
            LogInfo("Testing Fitness Inheritance...");
            
            try
            {
                var parentHH = _testGenotypes["HH"].PlantGenotype;
                var parenthh = _testGenotypes["hh"].PlantGenotype;
                
                // Set different fitness values
                parentHH.OverallFitness = 0.9f;
                parenthh.OverallFitness = 0.7f;
                
                var breedingResult = _breedingSimulator.PerformBreeding(
                    parentHH, parenthh, _testOffspringCount, _enableMutationsInTests, _testMutationRate);
                
                if (breedingResult.OffspringGenotypes.Count == 0)
                {
                    LogError("Fitness inheritance test failed - no offspring generated");
                    return false;
                }
                
                // Check that offspring fitness is reasonable (should show heterozygote advantage)
                var avgOffspringFitness = breedingResult.OffspringGenotypes.Average(o => o.OverallFitness);
                var expectedFitness = (parentHH.OverallFitness + parenthh.OverallFitness) * 0.5f;
                var fitnessVariation = Math.Abs(avgOffspringFitness - expectedFitness);
                
                bool fitnessValid = fitnessVariation <= _fitnessVariationThreshold;
                
                LogInfo($"Parent fitness: HH={parentHH.OverallFitness:F2}, hh={parenthh.OverallFitness:F2}");
                LogInfo($"Average offspring fitness: {avgOffspringFitness:F2} (expected ~{expectedFitness:F2})");
                
                if (fitnessValid)
                {
                    LogInfo("✓ Fitness inheritance validation passed");
                    AddValidationResult("Fitness Inheritance", true, "Offspring fitness within expected range");
                    return true;
                }
                else
                {
                    LogError("✗ Fitness inheritance validation failed");
                    AddValidationResult("Fitness Inheritance", false, "Offspring fitness outside expected range");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in fitness inheritance validation: {ex.Message}");
                AddValidationResult("Fitness Inheritance", false, $"Exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validate compatibility analysis functionality
        /// </summary>
        private bool ValidateCompatibilityAnalysis()
        {
            LogInfo("Testing Compatibility Analysis...");
            
            try
            {
                var parentHH = _testGenotypes["HH"].PlantGenotype;
                var parenthh = _testGenotypes["hh"].PlantGenotype;
                var parentHh = _testGenotypes["Hh"].PlantGenotype;
                
                // Test different compatibility scenarios
                var compatibility1 = _breedingSimulator.AnalyzeCompatibility(parentHH, parenthh);
                var compatibility2 = _breedingSimulator.AnalyzeCompatibility(parentHH, parentHH);
                var compatibility3 = _breedingSimulator.AnalyzeCompatibility(parentHh, parentHh);
                
                bool validAnalysis = true;
                
                // Diverse parents (HH x hh) should have good compatibility but high genetic distance
                if (compatibility1.GeneticDistance <= 0.5f || compatibility1.CompatibilityScore <= 0.3f)
                {
                    LogError("HH x hh compatibility analysis failed");
                    validAnalysis = false;
                }
                
                // Identical parents (HH x HH) should have zero genetic distance but high inbreeding risk
                if (compatibility2.GeneticDistance >= 0.1f || compatibility2.InbreedingRisk <= 0.2f)
                {
                    LogError("HH x HH compatibility analysis failed");
                    validAnalysis = false;
                }
                
                LogInfo($"Compatibility HH x hh: Distance={compatibility1.GeneticDistance:F2}, " +
                       $"Inbreeding={compatibility1.InbreedingRisk:F2}, Score={compatibility1.CompatibilityScore:F2}");
                LogInfo($"Compatibility HH x HH: Distance={compatibility2.GeneticDistance:F2}, " +
                       $"Inbreeding={compatibility2.InbreedingRisk:F2}, Score={compatibility2.CompatibilityScore:F2}");
                
                if (validAnalysis)
                {
                    LogInfo("✓ Compatibility analysis validation passed");
                    AddValidationResult("Compatibility Analysis", true, "All compatibility metrics working correctly");
                    return true;
                }
                else
                {
                    LogError("✗ Compatibility analysis validation failed");
                    AddValidationResult("Compatibility Analysis", false, "Compatibility metrics incorrect");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in compatibility analysis validation: {ex.Message}");
                AddValidationResult("Compatibility Analysis", false, $"Exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Validate integration with Debug Inspector
        /// </summary>
        private bool ValidateDebugInspectorIntegration()
        {
            LogInfo("Testing Debug Inspector Integration...");
            
            try
            {
                if (_debugInspector == null)
                {
                    LogWarning("Debug Inspector not found - skipping integration test");
                    AddValidationResult("Debug Inspector Integration", true, "Debug Inspector not available (optional)");
                    return true;
                }
                
                // Test that Debug Inspector can display genetic information
                var testPlant = _testScene?.GetTestResults()?.PlantHH;
                if (testPlant != null)
                {
                    _debugInspector.SelectPlant(testPlant);
                    var selectedPlant = _debugInspector.GetSelectedPlant();
                    
                    if (selectedPlant == testPlant)
                    {
                        LogInfo("✓ Debug Inspector integration validation passed");
                        AddValidationResult("Debug Inspector Integration", true, "Plant selection and display working");
                        return true;
                    }
                }
                
                LogError("✗ Debug Inspector integration validation failed");
                AddValidationResult("Debug Inspector Integration", false, "Plant selection not working");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error in Debug Inspector integration validation: {ex.Message}");
                AddValidationResult("Debug Inspector Integration", false, $"Exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Run simplified inheritance validation (for continuous monitoring)
        /// </summary>
        public void RunInheritanceValidation()
        {
            if (!_isInitialized || _isValidationRunning) return;
            
            LogInfo("Running inheritance validation check...");
            
            try
            {
                var parentHh1 = _testGenotypes["Hh"].PlantGenotype;
                var parentHh2 = _testGenotypes["Hh"].PlantGenotype;
                
                var breedingResult = _breedingSimulator.PerformBreeding(
                    parentHh1, parentHh2, 10, false, 0f);
                
                if (breedingResult.OffspringGenotypes.Count > 0)
                {
                    LogInfo($"Inheritance validation: Generated {breedingResult.OffspringGenotypes.Count} offspring");
                }
                else
                {
                    LogWarning("Inheritance validation: No offspring generated");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in inheritance validation: {ex.Message}");
            }
        }
        
        private void AddValidationResult(string testName, bool passed, string details)
        {
            var result = new ValidationResult
            {
                TestName = testName,
                Passed = passed,
                Details = details,
                Timestamp = DateTime.Now
            };
            
            _validationResults.Add(result);
        }
        
        private void GenerateValidationReport()
        {
            LogInfo("\n=== VALIDATION REPORT ===");
            LogInfo($"Total Validation Runs: {_totalValidationRuns}");
            LogInfo($"Passed Validations: {_passedValidations}");
            LogInfo($"Failed Validations: {_failedValidations}");
            LogInfo($"Success Rate: {(_passedValidations / (float)Math.Max(1, _totalValidationRuns)):P1}");
            
            LogInfo("\nIndividual Test Results:");
            foreach (var result in _validationResults.TakeLast(10)) // Show last 10 results
            {
                var status = result.Passed ? "✓" : "✗";
                LogInfo($"{status} {result.TestName}: {result.Details}");
            }
            
            LogInfo("=== END REPORT ===\n");
        }
        
        // Public API for external validation requests
        
        public ValidationSummary GetValidationSummary()
        {
            return new ValidationSummary
            {
                TotalRuns = _totalValidationRuns,
                PassedRuns = _passedValidations,
                FailedRuns = _failedValidations,
                LastValidationTime = _lastValidationTime,
                IsValidationRunning = _isValidationRunning,
                RecentResults = _validationResults.TakeLast(5).ToList()
            };
        }
        
        public void RequestValidation()
        {
            if (!_isValidationRunning)
            {
                RunComprehensiveValidation();
            }
        }
        
        // Logging helpers
        
        private void LogInfo(string message)
        {
            if (_enableDetailedLogging)
            {
                Debug.Log($"[GeneticInheritanceValidator] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[GeneticInheritanceValidator] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[GeneticInheritanceValidator] {message}");
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
    
    // Supporting data structures
    
    [Serializable]
    public class TestGenotype
    {
        public string GenotypeID;
        public float ExpectedHeightMultiplier;
        public List<string> AlleleCodesExpected;
        public PlantGenotype PlantGenotype;
    }
    
    [Serializable]
    public class ValidationResult
    {
        public string TestName;
        public bool Passed;
        public string Details;
        public DateTime Timestamp;
    }
    
    [Serializable]
    public class ValidationSummary
    {
        public int TotalRuns;
        public int PassedRuns;
        public int FailedRuns;
        public DateTime LastValidationTime;
        public bool IsValidationRunning;
        public List<ValidationResult> RecentResults;
    }
}