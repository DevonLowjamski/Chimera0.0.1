using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.UI.Core;
using ProjectChimera.UI.Components;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Data.UI;
// using ProjectChimera.Data.Genetics;
using ProjectChimera.Systems.Cultivation;
// Explicit alias to resolve ambiguous reference between Systems.Cultivation.PlantInstance and Data.UI.PlantInstance
using PlantInstance = ProjectChimera.Systems.Cultivation.PlantInstance;
// Explicit alias to resolve ambiguous reference between NotificationType and NotificationSeverity
using NotificationType = ProjectChimera.UI.Core.NotificationType;
// Explicit alias to resolve ambiguous reference for BreedingResult
using BreedingResult = ProjectChimera.Data.Genetics.BreedingResult;

namespace ProjectChimera.UI.Panels
{
    /// <summary>
    /// Plant breeding panel for Project Chimera.
    /// Provides an engaging interface for genetic crossbreeding, strain development, and trait tracking.
    /// Features game-like breeding mechanics with visual genetic displays.
    /// </summary>
    public class PlantBreedingPanel : UIPanel
    {
        [Header("Breeding Configuration")]
        [SerializeField] private float _breedingAnimationDuration = 2f;
        [SerializeField] private bool _enableBreedingEffects = true;
        [SerializeField] private int _maxBreedingHistory = 50;
        
        [Header("Breeding Events")]
        [SerializeField] private SimpleGameEventSO _onBreedingStarted;
        [SerializeField] private SimpleGameEventSO _onBreedingCompleted;
        [SerializeField] private SimpleGameEventSO _onNewStrainCreated;
        
        [Header("Audio")]
        [SerializeField] private AudioClip _breedingStartSound;
        [SerializeField] private AudioClip _breedingCompleteSound;
        [SerializeField] private AudioClip _newStrainSound;
        [SerializeField] private AudioSource _audioSource;
        
        // Main layout containers
        private VisualElement _headerContainer;
        private VisualElement _contentContainer;
        private VisualElement _parentSelectionContainer;
        private VisualElement _breedingProcessContainer;
        private VisualElement _offspringContainer;
        private VisualElement _strainLibraryContainer;
        
        // Header elements
        private Label _titleLabel;
        private Button _closeButton;
        private Label _xpDisplayLabel;
        private UIProgressBar _breedingLevelProgress;
        
        // Parent selection
        private VisualElement _parent1Container;
        private VisualElement _parent2Container;
        private PlantStrainCard _parent1Card;
        private PlantStrainCard _parent2Card;
        private Button _swapParentsButton;
        private Button _clearParentsButton;
        
        // Breeding process
        private VisualElement _breedingVisualization;
        private Button _startBreedingButton;
        private UIProgressBar _breedingProgress;
        private Label _breedingStatusLabel;
        private VisualElement _geneticCombinationDisplay;
        
        // Offspring results
        private VisualElement _offspringResultsContainer;
        private ScrollView _offspringScrollView;
        private Button _saveStrainButton;
        private Button _discardOffspringButton;
        
        // Strain library
        private ScrollView _strainLibraryScrollView;
        private TextField _strainSearchField;
        private DropdownField _strainTypeFilter;
        private Label _strainCountLabel;
        
        // Data and state
        private List<PlantStrainSO> _availableStrains = new List<PlantStrainSO>();
        private List<PlantStrainSO> _filteredStrains = new List<PlantStrainSO>();
        private List<BreedingResult> _breedingHistory = new List<BreedingResult>();
        private BreedingResult _currentBreeding;
        private bool _isBreeding = false;
        
        // Selected parents (PlantInstance objects for proper genetics integration)
        private PlantInstance _selectedParent1;
        private PlantInstance _selectedParent2;
        
        // Game managers
        // private GeneticsManager _geneticsManager;
        // private CultivationManager _cultivationManager;
        // private PlantManager _plantManager;
        
        // Player progression
        private int _breedingXP = 0;
        private int _breedingLevel = 1;
        private int _totalBreedings = 0;
        
        protected override void SetupUIElements()
        {
            base.SetupUIElements();
            
            // Get manager references
            // _geneticsManager = GameManager.Instance?.GetManager<GeneticsManager>();
            // _cultivationManager = GameManager.Instance?.GetManager<CultivationManager>();
            // _plantManager = GameManager.Instance?.GetManager<PlantManager>();
            
            LoadPlayerData();
            LoadAvailableStrains();
            
            CreateBreedingLayout();
            CreateHeader();
            CreateParentSelection();
            CreateBreedingProcess();
            CreateOffspringResults();
            CreateStrainLibrary();
            
            RefreshStrainLibrary();
            UpdatePlayerProgression();
        }
        
        protected override void BindUIEvents()
        {
            base.BindUIEvents();
            
            // Header controls
            _closeButton?.RegisterCallback<ClickEvent>(OnCloseClicked);
            
            // Parent selection
            _swapParentsButton?.RegisterCallback<ClickEvent>(OnSwapParentsClicked);
            _clearParentsButton?.RegisterCallback<ClickEvent>(OnClearParentsClicked);
            
            // Breeding process
            _startBreedingButton?.RegisterCallback<ClickEvent>(OnStartBreedingClicked);
            
            // Offspring management
            _saveStrainButton?.RegisterCallback<ClickEvent>(OnSaveStrainClicked);
            _discardOffspringButton?.RegisterCallback<ClickEvent>(OnDiscardOffspringClicked);
            
            // Strain library
            _strainSearchField?.RegisterCallback<ChangeEvent<string>>(OnStrainSearchChanged);
            _strainTypeFilter?.RegisterCallback<ChangeEvent<string>>(OnStrainTypeFilterChanged);
        }
        
