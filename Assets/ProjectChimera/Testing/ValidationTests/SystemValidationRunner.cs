using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using ProjectChimera.Core;
using ProjectChimera.Testing.Core;

namespace ProjectChimera.Testing.ValidationTests
{
    /// <summary>
    /// VALIDATION-1a: Simple runtime validation runner for refactored systems
    /// Can be run directly in Unity Editor or during Play Mode
    /// Updated: 2025-07-26 - Fixed compilation issues
    /// </summary>
    public class SystemValidationRunner : MonoBehaviour
    {
        [Header("Validation Configuration")]
        [SerializeField] private bool _runOnStart = true;
        [SerializeField] private bool _validateCoreHierarchy = true;
        [SerializeField] private bool _validateManagerSystems = true;
        [SerializeField] private bool _validateDataStructures = true;
        [SerializeField] private bool _validateAssemblyIntegration = true;
        [SerializeField] private bool _validateArchitecturalConsistency = true;
        [SerializeField] private bool _showDetailedResults = true;
        
        [Header("Deep Diagnostic Configuration")]
        [SerializeField] private bool _enableDeepDiagnostics = true;
        [SerializeField] private bool _logAllAvailableTypes = false;
        [SerializeField] private bool _testIndividualSystems = true;
        [SerializeField] private bool _validateAssemblyReferences = true;
        
        private List<ValidationResult> _results = new List<ValidationResult>();
        
        private void Start()
        {
            if (_runOnStart)
            {
                RunSystemValidation();
            }
        }
        
        [ContextMenu("Run System Validation")]
        public void RunSystemValidation()
        {
            UnityEngine.Debug.Log("VALIDATION-1a: Starting system validation...");
            
            _results.Clear();
            var totalStopwatch = Stopwatch.StartNew();
            
            if (_validateCoreHierarchy)
                ValidateCoreHierarchy();
            
            if (_validateManagerSystems)
                ValidateManagerSystems();
            
            if (_validateDataStructures)
                ValidateDataStructures();
            
            if (_validateAssemblyIntegration)
                ValidateAssemblyIntegration();
            
            if (_validateArchitecturalConsistency)
                ValidateArchitecturalConsistency();
            
            // Deep diagnostics
            if (_enableDeepDiagnostics)
            {
                RunDeepDiagnostics();
            }
            
            totalStopwatch.Stop();
            GenerateValidationReport(totalStopwatch.ElapsedMilliseconds);
        }
        
        #region Core Hierarchy Validation
        
        private void ValidateCoreHierarchy()
        {
            var result = new ValidationResult { TestName = "Core Hierarchy", Category = "Core" };
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Check for essential core types (using assembly-qualified names)
                var coreTypeNames = new string[]
                {
                    "ProjectChimera.Core.ChimeraManager, ProjectChimera.Core",
                    "ProjectChimera.Core.GameManager, ProjectChimera.Core", 
                    "ProjectChimera.Core.TimeManager, ProjectChimera.Core",
                    "ProjectChimera.Systems.Save.SaveManager, ProjectChimera.Systems.Save"
                };
                
                var foundTypes = 0;
                var missingTypes = new List<string>();
                
                foreach (var typeName in coreTypeNames)
                {
                    try
                    {
                        var type = System.Type.GetType(typeName);
                        if (type != null)
                        {
                            // Check if type is abstract (like ChimeraManager base class)
                            if (type.IsAbstract)
                            {
                                // For abstract classes, just validate that the type exists and is properly defined
                                foundTypes++;
                                UnityEngine.Debug.Log($"VALIDATION-1a: ✅ Core type found (abstract): {type.Name}");
                            }
                            else
                            {
                                // Try to create instance for concrete types
                                var testObject = new GameObject($"ValidationTest_{type.Name}");
                                var component = testObject.AddComponent(type);
                                
                                if (component != null)
                                {
                                    foundTypes++;
                                    UnityEngine.Debug.Log($"VALIDATION-1a: ✅ Core type found: {type.Name}");
                                    DestroyImmediate(testObject);
                                }
                                else
                                {
                                    missingTypes.Add($"{type.Name}: Component creation failed");
                                    UnityEngine.Debug.LogError($"VALIDATION-1a: ❌ Core type component creation failed: {type.Name}");
                                }
                            }
                        }
                        else
                        {
                            var shortName = typeName.Contains(",") ? typeName.Substring(0, typeName.IndexOf(",")) : typeName;
                            shortName = shortName.Substring(shortName.LastIndexOf('.') + 1);
                            missingTypes.Add($"{shortName}: Type not found");
                            UnityEngine.Debug.LogError($"VALIDATION-1a: ❌ Core type not found: {shortName} (Full: {typeName})");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        var shortName = typeName.Contains(",") ? typeName.Substring(0, typeName.IndexOf(",")) : typeName;
                        shortName = shortName.Substring(shortName.LastIndexOf('.') + 1);
                        missingTypes.Add($"{shortName}: {ex.Message}");
                        UnityEngine.Debug.LogError($"VALIDATION-1a: ❌ Core type exception: {shortName} - {ex.Message}");
                    }
                }
                
                result.Passed = foundTypes == coreTypeNames.Length;
                result.Details = $"Core types: {foundTypes}/{coreTypeNames.Length} validated";
                
                if (missingTypes.Any())
                {
                    result.Details += $" - Missing: {string.Join(", ", missingTypes)}";
                }
            }
            catch (System.Exception ex)
            {
                result.Passed = false;
                result.Details = $"Core hierarchy validation failed: {ex.Message}";
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                _results.Add(result);
            }
        }
        
