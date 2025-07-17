# Project Chimera - API Reference

## 📚 Comprehensive API Documentation

This reference provides detailed information about all public APIs, interfaces, and extension points in Project Chimera. This includes the advanced SpeedTree integration, complete cultivation simulation, and all 50+ manager systems that make up the ultimate cannabis cultivation experience.

---

## 🌿 SpeedTree Integration APIs

### AdvancedSpeedTreeManager
**Core SpeedTree cannabis plant simulation system**

```csharp
public class AdvancedSpeedTreeManager : ChimeraManager
{
    public int ActivePlantInstances { get; }
    public bool SpeedTreeEnabled { get; }
    public SpeedTreePerformanceMetrics PerformanceMetrics { get; }
    
    // Plant instance management
    public SpeedTreePlantInstance CreatePlantInstance(CannabisGenotype genotype, Vector3 position, Transform parent = null);
    public void DestroyPlantInstance(int instanceId);
    public SpeedTreePlantInstance GetPlantInstance(int instanceId);
    public List<SpeedTreePlantInstance> GetAllActiveInstances();
    
    // Genetic variation
    public SpeedTreePlantInstance CreateGeneticVariation(string baseStrainId, EnvironmentalConditions conditions);
    public void ApplyGeneticTraits(SpeedTreePlantInstance instance, CannabisGenotype genotype);
    public CannabisGenotype GenerateOffspringGenetics(CannabisGenotype parent1, CannabisGenotype parent2);
    
    // Environmental integration
    public void UpdateEnvironmentalResponse(int instanceId, EnvironmentalConditions conditions);
    public void ProcessStressResponse(int instanceId, EnvironmentalStressData stressData);
    public void TriggerAdaptation(int instanceId, EnvironmentalAdaptation adaptation);
    
    // Performance monitoring
    public SpeedTreeSystemReport GetSystemReport();
    public void OptimizePerformance();
    public void SetQualityLevel(QualityLevel level);
}
```

### CannabisGeneticsEngine
**Scientific cannabis genetics simulation**

```csharp
public class CannabisGeneticsEngine : ChimeraManager
{
    public int TotalGenotypes { get; }
    public int ActiveBreedingPrograms { get; }
    
    // Genetic operations
    public CannabisGenotype CreateBaseGenotype(string strainId, CannabisStrainSO strainData);
    public CannabisGenotype GenerateGeneticVariation(string baseStrainId, EnvironmentalConditions conditions = null);
    public BreedingResult CrossBreed(CannabisGenotype parent1, CannabisGenotype parent2, string offspringName);
    
    // Trait analysis
    public float CalculateTraitExpression(CannabisGenotype genotype, string traitName, EnvironmentalConditions conditions);
    public PhenotypeProfile GeneratePhenotypeProfile(CannabisGenotype genotype, EnvironmentalConditions conditions);
    public GeneticCompatibility AnalyzeBreedingCompatibility(CannabisGenotype parent1, CannabisGenotype parent2);
    
    // Inheritance simulation
    public AlleleInheritance SimulateMendelianInheritance(GeneticLocus locus, CannabisGenotype parent1, CannabisGenotype parent2);
    public float CalculatePolygeneticTrait(CannabisGenotype genotype, List<GeneticLocus> contributingLoci);
    public EpigeneticFactors CalculateEpigeneticInfluence(EnvironmentalConditions conditions, float exposureDuration);
}
```

### SpeedTreeEnvironmentalSystem
**Real-time environmental response and adaptation**

```csharp
public class SpeedTreeEnvironmentalSystem : ChimeraManager
{
    public int MonitoredPlants { get; }
    public EnvironmentalSystemMetrics SystemMetrics { get; }
    
    // Plant monitoring
    public void RegisterPlantForMonitoring(SpeedTreePlantInstance instance);
    public void UnregisterPlant(int instanceId);
    public PlantEnvironmentalState GetPlantState(int instanceId);
    public void UpdatePlantEnvironment(int instanceId, EnvironmentalConditions conditions);
    
    // Stress simulation
    public EnvironmentalStressData CalculateStressLevels(PlantEnvironmentalState plantState, EnvironmentalConditions conditions);
    public void ProcessStressAccumulation(PlantEnvironmentalState plantState, float deltaTime);
    public void TriggerStressRecovery(int instanceId, float recoveryRate);
    
    // Adaptation system
    public EnvironmentalAdaptation ProcessAdaptation(PlantEnvironmentalState plantState, AdaptationPressure pressure);
    public void ApplyAdaptation(int instanceId, EnvironmentalAdaptation adaptation);
    public AdaptationProgress GetAdaptationProgress(int instanceId);
    
    // Environmental zones
    public void CreateEnvironmentalZone(string zoneId, Bounds zoneBounds, EnvironmentalConditions conditions);
    public void UpdateZoneConditions(string zoneId, EnvironmentalConditions newConditions);
    public EnvironmentalZone GetZoneAtPosition(Vector3 position);
}
```

### SpeedTreeGrowthSystem
**Sophisticated growth animation and lifecycle management**

```csharp
public class SpeedTreeGrowthSystem : ChimeraManager
{
    public int ActiveGrowingPlants { get; }
    public GrowthPerformanceMetrics PerformanceMetrics { get; }
    
    // Growth management
    public void RegisterPlantForGrowth(SpeedTreePlantInstance instance);
    public void UnregisterPlantFromGrowth(int instanceId);
    public PlantGrowthState GetPlantGrowthState(int instanceId);
    public void TriggerStageTransition(int instanceId, PlantGrowthStage targetStage);
    
    // Animation system
    public void SetGrowthAnimationEnabled(bool enabled);
    public GrowthAnimationData GetGrowthAnimationData(int instanceId);
    public void UpdatePlantVisualProgression(int instanceId);
    
    // Specialized growth systems
    public BudDevelopmentData GetBudDevelopment(int instanceId);
    public TrichromeData GetTrichromeProgress(int instanceId);
    public LifecycleProgressData GetLifecycleProgress(int instanceId);
    public List<GrowthMilestone> GetAchievedMilestones(int instanceId);
    
    // Growth configuration
    public void SetGrowthTimeMultiplier(float multiplier);
    public void SetAcceleratedGrowth(bool enabled);
    public GrowthSystemReport GetSystemReport();
}
```

