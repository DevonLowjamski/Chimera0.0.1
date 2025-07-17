using UnityEngine;
using UnityEngine.UIElements;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Systems.Genetics;
using ProjectChimera.Systems.Cultivation;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ProjectChimera.UI
{
    /// <summary>
    /// Phase 3.3: Advanced Breeding UI with sophisticated genetic visualization and prediction tools.
    /// Enhances the existing breeding system with pedigree charts, trait prediction, and breeding goals.
    /// </summary>
    public class AdvancedBreedingUI : MonoBehaviour
    {
        [Header("UI Document References")]
        [SerializeField] private UIDocument _uiDocument;
        
        [Header("Breeding System References")]
        [SerializeField] private BreedingSimulator _breedingSimulator;
        [SerializeField] private ProjectChimera.Systems.Genetics.TraitExpressionEngine _traitEngine;
        [SerializeField] private PlantManager _plantManager;
        
        [Header("Advanced UI Settings")]
        [SerializeField] private float _predictionUpdateInterval = 0.5f;
        [SerializeField] private int _maxPedigreeGenerations = 5;
        [SerializeField] private bool _enableRealTimePrediction = true;
        [SerializeField] private bool _enableAdvancedFiltering = true;
        
        // UI Element References
        private VisualElement _rootElement;
        private VisualElement _parentSelectionContainer;
        private VisualElement _traitPredictionContainer;
        private VisualElement _pedigreeChartContainer;
        private VisualElement _breedingGoalsContainer;
        private VisualElement _geneticDiversityContainer;
        
        // Parent Selection Elements
        private DropdownField _parent1Dropdown;
        private DropdownField _parent2Dropdown;
        private Button _swapParentsButton;
        private Button _clearSelectionButton;
        
        // Trait Prediction Elements
        private VisualElement _punnettSquareContainer;
        private Label _inheritanceProbabilityLabel;
        private ProgressBar _thcPredictionBar;
        private ProgressBar _cbdPredictionBar;
        private ProgressBar _yieldPredictionBar;
        private ProgressBar _heightPredictionBar;
        
        // Breeding Goals Elements
        private Toggle _maximizeTHCToggle;
        private Toggle _maximizeCBDToggle;
        private Toggle _maximizeYieldToggle;
        private Toggle _balancedTraitsToggle;
        private Slider _targetTHCSlider;
        private Slider _targetCBDSlider;
        
        // Genetic Analysis Elements
        private Label _geneticDistanceLabel;
        private Label _inbreedingRiskLabel;
        private Label _heterosisLabel;
        private ProgressBar _geneticDiversityBar;
        
        // Data Management
        private PlantStrainSO _selectedParent1;
        private PlantStrainSO _selectedParent2;
        private List<PlantStrainSO> _availableStrains = new List<PlantStrainSO>();
        private BreedingPredictionResult _currentPrediction;
        private float _lastPredictionUpdate;
        
        // Events
        public event Action<PlantStrainSO, PlantStrainSO> OnParentsSelected;
        public event Action<BreedingPredictionResult> OnPredictionUpdated;
        public event Action<BreedingGoalConfiguration> OnBreedingGoalsChanged;
        
        private void Awake()
        {
            InitializeUI();
            LoadAvailableStrains();
        }
        
        private void Start()
        {
            SetupEventListeners();
            RefreshUI();
        }
        
        private void Update()
        {
            if (_enableRealTimePrediction && Time.time - _lastPredictionUpdate >= _predictionUpdateInterval)
            {
                UpdateTraitPredictions();
                _lastPredictionUpdate = Time.time;
            }
        }
        
        /// <summary>
        /// Initialize UI elements and references.
        /// </summary>
        private void InitializeUI()
        {
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();
            
            _rootElement = _uiDocument.rootVisualElement;
            
            // Get main container references
            _parentSelectionContainer = _rootElement.Q<VisualElement>("parent-selection-container");
            _traitPredictionContainer = _rootElement.Q<VisualElement>("trait-prediction-container");
            _pedigreeChartContainer = _rootElement.Q<VisualElement>("pedigree-chart-container");
            _breedingGoalsContainer = _rootElement.Q<VisualElement>("breeding-goals-container");
            _geneticDiversityContainer = _rootElement.Q<VisualElement>("genetic-diversity-container");
            
            // Get parent selection elements
            _parent1Dropdown = _rootElement.Q<DropdownField>("parent1-dropdown");
            _parent2Dropdown = _rootElement.Q<DropdownField>("parent2-dropdown");
            _swapParentsButton = _rootElement.Q<Button>("swap-parents-button");
            _clearSelectionButton = _rootElement.Q<Button>("clear-selection-button");
            
            // Get trait prediction elements
            _punnettSquareContainer = _rootElement.Q<VisualElement>("punnett-square-container");
            _inheritanceProbabilityLabel = _rootElement.Q<Label>("inheritance-probability-label");
            _thcPredictionBar = _rootElement.Q<ProgressBar>("thc-prediction-bar");
            _cbdPredictionBar = _rootElement.Q<ProgressBar>("cbd-prediction-bar");
            _yieldPredictionBar = _rootElement.Q<ProgressBar>("yield-prediction-bar");
            _heightPredictionBar = _rootElement.Q<ProgressBar>("height-prediction-bar");
            
            // Get breeding goals elements
            _maximizeTHCToggle = _rootElement.Q<Toggle>("maximize-thc-toggle");
            _maximizeCBDToggle = _rootElement.Q<Toggle>("maximize-cbd-toggle");
            _maximizeYieldToggle = _rootElement.Q<Toggle>("maximize-yield-toggle");
            _balancedTraitsToggle = _rootElement.Q<Toggle>("balanced-traits-toggle");
            _targetTHCSlider = _rootElement.Q<Slider>("target-thc-slider");
            _targetCBDSlider = _rootElement.Q<Slider>("target-cbd-slider");
            
            // Get genetic analysis elements
            _geneticDistanceLabel = _rootElement.Q<Label>("genetic-distance-label");
            _inbreedingRiskLabel = _rootElement.Q<Label>("inbreeding-risk-label");
            _heterosisLabel = _rootElement.Q<Label>("heterosis-label");
            _geneticDiversityBar = _rootElement.Q<ProgressBar>("genetic-diversity-bar");
            
            Debug.Log("✅ Advanced Breeding UI initialized successfully");
        }
        
        /// <summary>
        /// Set up event listeners for UI interactions.
        /// </summary>
        private void SetupEventListeners()
        {
            // Parent selection events
            if (_parent1Dropdown != null)
                _parent1Dropdown.RegisterValueChangedCallback(OnParent1Changed);
            
            if (_parent2Dropdown != null)
                _parent2Dropdown.RegisterValueChangedCallback(OnParent2Changed);
            
            if (_swapParentsButton != null)
                _swapParentsButton.clicked += SwapParents;
            
            if (_clearSelectionButton != null)
                _clearSelectionButton.clicked += ClearSelection;
            
            // Breeding goals events
            if (_maximizeTHCToggle != null)
                _maximizeTHCToggle.RegisterValueChangedCallback(OnBreedingGoalChanged);
            
            if (_maximizeCBDToggle != null)
                _maximizeCBDToggle.RegisterValueChangedCallback(OnBreedingGoalChanged);
            
            if (_maximizeYieldToggle != null)
                _maximizeYieldToggle.RegisterValueChangedCallback(OnBreedingGoalChanged);
            
            if (_balancedTraitsToggle != null)
                _balancedTraitsToggle.RegisterValueChangedCallback(OnBreedingGoalChanged);
            
            if (_targetTHCSlider != null)
                _targetTHCSlider.RegisterValueChangedCallback(OnTargetValueChanged);
            
            if (_targetCBDSlider != null)
                _targetCBDSlider.RegisterValueChangedCallback(OnTargetValueChanged);
            
            Debug.Log("✅ Advanced Breeding UI event listeners set up");
        }
        
        /// <summary>
        /// Load available plant strains for breeding selection.
        /// </summary>
        private void LoadAvailableStrains()
        {
            _availableStrains.Clear();
            
            // Get strains from plant manager if available
            if (_plantManager != null)
            {
                var managedPlants = _plantManager.GetAllPlants();
                foreach (var plant in managedPlants)
                {
                    if (plant.Strain != null && !_availableStrains.Contains(plant.Strain))
                    {
                        _availableStrains.Add(plant.Strain);
                    }
                }
            }
            
            // Load additional strains from resources
            var additionalStrains = Resources.LoadAll<PlantStrainSO>("Genetics/Strains");
            foreach (var strain in additionalStrains)
            {
                if (!_availableStrains.Contains(strain))
                {
                    _availableStrains.Add(strain);
                }
            }
            
            UpdateDropdownChoices();
            Debug.Log($"✅ Loaded {_availableStrains.Count} available strains for breeding");
        }
        
        /// <summary>
        /// Update dropdown choices with available strains.
        /// </summary>
        private void UpdateDropdownChoices()
        {
            var strainNames = _availableStrains.Select(s => s.StrainName).ToList();
            strainNames.Insert(0, "Select Parent...");
            
            if (_parent1Dropdown != null)
            {
                _parent1Dropdown.choices = strainNames;
                _parent1Dropdown.index = 0;
            }
            
            if (_parent2Dropdown != null)
            {
                _parent2Dropdown.choices = strainNames;
                _parent2Dropdown.index = 0;
            }
        }
        
        /// <summary>
        /// Handle parent 1 selection change.
        /// </summary>
        private void OnParent1Changed(ChangeEvent<string> evt)
        {
            var selectedStrain = _availableStrains.FirstOrDefault(s => s.StrainName == evt.newValue);
            _selectedParent1 = selectedStrain;
            
            UpdateParentAnalysis();
            UpdateTraitPredictions();
            UpdatePedigreeChart();
            
            OnParentsSelected?.Invoke(_selectedParent1, _selectedParent2);
        }
        
        /// <summary>
        /// Handle parent 2 selection change.
        /// </summary>
        private void OnParent2Changed(ChangeEvent<string> evt)
        {
            var selectedStrain = _availableStrains.FirstOrDefault(s => s.StrainName == evt.newValue);
            _selectedParent2 = selectedStrain;
            
            UpdateParentAnalysis();
            UpdateTraitPredictions();
            UpdatePedigreeChart();
            
            OnParentsSelected?.Invoke(_selectedParent1, _selectedParent2);
        }
        
        /// <summary>
        /// Swap the selected parents.
        /// </summary>
        private void SwapParents()
        {
            if (_selectedParent1 != null && _selectedParent2 != null)
            {
                var temp = _selectedParent1;
                _selectedParent1 = _selectedParent2;
                _selectedParent2 = temp;
                
                // Update UI to reflect the swap
                if (_parent1Dropdown != null && _parent2Dropdown != null)
                {
                    var parent1Index = _parent1Dropdown.index;
                    var parent2Index = _parent2Dropdown.index;
                    
                    _parent1Dropdown.index = parent2Index;
                    _parent2Dropdown.index = parent1Index;
                }
                
                UpdateParentAnalysis();
                UpdateTraitPredictions();
                
                Debug.Log($"✅ Swapped parents: {_selectedParent1?.StrainName} ↔ {_selectedParent2?.StrainName}");
            }
        }
        
        /// <summary>
        /// Clear parent selection.
        /// </summary>
        private void ClearSelection()
        {
            _selectedParent1 = null;
            _selectedParent2 = null;
            
            if (_parent1Dropdown != null)
                _parent1Dropdown.index = 0;
            
            if (_parent2Dropdown != null)
                _parent2Dropdown.index = 0;
            
            ClearPredictions();
            ClearPedigreeChart();
            
            Debug.Log("✅ Cleared parent selection");
        }
        
        /// <summary>
        /// Update genetic analysis between selected parents.
        /// </summary>
        private void UpdateParentAnalysis()
        {
            if (_selectedParent1 == null || _selectedParent2 == null)
            {
                ClearGeneticAnalysis();
                return;
            }
            
            if (_breedingSimulator != null)
            {
                var compatibility = _breedingSimulator.AnalyzeBreedingCompatibility(_selectedParent1, _selectedParent2);
                
                // Update genetic distance
                if (_geneticDistanceLabel != null)
                    _geneticDistanceLabel.text = $"Genetic Distance: {compatibility.GeneticDistance:F3}";
                
                // Update inbreeding risk
                if (_inbreedingRiskLabel != null)
                {
                    var risk = compatibility.InbreedingRisk;
                    var riskText = risk < 0.1f ? "Low" : risk < 0.3f ? "Moderate" : "High";
                    _inbreedingRiskLabel.text = $"Inbreeding Risk: {riskText} ({risk:P1})";
                }
                
                // Update heterosis prediction
                if (_heterosisLabel != null)
                {
                    var heterosis = compatibility.ExpectedHeterosis;
                    _heterosisLabel.text = $"Hybrid Vigor: {heterosis:F2}x";
                }
                
                // Update genetic diversity bar
                if (_geneticDiversityBar != null)
                {
                    var diversity = 1.0f - compatibility.InbreedingRisk;
                    _geneticDiversityBar.value = diversity * 100f;
                    _geneticDiversityBar.title = $"Genetic Diversity: {diversity:P1}";
                }
            }
        }
        
        /// <summary>
        /// Update trait prediction visualizations.
        /// </summary>
        private void UpdateTraitPredictions()
        {
            if (_selectedParent1 == null || _selectedParent2 == null)
            {
                ClearPredictions();
                return;
            }
            
            if (_breedingSimulator != null && _traitEngine != null)
            {
                var prediction = PredictOffspringTraits(_selectedParent1, _selectedParent2);
                _currentPrediction = prediction;
                
                // Update trait prediction bars
                if (_thcPredictionBar != null)
                {
                    _thcPredictionBar.value = prediction.ExpectedTHC * 100f;
                    _thcPredictionBar.title = $"THC: {prediction.ExpectedTHC:P1} ± {prediction.THCVariance:P1}";
                }
                
                if (_cbdPredictionBar != null)
                {
                    _cbdPredictionBar.value = prediction.ExpectedCBD * 100f;
                    _cbdPredictionBar.title = $"CBD: {prediction.ExpectedCBD:P1} ± {prediction.CBDVariance:P1}";
                }
                
                if (_yieldPredictionBar != null)
                {
                    _yieldPredictionBar.value = prediction.ExpectedYield * 100f;
                    _yieldPredictionBar.title = $"Yield: {prediction.ExpectedYield:P1} ± {prediction.YieldVariance:P1}";
                }
                
                if (_heightPredictionBar != null)
                {
                    _heightPredictionBar.value = prediction.ExpectedHeight * 100f;
                    _heightPredictionBar.title = $"Height: {prediction.ExpectedHeight:P1} ± {prediction.HeightVariance:P1}";
                }
                
                // Update probability text
                if (_inheritanceProbabilityLabel != null)
                {
                    _inheritanceProbabilityLabel.text = $"Success Probability: {prediction.SuccessProbability:P1}";
                }
                
                UpdatePunnettSquare(prediction);
                OnPredictionUpdated?.Invoke(prediction);
            }
        }
        
        /// <summary>
        /// Predict offspring traits using genetic simulation.
        /// </summary>
        private BreedingPredictionResult PredictOffspringTraits(PlantStrainSO parent1, PlantStrainSO parent2)
        {
            var prediction = new BreedingPredictionResult();
            
            // Simulate multiple offspring to get statistical predictions
            const int simulationCount = 100;
            var thcValues = new List<float>();
            var cbdValues = new List<float>();
            var yieldValues = new List<float>();
            var heightValues = new List<float>();
            
            for (int i = 0; i < simulationCount; i++)
            {
                // Create simulated offspring genotype
                var offspringGenotype = _breedingSimulator.SimulateOffspringGenotype(parent1, parent2);
                
                // Use trait expression engine to predict phenotype
                if (_traitEngine != null && offspringGenotype != null)
                {
                    var environmentalConditions = ProjectChimera.Data.Cultivation.EnvironmentalConditions.CreateIndoorDefault();
                    var expression = _traitEngine.CalculateTraitExpression(offspringGenotype, environmentalConditions);
                    
                    thcValues.Add(expression.THCExpression);
                    cbdValues.Add(expression.CBDExpression);
                    yieldValues.Add(expression.YieldExpression);
                    heightValues.Add(expression.HeightExpression);
                }
            }
            
            // Calculate statistics
            if (thcValues.Count > 0)
            {
                prediction.ExpectedTHC = thcValues.Average();
                prediction.THCVariance = CalculateVariance(thcValues);
                prediction.ExpectedCBD = cbdValues.Average();
                prediction.CBDVariance = CalculateVariance(cbdValues);
                prediction.ExpectedYield = yieldValues.Average();
                prediction.YieldVariance = CalculateVariance(yieldValues);
                prediction.ExpectedHeight = heightValues.Average();
                prediction.HeightVariance = CalculateVariance(heightValues);
                
                // Calculate overall success probability based on breeding goals
                prediction.SuccessProbability = CalculateSuccessProbability(prediction);
            }
            
            return prediction;
        }
        
        /// <summary>
        /// Calculate variance for a set of values.
        /// </summary>
        private float CalculateVariance(List<float> values)
        {
            if (values.Count < 2) return 0f;
            
            var mean = values.Average();
            var squaredDifferences = values.Select(v => (v - mean) * (v - mean));
            return (float)Math.Sqrt(squaredDifferences.Average());
        }
        
        /// <summary>
        /// Calculate success probability based on current breeding goals.
        /// </summary>
        private float CalculateSuccessProbability(BreedingPredictionResult prediction)
        {
            float probability = 1.0f;
            
            // Adjust probability based on breeding goals
            if (_maximizeTHCToggle != null && _maximizeTHCToggle.value)
            {
                probability *= prediction.ExpectedTHC;
            }
            
            if (_maximizeCBDToggle != null && _maximizeCBDToggle.value)
            {
                probability *= prediction.ExpectedCBD;
            }
            
            if (_maximizeYieldToggle != null && _maximizeYieldToggle.value)
            {
                probability *= prediction.ExpectedYield;
            }
            
            if (_balancedTraitsToggle != null && _balancedTraitsToggle.value)
            {
                // Balanced traits favor moderate values across all traits
                var balance = 1.0f - Math.Abs(prediction.ExpectedTHC - prediction.ExpectedCBD);
                probability *= balance;
            }
            
            return Mathf.Clamp01(probability);
        }
        
        /// <summary>
        /// Update Punnett square visualization.
        /// </summary>
        private void UpdatePunnettSquare(BreedingPredictionResult prediction)
        {
            if (_punnettSquareContainer == null) return;
            
            // Clear existing elements
            _punnettSquareContainer.Clear();
            
            // Create simplified Punnett square for dominant/recessive traits
            var punnettGrid = new VisualElement();
            punnettGrid.AddToClassList("punnett-grid");
            
            // Add header row
            var headerRow = new VisualElement();
            headerRow.AddToClassList("punnett-row");
            headerRow.Add(new Label("♀ \\ ♂"));
            headerRow.Add(new Label("A"));
            headerRow.Add(new Label("a"));
            punnettGrid.Add(headerRow);
            
            // Add data rows
            var row1 = new VisualElement();
            row1.AddToClassList("punnett-row");
            row1.Add(new Label("A"));
            row1.Add(CreatePunnettCell("AA", 0.25f));
            row1.Add(CreatePunnettCell("Aa", 0.25f));
            punnettGrid.Add(row1);
            
            var row2 = new VisualElement();
            row2.AddToClassList("punnett-row");
            row2.Add(new Label("a"));
            row2.Add(CreatePunnettCell("Aa", 0.25f));
            row2.Add(CreatePunnettCell("aa", 0.25f));
            punnettGrid.Add(row2);
            
            _punnettSquareContainer.Add(punnettGrid);
        }
        
        /// <summary>
        /// Create a Punnett square cell element.
        /// </summary>
        private VisualElement CreatePunnettCell(string genotype, float probability)
        {
            var cell = new VisualElement();
            cell.AddToClassList("punnett-cell");
            
            var genotypeLabel = new Label(genotype);
            genotypeLabel.AddToClassList("genotype-label");
            
            var probabilityLabel = new Label($"{probability:P0}");
            probabilityLabel.AddToClassList("probability-label");
            
            cell.Add(genotypeLabel);
            cell.Add(probabilityLabel);
            
            return cell;
        }
        
        /// <summary>
        /// Update pedigree chart visualization.
        /// </summary>
        private void UpdatePedigreeChart()
        {
            if (_pedigreeChartContainer == null) return;
            
            // Clear existing chart
            _pedigreeChartContainer.Clear();
            
            if (_selectedParent1 == null && _selectedParent2 == null) return;
            
            // Create pedigree visualization
            var pedigreeChart = new VisualElement();
            pedigreeChart.AddToClassList("pedigree-chart");
            
            // Add parent nodes
            if (_selectedParent1 != null)
            {
                var parent1Node = CreatePedigreeNode(_selectedParent1, "parent1");
                pedigreeChart.Add(parent1Node);
            }
            
            if (_selectedParent2 != null)
            {
                var parent2Node = CreatePedigreeNode(_selectedParent2, "parent2");
                pedigreeChart.Add(parent2Node);
            }
            
            // Add predicted offspring node
            if (_selectedParent1 != null && _selectedParent2 != null)
            {
                var offspringNode = CreateOffspringNode();
                pedigreeChart.Add(offspringNode);
            }
            
            _pedigreeChartContainer.Add(pedigreeChart);
        }
        
        /// <summary>
        /// Create a pedigree node for a plant strain.
        /// </summary>
        private VisualElement CreatePedigreeNode(PlantStrainSO strain, string nodeType)
        {
            var node = new VisualElement();
            node.AddToClassList("pedigree-node");
            node.AddToClassList(nodeType);
            
            var nameLabel = new Label(strain.StrainName);
            nameLabel.AddToClassList("strain-name");
            
            var typeLabel = new Label(strain.StrainType.ToString());
            typeLabel.AddToClassList("strain-type");
            
            node.Add(nameLabel);
            node.Add(typeLabel);
            
            return node;
        }
        
        /// <summary>
        /// Create a predicted offspring node.
        /// </summary>
        private VisualElement CreateOffspringNode()
        {
            var node = new VisualElement();
            node.AddToClassList("pedigree-node");
            node.AddToClassList("offspring");
            
            var nameLabel = new Label("Predicted Offspring");
            nameLabel.AddToClassList("strain-name");
            
            var probabilityLabel = new Label($"Success: {_currentPrediction?.SuccessProbability:P1}");
            probabilityLabel.AddToClassList("probability");
            
            node.Add(nameLabel);
            node.Add(probabilityLabel);
            
            return node;
        }
        
        /// <summary>
        /// Handle breeding goal changes.
        /// </summary>
        private void OnBreedingGoalChanged(ChangeEvent<bool> evt)
        {
            UpdateTraitPredictions();
            
            var goalConfig = GetCurrentBreedingGoals();
            OnBreedingGoalsChanged?.Invoke(goalConfig);
        }
        
        /// <summary>
        /// Handle target value changes.
        /// </summary>
        private void OnTargetValueChanged(ChangeEvent<float> evt)
        {
            UpdateTraitPredictions();
            
            var goalConfig = GetCurrentBreedingGoals();
            OnBreedingGoalsChanged?.Invoke(goalConfig);
        }
        
        /// <summary>
        /// Get current breeding goal configuration.
        /// </summary>
        private BreedingGoalConfiguration GetCurrentBreedingGoals()
        {
            return new BreedingGoalConfiguration
            {
                MaximizeTHC = _maximizeTHCToggle?.value ?? false,
                MaximizeCBD = _maximizeCBDToggle?.value ?? false,
                MaximizeYield = _maximizeYieldToggle?.value ?? false,
                BalancedTraits = _balancedTraitsToggle?.value ?? false,
                TargetTHC = _targetTHCSlider?.value ?? 0f,
                TargetCBD = _targetCBDSlider?.value ?? 0f
            };
        }
        
        /// <summary>
        /// Clear trait predictions.
        /// </summary>
        private void ClearPredictions()
        {
            if (_thcPredictionBar != null)
            {
                _thcPredictionBar.value = 0f;
                _thcPredictionBar.title = "THC: Select parents to predict";
            }
            
            if (_cbdPredictionBar != null)
            {
                _cbdPredictionBar.value = 0f;
                _cbdPredictionBar.title = "CBD: Select parents to predict";
            }
            
            if (_yieldPredictionBar != null)
            {
                _yieldPredictionBar.value = 0f;
                _yieldPredictionBar.title = "Yield: Select parents to predict";
            }
            
            if (_heightPredictionBar != null)
            {
                _heightPredictionBar.value = 0f;
                _heightPredictionBar.title = "Height: Select parents to predict";
            }
            
            if (_inheritanceProbabilityLabel != null)
                _inheritanceProbabilityLabel.text = "Select parents to calculate probability";
            
            if (_punnettSquareContainer != null)
                _punnettSquareContainer.Clear();
        }
        
        /// <summary>
        /// Clear genetic analysis displays.
        /// </summary>
        private void ClearGeneticAnalysis()
        {
            if (_geneticDistanceLabel != null)
                _geneticDistanceLabel.text = "Genetic Distance: -";
            
            if (_inbreedingRiskLabel != null)
                _inbreedingRiskLabel.text = "Inbreeding Risk: -";
            
            if (_heterosisLabel != null)
                _heterosisLabel.text = "Hybrid Vigor: -";
            
            if (_geneticDiversityBar != null)
            {
                _geneticDiversityBar.value = 0f;
                _geneticDiversityBar.title = "Genetic Diversity: Select parents";
            }
        }
        
        /// <summary>
        /// Clear pedigree chart.
        /// </summary>
        private void ClearPedigreeChart()
        {
            if (_pedigreeChartContainer != null)
                _pedigreeChartContainer.Clear();
        }
        
        /// <summary>
        /// Refresh the entire UI.
        /// </summary>
        private void RefreshUI()
        {
            LoadAvailableStrains();
            ClearSelection();
        }
        
        /// <summary>
        /// Get the current breeding prediction result.
        /// </summary>
        public BreedingPredictionResult GetCurrentPrediction()
        {
            return _currentPrediction;
        }
        
        /// <summary>
        /// Set breeding system references programmatically.
        /// </summary>
        public void SetBreedingSystemReferences(BreedingSimulator breedingSimulator, ProjectChimera.Systems.Genetics.TraitExpressionEngine traitEngine, PlantManager plantManager)
        {
            _breedingSimulator = breedingSimulator;
            _traitEngine = traitEngine;
            _plantManager = plantManager;
            
            LoadAvailableStrains();
        }
    }
    
    /// <summary>
    /// Data structure for breeding prediction results.
    /// </summary>
    [System.Serializable]
    public class BreedingPredictionResult
    {
        public float ExpectedTHC;
        public float THCVariance;
        public float ExpectedCBD;
        public float CBDVariance;
        public float ExpectedYield;
        public float YieldVariance;
        public float ExpectedHeight;
        public float HeightVariance;
        public float SuccessProbability;
        public DateTime PredictionTime = DateTime.Now;
    }
    
    /// <summary>
    /// Configuration for breeding goals and targets.
    /// </summary>
    [System.Serializable]
    public class BreedingGoalConfiguration
    {
        public bool MaximizeTHC;
        public bool MaximizeCBD;
        public bool MaximizeYield;
        public bool BalancedTraits;
        public float TargetTHC;
        public float TargetCBD;
    }
}