using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Systems.Genetics;
using ProjectChimera.Systems.Cultivation;
using BreedingCompatibility = ProjectChimera.Data.Genetics.BreedingCompatibility;
using BreedingResult = ProjectChimera.Systems.Genetics.BreedingResult;

namespace ProjectChimera.UI
{
    /// <summary>
    /// Phase 1.1: Breeding UI Implementation
    /// Provides comprehensive interface for plant breeding operations including
    /// parent selection, compatibility analysis, and offspring generation.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class BreedingUI : MonoBehaviour
    {
        [Header("Breeding UI Configuration")]
        [SerializeField] private bool _enableRealTimeUpdates = true;
        [SerializeField] private float _updateInterval = 1f;
        [SerializeField] private bool _enableCompatibilityAnalysis = true;
        [SerializeField] private bool _enableAdvancedOptions = true;
        
        [Header("Breeding Parameters")]
        [SerializeField] private int _defaultOffspringCount = 4;
        [SerializeField] private int _maxOffspringCount = 20;
        [SerializeField] private bool _allowInbreeding = false;
        [SerializeField] private float _inbreedingDepression = 0.15f;
        [SerializeField] private bool _enableMutations = true;
        [SerializeField] private float _defaultMutationRate = 1f;
        
        [Header("System References")]
        [SerializeField] private DataManager _dataManager;
        [SerializeField] private GeneticsManager _geneticsManager;
        [SerializeField] private PlantManager _plantManager;
        
        // UI Document and Root
        private UIDocument _uiDocument;
        private VisualElement _rootElement;
        
        // Parent Selection UI Elements
        private DropdownField _parent1Dropdown;
        private DropdownField _parent2Dropdown;
        private Button _swapParentsButton;
        private Button _clearSelectionButton;
        
        // Parent Display Elements
        private VisualElement _parent1Display;
        private VisualElement _parent2Display;
        private Label _parent1NameLabel;
        private Label _parent2NameLabel;
        private Label _parent1GenotypeLabel;
        private Label _parent2GenotypeLabel;
        private VisualElement _parent1TraitsList;
        private VisualElement _parent2TraitsList;
        
        // Compatibility Analysis Elements
        private VisualElement _compatibilitySection;
        private ProgressBar _compatibilityScoreBar;
        private ProgressBar _geneticDistanceBar;
        private ProgressBar _inbreedingRiskBar;
        private Label _compatibilityDetailsLabel;
        private Label _breedingRecommendationLabel;
        
        // Breeding Options Elements
        private IntegerField _offspringCountField;
        private Toggle _enableMutationsToggle;
        private Slider _mutationRateSlider;
        private Label _mutationRateLabel;
        private Toggle _allowInbreedingToggle;
        private Slider _inbreedingDepressionSlider;
        private Label _inbreedingDepressionLabel;
        
        // Breeding Control Elements
        private Button _analyzeCompatibilityButton;
        private Button _performBreedingButton;
        private Button _resetBreedingButton;
        private Label _breedingStatusLabel;
        
        // Results Display Elements
        private VisualElement _resultsSection;
        private ListView _offspringList;
        private Label _breedingResultsLabel;
        private Button _saveOffspringButton;
        private Button _exportResultsButton;
        
        // Advanced Options Elements
        private VisualElement _advancedOptionsSection;
        private Toggle _showAdvancedOptionsToggle;
        private DropdownField _breedingStrategyDropdown;
        private Toggle _enableParentalScreeningToggle;
        private Slider _fitnessThresholdSlider;
        private Label _fitnessThresholdLabel;
        
        // State Management
        private PlantGenotype _selectedParent1;
        private PlantGenotype _selectedParent2;
        private BreedingCompatibility _currentCompatibility;
        private BreedingResult _lastBreedingResult;
        private List<PlantGenotype> _availableParents = new List<PlantGenotype>();
        private BreedingSimulator _breedingSimulator;
        
        // UI State
        private bool _isInitialized = false;
        private bool _isBreedingInProgress = false;
        private bool _hasValidSelection = false;
        
        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            FindSystemReferences();
            InitializeBreedingSimulator();
        }
        
        private void Start()
        {
            InitializeUI();
            LoadAvailableParents();
            UpdateUI();
            
            if (_enableRealTimeUpdates)
            {
                InvokeRepeating(nameof(UpdateUI), 0f, _updateInterval);
            }
            
            _isInitialized = true;
            Debug.Log("Breeding UI initialized successfully");
        }
        
