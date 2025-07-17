using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Tutorial;
using ProjectChimera.Systems.Cultivation;
using EnvironmentSystems = ProjectChimera.Systems.Environment;

namespace ProjectChimera.Systems.Tutorial
{
    /// <summary>
    /// Cultivation tutorial manager for Project Chimera.
    /// Manages advanced growing mechanics tutorials and plant care education.
    /// </summary>
    public class CultivationTutorialManager : ChimeraManager
    {
        [Header("Cultivation Tutorial Configuration")]
        [SerializeField] private TutorialSequenceSO _cultivationTutorialSequence;
        [SerializeField] private List<CultivationTutorialModule> _tutorialModules;
        [SerializeField] private bool _autoStartAfterOnboarding = true;
        
        [Header("Tutorial Prerequisites")]
        [SerializeField] private int _minimumPlayerLevel = 2;
        [SerializeField] private int _minimumHarvestCount = 1;
        [SerializeField] private List<string> _requiredUnlocks;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onCultivationTutorialStarted;
        [SerializeField] private SimpleGameEventSO _onCultivationTutorialCompleted;
        [SerializeField] private StringGameEventSO _onCultivationModuleCompleted;
        
        // State
        private bool _isCultivationTutorialActive;
        private bool _isCultivationTutorialCompleted;
        private CultivationTutorialModule _currentModule;
        private int _currentModuleIndex;
        private string _currentStepId;
        
        // Managers
        private TutorialManager _tutorialManager;
        private CultivationManager _cultivationManager;
        private EnvironmentSystems.EnvironmentalManager _environmentalManager;
        
        // Tutorial tracking
        private Dictionary<string, bool> _moduleCompletionStatus;
        private Dictionary<string, float> _moduleProgressTracking;
        
        // Properties
        public bool IsCultivationTutorialActive => _isCultivationTutorialActive;
        public bool IsCultivationTutorialCompleted => _isCultivationTutorialCompleted;
        public CultivationTutorialModule CurrentModule => _currentModule;
        public int CurrentModuleIndex => _currentModuleIndex;
        public string CurrentStepId => _currentStepId;
        
        protected override void OnManagerInitialize()
        {
            // Get required managers
            _tutorialManager = GameManager.Instance.GetManager<TutorialManager>();
            _cultivationManager = GameManager.Instance.GetManager<CultivationManager>();
            _environmentalManager = GameManager.Instance.GetManager<EnvironmentSystems.EnvironmentalManager>();
            
            // Initialize tracking dictionaries
            _moduleCompletionStatus = new Dictionary<string, bool>();
            _moduleProgressTracking = new Dictionary<string, float>();
            
            // Initialize tutorial modules
            InitializeTutorialModules();
            
            // Check if tutorial should auto-start
            CheckTutorialPrerequisites();
            
            // Subscribe to events
            SubscribeToEvents();
            
            Debug.Log("Cultivation tutorial manager initialized");
        }

        protected override void OnManagerShutdown()
        {
            // Unsubscribe from events
            // Clean up any active tutorials
            
            Debug.Log("Cultivation tutorial manager shutdown");
        }
        
        /// <summary>
        /// Initialize tutorial modules
        /// </summary>
        private void InitializeTutorialModules()
        {
            if (_tutorialModules == null)
            {
                _tutorialModules = new List<CultivationTutorialModule>();
            }
            
            // Create default modules if none exist
            if (_tutorialModules.Count == 0)
            {
                CreateDefaultTutorialModules();
            }
            
            // Initialize completion tracking
            foreach (var module in _tutorialModules)
            {
                _moduleCompletionStatus[module.ModuleId] = false;
                _moduleProgressTracking[module.ModuleId] = 0f;
            }
        }
        
