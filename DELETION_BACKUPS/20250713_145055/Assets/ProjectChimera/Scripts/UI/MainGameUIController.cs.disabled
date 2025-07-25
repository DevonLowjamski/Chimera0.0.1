using UnityEngine;
using UnityEngine.UI;
using ProjectChimera.Core;
using CultivationSystems = ProjectChimera.Systems.Cultivation;
using EconomySystems = ProjectChimera.Systems.Economy;
using CommunitySystems = ProjectChimera.Systems.Community;
using ConstructionSystems = ProjectChimera.Systems.Construction;
using TutorialSystems = ProjectChimera.Systems.Tutorial;
using ProjectChimera.Scripts.Facilities;
using ProjectChimera.Data.Facilities;
using ProjectChimera.Data.Community;
using ProjectChimera.Data.Construction;
using ProjectChimera.Data.Economy;
using TMPro;
// Explicit aliases to resolve ambiguity
using PlantInstance = ProjectChimera.Systems.Cultivation.PlantInstance;
using HarvestResult = ProjectChimera.Data.Facilities.HarvestResult;
using InteractivePlantComponent = ProjectChimera.Systems.Cultivation.InteractivePlantComponent;
using PlayerProfile = ProjectChimera.Data.PlayerProfile;
using GrowRoomController = ProjectChimera.Scripts.Facilities.GrowRoomController;

namespace ProjectChimera.UI
{
    /// <summary>
    /// Main UI controller that manages the game's primary interface elements.
    /// Connects UI elements to game managers and handles user interactions.
    /// </summary>
    public class MainGameUIController : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _hudPanel;
        [SerializeField] private GameObject _sidePanel;
        
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private TextMeshProUGUI _reputationText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Slider _experienceSlider;
        [SerializeField] private TextMeshProUGUI _timeText;
        
        [Header("Action Buttons")]
        [SerializeField] private Button _plantButton;
        [SerializeField] private Button _harvestButton;
        [SerializeField] private Button _geneticsButton;
        [SerializeField] private Button _marketButton;
        [SerializeField] private Button _constructionButton;
        [SerializeField] private Button _tutorialButton;
        
        [Header("Info Panels")]
        [SerializeField] private GameObject _plantInfoPanel;
        [SerializeField] private TextMeshProUGUI _plantInfoText;
        [SerializeField] private GameObject _facilityInfoPanel;
        [SerializeField] private TextMeshProUGUI _facilityInfoText;
        
        [Header("Notification System")]
        [SerializeField] private GameObject _notificationPrefab;
        [SerializeField] private Transform _notificationContainer;
        [SerializeField] private float _notificationDuration = 3f;
        
        // Manager References
        private CultivationSystems.PlantManager _plantManager;
        private EconomySystems.CurrencyManager _currencyManager;
        private CommunitySystems.CommunityManager _communityManager;
        private ConstructionSystems.ConstructionManager _constructionManager;
        private TutorialSystems.EnhancedTutorialManager _tutorialManager;
        private TimeManager _timeManager;
        
        // UI State
        private InteractivePlantComponent _selectedPlant;
        private GrowRoomController _selectedRoom;
        private bool _isPanelOpen = false;
        
        // Update intervals
        private float _uiUpdateInterval = 0.5f; // Update UI twice per second
        private float _lastUIUpdate = 0f;
        
        private void Start()
        {
            InitializeManagers();
            InitializeUIElements();
            SetupEventListeners();
            UpdateUI();
        }
        
        private void Update()
        {
            if (Time.time - _lastUIUpdate >= _uiUpdateInterval)
            {
                UpdateUI();
                _lastUIUpdate = Time.time;
            }
        }
        
        #region Initialization
        