        private void FindSystemReferences()
        {
            if (_dataManager == null)
                _dataManager = FindObjectOfType<DataManager>();
            
            if (_geneticsManager == null)
                _geneticsManager = FindObjectOfType<GeneticsManager>();
            
            if (_plantManager == null)
                _plantManager = FindObjectOfType<PlantManager>();
        }
        
        private void InitializeBreedingSimulator()
        {
            _breedingSimulator = new BreedingSimulator(_allowInbreeding, _inbreedingDepression);
        }
        
        private void InitializeUI()
        {
            if (_uiDocument == null)
            {
                Debug.LogError("BreedingUI: UIDocument component missing!");
                return;
            }
            
            _rootElement = _uiDocument.rootVisualElement;
            CacheUIElements();
            SetupUICallbacks();
            InitializeDropdowns();
            SetupAdvancedOptions();
            
            UpdateBreedingStatus("Ready for breeding");
        }
        
        private void CacheUIElements()
        {
            // Parent Selection Elements
            _parent1Dropdown = _rootElement.Q<DropdownField>("parent1-dropdown");
            _parent2Dropdown = _rootElement.Q<DropdownField>("parent2-dropdown");
            _swapParentsButton = _rootElement.Q<Button>("swap-parents-button");
            _clearSelectionButton = _rootElement.Q<Button>("clear-selection-button");
            
            // Parent Display Elements
            _parent1Display = _rootElement.Q<VisualElement>("parent1-display");
            _parent2Display = _rootElement.Q<VisualElement>("parent2-display");
            _parent1NameLabel = _rootElement.Q<Label>("parent1-name-label");
            _parent2NameLabel = _rootElement.Q<Label>("parent2-name-label");
            _parent1GenotypeLabel = _rootElement.Q<Label>("parent1-genotype-label");
            _parent2GenotypeLabel = _rootElement.Q<Label>("parent2-genotype-label");
            _parent1TraitsList = _rootElement.Q<VisualElement>("parent1-traits-list");
            _parent2TraitsList = _rootElement.Q<VisualElement>("parent2-traits-list");
            
            // Compatibility Analysis Elements
            _compatibilitySection = _rootElement.Q<VisualElement>("compatibility-section");
            _compatibilityScoreBar = _rootElement.Q<ProgressBar>("compatibility-score-bar");
            _geneticDistanceBar = _rootElement.Q<ProgressBar>("genetic-distance-bar");
            _inbreedingRiskBar = _rootElement.Q<ProgressBar>("inbreeding-risk-bar");
            _compatibilityDetailsLabel = _rootElement.Q<Label>("compatibility-details-label");
            _breedingRecommendationLabel = _rootElement.Q<Label>("breeding-recommendation-label");
            
            // Breeding Options Elements
            _offspringCountField = _rootElement.Q<IntegerField>("offspring-count-field");
            _enableMutationsToggle = _rootElement.Q<Toggle>("enable-mutations-toggle");
            _mutationRateSlider = _rootElement.Q<Slider>("mutation-rate-slider");
            _mutationRateLabel = _rootElement.Q<Label>("mutation-rate-label");
            _allowInbreedingToggle = _rootElement.Q<Toggle>("allow-inbreeding-toggle");
            _inbreedingDepressionSlider = _rootElement.Q<Slider>("inbreeding-depression-slider");
            _inbreedingDepressionLabel = _rootElement.Q<Label>("inbreeding-depression-label");
            
            // Breeding Control Elements
            _analyzeCompatibilityButton = _rootElement.Q<Button>("analyze-compatibility-button");
            _performBreedingButton = _rootElement.Q<Button>("perform-breeding-button");
            _resetBreedingButton = _rootElement.Q<Button>("reset-breeding-button");
            _breedingStatusLabel = _rootElement.Q<Label>("breeding-status-label");
            
            // Results Display Elements
            _resultsSection = _rootElement.Q<VisualElement>("results-section");
            _offspringList = _rootElement.Q<ListView>("offspring-list");
            _breedingResultsLabel = _rootElement.Q<Label>("breeding-results-label");
            _saveOffspringButton = _rootElement.Q<Button>("save-offspring-button");
            _exportResultsButton = _rootElement.Q<Button>("export-results-button");
            
            // Advanced Options Elements
            _advancedOptionsSection = _rootElement.Q<VisualElement>("advanced-options-section");
            _showAdvancedOptionsToggle = _rootElement.Q<Toggle>("show-advanced-options-toggle");
            _breedingStrategyDropdown = _rootElement.Q<DropdownField>("breeding-strategy-dropdown");
            _enableParentalScreeningToggle = _rootElement.Q<Toggle>("enable-parental-screening-toggle");
            _fitnessThresholdSlider = _rootElement.Q<Slider>("fitness-threshold-slider");
            _fitnessThresholdLabel = _rootElement.Q<Label>("fitness-threshold-label");
        }
        
