using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.UI.Core;
using ProjectChimera.UI.Events;
using ProjectChimera.UI.Components;
using ProjectChimera.Data;
using ProjectChimera.Data.UI;
// using ProjectChimera.Systems.Cultivation;
// using ProjectChimera.Systems.Environment;
// using ProjectChimera.Data.Genetics;

namespace ProjectChimera.UI.Panels
{
    /// <summary>
    /// Plant management panel for Project Chimera.
    /// Provides detailed plant monitoring, care tracking, and cultivation management.
    /// </summary>
    public class PlantManagementPanel : UIPanel
    {
        [Header("Plant Events")]
        [SerializeField] private SimpleGameEventSO _onPlantSelected;
        [SerializeField] private SimpleGameEventSO _onPlantActionPerformed;
        [SerializeField] private UIButtonClickEventSO _onPlantButtonClicked;
        [SerializeField] private SimpleGameEventSO _onCareTaskCompleted;
        
        [Header("Update Settings")]
        [SerializeField] private float _plantDataUpdateInterval = 3f;
        [SerializeField] private bool _enableRealTimeTracking = true;
        
        // Main layout containers
        private VisualElement _headerContainer;
        private VisualElement _contentContainer;
        private VisualElement _plantListContainer;
        private VisualElement _plantDetailsContainer;
        private VisualElement _careActionsContainer;
        
        // Header elements
        private Label _titleLabel;
        private Button _closeButton;
        private DropdownField _facilityFilter;
        private DropdownField _growthStageFilter;
        private TextField _searchField;
        private Button _addPlantButton;
        
        // Plant list elements
        private ScrollView _plantListView;
        private Label _plantCountLabel;
        private DropdownField _sortDropdown;
        private Toggle _showOnlyAlertsToggle;
        
        // Plant details elements
        private VisualElement _selectedPlantHeader;
        private Label _plantNameLabel;
        private Label _plantStrainLabel;
        private Label _plantAgeLabel;
        private UIStatusIndicator _plantHealthStatus;
        private VisualElement _plantStatsContainer;
        private VisualElement _plantImageContainer;
        
        // Plant statistics cards
        private UIDataCard _heightCard;
        private UIDataCard _widthCard;
        private UIDataCard _yieldEstimateCard;
        private UIDataCard _floweringProgressCard;
        
        // Environmental data for selected plant
        private VisualElement _environmentalDataContainer;
        private UIDataCard _localTemperatureCard;
        private UIDataCard _localHumidityCard;
        private UIDataCard _lightExposureCard;
        private UIDataCard _soilMoistureCard;
        
        // Growth charts
        private VisualElement _growthChartsContainer;
        private UISimpleChart _heightGrowthChart;
        private UISimpleChart _healthTrendChart;
        private UIProgressBar _growthStageProgress;
        
        // Care actions panel
        private VisualElement _careActionsPanel;
        private Button _waterButton;
        private Button _feedButton;
        private Button _pruneButton;
        private Button _trainButton;
        private Button _harvestButton;
        private VisualElement _scheduledTasksList;
        
        // Alerts and notifications
        private VisualElement _plantAlertsContainer;
        private ScrollView _alertsScrollView;
        
        // Data structures
        private List<PlantData> _allPlants;
        private List<PlantData> _filteredPlants;
        private PlantData _selectedPlant;
        private Dictionary<string, PlantCareTask> _pendingTasks;
        
        // Game managers
        // private CultivationManager _cultivationManager;
        // private EnvironmentalManager _environmentalManager;
        // private GeneticsManager _geneticsManager;
        
        protected override void SetupUIElements()
        {
            base.SetupUIElements();
            
            // Get manager references
            // _cultivationManager = GameManager.Instance?.GetManager<CultivationManager>();
            // _environmentalManager = GameManager.Instance?.GetManager<EnvironmentalManager>();
            // _geneticsManager = GameManager.Instance?.GetManager<GeneticsManager>();
            
            // Initialize data structures
            _allPlants = new List<PlantData>();
            _filteredPlants = new List<PlantData>();
            _pendingTasks = new Dictionary<string, PlantCareTask>();
            
            LoadPlantData();
            
            CreatePlantManagementLayout();
            CreateHeader();
            CreatePlantList();
            CreatePlantDetails();
            CreateCareActions();
            CreateAlertsSection();
            
            RefreshPlantList();
            StartDataUpdates();
        }
        
        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            
            // Header controls
            _closeButton?.RegisterCallback<ClickEvent>(OnCloseClicked);
            _facilityFilter?.RegisterCallback<ChangeEvent<string>>(OnFacilityFilterChanged);
            _growthStageFilter?.RegisterCallback<ChangeEvent<string>>(OnGrowthStageFilterChanged);
            _searchField?.RegisterCallback<ChangeEvent<string>>(OnSearchChanged);
            _addPlantButton?.RegisterCallback<ClickEvent>(OnAddPlantClicked);
            
            // Plant list controls
            _sortDropdown?.RegisterCallback<ChangeEvent<string>>(OnSortChanged);
            _showOnlyAlertsToggle?.RegisterCallback<ChangeEvent<bool>>(OnShowAlertsToggled);
            
            // Care action buttons
            _waterButton?.RegisterCallback<ClickEvent>(OnWaterClicked);
            _feedButton?.RegisterCallback<ClickEvent>(OnFeedClicked);
            _pruneButton?.RegisterCallback<ClickEvent>(OnPruneClicked);
            _trainButton?.RegisterCallback<ClickEvent>(OnTrainClicked);
            _harvestButton?.RegisterCallback<ClickEvent>(OnHarvestClicked);
        }
        
