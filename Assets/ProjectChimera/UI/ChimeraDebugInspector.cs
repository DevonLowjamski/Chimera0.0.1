using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Core.Logging;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Systems.Cultivation;
using ProjectChimera.Systems.Environment;

namespace ProjectChimera.UI
{
    /// <summary>
    /// Critical Debug Inspector for Project Chimera development.
    /// Provides real-time plant data visualization, environmental monitoring,
    /// and time control for development workflow.
    /// 
    /// This is the "Single most critical prerequisite for all future work" 
    /// as identified in the Implementation Roadmap Phase 0.2.
    /// </summary>
    public class ChimeraDebugInspector : MonoBehaviour
    {
        [Header("Debug Inspector Configuration")]
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private bool _enableOnStart = false;
        [SerializeField] private KeyCode _toggleKey = KeyCode.F12;
        [SerializeField] private float _updateInterval = 0.5f;
        
        [Header("Manager References")]
        [SerializeField] private PlantManager _plantManager;
        [SerializeField] private EnvironmentalManager _environmentalManager;
        [SerializeField] private TimeManager _timeManager;
        [SerializeField] private GameManager _gameManager;
        
        // UI Elements
        private VisualElement _rootElement;
        private DropdownField _plantDropdown;
        private Button _pauseButton;
        private Button _playButton;
        private Button _fastButton;
        private Button _refreshPlantsButton;
        private Button _closeButton;
        
        // Plant Data Labels
        private Label _plantIdLabel;
        private Label _plantStrainLabel;
        private Label _plantAgeLabel;
        private Label _plantStageLabel;
        private Label _plantHealthLabel;
        private Label _plantStressLabel;
        private Label _plantWaterLabel;
        private Label _plantNutrientsLabel;
        
        // Phenotype Labels
        private Label _phenotypeHeightLabel;
        private Label _phenotypeThcLabel;
        private Label _phenotypeCbdLabel;
        private Label _phenotypeYieldLabel;
        private Label _phenotypeFloweringLabel;
        
        // Genotype Labels
        private Label _genotypeProfileLabel;
        private Label _genotypeDominantLabel;
        private Label _genotypeRecessiveLabel;
        
        // Environmental Labels
        private Label _envTemperatureLabel;
        private Label _envHumidityLabel;
        private Label _envLightLabel;
        private Label _envCo2Label;
        private Label _envAirflowLabel;
        
        // GxE Labels
        private Label _gxeExpressionLabel;
        private Label _gxeStressLabel;
        private Label _gxeAdaptationLabel;
        
        // Internal State
        private bool _isVisible = false;
        private ProjectChimera.Data.Cultivation.PlantInstanceSO _selectedPlant;
        private List<ProjectChimera.Data.Cultivation.PlantInstanceSO> _availablePlants = new List<ProjectChimera.Data.Cultivation.PlantInstanceSO>();
        private float _lastUpdateTime;
        
        private void Start()
        {
            InitializeDebugInspector();
            
            if (_enableOnStart)
            {
                ShowInspector();
            }
        }
        
        private void InitializeDebugInspector()
        {
            // Get manager references if not set
            if (_gameManager == null)
                _gameManager = GameManager.Instance;
            
            if (_plantManager == null)
                _plantManager = _gameManager?.GetManager<PlantManager>();
            
            if (_environmentalManager == null)
                _environmentalManager = _gameManager?.GetManager<EnvironmentalManager>();
            
            if (_timeManager == null)
                _timeManager = _gameManager?.GetManager<TimeManager>();
            
            // Setup UI Document
            if (_uiDocument == null)
                _uiDocument = GetComponent<UIDocument>();
            
            if (_uiDocument != null)
            {
                SetupUIElements();
                SetupEventHandlers();
            }
            
            ChimeraLogger.Log("UI", "Chimera Debug Inspector initialized. Press F12 to toggle.", this);
        }
        
