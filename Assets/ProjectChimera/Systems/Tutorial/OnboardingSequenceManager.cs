using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Tutorial;
using ProjectChimera.Systems.Cultivation;

namespace ProjectChimera.Systems.Tutorial
{
    /// <summary>
    /// Onboarding sequence manager for Project Chimera.
    /// Manages the first-time user experience and initial setup.
    /// </summary>
    public class OnboardingSequenceManager : ChimeraManager
    {
        [Header("Onboarding Configuration")]
        [SerializeField] private TutorialSequenceSO _onboardingSequence;
        [SerializeField] private List<TutorialStepSO> _welcomeSteps;
        [SerializeField] private List<TutorialStepSO> _facilitySetupSteps;
        [SerializeField] private List<TutorialStepSO> _firstPlantSteps;
        
        [Header("Player Setup")]
        [SerializeField] private float _startingCurrency = 25000f;
        [SerializeField] private int _startingSkillPoints = 5;
        [SerializeField] private List<string> _startingUnlocks;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onOnboardingStarted;
        [SerializeField] private SimpleGameEventSO _onOnboardingCompleted;
        [SerializeField] private StringGameEventSO _onOnboardingStepCompleted;
        
        // State
        private bool _isOnboardingActive;
        private bool _isOnboardingCompleted;
        private string _currentOnboardingStep;
        private int _currentStepIndex;
        private OnboardingPhase _currentPhase;
        
        // Managers
        private TutorialManager _tutorialManager;
        private CultivationManager _cultivationManager;
        // private MarketManager _marketManager; // Removed to prevent circular dependency
        
        // Properties
        public bool IsOnboardingActive => _isOnboardingActive;
        public bool IsOnboardingCompleted => _isOnboardingCompleted;
        public OnboardingPhase CurrentPhase => _currentPhase;
        public string CurrentOnboardingStep => _currentOnboardingStep;
        
        protected override void OnManagerInitialize()
        {
            // Get required managers
            _tutorialManager = GameManager.Instance.GetManager<TutorialManager>();
            _cultivationManager = GameManager.Instance.GetManager<CultivationManager>();
            // _marketManager = GameManager.Instance.GetManager<MarketManager>(); // Removed to prevent circular dependency
            
            // Check if onboarding should start
            CheckOnboardingStatus();
            
            // Subscribe to events
            SubscribeToEvents();
            
            Debug.Log("Onboarding sequence manager initialized");
        }

        protected override void OnManagerShutdown()
        {
            // Unsubscribe from events and clean up
            Debug.Log("Onboarding sequence manager shutdown");
        }
        
        /// <summary>
        /// Check onboarding status
        /// </summary>
        private void CheckOnboardingStatus()
        {
            // Check if this is a new player
            _isOnboardingCompleted = PlayerPrefs.GetInt("OnboardingCompleted", 0) == 1;
            
            if (!_isOnboardingCompleted)
            {
                // Start onboarding for new players
                StartOnboarding();
            }
        }
        
        /// <summary>
        /// Subscribe to events
        /// </summary>
        private void SubscribeToEvents()
        {
            // Subscribe to static events from TutorialManager
            TutorialManager.OnStepCompleted += HandleTutorialStepCompleted;
            TutorialManager.OnSequenceCompleted += HandleTutorialSequenceCompleted;
        }
        
        /// <summary>
        /// Start onboarding sequence
        /// </summary>
        public void StartOnboarding()
        {
            if (_isOnboardingActive || _isOnboardingCompleted)
                return;
            
            _isOnboardingActive = true;
            _currentPhase = OnboardingPhase.Welcome;
            _currentStepIndex = 0;
            
            // Setup initial player state
            SetupNewPlayer();
            
            // Start welcome phase
            StartWelcomePhase();
            
            _onOnboardingStarted?.Raise();
            
            Debug.Log("Started onboarding sequence");
        }
        
        /// <summary>
        /// Setup new player
        /// </summary>
        private void SetupNewPlayer()
        {
            // Give starting currency
            // TODO: Implement currency system in MarketManager or create dedicated Economy Manager
            /*
            if (_marketManager != null)
            {
                _marketManager.AddCurrency(_startingCurrency);
                Debug.Log($"Gave player starting currency: ${_startingCurrency:F0}");
            }
            */
            Debug.Log($"Would give player starting currency: ${_startingCurrency:F0} (currency system not yet implemented)");
            
            // Give starting skill points (would integrate with progression system)
            PlayerPrefs.SetInt("SkillPoints", _startingSkillPoints);
            
            // Unlock starting features
            foreach (var unlock in _startingUnlocks)
            {
                PlayerPrefs.SetInt($"Unlocked_{unlock}", 1);
                Debug.Log($"Unlocked starting feature: {unlock}");
            }
            
            // Set tutorial flags
            PlayerPrefs.SetInt("FirstTimePlayer", 1);
            PlayerPrefs.SetInt("OnboardingStarted", 1);
        }
        