        /// <summary>
        /// Create the main breeding layout
        /// </summary>
        private void CreateBreedingLayout()
        {
            _rootElement.Clear();
            
            var mainContainer = new VisualElement();
            mainContainer.name = "breeding-main-container";
            mainContainer.style.flexGrow = 1;
            // mainContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.BackgroundDark;
            mainContainer.style.flexDirection = FlexDirection.Column;
            
            // Header
            _headerContainer = new VisualElement();
            _headerContainer.name = "breeding-header";
            _headerContainer.style.height = 80;
            // _headerContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            _headerContainer.style.flexDirection = FlexDirection.Row;
            _headerContainer.style.alignItems = Align.Center;
            _headerContainer.style.justifyContent = Justify.SpaceBetween;
            _headerContainer.style.paddingLeft = 24;
            _headerContainer.style.paddingRight = 24;
            _headerContainer.style.borderBottomWidth = 2;
            // _headerContainer.style.borderBottomColor = _uiManager.DesignSystem.ColorPalette.AccentGold;
            
            // Content area
            _contentContainer = new VisualElement();
            _contentContainer.name = "breeding-content";
            _contentContainer.style.flexGrow = 1;
            _contentContainer.style.flexDirection = FlexDirection.Row;
            _contentContainer.style.paddingTop = 20;
            _contentContainer.style.paddingBottom = 20;
            _contentContainer.style.paddingLeft = 20;
            _contentContainer.style.paddingRight = 20;
            
            // Left side - breeding interface
            var leftContainer = new VisualElement();
            leftContainer.style.width = Length.Percent(70);
            leftContainer.style.marginRight = 20;
            leftContainer.style.flexDirection = FlexDirection.Column;
            
            _parentSelectionContainer = new VisualElement();
            _parentSelectionContainer.name = "parent-selection";
            _parentSelectionContainer.style.height = 200;
            _parentSelectionContainer.style.marginBottom = 20;
            
            _breedingProcessContainer = new VisualElement();
            _breedingProcessContainer.name = "breeding-process";
            _breedingProcessContainer.style.height = 250;
            _breedingProcessContainer.style.marginBottom = 20;
            
            _offspringContainer = new VisualElement();
            _offspringContainer.name = "offspring-container";
            _offspringContainer.style.flexGrow = 1;
            
            leftContainer.Add(_parentSelectionContainer);
            leftContainer.Add(_breedingProcessContainer);
            leftContainer.Add(_offspringContainer);
            
            // Right side - strain library
            _strainLibraryContainer = new VisualElement();
            _strainLibraryContainer.name = "strain-library";
            _strainLibraryContainer.style.width = Length.Percent(30);
            
            _contentContainer.Add(leftContainer);
            _contentContainer.Add(_strainLibraryContainer);
            
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
            leftSection.style.flexDirection = FlexDirection.Column;
            
            _titleLabel = new Label("🧬 Plant Breeding Laboratory");
            _titleLabel.style.fontSize = 24;
            // _titleLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.marginBottom = 4;
            
            var subtitleLabel = new Label("Genetic Engineering & Strain Development");
            subtitleLabel.style.fontSize = 14;
            // subtitleLabel.style.color = _uiManager.DesignSystem.ColorPalette.AccentGold;
            
            leftSection.Add(_titleLabel);
            leftSection.Add(subtitleLabel);
            
            // Center section - player progression
            var centerSection = new VisualElement();
            centerSection.style.flexDirection = FlexDirection.Column;
            centerSection.style.alignItems = Align.Center;
            
            _xpDisplayLabel = new Label($"Breeding Level {_breedingLevel}");
            _xpDisplayLabel.style.fontSize = 16;
            // _xpDisplayLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            _xpDisplayLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _xpDisplayLabel.style.marginBottom = 4;
            
            _breedingLevelProgress = new UIProgressBar(1000f);
            _breedingLevelProgress.Value = _breedingXP % 1000;
            _breedingLevelProgress.Format = "{0:F0} / 1000 XP";
            // _breedingLevelProgress.SetColor(_uiManager.DesignSystem.ColorPalette.AccentGold);
            _breedingLevelProgress.style.width = 200;
            
            centerSection.Add(_xpDisplayLabel);
            centerSection.Add(_breedingLevelProgress);
            
            // Right section
            var rightSection = new VisualElement();
            rightSection.style.flexDirection = FlexDirection.Row;
            rightSection.style.alignItems = Align.Center;
            
            var statsContainer = new VisualElement();
            statsContainer.style.flexDirection = FlexDirection.Column;
            statsContainer.style.alignItems = Align.FlexEnd;
            statsContainer.style.marginRight = 20;
            
            var totalBreedingsLabel = new Label($"Total Breedings: {_totalBreedings}");
            totalBreedingsLabel.style.fontSize = 12;
            // totalBreedingsLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            var strainsCreatedLabel = new Label($"Strains Created: {_availableStrains.Count(s => s.IsCustomStrain)}");
            strainsCreatedLabel.style.fontSize = 12;
            // strainsCreatedLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            statsContainer.Add(totalBreedingsLabel);
            statsContainer.Add(strainsCreatedLabel);
            
            _closeButton = new Button();
            _closeButton.name = "breeding-close-button";
            _closeButton.text = "✕";
            _closeButton.style.width = 40;
            _closeButton.style.height = 40;
            _closeButton.style.fontSize = 20;
            // _uiManager.ApplyDesignSystemStyle(_closeButton, UIStyleToken.SecondaryButton);
            
            rightSection.Add(statsContainer);
            rightSection.Add(_closeButton);
            
            _headerContainer.Add(leftSection);
            _headerContainer.Add(centerSection);
            _headerContainer.Add(rightSection);
        }
        