        private void SetupUIElements()
        {
            var root = _uiDocument.rootVisualElement;
            _rootElement = root.Q<VisualElement>("debug-inspector-root");
            
            // Time Controls
            _pauseButton = root.Q<Button>("pause-btn");
            _playButton = root.Q<Button>("play-btn");
            _fastButton = root.Q<Button>("fast-btn");
            
            // Plant Selection
            _plantDropdown = root.Q<DropdownField>("plant-dropdown");
            _refreshPlantsButton = root.Q<Button>("refresh-plants-btn");
            
            // Plant Data Labels
            _plantIdLabel = root.Q<Label>("plant-id-label");
            _plantStrainLabel = root.Q<Label>("plant-strain-label");
            _plantAgeLabel = root.Q<Label>("plant-age-label");
            _plantStageLabel = root.Q<Label>("plant-stage-label");
            _plantHealthLabel = root.Q<Label>("plant-health-label");
            _plantStressLabel = root.Q<Label>("plant-stress-label");
            _plantWaterLabel = root.Q<Label>("plant-water-label");
            _plantNutrientsLabel = root.Q<Label>("plant-nutrients-label");
            
            // Phenotype Labels
            _phenotypeHeightLabel = root.Q<Label>("phenotype-height-label");
            _phenotypeThcLabel = root.Q<Label>("phenotype-thc-label");
            _phenotypeCbdLabel = root.Q<Label>("phenotype-cbd-label");
            _phenotypeYieldLabel = root.Q<Label>("phenotype-yield-label");
            _phenotypeFloweringLabel = root.Q<Label>("phenotype-flowering-label");
            
            // Genotype Labels
            _genotypeProfileLabel = root.Q<Label>("genotype-profile-label");
            _genotypeDominantLabel = root.Q<Label>("genotype-dominant-label");
            _genotypeRecessiveLabel = root.Q<Label>("genotype-recessive-label");
            
            // Environmental Labels
            _envTemperatureLabel = root.Q<Label>("env-temperature-label");
            _envHumidityLabel = root.Q<Label>("env-humidity-label");
            _envLightLabel = root.Q<Label>("env-light-label");
            _envCo2Label = root.Q<Label>("env-co2-label");
            _envAirflowLabel = root.Q<Label>("env-airflow-label");
            
            // GxE Labels
            _gxeExpressionLabel = root.Q<Label>("gxe-expression-label");
            _gxeStressLabel = root.Q<Label>("gxe-stress-label");
            _gxeAdaptationLabel = root.Q<Label>("gxe-adaptation-label");
            
            // Close Button
            _closeButton = root.Q<Button>("close-btn");
        }
        
        private void SetupEventHandlers()
        {
            // Time Control Buttons
            _pauseButton?.RegisterCallback<ClickEvent>(evt => SetTimeScale(0f));
            _playButton?.RegisterCallback<ClickEvent>(evt => SetTimeScale(1f));
            _fastButton?.RegisterCallback<ClickEvent>(evt => SetTimeScale(3f));
            
            // Plant Selection
            _plantDropdown?.RegisterCallback<ChangeEvent<string>>(OnPlantSelected);
            _refreshPlantsButton?.RegisterCallback<ClickEvent>(evt => RefreshPlantList());
            
            // Close Button
            _closeButton?.RegisterCallback<ClickEvent>(evt => HideInspector());
        }
        
        private void Update()
        {
            // Handle toggle key
            if (Input.GetKeyDown(_toggleKey))
            {
                ToggleInspector();
            }
            
            // Update data if inspector is visible
            if (_isVisible && Time.time - _lastUpdateTime > _updateInterval)
            {
                UpdateInspectorData();
                _lastUpdateTime = Time.time;
            }
        }
        
        private void SetTimeScale(float scale)
        {
            if (_timeManager != null)
            {
                _timeManager.SetTimeScale(scale);
                ChimeraLogger.Log("UI", $"Time scale set to: {scale}x", this);
            }
            else
            {
                Time.timeScale = scale;
                ChimeraLogger.Log("UI", $"Unity time scale set to: {scale}x", this);
            }
        }
        
        private void ToggleInspector()
        {
            if (_isVisible)
            {
                HideInspector();
            }
            else
            {
                ShowInspector();
            }
        }
        
        private void ShowInspector()
        {
            _isVisible = true;
            if (_rootElement != null)
            {
                _rootElement.style.display = DisplayStyle.Flex;
            }
            
            RefreshPlantList();
            UpdateInspectorData();
            
            ChimeraLogger.Log("UI", "Debug Inspector shown", this);
        }
        
        private void HideInspector()
        {
            _isVisible = false;
            if (_rootElement != null)
            {
                _rootElement.style.display = DisplayStyle.None;
            }
            
            ChimeraLogger.Log("UI", "Debug Inspector hidden", this);
        }
        
