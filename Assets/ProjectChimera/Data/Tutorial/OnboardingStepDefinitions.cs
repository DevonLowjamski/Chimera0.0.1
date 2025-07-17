using UnityEngine;
using System;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Tutorial
{
    /// <summary>
    /// Onboarding step definitions for Project Chimera.
    /// Contains predefined tutorial steps for the onboarding sequence.
    /// </summary>
    [CreateAssetMenu(fileName = "OnboardingStepDefinitions", menuName = "Project Chimera/Tutorial/Onboarding Step Definitions")]
    public class OnboardingStepDefinitions : ChimeraDataSO
    {
        [Header("Welcome Phase Steps")]
        [SerializeField] private List<TutorialStepData> _welcomeSteps;
        
        [Header("Facility Setup Phase Steps")]
        [SerializeField] private List<TutorialStepData> _facilitySetupSteps;
        
        [Header("First Plant Phase Steps")]
        [SerializeField] private List<TutorialStepData> _firstPlantSteps;
        
        [Header("Step Templates")]
        [SerializeField] private TutorialStepTemplate _introductionTemplate;
        [SerializeField] private TutorialStepTemplate _navigationTemplate;
        [SerializeField] private TutorialStepTemplate _interactionTemplate;
        [SerializeField] private TutorialStepTemplate _validationTemplate;
        
        // Properties
        public List<TutorialStepData> WelcomeSteps => _welcomeSteps;
        public List<TutorialStepData> FacilitySetupSteps => _facilitySetupSteps;
        public List<TutorialStepData> FirstPlantSteps => _firstPlantSteps;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            // Initialize step definitions if empty
            if (_welcomeSteps == null || _welcomeSteps.Count == 0)
            {
                InitializeDefaultSteps();
            }
        }
        
        /// <summary>
        /// Initialize default onboarding steps
        /// </summary>
        private void InitializeDefaultSteps()
        {
            InitializeWelcomeSteps();
            InitializeFacilitySetupSteps();
            InitializeFirstPlantSteps();
            
            Debug.Log("Initialized default onboarding steps");
        }
        
        /// <summary>
        /// Initialize welcome phase steps
        /// </summary>
        private void InitializeWelcomeSteps()
        {
            _welcomeSteps = new List<TutorialStepData>
            {
                // Step 1: Welcome to Project Chimera
                new TutorialStepData
                {
                    StepId = "welcome_intro",
                    Title = "Welcome to Project Chimera", 
                    Description = "Welcome to the most advanced cannabis cultivation simulation! Let's get you started on your journey to becoming a master cultivator.",
                    DetailedInstructions = "Click 'Next' to begin your tutorial journey.",
                    StepType = TutorialStepType.Introduction,
                    ValidationType = TutorialValidationType.ButtonClick,
                    ValidationTarget = "next-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 2: Game Overview
                new TutorialStepData
                {
                    StepId = "welcome_overview",
                    Title = "About Project Chimera",
                    Description = "Project Chimera simulates real cannabis cultivation with scientific accuracy. You'll manage genetics, environment, economics, and advanced growing techniques.",
                    DetailedInstructions = "In this game, you'll build and manage cannabis cultivation facilities, experiment with genetics, control environmental systems, and participate in a dynamic market economy.",
                    StepType = TutorialStepType.Information,
                    ValidationType = TutorialValidationType.Timer,
                    TimeoutDuration = 8f,
                    CanSkip = true
                },
                
                // Step 3: Interface Introduction
                new TutorialStepData
                {
                    StepId = "welcome_interface",
                    Title = "User Interface Overview",
                    Description = "Let's familiarize you with the main interface elements you'll use throughout the game.",
                    DetailedInstructions = "Look around the interface. You'll see the main menu, resource displays, and navigation elements.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.Timer,
                    TimeoutDuration = 6f,
                    CanSkip = true
                },
                
                // Step 4: Starting Resources
                new TutorialStepData
                {
                    StepId = "welcome_resources",
                    Title = "Starting Resources",
                    Description = "You begin with $25,000 in funding and 5 skill points to help you get started.",
                    DetailedInstructions = "Check your currency display to see your starting funds. You'll use this money to purchase equipment, seeds, and supplies.",
                    StepType = TutorialStepType.Information,
                    ValidationType = TutorialValidationType.ButtonClick,
                    ValidationTarget = "currency-display",
                    TimeoutDuration = 0f,
                    CanSkip = false
                }
            };
        }
        
        /// <summary>
        /// Initialize facility setup phase steps
        /// </summary>
        private void InitializeFacilitySetupSteps()
        {
            _facilitySetupSteps = new List<TutorialStepData>
            {
                // Step 1: Facility Introduction
                new TutorialStepData
                {
                    StepId = "facility_intro",
                    Title = "Setting Up Your Facility",
                    Description = "Now let's set up your first cultivation facility. Every successful operation starts with proper planning.",
                    DetailedInstructions = "We'll guide you through creating your first grow room with essential equipment.",
                    StepType = TutorialStepType.Introduction,
                    ValidationType = TutorialValidationType.ButtonClick,
                    ValidationTarget = "continue-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 2: Access Facility Manager
                new TutorialStepData
                {
                    StepId = "facility_access_manager",
                    Title = "Open Facility Manager",
                    Description = "The Facility Manager is where you design and manage your grow spaces.",
                    DetailedInstructions = "Click on the 'Facility' button in the main navigation to open the Facility Manager.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.ButtonClick,
                    ValidationTarget = "facility-nav-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 3: Create First Room
                new TutorialStepData
                {
                    StepId = "facility_create_room",
                    Title = "Create Your First Grow Room",
                    Description = "Let's create a small grow room to start your cultivation journey.",
                    DetailedInstructions = "Click the 'Add Room' button to create a new cultivation space.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.ButtonClick,
                    ValidationTarget = "add-room-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 4: Room Configuration
                new TutorialStepData
                {
                    StepId = "facility_configure_room",
                    Title = "Configure Room Settings",
                    Description = "Set up the basic parameters for your grow room including size and type.",
                    DetailedInstructions = "Choose 'Small Room' (4x4 feet) for your first growing space. This is perfect for beginners.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.UIInteraction,
                    ValidationTarget = "room-size-dropdown",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 5: Add Essential Equipment
                new TutorialStepData
                {
                    StepId = "facility_add_lighting",
                    Title = "Install Lighting System",
                    Description = "Every grow room needs proper lighting. Let's install an LED lighting system.",
                    DetailedInstructions = "Click on 'Equipment' tab, then select 'LED Grow Light (300W)' and click 'Install'.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "led-light-300w",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 6: Add Ventilation
                new TutorialStepData
                {
                    StepId = "facility_add_ventilation",
                    Title = "Install Ventilation System",
                    Description = "Proper air circulation is crucial for healthy plant growth.",
                    DetailedInstructions = "Install the 'Inline Fan (4-inch)' to provide air circulation for your room.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "inline-fan-4inch",
                    TimeoutDuration = 0f,
                    CanSkip = false
                }
            };
        }
        
        /// <summary>
        /// Initialize first plant phase steps
        /// </summary>
        private void InitializeFirstPlantSteps()
        {
            _firstPlantSteps = new List<TutorialStepData>
            {
                // Step 1: Plant Introduction
                new TutorialStepData
                {
                    StepId = "plant_intro",
                    Title = "Growing Your First Plant",
                    Description = "Now that your facility is ready, let's plant your first cannabis seed!",
                    DetailedInstructions = "We'll guide you through selecting genetics, planting, and basic care.",
                    StepType = TutorialStepType.Introduction,
                    ValidationType = TutorialValidationType.ButtonClick,
                    ValidationTarget = "start-growing-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 2: Access Genetics Library
                new TutorialStepData
                {
                    StepId = "plant_access_genetics",
                    Title = "Choose Your Genetics",
                    Description = "Every grow starts with selecting the right genetics for your goals.",
                    DetailedInstructions = "Click on 'Genetics' in the main navigation to access your strain library.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.ButtonClick,
                    ValidationTarget = "genetics-nav-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 3: Select Starter Strain
                new TutorialStepData
                {
                    StepId = "plant_select_strain",
                    Title = "Select a Beginner-Friendly Strain",
                    Description = "For your first grow, we recommend a hardy, easy-to-grow strain.",
                    DetailedInstructions = "Select 'Northern Lights Auto' - it's perfect for beginners with its resilience and short flowering time.",
                    StepType = TutorialStepType.Choice,
                    ValidationType = TutorialValidationType.UIInteraction,
                    ValidationTarget = "northern-lights-auto",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 4: Plant the Seed
                new TutorialStepData
                {
                    StepId = "plant_seed",
                    Title = "Plant Your Seed",
                    Description = "Time to get your seed into the growing medium and start the lifecycle.",
                    DetailedInstructions = "Click 'Plant Seed' to place your Northern Lights Auto seed in the grow room you created.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "plant-seed-action",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 5: Monitor Plant Status
                new TutorialStepData
                {
                    StepId = "plant_monitor",
                    Title = "Monitor Your Plant",
                    Description = "Keep track of your plant's health and growth progress.",
                    DetailedInstructions = "Click on your planted seed to view its status, health, and environmental needs.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.UIInteraction,
                    ValidationTarget = "plant-status-panel",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 6: Time Acceleration
                new TutorialStepData
                {
                    StepId = "plant_time_acceleration",
                    Title = "Speed Up Time",
                    Description = "Cannabis plants take time to grow. Use time acceleration to see faster results.",
                    DetailedInstructions = "Click the time acceleration button (⏩) to speed up time and watch your plant grow.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "time-acceleration-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 7: First Harvest
                new TutorialStepData
                {
                    StepId = "plant_harvest",
                    Title = "Harvest Your First Plant",
                    Description = "Congratulations! Your plant has matured and is ready for harvest.",
                    DetailedInstructions = "When your plant reaches maturity, click 'Harvest' to collect your first yield.",
                    StepType = TutorialStepType.Completion,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "harvest-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                }
            };
        }
        
        /// <summary>
        /// Get step by ID
        /// </summary>
        public TutorialStepData GetStepById(string stepId)
        {
            // Search in all step lists
            foreach (var step in _welcomeSteps)
            {
                if (step.StepId == stepId)
                    return step;
            }
            
            foreach (var step in _facilitySetupSteps)
            {
                if (step.StepId == stepId)
                    return step;
            }
            
            foreach (var step in _firstPlantSteps)
            {
                if (step.StepId == stepId)
                    return step;
            }
            
            return default;
        }
        
        /// <summary>
        /// Get all onboarding steps
        /// </summary>
        public List<TutorialStepData> GetAllOnboardingSteps()
        {
            var allSteps = new List<TutorialStepData>();
            allSteps.AddRange(_welcomeSteps);
            allSteps.AddRange(_facilitySetupSteps);
            allSteps.AddRange(_firstPlantSteps);
            
            return allSteps;
        }
        
        /// <summary>
        /// Validate data integrity
        /// </summary>
        protected override bool ValidateDataSpecific()
        {
            var allSteps = GetAllOnboardingSteps();
            var stepIds = new HashSet<string>();
            
            foreach (var step in allSteps)
            {
                if (string.IsNullOrEmpty(step.StepId))
                {
                    LogError($"Onboarding step has empty StepId: {step.Title}");
                    continue;
                }
                
                if (stepIds.Contains(step.StepId))
                {
                    LogError($"Duplicate onboarding step ID: {step.StepId}");
                }
                else
                {
                    stepIds.Add(step.StepId);
                }
                
                if (string.IsNullOrEmpty(step.Title))
                {
                    LogWarning($"Onboarding step {step.StepId} has empty title");
                }
                
                if (string.IsNullOrEmpty(step.DetailedInstructions))
                {
                    LogWarning($"Onboarding step {step.StepId} has empty instruction text");
                }
            }
            
            Debug.Log($"Validated {allSteps.Count} onboarding steps");
            return true;
        }
    }
    
    /// <summary>
    /// Tutorial step template for common configurations
    /// </summary>
    [System.Serializable]
    public struct TutorialStepTemplate
    {
        public TutorialStepType StepType;
        public TutorialValidationType ValidationType;
        public float DefaultDuration;
        public bool IsSkippable;
        // TODO: Add highlight and audio settings when those classes are implemented
        // public TutorialHighlightSettings DefaultHighlight;
        // public TutorialAudioSettings DefaultAudio;
    }
}