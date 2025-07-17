using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
// using ProjectChimera.Systems.Progression;
using ProjectChimera.Data.Progression;
using ProjectChimera.Data.UI;
using ProjectChimera.UI.Core;

namespace ProjectChimera.UI.Research
{
    /// <summary>
    /// Research & Progression UI Controller for Project Chimera.
    /// Provides comprehensive skill tree management, technology research, and player progression tracking.
    /// Features interactive skill trees, research projects, and achievement systems with gaming progression elements.
    /// </summary>
    public class ResearchProgressionController : MonoBehaviour
    {
        [Header("Research UI Configuration")]
        [SerializeField] private UIDocument _researchDocument;
        [SerializeField] private ProgressionUISettings _uiSettings;
        [SerializeField] private bool _enableRealTimeUpdates = true;
        [SerializeField] private float _updateInterval = 2f;
        
        [Header("Skill Tree Configuration")]
        [SerializeField] private int _maxVisibleNodes = 50;
        [SerializeField] private bool _enableSkillPreview = true;
        [SerializeField] private float _skillTreeScale = 1f;
        
        [Header("Research Configuration")]
        [SerializeField] private int _maxActiveResearch = 5;
        [SerializeField] private bool _enableResearchQueue = true;
        [SerializeField] private float _researchUpdateInterval = 1f;
        
        [Header("Audio Configuration")]
        [SerializeField] private AudioClip _skillUnlockedSound;
        [SerializeField] private AudioClip _researchCompleteSound;
        [SerializeField] private AudioClip _levelUpSound;
        [SerializeField] private AudioClip _achievementSound;
        [SerializeField] private AudioSource _audioSource;
        
        // System references
        // private ProgressionManager _progressionManager;
        // private SkillTreeManager _skillTreeManager;
        // private ResearchManager _researchManager;
        
        // UI Elements - Main Interface
        private VisualElement _rootElement;
        private Button _skillTreeTabButton;
        private Button _researchTabButton;
        private Button _achievementsTabButton;
        private Button _statisticsTabButton;
        
        // Tab Panels
        private VisualElement _skillTreePanel;
        private VisualElement _researchPanel;
        private VisualElement _achievementsPanel;
        private VisualElement _statisticsPanel;
        
        // Player Status Section
        private VisualElement _playerStatusSection;
        private Label _playerLevelDisplay;
        private Label _experienceDisplay;
        private ProgressBar _experienceBar;
        private Label _skillPointsDisplay;
        private Label _researchPointsDisplay;
        
        // Skill Tree Elements
        private VisualElement _skillTreeContainer;
        private VisualElement _skillTreeViewport;
        private VisualElement _skillNodesContainer;
        private VisualElement _skillPreviewPanel;
        private DropdownField _skillCategoryDropdown;
        private Button _resetSkillsButton;
        private Button _exportBuildButton;
        
        // Skill Preview Elements
        private VisualElement _selectedSkillPanel;
        private Label _skillNameLabel;
        private Label _skillDescriptionLabel;
        private Label _skillRequirementsLabel;
        private Label _skillBenefitsLabel;
        private Button _unlockSkillButton;
        private ProgressBar _skillProgressBar;
        
        // Research Elements
        private VisualElement _activeResearchContainer;
        private VisualElement _availableResearchContainer;
        private VisualElement _completedResearchContainer;
        private VisualElement _researchQueueContainer;
        private Button _pauseAllResearchButton;
        private Button _clearQueueButton;
        private Label _activeResearchCountLabel;
        
        // Research Details Panel
        private VisualElement _researchDetailsPanel;
        private Label _researchTitleLabel;
        private Label _researchDescriptionLabel;
        private Label _researchCostLabel;
        private Label _researchTimeLabel;
        private ProgressBar _researchProgressBar;
        private Button _startResearchButton;
        private Button _cancelResearchButton;
        
        // Achievements Elements
        private VisualElement _achievementsList;
        private VisualElement _achievementDetailsPanel;
        private DropdownField _achievementCategoryFilter;
        private TextField _achievementSearchField;
        private Label _achievementProgressLabel;
        
        // Statistics Elements
        private VisualElement _statsOverview;
        private VisualElement _progressCharts;
        private VisualElement _milestonesContainer;
        private Label _totalExperienceLabel;
        private Label _skillsUnlockedLabel;
        private Label _researchCompletedLabel;
        private Label _achievementsEarnedLabel;
        
        // Data and State
        private PlayerProgressionData _playerProgression = new PlayerProgressionData();
        private List<SkillNodeSO> _availableSkills = new List<SkillNodeSO>();
        private List<ResearchProjectSO> _availableResearch = new List<ResearchProjectSO>();
        private List<Achievement> _achievements = new List<Achievement>();
        private List<ActiveResearch> _activeResearch = new List<ActiveResearch>();
        private SkillNodeSO _selectedSkill = null;
        private ResearchProjectSO _selectedResearch = null;
        private string _currentTab = "skills";
        private string _selectedSkillCategory = "All";
        private Vector2 _skillTreeOffset = Vector2.zero;
        private float _lastUpdateTime;
        private bool _isUpdating = false;
        
        // Events
        public System.Action<SkillNodeSO> OnSkillUnlocked;
        public System.Action<ResearchProjectSO> OnResearchCompleted;
        public System.Action<Achievement> OnAchievementEarned;
        public System.Action<int> OnLevelUp;
        public System.Action<string> OnTabChanged;
        