        private void SetupUICallbacks()
        {
            // Parent Selection Callbacks
            _parent1Dropdown?.RegisterValueChangedCallback(evt => OnParent1Changed(evt.newValue));
            _parent2Dropdown?.RegisterValueChangedCallback(evt => OnParent2Changed(evt.newValue));
            _swapParentsButton?.RegisterCallback<ClickEvent>(evt => OnSwapParentsClicked());
            _clearSelectionButton?.RegisterCallback<ClickEvent>(evt => OnClearSelectionClicked());
            
            // Breeding Options Callbacks
            _offspringCountField?.RegisterValueChangedCallback(evt => OnOffspringCountChanged(evt.newValue));
            _enableMutationsToggle?.RegisterValueChangedCallback(evt => OnMutationsToggleChanged(evt.newValue));
            _mutationRateSlider?.RegisterValueChangedCallback(evt => OnMutationRateChanged(evt.newValue));
            _allowInbreedingToggle?.RegisterValueChangedCallback(evt => OnInbreedingToggleChanged(evt.newValue));
            _inbreedingDepressionSlider?.RegisterValueChangedCallback(evt => OnInbreedingDepressionChanged(evt.newValue));
            
            // Breeding Control Callbacks
            _analyzeCompatibilityButton?.RegisterCallback<ClickEvent>(evt => OnAnalyzeCompatibilityClicked());
            _performBreedingButton?.RegisterCallback<ClickEvent>(evt => OnPerformBreedingClicked());
            _resetBreedingButton?.RegisterCallback<ClickEvent>(evt => OnResetBreedingClicked());
            
            // Results Callbacks
            _saveOffspringButton?.RegisterCallback<ClickEvent>(evt => OnSaveOffspringClicked());
            _exportResultsButton?.RegisterCallback<ClickEvent>(evt => OnExportResultsClicked());
            
            // Advanced Options Callbacks
            _showAdvancedOptionsToggle?.RegisterValueChangedCallback(evt => OnAdvancedOptionsToggleChanged(evt.newValue));
            _breedingStrategyDropdown?.RegisterValueChangedCallback(evt => OnBreedingStrategyChanged(evt.newValue));
            _enableParentalScreeningToggle?.RegisterValueChangedCallback(evt => OnParentalScreeningToggleChanged(evt.newValue));
            _fitnessThresholdSlider?.RegisterValueChangedCallback(evt => OnFitnessThresholdChanged(evt.newValue));
            
            // Offspring List Setup
            if (_offspringList != null)
            {
                _offspringList.makeItem = () => CreateOffspringListItem();
                _offspringList.bindItem = (element, index) => BindOffspringListItem(element, index);
                _offspringList.onSelectionChange += OnOffspringSelectionChanged;
            }
        }
        
        private void InitializeDropdowns()
        {
            // Initialize breeding strategy dropdown
            if (_breedingStrategyDropdown != null)
            {
                _breedingStrategyDropdown.choices = new List<string>
                {
                    "Standard Crossing",
                    "Backcrossing",
                    "Selfing",
                    "Line Breeding",
                    "Outcrossing"
                };
                _breedingStrategyDropdown.value = "Standard Crossing";
            }
        }
        
