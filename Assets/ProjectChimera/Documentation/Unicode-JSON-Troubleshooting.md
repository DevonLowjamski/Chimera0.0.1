# Unicode JSON Error Troubleshooting Guide

## Overview
This guide helps resolve "invalid_request_error" with "no low surrogate in string" errors that occur during JSON serialization in Project Chimera.

## Problem Description
The error occurs when JSON contains invalid Unicode surrogate pairs, typically in large payloads (~6MB). Surrogate pairs are Unicode encoding for characters outside the Basic Multilingual Plane.

## Root Causes

### 1. Invalid Unicode in Source Data
- Corrupted text from external sources
- Copy-paste from applications with encoding issues
- Generated text with incomplete Unicode sequences

### 2. Data Processing Issues
- String manipulation that breaks surrogate pairs
- Improper encoding/decoding operations
- Memory corruption affecting string data

### 3. Large Dataset Generation
- Procedural text generation
- Genetic algorithm outputs
- Automated testing data

## Solutions Implemented

### 1. Unicode Sanitization in DataSerializer
```csharp
// Usage in DataSerializer.cs
string jsonData = JsonUtility.ToJson(data, true);
jsonData = SanitizeUnicodeString(jsonData);
```

### 2. UI State Manager Protection
```csharp
// Usage in UIStateManager.cs
var json = JsonUtility.ToJson(stateData);
json = SanitizeUnicodeString(json);
```

### 3. Global JSON Helper
```csharp
using ProjectChimera.Core;

// Safe JSON serialization
string json = JsonUtilityHelper.ToJsonSafe(myObject, true);

// Safe JSON deserialization
MyClass obj = JsonUtilityHelper.FromJsonSafe<MyClass>(json);
```

### 4. Unicode Diagnostic Tools
```csharp
using ProjectChimera.Core;

// Check object for Unicode issues before serialization
var report = UnicodeDataDiagnostic.ScanObjectForUnicodeIssues(myObject);
if (report.HasIssues)
{
    Debug.LogWarning($"Unicode issues found: {report}");
}

// Quick string validation
if (UnicodeDataDiagnostic.HasUnicodeIssues(myString, out var issues))
{
    Debug.LogWarning($"String has {issues.Count} Unicode issues");
}
```

## Prevention Strategies

### 1. Input Validation
Always validate text input from external sources:
```csharp
public void ProcessExternalText(string input)
{
    // Validate before processing
    if (UnicodeDataDiagnostic.HasUnicodeIssues(input, out var issues))
    {
        // Log and sanitize
        Debug.LogWarning($"Input has Unicode issues: {issues.Count}");
        input = JsonUtilityHelper.SanitizeUnicodeString(input);
    }
    
    // Continue processing...
}
```

### 2. Safe Text Generation
When generating procedural text:
```csharp
public string GenerateProceduralText()
{
    string result = DoTextGeneration();
    
    // Always validate generated text
    var report = UnicodeDataDiagnostic.ScanObjectForUnicodeIssues(result);
    if (report.HasIssues)
    {
        Debug.LogWarning("Generated text has Unicode issues, regenerating...");
        return GenerateProceduralTextSafe();
    }
    
    return result;
}
```