        /// <summary>
        /// Create the main plant management layout
        /// </summary>
        private void CreatePlantManagementLayout()
        {
            _rootElement.Clear();
            
            var mainContainer = new VisualElement();
            mainContainer.name = "plant-main-container";
            mainContainer.style.flexGrow = 1;
            // mainContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.BackgroundDark;
            mainContainer.style.flexDirection = FlexDirection.Column;
            
            // Header
            _headerContainer = new VisualElement();
            _headerContainer.name = "plant-header";
            _headerContainer.style.height = 80;
            // _headerContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            _headerContainer.style.flexDirection = FlexDirection.Row;
            _headerContainer.style.alignItems = Align.Center;
            _headerContainer.style.justifyContent = Justify.SpaceBetween;
            _headerContainer.style.paddingLeft = 24;
            _headerContainer.style.paddingRight = 24;
            _headerContainer.style.borderBottomWidth = 2;
            // _headerContainer.style.borderBottomColor = _uiManager.DesignSystem.ColorPalette.PrimaryGreen;
            
            // Content area
            _contentContainer = new VisualElement();
            _contentContainer.name = "plant-content";
            _contentContainer.style.flexGrow = 1;
            _contentContainer.style.flexDirection = FlexDirection.Row;
            _contentContainer.style.paddingTop = 20;
            _contentContainer.style.paddingBottom = 20;
            _contentContainer.style.paddingLeft = 20;
            _contentContainer.style.paddingRight = 20;
            
            // Plant list (left side)
            _plantListContainer = new VisualElement();
            _plantListContainer.name = "plant-list-container";
            _plantListContainer.style.width = 350;
            _plantListContainer.style.marginRight = 20;
            
            // Plant details and care (right side)
            var rightContainer = new VisualElement();
            rightContainer.style.flexGrow = 1;
            rightContainer.style.flexDirection = FlexDirection.Column;
            
            _plantDetailsContainer = new VisualElement();
            _plantDetailsContainer.name = "plant-details-container";
            _plantDetailsContainer.style.flexGrow = 1;
            _plantDetailsContainer.style.marginBottom = 20;
            
            _careActionsContainer = new VisualElement();
            _careActionsContainer.name = "care-actions-container";
            _careActionsContainer.style.height = 200;
            
            rightContainer.Add(_plantDetailsContainer);
            rightContainer.Add(_careActionsContainer);
            
            _contentContainer.Add(_plantListContainer);
            _contentContainer.Add(rightContainer);
            
            mainContainer.Add(_headerContainer);
            mainContainer.Add(_contentContainer);
            
            _rootElement.Add(mainContainer);
        }
        
        /// <summary>
        /// Create the header section
        /// </summary>
        private void CreateHeader()
        {
            // Left section
            var leftSection = new VisualElement();
            leftSection.style.flexDirection = FlexDirection.Row;
            leftSection.style.alignItems = Align.Center;
            
            _titleLabel = new Label("Plant Management");
            _titleLabel.style.fontSize = 24;
            // _titleLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.marginRight = 20;
            
            // Facility filter
            _facilityFilter = new DropdownField("Facility", 
                new List<string> { "All Facilities", "Greenhouse A", "Greenhouse B", "Indoor Facility 1" }, 0);
            _facilityFilter.style.minWidth = 150;
            _facilityFilter.style.marginRight = 15;
            
            // Growth stage filter
            _growthStageFilter = new DropdownField("Stage", 
                new List<string> { "All Stages", "Seedling", "Vegetative", "Flowering", "Harvest Ready" }, 0);
            _growthStageFilter.style.minWidth = 130;
            _growthStageFilter.style.marginRight = 15;
            
            // Search field
            _searchField = new TextField("Search plants...");
            _searchField.style.minWidth = 200;
            _searchField.style.marginRight = 15;
            
            leftSection.Add(_titleLabel);
            leftSection.Add(_facilityFilter);
            leftSection.Add(_growthStageFilter);
            leftSection.Add(_searchField);
            
            // Right section
            var rightSection = new VisualElement();
            rightSection.style.flexDirection = FlexDirection.Row;
            rightSection.style.alignItems = Align.Center;
            
            _addPlantButton = new Button();
            _addPlantButton.name = "add-plant-button";
            _addPlantButton.text = "+ Add Plant";
            _addPlantButton.style.marginRight = 15;
            // _uiManager.ApplyDesignSystemStyle(_addPlantButton, UIStyleToken.PrimaryButton);
            
            _closeButton = new Button();
            _closeButton.name = "plant-close-button";
            _closeButton.text = "✕";
            _closeButton.style.width = 40;
            _closeButton.style.height = 40;
            _closeButton.style.fontSize = 20;
            // _uiManager.ApplyDesignSystemStyle(_closeButton, UIStyleToken.SecondaryButton);
            
            rightSection.Add(_addPlantButton);
            rightSection.Add(_closeButton);
            
            _headerContainer.Add(leftSection);
            _headerContainer.Add(rightSection);
        }
        