        /// <summary>
        /// Create parent selection section
        /// </summary>
        private void CreateParentSelection()
        {
            // _parentSelectionContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            _parentSelectionContainer.style.borderTopLeftRadius = 12;
            _parentSelectionContainer.style.borderTopRightRadius = 12;
            _parentSelectionContainer.style.borderBottomLeftRadius = 12;
            _parentSelectionContainer.style.borderBottomRightRadius = 12;
            _parentSelectionContainer.style.paddingTop = 20;
            _parentSelectionContainer.style.paddingBottom = 20;
            _parentSelectionContainer.style.paddingLeft = 20;
            _parentSelectionContainer.style.paddingRight = 20;
            _parentSelectionContainer.style.flexDirection = FlexDirection.Column;
            
            // Title
            var titleContainer = new VisualElement();
            titleContainer.style.flexDirection = FlexDirection.Row;
            titleContainer.style.justifyContent = Justify.SpaceBetween;
            titleContainer.style.alignItems = Align.Center;
            titleContainer.style.marginBottom = 16;
            
            var sectionTitle = new Label("👨‍👩‍👧‍👦 Parent Selection");
            sectionTitle.style.fontSize = 16;
            // sectionTitle.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            sectionTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            var controlsContainer = new VisualElement();
            controlsContainer.style.flexDirection = FlexDirection.Row;
            
            _swapParentsButton = new Button();
            _swapParentsButton.text = "🔄 Swap";
            _swapParentsButton.style.marginRight = 8;
            // _uiManager.ApplyDesignSystemStyle(_swapParentsButton, UIStyleToken.SecondaryButton);
            
            _clearParentsButton = new Button();
            _clearParentsButton.text = "🗑️ Clear";
            // _uiManager.ApplyDesignSystemStyle(_clearParentsButton, UIStyleToken.SecondaryButton);
            
            controlsContainer.Add(_swapParentsButton);
            controlsContainer.Add(_clearParentsButton);
            
            titleContainer.Add(sectionTitle);
            titleContainer.Add(controlsContainer);
            
            // Parent containers
            var parentsContainer = new VisualElement();
            parentsContainer.style.flexDirection = FlexDirection.Row;
            parentsContainer.style.justifyContent = Justify.SpaceBetween;
            parentsContainer.style.flexGrow = 1;
            
            _parent1Container = CreateParentSlot("Parent A", "♀️");
            _parent2Container = CreateParentSlot("Parent B", "♂️");
            
            // Crossbreeding indicator
            var crossIndicator = new VisualElement();
            crossIndicator.style.alignItems = Align.Center;
            crossIndicator.style.justifyContent = Justify.Center;
            crossIndicator.style.width = 60;
            
            var crossLabel = new Label("×");
            crossLabel.style.fontSize = 32;
            // crossLabel.style.color = _uiManager.DesignSystem.ColorPalette.AccentGold;
            crossLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            crossIndicator.Add(crossLabel);
            
            parentsContainer.Add(_parent1Container);
            parentsContainer.Add(crossIndicator);
            parentsContainer.Add(_parent2Container);
            
            _parentSelectionContainer.Add(titleContainer);
            _parentSelectionContainer.Add(parentsContainer);
        }
        
        /// <summary>
        /// Create a parent selection slot
        /// </summary>
        private VisualElement CreateParentSlot(string title, string icon)
        {
            var slotContainer = new VisualElement();
            slotContainer.style.width = Length.Percent(45);
            // slotContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.BackgroundMedium;
            slotContainer.style.borderTopLeftRadius = 8;
            slotContainer.style.borderTopRightRadius = 8;
            slotContainer.style.borderBottomLeftRadius = 8;
            slotContainer.style.borderBottomRightRadius = 8;
            slotContainer.style.paddingTop = 12;
            slotContainer.style.paddingBottom = 12;
            slotContainer.style.paddingLeft = 12;
            slotContainer.style.paddingRight = 12;
            slotContainer.style.flexDirection = FlexDirection.Column;
            slotContainer.style.alignItems = Align.Center;
            slotContainer.style.justifyContent = Justify.Center;
            
            // Header
            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.marginBottom = 8;
            
            var iconLabel = new Label(icon);
            iconLabel.style.fontSize = 20;
            iconLabel.style.marginRight = 8;
            
            var titleLabel = new Label(title);
            titleLabel.style.fontSize = 14;
            // titleLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            headerContainer.Add(iconLabel);
            headerContainer.Add(titleLabel);
            
            // Placeholder content
            var placeholderLabel = new Label("Drop strain here\nor select from library");
            placeholderLabel.style.fontSize = 12;
            // placeholderLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            placeholderLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            placeholderLabel.style.whiteSpace = WhiteSpace.Normal;
            
            slotContainer.Add(headerContainer);
            slotContainer.Add(placeholderLabel);
            
            // Add drop zone functionality
            slotContainer.AddToClassList("strain-drop-zone");
            
            return slotContainer;
        }
        