        /// <summary>
        /// Start welcome phase
        /// </summary>
        private void StartWelcomePhase()
        {
            _currentPhase = OnboardingPhase.Welcome;
            
            if (_welcomeSteps.Count > 0)
            {
                var welcomeStep = _welcomeSteps[0];
                _currentOnboardingStep = welcomeStep.StepId;
                
                if (_tutorialManager != null)
                {
                    _tutorialManager.StartTutorialStep(welcomeStep);
                }
            }
            
            Debug.Log("Started welcome phase");
        }
        
        /// <summary>
        /// Start facility setup phase
        /// </summary>
        private void StartFacilitySetupPhase()
        {
            _currentPhase = OnboardingPhase.FacilitySetup;
            _currentStepIndex = 0;
            
            if (_facilitySetupSteps.Count > 0)
            {
                var setupStep = _facilitySetupSteps[0];
                _currentOnboardingStep = setupStep.StepId;
                
                if (_tutorialManager != null)
                {
                    _tutorialManager.StartTutorialStep(setupStep);
                }
            }
            
            Debug.Log("Started facility setup phase");
        }
        
        /// <summary>
        /// Start first plant phase
        /// </summary>
        private void StartFirstPlantPhase()
        {
            _currentPhase = OnboardingPhase.FirstPlant;
            _currentStepIndex = 0;
            
            if (_firstPlantSteps.Count > 0)
            {
                var plantStep = _firstPlantSteps[0];
                _currentOnboardingStep = plantStep.StepId;
                
                if (_tutorialManager != null)
                {
                    _tutorialManager.StartTutorialStep(plantStep);
                }
            }
            
            Debug.Log("Started first plant phase");
        }
        
        /// <summary>
        /// Handle tutorial step completed
        /// </summary>
        private void HandleTutorialStepCompleted(TutorialStepSO step)
        {
            if (step == null) return;
            
            string stepId = step.StepId;
            
            if (stepId == _currentOnboardingStep)
            {
                ProgressOnboardingStep();
            }
            
            // Notify onboarding step completion
            _onOnboardingStepCompleted?.Raise(stepId);
        }
        
        /// <summary>
        /// Progress onboarding step
        /// </summary>
        private void ProgressOnboardingStep()
        {
            _currentStepIndex++;
            
            // Check if current phase is complete
            var currentSteps = GetCurrentPhaseSteps();
            
            if (_currentStepIndex >= currentSteps.Count)
            {
                // Phase complete, move to next phase
                CompleteCurrentPhase();
            }
            else
            {
                // Continue with next step in current phase
                var nextStep = currentSteps[_currentStepIndex];
                _currentOnboardingStep = nextStep.StepId;
                
                if (_tutorialManager != null)
                {
                    _tutorialManager.StartTutorialStep(nextStep);
                }
            }
        }
        
        /// <summary>
        /// Get current phase steps
        /// </summary>
        private List<TutorialStepSO> GetCurrentPhaseSteps()
        {
            switch (_currentPhase)
            {
                case OnboardingPhase.Welcome:
                    return _welcomeSteps;
                case OnboardingPhase.FacilitySetup:
                    return _facilitySetupSteps;
                case OnboardingPhase.FirstPlant:
                    return _firstPlantSteps;
                default:
                    return new List<TutorialStepSO>();
            }
        }
        
        /// <summary>
        /// Complete current phase
        /// </summary>
        private void CompleteCurrentPhase()
        {
            Debug.Log($"Completed onboarding phase: {_currentPhase}");
            
            switch (_currentPhase)
            {
                case OnboardingPhase.Welcome:
                    StartFacilitySetupPhase();
                    break;
                    
                case OnboardingPhase.FacilitySetup:
                    StartFirstPlantPhase();
                    break;
                    
                case OnboardingPhase.FirstPlant:
                    CompleteOnboarding();
                    break;
            }
        }
        
        /// <summary>
        /// Complete onboarding
        /// </summary>
        private void CompleteOnboarding()
        {
            _isOnboardingActive = false;
            _isOnboardingCompleted = true;
            _currentPhase = OnboardingPhase.Completed;
            
            // Save completion status
            PlayerPrefs.SetInt("OnboardingCompleted", 1);
            PlayerPrefs.SetInt("FirstTimePlayer", 0);
            
            // Give completion rewards
            GiveCompletionRewards();
            
            _onOnboardingCompleted?.Raise();
            
            Debug.Log("Onboarding sequence completed");
        }
        