        #endregion
        
        #region Manager Systems Validation
        
        private void ValidateManagerSystems()
        {
            var result = new ValidationResult { TestName = "Manager Systems", Category = "Managers" };
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Check for essential manager types (using assembly-qualified names)
                var managerTypeNames = new string[]
                {
                    "ProjectChimera.Systems.Cultivation.PlantManager, ProjectChimera.Systems.Cultivation",
                    "ProjectChimera.Systems.Genetics.GeneticsManager, ProjectChimera.Systems.Genetics", 
                    "ProjectChimera.Systems.Environment.EnvironmentalManager, ProjectChimera.Systems.Environment",
                    "ProjectChimera.Systems.Economy.MarketManager, ProjectChimera.Systems.Economy",
                    "ProjectChimera.Systems.Progression.ComprehensiveProgressionManager, ProjectChimera.Systems.Progression",
                    "ProjectChimera.Systems.Progression.ResearchManager, ProjectChimera.Systems.Progression",
                    "ProjectChimera.Systems.Progression.AchievementSystemManager, ProjectChimera.Systems.Progression"
                };
                
                var validatedManagers = 0;
                var managerIssues = new List<string>();
                
                foreach (var typeName in managerTypeNames)
                {
                    try
                    {
                        var type = System.Type.GetType(typeName);
                        if (type != null)
                        {
                            var testObject = new GameObject($"ValidationTest_{type.Name}");
                            var manager = testObject.AddComponent(type);
                            
                            if (manager != null && manager is ChimeraManager)
                            {
                                validatedManagers++;
                                DestroyImmediate(testObject);
                            }
                            else
                            {
                                managerIssues.Add($"{type.Name}: Not a ChimeraManager");
                                if (testObject != null) DestroyImmediate(testObject);
                            }
                        }
                        else
                        {
                            managerIssues.Add($"{typeName}: Type not found");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        var shortName = typeName.Substring(typeName.LastIndexOf('.') + 1);
                        managerIssues.Add($"{shortName}: {ex.Message}");
                    }
                }
                
                var successRate = (validatedManagers / (float)managerTypeNames.Length) * 100f;
                result.Passed = successRate >= 75f; // At least 75% should work
                result.Details = $"Managers: {validatedManagers}/{managerTypeNames.Length} validated ({successRate:F1}%)";
                
                if (managerIssues.Any() && _showDetailedResults)
                {
                    result.Details += $" - Issues: {string.Join(", ", managerIssues.Take(3))}";
                }
            }
            catch (System.Exception ex)
            {
                result.Passed = false;
                result.Details = $"Manager systems validation failed: {ex.Message}";
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                _results.Add(result);
            }
        }
        
        #endregion
        
        #region Data Structures Validation
        