        /// <summary>
        /// Create the plant list section
        /// </summary>
        private void CreatePlantList()
        {
            // List header
            var listHeader = new VisualElement();
            // listHeader.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            listHeader.style.borderTopLeftRadius = 12;
            listHeader.style.borderTopRightRadius = 12;
            listHeader.style.paddingTop = 16;
            listHeader.style.paddingBottom = 16;
            listHeader.style.paddingLeft = 16;
            listHeader.style.paddingRight = 16;
            listHeader.style.flexDirection = FlexDirection.Column;
            
            // Title and count
            var titleContainer = new VisualElement();
            titleContainer.style.flexDirection = FlexDirection.Row;
            titleContainer.style.justifyContent = Justify.SpaceBetween;
            titleContainer.style.alignItems = Align.Center;
            titleContainer.style.marginBottom = 12;
            
            var listTitle = new Label("Plants");
            listTitle.style.fontSize = 16;
            // listTitle.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            listTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            _plantCountLabel = new Label("0 plants");
            _plantCountLabel.style.fontSize = 12;
            // _plantCountLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            titleContainer.Add(listTitle);
            titleContainer.Add(_plantCountLabel);
            
            // Controls row
            var controlsContainer = new VisualElement();
            controlsContainer.style.flexDirection = FlexDirection.Row;
            controlsContainer.style.justifyContent = Justify.SpaceBetween;
            controlsContainer.style.alignItems = Align.Center;
            
            _sortDropdown = new DropdownField(new List<string> { "Name", "Age", "Health", "Growth Stage", "Last Care" }, 0);
            _sortDropdown.style.flexGrow = 1;
            _sortDropdown.style.marginRight = 8;
            
            _showOnlyAlertsToggle = new Toggle("Alerts Only");
            _showOnlyAlertsToggle.style.fontSize = 12;
            
            controlsContainer.Add(_sortDropdown);
            controlsContainer.Add(_showOnlyAlertsToggle);
            
            listHeader.Add(titleContainer);
            listHeader.Add(controlsContainer);
            
            // Plant list scroll view
            _plantListView = new ScrollView();
            _plantListView.name = "plant-list-scroll";
            // _plantListView.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.BackgroundMedium;
            _plantListView.style.flexGrow = 1;
            _plantListView.style.borderBottomLeftRadius = 12;
            _plantListView.style.borderBottomRightRadius = 12;
            _plantListView.style.paddingTop = 8;
            _plantListView.style.paddingBottom = 8;
            
            _plantListContainer.Add(listHeader);
            _plantListContainer.Add(_plantListView);
        }
        
        /// <summary>
        /// Create plant details section
        /// </summary>
        private void CreatePlantDetails()
        {
            // _plantDetailsContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            _plantDetailsContainer.style.borderTopLeftRadius = 12;
            _plantDetailsContainer.style.borderTopRightRadius = 12;
            _plantDetailsContainer.style.borderBottomLeftRadius = 12;
            _plantDetailsContainer.style.borderBottomRightRadius = 12;
            _plantDetailsContainer.style.paddingTop = 20;
            _plantDetailsContainer.style.paddingBottom = 20;
            _plantDetailsContainer.style.paddingLeft = 20;
            _plantDetailsContainer.style.paddingRight = 20;
            _plantDetailsContainer.style.flexDirection = FlexDirection.Column;
            
            CreatePlantHeader();
            CreatePlantStatistics();
            CreateEnvironmentalData();
            CreateGrowthCharts();
        }
        
        /// <summary>
        /// Create plant header section
        /// </summary>
        private void CreatePlantHeader()
        {
            _selectedPlantHeader = new VisualElement();
            _selectedPlantHeader.style.flexDirection = FlexDirection.Row;
            _selectedPlantHeader.style.alignItems = Align.Center;
            _selectedPlantHeader.style.marginBottom = 20;
            
            // Plant image placeholder
            _plantImageContainer = new VisualElement();
            _plantImageContainer.style.width = 80;
            _plantImageContainer.style.height = 80;
            // _plantImageContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.BackgroundMedium;
            _plantImageContainer.style.borderTopLeftRadius = 8;
            _plantImageContainer.style.borderTopRightRadius = 8;
            _plantImageContainer.style.borderBottomLeftRadius = 8;
            _plantImageContainer.style.borderBottomRightRadius = 8;
            _plantImageContainer.style.marginRight = 16;
            
            // Plant info
            var plantInfoContainer = new VisualElement();
            plantInfoContainer.style.flexGrow = 1;
            
            _plantNameLabel = new Label("Select a plant");
            _plantNameLabel.style.fontSize = 20;
            // _plantNameLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            _plantNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _plantNameLabel.style.marginBottom = 4;
            
            _plantStrainLabel = new Label("No strain selected");
            _plantStrainLabel.style.fontSize = 14;
            // _plantStrainLabel.style.color = _uiManager.DesignSystem.ColorPalette.AccentGold;
            _plantStrainLabel.style.marginBottom = 4;
            
            _plantAgeLabel = new Label("Age: --");
            _plantAgeLabel.style.fontSize = 12;
            // _plantAgeLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            plantInfoContainer.Add(_plantNameLabel);
            plantInfoContainer.Add(_plantStrainLabel);
            plantInfoContainer.Add(_plantAgeLabel);
            
            // Health status
            _plantHealthStatus = new UIStatusIndicator(UIStatus.Unknown, "No plant selected");
            _plantHealthStatus.style.alignSelf = Align.FlexEnd;
            
            _selectedPlantHeader.Add(_plantImageContainer);
            _selectedPlantHeader.Add(plantInfoContainer);
            _selectedPlantHeader.Add(_plantHealthStatus);
            
            _plantDetailsContainer.Add(_selectedPlantHeader);
        }
        