        /// <summary>
        /// Give completion rewards
        /// </summary>
        private void GiveCompletionRewards()
        {
            // Give bonus currency
            // TODO: Implement currency system in MarketManager or create dedicated Economy Manager
            /*
            if (_marketManager != null)
            {
                _marketManager.AddCurrency(5000f);
                Debug.Log("Gave onboarding completion bonus: $5000");
            }
            */
            Debug.Log("Would give onboarding completion bonus: $5000 (currency system not yet implemented)");
            
            // Unlock additional features
            PlayerPrefs.SetInt("Unlocked_AdvancedCultivation", 1);
            PlayerPrefs.SetInt("Unlocked_BasicGenetics", 1);
            
            // Give skill points
            var currentSkillPoints = PlayerPrefs.GetInt("SkillPoints", 0);
            PlayerPrefs.SetInt("SkillPoints", currentSkillPoints + 3);
        }
        
        /// <summary>
        /// Handle tutorial sequence completed
        /// </summary>
        private void HandleTutorialSequenceCompleted(TutorialSequenceSO sequence)
        {
            if (sequence == null) return;
            
            if (sequence.SequenceId == _onboardingSequence?.SequenceId)
            {
                CompleteOnboarding();
            }
        }
        
        /// <summary>
        /// Skip onboarding (for testing or returning players)
        /// </summary>
        public void SkipOnboarding()
        {
            if (_isOnboardingCompleted)
                return;
            
            // Setup player with all onboarding rewards
            SetupNewPlayer();
            GiveCompletionRewards();
            
            // Mark as completed
            _isOnboardingActive = false;
            _isOnboardingCompleted = true;
            _currentPhase = OnboardingPhase.Completed;
            
            PlayerPrefs.SetInt("OnboardingCompleted", 1);
            PlayerPrefs.SetInt("OnboardingSkipped", 1);
            
            _onOnboardingCompleted?.Raise();
            
            Debug.Log("Skipped onboarding sequence");
        }
        
        /// <summary>
        /// Reset onboarding (for testing)
        /// </summary>
        [ContextMenu("Reset Onboarding")]
        public void ResetOnboarding()
        {
            _isOnboardingActive = false;
            _isOnboardingCompleted = false;
            _currentPhase = OnboardingPhase.None;
            _currentStepIndex = 0;
            _currentOnboardingStep = "";
            
            // Clear player prefs
            PlayerPrefs.DeleteKey("OnboardingCompleted");
            PlayerPrefs.DeleteKey("OnboardingStarted");
            PlayerPrefs.DeleteKey("OnboardingSkipped");
            PlayerPrefs.DeleteKey("FirstTimePlayer");
            
            Debug.Log("Reset onboarding sequence");
        }
        
        /// <summary>
        /// Get onboarding progress
        /// </summary>
        public OnboardingProgress GetOnboardingProgress()
        {
            var totalSteps = _welcomeSteps.Count + _facilitySetupSteps.Count + _firstPlantSteps.Count;
            var completedSteps = 0;
            
            if (_currentPhase == OnboardingPhase.FacilitySetup)
                completedSteps = _welcomeSteps.Count;
            else if (_currentPhase == OnboardingPhase.FirstPlant)
                completedSteps = _welcomeSteps.Count + _facilitySetupSteps.Count;
            else if (_currentPhase == OnboardingPhase.Completed)
                completedSteps = totalSteps;
            
            completedSteps += _currentStepIndex;
            
            return new OnboardingProgress
            {
                CurrentPhase = _currentPhase,
                CurrentStepIndex = _currentStepIndex,
                TotalSteps = totalSteps,
                CompletedSteps = completedSteps,
                Progress = totalSteps > 0 ? (float)completedSteps / totalSteps : 0f,
                IsActive = _isOnboardingActive,
                IsCompleted = _isOnboardingCompleted
            };
        }
        
        protected override void OnDestroy()
        {
            // Unsubscribe from events
            TutorialManager.OnStepCompleted -= HandleTutorialStepCompleted;
            TutorialManager.OnSequenceCompleted -= HandleTutorialSequenceCompleted;
            
            base.OnDestroy();
        }
    }
    
    /// <summary>
    /// Onboarding phases
    /// </summary>
    public enum OnboardingPhase
    {
        None,
        Welcome,
        FacilitySetup,
        FirstPlant,
        Completed
    }
    
    /// <summary>
    /// Onboarding progress information
    /// </summary>
    [System.Serializable]
    public struct OnboardingProgress
    {
        public OnboardingPhase CurrentPhase;
        public int CurrentStepIndex;
        public int TotalSteps;
        public int CompletedSteps;
        public float Progress;
        public bool IsActive;
        public bool IsCompleted;
    }
}