        /// <summary>
        /// Create default tutorial modules
        /// </summary>
        private void CreateDefaultTutorialModules()
        {
            _tutorialModules.AddRange(new List<CultivationTutorialModule>
            {
                new CultivationTutorialModule
                {
                    ModuleId = "environmental_control",
                    ModuleName = "Environmental Control Systems",
                    Description = "Learn to control temperature, humidity, light cycles, and air circulation for optimal plant growth.",
                    EstimatedDuration = 15f,
                    DifficultyLevel = TutorialDifficultyLevel.Intermediate,
                    Prerequisites = new List<string> { "onboarding_completed" },
                    LearningObjectives = new List<string>
                    {
                        "Understand optimal environmental ranges for cannabis",
                        "Configure HVAC systems for temperature and humidity control",
                        "Set up lighting schedules for different growth phases",
                        "Monitor and adjust environmental parameters",
                        "Troubleshoot common environmental issues"
                    }
                },
                
                new CultivationTutorialModule
                {
                    ModuleId = "nutrient_management",
                    ModuleName = "Nutrient Management & Feeding",
                    Description = "Master nutrient solutions, feeding schedules, and plant nutrition for maximum yield and quality.",
                    EstimatedDuration = 20f,
                    DifficultyLevel = TutorialDifficultyLevel.Intermediate,
                    Prerequisites = new List<string> { "environmental_control" },
                    LearningObjectives = new List<string>
                    {
                        "Understand N-P-K ratios and micronutrients",
                        "Create custom nutrient solutions",
                        "Implement feeding schedules for different growth stages",
                        "Recognize and treat nutrient deficiencies",
                        "Monitor pH and EC/TDS levels"
                    }
                },
                
                new CultivationTutorialModule
                {
                    ModuleId = "advanced_growing_techniques",
                    ModuleName = "Advanced Growing Techniques",
                    Description = "Learn training methods, pruning techniques, and advanced cultivation strategies.",
                    EstimatedDuration = 25f,
                    DifficultyLevel = TutorialDifficultyLevel.Advanced,
                    Prerequisites = new List<string> { "nutrient_management" },
                    LearningObjectives = new List<string>
                    {
                        "Apply LST (Low Stress Training) techniques",
                        "Perform HST (High Stress Training) methods",
                        "Execute SCROG (Screen of Green) setups",
                        "Implement SOG (Sea of Green) techniques",
                        "Master defoliation and pruning strategies"
                    }
                },
                
                new CultivationTutorialModule
                {
                    ModuleId = "pest_disease_management",
                    ModuleName = "Pest & Disease Management",
                    Description = "Identify, prevent, and treat common pests and diseases in cannabis cultivation.",
                    EstimatedDuration = 18f,
                    DifficultyLevel = TutorialDifficultyLevel.Intermediate,
                    Prerequisites = new List<string> { "environmental_control" },
                    LearningObjectives = new List<string>
                    {
                        "Identify common cannabis pests and diseases",
                        "Implement Integrated Pest Management (IPM)",
                        "Use beneficial insects and biocontrols",
                        "Apply organic and chemical treatments safely",
                        "Prevent contamination and maintain sanitation"
                    }
                },
                
                new CultivationTutorialModule
                {
                    ModuleId = "harvest_processing",
                    ModuleName = "Harvest & Post-Processing",
                    Description = "Learn optimal harvest timing, drying, curing, and quality assessment techniques.",
                    EstimatedDuration = 22f,
                    DifficultyLevel = TutorialDifficultyLevel.Advanced,
                    Prerequisites = new List<string> { "advanced_growing_techniques" },
                    LearningObjectives = new List<string>
                    {
                        "Determine optimal harvest timing using trichome analysis",
                        "Execute proper harvesting techniques",
                        "Set up controlled drying environments",
                        "Implement curing processes for quality enhancement",
                        "Assess final product quality and potency"
                    }
                }
            });
        }
        
