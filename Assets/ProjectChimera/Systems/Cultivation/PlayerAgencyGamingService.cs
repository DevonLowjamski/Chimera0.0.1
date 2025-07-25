using UnityEngine;
using System;
using ProjectChimera.Core;
using ProjectChimera.Data.Cultivation;
using ProjectChimera.Data.Events;
using ProjectChimera.Core.Events;
using EventsPlayerChoiceEventData = ProjectChimera.Events.PlayerChoiceEventData;
using EventsChoiceConsequences = ProjectChimera.Events.ChoiceConsequences;
using DataCultivationApproach = ProjectChimera.Data.Cultivation.CultivationApproach;

namespace ProjectChimera.Systems.Cultivation
{
    /// <summary>
    /// Service responsible for player agency gaming mechanics
    /// Handles player choices, cultivation paths, and facility design
    /// </summary>
    public class PlayerAgencyGamingService
    {
        private readonly PlayerAgencyGamingSystem _playerAgencySystem;
        private readonly CultivationPathManager _cultivationPathManager;
        private readonly FacilityDesignGamingSystem _facilityDesignSystem;
        private readonly EnhancedCultivationGamingConfigSO _config;
        
        // Event channels
        private readonly GameEventChannelSO _onPlayerChoiceMade;
        private readonly GameEventChannelSO _onCultivationPathSelected;
        private readonly GameEventChannelSO _onFacilityDesignCompleted;
        
        // State
        private bool _isInitialized = false;
        private DataCultivationApproach _currentCultivationApproach = DataCultivationApproach.OrganicTraditional;
        private FacilityDesignApproach _currentFacilityDesignApproach = FacilityDesignApproach.MinimalistEfficient;
        private int _playerChoicesMade = 0;
        
        public bool IsInitialized => _isInitialized;
        public string ServiceName => "Player Agency Gaming Service";
        
        public PlayerAgencyGamingService(
            PlayerAgencyGamingSystem playerAgencySystem,
            CultivationPathManager cultivationPathManager,
            FacilityDesignGamingSystem facilityDesignSystem,
            EnhancedCultivationGamingConfigSO config,
            GameEventChannelSO onPlayerChoiceMade,
            GameEventChannelSO onCultivationPathSelected,
            GameEventChannelSO onFacilityDesignCompleted)
        {
            _playerAgencySystem = playerAgencySystem ?? throw new ArgumentNullException(nameof(playerAgencySystem));
            _cultivationPathManager = cultivationPathManager ?? throw new ArgumentNullException(nameof(cultivationPathManager));
            _facilityDesignSystem = facilityDesignSystem ?? throw new ArgumentNullException(nameof(facilityDesignSystem));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _onPlayerChoiceMade = onPlayerChoiceMade;
            _onCultivationPathSelected = onCultivationPathSelected;
            _onFacilityDesignCompleted = onFacilityDesignCompleted;
        }
        
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[PlayerAgencyGamingService] Already initialized");
                return;
            }
            