        private void SetupAdvancedOptions()
        {
            // Initialize field values
            if (_offspringCountField != null)
            {
                _offspringCountField.value = _defaultOffspringCount;
            }
            
            if (_enableMutationsToggle != null)
            {
                _enableMutationsToggle.value = _enableMutations;
            }
            
            if (_mutationRateSlider != null)
            {
                _mutationRateSlider.lowValue = 0f;
                _mutationRateSlider.highValue = 5f;
                _mutationRateSlider.value = _defaultMutationRate;
                UpdateMutationRateLabel(_defaultMutationRate);
            }
            
            if (_allowInbreedingToggle != null)
            {
                _allowInbreedingToggle.value = _allowInbreeding;
            }
            
            if (_inbreedingDepressionSlider != null)
            {
                _inbreedingDepressionSlider.lowValue = 0f;
                _inbreedingDepressionSlider.highValue = 1f;
                _inbreedingDepressionSlider.value = _inbreedingDepression;
                UpdateInbreedingDepressionLabel(_inbreedingDepression);
            }
            
            if (_fitnessThresholdSlider != null)
            {
                _fitnessThresholdSlider.lowValue = 0f;
                _fitnessThresholdSlider.highValue = 1f;
                _fitnessThresholdSlider.value = 0.5f;
                UpdateFitnessThresholdLabel(0.5f);
            }
            
            // Hide advanced options initially if toggle is available
            if (_advancedOptionsSection != null && _showAdvancedOptionsToggle?.value == false)
            {
                _advancedOptionsSection.style.display = DisplayStyle.None;
            }
        }
        
        private void LoadAvailableParents()
        {
            _availableParents.Clear();
            
            if (_dataManager != null)
            {
                var allPlants = _dataManager.GetDataAssets<PlantInstanceSO>();
                foreach (var plant in allPlants)
                {
                    if (plant.Genotype != null && plant.IsBreedingEligible())
                    {
                        _availableParents.Add(plant.GetPlantGenotype());
                    }
                }
            }
            
            UpdateParentDropdowns();
        }
        
        private void UpdateParentDropdowns()
        {
            var parentNames = new List<string> { "Select Parent..." };
            parentNames.AddRange(_availableParents.Select(p => $"{p.GenotypeID} ({p.StrainOrigin?.name ?? "Unknown"})"));
            
            if (_parent1Dropdown != null)
            {
                _parent1Dropdown.choices = parentNames;
                if (_selectedParent1 == null)
                    _parent1Dropdown.value = parentNames[0];
            }
            
            if (_parent2Dropdown != null)
            {
                _parent2Dropdown.choices = parentNames;
                if (_selectedParent2 == null)
                    _parent2Dropdown.value = parentNames[0];
            }
        }
        
        private void UpdateUI()
        {
            if (!_isInitialized) return;
            
            UpdateParentDisplays();
            UpdateCompatibilityDisplay();
            UpdateBreedingControls();
            UpdateResultsDisplay();
        }
        
        private void UpdateParentDisplays()
        {
            UpdateParentDisplay(_selectedParent1, _parent1NameLabel, _parent1GenotypeLabel, _parent1TraitsList);
            UpdateParentDisplay(_selectedParent2, _parent2NameLabel, _parent2GenotypeLabel, _parent2TraitsList);
        }
        
        private void UpdateParentDisplay(PlantGenotype parent, Label nameLabel, Label genotypeLabel, VisualElement traitsList)
        {
            if (parent == null)
            {
                            if (nameLabel != null) nameLabel.text = "No parent selected";
            if (genotypeLabel != null) genotypeLabel.text = "";
                traitsList?.Clear();
                return;
            }
            
            if (nameLabel != null) nameLabel.text = $"{parent.GenotypeID}";
            if (genotypeLabel != null) genotypeLabel.text = $"Strain: {parent.StrainOrigin?.name ?? "Unknown"} | Generation: {parent.Generation} | Fitness: {parent.OverallFitness:F2}";
            
            // Update traits list
            traitsList?.Clear();
            if (parent.Genotype != null)
            {
                foreach (var geneLocus in parent.Genotype.Keys)
                {
                    var alleles = parent.Genotype[geneLocus];
                    var traitElement = CreateTraitDisplayElement(geneLocus, alleles);
                    traitsList?.Add(traitElement);
                }
            }
        }
        