### SpeedTreeOptimizationSystem
**Advanced performance optimization for large-scale cultivation**

```csharp
public class SpeedTreeOptimizationSystem : ChimeraManager
{
    public PerformanceMetrics CurrentMetrics { get; }
    public QualityLevel CurrentQualityLevel { get; }
    public int VisiblePlantCount { get; }
    
    // Plant optimization
    public void RegisterPlantForOptimization(SpeedTreePlantInstance instance);
    public void UnregisterPlantFromOptimization(int instanceId);
    public PlantPerformanceData GetPlantPerformanceData(int instanceId);
    public OptimizationState GetOptimizationState(int instanceId);
    
    // Quality management
    public void ChangeQualityLevel(QualityLevel newQuality);
    public void EnableDynamicQuality();
    public void ForceQualityLevel(QualityLevel quality);
    
    // Performance control
    public void SetTargetFrameRate(int frameRate);
    public void SetMaxVisiblePlants(int maxPlants);
    public void SetOptimizationEnabled(bool enabled);
    public OptimizationSystemReport GetSystemReport();
}
```

**Usage Examples**:
```csharp
// Create SpeedTree cannabis plant with genetics
var geneticsEngine = GameManager.Instance.GetManager<CannabisGeneticsEngine>();
var genotype = geneticsEngine.CreateBaseGenotype("white_widow", whiteWidowStrain);
var speedTreeManager = GameManager.Instance.GetManager<AdvancedSpeedTreeManager>();
var plantInstance = speedTreeManager.CreatePlantInstance(genotype, Vector3.zero);

// Monitor environmental response
var environmentalSystem = GameManager.Instance.GetManager<SpeedTreeEnvironmentalSystem>();
environmentalSystem.RegisterPlantForMonitoring(plantInstance);
environmentalSystem.UpdatePlantEnvironment(plantInstance.InstanceId, currentConditions);

// Track growth progression
var growthSystem = GameManager.Instance.GetManager<SpeedTreeGrowthSystem>();
var growthState = growthSystem.GetPlantGrowthState(plantInstance.InstanceId);
var budProgress = growthSystem.GetBudDevelopment(plantInstance.InstanceId);

// Optimize performance for hundreds of plants
var optimizationSystem = GameManager.Instance.GetManager<SpeedTreeOptimizationSystem>();
optimizationSystem.RegisterPlantForOptimization(plantInstance);
optimizationSystem.SetMaxVisiblePlants(500);
```

---

## 🔧 Core Manager APIs

### GameManager
**Central coordination singleton for all game systems**

```csharp
public class GameManager : ChimeraManager
{
    public static GameManager Instance { get; }
    public GameState CurrentGameState { get; }
    public bool IsGamePaused { get; }
    public System.DateTime GameStartTime { get; }
    public System.TimeSpan TotalGameTime { get; }
    
    // Manager access
    public T GetManager<T>() where T : ChimeraManager;
    public void RegisterManager<T>(T manager) where T : ChimeraManager;
    
    // Game control
    public void PauseGame();
    public void ResumeGame();
    public void ManualSave();
}
```

**Usage Examples**:
```csharp
// Access any manager
var plantManager = GameManager.Instance.GetManager<PlantManager>();
var economyManager = GameManager.Instance.GetManager<EconomyManager>();

// Game state management
if (GameManager.Instance.CurrentGameState == GameState.Running)
{
    // Perform game operations
}

// Pause handling
GameManager.Instance.PauseGame();
```

### TimeManager
**Game time control and offline progression**

```csharp
public class TimeManager : ChimeraManager
{
    public float TimeScale { get; set; }
    public System.DateTime GameTime { get; }
    public System.TimeSpan SessionDuration { get; }
    public bool IsOfflineProgressionEnabled { get; set; }
    
    // Time utilities
    public float GetScaledDeltaTime();
    public System.DateTime ConvertToGameTime(System.DateTime realTime);
    public void SetTimeScale(float scale);
    public void ProcessOfflineTime(System.TimeSpan offlineTime);
}
```

**Usage Examples**:
```csharp
// Get scaled time for calculations
float deltaTime = _timeManager.GetScaledDeltaTime();

// Time acceleration
_timeManager.SetTimeScale(2.0f); // 2x speed

// Offline progression
_timeManager.ProcessOfflineTime(System.TimeSpan.FromHours(8));
```

---

## 🎮 Advanced Gameplay System APIs

### AdvancedCameraController
**Comprehensive camera system with multiple view modes**

```csharp
public class AdvancedCameraController : ChimeraManager
{
    public CameraMode CurrentMode { get; }
    public bool IsTransitioning { get; }
    public CameraControlSettings Settings { get; set; }
    
    // View mode management
    public void SetCameraMode(CameraMode mode);
    public void TransitionToMode(CameraMode mode, float duration);
    public void SetTarget(Transform target);
    public void SetPosition(Vector3 position, Quaternion rotation);
    
    // Interactive controls
    public void EnableOrbitControls(bool enabled);
    public void EnableFlyControls(bool enabled);
    public void SetMovementSpeed(float speed);
    public void SetRotationSensitivity(float sensitivity);
    
    // Advanced features
    public void FocusOnObject(GameObject target, float duration = 1f);
    public void SetupOverheadView(Bounds area);
    public void BeginVirtualTour(List<Transform> waypoints);
    public CameraState SaveCameraState();
    public void RestoreCameraState(CameraState state);
}
```