        /// <summary>
        /// Create plant statistics section
        /// </summary>
        private void CreatePlantStatistics()
        {
            _plantStatsContainer = new VisualElement();
            _plantStatsContainer.name = "plant-stats";
            _plantStatsContainer.style.flexDirection = FlexDirection.Row;
            _plantStatsContainer.style.justifyContent = Justify.SpaceBetween;
            _plantStatsContainer.style.marginBottom = 20;
            
            _heightCard = new UIDataCard("Height", "--", "cm");
            _heightCard.style.width = Length.Percent(23);
            
            _widthCard = new UIDataCard("Width", "--", "cm");
            _widthCard.style.width = Length.Percent(23);
            
            _yieldEstimateCard = new UIDataCard("Est. Yield", "--", "g");
            _yieldEstimateCard.style.width = Length.Percent(23);
            
            _floweringProgressCard = new UIDataCard("Flowering", "--", "%");
            _floweringProgressCard.style.width = Length.Percent(23);
            
            _plantStatsContainer.Add(_heightCard);
            _plantStatsContainer.Add(_widthCard);
            _plantStatsContainer.Add(_yieldEstimateCard);
            _plantStatsContainer.Add(_floweringProgressCard);
            
            _plantDetailsContainer.Add(_plantStatsContainer);
        }
        
        /// <summary>
        /// Create environmental data section
        /// </summary>
        private void CreateEnvironmentalData()
        {
            var envTitle = new Label("Local Environment");
            envTitle.style.fontSize = 14;
            // envTitle.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            envTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            envTitle.style.marginBottom = 12;
            
            _environmentalDataContainer = new VisualElement();
            _environmentalDataContainer.style.flexDirection = FlexDirection.Row;
            _environmentalDataContainer.style.justifyContent = Justify.SpaceBetween;
            _environmentalDataContainer.style.marginBottom = 20;
            
            _localTemperatureCard = new UIDataCard("Temperature", "--", "°C");
            _localTemperatureCard.style.width = Length.Percent(23);
            
            _localHumidityCard = new UIDataCard("Humidity", "--", "%");
            _localHumidityCard.style.width = Length.Percent(23);
            
            _lightExposureCard = new UIDataCard("Light", "--", "PPFD");
            _lightExposureCard.style.width = Length.Percent(23);
            
            _soilMoistureCard = new UIDataCard("Moisture", "--", "%");
            _soilMoistureCard.style.width = Length.Percent(23);
            
            _environmentalDataContainer.Add(_localTemperatureCard);
            _environmentalDataContainer.Add(_localHumidityCard);
            _environmentalDataContainer.Add(_lightExposureCard);
            _environmentalDataContainer.Add(_soilMoistureCard);
            
            _plantDetailsContainer.Add(envTitle);
            _plantDetailsContainer.Add(_environmentalDataContainer);
        }
        
        /// <summary>
        /// Create growth charts section
        /// </summary>
        private void CreateGrowthCharts()
        {
            _growthChartsContainer = new VisualElement();
            _growthChartsContainer.style.flexDirection = FlexDirection.Row;
            _growthChartsContainer.style.justifyContent = Justify.SpaceBetween;
            
            // Height growth chart
            _heightGrowthChart = new UISimpleChart("Height Growth");
            _heightGrowthChart.style.width = Length.Percent(48);
            _heightGrowthChart.style.height = 150;
            _heightGrowthChart.SetRange(0f, 200f);
            // _heightGrowthChart.LineColor = _uiManager.DesignSystem.ColorPalette.PrimaryGreen;
            
            // Health trend chart
            _healthTrendChart = new UISimpleChart("Health Trend");
            _healthTrendChart.style.width = Length.Percent(48);
            _healthTrendChart.style.height = 150;
            _healthTrendChart.SetRange(0f, 100f);
            // _healthTrendChart.LineColor = _uiManager.DesignSystem.ColorPalette.Success;
            
            _growthChartsContainer.Add(_heightGrowthChart);
            _growthChartsContainer.Add(_healthTrendChart);
            
            // Growth stage progress
            var progressContainer = new VisualElement();
            progressContainer.style.marginTop = 16;
            
            var progressTitle = new Label("Growth Stage Progress");
            progressTitle.style.fontSize = 14;
            // progressTitle.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            progressTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            progressTitle.style.marginBottom = 8;
            
            _growthStageProgress = new UIProgressBar(100f);
            _growthStageProgress.Value = 0f;
            _growthStageProgress.Format = "Stage Progress: {0:F0}%";
            // _growthStageProgress.SetColor(_uiManager.DesignSystem.ColorPalette.Info);
            
            progressContainer.Add(progressTitle);
            progressContainer.Add(_growthStageProgress);
            
            _plantDetailsContainer.Add(_growthChartsContainer);
            _plantDetailsContainer.Add(progressContainer);
        }
        