### 3. Regular Data Auditing
Add periodic Unicode validation:
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
public void AuditDataForUnicodeIssues()
{
    // Check genetic data
    foreach (var genotype in geneticDatabase)
    {
        UnicodeDataDiagnostic.LogUnicodeDiagnostic(genotype, $"Genotype {genotype.GenotypeId}");
    }
    
    // Check UI state
    foreach (var state in uiStates)
    {
        UnicodeDataDiagnostic.LogUnicodeDiagnostic(state, $"UI State {state.ElementId}");
    }
}
```

## Testing and Validation

### 1. Create Test Data with Unicode Issues
```csharp
[Test]
public void TestUnicodeErrorHandling()
{
    // Create test string with invalid surrogate
    string invalidUnicode = "Test\uD800Invalid"; // High surrogate without low surrogate
    
    // Verify detection
    Assert.IsTrue(UnicodeDataDiagnostic.HasUnicodeIssues(invalidUnicode, out var issues));
    Assert.AreEqual(1, issues.Count);
    
    // Verify safe serialization
    var testObject = new { text = invalidUnicode };
    string json = JsonUtilityHelper.ToJsonSafe(testObject);
    Assert.IsTrue(JsonUtilityHelper.IsValidJson(json));
}
```

### 2. Performance Testing
```csharp
[Test]
public void TestLargeDataPerformance()
{
    // Create large test dataset
    var largeData = CreateLargeTestDataset();
    
    var stopwatch = Stopwatch.StartNew();
    string json = JsonUtilityHelper.ToJsonSafe(largeData);
    stopwatch.Stop();
    
    Debug.Log($"Large data serialization: {stopwatch.ElapsedMilliseconds}ms");
    Assert.IsTrue(JsonUtilityHelper.IsValidJson(json));
}
```

## Debugging Steps

### 1. Identify Source of Unicode Issues
1. Enable debug logging in relevant classes
2. Use `UnicodeDataDiagnostic.LogUnicodeDiagnostic()` on suspect objects
3. Check the Unity Console for Unicode warnings

### 2. Locate Problematic Data
1. Add diagnostic calls before major JSON operations:
```csharp
// Before saving
UnicodeDataDiagnostic.LogUnicodeDiagnostic(saveData, "Before Save");

// Before API calls
UnicodeDataDiagnostic.LogUnicodeDiagnostic(requestData, "Before API Request");
```

### 3. Trace Data Flow
1. Add diagnostics at data transformation points
2. Check file I/O operations
3. Validate network request/response data

## Common Scenarios

### 1. Plant Name Generation
```csharp
public string GeneratePlantName()
{
    string name = ProceduralNameGenerator.Generate();
    
    // Validate generated name
    if (UnicodeDataDiagnostic.HasUnicodeIssues(name, out var issues))
    {
        Debug.LogWarning($"Generated plant name has Unicode issues: {name}");
        name = "Default Plant Name"; // Fallback
    }
    
    return name;
}
```

### 2. User Input Processing
```csharp
public void ProcessUserFacilityName(string input)
{
    // Sanitize user input
    string safeName = JsonUtilityHelper.SanitizeUnicodeString(input);
    
    if (safeName != input)
    {
        Debug.LogWarning("User input contained invalid Unicode characters, sanitized");
    }
    
    facility.Name = safeName;
}
```

### 3. Import/Export Operations
```csharp
public void ImportGeneticData(string filePath)
{
    string jsonData = File.ReadAllText(filePath, Encoding.UTF8);
    
    // Validate before parsing
    if (!JsonUtilityHelper.IsValidJson(jsonData))
    {
        Debug.LogError($"Invalid JSON in file: {filePath}");
        return;
    }
    
    var geneticData = JsonUtilityHelper.FromJsonSafe<GeneticDataCollection>(jsonData);
    // Continue processing...
}
```

## Emergency Fixes

If you encounter the error in production:

1. **Immediate Fix**: Replace direct `JsonUtility` calls with `JsonUtilityHelper` methods
2. **Data Recovery**: Use `JsonUtilityHelper.FromJsonSafe()` to attempt loading corrupted saves
3. **Prevention**: Add Unicode validation to all text input points

## Configuration Options

Add to your project settings:
```csharp
[CreateAssetMenu(fileName = "UnicodeSettings", menuName = "Project Chimera/Unicode Settings")]
public class UnicodeSettingsSO : ScriptableObject
{
    [Header("Unicode Validation")]
    public bool enableUnicodeValidation = true;
    public bool logUnicodeIssues = true;
    public bool autoSanitizeInput = true;
    
    [Header("Performance")]
    public bool enablePerformanceLogging = false;
    public int maxObjectDepthScan = 10;
}
```

## Support and Reporting

When reporting Unicode issues, include:
1. Unity Console logs with Unicode warnings
2. Sample data that reproduces the issue
3. Steps to reproduce the error
4. System locale and input method information

Remember: These solutions prevent the error at the source rather than just catching it, ensuring data integrity throughout Project Chimera. 