        /// <summary>
        /// Create breeding process section
        /// </summary>
        private void CreateBreedingProcess()
        {
            // _breedingProcessContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            _breedingProcessContainer.style.borderTopLeftRadius = 12;
            _breedingProcessContainer.style.borderTopRightRadius = 12;
            _breedingProcessContainer.style.borderBottomLeftRadius = 12;
            _breedingProcessContainer.style.borderBottomRightRadius = 12;
            _breedingProcessContainer.style.paddingTop = 20;
            _breedingProcessContainer.style.paddingBottom = 20;
            _breedingProcessContainer.style.paddingLeft = 20;
            _breedingProcessContainer.style.paddingRight = 20;
            _breedingProcessContainer.style.flexDirection = FlexDirection.Column;
            
            // Title
            var sectionTitle = new Label("🧪 Breeding Process");
            sectionTitle.style.fontSize = 16;
            // sectionTitle.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            sectionTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            sectionTitle.style.marginBottom = 16;
            
            // Breeding visualization
            _breedingVisualization = new VisualElement();
            _breedingVisualization.style.flexGrow = 1;
            // _breedingVisualization.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.BackgroundDark;
            _breedingVisualization.style.borderTopLeftRadius = 8;
            _breedingVisualization.style.borderTopRightRadius = 8;
            _breedingVisualization.style.borderBottomLeftRadius = 8;
            _breedingVisualization.style.borderBottomRightRadius = 8;
            _breedingVisualization.style.marginBottom = 16;
            _breedingVisualization.style.alignItems = Align.Center;
            _breedingVisualization.style.justifyContent = Justify.Center;
            
            CreateGeneticVisualization();
            
            // Controls
            var controlsContainer = new VisualElement();
            controlsContainer.style.flexDirection = FlexDirection.Row;
            controlsContainer.style.justifyContent = Justify.SpaceBetween;
            controlsContainer.style.alignItems = Align.Center;
            
            _startBreedingButton = new Button();
            _startBreedingButton.text = "🚀 Start Breeding";
            _startBreedingButton.style.width = 150;
            _startBreedingButton.SetEnabled(false);
            // _uiManager.ApplyDesignSystemStyle(_startBreedingButton, UIStyleToken.PrimaryButton);
            
            var progressContainer = new VisualElement();
            progressContainer.style.flexGrow = 1;
            progressContainer.style.marginLeft = 20;
            
            _breedingStatusLabel = new Label("Select two parent strains to begin");
            _breedingStatusLabel.style.fontSize = 14;
            // _breedingStatusLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            _breedingStatusLabel.style.marginBottom = 4;
            
            _breedingProgress = new UIProgressBar(100f);
            _breedingProgress.Value = 0f;
            _breedingProgress.Format = "Breeding Progress: {0:F0}%";
            // _breedingProgress.SetColor(_uiManager.DesignSystem.ColorPalette.AccentGold);
            
            progressContainer.Add(_breedingStatusLabel);
            progressContainer.Add(_breedingProgress);
            
            controlsContainer.Add(_startBreedingButton);
            controlsContainer.Add(progressContainer);
            
            _breedingProcessContainer.Add(sectionTitle);
            _breedingProcessContainer.Add(_breedingVisualization);
            _breedingProcessContainer.Add(controlsContainer);
        }
        
        /// <summary>
        /// Create genetic visualization
        /// </summary>
        private void CreateGeneticVisualization()
        {
            _geneticCombinationDisplay = new VisualElement();
            _geneticCombinationDisplay.style.flexDirection = FlexDirection.Column;
            _geneticCombinationDisplay.style.alignItems = Align.Center;
            _geneticCombinationDisplay.style.justifyContent = Justify.Center;
            _geneticCombinationDisplay.style.width = Length.Percent(100);
            _geneticCombinationDisplay.style.height = Length.Percent(100);
            
            var placeholderLabel = new Label("🧬");
            placeholderLabel.style.fontSize = 48;
            // placeholderLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            placeholderLabel.style.marginBottom = 8;
            
            var instructionLabel = new Label("Genetic combination will appear here");
            instructionLabel.style.fontSize = 12;
            // instructionLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            instructionLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            _geneticCombinationDisplay.Add(placeholderLabel);
            _geneticCombinationDisplay.Add(instructionLabel);
            
            _breedingVisualization.Add(_geneticCombinationDisplay);
        }
        