        /// <summary>
        /// Create care actions section
        /// </summary>
        private void CreateCareActions()
        {
            // _careActionsContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            _careActionsContainer.style.borderTopLeftRadius = 12;
            _careActionsContainer.style.borderTopRightRadius = 12;
            _careActionsContainer.style.borderBottomLeftRadius = 12;
            _careActionsContainer.style.borderBottomRightRadius = 12;
            _careActionsContainer.style.paddingTop = 16;
            _careActionsContainer.style.paddingBottom = 16;
            _careActionsContainer.style.paddingLeft = 20;
            _careActionsContainer.style.paddingRight = 20;
            _careActionsContainer.style.flexDirection = FlexDirection.Row;
            
            // Care actions panel
            _careActionsPanel = new VisualElement();
            _careActionsPanel.style.width = Length.Percent(60);
            _careActionsPanel.style.marginRight = 20;
            
            var actionsTitle = new Label("Plant Care Actions");
            actionsTitle.style.fontSize = 16;
            // actionsTitle.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            actionsTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            actionsTitle.style.marginBottom = 16;
            
            var actionsGrid = new VisualElement();
            actionsGrid.style.flexDirection = FlexDirection.Row;
            actionsGrid.style.flexWrap = Wrap.Wrap;
            actionsGrid.style.justifyContent = Justify.SpaceBetween;
            
            // Care action buttons
            _waterButton = CreateCareActionButton("💧", "Water", "Provide water to the plant");
            _feedButton = CreateCareActionButton("🌱", "Feed", "Apply nutrients and fertilizers");
            _pruneButton = CreateCareActionButton("✂️", "Prune", "Trim and shape the plant");
            _trainButton = CreateCareActionButton("🔗", "Train", "Apply training techniques (LST, HST)");
            _harvestButton = CreateCareActionButton("🌾", "Harvest", "Harvest mature plant material");
            
            actionsGrid.Add(_waterButton);
            actionsGrid.Add(_feedButton);
            actionsGrid.Add(_pruneButton);
            actionsGrid.Add(_trainButton);
            actionsGrid.Add(_harvestButton);
            
            _careActionsPanel.Add(actionsTitle);
            _careActionsPanel.Add(actionsGrid);
            
            // Scheduled tasks panel
            var tasksPanel = new VisualElement();
            tasksPanel.style.width = Length.Percent(38);
            
            var tasksTitle = new Label("Scheduled Tasks");
            tasksTitle.style.fontSize = 16;
            // tasksTitle.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            tasksTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            tasksTitle.style.marginBottom = 16;
            
            _scheduledTasksList = new VisualElement();
            _scheduledTasksList.style.flexDirection = FlexDirection.Column;
            
            // Sample scheduled tasks
            CreateSampleScheduledTasks();
            
            tasksPanel.Add(tasksTitle);
            tasksPanel.Add(_scheduledTasksList);
            
            _careActionsContainer.Add(_careActionsPanel);
            _careActionsContainer.Add(tasksPanel);
        }
        
        /// <summary>
        /// Create care action button
        /// </summary>
        private Button CreateCareActionButton(string icon, string label, string tooltip)
        {
            var button = new Button();
            button.name = label.ToLower() + "-action-button";
            
            var content = new VisualElement();
            content.style.alignItems = Align.Center;
            content.pickingMode = PickingMode.Ignore;
            
            var iconLabel = new Label(icon);
            iconLabel.style.fontSize = 20;
            iconLabel.style.marginBottom = 4;
            
            var textLabel = new Label(label);
            textLabel.style.fontSize = 11;
            // textLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            content.Add(iconLabel);
            content.Add(textLabel);
            button.Add(content);
            
            button.style.width = 70;
            button.style.height = 60;
            button.style.marginBottom = 8;
            button.style.marginRight = 8;
            // button.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.BackgroundMedium;
            button.style.borderTopWidth = 0;
            button.style.borderRightWidth = 0;
            button.style.borderBottomWidth = 0;
            button.style.borderLeftWidth = 0;
            button.style.borderTopLeftRadius = 8;
            button.style.borderTopRightRadius = 8;
            button.style.borderBottomLeftRadius = 8;
            button.style.borderBottomRightRadius = 8;
            
            if (!string.IsNullOrEmpty(tooltip))
            {
                button.SetupTooltip(tooltip, _rootElement);
            }
            
            return button;
        }
        
        /// <summary>
        /// Create sample scheduled tasks
        /// </summary>
        private void CreateSampleScheduledTasks()
        {
            var tasks = new[]
            {
                ("💧", "Water in 2 hours", UIStatus.Info),
                ("🌱", "Feed tomorrow", UIStatus.Warning),
                ("✂️", "Prune next week", UIStatus.Success)
            };
            
            foreach (var (icon, text, status) in tasks)
            {
                var taskItem = new VisualElement();
                taskItem.style.flexDirection = FlexDirection.Row;
                taskItem.style.alignItems = Align.Center;
                taskItem.style.marginBottom = 8;
                taskItem.style.paddingTop = 8;
                taskItem.style.paddingBottom = 8;
                taskItem.style.paddingLeft = 12;
                taskItem.style.paddingRight = 12;
                // taskItem.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.BackgroundMedium;
                taskItem.style.borderTopLeftRadius = 6;
                taskItem.style.borderTopRightRadius = 6;
                taskItem.style.borderBottomLeftRadius = 6;
                taskItem.style.borderBottomRightRadius = 6;
                
                var taskIcon = new Label(icon);
                taskIcon.style.fontSize = 16;
                taskIcon.style.marginRight = 8;
                
                var taskText = new Label(text);
                taskText.style.fontSize = 12;
                // taskText.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
                taskText.style.flexGrow = 1;
                
                var taskStatus = new UIStatusIndicator(status, "");
                
                taskItem.Add(taskIcon);
                taskItem.Add(taskText);
                taskItem.Add(taskStatus);
                
                _scheduledTasksList.Add(taskItem);
            }
        }
        
        /// <summary>
        /// Create alerts section
        /// </summary>
        private void CreateAlertsSection()
        {
            // This could be added as an overlay or additional panel for plant-specific alerts
        }
        