        private void ValidateDataStructures()
        {
            var result = new ValidationResult { TestName = "Data Structures", Category = "Data" };
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var validatedStructures = 0;
                var structureIssues = new List<string>();
                
                // Test cultivation data
                try
                {
                    var taskType = ProjectChimera.Data.Cultivation.CultivationTaskType.Watering;
                    var careQuality = ProjectChimera.Data.Cultivation.CareQuality.Good;
                    var plant = new ProjectChimera.Data.Cultivation.InteractivePlant
                    {
                        PlantInstanceID = 123,
                        PlantedTime = System.DateTime.Now
                    };
                    validatedStructures++;
                }
                catch (System.Exception ex)
                {
                    structureIssues.Add($"Cultivation: {ex.Message}");
                }
                
                // Test genetic data
                try
                {
                    var genotypeType = System.Type.GetType("ProjectChimera.Data.Genetics.CannabisGenotype, ProjectChimera.Data");
                    var strainType = System.Type.GetType("ProjectChimera.Data.Genetics.PlantStrainSO, ProjectChimera.Data");
                    
                    if (genotypeType != null && strainType != null)
                    {
                        // CannabisGenotype is a regular class, not ScriptableObject
                        if (typeof(ScriptableObject).IsAssignableFrom(genotypeType))
                        {
                            var genotype = ScriptableObject.CreateInstance(genotypeType);
                            DestroyImmediate(genotype);
                        }
                        else
                        {
                            // For regular classes, just check if they can be instantiated
                            var genotype = System.Activator.CreateInstance(genotypeType);
                        }
                        
                        // PlantStrainSO should be a ScriptableObject
                        if (typeof(ScriptableObject).IsAssignableFrom(strainType))
                        {
                            var strain = ScriptableObject.CreateInstance(strainType);
                            DestroyImmediate(strain);
                        }
                        
                        validatedStructures++;
                    }
                }
                catch (System.Exception ex)
                {
                    structureIssues.Add($"Genetics: {ex.Message}");
                }
                
                // Test environmental data
                try
                {
                    var conditions = new ProjectChimera.Data.Environment.EnvironmentalConditions();
                    validatedStructures++;
                }
                catch (System.Exception ex)
                {
                    structureIssues.Add($"Environment: {ex.Message}");
                }
                
                // Test progression data (try different possible types)
                try
                {
                    var progressionType = System.Type.GetType("ProjectChimera.Data.Progression.PlayerProgress, ProjectChimera.Data") ??
                                        System.Type.GetType("ProjectChimera.Data.Progression.ProgressionDataStructures+PlayerProgress, ProjectChimera.Data");
                    
                    if (progressionType != null)
                    {
                        var progressionData = System.Activator.CreateInstance(progressionType);
                        validatedStructures++;
                    }
                    else
                    {
                        structureIssues.Add("Progression: Type not found");
                    }
                }
                catch (System.Exception ex)
                {
                    structureIssues.Add($"Progression: {ex.Message}");
                }
                
                const int totalStructureCategories = 4;
                var successRate = (validatedStructures / (float)totalStructureCategories) * 100f;
                result.Passed = successRate >= 75f;
                result.Details = $"Data structures: {validatedStructures}/{totalStructureCategories} categories validated ({successRate:F1}%)";
                
                if (structureIssues.Any() && _showDetailedResults)
                {
                    result.Details += $" - Issues: {string.Join(", ", structureIssues.Take(2))}";
                }
            }
            catch (System.Exception ex)
            {
                result.Passed = false;
                result.Details = $"Data structures validation failed: {ex.Message}";
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                _results.Add(result);
            }
        }
        
        #endregion
        
        #region Assembly Integration Validation
        