        /// <summary>
        /// Check tutorial prerequisites
        /// </summary>
        private void CheckTutorialPrerequisites()
        {
            if (!_autoStartAfterOnboarding)
                return;
            
            // Check if onboarding is completed
            var onboardingCompleted = PlayerPrefs.GetInt("OnboardingCompleted", 0) == 1;
            if (!onboardingCompleted)
                return;
            
            // Check player level
            var playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
            if (playerLevel < _minimumPlayerLevel)
                return;
            
            // Check harvest count
            var harvestCount = PlayerPrefs.GetInt("TotalHarvestCount", 0);
            if (harvestCount < _minimumHarvestCount)
                return;
            
            // Check required unlocks
            foreach (var unlock in _requiredUnlocks)
            {
                if (PlayerPrefs.GetInt($"Unlocked_{unlock}", 0) == 0)
                    return;
            }
            
            // All prerequisites met
            Debug.Log("Cultivation tutorial prerequisites met - ready to start");
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
        /// Start cultivation tutorial
        /// </summary>
        public bool StartCultivationTutorial()
        {
            if (_isCultivationTutorialActive || _isCultivationTutorialCompleted)
                return false;
            
            // Check prerequisites one more time
            if (!ArePrerequisitesMet())
            {
                Debug.LogWarning("Prerequisites not met for cultivation tutorial");
                return false;
            }
            
            _isCultivationTutorialActive = true;
            _currentModuleIndex = 0;
            
            // Start first module
            StartTutorialModule(0);
            
            _onCultivationTutorialStarted?.Raise();
            
            Debug.Log("Started cultivation tutorial");
            return true;
        }
        
        /// <summary>
        /// Start specific tutorial module
        /// </summary>
        public bool StartTutorialModule(int moduleIndex)
        {
            if (moduleIndex < 0 || moduleIndex >= _tutorialModules.Count)
                return false;
            
            var module = _tutorialModules[moduleIndex];
            
            // Check module prerequisites
            if (!AreModulePrerequisitesMet(module))
            {
                Debug.LogWarning($"Prerequisites not met for module: {module.ModuleId}");
                return false;
            }
            
            _currentModule = module;
            _currentModuleIndex = moduleIndex;
            
            // Start first step of module
            StartModuleFirstStep(module);
            
            Debug.Log($"Started cultivation tutorial module: {module.ModuleName}");
            return true;
        }
        
        /// <summary>
        /// Start module first step
        /// </summary>
        private void StartModuleFirstStep(CultivationTutorialModule module)
        {
            // This would integrate with the specific step definitions for each module
            // For now, we'll use a placeholder approach
            
            switch (module.ModuleId)
            {
                case "environmental_control":
                    StartEnvironmentalControlModule();
                    break;
                    
                case "nutrient_management":
                    StartNutrientManagementModule();
                    break;
                    
                case "advanced_growing_techniques":
                    StartAdvancedGrowingModule();
                    break;
                    
                case "pest_disease_management":
                    StartPestDiseaseModule();
                    break;
                    
                case "harvest_processing":
                    StartHarvestProcessingModule();
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown module: {module.ModuleId}");
                    break;
            }
        }
        
        /// <summary>
        /// Start environmental control module
        /// </summary>
        private void StartEnvironmentalControlModule()
        {
            var introStep = CreateEnvironmentalIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create environmental intro step
        /// </summary>
        private TutorialStepSO CreateEnvironmentalIntroStep()
        {
            // This would normally be a proper ScriptableObject asset
            // For demonstration, creating a runtime step
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("env_control_intro");
            stepSO.SetTitle("Environmental Control Introduction");
            stepSO.SetShortDescription("Learn how environmental factors affect cannabis growth and quality.");
            stepSO.InstructionText = "Cannabis plants are highly sensitive to environmental conditions. Let's explore how to create the perfect growing environment.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start nutrient management module
        /// </summary>
        private void StartNutrientManagementModule()
        {
            var introStep = CreateNutrientIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create nutrient intro step
        /// </summary>
        private TutorialStepSO CreateNutrientIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("nutrient_mgmt_intro");
            stepSO.SetTitle("Nutrient Management Introduction");
            stepSO.SetShortDescription("Master the art of plant nutrition for maximum yield and potency.");
            stepSO.InstructionText = "Proper nutrition is crucial for healthy cannabis growth. We'll cover macronutrients, micronutrients, and feeding schedules.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start advanced growing module
        /// </summary>
        private void StartAdvancedGrowingModule()
        {
            var introStep = CreateAdvancedGrowingIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create advanced growing intro step
        /// </summary>
        private TutorialStepSO CreateAdvancedGrowingIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("advanced_growing_intro");
            stepSO.SetTitle("Advanced Growing Techniques");
            stepSO.SetShortDescription("Learn professional training and optimization techniques.");
            stepSO.InstructionText = "Advanced techniques like LST, HST, SCROG, and SOG can significantly improve your yields and plant structure.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start pest disease module
        /// </summary>
        private void StartPestDiseaseModule()
        {
            var introStep = CreatePestDiseaseIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create pest disease intro step
        /// </summary>
        private TutorialStepSO CreatePestDiseaseIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("pest_disease_intro");
            stepSO.SetTitle("Pest & Disease Management");
            stepSO.SetShortDescription("Protect your plants from common threats and maintain healthy crops.");
            stepSO.InstructionText = "Prevention is better than cure. Learn to identify, prevent, and treat common issues in cannabis cultivation.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start harvest processing module
        /// </summary>
        private void StartHarvestProcessingModule()
        {
            var introStep = CreateHarvestProcessingIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create harvest processing intro step
        /// </summary>
        private TutorialStepSO CreateHarvestProcessingIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("harvest_processing_intro");
            stepSO.SetTitle("Harvest & Post-Processing");
            stepSO.SetShortDescription("Learn optimal harvest timing and post-harvest processing techniques.");
            stepSO.InstructionText = "The harvest and curing process can make or break your final product quality. Let's master these crucial final steps.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Check if prerequisites are met
        /// </summary>
        private bool ArePrerequisitesMet()
        {
            // Check onboarding completion
            if (PlayerPrefs.GetInt("OnboardingCompleted", 0) == 0)
                return false;
            
            // Check player level
            var playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
            if (playerLevel < _minimumPlayerLevel)
                return false;
            
            // Check harvest count
            var harvestCount = PlayerPrefs.GetInt("TotalHarvestCount", 0);
            if (harvestCount < _minimumHarvestCount)
                return false;
            
            // Check required unlocks
            foreach (var unlock in _requiredUnlocks)
            {
                if (PlayerPrefs.GetInt($"Unlocked_{unlock}", 0) == 0)
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if module prerequisites are met
        /// </summary>
        private bool AreModulePrerequisitesMet(CultivationTutorialModule module)
        {
            foreach (var prerequisite in module.Prerequisites)
            {
                if (prerequisite == "onboarding_completed")
                {
                    if (PlayerPrefs.GetInt("OnboardingCompleted", 0) == 0)
                        return false;
                }
                else if (_moduleCompletionStatus.ContainsKey(prerequisite))
                {
                    if (!_moduleCompletionStatus[prerequisite])
                        return false;
                }
                else
                {
                    // Check if it's a general unlock
                    if (PlayerPrefs.GetInt($"Unlocked_{prerequisite}", 0) == 0)
                        return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Handle tutorial step completed
        /// </summary>
        private void HandleTutorialStepCompleted(TutorialStepSO step)
        {
            if (step == null) return;
            
            string stepId = step.StepId;
            
            if (!_isCultivationTutorialActive || stepId != _currentStepId)
                return;
            
            // Progress current module
            ProgressCurrentModule();
        }
        
        /// <summary>
        /// Progress current module
        /// </summary>
        private void ProgressCurrentModule()
        {
            if (string.IsNullOrEmpty(_currentModule.ModuleId))
                return;

            _currentStepId = "";
            
            // Update progress tracking
            var stepCount = GetModuleStepCount(_currentModule);
            var currentProgress = _moduleProgressTracking[_currentModule.ModuleId];
            _moduleProgressTracking[_currentModule.ModuleId] = Mathf.Min(currentProgress + 1f / stepCount, 1f);
            
            // Check if module is complete
            if (_moduleProgressTracking[_currentModule.ModuleId] >= 1f)
            {
                CompleteCurrentModule();
            }
            else
            {
                ContinueModuleNextStep();
            }
        }
        
        /// <summary>
        /// Get module step count
        /// </summary>
        private int GetModuleStepCount(CultivationTutorialModule module)
        {
            // This would normally be determined by the actual step definitions
            // For now, using estimated values based on module complexity
            switch (module.ModuleId)
            {
                case "environmental_control":
                    return 8;
                case "nutrient_management":
                    return 10;
                case "advanced_growing_techniques":
                    return 12;
                case "pest_disease_management":
                    return 9;
                case "harvest_processing":
                    return 11;
                default:
                    return 8;
            }
        }
        
        /// <summary>
        /// Continue module next step
        /// </summary>
        private void ContinueModuleNextStep()
        {
            // This would load the next step definition for the current module
            // For now, we'll use a placeholder approach
            Debug.Log($"Continuing to next step in module: {_currentModule.ModuleId}");
        }
        
        /// <summary>
        /// Complete current module
        /// </summary>
        private void CompleteCurrentModule()
        {
            if (string.IsNullOrEmpty(_currentModule.ModuleId))
                return;

            // Mark module as completed
            _moduleCompletionStatus[_currentModule.ModuleId] = true;
            _moduleProgressTracking[_currentModule.ModuleId] = 1f;
            
            // Give completion rewards
            GiveModuleCompletionRewards(_currentModule);
            
            // Fire completion event
            _onCultivationModuleCompleted?.Raise(_currentModule.ModuleId);
            
            Debug.Log($"Completed cultivation tutorial module: {_currentModule.ModuleName}");
            
            // Check if all modules are complete
            if (AreAllModulesComplete())
            {
                // Complete entire tutorial
                CompleteCultivationTutorial();
            }
            else
            {
                // Move to next module
                MoveToNextModule();
            }
        }
        
        /// <summary>
        /// Give module completion rewards
        /// </summary>
        private void GiveModuleCompletionRewards(CultivationTutorialModule module)
        {
            // Give skill points based on module difficulty
            var skillPointReward = GetModuleSkillPointReward(module.DifficultyLevel);
            var currentSkillPoints = PlayerPrefs.GetInt("SkillPoints", 0);
            PlayerPrefs.SetInt("SkillPoints", currentSkillPoints + skillPointReward);
            
            // Unlock module-specific features
            PlayerPrefs.SetInt($"Unlocked_Advanced_{module.ModuleId}", 1);
            
            Debug.Log($"Gave {skillPointReward} skill points for completing {module.ModuleName}");
        }
        
        /// <summary>
        /// Get module skill point reward
        /// </summary>
        private int GetModuleSkillPointReward(TutorialDifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case TutorialDifficultyLevel.Beginner:
                    return 2;
                case TutorialDifficultyLevel.Intermediate:
                    return 3;
                case TutorialDifficultyLevel.Advanced:
                    return 5;
                case TutorialDifficultyLevel.Expert:
                    return 7;
                default:
                    return 2;
            }
        }
        
        /// <summary>
        /// Check if all modules are complete
        /// </summary>
        private bool AreAllModulesComplete()
        {
            foreach (var module in _tutorialModules)
            {
                if (!_moduleCompletionStatus[module.ModuleId])
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Move to next module
        /// </summary>
        private void MoveToNextModule()
        {
            _currentModuleIndex++;
            
            if (_currentModuleIndex < _tutorialModules.Count)
            {
                StartTutorialModule(_currentModuleIndex);
            }
        }
        
        /// <summary>
        /// Complete cultivation tutorial
        /// </summary>
        private void CompleteCultivationTutorial()
        {
            _isCultivationTutorialActive = false;
            _isCultivationTutorialCompleted = true;
            
            // Save completion status
            PlayerPrefs.SetInt("CultivationTutorialCompleted", 1);
            
            // Give final completion rewards
            GiveFinalCompletionRewards();
            
            _onCultivationTutorialCompleted?.Raise();
            
            Debug.Log("Completed entire cultivation tutorial");
        }
        
        /// <summary>
        /// Give final completion rewards
        /// </summary>
        private void GiveFinalCompletionRewards()
        {
            // Give bonus skill points
            var currentSkillPoints = PlayerPrefs.GetInt("SkillPoints", 0);
            PlayerPrefs.SetInt("SkillPoints", currentSkillPoints + 10);
            
            // Unlock master cultivator features
            PlayerPrefs.SetInt("Unlocked_MasterCultivator", 1);
            PlayerPrefs.SetInt("Unlocked_AdvancedAnalytics", 1);
            PlayerPrefs.SetInt("Unlocked_ExpertMode", 1);
            
            Debug.Log("Gave final cultivation tutorial completion rewards");
        }
        
        /// <summary>
        /// Handle tutorial sequence completed
        /// </summary>
        private void HandleTutorialSequenceCompleted(TutorialSequenceSO sequence)
        {
            if (sequence == null) return;
            
            if (sequence.SequenceId == _cultivationTutorialSequence?.SequenceId)
            {
                CompleteCultivationTutorial();
            }
        }
        
        /// <summary>
        /// Get cultivation tutorial progress
        /// </summary>
        public CultivationTutorialProgress GetTutorialProgress()
        {
            var totalModules = _tutorialModules.Count;
            var completedModules = 0;
            var totalProgress = 0f;
            
            foreach (var module in _tutorialModules)
            {
                if (_moduleCompletionStatus[module.ModuleId])
                    completedModules++;
                
                totalProgress += _moduleProgressTracking[module.ModuleId];
            }
            
            return new CultivationTutorialProgress
            {
                IsActive = _isCultivationTutorialActive,
                IsCompleted = _isCultivationTutorialCompleted,
                CurrentModuleIndex = _currentModuleIndex,
                TotalModules = totalModules,
                CompletedModules = completedModules,
                OverallProgress = totalProgress / totalModules,
                CurrentModuleId = string.IsNullOrEmpty(_currentModule.ModuleId) ? "" : _currentModule.ModuleId,
                CurrentStepId = _currentStepId
            };
        }
        
        protected override void OnDestroy()
        {
            // Unsubscribe from events
            if (_tutorialManager != null)
            {
                TutorialManager.OnStepCompleted -= HandleTutorialStepCompleted;
                TutorialManager.OnSequenceCompleted -= HandleTutorialSequenceCompleted;
            }
            
            base.OnDestroy();
        }
    }
    
    /// <summary>
    /// Cultivation tutorial module definition
    /// </summary>
    [System.Serializable]
    public struct CultivationTutorialModule
    {
        public string ModuleId;
        public string ModuleName;
        public string Description;
        public float EstimatedDuration;
        public TutorialDifficultyLevel DifficultyLevel;
        public List<string> Prerequisites;
        public List<string> LearningObjectives;
    }
    
    /// <summary>
    /// Cultivation tutorial progress information
    /// </summary>
    [System.Serializable]
    public struct CultivationTutorialProgress
    {
        public bool IsActive;
        public bool IsCompleted;
        public int CurrentModuleIndex;
        public int TotalModules;
        public int CompletedModules;
        public float OverallProgress;
        public string CurrentModuleId;
        public string CurrentStepId;
    }
}