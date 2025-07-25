# Project Chimera - Testing Guide

## Overview

This guide covers testing strategies and implementations for Project Chimera's core systems. While the project is in its foundational phase, comprehensive testing ensures reliability and maintainability as we build more complex systems.

## What Can Be Tested Now

### ✅ Core Architecture Testing

#### 1. Manager System Integration
- **Manager Initialization**: All managers properly initialize and register
- **Singleton Patterns**: GameManager singleton works correctly
- **Manager Registry**: Dynamic manager access through GameManager
- **Lifecycle Management**: Proper startup and shutdown sequences

#### 2. Event System Validation
- **Event Channel Registration**: Events properly register with EventManager
- **Event Raising**: Events can be raised without errors
- **Type Safety**: Generic event channels maintain type safety
- **Memory Management**: Events properly clean up listeners

#### 3. Time Management System
- **Time Scale Changes**: Time scaling works correctly
- **Pause/Resume**: Game can be paused and resumed
- **Time Calculations**: Real-time to game-time conversions
- **Performance Tracking**: Frame time monitoring

#### 4. Data Management Validation
- **Asset Loading**: ScriptableObjects load from Resources
- **Data Validation**: Custom validation rules work correctly
- **Type-Safe Retrieval**: Generic data access methods
- **Cache Performance**: Data caching and retrieval speed

#### 5. Settings System Testing
- **Value Setting/Getting**: All setting types (bool, int, float, string)
- **Validation**: Min/max bounds and type validation
- **Persistence**: Settings save/load from PlayerPrefs
- **Real-time Application**: Settings apply immediately when changed

#### 6. Save System Foundation
- **Directory Creation**: Save directory properly created
- **Component Registration**: ISaveable components register correctly
- **Save Slot Management**: Save slot discovery and management
- **File Operations**: Basic file read/write operations

## Testing Infrastructure

### CoreSystemTester.cs
A comprehensive test controller that validates all core systems:

```csharp
// Run all tests
var tester = FindObjectOfType<CoreSystemTester>();
tester.RunAllTests();

// Run individual tests
tester.TestGameManagerOnly();
tester.TestTimeManagerOnly();
tester.TestEventSystemOnly();
```

### Test Data Assets
Sample ScriptableObjects for testing data management:
- **TestPlantDataSO**: Cannabis plant data with validation
- **TestEquipmentDataSO**: Cultivation equipment data
- **TestEnvironmentConfigSO**: Environmental configuration

### Test Event Channels
Event channels specifically for testing:
- **TestSimpleEventSO**: Basic event testing
- **TestStringEventSO**: String data events
- **TestFloatEventSO**: Numeric data events

## How to Run Tests

### Method 1: Automatic Testing
1. Open the **CoreSystemTest** scene in `/Assets/ProjectChimera/Testing/Scenes/`
2. Enter Play Mode
3. Tests will run automatically and results will appear in the Console

### Method 2: Manual Testing
1. Add the **CoreSystemTester** component to any GameObject
2. Use the Context Menu options:
   - "Run Tests" - Runs all tests
   - "Test GameManager Only" - Tests just the GameManager
   - "Test TimeManager Only" - Tests just the TimeManager
   - "Test EventSystem Only" - Tests just the event system

### Method 3: Inspector Testing
1. Select the GameObject with **CoreSystemTester**
2. Check "Run Tests On Start" to run automatically
3. Use the public methods for targeted testing

## Test Results Interpretation

### Expected Results
With all systems properly set up, you should see:

```
=== Starting Project Chimera Core System Tests ===
✅ Manager Initialization: PASSED
✅ Event System: PASSED
✅ Time Manager: PASSED
✅ Data Manager: PASSED
✅ Settings Manager: PASSED
✅ Save Manager: PASSED

=== Test Summary ===
Tests Run: 6
Tests Passed: 6
Tests Failed: 0
Success Rate: 100.0%
🎉 All tests passed!
```

### Common Issues and Solutions

#### Manager Not Initialized
**Issue**: Manager initialization tests fail
**Solution**: 
- Ensure all manager components are added to the test GameObject
- Check that managers have their required ScriptableObject references
- Verify initialization order in GameManager

#### Event System Failures
**Issue**: Event tests fail
**Solution**:
- Create test event ScriptableObjects in the Resources folder
- Assign event references in the CoreSystemTester inspector
- Check for null event references

#### Time Manager Issues
**Issue**: Time scaling tests fail
**Solution**:
- Verify TimeManager has proper configuration
- Check for conflicting time scale modifications
- Ensure Time.timeScale is not locked by other systems

#### Data Validation Errors
**Issue**: Data validation tests fail
**Solution**:
- Create test data assets using the provided test ScriptableObjects
- Place test assets in Resources folders for discovery
- Fix validation errors in test data

## Performance Testing

### Metrics to Monitor
- **Initialization Time**: How long managers take to initialize
- **Event Throughput**: Events processed per frame
- **Memory Usage**: Object creation and cleanup
- **Frame Rate Impact**: Performance impact of core systems

### Performance Benchmarks
With the current architecture, expect:
- Manager initialization: < 100ms total
- Event processing: > 1000 events/second
- Data asset loading: < 50ms for small datasets
- Settings changes: < 5ms application time

## Unit Testing Framework (Future)

### Planned Unit Tests
- **Manager Lifecycle Tests**: Automated initialization/shutdown testing
- **Event System Tests**: Comprehensive event flow validation
- **Data Integrity Tests**: ScriptableObject validation testing
- **Save/Load Tests**: Data persistence validation
- **Settings Tests**: All setting categories and validation

### Integration Testing
- **Cross-Manager Communication**: Manager interaction testing
- **Event Flow Testing**: End-to-end event processing
- **Time Scaling Integration**: Time-dependent system coordination
- **Data Pipeline Testing**: Asset loading and validation flow

## Best Practices for Testing

### 1. Test Early and Often
- Run tests after any significant changes
- Use automated testing for regression detection
- Test both success and failure cases

### 2. Realistic Test Data
- Use realistic cannabis cultivation data
- Test edge cases and boundary conditions
- Validate all data constraints

### 3. Performance Awareness
- Monitor performance impact of tests
- Use profiler to identify bottlenecks
- Test with various data set sizes

### 4. Maintainable Tests
- Keep tests simple and focused
- Document test purposes and expectations
- Update tests when systems change

## Continuous Integration Setup (Future)

### Automated Testing Pipeline
1. **Build Validation**: Ensure project compiles
2. **Unit Tests**: Run all unit tests
3. **Integration Tests**: Run manager integration tests
4. **Performance Tests**: Validate performance benchmarks
5. **Memory Tests**: Check for memory leaks

### Test Coverage Goals
- **Core Systems**: 100% test coverage
- **Manager Classes**: 95% test coverage
- **Event System**: 100% test coverage
- **Data Validation**: 90% test coverage

## Next Steps

As Project Chimera evolves, the testing infrastructure will expand to include:

1. **Genetics System Testing**: Breeding algorithm validation
2. **Environmental Simulation Testing**: Climate system accuracy
3. **Economic System Testing**: Market simulation validation
4. **UI/UX Testing**: User interaction validation
5. **Performance Stress Testing**: Large-scale simulation testing

The current testing foundation provides a solid base for validating core systems and will scale as the project grows in complexity.