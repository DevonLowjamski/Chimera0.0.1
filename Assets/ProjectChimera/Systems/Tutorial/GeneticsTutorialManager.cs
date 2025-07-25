using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Tutorial;
using ProjectChimera.Data.Genetics;
using ProjectChimera.Systems.Genetics;

namespace ProjectChimera.Systems.Tutorial
{
    /// <summary>
    /// Genetics tutorial manager for Project Chimera.
    /// Manages breeding, genetics, and strain development education.
    /// </summary>
    public class GeneticsTutorialManager : ChimeraManager
    {
        [Header("Genetics Tutorial Configuration")]
        [SerializeField] private TutorialSequenceSO _geneticsTutorialSequence;
        [SerializeField] private List<GeneticsTutorialModule> _tutorialModules;
        [SerializeField] private bool _autoStartAfterCultivation = true;
        
        [Header("Tutorial Prerequisites")]
        [SerializeField] private int _minimumPlayerLevel = 5;
        [SerializeField] private int _minimumHarvestCount = 3;
        [SerializeField] private List<string> _requiredUnlocks;
        
        [Header("Event Channels")]
        [SerializeField] private SimpleGameEventSO _onGeneticsTutorialStarted;
        [SerializeField] private SimpleGameEventSO _onGeneticsTutorialCompleted;
        [SerializeField] private StringGameEventSO _onGeneticsModuleCompleted;
        
        // State
        private bool _isGeneticsTutorialActive;
        private bool _isGeneticsTutorialCompleted;
        private GeneticsTutorialModule _currentModule;
        private int _currentModuleIndex;
        private string _currentStepId;
        
        // Managers
        private TutorialManager _tutorialManager;
        private GeneticsManager _geneticsManager;
        
        // Tutorial tracking
        private Dictionary<string, bool> _moduleCompletionStatus;
        private Dictionary<string, float> _moduleProgressTracking;
        private Dictionary<string, int> _moduleAttemptCounts;
        
        // Properties
        public bool IsGeneticsTutorialActive => _isGeneticsTutorialActive;
        public bool IsGeneticsTutorialCompleted => _isGeneticsTutorialCompleted;
        public GeneticsTutorialModule CurrentModule => _currentModule;
        public int CurrentModuleIndex => _currentModuleIndex;
        public string CurrentStepId => _currentStepId;
        
        protected override void OnManagerInitialize()
        {
            // Get required managers
            _tutorialManager = GameManager.Instance.GetManager<TutorialManager>();
            _geneticsManager = GameManager.Instance.GetManager<GeneticsManager>();
            
            // Initialize tracking dictionaries
            _moduleCompletionStatus = new Dictionary<string, bool>();
            _moduleProgressTracking = new Dictionary<string, float>();
            _moduleAttemptCounts = new Dictionary<string, int>();
            
            // Initialize tutorial modules
            InitializeTutorialModules();
            
            // Check if tutorial should auto-start
            CheckTutorialPrerequisites();
            
            // Subscribe to events
            SubscribeToEvents();
            
            Debug.Log("Genetics tutorial manager initialized");
        }

        protected override void OnManagerShutdown()
        {
            // Unsubscribe from events
            // Clean up any active tutorials
            
            Debug.Log("Genetics tutorial manager shutdown");
        }
        
        /// <summary>
        /// Initialize tutorial modules
        /// </summary>
        private void InitializeTutorialModules()
        {
            if (_tutorialModules == null)
            {
                _tutorialModules = new List<GeneticsTutorialModule>();
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
                _moduleAttemptCounts[module.ModuleId] = 0;
            }
        }
        