        /// <summary>
        /// Load plant data from managers
        /// </summary>
        private void LoadPlantData()
        {
            // This would integrate with cultivation manager
            _allPlants.Clear();
            
            // Sample plant data
            var samplePlants = new[]
            {
                new PlantData
                {
                    Id = "plant_001",
                    Name = "Northern Lights #1",
                    StrainName = "Northern Lights",
                    Age = 45,
                    GrowthStage = UIPlantGrowthStage.Vegetative,
                    Health = 92f,
                    Height = 85f,
                    Width = 65f,
                    EstimatedYield = 120f,
                    HasAlerts = false,
                    FacilityId = "greenhouse_a"
                },
                new PlantData
                {
                    Id = "plant_002",
                    Name = "White Widow #3",
                    StrainName = "White Widow",
                    Age = 72,
                    GrowthStage = UIPlantGrowthStage.Flowering,
                    Health = 88f,
                    Height = 110f,
                    Width = 85f,
                    EstimatedYield = 145f,
                    HasAlerts = true,
                    FacilityId = "greenhouse_a"
                },
                new PlantData
                {
                    Id = "plant_003",
                    Name = "Blue Dream #2",
                    StrainName = "Blue Dream",
                    Age = 28,
                    GrowthStage = UIPlantGrowthStage.Seedling,
                    Health = 95f,
                    Height = 25f,
                    Width = 15f,
                    EstimatedYield = 90f,
                    HasAlerts = false,
                    FacilityId = "indoor_facility_1"
                }
            };
            
            _allPlants.AddRange(samplePlants);
            _filteredPlants.AddRange(_allPlants);
        }
        
        /// <summary>
        /// Refresh plant list display
        /// </summary>
        private void RefreshPlantList()
        {
            _plantListView.Clear();
            
            foreach (var plant in _filteredPlants)
            {
                var plantItem = CreatePlantListItem(plant);
                _plantListView.Add(plantItem);
            }
            
            _plantCountLabel.text = $"{_filteredPlants.Count} plants";
        }
        
        /// <summary>
        /// Create plant list item
        /// </summary>
        private VisualElement CreatePlantListItem(PlantData plant)
        {
            var item = new VisualElement();
            item.name = $"plant-item-{plant.Id}";
            // item.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            item.style.borderTopLeftRadius = 8;
            item.style.borderTopRightRadius = 8;
            item.style.borderBottomLeftRadius = 8;
            item.style.borderBottomRightRadius = 8;
            item.style.marginBottom = 8;
            item.style.marginLeft = 8;
            item.style.marginRight = 8;
            item.style.paddingTop = 12;
            item.style.paddingBottom = 12;
            item.style.paddingLeft = 12;
            item.style.paddingRight = 12;
            item.style.flexDirection = FlexDirection.Row;
            item.style.alignItems = Align.Center;
            
            // Plant info
            var infoContainer = new VisualElement();
            infoContainer.style.flexGrow = 1;
            
            var nameLabel = new Label(plant.Name);
            nameLabel.style.fontSize = 14;
            // nameLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.marginBottom = 2;
            
            var strainLabel = new Label(plant.StrainName);
            strainLabel.style.fontSize = 12;
            // strainLabel.style.color = _uiManager.DesignSystem.ColorPalette.AccentGold;
            strainLabel.style.marginBottom = 2;
            
            var detailsLabel = new Label($"{plant.GrowthStage} • {plant.Age} days • {plant.Health:F0}% health");
            detailsLabel.style.fontSize = 10;
            // detailsLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            infoContainer.Add(nameLabel);
            infoContainer.Add(strainLabel);
            infoContainer.Add(detailsLabel);
            
            // Status indicators
            var statusContainer = new VisualElement();
            statusContainer.style.alignItems = Align.FlexEnd;
            
            // Health status
            UIStatus healthStatus;
            if (plant.Health >= 80f) healthStatus = UIStatus.Success;
            else if (plant.Health >= 60f) healthStatus = UIStatus.Warning;
            else healthStatus = UIStatus.Error;
            
            var healthIndicator = new UIStatusIndicator(healthStatus, "");
            healthIndicator.style.marginBottom = 4;
            
            // Alert indicator
            if (plant.HasAlerts)
            {
                var alertLabel = new Label("⚠️");
                alertLabel.style.fontSize = 16;
                // alertLabel.style.color = _uiManager.DesignSystem.ColorPalette.Warning;
                statusContainer.Add(alertLabel);
            }
            
            statusContainer.Add(healthIndicator);
            
            item.Add(infoContainer);
            item.Add(statusContainer);
            
            // Click handler
            item.RegisterCallback<ClickEvent>(evt => SelectPlant(plant));
            
            // Hover effect - commenting out until AddHoverEffects parameter signature is confirmed
            // item.AddHoverEffects(
            //     _uiManager.DesignSystem.ColorPalette.InteractiveHover,
            //     _uiManager.DesignSystem.ColorPalette.SurfaceDark
            // );
            
            return item;
        }
        
        /// <summary>
        /// Select a plant for detailed view
        /// </summary>
        private void SelectPlant(PlantData plant)
        {
            _selectedPlant = plant;
            UpdatePlantDetails();
            
            _onPlantSelected?.Raise();
            _onPlantButtonClicked?.RaiseButtonClick("plant-selected", PanelId, Vector2.zero);
            
            LogInfo($"Selected plant: {plant.Name}");
        }
        
