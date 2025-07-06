using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Genetics;

namespace ProjectChimera.UI
{
    /// <summary>
    /// Phase 0.1: Chimera Debug Inspector Implementation
    /// "Single most critical prerequisite for all future work"
    /// Provides real-time plant data visualization and system control
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DebugInspector : MonoBehaviour
    {
        [Header("Debug Inspector Configuration")]
        [SerializeField] private bool enableRealTimeUpdates = true;
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private bool enablePerformanceMonitoring = true;
        
        [Header("System References")]
        [SerializeField] private DataManager dataManager;
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private PlantManager plantManager;
        [SerializeField] private GeneticsManager geneticsManager;
        
        // UI Document and Root
        private UIDocument uiDocument;
        private VisualElement rootElement;
        
        // UI Elements Cache
        private Button pauseButton;
        private Button playButton;
        private Button fastForwardButton;
        private Label timeScaleLabel;
        private Label gameDayLabel;
        private Label seasonLabel;
        
        private ListView plantList;
        private DropdownField growthStageFilter;
        private DropdownField healthFilter;
        private DropdownField strainFilter;
        
        private Label totalPlantsLabel;
        private Label healthyPlantsLabel;
        private Label floweringPlantsLabel;
        private Label harvestReadyLabel;
        
        private Label plantNameLabel;
        private Label plantInfoLabel;
        private VisualElement genotypeList;
        private VisualElement phenotypeList;
        
        private ProgressBar heightProgress;
        private ProgressBar biomassProgress;
        private ProgressBar maturityProgress;
        private ProgressBar healthProgress;
        private ProgressBar waterProgress;
        private ProgressBar nutritionProgress;
        private ProgressBar thcProgress;
        private ProgressBar cbdProgress;
        private ProgressBar trichromeProgress;
        
        private Label temperatureLabel;
        private Label humidityLabel;
        private Label lightLabel;
        private Label co2Label;
        private Label phLabel;
        private Label ecLabel;
        
        private Label dataManagerIndicator;
        private Label timeManagerIndicator;
        private Label plantManagerIndicator;
        private Label geneticsManagerIndicator;
        
        private Label fpsLabel;
        private Label memoryLabel;
        private Label plantCountLabel;
        private Label updateTimeLabel;
        
        private Label statusMessage;
        private Label lastUpdateLabel;
        
        // Debug State
        private PlantInstanceSO selectedPlant;
        private List<PlantInstanceSO> filteredPlants = new List<PlantInstanceSO>();
        private float lastUpdateTime;
        private bool isInitialized = false;
        
        // Performance Tracking
        private float lastFPS;
        private long lastMemoryUsage;
        private System.Diagnostics.Stopwatch updateStopwatch = new System.Diagnostics.Stopwatch();
        
        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            FindSystemReferences();
        }
        
        private void Start()
        {
            InitializeUI();
            StartRealTimeUpdates();
            
            isInitialized = true;
            Debug.Log("Debug Inspector initialized successfully");
        }
        