        private VisualElement CreateTraitDisplayElement(string geneLocus, AlleleCouple alleles)
        {
            var element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;
            element.style.justifyContent = Justify.SpaceBetween;
            element.style.paddingBottom = 2;
            element.style.paddingTop = 2;
            element.style.paddingLeft = 5;
            element.style.paddingRight = 5;
            element.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            element.style.marginBottom = 2;
            element.style.borderTopLeftRadius = new Length(2, LengthUnit.Pixel);
            element.style.borderTopRightRadius = new Length(2, LengthUnit.Pixel);
            element.style.borderBottomLeftRadius = new Length(2, LengthUnit.Pixel);
            element.style.borderBottomRightRadius = new Length(2, LengthUnit.Pixel);
            
            var locusLabel = new Label(geneLocus);
            locusLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            locusLabel.style.fontSize = new Length(11, LengthUnit.Pixel);
            
            var alleleDisplay = $"{alleles?.Allele1?.AlleleCode ?? "?"}{alleles?.Allele2?.AlleleCode ?? "?"}";
            var alleleLabel = new Label(alleleDisplay);
            alleleLabel.style.color = new Color(0.7f, 0.9f, 0.7f);
            alleleLabel.style.fontSize = new Length(11, LengthUnit.Pixel);
            alleleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            element.Add(locusLabel);
            element.Add(alleleLabel);
            
            return element;
        }
        
        private void UpdateCompatibilityDisplay()
        {
            if (_currentCompatibility == null || !_hasValidSelection)
            {
                _compatibilitySection?.SetEnabled(false);
                return;
            }
            
            _compatibilitySection?.SetEnabled(true);
            
            // Update progress bars
            if (_compatibilityScoreBar != null)
            {
                _compatibilityScoreBar.value = _currentCompatibility.CompatibilityScore * 100f;
                _compatibilityScoreBar.title = $"Compatibility: {_currentCompatibility.CompatibilityScore:F2}";
            }
            
            if (_geneticDistanceBar != null)
            {
                _geneticDistanceBar.value = _currentCompatibility.GeneticDistance * 100f;
                _geneticDistanceBar.title = $"Genetic Distance: {_currentCompatibility.GeneticDistance:F2}";
            }
            
            if (_inbreedingRiskBar != null)
            {
                _inbreedingRiskBar.value = _currentCompatibility.InbreedingRisk * 100f;
                _inbreedingRiskBar.title = $"Inbreeding Risk: {_currentCompatibility.InbreedingRisk:F2}";
            }
            
            // Update details label
            if (_compatibilityDetailsLabel != null) _compatibilityDetailsLabel.text = $"Analysis of {_currentCompatibility.Parent1ID} × {_currentCompatibility.Parent2ID}";
            
            // Update breeding recommendation
            var recommendation = GetBreedingRecommendation(_currentCompatibility);
            if (_breedingRecommendationLabel != null) _breedingRecommendationLabel.text = recommendation;
        }
        
        private string GetBreedingRecommendation(BreedingCompatibility compatibility)
        {
            if (compatibility.CompatibilityScore >= 0.8f)
                return "Excellent breeding pair - highly recommended";
            else if (compatibility.CompatibilityScore >= 0.6f)
                return "Good breeding pair - recommended";
            else if (compatibility.CompatibilityScore >= 0.4f)
                return "Moderate breeding pair - proceed with caution";
            else if (compatibility.InbreedingRisk > 0.5f)
                return "High inbreeding risk - not recommended";
            else
                return "Poor compatibility - not recommended";
        }
        
        private void UpdateBreedingControls()
        {
            var canAnalyze = _hasValidSelection && !_isBreedingInProgress;
            var canBreed = _hasValidSelection && _currentCompatibility != null && !_isBreedingInProgress;
            
            _analyzeCompatibilityButton?.SetEnabled(canAnalyze);
            _performBreedingButton?.SetEnabled(canBreed);
            _resetBreedingButton?.SetEnabled(!_isBreedingInProgress);
        }
        
        private void UpdateResultsDisplay()
        {
            if (_lastBreedingResult == null || _lastBreedingResult.OffspringGenotypes.Count == 0)
            {
                _resultsSection?.SetEnabled(false);
                return;
            }
            
            _resultsSection?.SetEnabled(true);
            
            // Update results label
            var resultText = $"Breeding Results: {_lastBreedingResult.OffspringGenotypes.Count} offspring generated";
            if (_lastBreedingResult.MutationsOccurred.Count > 0)
                resultText += $" ({_lastBreedingResult.MutationsOccurred.Count} mutations)";
            
            if (_breedingResultsLabel != null) _breedingResultsLabel.text = resultText;
            
            // Update offspring list
            if (_offspringList != null)
            {
                _offspringList.itemsSource = _lastBreedingResult.OffspringGenotypes;
                _offspringList.RefreshItems();
            }
        }
        