        /// <summary>
        /// Update plant details display
        /// </summary>
        private void UpdatePlantDetails()
        {
            if (_selectedPlant == null) return;
            
            // Update header
            _plantNameLabel.text = _selectedPlant.Name;
            _plantStrainLabel.text = _selectedPlant.StrainName;
            _plantAgeLabel.text = $"Age: {_selectedPlant.Age} days";
            
            // Update health status
            UIStatus healthStatus;
            if (_selectedPlant.Health >= 80f) healthStatus = UIStatus.Success;
            else if (_selectedPlant.Health >= 60f) healthStatus = UIStatus.Warning;
            else healthStatus = UIStatus.Error;
            
            _plantHealthStatus.Status = healthStatus;
            _plantHealthStatus.Label = $"Health: {_selectedPlant.Health:F0}%";
            
            // Update statistics
            _heightCard.Value = _selectedPlant.Height.ToString("F1");
            _widthCard.Value = _selectedPlant.Width.ToString("F1");
            _yieldEstimateCard.Value = _selectedPlant.EstimatedYield.ToString("F0");
            
            // Calculate flowering progress
            float floweringProgress = 0f;
            if (_selectedPlant.GrowthStage == UIPlantGrowthStage.Flowering)
            {
                floweringProgress = (_selectedPlant.Age - 60f) / 60f * 100f; // Assume flowering starts at day 60
                floweringProgress = Mathf.Clamp(floweringProgress, 0f, 100f);
            }
            _floweringProgressCard.Value = floweringProgress.ToString("F0");
            
            // Update environmental data (sample data)
            UpdateEnvironmentalReadings();
            
            // Update charts
            UpdateGrowthCharts();
            
            // Update growth stage progress
            float stageProgress = CalculateStageProgress(_selectedPlant);
            _growthStageProgress.Value = stageProgress;
            
            // Update care action availability
            UpdateCareActionButtons();
        }
        
        /// <summary>
        /// Update environmental readings for selected plant
        /// </summary>
        private void UpdateEnvironmentalReadings()
        {
            // This would get real environmental data from sensors near the plant
            _localTemperatureCard.Value = (23.5f + Random.Range(-1f, 1f)).ToString("F1");
            _localHumidityCard.Value = (58f + Random.Range(-3f, 3f)).ToString("F0");
            _lightExposureCard.Value = (850f + Random.Range(-50f, 50f)).ToString("F0");
            _soilMoistureCard.Value = (72f + Random.Range(-5f, 5f)).ToString("F0");
        }
        
        /// <summary>
        /// Update growth charts for selected plant
        /// </summary>
        private void UpdateGrowthCharts()
        {
            // Sample growth data
            var heightData = new List<float> { 15f, 25f, 40f, 58f, 72f, 85f };
            var healthData = new List<float> { 95f, 93f, 90f, 88f, 91f, 92f };
            
            _heightGrowthChart.SetData(heightData);
            _healthTrendChart.SetData(healthData);
        }
        
        /// <summary>
        /// Calculate growth stage progress
        /// </summary>
        private float CalculateStageProgress(PlantData plant)
        {
            switch (plant.GrowthStage)
            {
                case UIPlantGrowthStage.Seedling:
                    return (plant.Age / 14f) * 100f; // 14 days for seedling stage
                case UIPlantGrowthStage.Vegetative:
                    return ((plant.Age - 14f) / 46f) * 100f; // 46 days for vegetative
                case UIPlantGrowthStage.Flowering:
                    return ((plant.Age - 60f) / 60f) * 100f; // 60 days for flowering
                default:
                    return 100f;
            }
        }
        
        /// <summary>
        /// Update care action button availability
        /// </summary>
        private void UpdateCareActionButtons()
        {
            if (_selectedPlant == null)
            {
                _waterButton.SetEnabled(false);
                _feedButton.SetEnabled(false);
                _pruneButton.SetEnabled(false);
                _trainButton.SetEnabled(false);
                _harvestButton.SetEnabled(false);
                return;
            }
            
            // Enable/disable buttons based on plant state
            _waterButton.SetEnabled(true);
            _feedButton.SetEnabled(true);
            _pruneButton.SetEnabled(_selectedPlant.GrowthStage != UIPlantGrowthStage.Seedling);
            _trainButton.SetEnabled(_selectedPlant.GrowthStage == UIPlantGrowthStage.Vegetative);
            _harvestButton.SetEnabled(_selectedPlant.GrowthStage == UIPlantGrowthStage.HarvestReady);
        }
        
        /// <summary>
        /// Start data update cycle
        /// </summary>
        private void StartDataUpdates()
        {
            if (_enableRealTimeTracking)
            {
                InvokeRepeating(nameof(UpdatePlantData), 0f, _plantDataUpdateInterval);
            }
        }
        
        /// <summary>
        /// Update plant data periodically
        /// </summary>
        private void UpdatePlantData()
        {
            if (_selectedPlant != null)
            {
                UpdateEnvironmentalReadings();
            }
        }
        
        // Event handlers
        private void OnCloseClicked(ClickEvent evt)
        {
            Hide();
        }
        
        private void OnFacilityFilterChanged(ChangeEvent<string> evt)
        {
            ApplyFilters();
        }
        