        /// <summary>
        /// Create default tutorial modules
        /// </summary>
        private void CreateDefaultTutorialModules()
        {
            _tutorialModules.AddRange(new List<GeneticsTutorialModule>
            {
                new GeneticsTutorialModule
                {
                    ModuleId = "genetics_fundamentals",
                    ModuleName = "Cannabis Genetics Fundamentals",
                    Description = "Learn the basics of cannabis genetics, inheritance patterns, and genetic terminology.",
                    EstimatedDuration = 20f,
                    DifficultyLevel = TutorialDifficultyLevel.Beginner,
                    Prerequisites = new List<string> { "cultivation_tutorial_completed" },
                    LearningObjectives = new List<string>
                    {
                        "Understand basic genetics terminology (alleles, phenotype, genotype)",
                        "Learn about dominant and recessive traits",
                        "Understand Mendelian inheritance patterns",
                        "Recognize the difference between genotype and phenotype",
                        "Learn about heterozygous and homozygous genetics"
                    }
                },
                
                new GeneticsTutorialModule
                {
                    ModuleId = "strain_analysis",
                    ModuleName = "Strain Analysis & Selection",
                    Description = "Learn to analyze strain characteristics, genetics profiles, and make informed breeding decisions.",
                    EstimatedDuration = 25f,
                    DifficultyLevel = TutorialDifficultyLevel.Intermediate,
                    Prerequisites = new List<string> { "genetics_fundamentals" },
                    LearningObjectives = new List<string>
                    {
                        "Analyze strain genetic profiles and lineages",
                        "Understand indica, sativa, and hybrid classifications",
                        "Learn to read and interpret genetic charts",
                        "Identify desirable traits for breeding goals",
                        "Understand genetic stability and variation"
                    }
                },
                
                new GeneticsTutorialModule
                {
                    ModuleId = "breeding_basics",
                    ModuleName = "Basic Breeding Techniques",
                    Description = "Learn fundamental breeding methods, pollination techniques, and seed production.",
                    EstimatedDuration = 30f,
                    DifficultyLevel = TutorialDifficultyLevel.Intermediate,
                    Prerequisites = new List<string> { "strain_analysis" },
                    LearningObjectives = new List<string>
                    {
                        "Learn controlled pollination techniques",
                        "Understand F1, F2, and subsequent generations",
                        "Master pollen collection and storage",
                        "Learn selective breeding strategies",
                        "Understand outcrossing vs. inbreeding"
                    }
                },
                
                new GeneticsTutorialModule
                {
                    ModuleId = "advanced_breeding",
                    ModuleName = "Advanced Breeding Strategies",
                    Description = "Master complex breeding techniques including backcrossing, line breeding, and trait stabilization.",
                    EstimatedDuration = 35f,
                    DifficultyLevel = TutorialDifficultyLevel.Advanced,
                    Prerequisites = new List<string> { "breeding_basics" },
                    LearningObjectives = new List<string>
                    {
                        "Master backcrossing for trait stabilization",
                        "Learn line breeding techniques",
                        "Understand polyploid breeding",
                        "Practice trait pyramiding strategies",
                        "Learn genetic bottleneck avoidance"
                    }
                },
                
                new GeneticsTutorialModule
                {
                    ModuleId = "phenotype_hunting",
                    ModuleName = "Phenotype Hunting & Selection",
                    Description = "Learn to identify and select superior phenotypes for breeding and production.",
                    EstimatedDuration = 28f,
                    DifficultyLevel = TutorialDifficultyLevel.Advanced,
                    Prerequisites = new List<string> { "breeding_basics" },
                    LearningObjectives = new List<string>
                    {
                        "Learn systematic phenotype evaluation",
                        "Understand trait scoring and documentation",
                        "Master clonal selection techniques",
                        "Learn statistical analysis for breeding",
                        "Understand progeny testing methods"
                    }
                },
                
                new GeneticsTutorialModule
                {
                    ModuleId = "genetic_preservation",
                    ModuleName = "Genetic Preservation & Banking",
                    Description = "Learn to preserve genetics through tissue culture, seed banking, and long-term storage.",
                    EstimatedDuration = 22f,
                    DifficultyLevel = TutorialDifficultyLevel.Expert,
                    Prerequisites = new List<string> { "phenotype_hunting", "advanced_breeding" },
                    LearningObjectives = new List<string>
                    {
                        "Master tissue culture techniques",
                        "Learn proper seed storage methods",
                        "Understand cryopreservation principles",
                        "Practice genetic library management",
                        "Learn contamination prevention protocols"
                    }
                }
            });
        }
        
