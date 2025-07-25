# Project Chimera: Event Channel Mappings & Documentation

## PC-007.4: Complete Event System Architecture Reference

This document provides comprehensive mapping of all event channels used in Project Chimera's event-driven architecture, ensuring consistent cross-system communication.

---

## Event System Architecture

### Core Event Types

#### **SimpleGameEventSO**
- **Purpose**: Events without data payload
- **Usage**: Trigger notifications, state changes
- **Examples**: `OnGameStarted`, `OnGamePaused`, `OnSaveCompleted`

#### **GameEventSO<T>**
- **Purpose**: Type-safe events with data payload
- **Usage**: Complex data transmission between systems
- **Examples**: `GameEventSO<PlantData>`, `GameEventSO<MarketTransaction>`

---

## Critical Event Channel Mappings

### 🌱 **Plant & Cultivation Events**

| Event Name | Type | Data Structure | Publisher | Subscribers |
|------------|------|---------------|-----------|-------------|
| `OnPlantCreated` | `GameEventSO<PlantData>` | PlantData | CultivationManager | PlantManager, ProgressionManager, UI |
| `OnPlantHarvested` | `GameEventSO<HarvestData>` | HarvestData | CultivationManager | MarketManager, ProgressionManager, UI |
| `OnPlantGrowthStageChanged` | `GameEventSO<PlantInstance>` | PlantInstance | PlantManager | UI, Effects, Tutorial |
| `OnPlantHealthChanged` | `GameEventSO<PlantInstance>` | PlantInstance | PlantManager | UI, Effects, AI Advisor |
| `OnPlantDied` | `GameEventSO<PlantInstance>` | PlantInstance | PlantManager | UI, Effects, ProgressionManager |

### 💰 **Economy & Market Events**

| Event Name | Type | Data Structure | Publisher | Subscribers |
|------------|------|---------------|-----------|-------------|
| `OnMarketPriceChanged` | `GameEventSO<MarketData>` | MarketData | MarketManager | UI, AI Advisor, Effects |
| `OnSaleCompleted` | `GameEventSO<TransactionData>` | TransactionData | MarketManager | ProgressionManager, UI, Effects |
| `OnMarketConditionsChanged` | `GameEventSO<MarketConditions>` | MarketConditions | MarketManager | AI Advisor, UI |
| `OnContractOffered` | `GameEventSO<ContractData>` | ContractData | ContractManager | UI, NotificationManager |
| `OnContractCompleted` | `GameEventSO<ContractData>` | ContractData | ContractManager | ProgressionManager, UI |

### 🧬 **Genetics & Breeding Events**

| Event Name | Type | Data Structure | Publisher | Subscribers |
|------------|------|---------------|-----------|-------------|
| `OnBreedingCompleted` | `GameEventSO<BreedingResult>` | BreedingResult | GeneticsManager | ProgressionManager, UI, AI Advisor |
| `OnTraitExpressionCalculated` | `GameEventSO<PlantInstanceSO>` | PlantInstanceSO | GeneticsManager | UI, PlantManager |
| `OnMutationOccurred` | `GameEventSO<GeneticMutation>` | GeneticMutation | GeneticsManager | UI, ProgressionManager |
| `OnGeneticAnalysisComplete` | `GameEventSO<GeneticAnalysis>` | GeneticAnalysis | GeneticsManager | UI, AI Advisor |

### 🌡️ **Environmental Events**

| Event Name | Type | Data Structure | Publisher | Subscribers |
|------------|------|---------------|-----------|-------------|
| `OnEnvironmentChanged` | `GameEventSO<EnvironmentalData>` | EnvironmentalData | EnvironmentalManager | PlantManager, UI, Effects |
| `OnEquipmentStatusChanged` | `GameEventSO<EquipmentData>` | EquipmentData | EquipmentManager | UI, MaintenanceManager |
| `OnAlertTriggered` | `GameEventSO<AlertData>` | AlertData | EnvironmentalManager | UI, NotificationManager, Effects |
| `OnOptimizationSuggestion` | `GameEventSO<OptimizationData>` | OptimizationData | AI Advisor | UI, EnvironmentalManager |

### 📈 **Progression & Achievement Events**

| Event Name | Type | Data Structure | Publisher | Subscribers |
|------------|------|---------------|-----------|-------------|
| `OnExperienceGained` | `GameEventSO<XPData>` | XPData | ProgressionManager | UI, Effects |
| `OnLevelUp` | `GameEventSO<ProgressionData>` | ProgressionData | ProgressionManager | UI, Effects, NotificationManager |
| `OnAchievementUnlocked` | `GameEventSO<AchievementData>` | AchievementData | AchievementManager | UI, Effects, NotificationManager |
| `OnSkillUnlocked` | `GameEventSO<SkillData>` | SkillData | SkillTreeManager | UI, FeatureUnlockManager |
| `OnResearchCompleted` | `GameEventSO<ResearchData>` | ResearchData | ResearchManager | UI, FeatureUnlockManager |

### 💾 **Save System Events**

| Event Name | Type | Data Structure | Publisher | Subscribers |
|------------|------|---------------|-----------|-------------|
| `OnSaveStarted` | `SimpleGameEventSO` | None | SaveManager | UI, LoadingManager |
| `OnSaveCompleted` | `SimpleGameEventSO` | None | SaveManager | UI, NotificationManager |
| `OnLoadStarted` | `SimpleGameEventSO` | None | SaveManager | UI, LoadingManager |
| `OnLoadCompleted` | `SimpleGameEventSO` | None | SaveManager | All Managers, UI |
| `OnSaveError` | `GameEventSO<SaveError>` | SaveError | SaveManager | UI, ErrorManager |