### InteractivePlantComponent
**Advanced plant interaction and cultivation mechanics**

```csharp
public class InteractivePlantComponent : MonoBehaviour
{
    public string PlantId { get; }
    public PlantGrowthStage CurrentStage { get; }
    public float Health { get; }
    public bool IsInteractable { get; }
    
    // Interaction system
    public void OnPlayerInteract();
    public void StartHarvest();
    public void ApplyTreatment(TreatmentType treatment);
    public void TakeClone();
    public void InspectPlant();
    
    // Visual feedback
    public void HighlightPlant(Color highlightColor);
    public void ShowInfoPanel();
    public void DisplayHealthStatus();
    public void ShowGrowthProgress();
    
    // Events
    public event System.Action<InteractivePlantComponent> OnPlantInteracted;
    public event System.Action<InteractivePlantComponent> OnHarvestCompleted;
    public event System.Action<InteractivePlantComponent, PlantGrowthStage> OnStageChanged;
}
```

### ProceduralSceneGenerator
**Dynamic scene generation and environment creation**

```csharp
public class ProceduralSceneGenerator : ChimeraManager
{
    public SceneGenerationSettings Settings { get; set; }
    public bool IsGenerating { get; }
    
    // Scene generation
    public void GenerateGrowRoom(GrowRoomConfiguration config);
    public void GenerateOutdoorFarm(OutdoorFarmConfiguration config);
    public void GenerateProcessingFacility(ProcessingConfiguration config);
    public void GenerateRetailSpace(RetailConfiguration config);
    
    // Dynamic loading
    public void LoadSceneAsync(string sceneName, System.Action onComplete);
    public void UnloadScene(string sceneName);
    public void TransitionToScene(string sceneName, TransitionType transition);
    
    // Environment management
    public void SetTimeOfDay(float time);
    public void SetWeatherConditions(WeatherType weather);
    public void UpdateLighting(LightingPreset preset);
}
```

### InteractiveFacilityConstructor
**Modular building system with economic integration**

```csharp
public class InteractiveFacilityConstructor : ChimeraManager
{
    public bool IsInBuildMode { get; }
    public ConstructionProject ActiveProject { get; }
    public float AvailableBudget { get; }
    
    // Construction operations
    public void StartConstruction(ConstructionTemplate template);
    public void PlaceModule(ModuleDefinition module, Vector3 position, Quaternion rotation);
    public void RemoveModule(ModuleInstance module);
    public void CompleteConstruction();
    
    // Economic integration
    public void PurchaseModule(ModuleDefinition module);
    public bool CanAffordModule(ModuleDefinition module);
    public ConstructionCostBreakdown CalculateCosts(ConstructionTemplate template);
    public void ProcessPayment(float amount);
    
    // Project management
    public List<ConstructionProject> GetActiveProjects();
    public void SaveProject(string projectName);
    public void LoadProject(string projectName);
    
    // Events
    public event System.Action<ConstructionProject> OnProjectCompleted;
    public event System.Action<ConstructionProject, string> OnMilestoneReached;
}
```

### AdvancedEffectsManager
**Comprehensive particle effects and visual feedback**

```csharp
public class AdvancedEffectsManager : ChimeraManager
{
    public bool EffectsEnabled { get; set; }
    public EffectQuality QualityLevel { get; set; }
    
    // Effect playback
    public void PlayEffect(EffectType type, Vector3 position, Transform parent = null, float duration = 0f);
    public void PlayEffectAtTarget(EffectType type, Transform target, float duration = 0f);
    public void StopEffect(int effectId);
    public void StopAllEffects();
    
    // Audio effects
    public void PlayAudioEffect(AudioClip clip, Vector3 position, float volume = 1f);
    public void PlayAmbientSound(AudioClip ambient, float volume = 1f, bool loop = true);
    public void StopAmbientSound();
    
    // Visual feedback
    public void TriggerScreenFlash(Color color, float intensity, float duration);
    public void TriggerScreenShake(float intensity, float duration);
    public void ShowParticleEffect(ParticleEffectType type, Vector3 position, float duration);
    
    // Effect management
    public void PreloadEffect(EffectType type);
    public void SetEffectPoolSize(EffectType type, int poolSize);
    public EffectPerformanceMetrics GetPerformanceMetrics();
}
```

### ComprehensiveProgressionManager
**Complete progression system with skills, research, and achievements**

```csharp
public class ComprehensiveProgressionManager : ChimeraManager
{
    public PlayerProgressionData PlayerProgression { get; }
    public ProgressionMetrics Metrics { get; }
    public bool ProgressionEnabled { get; }
    
    // Experience and skills
    public void GainExperience(string skillId, float amount, string source = "");
    public int GetSkillLevel(string skillId);
    public float GetSkillProgress(string skillId);
    public void SetProgressionMultiplier(float multiplier);
    
    // Research system
    public void StartResearch(string researchId);
    public List<string> GetAvailableResearch();
    public ResearchProgress GetResearchProgress(string researchId);
    
    // Achievement system
    public List<AchievementSO> GetAvailableAchievements();
    public bool IsContentUnlocked(string contentId);
    public void UnlockContent(string contentId, string source = "");
    
    // Progression reporting
    public ProgressionSystemReport GetSystemReport();
    public void SetExperienceMultiplier(float multiplier);
    
    // Events
    public event System.Action<string, int> OnSkillLevelUp;
    public event System.Action<string> OnResearchCompleted;
    public event System.Action<string> OnAchievementUnlocked;
    public event System.Action<string> OnContentUnlocked;
}
```