        /// <summary>
        /// Create offspring results section
        /// </summary>
        private void CreateOffspringResults()
        {
            // _offspringContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            _offspringContainer.style.borderTopLeftRadius = 12;
            _offspringContainer.style.borderTopRightRadius = 12;
            _offspringContainer.style.borderBottomLeftRadius = 12;
            _offspringContainer.style.borderBottomRightRadius = 12;
            _offspringContainer.style.paddingTop = 20;
            _offspringContainer.style.paddingBottom = 20;
            _offspringContainer.style.paddingLeft = 20;
            _offspringContainer.style.paddingRight = 20;
            _offspringContainer.style.flexDirection = FlexDirection.Column;
            
            // Title
            var titleContainer = new VisualElement();
            titleContainer.style.flexDirection = FlexDirection.Row;
            titleContainer.style.justifyContent = Justify.SpaceBetween;
            titleContainer.style.alignItems = Align.Center;
            titleContainer.style.marginBottom = 16;
            
            var sectionTitle = new Label("🌱 Breeding Results");
            sectionTitle.style.fontSize = 16;
            // sectionTitle.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            sectionTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            var actionsContainer = new VisualElement();
            actionsContainer.style.flexDirection = FlexDirection.Row;
            
            _saveStrainButton = new Button();
            _saveStrainButton.text = "💾 Save Strain";
            _saveStrainButton.style.marginRight = 8;
            _saveStrainButton.SetEnabled(false);
            // _uiManager.ApplyDesignSystemStyle(_saveStrainButton, UIStyleToken.PrimaryButton);
            
            _discardOffspringButton = new Button();
            _discardOffspringButton.text = "🗑️ Discard";
            _discardOffspringButton.SetEnabled(false);
            // _uiManager.ApplyDesignSystemStyle(_discardOffspringButton, UIStyleToken.SecondaryButton);
            
            actionsContainer.Add(_saveStrainButton);
            actionsContainer.Add(_discardOffspringButton);
            
            titleContainer.Add(sectionTitle);
            titleContainer.Add(actionsContainer);
            
            // Results area
            _offspringResultsContainer = new VisualElement();
            _offspringResultsContainer.style.flexGrow = 1;
            // _offspringResultsContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.BackgroundMedium;
            _offspringResultsContainer.style.borderTopLeftRadius = 8;
            _offspringResultsContainer.style.borderTopRightRadius = 8;
            _offspringResultsContainer.style.borderBottomLeftRadius = 8;
            _offspringResultsContainer.style.borderBottomRightRadius = 8;
            _offspringResultsContainer.style.alignItems = Align.Center;
            _offspringResultsContainer.style.justifyContent = Justify.Center;
            
            var placeholderLabel = new Label("Complete breeding to see results");
            placeholderLabel.style.fontSize = 14;
            // placeholderLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            placeholderLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            _offspringResultsContainer.Add(placeholderLabel);
            
            _offspringContainer.Add(titleContainer);
            _offspringContainer.Add(_offspringResultsContainer);
        }
        
        /// <summary>
        /// Create strain library section
        /// </summary>
        private void CreateStrainLibrary()
        {
            // _strainLibraryContainer.style.backgroundColor = _uiManager.DesignSystem.ColorPalette.SurfaceDark;
            _strainLibraryContainer.style.borderTopLeftRadius = 12;
            _strainLibraryContainer.style.borderTopRightRadius = 12;
            _strainLibraryContainer.style.borderBottomLeftRadius = 12;
            _strainLibraryContainer.style.borderBottomRightRadius = 12;
            _strainLibraryContainer.style.paddingTop = 20;
            _strainLibraryContainer.style.paddingBottom = 20;
            _strainLibraryContainer.style.paddingLeft = 20;
            _strainLibraryContainer.style.paddingRight = 20;
            _strainLibraryContainer.style.flexDirection = FlexDirection.Column;
            
            // Header
            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.marginBottom = 16;
            
            var sectionTitle = new Label("📚 Strain Library");
            sectionTitle.style.fontSize = 16;
            // sectionTitle.style.color = _uiManager.DesignSystem.ColorPalette.TextPrimary;
            sectionTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            _strainCountLabel = new Label($"{_availableStrains.Count} strains");
            _strainCountLabel.style.fontSize = 12;
            // _strainCountLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            
            headerContainer.Add(sectionTitle);
            headerContainer.Add(_strainCountLabel);
            
            // Search and filters
            _strainSearchField = new TextField();
            _strainSearchField.label = "Search";
            _strainSearchField.style.marginBottom = 8;
            
            _strainTypeFilter = new DropdownField("Type", new List<string> { "All", "Indica", "Sativa", "Hybrid", "Custom" }, 0);
            _strainTypeFilter.style.marginBottom = 16;
            
            // Strain list
            _strainLibraryScrollView = new ScrollView();
            _strainLibraryScrollView.style.flexGrow = 1;
            
            _strainLibraryContainer.Add(headerContainer);
            _strainLibraryContainer.Add(_strainSearchField);
            _strainLibraryContainer.Add(_strainTypeFilter);
            _strainLibraryContainer.Add(_strainLibraryScrollView);
        }
        
        /// <summary>
        /// Load player breeding data
        /// </summary>
        private void LoadPlayerData()
        {
            // This would load from save system
            _breedingXP = 450; // Sample data
            _breedingLevel = 2;
            _totalBreedings = 12;
        }
        
        /// <summary>
        /// Load available strains from ScriptableObject assets
        /// </summary>
        private void LoadAvailableStrains()
        {
            _availableStrains.Clear();
            
            // Runtime loading of PlantStrainSO assets
            var strains = Resources.LoadAll<PlantStrainSO>("Strains");
            _availableStrains.AddRange(strains);
            
            // If no resources found, create sample strains for testing
            if (_availableStrains.Count == 0)
            {
                Debug.LogWarning("No PlantStrainSO assets found in Resources/Strains. UI will show empty library.");
            }
            
            _filteredStrains.AddRange(_availableStrains);
            
            LogInfo($"Loaded {_availableStrains.Count} plant strains for breeding");
        }
        
        /// <summary>
        /// Refresh strain library display
        /// </summary>
        private void RefreshStrainLibrary()
        {
            _strainLibraryScrollView.Clear();
            
            foreach (var strain in _filteredStrains)
            {
                var strainCard = new PlantStrainCard(strain);
                strainCard.OnStrainSelected += OnStrainSelected;
                _strainLibraryScrollView.Add(strainCard);
            }
            
            _strainCountLabel.text = $"{_filteredStrains.Count} strains";
        }
        