        private void InitializeManagers()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found - UI cannot function properly");
                return;
            }
            
            _plantManager = gameManager.GetManager<CultivationSystems.PlantManager>();
            _currencyManager = gameManager.GetManager<EconomySystems.CurrencyManager>();
            _communityManager = gameManager.GetManager<CommunitySystems.CommunityManager>();
            _constructionManager = gameManager.GetManager<ConstructionSystems.ConstructionManager>();
            _tutorialManager = gameManager.GetManager<TutorialSystems.EnhancedTutorialManager>();
            _timeManager = gameManager.GetManager<TimeManager>();
            
            // Log warnings for missing managers
            if (_plantManager == null) Debug.LogWarning("PlantManager not found");
            if (_currencyManager == null) Debug.LogWarning("CurrencyManager not found");
            if (_communityManager == null) Debug.LogWarning("CommunityManager not found");
            if (_constructionManager == null) Debug.LogWarning("ConstructionManager not found");
            if (_tutorialManager == null) Debug.LogWarning("TutorialManager not found");
            if (_timeManager == null) Debug.LogWarning("TimeManager not found");
        }
        
        private void InitializeUIElements()
        {
            // Initialize panels
            if (_sidePanel != null)
                _sidePanel.SetActive(false);
            
            if (_plantInfoPanel != null)
                _plantInfoPanel.SetActive(false);
            
            if (_facilityInfoPanel != null)
                _facilityInfoPanel.SetActive(false);
            
            // Set initial text values
            UpdateCurrencyDisplay();
            UpdatePlayerStats();
            UpdateTimeDisplay();
        }
        
        private void SetupEventListeners()
        {
            // Action Button Events
            if (_plantButton != null)
                _plantButton.onClick.AddListener(OnPlantButtonClicked);
            
            if (_harvestButton != null)
                _harvestButton.onClick.AddListener(OnHarvestButtonClicked);
            
            if (_geneticsButton != null)
                _geneticsButton.onClick.AddListener(OnGeneticsButtonClicked);
            
            if (_marketButton != null)
                _marketButton.onClick.AddListener(OnMarketButtonClicked);
            
            if (_constructionButton != null)
                _constructionButton.onClick.AddListener(OnConstructionButtonClicked);
            
            if (_tutorialButton != null)
                _tutorialButton.onClick.AddListener(OnTutorialButtonClicked);
            
            // Manager Events
            if (_currencyManager != null)
            {
                _currencyManager.OnCurrencyChanged += OnCurrencyChanged;
            }
            
            if (_communityManager != null)
            {
                _communityManager.OnReputationChanged += OnReputationChanged;
                _communityManager.OnPlayerProfileUpdated += OnPlayerProfileUpdated;
            }
            
            if (_plantManager != null)
            {
                _plantManager.OnPlantHarvested += OnPlantHarvested;
            }
            
            // Plant Selection Events
            var plantComponents = UnityEngine.Object.FindObjectsByType<InteractivePlantComponent>(FindObjectsSortMode.None);
            foreach (var plant in plantComponents)
            {
                plant.OnPlantClicked += OnPlantSelected;
                plant.OnPlantHovered += OnPlantHovered;
            }
            
            // Room Selection Events
            var roomControllers = UnityEngine.Object.FindObjectsByType<GrowRoomController>(FindObjectsSortMode.None);
            foreach (var room in roomControllers)
            {
                room.OnRoomStatusChanged += OnRoomStatusChanged;
            }
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateUI()
        {
            UpdateCurrencyDisplay();
            UpdatePlayerStats();
            UpdateTimeDisplay();
            UpdateActionButtons();
            UpdateSelectedPlantInfo();
            UpdateSelectedRoomInfo();
        }
        
        private void UpdateCurrencyDisplay()
        {
            if (_currencyText != null && _currencyManager != null)
            {
                float cash = _currencyManager.GetBalance(CurrencyType.Cash);
                _currencyText.text = $"Cash: ${cash:F0}";
            }
        }
        
        private void UpdatePlayerStats()
        {
            if (_communityManager?.CurrentPlayer != null)
            {
                var player = _communityManager.CurrentPlayer;
                
                if (_reputationText != null)
                    _reputationText.text = $"Reputation: {player.ReputationPoints}";
                
                if (_levelText != null)
                    _levelText.text = $"Level: {player.Level}";
                
                if (_experienceSlider != null)
                {
                    int currentLevelExp = (player.Level - 1) * 1000;
                    int nextLevelExp = player.Level * 1000;
                    float progress = (float)(player.Experience - currentLevelExp) / (nextLevelExp - currentLevelExp);
                    _experienceSlider.value = progress;
                }
            }
        }
        
        private void UpdateTimeDisplay()
        {
            if (_timeText != null && _timeManager != null)
            {
                var gameTime = _timeManager.TotalGameTime;
                _timeText.text = gameTime.ToString("MMM dd, yyyy HH:mm");
            }
        }
        
        private void UpdateActionButtons()
        {
            // Update button states based on game state
            if (_harvestButton != null)
            {
                bool hasHarvestablePlants = _plantManager?.GetHarvestablePlants()?.Count > 0;
                _harvestButton.interactable = hasHarvestablePlants;
            }
            
            if (_constructionButton != null)
            {
                bool canAffordConstruction = _currencyManager?.GetBalance(CurrencyType.Cash) >= 1000f;
                _constructionButton.interactable = canAffordConstruction;
            }
        }
        
        private void UpdateSelectedPlantInfo()
        {
            if (_selectedPlant != null && _plantInfoPanel != null && _plantInfoText != null)
            {
                var status = _selectedPlant.GetStatusInfo();
                if (status != null)
                {
                    string info = $"<b>{status.StrainName}</b>\n" +
                                 $"Stage: {status.CurrentStage}\n" +
                                 $"Health: {status.Health:F1}%\n" +
                                 $"Growth: {status.GrowthProgress:P0}\n" +
                                 $"Water: {status.WaterLevel:F1}%\n" +
                                 $"Nutrients: {status.NutrientLevel:F1}%\n" +
                                 $"Age: {status.DaysOld} days\n";
                    
                    if (status.IsHarvestable)
                    {
                        info += $"<color=green>Ready to Harvest!</color>\n";
                        info += $"Est. Yield: {status.EstimatedYield:F1}g";
                    }
                    
                    _plantInfoText.text = info;
                }
            }
        }
        
        private void UpdateSelectedRoomInfo()
        {
            if (_selectedRoom != null && _facilityInfoPanel != null && _facilityInfoText != null)
            {
                var status = _selectedRoom.GetRoomStatus();
                if (status != null)
                {
                    string info = $"<b>{status.RoomName}</b>\n" +
                                 $"Status: {status.Status}\n" +
                                 $"Plants: {status.PlantCount}/{status.MaxPlants}\n" +
                                 $"Occupancy: {status.OccupancyRate:P0}\n" +
                                 $"Avg Health: {status.AverageHealth:F1}%\n" +
                                 $"Temperature: {status.Conditions.Temperature:F1}°C\n" +
                                 $"Humidity: {status.Conditions.Humidity:F1}%\n" +
                                 $"CO2: {status.Conditions.CO2Level:F0}ppm";
                    
                    _facilityInfoText.text = info;
                }
            }
        }
        
        #endregion
        
        #region Button Event Handlers
        
        private void OnPlantButtonClicked()
        {
            Debug.Log("Plant button clicked");
            
            // Find available grow room
            var growRooms = UnityEngine.Object.FindObjectsByType<GrowRoomController>(FindObjectsSortMode.None);
            var availableRoom = System.Array.Find(growRooms, room => room.HasAvailableSpace);
            
            if (availableRoom != null)
            {
                // Try to plant using a default plant prefab
                var plantPrefab = Resources.Load<GameObject>("Prefabs/Plants/DefaultPlant");
                if (plantPrefab != null)
                {
                    bool planted = availableRoom.AddPlant(plantPrefab);
                    if (planted)
                    {
                        ShowNotification("Plant added to grow room!", Color.green);
                        
                        // Deduct cost
                        _currencyManager?.SpendCurrency(CurrencyType.Cash, 50f, "Plant seed cost");
                        
                        // Award experience
                        _communityManager?.AddExperience(10, "Planted a seed");
                    }
                    else
                    {
                        ShowNotification("Failed to add plant", Color.red);
                    }
                }
                else
                {
                    ShowNotification("No plant prefab found", Color.red);
                }
            }
            else
            {
                ShowNotification("No available space in grow rooms", Color.yellow);
            }
        }
        
        private void OnHarvestButtonClicked()
        {
            Debug.Log("Harvest button clicked");
            
            if (_selectedPlant != null && _selectedPlant.IsHarvestable)
            {
                var result = _selectedPlant.HarvestPlant();
                if (result != null)
                {
                    ShowNotification($"Harvested {result.TotalYield:F1}g!", Color.green);
                    
                    // Award currency
                    float value = result.TotalYield * 10f; // $10 per gram
                    _currencyManager?.AddCurrency(CurrencyType.Cash, value, "Plant harvest");
                    
                    // Award experience
                    _communityManager?.AddExperience(50, "Harvested a plant");
                    
                    // Clear selection
                    SetSelectedPlant(null);
                }
            }
            else
            {
                ShowNotification("No harvestable plant selected", Color.yellow);
            }
        }
        
        private void OnGeneticsButtonClicked()
        {
            Debug.Log("Genetics button clicked");
            ShowNotification("Genetics system coming soon!", Color.blue);
        }
        
        private void OnMarketButtonClicked()
        {
            Debug.Log("Market button clicked");
            ShowNotification("Market system coming soon!", Color.blue);
        }
        
        private void OnConstructionButtonClicked()
        {
            Debug.Log("Construction button clicked");
            
            if (_constructionManager != null)
            {
                // Start a simple grow room construction
                string projectId = _constructionManager.StartConstructionProject(
                    "GrowRoom", 
                    Vector3.zero, 
                    ProjectChimera.Data.Construction.BuildingQuality.Standard
                );
                
                if (!string.IsNullOrEmpty(projectId))
                {
                    ShowNotification("Construction project started!", Color.green);
                    
                    // Deduct cost
                    _currencyManager?.SpendCurrency(CurrencyType.Cash, 10000f, "Construction project");
                }
                else
                {
                    ShowNotification("Failed to start construction", Color.red);
                }
            }
        }
        
        private void OnTutorialButtonClicked()
        {
            Debug.Log("Tutorial button clicked");
            
            if (_tutorialManager != null)
            {
                bool started = _tutorialManager.StartTutorial("basic_tutorial");
                if (started)
                {
                    ShowNotification("Tutorial started!", Color.green);
                }
                else
                {
                    ShowNotification("Tutorial already completed or unavailable", Color.yellow);
                }
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnCurrencyChanged(CurrencyType currencyType, float oldBalance, float newBalance)
        {
            if (currencyType == CurrencyType.Cash)
            {
                UpdateCurrencyDisplay();
            }
        }
        
        private void OnReputationChanged(int newReputation)
        {
            UpdatePlayerStats();
            ShowNotification($"Reputation: +{newReputation}", Color.blue);
        }
        
        private void OnPlayerProfileUpdated(PlayerProfile profile)
        {
            UpdatePlayerStats();
        }
        
        private void OnPlantHarvested(PlantInstance plant)
        {
            ShowNotification($"Plant harvested: {plant.PlantID}", Color.green);
        }
        
        
        private void OnPlantSelected(InteractivePlantComponent plant)
        {
            SetSelectedPlant(plant);
        }
        
        private void OnPlantHovered(InteractivePlantComponent plant)
        {
            // Could show tooltip here
        }
        
        private void OnRoomStatusChanged(GrowRoomController room)
        {
            if (room == _selectedRoom)
            {
                UpdateSelectedRoomInfo();
            }
        }
        
        #endregion
        
        #region Selection Management
        
        private void SetSelectedPlant(InteractivePlantComponent plant)
        {
            _selectedPlant = plant;
            
            if (_plantInfoPanel != null)
            {
                _plantInfoPanel.SetActive(plant != null);
            }
            
            UpdateSelectedPlantInfo();
        }
        
        private void SetSelectedRoom(GrowRoomController room)
        {
            _selectedRoom = room;
            
            if (_facilityInfoPanel != null)
            {
                _facilityInfoPanel.SetActive(room != null);
            }
            
            UpdateSelectedRoomInfo();
        }
        
        #endregion
        
        #region Notification System
        
        private void ShowNotification(string message, Color color)
        {
            if (_notificationPrefab == null || _notificationContainer == null)
            {
                Debug.Log($"Notification: {message}");
                return;
            }
            
            GameObject notification = Instantiate(_notificationPrefab, _notificationContainer);
            
            var text = notification.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = message;
                text.color = color;
            }
            
            // Auto-destroy after duration
            Destroy(notification, _notificationDuration);
            
            Debug.Log($"Notification: {message}");
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Toggle side panel visibility
        /// </summary>
        public void ToggleSidePanel()
        {
            if (_sidePanel != null)
            {
                _isPanelOpen = !_isPanelOpen;
                _sidePanel.SetActive(_isPanelOpen);
            }
        }
        
        /// <summary>
        /// Show specific info panel
        /// </summary>
        public void ShowInfoPanel(string panelType)
        {
            switch (panelType.ToLower())
            {
                case "plant":
                    if (_plantInfoPanel != null)
                        _plantInfoPanel.SetActive(true);
                    break;
                case "facility":
                    if (_facilityInfoPanel != null)
                        _facilityInfoPanel.SetActive(true);
                    break;
            }
        }
        
        /// <summary>
        /// Hide all info panels
        /// </summary>
        public void HideAllInfoPanels()
        {
            if (_plantInfoPanel != null)
                _plantInfoPanel.SetActive(false);
            if (_facilityInfoPanel != null)
                _facilityInfoPanel.SetActive(false);
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Clean up event listeners
            if (_currencyManager != null)
                _currencyManager.OnCurrencyChanged -= OnCurrencyChanged;
            
            if (_communityManager != null)
            {
                _communityManager.OnReputationChanged -= OnReputationChanged;
                _communityManager.OnPlayerProfileUpdated -= OnPlayerProfileUpdated;
            }
            
            if (_plantManager != null)
            {
                _plantManager.OnPlantHarvested -= OnPlantHarvested;
            }
        }
    }
}