**Usage Examples**:
```csharp
// Advanced camera control
var cameraController = GameManager.Instance.GetManager<AdvancedCameraController>();
cameraController.SetCameraMode(CameraMode.Orbit);
cameraController.FocusOnObject(selectedPlant.gameObject, 2f);

// Interactive plant cultivation
var plant = GetComponent<InteractivePlantComponent>();
plant.OnPlantInteracted += (p) => Debug.Log($"Interacted with {p.PlantId}");
plant.StartHarvest();

// Facility construction
var constructor = GameManager.Instance.GetManager<InteractiveFacilityConstructor>();
var growRoomTemplate = Resources.Load<ConstructionTemplate>("Templates/BasicGrowRoom");
constructor.StartConstruction(growRoomTemplate);

// Progression tracking
var progressionManager = GameManager.Instance.GetManager<ComprehensiveProgressionManager>();
progressionManager.GainExperience("cultivation", 100f, "Plant Harvested");
progressionManager.StartResearch("advanced_genetics");
```

---

## 🌱 Cultivation System APIs

### PlantManager
**Core plant lifecycle management**

```csharp
public class PlantManager : ChimeraManager
{
    public int ActivePlantCount { get; }
    public float GlobalGrowthModifier { get; set; }
    
    // Plant creation and management
    public PlantInstance CreatePlant(PlantStrainSO strain, Vector3 position, Transform parent = null);
    public List<PlantInstance> CreatePlants(PlantStrainSO strain, List<Vector3> positions, Transform parent = null);
    public void RegisterPlant(PlantInstance plant);
    public void UnregisterPlant(string plantID, PlantRemovalReason reason = PlantRemovalReason.Other);
    
    // Plant access
    public PlantInstance GetPlant(string plantID);
    public List<PlantInstance> GetPlantsInStage(PlantGrowthStage stage);
    public List<PlantInstance> GetHarvestablePlants();
    public List<PlantInstance> GetPlantsNeedingAttention();
    
    // Operations
    public void UpdateEnvironmentalConditions(EnvironmentalConditions newConditions);
    public void ApplyEnvironmentalStress(EnvironmentalStressSO stressSource, float intensity);
    public HarvestResults HarvestPlant(string plantID);
    public PlantManagerStatistics GetStatistics();
}
```

### PlantInstance
**Individual plant state and behavior**

```csharp
public class PlantInstance : MonoBehaviour
{
    public string PlantID { get; }
    public string PlantName { get; set; }
    public PlantStrainSO Strain { get; }
    public PlantGrowthStage CurrentGrowthStage { get; }
    public float CurrentHealth { get; }
    public float StressLevel { get; }
    public float GrowthProgress { get; }
    public bool IsActive { get; }
    
    // Events
    public event System.Action<PlantInstance> OnGrowthStageChanged;
    public event System.Action<PlantInstance> OnHealthChanged;
    public event System.Action<PlantInstance> OnPlantDied;
    
    // Operations
    public void UpdateEnvironmentalConditions(EnvironmentalConditions conditions);
    public bool ApplyStress(EnvironmentalStressSO stressSource, float intensity);
    public HarvestResults Harvest();
    public bool HasActiveStressors();
    
    // Static factory
    public static PlantInstance CreateFromStrain(PlantStrainSO strain, Vector3 position, Transform parent = null);
}
```

**Usage Examples**:
```csharp
// Create and manage plants
var strain = Resources.Load<PlantStrainSO>("Strains/WhiteWidow");
var plant = _plantManager.CreatePlant(strain, Vector3.zero);

// Monitor plant health
plant.OnHealthChanged += (p) => Debug.Log($"Plant {p.PlantName} health: {p.CurrentHealth}");

// Harvest when ready
if (plant.CurrentGrowthStage == PlantGrowthStage.Harvest)
{
    var results = _plantManager.HarvestPlant(plant.PlantID);
    Debug.Log($"Harvested {results.TotalYieldGrams}g with quality {results.QualityScore}");
}
```

---

## 🧬 Genetics System APIs

### GeneticsManager
**Breeding mechanics and trait inheritance**

```csharp
public class GeneticsManager : ChimeraManager
{
    // Breeding operations
    public BreedingResults BreedPlants(PlantInstance parent1, PlantInstance parent2);
    public PlantStrainSO CreateOffspringStrain(PlantStrainSO parent1, PlantStrainSO parent2, string offspringName);
    public GeneticPrediction PredictOffspring(PlantStrainSO parent1, PlantStrainSO parent2);
    
    // Trait analysis
    public List<GeneticTrait> AnalyzeGenotype(PlantStrainSO strain);
    public float CalculateTraitExpression(GeneticTrait trait, EnvironmentalConditions conditions);
    public Dictionary<string, float> GetPhenotypeProfile(PlantInstance plant);
    
    // Strain management
    public void RegisterCustomStrain(PlantStrainSO strain);
    public List<PlantStrainSO> GetAvailableStrains();
    public PlantStrainSO GetStrainByName(string strainName);
}
```

### PlantStrainSO
**Genetic data structure**

```csharp
[CreateAssetMenu(fileName = "New Plant Strain", menuName = "Project Chimera/Genetics/Plant Strain")]
public class PlantStrainSO : ChimeraDataSO
{
    [Header("Basic Information")]
    public string StrainName;
    public string Description;
    public StrainType Type; // Indica, Sativa, Hybrid
    public Sprite StrainImage;
    
    [Header("Genetic Traits")]
    public GeneticTraitRange ThcContent;
    public GeneticTraitRange CbdContent;
    public GeneticTraitRange YieldPotential;
    public GeneticTraitRange FloweringTime;
    public TerpeneProfile TerpeneProfile;
    
    [Header("Environmental Preferences")]
    public EnvironmentalTolerances EnvironmentalTolerance;
    public DiseaseResistance DiseaseResistance;
    public NutrientRequirements NutrientNeeds;
    
    // Validation and utilities
    public override bool ValidateDataSpecific();
    public float GetTraitValue(string traitName);
    public bool IsCompatibleWith(PlantStrainSO otherStrain);
}
```