        /// <summary>
        /// Update player progression display
        /// </summary>
        private void UpdatePlayerProgression()
        {
            _xpDisplayLabel.text = $"Breeding Level {_breedingLevel}";
            _breedingLevelProgress.Value = _breedingXP % 1000;
        }
        
        // Event handlers
        private void OnCloseClicked(ClickEvent evt)
        {
            Hide();
        }
        
        private void OnSwapParentsClicked(ClickEvent evt)
        {
            var temp = _selectedParent1;
            _selectedParent1 = _selectedParent2;
            _selectedParent2 = temp;
            
            UpdateParentDisplay();
            UpdateBreedingVisualization();
        }
        
        private void OnClearParentsClicked(ClickEvent evt)
        {
            _selectedParent1 = null;
            _selectedParent2 = null;
            
            UpdateParentDisplay();
            UpdateBreedingVisualization();
            _startBreedingButton.SetEnabled(false);
        }
        
        private void OnStartBreedingClicked(ClickEvent evt)
        {
            if (_selectedParent1 != null && _selectedParent2 != null && !_isBreeding)
            {
                StartBreeding();
            }
        }
        
        private void OnSaveStrainClicked(ClickEvent evt)
        {
            if (_currentBreeding?.OffspringGenotypes != null && _currentBreeding.OffspringGenotypes.Count > 0)
            {
                SaveNewStrain(_currentBreeding.OffspringGenotypes[0]); // Save first offspring for now
            }
        }
        
        private void OnDiscardOffspringClicked(ClickEvent evt)
        {
            DiscardOffspring();
        }
        
        private void OnStrainSearchChanged(ChangeEvent<string> evt)
        {
            ApplyStrainFilters();
        }
        
        private void OnStrainTypeFilterChanged(ChangeEvent<string> evt)
        {
            ApplyStrainFilters();
        }
        
        private void OnStrainSelected(PlantStrainSO strain)
        {
            // Create PlantInstance objects for proper genetics integration
            PlantInstance plantInstance = CreateTemporaryPlantInstance(strain);
            
            // Assign to first available parent slot
            if (_selectedParent1 == null)
            {
                _selectedParent1 = plantInstance;
                _parent1Card = new PlantStrainCard(strain);
            }
            // else if (_selectedParent2 == null)
            // {
                _selectedParent2 = plantInstance;
                _parent2Card = new PlantStrainCard(strain);
            // }
            // else
            // {
                // Replace parent 1 if both slots are full
                _selectedParent1 = plantInstance;
                _parent1Card = new PlantStrainCard(strain);
            // }
            
            UpdateParentDisplay();
            UpdateBreedingVisualization();
            
            // Enable breeding if both parents are selected
            _startBreedingButton.SetEnabled(_selectedParent1 != null && _selectedParent2 != null);
        }
        
        /// <summary>
        /// Update parent display
        /// </summary>
        private void UpdateParentDisplay()
        {
            // Update parent containers with selected strains
            // This would show strain details in the parent slots
        }
        
        /// <summary>
        /// Update breeding visualization
        /// </summary>
        private void UpdateBreedingVisualization()
        {
            if (_selectedParent1 != null && _selectedParent2 != null)
            {
                // Show genetic combination preview
                _breedingStatusLabel.text = "Ready to breed! Click Start Breeding to begin.";
            }
            // else
            // {
                _breedingStatusLabel.text = "Select two parent strains to begin";
            // }
        }
        
        /// <summary>
        /// Start the breeding process
        /// </summary>
        private void StartBreeding()
        {
            _isBreeding = true;
            _startBreedingButton.SetEnabled(false);
            _breedingStatusLabel.text = "Breeding in progress...";
            
            PlaySound(_breedingStartSound);
            _onBreedingStarted?.Raise();
            
            // Start breeding animation/process
            StartCoroutine(BreedingProcess());
        }
        
        /// <summary>
        /// Breeding process coroutine
        /// </summary>
        private System.Collections.IEnumerator BreedingProcess()
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < _breedingAnimationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / _breedingAnimationDuration;
                
                _breedingProgress.Value = progress * 100f;
                
                yield return null;
            }
            
