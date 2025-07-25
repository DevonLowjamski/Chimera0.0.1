# Project Chimera Testing System - Setup Guide

## Overview

This guide provides step-by-step instructions for setting up and using the comprehensive Project Chimera Testing System. The framework includes 14 specialized components providing complete validation of all cultivation systems.

## Prerequisites

### System Requirements
- **Unity Version**: Unity 6.x (6000.x or higher)
- **Platform**: Windows, macOS, or Linux development environment
- **Memory**: Minimum 8GB RAM (16GB recommended for performance testing)
- **Storage**: 2GB free space for test reports and documentation

### Project Setup
1. Ensure Project Chimera is properly imported
2. Verify all cultivation system ScriptableObjects are created
3. Confirm all core managers are properly configured
4. Check that the testing scene includes all required components

## Component Setup

### 1. Core Testing Scene Setup

Create or open the `CoreTestScene.unity` file and ensure it contains:

```
CoreTestScene
├── GameManager (with all sub-managers)
│   ├── SettingsManager
│   ├── SaveManager
│   ├── EventManager
│   ├── DataManager
│   └── TimeManager
├── CultivationManager
├── Testing Components
│   ├── AutomatedTestManager
│   ├── CultivationTestCoordinator
│   ├── AdvancedCultivationTestRunner
│   ├── AdvancedCultivationTester
│   ├── CoreSystemTester
│   ├── CultivationSystemValidator
│   ├── CultivationIntegrationTests
│   ├── CultivationPerformanceTests
│   ├── TestReportGenerator
│   ├── TestDashboard
│   └── TestDocumentationGenerator
└── UI Canvas (for TestDashboard)
```

### 2. Component Configuration

#### AutomatedTestManager Configuration
```csharp
[Header("Automation Configuration")]
runFullSuiteOnStart = false          // Set to true for automatic testing
enableScheduledTests = false         // Enable for periodic testing
scheduledTestInterval = 3600f        // 1 hour intervals
enablePerformanceBaseline = true    // Track performance over time
enableRegressionTesting = true      // Detect performance regressions

[Header("Test Suite Configuration")]
EnableCoreTests = true              // Test core Unity systems
EnableCultivationTests = true       // Test cultivation systems
EnableIntegrationTests = true       // Test system integration
EnablePerformanceTests = true       // Test performance benchmarks
TestTimeout = 300f                  // 5 minute timeout per test phase
```

#### TestDashboard Configuration
```csharp
[Header("Dashboard Configuration")]
showDashboard = true                // Display real-time dashboard
showDetailedStats = true           // Show detailed performance metrics
enableRealTimeUpdates = true       // Update dashboard in real-time
updateInterval = 1f                // Update every second

[Header("UI Configuration")]
dashboardSize = (800, 600)         // Dashboard window size
dashboardPosition = (10, 10)       // Screen position
scrollViewHeight = 150f            // Log scroll area height
```

#### Performance Test Thresholds
```csharp
[Header("Performance Thresholds")]
maxMemoryUsageMB = 500f            // Maximum memory usage
maxFrameTimeMS = 16.67f            // Target 60 FPS
maxGCAllocPerFrame = 1024f         // 1KB GC allocation per frame
maxDrawCalls = 1000                // Maximum draw calls
```

## Usage Instructions

### 1. Quick Start - Automated Testing

For complete automated testing:

```csharp
// Method 1: Using the AutomatedTestManager
var testManager = FindObjectOfType<AutomatedTestManager>();
testManager.StartAutomatedTestSuite();

// Method 2: Using Context Menu
// Right-click on AutomatedTestManager → "Start Automated Test Suite"
```

### 2. Individual Component Testing

For testing specific systems:

```csharp
// Core system testing
var coreTests = FindObjectOfType<CoreSystemTester>();
coreTests.RunTestsManually();

// Cultivation system testing
var cultivationCoordinator = FindObjectOfType<CultivationTestCoordinator>();
cultivationCoordinator.StartCoordinatedTesting();

// Performance testing only
var performanceTests = FindObjectOfType<CultivationPerformanceTests>();
performanceTests.StartPerformanceTests();

// Integration testing only
var integrationTests = FindObjectOfType<CultivationIntegrationTests>();
integrationTests.StartIntegrationTests();
```

### 3. Real-Time Monitoring

The TestDashboard provides real-time monitoring:

1. **System Status**: Shows which components are available and running
2. **Test Controls**: Interactive buttons for manual test execution
3. **Current Progress**: Real-time display of active test phase
4. **Performance Metrics**: Live frame time and memory usage
5. **Test History**: Recent test results with status indicators
6. **Log Messages**: Comprehensive logging with auto-scroll

### 4. Report Generation

Generate comprehensive reports:

```csharp
// Generate all report formats
var reportGenerator = FindObjectOfType<TestReportGenerator>();
reportGenerator.GenerateComprehensiveReport();

// Generate technical documentation
var docGenerator = FindObjectOfType<TestDocumentationGenerator>();
docGenerator.GenerateComprehensiveDocumentation();
```

## Testing Workflow

### Phase 1: Pre-Test Validation
- Unity version compatibility check
- System memory and performance baseline
- Component availability verification
- Manager initialization validation