        private void OnGrowthStageFilterChanged(ChangeEvent<string> evt)
        {
            ApplyFilters();
        }
        
        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            ApplyFilters();
        }
        
        private void OnSortChanged(ChangeEvent<string> evt)
        {
            SortPlantList();
        }
        
        private void OnShowAlertsToggled(ChangeEvent<bool> evt)
        {
            ApplyFilters();
        }
        
        private void OnAddPlantClicked(ClickEvent evt)
        {
            _onPlantButtonClicked?.RaiseButtonClick("add-plant", PanelId, evt.position);
            // Show add plant dialog
        }
        
        // Care action handlers
        private void OnWaterClicked(ClickEvent evt)
        {
            if (_selectedPlant == null) return;
            
            PerformCareAction("water", "Plant watered successfully");
            _onPlantButtonClicked?.RaiseButtonClick("water-plant", PanelId, evt.position);
        }
        
        private void OnFeedClicked(ClickEvent evt)
        {
            if (_selectedPlant == null) return;
            
            PerformCareAction("feed", "Nutrients applied successfully");
            _onPlantButtonClicked?.RaiseButtonClick("feed-plant", PanelId, evt.position);
        }
        
        private void OnPruneClicked(ClickEvent evt)
        {
            if (_selectedPlant == null) return;
            
            PerformCareAction("prune", "Plant pruned successfully");
            _onPlantButtonClicked?.RaiseButtonClick("prune-plant", PanelId, evt.position);
        }
        
        private void OnTrainClicked(ClickEvent evt)
        {
            if (_selectedPlant == null) return;
            
            PerformCareAction("train", "Training technique applied");
            _onPlantButtonClicked?.RaiseButtonClick("train-plant", PanelId, evt.position);
        }
        
        private void OnHarvestClicked(ClickEvent evt)
        {
            if (_selectedPlant == null) return;
            
            PerformCareAction("harvest", "Plant harvested successfully");
            _onPlantButtonClicked?.RaiseButtonClick("harvest-plant", PanelId, evt.position);
        }
        
        /// <summary>
        /// Perform a care action on the selected plant
        /// </summary>
        private void PerformCareAction(string action, string message)
        {
            // This would integrate with the cultivation manager
            LogInfo($"Performed {action} on plant {_selectedPlant.Name}");
            
            // Show success notification
            var uiManager = GameManager.Instance?.GetManager<GameUIManager>();
            if (uiManager != null)
            {
                uiManager.ShowNotification("Plant Care", message, NotificationType.Success);
            }
            
            _onCareTaskCompleted?.Raise();
        }
        
        /// <summary>
        /// Apply search and filter criteria
        /// </summary>
        private void ApplyFilters()
        {
            _filteredPlants.Clear();
            
            foreach (var plant in _allPlants)
            {
                bool passesFilter = true;
                
                // Facility filter
                if (_facilityFilter.value != "All Facilities" && 
                    !string.IsNullOrEmpty(_facilityFilter.value))
                {
                    // Would check against actual facility IDs
                    passesFilter = passesFilter && true; // Simplified
                }
                
                // Growth stage filter
                if (_growthStageFilter.value != "All Stages" && 
                    !string.IsNullOrEmpty(_growthStageFilter.value))
                {
                    passesFilter = passesFilter && plant.GrowthStage.ToString() == _growthStageFilter.value;
                }
                
                // Search filter
                if (!string.IsNullOrEmpty(_searchField.value))
                {
                    var searchTerm = _searchField.value.ToLower();
                    passesFilter = passesFilter && (
                        plant.Name.ToLower().Contains(searchTerm) ||
                        plant.StrainName.ToLower().Contains(searchTerm)
                    );
                }
                
                // Alerts filter
                if (_showOnlyAlertsToggle.value)
                {
                    passesFilter = passesFilter && plant.HasAlerts;
                }
                
                if (passesFilter)
                {
                    _filteredPlants.Add(plant);
                }
            }
            
            SortPlantList();
            RefreshPlantList();
        }
        
        /// <summary>
        /// Sort the plant list
        /// </summary>
        private void SortPlantList()
        {
            switch (_sortDropdown.value)
            {
                case "Name":
                    _filteredPlants = _filteredPlants.OrderBy(p => p.Name).ToList();
                    break;
                case "Age":
                    _filteredPlants = _filteredPlants.OrderByDescending(p => p.Age).ToList();
                    break;
                case "Health":
                    _filteredPlants = _filteredPlants.OrderByDescending(p => p.Health).ToList();
                    break;
                case "Growth Stage":
                    _filteredPlants = _filteredPlants.OrderBy(p => p.GrowthStage).ToList();
                    break;
            }
        }
        
        protected override void OnAfterShow()
        {
            base.OnAfterShow();
            LoadPlantData();
            RefreshPlantList();
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            CancelInvoke(nameof(UpdatePlantData));
        }
    }
    
    /// <summary>
    /// Plant data structure
    /// </summary>
    [System.Serializable]
    public class PlantData
    {
        public string Id;
        public string Name;
        public string StrainName;
        public int Age;
        public UIPlantGrowthStage GrowthStage;
        public float Health;
        public float Height;
        public float Width;
        public float EstimatedYield;
        public bool HasAlerts;
        public string FacilityId;
    }
    
    /// <summary>
    /// Plant growth stages
    /// </summary>
    public enum UIPlantGrowthStage
    {
        Seed,
        Seedling,
        Vegetative,
        Flowering,
        HarvestReady,
        Harvested
    }
    
    /// <summary>
    /// Plant care task structure
    /// </summary>
    [System.Serializable]
    public struct PlantCareTask
    {
        public string TaskId;
        public string PlantId;
        public string TaskType;
        public string Description;
        public float ScheduledTime;
        public bool IsCompleted;
    }
}