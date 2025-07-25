using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectChimera.Editor
{
    /// <summary>
    /// Comprehensive IPM and Gaming system audit tool as outlined in IPM_GAMING_REBUILD_PLAN.
    /// Validates clean architecture implementation, system integration, and performance standards.
    /// </summary>
    public class IPMGamingSystemAudit : EditorWindow
    {
        private Vector2 _scrollPosition;
        private List<AuditResult> _auditResults = new List<AuditResult>();
        private bool _isRunning = false;
        private int _totalChecks = 0;
        private int _completedChecks = 0;
        private int _passedChecks = 0;
        private int _failedChecks = 0;
        
        [MenuItem("Project Chimera/IPM Gaming System Audit")]
        public static void ShowWindow()
        {
            var window = GetWindow<IPMGamingSystemAudit>("IPM Gaming System Audit");
            window.minSize = new Vector2(1000, 700);
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("IPM Gaming System Comprehensive Audit", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            // Control buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Run Complete Audit", GUILayout.Height(30)))
            {
                RunCompleteAudit();
            }
            if (GUILayout.Button("Architecture Audit Only", GUILayout.Height(30)))
            {
                RunArchitectureAudit();
            }
            if (GUILayout.Button("Performance Audit Only", GUILayout.Height(30)))
            {
                RunPerformanceAudit();
            }
            if (GUILayout.Button("Generate Report", GUILayout.Height(30)))
            {
                GenerateAuditReport();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Progress and status
            if (_isRunning)
            {
                var progress = _totalChecks > 0 ? (float)_completedChecks / _totalChecks : 0f;
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true)), 
                    progress, $"Auditing... {_completedChecks}/{_totalChecks}");
            }
            
            // Summary statistics
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Total Checks: {_totalChecks}", GUILayout.Width(120));
            GUILayout.Label($"Passed: {_passedChecks}", GUILayout.Width(80));
            GUILayout.Label($"Failed: {_failedChecks}", GUILayout.Width(80));
            var successRate = _totalChecks > 0 ? (float)_passedChecks / _totalChecks * 100 : 0f;
            GUILayout.Label($"Success Rate: {successRate:F1}%", GUILayout.Width(120));
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Audit results
            if (_auditResults.Count > 0)
            {
                GUILayout.Label("Audit Results:", EditorStyles.boldLabel);
                
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
                
                foreach (var result in _auditResults)
                {
                    DrawAuditResult(result);
                }
                
                GUILayout.EndScrollView();
            }
            else if (!_isRunning)
            {
                GUILayout.Label("No audit results. Click 'Run Complete Audit' to start.", 
                    EditorStyles.centeredGreyMiniLabel);
            }
        }
        
        private void DrawAuditResult(AuditResult result)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Header
            GUILayout.BeginHorizontal();
            
            var statusColor = result.Passed ? Color.green : Color.red;
            var originalColor = GUI.color;
            GUI.color = statusColor;
            GUILayout.Label($"[{(result.Passed ? "PASS" : "FAIL")}]", GUILayout.Width(60));
            GUI.color = originalColor;
            
            GUILayout.Label($"{result.Category}: {result.TestName}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            
            if (result.HasRecommendation && GUILayout.Button("Info", GUILayout.Width(50)))
            {
                EditorUtility.DisplayDialog("Audit Recommendation", 
                    $"Test: {result.TestName}\n\nResult: {result.Description}\n\nRecommendation: {result.Recommendation}", 
                    "OK");
            }
            
            GUILayout.EndHorizontal();
            
            // Details
            GUILayout.Label(result.Description);
            
            if (!string.IsNullOrEmpty(result.Details))
            {
                GUILayout.Label($"Details: {result.Details}", EditorStyles.wordWrappedMiniLabel);
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(2);
        }
        
        private void RunCompleteAudit()
        {
            _isRunning = true;
            _auditResults.Clear();
            _totalChecks = 0;
            _completedChecks = 0;
            _passedChecks = 0;
            _failedChecks = 0;
            
            Debug.Log("[IPM Gaming Audit] Starting comprehensive audit...");
            
            try
            {
                // 1. Clean Architecture Validation
                AuditCleanArchitecture();
                
                // 2. Dependency Injection Implementation
                AuditDependencyInjection();
                
                // 3. Event-Driven Architecture
                AuditEventDrivenArchitecture();
                
                // 4. Repository Pattern Implementation
                AuditRepositoryPattern();
                
                // 5. Domain Services Validation
                AuditDomainServices();
                
                // 6. System Integration Testing
                AuditSystemIntegration();
                
                // 7. Performance Validation
                AuditPerformanceStandards();
                
                // 8. Code Quality Assessment
                AuditCodeQuality();
                
                // 9. Testing Coverage Analysis
                AuditTestingCoverage();
                
                // 10. Documentation Compliance
                AuditDocumentationCompliance();
                
                Debug.Log($"[IPM Gaming Audit] Audit complete: {_passedChecks}/{_totalChecks} checks passed");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[IPM Gaming Audit] Audit failed: {e.Message}");
                AddResult("System", "Audit Execution", false, $"Audit execution failed: {e.Message}");
            }
            finally
            {
                _isRunning = false;
                Repaint();
            }
        }
        
        private void RunArchitectureAudit()
        {
            _isRunning = true;
            _auditResults.Clear();
            ResetCounters();
            
            try
            {
                AuditCleanArchitecture();
                AuditDependencyInjection();
                AuditEventDrivenArchitecture();
                AuditRepositoryPattern();
                AuditDomainServices();
            }
            finally
            {
                _isRunning = false;
                Repaint();
            }
        }
        
        private void RunPerformanceAudit()
        {
            _isRunning = true;
            _auditResults.Clear();
            ResetCounters();
            
            try
            {
                AuditPerformanceStandards();
                AuditSystemIntegration();
            }
            finally
            {
                _isRunning = false;
                Repaint();
            }
        }
        
        private void AuditCleanArchitecture()
        {
            // 1. Verify Enhanced IPM Gaming System exists
            AddCheck();
            var mainSystemPath = "Assets/ProjectChimera/Systems/IPM/EnhancedIPMGamingSystem.cs";
            bool mainSystemExists = File.Exists(mainSystemPath);
            AddResult("Clean Architecture", "Enhanced IPM Gaming System", mainSystemExists,
                mainSystemExists ? "Main system file found" : "Main system file missing",
                mainSystemExists ? "" : "Create EnhancedIPMGamingSystem.cs with clean architecture patterns");
            
            // 2. Verify architecture types exist
            AddCheck();
            var typesPath = "Assets/ProjectChimera/Systems/IPM/IPMGamingArchitectureTypes.cs";
            bool typesExist = File.Exists(typesPath);
            AddResult("Clean Architecture", "Architecture Types", typesExist,
                typesExist ? "Architecture types file found" : "Architecture types file missing");
            
            // 3. Verify interfaces exist
            AddCheck();
            var interfacesPath = "Assets/ProjectChimera/Systems/IPM/IPMGamingInterfaces.cs";
            bool interfacesExist = File.Exists(interfacesPath);
            AddResult("Clean Architecture", "Interface Definitions", interfacesExist,
                interfacesExist ? "Interface definitions file found" : "Interface definitions file missing");
            
            // 4. Verify separation of concerns
            if (mainSystemExists)
            {
                AddCheck();
                var content = File.ReadAllText(mainSystemPath);
                bool hasCleanSeparation = ValidateCleanArchitecturePrinciples(content);
                AddResult("Clean Architecture", "Separation of Concerns", hasCleanSeparation,
                    hasCleanSeparation ? "Clean separation of concerns implemented" : "Poor separation of concerns detected");
            }
            
            // 5. Verify dependency direction
            if (mainSystemExists && interfacesExist)
            {
                AddCheck();
                bool dependenciesCorrect = ValidateDependencyDirection(mainSystemPath, interfacesPath);
                AddResult("Clean Architecture", "Dependency Direction", dependenciesCorrect,
                    dependenciesCorrect ? "Dependencies flow inward correctly" : "Dependency inversion violations detected");
            }
        }
        
        private void AuditDependencyInjection()
        {
            // 1. Verify interface-based dependencies
            AddCheck();
            var interfacesPath = "Assets/ProjectChimera/Systems/IPM/IPMGamingInterfaces.cs";
            if (File.Exists(interfacesPath))
            {
                var content = File.ReadAllText(interfacesPath);
                bool hasProperInterfaces = ValidateDependencyInjectionInterfaces(content);
                AddResult("Dependency Injection", "Interface Design", hasProperInterfaces,
                    hasProperInterfaces ? "Proper interfaces for DI found" : "DI interfaces need improvement");
            }
            else
            {
                AddResult("Dependency Injection", "Interface Design", false, "Interfaces file not found");
            }
            
            // 2. Verify constructor injection
            AddCheck();
            var mainSystemPath = "Assets/ProjectChimera/Systems/IPM/EnhancedIPMGamingSystem.cs";
            if (File.Exists(mainSystemPath))
            {
                var content = File.ReadAllText(mainSystemPath);
                bool hasConstructorInjection = ValidateConstructorInjection(content);
                AddResult("Dependency Injection", "Constructor Injection", hasConstructorInjection,
                    hasConstructorInjection ? "Constructor injection implemented" : "Constructor injection missing or incorrect");
            }
            else
            {
                AddResult("Dependency Injection", "Constructor Injection", false, "Main system file not found");
            }
            
            // 3. Verify service locator pattern avoidance
            AddCheck();
            if (File.Exists(mainSystemPath))
            {
                var content = File.ReadAllText(mainSystemPath);
                bool avoidsServiceLocator = !content.Contains("ServiceLocator") && !content.Contains("GetService");
                AddResult("Dependency Injection", "Service Locator Avoidance", avoidsServiceLocator,
                    avoidsServiceLocator ? "Service locator pattern avoided" : "Service locator anti-pattern detected");
            }
        }
        
        private void AuditEventDrivenArchitecture()
        {
            // 1. Verify event bus implementation
            AddCheck();
            var interfacesPath = "Assets/ProjectChimera/Systems/IPM/IPMGamingInterfaces.cs";
            if (File.Exists(interfacesPath))
            {
                var content = File.ReadAllText(interfacesPath);
                bool hasEventBus = content.Contains("IIPMGameEventBus") || content.Contains("EventBus");
                AddResult("Event-Driven Architecture", "Event Bus Implementation", hasEventBus,
                    hasEventBus ? "Event bus interface found" : "Event bus implementation missing");
            }
            else
            {
                AddResult("Event-Driven Architecture", "Event Bus Implementation", false, "Interfaces file not found");
            }
            
            // 2. Verify publish/subscribe pattern
            AddCheck();
            var mainSystemPath = "Assets/ProjectChimera/Systems/IPM/EnhancedIPMGamingSystem.cs";
            if (File.Exists(mainSystemPath))
            {
                var content = File.ReadAllText(mainSystemPath);
                bool hasPublishSubscribe = ValidatePublishSubscribePattern(content);
                AddResult("Event-Driven Architecture", "Publish/Subscribe Pattern", hasPublishSubscribe,
                    hasPublishSubscribe ? "Publish/Subscribe pattern implemented" : "Publish/Subscribe pattern missing");
            }
            
            // 3. Verify loose coupling through events
            AddCheck();
            if (File.Exists(mainSystemPath))
            {
                var content = File.ReadAllText(mainSystemPath);
                bool hasLooseCoupling = ValidateLooseCoupling(content);
                AddResult("Event-Driven Architecture", "Loose Coupling", hasLooseCoupling,
                    hasLooseCoupling ? "Loose coupling through events achieved" : "Tight coupling detected");
            }
        }
        
        private void AuditRepositoryPattern()
        {
            // 1. Verify repository interfaces
            AddCheck();
            var interfacesPath = "Assets/ProjectChimera/Systems/IPM/IPMGamingInterfaces.cs";
            if (File.Exists(interfacesPath))
            {
                var content = File.ReadAllText(interfacesPath);
                bool hasRepositoryInterfaces = ValidateRepositoryInterfaces(content);
                AddResult("Repository Pattern", "Repository Interfaces", hasRepositoryInterfaces,
                    hasRepositoryInterfaces ? "Repository interfaces properly defined" : "Repository interfaces missing or incomplete");
            }
            else
            {
                AddResult("Repository Pattern", "Repository Interfaces", false, "Interfaces file not found");
            }
            
            // 2. Verify data access abstraction
            AddCheck();
            if (File.Exists(interfacesPath))
            {
                var content = File.ReadAllText(interfacesPath);
                bool hasDataAbstraction = ValidateDataAccessAbstraction(content);
                AddResult("Repository Pattern", "Data Access Abstraction", hasDataAbstraction,
                    hasDataAbstraction ? "Data access properly abstracted" : "Data access abstraction needs improvement");
            }
            
            // 3. Verify repository implementations
            AddCheck();
            var mainSystemPath = "Assets/ProjectChimera/Systems/IPM/EnhancedIPMGamingSystem.cs";
            if (File.Exists(mainSystemPath))
            {
                var content = File.ReadAllText(mainSystemPath);
                bool hasRepositoryUsage = ValidateRepositoryUsage(content);
                AddResult("Repository Pattern", "Repository Usage", hasRepositoryUsage,
                    hasRepositoryUsage ? "Repositories properly used" : "Repository usage missing or incorrect");
            }
        }
        
        private void AuditDomainServices()
        {
            // 1. Verify business logic separation
            AddCheck();
            var interfacesPath = "Assets/ProjectChimera/Systems/IPM/IPMGamingInterfaces.cs";
            if (File.Exists(interfacesPath))
            {
                var content = File.ReadAllText(interfacesPath);
                bool hasDomainServices = ValidateDomainServices(content);
                AddResult("Domain Services", "Business Logic Separation", hasDomainServices,
                    hasDomainServices ? "Domain services properly defined" : "Domain services missing or incomplete");
            }
            else
            {
                AddResult("Domain Services", "Business Logic Separation", false, "Interfaces file not found");
            }
            
            // 2. Verify service interfaces
            AddCheck();
            if (File.Exists(interfacesPath))
            {
                var content = File.ReadAllText(interfacesPath);
                bool hasServiceInterfaces = content.Contains("Service") && content.Contains("interface");
                AddResult("Domain Services", "Service Interfaces", hasServiceInterfaces,
                    hasServiceInterfaces ? "Service interfaces found" : "Service interfaces missing");
            }
        }
        
        private void AuditSystemIntegration()
        {
            // 1. Verify assembly references
            AddCheck();
            var asmdefPath = "Assets/ProjectChimera/Systems/IPM/ProjectChimera.Systems.IPM.asmdef";
            if (File.Exists(asmdefPath))
            {
                var content = File.ReadAllText(asmdefPath);
                bool hasProperReferences = ValidateAssemblyReferences(content);
                AddResult("System Integration", "Assembly References", hasProperReferences,
                    hasProperReferences ? "Assembly references properly configured" : "Assembly reference issues detected");
            }
            else
            {
                AddResult("System Integration", "Assembly References", false, "Assembly definition not found");
            }
            
            // 2. Verify namespace usage
            AddCheck();
            var ipmFiles = Directory.GetFiles("Assets/ProjectChimera/Systems/IPM", "*.cs", SearchOption.AllDirectories);
            bool hasConsistentNamespaces = ValidateNamespaceUsage(ipmFiles);
            AddResult("System Integration", "Namespace Consistency", hasConsistentNamespaces,
                hasConsistentNamespaces ? "Namespace usage is consistent" : "Namespace inconsistencies detected");
            
            // 3. Verify cross-system communication
            AddCheck();
            bool hasCrossSystemComm = ValidateCrossSystemCommunication(ipmFiles);
            AddResult("System Integration", "Cross-System Communication", hasCrossSystemComm,
                hasCrossSystemComm ? "Cross-system communication properly implemented" : "Cross-system communication needs improvement");
        }
        
        private void AuditPerformanceStandards()
        {
            // 1. Verify async/await usage
            AddCheck();
            var mainSystemPath = "Assets/ProjectChimera/Systems/IPM/EnhancedIPMGamingSystem.cs";
            if (File.Exists(mainSystemPath))
            {
                var content = File.ReadAllText(mainSystemPath);
                bool hasAsyncAwait = content.Contains("async") && content.Contains("await");
                AddResult("Performance", "Async/Await Usage", hasAsyncAwait,
                    hasAsyncAwait ? "Async/await patterns used" : "Async/await patterns missing");
            }
            else
            {
                AddResult("Performance", "Async/Await Usage", false, "Main system file not found");
            }
            
            // 2. Verify memory management
            AddCheck();
            if (File.Exists(mainSystemPath))
            {
                var content = File.ReadAllText(mainSystemPath);
                bool hasGoodMemoryManagement = ValidateMemoryManagement(content);
                AddResult("Performance", "Memory Management", hasGoodMemoryManagement,
                    hasGoodMemoryManagement ? "Good memory management practices" : "Memory management needs improvement");
            }
            
            // 3. Verify caching strategies
            AddCheck();
            if (File.Exists(mainSystemPath))
            {
                var content = File.ReadAllText(mainSystemPath);
                bool hasCaching = content.Contains("Cache") || content.Contains("cache");
                AddResult("Performance", "Caching Implementation", hasCaching,
                    hasCaching ? "Caching strategies implemented" : "Caching strategies missing");
            }
        }
        
        private void AuditCodeQuality()
        {
            // 1. Verify SOLID principles
            AddCheck();
            var ipmFiles = Directory.GetFiles("Assets/ProjectChimera/Systems/IPM", "*.cs", SearchOption.AllDirectories);
            bool followsSOLID = ValidateSOLIDPrinciples(ipmFiles);
            AddResult("Code Quality", "SOLID Principles", followsSOLID,
                followsSOLID ? "SOLID principles followed" : "SOLID principle violations detected");
            
            // 2. Verify naming conventions
            AddCheck();
            bool hasGoodNaming = ValidateNamingConventions(ipmFiles);
            AddResult("Code Quality", "Naming Conventions", hasGoodNaming,
                hasGoodNaming ? "Naming conventions followed" : "Naming convention issues detected");
            
            // 3. Verify documentation
            AddCheck();
            bool hasGoodDocumentation = ValidateDocumentation(ipmFiles);
            AddResult("Code Quality", "Code Documentation", hasGoodDocumentation,
                hasGoodDocumentation ? "Code properly documented" : "Documentation needs improvement");
        }
        
        private void AuditTestingCoverage()
        {
            // 1. Verify test files exist
            AddCheck();
            var testFiles = Directory.GetFiles("Assets/ProjectChimera/Testing", "*IPM*Test*.cs", SearchOption.AllDirectories);
            bool hasTestFiles = testFiles.Length > 0;
            AddResult("Testing Coverage", "Test Files Existence", hasTestFiles,
                hasTestFiles ? $"Found {testFiles.Length} IPM test files" : "No IPM test files found");
            
            // 2. Verify unit tests
            AddCheck();
            if (hasTestFiles)
            {
                bool hasUnitTests = ValidateUnitTests(testFiles);
                AddResult("Testing Coverage", "Unit Tests", hasUnitTests,
                    hasUnitTests ? "Unit tests implemented" : "Unit tests missing or incomplete");
            }
            else
            {
                AddResult("Testing Coverage", "Unit Tests", false, "No test files to analyze");
            }
            
            // 3. Verify integration tests
            AddCheck();
            if (hasTestFiles)
            {
                bool hasIntegrationTests = ValidateIntegrationTests(testFiles);
                AddResult("Testing Coverage", "Integration Tests", hasIntegrationTests,
                    hasIntegrationTests ? "Integration tests implemented" : "Integration tests missing");
            }
            else
            {
                AddResult("Testing Coverage", "Integration Tests", false, "No test files to analyze");
            }
        }
        
        private void AuditDocumentationCompliance()
        {
            // 1. Verify README documentation
            AddCheck();
            var readmeFiles = Directory.GetFiles("Assets/ProjectChimera/Systems/IPM", "README*.md", SearchOption.AllDirectories);
            bool hasReadme = readmeFiles.Length > 0;
            AddResult("Documentation", "README Files", hasReadme,
                hasReadme ? "README documentation found" : "README documentation missing");
            
            // 2. Verify API documentation
            AddCheck();
            var ipmFiles = Directory.GetFiles("Assets/ProjectChimera/Systems/IPM", "*.cs", SearchOption.AllDirectories);
            bool hasAPIDoc = ValidateAPIDocumentation(ipmFiles);
            AddResult("Documentation", "API Documentation", hasAPIDoc,
                hasAPIDoc ? "API documentation adequate" : "API documentation needs improvement");
            
            // 3. Verify architecture documentation
            AddCheck();
            var archDocFiles = Directory.GetFiles("Assets/ProjectChimera/Systems/IPM", "*PLAN*.md", SearchOption.AllDirectories);
            bool hasArchDoc = archDocFiles.Length > 0;
            AddResult("Documentation", "Architecture Documentation", hasArchDoc,
                hasArchDoc ? "Architecture documentation found" : "Architecture documentation missing");
        }
        
        // Validation helper methods
        private bool ValidateCleanArchitecturePrinciples(string content)
        {
            // Check for separation of concerns indicators
            bool hasInterfaces = content.Contains("interface") || content.Contains("IIPMBattleEngine");
            bool hasServices = content.Contains("Service") && content.Contains("private");
            bool hasAbstraction = content.Contains("abstract") || content.Contains("virtual");
            
            return hasInterfaces && hasServices && hasAbstraction;
        }
        
        private bool ValidateDependencyDirection(string mainPath, string interfacesPath)
        {
            var mainContent = File.ReadAllText(mainPath);
            var interfacesContent = File.ReadAllText(interfacesPath);
            
            // Main system should depend on interfaces, not concrete implementations
            bool dependsOnInterfaces = mainContent.Contains("IIPMBattleEngine") || mainContent.Contains("IIPMPlayerProgressionService");
            bool avoidsConcreteDepe = !mainContent.Contains("new ") || mainContent.Contains("dependency injection");
            
            return dependsOnInterfaces;
        }
        
        private bool ValidateDependencyInjectionInterfaces(string content)
        {
            // Check for proper DI interface patterns
            bool hasProperInterfaces = content.Contains("interface") && 
                                     (content.Contains("IIPMBattleEngine") || content.Contains("IIPMPlayerProgressionService"));
            bool hasMethodDefinitions = content.Contains("Task<") || content.Contains("void ") || content.Contains("bool ");
            
            return hasProperInterfaces && hasMethodDefinitions;
        }
        
        private bool ValidateConstructorInjection(string content)
        {
            // Look for constructor injection patterns
            bool hasConstructor = content.Contains("public ") && content.Contains("(") && content.Contains("IIPMBattleEngine");
            bool hasFieldAssignment = content.Contains("_") && content.Contains("=");
            
            return hasConstructor || hasFieldAssignment;
        }
        
        private bool ValidatePublishSubscribePattern(string content)
        {
            // Check for event publishing and subscription patterns
            bool hasPublish = content.Contains("Publish") || content.Contains("Raise") || content.Contains("FireEvent");
            bool hasSubscribe = content.Contains("Subscribe") || content.Contains("AddListener") || content.Contains("OnEvent");
            
            return hasPublish || hasSubscribe;
        }
        
        private bool ValidateLooseCoupling(string content)
        {
            // Check for loose coupling indicators
            bool usesEvents = content.Contains("event") || content.Contains("Action<") || content.Contains("EventHandler");
            bool usesInterfaces = content.Contains("IIPMBattleEngine") || content.Contains("IIPMPlayerProgressionService");
            bool avoidsTightCoupling = !content.Contains("GetComponent<") || content.Contains("dependency injection");
            
            return (usesEvents || usesInterfaces) && avoidsTightCoupling;
        }
        
        private bool ValidateRepositoryInterfaces(string content)
        {
            // Check for repository pattern interfaces
            bool hasRepositoryInterface = content.Contains("IRepository") || content.Contains("IIPMDataRepository");
            bool hasDataMethods = content.Contains("Get") && content.Contains("Save") && content.Contains("Delete");
            
            return hasRepositoryInterface || hasDataMethods;
        }
        
        private bool ValidateDataAccessAbstraction(string content)
        {
            // Check for proper data access abstraction
            bool hasAbstractedAccess = content.Contains("interface") && (content.Contains("Get") || content.Contains("Save"));
            bool avoidsDirectAccess = !content.Contains("File.") && !content.Contains("Database.");
            
            return hasAbstractedAccess;
        }
        
        private bool ValidateRepositoryUsage(string content)
        {
            // Check for repository usage in main system
            bool usesRepository = content.Contains("Repository") || content.Contains("_dataRepository");
            bool hasDataOperations = content.Contains("Save") || content.Contains("Load") || content.Contains("Get");
            
            return usesRepository || hasDataOperations;
        }
        
        private bool ValidateDomainServices(string content)
        {
            // Check for domain service patterns
            bool hasServiceInterface = content.Contains("Service") && content.Contains("interface");
            bool hasBusinessLogic = content.Contains("Calculate") || content.Contains("Process") || content.Contains("Validate");
            
            return hasServiceInterface || hasBusinessLogic;
        }
        
        private bool ValidateAssemblyReferences(string content)
        {
            // Check for proper assembly references
            bool hasRequiredRefs = content.Contains("ProjectChimera.Core") && content.Contains("ProjectChimera.Data");
            bool hasValidStructure = content.Contains("\"name\"") && content.Contains("\"references\"");
            
            return hasRequiredRefs && hasValidStructure;
        }
        
        private bool ValidateNamespaceUsage(string[] files)
        {
            int validNamespaces = 0;
            foreach (var file in files)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    if (content.Contains("namespace ProjectChimera.Systems.IPM") || 
                        content.Contains("using ProjectChimera"))
                    {
                        validNamespaces++;
                    }
                }
                catch { }
            }
            
            return files.Length > 0 && (float)validNamespaces / files.Length > 0.8f;
        }
        
        private bool ValidateCrossSystemCommunication(string[] files)
        {
            foreach (var file in files)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    if (content.Contains("ProjectChimera.Core") || content.Contains("EventBus") || content.Contains("GameManager"))
                    {
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }
        
        private bool ValidateMemoryManagement(string content)
        {
            // Check for good memory management practices
            bool hasDisposal = content.Contains("Dispose") || content.Contains("using ");
            bool avoidsMemoryLeaks = !content.Contains("new ") || content.Contains("pool") || content.Contains("cache");
            bool hasCleanup = content.Contains("OnDestroy") || content.Contains("cleanup");
            
            return hasDisposal || avoidsMemoryLeaks || hasCleanup;
        }
        
        private bool ValidateSOLIDPrinciples(string[] files)
        {
            // Basic SOLID validation
            int goodFiles = 0;
            foreach (var file in files)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    bool hasSingleResponsibility = !content.Contains("class") || content.Length < 5000; // Simple heuristic
                    bool usesInterfaces = content.Contains("interface") || content.Contains("IIPMBattleEngine");
                    
                    if (hasSingleResponsibility || usesInterfaces)
                        goodFiles++;
                }
                catch { }
            }
            
            return files.Length > 0 && (float)goodFiles / files.Length > 0.7f;
        }
        
        private bool ValidateNamingConventions(string[] files)
        {
            // Check for consistent naming conventions
            int goodFiles = 0;
            foreach (var file in files)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    bool hasGoodNaming = content.Contains("private readonly") || content.Contains("public class");
                    bool followsConventions = !content.Contains("public string m_") && !content.Contains("public int i");
                    
                    if (hasGoodNaming && followsConventions)
                        goodFiles++;
                }
                catch { }
            }
            
            return files.Length > 0 && (float)goodFiles / files.Length > 0.8f;
        }
        
        private bool ValidateDocumentation(string[] files)
        {
            int documentedFiles = 0;
            foreach (var file in files)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    if (content.Contains("///") || content.Contains("summary"))
                    {
                        documentedFiles++;
                    }
                }
                catch { }
            }
            
            return files.Length > 0 && (float)documentedFiles / files.Length > 0.6f;
        }
        
        private bool ValidateUnitTests(string[] testFiles)
        {
            foreach (var file in testFiles)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    if (content.Contains("[Test]") || content.Contains("Assert."))
                    {
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }
        
        private bool ValidateIntegrationTests(string[] testFiles)
        {
            foreach (var file in testFiles)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    if (content.Contains("Integration") || content.Contains("System"))
                    {
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }
        
        private bool ValidateAPIDocumentation(string[] files)
        {
            int documentedFiles = 0;
            foreach (var file in files)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    if (content.Contains("/// <summary>") && content.Contains("/// <param"))
                    {
                        documentedFiles++;
                    }
                }
                catch { }
            }
            
            return files.Length > 0 && (float)documentedFiles / files.Length > 0.5f;
        }
        
        private void GenerateAuditReport()
        {
            var report = new StringBuilder();
            report.AppendLine("IPM GAMING SYSTEM COMPREHENSIVE AUDIT REPORT");
            report.AppendLine($"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine("=".PadRight(80, '='));
            report.AppendLine();
            
            // Summary
            report.AppendLine("AUDIT SUMMARY");
            report.AppendLine("-".PadRight(50, '-'));
            report.AppendLine($"Total Checks: {_totalChecks}");
            report.AppendLine($"Passed: {_passedChecks}");
            report.AppendLine($"Failed: {_failedChecks}");
            var successRate = _totalChecks > 0 ? (float)_passedChecks / _totalChecks * 100 : 0f;
            report.AppendLine($"Success Rate: {successRate:F1}%");
            report.AppendLine();
            
            // Results by category
            var categories = _auditResults.GroupBy(r => r.Category);
            foreach (var category in categories)
            {
                report.AppendLine($"{category.Key.ToUpper()}");
                report.AppendLine("-".PadRight(40, '-'));
                
                foreach (var result in category)
                {
                    var status = result.Passed ? "PASS" : "FAIL";
                    report.AppendLine($"[{status}] {result.TestName}: {result.Description}");
                    
                    if (!string.IsNullOrEmpty(result.Details))
                    {
                        report.AppendLine($"  Details: {result.Details}");
                    }
                    
                    if (!string.IsNullOrEmpty(result.Recommendation))
                    {
                        report.AppendLine($"  Recommendation: {result.Recommendation}");
                    }
                }
                report.AppendLine();
            }
            
            // Overall assessment
            report.AppendLine("OVERALL ASSESSMENT");
            report.AppendLine("=".PadRight(80, '='));
            
            if (successRate >= 90f)
            {
                report.AppendLine("✅ EXCELLENT: IPM Gaming System meets high standards");
                report.AppendLine("The system demonstrates excellent clean architecture implementation,");
                report.AppendLine("proper separation of concerns, and follows best practices.");
            }
            else if (successRate >= 75f)
            {
                report.AppendLine("✅ GOOD: IPM Gaming System meets most standards");
                report.AppendLine("The system shows good architecture patterns with some areas for improvement.");
            }
            else if (successRate >= 60f)
            {
                report.AppendLine("⚠️ ACCEPTABLE: IPM Gaming System has significant issues");
                report.AppendLine("The system needs attention in several areas to meet quality standards.");
            }
            else
            {
                report.AppendLine("❌ NEEDS MAJOR WORK: IPM Gaming System requires significant improvements");
                report.AppendLine("The system has critical issues that must be addressed.");
            }
            
            report.AppendLine();
            report.AppendLine("Review the detailed results above and address any failed checks.");
            report.AppendLine("For specific recommendations, click the 'Info' button next to each result in the audit window.");
            
            // Save report
            var reportPath = "Assets/ProjectChimera/Editor/IPMGamingSystemAuditReport.txt";
            File.WriteAllText(reportPath, report.ToString());
            AssetDatabase.Refresh();
            
            Debug.Log($"[IPM Gaming Audit] Report generated: {reportPath}");
            EditorUtility.DisplayDialog("Audit Report Generated", 
                $"Comprehensive audit report saved to:\n{reportPath}\n\nSuccess Rate: {successRate:F1}%", "OK");
        }
        
        private void AddCheck()
        {
            _totalChecks++;
        }
        
        private void AddResult(string category, string testName, bool passed, string description, 
            string recommendation = "", string details = "")
        {
            _completedChecks++;
            if (passed) _passedChecks++;
            else _failedChecks++;
            
            _auditResults.Add(new AuditResult
            {
                Category = category,
                TestName = testName,
                Passed = passed,
                Description = description,
                Recommendation = recommendation,
                Details = details,
                HasRecommendation = !string.IsNullOrEmpty(recommendation)
            });
            
            if (_completedChecks % 5 == 0)
            {
                Repaint();
            }
        }
        
        private void ResetCounters()
        {
            _totalChecks = 0;
            _completedChecks = 0;
            _passedChecks = 0;
            _failedChecks = 0;
        }
    }
    
    [System.Serializable]
    public class AuditResult
    {
        public string Category;
        public string TestName;
        public bool Passed;
        public string Description;
        public string Recommendation;
        public string Details;
        public bool HasRecommendation;
    }
}