        private void RefreshPlantList()
        {
            _availablePlants.Clear();
            
            if (_plantManager != null)
            {
                var plants = _plantManager.GetAllPlants();
                if (plants != null)
                {
                    _availablePlants.AddRange(plants);
                }
            }
            
            // Update dropdown choices
            if (_plantDropdown != null)
            {
                var choices = _availablePlants.Select(p => $"{p.PlantID} - {p.Strain?.name ?? "Unknown"}").ToList();
                
                if (choices.Count == 0)
                {
                    choices.Add("No plants available");
                }
                
                _plantDropdown.choices = choices;
                
                if (_availablePlants.Count > 0 && _selectedPlant == null)
                {
                    _plantDropdown.index = 0;
                    _selectedPlant = _availablePlants[0];
                }
            }
            
            ChimeraLogger.Log("UI", $"Found {_availablePlants.Count} plants", this);
        }
        
        private void OnPlantSelected(ChangeEvent<string> evt)
        {
            if (_plantDropdown.index >= 0 && _plantDropdown.index < _availablePlants.Count)
            {
                _selectedPlant = _availablePlants[_plantDropdown.index];
                ChimeraLogger.Log("UI", $"Selected plant: {_selectedPlant.PlantID}", this);
                UpdateInspectorData();
            }
        }
        
        private void UpdateInspectorData()
        {
            if (_selectedPlant == null)
            {
                UpdateLabelsWithNoPlant();
                return;
            }
            
            UpdateBasicInfo();
            UpdateHealthStatus();
            UpdatePhenotypeData();
            UpdateGenotypeData();
            UpdateEnvironmentalData();
            UpdateGxEData();
        }
        
        private void UpdateLabelsWithNoPlant()
        {
            if (_plantIdLabel != null) _plantIdLabel.text = "ID: No plant selected";
            if (_plantStrainLabel != null) _plantStrainLabel.text = "Strain: None";
            if (_plantAgeLabel != null) _plantAgeLabel.text = "Age: 0 days";
            if (_plantStageLabel != null) _plantStageLabel.text = "Stage: None";
            if (_plantHealthLabel != null) _plantHealthLabel.text = "Health: N/A";
            if (_plantStressLabel != null) _plantStressLabel.text = "Stress: N/A";
            if (_plantWaterLabel != null) _plantWaterLabel.text = "Water: N/A";
            if (_plantNutrientsLabel != null) _plantNutrientsLabel.text = "Nutrients: N/A";
        }
        
        private void UpdateBasicInfo()
        {
            if (_plantIdLabel != null) _plantIdLabel.text = $"ID: {_selectedPlant.PlantID}";
            if (_plantStrainLabel != null) _plantStrainLabel.text = $"Strain: {_selectedPlant.Strain?.name ?? "Unknown"}";
            if (_plantAgeLabel != null) _plantAgeLabel.text = $"Age: N/A"; // Age not available in PlantInstance
            if (_plantStageLabel != null) _plantStageLabel.text = $"Stage: {_selectedPlant.CurrentGrowthStage}";
        }
        
        private void UpdateHealthStatus()
        {
            var health = _selectedPlant.OverallHealth * 100f;
            var stress = _selectedPlant.StressLevel * 100f;
            
            if (_plantHealthLabel != null) _plantHealthLabel.text = $"Health: {health:F1}%";
            if (_plantStressLabel != null) _plantStressLabel.text = $"Stress: {stress:F1}%";
            if (_plantWaterLabel != null) _plantWaterLabel.text = "Water: N/A"; // Not available in PlantInstance
            if (_plantNutrientsLabel != null) _plantNutrientsLabel.text = "Nutrients: N/A"; // Not available in PlantInstance
        }
        
        private void UpdatePhenotypeData()
        {
            if (_selectedPlant.LastTraitExpression != null)
            {
                var traits = _selectedPlant.LastTraitExpression;
                if (_phenotypeHeightLabel != null) _phenotypeHeightLabel.text = "Height: N/A"; // Height not available in PlantInstance
                if (_phenotypeThcLabel != null) _phenotypeThcLabel.text = "THC: N/A"; // Would need trait expression data
                if (_phenotypeCbdLabel != null) _phenotypeCbdLabel.text = "CBD: N/A"; // Would need trait expression data
                if (_phenotypeYieldLabel != null) _phenotypeYieldLabel.text = $"Yield: {_selectedPlant.CalculateYieldPotential():F1}%";
                if (_phenotypeFloweringLabel != null) _phenotypeFloweringLabel.text = "Flowering: N/A"; // Would need trait expression data
            }
            else
            {
                if (_phenotypeHeightLabel != null) _phenotypeHeightLabel.text = "Height: N/A";
                if (_phenotypeThcLabel != null) _phenotypeThcLabel.text = "THC: N/A";
                if (_phenotypeCbdLabel != null) _phenotypeCbdLabel.text = "CBD: N/A";
                if (_phenotypeYieldLabel != null) _phenotypeYieldLabel.text = $"Yield: {_selectedPlant.CalculateYieldPotential():F1}%";
                if (_phenotypeFloweringLabel != null) _phenotypeFloweringLabel.text = "Flowering: N/A";
            }
        }
        