        // Event Handlers
        
        private void OnParent1Changed(string value)
        {
            if (value == "Select Parent..." || string.IsNullOrEmpty(value))
            {
                _selectedParent1 = null;
            }
            else
            {
                var index = _parent1Dropdown.choices.IndexOf(value) - 1; // Subtract 1 for "Select Parent..." option
                if (index >= 0 && index < _availableParents.Count)
                {
                    _selectedParent1 = _availableParents[index];
                }
            }
            
            UpdateSelectionState();
        }
        
        private void OnParent2Changed(string value)
        {
            if (value == "Select Parent..." || string.IsNullOrEmpty(value))
            {
                _selectedParent2 = null;
            }
            else
            {
                var index = _parent2Dropdown.choices.IndexOf(value) - 1; // Subtract 1 for "Select Parent..." option
                if (index >= 0 && index < _availableParents.Count)
                {
                    _selectedParent2 = _availableParents[index];
                }
            }
            
            UpdateSelectionState();
        }
        
        private void UpdateSelectionState()
        {
            _hasValidSelection = _selectedParent1 != null && _selectedParent2 != null && _selectedParent1 != _selectedParent2;
            _currentCompatibility = null; // Clear previous analysis
            UpdateUI();
        }
        
        private void OnSwapParentsClicked()
        {
            var temp = _selectedParent1;
            _selectedParent1 = _selectedParent2;
            _selectedParent2 = temp;
            
            // Update dropdown selections
            if (_parent1Dropdown != null && _parent2Dropdown != null)
            {
                var temp1 = _parent1Dropdown.value;
                _parent1Dropdown.value = _parent2Dropdown.value;
                _parent2Dropdown.value = temp1;
            }
            
            UpdateUI();
            UpdateBreedingStatus("Parents swapped");
        }
        
        private void OnClearSelectionClicked()
        {
            _selectedParent1 = null;
            _selectedParent2 = null;
            _currentCompatibility = null;
            _lastBreedingResult = null;
            
            // Reset dropdowns
            if (_parent1Dropdown != null)
                _parent1Dropdown.value = _parent1Dropdown.choices[0];
            if (_parent2Dropdown != null)
                _parent2Dropdown.value = _parent2Dropdown.choices[0];
            
            UpdateSelectionState();
            UpdateBreedingStatus("Selection cleared");
        }
        
        private void OnOffspringCountChanged(int value)
        {
            _defaultOffspringCount = Mathf.Clamp(value, 1, _maxOffspringCount);
        }
        
        private void OnMutationsToggleChanged(bool value)
        {
            _enableMutations = value;
            _mutationRateSlider?.SetEnabled(value);
        }
        
        private void OnMutationRateChanged(float value)
        {
            _defaultMutationRate = value;
            UpdateMutationRateLabel(value);
        }
        
        private void OnInbreedingToggleChanged(bool value)
        {
            _allowInbreeding = value;
            _breedingSimulator = new BreedingSimulator(_allowInbreeding, _inbreedingDepression);
        }
        
        private void OnInbreedingDepressionChanged(float value)
        {
            _inbreedingDepression = value;
            UpdateInbreedingDepressionLabel(value);
            _breedingSimulator = new BreedingSimulator(_allowInbreeding, _inbreedingDepression);
        }
        
        private void OnAnalyzeCompatibilityClicked()
        {
            if (!_hasValidSelection) return;
            
            var systemsCompatibility = _breedingSimulator.AnalyzeCompatibility(_selectedParent1, _selectedParent2);
            _currentCompatibility = ConvertToDataBreedingCompatibility(systemsCompatibility);
            UpdateUI();
            UpdateBreedingStatus("Compatibility analysis complete");
        }
        