**Usage Examples**:
```csharp
// Breed two plants
var parent1 = _plantManager.GetPlant("plant_001");
var parent2 = _plantManager.GetPlant("plant_002");
var breedingResults = _geneticsManager.BreedPlants(parent1, parent2);

// Create new strain from breeding
var offspringStrain = _geneticsManager.CreateOffspringStrain(
    parent1.Strain, 
    parent2.Strain, 
    "Custom Hybrid #1"
);

// Analyze genetics
var traits = _geneticsManager.AnalyzeGenotype(strain);
foreach (var trait in traits)
{
    Debug.Log($"Trait: {trait.Name}, Value: {trait.Expression}");
}
```

---

## 🏭 Environmental System APIs

### EnvironmentalManager
**Climate control and monitoring**

```csharp
public class EnvironmentalManager : ChimeraManager
{
    public EnvironmentalConditions CurrentConditions { get; }
    public bool AutomationEnabled { get; set; }
    public float EnergyEfficiency { get; }
    
    // Environmental control
    public void SetTargetConditions(EnvironmentalConditions targets);
    public void UpdateConditions(EnvironmentalConditions newConditions);
    public void ApplyEnvironmentalStress(EnvironmentalStressSO stress, float intensity);
    
    // Equipment management
    public void RegisterEquipment(EnvironmentalEquipmentSO equipment);
    public void UnregisterEquipment(string equipmentId);
    public List<EnvironmentalEquipmentSO> GetActiveEquipment();
    
    // Monitoring
    public EnvironmentalHistory GetHistoricalData(System.TimeSpan timeRange);
    public EnvironmentalForecast GetForecast(System.TimeSpan duration);
    public List<EnvironmentalAlert> GetActiveAlerts();
}
```

### EnvironmentalConditions
**Environmental state data structure**

```csharp
[System.Serializable]
public class EnvironmentalConditions
{
    [Header("Climate Parameters")]
    [Range(10f, 40f)] public float Temperature = 24f;
    [Range(20f, 80f)] public float Humidity = 60f;
    [Range(300f, 1500f)] public float CO2Level = 400f;
    
    [Header("Lighting")]
    [Range(0f, 2000f)] public float PPFD = 800f;
    [Range(10f, 18f)] public float DailyLightIntegral = 14f;
    public LightSpectrum LightSpectrum;
    
    [Header("Air Quality")]
    [Range(0f, 10f)] public float AirFlow = 5f;
    [Range(0f, 100f)] public float AirPurity = 95f;
    
    // Utilities
    public bool IsWithinOptimalRange(PlantStrainSO strain);
    public float CalculateStressLevel(PlantStrainSO strain);
    public EnvironmentalConditions Lerp(EnvironmentalConditions target, float t);
}
```

**Usage Examples**:
```csharp
// Set environmental targets
var targetConditions = new EnvironmentalConditions
{
    Temperature = 26f,
    Humidity = 65f,
    CO2Level = 800f,
    PPFD = 900f
};
_environmentalManager.SetTargetConditions(targetConditions);

// Monitor conditions
var currentConditions = _environmentalManager.CurrentConditions;
if (!currentConditions.IsWithinOptimalRange(plantStrain))
{
    Debug.LogWarning("Environmental conditions suboptimal for strain");
}
```

---

## 💰 Economic System APIs

### MarketManager
**Market simulation and trading**

```csharp
public class MarketManager : ChimeraManager
{
    public MarketConditions CurrentMarketConditions { get; }
    public PlayerPortfolio PlayerPortfolio { get; }
    public float PlayerReputation { get; }
    
    // Trading operations
    public TransactionResult ProcessSale(ProductSO product, float quantity, float quality, bool isContractSale);
    public TransactionResult ProcessPurchase(ProductSO product, float quantity, float maxPrice);
    public List<MarketOpportunity> GetAvailableOpportunities();
    
    // Market analysis
    public MarketData GetMarketData(ProductSO product);
    public PriceHistory GetPriceHistory(ProductSO product, System.TimeSpan timeRange);
    public MarketForecast GetMarketForecast(ProductSO product);
    
    // Portfolio management
    public PortfolioMetrics GetPortfolioMetrics();
    public FinancialData GetFinancialData();
    public void UpdatePlayerReputation(float change, string reason);
}
```

### EconomyManager
**Financial management and investments**

```csharp
public class EconomyManager : ChimeraManager
{
    public float PlayerCash { get; }
    public float NetWorth { get; }
    public float MonthlyRevenue { get; }
    public float MonthlyExpenses { get; }
    
    // Financial operations
    public bool ProcessTransaction(float amount, string description, TransactionType type);
    public void AddRecurringExpense(string name, float amount, RecurrenceType frequency);
    public void RemoveRecurringExpense(string name);
    
    // Investment management
    public List<InvestmentOpportunity> GetAvailableInvestments();
    public InvestmentResult MakeInvestment(InvestmentOpportunity opportunity, float amount);
    public List<ActiveInvestment> GetActiveInvestments();
    
    // Financial reporting
    public FinancialReport GenerateFinancialReport(System.TimeSpan period);
    public CashFlowProjection ProjectCashFlow(System.TimeSpan duration);
    public List<FinancialAlert> GetFinancialAlerts();
}
```

**Usage Examples**:
```csharp
// Sell harvested product
var harvestResults = _plantManager.HarvestPlant(plantId);
var product = Resources.Load<ProductSO>("Products/DriedFlower");
var saleResult = _marketManager.ProcessSale(product, harvestResults.TotalYieldGrams, harvestResults.QualityScore, false);

// Check financial status
var financialData = _economyManager.GetFinancialData();
Debug.Log($"Cash: ${financialData.CurrentCash:F2}, Net Worth: ${financialData.NetWorth:F2}");

// Make investment
var investments = _economyManager.GetAvailableInvestments();
var bestInvestment = investments.OrderByDescending(i => i.ExpectedROI).First();
_economyManager.MakeInvestment(bestInvestment, 10000f);
```

