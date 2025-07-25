using UnityEngine;
using System.Collections.Generic;
using ProjectChimera.Core;

namespace ProjectChimera.Data.Tutorial
{
    /// <summary>
    /// Cultivation tutorial step definitions for Project Chimera.
    /// Contains detailed tutorial steps for advanced growing mechanics education.
    /// </summary>
    [CreateAssetMenu(fileName = "CultivationTutorialStepDefinitions", menuName = "Project Chimera/Tutorial/Cultivation Tutorial Step Definitions")]
    public class CultivationTutorialStepDefinitions : ChimeraDataSO
    {
        [Header("Environmental Control Module Steps")]
        [SerializeField] private List<TutorialStepData> _environmentalControlSteps;
        
        [Header("Nutrient Management Module Steps")]
        [SerializeField] private List<TutorialStepData> _nutrientManagementSteps;
        
        [Header("Advanced Growing Techniques Module Steps")]
        [SerializeField] private List<TutorialStepData> _advancedGrowingSteps;
        
        [Header("Pest & Disease Management Module Steps")]
        [SerializeField] private List<TutorialStepData> _pestDiseaseSteps;
        
        [Header("Harvest & Processing Module Steps")]
        [SerializeField] private List<TutorialStepData> _harvestProcessingSteps;
        
        // Properties
        public List<TutorialStepData> EnvironmentalControlSteps => _environmentalControlSteps;
        public List<TutorialStepData> NutrientManagementSteps => _nutrientManagementSteps;
        public List<TutorialStepData> AdvancedGrowingSteps => _advancedGrowingSteps;
        public List<TutorialStepData> PestDiseaseSteps => _pestDiseaseSteps;
        public List<TutorialStepData> HarvestProcessingSteps => _harvestProcessingSteps;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            // Initialize step definitions if empty
            if (_environmentalControlSteps == null || _environmentalControlSteps.Count == 0)
            {
                InitializeAllModuleSteps();
            }
        }
        
        /// <summary>
        /// Initialize all module step definitions
        /// </summary>
        private void InitializeAllModuleSteps()
        {
            InitializeEnvironmentalControlSteps();
            InitializeNutrientManagementSteps();
            InitializeAdvancedGrowingSteps();
            InitializePestDiseaseSteps();
            InitializeHarvestProcessingSteps();
            
            Debug.Log("Initialized all cultivation tutorial module steps");
        }
        