        /// <summary>
        /// PC014-FIX-41: Convert Systems.Genetics.BreedingCompatibility to Data.Genetics.BreedingCompatibility
        /// </summary>
        private ProjectChimera.Data.Genetics.BreedingCompatibility ConvertToDataBreedingCompatibility(ProjectChimera.Systems.Genetics.BreedingCompatibility systemsCompatibility)
        {
            if (systemsCompatibility == null) return null;
            
            return new ProjectChimera.Data.Genetics.BreedingCompatibility
            {
                Parent1ID = systemsCompatibility.Parent1ID ?? systemsCompatibility.Plant1Id ?? "unknown",
                Parent2ID = systemsCompatibility.Parent2ID ?? systemsCompatibility.Plant2Id ?? "unknown",
                CompatibilityScore = systemsCompatibility.CompatibilityScore,
                GeneticDistance = systemsCompatibility.GeneticDistance,
                InbreedingRisk = systemsCompatibility.InbreedingRisk,
                ExpectedHeterosis = systemsCompatibility.ExpectedHeterosis,
                ComplementarityScore = systemsCompatibility.ComplementarityScore,
                PredictedHeterosis = systemsCompatibility.PredictedHeterosis
            };
        }
        
        private void OnPerformBreedingClicked()
        {
            if (!_hasValidSelection || _isBreedingInProgress) return;
            
            _isBreedingInProgress = true;
            UpdateBreedingStatus("Performing breeding...");
            
            try
            {
                _lastBreedingResult = _breedingSimulator.PerformBreeding(
                    _selectedParent1,
                    _selectedParent2,
                    _defaultOffspringCount,
                    _enableMutations,
                    _defaultMutationRate
                );
                
                UpdateBreedingStatus($"Breeding complete - {_lastBreedingResult.OffspringGenotypes.Count} offspring generated");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during breeding: {ex.Message}");
                UpdateBreedingStatus("Breeding failed");
            }
            finally
            {
                _isBreedingInProgress = false;
                UpdateUI();
            }
        }
        
        private void OnResetBreedingClicked()
        {
            OnClearSelectionClicked();
            LoadAvailableParents();
            UpdateBreedingStatus("Breeding UI reset");
        }
        
        private void OnSaveOffspringClicked()
        {
            if (_lastBreedingResult?.OffspringGenotypes?.Count > 0)
            {
                // Implementation would save offspring to the data system
                UpdateBreedingStatus($"Saved {_lastBreedingResult.OffspringGenotypes.Count} offspring to genetics library");
                Debug.Log("Save offspring functionality would be implemented here");
            }
        }
        
        private void OnExportResultsClicked()
        {
            if (_lastBreedingResult != null)
            {
                // Implementation would export breeding results
                UpdateBreedingStatus("Breeding results exported");
                Debug.Log("Export results functionality would be implemented here");
            }
        }
        