---

## 🤖 Automation & AI APIs

### AutomationManager
**IoT integration and intelligent control**

```csharp
public class AutomationManager : ChimeraManager
{
    public bool AutomationEnabled { get; set; }
    public int ConnectedDevices { get; }
    public float SystemEfficiency { get; }
    
    // Device management
    public void RegisterDevice(IoTDeviceSO device);
    public void UnregisterDevice(string deviceId);
    public List<IoTDeviceSO> GetConnectedDevices();
    public IoTDeviceSO GetDevice(string deviceId);
    
    // Automation schedules
    public void CreateSchedule(AutomationScheduleSO schedule);
    public void UpdateSchedule(string scheduleId, AutomationScheduleSO newSchedule);
    public void DeleteSchedule(string scheduleId);
    public List<AutomationScheduleSO> GetActiveSchedules();
    
    // Control operations
    public void ExecuteAutomationRule(AutomationRuleSO rule);
    public void TriggerEmergencyShutdown(string reason);
    public AutomationStatus GetSystemStatus();
}
```

### AIAdvisorManager
**Intelligent recommendations and optimization**

```csharp
public class AIAdvisorManager : ChimeraManager
{
    public bool AIEnabled { get; set; }
    public float ConfidenceLevel { get; }
    public int RecommendationsGenerated { get; }
    
    // AI analysis
    public void ProcessUserQuery(string query, System.Action<string> responseCallback);
    public FacilityAnalysis AnalyzeFacilityState();
    public List<AIRecommendation> GenerateRecommendations();
    public PredictionResults GeneratePredictions();
    
    // Optimization
    public OptimizationSuggestions AnalyzeOptimizationOpportunities();
    public void ApplyOptimizationSuggestion(OptimizationSuggestions suggestion);
    public PerformanceMetrics GetAIPerformanceMetrics();
    
    // Learning system
    public void ProvideFeedback(string recommendationId, FeedbackType feedback);
    public void UpdateLearningModel(UserActionData actionData);
    public AILearningProgress GetLearningProgress();
}
```

**Usage Examples**:
```csharp
// Set up automation
var schedule = ScriptableObject.CreateInstance<AutomationScheduleSO>();
schedule.DeviceId = "hvac_001";
schedule.TriggerConditions.Add(new TemperatureTrigger { Threshold = 28f });
schedule.Actions.Add(new CoolingAction { Intensity = 0.8f });
_automationManager.CreateSchedule(schedule);

// Get AI recommendations
var recommendations = _aiAdvisorManager.GenerateRecommendations();
foreach (var rec in recommendations)
{
    Debug.Log($"AI Recommendation: {rec.Title} - {rec.Description}");
}

// Process natural language query
_aiAdvisorManager.ProcessUserQuery("What's the optimal temperature for flowering?", response =>
{
    Debug.Log($"AI Response: {response}");
});
```

---

## 📊 Analytics & Data APIs

### AnalyticsManager
**Data collection and business intelligence**

```csharp
public class AnalyticsManager : ChimeraManager
{
    public bool AnalyticsEnabled { get; set; }
    public bool PrivacyMode { get; set; }
    public long EventsTracked { get; }
    
    // Event tracking
    public void TrackEvent(string eventName, Dictionary<string, object> parameters = null);
    public void TrackCustomEvent(AnalyticsEvent customEvent);
    public void TrackPerformanceMetric(string metricName, float value);
    
    // Data analysis
    public AnalyticsReport GenerateReport(ReportType reportType, System.TimeSpan timeRange);
    public List<AnalyticsInsight> GetBusinessInsights();
    public PlayerBehaviorAnalysis AnalyzePlayerBehavior();
    
    // Experiments
    public void StartABTest(ABTestConfiguration testConfig);
    public void EndABTest(string testId);
    public ABTestResults GetABTestResults(string testId);
    
    // Data export
    public void ExportData(DataExportFormat format, string filePath);
    public AnalyticsDashboardData GetDashboardData();
}
```

### DataManager
**ScriptableObject asset management**

```csharp
public class DataManager : ChimeraManager
{
    // Asset loading
    public T LoadAsset<T>(string assetPath) where T : ChimeraScriptableObject;
    public List<T> LoadAssetsByType<T>() where T : ChimeraScriptableObject;
    public void LoadAssetsAsync<T>(System.Action<List<T>> onComplete) where T : ChimeraScriptableObject;
    
    // Asset validation
    public bool ValidateAsset<T>(T asset) where T : ChimeraScriptableObject;
    public List<DataValidationError> ValidateAllAssets();
    public void RepairAsset<T>(T asset) where T : ChimeraScriptableObject;
    
    // Asset registry
    public void RegisterAsset<T>(T asset) where T : ChimeraScriptableObject;
    public void UnregisterAsset<T>(T asset) where T : ChimeraScriptableObject;
    public Dictionary<System.Type, List<ChimeraScriptableObject>> GetAssetRegistry();
}
```

**Usage Examples**:
```csharp
// Track player actions
_analyticsManager.TrackEvent("PlantHarvested", new Dictionary<string, object>
{
    {"strain", plant.Strain.StrainName},
    {"yield", harvestResults.TotalYieldGrams},
    {"quality", harvestResults.QualityScore},
    {"timeToHarvest", plant.GrowthDuration.TotalDays}
});

// Generate business report
var report = _analyticsManager.GenerateReport(ReportType.WeeklyPerformance, System.TimeSpan.FromDays(7));
Debug.Log($"Weekly Revenue: ${report.TotalRevenue:F2}");

// Load game data
var strains = _dataManager.LoadAssetsByType<PlantStrainSO>();
Debug.Log($"Loaded {strains.Count} plant strains");
```

---

## 🎯 Progression & UI APIs

### ProgressionManager
**Player advancement and skill development**