        /// <summary>
        /// Check tutorial prerequisites
        /// </summary>
        private void CheckTutorialPrerequisites()
        {
            if (!_autoStartAfterCultivation)
                return;
            
            // Check if cultivation tutorial is completed
            var cultivationCompleted = PlayerPrefs.GetInt("CultivationTutorialCompleted", 0) == 1;
            if (!cultivationCompleted)
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
            Debug.Log("Genetics tutorial prerequisites met - ready to start");
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
        /// Start genetics tutorial
        /// </summary>
        public bool StartGeneticsTutorial()
        {
            if (_isGeneticsTutorialActive || _isGeneticsTutorialCompleted)
                return false;
            
            // Check prerequisites one more time
            if (!ArePrerequisitesMet())
            {
                Debug.LogWarning("Prerequisites not met for genetics tutorial");
                return false;
            }
            
            _isGeneticsTutorialActive = true;
            _currentModuleIndex = 0;
            
            // Start first module
            StartTutorialModule(0);
            
            _onGeneticsTutorialStarted?.Raise();
            
            Debug.Log("Started genetics tutorial");
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
            _moduleAttemptCounts[module.ModuleId]++;
            
            // Start first step of module
            StartModuleFirstStep(module);
            
            Debug.Log($"Started genetics tutorial module: {module.ModuleName}");
            return true;
        }
        
        /// <summary>
        /// Start module first step
        /// </summary>
        private void StartModuleFirstStep(GeneticsTutorialModule module)
        {
            switch (module.ModuleId)
            {
                case "genetics_fundamentals":
                    StartGeneticsFundamentalsModule();
                    break;
                    
                case "strain_analysis":
                    StartStrainAnalysisModule();
                    break;
                    
                case "breeding_basics":
                    StartBreedingBasicsModule();
                    break;
                    
                case "advanced_breeding":
                    StartAdvancedBreedingModule();
                    break;
                    
                case "phenotype_hunting":
                    StartPhenotypeHuntingModule();
                    break;
                    
                case "genetic_preservation":
                    StartGeneticPreservationModule();
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown genetics module: {module.ModuleId}");
                    break;
            }
        }
        
        /// <summary>
        /// Start genetics fundamentals module
        /// </summary>
        private void StartGeneticsFundamentalsModule()
        {
            var introStep = CreateGeneticsFundamentalsIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create genetics fundamentals intro step
        /// </summary>
        private TutorialStepSO CreateGeneticsFundamentalsIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("genetics_fundamentals_intro");
            stepSO.SetTitle("Cannabis Genetics Fundamentals");
            stepSO.SetShortDescription("Learn the basic principles of cannabis genetics and inheritance.");
            stepSO.InstructionText = "Understanding genetics is crucial for successful breeding. We'll cover the fundamental concepts that govern how traits are passed from parent plants to offspring.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start strain analysis module
        /// </summary>
        private void StartStrainAnalysisModule()
        {
            var introStep = CreateStrainAnalysisIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create strain analysis intro step
        /// </summary>
        private TutorialStepSO CreateStrainAnalysisIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("strain_analysis_intro");
            stepSO.SetTitle("Strain Analysis & Selection");
            stepSO.SetShortDescription("Learn to analyze and select strains for breeding programs.");
            stepSO.InstructionText = "Successful breeding starts with understanding the genetic background and characteristics of your parent strains. We'll learn to analyze lineages and select optimal breeding pairs.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start breeding basics module
        /// </summary>
        private void StartBreedingBasicsModule()
        {
            var introStep = CreateBreedingBasicsIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create breeding basics intro step
        /// </summary>
        private TutorialStepSO CreateBreedingBasicsIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("breeding_basics_intro");
            stepSO.SetTitle("Basic Breeding Techniques");
            stepSO.SetShortDescription("Master fundamental breeding methods and seed production.");
            stepSO.InstructionText = "Learn the practical techniques for controlled pollination, seed production, and managing breeding populations. These skills form the foundation of all cannabis breeding work.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start advanced breeding module
        /// </summary>
        private void StartAdvancedBreedingModule()
        {
            var introStep = CreateAdvancedBreedingIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create advanced breeding intro step
        /// </summary>
        private TutorialStepSO CreateAdvancedBreedingIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("advanced_breeding_intro");
            stepSO.SetTitle("Advanced Breeding Strategies");
            stepSO.SetShortDescription("Master complex breeding techniques for trait stabilization.");
            stepSO.InstructionText = "Advanced breeding techniques allow you to stabilize desirable traits and create consistent, high-quality genetics. Learn backcrossing, line breeding, and trait pyramiding strategies.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start phenotype hunting module
        /// </summary>
        private void StartPhenotypeHuntingModule()
        {
            var introStep = CreatePhenotypeHuntingIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create phenotype hunting intro step
        /// </summary>
        private TutorialStepSO CreatePhenotypeHuntingIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("phenotype_hunting_intro");
            stepSO.SetTitle("Phenotype Hunting & Selection");
            stepSO.SetShortDescription("Learn to identify and select superior genetic expressions.");
            stepSO.InstructionText = "Phenotype hunting is the art and science of finding exceptional individual plants within genetic populations. Master systematic evaluation and selection techniques.";
            stepSO.SetStepType(TutorialStepType.Introduction);
            stepSO.SetValidationType(TutorialValidationType.ButtonClick);
            stepSO.SetTimeoutDuration(0f);
            stepSO.SetCanSkip(false);
            
            return stepSO;
        }
        
        /// <summary>
        /// Start genetic preservation module
        /// </summary>
        private void StartGeneticPreservationModule()
        {
            var introStep = CreateGeneticPreservationIntroStep();
            _currentStepId = introStep.StepId;
            
            if (_tutorialManager != null)
            {
                _tutorialManager.StartTutorialStep(introStep);
            }
        }
        
        /// <summary>
        /// Create genetic preservation intro step
        /// </summary>
        private TutorialStepSO CreateGeneticPreservationIntroStep()
        {
            var stepSO = ScriptableObject.CreateInstance<TutorialStepSO>();
            stepSO.SetStepId("genetic_preservation_intro");
            stepSO.SetTitle("Genetic Preservation & Banking");
            stepSO.SetShortDescription("Learn advanced techniques for preserving valuable genetics.");
            stepSO.InstructionText = "Genetic preservation ensures valuable strains and breeding lines are maintained for future use. Learn tissue culture, seed banking, and professional preservation techniques.";
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
            // Check cultivation tutorial completion
            if (PlayerPrefs.GetInt("CultivationTutorialCompleted", 0) == 0)
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
        private bool AreModulePrerequisitesMet(GeneticsTutorialModule module)
        {
            foreach (var prerequisite in module.Prerequisites)
            {
                if (prerequisite == "cultivation_tutorial_completed")
                {
                    if (PlayerPrefs.GetInt("CultivationTutorialCompleted", 0) == 0)
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
            
            if (!_isGeneticsTutorialActive || stepId != _currentStepId)
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
        private int GetModuleStepCount(GeneticsTutorialModule module)
        {
            // This would normally be determined by the actual step definitions
            // For now, using estimated values based on module complexity
            switch (module.ModuleId)
            {
                case "genetics_fundamentals":
                    return 10;
                case "strain_analysis":
                    return 12;
                case "breeding_basics":
                    return 14;
                case "advanced_breeding":
                    return 16;
                case "phenotype_hunting":
                    return 13;
                case "genetic_preservation":
                    return 11;
                default:
                    return 10;
            }
        }
        
        /// <summary>
        /// Continue module next step
        /// </summary>
        private void ContinueModuleNextStep()
        {
            // This would load the next step definition for the current module
            // For now, we'll use a placeholder approach
            Debug.Log($"Continuing to next step in genetics module: {_currentModule.ModuleId}");
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
            _onGeneticsModuleCompleted?.Raise(_currentModule.ModuleId);
            
            Debug.Log($"Completed genetics tutorial module: {_currentModule.ModuleName}");
            
            // Check if all modules are complete
            if (AreAllModulesComplete())
            {
                // Complete entire tutorial
                CompleteGeneticsTutorial();
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
        private void GiveModuleCompletionRewards(GeneticsTutorialModule module)
        {
            // Give skill points based on module difficulty and attempts
            var baseSkillPoints = GetModuleSkillPointReward(module.DifficultyLevel);
            var attemptBonus = _moduleAttemptCounts[module.ModuleId] == 1 ? 1 : 0; // Bonus for first try
            var totalSkillPoints = baseSkillPoints + attemptBonus;
            
            var currentSkillPoints = PlayerPrefs.GetInt("SkillPoints", 0);
            PlayerPrefs.SetInt("SkillPoints", currentSkillPoints + totalSkillPoints);
            
            // Unlock module-specific features
            PlayerPrefs.SetInt($"Unlocked_Genetics_{module.ModuleId}", 1);
            
            // Special unlocks for advanced modules
            if (module.ModuleId == "advanced_breeding")
            {
                PlayerPrefs.SetInt("Unlocked_AdvancedBreeding", 1);
                PlayerPrefs.SetInt("Unlocked_TraitStabilization", 1);
            }
            else if (module.ModuleId == "genetic_preservation")
            {
                PlayerPrefs.SetInt("Unlocked_TissueCulture", 1);
                PlayerPrefs.SetInt("Unlocked_GeneticBanking", 1);
            }
            
            Debug.Log($"Gave {totalSkillPoints} skill points for completing {module.ModuleName}");
        }
        
        /// <summary>
        /// Get module skill point reward
        /// </summary>
        private int GetModuleSkillPointReward(TutorialDifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case TutorialDifficultyLevel.Beginner:
                    return 3;
                case TutorialDifficultyLevel.Intermediate:
                    return 5;
                case TutorialDifficultyLevel.Advanced:
                    return 8;
                case TutorialDifficultyLevel.Expert:
                    return 12;
                default:
                    return 3;
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
        /// Complete genetics tutorial
        /// </summary>
        private void CompleteGeneticsTutorial()
        {
            _isGeneticsTutorialActive = false;
            _isGeneticsTutorialCompleted = true;
            
            // Save completion status
            PlayerPrefs.SetInt("GeneticsTutorialCompleted", 1);
            
            // Give final completion rewards
            GiveFinalCompletionRewards();
            
            _onGeneticsTutorialCompleted?.Raise();
            
            Debug.Log("Completed entire genetics tutorial");
        }
        
        /// <summary>
        /// Give final completion rewards
        /// </summary>
        private void GiveFinalCompletionRewards()
        {
            // Give bonus skill points
            var currentSkillPoints = PlayerPrefs.GetInt("SkillPoints", 0);
            PlayerPrefs.SetInt("SkillPoints", currentSkillPoints + 15);
            
            // Unlock master breeder features
            PlayerPrefs.SetInt("Unlocked_MasterBreeder", 1);
            PlayerPrefs.SetInt("Unlocked_GeneticAnalysis", 1);
            PlayerPrefs.SetInt("Unlocked_CustomBreedingPrograms", 1);
            PlayerPrefs.SetInt("Unlocked_ProfessionalGenetics", 1);
            
            Debug.Log("Gave final genetics tutorial completion rewards");
        }
        
        /// <summary>
        /// Handle tutorial sequence completed
        /// </summary>
        private void HandleTutorialSequenceCompleted(TutorialSequenceSO sequence)
        {
            if (sequence == null) return;
            
            if (sequence.SequenceId == _geneticsTutorialSequence?.SequenceId)
            {
                CompleteGeneticsTutorial();
            }
        }
        
        /// <summary>
        /// Get genetics tutorial progress
        /// </summary>
        public GeneticsTutorialProgress GetTutorialProgress()
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
            
            return new GeneticsTutorialProgress
            {
                IsActive = _isGeneticsTutorialActive,
                IsCompleted = _isGeneticsTutorialCompleted,
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
    /// Genetics tutorial module definition
    /// </summary>
    [System.Serializable]
    public struct GeneticsTutorialModule
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
    /// Genetics tutorial progress information
    /// </summary>
    [System.Serializable]
    public struct GeneticsTutorialProgress
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