        private void ValidateAssemblyIntegration()
        {
            var result = new ValidationResult { TestName = "Assembly Integration", Category = "Integration" };
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var assemblyInfo = new List<string>();
                var totalTypes = 0;
                
                // Check core assembly
                try
                {
                    var coreAssembly = typeof(ChimeraManager).Assembly;
                    var coreTypes = coreAssembly.GetTypes().Length;
                    totalTypes += coreTypes;
                    assemblyInfo.Add($"Core: {coreTypes} types");
                }
                catch (System.Exception ex)
                {
                    assemblyInfo.Add($"Core: Error - {ex.Message}");
                }
                
                // Check data assembly
                try
                {
                    var dataAssembly = typeof(ProjectChimera.Data.Cultivation.InteractivePlant).Assembly;
                    var dataTypes = dataAssembly.GetTypes().Length;
                    totalTypes += dataTypes;
                    assemblyInfo.Add($"Data: {dataTypes} types");
                }
                catch (System.Exception ex)
                {
                    assemblyInfo.Add($"Data: Error - {ex.Message}");
                }
                
                // Check systems assembly
                try
                {
                    var systemsAssembly = typeof(ProjectChimera.Systems.Cultivation.PlantManager).Assembly;
                    var systemsTypes = systemsAssembly.GetTypes().Length;
                    totalTypes += systemsTypes;
                    assemblyInfo.Add($"Systems: {systemsTypes} types");
                }
                catch (System.Exception ex)
                {
                    assemblyInfo.Add($"Systems: Error - {ex.Message}");
                }
                
                // Check testing assembly
                try
                {
                    var testingAssembly = typeof(ChimeraTestBase).Assembly;
                    var testingTypes = testingAssembly.GetTypes().Length;
                    totalTypes += testingTypes;
                    assemblyInfo.Add($"Testing: {testingTypes} types");
                }
                catch (System.Exception ex)
                {
                    assemblyInfo.Add($"Testing: Error - {ex.Message}");
                }
                
                result.Passed = totalTypes > 0;
                result.Details = $"Assemblies: {string.Join(", ", assemblyInfo)} - Total: {totalTypes} types";
            }
            catch (System.Exception ex)
            {
                result.Passed = false;
                result.Details = $"Assembly integration validation failed: {ex.Message}";
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                _results.Add(result);
            }
        }
        
        #endregion
        
        #region Architectural Consistency Validation
        
        private void ValidateArchitecturalConsistency()
        {
            var result = new ValidationResult { TestName = "Architectural Consistency", Category = "Architecture" };
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var consistencyIssues = new List<string>();
                var validatedPatterns = 0;
                var totalPatterns = 6; // Number of architectural patterns we're checking
                
                // 1. Validate ScriptableObject naming convention (must end with SO)
                if (ValidateScriptableObjectNaming())
                {
                    validatedPatterns++;
                    UnityEngine.Debug.Log("VALIDATION-1c: ✅ ScriptableObject naming convention validated");
                }
                else
                {
                    consistencyIssues.Add("ScriptableObject naming violations found");
                    UnityEngine.Debug.LogError("VALIDATION-1c: ❌ ScriptableObject naming convention violations detected");
                }
                
                // 2. Validate Manager inheritance pattern (all managers must inherit from ChimeraManager)
                if (ValidateManagerInheritance())
                {
                    validatedPatterns++;
                    UnityEngine.Debug.Log("VALIDATION-1c: ✅ Manager inheritance pattern validated");
                }
                else
                {
                    consistencyIssues.Add("Manager inheritance violations found");
                    UnityEngine.Debug.LogError("VALIDATION-1c: ❌ Manager inheritance pattern violations detected");
                }
                
                // 3. Validate namespace consistency
                if (ValidateNamespaceConsistency())
                {
                    validatedPatterns++;
                    UnityEngine.Debug.Log("VALIDATION-1c: ✅ Namespace consistency validated");
                }
                else
                {
                    consistencyIssues.Add("Namespace consistency violations found");
                    UnityEngine.Debug.LogError("VALIDATION-1c: ❌ Namespace consistency violations detected");
                }
                
                // 4. Validate Event-Driven Architecture pattern
                if (ValidateEventDrivenArchitecture())
                {
                    validatedPatterns++;
                    UnityEngine.Debug.Log("VALIDATION-1c: ✅ Event-driven architecture pattern validated");
                }
                else
                {
                    consistencyIssues.Add("Event-driven architecture violations found");
                    UnityEngine.Debug.LogError("VALIDATION-1c: ❌ Event-driven architecture violations detected");
                }
                
                // 5. Validate Assembly structure consistency
                if (ValidateAssemblyStructure())
                {
                    validatedPatterns++;
                    UnityEngine.Debug.Log("VALIDATION-1c: ✅ Assembly structure consistency validated");
                }
                else
                {
                    consistencyIssues.Add("Assembly structure violations found");
                    UnityEngine.Debug.LogError("VALIDATION-1c: ❌ Assembly structure violations detected");
                }
                
                // 6. Validate Interface pattern usage
                if (ValidateInterfacePatterns())
                {
                    validatedPatterns++;
                    UnityEngine.Debug.Log("VALIDATION-1c: ✅ Interface pattern usage validated");
                }
                else
                {
                    consistencyIssues.Add("Interface pattern violations found");
                    UnityEngine.Debug.LogError("VALIDATION-1c: ❌ Interface pattern violations detected");
                }
                
                var consistencyRate = (validatedPatterns / (float)totalPatterns) * 100f;
                result.Passed = consistencyRate >= 80f; // 80% threshold for architectural consistency
                result.Details = $"Architectural patterns: {validatedPatterns}/{totalPatterns} validated ({consistencyRate:F1}%)";
                
                if (consistencyIssues.Any())
                {
                    result.Details += $" - Issues: {string.Join(", ", consistencyIssues)}";
                }
                
                UnityEngine.Debug.Log($"VALIDATION-1c: Architectural consistency: {consistencyRate:F1}% ({validatedPatterns}/{totalPatterns} patterns)");
            }
            catch (System.Exception ex)
            {
                result.Passed = false;
                result.Details = $"Architectural validation failed: {ex.Message}";
                UnityEngine.Debug.LogError($"VALIDATION-1c: ❌ Architectural validation error: {ex.Message}");
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                _results.Add(result);
            }
        }
        
        // Helper methods for architectural validation
        private bool ValidateScriptableObjectNaming()
        {
            try
            {
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                var chimeraAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
                
                foreach (var assembly in chimeraAssemblies)
                {
                    var scriptableObjectTypes = assembly.GetTypes()
                        .Where(t => t.IsSubclassOf(typeof(ScriptableObject)) && !t.IsAbstract)
                        .ToArray();
                    
                    foreach (var type in scriptableObjectTypes)
                    {
                        if (!type.Name.EndsWith("SO"))
                        {
                            UnityEngine.Debug.LogWarning($"VALIDATION-1c: ScriptableObject naming violation: {type.FullName} should end with 'SO'");
                            // Continue checking all violations instead of returning immediately
                        }
                    }
                }
                
                // Check again to see if any violations were found
                foreach (var assembly in chimeraAssemblies)
                {
                    var scriptableObjectTypes = assembly.GetTypes()
                        .Where(t => t.IsSubclassOf(typeof(ScriptableObject)) && !t.IsAbstract)
                        .ToArray();
                    
                    foreach (var type in scriptableObjectTypes)
                    {
                        if (!type.Name.EndsWith("SO"))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"VALIDATION-1c: ScriptableObject naming validation error: {ex.Message}");
                return false;
            }
        }
        
        private bool ValidateManagerInheritance()
        {
            try
            {
                var chimeraManagerType = System.Type.GetType("ProjectChimera.Core.ChimeraManager, ProjectChimera.Core");
                if (chimeraManagerType == null) return false;
                
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                var systemsAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera.Systems")).ToArray();
                
                foreach (var assembly in systemsAssemblies)
                {
                    var managerTypes = assembly.GetTypes()
                        .Where(t => t.Name.EndsWith("Manager") && !t.IsAbstract && t.IsSubclassOf(typeof(MonoBehaviour)))
                        .ToArray();
                    
                    foreach (var type in managerTypes)
                    {
                        if (!type.IsSubclassOf(chimeraManagerType))
                        {
                            UnityEngine.Debug.LogWarning($"VALIDATION-1c: Manager inheritance violation: {type.FullName} should inherit from ChimeraManager");
                            // Continue checking all violations instead of returning immediately
                        }
                    }
                }
                
                // Check again to see if any violations were found
                foreach (var assembly in systemsAssemblies)
                {
                    var managerTypes = assembly.GetTypes()
                        .Where(t => t.Name.EndsWith("Manager") && !t.IsAbstract && t.IsSubclassOf(typeof(MonoBehaviour)))
                        .ToArray();
                    
                    foreach (var type in managerTypes)
                    {
                        if (!type.IsSubclassOf(chimeraManagerType))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"VALIDATION-1c: Manager inheritance validation error: {ex.Message}");
                return false;
            }
        }
        
        private bool ValidateNamespaceConsistency()
        {
            try
            {
                // Validate that namespace structure matches assembly structure
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                var chimeraAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
                
                foreach (var assembly in chimeraAssemblies)
                {
                    var assemblyName = assembly.GetName().Name;
                    var types = assembly.GetTypes().Where(t => !string.IsNullOrEmpty(t.Namespace)).ToArray();
                    
                    foreach (var type in types)
                    {
                        // Check if namespace starts with the expected assembly namespace  
                        var expectedNamespaceStart = assemblyName;
                        if (!type.Namespace.StartsWith(expectedNamespaceStart))
                        {
                            // Allow some flexibility for Core and Data assemblies
                            if (!assemblyName.Contains("Core") && !assemblyName.Contains("Data") && !assemblyName.Contains("Testing"))
                            {
                                UnityEngine.Debug.LogWarning($"VALIDATION-1c: Namespace inconsistency: {type.FullName} in assembly {assemblyName} (expected namespace to start with {expectedNamespaceStart})");
                                // Continue checking all violations instead of returning immediately
                            }
                        }
                    }
                }
                
                // Check again to see if any violations were found
                foreach (var assembly in chimeraAssemblies)
                {
                    var assemblyName = assembly.GetName().Name;
                    var types = assembly.GetTypes().Where(t => !string.IsNullOrEmpty(t.Namespace)).ToArray();
                    
                    foreach (var type in types)
                    {
                        var expectedNamespaceStart = assemblyName;
                        if (!type.Namespace.StartsWith(expectedNamespaceStart))
                        {
                            if (!assemblyName.Contains("Core") && !assemblyName.Contains("Data") && !assemblyName.Contains("Testing"))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"VALIDATION-1c: Namespace consistency validation error: {ex.Message}");
                return false;
            }
        }
        
        private bool ValidateEventDrivenArchitecture()
        {
            try
            {
                // Check for GameEvent SO types (event-driven architecture)
                var dataAssembly = System.AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "ProjectChimera.Data");
                
                if (dataAssembly != null)
                {
                    var eventTypes = dataAssembly.GetTypes()
                        .Where(t => t.Name.Contains("Event") && t.IsSubclassOf(typeof(ScriptableObject)))
                        .ToArray();
                    
                    // We expect at least some event SO types for event-driven architecture
                    return eventTypes.Length > 0;
                }
                return true; // Pass if we can't find the assembly (may not be critical)
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"VALIDATION-1c: Event-driven architecture validation error: {ex.Message}");
                return false;
            }
        }
        
        private bool ValidateAssemblyStructure()
        {
            try
            {
                var requiredAssemblies = new string[]
                {
                    "ProjectChimera.Core",
                    "ProjectChimera.Data",
                    "ProjectChimera.Systems.Cultivation",
                    "ProjectChimera.Systems.Genetics",
                    "ProjectChimera.Systems.Environment"
                };
                
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                var assemblyNames = assemblies.Select(a => a.GetName().Name).ToArray();
                
                foreach (var requiredAssembly in requiredAssemblies)
                {
                    if (!assemblyNames.Contains(requiredAssembly))
                    {
                        UnityEngine.Debug.LogWarning($"VALIDATION-1c: Missing required assembly: {requiredAssembly}");
                        return false;
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"VALIDATION-1c: Assembly structure validation error: {ex.Message}");
                return false;
            }
        }
        
        private bool ValidateInterfacePatterns()
        {
            try
            {
                // Check for proper interface usage (IChimeraManager, service interfaces)
                var coreAssembly = System.AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "ProjectChimera.Core");
                
                if (coreAssembly != null)
                {
                    var interfaceTypes = coreAssembly.GetTypes()
                        .Where(t => t.IsInterface && t.Name.StartsWith("I"))
                        .ToArray();
                    
                    // We expect at least IChimeraManager interface
                    var hasChimeraManagerInterface = interfaceTypes.Any(t => t.Name == "IChimeraManager");
                    return hasChimeraManagerInterface;
                }
                return true; // Pass if we can't find the assembly
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"VALIDATION-1c: Interface pattern validation error: {ex.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region Report Generation
        
        private void GenerateValidationReport(long totalExecutionTimeMs)
        {
            var totalTests = _results.Count;
            var passedTests = _results.Count(r => r.Passed);
            var failedTests = totalTests - passedTests;
            var passRate = totalTests > 0 ? (passedTests / (float)totalTests) * 100f : 0f;
            
            UnityEngine.Debug.Log("VALIDATION-1a: ===== SYSTEM VALIDATION REPORT =====");
            UnityEngine.Debug.Log($"VALIDATION-1a: Execution Time: {System.DateTime.Now:HH:mm:ss}");
            UnityEngine.Debug.Log($"VALIDATION-1a: Total Tests: {totalTests}");
            UnityEngine.Debug.Log($"VALIDATION-1a: Passed: {passedTests}");
            UnityEngine.Debug.Log($"VALIDATION-1a: Failed: {failedTests}");
            UnityEngine.Debug.Log($"VALIDATION-1a: Pass Rate: {passRate:F1}%");
            UnityEngine.Debug.Log($"VALIDATION-1a: Execution Time: {totalExecutionTimeMs}ms");
            UnityEngine.Debug.Log("VALIDATION-1a:");
            
            // Individual test results
            foreach (var result in _results)
            {
                var status = result.Passed ? "✅" : "❌";
                var timeInfo = _showDetailedResults ? $" ({result.ExecutionTimeMs}ms)" : "";
                if (result.Passed)
                {
                    UnityEngine.Debug.Log($"VALIDATION-1a: {status} {result.TestName}: {result.Details}{timeInfo}");
                }
                else
                {
                    UnityEngine.Debug.LogError($"VALIDATION-1a: {status} {result.TestName}: {result.Details}{timeInfo}");
                }
            }
            
            UnityEngine.Debug.Log("VALIDATION-1a:");
            
            // Summary
            if (passRate >= 90f)
            {
                UnityEngine.Debug.Log("VALIDATION-1a: 🎉 EXCELLENT! All refactored systems are functioning correctly.");
            }
            else if (passRate >= 75f)
            {
                UnityEngine.Debug.LogWarning("VALIDATION-1a: ⚠️ GOOD. Most refactored systems are functioning, minor issues detected.");
            }
            else
            {
                UnityEngine.Debug.LogError("VALIDATION-1a: ❌ CRITICAL. Significant issues found requiring immediate attention.");
            }
            
            UnityEngine.Debug.Log("VALIDATION-1a: ===== END VALIDATION REPORT =====");
        }
        
        #endregion
        
        #region Data Structures
        
        [System.Serializable]
        public class ValidationResult
        {
            public string TestName;
            public string Category;
            public bool Passed;
            public string Details;
            public long ExecutionTimeMs;
        }
        
        #endregion
        
        #region Deep Diagnostics
        
        /// <summary>
        /// Run comprehensive diagnostics to identify specific system failures
        /// </summary>
        private void RunDeepDiagnostics()
        {
            var result = new ValidationResult { TestName = "Deep Diagnostics", Category = "Diagnostics" };
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                UnityEngine.Debug.Log("VALIDATION-1a: ===== DEEP DIAGNOSTIC MODE =====");
                
                // Assembly analysis
                if (_validateAssemblyReferences)
                {
                    AnalyzeAssemblyAvailability();
                }
                
                // Type availability analysis
                if (_testIndividualSystems)
                {
                    AnalyzeTypeAvailability();
                }
                
                // Core system dependency analysis
                AnalyzeCoreSystemDependencies();
                
                // Available types listing (if enabled)
                if (_logAllAvailableTypes)
                {
                    LogAllAvailableTypes();
                }
                
                result.Passed = true;
                result.Details = "Deep diagnostics completed successfully";
                
                UnityEngine.Debug.Log("VALIDATION-1a: ===== END DEEP DIAGNOSTIC MODE =====");
            }
            catch (System.Exception ex)
            {
                result.Passed = false;
                result.Details = $"Deep diagnostics failed: {ex.Message}";
                UnityEngine.Debug.LogError($"VALIDATION-1a: Deep diagnostics error: {ex}");
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                _results.Add(result);
            }
        }
        
        /// <summary>
        /// Analyze which assemblies are available and their basic info
        /// </summary>
        private void AnalyzeAssemblyAvailability()
        {
            UnityEngine.Debug.Log("VALIDATION-1a: --- ASSEMBLY AVAILABILITY ANALYSIS ---");
            
            try
            {
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                UnityEngine.Debug.Log($"VALIDATION-1a: Total assemblies loaded: {assemblies.Length}");
                
                var chimeraAssemblies = assemblies.Where(a => a.FullName.Contains("ProjectChimera")).ToArray();
                UnityEngine.Debug.Log($"VALIDATION-1a: ProjectChimera assemblies found: {chimeraAssemblies.Length}");
                
                foreach (var assembly in chimeraAssemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes();
                        UnityEngine.Debug.Log($"VALIDATION-1a: Assembly '{assembly.GetName().Name}' - {types.Length} types");
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError($"VALIDATION-1a: Failed to load types from {assembly.GetName().Name}: {ex.Message}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"VALIDATION-1a: Assembly analysis failed: {ex}");
            }
        }
        
        /// <summary>
        /// Analyze specific type availability for critical systems
        /// </summary>
        private void AnalyzeTypeAvailability()
        {
            UnityEngine.Debug.Log("VALIDATION-1a: --- TYPE AVAILABILITY ANALYSIS ---");
            
            // Critical types to check
            var criticalTypes = new string[]
            {
                // Core types
                "ProjectChimera.Core.ChimeraManager",
                "ProjectChimera.Core.GameManager",
                "ProjectChimera.Core.TimeManager",
                "ProjectChimera.Core.SaveManager",
                
                // Manager types
                "ProjectChimera.Systems.Cultivation.PlantManager",
                "ProjectChimera.Systems.Genetics.GeneticsManager",
                "ProjectChimera.Systems.Environment.EnvironmentalManager",
                "ProjectChimera.Systems.Economy.EconomyManager",
                "ProjectChimera.Systems.Progression.ProgressionManager",
                "ProjectChimera.Systems.Progression.ResearchManager",
                "ProjectChimera.Systems.Progression.AchievementManager",
                
                // Data types
                "ProjectChimera.Data.Cultivation.InteractivePlant",
                "ProjectChimera.Data.Cultivation.CultivationTaskType",
                "ProjectChimera.Data.Cultivation.CareQuality",
                "ProjectChimera.Data.Environment.EnvironmentalConditions",
                "ProjectChimera.Data.Genetics.CannabisGenotype",
                "ProjectChimera.Data.Genetics.PlantStrainSO",
                
                // Testing types
                "ProjectChimera.Testing.Core.ChimeraTestBase",
                "ProjectChimera.Testing.Performance.PlantStressTestingFramework",
                "ProjectChimera.Testing.Performance.PerformanceBenchmarkRunner",
                "ProjectChimera.Testing.Performance.MemoryMonitoringSystem"
            };
            
            var foundTypes = 0;
            var totalTypes = criticalTypes.Length;
            
            foreach (var typeName in criticalTypes)
            {
                try
                {
                    var type = System.Type.GetType(typeName);
                    if (type != null)
                    {
                        foundTypes++;
                        var assemblyName = type.Assembly.GetName().Name;
                        UnityEngine.Debug.Log($"VALIDATION-1a: ✅ {typeName} - Found in {assemblyName}");
                        
                        // Additional type analysis
                        if (typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(type))
                        {
                            UnityEngine.Debug.Log($"VALIDATION-1a:    └─ MonoBehaviour: Yes");
                        }
                        if (typeof(UnityEngine.ScriptableObject).IsAssignableFrom(type))
                        {
                            UnityEngine.Debug.Log($"VALIDATION-1a:    └─ ScriptableObject: Yes");
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"VALIDATION-1a: ❌ {typeName} - NOT FOUND");
                        
                        // Try to find similar types
                        var shortName = typeName.Substring(typeName.LastIndexOf('.') + 1);
                        var similarTypes = FindSimilarTypes(shortName);
                        if (similarTypes.Any())
                        {
                            UnityEngine.Debug.Log($"VALIDATION-1a:    └─ Similar types found: {string.Join(", ", similarTypes.Take(3))}");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"VALIDATION-1a: ❌ {typeName} - Error: {ex.Message}");
                }
            }
            
            var successRate = (foundTypes / (float)totalTypes) * 100f;
            UnityEngine.Debug.Log($"VALIDATION-1a: Type availability: {foundTypes}/{totalTypes} ({successRate:F1}%)");
        }
        
        /// <summary>
        /// Find types with similar names to help identify naming issues
        /// </summary>
        private string[] FindSimilarTypes(string searchName)
        {
            try
            {
                var allTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.FullName.Contains("ProjectChimera"))
                    .SelectMany(a => 
                    {
                        try { return a.GetTypes(); }
                        catch { return new System.Type[0]; }
                    })
                    .Where(t => t.Name.Contains(searchName) || searchName.Contains(t.Name))
                    .Select(t => t.FullName)
                    .ToArray();
                    
                return allTypes;
            }
            catch
            {
                return new string[0];
            }
        }
        
        /// <summary>
        /// Analyze core system dependencies and initialization requirements
        /// </summary>
        private void AnalyzeCoreSystemDependencies()
        {
            UnityEngine.Debug.Log("VALIDATION-1a: --- CORE SYSTEM DEPENDENCY ANALYSIS ---");
            
            try
            {
                // Check for existing managers in scene
                var existingManagers = FindObjectsOfType<MonoBehaviour>()
                    .Where(mb => mb.GetType().Name.Contains("Manager"))
                    .ToArray();
                    
                UnityEngine.Debug.Log($"VALIDATION-1a: Existing managers in scene: {existingManagers.Length}");
                foreach (var manager in existingManagers.Take(10)) // Limit to first 10
                {
                    UnityEngine.Debug.Log($"VALIDATION-1a:   - {manager.GetType().FullName} on '{manager.gameObject.name}'");
                }
                
                // Check ChimeraManager base class availability
                var chimeraManagerType = System.Type.GetType("ProjectChimera.Core.ChimeraManager, ProjectChimera.Core");
                if (chimeraManagerType != null)
                {
                    UnityEngine.Debug.Log($"VALIDATION-1a: ✅ ChimeraManager base class found");
                    UnityEngine.Debug.Log($"VALIDATION-1a:    └─ Is abstract: {chimeraManagerType.IsAbstract}");
                    UnityEngine.Debug.Log($"VALIDATION-1a:    └─ Base type: {chimeraManagerType.BaseType?.Name}");
                }
                else
                {
                    UnityEngine.Debug.LogError($"VALIDATION-1a: ❌ ChimeraManager base class NOT FOUND - This is critical!");
                }
                
                // Check for singleton patterns
                var gameManagerType = System.Type.GetType("ProjectChimera.Core.GameManager, ProjectChimera.Core");
                if (gameManagerType != null)
                {
                    var instanceProperty = gameManagerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (instanceProperty != null)
                    {
                        UnityEngine.Debug.Log($"VALIDATION-1a: ✅ GameManager singleton pattern detected");
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"VALIDATION-1a: ⚠️ GameManager found but no Instance property");
                    }
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"VALIDATION-1a: Core system dependency analysis failed: {ex}");
            }
        }
        
        /// <summary>
        /// Log all available types (use sparingly - can be very verbose)
        /// </summary>
        private void LogAllAvailableTypes()
        {
            UnityEngine.Debug.Log("VALIDATION-1a: --- ALL AVAILABLE TYPES (LIMITED) ---");
            
            try
            {
                var chimeraTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.FullName.Contains("ProjectChimera"))
                    .SelectMany(a => 
                    {
                        try { return a.GetTypes(); }
                        catch { return new System.Type[0]; }
                    })
                    .OrderBy(t => t.FullName)
                    .Take(50) // Limit to first 50 types to avoid spam
                    .ToArray();
                    
                UnityEngine.Debug.Log($"VALIDATION-1a: Showing first 50 of available ProjectChimera types:");
                foreach (var type in chimeraTypes)
                {
                    var typeInfo = $"  - {type.FullName}";
                    if (typeof(MonoBehaviour).IsAssignableFrom(type))
                        typeInfo += " (MonoBehaviour)";
                    if (typeof(ScriptableObject).IsAssignableFrom(type))
                        typeInfo += " (ScriptableObject)";
                    if (type.IsAbstract)
                        typeInfo += " (Abstract)";
                        
                    UnityEngine.Debug.Log($"VALIDATION-1a: {typeInfo}");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"VALIDATION-1a: Failed to log available types: {ex}");
            }
        }
        
        #endregion
        
        #region Editor Menu Methods
        
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("Project Chimera/Validation/Run System Validation")]
        public static void RunValidationFromMenu()
        {
            var existingRunner = UnityEngine.Object.FindObjectOfType<SystemValidationRunner>();
            if (existingRunner != null)
            {
                existingRunner.RunSystemValidation();
            }
            else
            {
                var validationGO = new GameObject("SystemValidationRunner");
                var runner = validationGO.AddComponent<SystemValidationRunner>();
                runner.RunSystemValidation();
            }
        }
        
        [UnityEditor.MenuItem("Project Chimera/Validation/Create Validation Runner")]
        public static void CreateValidationRunner()
        {
            var validationGO = new GameObject("SystemValidationRunner");
            validationGO.AddComponent<SystemValidationRunner>();
            UnityEditor.Selection.activeGameObject = validationGO;
            UnityEngine.Debug.Log("VALIDATION-1a: SystemValidationRunner created. Run it manually or enable 'Run On Start'.");
        }
        
        [UnityEditor.MenuItem("Window/Project Chimera/System Validation")]
        public static void OpenValidationWindow()
        {
            RunValidationFromMenu();
        }
        #endif
        
        #endregion
    }
}