```csharp
public class ProgressionManager : ChimeraManager
{
    public int PlayerLevel { get; }
    public float CurrentExperience { get; }
    public float ExperienceToNextLevel { get; }
    
    // Experience and leveling
    public void AwardExperience(float amount, string source);
    public void AwardSkillExperience(SkillType skill, float amount);
    public bool CanLevelUp();
    public void LevelUp();
    
    // Skills and unlocks
    public Dictionary<SkillType, float> GetSkillLevels();
    public bool IsUnlocked(string unlockId);
    public void UnlockFeature(string featureId);
    public List<AvailableUnlock> GetAvailableUnlocks();
    
    // Achievements
    public void TriggerAchievement(string achievementId);
    public List<Achievement> GetUnlockedAchievements();
    public float GetAchievementProgress(string achievementId);
}
```

### UI System APIs
**Interface management and customization**

```csharp
// GameUIManager - Main interface controller
public class GameUIManager : ChimeraManager
{
    public bool UIVisible { get; set; }
    public UITheme CurrentTheme { get; set; }
    
    // Panel management
    public void ShowPanel(string panelName);
    public void HidePanel(string panelName);
    public void TogglePanel(string panelName);
    public bool IsPanelVisible(string panelName);
    
    // Notifications
    public void ShowNotification(string message, NotificationType type);
    public void ShowTooltip(string text, Vector2 position);
    public void HideTooltip();
    
    // Themes and customization
    public void ApplyTheme(UITheme theme);
    public List<UITheme> GetAvailableThemes();
    public void CustomizeInterface(UICustomization customization);
}
```

**Usage Examples**:
```csharp
// Award experience for achievements
_progressionManager.AwardExperience(500f, "First Harvest");
_progressionManager.AwardSkillExperience(SkillType.Cultivation, 100f);

// Check unlocks
if (_progressionManager.IsUnlocked("AdvancedGenetics"))
{
    // Enable advanced breeding features
}

// UI management
_gameUIManager.ShowPanel("PlantManagement");
_gameUIManager.ShowNotification("Harvest completed successfully!", NotificationType.Success);
```

---

## 🧪 Testing APIs

### TestingManager
**Automated testing and validation**

```csharp
public class TestingManager : ChimeraManager
{
    public bool TestingEnabled { get; set; }
    public TestingConfiguration Configuration { get; set; }
    
    // Test execution
    public string StartTestSession(string sessionName = "Default Session", TestSessionConfiguration config = null);
    public void StopTestSession(string sessionId);
    public TestSessionResults GetTestResults(string sessionId);
    
    // Test management
    public void RegisterTestSuite(TestSuite testSuite);
    public void UnregisterTestSuite(string suiteId);
    public List<TestSuite> GetAvailableTestSuites();
    
    // Validation
    public SystemValidationResults ValidateAllSystems();
    public bool ValidateSystemIntegration();
    public PerformanceTestResults RunPerformanceTests();
}
```

---

## 🔌 Extension and Modding APIs

### Modding Interfaces
**Plugin system for community content**

```csharp
// Core modding interface
public interface IChimeraMod
{
    string ModName { get; }
    string ModVersion { get; }
    string ModAuthor { get; }
    
    void Initialize();
    void Shutdown();
    bool IsCompatible(string gameVersion);
}

// Custom strain provider
public interface ICustomStrainProvider : IChimeraMod
{
    IEnumerable<PlantStrainSO> GetCustomStrains();
    bool ValidateStrain(PlantStrainSO strain);
}

// Custom equipment provider
public interface ICustomEquipmentProvider : IChimeraMod
{
    IEnumerable<EquipmentDataSO> GetCustomEquipment();
    void ProcessEquipmentEffect(EquipmentDataSO equipment, PlantInstance plant);
}
```

**Usage Examples**:
```csharp
// Register custom mod
public class CustomGeneticsProvider : MonoBehaviour, ICustomStrainProvider
{
    public string ModName => "Advanced Genetics Pack";
    public string ModVersion => "1.0.0";
    public string ModAuthor => "ModAuthor";
    
    public void Initialize()
    {
        var modManager = GameManager.Instance.GetManager<ModManager>();
        modManager.RegisterMod(this);
    }
    
    public IEnumerable<PlantStrainSO> GetCustomStrains()
    {
        // Return custom strain definitions
        return _customStrains;
    }
    
    public bool ValidateStrain(PlantStrainSO strain)
    {
        // Validate custom strain data
        return strain != null && !string.IsNullOrEmpty(strain.StrainName);
    }
}
```

---

## 🔍 Utility APIs

### Common Utilities
**Helper functions and data structures**

```csharp
// Event system utilities
public static class ChimeraEvents
{
    public static void Subscribe<T>(GameEventSO<T> eventChannel, System.Action<T> callback);
    public static void Unsubscribe<T>(GameEventSO<T> eventChannel, System.Action<T> callback);
    public static void Raise<T>(GameEventSO<T> eventChannel, T data);
}

// Math utilities
public static class ChimeraMath
{
    public static float CalculateGrowthRate(float baseRate, EnvironmentalConditions conditions);
    public static float ApplyGeneticModifier(float baseValue, float geneticFactor);
    public static Vector2 CalculateOptimalRange(float centerValue, float tolerance);
}

// Validation utilities
public static class ChimeraValidation
{
    public static bool ValidateRange(float value, float min, float max);
    public static bool ValidatePercentage(float value);
    public static bool ValidateAssetReference<T>(T asset) where T : ChimeraScriptableObject;
}
```

---

## 📊 SpeedTree Data Structures

### Core SpeedTree Types
**Cannabis-specific data structures for SpeedTree integration**

