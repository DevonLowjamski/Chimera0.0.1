# Project Chimera - Architecture Overview

## Core Architecture Principles

### 1. ScriptableObject-Driven Design
All game data, configurations, and parameters are defined using ScriptableObjects:
- **Data Layer**: `ProjectChimera.Data` assembly contains all SO definitions
- **Configuration**: Equipment specs, environmental parameters, genetic traits
- **Event System**: SO-based event channels for decoupled communication

### 2. Modular Assembly Structure
```
ProjectChimera.Core          # Foundation systems
├── ProjectChimera.Data      # ScriptableObject definitions
├── ProjectChimera.Genetics  # Genetics and breeding
├── ProjectChimera.Cultivation # Plant lifecycle
├── ProjectChimera.Environment # Environmental simulation
├── ProjectChimera.Construction # Building systems
├── ProjectChimera.Economy   # Economic systems
├── ProjectChimera.Progression # Skills and research
├── ProjectChimera.UI        # User interface
└── ProjectChimera.Testing   # Testing framework
```

### 3. Event-Driven Architecture
- **Global Events**: SO-based event channels for cross-system communication
- **Local Events**: C# Actions and UnityEvents for component-level interaction
- **Decoupled Systems**: Minimal direct dependencies between major systems

### 4. State Management
- **Entity States**: State pattern for complex entity behaviors (plants, equipment)
- **System States**: Manager classes coordinate subsystem states
- **Persistence**: DTO pattern for save/load operations

## Core Systems

### Genetics Engine (`ProjectChimera.Genetics`)
- **Inheritance Simulation**: Polygenic traits, Mendelian inheritance
- **Trait Expression**: Genotype × Environment interactions
- **Breeding Operations**: Advanced techniques (backcrossing, tissue culture)
- **Genetic Library**: Dynamic trait discovery and cataloging

### Environmental Simulation (`ProjectChimera.Environment`)
- **Microclimate Modeling**: Zone-based environmental calculation
- **HVAC Simulation**: Equipment influence on environmental conditions
- **Utility Networks**: Electrical, plumbing, HVAC routing and capacity

### Cultivation System (`ProjectChimera.Cultivation`)
- **Plant Lifecycle**: Growth stages with state pattern implementation
- **GxE Integration**: Environmental influence on genetic expression
- **Health Simulation**: Stress, nutrients, pest/disease modeling

### Construction System (`ProjectChimera.Construction`)
- **Grid-Based Building**: Precise facility construction
- **Utility Routing**: 3D utility network placement and validation
- **Material Properties**: Construction materials affect facility performance

### Data Collection (`ProjectChimera.DataCollection`)
- **Sensor Networks**: Environmental and plant monitoring
- **Analytics Engine**: Data analysis and trend detection
- **Visualization**: Dynamic chart and dashboard generation

## Key Design Patterns

### 1. Manager Pattern
Central coordination for each major system:
- `GeneticsManager`: Coordinates all genetics operations
- `EnvironmentManager`: Manages environmental simulation
- `ConstructionManager`: Handles building operations

### 2. ScriptableObject Configuration
```csharp
[CreateAssetMenu]
public class PlantStrainSO : ChimeraDataSO
{
    public GeneticProfile BaseGenetics;
    public EnvironmentalRequirements OptimalConditions;
    public List<TraitPotential> TraitPotentials;
}
```

### 3. Event Channel Pattern
```csharp
[CreateAssetMenu]
public class PlantStateChangedEventSO : GameEventSO<PlantStateData>
{
    // Event channel for plant state changes
}
```

### 4. State Pattern for Complex Entities
```csharp
public interface IPlantGrowthState
{
    void Enter(PlantInstance plant);
    void Update(PlantInstance plant, float deltaTime);
    void Exit(PlantInstance plant);
}
```

## Performance Considerations

### 1. LOD Systems
- **Plant Models**: 3-4 LOD levels for complex plant geometry
- **Equipment**: Progressive detail reduction based on camera distance
- **UI Elements**: Efficient rendering for complex data visualizations

### 2. Update Optimization
- **Scheduled Updates**: Time-sliced system updates based on priority
- **Dirty Flagging**: Only update systems when data changes
- **Object Pooling**: Reuse of frequently created/destroyed objects

### 3. Data Streaming
- **Addressable Assets**: Dynamic loading of plant models and equipment
- **Incremental Loading**: Large facility data loaded progressively
- **Memory Management**: Careful management of genetics data and historical logs

## Testing Strategy

### 1. Unit Testing
- **Data Validation**: SO reference integrity and circular dependencies
- **System Logic**: Core calculations and state transitions
- **Performance**: Benchmark critical paths and memory usage

### 2. Integration Testing
- **Cross-System**: Genetics → Environment → Plant growth chains
- **UI Integration**: Data binding and visualization accuracy
- **Save/Load**: Data persistence and restoration integrity

### 3. Performance Testing
- **Large Facilities**: 100+ plants with complex genetics
- **Time Scaling**: Simulation accuracy at various time scales
- **Memory Profiling**: Long-term stability and memory leaks

This architecture provides the foundation for Project Chimera's complex simulation while maintaining performance, modularity, and testability.