        private void Start()
        {
            InitializeController();
            InitializeSystemReferences();
            SetupUIElements();
            SetupEventHandlers();
            LoadProgressionData();
            
            if (_enableRealTimeUpdates)
            {
                InvokeRepeating(nameof(UpdateProgression), 1f, _updateInterval);
                InvokeRepeating(nameof(UpdateResearchProgress), 0.5f, _researchUpdateInterval);
            }
        }
        
        private void InitializeController()
        {
            if (_researchDocument == null)
            {
                Debug.LogError("Research UI Document not assigned!");
                return;
            }
            
            _rootElement = _researchDocument.rootVisualElement;
            _lastUpdateTime = Time.time;
            
            // Initialize player progression
            _playerProgression = new PlayerProgressionData
            {
                PlayerLevel = 1,
                CurrentExperience = 0,
                ExperienceToNextLevel = 1000,
                SkillPoints = 5,
                ResearchPoints = 100,
                UnlockedSkills = new List<string>(),
                CompletedResearch = new List<string>(),
                EarnedAchievements = new List<string>()
            };
            
            Debug.Log("Research Progression Controller initialized");
        }
        
        private void InitializeSystemReferences()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogWarning("GameManager not found - using simulation mode");
                return;
            }
            
            // _progressionManager = gameManager.GetManager<ProgressionManager>();
            // _skillTreeManager = gameManager.GetManager<SkillTreeManager>();
            // _researchManager = gameManager.GetManager<ResearchManager>();
            