### 🎮 **UI & User Interaction Events**

| Event Name | Type | Data Structure | Publisher | Subscribers |
|------------|------|---------------|-----------|-------------|
| `OnUIStateChanged` | `GameEventSO<UIState>` | UIState | UIManager | All UI Panels |
| `OnUserAction` | `GameEventSO<UserActionData>` | UserActionData | UI Components | Tutorial, Analytics |
| `OnNotificationTriggered` | `GameEventSO<NotificationData>` | NotificationData | Various | NotificationManager, UI |
| `OnModalOpened` | `GameEventSO<ModalData>` | ModalData | UIManager | Input Manager, Tutorial |

---

## Event Data Structures

### Core Data Types

```csharp
// Plant Events
public struct PlantData
{
    public string PlantID;
    public PlantStrainSO Strain;
    public Vector3 Position;
    public PlantGrowthStage GrowthStage;
    public float Health;
}

public struct HarvestData
{
    public string PlantID;
    public PlantStrainSO Strain;
    public float YieldAmount;
    public float Quality;
    public DateTime HarvestTime;
}

// Market Events
public struct TransactionData
{
    public MarketProductSO Product;
    public float Quantity;
    public float Price;
    public float TotalValue;
    public DateTime TransactionTime;
}

// Progression Events
public struct XPData
{
    public SkillType SkillType;
    public float XPGained;
    public string Source;
    public float NewTotal;
}
```

---

## Event Channel Usage Patterns

### ✅ **Correct Event Usage**

```csharp
// Publishing an event
[SerializeField] private GameEventSO<PlantData> _onPlantCreated;

public void CreatePlant(PlantStrainSO strain, Vector3 position)
{
    var plant = InstantiatePlant(strain, position);
    
    // Raise event with plant data
    var plantData = new PlantData
    {
        PlantID = plant.PlantID,
        Strain = strain,
        Position = position,
        GrowthStage = PlantGrowthStage.Seed,
        Health = 100f
    };
    
    _onPlantCreated?.Raise(plantData);
}

// Subscribing to an event
protected override void OnManagerInitialize()
{
    if (_onPlantCreated != null)
    {
        _onPlantCreated.OnEventRaised += OnPlantCreated;
    }
}

private void OnPlantCreated(PlantData plantData)
{
    // React to plant creation
    UpdateUI(plantData);
    TriggerEffects(plantData.Position);
}
```

### ❌ **Incorrect Direct Reference (Architectural Violation)**

```csharp
// VIOLATION: Direct manager reference
private PlantManager _plantManager;

void Start()
{
    _plantManager = GameManager.Instance.GetManager<PlantManager>();
    var plants = _plantManager.GetAllPlants(); // WRONG!
}
```

---

## Event System Best Practices

### 1. **Event Naming Conventions**
- Use descriptive names: `OnPlantHarvested` not `OnHarvest`
- Start with `On` prefix for consistency
- Use PascalCase for event names
- Include relevant context in name

### 2. **Data Structure Design**
- Keep event data structures lightweight
- Include all necessary context in event data
- Use value types (structs) for simple data
- Avoid complex object hierarchies in events

### 3. **Publisher Responsibilities**
- Only raise events for significant state changes
- Include complete context in event data
- Raise events consistently across similar operations
- Document expected subscriber behavior

### 4. **Subscriber Responsibilities**
- Subscribe in OnManagerInitialize or OnEnable
- Unsubscribe in OnManagerDestroy or OnDisable
- Handle null/invalid event data gracefully
- Avoid heavy processing in event handlers

### 5. **Performance Considerations**
- Minimize event frequency for high-frequency operations
- Use object pooling for complex event data
- Batch similar events when possible
- Avoid creating garbage in event handlers

---

## Event System Testing

### Validation Methods

1. **Event Flow Testing**
   - Verify events are raised correctly
   - Confirm subscribers receive events
   - Test event data integrity

2. **Integration Testing**
   - Test complete event chains
   - Verify cross-system communication
   - Test error handling and edge cases

3. **Performance Testing**
   - Measure event system overhead
   - Test with high event frequency
   - Validate memory usage patterns

---

## Migration Guide

### Converting Direct References to Events

1. **Identify Direct References**
   ```csharp
   // BEFORE: Direct reference
   private PlantManager _plantManager;
   var plant = _plantManager.GetPlant(id);
   ```

2. **Create Event Channel**
   ```csharp
   // Add event channel
   [SerializeField] private GameEventSO<PlantRequestData> _onPlantRequested;
   [SerializeField] private GameEventSO<PlantData> _onPlantDataReceived;
   ```

3. **Implement Event-Driven Communication**
   ```csharp
   // AFTER: Event-driven
   public void RequestPlant(string plantId)
   {
       var request = new PlantRequestData { PlantID = plantId };
       _onPlantRequested?.Raise(request);
   }
   
   private void OnPlantDataReceived(PlantData plantData)
   {
       // Handle received plant data
   }
   ```

---

## Event System Status

### ✅ **Implemented & Verified**
- Core event infrastructure (GameEventSO<T>, SimpleGameEventSO)
- Event listener system with GameEventListener<T>
- Basic event channels for plant, market, and progression systems
- Test event channels for validation

### 🔄 **In Progress**
- Migration of remaining direct manager references
- Standardization of event naming conventions
- Complete event channel documentation

### 📋 **Planned**
- Advanced event filtering and routing
- Event replay system for debugging
- Event analytics and monitoring
- Performance optimization for high-frequency events

---

**Completed**: January 14, 2025  
**Status**: Event system enforcement implemented - Phase 1 PC-007 complete  
**Next**: PC-008 Foundation Validation & Testing