        private void OnAdvancedOptionsToggleChanged(bool value)
        {
            if (_advancedOptionsSection != null)
            {
                _advancedOptionsSection.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        
        private void OnBreedingStrategyChanged(string value)
        {
            // Implementation would adjust breeding parameters based on strategy
            Debug.Log($"Breeding strategy changed to: {value}");
        }
        
        private void OnParentalScreeningToggleChanged(bool value)
        {
            _fitnessThresholdSlider?.SetEnabled(value);
        }
        
        private void OnFitnessThresholdChanged(float value)
        {
            UpdateFitnessThresholdLabel(value);
        }
        
        private void OnOffspringSelectionChanged(IEnumerable<object> selectedItems)
        {
            var selected = selectedItems?.FirstOrDefault() as PlantGenotype;
            if (selected != null)
            {
                UpdateBreedingStatus($"Selected offspring: {selected.GenotypeID}");
            }
        }
        
        // UI Helper Methods
        
        private VisualElement CreateOffspringListItem()
        {
            var item = new VisualElement();
            item.style.flexDirection = FlexDirection.Row;
            item.style.justifyContent = Justify.SpaceBetween;
            item.style.paddingBottom = 5;
            item.style.paddingTop = 5;
            item.style.paddingLeft = 8;
            item.style.paddingRight = 8;
            
            var nameLabel = new Label();
            nameLabel.name = "offspring-name";
            nameLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
            nameLabel.style.fontSize = new Length(12, LengthUnit.Pixel);
            
            var statsLabel = new Label();
            statsLabel.name = "offspring-stats";
            statsLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            statsLabel.style.fontSize = new Length(10, LengthUnit.Pixel);
            
            item.Add(nameLabel);
            item.Add(statsLabel);
            
            return item;
        }
        
        private void BindOffspringListItem(VisualElement element, int index)
        {
            if (_lastBreedingResult?.OffspringGenotypes == null || index >= _lastBreedingResult.OffspringGenotypes.Count) 
                return;
            
            var offspring = _lastBreedingResult.OffspringGenotypes[index];
            var nameLabel = element.Q<Label>("offspring-name");
            var statsLabel = element.Q<Label>("offspring-stats");
            
            nameLabel.text = offspring.GenotypeID;
            statsLabel.text = $"Gen {offspring.Generation} - Fitness: {offspring.OverallFitness:F2} - Inbreeding: {offspring.InbreedingCoefficient:F2}";
            
            // Color code by fitness
            if (offspring.OverallFitness >= 0.8f)
                statsLabel.style.color = new Color(0.2f, 0.8f, 0.2f);
            else if (offspring.OverallFitness >= 0.6f)
                statsLabel.style.color = new Color(0.8f, 0.8f, 0.2f);
            else
                statsLabel.style.color = new Color(0.8f, 0.2f, 0.2f);
        }
        
        private void UpdateMutationRateLabel(float value)
        {
            if (_mutationRateLabel != null) _mutationRateLabel.text = $"Mutation Rate: {value:F1}x";
        }
        
        private void UpdateInbreedingDepressionLabel(float value)
        {
            if (_inbreedingDepressionLabel != null) _inbreedingDepressionLabel.text = $"Inbreeding Depression: {value:F1}%";
        }
        
        private void UpdateFitnessThresholdLabel(float value)
        {
            if (_fitnessThresholdLabel != null) _fitnessThresholdLabel.text = $"Fitness Threshold: {value:F1}";
        }
        
        private void UpdateBreedingStatus(string status)
        {
            if (_breedingStatusLabel != null) _breedingStatusLabel.text = status;
            Debug.Log($"BreedingUI: {status}");
        }
        
        // Public API
        
        public void SelectParents(PlantGenotype parent1, PlantGenotype parent2)
        {
            _selectedParent1 = parent1;
            _selectedParent2 = parent2;
            
            // Update dropdown selections
            if (_availableParents.Contains(parent1) && _availableParents.Contains(parent2))
            {
                var index1 = _availableParents.IndexOf(parent1) + 1; // Add 1 for "Select Parent..." option
                var index2 = _availableParents.IndexOf(parent2) + 1;
                
                if (_parent1Dropdown != null && index1 < _parent1Dropdown.choices.Count)
                    _parent1Dropdown.value = _parent1Dropdown.choices[index1];
                
                if (_parent2Dropdown != null && index2 < _parent2Dropdown.choices.Count)
                    _parent2Dropdown.value = _parent2Dropdown.choices[index2];
            }
            
            UpdateSelectionState();
        }
        
        public BreedingResult GetLastBreedingResult()
        {
            return _lastBreedingResult;
        }
        
        public void RefreshAvailableParents()
        {
            LoadAvailableParents();
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
    }
    
    // Extension method for PlantInstanceSO
    public static class PlantInstanceExtensions
    {
        public static bool IsBreedingEligible(this PlantInstanceSO plant)
        {
            return plant != null && 
                   plant.Genotype != null && 
                   plant.OverallHealth >= 0.5f && 
                   plant.AgeInDays >= 30f; // Minimum age for breeding
        }
        
        public static PlantGenotype GetPlantGenotype(this PlantInstanceSO plant)
        {
            if (plant?.Genotype == null) return null;
            
            return new PlantGenotype
            {
                GenotypeID = plant.PlantID,
                StrainOrigin = plant.Strain,
                Generation = 1, // Would be calculated from breeding history
                IsFounder = true, // Would be determined from breeding history
                CreationDate = DateTime.Now,
                ParentIDs = new List<string>(),
                Genotype = ConvertGenotypeDataToGenotype(plant.Genotype),
                OverallFitness = plant.Genotype.OverallFitness,
                InbreedingCoefficient = 0f, // Would be calculated
                Mutations = new List<MutationRecord>()
            };
        }
        
        private static Dictionary<string, AlleleCouple> ConvertGenotypeDataToGenotype(GenotypeDataSO genotypeData)
        {
            var genotype = new Dictionary<string, AlleleCouple>();
            
            if (genotypeData.GenePairs != null)
            {
                foreach (var genePair in genotypeData.GenePairs)
                {
                    if (genePair?.Gene != null)
                    {
                        var alleleCouple = new AlleleCouple(genePair.Allele1, genePair.Allele2);
                        genotype[genePair.Gene.GeneCode] = alleleCouple;
                    }
                }
            }
            
            return genotype;
        }
    }
}