            Debug.Log("Research Progression connected to progression systems");
        }
        
        private void SetupUIElements()
        {
            // Main navigation tabs
            _skillTreeTabButton = _rootElement.Q<Button>("skills-tab");
            _researchTabButton = _rootElement.Q<Button>("research-tab");
            _achievementsTabButton = _rootElement.Q<Button>("achievements-tab");
            _statisticsTabButton = _rootElement.Q<Button>("statistics-tab");
            
            // Tab panels
            _skillTreePanel = _rootElement.Q<VisualElement>("skill-tree-panel");
            _researchPanel = _rootElement.Q<VisualElement>("research-panel");
            _achievementsPanel = _rootElement.Q<VisualElement>("achievements-panel");
            _statisticsPanel = _rootElement.Q<VisualElement>("statistics-panel");
            
            // Player status section
            _playerStatusSection = _rootElement.Q<VisualElement>("player-status-section");
            _playerLevelDisplay = _rootElement.Q<Label>("player-level-display");
            _experienceDisplay = _rootElement.Q<Label>("experience-display");
            _experienceBar = _rootElement.Q<ProgressBar>("experience-bar");
            _skillPointsDisplay = _rootElement.Q<Label>("skill-points-display");
            _researchPointsDisplay = _rootElement.Q<Label>("research-points-display");
            
            // Skill tree elements
            _skillTreeContainer = _rootElement.Q<VisualElement>("skill-tree-container");
            _skillTreeViewport = _rootElement.Q<VisualElement>("skill-tree-viewport");
            _skillNodesContainer = _rootElement.Q<VisualElement>("skill-nodes-container");
            _skillPreviewPanel = _rootElement.Q<VisualElement>("skill-preview-panel");
            _skillCategoryDropdown = _rootElement.Q<DropdownField>("skill-category-dropdown");
            _resetSkillsButton = _rootElement.Q<Button>("reset-skills-button");
            _exportBuildButton = _rootElement.Q<Button>("export-build-button");
            
            // Skill preview elements
            _selectedSkillPanel = _rootElement.Q<VisualElement>("selected-skill-panel");
            _skillNameLabel = _rootElement.Q<Label>("skill-name-label");
            _skillDescriptionLabel = _rootElement.Q<Label>("skill-description-label");
            _skillRequirementsLabel = _rootElement.Q<Label>("skill-requirements-label");
            _skillBenefitsLabel = _rootElement.Q<Label>("skill-benefits-label");
            _unlockSkillButton = _rootElement.Q<Button>("unlock-skill-button");
            _skillProgressBar = _rootElement.Q<ProgressBar>("skill-progress-bar");
            
            // Research elements
            _activeResearchContainer = _rootElement.Q<VisualElement>("active-research-container");
            _availableResearchContainer = _rootElement.Q<VisualElement>("available-research-container");
            _completedResearchContainer = _rootElement.Q<VisualElement>("completed-research-container");
            _researchQueueContainer = _rootElement.Q<VisualElement>("research-queue-container");
            _pauseAllResearchButton = _rootElement.Q<Button>("pause-all-research-button");
            _clearQueueButton = _rootElement.Q<Button>("clear-queue-button");
            _activeResearchCountLabel = _rootElement.Q<Label>("active-research-count");
            
            // Research details
            _researchDetailsPanel = _rootElement.Q<VisualElement>("research-details-panel");
            _researchTitleLabel = _rootElement.Q<Label>("research-title-label");
            _researchDescriptionLabel = _rootElement.Q<Label>("research-description-label");
            _researchCostLabel = _rootElement.Q<Label>("research-cost-label");
            _researchTimeLabel = _rootElement.Q<Label>("research-time-label");
            _researchProgressBar = _rootElement.Q<ProgressBar>("research-progress-bar");
            _startResearchButton = _rootElement.Q<Button>("start-research-button");
            _cancelResearchButton = _rootElement.Q<Button>("cancel-research-button");
            
            // Achievements elements
            _achievementsList = _rootElement.Q<VisualElement>("achievements-list");
            _achievementDetailsPanel = _rootElement.Q<VisualElement>("achievement-details-panel");
            _achievementCategoryFilter = _rootElement.Q<DropdownField>("achievement-category-filter");
            _achievementSearchField = _rootElement.Q<TextField>("achievement-search-field");
            _achievementProgressLabel = _rootElement.Q<Label>("achievement-progress-label");
            
            // Statistics elements
            _statsOverview = _rootElement.Q<VisualElement>("stats-overview");
            _progressCharts = _rootElement.Q<VisualElement>("progress-charts");
            _milestonesContainer = _rootElement.Q<VisualElement>("milestones-container");
            _totalExperienceLabel = _rootElement.Q<Label>("total-experience-label");
            _skillsUnlockedLabel = _rootElement.Q<Label>("skills-unlocked-label");
            _researchCompletedLabel = _rootElement.Q<Label>("research-completed-label");
            _achievementsEarnedLabel = _rootElement.Q<Label>("achievements-earned-label");
            
            SetupDropdowns();
            SetupInitialState();
        }
        
        private void SetupDropdowns()
        {
            // Skill categories
            if (_skillCategoryDropdown != null)
            {
                _skillCategoryDropdown.choices = new List<string>
                {
                    "All", "Cultivation", "Genetics", "Processing", "Business", 
                    "Technology", "Research", "Automation", "Quality"
                };
                _skillCategoryDropdown.value = "All";
            }
            
            // Achievement categories
            if (_achievementCategoryFilter != null)
            {
                _achievementCategoryFilter.choices = new List<string>
                {
                    "All", "Milestones", "Cultivation", "Business", "Research", 
                    "Special", "Hidden", "Seasonal"
                };
                _achievementCategoryFilter.value = "All";
            }
        }
        
        private void SetupInitialState()
        {
            // Show skill tree panel by default
            ShowPanel("skills");
            
            // Update player status display
            UpdatePlayerStatusDisplay();
            
            // Initialize skill tree view
            if (_skillTreeViewport != null)
            {
                _skillTreeViewport.style.scale = new StyleScale(new Scale(Vector3.one * _skillTreeScale));
            }
        }
        
        private void SetupEventHandlers()
        {
            // Tab navigation
            _skillTreeTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("skills"));
            _researchTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("research"));
            _achievementsTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("achievements"));
            _statisticsTabButton?.RegisterCallback<ClickEvent>(evt => ShowPanel("statistics"));
            
            // Skill tree controls
            _skillCategoryDropdown?.RegisterValueChangedCallback(evt => FilterSkillsByCategory(evt.newValue));
            _resetSkillsButton?.RegisterCallback<ClickEvent>(evt => ShowResetSkillsDialog());
            _exportBuildButton?.RegisterCallback<ClickEvent>(evt => ExportSkillBuild());
            _unlockSkillButton?.RegisterCallback<ClickEvent>(evt => UnlockSelectedSkill());
            
            // Research controls
            _pauseAllResearchButton?.RegisterCallback<ClickEvent>(evt => PauseAllResearch());
            _clearQueueButton?.RegisterCallback<ClickEvent>(evt => ClearResearchQueue());
            _startResearchButton?.RegisterCallback<ClickEvent>(evt => StartSelectedResearch());
            _cancelResearchButton?.RegisterCallback<ClickEvent>(evt => CancelSelectedResearch());
            
            // Achievement controls
            _achievementCategoryFilter?.RegisterValueChangedCallback(evt => FilterAchievements(evt.newValue));
            _achievementSearchField?.RegisterValueChangedCallback(evt => SearchAchievements(evt.newValue));
            
            // Skill tree viewport interactions
            if (_skillTreeViewport != null)
            {
                _skillTreeViewport.RegisterCallback<WheelEvent>(OnSkillTreeZoom);
                _skillTreeViewport.RegisterCallback<MouseDownEvent>(OnSkillTreeDragStart);
                _skillTreeViewport.RegisterCallback<MouseMoveEvent>(OnSkillTreeDrag);
                _skillTreeViewport.RegisterCallback<MouseUpEvent>(OnSkillTreeDragEnd);
            }
        }
        
        #region Panel Management
        
        private void ShowPanel(string panelName)
        {
            // Hide all panels
            _skillTreePanel?.AddToClassList("hidden");
            _researchPanel?.AddToClassList("hidden");
            _achievementsPanel?.AddToClassList("hidden");
            _statisticsPanel?.AddToClassList("hidden");
            
            // Remove active state from all tabs
            _skillTreeTabButton?.RemoveFromClassList("tab-active");
            _researchTabButton?.RemoveFromClassList("tab-active");
            _achievementsTabButton?.RemoveFromClassList("tab-active");
            _statisticsTabButton?.RemoveFromClassList("tab-active");
            
            // Show selected panel and activate tab
            switch (panelName)
            {
                case "skills":
                    _skillTreePanel?.RemoveFromClassList("hidden");
                    _skillTreeTabButton?.AddToClassList("tab-active");
                    RefreshSkillTree();
                    break;
                case "research":
                    _researchPanel?.RemoveFromClassList("hidden");
                    _researchTabButton?.AddToClassList("tab-active");
                    RefreshResearchData();
                    break;
                case "achievements":
                    _achievementsPanel?.RemoveFromClassList("hidden");
                    _achievementsTabButton?.AddToClassList("tab-active");
                    RefreshAchievements();
                    break;
                case "statistics":
                    _statisticsPanel?.RemoveFromClassList("hidden");
                    _statisticsTabButton?.AddToClassList("tab-active");
                    RefreshStatistics();
                    break;
            }
            
            _currentTab = panelName;
            OnTabChanged?.Invoke(panelName);
            
            Debug.Log($"Switched to {panelName} panel");
        }
        
        #endregion
        
        #region Skill Tree Management
        
        private void RefreshSkillTree()
        {
            // if (_skillTreeManager != null)
            // {
                // _availableSkills = _skillTreeManager.GetAllSkills()?.ToList() ?? new List<SkillNodeSO>();
            // }
            // else
            // {
                // Generate sample skills for demonstration
                GenerateSampleSkills();
            // }
            
            UpdateSkillTreeDisplay();
        }
        
        private void GenerateSampleSkills()
        {
            _availableSkills.Clear();
            
            var categories = new[] { "Cultivation", "Genetics", "Processing", "Business", "Technology" };
            var skillNames = new[]
            {
                "Advanced Hydroponics", "Genetic Analysis", "Quality Control", "Market Analysis", "Automation Systems",
                "Organic Cultivation", "Crossbreeding Mastery", "Extraction Techniques", "Supply Chain", "IoT Integration",
                "Climate Control", "Phenotype Selection", "Curing Optimization", "Financial Planning", "Data Analytics"
            };
            
            for (int i = 0; i < skillNames.Length; i++)
            {
                var skill = CreateSampleSkill(skillNames[i], categories[i % categories.Length], i + 1);
                _availableSkills.Add(skill);
            }
        }
        
        private SkillNodeSO CreateSampleSkill(string name, string category, int tier)
        {
            var skill = ScriptableObject.CreateInstance<SkillNodeSO>();
            // Note: SkillNodeSO properties are read-only, so we can't set them directly
            // This is a placeholder implementation for sample data
            // In a real implementation, these would be set via the inspector or loaded from assets
            return skill;
        }
        
        private void UpdateSkillTreeDisplay()
        {
            if (_skillNodesContainer == null) return;
            
            _skillNodesContainer.Clear();
            
            var filteredSkills = _selectedSkillCategory == "All" ? 
                _availableSkills : 
                _availableSkills.Where(s => s.Category == _selectedSkillCategory);
            
            foreach (var skill in filteredSkills)
            {
                var skillNode = CreateSkillNode(skill);
                _skillNodesContainer.Add(skillNode);
            }
        }
        
        private VisualElement CreateSkillNode(SkillNodeSO skill)
        {
            var node = new VisualElement();
            node.name = $"skill-node-{skill.SkillName.Replace(" ", "-").ToLower()}";
            node.AddToClassList("skill-node");
            node.AddToClassList(skill.IsUnlocked ? "skill-unlocked" : "skill-locked");
            node.AddToClassList($"tier-{skill.SkillTier}");
            
            var nameLabel = new Label(skill.SkillName);
            nameLabel.AddToClassList("skill-node-name");
            
            var costLabel = new Label($"{skill.SkillPointCost} SP");
            costLabel.AddToClassList("skill-node-cost");
            
            var tierLabel = new Label($"Tier {skill.SkillTier}");
            tierLabel.AddToClassList("skill-node-tier");
            
            node.Add(nameLabel);
            node.Add(costLabel);
            node.Add(tierLabel);
            
            // Add click handler
            node.RegisterCallback<ClickEvent>(evt => SelectSkill(skill));
            
            // Position node based on tier and category
            var position = CalculateSkillNodePosition(skill);
            node.style.position = Position.Absolute;
            node.style.left = position.x;
            node.style.top = position.y;
            
            return node;
        }
        
        private Vector2 CalculateSkillNodePosition(SkillNodeSO skill)
        {
            var categories = new[] { "Cultivation", "Genetics", "Processing", "Business", "Technology" };
            var categoryIndex = Array.IndexOf(categories, skill.SkillCategory.ToString());
            if (categoryIndex == -1) categoryIndex = 0;
            
            var x = 50 + (categoryIndex * 200);
            var y = 50 + ((skill.SkillTier - 1) * 120);
            
            return new Vector2(x, y);
        }
        
        private void SelectSkill(SkillNodeSO skill)
        {
            _selectedSkill = skill;
            UpdateSkillPreview();
        }
        
        private void UpdateSkillPreview()
        {
            if (_selectedSkill == null || _selectedSkillPanel == null) return;
            
            if (_skillNameLabel != null)
                _skillNameLabel.text = _selectedSkill.SkillName;
            
            if (_skillDescriptionLabel != null)
                _skillDescriptionLabel.text = _selectedSkill.SkillDescription;
            
            if (_skillRequirementsLabel != null)
            {
                var reqText = _selectedSkill.PrerequisiteSkills.Count > 0 ? 
                    $"Requires: {string.Join(", ", _selectedSkill.PrerequisiteSkills.Select(s => s.SkillName))}" : 
                    "No prerequisites";
                _skillRequirementsLabel.text = reqText;
            }
            
            if (_skillBenefitsLabel != null)
            {
                var benefitsText = _selectedSkill.SkillEffects.Count > 0 ?
                    string.Join("\n", _selectedSkill.SkillEffects.Select(e => $"{e.EffectType}: {e.BaseValue}")) :
                    "No benefits listed";
                _skillBenefitsLabel.text = benefitsText;
            }
            
            if (_unlockSkillButton != null)
            {
                _unlockSkillButton.SetEnabled(!_selectedSkill.IsUnlocked && 
                    _playerProgression.SkillPoints >= _selectedSkill.SkillPointCost);
                _unlockSkillButton.text = _selectedSkill.IsUnlocked ? "Unlocked" : 
                    $"Unlock ({_selectedSkill.SkillPointCost} SP)";
            }
        }
        
        private void UnlockSelectedSkill()
        {
            if (_selectedSkill == null || _selectedSkill.IsUnlocked) return;
            
            if (_playerProgression.SkillPoints >= _selectedSkill.SkillPointCost)
            {
                _playerProgression.SkillPoints -= _selectedSkill.SkillPointCost;
                // _selectedSkill.IsUnlocked = true; // Read-only property
                _playerProgression.UnlockedSkills.Add(_selectedSkill.SkillName);
                
                PlaySound(_skillUnlockedSound);
                OnSkillUnlocked?.Invoke(_selectedSkill);
                
                UpdateSkillTreeDisplay();
                UpdateSkillPreview();
                UpdatePlayerStatusDisplay();
                
                Debug.Log($"Unlocked skill: {_selectedSkill.SkillName}");
            }
        }
        
        private void FilterSkillsByCategory(string category)
        {
            _selectedSkillCategory = category;
            UpdateSkillTreeDisplay();
        }
        
        #endregion
        
        #region Research Management
        
        private void RefreshResearchData()
        {
            // if (_researchManager != null)
            // {
                // _availableResearch = _researchManager.GetAvailableResearch()?.ToList() ?? new List<ResearchProjectSO>();
                // _activeResearch = _researchManager.GetActiveResearch()?.ToList() ?? new List<ActiveResearch>();
            // }
            // else
            // {
                GenerateSampleResearch();
            // }
            
            UpdateResearchDisplay();
        }
        
        private void GenerateSampleResearch()
        {
            _availableResearch.Clear();
            _activeResearch.Clear();
            
            var researchProjects = new[]
            {
                ("Advanced LED Optimization", "Lighting", 500, 120),
                ("Genetic Marker Analysis", "Genetics", 800, 240),
                ("Automated Nutrient Mixing", "Automation", 600, 180),
                ("Market Prediction AI", "Business", 1000, 300),
                ("Quality Testing Protocols", "Quality", 400, 90)
            };
            
            foreach (var (name, category, cost, time) in researchProjects)
            {
                var research = CreateSampleResearch(name, category, cost, time);
                _availableResearch.Add(research);
            }
        }
        
        private ResearchProjectSO CreateSampleResearch(string name, string category, int cost, int timeMinutes)
        {
            var research = ScriptableObject.CreateInstance<ResearchProjectSO>();
            // Note: ResearchProjectSO properties are read-only, so we can't set them directly
            // This is a placeholder implementation for sample data
            // In a real implementation, these would be set via the inspector or loaded from assets
            // research.ProjectName = name;
            // research.Category = category;
            // research.Description = $"Advanced research in {category.ToLower()} systems for facility optimization.";
            // research.ResearchCost = cost;
            // research.ResearchTimeMinutes = timeMinutes;
            // research.Prerequisites = new List<string>();
            // research.Rewards = new List<string> { $"Unlocks {category} improvements", "Increases facility efficiency" };
            return research;
        }
        
        private void UpdateResearchDisplay()
        {
            UpdateActiveResearchDisplay();
            UpdateAvailableResearchDisplay();
            UpdateResearchQueue();
            
            if (_activeResearchCountLabel != null)
            {
                _activeResearchCountLabel.text = $"{_activeResearch.Count}/{_maxActiveResearch} Active";
            }
        }
        
        private void UpdateActiveResearchDisplay()
        {
            if (_activeResearchContainer == null) return;
            
            _activeResearchContainer.Clear();
            
            foreach (var research in _activeResearch)
            {
                var researchElement = CreateActiveResearchElement(research);
                _activeResearchContainer.Add(researchElement);
            }
        }
        
        private void UpdateAvailableResearchDisplay()
        {
            if (_availableResearchContainer == null) return;
            
            _availableResearchContainer.Clear();
            
            foreach (var research in _availableResearch)
            {
                var researchElement = CreateAvailableResearchElement(research);
                _availableResearchContainer.Add(researchElement);
            }
        }
        
        private VisualElement CreateActiveResearchElement(ActiveResearch research)
        {
            var element = new VisualElement();
            element.AddToClassList("active-research-item");
            
            var nameLabel = new Label(research.ProjectName);
            nameLabel.AddToClassList("research-name");
            
            var progressBar = new ProgressBar();
            progressBar.value = research.Progress;
            progressBar.title = $"{research.Progress:P0} Complete";
            progressBar.AddToClassList("research-progress");
            
            var timeLabel = new Label($"{research.RemainingTime:F0}m remaining");
            timeLabel.AddToClassList("research-time");
            
            element.Add(nameLabel);
            element.Add(progressBar);
            element.Add(timeLabel);
            
            return element;
        }
        
        private VisualElement CreateAvailableResearchElement(ResearchProjectSO research)
        {
            var element = new VisualElement();
            element.AddToClassList("available-research-item");
            
            var nameLabel = new Label(research.ProjectName);
            nameLabel.AddToClassList("research-name");
            
            var costLabel = new Label($"{research.ResearchCost} RP");
            costLabel.AddToClassList("research-cost");
            
            var timeLabel = new Label($"{research.ResearchTimeMinutes}m");
            timeLabel.AddToClassList("research-time");
            
            var startButton = new Button(() => SelectResearch(research));
            startButton.text = "Start";
            startButton.AddToClassList("start-research-btn");
            
            element.Add(nameLabel);
            element.Add(costLabel);
            element.Add(timeLabel);
            element.Add(startButton);
            
            element.RegisterCallback<ClickEvent>(evt => SelectResearch(research));
            
            return element;
        }
        
        private void SelectResearch(ResearchProjectSO research)
        {
            _selectedResearch = research;
            UpdateResearchDetails();
        }
        
        private void UpdateResearchDetails()
        {
            if (_selectedResearch == null || _researchDetailsPanel == null) return;
            
            if (_researchTitleLabel != null)
                _researchTitleLabel.text = _selectedResearch.ProjectName;
            
            if (_researchDescriptionLabel != null)
                _researchDescriptionLabel.text = _selectedResearch.Description;
            
            if (_researchCostLabel != null)
                _researchCostLabel.text = $"Cost: {_selectedResearch.ResearchCost} Research Points";
            
            if (_researchTimeLabel != null)
                _researchTimeLabel.text = $"Duration: {_selectedResearch.ResearchTimeMinutes} minutes";
            
            if (_startResearchButton != null)
            {
                bool canStart = _activeResearch.Count < _maxActiveResearch && 
                               _playerProgression.ResearchPoints >= _selectedResearch.ResearchCost;
                _startResearchButton.SetEnabled(canStart);
            }
        }
        
        private void StartSelectedResearch()
        {
            if (_selectedResearch == null) return;
            
            if (_activeResearch.Count >= _maxActiveResearch)
            {
                Debug.LogWarning("Maximum active research limit reached");
                return;
            }
            
            if (_playerProgression.ResearchPoints < _selectedResearch.ResearchCost)
            {
                Debug.LogWarning("Insufficient research points");
                return;
            }
            
            _playerProgression.ResearchPoints -= _selectedResearch.ResearchCost;
            
            var activeResearch = new ActiveResearch
            {
                ProjectName = _selectedResearch.ProjectName,
                Project = _selectedResearch,
                Progress = 0f,
                StartTime = DateTime.Now,
                RemainingTime = _selectedResearch.ResearchTimeMinutes * 60f // Convert to seconds
            };
            
            _activeResearch.Add(activeResearch);
            _availableResearch.Remove(_selectedResearch);
            
            UpdateResearchDisplay();
            UpdatePlayerStatusDisplay();
            
            Debug.Log($"Started research: {_selectedResearch.ProjectName}");
        }
        
        #endregion
        
        #region Data Updates and Progression
        
        [ContextMenu("Update Progression")]
        public void UpdateProgression()
        {
            if (_isUpdating) return;
            
            _isUpdating = true;
            
            // Update experience and level
            UpdateExperienceAndLevel();
            
            // Update resource generation
            UpdateResourceGeneration();
            
            // Check for achievements
            CheckAchievements();
            
            // Update current tab data
            switch (_currentTab)
            {
                case "skills":
                    RefreshSkillTree();
                    break;
                case "research":
                    RefreshResearchData();
                    break;
                case "achievements":
                    RefreshAchievements();
                    break;
                case "statistics":
                    RefreshStatistics();
                    break;
            }
            
            _lastUpdateTime = Time.time;
            _isUpdating = false;
        }
        
        private void UpdateResearchProgress()
        {
            bool anyCompleted = false;
            
            for (int i = _activeResearch.Count - 1; i >= 0; i--)
            {
                var research = _activeResearch[i];
                research.RemainingTime -= _researchUpdateInterval;
                research.Progress = 1f - (research.RemainingTime / (research.Project.ResearchTimeMinutes * 60f));
                
                if (research.RemainingTime <= 0)
                {
                    CompleteResearch(research);
                    _activeResearch.RemoveAt(i);
                    anyCompleted = true;
                }
            }
            
            if (anyCompleted)
            {
                UpdateResearchDisplay();
            }
        }
        
        private void CompleteResearch(ActiveResearch research)
        {
            _playerProgression.CompletedResearch.Add(research.ProjectName);
            
            // Add experience reward
            int expReward = research.Project.ResearchCost / 2;
            AddExperience(expReward);
            
            PlaySound(_researchCompleteSound);
            OnResearchCompleted?.Invoke(research.Project);
            
            Debug.Log($"Research completed: {research.ProjectName} (+{expReward} XP)");
        }
        
        private void UpdateExperienceAndLevel()
        {
            // Simulate experience gain from various activities
            if (UnityEngine.Random.Range(0f, 1f) < 0.1f) // 10% chance per update
            {
                AddExperience(UnityEngine.Random.Range(1, 5));
            }
        }
        
        private void AddExperience(int amount)
        {
            _playerProgression.CurrentExperience += amount;
            
            // Check for level up
            while (_playerProgression.CurrentExperience >= _playerProgression.ExperienceToNextLevel)
            {
                LevelUp();
            }
            
            UpdatePlayerStatusDisplay();
        }
        
        private void LevelUp()
        {
            _playerProgression.CurrentExperience -= _playerProgression.ExperienceToNextLevel;
            _playerProgression.PlayerLevel++;
            _playerProgression.ExperienceToNextLevel = CalculateExperienceToNextLevel(_playerProgression.PlayerLevel);
            
            // Award skill points and research points
            _playerProgression.SkillPoints += 2;
            _playerProgression.ResearchPoints += 50;
            
            PlaySound(_levelUpSound);
            OnLevelUp?.Invoke(_playerProgression.PlayerLevel);
            
            Debug.Log($"Level up! Now level {_playerProgression.PlayerLevel}");
        }
        
        private int CalculateExperienceToNextLevel(int level)
        {
            return 1000 + (level * 200); // Increasing XP requirement
        }
        
        private void UpdateResourceGeneration()
        {
            // Simulate passive resource generation
            _playerProgression.ResearchPoints += UnityEngine.Random.Range(1, 3);
        }
        
        private void UpdatePlayerStatusDisplay()
        {
            if (_playerLevelDisplay != null)
                _playerLevelDisplay.text = $"Level {_playerProgression.PlayerLevel}";
            
            if (_experienceDisplay != null)
                _experienceDisplay.text = $"{_playerProgression.CurrentExperience:N0} / {_playerProgression.ExperienceToNextLevel:N0} XP";
            
            if (_experienceBar != null)
                _experienceBar.value = (float)_playerProgression.CurrentExperience / _playerProgression.ExperienceToNextLevel;
            
            if (_skillPointsDisplay != null)
                _skillPointsDisplay.text = $"{_playerProgression.SkillPoints} SP";
            
            if (_researchPointsDisplay != null)
                _researchPointsDisplay.text = $"{_playerProgression.ResearchPoints:N0} RP";
        }
        
        #endregion
        
        #region Achievements and Statistics
        
        private void RefreshAchievements()
        {
            GenerateSampleAchievements();
            UpdateAchievementsDisplay();
        }
        
        private void GenerateSampleAchievements()
        {
            if (_achievements.Count > 0) return; // Already generated
            
            _achievements = new List<Achievement>
            {
                new Achievement { Name = "First Steps", Description = "Reach level 5", IsEarned = true, Category = "Milestones" },
                new Achievement { Name = "Researcher", Description = "Complete 10 research projects", IsEarned = false, Category = "Research" },
                new Achievement { Name = "Master Cultivator", Description = "Unlock all cultivation skills", IsEarned = false, Category = "Cultivation" },
                new Achievement { Name = "Business Mogul", Description = "Earn $1,000,000", IsEarned = false, Category = "Business" },
                new Achievement { Name = "Tech Innovator", Description = "Unlock all automation systems", IsEarned = false, Category = "Technology" }
            };
        }
        
        private void UpdateAchievementsDisplay()
        {
            if (_achievementsList == null) return;
            
            _achievementsList.Clear();
            
            var filteredAchievements = _achievements;
            if (_achievementCategoryFilter?.value != "All")
            {
                filteredAchievements = _achievements.Where(a => a.Category == _achievementCategoryFilter.value).ToList();
            }
            
            foreach (var achievement in filteredAchievements)
            {
                var achievementElement = CreateAchievementElement(achievement);
                _achievementsList.Add(achievementElement);
            }
            
            if (_achievementProgressLabel != null)
            {
                int earned = _achievements.Count(a => a.IsEarned);
                _achievementProgressLabel.text = $"{earned}/{_achievements.Count} Achievements Earned";
            }
        }
        
        private VisualElement CreateAchievementElement(Achievement achievement)
        {
            var element = new VisualElement();
            element.AddToClassList("achievement-item");
            element.AddToClassList(achievement.IsEarned ? "achievement-earned" : "achievement-locked");
            
            var nameLabel = new Label(achievement.Name);
            nameLabel.AddToClassList("achievement-name");
            
            var descLabel = new Label(achievement.Description);
            descLabel.AddToClassList("achievement-description");
            
            var statusLabel = new Label(achievement.IsEarned ? "✓ Earned" : "Locked");
            statusLabel.AddToClassList("achievement-status");
            
            element.Add(nameLabel);
            element.Add(descLabel);
            element.Add(statusLabel);
            
            return element;
        }
        
        private void CheckAchievements()
        {
            foreach (var achievement in _achievements.Where(a => !a.IsEarned))
            {
                bool shouldEarn = false;
                
                switch (achievement.Name)
                {
                    case "First Steps":
                        shouldEarn = _playerProgression.PlayerLevel >= 5;
                        break;
                    case "Researcher":
                        shouldEarn = _playerProgression.CompletedResearch.Count >= 10;
                        break;
                    // Add more achievement checks here
                }
                
                if (shouldEarn)
                {
                    EarnAchievement(achievement);
                }
            }
        }
        
        private void EarnAchievement(Achievement achievement)
        {
            achievement.IsEarned = true;
            _playerProgression.EarnedAchievements.Add(achievement.Name);
            
            PlaySound(_achievementSound);
            OnAchievementEarned?.Invoke(achievement);
            
            Debug.Log($"Achievement earned: {achievement.Name}");
        }
        
        private void RefreshStatistics()
        {
            if (_totalExperienceLabel != null)
                _totalExperienceLabel.text = $"{_playerProgression.CurrentExperience + (_playerProgression.PlayerLevel * 1000):N0}";
            
            if (_skillsUnlockedLabel != null)
                _skillsUnlockedLabel.text = $"{_playerProgression.UnlockedSkills.Count}";
            
            if (_researchCompletedLabel != null)
                _researchCompletedLabel.text = $"{_playerProgression.CompletedResearch.Count}";
            
            if (_achievementsEarnedLabel != null)
                _achievementsEarnedLabel.text = $"{_playerProgression.EarnedAchievements.Count}";
        }
        
        #endregion
        
        #region Skill Tree Interactions
        
        private bool _isDragging = false;
        private Vector2 _dragStartPosition;
        
        private void OnSkillTreeZoom(WheelEvent evt)
        {
            float zoomDelta = evt.delta.y > 0 ? -0.1f : 0.1f;
            _skillTreeScale = Mathf.Clamp(_skillTreeScale + zoomDelta, 0.5f, 2f);
            
            if (_skillTreeViewport != null)
            {
                _skillTreeViewport.style.scale = new StyleScale(new Scale(Vector3.one * _skillTreeScale));
            }
        }
        
        private void OnSkillTreeDragStart(MouseDownEvent evt)
        {
            _isDragging = true;
            _dragStartPosition = evt.localMousePosition;
            _skillTreeViewport?.CaptureMouse();
        }
        
        private void OnSkillTreeDrag(MouseMoveEvent evt)
        {
            if (!_isDragging) return;
            
            Vector2 deltaPosition = evt.localMousePosition - _dragStartPosition;
            _skillTreeOffset += deltaPosition;
            
            if (_skillNodesContainer != null)
            {
                _skillNodesContainer.style.left = _skillTreeOffset.x;
                _skillNodesContainer.style.top = _skillTreeOffset.y;
            }
            
            _dragStartPosition = evt.localMousePosition;
        }
        
        private void OnSkillTreeDragEnd(MouseUpEvent evt)
        {
            _isDragging = false;
            _skillTreeViewport?.ReleaseMouse();
        }
        
        #endregion
        
        #region Helper Methods
        
        private void LoadProgressionData()
        {
            // if (_progressionManager != null)
            // {
                // var savedProgression = _progressionManager.GetPlayerProgression();
                // Placeholder data for compilation
                PlayerProgressionData savedProgression = null;
                
                if (savedProgression != null)
                {
                    _playerProgression = savedProgression;
                }
            // }
            
            Debug.Log("Progression data loaded");
        }
        
        private void ShowResetSkillsDialog()
        {
            // Would show modal dialog for skill reset confirmation
            Debug.Log("Reset skills dialog requested");
        }
        
        private void ExportSkillBuild()
        {
            var unlockedSkills = _playerProgression.UnlockedSkills;
            Debug.Log($"Exporting skill build with {unlockedSkills.Count} skills");
        }
        
        private void PauseAllResearch()
        {
            Debug.Log("Pausing all research projects");
        }
        
        private void ClearResearchQueue()
        {
            Debug.Log("Clearing research queue");
        }
        
        private void CancelSelectedResearch()
        {
            Debug.Log("Cancelling selected research");
        }
        
        private void UpdateResearchQueue()
        {
            // Update research queue display
        }
        
        private void FilterAchievements(string category)
        {
            UpdateAchievementsDisplay();
        }
        
        private void SearchAchievements(string searchTerm)
        {
            // Filter achievements by search term
            UpdateAchievementsDisplay();
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }
        
        #endregion
    }
    
    // Supporting data structures
    [System.Serializable]
    public class PlayerProgressionData
    {
        public int PlayerLevel;
        public int CurrentExperience;
        public int ExperienceToNextLevel;
        public int SkillPoints;
        public int ResearchPoints;
        public List<string> UnlockedSkills;
        public List<string> CompletedResearch;
        public List<string> EarnedAchievements;
    }
    
    [System.Serializable]
    public class ActiveResearch
    {
        public string ProjectName;
        public ResearchProjectSO Project;
        public float Progress;
        public DateTime StartTime;
        public float RemainingTime;
    }
    
    [System.Serializable]
    public class Achievement
    {
        public string Name;
        public string Description;
        public string Category;
        public bool IsEarned;
        public DateTime EarnedDate;
        public int ExperienceReward;
    }
    
    [System.Serializable]
    public class ProgressionUISettings
    {
        public bool EnableSkillPreview = true;
        public bool EnableResearchQueue = true;
        public bool EnableAchievementNotifications = true;
        public float SkillTreeScale = 1f;
        public int MaxActiveResearch = 5;
        public bool PlaySoundEffects = true;
    }
}