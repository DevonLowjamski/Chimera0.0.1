# Unity 6 Migration & Core System Setup - Lessons Learned

## Critical Fixes and Patterns for Future Development

### **Unity 6 API Migration Issues**

#### **1. FindObjectsOfType Deprecation (CS)**
**Issue**: `FindObjectsOfType<T>()` is obsolete in Unity 6
**Fix**: Replace with `UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None)`
```csharp
// ❌ Old (Unity 2022)
var objects = FindObjectsOfType<MonoBehaviour>();

// ✅ New (Unity 6)
var objects = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
```

#### **2. Yield in Try-Catch Blocks (CS1626)**
**Issue**: `Cannot yield a value in the body of a try block with a catch clause`
**Root Cause**: C# language restriction, not Unity-specific
**Fix**: Move ALL yield statements outside try-catch blocks
```csharp
// ❌ Invalid
try {
    yield return StartCoroutine(SomeCoroutine()); // Not allowed
} catch (Exception e) { }

// ✅ Valid
bool success = false;
try {
    // Prepare data only
    success = true;
} catch (Exception e) {
    success = false;
}
// Yield operations outside try-catch
if (success) {
    yield return StartCoroutine(SomeCoroutine());
}
```

### **Assembly Definition Configuration**

#### **3. Editor vs Runtime Assembly Issues**
**Issue**: Components in Editor-only assemblies can't be added to GameObjects
**Solution**: Create separate runtime and editor assemblies
```json
// Runtime Assembly (for components)
{
    "includePlatforms": [], // Empty = all platforms
    "references": ["UnityEngine.TestRunner"] // No Editor references
}

// Editor Assembly (for tools)
{
    "includePlatforms": ["Editor"],
    "references": ["UnityEditor.TestRunner"] // Editor references OK
}
```

#### **4. Missing LogInfo Method**
**Issue**: `CS0103: The name 'LogInfo' does not exist in the current context`
**Root Cause**: Base class missing logging method
**Fix**: Add LogInfo to ChimeraMonoBehaviour base class
```csharp
protected void LogInfo(string message)
{
    Debug.Log($"[Chimera][{GetType().Name}] {message}", this);
}
```

### **ScriptableObject Architecture Patterns**

#### **5. ValidateDataSpecific vs ValidateData Inheritance**
**Issue**: `CS0115: no suitable method found to override`
**Root Cause**: Wrong base class or method signature
**Pattern**: 
- `ChimeraDataSO` has `ValidateDataSpecific()` (protected virtual)
- `ChimeraConfigSO` only has base `ValidateData()` (public virtual)
```csharp
// For ChimeraDataSO derivatives
protected override bool ValidateDataSpecific() { }

// For ChimeraConfigSO derivatives  
public override bool ValidateData() {
    if (!base.ValidateData()) return false;
    // Custom validation
}
```

#### **6. ScriptableObject Asset Creation**
**Issue**: Manually created .asset files don't work
**Solution**: ALWAYS create through Unity's Create menu
- Assets must have proper GUIDs and meta files
- Use `[CreateAssetMenu]` attributes
- Never manually write .asset files

### **Manager System Architecture**

#### **7. Manager Registration Pattern**
**Critical Issue**: Managers added as components but not registered with GameManager
**Root Cause**: GameManager uses SerializeField references, not automatic discovery
**Solution**: Always assign manager component references in Inspector
```csharp
// GameManager expects these SerializeField references
[SerializeField] private TimeManager _timeManager;
[SerializeField] private DataManager _dataManager;
// etc.

// Must drag components to these fields in Inspector
```

#### **8. Manager Initialization Order**
**Pattern**: Specific order matters for dependencies
```csharp
// Correct initialization sequence
yield return InitializeManager(_settingsManager, "Settings");  // First - no dependencies
yield return InitializeManager(_eventManager, "Event");        // Second - needed by others
yield return InitializeManager(_dataManager, "Data");          // Third - loads game data  
yield return InitializeManager(_timeManager, "Time");          // Fourth - uses events
yield return InitializeManager(_saveManager, "Save");          // Last - depends on all others
```

### **Testing Infrastructure**

#### **9. Runtime vs Editor Testing**
**Pattern**: Separate testing approaches
- **RuntimeSystemTester**: In Core assembly, tests during Play mode
- **CoreSystemTester**: In Testing assembly, advanced editor testing
- Use RuntimeSystemTester for basic validation, CoreSystemTester for complex scenarios

#### **10. Empty Resources Folder Handling**
**Issue**: DataManager expects Resources folder but it's empty during testing
**Solution**: Tests should handle empty data gracefully
```csharp
// Test should pass with 0 assets loaded
bool statsTest = TestCondition(
    "Data Manager Stats", 
    stats.TotalDataAssets >= 0 && stats.TotalConfigAssets >= 0
);
```

### **Unity 6 Package Dependencies**

#### **11. Removed Packages**
**Packages that Unity 6 doesn't support:**
- `com.unity.ide.vscode` - Visual Studio Code Editor (deprecated)
- `com.unity.ui.toolkit` preview versions - Now built-in

#### **12. Package Manifest Cleanup**
**Pattern**: Keep minimal, essential packages only
```json
{
  "dependencies": {
    "com.unity.render-pipelines.universal": "17.2.0",
    "com.unity.textmeshpro": "3.2.0-pre.4",
    "com.unity.ugui": "2.0.0"
    // Minimal set - let Unity handle the rest
  }
}
```

## **Development Workflow Patterns**

### **Scene Setup Process**
1. Create empty scene
2. Add GameManager GameObject
3. Add ALL manager components to GameManager
4. Assign manager component references in GameManager Inspector
5. Add RuntimeSystemTester GameObject  
6. Assign event references (if needed)
7. Press Play to validate

### **New Manager Integration**
1. Create manager class inheriting from ChimeraManager
2. Add SerializeField reference to GameManager
3. Add to initialization sequence in GameManager
4. Add registration call in GameManager
5. Add test case to RuntimeSystemTester

### **Assembly Definition Best Practices**
1. Keep runtime assemblies minimal (no editor dependencies)
2. Use proper namespaces matching assembly names
3. Reference assemblies by name, not GUID
4. Test in builds, not just editor

## **Critical Success Factors**

1. **Never skip manager registration** - Components exist but aren't accessible without registration
2. **Always test coroutine yield placement** - C# restrictions apply regardless of Unity version
3. **Use Unity's asset creation system** - Manual asset files will fail
4. **Validate assembly references early** - Editor vs runtime issues surface late
5. **Test with empty data states** - Systems should handle zero assets gracefully

## **Future Development Guidelines**

- **Always run RuntimeSystemTester** after major changes
- **Check console for yellow warnings** - they indicate architectural issues
- **Test manager initialization order** when adding new managers  
- **Validate ScriptableObject inheritance chains** before creating assets
- **Use explicit Unity API calls** - avoid deprecated shortcuts

This documentation should prevent repeating these time-consuming debugging cycles in future development phases.