        /// <summary>
        /// Initialize environmental control module steps
        /// </summary>
        private void InitializeEnvironmentalControlSteps()
        {
            _environmentalControlSteps = new List<TutorialStepData>
            {
                // Step 1: Introduction to Environmental Control
                new TutorialStepData
                {
                    StepId = "env_control_intro",
                    Title = "Environmental Control Introduction",
                    Description = "Learn how environmental factors affect cannabis growth and quality.",
                    DetailedInstructions = "Cannabis plants are highly sensitive to environmental conditions. Temperature, humidity, light, and air circulation all play crucial roles in plant health and final product quality.",
                    StepType = TutorialStepType.Introduction,
                    ValidationType = TutorialValidationType.ButtonClick,
                    ValidationTarget = "continue-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 2: Temperature Control Basics
                new TutorialStepData
                {
                    StepId = "env_temperature_basics",
                    Title = "Understanding Temperature Control",
                    Description = "Learn optimal temperature ranges for different growth phases.",
                    DetailedInstructions = "Cannabis thrives in specific temperature ranges: 70-85°F (21-29°C) during vegetative growth, and 65-80°F (18-26°C) during flowering. Let's explore how to maintain these ranges.",
                    StepType = TutorialStepType.Information,
                    ValidationType = TutorialValidationType.Timer,
                    TimeoutDuration = 10f,
                    CanSkip = true
                },
                
                // Step 3: HVAC System Configuration
                new TutorialStepData
                {
                    StepId = "env_hvac_config",
                    Title = "Configure HVAC System",
                    Description = "Set up heating, ventilation, and air conditioning for your grow room.",
                    DetailedInstructions = "Access the Environmental Controls panel and configure your HVAC system. Set the target temperature to 75°F (24°C) for optimal vegetative growth.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "hvac-temperature-set",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 4: Humidity Management
                new TutorialStepData
                {
                    StepId = "env_humidity_management",
                    Title = "Humidity Control",
                    Description = "Learn about relative humidity and its impact on plant health.",
                    DetailedInstructions = "Maintain 40-70% relative humidity during vegetative growth and 40-50% during flowering to prevent mold and optimize transpiration. Configure your humidifier/dehumidifier settings.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "humidity-control-set",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 5: Lighting Schedule Setup
                new TutorialStepData
                {
                    StepId = "env_lighting_schedule",
                    Title = "Lighting Schedule Configuration",
                    Description = "Set up proper photoperiods for different growth phases.",
                    DetailedInstructions = "Configure your lighting schedule: 18/6 (18 hours on, 6 hours off) for vegetative growth, and 12/12 for flowering. This mimics natural seasonal changes.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "lighting-schedule-set",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 6: Air Circulation Setup
                new TutorialStepData
                {
                    StepId = "env_air_circulation",
                    Title = "Air Circulation Configuration",
                    Description = "Configure fans and air movement for optimal plant health.",
                    DetailedInstructions = "Set up intake and exhaust fans to maintain proper air exchange. Configure oscillating fans to strengthen plant stems and prevent stagnant air pockets.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "air-circulation-set",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 7: Environmental Monitoring
                new TutorialStepData
                {
                    StepId = "env_monitoring_setup",
                    Title = "Environmental Monitoring",
                    Description = "Set up sensors and monitoring systems to track environmental conditions.",
                    DetailedInstructions = "Install and configure temperature, humidity, and CO2 sensors. Set up alerts for when conditions go outside optimal ranges.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "monitoring-sensors-configured",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 8: Environmental Troubleshooting
                new TutorialStepData
                {
                    StepId = "env_troubleshooting",
                    Title = "Environmental Troubleshooting",
                    Description = "Learn to identify and fix common environmental issues.",
                    DetailedInstructions = "Review common problems like temperature fluctuations, humidity spikes, and poor air circulation. Practice adjusting settings to correct environmental imbalances.",
                    StepType = TutorialStepType.Problem_Solving,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "environmental-issue-resolved",
                    TimeoutDuration = 0f,
                    CanSkip = false
                }
            };
        }
        
        /// <summary>
        /// Initialize nutrient management module steps
        /// </summary>
        private void InitializeNutrientManagementSteps()
        {
            _nutrientManagementSteps = new List<TutorialStepData>
            {
                // Step 1: Nutrient Basics Introduction
                new TutorialStepData
                {
                    StepId = "nutrient_basics_intro",
                    Title = "Cannabis Nutrition Fundamentals",
                    Description = "Learn about macronutrients, micronutrients, and plant nutrition principles.",
                    DetailedInstructions = "Cannabis requires three primary macronutrients: Nitrogen (N), Phosphorus (P), and Potassium (K), plus secondary nutrients and micronutrients for optimal growth.",
                    StepType = TutorialStepType.Introduction,
                    ValidationType = TutorialValidationType.ButtonClick,
                    ValidationTarget = "continue-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 2: Understanding N-P-K Ratios
                new TutorialStepData
                {
                    StepId = "nutrient_npk_ratios",
                    Title = "N-P-K Ratios Explained",
                    Description = "Learn how to read and apply different N-P-K ratios for growth phases.",
                    DetailedInstructions = "Vegetative growth requires higher nitrogen (e.g., 3-1-2 ratio), while flowering needs more phosphorus and potassium (e.g., 1-3-2 ratio). Let's explore these ratios.",
                    StepType = TutorialStepType.Information,
                    ValidationType = TutorialValidationType.Timer,
                    TimeoutDuration = 12f,
                    CanSkip = true
                },
                
                // Step 3: Creating Nutrient Solutions
                new TutorialStepData
                {
                    StepId = "nutrient_solution_creation",
                    Title = "Create Custom Nutrient Solution",
                    Description = "Mix a balanced nutrient solution for your plants' current growth stage.",
                    DetailedInstructions = "Access the Nutrient Mixing Station and create a vegetative growth solution. Start with a 3-1-2 N-P-K ratio at 400-600 PPM (0.8-1.2 EC).",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "nutrient-solution-mixed",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 4: pH Management
                new TutorialStepData
                {
                    StepId = "nutrient_ph_management",
                    Title = "pH Level Management",
                    Description = "Learn to maintain optimal pH levels for nutrient absorption.",
                    DetailedInstructions = "Cannabis absorbs nutrients best at pH 6.0-7.0 in soil and 5.5-6.5 in hydroponic systems. Use pH adjusters to maintain these ranges.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "ph-adjusted",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 5: EC/TDS Monitoring
                new TutorialStepData
                {
                    StepId = "nutrient_ec_monitoring",
                    Title = "EC/TDS Monitoring",
                    Description = "Monitor electrical conductivity and total dissolved solids in your nutrient solution.",
                    DetailedInstructions = "Use an EC/TDS meter to measure nutrient concentration. Maintain 400-800 PPM for seedlings, 800-1200 PPM for vegetative growth, and 1000-1600 PPM for flowering.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "ec-tds-measured",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 6: Feeding Schedule Implementation
                new TutorialStepData
                {
                    StepId = "nutrient_feeding_schedule",
                    Title = "Feeding Schedule Setup",
                    Description = "Create and implement a feeding schedule for your plants.",
                    DetailedInstructions = "Set up an automated feeding schedule that alternates between nutrient solution and plain water. Most plants benefit from feed-water-water cycles.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "feeding-schedule-set",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 7: Nutrient Deficiency Identification
                new TutorialStepData
                {
                    StepId = "nutrient_deficiency_id",
                    Title = "Identifying Nutrient Deficiencies",
                    Description = "Learn to recognize and diagnose common nutrient deficiencies.",
                    DetailedInstructions = "Study leaf discoloration patterns, growth abnormalities, and other symptoms to identify nitrogen, phosphorus, potassium, and micronutrient deficiencies.",
                    StepType = TutorialStepType.Problem_Solving,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "deficiency-identified",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 8: Corrective Measures
                new TutorialStepData
                {
                    StepId = "nutrient_corrective_measures",
                    Title = "Correcting Nutrient Issues",
                    Description = "Apply corrective measures to address nutrient deficiencies and toxicities.",
                    DetailedInstructions = "Adjust your nutrient solution based on plant symptoms. Flush with plain water if toxicity is suspected, or increase specific nutrients for deficiencies.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "nutrient-issue-corrected",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 9: Advanced Nutrition Techniques
                new TutorialStepData
                {
                    StepId = "nutrient_advanced_techniques",
                    Title = "Advanced Nutrition Strategies",
                    Description = "Explore advanced feeding techniques like flushing and bloom boosters.",
                    DetailedInstructions = "Learn about pre-harvest flushing, bloom boosters, organic additives, and other advanced nutrition techniques to maximize quality and yield.",
                    StepType = TutorialStepType.Information,
                    ValidationType = TutorialValidationType.Timer,
                    TimeoutDuration = 10f,
                    CanSkip = true
                },
                
                // Step 10: Nutrition Program Optimization
                new TutorialStepData
                {
                    StepId = "nutrient_program_optimization",
                    Title = "Optimize Your Nutrition Program",
                    Description = "Fine-tune your feeding program based on plant response and growth stage.",
                    DetailedInstructions = "Review your plant's response to the nutrition program and make adjustments. Create a customized feeding schedule optimized for your specific strain and growing conditions.",
                    StepType = TutorialStepType.Completion,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "nutrition-program-optimized",
                    TimeoutDuration = 0f,
                    CanSkip = false
                }
            };
        }
        
        /// <summary>
        /// Initialize advanced growing techniques module steps
        /// </summary>
        private void InitializeAdvancedGrowingSteps()
        {
            _advancedGrowingSteps = new List<TutorialStepData>
            {
                // Step 1: Advanced Techniques Introduction
                new TutorialStepData
                {
                    StepId = "advanced_techniques_intro",
                    Title = "Advanced Growing Techniques Overview",
                    Description = "Introduction to professional plant training and optimization methods.",
                    DetailedInstructions = "Advanced techniques like LST, HST, SCROG, and SOG can significantly improve yields, plant structure, and canopy management. Let's explore these methods.",
                    StepType = TutorialStepType.Introduction,
                    ValidationType = TutorialValidationType.ButtonClick,
                    ValidationTarget = "continue-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 2: Low Stress Training (LST)
                new TutorialStepData
                {
                    StepId = "advanced_lst_technique",
                    Title = "Low Stress Training (LST)",
                    Description = "Learn to gently bend and tie branches to improve light exposure.",
                    DetailedInstructions = "LST involves gently bending branches horizontally to create an even canopy. Select a plant and apply LST by tying down the main stem and training side branches.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "lst-applied",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 3: High Stress Training (HST)
                new TutorialStepData
                {
                    StepId = "advanced_hst_technique",
                    Title = "High Stress Training (HST)",
                    Description = "Learn topping, FIMing, and other high-stress training methods.",
                    DetailedInstructions = "HST techniques like topping remove the main growing tip to encourage multiple main colas. Practice topping a plant during vegetative growth to increase yield potential.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "hst-applied",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 4: Screen of Green (SCROG)
                new TutorialStepData
                {
                    StepId = "advanced_scrog_setup",
                    Title = "Screen of Green (SCROG) Setup",
                    Description = "Install and configure a SCROG net for canopy management.",
                    DetailedInstructions = "Install a horizontal screen 12-18 inches above your plants. Train branches through the screen squares to create an even canopy and maximize light exposure.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "scrog-net-installed",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 5: Sea of Green (SOG)
                new TutorialStepData
                {
                    StepId = "advanced_sog_technique",
                    Title = "Sea of Green (SOG) Technique",
                    Description = "Learn to grow many small plants for faster harvests.",
                    DetailedInstructions = "SOG involves growing many small plants close together, flipping to flowering early to create a 'sea' of small colas. Set up a SOG arrangement with proper plant spacing.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "sog-arrangement-set",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 6: Defoliation Techniques
                new TutorialStepData
                {
                    StepId = "advanced_defoliation",
                    Title = "Strategic Defoliation",
                    Description = "Learn when and how to remove leaves for improved light penetration.",
                    DetailedInstructions = "Remove large fan leaves that block light to lower bud sites. Practice selective defoliation during early flowering to improve light distribution and airflow.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "defoliation-performed",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 7: Lollipopping Technique
                new TutorialStepData
                {
                    StepId = "advanced_lollipopping",
                    Title = "Lollipopping for Better Yields",
                    Description = "Remove lower growth to focus energy on top colas.",
                    DetailedInstructions = "Lollipopping removes lower branches and leaves that won't receive adequate light. Trim the bottom third of the plant to redirect energy to main colas.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "lollipopping-performed",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 8: Supercropping Technique
                new TutorialStepData
                {
                    StepId = "advanced_supercropping",
                    Title = "Supercropping for Stem Strength",
                    Description = "Learn to stress stems to increase strength and yield potential.",
                    DetailedInstructions = "Supercropping involves gently crushing the inner stem fibers to create stronger, thicker branches. Practice this technique on healthy vegetative plants.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "supercropping-performed",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 9: Mainlining/Manifolding
                new TutorialStepData
                {
                    StepId = "advanced_mainlining",
                    Title = "Mainlining/Manifolding Technique",
                    Description = "Create a symmetrical plant structure with multiple main colas.",
                    DetailedInstructions = "Mainlining creates 8-16 main colas of equal size through systematic topping and training. Start with young plants for best results.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "mainlining-started",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 10: Timing and Recovery
                new TutorialStepData
                {
                    StepId = "advanced_timing_recovery",
                    Title = "Training Timing and Plant Recovery",
                    Description = "Learn optimal timing for training techniques and recovery management.",
                    DetailedInstructions = "Understand when to apply different techniques and how to help plants recover. Monitor plant health and adjust training intensity based on plant response.",
                    StepType = TutorialStepType.Information,
                    ValidationType = TutorialValidationType.Timer,
                    TimeoutDuration = 8f,
                    CanSkip = true
                },
                
                // Step 11: Strain-Specific Considerations
                new TutorialStepData
                {
                    StepId = "advanced_strain_considerations",
                    Title = "Strain-Specific Training Approaches",
                    Description = "Adapt training techniques to different strain characteristics.",
                    DetailedInstructions = "Learn how indica, sativa, and hybrid strains respond differently to training. Adjust techniques based on growth patterns, stretch, and branch flexibility.",
                    StepType = TutorialStepType.Information,
                    ValidationType = TutorialValidationType.Timer,
                    TimeoutDuration = 6f,
                    CanSkip = true
                },
                
                // Step 12: Combining Techniques
                new TutorialStepData
                {
                    StepId = "advanced_combining_techniques",
                    Title = "Combining Multiple Training Methods",
                    Description = "Learn to effectively combine different training techniques for maximum results.",
                    DetailedInstructions = "Practice combining LST with SCROG, or HST with defoliation. Create a comprehensive training plan that maximizes your specific growing setup and goals.",
                    StepType = TutorialStepType.Completion,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "combined-techniques-applied",
                    TimeoutDuration = 0f,
                    CanSkip = false
                }
            };
        }
        
        /// <summary>
        /// Initialize pest and disease management module steps
        /// </summary>
        private void InitializePestDiseaseSteps()
        {
            _pestDiseaseSteps = new List<TutorialStepData>
            {
                // Step 1: IPM Introduction
                new TutorialStepData
                {
                    StepId = "pest_ipm_intro",
                    Title = "Integrated Pest Management (IPM)",
                    Description = "Learn the principles of IPM for sustainable pest and disease control.",
                    DetailedInstructions = "IPM combines prevention, monitoring, and treatment strategies to manage pests and diseases while minimizing environmental impact and maintaining plant health.",
                    StepType = TutorialStepType.Introduction,
                    ValidationType = TutorialValidationType.ButtonClick,
                    ValidationTarget = "continue-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 2: Common Pest Identification
                new TutorialStepData
                {
                    StepId = "pest_identification",
                    Title = "Identifying Common Cannabis Pests",
                    Description = "Learn to identify spider mites, aphids, thrips, and other common pests.",
                    DetailedInstructions = "Study visual guides and symptoms of common pests: spider mites (tiny webs, stippling), aphids (clusters on new growth), thrips (silvery damage), and more.",
                    StepType = TutorialStepType.Information,
                    ValidationType = TutorialValidationType.Timer,
                    TimeoutDuration = 15f,
                    CanSkip = true
                },
                
                // Step 3: Disease Recognition
                new TutorialStepData
                {
                    StepId = "disease_recognition",
                    Title = "Recognizing Plant Diseases",
                    Description = "Identify powdery mildew, bud rot, root rot, and other diseases.",
                    DetailedInstructions = "Learn to recognize symptoms of fungal, bacterial, and viral diseases. Early detection is crucial for effective treatment and preventing spread.",
                    StepType = TutorialStepType.Information,
                    ValidationType = TutorialValidationType.Timer,
                    TimeoutDuration = 12f,
                    CanSkip = true
                },
                
                // Step 4: Prevention Strategies
                new TutorialStepData
                {
                    StepId = "pest_prevention",
                    Title = "Prevention is the Best Medicine",
                    Description = "Implement preventive measures to avoid pest and disease problems.",
                    DetailedInstructions = "Set up quarantine procedures, maintain proper environmental conditions, implement sanitation protocols, and use companion planting strategies.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "prevention-measures-implemented",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 5: Beneficial Insects
                new TutorialStepData
                {
                    StepId = "pest_beneficial_insects",
                    Title = "Using Beneficial Insects",
                    Description = "Introduce predatory insects to control pest populations naturally.",
                    DetailedInstructions = "Release ladybugs for aphid control, predatory mites for spider mites, or parasitic wasps for various pests. Learn proper release timing and conditions.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "beneficial-insects-released",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 6: Organic Treatment Options
                new TutorialStepData
                {
                    StepId = "pest_organic_treatments",
                    Title = "Organic Treatment Methods",
                    Description = "Apply organic sprays and treatments for pest and disease control.",
                    DetailedInstructions = "Use neem oil, insecticidal soaps, diatomaceous earth, and other organic treatments. Learn proper application timing, dilution rates, and safety precautions.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "organic-treatment-applied",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 7: Chemical Treatment Safety
                new TutorialStepData
                {
                    StepId = "pest_chemical_safety",
                    Title = "Safe Chemical Treatment Practices",
                    Description = "Learn when and how to safely apply chemical treatments if necessary.",
                    DetailedInstructions = "Understand proper PPE, application techniques, pre-harvest intervals, and worker safety when using chemical pesticides or fungicides.",
                    StepType = TutorialStepType.Information,
                    ValidationType = TutorialValidationType.Timer,
                    TimeoutDuration = 10f,
                    CanSkip = true
                },
                
                // Step 8: Monitoring and Scouting
                new TutorialStepData
                {
                    StepId = "pest_monitoring",
                    Title = "Regular Monitoring and Scouting",
                    Description = "Establish regular inspection routines to catch problems early.",
                    DetailedInstructions = "Set up a weekly scouting schedule, use yellow sticky traps for monitoring, and maintain inspection logs to track pest and disease pressure.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "monitoring-system-established",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 9: Emergency Response Protocols
                new TutorialStepData
                {
                    StepId = "pest_emergency_response",
                    Title = "Emergency Response for Severe Infestations",
                    Description = "Learn to respond quickly to severe pest or disease outbreaks.",
                    DetailedInstructions = "Practice rapid response protocols for serious infestations: isolation, intensive treatment, and recovery strategies to save your crop.",
                    StepType = TutorialStepType.Problem_Solving,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "emergency-response-executed",
                    TimeoutDuration = 0f,
                    CanSkip = false
                }
            };
        }
        
        /// <summary>
        /// Initialize harvest and processing module steps
        /// </summary>
        private void InitializeHarvestProcessingSteps()
        {
            _harvestProcessingSteps = new List<TutorialStepData>
            {
                // Step 1: Harvest Timing Introduction
                new TutorialStepData
                {
                    StepId = "harvest_timing_intro",
                    Title = "Determining Optimal Harvest Timing",
                    Description = "Learn to identify when your plants are ready for harvest.",
                    DetailedInstructions = "Harvest timing dramatically affects potency, flavor, and effects. Learn to use visual cues, trichome analysis, and timing guidelines to determine peak harvest windows.",
                    StepType = TutorialStepType.Introduction,
                    ValidationType = TutorialValidationType.ButtonClick,
                    ValidationTarget = "continue-button",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 2: Trichome Analysis
                new TutorialStepData
                {
                    StepId = "harvest_trichome_analysis",
                    Title = "Trichome Analysis with Magnification",
                    Description = "Use a jeweler's loupe or microscope to examine trichome development.",
                    DetailedInstructions = "Examine trichomes under magnification: clear (immature), cloudy/milky (peak THC), amber (degrading to CBN). Practice identifying these stages for optimal harvest timing.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "trichome-analysis-performed",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 3: Pre-Harvest Preparation
                new TutorialStepData
                {
                    StepId = "harvest_preparation",
                    Title = "Pre-Harvest Preparation",
                    Description = "Prepare your workspace and tools for harvest day.",
                    DetailedInstructions = "Set up a clean harvest area, sterilize cutting tools, prepare drying racks, and ensure proper environmental conditions for the harvest and drying process.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "harvest-workspace-prepared",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 4: Flushing Before Harvest
                new TutorialStepData
                {
                    StepId = "harvest_flushing",
                    Title = "Pre-Harvest Flushing",
                    Description = "Flush plants with plain water to improve final product quality.",
                    DetailedInstructions = "Stop feeding nutrients 1-2 weeks before harvest and flush with plain pH-adjusted water. This helps remove residual nutrients and improve flavor and burn quality.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "pre-harvest-flush-completed",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 5: Harvest Techniques
                new TutorialStepData
                {
                    StepId = "harvest_techniques",
                    Title = "Proper Harvesting Techniques",
                    Description = "Learn different methods for cutting and handling plants at harvest.",
                    DetailedInstructions = "Practice whole-plant harvest vs. selective branch harvest. Learn proper cutting techniques, handling procedures, and initial processing steps.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "harvest-technique-demonstrated",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 6: Wet Trimming vs Dry Trimming
                new TutorialStepData
                {
                    StepId = "harvest_trimming_methods",
                    Title = "Wet Trimming vs. Dry Trimming",
                    Description = "Compare the advantages and techniques of wet vs. dry trimming.",
                    DetailedInstructions = "Learn the pros and cons of trimming fresh vs. dried cannabis. Practice both techniques and understand when to use each method based on your situation.",
                    StepType = TutorialStepType.Choice,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "trimming-method-selected",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 7: Drying Environment Setup
                new TutorialStepData
                {
                    StepId = "harvest_drying_setup",
                    Title = "Setting Up the Drying Environment",
                    Description = "Create optimal conditions for drying your harvested cannabis.",
                    DetailedInstructions = "Set up a dark, well-ventilated drying area with 60-70°F temperature and 45-55% humidity. Ensure gentle air circulation and protection from light.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "drying-environment-configured",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 8: Monitoring the Drying Process
                new TutorialStepData
                {
                    StepId = "harvest_drying_monitoring",
                    Title = "Monitoring the Drying Process",
                    Description = "Track drying progress and adjust conditions as needed.",
                    DetailedInstructions = "Monitor humidity, temperature, and drying progress daily. Learn to recognize when buds are properly dried (stems snap, not bend) and ready for curing.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "drying-progress-monitored",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 9: Curing Process Setup
                new TutorialStepData
                {
                    StepId = "harvest_curing_setup",
                    Title = "Setting Up the Curing Process",
                    Description = "Transfer dried buds to curing containers for quality enhancement.",
                    DetailedInstructions = "Place dried buds in glass jars or curing containers at 62-65% humidity. Learn proper jar filling, burping schedules, and humidity monitoring during curing.",
                    StepType = TutorialStepType.Interaction,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "curing-process-initiated",
                    TimeoutDuration = 0f,
                    CanSkip = false
                },
                
                // Step 10: Quality Assessment
                new TutorialStepData
                {
                    StepId = "harvest_quality_assessment",
                    Title = "Final Product Quality Assessment",
                    Description = "Evaluate the quality, potency, and characteristics of your finished product.",
                    DetailedInstructions = "Assess visual appearance, aroma, texture, and overall quality. Learn to identify signs of proper vs. improper drying and curing, and document your results.",
                    StepType = TutorialStepType.Information,
                    ValidationType = TutorialValidationType.Timer,
                    TimeoutDuration = 8f,
                    CanSkip = true
                },
                
                // Step 11: Storage and Preservation
                new TutorialStepData
                {
                    StepId = "harvest_storage_preservation",
                    Title = "Long-term Storage and Preservation",
                    Description = "Learn proper storage techniques to maintain quality over time.",
                    DetailedInstructions = "Set up proper storage containers, humidity control, and environmental conditions for long-term preservation. Understand how to maintain potency and prevent degradation.",
                    StepType = TutorialStepType.Completion,
                    ValidationType = TutorialValidationType.SystemEvent,
                    ValidationTarget = "storage-system-established",
                    TimeoutDuration = 0f,
                    CanSkip = false
                }
            };
        }
        
        /// <summary>
        /// Get steps by module ID
        /// </summary>
        public List<TutorialStepData> GetStepsByModuleId(string moduleId)
        {
            switch (moduleId.ToLower())
            {
                case "environmental_control":
                    return _environmentalControlSteps;
                case "nutrient_management":
                    return _nutrientManagementSteps;
                case "advanced_growing_techniques":
                    return _advancedGrowingSteps;
                case "pest_disease_management":
                    return _pestDiseaseSteps;
                case "harvest_processing":
                    return _harvestProcessingSteps;
                default:
                    return new List<TutorialStepData>();
            }
        }
        
        /// <summary>
        /// Get all cultivation tutorial steps
        /// </summary>
        public List<TutorialStepData> GetAllCultivationSteps()
        {
            var allSteps = new List<TutorialStepData>();
            allSteps.AddRange(_environmentalControlSteps);
            allSteps.AddRange(_nutrientManagementSteps);
            allSteps.AddRange(_advancedGrowingSteps);
            allSteps.AddRange(_pestDiseaseSteps);
            allSteps.AddRange(_harvestProcessingSteps);
            
            return allSteps;
        }
        
        /// <summary>
        /// Validate data integrity
        /// </summary>
        protected override bool ValidateDataSpecific()
        {
            var allSteps = GetAllCultivationSteps();
            var stepIds = new HashSet<string>();
            
            foreach (var step in allSteps)
            {
                if (string.IsNullOrEmpty(step.StepId))
                {
                    LogError($"Cultivation tutorial step has empty StepId: {step.Title}");
                    continue;
                }
                
                if (stepIds.Contains(step.StepId))
                {
                    LogError($"Duplicate cultivation tutorial step ID: {step.StepId}");
                }
                else
                {
                    stepIds.Add(step.StepId);
                }
                
                if (string.IsNullOrEmpty(step.Title))
                {
                    LogWarning($"Cultivation tutorial step {step.StepId} has empty title");
                }
                
                if (string.IsNullOrEmpty(step.DetailedInstructions))
                {
                    LogWarning($"Cultivation tutorial step {step.StepId} has empty instruction text");
                }
            }
            
            Debug.Log($"Validated {allSteps.Count} cultivation tutorial steps across {GetModuleCount()} modules");
            return true;
        }
        
        /// <summary>
        /// Get module count
        /// </summary>
        private int GetModuleCount()
        {
            int count = 0;
            if (_environmentalControlSteps?.Count > 0) count++;
            if (_nutrientManagementSteps?.Count > 0) count++;
            if (_advancedGrowingSteps?.Count > 0) count++;
            if (_pestDiseaseSteps?.Count > 0) count++;
            if (_harvestProcessingSteps?.Count > 0) count++;
            return count;
        }
    }
}