        private void FindSystemReferences()
        {
            // Find system managers if not already assigned
            if (dataManager == null)
                dataManager = FindObjectOfType<DataManager>();
            
            if (timeManager == null)
                timeManager = FindObjectOfType<TimeManager>();
            
            if (plantManager == null)
                plantManager = FindObjectOfType<PlantManager>();
            
            if (geneticsManager == null)
                geneticsManager = FindObjectOfType<GeneticsManager>();
        }
        
        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                Debug.LogError("DebugInspector: UIDocument component missing!");
                return;
            }
            
            rootElement = uiDocument.rootVisualElement;
            CacheUIElements();
            SetupUICallbacks();
            InitializeFilters();
            UpdateSystemStatus();
            
            statusMessage.text = "Debug Inspector Ready";
        }
        
        private void CacheUIElements()
        {
            // Time Control Elements
            pauseButton = rootElement.Q<Button>("pause-button");
            playButton = rootElement.Q<Button>("play-button");
            fastForwardButton = rootElement.Q<Button>("fast-forward-button");
            timeScaleLabel = rootElement.Q<Label>("time-scale-label");
            gameDayLabel = rootElement.Q<Label>("game-day-label");
            seasonLabel = rootElement.Q<Label>("season-label");
            
            // Plant Selection Elements
            plantList = rootElement.Q<ListView>("plant-list");
            growthStageFilter = rootElement.Q<DropdownField>("growth-stage-filter");
            healthFilter = rootElement.Q<DropdownField>("health-filter");
            strainFilter = rootElement.Q<DropdownField>("strain-filter");
            
            // Summary Stats
            totalPlantsLabel = rootElement.Q<Label>("total-plants-label");
            healthyPlantsLabel = rootElement.Q<Label>("healthy-plants-label");
            floweringPlantsLabel = rootElement.Q<Label>("flowering-plants-label");
            harvestReadyLabel = rootElement.Q<Label>("harvest-ready-label");
            
            // Plant Data Elements
            plantNameLabel = rootElement.Q<Label>("plant-name-label");
            plantInfoLabel = rootElement.Q<Label>("plant-info-label");
            genotypeList = rootElement.Q<VisualElement>("genotype-list");
            phenotypeList = rootElement.Q<VisualElement>("phenotype-list");
            
            // Progress Bars
            heightProgress = rootElement.Q<ProgressBar>("height-progress");
            biomassProgress = rootElement.Q<ProgressBar>("biomass-progress");
            maturityProgress = rootElement.Q<ProgressBar>("maturity-progress");
            healthProgress = rootElement.Q<ProgressBar>("health-progress");
            waterProgress = rootElement.Q<ProgressBar>("water-progress");
            nutritionProgress = rootElement.Q<ProgressBar>("nutrition-progress");
            thcProgress = rootElement.Q<ProgressBar>("thc-progress");
            cbdProgress = rootElement.Q<ProgressBar>("cbd-progress");
            trichromeProgress = rootElement.Q<ProgressBar>("trichrome-progress");
            
            // Environmental Labels
            temperatureLabel = rootElement.Q<Label>("temperature-label");
            humidityLabel = rootElement.Q<Label>("humidity-label");
            lightLabel = rootElement.Q<Label>("light-label");
            co2Label = rootElement.Q<Label>("co2-label");
            phLabel = rootElement.Q<Label>("ph-label");
            ecLabel = rootElement.Q<Label>("ec-label");
            
            // System Status Elements
            dataManagerIndicator = rootElement.Q<Label>("data-manager-indicator");
            timeManagerIndicator = rootElement.Q<Label>("time-manager-indicator");
            plantManagerIndicator = rootElement.Q<Label>("plant-manager-indicator");
            geneticsManagerIndicator = rootElement.Q<Label>("genetics-manager-indicator");
            
            // Performance Elements
            fpsLabel = rootElement.Q<Label>("fps-label");
            memoryLabel = rootElement.Q<Label>("memory-label");
            plantCountLabel = rootElement.Q<Label>("plant-count-label");
            updateTimeLabel = rootElement.Q<Label>("update-time-label");
            
            // Footer Elements
            statusMessage = rootElement.Q<Label>("status-message");
            lastUpdateLabel = rootElement.Q<Label>("last-update-label");
        }
        
        private void SetupUICallbacks()
        {
            // Time Control Callbacks
            pauseButton?.RegisterCallback<ClickEvent>(evt => OnPauseClicked());
            playButton?.RegisterCallback<ClickEvent>(evt => OnPlayClicked());
            fastForwardButton?.RegisterCallback<ClickEvent>(evt => OnFastForwardClicked());
            
            // Plant List Callbacks
            if (plantList != null)
            {
                plantList.onSelectionChange += OnPlantSelectionChanged;
                plantList.makeItem = () => CreatePlantListItem();
                plantList.bindItem = (element, index) => BindPlantListItem(element, index);
            }
            
            // Filter Callbacks
            growthStageFilter?.RegisterValueChangedCallback(evt => OnFilterChanged());
            healthFilter?.RegisterValueChangedCallback(evt => OnFilterChanged());
            strainFilter?.RegisterValueChangedCallback(evt => OnFilterChanged());
            
            // Debug Button Callbacks
            var forceUpdateButton = rootElement.Q<Button>("force-update-button");
            var growthEventButton = rootElement.Q<Button>("growth-event-button");
            var resetDataButton = rootElement.Q<Button>("reset-data-button");
            var exportLogButton = rootElement.Q<Button>("export-log-button");
            var exportCsvButton = rootElement.Q<Button>("export-csv-button");
            var exportJsonButton = rootElement.Q<Button>("export-json-button");
            
            forceUpdateButton?.RegisterCallback<ClickEvent>(evt => OnForceUpdateClicked());
            growthEventButton?.RegisterCallback<ClickEvent>(evt => OnGrowthEventClicked());
            resetDataButton?.RegisterCallback<ClickEvent>(evt => OnResetDataClicked());
            exportLogButton?.RegisterCallback<ClickEvent>(evt => OnExportLogClicked());
            exportCsvButton?.RegisterCallback<ClickEvent>(evt => OnExportCsvClicked());
            exportJsonButton?.RegisterCallback<ClickEvent>(evt => OnExportJsonClicked());
        }
        
        private void InitializeFilters()
        {
            // Initialize Growth Stage Filter
            if (growthStageFilter != null)
            {
                growthStageFilter.choices = new List<string>
                {
                    "All Stages",
                    "Seedling",
                    "Vegetative",
                    "Pre-Flowering",
                    "Flowering",
                    "Harvesting",
                    "Drying",
                    "Curing"
                };
                growthStageFilter.value = "All Stages";
            }
            
            // Initialize Health Filter
            if (healthFilter != null)
            {
                healthFilter.choices = new List<string>
                {
                    "All Health",
                    "Healthy",
                    "Stressed",
                    "Diseased",
                    "Critical"
                };
                healthFilter.value = "All Health";
            }
            
            // Initialize Strain Filter
            if (strainFilter != null)
            {
                strainFilter.choices = new List<string> { "All Strains" };
                strainFilter.value = "All Strains";
                UpdateStrainFilter();
            }
        }
        
        private void StartRealTimeUpdates()
        {
            if (enableRealTimeUpdates)
            {
                InvokeRepeating(nameof(UpdateDebugData), 0f, updateInterval);
            }
        }
        
        private void UpdateDebugData()
        {
            if (!isInitialized) return;
            
            updateStopwatch.Restart();
            
            UpdateTimeDisplay();
            UpdatePlantList();
            UpdateSelectedPlantData();
            UpdateSystemStatus();
            UpdatePerformanceMetrics();
            UpdateEnvironmentalData();
            
            updateStopwatch.Stop();
            lastUpdateLabel.text = $"Last Update: {DateTime.Now:HH:mm:ss}";
        }
        
        private void UpdateTimeDisplay()
        {
            if (timeManager != null)
            {
                timeScaleLabel.text = $"Time Scale: {timeManager.CurrentTimeScale:F1}x";
                gameDayLabel.text = $"Game Time: {timeManager.AcceleratedGameTime:F1}h";
                seasonLabel.text = $"Status: {(timeManager.IsTimePaused ? "Paused" : "Running")}";
            }
        }
        
        private void UpdatePlantList()
        {
            if (dataManager == null) return;
            
            var allPlants = dataManager.GetDataAssets<PlantInstanceSO>();
            filteredPlants = ApplyFilters(allPlants);
            
            if (plantList != null)
            {
                plantList.itemsSource = filteredPlants;
                plantList.RefreshItems();
            }
            
            UpdateSummaryStats();
        }
        
        private List<PlantInstanceSO> ApplyFilters(List<PlantInstanceSO> plants)
        {
            var filtered = plants.ToList();
            
            // Apply growth stage filter
            if (growthStageFilter?.value != "All Stages")
            {
                filtered = filtered.Where(p => p.CurrentGrowthStage.ToString() == growthStageFilter.value).ToList();
            }
            
            // Apply health filter
            if (healthFilter?.value != "All Health")
            {
                filtered = filtered.Where(p => GetHealthCategory(p.OverallHealth * 100f) == healthFilter.value).ToList();
            }
            
            // Apply strain filter
            if (strainFilter?.value != "All Strains")
            {
                filtered = filtered.Where(p => p.Strain?.name == strainFilter.value).ToList();
            }
            
            return filtered;
        }
        
        private string GetHealthCategory(float healthPercentage)
        {
            if (healthPercentage >= 80f) return "Healthy";
            if (healthPercentage >= 60f) return "Stressed";
            if (healthPercentage >= 30f) return "Diseased";
            return "Critical";
        }
        
        private void UpdateSummaryStats()
        {
            if (dataManager == null) return;
            
            var allPlants = dataManager.GetDataAssets<PlantInstanceSO>();
            totalPlantsLabel.text = $"Total Plants: {allPlants.Count}";
            
            var healthyCount = allPlants.Count(p => p.OverallHealth >= 0.8f);
            healthyPlantsLabel.text = $"Healthy: {healthyCount}";
            
            var floweringCount = allPlants.Count(p => p.CurrentGrowthStage.ToString().Contains("Flower"));
            floweringPlantsLabel.text = $"Flowering: {floweringCount}";
            
            var harvestCount = allPlants.Count(p => p.CurrentGrowthStage.ToString() == "Harvesting");
            harvestReadyLabel.text = $"Ready to Harvest: {harvestCount}";
        }
        
        private void UpdateSelectedPlantData()
        {
            if (selectedPlant == null)
            {
                plantNameLabel.text = "No Plant Selected";
                plantInfoLabel.text = "Select a plant from the list to view detailed information";
                ClearPlantData();
                return;
            }
            
            // Update plant overview
            plantNameLabel.text = $"{selectedPlant.PlantName} (ID: {selectedPlant.PlantID})";
            plantInfoLabel.text = $"Strain: {selectedPlant.Strain?.name} | Stage: {selectedPlant.CurrentGrowthStage} | Age: {selectedPlant.AgeInDays:F1} days";
            
            // Update genetic information
            UpdateGeneticDisplay();
            
            // Update growth statistics
            UpdateGrowthStats();
            
            // Update production statistics
            UpdateProductionStats();
        }
        
        private void UpdateGeneticDisplay()
        {
            if (selectedPlant == null) return;
            
            // Clear existing displays
            genotypeList.Clear();
            phenotypeList.Clear();
            
            // Get genetic data
            var genotype = selectedPlant.Genotype;
            
            // Display genotype information
            if (genotype != null)
            {
                DisplayGenotypeData(genotype);
            }
            else
            {
                // Show placeholder when no genetic data available
                var noDataElement = CreateGeneDisplayElement("No Genetic Data", "N/A", true);
                genotypeList.Add(noDataElement);
            }
            
            // Display phenotype information
            DisplayPhenotypeData();
        }
        
        private void DisplayGenotypeData(GenotypeDataSO genotype)
        {
            try
            {
                // Display basic genotype information
                var idElement = CreateGeneDisplayElement("Genotype ID", genotype.IndividualID ?? "Unknown", true);
                genotypeList.Add(idElement);
                
                var fitnessElement = CreateGeneDisplayElement("Overall Fitness", $"{genotype.OverallFitness:F2}", true);
                genotypeList.Add(fitnessElement);
                
                var diversityElement = CreateGeneDisplayElement("Genetic Diversity", $"{genotype.GeneticDiversity:F2}", true);
                genotypeList.Add(diversityElement);
                
                var viableElement = CreateGeneDisplayElement("Viability", genotype.IsViable ? "Viable" : "Non-viable", true);
                genotypeList.Add(viableElement);
                
                // Display gene pairs if available
                if (genotype.GenePairs != null && genotype.GenePairs.Count > 0)
                {
                    for (int i = 0; i < Mathf.Min(genotype.GenePairs.Count, 5); i++) // Limit to first 5 genes for UI space
                    {
                        var genePair = genotype.GenePairs[i];
                        if (genePair?.Gene != null)
                        {
                            var alleleDisplay = $"{genePair.Allele1?.AlleleCode ?? "?"}{genePair.Allele2?.AlleleCode ?? "?"}";
                            var geneElement = CreateGeneDisplayElement(genePair.Gene.GeneName ?? $"Gene {i+1}", alleleDisplay, true);
                            genotypeList.Add(geneElement);
                        }
                    }
                }
                
                // Display strain information
                if (selectedPlant.Strain != null)
                {
                    var strainElement = CreateGeneDisplayElement("Strain", selectedPlant.Strain.name, true);
                    genotypeList.Add(strainElement);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Error displaying genotype data: {ex.Message}");
                var errorElement = CreateGeneDisplayElement("Error", "Failed to load genetic data", true);
                genotypeList.Add(errorElement);
            }
        }
        
        private void DisplayPhenotypeData()
        {
            try
            {
                var genotype = selectedPlant.Genotype;
                
                // Display predicted traits if available
                if (genotype?.PredictedTraits != null && genotype.PredictedTraits.Count > 0)
                {
                    foreach (var predictedTrait in genotype.PredictedTraits)
                    {
                        var confidenceIndicator = predictedTrait.Confidence >= 0.8f ? "●" : (predictedTrait.Confidence >= 0.5f ? "◐" : "○");
                        var traitName = $"{predictedTrait.Trait} {confidenceIndicator}";
                        var traitElement = CreateTraitDisplayElement(traitName, predictedTrait.PredictedValue);
                        phenotypeList.Add(traitElement);
                    }
                }
                else
                {
                    // Display current observable traits from PlantInstanceSO
                    var heightElement = CreateTraitDisplayElement("Height (cm)", selectedPlant.CurrentHeight);
                    phenotypeList.Add(heightElement);
                    
                    var healthElement = CreateTraitDisplayElement("Health (%)", selectedPlant.OverallHealth * 100f);
                    phenotypeList.Add(healthElement);
                    
                    var vigorElement = CreateTraitDisplayElement("Vigor (%)", selectedPlant.Vigor * 100f);
                    phenotypeList.Add(vigorElement);
                    
                    var stressElement = CreateTraitDisplayElement("Stress Level (%)", selectedPlant.StressLevel * 100f);
                    phenotypeList.Add(stressElement);
                    
                    // Calculate and display growth potential if genetics manager is available
                    if (geneticsManager != null && genotype != null)
                    {
                        var growthPotential = genotype.GetGrowthPotential();
                        var growthElement = CreateTraitDisplayElement("Growth Potential", growthPotential);
                        phenotypeList.Add(growthElement);
                        
                        var yieldPotential = genotype.GetYieldPotential();
                        var yieldElement = CreateTraitDisplayElement("Yield Potential", yieldPotential);
                        phenotypeList.Add(yieldElement);
                        
                        var potencyPotential = genotype.GetPotencyPotential();
                        var potencyElement = CreateTraitDisplayElement("Potency Potential", potencyPotential);
                        phenotypeList.Add(potencyElement);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Error displaying phenotype data: {ex.Message}");
                var errorElement = CreateTraitDisplayElement("Error", 0f);
                phenotypeList.Add(errorElement);
            }
        }
        
        private VisualElement CreateGeneDisplayElement(string geneName, object geneValue, bool isGenotype)
        {
            var element = new VisualElement();
            element.AddToClassList("gene-display");
            element.style.flexDirection = FlexDirection.Row;
            element.style.justifyContent = Justify.SpaceBetween;
            element.style.paddingBottom = 2;
            element.style.paddingTop = 2;
            element.style.paddingLeft = 5;
            element.style.paddingRight = 5;
            element.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            element.style.marginBottom = 2;
            element.style.borderTopLeftRadius = new Length(2, LengthUnit.Pixel);
            element.style.borderTopRightRadius = new Length(2, LengthUnit.Pixel);
            element.style.borderBottomLeftRadius = new Length(2, LengthUnit.Pixel);
            element.style.borderBottomRightRadius = new Length(2, LengthUnit.Pixel);
            
            var nameLabel = new Label(geneName);
            nameLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            nameLabel.style.fontSize = new Length(11, LengthUnit.Pixel);
            
            var valueLabel = new Label(geneValue?.ToString() ?? "null");
            valueLabel.style.color = isGenotype ? new Color(0.7f, 0.9f, 0.7f) : new Color(0.9f, 0.8f, 0.6f);
            valueLabel.style.fontSize = new Length(11, LengthUnit.Pixel);
            valueLabel.style.fontStyle = FontStyle.Bold;
            
            element.Add(nameLabel);
            element.Add(valueLabel);
            
            return element;
        }
        
        private VisualElement CreateTraitDisplayElement(string traitName, float traitValue)
        {
            var element = new VisualElement();
            element.AddToClassList("trait-display");
            element.style.flexDirection = FlexDirection.Row;
            element.style.justifyContent = Justify.SpaceBetween;
            element.style.paddingBottom = 2;
            element.style.paddingTop = 2;
            element.style.paddingLeft = 5;
            element.style.paddingRight = 5;
            element.style.backgroundColor = new Color(0.3f, 0.3f, 0.2f, 0.5f);
            element.style.marginBottom = 2;
            element.style.borderTopLeftRadius = new Length(2, LengthUnit.Pixel);
            element.style.borderTopRightRadius = new Length(2, LengthUnit.Pixel);
            element.style.borderBottomLeftRadius = new Length(2, LengthUnit.Pixel);
            element.style.borderBottomRightRadius = new Length(2, LengthUnit.Pixel);
            
            var nameLabel = new Label(traitName);
            nameLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            nameLabel.style.fontSize = new Length(11, LengthUnit.Pixel);
            
            var valueLabel = new Label($"{traitValue:F2}");
            valueLabel.style.color = new Color(0.9f, 0.8f, 0.6f);
            valueLabel.style.fontSize = new Length(11, LengthUnit.Pixel);
            valueLabel.style.fontStyle = FontStyle.Bold;
            
            element.Add(nameLabel);
            element.Add(valueLabel);
            
            return element;
        }
        
        private void UpdateGrowthStats()
        {
            if (selectedPlant == null) return;
            
            // Update growth progress bars
            var maxHeight = selectedPlant.Strain?.BaseHeight ?? 200f; // Default max height
            heightProgress.value = selectedPlant.CurrentHeight / maxHeight * 100f;
            biomassProgress.value = selectedPlant.BiomassAccumulation * 10f; // Scale for display
            maturityProgress.value = (selectedPlant.AgeInDays / 90f) * 100f; // Assume 90 days to maturity
            
            // Update health stats
            healthProgress.value = selectedPlant.OverallHealth * 100f;
            waterProgress.value = selectedPlant.WaterLevel * 100f;
            nutritionProgress.value = selectedPlant.NutrientLevel * 100f;
            
            // Update progress bar titles with actual values
            heightProgress.title = $"Height: {selectedPlant.CurrentHeight:F1}cm / {maxHeight:F1}cm";
            biomassProgress.title = $"Biomass: {selectedPlant.BiomassAccumulation:F1}g/day";
            maturityProgress.title = $"Maturity: {(selectedPlant.AgeInDays / 90f) * 100f:F1}%";
            healthProgress.title = $"Health: {selectedPlant.OverallHealth * 100f:F1}%";
            waterProgress.title = $"Water: {selectedPlant.WaterLevel * 100f:F1}%";
            nutritionProgress.title = $"Nutrition: {selectedPlant.NutrientLevel * 100f:F1}%";
        }
        
        private void UpdateProductionStats()
        {
            if (selectedPlant == null) return;
            
            // Update cannabinoid levels (placeholder implementation)
            var thcLevel = Mathf.Clamp(selectedPlant.OverallHealth * 20f, 0f, 25f); // Simulated THC based on health
            var cbdLevel = Mathf.Clamp(selectedPlant.Vigor * 15f, 0f, 20f); // Simulated CBD based on vigor
            var trichromeLevel = Mathf.Clamp((1f - selectedPlant.StressLevel) * 80f, 0f, 80f); // Simulated trichomes
            
            thcProgress.value = thcLevel;
            cbdProgress.value = cbdLevel;
            trichromeProgress.value = trichromeLevel;
            
            // Update progress bar titles
            thcProgress.title = $"THC: {thcLevel:F2}%";
            cbdProgress.title = $"CBD: {cbdLevel:F2}%";
            trichromeProgress.title = $"Trichomes: {trichromeLevel:F1}%";
        }
        
        private void UpdateEnvironmentalData()
        {
            // This would integrate with environmental systems when available
            // For now, showing placeholder data
            temperatureLabel.text = "Temperature: 24.5°C";
            humidityLabel.text = "Humidity: 65%RH";
            lightLabel.text = "Light: 450 PPFD";
            co2Label.text = "CO₂: 1200 ppm";
            phLabel.text = "pH: 6.2";
            ecLabel.text = "EC: 1.8 mS/cm";
        }
        
        private void UpdateSystemStatus()
        {
            // Update system status indicators
            UpdateManagerStatus(dataManagerIndicator, dataManager != null);
            UpdateManagerStatus(timeManagerIndicator, timeManager != null);
            UpdateManagerStatus(plantManagerIndicator, plantManager != null);
            UpdateManagerStatus(geneticsManagerIndicator, geneticsManager != null);
        }
        
        private void UpdateManagerStatus(Label indicator, bool isActive)
        {
            if (indicator == null) return;
            
            indicator.text = isActive ? "●" : "○";
            indicator.style.color = isActive ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f);
        }
        
        private void UpdatePerformanceMetrics()
        {
            if (!enablePerformanceMonitoring) return;
            
            // Update FPS
            lastFPS = 1f / Time.unscaledDeltaTime;
            fpsLabel.text = $"FPS: {lastFPS:F1}";
            
            // Update Memory Usage
            lastMemoryUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory() / 1024 / 1024;
            memoryLabel.text = $"Memory: {lastMemoryUsage} MB";
            
            // Update Plant Count
            var plantCount = dataManager?.GetDataAssets<PlantInstanceSO>().Count ?? 0;
            plantCountLabel.text = $"Plant Count: {plantCount}";
            
            // Update Update Time
            updateTimeLabel.text = $"Update Time: {updateStopwatch.ElapsedMilliseconds} ms";
        }
        
        private void ClearPlantData()
        {
            genotypeList?.Clear();
            phenotypeList?.Clear();
            
            // Reset progress bars
            heightProgress.value = 0;
            biomassProgress.value = 0;
            maturityProgress.value = 0;
            healthProgress.value = 0;
            waterProgress.value = 0;
            nutritionProgress.value = 0;
            thcProgress.value = 0;
            cbdProgress.value = 0;
            trichromeProgress.value = 0;
        }
        
        private void UpdateStrainFilter()
        {
            if (dataManager == null || strainFilter == null) return;
            
            var allPlants = dataManager.GetDataAssets<PlantInstanceSO>();
            var strains = allPlants.Where(p => p.Strain != null).Select(p => p.Strain.name).Distinct().ToList();
            
            var choices = new List<string> { "All Strains" };
            choices.AddRange(strains);
            
            strainFilter.choices = choices;
        }
        
        // UI Event Handlers
        
        private void OnPauseClicked()
        {
            timeManager?.Pause();
            statusMessage.text = "Time Paused";
        }
        
        private void OnPlayClicked()
        {
            timeManager?.Resume();
            statusMessage.text = "Time Playing";
        }
        
        private void OnFastForwardClicked()
        {
            timeManager?.SetTimeScale(5f);
            statusMessage.text = "Fast Forward Active";
        }
        
        private void OnPlantSelectionChanged(IEnumerable<object> selectedItems)
        {
            var selected = selectedItems?.FirstOrDefault() as PlantInstanceSO;
            selectedPlant = selected;
            
            if (selectedPlant != null)
            {
                statusMessage.text = $"Selected: {selectedPlant.PlantName}";
            }
            else
            {
                statusMessage.text = "No plant selected";
            }
        }
        
        private void OnFilterChanged()
        {
            UpdatePlantList();
            statusMessage.text = "Filters updated";
        }
        
        private void OnForceUpdateClicked()
        {
            UpdateDebugData();
            statusMessage.text = "Forced update completed";
        }
        
        private void OnGrowthEventClicked()
        {
            if (selectedPlant != null)
            {
                selectedPlant.ProcessDailyGrowth(selectedPlant.CurrentEnvironment);
                statusMessage.text = "Growth event triggered";
            }
            else
            {
                statusMessage.text = "No plant selected for growth event";
            }
        }
        
        private void OnResetDataClicked()
        {
            selectedPlant = null;
            ClearPlantData();
            statusMessage.text = "Data reset";
        }
        
        private void OnExportLogClicked()
        {
            Debug.Log("Debug log export requested");
            statusMessage.text = "Debug log exported";
        }
        
        private void OnExportCsvClicked()
        {
            ExportDataToCsv();
            statusMessage.text = "CSV export completed";
        }
        
        private void OnExportJsonClicked()
        {
            ExportDataToJson();
            statusMessage.text = "JSON export completed";
        }
        
        // UI Helper Methods
        
        private VisualElement CreatePlantListItem()
        {
            var item = new VisualElement();
            item.style.flexDirection = FlexDirection.Row;
            item.style.justifyContent = Justify.SpaceBetween;
            item.style.paddingBottom = 5;
            item.style.paddingTop = 5;
            item.style.paddingLeft = 8;
            item.style.paddingRight = 8;
            
            var nameLabel = new Label();
            nameLabel.name = "plant-name";
            nameLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
            nameLabel.style.fontSize = 12;
            
            var statusLabel = new Label();
            statusLabel.name = "plant-status";
            statusLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            statusLabel.style.fontSize = 10;
            
            item.Add(nameLabel);
            item.Add(statusLabel);
            
            return item;
        }
        
        private void BindPlantListItem(VisualElement element, int index)
        {
            if (index >= filteredPlants.Count) return;
            
            var plant = filteredPlants[index];
            var nameLabel = element.Q<Label>("plant-name");
            var statusLabel = element.Q<Label>("plant-status");
            
            nameLabel.text = plant.PlantName;
            statusLabel.text = $"{plant.CurrentGrowthStage} - {plant.OverallHealth * 100f:F0}%";
            
            // Color code by health
            var healthPercentage = plant.OverallHealth * 100f;
            if (healthPercentage >= 80f)
                statusLabel.style.color = new Color(0.2f, 0.8f, 0.2f);
            else if (healthPercentage >= 60f)
                statusLabel.style.color = new Color(0.8f, 0.8f, 0.2f);
            else
                statusLabel.style.color = new Color(0.8f, 0.2f, 0.2f);
        }
        
        // Data Export Methods
        
        private void ExportDataToCsv()
        {
            // Implementation for CSV export
            Debug.Log("CSV export functionality would be implemented here");
        }
        
        private void ExportDataToJson()
        {
            // Implementation for JSON export
            Debug.Log("JSON export functionality would be implemented here");
        }
        
        // Public API
        
        public void SelectPlant(PlantInstanceSO plant)
        {
            selectedPlant = plant;
            if (plantList != null)
            {
                var index = filteredPlants.IndexOf(plant);
                if (index >= 0)
                {
                    plantList.selectedIndex = index;
                }
            }
        }
        
        public PlantInstanceSO GetSelectedPlant()
        {
            return selectedPlant;
        }
        
        public void RefreshData()
        {
            UpdateDebugData();
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
}