            // Complete breeding
            CompleteBreeding();
        }
        
        /// <summary>
        /// Complete the breeding process
        /// </summary>
        private void CompleteBreeding()
        {
            _isBreeding = false;
            _breedingProgress.Value = 100f;
            _breedingStatusLabel.text = "Breeding complete!";
            
            // Use GeneticsManager for proper breeding simulation
            // if (_geneticsManager != null && _selectedParent1 != null && _selectedParent2 != null)
            // {
                // _currentBreeding = _geneticsManager.BreedPlants(_selectedParent1, _selectedParent2, 1);
            // }
            // else
            // {
                // LogWarning("Cannot complete breeding: GeneticsManager or parents not available");
                return;
            // }
            
            // Update UI
            DisplayBreedingResults();
            
            // Update player progression
            _breedingXP += 50;
            _totalBreedings++;
            UpdatePlayerProgression();
            
            PlaySound(_breedingCompleteSound);
            _onBreedingCompleted?.Raise();
            
            _saveStrainButton.SetEnabled(true);
            _discardOffspringButton.SetEnabled(true);
            _startBreedingButton.SetEnabled(true);
        }
        
        /// <summary>
        /// Create temporary plant instance for breeding purposes
        /// </summary>
        private PlantInstance CreateTemporaryPlantInstance(PlantStrainSO strain)
        {
            // Create a temporary GameObject for the PlantInstance
            var tempGameObject = new GameObject($"TempPlant_{strain.StrainName}");
            var tempPlant = tempGameObject.AddComponent<PlantInstance>();
            
            // Initialize the plant instance from the strain
            tempPlant.InitializeFromStrain(strain);
            
            return tempPlant;
        }
        
        /// <summary>
        /// Display breeding results
        /// </summary>
        private void DisplayBreedingResults()
        {
            _offspringResultsContainer.Clear();
            
            if (_currentBreeding?.OffspringGenotypes != null && _currentBreeding.OffspringGenotypes.Count > 0)
            {
                foreach (var offspring in _currentBreeding.OffspringGenotypes)
                {
                    // Create a card showing the genetic result
                    var resultCard = CreateOffspringCard(offspring);
                    _offspringResultsContainer.Add(resultCard);
                }
            }
        }
        
        /// <summary>
        /// Save new strain to library (creates new PlantStrainSO asset)
        /// </summary>
        private void SaveNewStrain(CannabisGenotype offspring)
        {
            if (offspring?.StrainOrigin == null)
            {
                Debug.LogWarning("Cannot save strain: invalid offspring data");
                return;
            }
            
            // Create new PlantStrainSO asset from breeding result
            var newStrain = CreatePlantStrainFromGenotype(offspring);
            if (newStrain != null)
            {
                _availableStrains.Add(newStrain);
                _filteredStrains.Add(newStrain);
                RefreshStrainLibrary();
                
                PlaySound(_newStrainSound);
                _onNewStrainCreated?.Raise();
                
                // Add bonus XP for creating new strain
                _breedingXP += 100;
                UpdatePlayerProgression();
                
                // Show success notification
                // TODO: GameUIManager doesn't exist yet - implement notification system
                // var uiManager = GameManager.Instance?.GetManager<GameUIManager>();
                // if (uiManager != null)
                // {
                //     uiManager.ShowNotification("New Strain Created", $"New strain '{newStrain.StrainName}' added to library!", NotificationType.Success);
                // }
                LogInfo($"Notification: New strain '{newStrain.StrainName}' added to library!");
            }
            
            DiscardOffspring();
        }
        
        /// <summary>
        /// Discard offspring
        /// </summary>
        private void DiscardOffspring()
        {
            _currentBreeding = null;
            _offspringResultsContainer.Clear();
            
            var placeholderLabel = new Label("Complete breeding to see results");
            placeholderLabel.style.fontSize = 14;
            // placeholderLabel.style.color = _uiManager.DesignSystem.ColorPalette.TextSecondary;
            placeholderLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            
            _offspringResultsContainer.Add(placeholderLabel);
            
            _saveStrainButton.SetEnabled(false);
            _discardOffspringButton.SetEnabled(false);
            _breedingProgress.Value = 0f;
            _breedingStatusLabel.text = "Ready for next breeding";
        }
        
        /// <summary>
        /// Apply strain filters
        /// </summary>
        private void ApplyStrainFilters()
        {
            _filteredStrains.Clear();
            
            foreach (var strain in _availableStrains)
            {
                bool passesFilter = true;
                
                // Search filter
                if (!string.IsNullOrEmpty(_strainSearchField.value))
                {
                    var searchTerm = _strainSearchField.value.ToLower();
                    passesFilter = passesFilter && strain.StrainName.ToLower().Contains(searchTerm);
                }
                
                // Type filter
                if (_strainTypeFilter.value != "All")
                {
                    if (_strainTypeFilter.value == "Custom")
                    {
                        // Check if this is a custom bred strain (has parent strains)
                        passesFilter = passesFilter && (strain.ParentStrain1 != null || strain.ParentStrain2 != null);
                    }
                    // else
                    // {
                        passesFilter = passesFilter && strain.StrainType.ToString() == _strainTypeFilter.value;
                    // }
                }
                
                if (passesFilter)
                {
                    _filteredStrains.Add(strain);
                }
            }
            
            RefreshStrainLibrary();
        }
        
        /// <summary>
        /// Play audio clip
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllCoroutines();
            
            // Cleanup events
            _onBreedingCompleted = null;
            _onNewStrainCreated = null;
        }
        
        /// <summary>
        /// Create an offspring result card
        /// </summary>
        private VisualElement CreateOffspringCard(CannabisGenotype offspring)
        {
            var card = new VisualElement();
            card.name = $"offspring-card-{offspring.GenotypeID}";
            
            // Card styling
            card.style.backgroundColor = new Color(0.1f, 0.15f, 0.1f, 1f);
            card.style.borderTopLeftRadius = 8;
            card.style.borderTopRightRadius = 8;
            card.style.borderBottomLeftRadius = 8;
            card.style.borderBottomRightRadius = 8;
            card.style.paddingTop = 12;
            card.style.paddingBottom = 12;
            card.style.paddingLeft = 12;
            card.style.paddingRight = 12;
            card.style.marginBottom = 8;
            card.style.flexDirection = FlexDirection.Column;
            
            // Header
            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.marginBottom = 8;
            
            var nameLabel = new Label($"Offspring {offspring.Generation}");
            nameLabel.style.fontSize = 14;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            headerContainer.Add(nameLabel);
            card.Add(headerContainer);
            
            // Genetic information
            var infoContainer = new VisualElement();
            infoContainer.style.flexDirection = FlexDirection.Column;
            
            var generationLabel = new Label($"Generation: F{offspring.Generation}");
            generationLabel.style.fontSize = 12;
            generationLabel.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            
            var parentageLabel = new Label($"Parents: {offspring.ParentIDs.Count} selected");
            parentageLabel.style.fontSize = 12;
            parentageLabel.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            
            infoContainer.Add(generationLabel);
            infoContainer.Add(parentageLabel);
            card.Add(infoContainer);
            
            return card;
        }
        
        /// <summary>
        /// Create a PlantStrainSO from breeding result
        /// </summary>
        private PlantStrainSO CreatePlantStrainFromGenotype(CannabisGenotype offspring)
        {
            if (offspring?.StrainOrigin == null)
            {
                Debug.LogWarning("Cannot create strain from invalid genotype");
                return null;
            }
            
            // Create a new strain based on the parent strain with modifications
            var newStrain = ScriptableObject.CreateInstance<PlantStrainSO>();
            
            // Copy basic properties from origin strain
            newStrain.name = $"CustomStrain_F{offspring.Generation}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            // newStrain.StrainName = $"{offspring.StrainOrigin.StrainName} F{offspring.Generation}";
            // newStrain.BreederName = "Player";
            // newStrain.IsCustomStrain = true;
            // newStrain.GenerationNumber = offspring.Generation;
            
            // Set parent lineage
            if (_selectedParent1?.Strain != null)
            {
                // newStrain.ParentStrain1 = _selectedParent1.Strain;
            }
            if (_selectedParent2?.Strain != null)
            {
                // newStrain.ParentStrain2 = _selectedParent2.Strain;
            }
            
            // Apply genetic modifications based on offspring genotype
            // This would involve complex genetic calculations in a full implementation
            
            return newStrain;
        }
    }
    
    /// <summary>
    /// UI card for displaying plant strain information
    /// </summary>
    public class PlantStrainCard : VisualElement
    {
        public PlantStrainSO StrainData { get; private set; }
        public bool ShowAsOffspring { get; set; } = false;
        public System.Action<PlantStrainSO> OnStrainSelected;
        
        public PlantStrainCard(PlantStrainSO strainData)
        {
            StrainData = strainData;
            SetupCard();
        }
        
        private void SetupCard()
        {
            name = $"strain-card-{StrainData.GetInstanceID()}";
            
            // Card styling
            style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            style.borderTopLeftRadius = 8;
            style.borderTopRightRadius = 8;
            style.borderBottomLeftRadius = 8;
            style.borderBottomRightRadius = 8;
            style.paddingTop = 12;
            style.paddingBottom = 12;
            style.paddingLeft = 12;
            style.paddingRight = 12;
            style.marginBottom = 8;
            style.flexDirection = FlexDirection.Column;
            
            // Header
            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.marginBottom = 8;
            
            var nameLabel = new Label(StrainData.StrainName);
            nameLabel.style.fontSize = 14;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            
            var rarityColor = StrainData.StrainRarity switch
            {
                StrainRarity.Common => Color.gray,
                StrainRarity.Uncommon => Color.green,
                StrainRarity.Rare => Color.blue,
                StrainRarity.Epic => new Color(0.6f, 0.2f, 0.8f, 1f),
                StrainRarity.Legendary => new Color(1f, 0.6f, 0f, 1f),
                StrainRarity.Custom => new Color(1f, 0.8f, 0.2f, 1f),
                _ => Color.white
            };
            
            var rarityIndicator = new VisualElement();
            rarityIndicator.style.width = 12;
            rarityIndicator.style.height = 12;
            rarityIndicator.style.backgroundColor = rarityColor;
            rarityIndicator.style.borderTopLeftRadius = 6;
            rarityIndicator.style.borderTopRightRadius = 6;
            rarityIndicator.style.borderBottomLeftRadius = 6;
            rarityIndicator.style.borderBottomRightRadius = 6;
            
            headerContainer.Add(nameLabel);
            headerContainer.Add(rarityIndicator);
            
            // Details
            var typeLabel = new Label($"{StrainData.Type}");
            typeLabel.style.fontSize = 12;
            typeLabel.style.color = new Color(0.8f, 0.6f, 0.2f, 1f);
            typeLabel.style.marginBottom = 4;
            
            var statsContainer = new VisualElement();
            statsContainer.style.flexDirection = FlexDirection.Row;
            statsContainer.style.justifyContent = Justify.SpaceBetween;
            
            var thcLabel = new Label($"THC: {StrainData.THCLevel:F1}%");
            thcLabel.style.fontSize = 10;
            thcLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            var cbdLabel = new Label($"CBD: {StrainData.CBDLevel:F1}%");
            cbdLabel.style.fontSize = 10;
            cbdLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            var yieldLabel = new Label($"Yield: {StrainData.YieldPotential:F0}g");
            yieldLabel.style.fontSize = 10;
            yieldLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            statsContainer.Add(thcLabel);
            statsContainer.Add(cbdLabel);
            statsContainer.Add(yieldLabel);
            
            Add(headerContainer);
            Add(typeLabel);
            Add(statsContainer);
            
            // Click handler
            if (!ShowAsOffspring)
            {
                RegisterCallback<ClickEvent>(OnCardClicked);
                
                // Hover effects
                AddToClassList("hoverable");
            }
        }
        
        private void OnCardClicked(ClickEvent evt)
        {
            OnStrainSelected?.Invoke(StrainData);
        }
    }
}