        private void UpdateGenotypeData()
        {
            if (_selectedPlant.Strain != null)
            {
                var strain = _selectedPlant.Strain;
                if (_genotypeProfileLabel != null) _genotypeProfileLabel.text = $"Profile: {strain.name}";
                if (_genotypeDominantLabel != null) _genotypeDominantLabel.text = "Dominant: N/A"; // Would need genotype analysis
                if (_genotypeRecessiveLabel != null) _genotypeRecessiveLabel.text = "Recessive: N/A"; // Would need genotype analysis
            }
            else
            {
                if (_genotypeProfileLabel != null) _genotypeProfileLabel.text = "Profile: N/A";
                if (_genotypeDominantLabel != null) _genotypeDominantLabel.text = "Dominant: N/A";
                if (_genotypeRecessiveLabel != null) _genotypeRecessiveLabel.text = "Recessive: N/A";
            }
        }
        
        private void UpdateEnvironmentalData()
        {
            if (_environmentalManager != null)
            {
                // PlantInstance doesn't have WorldPosition, use Vector3.zero as fallback
                var conditions = _environmentalManager.GetEnvironmentalConditions(Vector3.zero);
                
                if (_envTemperatureLabel != null) _envTemperatureLabel.text = $"Temperature: {conditions.Temperature:F1}°C";
                if (_envHumidityLabel != null) _envHumidityLabel.text = $"Humidity: {conditions.Humidity:F1}%";
                if (_envLightLabel != null) _envLightLabel.text = $"Light: {conditions.LightIntensity:F0} μmol/m²/s";
                if (_envCo2Label != null) _envCo2Label.text = $"CO2: {conditions.CO2Level:F0} ppm";
                if (_envAirflowLabel != null) _envAirflowLabel.text = $"Airflow: {conditions.AirVelocity:F1} m/s";
            }
            else
            {
                if (_envTemperatureLabel != null) _envTemperatureLabel.text = "Temperature: N/A";
                if (_envHumidityLabel != null) _envHumidityLabel.text = "Humidity: N/A";
                if (_envLightLabel != null) _envLightLabel.text = "Light: N/A";
                if (_envCo2Label != null) _envCo2Label.text = "CO2: N/A";
                if (_envAirflowLabel != null) _envAirflowLabel.text = "Airflow: N/A";
            }
        }
        
        private void UpdateGxEData()
        {
            // GxE (Genotype × Environment) interaction data
            if (_selectedPlant.LastTraitExpression != null && _environmentalManager != null)
            {
                var expressionModifier = CalculateExpressionModifier();
                var stressResponse = _selectedPlant.StressLevel > 0.1f ? "Active" : "Normal";
                var adaptationLevel = 50f; // Placeholder since ImmuneResponse not available in PlantInstance
                
                if (_gxeExpressionLabel != null) _gxeExpressionLabel.text = $"Expression Modifier: {expressionModifier:F2}x";
                if (_gxeStressLabel != null) _gxeStressLabel.text = $"Stress Response: {stressResponse}";
                if (_gxeAdaptationLabel != null) _gxeAdaptationLabel.text = $"Adaptation Level: {adaptationLevel:F1}%";
            }
            else
            {
                if (_gxeExpressionLabel != null) _gxeExpressionLabel.text = "Expression Modifier: N/A";
                if (_gxeStressLabel != null) _gxeStressLabel.text = "Stress Response: N/A";
                if (_gxeAdaptationLabel != null) _gxeAdaptationLabel.text = "Adaptation Level: N/A";
            }
        }
        
        private float CalculateExpressionModifier()
        {
            // Simplified expression modifier calculation
            var baseModifier = 1.0f;
            var healthModifier = _selectedPlant.OverallHealth;
            var stressModifier = 1.0f - (_selectedPlant.StressLevel * 0.5f);
            
            return baseModifier * healthModifier * stressModifier;
        }
        
        private void OnEnable()
        {
            // Subscribe to plant-related events if available
            if (_plantManager != null)
            {
                // Subscribe to plant events for real-time updates
                // This would be implemented once event system is available
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from events
            if (_plantManager != null)
            {
                // Unsubscribe from plant events
            }
        }
    }
}