```csharp
// Cannabis Genotype
[System.Serializable]
public class CannabisGenotype
{
    public string GenotypeId;
    public string StrainId;
    public string StrainName;
    public int Generation;
    public DateTime CreationDate;
    public List<string> ParentGenotypes;
    public Dictionary<string, AlleleData> Alleles;
    
    // Cannabis-specific traits
    public float PlantSize;
    public float GrowthRate;
    public float FloweringSpeed;
    public float YieldPotential;
    public float TrichromeAmount;
    public float BudDensity;
    public Color BudColor;
    public TerpeneProfile TerpeneProfile;
    public DiseaseResistanceProfile ResistanceProfile;
}

// SpeedTree Plant Instance
public class SpeedTreePlantInstance
{
    public int InstanceId;
    public string PlantName;
    public Vector3 Position;
    public Vector3 Scale;
    public PlantGrowthStage GrowthStage;
    public float Health;
    public float Age;
    public CannabisGenotype GeneticData;
    public PlantStrainSO StrainAsset;
    public Renderer Renderer;
    public Dictionary<string, float> EnvironmentalModifiers;
    
    // Growth progression
    public float GrowthProgress;
    public float StageProgress;
    public DateTime PlantingDate;
    public DateTime LastUpdate;
}

// Environmental Conditions
[System.Serializable]
public class EnvironmentalConditions
{
    public float Temperature;
    public float Humidity;
    public float LightIntensity;
    public float CO2Level;
    public float AirVelocity;
    public float SoilMoisture;
    public float SoilPH;
    public Vector3 LightDirection;
    public Color LightColor;
    public float UVIndex;
    public float VPD; // Vapor Pressure Deficit
    public float DailyLightIntegral;
    public float Timestamp;
}

// Growth Animation Data
[System.Serializable]
public class GrowthAnimationData
{
    public int InstanceId;
    public float AnimationStartTime;
    public GrowthAnimationPhase CurrentAnimationPhase;
    public float AnimationSpeed;
    public Vector3 TargetSize;
    public float CurrentSize;
    public Vector3 TargetRotation;
    public Dictionary<string, float> AnimationParameters;
    public bool IsAnimating;
    public List<AnimationKeyframe> Keyframes;
}
```

### Performance and Optimization Types
**Advanced optimization data structures**

```csharp
// Plant Performance Data
[System.Serializable]
public class PlantPerformanceData
{
    public int InstanceId;
    public SpeedTreePlantInstance PlantInstance;
    public float LastVisibleTime;
    public float RenderTime;
    public float DistanceToCamera;
    public int CurrentLODLevel;
    public bool IsCulled;
    public bool IsBatched;
    public float MemoryFootprint;
    public int OptimizationPriority;
}

// Performance Metrics
[System.Serializable]
public class PerformanceMetrics
{
    public float FrameTime;
    public float AverageFrameRate;
    public int VisiblePlants;
    public int DrawCalls;
    public int Batches;
    public float MemoryUsage;
    public int CulledPlants;
    public int LODTransitions;
    public DateTime LastUpdate;
}

// Quality Settings
[System.Serializable]
public class QualitySettings
{
    public int MaxVisiblePlants;
    public float LODDistanceMultiplier;
    public float CullingDistance;
    public ShadowQuality ShadowQuality;
    public int TextureQuality;
    public AnisotropicFiltering AnisotropicFiltering;
    public int AntiAliasing;
    public int VSyncCount;
    public int TargetFrameRate;
}
```

---

## 🎯 Configuration ScriptableObjects

### SpeedTree Configuration
**ScriptableObject-based configuration system for SpeedTree**

```csharp
[CreateAssetMenu(fileName = "SpeedTree Config", menuName = "Project Chimera/SpeedTree/Core Config")]
public class SpeedTreeConfigSO : ScriptableObject
{
    [Header("Performance Settings")]
    public int MaxSimultaneousPlants = 500;
    public float DefaultLODDistance = 50f;
    public bool EnableAutoOptimization = true;
    public QualityLevel DefaultQuality = QualityLevel.High;
    
    [Header("Genetics Settings")]
    public bool EnableGeneticVariation = true;
    public float MutationRate = 0.01f;
    public int MaxGenerations = 10;
    
    [Header("Environmental Settings")]
    public bool EnableEnvironmentalResponse = true;
    public float StressThreshold = 0.7f;
    public float AdaptationRate = 0.1f;
    
    [Header("Growth Settings")]
    public bool EnableRealTimeGrowth = true;
    public float GrowthSpeedMultiplier = 1f;
    public bool EnableStageProgression = true;
}

[CreateAssetMenu(fileName = "Cannabis Strain", menuName = "Project Chimera/SpeedTree/Cannabis Strain")]
public class CannabisStrainSO : ScriptableObject
{
    [Header("Basic Information")]
    public string StrainName;
    public string Description;
    public StrainType Type; // Indica, Sativa, Hybrid
    public Sprite StrainImage;
    
    [Header("Growth Characteristics")]
    public Vector2 HeightRange = new Vector2(1.5f, 3f);
    public Vector2 WidthRange = new Vector2(1f, 2f);
    public Vector2 FloweringTimeRange = new Vector2(8f, 12f); // weeks
    public float TypicalYield = 500f; // grams
    
    [Header("Genetic Traits")]
    public Vector2 THCRange = new Vector2(15f, 25f);
    public Vector2 CBDRange = new Vector2(0.1f, 2f);
    public TerpeneProfile TerpeneProfile;
    public Color BudColor = Color.green;
    
    [Header("Environmental Preferences")]
    public Vector2 OptimalTemperature = new Vector2(20f, 28f);
    public Vector2 OptimalHumidity = new Vector2(50f, 70f);
    public Vector2 OptimalLighting = new Vector2(600f, 1000f);
    public DiseaseResistance DiseaseResistance;
}
```

This comprehensive API reference provides all the information needed to develop with, extend, or integrate Project Chimera's advanced systems including the sophisticated SpeedTree integration, complete cultivation simulation, and performance optimization. For implementation examples and best practices, refer to the Developer Documentation.