### Phase 2: Core System Testing
- GameManager and sub-manager functionality
- Event system validation
- Data persistence testing
- Settings management verification

### Phase 3: Cultivation System Testing
- VPD management calculations
- Environmental automation control
- Fertigation system operations
- Plant physiology simulations

### Phase 4: Integration Testing
- Cross-system data flow validation
- Manager coordination testing
- Component interaction verification
- System-wide state management

### Phase 5: Performance Testing
- Frame rate benchmarking
- Memory usage monitoring
- Garbage collection analysis
- Resource utilization assessment

### Phase 6: Regression Analysis
- Performance baseline comparison
- Trend analysis and detection
- Performance degradation alerts
- Historical data comparison

### Phase 7: Report Generation
- Multi-format report creation
- Documentation generation
- Performance analysis compilation
- Recommendation development

## Output Files and Locations

### Test Reports
- **Location**: `Application.persistentDataPath/TestReports/`
- **Formats**: HTML, JSON, CSV
- **Naming**: `ProjectChimera_TestReport_YYYY-MM-DD_HH-mm-ss`

### Technical Documentation
- **Location**: `Assets/TestDocumentation/`
- **Files**:
  - `TestingFrameworkDocumentation.md/html`
  - `QuickStartGuide.md`
  - `APIReference.md`
  - `ArchitectureOverview.md`
  - `TroubleshootingGuide.md`
  - `TestCoverageReport.md`

## Performance Benchmarks

### Target Metrics
- **Frame Time**: < 16.67ms (60 FPS minimum)
- **Memory Usage**: < 500MB peak usage
- **Test Suite Duration**: < 10 minutes complete execution
- **System Startup**: < 5 seconds initialization

### Optimization Tips
1. Close unnecessary applications during testing
2. Ensure adequate system memory (8GB+ available)
3. Use SSD storage for better I/O performance
4. Monitor system resources before testing

## Troubleshooting

### Common Issues

#### Tests Not Starting
**Symptoms**: Components don't respond to start commands
**Solutions**:
1. Verify all managers are properly initialized
2. Check Unity Console for error messages
3. Ensure scene contains all required components
4. Restart Unity and reload the test scene

#### Performance Test Failures
**Symptoms**: Performance tests consistently fail benchmarks
**Solutions**:
1. Adjust performance thresholds in component configuration
2. Close resource-intensive applications
3. Check system specifications meet requirements
4. Monitor background processes

#### Report Generation Failures
**Symptoms**: Reports not generating or incomplete
**Solutions**:
1. Verify write permissions for output directories
2. Check available disk space (minimum 1GB free)
3. Ensure test data is properly collected before generation
4. Check Unity Console for specific error messages

#### Dashboard Not Displaying
**Symptoms**: TestDashboard UI not visible
**Solutions**:
1. Verify `showDashboard` is set to true
2. Check dashboard position is within screen bounds
3. Ensure UI Canvas is properly configured
4. Try toggling dashboard with Context Menu

### Debug Information

#### Enabling Detailed Logging
```csharp
// Enable detailed logging for all components
testComponent.EnableComprehensiveLogging = true;
testComponent.EnableDetailedLogging = true;
```

#### Console Commands
- Use Context Menu options on any test component
- Monitor Unity Console for real-time status updates
- Check TestDashboard for visual status indicators
- Review generated log files for detailed information

## Advanced Configuration

### Custom Test Suites
```csharp
// Create custom test configuration
var customConfig = new TestSuiteConfig
{
    EnableCoreTests = true,
    EnableCultivationTests = true,
    EnableIntegrationTests = false,  // Skip integration
    EnablePerformanceTests = true,
    TestTimeout = 600f,              // Longer timeout
    EnableDetailedLogging = true
};

// Apply to AutomatedTestManager
automatedTestManager.TestSuiteConfig = customConfig;
```

### Performance Monitoring
```csharp
// Start continuous performance monitoring
var performanceTests = FindObjectOfType<CultivationPerformanceTests>();
performanceTests.StartContinuousMonitoring();

// Set custom performance thresholds
performanceTests.MaxMemoryUsageMB = 750f;  // Higher memory limit
performanceTests.MaxFrameTimeMS = 20f;     // Allow 50 FPS minimum
```

### Scheduled Testing
```csharp
// Enable automated scheduled testing
var testManager = FindObjectOfType<AutomatedTestManager>();
testManager.EnableScheduledTests = true;
testManager.ScheduledTestInterval = 7200f;  // Every 2 hours
```

## Integration with Development Workflow

### Continuous Integration
1. Configure automated testing in CI/CD pipeline
2. Set performance regression thresholds
3. Generate reports for each build
4. Archive test results for historical analysis

### Code Reviews
1. Run tests before submitting code changes
2. Check performance impact of new features
3. Validate cultivation system accuracy
4. Ensure integration points remain stable

### Release Validation
1. Execute complete test suite before releases
2. Validate performance benchmarks
3. Generate comprehensive documentation
4. Verify scientific accuracy of calculations

---

*Project Chimera Testing System Setup Guide*  
*Version 2.0 | 14 Components | Enterprise-Grade Testing*  
*Last Updated: 2024-12-07* 