            try
            {
                InitializePlayerAgencySystem();
                InitializeCultivationPathManager();
                InitializeFacilityDesignSystem();
                RegisterEventHandlers();
                
                _isInitialized = true;
                Debug.Log("PlayerAgencyGamingService initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize PlayerAgencyGamingService: {ex.Message}");
                throw;
            }
        }
        
        public void Shutdown()
        {
            if (!_isInitialized) return;
            
            UnregisterEventHandlers();
            _isInitialized = false;
            
            Debug.Log("PlayerAgencyGamingService shutdown completed");
        }
        
        public void Update(float deltaTime)
        {
            if (!_isInitialized) return;
            
            _playerAgencySystem?.UpdateSystem(deltaTime);
        }
        
        private void InitializePlayerAgencySystem()
        {
            _playerAgencySystem?.Initialize(_config?.PlayerAgencyGamingConfig);
        }
        
        private void InitializeCultivationPathManager()
        {
            _cultivationPathManager?.Initialize(_config?.CultivationPathLibrary);
        }
        
        private void InitializeFacilityDesignSystem()
        {
            _facilityDesignSystem?.Initialize(_config?.FacilityDesignLibrary);
        }
        
        private void RegisterEventHandlers()
        {
            if (_onPlayerChoiceMade != null)
                _onPlayerChoiceMade.OnEventRaisedWithData.AddListener(OnPlayerChoiceMade);
            
            if (_onCultivationPathSelected != null)
                _onCultivationPathSelected.OnEventRaisedWithData.AddListener(OnCultivationPathSelected);
            
            if (_onFacilityDesignCompleted != null)
                _onFacilityDesignCompleted.OnEventRaisedWithData.AddListener(OnFacilityDesignCompleted);
        }
        
        private void UnregisterEventHandlers()
        {
            if (_onPlayerChoiceMade != null)
                _onPlayerChoiceMade.OnEventRaisedWithData.RemoveListener(OnPlayerChoiceMade);
            
            if (_onCultivationPathSelected != null)
                _onCultivationPathSelected.OnEventRaisedWithData.RemoveListener(OnCultivationPathSelected);
            
            if (_onFacilityDesignCompleted != null)
                _onFacilityDesignCompleted.OnEventRaisedWithData.RemoveListener(OnFacilityDesignCompleted);
        }
        
        private void OnPlayerChoiceMade(object eventData)
        {
            _playerChoicesMade++;
            
            if (eventData is EventsPlayerChoiceEventData choiceData)
            {
                ProcessPlayerChoiceConsequences(choiceData);
                UpdateCultivationPath(choiceData);
            }
        }
        
        private void OnCultivationPathSelected(object eventData)
        {
            if (eventData is CultivationPathEventData pathData)
            {
                _currentCultivationApproach = pathData.NewApproach;
                ProcessCultivationPathChange(pathData);
            }
        }
        
        private void OnFacilityDesignCompleted(object eventData)
        {
            if (eventData is FacilityDesignEventData designData)
            {
                _currentFacilityDesignApproach = designData.DesignApproach;
                ProcessFacilityDesignCompletion(designData);
            }
        }
        
        private void ProcessPlayerChoiceConsequences(EventsPlayerChoiceEventData choiceData)
        {
            _playerAgencySystem?.ApplyChoiceConsequences(choiceData);
        }
        
        private void UpdateCultivationPath(EventsPlayerChoiceEventData choiceData)
        {
            _cultivationPathManager?.UpdatePath(choiceData);
        }
        
        private void ProcessCultivationPathChange(CultivationPathEventData pathData)
        {
            // Process cultivation path change effects
            Debug.Log($"Cultivation path changed to: {pathData.NewApproach}");
        }
        
        private void ProcessFacilityDesignCompletion(FacilityDesignEventData designData)
        {
            // Process facility design completion effects
            Debug.Log($"Facility design completed: {designData.DesignApproach}");
        }
        
        // Public API
        public EventsChoiceConsequences MakePlayerChoice(PlayerChoice choice)
        {
            if (!_isInitialized || choice == null) return EventsChoiceConsequences.None;
            return _playerAgencySystem?.ProcessPlayerChoice(choice) ?? EventsChoiceConsequences.None;
        }
        
        public DataCultivationApproach GetCurrentCultivationApproach()
        {
            return _currentCultivationApproach;
        }
        
        public FacilityDesignApproach GetCurrentFacilityDesignApproach()
        {
            return _currentFacilityDesignApproach;
        }
        
        public int GetPlayerChoicesMade()
        {
            return _playerChoicesMade;
        }
        
        public bool SetCultivationPath(DataCultivationApproach approach)
        {
            if (!_isInitialized) return false;
            
            _currentCultivationApproach = approach;
            _onCultivationPathSelected?.RaiseEvent(new CultivationPathEventData 
            { 
                NewApproach = approach,
                Timestamp = Time.time 
            });
            
            return true;
        }
        
        public bool SetFacilityDesignApproach(FacilityDesignApproach approach)
        {
            if (!_isInitialized) return false;
            
            _currentFacilityDesignApproach = approach;
            _onFacilityDesignCompleted?.RaiseEvent(new FacilityDesignEventData 
            { 
                DesignApproach = approach,
                Timestamp = Time.time 
            });
            
            return true;
        }
    }
    
    // Supporting data structures
    [System.Serializable]
    public class CultivationPathEventData
    {
        public DataCultivationApproach NewApproach;
        public float Timestamp;
    }
    
    [System.Serializable]
    public class FacilityDesignEventData
    {
        public FacilityDesignApproach DesignApproach;
        public float Timestamp;
    }
}